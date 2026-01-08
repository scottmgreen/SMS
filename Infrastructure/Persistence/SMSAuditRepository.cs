using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Infrastructure.Common;
using SMS_Infrastructure.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Infrastructure.Persistence;

public sealed class SMSAuditRepository : BaseRepository<SMSAuditRepository, SMSAudit>
{
    private readonly ILogger<SMSAuditRepository> _logger;
    private readonly string _logheader;
    private readonly string _connectionString;

    public SMSAuditRepository(
        ILogger<SMSAuditRepository> logger, 
        ILogSupport logsupport, 
        IConfiguration configuration)
        : base(logger, logsupport, configuration)
    {
        _logger = Logger;
        _logheader = LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, 
            $"{_logheader} SMSAudit Repository Initialized");
    }

    #region SMS Audit CRUD Operations

    public async Task<Result<SMSAudit>> CreateSMSAuditAsync(SMSAudit audit, CancellationToken ct = default)
    {
        try
        {
            if (audit is null)
            {
                return Result<SMSAudit>.Failure<SMSAudit>(DomainErrors.SMSAuditError.NullOrEmpty);
            }

            _logger.LogInfrastructurePostItem($"{_logheader} {StoredProcs.pr_SMSAudit_Insert} Code:{audit.Code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSAudit_Insert, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Add parameters
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditCode, audit.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditName, audit.Name));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditDescription, audit.Description));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditAuditPlanCode, audit.AuditPlanCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditAuditType, audit.AuditType));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditScope, audit.Scope));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditObjectives, audit.Objectives));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditScheduledStartDate, audit.ScheduledStartDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditScheduledEndDate, audit.ScheduledEndDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditLeadAuditor, audit.LeadAuditor));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditAuditorTeam, audit.AuditorTeam));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditResponsibleDepartment, audit.ResponsibleDepartment));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditStatus, audit.Status));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPriority, audit.Priority));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditContactPerson, audit.ContactPerson));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditLocation, audit.AuditLocation));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, audit.CreatedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, audit.CreatedDate));
            var newID = new SqlParameter("@pNewID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var newCode = new SqlParameter("@pNewAuditCode", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(newID);
            cmd.Parameters.Add(newCode);

            await sql.OpenAsync(ct).ConfigureAwait(false);
            var newId = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);


            int newIdValue = (int)newID.Value;
            string newCodeValue = Convert.ToString(newCode.Value) ?? string.Empty;

            if (newId != null)
            {
                var result = await GetSMSAuditByCodeAsync(newCodeValue, ct).ConfigureAwait(false);
                return result;
            }

            return Result.Failure<SMSAudit>(DomainErrors.GeneralError.UnProcessableRequest);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logheader} {ex.Message}", null);
            return Result.Failure<SMSAudit>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<SMSAudit>> GetSMSAuditByCodeAsync(string code, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureGetItem($"{_logheader} {StoredProcs.pr_SMSAudit_GetById} ID:{code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSAudit_GetById, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditId, code));

            SMSAudit? audit = null;

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            
            if (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                audit = Mappers.MapToSMSAudit(reader);
            }
            
            await sql.CloseAsync().ConfigureAwait(false);

            if (audit != null)
            {
                return Result.Success(audit);
            }
            else
            {
                return Result<SMSAudit>.Failure<SMSAudit>(DomainErrors.SMSAuditError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logheader} {ex.Message}", null);
            return Result.Failure<SMSAudit>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<List<SMSAudit>>> GetAllSMSAuditsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_SMSAudit_GetAll}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSAudit_GetAll, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            List<SMSAudit> audits = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            
            while (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                var audit = Mappers.MapToSMSAudit(reader);
                audits.Add(audit);
            }
            
            await sql.CloseAsync().ConfigureAwait(false);

            return Result.Success(audits);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result.Failure<List<SMSAudit>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<SMSAudit>> UpdateSMSAuditAsync(SMSAudit audit, CancellationToken ct = default)
    {
        try
        {
            if (audit is null)
            {
                return Result<SMSAudit>.Failure<SMSAudit>(DomainErrors.SMSAuditError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.pr_SMSAudit_Update} Code:{audit.Code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSAudit_Update, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Add parameters
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditId, audit.Id.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditCode, audit.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditName, audit.Name));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditDescription, audit.Description));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditAuditPlanCode, audit.AuditPlanCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditAuditType, audit.AuditType));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditScope, audit.Scope));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditObjectives, audit.Objectives));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditScheduledStartDate, audit.ScheduledStartDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditScheduledEndDate, audit.ScheduledEndDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditActualStartDate, audit.ActualStartDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditActualEndDate, audit.ActualEndDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditLeadAuditor, audit.LeadAuditor));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditAuditorTeam, audit.AuditorTeam));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditResponsibleDepartment, audit.ResponsibleDepartment));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditStatus, audit.Status));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPriority, audit.Priority));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditContactPerson, audit.ContactPerson));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditLocation, audit.AuditLocation));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditExecutiveSummary, audit.ExecutiveSummary));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditNotes, audit.Notes));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, audit.UpdatedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, audit.UpdatedDate ?? DateTime.UtcNow));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            var rowsAffected = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            if (rowsAffected > 0)
            {
                return Result.Success(audit);
            }

            return Result<SMSAudit>.Failure<SMSAudit>(DomainErrors.SMSAuditError.NotFound);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result.Failure<SMSAudit>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<bool>> DeleteSMSAuditAsync(string code, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureDeleteItem($"{_logheader} {StoredProcs.pr_SMSAudit_Delete} ID:{code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSAudit_Delete, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditCode, code));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            var rowsAffected = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            if (rowsAffected > 0)
            {
                return Result.Success(true);
            }

            return Result<bool>.Failure<bool>(DomainErrors.SMSAuditError.NotFound);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureDeleteItemError($"{_logheader} {ex.Message}", null);
            return Result.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    #endregion
}