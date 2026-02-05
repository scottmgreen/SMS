using SMS_Domain.Common;

namespace SMS_Domain.Enums;

/// <summary>
/// Mitigation Status Smart Enumeration - NEWLY CREATED
/// Represents the various stages of mitigation implementation in the SMS mitigation process
/// Based on approved final status list from StatusList.txt
/// </summary>
public sealed class MitigationStatus : BaseEnum<MitigationStatus>
{
    // ? APPROVED FINAL STATUS VALUES FROM StatusList.txt
    public static readonly MitigationStatus PendingApproval = new("PENDING_APPROVAL", "Mitigation Pending Approval");
    public static readonly MitigationStatus Approved = new("APPROVED", "Mitigation Approved");
    public static readonly MitigationStatus InProgressDueDate = new("IN_PROGRESS_DUE_DATE", "Mitigation in Progress – Due Date");
    public static readonly MitigationStatus Rejected = new("REJECTED", "Mitigation Rejected");
    public static readonly MitigationStatus Complete = new("COMPLETE", "Mitigation Complete");
    public static readonly MitigationStatus MonitoringHazard = new("MONITORING_HAZARD", "Monitoring Hazard");

    private MitigationStatus(string value, string name) : base(value, name)
    {
    }

    /// <summary>
    /// Get all mitigation statuses that are considered active (in progress)
    /// </summary>
    public static IEnumerable<MitigationStatus> GetActiveStatuses()
    {
        return new[] { PendingApproval, Approved, InProgressDueDate };
    }

    /// <summary>
    /// Get all mitigation statuses that are considered final (completed)
    /// </summary>
    public static IEnumerable<MitigationStatus> GetFinalStatuses()
    {
        return new[] { Complete, Rejected, MonitoringHazard };
    }

    /// <summary>
    /// Check if this status allows modifications
    /// </summary>
    public bool AllowsModifications()
    {
        return this == PendingApproval || this == Approved || this == InProgressDueDate;
    }

    /// <summary>
    /// Check if this status indicates mitigation is pending approval
    /// </summary>
    public bool IsPendingApproval()
    {
        return this == PendingApproval;
    }

    /// <summary>
    /// Check if this status indicates mitigation is approved
    /// </summary>
    public bool IsApproved()
    {
        return this == Approved;
    }

    /// <summary>
    /// Check if this status indicates mitigation is in progress
    /// </summary>
    public bool IsInProgress()
    {
        return this == InProgressDueDate;
    }

    /// <summary>
    /// Check if this status indicates mitigation is rejected
    /// </summary>
    public bool IsRejected()
    {
        return this == Rejected;
    }

    /// <summary>
    /// Check if this status indicates mitigation is complete
    /// </summary>
    public bool IsComplete()
    {
        return this == Complete;
    }

    /// <summary>
    /// Check if this status indicates hazard is being monitored
    /// </summary>
    public bool IsMonitoring()
    {
        return this == MonitoringHazard;
    }

    /// <summary>
    /// Get the next possible statuses from this status
    /// </summary>
    public IEnumerable<MitigationStatus> GetPossibleNextStatuses()
    {
        return Value switch
        {
            "PENDING_APPROVAL" => new[] { Approved, Rejected },
            "APPROVED" => new[] { InProgressDueDate, Rejected },
            "IN_PROGRESS_DUE_DATE" => new[] { Complete, MonitoringHazard, Rejected },
            "REJECTED" => new[] { PendingApproval }, // Can be resubmitted
            "COMPLETE" => new[] { MonitoringHazard }, // May need monitoring after completion
            _ => new MitigationStatus[] { } // MonitoringHazard is typically final
        };
    }

    /// <summary>
    /// Check if transition to target status is allowed
    /// </summary>
    public bool CanTransitionTo(MitigationStatus targetStatus)
    {
        var possibleStatuses = GetPossibleNextStatuses();
        return possibleStatuses.Contains(targetStatus);
    }

    /// <summary>
    /// Determines if this status is a final state
    /// </summary>
    public bool IsFinalStatus => this == Complete || this == MonitoringHazard || this == Rejected;

    /// <summary>
    /// Determines if this status is an active processing state
    /// </summary>
    public bool IsActiveStatus => this == PendingApproval || this == Approved || this == InProgressDueDate;

    /// <summary>
    /// Get workflow order for this status
    /// </summary>
    public int WorkflowOrder => Value switch
    {
        "PENDING_APPROVAL" => 1,
        "APPROVED" => 2,
        "IN_PROGRESS_DUE_DATE" => 3,
        "COMPLETE" => 4,
        "MONITORING_HAZARD" => 5,
        "REJECTED" => 99, // Can happen at any stage
        _ => 0
    };
}