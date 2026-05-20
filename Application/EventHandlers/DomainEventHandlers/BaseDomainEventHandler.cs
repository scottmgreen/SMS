using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;
using SMS_Domain.Common;
using SMS_Domain.Interfaces;

namespace SMS_Application.EventHandlers;

public abstract class BaseDomainEventHandler<TEvent> : Interfaces.BaseDomainEventHandler<TEvent>
    where TEvent : IBaseDomainEvent
{
    protected BaseDomainEventHandler(ILogger logger, IBaseEventBus eventBus)
        : base(logger)
    {
        EventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }

    protected IBaseEventBus EventBus { get; }

    protected override async Task<Result> ProcessEventAsync(TEvent domainEvent, CancellationToken cancellationToken)
    {
        var domainResult = await HandleDomainEventAsync(domainEvent, cancellationToken);
        if (domainResult.IsFailure)
        {
            return domainResult;
        }

        var integrationResult = await HandleIntegrationEventAsync(domainEvent, cancellationToken);
        if (integrationResult.IsFailure)
        {
            return integrationResult;
        }

        return await HandleUIEventAsync(domainEvent, cancellationToken);
    }

    protected virtual Task<Result> HandleDomainEventAsync(TEvent domainEvent, CancellationToken cancellationToken)
        => Task.FromResult(Result.Success());

    protected virtual Task<Result> HandleIntegrationEventAsync(TEvent domainEvent, CancellationToken cancellationToken)
        => Task.FromResult(Result.Success());

    protected virtual Task<Result> HandleUIEventAsync(TEvent domainEvent, CancellationToken cancellationToken)
        => Task.FromResult(Result.Success());
}
