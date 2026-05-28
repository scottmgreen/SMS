//-----------------------------------------------------------------------
// <copyright file="BaseUIEventHandler.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Generic event handler interfaces for SMS comprehensive event processing.
//                  Provides consistent event handling patterns for Domain, UI, and Integration events
//                  that integrate with existing SMS service infrastructure and logging patterns.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Interfaces;

/// <summary>
/// Base class for UI event handlers providing common SMS infrastructure integration
/// Optimized for responsive UI updates and user experience
/// </summary>
/// <typeparam name="T">UI event type implementing IBaseUIEvent</typeparam>
public abstract class BaseUIEventHandler<T> : IUIEventHandler<T> where T : IBaseUIEvent
{
    protected readonly ILogger Logger;

    protected BaseUIEventHandler(ILogger logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Template method for UI event handling with emphasis on responsiveness
    /// </summary>
    public async Task<Result> HandleAsync(T uiEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogApplicationDebug("Processing UI event {EventType} for {TargetComponent} (Priority: {Priority})", 
                uiEvent.EventType, uiEvent.TargetComponent, uiEvent.Priority);

            var result = await ProcessUIEventAsync(uiEvent, cancellationToken);

            if (result.IsSuccess)
            {
                Logger.LogApplicationDebug("Successfully processed UI event {EventType} for {TargetComponent}", 
                    uiEvent.EventType, uiEvent.TargetComponent);
            }
            else
            {
                Logger.LogApplicationWarning("Failed to process UI event {EventType} for {TargetComponent}: {Error}", 
                    uiEvent.EventType, uiEvent.TargetComponent, result.Error.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            Logger.LogApplicationError(ex, "Error processing UI event {EventType} for {TargetComponent}", 
                uiEvent.EventType, uiEvent.TargetComponent);
            return Result.Failure(new Error("UI_EVENT_HANDLER_ERROR", $"UI event processing failed: {ex.Message}"));
        }
    }

    /// <summary>
    /// Abstract method for specific UI event processing logic
    /// </summary>
    protected abstract Task<Result> ProcessUIEventAsync(T uiEvent, CancellationToken cancellationToken);
}

