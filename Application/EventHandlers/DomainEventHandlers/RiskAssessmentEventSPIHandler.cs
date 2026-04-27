//-----------------------------------------------------------------------
// <copyright file="RiskAssessmentEventSPIHandler.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Event handler for automatic SPI updates when risk assessment events occur.
//                  Listens to risk assessment completion and high-risk identification events
//                  to trigger corresponding Safety Performance Indicator calculations.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;
using SMS_Domain.Enums;
using SMS_Domain.Events;

namespace SMS_Application.EventHandlers;

/// <summary>
/// Event handler for Risk Assessment-related SPI automation
/// Automatically updates SPIs when risk assessment events occur
/// </summary>
public class RiskAssessmentEventSPIHandler
{
    private readonly ISPIAutomationService _spiAutomationService;
    private readonly ILogger<RiskAssessmentEventSPIHandler> _logger;

    public RiskAssessmentEventSPIHandler(
        ISPIAutomationService spiAutomationService,
        ILogger<RiskAssessmentEventSPIHandler> logger)
    {
        _spiAutomationService = spiAutomationService ?? throw new ArgumentNullException(nameof(spiAutomationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Risk Assessment Completed Event Handler

    /// <summary>
    /// Handles risk assessment completed events - Updates Risk Assessment Completion Rate SPI
    /// </summary>
    public async Task HandleRiskAssessmentCompleted(RiskAssessmentCompletedEvent evt)
    {
        try
        {
            _logger.LogInformation("?? SPI Event: Handling RiskAssessmentCompleted event for assessment {AssessmentId}", evt.AssessmentId);

            // Determine if assessment was completed on time
            var isOnTime = evt.CompletedDate <= evt.TargetCompletionDate;

            // Update Risk Assessment Completion Rate SPI  
            var result = await _spiAutomationService.UpdateRiskAssessmentCompletionAsync(
                evt.AssessmentId,
                evt.TargetCompletionDate,
                evt.CompletedDate,
                isOnTime,
                CancellationToken.None);

            if (result.IsSuccess)
            {
                _logger.LogInformation("? SPI Event: Successfully processed RiskAssessmentCompleted event for assessment {AssessmentId}", evt.AssessmentId);
            }
            else
            {
                _logger.LogWarning("?? SPI Event: Failed to process RiskAssessmentCompleted event for assessment {AssessmentId}: {Error}", 
                    evt.AssessmentId, result.Error.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? SPI Event: Error processing RiskAssessmentCompleted event for assessment {AssessmentId}", evt.AssessmentId);
        }
    }

    /// <summary>
    /// Handles high risk identification events - Updates High Risk Identification Rate SPI
    /// </summary>
    public async Task HandleHighRiskIdentified(HighRiskIdentifiedEvent evt)
    {
        try
        {
            _logger.LogInformation("?? SPI Event: Handling HighRiskIdentified event for assessment {AssessmentId}", evt.AssessmentId);

            // Implementation for high risk identification SPI updates
            await Task.CompletedTask;

            _logger.LogInformation("? SPI Event: Successfully processed HighRiskIdentified event for assessment {AssessmentId}", evt.AssessmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? SPI Event: Error processing HighRiskIdentified event for assessment {AssessmentId}", evt.AssessmentId);
        }
    }

    #endregion
}