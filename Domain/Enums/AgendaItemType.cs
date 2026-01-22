namespace SMS_Domain.Enums;

/// <summary>
/// Agenda item types for meeting organization and categorization
/// </summary>
public abstract class AgendaItemType : BaseEnum<AgendaItemType>
{
    protected AgendaItemType(string value, string name, string description, int priority, bool requiresVoting) : base(value, name)
    {
        Description = description;
        Priority = priority;
        RequiresVoting = requiresVoting;
    }

    public string Description { get; }
    public int Priority { get; }
    public bool RequiresVoting { get; }

    #region Agenda Item Types

    /// <summary>Review of hazard reports and assessments</summary>
    public static readonly AgendaItemType HazardReview = new HazardReviewType();

    /// <summary>Risk assessment and mitigation planning</summary>
    public static readonly AgendaItemType RiskAssessment = new RiskAssessmentType();

    /// <summary>General business and administrative matters</summary>
    public static readonly AgendaItemType General = new GeneralType();

    /// <summary>Action item review and status updates</summary>
    public static readonly AgendaItemType ActionItems = new ActionItemsType();

    /// <summary>Policy review and updates</summary>
    public static readonly AgendaItemType PolicyReview = new PolicyReviewType();

    /// <summary>Training and educational topics</summary>
    public static readonly AgendaItemType Training = new TrainingType();

    /// <summary>Incident investigation findings</summary>
    public static readonly AgendaItemType Investigation = new InvestigationType();

    /// <summary>Performance metrics and reporting</summary>
    public static readonly AgendaItemType Performance = new PerformanceType();

    /// <summary>Regulatory compliance matters</summary>
    public static readonly AgendaItemType Compliance = new ComplianceType();

    #endregion

    #region Implementations

    private sealed class HazardReviewType : AgendaItemType
    {
        public HazardReviewType() : base("HAZARD_REVIEW", "Hazard Review",
            "Review and discussion of hazard reports and safety concerns", 9, true)
        {
        }
    }

    private sealed class RiskAssessmentType : AgendaItemType
    {
        public RiskAssessmentType() : base("RISK_ASSESSMENT", "Risk Assessment",
            "Risk evaluation, analysis, and mitigation strategy development", 10, true)
        {
        }
    }

    private sealed class GeneralType : AgendaItemType
    {
        public GeneralType() : base("GENERAL", "General Business",
            "General business matters and administrative items", 3, false)
        {
        }
    }

    private sealed class ActionItemsType : AgendaItemType
    {
        public ActionItemsType() : base("ACTION_ITEMS", "Action Items",
            "Review of action items, progress updates, and follow-up activities", 6, false)
        {
        }
    }

    private sealed class PolicyReviewType : AgendaItemType
    {
        public PolicyReviewType() : base("POLICY_REVIEW", "Policy Review",
            "Review and approval of policies, procedures, and documentation", 7, true)
        {
        }
    }

    private sealed class TrainingType : AgendaItemType
    {
        public TrainingType() : base("TRAINING", "Training",
            "Training topics, educational content, and competency development", 4, false)
        {
        }
    }

    private sealed class InvestigationType : AgendaItemType
    {
        public InvestigationType() : base("INVESTIGATION", "Investigation",
            "Investigation findings, root cause analysis, and corrective actions", 8, true)
        {
        }
    }

    private sealed class PerformanceType : AgendaItemType
    {
        public PerformanceType() : base("PERFORMANCE", "Performance",
            "Performance metrics, KPIs, and safety performance monitoring", 5, false)
        {
        }
    }

    private sealed class ComplianceType : AgendaItemType
    {
        public ComplianceType() : base("COMPLIANCE", "Compliance",
            "Regulatory compliance matters and audit findings", 8, true)
        {
        }
    }

    #endregion

    /// <summary>
    /// Gets agenda item types that require voting
    /// </summary>
    public static IEnumerable<AgendaItemType> GetVotingItemTypes()
    {
        return GetAllValues().Where(ait => ait.RequiresVoting);
    }

    /// <summary>
    /// Gets agenda item types by priority level
    /// </summary>
    public static IEnumerable<AgendaItemType> GetItemTypesByPriority(int minPriority)
    {
        return GetAllValues().Where(ait => ait.Priority >= minPriority);
    }

    /// <summary>
    /// Checks if this is a high-priority agenda item type
    /// </summary>
    public bool IsHighPriority => Priority >= 8;

    /// <summary>
    /// Checks if this item type is safety-critical
    /// </summary>
    public bool IsSafetyCritical => Priority >= 7 && (this == HazardReview || this == RiskAssessment || this == Investigation);
}