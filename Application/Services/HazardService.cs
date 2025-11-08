using Microsoft.Extensions.Logging;

namespace SMS_Application.Services;

/// <summary>
/// Enhanced Hazard Service with comprehensive file management capabilities
/// </summary>
public sealed class HazardService
{
    private readonly HazardDataService _dataService;
    private readonly IHazardFileRepository _hazardFileRepository;
    private readonly ILogger<HazardService> _logger;

    public HazardService(
        HazardDataService dataService, 
        IHazardFileRepository hazardFileRepository,
        ILogger<HazardService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _hazardFileRepository = hazardFileRepository ?? throw new ArgumentNullException(nameof(hazardFileRepository));
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

    #region Hazard File Management Operations

    /// <summary>
    /// Upload file for database storage
    /// </summary>
    public async Task<Result<HazardFile>> UploadFileForDatabaseAsync(
        string hazardCode, 
        string fileName, 
        string fileType, 
        byte[] fileData, 
        string uploadedBy,
        string? reportCode = null,
        string? description = null,
        HazardFileCategory? category = null,
        bool isConfidential = false,
        string? tags = null,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Uploading file {FileName} for hazard {HazardCode} to database", fileName, hazardCode);

            var fileResult = HazardFile.CreateForDatabase(hazardCode, fileName, fileType, fileData, uploadedBy, reportCode);
            if (fileResult.IsFailure)
            {
                return Result<HazardFile>.Failure<HazardFile>(fileResult.Error);
            }

            var file = fileResult.Value;
            
            // Update metadata if provided
            if (description != null || category != null || isConfidential || tags != null)
            {
                var metadataResult = file.UpdateMetadata(description, category, isConfidential, tags);
                if (metadataResult.IsFailure)
                {
                    return Result<HazardFile>.Failure<HazardFile>(metadataResult.Error);
                }
            }

            var result = await _hazardFileRepository.AddAsync(file, ct).ConfigureAwait(false);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully uploaded file {FileName} with code {Code}", fileName, file.Code);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error uploading file {FileName} for hazard {HazardCode}", fileName, hazardCode);
            return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.CreateFailed);
        }
    }

    /// <summary>
    /// Upload file for file system storage
    /// </summary>
    public async Task<Result<HazardFile>> UploadFileForFileSystemAsync(
        string hazardCode, 
        string fileName, 
        string fileType, 
        long fileSizeBytes, 
        string filePath, 
        string uploadedBy,
        string? reportCode = null,
        string? description = null,
        HazardFileCategory? category = null,
        bool isConfidential = false,
        string? tags = null,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Uploading file {FileName} for hazard {HazardCode} to file system", fileName, hazardCode);

            var fileResult = HazardFile.CreateForFileSystem(hazardCode, fileName, fileType, fileSizeBytes, filePath, uploadedBy, reportCode);
            if (fileResult.IsFailure)
            {
                return Result<HazardFile>.Failure<HazardFile>(fileResult.Error);
            }

            var file = fileResult.Value;
            
            // Update metadata if provided
            if (description != null || category != null || isConfidential || tags != null)
            {
                var metadataResult = file.UpdateMetadata(description, category, isConfidential, tags);
                if (metadataResult.IsFailure)
                {
                    return Result<HazardFile>.Failure<HazardFile>(metadataResult.Error);
                }
            }

            var result = await _hazardFileRepository.AddAsync(file, ct).ConfigureAwait(false);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully uploaded file {FileName} with code {Code}", fileName, file.Code);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error uploading file {FileName} for hazard {HazardCode}", fileName, hazardCode);
            return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.CreateFailed);
        }
    }

    /// <summary>
    /// Get all files for a hazard
    /// </summary>
    public async Task<Result<IEnumerable<HazardFile>>> GetHazardFilesAsync(string hazardCode, bool includeFileData = false, string? category = null, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving files for hazard {HazardCode}", hazardCode);
            return await _hazardFileRepository.GetByHazardCodeAsync(hazardCode, includeFileData, category, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving files for hazard {HazardCode}", hazardCode);
            return Result<IEnumerable<HazardFile>>.Failure<IEnumerable<HazardFile>>(DomainErrors.HazardFileError.NotFound);
        }
    }

    /// <summary>
    /// Get file by code with file data
    /// </summary>
    public async Task<Result<HazardFile>> GetFileWithDataAsync(string fileCode, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving file data for code {FileCode}", fileCode);
            return await _hazardFileRepository.GetFileDataAsync(fileCode, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving file data for code {FileCode}", fileCode);
            return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.NotFound);
        }
    }

    /// <summary>
    /// Update file metadata
    /// </summary>
    public async Task<Result<HazardFile>> UpdateFileMetadataAsync(
        string fileCode, 
        string? description = null,
        HazardFileCategory? category = null,
        bool? isConfidential = null,
        string? tags = null,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Updating metadata for file {FileCode}", fileCode);

            var fileResult = await _hazardFileRepository.GetByCodeAsync(fileCode, ct);
            if (fileResult.IsFailure)
            {
                return fileResult;
            }

            var file = fileResult.Value;
            var metadataResult = file.UpdateMetadata(description, category, isConfidential, tags);
            if (metadataResult.IsFailure)
            {
                return Result<HazardFile>.Failure<HazardFile>(metadataResult.Error);
            }

            return await _hazardFileRepository.UpdateAsync(file, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating metadata for file {FileCode}", fileCode);
            return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.UpdateFailed);
        }
    }

    /// <summary>
    /// Set file confidentiality
    /// </summary>
    public async Task<Result<bool>> SetFileConfidentialityAsync(string fileCode, bool isConfidential, string updatedBy, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Setting confidentiality for file {FileCode} to {IsConfidential}", fileCode, isConfidential);
            return await _hazardFileRepository.SetFileConfidentialityAsync(fileCode, isConfidential, updatedBy, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error setting confidentiality for file {FileCode}", fileCode);
            return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.UpdateFailed);
        }
    }

    /// <summary>
    /// Deactivate a file
    /// </summary>
    public async Task<Result<bool>> DeactivateFileAsync(int fileId, string reason, string deactivatedBy, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Deactivating file {FileId}", fileId);
            return await _hazardFileRepository.DeactivateAsync(fileId, reason, deactivatedBy, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deactivating file {FileId}", fileId);
            return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.DeleteFailed);
        }
    }

    /// <summary>
    /// Get file statistics for a hazard
    /// </summary>
    public async Task<Result<HazardFileStatistics>> GetHazardFileStatisticsAsync(string hazardCode, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving file statistics for hazard {HazardCode}", hazardCode);
            return await _hazardFileRepository.GetStatisticsAsync(hazardCode, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving file statistics for hazard {HazardCode}", hazardCode);
            return Result<HazardFileStatistics>.Failure<HazardFileStatistics>(DomainErrors.HazardFileError.NotFound);
        }
    }

    /// <summary>
    /// Search files across multiple criteria
    /// </summary>
    public async Task<Result<IEnumerable<HazardFile>>> SearchFilesAsync(
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
            _logger.LogInformation("Searching files with criteria");
            return await _hazardFileRepository.SearchAsync(
                hazardCode, reportCode, fileType, category, searchText, 
                uploadedBy, dateFrom, dateTo, includeConfidential, maxResults, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error searching files");
            return Result<IEnumerable<HazardFile>>.Failure<IEnumerable<HazardFile>>(DomainErrors.HazardFileError.NotFound);
        }
    }

    /// <summary>
    /// Get photo files for a hazard
    /// </summary>
    public async Task<Result<IEnumerable<HazardFile>>> GetHazardPhotosAsync(string hazardCode, CancellationToken ct = default)
    {
        return await _hazardFileRepository.GetImageFilesAsync(hazardCode, ct);
    }

    /// <summary>
    /// Get document files for a hazard
    /// </summary>
    public async Task<Result<IEnumerable<HazardFile>>> GetHazardDocumentsAsync(string hazardCode, CancellationToken ct = default)
    {
        return await _hazardFileRepository.GetDocumentFilesAsync(hazardCode, ct);
    }

    /// <summary>
    /// Get video files for a hazard
    /// </summary>
    public async Task<Result<IEnumerable<HazardFile>>> GetHazardVideosAsync(string hazardCode, CancellationToken ct = default)
    {
        return await _hazardFileRepository.GetVideoFilesAsync(hazardCode, ct);
    }

    #endregion
}