//-----------------------------------------------------------------------
// <copyright file="HazardFileRepository.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Repository implementing data access operations for SMS hazardfile entities with safety management integration.
//                  Repository implementation providing data access operations
//                  with stored procedure integration and entity mapping.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using SMS_Domain.Errors;

using SMS_Infrastructure.Interfaces;

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

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, code));

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
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, hazardFile.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileHazardCode, hazardFile.HazardCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileReportCode, hazardFile.ReportCode ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileFileName, hazardFile.FileName));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileFileType, hazardFile.FileType));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileContentType, hazardFile.ContentType));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileFileSizeBytes, hazardFile.FileSizeBytes));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileStorageType, hazardFile.StorageType ?? "Database"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileFilePath, hazardFile.FilePath ?? (object)DBNull.Value));
            var fileDataParameter = new SqlParameter(ParameterNames.pmHazardFileFileData, SqlDbType.VarBinary)
            {
                Value = hazardFile.FileData is { Length: > 0 } ? hazardFile.FileData : DBNull.Value
            };
            cmd.Parameters.Add(fileDataParameter);
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileDescription, hazardFile.Description ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileCategory, hazardFile.Category ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileIsConfidential, hazardFile.IsConfidential));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileUploadedBy, hazardFile.UploadedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, hazardFile.CreatedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, hazardFile.CreatedDate));
            var newID = new SqlParameter("@pNewID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var newCode = new SqlParameter("@pNewHazardFileCode", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(newID);
            cmd.Parameters.Add(newCode);

            await sql.OpenAsync(cancellationToken).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            int newIdValue = (int)newID.Value;
            string newCodeValue = Convert.ToString(newCode.Value) ?? string.Empty;
            HazardFileID hazardId = new(newCodeValue);

            return await GetByCodeAsync(hazardId.Value, cancellationToken).ConfigureAwait(false);

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

                       

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, hazardFile.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileFileName, hazardFile.FileName));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileDescription, hazardFile.Description ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileCategory, hazardFile.Category ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardFileIsConfidential, hazardFile.IsConfidential));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, hazardFile.UpdatedBy ?? hazardFile.CreatedBy ?? string.Empty));
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

    public async Task<Result<bool>> DeactivateAsync(string code, string reason, string deactivatedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInfrastructureDeleteItem($"{_logHeader} {StoredProcs.pr_HazardFile_Deactivate} Code:{code}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_HazardFile_Deactivate, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, code));
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

    public async Task<Result<bool>> ReactivateAsync(string code, string reactivatedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInfrastructurePostItem($"{_logHeader} {StoredProcs.pr_HazardFile_Reactivate} Code:{code}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_HazardFile_Reactivate, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, code));
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

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, code));

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

    
}
