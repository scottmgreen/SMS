using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Infrastructure.Common;
using SMS_Infrastructure.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Infrastructure.Persistence;

public sealed class SMSAuditEvidenceRepository : BaseRepository<SMSAuditEvidenceRepository, SMSAuditEvidence>
{
    private readonly ILogger<SMSAuditEvidenceRepository> _logger;
    private readonly string _logheader;
    private readonly string _connectionString;

    public SMSAuditEvidenceRepository(
        ILogger<SMSAuditEvidenceRepository> logger, 
        ILogSupport logsupport, 
        IConfiguration configuration)
        : base(logger, logsupport, configuration)
    {
        _logger = Logger;
        _logheader = LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, 
            $"{_logheader} SMSAuditEvidence Repository Initialized");
    }

    #region SMS Audit Evidence CRUD Operations

    public async Task<Result<SMSAuditEvidence>> CreateSMSAuditEvidenceAsync(SMSAuditEvidence evidence, CancellationToken ct = default)
    {
        try
        {
            if (evidence is null)
            {
                return Result<SMSAuditEvidence>.Failure<SMSAuditEvidence>(DomainErrors.SMSAuditEvidenceError.NullOrEmpty);
            }

            _logger.LogInfrastructurePostItem($"{_logheader} {StoredProcs.pr_SMSAuditEvidence_Insert} Code:{evidence.Code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSAuditEvidence_Insert, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Add parameters
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceCode, evidence.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceAuditCode, evidence.AuditCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceFindingCode, evidence.FindingCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceTitle, evidence.Title));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceDescription, evidence.Description));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceType, evidence.EvidenceType));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceSource, evidence.Source));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceCollectedBy, evidence.CollectedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceCollectionDate, evidence.CollectionDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceFilePath, evidence.FilePath));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceFileSize, evidence.FileSize));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceContentType, evidence.ContentType));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceStorageLocation, evidence.StorageLocation));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceConfidentialityLevel, evidence.ConfidentialityLevel));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceRetentionPeriodMonths, evidence.RetentionPeriodMonths));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceRetentionReason, evidence.RetentionReason));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceIsVerified, evidence.IsVerified));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceIsArchived, evidence.IsArchived));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceNotes, evidence.Notes));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, evidence.CreatedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, evidence.CreatedDate));
            var newID = new SqlParameter("@pNewID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var newCode = new SqlParameter("@pNewAuditEvidenceCode", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(newID);
            cmd.Parameters.Add(newCode);

            await sql.OpenAsync(ct).ConfigureAwait(false);
            var newId = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);


            int newIdValue = (int)newID.Value;
            string newCodeValue = Convert.ToString(newCode.Value) ?? string.Empty;

            if (newId != null)
            {
                var result = await GetSMSAuditEvidenceByCodeAsync(newCodeValue, ct).ConfigureAwait(false);
                return result;
            }

            return Result.Failure<SMSAuditEvidence>(DomainErrors.GeneralError.UnProcessableRequest);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logheader} {ex.Message}", null);
            return Result.Failure<SMSAuditEvidence>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<SMSAuditEvidence>> GetSMSAuditEvidenceByCodeAsync(string code, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureGetItem($"{_logheader} {StoredProcs.pr_SMSAuditEvidence_GetById} ID:{code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSAuditEvidence_GetByCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceCode, code));

            SMSAuditEvidence? evidence = null;

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            
            if (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                evidence = Mappers.MapToSMSAuditEvidence(reader);
            }
            
            await sql.CloseAsync().ConfigureAwait(false);

            if (evidence != null)
            {
                return Result.Success(evidence);
            }
            else
            {
                return Result<SMSAuditEvidence>.Failure<SMSAuditEvidence>(DomainErrors.SMSAuditEvidenceError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logheader} {ex.Message}", null);
            return Result.Failure<SMSAuditEvidence>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<IEnumerable<SMSAuditEvidence>>> GetAllSMSAuditEvidenceAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_SMSAuditEvidence_GetAll}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSAuditEvidence_GetAll, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            List<SMSAuditEvidence> evidenceList = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            
            while (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                var evidence = Mappers.MapToSMSAuditEvidence(reader);
                evidenceList.Add(evidence);
            }
            
            await sql.CloseAsync().ConfigureAwait(false);

            return Result.Success(evidenceList.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result.Failure<IEnumerable<SMSAuditEvidence>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<SMSAuditEvidence>> UpdateSMSAuditEvidenceAsync(SMSAuditEvidence evidence, CancellationToken ct = default)
    {
        try
        {
            if (evidence is null)
            {
                return Result<SMSAuditEvidence>.Failure<SMSAuditEvidence>(DomainErrors.SMSAuditEvidenceError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.pr_SMSAuditEvidence_Update} Code:{evidence.Code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSAuditEvidence_Update, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            // FIXED: Use correct parameter name to match stored procedure exactly - @pID parameter
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceId, evidence.Id.Value)); 
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceCode, evidence.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceAuditCode, evidence.AuditCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceFindingCode, evidence.FindingCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceTitle, evidence.Title));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceDescription, evidence.Description));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceType, evidence.EvidenceType));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceSource, evidence.Source));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceCollectedBy, evidence.CollectedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceCollectionDate, evidence.CollectionDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceFilePath, evidence.FilePath));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceFileSize, evidence.FileSize));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceContentType, evidence.ContentType));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceStorageLocation, evidence.StorageLocation));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceConfidentialityLevel, evidence.ConfidentialityLevel));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceRetentionPeriodMonths, evidence.RetentionPeriodMonths));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceRetentionReason, evidence.RetentionReason));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceIsVerified, evidence.IsVerified));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceVerifiedBy, evidence.VerifiedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceVerificationDate, evidence.VerificationDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceIsArchived, evidence.IsArchived));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceArchivedDate, evidence.ArchivedDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceNotes, evidence.Notes));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, evidence.UpdatedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, evidence.UpdatedDate ?? DateTime.UtcNow));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            var rowsAffected = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            if (rowsAffected > 0)
            {
                return await GetSMSAuditEvidenceByCodeAsync(evidence.Code, ct).ConfigureAwait(false);
            }

            return Result<SMSAuditEvidence>.Failure<SMSAuditEvidence>(DomainErrors.SMSAuditEvidenceError.NotFound);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result.Failure<SMSAuditEvidence>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<bool>> DeleteSMSAuditEvidenceAsync(string code, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureDeleteItem($"{_logheader} {StoredProcs.pr_SMSAuditEvidence_Delete} ID:{code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSAuditEvidence_Delete, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceCode, code));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            var rowsAffected = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            if (rowsAffected > 0)
            {
                return Result.Success(true);
            }

            return Result<bool>.Failure<bool>(DomainErrors.SMSAuditEvidenceError.NotFound);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureDeleteItemError($"{_logheader} {ex.Message}", null);
            return Result.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<IEnumerable<SMSAuditEvidence>>> GetSMSAuditEvidenceByAuditCodeAsync(string auditCode, bool includeArchived = false, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_SMSAuditEvidence_GetByAudit} AuditCode:{auditCode}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSAuditEvidence_GetByAudit, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditCode, auditCode));
            cmd.Parameters.Add(DataAccess.Parameter("@pIncludeArchived", includeArchived));

            List<SMSAuditEvidence> evidenceList = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            
            while (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                var evidence = Mappers.MapToSMSAuditEvidence(reader);
                evidenceList.Add(evidence);
            }
            
            await sql.CloseAsync().ConfigureAwait(false);

            return Result.Success(evidenceList.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result.Failure<IEnumerable<SMSAuditEvidence>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<IEnumerable<SMSAuditEvidence>>> GetSMSAuditEvidenceByFindingCodeAsync(string findingCode, bool includeArchived = false, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_SMSAuditEvidence_GetByFinding} FindingCode:{findingCode}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSAuditEvidence_GetByFinding, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditFindingCode, findingCode));
            cmd.Parameters.Add(DataAccess.Parameter("@pIncludeArchived", includeArchived));

            List<SMSAuditEvidence> evidenceList = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            
            while (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                var evidence = Mappers.MapToSMSAuditEvidence(reader);
                evidenceList.Add(evidence);
            }
            
            await sql.CloseAsync().ConfigureAwait(false);

            return Result.Success(evidenceList.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result.Failure<IEnumerable<SMSAuditEvidence>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<bool>> ArchiveSMSAuditEvidenceAsync(string code, string archivedBy = "SYSTEM", CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructurePostItem($"{_logheader} {StoredProcs.pr_SMSAuditEvidence_Archive} Code:{code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSAuditEvidence_Archive, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditEvidenceCode, code));
            cmd.Parameters.Add(DataAccess.Parameter("@pArchivedBy", archivedBy));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            var rowsAffected = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            if (rowsAffected > 0)
            {
                return Result.Success(true);
            }

            return Result<bool>.Failure<bool>(DomainErrors.SMSAuditEvidenceError.NotFound);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logheader} {ex.Message}", null);
            return Result.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    #endregion
}