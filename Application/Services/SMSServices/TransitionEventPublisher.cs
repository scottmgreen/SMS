using SMS_Application.Interfaces;
using SMS_Domain.Interfaces;

namespace SMS_Application.Services;

internal static class TransitionEventPublisher
{
    public static async Task PublishIfChangedAsync<TTransition, TEvent>(
        IBaseEventBus eventBus,
        TTransition? previous,
        TTransition? current,
        Func<TEvent> eventFactory,
        CancellationToken ct = default)
        where TTransition : class
        where TEvent : IBaseDomainEvent
    {
        if (previous is null || current is null || Equals(previous, current))
        {
            return;
        }

        await eventBus.PublishDomainEventAsync(eventFactory(), ct).ConfigureAwait(false);
    }
}
