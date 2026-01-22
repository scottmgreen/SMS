using Microsoft.Extensions.Logging;

namespace SMS_Application.Services;

/// <summary>
/// HazardLocation Service following the exact SMS pattern
/// </summary>
public sealed class HazardLocationService
{
    private readonly HazardLocationDataService _dataService;
    private readonly ILogger<HazardLocationService> _logger;

    public HazardLocationService(
        HazardLocationDataService dataService,
        ILogger<HazardLocationService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region HazardLocation CRUD Operations

    public async Task<Result<HazardLocation>> CreateHazardLocationAsync(HazardLocation hazardLocation, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Creating hazard location with code: {Code}", hazardLocation?.Code);
            var result = await _dataService.CreateHazardLocationAsync(hazardLocation, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created hazard location with ID: {Id}", result.Value?.Id);
            }
            else
            {
                _logger.LogError("Failed to create hazard location. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating hazard location");
            return Result<HazardLocation>.Failure<HazardLocation>(DomainErrors.HazardLocationError.CreateFailed);
        }
    }

    public async Task<Result<HazardLocation>> GetHazardLocationByCodeAsync(HazardLocationID code, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving hazard location with Code: {Code}", code);
            return await _dataService.GetHazardLocationByCodeAsync(code, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving hazard location with Code:{Code}", code);
            return Result<HazardLocation>.Failure<HazardLocation>(DomainErrors.HazardLocationError.NotFound);
        }
    }

    public async Task<Result<List<HazardLocation>>> GetAllHazardLocationsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving all hazard locations");
            return await _dataService.GetAllHazardLocationsAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving all hazard locations");
            return Result<List<HazardLocation>>.Failure<List<HazardLocation>>(DomainErrors.HazardLocationError.NullOrEmpty);
        }
    }

    public async Task<Result<List<HazardLocation>>> GetHazardLocationsByHazardCodeAsync(string hazardCode, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving hazard locations for hazard code: {HazardCode}", hazardCode);
            return await _dataService.GetHazardLocationsByHazardCodeAsync(hazardCode, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving hazard locations for hazard code: {HazardCode}", hazardCode);
            return Result<List<HazardLocation>>.Failure<List<HazardLocation>>(DomainErrors.HazardLocationError.NullOrEmpty);
        }
    }

    public async Task<Result<HazardLocation>> UpdateHazardLocationAsync(HazardLocation hazardLocation, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Updating hazard location with ID: {Id}", hazardLocation?.Id);
            var result = await _dataService.UpdateHazardLocationAsync(hazardLocation, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated hazard location with ID: {Id}", hazardLocation?.Id);
            }
            else
            {
                _logger.LogError("Failed to update hazard location. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating hazard location with ID: {Id}", hazardLocation?.Id);
            return Result<HazardLocation>.Failure<HazardLocation>(DomainErrors.HazardLocationError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteHazardLocationAsync(HazardLocationID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Deleting hazard location with ID: {Id}", id);
            var result = await _dataService.DeleteHazardLocationAsync(id, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted hazard location with ID: {Id}", id);
            }
            else
            {
                _logger.LogError("Failed to delete hazard location. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting hazard location with ID: {Id}", id);
            return Result<bool>.Failure<bool>(DomainErrors.HazardLocationError.DeleteFailed);
        }
    }

    #endregion
}