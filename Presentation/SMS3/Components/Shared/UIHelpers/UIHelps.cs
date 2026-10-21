using SMS_Domain.Enums;
using Radzen;
using System.Text.RegularExpressions;
using SMS_Application.Interfaces;
using SMS_Domain.Events;
using Microsoft.AspNetCore.Http.Features;

namespace SMS3.Components.Shared.UIHelpers;

#region Base Option Classes

/// <summary>
/// Abstract base class for all dropdown/selection option types
/// </summary>
/// <typeparam name="TValue">The type of the Value property</typeparam>
public abstract class BaseOption<TValue>
{
    public TValue Value { get; set; } = default!;
    public string Text { get; set; } = string.Empty;

    protected BaseOption() { }

    protected BaseOption(TValue value, string text)
    {
        Value = value;
        Text = text;
    }

    public override string ToString() => Text;

    public override bool Equals(object? obj)
    {
        if (obj is BaseOption<TValue> other)
            return EqualityComparer<TValue>.Default.Equals(Value, other.Value);
        return false;
    }

    public override int GetHashCode()
    {
        return Value?.GetHashCode() ?? 0;
    }
}

#endregion

#region Concrete Option Classes

/// <summary>
/// Standard dropdown option with string value
/// </summary>
public class DropdownOption : BaseOption<string>
{
    public DropdownOption() : base() { }
    public DropdownOption(string value, string text) : base(value, text) { }
}

/// <summary>
/// Filter option with string value (alias for DropdownOption for semantic clarity)
/// </summary>
public class FilterOption : BaseOption<string>
{
    public FilterOption() : base() { }
    public FilterOption(string value, string text) : base(value, text) { }
}

/// <summary>
/// Status option with boolean value
/// </summary>
public class StatusOption : BaseOption<bool>
{
    public StatusOption() : base() { }
    public StatusOption(bool value, string text) : base(value, text) { }
}

/// <summary>
/// Risk severity option with integer value
/// </summary>
public class SeverityOption : BaseOption<int>
{
    public SeverityOption() : base() { }
    public SeverityOption(int value, string text) : base(value, text) { }
}

/// <summary>
/// Risk likelihood option with integer value
/// </summary>
public class LikelihoodOption : BaseOption<int>
{
    public LikelihoodOption() : base() { }
    public LikelihoodOption(int value, string text) : base(value, text) { }
}

#endregion

#region Container Classes

public class StandardDropdowns
{
    public List<DropdownOption> HazardCategories { get; set; } = new();
    public List<DropdownOption> HazardTypes { get; set; } = new();
    public List<DropdownOption> Departments { get; set; } = new();
}

public class SPIDropdowns
{
    public List<SMSSafetyPerformanceIndicatorType> SPITypes { get; set; } = new();
    public List<SPIStatus> SPIStatuses { get; set; } = new();
    public List<SPIMeasurementFrequency> SPIFrequencies { get; set; } = new();
    public List<string> Departments { get; set; } = new();
}

#endregion

public static class DropdownHelper
{
       

    public static List<DropdownOption> GetHazardCategoryOptions()
    {
        return HazardCategory.GetAllValues()
            .Select(hc => new DropdownOption(hc.Value, hc.Name))
            .ToList();
    }

    public static List<DropdownOption> GetAllHazardTypeOptions()
    {
        return HazardType.GetAllValues()
            .Select(ht => new DropdownOption(ht.Value, ht.Name))
            .ToList();
    }

    public static List<DropdownOption> GetHazardTypeOptionsByCategory(string categoryValue)
    {
        if (string.IsNullOrEmpty(categoryValue))
            return new List<DropdownOption>();

        var category = HazardCategory.FromValue(categoryValue);
        if (category is null)
            return new List<DropdownOption>();

        return HazardType.GetByCategory(category)
            .Select(ht => new DropdownOption(ht.Value, ht.Name))
            .ToList();
    }

    public static List<DropdownOption> GetDepartmentOptions()
    {
        return SMSOrganization.GetAllValues()
            .Select(dept => new DropdownOption(dept.Value, dept.Name))
            .ToList();
    }

    public static List<string> GetDepartmentNames()
    {
        return SMSOrganization.GetAllDepartments()
            .Select(d => d.Name)
            .OrderBy(name => name)
            .ToList();
    }

    public static List<DropdownOption> GetRiskLevelOptions()
    {
        return RiskLevel.GetOrderedBySeverity()
            .Select(rl => new DropdownOption(rl.Value, rl.Name))
            .ToList();
    }

    public static List<DropdownOption> GetValidationDecisionOptions()
    {
        return ValidationDecision.GetAllValues()
            .Select(vd => new DropdownOption(vd.Value, vd.Name))
            .ToList();
    }

    public static List<DropdownOption> GetReportStatusOptions()
    {
        return ReportStatus.GetAllValues()
            .Select(rs => new DropdownOption(rs.Value, rs.Name))
            .ToList();
    }

    public static List<DropdownOption> GetHazardStatusOptions()
    {
        return HazardStatus.GetAllValues()
            .Select(hs => new DropdownOption(hs.Value, hs.Name))
            .ToList();
    }

    public static List<DropdownOption> GetInvestigationStatusOptions()
    {
        return InvestigationStatus.GetAllValues()
            .Select(invs => new DropdownOption(invs.Value, invs.Name))
            .ToList();
    }

    public static List<SMSSafetyPerformanceIndicatorType> GetSPITypeOptions()
    {
        return SMSSafetyPerformanceIndicatorType.GetAllValues().ToList();
    }

    public static List<SPIStatus> GetSPIStatusOptions()
    {
        return new List<SPIStatus>
        {
            SPIStatus.Active,
            SPIStatus.Inactive,
            SPIStatus.UnderReview,
            SPIStatus.Deprecated
        };
    }

    public static List<SPIMeasurementFrequency> GetSPIMeasurementFrequencyOptions()
    {
        return new List<SPIMeasurementFrequency>
        {
            SPIMeasurementFrequency.Daily,
            SPIMeasurementFrequency.Weekly,
            SPIMeasurementFrequency.Monthly,
            SPIMeasurementFrequency.Quarterly,
            SPIMeasurementFrequency.Annually
        };
    }

    public static (List<DropdownOption> HazardCategories, List<DropdownOption> HazardTypes, List<DropdownOption> Departments)
        InitializeHazardReportingDropdowns()
    {
        return (
            GetHazardCategoryOptions(),
            new List<DropdownOption>(),
            GetDepartmentOptions()
        );
    }

    public static StandardDropdowns InitializeStandardDropdowns()
    {
        return new StandardDropdowns
        {
            HazardCategories = GetHazardCategoryOptions(),
            HazardTypes = new List<DropdownOption>(),
            Departments = GetDepartmentOptions()
        };
    }

    public static SPIDropdowns InitializeSPIDropdowns()
    {
        return new SPIDropdowns
        {
            SPITypes = GetSPITypeOptions(),
            SPIStatuses = GetSPIStatusOptions(),
            SPIFrequencies = GetSPIMeasurementFrequencyOptions(),
            Departments = GetDepartmentNames()
        };
    }

    public static List<DropdownOption> HandleCategoryChange(string? categoryValue)
    {
        if (string.IsNullOrEmpty(categoryValue))
            return new List<DropdownOption>();

        return GetHazardTypeOptionsByCategory(categoryValue);
    }

    public static List<FilterOption> GenerateFilterOptions(IEnumerable<string> values, bool includeAll = true)
    {
        var options = new List<FilterOption>();

        if (includeAll)
            options.Add(new FilterOption("", "All"));

        options.AddRange(values.Where(v => !string.IsNullOrEmpty(v))
                               .Distinct()
                               .OrderBy(v => v)
                               .Select(v => new FilterOption(v, v)));

        return options;
    }

    public static List<FilterOption> GetRiskLevelFilterOptions()
    {
        var options = new List<FilterOption> { new("", "All Risk Levels") };
        options.AddRange(RiskLevel.GetOrderedBySeverity()
                                 .Select(rl => new FilterOption(rl.Value, rl.Name)));
        return options;
    }

    /// <summary>
    /// Get SMS Organization Level dropdown options ordered by authority level
    /// </summary>
    public static List<DropdownOption> GetOrganizationLevelOptions()
    {
        return SMSOrganizationalLevel.GetAllValues()
            .OrderByDescending(level => level.AuthorityLevel)
            .Select(level => new DropdownOption
            {
                Text = $"{level.Name} (Authority {level.AuthorityLevel})",
                Value = level.Name
            })
            .ToList();
    }

    /// <summary>
    /// Get Risk Severity dropdown options for hazard scoring
    /// </summary>
    public static List<SeverityOption> GetSeverityOptions()
    {
        return new List<SeverityOption>
        {
            new(1, "1 - Minor"),
            new(2, "2 - Moderate"),
            new(3, "3 - Serious"),
            new(4, "4 - Major"),
            new(5, "5 - Catastrophic")
        };
    }

    /// <summary>
    /// Get Risk Likelihood dropdown options for hazard scoring
    /// </summary>
    public static List<LikelihoodOption> GetLikelihoodOptions()
    {
        return new List<LikelihoodOption>
        {
            new(1, "A - Rare"),
            new(2, "B - Unlikely"),
            new(3, "C - Possible"),
            new(4, "D - Likely"),
            new(5, "E - Frequent")
        };
    }

    /// <summary>
    /// Get Investigation Decision dropdown options
    /// </summary>
    public static List<DropdownOption> GetInvestigationDecisionOptions()
    {
        return new List<DropdownOption>
        {
            new("NO_FURTHER_ACTION", "No Further Action"),
            new("CONTINUE_LATER", "Continue Later"),
            new("RETURN_TO_VALIDATION", "Return to Validation")
        };
    }
}

public static class EntityDisplayNameHelper
{
    public static string GetPluralEntityName<T>()
    {
        return GetPluralEntityName(typeof(T));
    }

    public static string GetPluralEntityName(Type type)
    {
        var typeName = type.Name;

        if (typeName.StartsWith("SMS", StringComparison.Ordinal))
        {
            typeName = typeName[3..];
        }

        var spacedName = Regex.Replace(typeName, "([a-z0-9])([A-Z])", "$1 $2");
        var parts = spacedName.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

        if (!parts.Any())
        {
            return "Items";
        }

        var lastIndex = parts.Count - 1;
        parts[lastIndex] = PluralizeWord(parts[lastIndex]);

        return string.Join(" ", parts);
    }

    private static string PluralizeWord(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            return "Items";
        }

        if (word.EndsWith("y", StringComparison.OrdinalIgnoreCase) && word.Length > 1 && !"aeiou".Contains(char.ToLowerInvariant(word[^2])))
        {
            return word[..^1] + "ies";
        }

        if (word.EndsWith("s", StringComparison.OrdinalIgnoreCase)
            || word.EndsWith("x", StringComparison.OrdinalIgnoreCase)
            || word.EndsWith("z", StringComparison.OrdinalIgnoreCase)
            || word.EndsWith("ch", StringComparison.OrdinalIgnoreCase)
            || word.EndsWith("sh", StringComparison.OrdinalIgnoreCase))
        {
            return word + "es";
        }

        return word + "s";
    }
}

/// <summary>
/// Interface for notification helper service
/// </summary>
public interface INotificationHelper
{
    Task ShowSuccessAsync(string message, int duration = 4000);
    Task ShowErrorAsync(string message, int duration = 6000);
    Task ShowInfoAsync(string message, int duration = 4000);
    Task ShowWarningAsync(string message, int duration = 5000);
}

/// <summary>
/// Notification helper service that routes all toasts through the EventBus UI notification pipeline
/// </summary>
public sealed class NotificationHelper : INotificationHelper
{
    private readonly IBaseEventBus _eventBus;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<NotificationHelper> _logger;

    public NotificationHelper(
        IBaseEventBus eventBus,
        ICurrentUserService currentUserService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<NotificationHelper> logger)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ShowSuccessAsync(string message, int duration = 4000)
    {
        await PublishNotificationAsync(UINotificationSeverity.Success, "Success", message, duration);
    }

    public async Task ShowErrorAsync(string message, int duration = 6000)
    {
        await PublishNotificationAsync(UINotificationSeverity.Error, "Error", message, duration);
    }

    public async Task ShowInfoAsync(string message, int duration = 4000)
    {
        await PublishNotificationAsync(UINotificationSeverity.Info, "Information", message, duration);
    }

    public async Task ShowWarningAsync(string message, int duration = 5000)
    {
        await PublishNotificationAsync(UINotificationSeverity.Warning, "Warning", message, duration);
    }

    private async Task PublishNotificationAsync(UINotificationSeverity severity, string title, string message, int duration)
    {
        var targetUserCode = _currentUserService?.UserCode;
        var targetSessionKey = BuildCurrentSessionKey();

        var metadata = new Dictionary<string, object>();

        if (!string.IsNullOrWhiteSpace(targetUserCode))
        {
            metadata["TargetUserCode"] = targetUserCode;
        }

        if (!string.IsNullOrWhiteSpace(targetSessionKey))
        {
            metadata["TargetSessionKey"] = targetSessionKey;
        }

        var uiEvent = new UINotificationEvent(
            severity,
            title,
            message,
            duration,
            category: "UserAction",
            sourceLayer: "Presentation",
            metadata: metadata);

        var result = await _eventBus.PublishUIEventAsync(uiEvent);
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to publish UI notification event: {ErrorMessage}", result.Error?.Message);
        }
    }

    private string BuildCurrentSessionKey()
    {
        var context = _httpContextAccessor.HttpContext;
        var sessionId = context?.Features.Get<ISessionFeature>()?.Session?.Id;

        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            return $"session:{sessionId}";
        }

        var userCode = _currentUserService?.UserCode ?? "unknown";
        var remoteIp = context?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = context?.Request?.Headers["User-Agent"].FirstOrDefault() ?? "unknown";

        return $"fallback:{userCode}:{remoteIp}:{userAgent.GetHashCode()}";
    }
}

/// <summary>
/// Simple service locator for accessing DI container from static contexts
/// </summary>
public static class ServiceLocator
{
    public static IServiceProvider? Current { get; set; }
}

public static class StatusOptions
{
    public static readonly List<StatusOption> ActiveInactiveOptions = new()
    {
        new StatusOption(true, "Active"),
        new StatusOption(false, "Inactive")
    };

    public static readonly List<StatusOption> YesNoOptions = new()
    {
        new StatusOption(true, "Yes"),
        new StatusOption(false, "No")
    };

    public static readonly List<StatusOption> EnabledDisabledOptions = new()
    {
        new StatusOption(true, "Enabled"),
        new StatusOption(false, "Disabled")
    };
}
