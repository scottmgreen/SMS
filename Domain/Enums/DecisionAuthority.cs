namespace SMS_Domain.Enums;

/// <summary>
/// Enhanced Decision Authority framework that integrates with SMSRole authority levels
/// Replaces the old simple decision authority system with proper SMS compliance
/// </summary>
public abstract class DecisionAuthority : BaseEnum<DecisionAuthority>
{
    protected DecisionAuthority(string value, string name, int requiredAuthorityLevel, string[] approverRoles, string description) : base(value, name)
    {
        RequiredAuthorityLevel = requiredAuthorityLevel;
        ApproverRoles = approverRoles;
        Description = description;
    }

    public int RequiredAuthorityLevel { get; }
    public string[] ApproverRoles { get; }
    public string Description { get; }

    #region Decision Authority Levels

    /// <summary>Critical Decision Authority - Requires Accountable Executive (Level 10)</summary>
    public static readonly DecisionAuthority Critical = new CriticalAuthority();

    /// <summary>High Decision Authority - Requires Responsible Executive or higher (Level 9+)</summary>
    public static readonly DecisionAuthority High = new HighAuthority();

    /// <summary>Medium Decision Authority - Requires Responsible Manager or higher (Level 8+)</summary>
    public static readonly DecisionAuthority Medium = new MediumAuthority();

    /// <summary>Low Decision Authority - Requires SMS Manager or higher (Level 7+)</summary>
    public static readonly DecisionAuthority Low = new LowAuthority();

    /// <summary>Standard Decision Authority - Requires SMS Coordinator or higher (Level 6+)</summary>
    public static readonly DecisionAuthority Standard = new StandardAuthority();

    #endregion

    #region Implementations

    private sealed class CriticalAuthority : DecisionAuthority
    {
        public CriticalAuthority() : base("CRITICAL", "Critical Decision Authority", 10,
            new[] { "ACCOUNTABLE_EXECUTIVE" },
            "Critical decisions requiring CEO/Accountable Executive approval - impacts strategic direction, regulatory compliance, or involves significant risk")
        {
        }
    }

    private sealed class HighAuthority : DecisionAuthority
    {
        public HighAuthority() : base("HIGH", "High Decision Authority", 9,
            new[] { "ACCOUNTABLE_EXECUTIVE", "RESPONSIBLE_EXECUTIVE" },
            "High-level decisions requiring executive approval - significant safety implications, policy changes, or major resource allocation")
        {
        }
    }

    private sealed class MediumAuthority : DecisionAuthority
    {
        public MediumAuthority() : base("MEDIUM", "Medium Decision Authority", 8,
            new[] { "ACCOUNTABLE_EXECUTIVE", "RESPONSIBLE_EXECUTIVE", "RESPONSIBLE_MANAGER" },
            "Medium-level decisions requiring management approval - operational changes, departmental policies, or moderate risk acceptance")
        {
        }
    }

    private sealed class LowAuthority : DecisionAuthority
    {
        public LowAuthority() : base("LOW", "Low Decision Authority", 7,
            new[] { "ACCOUNTABLE_EXECUTIVE", "RESPONSIBLE_EXECUTIVE", "RESPONSIBLE_MANAGER", "SMS_MANAGER" },
            "Low-level decisions requiring SMS management approval - routine operational matters, minor policy updates, or low-risk activities")
        {
        }
    }

    private sealed class StandardAuthority : DecisionAuthority
    {
        public StandardAuthority() : base("STANDARD", "Standard Decision Authority", 6,
            new[] { "ACCOUNTABLE_EXECUTIVE", "RESPONSIBLE_EXECUTIVE", "RESPONSIBLE_MANAGER", "SMS_MANAGER", "SMS_COORDINATOR" },
            "Standard decisions within normal operational scope - routine processes, standard procedures, or administrative matters")
        {
        }
    }

    #endregion

    /// <summary>
    /// Checks if the given role can approve decisions at this authority level
    /// </summary>
    //public bool CanApprove(SMSRole role)
    //{
    //    if (role == null) return false;

    //    return role.AuthorityLevel >= RequiredAuthorityLevel || 
    //           ApproverRoles.Contains(role.Value);
    //}

    /// <summary>
    /// Checks if a user with the given authority level can approve decisions at this level
    /// </summary>
    public bool CanApprove(int userAuthorityLevel)
    {
        return userAuthorityLevel >= RequiredAuthorityLevel;
    }

    /// <summary>
    /// Gets all roles that can approve at this authority level
    /// </summary>
    //public IEnumerable<SMSRole> GetApproverRoles()
    //{
    //    return SMSRole.GetAllValues().Where(role => CanApprove(role));
    //}

    /// <summary>
    /// Gets the minimum role required for approval at this level
    /// </summary>
    //public SMSApplicationUserRole GetMinimumRequiredRole()
    //{
    //    return SMSApplicationUserRoleID..GetAllValues()
    //        .Where(role => role.AuthorityLevel >= RequiredAuthorityLevel)
    //        .OrderBy(role => role.AuthorityLevel)
    //        .First();
    //}

    /// <summary>
    /// Gets escalation authority level (next higher level)
    /// </summary>
    public DecisionAuthority? GetEscalationLevel()
    {
        return this switch
        {
            _ when this == Standard => Low,
            _ when this == Low => Medium,
            _ when this == Medium => High,
            _ when this == High => Critical,
            _ when this == Critical => null, // No escalation above critical
            _ => null
        };
    }

    /// <summary>
    /// Gets all available decision authority values
    /// </summary>
    public static IEnumerable<DecisionAuthority> GetAllValues()
    {
        return typeof(DecisionAuthority)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(DecisionAuthority))
            .Select(f => (DecisionAuthority)f.GetValue(null)!)
            .Where(da => da != null)
            .OrderByDescending(da => da.RequiredAuthorityLevel);
    }

    /// <summary>
    /// Gets decision authority levels by minimum required authority
    /// </summary>
    public static IEnumerable<DecisionAuthority> GetAuthorityLevelsByRequirement(int minAuthorityLevel)
    {
        return GetAllValues().Where(da => da.RequiredAuthorityLevel >= minAuthorityLevel);
    }

    /// <summary>
    /// Gets appropriate decision authority for a given risk level
    /// </summary>
    public static DecisionAuthority GetAuthorityForRiskLevel(RiskLevel riskLevel)
    {
        return riskLevel.Value switch
        {
            "CRITICAL" => Critical,
            "HIGH" => High,
            "MEDIUM" => Medium,
            "LOW" => Low,
            _ => Standard
        };
    }

    /// <summary>
    /// Gets appropriate decision authority for a committee type
    /// </summary>
    public static DecisionAuthority GetAuthorityForCommitteeType(CommitteeType committeeType)
    {
        return committeeType.AuthorityLevel switch
        {
            >= 10 => Critical,
            >= 9 => High,
            >= 8 => Medium,
            >= 7 => Low,
            _ => Standard
        };
    }

    /// <summary>
    /// Checks if this is a critical decision authority
    /// </summary>
    public bool IsCritical => RequiredAuthorityLevel >= 10;

    /// <summary>
    /// Checks if this requires executive approval
    /// </summary>
    public bool RequiresExecutiveApproval => RequiredAuthorityLevel >= 9;

    /// <summary>
    /// Checks if this is a high-level decision
    /// </summary>
    public bool IsHighLevel => RequiredAuthorityLevel >= 8;

    /// <summary>
    /// Gets the decision complexity level
    /// </summary>
    public string GetComplexityLevel()
    {
        return RequiredAuthorityLevel switch
        {
            >= 10 => "Strategic",
            >= 9 => "Executive",
            >= 8 => "Managerial",
            >= 7 => "Supervisory",
            _ => "Operational"
        };
    }

    /// <summary>
    /// Validates if escalation is allowed from this authority level
    /// </summary>
    public bool CanEscalate()
    {
        return GetEscalationLevel() != null;
    }

    /// <summary>
    /// Gets the typical approval timeframe for this authority level
    /// </summary>
    public TimeSpan GetTypicalApprovalTimeframe()
    {
        return RequiredAuthorityLevel switch
        {
            >= 10 => TimeSpan.FromDays(7),   // Critical: 1 week
            >= 9 => TimeSpan.FromDays(5),    // High: 5 days
            >= 8 => TimeSpan.FromDays(3),    // Medium: 3 days
            >= 7 => TimeSpan.FromDays(2),    // Low: 2 days
            _ => TimeSpan.FromDays(1)        // Standard: 1 day
        };
    }
}