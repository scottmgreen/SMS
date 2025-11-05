using System.Reflection;
using SMS_Domain.Common;

namespace SMS_Domain.Enums;

/// <summary>
/// Risk levels for approval workflow and decision authority
/// Integrates with SMSRole authority levels for proper approval routing
/// </summary>
public abstract class RiskLevel : BaseEnum<RiskLevel>
{
    protected RiskLevel(string value, string name, string description, int requiredAuthorityLevel, string[] approverRoles) : base(value, name)
    {
        Description = description;
        RequiredAuthorityLevel = requiredAuthorityLevel;
        ApproverRoles = approverRoles;
    }

    public string Description { get; }
    public int RequiredAuthorityLevel { get; }
    public string[] ApproverRoles { get; }

    #region Risk Levels

    /// <summary>Critical risk requiring Accountable Executive approval</summary>
    public static readonly RiskLevel Critical = new CriticalLevel();

    /// <summary>High risk requiring Responsible Executive or higher approval</summary>
    public static readonly RiskLevel High = new HighLevel();

    /// <summary>Medium risk requiring Responsible Manager or higher approval</summary>
    public static readonly RiskLevel Medium = new MediumLevel();

    /// <summary>Low risk requiring SMS Manager or higher approval</summary>
    public static readonly RiskLevel Low = new LowLevel();

    #endregion

    #region Implementations

    private sealed class CriticalLevel : RiskLevel
    {
        public CriticalLevel() : base("CRITICAL", "Critical Risk",
            "Critical risk requiring immediate action and Accountable Executive approval", 10,
            new[] { "ACCOUNTABLE_EXECUTIVE" })
        {
        }
    }

    private sealed class HighLevel : RiskLevel
    {
        public HighLevel() : base("HIGH", "High Risk",
            "High risk requiring Responsible Executive or Accountable Executive approval", 9,
            new[] { "ACCOUNTABLE_EXECUTIVE", "RESPONSIBLE_EXECUTIVE" })
        {
        }
    }

    private sealed class MediumLevel : RiskLevel
    {
        public MediumLevel() : base("MEDIUM", "Medium Risk",
            "Medium risk requiring Responsible Manager or higher approval", 8,
            new[] { "ACCOUNTABLE_EXECUTIVE", "RESPONSIBLE_EXECUTIVE", "RESPONSIBLE_MANAGER" })
        {
        }
    }

    private sealed class LowLevel : RiskLevel
    {
        public LowLevel() : base("LOW", "Low Risk",
            "Low risk requiring SMS Manager or higher approval", 7,
            new[] { "ACCOUNTABLE_EXECUTIVE", "RESPONSIBLE_EXECUTIVE", "RESPONSIBLE_MANAGER", "SMS_MANAGER" })
        {
        }
    }

    #endregion

    /// <summary>
    /// Checks if the given role can approve decisions at this risk level
    /// </summary>
    public bool CanApprove(SMSRole role)
    {
        return role.AuthorityLevel >= RequiredAuthorityLevel || 
               ApproverRoles.Contains(role.Value);
    }

    /// <summary>
    /// Gets all available risk levels
    /// </summary>
    public static IEnumerable<RiskLevel> GetAllValues()
    {
        return typeof(RiskLevel)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(RiskLevel))
            .Select(f => (RiskLevel)f.GetValue(null)!)
            .Where(rl => rl != null);
    }

    /// <summary>
    /// Gets risk levels that can be approved by the given authority level
    /// </summary>
    public static IEnumerable<RiskLevel> GetApprovableRiskLevels(int authorityLevel)
    {
        return GetAllValues().Where(rl => rl.RequiredAuthorityLevel <= authorityLevel);
    }

    /// <summary>
    /// Checks if this is a critical risk level
    /// </summary>
    public bool IsCritical => RequiredAuthorityLevel >= 10;

    /// <summary>
    /// Checks if this risk requires executive approval
    /// </summary>
    public bool RequiresExecutiveApproval => RequiredAuthorityLevel >= 9;

    /// <summary>
    /// Gets the escalation risk level (next higher level)
    /// </summary>
    public RiskLevel? GetEscalationLevel()
    {
        return this switch
        {
            _ when this == Low => Medium,
            _ when this == Medium => High,
            _ when this == High => Critical,
            _ when this == Critical => null,
            _ => null
        };
    }
}