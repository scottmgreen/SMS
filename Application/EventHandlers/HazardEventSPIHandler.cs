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
            _logger.LogInformation("?? SPI Event: Handling HazardCreated event for hazard {HazardId}", evt.HazardId);

            // Update Hazard Report Rate SPI
            var result = await _spiAutomationService.UpdateHazardReportRateAsync(evt.CreatedDate, CancellationToken.None);

            if (result.IsSuccess)
            {
                _logger.LogInformation("? SPI Event: Successfully processed HazardCreated event for hazard {HazardId}", evt.HazardId);
            }
            else
            {
                _logger.LogError("? SPI Event: Failed to process HazardCreated event for hazard {HazardId} - Error: {Error}", 
                    evt.HazardId, result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? SPI Event: Exception handling HazardCreated event for hazard {HazardId}", evt.HazardId);
        }
    }

    #endregion

    #region Hazard Closed Event Handler

    /// <summary>
    /// Handles hazard closed events - Updates Hazard Closure Time SPI
    /// </summary>
    public async Task HandleHazardClosed(HazardClosedEvent evt)
    {
        try
        {
            _logger.LogInformation("?? SPI Event: Handling HazardClosed event for hazard {HazardId}", evt.HazardId);

            // Update Hazard Closure Time SPI
            var result = await _spiAutomationService.UpdateHazardClosureTimeAsync(
                evt.HazardId, 
                evt.SubmittedDate, 
                evt.ClosedDate, 
                CancellationToken.None);

            if (result.IsSuccess)
            {
                _logger.LogInformation("? SPI Event: Successfully processed HazardClosed event for hazard {HazardId}", evt.HazardId);
            }
            else
            {
                _logger.LogError("? SPI Event: Failed to process HazardClosed event for hazard {HazardId} - Error: {Error}", 
                    evt.HazardId, result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? SPI Event: Exception handling HazardClosed event for hazard {HazardId}", evt.HazardId);
        }
    }

    #endregion
}

#region Event Models

/// <summary>
/// Event model for hazard creation
/// </summary>
public class HazardCreatedEvent
{
    public string HazardId { get; set; } = string.Empty;
    public string HazardCode { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string ReportId { get; set; } = string.Empty;
    public string HazardType { get; set; } = string.Empty;
    public string HazardCategory { get; set; } = string.Empty;

    public HazardCreatedEvent() { }

    public HazardCreatedEvent(string hazardId, string hazardCode, DateTime createdDate, string createdBy)
    {
        HazardId = hazardId;
        HazardCode = hazardCode;
        CreatedDate = createdDate;
        CreatedBy = createdBy;
    }
}

/// <summary>
/// Event model for hazard closure
/// </summary>
public class HazardClosedEvent
{
    public string HazardId { get; set; } = string.Empty;
    public string HazardCode { get; set; } = string.Empty;
    public DateTime SubmittedDate { get; set; }
    public DateTime ClosedDate { get; set; }
    public string ClosedBy { get; set; } = string.Empty;
    public string ClosureReason { get; set; } = string.Empty;
    public string Resolution { get; set; } = string.Empty;

    public HazardClosedEvent() { }

    public HazardClosedEvent(string hazardId, string hazardCode, DateTime submittedDate, DateTime closedDate, string closedBy)
    {
        HazardId = hazardId;
        HazardCode = hazardCode;
        SubmittedDate = submittedDate;
        ClosedDate = closedDate;
        ClosedBy = closedBy;
    }
}

#endregion