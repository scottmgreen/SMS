//-----------------------------------------------------------------------
// <copyright file="HazardLocationService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS Hazard management service handling hazard identification and lifecycle.
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Domain.Entities;


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
            _logger.LogApplicationInformation("Creating hazard location with code: {Code}", hazardLocation?.Code);
            var result = await _dataService.CreateHazardLocationAsync(hazardLocation, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully created hazard location with ID: {Id}", result.Value?.Id);
            }
            else
            {
                _logger.LogApplicationError("Failed to create hazard location. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error creating hazard location");
            return Result<HazardLocation>.Failure<HazardLocation>(DomainErrors.HazardLocationError.CreateFailed);
        }
    }

    public async Task<Result<HazardLocation>> GetHazardLocationByCodeAsync(HazardLocationID code, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving hazard location with Code: {Code}", code);
            return await _dataService.GetHazardLocationByCodeAsync(code, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving hazard location with Code:{Code}", code);
            return Result<HazardLocation>.Failure<HazardLocation>(DomainErrors.HazardLocationError.NotFound);
        }
    }

    public async Task<Result<List<HazardLocation>>> GetAllHazardLocationsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving all hazard locations");
            return await _dataService.GetAllHazardLocationsAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving all hazard locations");
            return Result<List<HazardLocation>>.Failure<List<HazardLocation>>(DomainErrors.HazardLocationError.NullOrEmpty);
        }
    }

    public async Task<Result<List<HazardLocation>>> GetHazardLocationsByHazardCodeAsync(string hazardCode, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving hazard locations for hazard code: {HazardCode}", hazardCode);
            return await _dataService.GetHazardLocationsByHazardCodeAsync(hazardCode, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving hazard locations for hazard code: {HazardCode}", hazardCode);
            return Result<List<HazardLocation>>.Failure<List<HazardLocation>>(DomainErrors.HazardLocationError.NullOrEmpty);
        }
    }

    public async Task<Result<HazardLocation>> UpdateHazardLocationAsync(HazardLocation hazardLocation, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Updating hazard location with ID: {Id}", hazardLocation?.Id);
            var result = await _dataService.UpdateHazardLocationAsync(hazardLocation, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully updated hazard location with ID: {Id}", hazardLocation?.Id);
            }
            else
            {
                _logger.LogApplicationError("Failed to update hazard location. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error updating hazard location with ID: {Id}", hazardLocation?.Id);
            return Result<HazardLocation>.Failure<HazardLocation>(DomainErrors.HazardLocationError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteHazardLocationAsync(HazardLocationID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Deleting hazard location with ID: {Id}", id);
            var result = await _dataService.DeleteHazardLocationAsync(id, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully deleted hazard location with ID: {Id}", id);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete hazard location. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error deleting hazard location with ID: {Id}", id);
            return Result<bool>.Failure<bool>(DomainErrors.HazardLocationError.DeleteFailed);
        }
    }

    #endregion
}

