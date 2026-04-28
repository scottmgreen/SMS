//-----------------------------------------------------------------------
// <copyright file="IRiskAssessmentService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application service interface for SMS risk assessment management.
//                  Provides business logic operations for risk assessment entities.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Application.Interfaces;

/// <summary>
/// Application service interface for Risk Assessment management and business operations
/// </summary>
public interface IRiskAssessmentService
{
    /// <summary>
    /// Creates a new risk assessment
    /// </summary>
    Task<Result<RiskAssessment>> CreateRiskAssessmentAsync(RiskAssessment assessment, CancellationToken ct = default);

    /// <summary>
    /// Gets risk assessment by ID
    /// </summary>
    Task<Result<RiskAssessment>> GetRiskAssessmentByIdAsync(RiskAssessmentID id, CancellationToken ct = default);

    /// <summary>
    /// Gets all risk assessments for a specific hazard
    /// </summary>
    Task<Result<List<RiskAssessment>>> GetRiskAssessmentsByHazardCodeAsync(string hazardCode, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing risk assessment
    /// </summary>
    Task<Result<RiskAssessment>> UpdateRiskAssessmentAsync(RiskAssessment assessment, CancellationToken ct = default);

    /// <summary>
    /// Deletes a risk assessment
    /// </summary>
    Task<Result<bool>> DeleteRiskAssessmentAsync(RiskAssessmentID id, CancellationToken ct = default);

    /// <summary>
    /// Calculates risk level based on assessment parameters
    /// </summary>
    Task<Result<string>> CalculateRiskLevelAsync(RiskAssessment assessment, CancellationToken ct = default);
}