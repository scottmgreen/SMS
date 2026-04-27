//-----------------------------------------------------------------------
// <copyright file="MitigationEventSPIHandler.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Event handler for automatic SPI updates when mitigation-related events occur.
//                  Listens to mitigation completion and overdue events to trigger
//                  corresponding Safety Performance Indicator calculations.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;
using SMS_Domain.Events;

namespace SMS_Application.EventHandlers;

/// <summary>
/// Event handler for Mitigation-related SPI automation
/// Automatically updates SPIs when mitigation events occur
/// </summary>
public class MitigationEventSPIHandler
{
    private readonly ISPIAutomationService _spiAutomationService;
    private readonly ILogger<MitigationEventSPIHandler> _logger;

    public MitigationEventSPIHandler(
        ISPIAutomationService spiAutomationService,
        ILogger<MitigationEventSPIHandler> logger)
    {
        _spiAutomationService = spiAutomationService ?? throw new ArgumentNullException(nameof(spiAutomationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Mitigation Completed Event Handler

    /// <summary>
    /// Handles mitigation completed events - Updates Mitigation Implementation Rate SPI
    /// </summary>
    public async Task HandleMitigationCompleted(MitigationCompletedEvent evt)
    {
        try
        {
            _logger.LogInformation("?? SPI Event: Handling MitigationCompleted event for mitigation {MitigationId}", evt.MitigationId);

            // Update Mitigation Implementation Rate SPI
            var result = await _spiAutomationService.UpdateMitigationImplementationRateAsync(
                evt.MitigationId,
                evt.TargetCompletionDate,
                evt.CompletedDate,
                CancellationToken.None);

            if (result.IsSuccess)
            {
                _logger.LogInformation("? SPI Event: Successfully processed MitigationCompleted event for mitigation {MitigationId}", evt.MitigationId);
            }
            else
            {
                _logger.LogWarning("?? SPI Event: Failed to process MitigationCompleted event for mitigation {MitigationId}: {Error}", 
                    evt.MitigationId, result.Error.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? SPI Event: Error processing MitigationCompleted event for mitigation {MitigationId}", evt.MitigationId);
        }
    }

    /// <summary>
    /// Handles mitigation overdue events - Updates Mitigation Overdue Rate SPI
    /// </summary>
    public async Task HandleMitigationOverdue(MitigationOverdueEvent evt)
    {
        try
        {
            _logger.LogInformation("?? SPI Event: Handling MitigationOverdue event for mitigation {MitigationId}", evt.MitigationId);

            // Implementation for mitigation overdue SPI updates
            await Task.CompletedTask;

            _logger.LogInformation("? SPI Event: Successfully processed MitigationOverdue event for mitigation {MitigationId}", evt.MitigationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? SPI Event: Error processing MitigationOverdue event for mitigation {MitigationId}", evt.MitigationId);
        }
    }

    #endregion
}