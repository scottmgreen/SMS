using Microsoft.Extensions.Logging;

namespace SMS_Application.Services;

public sealed class MitigationService
{
    private readonly MitigationDataService _dataService;
    private readonly ILogger<MitigationService> _logger;

    public MitigationService(MitigationDataService dataService, ILogger<MitigationService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Mitigation>> CreateMitigationAsync(Mitigation mitigation, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Creating mitigation with code: {Code}", mitigation?.Code);
            var result = await _dataService.CreateMitigationAsync(mitigation, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created mitigation with ID: {Id}", result.Value?.Id);
            }
            else
            {
                _logger.LogError("Failed to create mitigation. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating mitigation");
            return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.CreateFailed);
        }
    }

    public async Task<Result<Mitigation>> GetMitigationByCodeAsync(MitigationID code, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving mitigation with Code: {Code}", code);
            return await _dataService.GetMitigationByCodeAsync(code, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving mitigation with Code: {Code}", code);
            return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.NotFound);
        }
    }

    public async Task<Result<List<Mitigation>>> GetAllMitigationsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving all mitigations");
            return await _dataService.GetAllMitigationsAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving all mitigations");
            return Result<List<Mitigation>>.Failure<List<Mitigation>>(DomainErrors.MitigationError.NullOrEmpty);
        }
    }

    public async Task<Result<Mitigation>> UpdateMitigationAsync(Mitigation mitigation, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Updating mitigation with ID: {Id}", mitigation?.Id);
            var result = await _dataService.UpdateMitigationAsync(mitigation, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated mitigation with ID: {Id}", mitigation?.Id);
            }
            else
            {
                _logger.LogError("Failed to update mitigation. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating mitigation with ID: {Id}", mitigation?.Id);
            return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteMitigationAsync(MitigationID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Deleting mitigation with ID: {Id}", id);
            var result = await _dataService.DeleteMitigationAsync(id, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted mitigation with ID: {Id}", id);
            }
            else
            {
                _logger.LogError("Failed to delete mitigation. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting mitigation with ID: {Id}", id);
            return Result<bool>.Failure<bool>(DomainErrors.MitigationError.DeleteFailed);
        }
    }

    /// <summary>
    /// Gets all mitigations for a specific hazard code - Used for populating Hazard.Mitigations list
    /// This maintains proper architectural separation between hazards and mitigations
    /// </summary>
    public async Task<Result<List<Mitigation>>> GetMitigationsByHazardCodeAsync(string hazardCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(hazardCode))
            {
                _logger.LogWarning("GetMitigationsByHazardCodeAsync called with null or empty hazard code");
                return Result<List<Mitigation>>.Failure<List<Mitigation>>(DomainErrors.MitigationError.NullOrEmpty);
            }

            _logger.LogInformation("Retrieving mitigations for hazard code: {HazardCode}", hazardCode);
            return await _dataService.GetMitigationsByHazardCodeAsync(hazardCode, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving mitigations for hazard code: {HazardCode}", hazardCode);
            return Result<List<Mitigation>>.Failure<List<Mitigation>>(DomainErrors.MitigationError.NullOrEmpty);
        }
    }
}