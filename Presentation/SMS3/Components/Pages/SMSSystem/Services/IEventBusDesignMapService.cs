using SMS_Domain.Enums;

namespace SMS3.Components.Pages.SMSSystem.Services;

public interface IEventBusDesignMapService
{
    Task<IReadOnlyList<EventBusMapEventItem>> BuildMapAsync(CancellationToken cancellationToken = default);
}

public sealed record EventBusMapEventItem(
    string EventClassName,
    string Category,
    EventLifecycleState LifecycleState,
    string LifecycleNotes,
    IReadOnlyList<string> TriggeredAt,
    IReadOnlyList<string> HandledBy,
    IReadOnlyList<string> ChainedTo,
    IReadOnlyList<string> ChainedFrom,
    IReadOnlyList<string> ChainFlow,
    bool IsUnused);
