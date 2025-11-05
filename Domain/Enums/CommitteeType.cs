using System.Reflection;
using SMS_Domain.Common;

namespace SMS_Domain.Enums;

/// <summary>
/// Committee types for SMS governance and decision-making processes
/// Each committee serves specific functions in the SMS framework
/// </summary>
public abstract class CommitteeType : BaseEnum<CommitteeType>
{
    protected CommitteeType(string value, string name, string description, string[] responsibilities, int authorityLevel) : base(value, name)
    {
        Description = description;
        Responsibilities = responsibilities;
        AuthorityLevel = authorityLevel;
    }

    public string Description { get; }
    public string[] Responsibilities { get; }
    public int AuthorityLevel { get; }

    #region Committee Types

    /// <summary>SMS Rapid Review Team - Initial hazard triage and assessment</summary>
    public static readonly CommitteeType RapidReviewTeam = new RapidReviewTeamType();

    /// <summary>SMS Airside Safety Committee - Operational safety coordination and review</summary>
    public static readonly CommitteeType AirsideSafety = new AirsideSafetyType();

    /// <summary>SMS Executive Committee - Strategic oversight and critical decision making</summary>
    public static readonly CommitteeType Executive = new ExecutiveType();

    /// <summary>Risk Assessment Committee - Detailed risk evaluation and mitigation planning</summary>
    public static readonly CommitteeType RiskAssessment = new RiskAssessmentType();

    /// <summary>Safety Review Committee - Periodic safety performance review</summary>
    public static readonly CommitteeType SafetyReview = new SafetyReviewType();

    /// <summary>Incident Review Committee - Investigation oversight and corrective action</summary>
    public static readonly CommitteeType IncidentReview = new IncidentReviewType();

    /// <summary>Stakeholder Advisory Committee - External stakeholder engagement</summary>
    public static readonly CommitteeType StakeholderAdvisory = new StakeholderAdvisoryType();

    /// <summary>Training and Development Committee - Safety training and competency oversight</summary>
    public static readonly CommitteeType TrainingDevelopment = new TrainingDevelopmentType();

    /// <summary>Emergency Response Committee - Emergency preparedness and response coordination</summary>
    public static readonly CommitteeType EmergencyResponse = new EmergencyResponseType();

    #endregion

    #region Implementations

    private sealed class RapidReviewTeamType : CommitteeType
    {
        public RapidReviewTeamType() : base("RAPID_REVIEW_TEAM", "SMS Rapid Review Team",
            "Initial hazard triage and assessment for immediate safety concerns",
            new[] { "Hazard Triage", "Initial Assessment", "Emergency Response", "Immediate Actions" }, 6)
        {
        }
    }

    private sealed class AirsideSafetyType : CommitteeType
    {
        public AirsideSafetyType() : base("AIRSIDE_SAFETY", "SMS Airside Safety Committee",
            "Operational safety coordination and review for airside operations",
            new[] { "Airside Operations", "Safety Coordination", "Operational Review", "Risk Management" }, 7)
        {
        }
    }

    private sealed class ExecutiveType : CommitteeType
    {
        public ExecutiveType() : base("EXECUTIVE", "SMS Executive Committee",
            "Strategic oversight and critical decision making for SMS program",
            new[] { "Strategic Oversight", "Critical Decisions", "Resource Allocation", "Policy Direction" }, 10)
        {
        }
    }

    private sealed class RiskAssessmentType : CommitteeType
    {
        public RiskAssessmentType() : base("RISK_ASSESSMENT", "Risk Assessment Committee",
            "Detailed risk evaluation and mitigation planning",
            new[] { "Risk Evaluation", "Mitigation Planning", "Risk Analysis", "Assessment Review" }, 8)
        {
        }
    }

    private sealed class SafetyReviewType : CommitteeType
    {
        public SafetyReviewType() : base("SAFETY_REVIEW", "Safety Review Committee",
            "Periodic safety performance review and trend analysis",
            new[] { "Performance Review", "Trend Analysis", "Safety Metrics", "Continuous Improvement" }, 7)
        {
        }
    }

    private sealed class IncidentReviewType : CommitteeType
    {
        public IncidentReviewType() : base("INCIDENT_REVIEW", "Incident Review Committee",
            "Investigation oversight and corrective action implementation",
            new[] { "Investigation Oversight", "Corrective Actions", "Root Cause Analysis", "Lessons Learned" }, 8)
        {
        }
    }

    private sealed class StakeholderAdvisoryType : CommitteeType
    {
        public StakeholderAdvisoryType() : base("STAKEHOLDER_ADVISORY", "Stakeholder Advisory Committee",
            "External stakeholder engagement and coordination",
            new[] { "Stakeholder Engagement", "External Coordination", "Advisory Input", "Collaboration" }, 5)
        {
        }
    }

    private sealed class TrainingDevelopmentType : CommitteeType
    {
        public TrainingDevelopmentType() : base("TRAINING_DEVELOPMENT", "Training and Development Committee",
            "Safety training and competency oversight",
            new[] { "Training Development", "Competency Management", "Educational Programs", "Skills Assessment" }, 6)
        {
        }
    }

    private sealed class EmergencyResponseType : CommitteeType
    {
        public EmergencyResponseType() : base("EMERGENCY_RESPONSE", "Emergency Response Committee",
            "Emergency preparedness and response coordination",
            new[] { "Emergency Preparedness", "Response Coordination", "Crisis Management", "Emergency Planning" }, 9)
        {
        }
    }

    #endregion

    /// <summary>
    /// Checks if this committee type has a specific responsibility
    /// </summary>
    public bool HasResponsibility(string responsibility)
    {
        return Responsibilities.Contains(responsibility, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets committee types by responsibility area
    /// </summary>
    public static IEnumerable<CommitteeType> GetCommitteeTypesByResponsibility(string responsibility)
    {
        return GetAllValues().Where(ct => ct.HasResponsibility(responsibility));
    }

    /// <summary>
    /// Gets committee types by minimum authority level
    /// </summary>
    public static IEnumerable<CommitteeType> GetCommitteeTypesByAuthorityLevel(int minAuthorityLevel)
    {
        return GetAllValues().Where(ct => ct.AuthorityLevel >= minAuthorityLevel);
    }

    /// <summary>
    /// Gets all available committee types
    /// </summary>
    public static IEnumerable<CommitteeType> GetAllValues()
    {
        return typeof(CommitteeType)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(CommitteeType))
            .Select(f => (CommitteeType)f.GetValue(null)!)
            .Where(ct => ct != null);
    }

    /// <summary>
    /// Checks if this is a high-authority committee
    /// </summary>
    public bool IsHighAuthorityCommittee => AuthorityLevel >= 8;

    /// <summary>
    /// Checks if this committee can make critical decisions
    /// </summary>
    public bool CanMakeCriticalDecisions => AuthorityLevel >= 9;
}