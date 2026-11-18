namespace SMS_Domain.Interfaces;

/// <summary>
/// Unified metadata contract for all SMS EventBus events.
/// Provides stable machine keys and readable labels across Domain, Integration, and UI events.
/// </summary>
public interface IEventMetadata
{
    /// <summary>
    /// Runtime event class name.
    /// </summary>
    string EventClassName { get; }

    /// <summary>
    /// Logical event category name used for key prefixes.
    /// Expected values: DomainEvent, IntegrationEvent, UIEvent.
    /// </summary>
    string EventCategoryName { get; }

    /// <summary>
    /// Stable configuration key for this event.
    /// Example: DomainEvent:HazardCreatedEvent.
    /// </summary>
    string TriggerEventKey { get; }

    /// <summary>
    /// Human-readable display text for settings, maps, and diagnostics.
    /// </summary>
    string TriggerDisplayName { get; }
}