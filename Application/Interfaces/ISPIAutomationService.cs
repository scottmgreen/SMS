//-----------------------------------------------------------------------
// <copyright file="ISPIAutomationService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Interface for SPI automation service defining automated calculation 
//                  and data point creation contracts for Safety Performance Indicators.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Application.Interfaces;

/// <summary>
/// Interface for SPI Automation Service - Defines contracts for automated SPI calculations
/// </summary>
public interface ISPIAutomationService
{
    #region Hazard Report SPI Automation

    /// <summary>
    /// Updates Hazard Report Rate SPI when a new hazard is created
    /// </summary>
    Task<Result<bool>> UpdateHazardReportRateAsync(DateTime reportDate, CancellationToken ct = default);

    /// <summary>
    /// Updates Time to Close Hazards SPI when a hazard is closed
    /// </summary>
    Task<Result<bool>> UpdateHazardClosureTimeAsync(string hazardId, DateTime submittedDate, DateTime closedDate, CancellationToken ct = default);

    #endregion

    #region Risk Assessment SPI Automation

    /// <summary>
    /// Updates Risk Assessment Completion Rate SPI when an assessment is completed
    /// </summary>
    Task<Result<bool>> UpdateRiskAssessmentCompletionAsync(string assessmentId, DateTime startDate, DateTime completedDate, bool isOnTime, CancellationToken ct = default);

    /// <summary>
    /// Updates High Risk Exposure SPI when high-risk conditions are identified
    /// </summary>
    Task<Result<bool>> UpdateHighRiskExposureAsync(string riskLevel, DateTime identifiedDate, CancellationToken ct = default);

    /// <summary>
    /// Updates Risk Identification Effectiveness SPI when validation decisions are made
    /// </summary>
    Task<Result<bool>> UpdateRiskIdentificationEffectivenessAsync(string reportCode, DateTime validatedDate, string validationDecision, CancellationToken ct = default);

    #endregion

    #region Mitigation SPI Automation

    /// <summary>
    /// Updates Mitigation Implementation Rate SPI when a mitigation is completed
    /// </summary>
    Task<Result<bool>> UpdateMitigationImplementationRateAsync(string mitigationId, DateTime targetDate, DateTime completedDate, CancellationToken ct = default);

    /// <summary>
    /// Updates Corrective Action Closure SPI when checking for overdue mitigations
    /// </summary>
    Task<Result<bool>> UpdateCorrectiveActionClosureAsync(DateTime calculationDate, CancellationToken ct = default);

    #endregion
}