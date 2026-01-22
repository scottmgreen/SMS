using Microsoft.Extensions.Logging;

namespace SMS_Application.Services;

/// <summary>
/// HazardFile Service - follows the exact same pattern as HazardService
/// Provides high-level business operations for hazard file management
/// </summary>
public sealed class HazardFileService : IHazardFileService
{
    private readonly HazardFileDataService _dataService;
    private readonly ILogger<HazardFileService> _logger;

    public HazardFileService(HazardFileDataService dataService, ILogger<HazardFileService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<HazardFile>> CreateHazardFileAsync(HazardFile hazardFile, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Creating hazard file with code: {Code}", hazardFile?.Code);
            var result = await _dataService.CreateHazardFileAsync(hazardFile, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created hazard file with Code: {Code}", result.Value?.Code);
            }
            else
            {
                _logger.LogError("Failed to create hazard file. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating hazard file");
            return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.CreateFailed);
        }
    }

    public async Task<Result<HazardFile>> GetHazardFileByCodeAsync(string code, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving hazard file with Code: {Code}", code);
            return await _dataService.GetHazardFileByCodeAsync(code, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving hazard file with Code: {Code}", code);
            return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.NotFound);
        }
    }

    public async Task<Result<IEnumerable<HazardFile>>> GetHazardFilesByHazardCodeAsync(string hazardCode, bool includeFileData = false, string? category = null, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving hazard files for hazard code: {HazardCode}", hazardCode);
            return await _dataService.GetHazardFilesByHazardCodeAsync(hazardCode, includeFileData, category, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving hazard files for hazard code: {HazardCode}", hazardCode);
            return Result<IEnumerable<HazardFile>>.Failure<IEnumerable<HazardFile>>(DomainErrors.HazardFileError.NotFound);
        }
    }

    public async Task<Result<HazardFile>> UpdateHazardFileAsync(HazardFile hazardFile, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Updating hazard file with Code: {Code}", hazardFile?.Code);
            var result = await _dataService.UpdateHazardFileAsync(hazardFile, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated hazard file with Code: {Code}", hazardFile?.Code);
            }
            else
            {
                _logger.LogError("Failed to update hazard file. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating hazard file with Code: {Code}", hazardFile?.Code);
            return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeactivateHazardFileAsync(int id, string reason, string deactivatedBy, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Deactivating hazard file with ID: {Id}", id);
            var result = await _dataService.DeactivateHazardFileAsync(id, reason, deactivatedBy, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deactivated hazard file with ID: {Id}", id);
            }
            else
            {
                _logger.LogError("Failed to deactivate hazard file. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deactivating hazard file with ID: {Id}", id);
            return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.DeleteFailed);
        }
    }

    public async Task<Result<HazardFile>> GetHazardFileDataAsync(string code, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving hazard file data for Code: {Code}", code);
            return await _dataService.GetHazardFileDataAsync(code, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving hazard file data for Code: {Code}", code);
            return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.NotFound);
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
            _logger.LogInformation("Searching hazard files with criteria");
            return await _dataService.SearchHazardFilesAsync(hazardCode, reportCode, fileType, category, searchText, uploadedBy, dateFrom, dateTo, includeConfidential, maxResults, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error searching hazard files");
            return Result<IEnumerable<HazardFile>>.Failure<IEnumerable<HazardFile>>(DomainErrors.HazardFileError.NotFound);
        }
    }


}