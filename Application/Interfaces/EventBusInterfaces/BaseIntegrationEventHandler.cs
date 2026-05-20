//-----------------------------------------------------------------------
// <copyright file="BaseIntegrationEventHandler.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Generic event handler interfaces for SMS comprehensive event processing.
//                  Provides consistent event handling patterns for Domain, UI, and Integration events
//                  that integrate with existing SMS service infrastructure and logging patterns.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Interfaces;

/// <summary>
/// Base class for integration event handlers providing external system integration
/// Includes retry logic and external system coordination patterns
/// </summary>
/// <typeparam name="T">Integration event type implementing IBaseIntegrationEvent</typeparam>
public abstract class BaseIntegrationEventHandler<T> : IIntegrationEventHandler<T> where T : IBaseIntegrationEvent
{
    protected readonly ILogger Logger;

    protected BaseIntegrationEventHandler(ILogger logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Template method for integration event handling with retry logic
    /// </summary>
    public async Task<Result> HandleAsync(T integrationEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Processing integration event {EventType} for {TargetSystem} (Delivery: {DeliveryMode})", integrationEvent.EventType, integrationEvent.TargetSystem, integrationEvent.DeliveryMode);

            var result = await ProcessIntegrationEventAsync(integrationEvent, cancellationToken);

            if (result.IsSuccess)
            {
                Logger.LogInformation("Successfully processed integration event {EventType} for {TargetSystem}", integrationEvent.EventType, integrationEvent.TargetSystem);
            }
            else
            {
                Logger.LogWarning("Failed to process integration event {EventType} for {TargetSystem}: {Error}", integrationEvent.EventType, integrationEvent.TargetSystem, result.Error.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing integration event {EventType} for {TargetSystem}", integrationEvent.EventType, integrationEvent.TargetSystem);
            return Result.Failure(new Error("INTEGRATION_EVENT_HANDLER_ERROR", $"Integration event processing failed: {ex.Message}"));
        }
    }

    /// <summary>
    /// Abstract method for specific integration event processing logic
    /// </summary>
    protected abstract Task<Result> ProcessIntegrationEventAsync(T integrationEvent, CancellationToken cancellationToken);
}