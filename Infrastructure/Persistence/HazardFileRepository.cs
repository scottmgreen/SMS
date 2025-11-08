using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Domain.Models;
using SMS_Infrastructure.Common;
using SMS_Infrastructure.Interfaces;
using SMS_Shared.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;

namespace SMS_Infrastructure.Persistence;

/// <summary>
/// Repository implementation for Hazard File operations
/// </summary>
public sealed class HazardFileRepository : BaseRepository<HazardFileRepository, HazardFile>, IHazardFileRepository
{
    private readonly ILogger<HazardFileRepository> _logger;
    private readonly string _logHeader;
    private readonly string _connectionString;

    public HazardFileRepository(ILogger<HazardFileRepository> logger, ILogSupport logSupport, IConfiguration configuration)
        : base(logger, logSupport, configuration)
    {
        _logger = base.Logger;
        _logHeader = base.LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logHeader} Hazard File Repository Initialized");
    }

    public async Task<Result<HazardFile>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInfrastructureGetItem($"{_logHeader} {StoredProcs.pr_HazardFile_GetById} ID:{id}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_HazardFile_GetById, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileId, id));

            HazardFile? hazardFile = null;

            await sql.OpenAsync(cancellationToken).ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            
            if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                hazardFile = Mappers.MapToHazardFile(reader);
            }
            
            await sql.CloseAsync().ConfigureAwait(false);

            if (hazardFile != null)
            {
                return Result<HazardFile>.Success(hazardFile);
            }
            else
            {
                return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logHeader} {ex.Message}", null);
            return Result<HazardFile>.Failure<HazardFile>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<HazardFile>> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItem($"{_logHeader} {StoredProcs.pr_HazardFile_GetByCode} Code:{code}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_HazardFile_GetByCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileCode, code));

            HazardFile? hazardFile = null;

            await sql.OpenAsync(cancellationToken).ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            
            if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                hazardFile = Mappers.MapToHazardFile(reader);
            }
            
            await sql.CloseAsync().ConfigureAwait(false);

            if (hazardFile != null)
            {
                return Result<HazardFile>.Success(hazardFile);
            }
            else
            {
                return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logHeader} {ex.Message}", null);
            return Result<HazardFile>.Failure<HazardFile>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<HazardFile>> AddAsync(HazardFile hazardFile, CancellationToken cancellationToken = default)
    {
        try
        {
            if (hazardFile is null)
            {
                return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.NullOrEmpty);
            }

            _logger.LogInfrastructurePostItem($"{_logHeader} {StoredProcs.pr_HazardFile_Insert} Code:{hazardFile.Code}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_HazardFile_Insert, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Add parameters
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileCode, hazardFile.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileHazardCode, hazardFile.HazardCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileReportCode, hazardFile.ReportCode ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileFileName, hazardFile.FileName));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileFileType, hazardFile.FileType));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileContentType, hazardFile.ContentType));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileFileSizeBytes, hazardFile.FileSizeBytes));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileFileHash, hazardFile.FileHash ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileStorageType, hazardFile.StorageType.ToString()));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileFilePath, hazardFile.FilePath ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileFileData, hazardFile.FileData ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileDescription, hazardFile.Description ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileCategory, hazardFile.Category?.ToString() ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileIsConfidential, hazardFile.IsConfidential));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileUploadedBy, hazardFile.UploadedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileTags, hazardFile.Tags ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, hazardFile.CreatedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, hazardFile.CreatedDate));

            await sql.OpenAsync(cancellationToken).ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            
            HazardFile? result = null;
            if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                result = Mappers.MapToHazardFile(reader);
            }
            
            await sql.CloseAsync().ConfigureAwait(false);

            if (result != null)
            {
                return Result<HazardFile>.Success(result);
            }
            else
            {
                return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.CreateFailed);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logHeader} {ex.Message}", null);
            return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.CreateFailed);
        }
    }

    public async Task<Result<HazardFile>> UpdateAsync(HazardFile hazardFile, CancellationToken cancellationToken = default)
    {
        try
        {
            if (hazardFile is null)
            {
                return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logHeader} {StoredProcs.pr_HazardFile_Update} Code:{hazardFile.Code}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_HazardFile_Update, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Extract ID from the HazardFile (you may need to add this property or modify based on your ID strategy)
            var id = ExtractIdFromHazardFile(hazardFile); // This method would need to be implemented
            
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileId, id));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileFileName, hazardFile.FileName));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileDescription, hazardFile.Description ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileCategory, hazardFile.Category?.ToString() ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileIsConfidential, hazardFile.IsConfidential));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileTags, hazardFile.Tags ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, hazardFile.UpdatedBy ?? "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, hazardFile.UpdatedDate ?? DateTime.UtcNow));

            await sql.OpenAsync(cancellationToken).ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            
            HazardFile? result = null;
            if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                result = Mappers.MapToHazardFile(reader);
            }
            
            await sql.CloseAsync().ConfigureAwait(false);

            if (result != null)
            {
                return Result<HazardFile>.Success(result);
            }
            else
            {
                return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.UpdateFailed);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logHeader} {ex.Message}", null);
            return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeactivateAsync(int id, string reason, string deactivatedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInfrastructureDeleteItem($"{_logHeader} {StoredProcs.pr_HazardFile_Deactivate} ID:{id}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_HazardFile_Deactivate, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileId, id));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileInactiveReason, reason));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileInactiveBy, deactivatedBy));

            await sql.OpenAsync(cancellationToken).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureDeleteItemError($"{_logHeader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.DeleteFailed);
        }
    }

    public async Task<Result<bool>> ReactivateAsync(int id, string reactivatedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInfrastructurePostItem($"{_logHeader} {StoredProcs.pr_HazardFile_Reactivate} ID:{id}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_HazardFile_Reactivate, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileId, id));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, reactivatedBy));

            await sql.OpenAsync(cancellationToken).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logHeader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.UpdateFailed);
        }
    }

    public async Task<Result<IEnumerable<HazardFile>>> GetByHazardCodeAsync(string hazardCode, bool includeFileData = false, string? category = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(hazardCode))
            {
                return Result<IEnumerable<HazardFile>>.Failure<IEnumerable<HazardFile>>(DomainErrors.HazardFileError.HazardCodeRequired);
            }

            _logger.LogInfrastructureGetItems($"{_logHeader} {StoredProcs.pr_HazardFile_GetByHazardCode} HazardCode:{hazardCode}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_HazardFile_GetByHazardCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileHazardCode, hazardCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileIncludeFileData, includeFileData));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileCategory, category ?? (object)DBNull.Value));

            var hazardFiles = new List<HazardFile>();

            await sql.OpenAsync(cancellationToken).ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var hazardFile = Mappers.MapToHazardFile(reader);
                hazardFiles.Add(hazardFile);
            }
            
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<HazardFile>>.Success(hazardFiles.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<HazardFile>>.Failure<IEnumerable<HazardFile>>(DomainErrors.HazardFileError.NotFound);
        }
    }

    public async Task<Result<IEnumerable<HazardFile>>> GetByReportCodeAsync(string reportCode, bool includeFileData = false, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(reportCode))
            {
                return Result<IEnumerable<HazardFile>>.Failure<IEnumerable<HazardFile>>(DomainErrors.HazardFileError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItems($"{_logHeader} {StoredProcs.pr_HazardFile_GetByReportCode} ReportCode:{reportCode}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_HazardFile_GetByReportCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileReportCode, reportCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileIncludeFileData, includeFileData));

            var hazardFiles = new List<HazardFile>();

            await sql.OpenAsync(cancellationToken).ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var hazardFile = Mappers.MapToHazardFile(reader);
                hazardFiles.Add(hazardFile);
            }
            
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<HazardFile>>.Success(hazardFiles.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<HazardFile>>.Failure<IEnumerable<HazardFile>>(DomainErrors.HazardFileError.NotFound);
        }
    }

    public async Task<Result<IEnumerable<HazardFile>>> GetActiveFilesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logHeader} GetActiveFiles", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("SELECT * FROM tbld_HazardFiles WHERE fldb_IsActive = 1 ORDER BY fldd_UploadedDate DESC", sql)
            {
                CommandType = CommandType.Text
            };

            var hazardFiles = new List<HazardFile>();

            await sql.OpenAsync(cancellationToken).ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var hazardFile = Mappers.MapToHazardFile(reader);
                hazardFiles.Add(hazardFile);
            }
            
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<HazardFile>>.Success(hazardFiles.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<HazardFile>>.Failure<IEnumerable<HazardFile>>(DomainErrors.HazardFileError.NotFound);
        }
    }

    public async Task<Result<HazardFile>> GetFileDataAsync(string code, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItem($"{_logHeader} {StoredProcs.pr_HazardFile_GetFileData} Code:{code}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_HazardFile_GetFileData, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileCode, code));

            HazardFile? hazardFile = null;

            await sql.OpenAsync(cancellationToken).ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            
            if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                hazardFile = Mappers.MapToHazardFile(reader);
            }
            
            await sql.CloseAsync().ConfigureAwait(false);

            if (hazardFile != null)
            {
                return Result<HazardFile>.Success(hazardFile);
            }
            else
            {
                return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logHeader} {ex.Message}", null);
            return Result<HazardFile>.Failure<HazardFile>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<IEnumerable<HazardFile>>> SearchAsync(
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
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logHeader} {StoredProcs.pr_HazardFile_Search}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_HazardFile_Search, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileHazardCode, hazardCode ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileReportCode, reportCode ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileFileType, fileType ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileCategory, category ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileSearchText, searchText ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileUploadedBy, uploadedBy ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileDateFrom, dateFrom ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileDateTo, dateTo ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileIncludeConfidential, includeConfidential));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileMaxResults, maxResults));

            var hazardFiles = new List<HazardFile>();

            await sql.OpenAsync(cancellationToken).ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var hazardFile = Mappers.MapToHazardFile(reader);
                hazardFiles.Add(hazardFile);
            }
            
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<HazardFile>>.Success(hazardFiles.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<HazardFile>>.Failure<IEnumerable<HazardFile>>(DomainErrors.HazardFileError.NotFound);
        }
    }

    public async Task<Result<HazardFileStatistics>> GetStatisticsAsync(string hazardCode, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(hazardCode))
            {
                return Result<HazardFileStatistics>.Failure<HazardFileStatistics>(DomainErrors.HazardFileError.HazardCodeRequired);
            }

            _logger.LogInfrastructureGetItem($"{_logHeader} {StoredProcs.pr_HazardFile_GetStatistics} HazardCode:{hazardCode}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_HazardFile_GetStatistics, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileHazardCode, hazardCode));

            HazardFileStatistics? statistics = null;

            await sql.OpenAsync(cancellationToken).ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            
            if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                statistics = Mappers.MapToHazardFileStatistics(reader);
            }
            
            await sql.CloseAsync().ConfigureAwait(false);

            if (statistics != null)
            {
                return Result<HazardFileStatistics>.Success(statistics);
            }
            else
            {
                return Result<HazardFileStatistics>.Failure<HazardFileStatistics>(DomainErrors.HazardFileError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logHeader} {ex.Message}", null);
            return Result<HazardFileStatistics>.Failure<HazardFileStatistics>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    // Convenience methods for specific file types
    public async Task<Result<IEnumerable<HazardFile>>> GetImageFilesAsync(string hazardCode, CancellationToken cancellationToken = default)
    {
        return await GetByHazardCodeAsync(hazardCode, false, "Photo", cancellationToken);
    }

    public async Task<Result<IEnumerable<HazardFile>>> GetDocumentFilesAsync(string hazardCode, CancellationToken cancellationToken = default)
    {
        return await GetByHazardCodeAsync(hazardCode, false, "Document", cancellationToken);
    }

    public async Task<Result<IEnumerable<HazardFile>>> GetVideoFilesAsync(string hazardCode, CancellationToken cancellationToken = default)
    {
        return await GetByHazardCodeAsync(hazardCode, false, "Video", cancellationToken);
    }

    public async Task<Result<IEnumerable<HazardFile>>> GetConfidentialFilesAsync(string hazardCode, CancellationToken cancellationToken = default)
    {
        try
        {
            var allFilesResult = await GetByHazardCodeAsync(hazardCode, false, null, cancellationToken);
            if (allFilesResult.IsFailure)
            {
                return allFilesResult;
            }

            var confidentialFiles = allFilesResult.Value.Where(f => f.IsConfidential).ToList();
            return Result<IEnumerable<HazardFile>>.Success(confidentialFiles.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<HazardFile>>.Failure<IEnumerable<HazardFile>>(DomainErrors.HazardFileError.NotFound);
        }
    }

    public async Task<Result<bool>> SetFileConfidentialityAsync(string code, bool isConfidential, string updatedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            // First get the file
            var fileResult = await GetByCodeAsync(code, cancellationToken);
            if (fileResult.IsFailure)
            {
                return Result<bool>.Failure<bool>(fileResult.Error);
            }

            var file = fileResult.Value;
            
            // Update confidentiality
            var updateResult = file.SetConfidential(isConfidential);
            if (updateResult.IsFailure)
            {
                return updateResult;
            }

            // Save the changes
            var saveResult = await UpdateAsync(file, cancellationToken);
            if (saveResult.IsFailure)
            {
                return Result<bool>.Failure<bool>(saveResult.Error);
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logHeader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.UpdateFailed);
        }
    }

    // Helper method - you'll need to implement this based on your ID strategy
    private int ExtractIdFromHazardFile(HazardFile hazardFile)
    {
        // This is a placeholder - you'll need to implement based on how you handle IDs
        // You might need to add a numeric ID property to HazardFile or use a different approach
        throw new NotImplementedException("ExtractIdFromHazardFile method needs to be implemented based on your ID strategy");
    }
}