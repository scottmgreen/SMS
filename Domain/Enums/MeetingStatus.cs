using System.Reflection;
using SMS_Domain.Common;

namespace SMS_Domain.Enums;

/// <summary>
/// Meeting status tracking for committee meetings
/// </summary>
public abstract class MeetingStatus : BaseEnum<MeetingStatus>
{
    protected MeetingStatus(string value, string name, string description) : base(value, name)
    {
        Description = description;
    }

    public string Description { get; }

    #region Meeting Status Types

    /// <summary>Meeting is scheduled but not yet started</summary>
    public static readonly MeetingStatus Scheduled = new ScheduledStatus();

    /// <summary>Meeting is currently in progress</summary>
    public static readonly MeetingStatus InProgress = new InProgressStatus();

    /// <summary>Meeting has been completed</summary>
    public static readonly MeetingStatus Completed = new CompletedStatus();

    /// <summary>Meeting has been cancelled</summary>
    public static readonly MeetingStatus Cancelled = new CancelledStatus();

    /// <summary>Meeting has been postponed to a later date</summary>
    public static readonly MeetingStatus Postponed = new PostponedStatus();

    /// <summary>Meeting minutes are pending approval</summary>
    public static readonly MeetingStatus PendingApproval = new PendingApprovalStatus();

    #endregion

    #region Implementations

    private sealed class ScheduledStatus : MeetingStatus
    {
        public ScheduledStatus() : base("SCHEDULED", "Scheduled",
            "Meeting is scheduled but has not yet begun")
        {
        }
    }

    private sealed class InProgressStatus : MeetingStatus
    {
        public InProgressStatus() : base("IN_PROGRESS", "In Progress",
            "Meeting is currently taking place")
        {
        }
    }

    private sealed class CompletedStatus : MeetingStatus
    {
        public CompletedStatus() : base("COMPLETED", "Completed",
            "Meeting has been completed and minutes approved")
        {
        }
    }

    private sealed class CancelledStatus : MeetingStatus
    {
        public CancelledStatus() : base("CANCELLED", "Cancelled",
            "Meeting has been cancelled and will not take place")
        {
        }
    }

    private sealed class PostponedStatus : MeetingStatus
    {
        public PostponedStatus() : base("POSTPONED", "Postponed",
            "Meeting has been postponed to a later date")
        {
        }
    }

    private sealed class PendingApprovalStatus : MeetingStatus
    {
        public PendingApprovalStatus() : base("PENDING_APPROVAL", "Pending Approval",
            "Meeting is complete but minutes are awaiting approval")
        {
        }
    }

    #endregion

    /// <summary>
    /// Gets all available meeting status values
    /// </summary>
    public static IEnumerable<MeetingStatus> GetAllValues()
    {
        return typeof(MeetingStatus)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(MeetingStatus))
            .Select(f => (MeetingStatus)f.GetValue(null)!)
            .Where(ms => ms != null);
    }

    /// <summary>
    /// Checks if this status indicates the meeting is active
    /// </summary>
    public bool IsActive => this == Scheduled || this == InProgress;

    /// <summary>
    /// Checks if this status indicates the meeting is finished
    /// </summary>
    public bool IsFinished => this == Completed || this == Cancelled;

    /// <summary>
    /// Checks if this status allows modifications
    /// </summary>
    public bool AllowsModifications => this == Scheduled || this == Postponed;
}