//-----------------------------------------------------------------------
// <copyright file="SPIEventCoordinator.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Central coordinator for SPI event handling and automation.
//                  Orchestrates all SPI-related event handlers and provides unified
//                  interface for triggering SPI calculations from SMS operations.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.EventHandlers;
using SMS_Application.Interfaces;

namespace SMS_Application.Services;

/// <summary>
/// Central coordinator for SPI event handling and automation
/// Provides unified interface for triggering SPI calculations from SMS operations
/// </summary>
public class SPIEventCoordinator
{
    private readonly HazardEventSPIHandler _hazardHandler;
    private readonly RiskAssessmentEventSPIHandler _riskAssessmentHandler;
    private readonly MitigationEventSPIHandler _mitigationHandler;
    private readonly ISPIAutomationService _spiAutomationService;
    private readonly ILogger<SPIEventCoordinator> _logger;

    public SPIEventCoordinator(
        HazardEventSPIHandler hazardHandler,
        RiskAssessmentEventSPIHandler riskAssessmentHandler,
        MitigationEventSPIHandler mitigationHandler,
        ISPIAutomationService spiAutomationService,
        ILogger<SPIEventCoordinator> logger)
    {
        _hazardHandler = hazardHandler ?? throw new ArgumentNullException(nameof(hazardHandler));
        _riskAssessmentHandler = riskAssessmentHandler ?? throw new ArgumentNullException(nameof(riskAssessmentHandler));
        _mitigationHandler = mitigationHandler ?? throw new ArgumentNullException(nameof(mitigationHandler));
        _spiAutomationService = spiAutomationService ?? throw new ArgumentNullException(nameof(spiAutomationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Hazard Events

    /// <summary>
    /// Triggers SPI updates when a hazard is created
    /// Call this from HazardReporting or wherever hazards are created
    /// </summary>
    public async Task OnHazardCreated(string hazardId, string hazardCode, DateTime createdDate, string createdBy, 
        string reportId = "", string hazardType = "", string hazardCategory = "")
    {
        try
        {
            _logger.LogInformation("?? SPI Coordinator: Processing hazard created event for {HazardId}", hazardId);

            var evt = new HazardCreatedEvent(hazardId, hazardCode, createdDate, createdBy)
            {
                ReportId = reportId,
                HazardType = hazardType,
                HazardCategory = hazardCategory
            };

            await _hazardHandler.HandleHazardCreated(evt);

            _logger.LogInformation("? SPI Coordinator: Completed hazard created event processing for {HazardId}", hazardId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? SPI Coordinator: Error processing hazard created event for {HazardId}", hazardId);
        }
    }

    /// <summary>
    /// Triggers SPI updates when a hazard is closed
    /// Call this from hazard closure operations
    /// </summary>
    public async Task OnHazardClosed(string hazardId, string hazardCode, DateTime submittedDate, DateTime closedDate, 
        string closedBy, string closureReason = "", string resolution = "")
    {
        try
        {
            _logger.LogInformation("?? SPI Coordinator: Processing hazard closed event for {HazardId}", hazardId);

            var evt = new HazardClosedEvent(hazardId, hazardCode, submittedDate, closedDate, closedBy)
            {
                ClosureReason = closureReason,
                Resolution = resolution
            };

            await _hazardHandler.HandleHazardClosed(evt);

            _logger.LogInformation("? SPI Coordinator: Completed hazard closed event processing for {HazardId}", hazardId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? SPI Coordinator: Error processing hazard closed event for {HazardId}", hazardId);
        }
    }

    #endregion

    #region Risk Assessment Events

    /// <summary>
    /// Triggers SPI updates when a risk assessment is completed
    /// Call this from risk assessment completion operations
    /// </summary>
    public async Task OnRiskAssessmentCompleted(string assessmentId, string assessmentCode, DateTime startDate,
        DateTime completedDate, DateTime targetCompletionDate, string completedBy, string hazardId = "",
        string reportId = "", string riskLevel = "", decimal riskScore = 0, string assessmentType = "")
    {
        try
        {
            _logger.LogInformation("?? SPI Coordinator: Processing risk assessment completed event for {AssessmentId}", assessmentId);

            var evt = new RiskAssessmentCompletedEvent(assessmentId, assessmentCode, startDate,
                completedDate, targetCompletionDate, completedBy)
            {
                HazardId = hazardId,
                ReportId = reportId,
                RiskLevel = riskLevel,
                RiskScore = riskScore,
                AssessmentType = assessmentType
            };

            await _riskAssessmentHandler.HandleRiskAssessmentCompleted(evt);

            _logger.LogInformation("? SPI Coordinator: Completed risk assessment completed event processing for {AssessmentId}", assessmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? SPI Coordinator: Error processing risk assessment completed event for {AssessmentId}", assessmentId);
        }
    }

    /// <summary>
    /// Triggers SPI updates when high risk is identified
    /// Call this from risk assessment operations when Critical or High risk is identified
    /// </summary>
    public async Task OnHighRiskIdentified(string assessmentId, string hazardId, string riskLevel, decimal riskScore,
        DateTime identifiedDate, string identifiedBy, string reportId = "", string riskDescription = "", string impactArea = "")
    {
        try
        {
            _logger.LogInformation("?? SPI Coordinator: Processing high risk identified event - Risk Level: {RiskLevel}", riskLevel);

            var evt = new HighRiskIdentifiedEvent(assessmentId, "", riskLevel, riskScore, identifiedDate, identifiedBy)
            {
                ReportId = reportId,
                RiskDescription = riskDescription,
                RiskScore = riskScore
            };

            await _riskAssessmentHandler.HandleHighRiskIdentified(evt);

            _logger.LogInformation("? SPI Coordinator: Completed high risk identified event processing for risk level {RiskLevel}", riskLevel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? SPI Coordinator: Error processing high risk identified event for risk level {RiskLevel}", riskLevel);
        }
    }

    /// <summary>
    /// Triggers SPI updates when a validation decision is made
    /// Call this from ReportValidation when ValidationDecision is set
    /// </summary>
    public async Task OnValidationDecisionMade(string reportId, string reportCode, string validationDecision,
        DateTime validatedDate, string validatedBy, string validationComments = "")
    {
        try
        {
            _logger.LogInformation("?? SPI Coordinator: Processing validation decision event for {ReportCode} - Decision: {Decision}", 
                reportCode, validationDecision);

            var evt = new ValidationDecisionEvent(reportId, reportCode, validationDecision, validatedDate, validatedBy)
            {
                ValidationComments = validationComments
            };

            // Use the risk identification effectiveness automation we created earlier
            await _spiAutomationService.UpdateRiskIdentificationEffectivenessAsync(reportCode, validatedDate, validationDecision, CancellationToken.None);

            _logger.LogInformation("? SPI Coordinator: Completed validation decision event processing for {ReportCode}", reportCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? SPI Coordinator: Error processing validation decision event for {ReportCode}", reportCode);
        }
    }

    #endregion

    #region Mitigation Events

    /// <summary>
    /// Triggers SPI updates when a mitigation is completed
    /// Call this from mitigation completion operations
    /// </summary>
    public async Task OnMitigationCompleted(string mitigationId, string mitigationCode, string hazardId,
        DateTime targetCompletionDate, DateTime completedDate, string completedBy, string reportId = "",
        string completionNotes = "", string effectivenessRating = "")
    {
        try
        {
            _logger.LogInformation("?? SPI Coordinator: Processing mitigation completed event for {MitigationId}", mitigationId);

            var evt = new MitigationCompletedEvent(mitigationId, mitigationCode, hazardId, 
                targetCompletionDate, completedDate, completedBy)
            {
                ReportId = reportId,
                CompletionNotes = completionNotes,
                EffectivenessRating = effectivenessRating
            };

            await _mitigationHandler.HandleMitigationCompleted(evt);

            _logger.LogInformation("? SPI Coordinator: Completed mitigation completed event processing for {MitigationId}", mitigationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? SPI Coordinator: Error processing mitigation completed event for {MitigationId}", mitigationId);
        }
    }

    /// <summary>
    /// Triggers SPI updates when a mitigation becomes overdue
    /// Call this from scheduled checks or when mitigation status changes
    /// </summary>
    public async Task OnMitigationOverdue(string mitigationId, string mitigationCode, string hazardId,
        DateTime targetCompletionDate, int daysOverdue, string reportId = "", string assignedTo = "", string priority = "")
    {
        try
        {
            _logger.LogInformation("?? SPI Coordinator: Processing mitigation overdue event for {MitigationId} - Days overdue: {Days}", 
                mitigationId, daysOverdue);

            var evt = new MitigationOverdueEvent(mitigationId, mitigationCode, hazardId, targetCompletionDate, daysOverdue)
            {
                ReportId = reportId,
                AssignedTo = assignedTo,
                Priority = priority
            };

            await _mitigationHandler.HandleMitigationOverdue(evt);

            _logger.LogInformation("? SPI Coordinator: Completed mitigation overdue event processing for {MitigationId}", mitigationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? SPI Coordinator: Error processing mitigation overdue event for {MitigationId}", mitigationId);
        }
    }

    /// <summary>
    /// Triggers SPI updates when mitigation status changes
    /// Call this from mitigation update operations
    /// </summary>
    public async Task OnMitigationStatusChanged(string mitigationId, string mitigationCode, string hazardId,
        string oldStatus, string newStatus, DateTime statusChangedDate, string changedBy,
        DateTime targetCompletionDate, string reportId = "", string changeReason = "")
    {
        try
        {
            _logger.LogInformation("?? SPI Coordinator: Processing mitigation status changed event for {MitigationId} - {OldStatus} ? {NewStatus}", 
                mitigationId, oldStatus, newStatus);

            var evt = new MitigationStatusChangedEvent(mitigationId, mitigationCode, hazardId, 
                oldStatus, newStatus, statusChangedDate, changedBy)
            {
                TargetCompletionDate = targetCompletionDate,
                ReportId = reportId,
                ChangeReason = changeReason
            };

            await _mitigationHandler.HandleMitigationStatusChanged(evt);

            _logger.LogInformation("? SPI Coordinator: Completed mitigation status changed event processing for {MitigationId}", mitigationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? SPI Coordinator: Error processing mitigation status changed event for {MitigationId}", mitigationId);
        }
    }

    #endregion

    #region Batch Operations

    /// <summary>
    /// Triggers multiple SPI updates for batch operations
    /// Useful for data imports or bulk operations
    /// </summary>
    public async Task ProcessBatchEvents<T>(IEnumerable<T> events, Func<T, Task> processor, string operationType = "batch")
    {
        try
        {
            var eventList = events.ToList();
            _logger.LogInformation("?? SPI Coordinator: Processing batch {OperationType} events - Count: {Count}", operationType, eventList.Count);

            var tasks = eventList.Select(processor);
            await Task.WhenAll(tasks);

            _logger.LogInformation("? SPI Coordinator: Completed batch {OperationType} event processing - Count: {Count}", operationType, eventList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? SPI Coordinator: Error processing batch {OperationType} events", operationType);
        }
    }

    #endregion

    #region Integration Helpers

    /// <summary>
    /// Easy integration point for HazardReporting.razor.cs
    /// Call this after a new hazard is successfully created
    /// </summary>
    public async Task NotifyHazardCreated(string hazardCode, string createdBy, string reportId = "")
    {
        await OnHazardCreated(hazardCode, hazardCode, DateTime.UtcNow, createdBy, reportId);
    }

    /// <summary>
    /// Easy integration point for Risk Assessment completion
    /// Call this after a risk assessment is successfully completed
    /// </summary>
    public async Task NotifyRiskAssessmentCompleted(string assessmentCode, string completedBy, string riskLevel = "")
    {
        await OnRiskAssessmentCompleted(assessmentCode, assessmentCode, DateTime.UtcNow.AddDays(-7), 
            DateTime.UtcNow, DateTime.UtcNow, completedBy, riskLevel: riskLevel);
    }

    /// <summary>
    /// Easy integration point for Mitigation completion
    /// Call this after a mitigation is successfully completed
    /// </summary>
    public async Task NotifyMitigationCompleted(string mitigationCode, string completedBy, DateTime targetDate)
    {
        await OnMitigationCompleted(mitigationCode, mitigationCode, "", targetDate, DateTime.UtcNow, completedBy);
    }

    #endregion
}