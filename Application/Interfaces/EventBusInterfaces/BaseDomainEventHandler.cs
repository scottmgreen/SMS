//-----------------------------------------------------------------------
// <copyright file="BaseDomainEventHandler.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Generic event handler interfaces for SMS comprehensive event processing.
//                  Provides consistent event handling patterns for Domain, UI, and Integration events
//                  that integrate with existing SMS service infrastructure and logging patterns.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Interfaces;

/// <summary>
/// Base class for domain event handlers providing common SMS infrastructure integration
/// Maintains existing patterns while supporting the new event architecture
/// </summary>
/// <typeparam name="T">Domain event type implementing IBaseDomainEvent</typeparam>
public abstract class BaseDomainEventHandler<T> : IDomainEventHandler<T> where T : IBaseDomainEvent
{
    protected readonly ILogger Logger;
    protected readonly IBaseEventBus? EventBus;

    protected BaseDomainEventHandler(ILogger logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected BaseDomainEventHandler(ILogger logger, IBaseEventBus eventBus)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        EventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }

    /// <summary>
    /// Template method for domain event handling with consistent logging and error handling
    /// </summary>
    public async Task<Result> HandleAsync(T domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Processing domain event {EventType} with ID {EventId}", domainEvent.EventType, domainEvent.EventId);

            var result = await ProcessEventAsync(domainEvent, cancellationToken);

            if (result.IsSuccess)
            {
                Logger.LogInformation("Successfully processed domain event {EventType} with ID {EventId}", domainEvent.EventType, domainEvent.EventId);
            }
            else
            {
                Logger.LogWarning("Failed to process domain event {EventType} with ID {EventId}: {Error}", domainEvent.EventType, domainEvent.EventId, result.Error.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing domain event {EventType} with ID {EventId}", domainEvent.EventType, domainEvent.EventId);
            return Result.Failure(new Error("DOMAIN_EVENT_HANDLER_ERROR", $"Domain event processing failed: {ex.Message}"));
        }
    }

    /// <summary>
    /// Abstract method for specific domain event processing logic
    /// </summary>
    protected abstract Task<Result> ProcessEventAsync(T domainEvent, CancellationToken cancellationToken);
}
