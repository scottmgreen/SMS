using Microsoft.Extensions.Logging;

namespace SMS_Application.Services;

public sealed class ScoringPanelService
{
    private readonly ScoringPanelDataService _dataService;
    private readonly ILogger<ScoringPanelService> _logger;

    public ScoringPanelService(ScoringPanelDataService dataService, ILogger<ScoringPanelService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ScoringPanel>> CreateScoringPanelAsync(ScoringPanel scoringPanel, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Creating scoring panel with code: {Code}", scoringPanel?.Code);
            var result = await _dataService.CreateScoringPanelAsync(scoringPanel, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created scoring panel with ID: {Id}", result.Value?.Id);
            }
            else
            {
                _logger.LogError("Failed to create scoring panel. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating scoring panel");
            return Result<ScoringPanel>.Failure<ScoringPanel>(DomainErrors.ScoringPanelError.CreateFailed);
        }
    }

    public async Task<Result<ScoringPanel>> GetScoringPanelByIdAsync(ScoringPanelID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving scoring panel with ID: {Id}", id);
            return await _dataService.GetScoringPanelByIdAsync(id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving scoring panel with ID: {Id}", id);
            return Result<ScoringPanel>.Failure<ScoringPanel>(DomainErrors.ScoringPanelError.NotFound);
        }
    }

    public async Task<Result<List<ScoringPanel>>> GetAllScoringPanelsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving all scoring panels");
            return await _dataService.GetAllScoringPanelsAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving all scoring panels");
            return Result<List<ScoringPanel>>.Failure<List<ScoringPanel>>(DomainErrors.ScoringPanelError.NullOrEmpty);
        }
    }

    public async Task<Result<List<ScoringPanel>>> GetScoringPanelsByHazardCodeAsync(string hazardCode, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving scoring panels for hazard code: {HazardCode}", hazardCode);
            var result = await _dataService.GetScoringPanelsByHazardCodeAsync(hazardCode, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} scoring panels for hazard code: {HazardCode}",
                    result.Value?.Count ?? 0, hazardCode);
            }
            else
            {
                _logger.LogWarning("No scoring panels found for hazard code: {HazardCode}. Error: {Error}",
                    hazardCode, result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving scoring panels for hazard code: {HazardCode}", hazardCode);
            return Result<List<ScoringPanel>>.Failure<List<ScoringPanel>>(DomainErrors.ScoringPanelError.NullOrEmpty);
        }
    }

    public async Task<Result<ScoringPanel>> UpdateScoringPanelAsync(ScoringPanel scoringPanel, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Updating scoring panel with ID: {Id}", scoringPanel?.Id);
            var result = await _dataService.UpdateScoringPanelAsync(scoringPanel, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated scoring panel with ID: {Id}", scoringPanel?.Id);
            }
            else
            {
                _logger.LogError("Failed to update scoring panel. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating scoring panel with ID: {Id}", scoringPanel?.Id);
            return Result<ScoringPanel>.Failure<ScoringPanel>(DomainErrors.ScoringPanelError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteScoringPanelAsync(ScoringPanelID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Deleting scoring panel with ID: {Id}", id);
            var result = await _dataService.DeleteScoringPanelAsync(id, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted scoring panel with ID: {Id}", id);
            }
            else
            {
                _logger.LogError("Failed to delete scoring panel. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting scoring panel with ID: {Id}", id);
            return Result<bool>.Failure<bool>(DomainErrors.ScoringPanelError.DeleteFailed);
        }
    }
}