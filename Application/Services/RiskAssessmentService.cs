using Microsoft.Extensions.Logging;

namespace SMS_Application.Services;

/// <summary>
/// Enhanced RiskAssessmentService with Steps 1-5 support
/// Provides application-level orchestration for risk assessment operations
/// </summary>
public sealed class RiskAssessmentService
{
    private readonly RiskAssessmentDataService _dataService;
    private readonly IMediator _mediator;
    private readonly ILogger<RiskAssessmentService> _logger;

    public RiskAssessmentService(
        RiskAssessmentDataService dataService, 
        IMediator mediator,
        ILogger<RiskAssessmentService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Basic CRUD Operations

    public async Task<Result<RiskAssessment>> CreateRiskAssessmentAsync(RiskAssessment riskAssessment, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Creating risk assessment with code: {Code}", riskAssessment?.Code);
            
            var command = new CreateRiskAssessmentCommand(riskAssessment);
            var result = await _mediator.SendAsync(command, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created risk assessment with ID: {Id}", result.Value?.Id);
            }
            else
            {
                _logger.LogError("Failed to create risk assessment. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating risk assessment");
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.CreateFailed);
        }
    }

    public async Task<Result<RiskAssessment>> GetRiskAssessmentByIdAsync(RiskAssessmentID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving risk assessment with ID: {Id}", id);
            return await _dataService.GetRiskAssessmentByIdAsync(id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving risk assessment with ID: {Id}", id);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.NotFound);
        }
    }
    public async Task<Result<List<RiskAssessment>>> GetRiskAssessmentsByHazardIdAsync(HazardID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving risk assessment with ID: {Id}", id);
            return await _dataService.GetRiskAssessmentsByHazardIdAsync(id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving risk assessment with Hazard ID: {Id}", id);
            return Result<List<RiskAssessment>>.Failure<List<RiskAssessment>>(DomainErrors.RiskAssessmentError.NotFound);
        }
    }
    public async Task<Result<List<RiskAssessment>>> GetAllRiskAssessmentsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving all risk assessments");
            return await _dataService.GetAllRiskAssessmentsAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving all risk assessments");
            return Result<List<RiskAssessment>>.Failure<List<RiskAssessment>>(DomainErrors.RiskAssessmentError.NullOrEmpty);
        }
    }

    public async Task<Result<RiskAssessment>> UpdateRiskAssessmentAsync(RiskAssessment riskAssessment, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Updating risk assessment with ID: {Id}", riskAssessment?.Id);
            
            var command = new UpdateRiskAssessmentCommand(riskAssessment);
            var result = await _mediator.SendAsync(command, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated risk assessment with ID: {Id}", riskAssessment?.Id);
            }
            else
            {
                _logger.LogError("Failed to update risk assessment. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating risk assessment with ID: {Id}", riskAssessment?.Id);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteRiskAssessmentAsync(RiskAssessmentID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Deleting risk assessment with ID: {Id}", id);
            
            var command = new DeleteRiskAssessmentCommand(id);
            var result = await _mediator.SendAsync(command, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted risk assessment with ID: {Id}", id);
            }
            else
            {
                _logger.LogError("Failed to delete risk assessment. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting risk assessment with ID: {Id}", id);
            return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.DeleteFailed);
        }
    }

    #endregion

    #region Step-Specific Operations - NEW for Steps 1-5 Support

    /// <summary>
    /// Saves Step 1 - System Description data using command pattern
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
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Saving Step 1 for RiskAssessment: {Id}", riskAssessmentId);

            var command = new SaveStep1Command(
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
                fiveMOperationalEnvironment);

            var result = await _mediator.SendAsync(command, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully saved Step 1 for RiskAssessment: {Id}", riskAssessmentId);
            }
            else
            {
                _logger.LogError("Failed to save Step 1 for RiskAssessment: {Id}. Error: {Error}", 
                    riskAssessmentId, result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error saving Step 1 for RiskAssessment: {Id}", riskAssessmentId);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }

    /// <summary>
    /// Saves Step 3 - Risk Analysis data using command pattern
    /// </summary>
    public async Task<Result<RiskAssessment>> SaveStep3Async(
        string riskAssessmentId,
        string riskAnalysisMethod,
        string riskCriteria,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Saving Step 3 for RiskAssessment: {Id}", riskAssessmentId);

            var command = new SaveStep3Command(
                riskAssessmentId,
                riskAnalysisMethod,
                riskCriteria);

            var result = await _mediator.SendAsync(command, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully saved Step 3 for RiskAssessment: {Id}", riskAssessmentId);
            }
            else
            {
                _logger.LogError("Failed to save Step 3 for RiskAssessment: {Id}. Error: {Error}", 
                    riskAssessmentId, result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error saving Step 3 for RiskAssessment: {Id}", riskAssessmentId);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }

    /// <summary>
    /// Saves Step 4 - Risk Assessment data using command pattern
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
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Saving Step 4 for RiskAssessment: {Id}", riskAssessmentId);

            var command = new SaveStep4Command(
                riskAssessmentId,
                tolerabilityFramework,
                riskAcceptanceCriteria,
                finalSeverityScore,
                finalLikelihoodScore,
                finalRiskLevel,
                riskTolerability,
                assessmentRationale);

            var result = await _mediator.SendAsync(command, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully saved Step 4 for RiskAssessment: {Id}", riskAssessmentId);
            }
            else
            {
                _logger.LogError("Failed to save Step 4 for RiskAssessment: {Id}. Error: {Error}", 
                    riskAssessmentId, result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error saving Step 4 for RiskAssessment: {Id}", riskAssessmentId);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }

    /// <summary>
    /// Saves Step 5 - Implementation data using command pattern
    /// </summary>
    public async Task<Result<RiskAssessment>> SaveStep5Async(
        string riskAssessmentId,
        string implementationStrategy,
        DateTime? overallTargetDate,
        string implementationNotes,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Saving Step 5 for RiskAssessment: {Id}", riskAssessmentId);

            var command = new SaveStep5Command(
                riskAssessmentId,
                implementationStrategy,
                overallTargetDate,
                implementationNotes);

            var result = await _mediator.SendAsync(command, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully saved Step 5 for RiskAssessment: {Id}", riskAssessmentId);
            }
            else
            {
                _logger.LogError("Failed to save Step 5 for RiskAssessment: {Id}. Error: {Error}", 
                    riskAssessmentId, result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error saving Step 5 for RiskAssessment: {Id}", riskAssessmentId);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }

    /// <summary>
    /// Updates progress tracking data using command pattern
    /// </summary>
    public async Task<Result<RiskAssessment>> UpdateProgressAsync(
        string riskAssessmentId,
        int currentStep,
        string completedSteps,
        int completionPercentage,
        string status = null,
        string stage = null,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Updating progress for RiskAssessment: {Id}, Step: {Step}, Completion: {Percentage}%", 
                riskAssessmentId, currentStep, completionPercentage);

            var command = new UpdateProgressCommand(
                riskAssessmentId,
                currentStep,
                completedSteps,
                completionPercentage,
                status,
                stage);

            var result = await _mediator.SendAsync(command, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated progress for RiskAssessment: {Id}", riskAssessmentId);
            }
            else
            {
                _logger.LogError("Failed to update progress for RiskAssessment: {Id}. Error: {Error}", 
                    riskAssessmentId, result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating progress for RiskAssessment: {Id}", riskAssessmentId);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }

    #endregion

    #region Validation and Business Logic - NEW for Steps 1-5 Support

    /// <summary>
    /// Validates if a step can be saved
    /// </summary>
    public async Task<Result<bool>> ValidateStepCanBeSavedAsync(
        string riskAssessmentId, 
        int stepNumber, 
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Validating step save for RiskAssessment: {Id}, Step: {Step}", riskAssessmentId, stepNumber);
            return await _dataService.ValidateStepCanBeSavedAsync(riskAssessmentId, stepNumber, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error validating step save for RiskAssessment: {Id}, Step: {Step}", riskAssessmentId, stepNumber);
            return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }

    /// <summary>
    /// Gets step completion status for an assessment
    /// </summary>
    public async Task<Result<Dictionary<int, bool>>> GetStepCompletionStatusAsync(
        string riskAssessmentId, 
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting step completion status for RiskAssessment: {Id}", riskAssessmentId);
            return await _dataService.GetStepCompletionStatusAsync(riskAssessmentId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error getting step completion status for RiskAssessment: {Id}", riskAssessmentId);
            return Result<Dictionary<int, bool>>.Failure<Dictionary<int, bool>>(DomainErrors.RiskAssessmentError.NotFound);
        }
    }

    #endregion
}