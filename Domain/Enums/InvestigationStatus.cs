using SMS_Domain.Common;

namespace SMS_Domain.Enums;

/// <summary>
/// Investigation Status Smart Enumeration
/// Represents the various stages of an investigation in the SMS investigation process
/// </summary>
public sealed class InvestigationStatus : BaseEnum<InvestigationStatus>
{
    public static readonly InvestigationStatus InProgress = new("IN_PROGRESS", nameof(InProgress));
    public static readonly InvestigationStatus Completed = new("COMPLETED", nameof(Completed));
    public static readonly InvestigationStatus OnHold = new("ON_HOLD", nameof(OnHold));
    public static readonly InvestigationStatus Cancelled = new("CANCELLED", nameof(Cancelled));

    public InvestigationStatus(string value, string name) : base(value, name)
    {
    }

    /// <summary>
    /// Get all investigation statuses that are considered active
    /// </summary>
    public static IEnumerable<InvestigationStatus> GetActiveStatuses()
    {
        return new[] { InProgress, OnHold };
    }

    /// <summary>
    /// Get all investigation statuses that are considered final
    /// </summary>
    public static IEnumerable<InvestigationStatus> GetFinalStatuses()
    {
        return new[] { Completed, Cancelled };
    }

    /// <summary>
    /// Check if this status allows modifications
    /// </summary>
    public bool AllowsModifications()
    {
        return this == InProgress || this == OnHold;
    }

    /// <summary>
    /// Check if this status indicates the investigation is complete
    /// </summary>
    public bool IsComplete()
    {
        return this == Completed;
    }

    /// <summary>
    /// Check if this status indicates the investigation is cancelled
    /// </summary>
    public bool IsCancelled()
    {
        return this == Cancelled;
    }

    /// <summary>
    /// Check if this status allows the investigation to be completed
    /// </summary>
    public bool CanComplete()
    {
        return this == InProgress;
    }

    /// <summary>
    /// Check if this status allows the investigation to be put on hold
    /// </summary>
    public bool CanPutOnHold()
    {
        return this == InProgress;
    }

    /// <summary>
    /// Check if this status allows the investigation to be resumed
    /// </summary>
    public bool CanResume()
    {
        return this == OnHold;
    }

    /// <summary>
    /// Check if this status allows the investigation to be cancelled
    /// </summary>
    public bool CanCancel()
    {
        return this == InProgress || this == OnHold;
    }

    /// <summary>
    /// Get the next possible statuses from this status
    /// </summary>
    public IEnumerable<InvestigationStatus> GetPossibleNextStatuses()
    {
        return Value switch
        {
            "IN_PROGRESS" => new[] { Completed, OnHold, Cancelled },
            "ON_HOLD" => new[] { InProgress, Cancelled },
            _ => new InvestigationStatus[] { } // Completed and Cancelled are final
        };
    }

    /// <summary>
    /// Get display description for this status
    /// </summary>
    public string GetDisplayDescription()
    {
        return Value switch
        {
            "IN_PROGRESS" => "Investigation is actively being conducted",
            "COMPLETED" => "Investigation has been completed successfully",
            "ON_HOLD" => "Investigation is temporarily suspended",
            "CANCELLED" => "Investigation has been cancelled",
            _ => Name
        };
    }

    /// <summary>
    /// Get CSS class for status display
    /// </summary>
    public string GetCssClass()
    {
        return Value switch
        {
            "IN_PROGRESS" => "status-in-progress",
            "COMPLETED" => "status-completed",
            "ON_HOLD" => "status-on-hold",
            "CANCELLED" => "status-cancelled",
            _ => "status-default"
        };
    }

    /// <summary>
    /// Get icon class for status display
    /// </summary>
    public string GetIconClass()
    {
        return Value switch
        {
            "IN_PROGRESS" => "fas fa-play-circle",
            "COMPLETED" => "fas fa-check-circle",
            "ON_HOLD" => "fas fa-pause-circle",
            "CANCELLED" => "fas fa-times-circle",
            _ => "fas fa-question-circle"
        };
    }

    /// <summary>
    /// Get color class for status display
    /// </summary>
    public string GetColorClass()
    {
        return Value switch
        {
            "IN_PROGRESS" => "text-primary",
            "COMPLETED" => "text-success",
            "ON_HOLD" => "text-warning",
            "CANCELLED" => "text-danger",
            _ => "text-muted"
        };
    }

    /// <summary>
    /// Check if this status represents an active investigation workflow state
    /// </summary>
    public bool IsActiveWorkflow()
    {
        return this == InProgress;
    }

    /// <summary>
    /// Get progress percentage for this status
    /// </summary>
    public int GetProgressPercentage()
    {
        return Value switch
        {
            "IN_PROGRESS" => 50,
            "COMPLETED" => 100,
            "ON_HOLD" => 25,
            "CANCELLED" => 0,
            _ => 0
        };
    }

    /// <summary>
    /// Check if investigation requires immediate attention
    /// </summary>
    public bool RequiresAttention()
    {
        return this == OnHold;
    }
}