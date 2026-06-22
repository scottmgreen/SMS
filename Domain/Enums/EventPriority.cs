namespace SMS_Domain.Enums;

/// <summary>
/// Priority level for event processing
/// </summary>
public enum EventPriority
{
    /// <summary>
    /// Low priority - UI updates, non-critical notifications
    /// </summary>
    Low = 0,

    /// <summary>
    /// Normal priority - Standard business events
    /// </summary>
    Normal = 1,

    /// <summary>
    /// High priority - Integration events, critical notifications
    /// </summary>
    High = 2,

    /// <summary>
    /// Critical priority - Security alerts, system failures
    /// </summary>
    Critical = 3
}
