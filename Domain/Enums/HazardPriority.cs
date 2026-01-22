namespace SMS_Domain.Enums;

/// <summary>
/// Hazard priority enumeration for risk-based prioritization
/// Provides rich behavior and metadata for hazard priority management
/// </summary>
public abstract class HazardPriority : BaseEnum<HazardPriority>
{
    protected HazardPriority(string value, string name, string description, int level, string riskCategory, int responseDays, bool requiresImmediateAction) : base(value, name)
    {
        Description = description;
        Level = level;
        RiskCategory = riskCategory;
        ResponseDays = responseDays;
        RequiresImmediateAction = requiresImmediateAction;
    }

    public string Description { get; }
    public int Level { get; }
    public string RiskCategory { get; }
    public int ResponseDays { get; }
    public bool RequiresImmediateAction { get; }

    #region Hazard Priority Types

    /// <summary>Low priority hazard with minimal safety impact</summary>
    public static readonly HazardPriority Low = new LowPriority();

    /// <summary>Medium priority hazard requiring routine attention</summary>
    public static readonly HazardPriority Medium = new MediumPriority();

    /// <summary>High priority hazard requiring prompt attention</summary>
    public static readonly HazardPriority High = new HighPriority();

    /// <summary>Critical priority hazard requiring immediate action</summary>
    public static readonly HazardPriority Critical = new CriticalPriority();

    #endregion

    #region Implementations

    private sealed class LowPriority : HazardPriority
    {
        public LowPriority() : base("LOW", "Low",
            "Low risk hazard with minimal safety impact requiring routine monitoring", 1, "Low Risk", 30, false)
        {
        }
    }

    private sealed class MediumPriority : HazardPriority
    {
        public MediumPriority() : base("MEDIUM", "Medium",
            "Medium risk hazard requiring timely attention and appropriate mitigation measures", 2, "Medium Risk", 14, false)
        {
        }
    }

    private sealed class HighPriority : HazardPriority
    {
        public HighPriority() : base("HIGH", "High",
            "High risk hazard requiring prompt investigation and immediate mitigation planning", 3, "High Risk", 7, false)
        {
        }
    }

    private sealed class CriticalPriority : HazardPriority
    {
        public CriticalPriority() : base("CRITICAL", "Critical",
            "Critical risk hazard requiring immediate action and emergency response procedures", 4, "Critical Risk", 1, true)
        {
        }
    }

    #endregion

    /// <summary>
    /// Gets all available hazard priority values
    /// </summary>
    public static IEnumerable<HazardPriority> GetAllValues()
    {
        return typeof(HazardPriority)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(HazardPriority))
            .Select(f => (HazardPriority)f.GetValue(null)!)
            .Where(hp => hp != null)
            .OrderBy(hp => hp.Level);
    }

    /// <summary>
    /// Gets high-priority values requiring urgent attention
    /// </summary>
    public static IEnumerable<HazardPriority> GetHighPriorityValues()
    {
        return GetAllValues().Where(hp => hp.Level >= 3);
    }

    /// <summary>
    /// Gets priorities requiring immediate action
    /// </summary>
    public static IEnumerable<HazardPriority> GetImmediateActionPriorities()
    {
        return GetAllValues().Where(hp => hp.RequiresImmediateAction);
    }

    /// <summary>
    /// Gets priority by risk level (1-5 scale)
    /// </summary>
    public static HazardPriority FromRiskLevel(int riskLevel)
    {
        return riskLevel switch
        {
            1 => Low,
            2 => Medium,
            3 => Medium,
            4 => High,
            5 => Critical,
            _ => Medium
        };
    }

    /// <summary>
    /// Determines if this priority level is higher than the comparison priority
    /// </summary>
    public bool IsHigherThan(HazardPriority other)
    {
        return Level > other.Level;
    }

    /// <summary>
    /// Determines if this priority level is lower than the comparison priority
    /// </summary>
    public bool IsLowerThan(HazardPriority other)
    {
        return Level < other.Level;
    }

    /// <summary>
    /// Determines if this priority is at or above the specified level
    /// </summary>
    public bool IsAtOrAboveLevel(int level)
    {
        return Level >= level;
    }

    /// <summary>
    /// Gets the response deadline based on current date and response time
    /// </summary>
    public DateTime GetResponseDeadline(DateTime fromDate)
    {
        return fromDate.AddDays(ResponseDays);
    }

    /// <summary>
    /// Determines if response deadline has been exceeded
    /// </summary>
    public bool IsOverdue(DateTime hazardReportedDate, DateTime currentDate)
    {
        var deadline = GetResponseDeadline(hazardReportedDate);
        return currentDate > deadline;
    }

    /// <summary>
    /// Gets the urgency indicator for UI display
    /// </summary>
    public string GetUrgencyIndicator()
    {
        return this switch
        {
            var p when p == Critical => "?? CRITICAL",
            var p when p == High => "?? HIGH",
            var p when p == Medium => "?? MEDIUM",
            var p when p == Low => "?? LOW",
            _ => "? UNKNOWN"
        };
    }

    /// <summary>
    /// Gets the CSS class for styling based on priority
    /// </summary>
    public string GetCssClass()
    {
        return this switch
        {
            var p when p == Critical => "priority-critical",
            var p when p == High => "priority-high",
            var p when p == Medium => "priority-medium",
            var p when p == Low => "priority-low",
            _ => "priority-default"
        };
    }

    /// <summary>
    /// Escalates priority to the next higher level
    /// </summary>
    public HazardPriority Escalate()
    {
        return this switch
        {
            var p when p == Low => Medium,
            var p when p == Medium => High,
            var p when p == High => Critical,
            var p when p == Critical => Critical, // Cannot escalate beyond critical
            _ => this
        };
    }

    /// <summary>
    /// Determines if priority escalation is possible
    /// </summary>
    public bool CanEscalate => this != Critical;

    /// <summary>
    /// Determines if this is a high-risk priority
    /// </summary>
    public bool IsHighRisk => Level >= 3;

    /// <summary>
    /// Determines if this priority requires management notification
    /// </summary>
    public bool RequiresManagementNotification => Level >= 3 || RequiresImmediateAction;
}