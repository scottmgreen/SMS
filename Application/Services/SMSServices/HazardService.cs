//-----------------------------------------------------------------------
// <copyright file="HazardService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS Hazard management service handling hazard identification and lifecycle.
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;
using SMS_Domain.Entities;

namespace SMS_Application.Services;

/// <summary>
/// Enhanced Hazard Service with comprehensive file management capabilities and complex entity population
/// </summary>
public sealed class HazardService : IHazardService
{
    private readonly HazardDataService _dataService;
    private readonly HazardLocationService _hazardLocationService;
    private readonly ILogger<HazardService> _logger;

    public HazardService(
        HazardDataService dataService,
        HazardLocationService hazardLocationService,
        ILogger<HazardService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _hazardLocationService = hazardLocationService ?? throw new ArgumentNullException(nameof(hazardLocationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region IHazardService Implementation

    public async Task<Result<Hazard>> CreateHazardAsync(Hazard hazard, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Creating hazard with code: {Code}", hazard?.Code);
            var result = await _dataService.CreateHazardAsync(hazard, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully created hazard with ID: {Id}", result.Value?.Id);
            }
            else
            {
                _logger.LogApplicationError("Failed to create hazard. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error creating hazard");
            return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.CreateFailed);
        }
    }

    public async Task<Result<Hazard>> GetHazardByIdAsync(HazardID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving hazard with ID: {Id}", id);
            return await _dataService.GetHazardByCodeAsync(id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving hazard with ID: {Id}", id);
            return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.NotFound);
        }
    }

    public async Task<Result<Hazard>> GetHazardByCodeAsync(string code, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving hazard with Code: {Code}", code);
            return await _dataService.GetHazardByCodeAsync(new HazardID(code), ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving hazard with Code: {Code}", code);
            return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.NotFound);
        }
    }

    public async Task<Result<List<Hazard>>> GetAllHazardsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving all hazards");
            return await _dataService.GetAllHazardsAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving all hazards");
            return Result<List<Hazard>>.Failure<List<Hazard>>(DomainErrors.HazardError.NullOrEmpty);
        }
    }

    public async Task<Result<Hazard>> UpdateHazardAsync(Hazard hazard, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Updating hazard with ID: {Id}", hazard?.Id);
            var result = await _dataService.UpdateHazardAsync(hazard, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully updated hazard with ID: {Id}", hazard?.Id);
            }
            else
            {
                _logger.LogApplicationError("Failed to update hazard. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error updating hazard with ID: {Id}", hazard?.Id);
            return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteHazardAsync(HazardID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Deleting hazard with ID: {Id}", id);
            var result = await _dataService.DeleteHazardAsync(id, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully deleted hazard with ID: {Id}", id);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete hazard. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error deleting hazard with ID: {Id}", id);
            return Result<bool>.Failure<bool>(DomainErrors.HazardError.DeleteFailed);
        }
    }

    public async Task<Result<List<Hazard>>> GetHazardsByReportCodeAsync(string reportCode, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving hazards for report code: {ReportCode}", reportCode);
            return await _dataService.GetHazardsByReportCodeAsync(new ReportID(reportCode), ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving hazards for report code: {ReportCode}", reportCode);
            return Result<List<Hazard>>.Failure<List<Hazard>>(DomainErrors.HazardError.NullOrEmpty);
        }
    }

    #endregion

    #region Extended Service Methods

    public async Task<Result<Hazard>> GetHazardByCodeAsync(HazardID code, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving hazard with Code: {Id}", code);
            return await _dataService.GetHazardByCodeAsync(code, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving hazard with Code: {Id}", code);
            return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.NotFound);
        }
    }

    /// <summary>
    /// Gets hazard with complete HazardLocation entity populated
    /// This method demonstrates proper complex entity population using Application Services
    /// </summary>
    public async Task<Result<Hazard>> GetHazardWithLocationByIdAsync(HazardID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving hazard with location data for ID: {Id}", id);

            // First get the basic hazard
            var hazardResult = await _dataService.GetHazardByCodeAsync(id, ct).ConfigureAwait(false);
            if (hazardResult.IsFailure)
            {
                return hazardResult;
            }

            var hazard = hazardResult.Value;

            // Then populate the complex HazardLocation entity
            await PopulateHazardLocationAsync(hazard, ct);

            _logger.LogApplicationInformation("Successfully retrieved hazard with location data for ID: {Id}", id);
            return Result<Hazard>.Success(hazard);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving hazard with location for ID: {Id}", id);
            return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.NotFound);
        }
    }

    public async Task<Result<List<Hazard>>> GetHazardsByReportCodeAsync(ReportID reportId, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving hazards for report ID: {ReportId}", reportId);
            return await _dataService.GetHazardsByReportCodeAsync(reportId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving hazards for report ID: {ReportId}", reportId);
            return Result<List<Hazard>>.Failure<List<Hazard>>(DomainErrors.HazardError.NullOrEmpty);
        }
    }

    /// <summary>
    /// Gets all hazards with complete HazardLocation entities populated
    /// This method shows how to efficiently populate complex entities for collections
    /// </summary>
    public async Task<Result<List<Hazard>>> GetAllHazardsWithLocationAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving all hazards with location data");

            // First get all basic hazards
            var hazardsResult = await _dataService.GetAllHazardsAsync(ct).ConfigureAwait(false);
            if (hazardsResult.IsFailure)
            {
                return hazardsResult;
            }

            var hazards = hazardsResult.Value;

            // Then populate HazardLocation for each hazard
            var tasks = hazards.Select(hazard => PopulateHazardLocationAsync(hazard, ct));
            await Task.WhenAll(tasks);

            _logger.LogApplicationInformation("Successfully retrieved {Count} hazards with location data", hazards.Count);
            return Result<List<Hazard>>.Success(hazards);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving all hazards with location");
            return Result<List<Hazard>>.Failure<List<Hazard>>(DomainErrors.HazardError.NullOrEmpty);
        }
    }

    #endregion

    #region Complex Entity Population Methods

    /// <summary>
    /// Populates the HazardLocation entity for a given Hazard using Application Services
    /// This demonstrates the proper pattern for complex entity population
    /// </summary>
    private async Task PopulateHazardLocationAsync(Hazard hazard, CancellationToken ct = default)
    {
        if (hazard == null || string.IsNullOrEmpty(hazard.Code))
        {
            return;
        }

        try
        {
            // Get HazardLocation(s) for this Hazard via Application Service
            var locationsResult = await _hazardLocationService.GetHazardLocationsByHazardCodeAsync(hazard.Code, ct);

            if (locationsResult.IsSuccess && locationsResult.Value.Any())
            {
                // Get the primary/most recent location
                var primaryLocation = locationsResult.Value
                    .OrderByDescending(l => l.CreatedDate)
                    .First();

                // Populate the HazardLocation property
                hazard.HazardLocation = primaryLocation;

                _logger.LogApplicationDebug("Populated HazardLocation for hazard {HazardCode}", hazard.Code);
            }
            else
            {
                _logger.LogApplicationDebug("No HazardLocation found for hazard {HazardCode}", hazard.Code);
            }
        }
        catch (Exception ex)
        {
            _logger.LogApplicationWarning(ex, "Failed to populate HazardLocation for hazard {HazardCode}", hazard.Code);
            // Don't throw - continue without location data
        }
    }

    #endregion
}

