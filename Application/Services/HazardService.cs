using Microsoft.Extensions.Logging;
using SMS_Infrastructure.Services;
using SMS_Infrastructure.Interfaces;
using SMS_Domain.Entities;
using SMS_Domain.Models;
using SMS_Domain.Errors;
using SMS_Domain.Enums;
using SMS_Shared.Common;

namespace SMS_Application.Services;

/// <summary>
/// Enhanced Hazard Service with comprehensive file management capabilities
/// </summary>
public sealed class HazardService
{
    private readonly HazardDataService _dataService;
    private readonly ILogger<HazardService> _logger;

    public HazardService(
        HazardDataService dataService, 
        //IHazardFileRepository hazardFileRepository,
        ILogger<HazardService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Hazard CRUD Operations

    public async Task<Result<Hazard>> CreateHazardAsync(Hazard hazard, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Creating hazard with code: {Code}", hazard?.Code);
            var result = await _dataService.CreateHazardAsync(hazard, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created hazard with ID: {Id}", result.Value?.Id);
            }
            else
            {
                _logger.LogError("Failed to create hazard. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating hazard");
            return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.CreateFailed);
        }
    }

    public async Task<Result<Hazard>> GetHazardByIdAsync(HazardID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving hazard with ID: {Id}", id);
            return await _dataService.GetHazardByIdAsync(id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving hazard with ID: {Id}", id);
            return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.NotFound);
        }
    }

    public async Task<Result<List<Hazard>>> GetAllHazardsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving all hazards");
            return await _dataService.GetAllHazardsAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving all hazards");
            return Result<List<Hazard>>.Failure<List<Hazard>>(DomainErrors.HazardError.NullOrEmpty);
        }
    }

    public async Task<Result<Hazard>> UpdateHazardAsync(Hazard hazard, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Updating hazard with ID: {Id}", hazard?.Id);
            var result = await _dataService.UpdateHazardAsync(hazard, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated hazard with ID: {Id}", hazard?.Id);
            }
            else
            {
                _logger.LogError("Failed to update hazard. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating hazard with ID: {Id}", hazard?.Id);
            return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteHazardAsync(HazardID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Deleting hazard with ID: {Id}", id);
            var result = await _dataService.DeleteHazardAsync(id, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted hazard with ID: {Id}", id);
            }
            else
            {
                _logger.LogError("Failed to delete hazard. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting hazard with ID: {Id}", id);
            return Result<bool>.Failure<bool>(DomainErrors.HazardError.DeleteFailed);
        }
    }

    #endregion

      


}