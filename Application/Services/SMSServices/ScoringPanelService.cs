//-----------------------------------------------------------------------
// <copyright file="ScoringPanelService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application service providing business logic operations for SMS domain entities.
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Domain.Entities;

using SMS_Domain.Entities;

namespace SMS_Application.Services;

public sealed class ScoringPanelService : IScoringPanelService
{
    private readonly ScoringPanelDataService _dataService;
    private readonly ILogger<ScoringPanelService> _logger;

    public ScoringPanelService(ScoringPanelDataService dataService, ILogger<ScoringPanelService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region IScoringPanelService Implementation

    public async Task<Result<ScoringPanel>> CreateScoringPanelAsync(ScoringPanel panel, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Creating scoring panel with code: {Code}", panel?.Code);
            var result = await _dataService.CreateScoringPanelAsync(panel, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully created scoring panel with ID: {Id}", result.Value?.Id);
            }
            else
            {
                _logger.LogApplicationError("Failed to create scoring panel. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error creating scoring panel");
            return Result<ScoringPanel>.Failure<ScoringPanel>(DomainErrors.ScoringPanelError.CreateFailed);
        }
    }

    public async Task<Result<ScoringPanel>> GetScoringPanelByIdAsync(ScoringPanelID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving scoring panel with ID: {Id}", id);
            return await _dataService.GetScoringPanelByIdAsync(id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving scoring panel with ID: {Id}", id);
            return Result<ScoringPanel>.Failure<ScoringPanel>(DomainErrors.ScoringPanelError.NotFound);
        }
    }

    public async Task<Result<List<ScoringPanel>>> GetScoringPanelsByHazardCodeAsync(string hazardCode, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving scoring panels for hazard code: {HazardCode}", hazardCode);
            var result = await _dataService.GetScoringPanelsByHazardCodeAsync(hazardCode, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully retrieved {Count} scoring panels for hazard code: {HazardCode}",
                    result.Value?.Count ?? 0, hazardCode);
            }
            else
            {
                _logger.LogApplicationWarning("No scoring panels found for hazard code: {HazardCode}. Error: {Error}",
                    hazardCode, result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving scoring panels for hazard code: {HazardCode}", hazardCode);
            return Result<List<ScoringPanel>>.Failure<List<ScoringPanel>>(DomainErrors.ScoringPanelError.NullOrEmpty);
        }
    }

    public async Task<Result<ScoringPanel>> UpdateScoringPanelAsync(ScoringPanel panel, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Updating scoring panel with ID: {Id}", panel?.Id);
            var result = await _dataService.UpdateScoringPanelAsync(panel, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully updated scoring panel with ID: {Id}", panel?.Id);
            }
            else
            {
                _logger.LogApplicationError("Failed to update scoring panel. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error updating scoring panel with ID: {Id}", panel?.Id);
            return Result<ScoringPanel>.Failure<ScoringPanel>(DomainErrors.ScoringPanelError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteScoringPanelAsync(ScoringPanelID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Deleting scoring panel with ID: {Id}", id);
            var result = await _dataService.DeleteScoringPanelAsync(id, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully deleted scoring panel with ID: {Id}", id);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete scoring panel. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error deleting scoring panel with ID: {Id}", id);
            return Result<bool>.Failure<bool>(DomainErrors.ScoringPanelError.DeleteFailed);
        }
    }

    #endregion

    #region Legacy Methods (keeping for backward compatibility)

    public async Task<Result<List<ScoringPanel>>> GetAllScoringPanelsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving all scoring panels");
            return await _dataService.GetAllScoringPanelsAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving all scoring panels");
            return Result<List<ScoringPanel>>.Failure<List<ScoringPanel>>(DomainErrors.ScoringPanelError.NullOrEmpty);
        }
    }

    #endregion
}

