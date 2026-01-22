namespace SMS_Domain.Enums;

/// <summary>
/// Hazard status enumeration for tracking hazard lifecycle
/// Provides rich behavior and metadata for hazard workflow management
/// </summary>
public abstract class HazardStatus : BaseEnum<HazardStatus>
{
    protected HazardStatus(string value, string name, string description, bool allowsModification, bool requiresApproval, int workflowOrder) : base(value, name)
    {
        Description = description;
        AllowsModification = allowsModification;
        RequiresApproval = requiresApproval;
        WorkflowOrder = workflowOrder;
    }

    public string Description { get; }
    public bool AllowsModification { get; }
    public bool RequiresApproval { get; }
    public int WorkflowOrder { get; }

    #region Hazard Status Types

    /// <summary>Hazard is active and being tracked</summary>
    public static readonly HazardStatus Active = new ActiveStatus();

    /// <summary>Hazard is under active investigation</summary>
    public static readonly HazardStatus UnderInvestigation = new UnderInvestigationStatus();

    /// <summary>Hazard is under management review</summary>
    public static readonly HazardStatus UnderReview = new UnderReviewStatus();

    /// <summary>Hazard has been closed and resolved</summary>
    public static readonly HazardStatus Closed = new ClosedStatus();

    /// <summary>Hazard has been cancelled</summary>
    public static readonly HazardStatus Cancelled = new CancelledStatus();

    /// <summary>Hazard processing is on hold</summary>
    public static readonly HazardStatus OnHold = new OnHoldStatus();

    #endregion

    #region Implementations

    private sealed class ActiveStatus : HazardStatus
    {
        public ActiveStatus() : base("ACTIVE", "Active",
            "Hazard is active and being tracked in the system", true, false, 1)
        {
        }
    }

    private sealed class UnderInvestigationStatus : HazardStatus
    {
        public UnderInvestigationStatus() : base("UNDER_INVESTIGATION", "Under Investigation",
            "Hazard is under active investigation to determine root causes and impacts", false, false, 2)
        {
        }
    }

    private sealed class UnderReviewStatus : HazardStatus
    {
        public UnderReviewStatus() : base("UNDER_REVIEW", "Under Review",
            "Hazard is under management review for risk assessment and mitigation decisions", false, true, 3)
        {
        }
    }

    private sealed class ClosedStatus : HazardStatus
    {
        public ClosedStatus() : base("CLOSED", "Closed",
            "Hazard has been resolved and closed with appropriate mitigations implemented", false, true, 5)
        {
        }
    }

    private sealed class CancelledStatus : HazardStatus
    {
        public CancelledStatus() : base("CANCELLED", "Cancelled",
            "Hazard has been cancelled due to invalid submission or duplicate entry", false, true, 6)
        {
        }
    }

    private sealed class OnHoldStatus : HazardStatus
    {
        public OnHoldStatus() : base("ON_HOLD", "On Hold",
            "Hazard processing is temporarily on hold pending additional information or resources", true, false, 4)
        {
        }
    }

    #endregion

    /// <summary>
    /// Gets all available hazard status values
    /// </summary>
    public static IEnumerable<HazardStatus> GetAllValues()
    {
        return typeof(HazardStatus)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(HazardStatus))
            .Select(f => (HazardStatus)f.GetValue(null)!)
            .Where(hs => hs != null)
            .OrderBy(hs => hs.WorkflowOrder);
    }

    /// <summary>
    /// Gets active status values that allow processing
    /// </summary>
    public static IEnumerable<HazardStatus> GetActiveStatuses()
    {
        return GetAllValues().Where(hs => hs.AllowsModification || hs == Active || hs == UnderInvestigation);
    }

    /// <summary>
    /// Gets final status values that conclude hazard processing
    /// </summary>
    public static IEnumerable<HazardStatus> GetFinalStatuses()
    {
        return GetAllValues().Where(hs => hs == Closed || hs == Cancelled);
    }

    /// <summary>
    /// Determines if this status is a final state
    /// </summary>
    public bool IsFinalStatus => this == Closed || this == Cancelled;

    /// <summary>
    /// Determines if this status is an active processing state
    /// </summary>
    public bool IsActiveStatus => this == Active || this == UnderInvestigation || this == UnderReview || this == OnHold;

    /// <summary>
    /// Determines if this status allows hazard to be reopened
    /// </summary>
    public bool CanBeReopened => this == Closed || this == OnHold;

    /// <summary>
    /// Determines if this status requires management approval for changes
    /// </summary>
    public bool RequiresManagementApproval => RequiresApproval;

    /// <summary>
    /// Gets the next logical workflow status
    /// </summary>
    public HazardStatus? GetNextWorkflowStatus()
    {
        return this switch
        {
            var s when s == Active => UnderInvestigation,
            var s when s == UnderInvestigation => UnderReview,
            var s when s == UnderReview => Closed,
            var s when s == OnHold => Active,
            _ => null
        };
    }

    /// <summary>
    /// Validates if transition to target status is allowed
    /// </summary>
    public bool CanTransitionTo(HazardStatus targetStatus)
    {
        return this switch
        {
            var s when s == Active => targetStatus == UnderInvestigation || targetStatus == OnHold || targetStatus == Cancelled,
            var s when s == UnderInvestigation => targetStatus == UnderReview || targetStatus == Active || targetStatus == OnHold,
            var s when s == UnderReview => targetStatus == Closed || targetStatus == Active || targetStatus == OnHold,
            var s when s == OnHold => targetStatus == Active || targetStatus == Cancelled,
            var s when s == Closed => targetStatus == Active, // Can reopen
            var s when s == Cancelled => false, // Cannot transition from cancelled
            _ => false
        };
    }
}