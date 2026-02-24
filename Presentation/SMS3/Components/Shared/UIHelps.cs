using SMS_Domain.Enums;
using Radzen;
using SMS3.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;

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
    public List<SPIType> SPITypes { get; set; } = new();
    public List<SPIStatus> SPIStatuses { get; set; } = new();
    public List<SPIMeasurementFrequency> SPIFrequencies { get; set; } = new();
    public List<string> Departments { get; set; } = new();
}

#endregion

public static class DropdownHelper
{
    public static List<DropdownOption> GroupTypeOptions = new()
    {
        new() { Text = "Department", Value = "Department" },
        new() { Text = "SMS Role", Value = "SMS Role" },
        new() { Text = "Committee", Value = "Committee" },
        new() { Text = "Work Group", Value = "Work Group" },
        new() { Text = "Management Team", Value = "Management Team" }
    };

    public static List<DropdownOption> AuthorityLevelOptions = new()
    {
        new() { Text = "Strategic", Value = "Strategic" },
        new() { Text = "Executive", Value = "Executive" },
        new() { Text = "Operational", Value = "Operational" },
        new() { Text = "Process", Value = "Process" },
        new() { Text = "Support", Value = "Support" }
    };

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
        if (category == null)
            return new List<DropdownOption>();

        return HazardType.GetByCategory(category)
            .Select(ht => new DropdownOption(ht.Value, ht.Name))
            .ToList();
    }

    public static List<DropdownOption> GetDepartmentOptions()
    {
        return SMSDepartment.GetAllValues()
            .Select(dept => new DropdownOption(dept.Value, dept.Name))
            .ToList();
    }

    public static List<string> GetDepartmentNames()
    {
        return SMSDepartment.GetAllDepartments()
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

    public static List<SPIType> GetSPITypeOptions()
    {
        return SPIType.GetAllValues().ToList();
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

/// <summary>
/// Centralized notification helper methods with configuration support
/// </summary>
public static class NotificationHelper
{
    /// <summary>
    /// Show success notification (respects configuration)
    /// </summary>
    public static void ShowSuccess(NotificationService notificationService, string message, int duration = 4000, NotificationSettings? settings = null)
    {
        // If settings not provided, try to get from current HTTP context
        settings ??= GetNotificationSettings();
        
        if (settings?.AllowSuccessNotifications == false) return;
        
        notificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = "Success",
            Detail = message,
            Duration = duration
        });
    }

    /// <summary>
    /// Show error notification (respects configuration)
    /// </summary>
    public static void ShowError(NotificationService notificationService, string message, int duration = 6000, NotificationSettings? settings = null)
    {
        // If settings not provided, try to get from current HTTP context
        settings ??= GetNotificationSettings();
        
        if (settings?.AllowErrorNotifications == false) return;
        
        notificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = "Error",
            Detail = message,
            Duration = duration
        });
    }

    /// <summary>
    /// Show info notification (respects configuration)
    /// </summary>
    public static void ShowInfo(NotificationService notificationService, string message, int duration = 4000, NotificationSettings? settings = null)
    {
        // If settings not provided, try to get from current HTTP context
        settings ??= GetNotificationSettings();
        
        if (settings?.AllowInfoNotifications == false) return;
        
        notificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Info,
            Summary = "Information",
            Detail = message,
            Duration = duration
        });
    }

    /// <summary>
    /// Show warning notification (respects configuration)
    /// </summary>
    public static void ShowWarning(NotificationService notificationService, string message, int duration = 5000, NotificationSettings? settings = null)
    {
        // If settings not provided, try to get from current HTTP context
        settings ??= GetNotificationSettings();
        
        if (settings?.AllowWarningNotifications == false) return;
        
        notificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Warning,
            Summary = "Warning",
            Detail = message,
            Duration = duration
        });
    }

    /// <summary>
    /// Try to get NotificationSettings from current HTTP context
    /// </summary>
    private static NotificationSettings? GetNotificationSettings()
    {
        try
        {
            // Access current HTTP context to get services
            var httpContextAccessor = ServiceLocator.Current?.GetService<IHttpContextAccessor>();
            if (httpContextAccessor?.HttpContext?.RequestServices == null)
                return null;

            var optionsAccessor = httpContextAccessor.HttpContext.RequestServices
                .GetService<IOptions<NotificationSettings>>();
            
            return optionsAccessor?.Value;
        }
        catch
        {
            // If we can't get the settings, return null and let notifications through
            // (fail-open approach for better user experience)
            return null;
        }
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