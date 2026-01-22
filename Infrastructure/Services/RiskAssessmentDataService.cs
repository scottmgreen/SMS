// =============================================
// SMS DATA SERVICES - RISK ASSESSMENT ENHANCED FOR STEPS 1-5
// Enhanced RiskAssessmentDataService with Steps 1-5 support
// =============================================

using SMS_Domain.Errors;

namespace SMS_Infrastructure.Services;

/// <summary>
/// Enhanced RiskAssessmentDataService with Steps 1-5 support
/// Provides step-specific update methods for the Risk Assessment Wizard
/// </summary>
public class RiskAssessmentDataService : BaseDataService<RiskAssessmentDataService>
{
    private readonly ILogger<RiskAssessmentDataService> _logger;
    private readonly string _logheader;
    private readonly RiskAssessmentRepository _repo;

    public RiskAssessmentDataService(ILogger<RiskAssessmentDataService> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, RiskAssessmentRepository repo)
        : base(logger, serviceScopeFactory, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _repo = repo;

        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} {repo.GetType().Name} - Enhanced for Steps 1-5");
    }

    #region Basic CRUD Operations

    public Task<Result<RiskAssessment>> CreateRiskAssessmentAsync(RiskAssessment riskAssessment, CancellationToken ct = default)
    {
        return _repo.CreateRiskAssessmentAsync(riskAssessment, ct);
    }

    public async Task<Result<RiskAssessment>> GetRiskAssessmentByIdAsync(RiskAssessmentID riskAssessmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving RiskAssessment by ID: {Id}", riskAssessmentId);

            var result = await _repo.GetRiskAssessmentByCodeAsync(riskAssessmentId, cancellationToken).ConfigureAwait(false);

            if (result.IsFailure)
            {
                _logger.LogWarning("RiskAssessment not found with ID: {Id}", riskAssessmentId);
                return result;
            }

            _logger.LogInformation("Successfully retrieved RiskAssessment: {Id}", riskAssessmentId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve RiskAssessment by ID: {Id}", riskAssessmentId);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.NotFound);
        }
    }

    public async Task<Result<List<RiskAssessment>>> GetRiskAssessmentsByHazardIdAsync(HazardID hazardId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving RiskAssessment by ID: {Id}", hazardId);

            var result = await _repo.GetRiskAssessmentsByHazardIdAsync(hazardId, cancellationToken).ConfigureAwait(false);

            if (result.IsFailure)
            {
                _logger.LogWarning("RiskAssessment not found with ID: {Id}", hazardId);
                return result;
            }

            _logger.LogInformation("Successfully retrieved RiskAssessment: {Id}", hazardId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve RiskAssessment by ID: {Id}", hazardId);
            return Result<List<RiskAssessment>>.Failure<List<RiskAssessment>>(DomainErrors.RiskAssessmentError.NotFound);
        }
    }



    public Task<Result<List<RiskAssessment>>> GetAllRiskAssessmentsAsync(CancellationToken ct = default)
    {
        return _repo.GetAllRiskAssessmentsAsync(ct);
    }

    public Task<Result<RiskAssessment>> UpdateRiskAssessmentAsync(RiskAssessment riskAssessment, CancellationToken ct = default)
    {
        return _repo.UpdateRiskAssessmentAsync(riskAssessment, ct);
    }

    public async Task<Result<bool>> DeleteRiskAssessmentAsync(RiskAssessmentID riskAssessmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting RiskAssessment with ID: {Id}", riskAssessmentId);

            var result = await _repo.DeleteRiskAssessmentAsync(riskAssessmentId, cancellationToken).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted RiskAssessment with ID: {Id}", riskAssessmentId);
            }
            else
            {
                _logger.LogWarning("Failed to delete RiskAssessment with ID: {Id}", riskAssessmentId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting RiskAssessment with ID: {Id}", riskAssessmentId);
            return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.DeletionFailed);
        }
    }

    #endregion

    #region Step-Specific Update Methods - NEW for Steps 1-5 Support

    /// <summary>
    /// Saves Step 1 - System Description data
    /// </summary>
    public async Task<Result<RiskAssessment>> SaveStep1Async(
        string riskAssessmentId,
        string leadAssessorId,
        string systemDescription,
        string systemBoundaries,
        string systemPurpose,
        string fiveMPersonnel,
        string fiveMEquipment,
        string fiveMProcedures,
        string fiveMResources,
        string fiveMPhysicalEnvironment,
        string fiveMOperationalEnvironment,
        string updatedBy = "SYSTEM",
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Saving Step 1 data for RiskAssessment: {Id}", riskAssessmentId);

            var result = await _repo.UpdateStep1Async(
                riskAssessmentId,
                leadAssessorId,
                systemDescription,
                systemBoundaries,
                systemPurpose,
                fiveMPersonnel,
                fiveMEquipment,
                fiveMProcedures,
                fiveMResources,
                fiveMPhysicalEnvironment,
                fiveMOperationalEnvironment,
                updatedBy,
                ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully saved Step 1 data for RiskAssessment: {Id}", riskAssessmentId);
            }
            else
            {
                _logger.LogWarning("Failed to save Step 1 data for RiskAssessment: {Id}", riskAssessmentId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving Step 1 data for RiskAssessment: {Id}", riskAssessmentId);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }

    /// <summary>
    /// Saves Step 3 - Risk Analysis data
    /// </summary>
    public async Task<Result<RiskAssessment>> SaveStep3Async(
        string riskAssessmentId,
        string riskAnalysisMethod,
        string riskCriteria,
        string updatedBy = "SYSTEM",
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Saving Step 3 data for RiskAssessment: {Id}", riskAssessmentId);

            var result = await _repo.UpdateStep3Async(
                riskAssessmentId,
                riskAnalysisMethod,
                riskCriteria,
                updatedBy,
                ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully saved Step 3 data for RiskAssessment: {Id}", riskAssessmentId);
            }
            else
            {
                _logger.LogWarning("Failed to save Step 3 data for RiskAssessment: {Id}", riskAssessmentId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving Step 3 data for RiskAssessment: {Id}", riskAssessmentId);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }

    /// <summary>
    /// Saves Step 4 - Risk Assessment data
    /// </summary>
    public async Task<Result<RiskAssessment>> SaveStep4Async(
        string riskAssessmentId,
        string tolerabilityFramework,
        string riskAcceptanceCriteria,
        int? finalSeverityScore,
        int? finalLikelihoodScore,
        string finalRiskLevel,
        string riskTolerability,
        string assessmentRationale,
        string updatedBy = "SYSTEM",
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Saving Step 4 data for RiskAssessment: {Id}", riskAssessmentId);

            var result = await _repo.UpdateStep4Async(
                riskAssessmentId,
                tolerabilityFramework,
                riskAcceptanceCriteria,
                finalSeverityScore,
                finalLikelihoodScore,
                finalRiskLevel,
                riskTolerability,
                assessmentRationale,
                updatedBy,
                ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully saved Step 4 data for RiskAssessment: {Id}", riskAssessmentId);
            }
            else
            {
                _logger.LogWarning("Failed to save Step 4 data for RiskAssessment: {Id}", riskAssessmentId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving Step 4 data for RiskAssessment: {Id}", riskAssessmentId);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }

    /// <summary>
    /// Saves Step 5 - Implementation data
    /// </summary>
    public async Task<Result<RiskAssessment>> SaveStep5Async(
        string riskAssessmentId,
        string implementationStrategy,
        DateTime? overallTargetDate,
        string implementationNotes,
        string updatedBy = "SYSTEM",
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Saving Step 5 data for RiskAssessment: {Id}", riskAssessmentId);

            var result = await _repo.UpdateStep5Async(
                riskAssessmentId,
                implementationStrategy,
                overallTargetDate,
                implementationNotes,
                updatedBy,
                ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully saved Step 5 data for RiskAssessment: {Id}", riskAssessmentId);
            }
            else
            {
                _logger.LogWarning("Failed to save Step 5 data for RiskAssessment: {Id}", riskAssessmentId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving Step 5 data for RiskAssessment: {Id}", riskAssessmentId);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }

    /// <summary>
    /// Updates progress tracking data
    /// </summary>
    public async Task<Result<RiskAssessment>> UpdateProgressAsync(
        string riskAssessmentId,
        int currentStep,
        string completedSteps,
        int completionPercentage,
        string status = null,
        string stage = null,
        string updatedBy = "SYSTEM",
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Updating progress for RiskAssessment: {Id}, Step: {Step}, Completion: {Percentage}%",
                riskAssessmentId, currentStep, completionPercentage);

            var result = await _repo.UpdateProgressAsync(
                riskAssessmentId,
                currentStep,
                completedSteps,
                completionPercentage,
                status,
                stage,
                updatedBy,
                ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated progress for RiskAssessment: {Id}", riskAssessmentId);
            }
            else
            {
                _logger.LogWarning("Failed to update progress for RiskAssessment: {Id}", riskAssessmentId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating progress for RiskAssessment: {Id}", riskAssessmentId);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }

    #endregion

    #region Convenience Methods for Step Models

    /// <summary>
    /// Saves data from Step1 model properties to the database
    /// </summary>
    public async Task<Result<RiskAssessment>> SaveFromStep1DataAsync(
        string riskAssessmentId,
        string leadAssessor,
        string systemDescription,
        string systemBoundaries,
        string systemPurpose,
        string fiveMPersonnel,
        string fiveMEquipment,
        string fiveMProcedures,
        string fiveMResources,
        string fiveMPhysicalEnvironment,
        string fiveMOperationalEnvironment,
        string updatedBy = "SYSTEM",
        CancellationToken ct = default)
    {
        return await SaveStep1Async(
            riskAssessmentId,
            leadAssessor,
            systemDescription,
            systemBoundaries,
            systemPurpose,
            fiveMPersonnel,
            fiveMEquipment,
            fiveMProcedures,
            fiveMResources,
            fiveMPhysicalEnvironment,
            fiveMOperationalEnvironment,
            updatedBy,
            ct);
    }

    /// <summary>
    /// Generic save method that routes to the appropriate step save method
    /// Note: Step models should be converted to primitive parameters before calling this
    /// </summary>
    public async Task<Result<RiskAssessment>> SaveStepDataAsync(
        string riskAssessmentId,
        int stepNumber,
        Dictionary<string, object> stepData,
        string updatedBy = "SYSTEM",
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Saving Step {Step} data for RiskAssessment: {Id}", stepNumber, riskAssessmentId);

            return stepNumber switch
            {
                1 => await SaveStep1Async(
                    riskAssessmentId,
                    stepData.GetValueOrDefault("LeadAssessor", "")?.ToString() ?? "",
                    stepData.GetValueOrDefault("SystemDescription", "")?.ToString() ?? "",
                    stepData.GetValueOrDefault("SystemBoundaries", "")?.ToString() ?? "",
                    stepData.GetValueOrDefault("SystemPurpose", "")?.ToString() ?? "",
                    stepData.GetValueOrDefault("FiveMPersonnel", "")?.ToString() ?? "",
                    stepData.GetValueOrDefault("FiveMEquipment", "")?.ToString() ?? "",
                    stepData.GetValueOrDefault("FiveMProcedures", "")?.ToString() ?? "",
                    stepData.GetValueOrDefault("FiveMResources", "")?.ToString() ?? "",
                    stepData.GetValueOrDefault("FiveMPhysicalEnvironment", "")?.ToString() ?? "",
                    stepData.GetValueOrDefault("FiveMOperationalEnvironment", "")?.ToString() ?? "",
                    updatedBy, ct),

                3 => await SaveStep3Async(
                    riskAssessmentId,
                    stepData.GetValueOrDefault("RiskAnalysisMethod", "SMS Risk Matrix")?.ToString() ?? "SMS Risk Matrix",
                    stepData.GetValueOrDefault("RiskCriteria", "")?.ToString() ?? "",
                    updatedBy, ct),

                4 => await SaveStep4Async(
                    riskAssessmentId,
                    stepData.GetValueOrDefault("TolerabilityFramework", "PDX-SMS Default")?.ToString() ?? "PDX-SMS Default",
                    stepData.GetValueOrDefault("RiskAcceptanceCriteria", "")?.ToString() ?? "",
                    stepData.GetValueOrDefault("FinalSeverityScore", null) as int?,
                    stepData.GetValueOrDefault("FinalLikelihoodScore", null) as int?,
                    stepData.GetValueOrDefault("FinalRiskLevel", "")?.ToString() ?? "",
                    stepData.GetValueOrDefault("RiskTolerability", "")?.ToString() ?? "",
                    stepData.GetValueOrDefault("AssessmentRationale", "")?.ToString() ?? "",
                    updatedBy, ct),

                5 => await SaveStep5Async(
                    riskAssessmentId,
                    stepData.GetValueOrDefault("ImplementationStrategy", "")?.ToString() ?? "",
                    stepData.GetValueOrDefault("OverallTargetDate", null) as DateTime?,
                    stepData.GetValueOrDefault("ImplementationNotes", "")?.ToString() ?? "",
                    updatedBy, ct),

                _ => Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.InvalidStep)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving Step {Step} data for RiskAssessment: {Id}", stepNumber, riskAssessmentId);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }

    #endregion

    #region Validation and Business Logic Methods

    /// <summary>
    /// Validates if a step can be saved based on current assessment state
    /// </summary>
    public async Task<Result<bool>> ValidateStepCanBeSavedAsync(
        string riskAssessmentId,
        int stepNumber,
        CancellationToken ct = default)
    {
        try
        {
            var assessmentResult = await GetRiskAssessmentByIdAsync(new RiskAssessmentID(riskAssessmentId), ct);

            if (assessmentResult.IsFailure)
            {
                return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.NotFound);
            }

            var assessment = assessmentResult.Value;

            // Check if assessment is in a state that allows editing
            if (assessment.Status == RiskAssessmentStatus.Completed)
            {
                return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.CannotModifyCompleted);
            }

            // Check if trying to save a step that's too far ahead
            if (stepNumber > assessment.CurrentStep + 1)
            {
                return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.UpdateFailed);
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating step save for RiskAssessment: {Id}, Step: {Step}", riskAssessmentId, stepNumber);
            return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }

    /// <summary>
    /// Gets the completion status for all steps of an assessment
    /// </summary>
    public async Task<Result<Dictionary<int, bool>>> GetStepCompletionStatusAsync(
        string riskAssessmentId,
        CancellationToken ct = default)
    {
        try
        {
            var assessmentResult = await GetRiskAssessmentByIdAsync(new RiskAssessmentID(riskAssessmentId), ct);

            if (assessmentResult.IsFailure)
            {
                return Result<Dictionary<int, bool>>.Failure<Dictionary<int, bool>>(DomainErrors.RiskAssessmentError.NotFound);
            }

            var assessment = assessmentResult.Value;
            var completionStatus = new Dictionary<int, bool>
            {
                { 1, assessment.IsStepCompleted(1) },
                { 2, assessment.IsStepCompleted(2) },
                { 3, assessment.IsStepCompleted(3) },
                { 4, assessment.IsStepCompleted(4) },
                { 5, assessment.IsStepCompleted(5) }
            };

            return Result<Dictionary<int, bool>>.Success(completionStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting step completion status for RiskAssessment: {Id}", riskAssessmentId);
            return Result<Dictionary<int, bool>>.Failure<Dictionary<int, bool>>(DomainErrors.RiskAssessmentError.NotFound);
        }
    }

    #endregion
}
