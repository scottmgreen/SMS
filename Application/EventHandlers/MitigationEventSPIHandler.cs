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
                var daysFromTarget = (evt.CompletedDate - evt.TargetCompletionDate).TotalDays;
                _logger.LogInformation("? SPI Event: Successfully processed MitigationCompleted event for mitigation {MitigationId} - Days from target: {Days}", 
                    evt.MitigationId, daysFromTarget);
            }
            else
            {
                _logger.LogError("? SPI Event: Failed to process MitigationCompleted event for mitigation {MitigationId} - Error: {Error}", 
                    evt.MitigationId, result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? SPI Event: Exception handling MitigationCompleted event for mitigation {MitigationId}", evt.MitigationId);
        }
    }

    #endregion

    #region Mitigation Overdue Event Handler

    /// <summary>
    /// Handles mitigation overdue events - Triggers Corrective Action Closure SPI update
    /// </summary>
    public async Task HandleMitigationOverdue(MitigationOverdueEvent evt)
    {
        try
        {
            _logger.LogInformation("?? SPI Event: Handling MitigationOverdue event for mitigation {MitigationId}", evt.MitigationId);

            // Update Corrective Action Closure SPI (recalculate overdue rates)
            var result = await _spiAutomationService.UpdateCorrectiveActionClosureAsync(
                DateTime.Today,
                CancellationToken.None);

            if (result.IsSuccess)
            {
                var daysOverdue = (DateTime.Today - evt.TargetCompletionDate).TotalDays;
                _logger.LogInformation("? SPI Event: Successfully processed MitigationOverdue event for mitigation {MitigationId} - Days overdue: {Days}", 
                    evt.MitigationId, daysOverdue);
            }
            else
            {
                _logger.LogError("? SPI Event: Failed to process MitigationOverdue event for mitigation {MitigationId} - Error: {Error}", 
                    evt.MitigationId, result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? SPI Event: Exception handling MitigationOverdue event for mitigation {MitigationId}", evt.MitigationId);
        }
    }

    #endregion

    #region Mitigation Status Changed Event Handler

    /// <summary>
    /// Handles mitigation status changed events - Updates relevant SPIs based on new status
    /// </summary>
    public async Task HandleMitigationStatusChanged(MitigationStatusChangedEvent evt)
    {
        try
        {
            _logger.LogInformation("?? SPI Event: Handling MitigationStatusChanged event for mitigation {MitigationId} - New Status: {NewStatus}", 
                evt.MitigationId, evt.NewStatus);

            // If changed to completed, trigger completion handler
            if (evt.NewStatus.Equals("Complete", StringComparison.OrdinalIgnoreCase) ||
                evt.NewStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                var completedEvent = new MitigationCompletedEvent
                {
                    MitigationId = evt.MitigationId,
                    MitigationCode = evt.MitigationCode,
                    HazardId = evt.HazardId,
                    TargetCompletionDate = evt.TargetCompletionDate,
                    CompletedDate = evt.StatusChangedDate,
                    CompletedBy = evt.ChangedBy
                };

                await HandleMitigationCompleted(completedEvent);
            }
            // If status indicates overdue, trigger overdue handler
            else if (evt.NewStatus.Equals("Overdue", StringComparison.OrdinalIgnoreCase) ||
                     (evt.TargetCompletionDate < DateTime.Today && !evt.NewStatus.Equals("Complete", StringComparison.OrdinalIgnoreCase)))
            {
                var overdueEvent = new MitigationOverdueEvent
                {
                    MitigationId = evt.MitigationId,
                    MitigationCode = evt.MitigationCode,
                    HazardId = evt.HazardId,
                    TargetCompletionDate = evt.TargetCompletionDate,
                    DaysOverdue = (DateTime.Today - evt.TargetCompletionDate).Days
                };

                await HandleMitigationOverdue(overdueEvent);
            }

            _logger.LogInformation("? SPI Event: Successfully processed MitigationStatusChanged event for mitigation {MitigationId}", evt.MitigationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? SPI Event: Exception handling MitigationStatusChanged event for mitigation {MitigationId}", evt.MitigationId);
        }
    }

    #endregion
}

#region Event Models

/// <summary>
/// Event model for mitigation completion
/// </summary>
public class MitigationCompletedEvent
{
    public string MitigationId { get; set; } = string.Empty;
    public string MitigationCode { get; set; } = string.Empty;
    public string HazardId { get; set; } = string.Empty;
    public string ReportId { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime TargetCompletionDate { get; set; }
    public DateTime CompletedDate { get; set; }
    public string CompletedBy { get; set; } = string.Empty;
    public string CompletionNotes { get; set; } = string.Empty;
    public string EffectivenessRating { get; set; } = string.Empty;

    public MitigationCompletedEvent() { }

    public MitigationCompletedEvent(string mitigationId, string mitigationCode, string hazardId,
        DateTime targetCompletionDate, DateTime completedDate, string completedBy)
    {
        MitigationId = mitigationId;
        MitigationCode = mitigationCode;
        HazardId = hazardId;
        TargetCompletionDate = targetCompletionDate;
        CompletedDate = completedDate;
        CompletedBy = completedBy;
    }
}

/// <summary>
/// Event model for mitigation overdue
/// </summary>
public class MitigationOverdueEvent
{
    public string MitigationId { get; set; } = string.Empty;
    public string MitigationCode { get; set; } = string.Empty;
    public string HazardId { get; set; } = string.Empty;
    public string ReportId { get; set; } = string.Empty;
    public DateTime TargetCompletionDate { get; set; }
    public int DaysOverdue { get; set; }
    public string AssignedTo { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;

    public MitigationOverdueEvent() { }

    public MitigationOverdueEvent(string mitigationId, string mitigationCode, string hazardId,
        DateTime targetCompletionDate, int daysOverdue)
    {
        MitigationId = mitigationId;
        MitigationCode = mitigationCode;
        HazardId = hazardId;
        TargetCompletionDate = targetCompletionDate;
        DaysOverdue = daysOverdue;
    }
}

/// <summary>
/// Event model for mitigation status changes
/// </summary>
public class MitigationStatusChangedEvent
{
    public string MitigationId { get; set; } = string.Empty;
    public string MitigationCode { get; set; } = string.Empty;
    public string HazardId { get; set; } = string.Empty;
    public string ReportId { get; set; } = string.Empty;
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public DateTime StatusChangedDate { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public DateTime TargetCompletionDate { get; set; }
    public string ChangeReason { get; set; } = string.Empty;

    public MitigationStatusChangedEvent() { }

    public MitigationStatusChangedEvent(string mitigationId, string mitigationCode, string hazardId,
        string oldStatus, string newStatus, DateTime statusChangedDate, string changedBy)
    {
        MitigationId = mitigationId;
        MitigationCode = mitigationCode;
        HazardId = hazardId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        StatusChangedDate = statusChangedDate;
        ChangedBy = changedBy;
    }
}

#endregion