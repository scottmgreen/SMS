using Microsoft.Extensions.Logging;

namespace SMS_Application.Services;

public sealed class RiskAnalysisService
{
    private readonly RiskAnalysisDataService _dataService;
    private readonly ILogger<RiskAnalysisService> _logger;

    public RiskAnalysisService(RiskAnalysisDataService dataService, ILogger<RiskAnalysisService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RiskAnalysis>> CreateRiskAnalysisAsync(RiskAnalysis riskAnalysis, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Creating risk analysis with code: {Code}", riskAnalysis?.Code);
            var result = await _dataService.CreateRiskAnalysisAsync(riskAnalysis, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created risk analysis with ID: {Id}", result.Value?.Id);
            }
            else
            {
                _logger.LogError("Failed to create risk analysis. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating risk analysis");
            return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.RiskAnalysisError.CreateFailed);
        }
    }

    public async Task<Result<RiskAnalysis>> GetRiskAnalysisByIdAsync(RiskAnalysisID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving risk analysis with ID: {Id}", id);
            return await _dataService.GetRiskAnalysisByIdAsync(id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving risk analysis with ID: {Id}", id);
            return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.RiskAnalysisError.NotFound);
        }
    }
    public async Task<Result<RiskAnalysis>> GetRiskAnalysisByHazardIdAsync(HazardID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving risk analysis with ID: {Id}", id);
            return await _dataService.GetRiskAnalysisByHazardIdAsync(id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving risk analysis with ID: {Id}", id);
            return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.RiskAnalysisError.NotFound);
        }
    }
    public async Task<Result<List<RiskAnalysis>>> GetAllRiskAnalysisAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving all risk analysis");
            return await _dataService.GetAllRiskAnalysisAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving all risk analysis");
            return Result<List<RiskAnalysis>>.Failure<List<RiskAnalysis>>(DomainErrors.RiskAnalysisError.NullOrEmpty);
        }
    }

    public async Task<Result<RiskAnalysis>> UpdateRiskAnalysisAsync(RiskAnalysis riskAnalysis, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Updating risk analysis with ID: {Id}", riskAnalysis?.Id);
            var result = await _dataService.UpdateRiskAnalysisAsync(riskAnalysis, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated risk analysis with ID: {Id}", riskAnalysis?.Id);
            }
            else
            {
                _logger.LogError("Failed to update risk analysis. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating risk analysis with ID: {Id}", riskAnalysis?.Id);
            return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.RiskAnalysisError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteRiskAnalysisAsync(RiskAnalysisID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Deleting risk analysis with ID: {Id}", id);
            var result = await _dataService.DeleteRiskAnalysisAsync(id, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted risk analysis with ID: {Id}", id);
            }
            else
            {
                _logger.LogError("Failed to delete risk analysis. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting risk analysis with ID: {Id}", id);
            return Result<bool>.Failure<bool>(DomainErrors.RiskAnalysisError.DeleteFailed);
        }
    }
}