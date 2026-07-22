//-----------------------------------------------------------------------
// <copyright file="HazardFileService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS Hazard management service handling hazard identification and lifecycle.
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;

namespace SMS_Application.Services;

/// <summary>
/// HazardFile Service - follows the exact same pattern as HazardService
/// Provides high-level business operations for hazard file management
/// </summary>
public sealed class HazardFileService : IHazardFileService
{
    private readonly HazardFileDataService _dataService;
    private readonly ILogger<HazardFileService> _logger;

    public HazardFileService(
        HazardFileDataService dataService,
        ILogger<HazardFileService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Core CRUD Operations

    public async Task<Result<HazardFile>> CreateHazardFileAsync(HazardFile hazardFile, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Creating hazard file with code: {Code}", hazardFile?.Code);
            var result = await _dataService.CreateHazardFileAsync(hazardFile, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully created hazard file with Code: {Code}", result.Value?.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to create hazard file. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error creating hazard file");
            return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.CreateFailed);
        }
    }

    public async Task<Result<HazardFile>> UpdateHazardFileAsync(HazardFile hazardFile, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Updating hazard file with Code: {Code}", hazardFile?.Code);
            var result = await _dataService.UpdateHazardFileAsync(hazardFile, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully updated hazard file with Code: {Code}", hazardFile?.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to update hazard file. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error updating hazard file with Code: {Code}", hazardFile?.Code);
            return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeactivateHazardFileAsync(string code, string reason, string deactivatedBy, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Deactivating hazard file with Code: {Code}", code);
            var result = await _dataService.DeactivateHazardFileAsync(code, reason, deactivatedBy, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully deactivated hazard file with Code: {Code}", code);
            }
            else
            {
                _logger.LogApplicationError("Failed to deactivate hazard file. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error deactivating hazard file with Code: {Code}", code);
            return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.DeleteFailed);
        }
    }

    public async Task<Result<bool>> ReactivateHazardFileAsync(string code, string reactivatedBy, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Reactivating hazard file with Code: {Code}", code);
            var result = await _dataService.ReactivateHazardFileAsync(code, reactivatedBy, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully reactivated hazard file with Code: {Code}", code);
            }
            else
            {
                _logger.LogApplicationError("Failed to reactivate hazard file. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error reactivating hazard file with Code: {Code}", code);
            return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> SetFileConfidentialityAsync(string fileCode, bool isConfidential, string updatedBy, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Setting confidentiality for hazard file with Code: {FileCode} to {IsConfidential}", fileCode, isConfidential);
            var result = await _dataService.SetFileConfidentialityAsync(fileCode, isConfidential, updatedBy, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully set confidentiality for hazard file with Code: {FileCode}", fileCode);
            }
            else
            {
                _logger.LogApplicationError("Failed to set confidentiality for hazard file. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error setting confidentiality for hazard file with Code: {FileCode}", fileCode);
            return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.UpdateFailed);
        }
    }

    #endregion

    #region Query Operations

    public async Task<Result<HazardFile>> GetHazardFileByCodeAsync(string code, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving hazard file with Code: {Code}", code);
            return await _dataService.GetHazardFileByCodeAsync(code, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving hazard file with Code: {Code}", code);
            return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.NotFound);
        }
    }

    public async Task<Result<HazardFile>> GetHazardFileDataAsync(string code, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving hazard file data for Code: {Code}", code);
            return await _dataService.GetHazardFileDataAsync(code, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving hazard file data for Code: {Code}", code);
            return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.NotFound);
        }
    }

    public async Task<Result<IEnumerable<HazardFile>>> GetHazardFilesByHazardCodeAsync(string hazardCode, bool includeFileData = false, string? category = null, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving hazard files for hazard code: {HazardCode}", hazardCode);
            return await _dataService.GetHazardFilesByHazardCodeAsync(hazardCode, includeFileData, category, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving hazard files for hazard code: {HazardCode}", hazardCode);
            return Result<IEnumerable<HazardFile>>.Failure<IEnumerable<HazardFile>>(DomainErrors.HazardFileError.NotFound);
        }
    }

    public async Task<Result<IEnumerable<HazardFile>>> GetHazardFilesByReportCodeAsync(string reportCode, bool includeFileData = false, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving hazard files for report code: {ReportCode}", reportCode);
            return await _dataService.GetHazardFilesByReportCodeAsync(reportCode, includeFileData, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving hazard files for report code: {ReportCode}", reportCode);
            return Result<IEnumerable<HazardFile>>.Failure<IEnumerable<HazardFile>>(DomainErrors.HazardFileError.NotFound);
        }
    }

    public async Task<Result<IEnumerable<HazardFile>>> GetActiveHazardFilesAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving all active hazard files");
            return await _dataService.GetActiveHazardFilesAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving active hazard files");
            return Result<IEnumerable<HazardFile>>.Failure<IEnumerable<HazardFile>>(DomainErrors.HazardFileError.NotFound);
        }
    }

    public async Task<Result<IEnumerable<HazardFile>>> SearchHazardFilesAsync(
        string? hazardCode = null,
        string? reportCode = null,
        string? fileType = null,
        string? category = null,
        string? searchText = null,
        string? uploadedBy = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        bool includeConfidential = false,
        int maxResults = 100,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Searching hazard files with criteria");
            return await _dataService.SearchHazardFilesAsync(hazardCode, reportCode, fileType, category, searchText, uploadedBy, dateFrom, dateTo, includeConfidential, maxResults, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error searching hazard files");
            return Result<IEnumerable<HazardFile>>.Failure<IEnumerable<HazardFile>>(DomainErrors.HazardFileError.NotFound);
        }
    }

    #endregion

    #region File Type Specific Queries

    public async Task<Result<IEnumerable<HazardFile>>> GetHazardPhotosAsync(string hazardCode, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving hazard photos for hazard code: {HazardCode}", hazardCode);
            return await _dataService.GetHazardPhotosAsync(hazardCode, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving hazard photos for hazard code: {HazardCode}", hazardCode);
            return Result<IEnumerable<HazardFile>>.Failure<IEnumerable<HazardFile>>(DomainErrors.HazardFileError.NotFound);
        }
    }

    public async Task<Result<IEnumerable<HazardFile>>> GetHazardDocumentsAsync(string hazardCode, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving hazard documents for hazard code: {HazardCode}", hazardCode);
            return await _dataService.GetHazardDocumentsAsync(hazardCode, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving hazard documents for hazard code: {HazardCode}", hazardCode);
            return Result<IEnumerable<HazardFile>>.Failure<IEnumerable<HazardFile>>(DomainErrors.HazardFileError.NotFound);
        }
    }

    public async Task<Result<IEnumerable<HazardFile>>> GetHazardVideosAsync(string hazardCode, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving hazard videos for hazard code: {HazardCode}", hazardCode);
            return await _dataService.GetHazardVideosAsync(hazardCode, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving hazard videos for hazard code: {HazardCode}", hazardCode);
            return Result<IEnumerable<HazardFile>>.Failure<IEnumerable<HazardFile>>(DomainErrors.HazardFileError.NotFound);
        }
    }

    public async Task<Result<IEnumerable<HazardFile>>> GetConfidentialHazardFilesAsync(string hazardCode, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving confidential hazard files for hazard code: {HazardCode}", hazardCode);
            return await _dataService.GetConfidentialHazardFilesAsync(hazardCode, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving confidential hazard files for hazard code: {HazardCode}", hazardCode);
            return Result<IEnumerable<HazardFile>>.Failure<IEnumerable<HazardFile>>(DomainErrors.HazardFileError.NotFound);
        }
    }

    #endregion
}

