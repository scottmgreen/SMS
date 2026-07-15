//-----------------------------------------------------------------------
// <copyright file="ReportValidationService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application service for SMS report validation management.
//                  Provides business logic operations and coordinates domain entities.
//                  Handles complex SMS Risk assessment creation workflow.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Application.Interfaces;

using SMS_Domain.Entities;
using SMS_Domain.Entities;
using SMS_Domain.Enums;

namespace SMS_Application.Services;

/// <summary>
/// Application service for ReportValidation management and complex business operations
/// Encapsulates the complex SMS Risk assessment creation logic previously in CommandHandlers
/// </summary>
public sealed class ReportValidationService : IReportValidationService
{
    private readonly ReportValidationDataService _dataService;
    private readonly HazardDataService _hazardDataService;
    private readonly IRiskAssessmentService _riskAssessmentService;
    private readonly IRiskAnalysisService _riskAnalysisService;
    private readonly ReportService _reportService;
    private readonly ILogger<ReportValidationService> _logger;

    public ReportValidationService(
        ReportValidationDataService dataService,
        HazardDataService hazardDataService,
        IRiskAssessmentService riskAssessmentService,
        IRiskAnalysisService riskAnalysisService,
        ReportService reportService,
        ILogger<ReportValidationService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _hazardDataService = hazardDataService ?? throw new ArgumentNullException(nameof(hazardDataService));
        _riskAssessmentService = riskAssessmentService ?? throw new ArgumentNullException(nameof(riskAssessmentService));
        _riskAnalysisService = riskAnalysisService ?? throw new ArgumentNullException(nameof(riskAnalysisService));
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region IReportValidationService Implementation

    public async Task<Result<ReportValidation>> CreateReportValidationAsync(ReportValidation reportValidation, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation(" Creating report validation with code: {Code}", reportValidation?.Code);
            
            var result = await _dataService.CreateReportValidationAsync(reportValidation, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully created report validation with Code: {Code}", result.Value?.Code);

                // ?? COMPLEX BUSINESS LOGIC: Handle SMS Risk validation decision
                if (reportValidation?.ValidationDecision == ValidationDecision.SmsRisk.Value)
                {
                    _logger.LogApplicationInformation("Business Logic: Processing SMS Risk decision for Report: {ReportCode}", reportValidation.ReportCode);
                    
                    var isRiskRegistryOnly = IsRiskRegistryOnlyValidation(reportValidation.ValidationType);

                    var riskCreationResult = await CreateSmsRiskAssessmentsAsync(reportValidation.ReportCode, isRiskRegistryOnly, ct);
                    if (riskCreationResult.IsFailure)
                    {
                        _logger.LogApplicationWarning("Business Logic: Failed to create SMS Risk assessments for Report: {ReportCode}", reportValidation.ReportCode);
                        // Don't fail the entire operation - log and continue
                    }
                    else
                    {
                        _logger.LogApplicationInformation("Business Logic: Successfully created SMS Risk assessments for Report: {ReportCode}", reportValidation.ReportCode);
                    }
                }
            }
            else
            {
                _logger.LogApplicationError("Failed to create report validation. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error creating report validation");
            return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportError.CreateFailed);
        }
    }

    public async Task<Result<ReportValidation>> GetReportValidationByIdAsync(ReportValidationID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving report validation with ID: {Id}", id);
            return await _dataService.GetReportValidationByIdAsync(id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving report validation with ID: {Id}", id);
            return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportError.NotFound);
        }
    }

    public async Task<Result<List<ReportValidation>>> GetAllReportValidationsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving all report validations");
            return await _dataService.GetAllReportValidationsAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving all report validations");
            return Result<List<ReportValidation>>.Failure<List<ReportValidation>>(DomainErrors.ReportError.NullOrEmpty);
        }
    }

    public async Task<Result<List<ReportValidation>>> GetReportValidationsByReportCodeAsync(string reportCode, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving report validations for report code: {ReportCode}", reportCode);
            
            // Since DataService may not have this method, get all and filter
            var allValidationsResult = await _dataService.GetAllReportValidationsAsync(ct);
            if (allValidationsResult.IsFailure)
            {
                return Result<List<ReportValidation>>.Failure<List<ReportValidation>>(allValidationsResult.Error);
            }

            var filteredValidations = allValidationsResult.Value?
                .Where(rv => rv.ReportCode == reportCode)
                .ToList() ?? new List<ReportValidation>();

            return Result<List<ReportValidation>>.Success(filteredValidations);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving report validations for report code: {ReportCode}", reportCode);
            return Result<List<ReportValidation>>.Failure<List<ReportValidation>>(DomainErrors.ReportError.NotFound);
        }
    }

    public async Task<Result<ReportValidation>> UpdateReportValidationAsync(ReportValidation reportValidation, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation(" Updating report validation with Code: {Code}", reportValidation?.Code);
            var result = await _dataService.UpdateReportValidationAsync(reportValidation, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully updated report validation with Code: {Code}", reportValidation?.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to update report validation. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error updating report validation with Code: {Code}", reportValidation?.Code);
            return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteReportValidationAsync(ReportValidationID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation(" Deleting report validation with Code: {Code}", id);
            var result = await _dataService.DeleteReportValidationAsync(id, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully deleted report validation with Code: {Code}", id);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete report validation. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error deleting report validation with Code: {Code}", id);
            return Result<bool>.Failure<bool>(DomainErrors.ReportError.DeleteFailed);
        }
    }

    public async Task<Result<bool>> ResetReportValidationAsync(ReportValidationID code, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Business Logic: Resetting report validation {Code} for re-validation", code);
            
            var validationResult = await _dataService.GetReportValidationByIdAsync(code, ct);
            if (validationResult.IsFailure || validationResult.Value == null)
            {
                return Result<bool>.Failure<bool>(DomainErrors.ReportError.NotFound);
            }

            var validation = validationResult.Value;
            
            // CRITICAL: Reset the validation to make it appear in Validation tab again
            validation.Status = ReportValidationStatus.ValidationNeeded;
            validation.Stage = "INITIAL";
            validation.UpdatedDate = DateTime.UtcNow;

            // IMPORTANT: Clear these key fields so the validation appears as needing re-validation
            validation.ValidationType = null;        // Clear validation type - this is key!
            validation.ValidationDecision = null;    // Clear validation decision - this is key!
            validation.ValidatedDate = null;         // Clear validated date
            validation.ValidatedBy = null;          // Clear who validated it

            // Add a comment about the reset
            validation.ValidationComments = $"Reset on {DateTime.UtcNow:yyyy-MM-dd HH:mm} - Returned for re-validation";

            var updateResult = await _dataService.UpdateReportValidationAsync(validation, ct);
            
            if (updateResult.IsSuccess)
            {
                _logger.LogApplicationInformation("Business Logic: Successfully reset report validation {Code}", code);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogApplicationError("Failed to reset report validation {Code}. Error: {Error}", code, updateResult.Error?.Message);
                return Result<bool>.Failure<bool>(updateResult.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error resetting report validation with Code: {Code}", code);
            return Result<bool>.Failure<bool>(DomainErrors.ReportError.UpdateFailed);
        }
    }

    public async Task<Result<ReportValidation>> ValidateReportAsync(ReportValidationID code, ValidationDecision decision, string validatedBy, string comments = null, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Business Logic: Validating report {Code} with decision {Decision} by {ValidatedBy}", code, decision.Value, validatedBy);
            
            var validationResult = await _dataService.GetReportValidationByIdAsync(code, ct);
            if (validationResult.IsFailure || validationResult.Value == null)
            {
                return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportError.NotFound);
            }

            var validation = validationResult.Value;
            validation.ValidationDecision = decision.Value;
            validation.ValidatedBy = validatedBy;
            validation.ValidatedDate = DateTime.UtcNow;
            validation.ValidationComments = comments;
            validation.Status = ReportValidationStatus.ValidationComplete;
            validation.UpdatedDate = DateTime.UtcNow;

            var result = await _dataService.UpdateReportValidationAsync(validation, ct);
            
            // Handle SMS Risk decision
            if (decision == ValidationDecision.SmsRisk)
            {
                var isRiskRegistryOnly = IsRiskRegistryOnlyValidation(validation.ValidationType);

                await CreateSmsRiskAssessmentsAsync(validation.ReportCode, isRiskRegistryOnly, ct);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error validating report with Code: {Code}", code);
            return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportError.UpdateFailed);
        }
    }

    #endregion

    #region Complex Business Logic - SMS Risk Assessment Creation

    /// <summary>
    /// BUSINESS LOGIC: Creates risk assessments and analysis for SMS Risk validation decisions
    /// This encapsulates the complex logic previously scattered in CommandHandlers
    /// </summary>
    public async Task<Result<bool>> CreateSmsRiskAssessmentsAsync(string reportCode, bool isRiskRegistryOnly = false, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Complex Business Logic: Creating SMS Risk assessments for Report: {ReportCode}", reportCode);

            // Step 1: Get hazards for this report using HazardDataService
            var hazardsResult = await _hazardDataService.GetHazardsByReportCodeAsync(new ReportID(reportCode), ct);
            if (hazardsResult.IsFailure || !hazardsResult.Value?.Any() == true)
            {
                _logger.LogApplicationWarning("No hazards found for report {ReportCode}", reportCode);
                return Result<bool>.Success(false); // Not an error - just no hazards to process
            }

            var hazard = hazardsResult.Value.FirstOrDefault();
            if (hazard == null)
            {
                return Result<bool>.Success(false);
            }

            _logger.LogApplicationInformation("Found hazard {HazardCode} for report {ReportCode}", hazard.Code, reportCode);

            // Step 2: Check for existing RiskAssessments for this report (avoid duplicates)
            var existingAssessments = await FindExistingRiskAssessmentsForReportAsync(reportCode, isRiskRegistryOnly, ct);

            string initialRiskAssessmentCode;

            if (existingAssessments.InitialAssessment != null)
            {
                // Use existing shared RiskAssessments - perfect for multiple hazards per report!
                initialRiskAssessmentCode = existingAssessments.InitialAssessment.Code;
                _logger.LogApplicationInformation("Using existing Risk Assessment: {Code}", initialRiskAssessmentCode);
            }
            else
            {
                // Create NEW shared RiskAssessments only if none exist (first hazard in the report)
                initialRiskAssessmentCode = await CreateRiskAssessmentsForReportAsync(hazard, isRiskRegistryOnly, ct);
                _logger.LogApplicationInformation("Created new Risk Assessment: {Code}", initialRiskAssessmentCode);
            }

            // Step 3: ALWAYS create RiskAnalysis records linking this hazard to the shared assessments
            await CreateRiskAnalysisForHazardAsync(hazard.Code, initialRiskAssessmentCode, ct);

            _logger.LogApplicationInformation("Successfully completed SMS Risk assessment creation for Report: {ReportCode}", reportCode);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error creating SMS Risk assessments for report {ReportCode}", reportCode);
            return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.CreateFailed);
        }
    }

    /// <summary>
    /// Find existing RiskAssessments for this report using proper code-based matching
    /// This prevents creating duplicate RiskAssessments when adding hazards to existing reports
    /// </summary>
    private async Task<(RiskAssessment? InitialAssessment, RiskAssessment? ResidualAssessment)> FindExistingRiskAssessmentsForReportAsync(string reportCode, bool isRiskRegistryOnly, CancellationToken ct)
    {
        try
        {
            _logger.LogApplicationInformation("Searching for existing RiskAssessments for Report: {ReportCode}", reportCode);
            var expectedAssessmentType = isRiskRegistryOnly ? RiskAssessmentType.RiskRegistryOnly : RiskAssessmentType.Technical;

            // Get hazards for this specific report using HazardDataService
            var reportHazardsResult = await _hazardDataService.GetHazardsByReportCodeAsync(new ReportID(reportCode), ct);

            if (reportHazardsResult.IsSuccess && reportHazardsResult.Value?.Any() == true)
            {
                var reportHazards = reportHazardsResult.Value;
                var reportHazardCodes = reportHazards.Select(h => h.Code).ToList();
                
                _logger.LogApplicationInformation("Found {Count} hazards for Report {ReportCode}: {HazardCodes}", 
                    reportHazards.Count, reportCode, string.Join(", ", reportHazardCodes));

                // Get risk assessments by hazard codes
                var allAssessments = new List<RiskAssessment>();
                foreach (var hazardCode in reportHazardCodes)
                {
                    var assessmentsResult = await _riskAssessmentService.GetRiskAssessmentsByHazardCodeAsync(hazardCode, ct);
                    if (assessmentsResult.IsSuccess && assessmentsResult.Value?.Any() == true)
                    {
                        allAssessments.AddRange(assessmentsResult.Value);
                    }
                }

                if (allAssessments.Any())
                {
                    var uniqueAssessments = allAssessments
                        .Where(x => x is not null && !string.IsNullOrWhiteSpace(x.Code))
                        .GroupBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
                        .Select(g => g.OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate ?? DateTime.MinValue).First())
                        .ToList();

                    var initialAssessment = uniqueAssessments.FirstOrDefault(x => x.AssessmentType == expectedAssessmentType)
                        ?? (!isRiskRegistryOnly
                            ? uniqueAssessments.FirstOrDefault(x => x.AssessmentType == RiskAssessmentType.RiskRegistryOnly)
                            : uniqueAssessments.FirstOrDefault(x => x.AssessmentType == RiskAssessmentType.Technical))
                        ?? uniqueAssessments.FirstOrDefault();

                    if (!isRiskRegistryOnly &&
                        initialAssessment is not null &&
                        initialAssessment.AssessmentType == RiskAssessmentType.RiskRegistryOnly)
                    {
                        _logger.LogApplicationInformation(
                            "Reusing existing Risk Registry Only assessment {AssessmentCode} for report {ReportCode} by converting AssessmentType to Technical",
                            initialAssessment.Code,
                            reportCode);

                        initialAssessment.AssessmentType = RiskAssessmentType.Technical;
                        initialAssessment.CurrentStep = 1;
                        initialAssessment.Stage = RiskAssessmentStage.DescribingSystem;
                        initialAssessment.Status = RiskAssessmentStatus.AssessmentCreate;
                        initialAssessment.CompletedDate = null;
                        initialAssessment.CompletedBy = null;
                        initialAssessment.UpdatedDate = DateTime.UtcNow;
                        var updateAssessmentResult = await _riskAssessmentService.UpdateRiskAssessmentAsync(initialAssessment, ct);
                        if (updateAssessmentResult.IsSuccess && updateAssessmentResult.Value is not null)
                        {
                            initialAssessment = updateAssessmentResult.Value;
                        }
                        else
                        {
                            _logger.LogApplicationWarning(
                                "Failed to convert assessment {AssessmentCode} to Technical for report {ReportCode}. Continuing with existing assessment type. Error: {Error}",
                                initialAssessment.Code,
                                reportCode,
                                updateAssessmentResult.Error?.Message ?? "Unknown error");
                        }
                    }
                    else if (isRiskRegistryOnly &&
                             initialAssessment is not null &&
                             initialAssessment.AssessmentType == RiskAssessmentType.Technical)
                    {
                        _logger.LogApplicationInformation(
                            "Reusing existing Technical assessment {AssessmentCode} for report {ReportCode} by converting AssessmentType to RiskRegistryOnly",
                            initialAssessment.Code,
                            reportCode);

                        initialAssessment.AssessmentType = RiskAssessmentType.RiskRegistryOnly;
                        initialAssessment.CurrentStep = 4;
                        initialAssessment.Stage = RiskAssessmentStage.AssessingRisk;
                        initialAssessment.Status = RiskAssessmentStatus.AssessmentUnderway;
                        initialAssessment.CompletedDate = null;
                        initialAssessment.CompletedBy = null;
                        initialAssessment.UpdatedDate = DateTime.UtcNow;

                        var updateAssessmentResult = await _riskAssessmentService.UpdateRiskAssessmentAsync(initialAssessment, ct);
                        if (updateAssessmentResult.IsSuccess && updateAssessmentResult.Value is not null)
                        {
                            initialAssessment = updateAssessmentResult.Value;
                        }
                        else
                        {
                            _logger.LogApplicationWarning(
                                "Failed to convert assessment {AssessmentCode} to RiskRegistryOnly for report {ReportCode}. Continuing with existing assessment type. Error: {Error}",
                                initialAssessment.Code,
                                reportCode,
                                updateAssessmentResult.Error?.Message ?? "Unknown error");
                        }
                    }
                    
                    _logger.LogApplicationInformation("Found existing assessments: Technical={Technical}", 
                        initialAssessment?.Code ?? "None");
                    
                    return (initialAssessment, null);
                }
            }

            _logger.LogApplicationInformation("No existing RiskAssessments found for Report {ReportCode}", reportCode);
            return (null, null);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error finding existing RiskAssessments for report {ReportCode}", reportCode);
            return (null, null);
        }
    }

    /// <summary>
    /// Create shared RiskAssessments for this report (first hazard creates them, subsequent hazards reuse them)
    /// </summary>
    private async Task<string> CreateRiskAssessmentsForReportAsync(Hazard hazard, bool isRiskRegistryOnly, CancellationToken ct)
    {
        _logger.LogApplicationInformation("Creating new Risk Assessment for hazard {HazardCode}", hazard.Code);

        var assessmentType = isRiskRegistryOnly ? RiskAssessmentType.RiskRegistryOnly : RiskAssessmentType.Technical;
        var initialStep = isRiskRegistryOnly ? 4 : 1;
        var initialStage = isRiskRegistryOnly ? RiskAssessmentStage.AssessingRisk : RiskAssessmentStage.DescribingSystem;
        var initialStatus = isRiskRegistryOnly ? RiskAssessmentStatus.AssessmentUnderway : RiskAssessmentStatus.AssessmentCreate;
        var assessmentLabel = isRiskRegistryOnly ? RiskAssessmentType.RiskRegistryOnly.Name : RiskAssessmentType.Technical.Name;

        var initialRiskAssessment = new RiskAssessment(new RiskAssessmentID("RS-0000"))
        {
            HazardCode = hazard.Code,
            AssessmentType = assessmentType,
            CurrentStep = initialStep,
            Stage = initialStage,
            Status = initialStatus,
            PrimaryHazardId = hazard.Code,
            RiskAssessmentCategory = RiskAssessmentCategory.Technical,
            Description = $"{assessmentLabel} Risk Assessment for Report {hazard.ReportCode}",
            Name = $"{assessmentLabel} Risk Assessment - {hazard.ReportCode}"
        };

        var initialResult = await _riskAssessmentService.CreateRiskAssessmentAsync(initialRiskAssessment, ct);
        
        if (initialResult.IsSuccess)
        {
            _logger.LogApplicationInformation("Successfully created Risk Assessment: {Code}", initialResult.Value.Code);
            return initialResult.Value.Code;
        }
        else
        {
            _logger.LogApplicationError("Failed to create Risk Assessment: {Error}", initialResult.Error?.Message);
            throw new InvalidOperationException($"Failed to create Risk Assessment: {initialResult.Error?.Message}");
        }
    }

    /// <summary>
    /// Create RiskAnalysis records linking this hazard to its RiskAssessments
    /// </summary>
    private async Task CreateRiskAnalysisForHazardAsync(string hazardCode, string initialAssessmentCode, CancellationToken ct)
    {
        _logger.LogApplicationInformation("Creating Risk Analysis link: Hazard {HazardCode} -> Assessment {AssessmentCode}", 
            hazardCode, initialAssessmentCode);

        var initialRiskAnalysis = new RiskAnalysis(new RiskAnalysisID("RA-0000"))
        {
            AssessmentType = RiskAnalysisType.Initial,
            HazardCode = hazardCode,
            RiskAssessmentCode = initialAssessmentCode
        };

        var result = await _riskAnalysisService.CreateRiskAnalysisAsync(initialRiskAnalysis, ct);
        
        if (result.IsSuccess)
        {
            _logger.LogApplicationInformation("Successfully created Risk Analysis: {Code}", result.Value.Code);
        }
        else
        {
            _logger.LogApplicationError("Failed to create Risk Analysis: {Error}", result.Error?.Message);
        }
    }

    private static bool IsRiskRegistryOnlyValidation(string? validationType)
    {
        if (string.IsNullOrWhiteSpace(validationType))
        {
            return false;
        }

        return validationType.Equals(RiskAssessmentType.RiskRegistryOnly.Value, StringComparison.OrdinalIgnoreCase)
               || validationType.Equals(RiskAssessmentType.RiskRegistryOnly.Name, StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}


