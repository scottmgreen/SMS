//-----------------------------------------------------------------------
// <copyright file="SMSAuditFindingRepository.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Repository implementing data access operations for SMS smsauditfinding entities supporting compliance and audit workflows.
//                  Repository implementation providing data access operations
//                  with stored procedure integration and entity mapping.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Errors;

using SMS_Infrastructure.Interfaces;

namespace Infrastructure.Persistence;

public sealed class SMSAuditFindingRepository : BaseRepository<SMSAuditFindingRepository, SMSAuditFinding>
{
    private readonly ILogger<SMSAuditFindingRepository> _logger;
    private readonly string _logheader;
    private readonly string _connectionString;

    public SMSAuditFindingRepository(
        ILogger<SMSAuditFindingRepository> logger,
        ILogSupport logsupport,
        IConfiguration configuration)
        : base(logger, logsupport, configuration)
    {
        _logger = Logger;
        _logheader = LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent,
            $"{_logheader} SMSAuditFinding Repository Initialized");
    }

    #region SMS Audit Finding CRUD Operations

    public async Task<Result<SMSAuditFinding>> CreateSMSAuditFindingAsync(SMSAuditFinding finding, CancellationToken ct = default)
    {
        try
        {
            if (finding is null)
            {
                return Result<SMSAuditFinding>.Failure<SMSAuditFinding>(DomainErrors.SMSAuditFindingError.NullOrEmpty);
            }

            _logger.LogInfrastructurePostItem($"{_logheader} {StoredProcs.pr_SMSAuditFinding_Insert} Code:{finding.Code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSAuditFinding_Insert, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Add parameters
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditFindingCode, finding.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditFindingAuditCode, finding.AuditCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditFindingTitle, finding.Title));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditFindingDescription, finding.Description));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditFindingSeverity, finding.Severity));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditFindingCategory, finding.Category));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditFindingStatus, finding.Status));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditFindingDiscoveredDate, finding.DiscoveredDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditFindingResponsiblePerson, finding.ResponsiblePerson));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditFindingTargetResolutionDate, finding.TargetResolutionDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditFindingCorrectiveAction, finding.CorrectiveAction));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditFindingRootCauseAnalysis, finding.RootCauseAnalysis));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditFindingVerificationRequired, finding.VerificationRequired));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditFindingNotes, finding.Notes));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, finding.CreatedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, finding.CreatedDate));

            // ? FIXED: Use correct output parameter names to match stored procedure exactly
            var newID = new SqlParameter("@pNewID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var newCode = new SqlParameter("@pNewAuditFindingCode", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(newID);
            cmd.Parameters.Add(newCode);

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false); // ? FIXED: Use ExecuteNonQuery not ExecuteScalar for INSERT with output params
            await sql.CloseAsync().ConfigureAwait(false);

            // ? FIXED: Get values from output parameters 
            int newIdValue = (int)newID.Value;
            string newCodeValue = Convert.ToString(newCode.Value) ?? string.Empty;

            if (newIdValue > 0 && !string.IsNullOrEmpty(newCodeValue))
            {
                var result = await GetSMSAuditFindingByCodeAsync(newCodeValue, ct).ConfigureAwait(false);
                return result;
            }

            return Result.Failure<SMSAuditFinding>(DomainErrors.GeneralError.UnProcessableRequest);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logheader} {ex.Message}", null);
            return Result.Failure<SMSAuditFinding>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<SMSAuditFinding>> GetSMSAuditFindingByCodeAsync(string code, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureGetItem($"{_logheader} {StoredProcs.pr_SMSAuditFinding_GetByCode} Code:{code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSAuditFinding_GetByCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditFindingCode, code));

            SMSAuditFinding? finding = null;

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);

            if (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                finding = Mappers.MapToSMSAuditFinding(reader);
            }

            await sql.CloseAsync().ConfigureAwait(false);

            if (finding != null)
            {
                return Result.Success(finding);
            }
            else
            {
                return Result<SMSAuditFinding>.Failure<SMSAuditFinding>(DomainErrors.SMSAuditFindingError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logheader} {ex.Message}", null);
            return Result.Failure<SMSAuditFinding>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<IEnumerable<SMSAuditFinding>>> GetAllSMSAuditFindingsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_SMSAuditFinding_GetAll}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSAuditFinding_GetAll, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            List<SMSAuditFinding> findings = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);

            while (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                var finding = Mappers.MapToSMSAuditFinding(reader);
                findings.Add(finding);
            }

            await sql.CloseAsync().ConfigureAwait(false);

            return Result.Success(findings.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result.Failure<IEnumerable<SMSAuditFinding>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<SMSAuditFinding>> UpdateSMSAuditFindingAsync(SMSAuditFinding finding, CancellationToken ct = default)
    {
        try
        {
            if (finding is null)
            {
                return Result<SMSAuditFinding>.Failure<SMSAuditFinding>(DomainErrors.SMSAuditFindingError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.pr_SMSAuditFinding_Update} Code:{finding.Code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSAuditFinding_Update, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            // ? FIXED: Use correct ID parameter name for UPDATE operations
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditFindingId, finding.Id.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditFindingCode, finding.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditFindingAuditCode, finding.AuditCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditFindingTitle, finding.Title));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditFindingDescription, finding.Description));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditFindingSeverity, finding.Severity));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditFindingCategory, finding.Category));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditFindingStatus, finding.Status));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditFindingDiscoveredDate, finding.DiscoveredDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditFindingResponsiblePerson, finding.ResponsiblePerson));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditFindingTargetResolutionDate, finding.TargetResolutionDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditFindingActualResolutionDate, finding.ActualResolutionDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditFindingCorrectiveAction, finding.CorrectiveAction));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditFindingRootCauseAnalysis, finding.RootCauseAnalysis));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditFindingVerificationRequired, finding.VerificationRequired));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditFindingVerifiedBy, finding.VerifiedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditFindingVerificationDate, finding.VerificationDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditFindingNotes, finding.Notes));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, finding.UpdatedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, finding.UpdatedDate ?? DateTime.UtcNow));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            var rowsAffected = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            if (rowsAffected > 0)
            {
                return Result.Success(finding);
            }

            return Result<SMSAuditFinding>.Failure<SMSAuditFinding>(DomainErrors.SMSAuditFindingError.NotFound);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result.Failure<SMSAuditFinding>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<bool>> DeleteSMSAuditFindingAsync(string code, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureDeleteItem($"{_logheader} {StoredProcs.pr_SMSAuditFinding_Delete} ID:{code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSAuditFinding_Delete, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditFindingCode, code));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            var rowsAffected = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            if (rowsAffected > 0)
            {
                return Result.Success(true);
            }

            return Result<bool>.Failure<bool>(DomainErrors.SMSAuditFindingError.NotFound);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureDeleteItemError($"{_logheader} {ex.Message}", null);
            return Result.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<IEnumerable<SMSAuditFinding>>> GetSMSAuditFindingsByAuditCodeAsync(string auditCode, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_SMSAuditFinding_GetByAudit} AuditCode:{auditCode}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSAuditFinding_GetByAudit, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditCode, auditCode));

            List<SMSAuditFinding> findings = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);

            while (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                var finding = Mappers.MapToSMSAuditFinding(reader);
                findings.Add(finding);
            }

            await sql.CloseAsync().ConfigureAwait(false);

            return Result.Success(findings.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result.Failure<IEnumerable<SMSAuditFinding>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<IEnumerable<SMSAuditFinding>>> GetOverdueSMSAuditFindingsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_SMSAuditFinding_GetOverdue}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSAuditFinding_GetOverdue, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            List<SMSAuditFinding> findings = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);

            while (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                var finding = Mappers.MapToSMSAuditFinding(reader);
                findings.Add(finding);
            }

            await sql.CloseAsync().ConfigureAwait(false);

            return Result.Success(findings.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result.Failure<IEnumerable<SMSAuditFinding>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
    #endregion
}
