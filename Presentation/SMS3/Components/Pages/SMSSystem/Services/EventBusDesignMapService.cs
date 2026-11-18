using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;
using SMS_Domain.Enums;

namespace SMS3.Components.Pages.SMSSystem.Services;

public sealed class EventBusDesignMapService : IEventBusDesignMapService
{
    private static readonly Regex HandlerClassRegex = new(
        @"\bclass\s+(?<className>[A-Za-z_]\w*)\s*:\s*[^\r\n]*Base(?:Domain|Integration|UI)EventHandler<(?<eventType>[^>]+)>",
        RegexOptions.Compiled);

    private static readonly Regex MethodRegex = new(
        @"\b(?:public|private|protected|internal)\s+(?:override\s+)?(?:async\s+)?(?:Task(?:<[^>]+>)?|Result|void)\s+(?<methodName>[A-Za-z_]\w+)\s*\(",
        RegexOptions.Compiled);

    private static readonly Regex NewEventRegex = new(
        @"\bnew\s+(?<eventType>(?:[A-Za-z_]\w*\.)*[A-Za-z_]\w*Event)\s*\(",
        RegexOptions.Compiled);

    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<EventBusDesignMapService> _logger;
    private readonly IEventLifecycleRegistry _eventLifecycleRegistry;

    public EventBusDesignMapService(
        IWebHostEnvironment environment,
        ILogger<EventBusDesignMapService> logger,
        IEventLifecycleRegistry eventLifecycleRegistry)
    {
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventLifecycleRegistry = eventLifecycleRegistry ?? throw new ArgumentNullException(nameof(eventLifecycleRegistry));
    }

    public Task<IReadOnlyList<EventBusMapEventItem>> BuildMapAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var repositoryRoot = ResolveRepositoryRoot(_environment.ContentRootPath);
            if (string.IsNullOrWhiteSpace(repositoryRoot))
            {
                _logger.LogWarning("EventBus map discovery skipped because repository root could not be resolved from {ContentRootPath}.", _environment.ContentRootPath);
                return Task.FromResult<IReadOnlyList<EventBusMapEventItem>>([]);
            }

            var eventDefinitions = LoadEventDefinitions(repositoryRoot);
            if (eventDefinitions.Count == 0)
            {
                return Task.FromResult<IReadOnlyList<EventBusMapEventItem>>([]);
            }

            var sourceFiles = LoadSourceFileSnapshots(repositoryRoot);
            var handlerSnapshots = sourceFiles.Where(IsHandlerFile).ToList();
            var handlerClassDetails = handlerSnapshots.SelectMany(ParseHandlerClasses).ToList();

            var categorySort = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["DomainEvent"] = 0,
                ["IntegrationEvent"] = 1,
                ["UIEvent"] = 2
            };

            var discoveredItems = new List<EventBusMapEventItem>();

            foreach (var definition in eventDefinitions
                         .OrderBy(x => categorySort.TryGetValue(x.Category, out var sortOrder) ? sortOrder : 99)
                         .ThenBy(x => x.EventClassName, StringComparer.OrdinalIgnoreCase))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var triggeredAt = FindTriggerLocations(sourceFiles, definition.EventClassName);

                var ownedHandlerClasses = handlerClassDetails
                    .Where(x => string.Equals(x.HandledEventName, definition.EventClassName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var handledBy = ownedHandlerClasses
                    .Select(FormatHandledBy)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var chainedTo = ownedHandlerClasses
                    .SelectMany(x => x.ChainedEventNames)
                    .Where(x => !string.Equals(x, definition.EventClassName, StringComparison.OrdinalIgnoreCase))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var chainedFrom = handlerClassDetails
                    .Where(x => x.ChainedEventNames.Any(y => string.Equals(y, definition.EventClassName, StringComparison.OrdinalIgnoreCase)))
                    .Select(x => x.ClassName)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var chainFlow = ownedHandlerClasses
                    .Select(FormatChainFlow)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var isUnused = triggeredAt.Count == 0
                               && handledBy.Count == 0
                               && chainedTo.Count == 0
                               && chainedFrom.Count == 0;

                var lifecycleFound = _eventLifecycleRegistry.TryGet(definition.EventClassName, out var lifecycleEntry);
                var lifecycleState = lifecycleFound
                    ? lifecycleEntry.LifecycleState
                    : EventLifecycleState.Active;
                var lifecycleNotes = lifecycleFound
                    ? lifecycleEntry.Notes
                    : "Lifecycle registry entry not found; defaulted to Active.";

                discoveredItems.Add(new EventBusMapEventItem(
                    definition.EventClassName,
                    definition.Category,
                    lifecycleState,
                    lifecycleNotes,
                    triggeredAt,
                    handledBy,
                    chainedTo,
                    chainedFrom,
                    chainFlow,
                    isUnused));
            }

            return Task.FromResult<IReadOnlyList<EventBusMapEventItem>>(discoveredItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build EventBus design map.");
            return Task.FromResult<IReadOnlyList<EventBusMapEventItem>>([]);
        }
    }

    private static string? ResolveRepositoryRoot(string contentRootPath)
    {
        var directory = new DirectoryInfo(contentRootPath);
        while (directory is not null)
        {
            var domainPath = Path.Combine(directory.FullName, "Domain");
            var applicationPath = Path.Combine(directory.FullName, "Application");
            var presentationPath = Path.Combine(directory.FullName, "Presentation");

            if (Directory.Exists(domainPath)
                && Directory.Exists(applicationPath)
                && Directory.Exists(presentationPath))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }

    private static List<EventDefinition> LoadEventDefinitions(string repositoryRoot)
    {
        var eventsRoot = Path.Combine(repositoryRoot, "Domain", "Events");
        if (!Directory.Exists(eventsRoot))
        {
            return [];
        }

        return Directory
            .EnumerateFiles(eventsRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains("\\bin\\", StringComparison.OrdinalIgnoreCase)
                           && !path.Contains("\\obj\\", StringComparison.OrdinalIgnoreCase))
            .Select(path => new EventDefinition(
                Path.GetFileNameWithoutExtension(path),
                ResolveCategoryFromPath(path),
                path))
            .OrderBy(x => x.EventClassName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string ResolveCategoryFromPath(string filePath)
    {
        if (filePath.Contains("\\DomainEvents\\", StringComparison.OrdinalIgnoreCase))
        {
            return "DomainEvent";
        }

        if (filePath.Contains("\\IntegrationEvents\\", StringComparison.OrdinalIgnoreCase))
        {
            return "IntegrationEvent";
        }

        if (filePath.Contains("\\UIEvents\\", StringComparison.OrdinalIgnoreCase))
        {
            return "UIEvent";
        }

        return "Unknown";
    }

    private static List<SourceFileSnapshot> LoadSourceFileSnapshots(string repositoryRoot)
    {
        var roots = new[]
        {
            Path.Combine(repositoryRoot, "Domain"),
            Path.Combine(repositoryRoot, "Application"),
            Path.Combine(repositoryRoot, "Presentation", "SMS3")
        };

        var files = new List<SourceFileSnapshot>();

        foreach (var root in roots)
        {
            if (!Directory.Exists(root))
            {
                continue;
            }

            foreach (var filePath in Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories))
            {
                if (filePath.Contains("\\bin\\", StringComparison.OrdinalIgnoreCase)
                    || filePath.Contains("\\obj\\", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var lines = File.ReadAllLines(filePath);
                var relativePath = Path.GetRelativePath(repositoryRoot, filePath)
                    .Replace('/', '\\');

                files.Add(new SourceFileSnapshot(relativePath, lines));
            }
        }

        return files;
    }

    private static bool IsHandlerFile(SourceFileSnapshot snapshot)
    {
        return snapshot.RelativePath.StartsWith("Application\\EventHandlers\\", StringComparison.OrdinalIgnoreCase)
               || snapshot.RelativePath.StartsWith("Presentation\\SMS3\\EventHandlers\\", StringComparison.OrdinalIgnoreCase);
    }

    private static List<string> FindTriggerLocations(IEnumerable<SourceFileSnapshot> snapshots, string eventClassName)
    {
        var creationRegex = new Regex(
            $@"\bnew\s+(?:[A-Za-z_]\w*\.)*{Regex.Escape(eventClassName)}\s*\(",
            RegexOptions.Compiled);

        var locations = new List<string>();

        foreach (var snapshot in snapshots)
        {
            for (var lineIndex = 0; lineIndex < snapshot.Lines.Length; lineIndex++)
            {
                var line = snapshot.Lines[lineIndex];
                if (!creationRegex.IsMatch(line))
                {
                    continue;
                }

                locations.Add($"{snapshot.RelativePath}:{lineIndex + 1} - {TrimSnippet(line)}");
            }
        }

        return locations
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IEnumerable<HandlerClassDetail> ParseHandlerClasses(SourceFileSnapshot snapshot)
    {
        var lines = snapshot.Lines;
        var classMatches = new List<(int StartLine, string ClassName, string HandledEventName)>();

        for (var index = 0; index < lines.Length; index++)
        {
            var match = HandlerClassRegex.Match(lines[index]);
            if (!match.Success)
            {
                continue;
            }

            var className = match.Groups["className"].Value.Trim();
            var handledEventName = NormalizeTypeName(match.Groups["eventType"].Value);

            if (string.IsNullOrWhiteSpace(className) || string.IsNullOrWhiteSpace(handledEventName))
            {
                continue;
            }

            classMatches.Add((index, className, handledEventName));
        }

        for (var classIndex = 0; classIndex < classMatches.Count; classIndex++)
        {
            var current = classMatches[classIndex];
            var nextStart = classIndex + 1 < classMatches.Count
                ? classMatches[classIndex + 1].StartLine
                : lines.Length;

            var blockLines = lines
                .Skip(current.StartLine)
                .Take(nextStart - current.StartLine)
                .ToList();

            var methods = blockLines
                .Select(line => MethodRegex.Match(line))
                .Where(match => match.Success)
                .Select(match => match.Groups["methodName"].Value.Trim())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var chainedEvents = blockLines
                .Select(line => NewEventRegex.Match(line))
                .Where(match => match.Success)
                .Select(match => NormalizeTypeName(match.Groups["eventType"].Value))
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            yield return new HandlerClassDetail(
                current.ClassName,
                current.HandledEventName,
                snapshot.RelativePath,
                current.StartLine + 1,
                methods,
                chainedEvents);
        }
    }

    private static string NormalizeTypeName(string rawTypeName)
    {
        if (string.IsNullOrWhiteSpace(rawTypeName))
        {
            return string.Empty;
        }

        var normalized = rawTypeName.Trim();

        var genericBoundary = normalized.IndexOf(',');
        if (genericBoundary >= 0)
        {
            normalized = normalized[..genericBoundary];
        }

        normalized = normalized.Trim().TrimEnd('>');
        if (normalized.EndsWith("?", StringComparison.Ordinal))
        {
            normalized = normalized[..^1];
        }

        var lastDot = normalized.LastIndexOf('.');
        if (lastDot >= 0 && lastDot < normalized.Length - 1)
        {
            normalized = normalized[(lastDot + 1)..];
        }

        return normalized.Trim();
    }

    private static string FormatHandledBy(HandlerClassDetail detail)
    {
        var methodSegment = detail.MethodNames.Count > 0
            ? string.Join(" => ", detail.MethodNames)
            : "HandleAsync";

        return $"{detail.ClassName} => {methodSegment} ({detail.FilePath}:{detail.StartLine})";
    }

    private static string FormatChainFlow(HandlerClassDetail detail)
    {
        var methodSegment = detail.MethodNames.Count > 0
            ? string.Join(" => ", detail.MethodNames)
            : "HandleAsync";

        var chainedEventsSegment = detail.ChainedEventNames.Count > 0
            ? string.Join(", ", detail.ChainedEventNames)
            : "No chained events";

        return $"{detail.ClassName} => {methodSegment} => {chainedEventsSegment}";
    }

    private static string TrimSnippet(string line)
    {
        var compact = Regex.Replace(line.Trim(), @"\s+", " ");
        if (compact.Length <= 140)
        {
            return compact;
        }

        return compact[..140] + "...";
    }

    private sealed record EventDefinition(string EventClassName, string Category, string SourcePath);

    private sealed record SourceFileSnapshot(string RelativePath, string[] Lines);

    private sealed record HandlerClassDetail(
        string ClassName,
        string HandledEventName,
        string FilePath,
        int StartLine,
        IReadOnlyList<string> MethodNames,
        IReadOnlyList<string> ChainedEventNames);
}
