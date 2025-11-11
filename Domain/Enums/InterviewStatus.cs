using SMS_Domain.Common;

namespace SMS_Domain.Enums;

/// <summary>
/// Interview Status Smart Enumeration
/// Represents the various stages of an interview in the SMS investigation process
/// </summary>
public sealed class InterviewStatus : BaseEnum<InterviewStatus>
{
    public static readonly InterviewStatus Planned = new("PLANNED", nameof(Planned));
    public static readonly InterviewStatus Scheduled = new("SCHEDULED", nameof(Scheduled));
    public static readonly InterviewStatus InProgress = new("IN_PROGRESS", nameof(InProgress));
    public static readonly InterviewStatus Completed = new("COMPLETED", nameof(Completed));
    public static readonly InterviewStatus Cancelled = new("CANCELLED", nameof(Cancelled));

    public InterviewStatus(string value, string name) : base(value, name)
    {
    }

    /// <summary>
    /// Get all interview statuses that are considered active
    /// </summary>
    public static IEnumerable<InterviewStatus> GetActiveStatuses()
    {
        return new[] { Planned, Scheduled, InProgress };
    }

    /// <summary>
    /// Get all interview statuses that are considered final
    /// </summary>
    public static IEnumerable<InterviewStatus> GetFinalStatuses()
    {
        return new[] { Completed, Cancelled };
    }

    /// <summary>
    /// Check if this status allows modifications
    /// </summary>
    public bool AllowsModifications()
    {
        return this == Planned || this == Scheduled;
    }

    /// <summary>
    /// Check if this status indicates the interview is in progress or completed
    /// </summary>
    public bool IsStarted()
    {
        return this == InProgress || this == Completed;
    }

    /// <summary>
    /// Check if this status indicates the interview is complete
    /// </summary>
    public bool IsComplete()
    {
        return this == Completed;
    }

    /// <summary>
    /// Check if this status indicates the interview is cancelled
    /// </summary>
    public bool IsCancelled()
    {
        return this == Cancelled;
    }

    /// <summary>
    /// Check if this status allows the interview to be started
    /// </summary>
    public bool CanStart()
    {
        return this == Scheduled;
    }

    /// <summary>
    /// Check if this status allows the interview to be completed
    /// </summary>
    public bool CanComplete()
    {
        return this == InProgress;
    }

    /// <summary>
    /// Check if this status allows the interview to be cancelled
    /// </summary>
    public bool CanCancel()
    {
        return this == Planned || this == Scheduled || this == InProgress;
    }

    /// <summary>
    /// Get the next possible statuses from this status
    /// </summary>
    public IEnumerable<InterviewStatus> GetPossibleNextStatuses()
    {
        return Value switch
        {
            "PLANNED" => new[] { Scheduled, Cancelled },
            "SCHEDULED" => new[] { InProgress, Cancelled },
            "IN_PROGRESS" => new[] { Completed, Cancelled },
            _ => new InterviewStatus[] { } // Completed and Cancelled are final
        };
    }

    /// <summary>
    /// Get display description for this status
    /// </summary>
    public string GetDisplayDescription()
    {
        return Value switch
        {
            "PLANNED" => "Interview has been planned but not yet scheduled",
            "SCHEDULED" => "Interview has been scheduled with a specific date and time",
            "IN_PROGRESS" => "Interview is currently in progress",
            "COMPLETED" => "Interview has been completed successfully",
            "CANCELLED" => "Interview has been cancelled",
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
            "PLANNED" => "status-planned",
            "SCHEDULED" => "status-scheduled", 
            "IN_PROGRESS" => "status-in-progress",
            "COMPLETED" => "status-completed",
            "CANCELLED" => "status-cancelled",
            _ => "status-default"
        };
    }
}