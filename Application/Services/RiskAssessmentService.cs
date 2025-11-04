using Microsoft.Extensions.Logging;

namespace SMS_Application.Services;

public sealed class RiskAssessmentService
{
    private readonly RiskAssessmentDataService _dataService;
    private readonly ILogger<RiskAssessmentService> _logger;

    public RiskAssessmentService(RiskAssessmentDataService dataService, ILogger<RiskAssessmentService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RiskAssessment>> CreateRiskAssessmentAsync(RiskAssessment riskAssessment, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Creating risk assessment with code: {Code}", riskAssessment?.Code);
            var result = await _dataService.CreateRiskAssessmentAsync(riskAssessment, ct).ConfigureAwait(false);

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
            var result = await _dataService.UpdateRiskAssessmentAsync(riskAssessment, ct).ConfigureAwait(false);

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
            var result = await _dataService.DeleteRiskAssessmentAsync(id, ct).ConfigureAwait(false);

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
}