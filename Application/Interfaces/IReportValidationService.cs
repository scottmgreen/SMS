//-----------------------------------------------------------------------
// <copyright file="IReportValidationService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application service interface for SMS report validation management.
//                  Provides business logic operations for report validation entities.
// </copyright>
//-----------------------------------------------------------------------

namespace Application.Interfaces;

/// <summary>
/// Application service interface for ReportValidation management and business operations
/// Handles complex business logic for report validation including risk assessment creation
/// </summary>
public interface IReportValidationService
{
    /// <summary>
    /// Creates a new report validation with complex business logic
    /// If validation decision is SMS Risk, creates associated risk assessments and analysis
    /// </summary>
    Task<Result<ReportValidation>> CreateReportValidationAsync(ReportValidation reportValidation, CancellationToken ct = default);

    /// <summary>
    /// Gets report validation by ID
    /// </summary>
    Task<Result<ReportValidation>> GetReportValidationByIdAsync(ReportValidationID id, CancellationToken ct = default);

    /// <summary>
    /// Gets all report validations
    /// </summary>
    Task<Result<List<ReportValidation>>> GetAllReportValidationsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets report validations by report code
    /// </summary>
    Task<Result<List<ReportValidation>>> GetReportValidationsByReportCodeAsync(string reportCode, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing report validation
    /// </summary>
    Task<Result<ReportValidation>> UpdateReportValidationAsync(ReportValidation reportValidation, CancellationToken ct = default);

    /// <summary>
    /// Deletes a report validation by code
    /// </summary>
    Task<Result<bool>> DeleteReportValidationAsync(ReportValidationID id, CancellationToken ct = default);

    /// <summary>
    /// Resets a report validation to require re-validation
    /// Clears validation decision, type, date, and validator
    /// </summary>
    Task<Result<bool>> ResetReportValidationAsync(ReportValidationID code, CancellationToken ct = default);

    /// <summary>
    /// Validates a report and sets appropriate status and decision
    /// </summary>
    Task<Result<ReportValidation>> ValidateReportAsync(ReportValidationID code, ValidationDecision decision, string validatedBy, string comments = null, CancellationToken ct = default);

    /// <summary>
    /// Creates risk assessments and analysis for SMS Risk validation decisions
    /// This encapsulates the complex business logic for risk management creation
    /// </summary>
    Task<Result<bool>> CreateSmsRiskAssessmentsAsync(string reportCode, CancellationToken ct = default);
}
