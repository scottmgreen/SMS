using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using SMS_Application.Interfaces;
using SMS_Domain.Common;
using SMS_Domain.Entities;
using SMS_Domain.Enums;

namespace SMS3.Components.Pages.SMSSystem.Services;

public sealed class EmailNotificationSettingsService : IEmailNotificationSettingsService, IEmailNotificationExecutionModeResolver
{
    private const string NotificationEventsSectionName = "NotificationEvents";
    private const string ChannelDisplayNamesSectionName = "ChannelDisplayNames";
    private const string EnabledPropertyName = "Enabled";
    private const string TriggerEventKeyPropertyName = "TriggerEventKey";
    private const string TriggerDisplayNamePropertyName = "TriggerDisplayName";
    private const string TriggerPropertyName = "Trigger";
    private const string ChannelsSectionName = "Channels";
    private const string ExecutionModePropertyName = "ExecutionMode";

    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<EmailNotificationSettingsService> _logger;

    public EmailNotificationSettingsService(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        ILogger<EmailNotificationSettingsService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IReadOnlyList<NotificationSetting> GetAllNotificationSettings()
    {
        return GetNotificationDefinitions()
            .Select(definition => new NotificationSetting
            {
                NotificationType = definition.NotificationType,
                Enabled = ReadEnabled(definition),
                TriggerEventKey = definition.TriggerEventKey,
                TriggerDisplayName = definition.TriggerDisplayName,
                TriggerDomainEvent = definition.TriggerDomainEvent,
                AppSettingsSection = definition.SectionPath,
                ChannelName = definition.ChannelName,
                ExecutionMode = ReadExecutionMode(definition)
            })
            .ToList();
    }

    public bool IsEnabled(string? notificationType, bool defaultValue = true)
    {
        var normalizedNotificationType = notificationType?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedNotificationType))
        {
            return defaultValue;
        }

        var section = _configuration.GetSection($"{NotificationEventsSectionName}:{normalizedNotificationType}");
        if (!section.Exists())
        {
            return defaultValue;
        }

        return section.GetValue<bool?>(EnabledPropertyName) ?? defaultValue;
    }

    public NotificationExecutionMode ResolveExecutionMode(string? notificationType, NotificationExecutionMode fallbackMode)
    {
        var normalizedNotificationType = notificationType?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedNotificationType))
        {
            return fallbackMode;
        }

        var definitions = GetNotificationDefinitions();
        var preferredChannelName = GetPreferredChannelName(normalizedNotificationType);

        var definition = definitions.FirstOrDefault(x =>
                             string.Equals(x.NotificationType, normalizedNotificationType, StringComparison.OrdinalIgnoreCase)
                             && string.Equals(x.ChannelName, preferredChannelName, StringComparison.OrdinalIgnoreCase))
                         ?? definitions.FirstOrDefault(x =>
                             string.Equals(x.NotificationType, normalizedNotificationType, StringComparison.OrdinalIgnoreCase));

        if (definition is null)
        {
            return fallbackMode;
        }

        var configured = _configuration.GetValue<string>($"{definition.SectionPath}:{ExecutionModePropertyName}");
        return ParseAllowedMode(configured, definition.DefaultMode);
    }

    public async Task<Result> SaveNotificationSettingsAsync(IEnumerable<NotificationSetting> settings, CancellationToken cancellationToken = default)
    {
        var requestedSettings = settings?.ToList() ?? [];
        if (requestedSettings.Count == 0)
        {
            return Result.Success();
        }

        var appSettingsPath = Path.Combine(_environment.ContentRootPath, "appsettings.json");
        if (!File.Exists(appSettingsPath))
        {
            return Result.Failure(new SMS_Domain.Common.Error("EMAIL_NOTIFICATION_SETTINGS_FILE_NOT_FOUND", $"appsettings.json was not found at '{appSettingsPath}'."));
        }

        string appSettingsText;
        try
        {
            appSettingsText = await File.ReadAllTextAsync(appSettingsPath, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load appsettings.json for notification settings update.");
            return Result.Failure(new SMS_Domain.Common.Error("EMAIL_NOTIFICATION_SETTINGS_READ_FAILED", "Failed to read appsettings.json for email notification settings."));
        }

        var updatedAppSettingsText = appSettingsText;
        var updatedEntryCount = 0;
        var notificationDefinitions = GetNotificationDefinitions();

        if (notificationDefinitions.Count == 0)
        {
            return Result.Failure(new SMS_Domain.Common.Error(
                "EMAIL_NOTIFICATION_SETTINGS_SECTION_NOT_FOUND",
                "No NotificationEvents channel settings were discovered in appsettings.json."));
        }

        var requestedEnabledByNotification = requestedSettings
            .Where(x => !string.IsNullOrWhiteSpace(x.NotificationType))
            .GroupBy(x => x.NotificationType.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                x => x.Key,
                x => x.Last().Enabled,
                StringComparer.OrdinalIgnoreCase);

        var requestedModeByChannelKey = requestedSettings
            .Where(x => !string.IsNullOrWhiteSpace(x.NotificationType) && !string.IsNullOrWhiteSpace(x.ChannelName))
            .GroupBy(
                x => BuildNotificationChannelKey(x.NotificationType, x.ChannelName),
                StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                x => x.Key,
                x => x.Last().ExecutionMode,
                StringComparer.OrdinalIgnoreCase);

        foreach (var definition in notificationDefinitions)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var channelKey = BuildNotificationChannelKey(definition.NotificationType, definition.ChannelName);
            var hasRequestedMode = requestedModeByChannelKey.TryGetValue(channelKey, out var requestedMode);
            var hasRequestedEnabled = requestedEnabledByNotification.TryGetValue(definition.NotificationType, out var requestedEnabled);

            if (!hasRequestedMode && !hasRequestedEnabled)
            {
                continue;
            }

            var parsedMode = hasRequestedMode
                ? ParseAllowedMode(requestedMode.ToString(), definition.DefaultMode)
                : ReadExecutionMode(definition);

            var enabledValue = hasRequestedEnabled
                ? requestedEnabled
                : ReadEnabled(definition);

            var replacementResult = ReplaceNotificationSettings(
                updatedAppSettingsText,
                definition.NotificationType,
                enabledValue,
                parsedMode.ToString(),
                definition.ChannelName);
            updatedAppSettingsText = replacementResult.UpdatedText;
            updatedEntryCount += replacementResult.ReplacementCount;
        }

        if (updatedEntryCount == 0)
        {
            return Result.Failure(new SMS_Domain.Common.Error(
                "EMAIL_NOTIFICATION_SETTINGS_SECTION_NOT_FOUND",
                "No NotificationEvents entries were updated. Verify appsettings contains NotificationEvents entries for the configured notification/channel types."));
        }

        try
        {
            await File.WriteAllTextAsync(
                appSettingsPath,
                updatedAppSettingsText,
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist email notification settings to appsettings.json.");
            return Result.Failure(new SMS_Domain.Common.Error("EMAIL_NOTIFICATION_SETTINGS_WRITE_FAILED", "Failed to save email notification settings to appsettings.json."));
        }

        if (_configuration is IConfigurationRoot configurationRoot)
        {
            configurationRoot.Reload();
        }

        return Result.Success();
    }

    private static ReplacementResult ReplaceNotificationSettings(string appSettingsText, string notificationType, bool enabled, string executionMode, string channelName)
    {
        if (string.IsNullOrWhiteSpace(appSettingsText)
            || string.IsNullOrWhiteSpace(notificationType)
            || string.IsNullOrWhiteSpace(executionMode))
        {
            return new ReplacementResult(appSettingsText, 0);
        }

        var enabledReplacement = ReplaceNotificationProperty(
            appSettingsText,
            notificationType,
            "Enabled",
            enabled ? "true" : "false");

        var modeReplacement = ReplaceNotificationProperty(
            enabledReplacement.UpdatedText,
            notificationType,
            "ExecutionMode",
            executionMode,
            channelName);

        return new ReplacementResult(
            modeReplacement.UpdatedText,
            enabledReplacement.ReplacementCount + modeReplacement.ReplacementCount);
    }

    private static ReplacementResult ReplaceNotificationProperty(string appSettingsText, string notificationType, string propertyName, string propertyValue, string channelName = "Email")
    {
        if (string.IsNullOrWhiteSpace(appSettingsText)
            || string.IsNullOrWhiteSpace(notificationType)
            || string.IsNullOrWhiteSpace(propertyName)
            || string.IsNullOrWhiteSpace(propertyValue))
        {
            return new ReplacementResult(appSettingsText, 0);
        }

        string pattern;
        if (string.Equals(propertyName, "Enabled", StringComparison.OrdinalIgnoreCase))
        {
            pattern = $"(\"{Regex.Escape(notificationType)}\"\\s*:\\s*\\{{[\\s\\S]*?\"Enabled\"\\s*:\\s*)(true|false)";
        }
        else
        {
            pattern = $"(\"{Regex.Escape(notificationType)}\"\\s*:\\s*\\{{[\\s\\S]*?\"Channels\"\\s*:\\s*\\{{[\\s\\S]*?\"{Regex.Escape(channelName)}\"\\s*:\\s*\\{{[\\s\\S]*?\"{Regex.Escape(propertyName)}\"\\s*:\\s*\")[^\"]+(\")";
        }

        var regex = new Regex(pattern, RegexOptions.Multiline);
        var replacementCount = 0;
        var updatedText = regex.Replace(
            appSettingsText,
            match =>
            {
                replacementCount++;
                if (string.Equals(propertyName, "Enabled", StringComparison.OrdinalIgnoreCase))
                {
                    return $"{match.Groups[1].Value}{propertyValue}";
                }

                return $"{match.Groups[1].Value}{propertyValue}{match.Groups[2].Value}";
            });

        return new ReplacementResult(updatedText, replacementCount);
    }

    private NotificationExecutionMode ReadExecutionMode(NotificationDefinition definition)
    {
        var configured = _configuration.GetValue<string>($"{definition.SectionPath}:{ExecutionModePropertyName}");
        return ParseAllowedMode(configured, definition.DefaultMode);
    }

    private bool ReadEnabled(NotificationDefinition definition)
    {
        return _configuration.GetValue<bool?>(definition.EnabledSectionPath) ?? definition.DefaultEnabled;
    }

    private IReadOnlyList<NotificationDefinition> GetNotificationDefinitions()
    {
        var notificationEventsSection = _configuration.GetSection(NotificationEventsSectionName);
        if (!notificationEventsSection.Exists())
        {
            return [];
        }

        var definitions = new List<NotificationDefinition>();

        foreach (var notificationSection in notificationEventsSection.GetChildren())
        {
            if (IsMetadataSection(notificationSection.Key))
            {
                continue;
            }

            var channelsSection = notificationSection.GetSection(ChannelsSectionName);
            if (!channelsSection.Exists())
            {
                continue;
            }

            var metadata = ResolveEventMetadata(notificationSection.Key);

            var triggerEventKey = notificationSection.GetValue<string>(TriggerEventKeyPropertyName);
            if (string.IsNullOrWhiteSpace(triggerEventKey))
            {
                triggerEventKey = notificationSection.GetValue<string>(TriggerPropertyName);
            }

            if (string.IsNullOrWhiteSpace(triggerEventKey))
            {
                triggerEventKey = metadata?.TriggerEventKey;
            }

            if (string.IsNullOrWhiteSpace(triggerEventKey))
            {
                triggerEventKey = notificationSection.Key;
            }

            var triggerDisplayName = notificationSection.GetValue<string>(TriggerDisplayNamePropertyName);
            if (string.IsNullOrWhiteSpace(triggerDisplayName))
            {
                triggerDisplayName = metadata?.TriggerDisplayName;
            }

            if (string.IsNullOrWhiteSpace(triggerDisplayName))
            {
                triggerDisplayName = triggerEventKey;
            }

            var triggerDomainEvent = triggerDisplayName ?? string.Empty;
            var defaultEnabled = notificationSection.GetValue<bool?>(EnabledPropertyName) ?? true;

            foreach (var channelSection in channelsSection.GetChildren())
            {
                if (string.IsNullOrWhiteSpace(channelSection.Key))
                {
                    continue;
                }

                definitions.Add(new NotificationDefinition(
                    notificationSection.Key,
                    triggerEventKey ?? string.Empty,
                    triggerDisplayName ?? string.Empty,
                    triggerDomainEvent,
                    channelSection.Path,
                    GetDefaultExecutionMode(channelSection.Key),
                    channelSection.Key,
                    defaultEnabled));
            }
        }

        return definitions
            .OrderBy(x => GetGroupSortOrder(x.ChannelName))
            .ThenBy(x => x.NotificationType, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.ChannelName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static NotificationExecutionMode GetDefaultExecutionMode(string channelName)
    {
        if (string.Equals(channelName, "UI", StringComparison.OrdinalIgnoreCase)
            || string.Equals(channelName, "Domain", StringComparison.OrdinalIgnoreCase))
        {
            return NotificationExecutionMode.Immediate;
        }

        return NotificationExecutionMode.Manual;
    }

    private static string GetPreferredChannelName(string notificationType)
    {
        if (string.Equals(notificationType, "DomainEventPublishing", StringComparison.OrdinalIgnoreCase))
        {
            return "Domain";
        }

        if (notificationType.StartsWith("UINotification", StringComparison.OrdinalIgnoreCase))
        {
            return "UI";
        }

        return "Email";
    }

    private static bool IsMetadataSection(string sectionName)
    {
        return string.Equals(sectionName, ChannelDisplayNamesSectionName, StringComparison.OrdinalIgnoreCase);
    }

    private static int GetGroupSortOrder(string channelName)
    {
        if (string.Equals(channelName, "Domain", StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        if (string.Equals(channelName, "UI", StringComparison.OrdinalIgnoreCase))
        {
            return 2;
        }

        return 1;
    }

    private static string BuildNotificationChannelKey(string notificationType, string channelName)
    {
        return $"{notificationType.Trim()}::{channelName.Trim()}";
    }

    private static NotificationExecutionMode ParseAllowedMode(string? configured, NotificationExecutionMode defaultMode)
    {
        if (!Enum.TryParse<NotificationExecutionMode>(configured, true, out var parsedMode))
        {
            return defaultMode;
        }

        return parsedMode is NotificationExecutionMode.Immediate or NotificationExecutionMode.Manual
            ? parsedMode
            : defaultMode;
    }

    private static TriggerMetadata? ResolveEventMetadata(string notificationType)
    {
        if (string.IsNullOrWhiteSpace(notificationType))
        {
            return null;
        }

        if (string.Equals(notificationType, "MitigationApproval", StringComparison.OrdinalIgnoreCase))
        {
            return new TriggerMetadata("DomainEvent:MitigationApprovalRequestedEvent", "MitigationApprovalRequestedEvent");
        }

        if (string.Equals(notificationType, "MitigationApproved", StringComparison.OrdinalIgnoreCase))
        {
            return new TriggerMetadata("DomainEvent:MitigationStatusChangedEvent", "MitigationStatusChangedEvent");
        }

        if (string.Equals(notificationType, "MitigationTargetDateNotification", StringComparison.OrdinalIgnoreCase))
        {
            return new TriggerMetadata("ScheduledTask:MitigationTargetDateNotification", "Mitigation target-date scan trigger");
        }

        if (string.Equals(notificationType, "HazardSubmissionStatusEscalation", StringComparison.OrdinalIgnoreCase))
        {
            return new TriggerMetadata("ScheduledTask:HazardSubmissionStatusEscalation", "Report status escalation scan + API escalation");
        }

        if (notificationType.StartsWith("UINotification", StringComparison.OrdinalIgnoreCase))
        {
            var severity = notificationType["UINotification".Length..].Trim();
            if (string.IsNullOrWhiteSpace(severity))
            {
                severity = "Info";
            }

            return new TriggerMetadata($"UIEvent:UINotificationEvent:{severity}", $"UINotificationEvent severity {severity}");
        }

        if (string.Equals(notificationType, "DomainEventPublishing", StringComparison.OrdinalIgnoreCase))
        {
            return new TriggerMetadata("DomainControl:DomainEventPublishing", "Domain EventBus publish control");
        }

        return null;
    }

    private sealed record NotificationDefinition(
        string NotificationType,
        string TriggerEventKey,
        string TriggerDisplayName,
        string TriggerDomainEvent,
        string SectionPath,
        NotificationExecutionMode DefaultMode,
        string ChannelName,
        bool DefaultEnabled = true)
    {
        public string EnabledSectionPath => $"{NotificationEventsSectionName}:{NotificationType}:{EnabledPropertyName}";
    }

    private sealed record TriggerMetadata(string TriggerEventKey, string TriggerDisplayName);

    private sealed record ReplacementResult(string UpdatedText, int ReplacementCount);
}
