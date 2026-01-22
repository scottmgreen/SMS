namespace SMS_Domain.Enums;

/// <summary>
/// Approval status for risk decisions and workflow management
/// </summary>
public abstract class ApprovalStatus : BaseEnum<ApprovalStatus>
{
    protected ApprovalStatus(string value, string name, string description, bool isFinal, bool allowsModification) : base(value, name)
    {
        Description = description;
        IsFinal = isFinal;
        AllowsModification = allowsModification;
    }

    public string Description { get; }
    public bool IsFinal { get; }
    public bool AllowsModification { get; }

    #region Approval Status Types

    /// <summary>Approval is pending review</summary>
    public static readonly ApprovalStatus Pending = new PendingStatus();

    /// <summary>Approval has been granted</summary>
    public static readonly ApprovalStatus Approved = new ApprovedStatus();

    /// <summary>Approval has been rejected</summary>
    public static readonly ApprovalStatus Rejected = new RejectedStatus();

    /// <summary>Approval has been escalated to higher authority</summary>
    public static readonly ApprovalStatus Escalated = new EscalatedStatus();

    /// <summary>Approval is under active review</summary>
    public static readonly ApprovalStatus UnderReview = new UnderReviewStatus();

    /// <summary>Approval request has expired</summary>
    public static readonly ApprovalStatus Expired = new ExpiredStatus();

    #endregion

    #region Implementations

    private sealed class PendingStatus : ApprovalStatus
    {
        public PendingStatus() : base("PENDING", "Pending",
            "Approval request is pending review by the appropriate authority", false, true)
        {
        }
    }

    private sealed class ApprovedStatus : ApprovalStatus
    {
        public ApprovedStatus() : base("APPROVED", "Approved",
            "Approval has been granted and decision is final", true, false)
        {
        }
    }

    private sealed class RejectedStatus : ApprovalStatus
    {
        public RejectedStatus() : base("REJECTED", "Rejected",
            "Approval has been rejected and requires revision", true, true)
        {
        }
    }

    private sealed class EscalatedStatus : ApprovalStatus
    {
        public EscalatedStatus() : base("ESCALATED", "Escalated",
            "Approval has been escalated to a higher authority for decision", false, false)
        {
        }
    }

    private sealed class UnderReviewStatus : ApprovalStatus
    {
        public UnderReviewStatus() : base("UNDER_REVIEW", "Under Review",
            "Approval is currently being actively reviewed", false, false)
        {
        }
    }

    private sealed class ExpiredStatus : ApprovalStatus
    {
        public ExpiredStatus() : base("EXPIRED", "Expired",
            "Approval request has expired and requires resubmission", true, true)
        {
        }
    }

    #endregion

    /// <summary>
    /// Gets all available approval status values
    /// </summary>
    public static IEnumerable<ApprovalStatus> GetAllValues()
    {
        return typeof(ApprovalStatus)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(ApprovalStatus))
            .Select(f => (ApprovalStatus)f.GetValue(null)!)
            .Where(aps => aps != null);
    }

    /// <summary>
    /// Gets status values that indicate pending state
    /// </summary>
    public static IEnumerable<ApprovalStatus> GetPendingStatuses()
    {
        return GetAllValues().Where(aps => !aps.IsFinal);
    }

    /// <summary>
    /// Gets status values that allow modifications
    /// </summary>
    public static IEnumerable<ApprovalStatus> GetModifiableStatuses()
    {
        return GetAllValues().Where(aps => aps.AllowsModification);
    }

    /// <summary>
    /// Checks if this status indicates completion
    /// </summary>
    public bool IsCompleted => this == Approved;

    /// <summary>
    /// Checks if this status indicates failure
    /// </summary>
    public bool IsFailed => this == Rejected || this == Expired;

    /// <summary>
    /// Checks if this status is in progress
    /// </summary>
    public bool IsInProgress => this == Pending || this == UnderReview || this == Escalated;
}