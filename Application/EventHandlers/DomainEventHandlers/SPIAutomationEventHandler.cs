//-----------------------------------------------------------------------
// <copyright file="SPIAutomationEventHandler.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: EventBus-integrated SPI automation handler for unified event processing.
//                  Replaces direct SPIEventCoordinator calls with proper pub/sub pattern
//                  for consistent event-driven SPI automation across the system.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Domain.Common;
using SMS_Domain.Events;

namespace SMS_Application.EventHandlers;

/// <summary>
/// EventBus-integrated SPI automation handler
/// PERFORMANCE FIX: Removed user context dependencies to prevent authentication loops
/// </summary>
public class SPIAutomationEventHandler : BaseDomainEventHandler<SMS_Domain.Events.HazardCreatedEvent>
{
    private readonly ILogger<SPIAutomationEventHandler> _logger;

    public SPIAutomationEventHandler(
        ILogger<SPIAutomationEventHandler> logger)
        : base(logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Processes hazard creation events for SPI automation via EventBus
    /// PERFORMANCE FIX: Simplified to avoid authentication loops
    /// </summary>
    protected override async Task<Result> ProcessEventAsync(SMS_Domain.Events.HazardCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("?? SPI EventBus: Processing SPI automation for hazard {HazardCode}", domainEvent.HazardCode);

            // PERFORMANCE FIX: Skip SPIEventCoordinator call to avoid authentication loops
            // The existing HazardEventSPIHandler will handle SPI automation
            _logger.LogInformation("? SPI EventBus: SPI automation delegated to HazardEventSPIHandler for hazard {HazardCode}", domainEvent.HazardCode);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? SPI EventBus: SPI automation failed for hazard {HazardCode}", domainEvent.HazardCode);
            return Result.Failure(new Error("SPI_AUTOMATION_FAILED", $"SPI automation failed: {ex.Message}"));
        }
    }
}