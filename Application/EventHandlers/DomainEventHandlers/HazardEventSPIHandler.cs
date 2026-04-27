//-----------------------------------------------------------------------
// <copyright file="HazardEventSPIHandler.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Event handler for automatic SPI updates when hazard-related events occur.
//                  Listens to hazard creation, closure, and update events to trigger
//                  corresponding Safety Performance Indicator calculations.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;
using SMS_Domain.Events;

namespace SMS_Application.EventHandlers;

/// <summary>
/// Event handler for Hazard-related SPI automation
/// Automatically updates SPIs when hazard events occur
/// </summary>
public class HazardEventSPIHandler
{
    private readonly ISPIAutomationService _spiAutomationService;
    private readonly ILogger<HazardEventSPIHandler> _logger;

    public HazardEventSPIHandler(
        ISPIAutomationService spiAutomationService,
        ILogger<HazardEventSPIHandler> logger)
    {
        _spiAutomationService = spiAutomationService ?? throw new ArgumentNullException(nameof(spiAutomationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Hazard Created Event Handler

    /// <summary>
    /// Handles hazard created events - Updates Hazard Report Rate SPI
    /// </summary>
    public async Task HandleHazardCreated(HazardCreatedEvent evt)
    {
        try
        {
            _logger.LogInformation("?? SPI Event: Handling HazardCreated event for hazard {HazardCode}", evt.HazardCode);

            // Update Hazard Report Rate SPI
            var result = await _spiAutomationService.UpdateHazardReportRateAsync(evt.CreatedDate, CancellationToken.None);

            if (result.IsSuccess)
            {
                _logger.LogInformation("? SPI Event: Successfully processed HazardCreated event for hazard {HazardCode}", evt.HazardCode);
            }
            else
            {
                _logger.LogWarning("?? SPI Event: Failed to process HazardCreated event for hazard {HazardCode}: {Error}", 
                    evt.HazardCode, result.Error.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? SPI Event: Error processing HazardCreated event for hazard {HazardCode}", evt.HazardCode);
        }
    }

    #endregion
}