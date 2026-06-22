namespace SMS_Domain.Enums;

/// <summary>
/// Status of a queued event
/// </summary>
public enum QueuedEventStatus
{
    /// <summary>
    /// Event is waiting to be processed
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Event is currently being processed
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Event was processed successfully
    /// </summary>
    Processed = 2,

    /// <summary>
    /// Event processing failed
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Event was cancelled
    /// </summary>
    Cancelled = 4
}
