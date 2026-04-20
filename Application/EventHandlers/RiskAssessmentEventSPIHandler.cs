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
                evt.StartDate,
                evt.CompletedDate,
                isOnTime,
                CancellationToken.None);

            if (result.IsSuccess)
            {
                _logger.LogInformation("? SPI Event: Successfully processed RiskAssessmentCompleted event for assessment {AssessmentId} - On Time: {IsOnTime}", 
                    evt.AssessmentId, isOnTime);
            }
            else
            {
                _logger.LogError("? SPI Event: Failed to process RiskAssessmentCompleted event for assessment {AssessmentId} - Error: {Error}", 
                    evt.AssessmentId, result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? SPI Event: Exception handling RiskAssessmentCompleted event for assessment {AssessmentId}", evt.AssessmentId);
        }
    }

    #endregion

    #region High Risk Identified Event Handler

    /// <summary>
    /// Handles high risk identified events - Updates High Risk Exposure SPI
    /// </summary>
    public async Task HandleHighRiskIdentified(HighRiskIdentifiedEvent evt)
    {
        try
        {
            _logger.LogInformation("?? SPI Event: Handling HighRiskIdentified event - Risk Level: {RiskLevel}", evt.RiskLevel);

            // Update High Risk Exposure SPI
            var result = await _spiAutomationService.UpdateHighRiskExposureAsync(
                evt.RiskLevel,
                evt.IdentifiedDate,
                CancellationToken.None);

            if (result.IsSuccess)
            {
                _logger.LogInformation("? SPI Event: Successfully processed HighRiskIdentified event for risk level {RiskLevel}", evt.RiskLevel);
            }
            else
            {
                _logger.LogError("? SPI Event: Failed to process HighRiskIdentified event for risk level {RiskLevel} - Error: {Error}", 
                    evt.RiskLevel, result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? SPI Event: Exception handling HighRiskIdentified event for risk level {RiskLevel}", evt.RiskLevel);
        }
    }

    #endregion
}

/// <summary>
/// Event triggered when a validation decision is made on a report
/// </summary>
public class ValidationDecisionEvent
{
    public string ReportId { get; set; } = string.Empty;
    public string ReportCode { get; set; } = string.Empty;
    public string ValidationDecision { get; set; } = string.Empty; // Use ValidationDecision enum values
    public DateTime ValidatedDate { get; set; }
    public string ValidatedBy { get; set; } = string.Empty;
    public string ValidationComments { get; set; } = string.Empty;
    public bool IsSMSRisk => ValidationDecision == SMS_Domain.Enums.ValidationDecision.SmsRisk.Value;

    public ValidationDecisionEvent(string reportId, string reportCode, string validationDecision, 
        DateTime validatedDate, string validatedBy)
    {
        ReportId = reportId;
        ReportCode = reportCode;
        ValidationDecision = validationDecision;
        ValidatedDate = validatedDate;
        ValidatedBy = validatedBy;
    }
}

#region Event Models

/// <summary>
/// Event model for risk assessment completion
/// </summary>
public class RiskAssessmentCompletedEvent
{
    public string AssessmentId { get; set; } = string.Empty;
    public string AssessmentCode { get; set; } = string.Empty;
    public string HazardId { get; set; } = string.Empty;
    public string ReportId { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime CompletedDate { get; set; }
    public DateTime TargetCompletionDate { get; set; }
    public string CompletedBy { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = string.Empty;
    public decimal RiskScore { get; set; }
    public string AssessmentType { get; set; } = string.Empty; // Preliminary, Technical

    public RiskAssessmentCompletedEvent() { }

    public RiskAssessmentCompletedEvent(string assessmentId, string assessmentCode, DateTime startDate, 
        DateTime completedDate, DateTime targetCompletionDate, string completedBy)
    {
        AssessmentId = assessmentId;
        AssessmentCode = assessmentCode;
        StartDate = startDate;
        CompletedDate = completedDate;
        TargetCompletionDate = targetCompletionDate;
        CompletedBy = completedBy;
    }
}

/// <summary>
/// Event model for high risk identification
/// </summary>
public class HighRiskIdentifiedEvent
{
    public string AssessmentId { get; set; } = string.Empty;
    public string HazardId { get; set; } = string.Empty;
    public string ReportId { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = string.Empty;
    public decimal RiskScore { get; set; }
    public DateTime IdentifiedDate { get; set; }
    public string IdentifiedBy { get; set; } = string.Empty;
    public string RiskDescription { get; set; } = string.Empty;
    public string ImpactArea { get; set; } = string.Empty;

    public HighRiskIdentifiedEvent() { }

    public HighRiskIdentifiedEvent(string assessmentId, string hazardId, string riskLevel, 
        decimal riskScore, DateTime identifiedDate, string identifiedBy)
    {
        AssessmentId = assessmentId;
        HazardId = hazardId;
        RiskLevel = riskLevel;
        RiskScore = riskScore;
        IdentifiedDate = identifiedDate;
        IdentifiedBy = identifiedBy;
    }
}

#endregion