using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Infrastructure.Common;
using SMS_Infrastructure.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Infrastructure.Persistence;

public sealed class SMSAuditPlanRepository : BaseRepository<SMSAuditPlanRepository, SMSAuditPlan>
{
    private readonly ILogger<SMSAuditPlanRepository> _logger;
    private readonly string _logheader;
    private readonly string _connectionString;

    public SMSAuditPlanRepository(
        ILogger<SMSAuditPlanRepository> logger, 
        ILogSupport logsupport, 
        IConfiguration configuration)
        : base(logger, logsupport, configuration)
    {
        _logger = Logger;
        _logheader = LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, 
            $"{_logheader} SMSAuditPlan Repository Initialized");
    }

    #region SMS Audit Plan CRUD Operations

    public async Task<Result<SMSAuditPlan>> CreateSMSAuditPlanAsync(SMSAuditPlan auditPlan, CancellationToken ct = default)
    {
        try
        {
            if (auditPlan is null)
            {
                return Result<SMSAuditPlan>.Failure<SMSAuditPlan>(DomainErrors.SMSAuditPlanError.NullOrEmpty);
            }

            _logger.LogInfrastructurePostItem($"{_logheader} {StoredProcs.pr_SMSAuditPlan_Insert} Code:{auditPlan.Code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSAuditPlan_Insert, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Add parameters
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanCode, auditPlan.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanName, auditPlan.Name));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanDescription, auditPlan.Description));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanAuditType, auditPlan.AuditType));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanScope, auditPlan.Scope));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanObjectives, auditPlan.Objectives));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanPlannedStartDate, auditPlan.PlannedStartDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanPlannedEndDate, auditPlan.PlannedEndDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanLeadAuditor, auditPlan.LeadAuditor));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanAuditorTeam, auditPlan.AuditorTeam));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanResponsibleDepartment, auditPlan.ResponsibleDepartment));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanStatus, auditPlan.Status));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanPriority, auditPlan.Priority));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanRecurrencePattern, auditPlan.RecurrencePattern));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanRequiresApproval, auditPlan.RequiresApproval));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanExpectedDurationHours, auditPlan.ExpectedDurationHours));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanNotes, auditPlan.Notes));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, auditPlan.CreatedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, auditPlan.CreatedDate));
            var newID = new SqlParameter("@pNewID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var newCode = new SqlParameter("@pNewAuditPlanCodeCode", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(newID);
            cmd.Parameters.Add(newCode);

            await sql.OpenAsync(ct).ConfigureAwait(false);
            var newId = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);


            int newIdValue = (int)newID.Value;
            string newCodeValue = Convert.ToString(newCode.Value) ?? string.Empty;
            if (newCodeValue != null)
            {
                var result = await GetSMSAuditPlanByCodeAsync(newCodeValue, ct).ConfigureAwait(false);
                return result;
            }

            return Result.Failure<SMSAuditPlan>(DomainErrors.GeneralError.UnProcessableRequest);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logheader} {ex.Message}", null);
            return Result.Failure<SMSAuditPlan>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<SMSAuditPlan>> GetSMSAuditPlanByCodeAsync(string code, CancellationToken ct = default)

    {
        try
        {
            _logger.LogInfrastructureGetItem($"{_logheader} {StoredProcs.pr_SMSAuditPlan_GetById} ID:{code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSAuditPlan_GetById, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanCode, code));

            SMSAuditPlan? auditPlan = null;

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            
            if (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                auditPlan = Mappers.MapToSMSAuditPlan(reader);
            }
            
            await sql.CloseAsync().ConfigureAwait(false);

            if (auditPlan != null)
            {
                return Result.Success(auditPlan);
            }
            else
            {
                return Result<SMSAuditPlan>.Failure<SMSAuditPlan>(DomainErrors.SMSAuditPlanError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logheader} {ex.Message}", null);
            return Result.Failure<SMSAuditPlan>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<List<SMSAuditPlan>>> GetAllSMSAuditPlansAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_SMSAuditPlan_GetAll}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSAuditPlan_GetAll, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            List<SMSAuditPlan> auditPlans = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            
            while (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                var auditPlan = Mappers.MapToSMSAuditPlan(reader);
                auditPlans.Add(auditPlan);
            }
            
            await sql.CloseAsync().ConfigureAwait(false);

            return Result.Success(auditPlans);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result.Failure<List<SMSAuditPlan>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<SMSAuditPlan>> UpdateSMSAuditPlanAsync(SMSAuditPlan auditPlan, CancellationToken ct = default)
    {
        try
        {
            if (auditPlan is null)
            {
                return Result<SMSAuditPlan>.Failure<SMSAuditPlan>(DomainErrors.SMSAuditPlanError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.pr_SMSAuditPlan_Update} Code:{auditPlan.Code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSAuditPlan_Update, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Add parameters
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanId, auditPlan.Id.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanCode, auditPlan.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanName, auditPlan.Name));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanDescription, auditPlan.Description));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanAuditType, auditPlan.AuditType));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanScope, auditPlan.Scope));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanObjectives, auditPlan.Objectives));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanPlannedStartDate, auditPlan.PlannedStartDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanPlannedEndDate, auditPlan.PlannedEndDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanLeadAuditor, auditPlan.LeadAuditor));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanAuditorTeam, auditPlan.AuditorTeam));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanResponsibleDepartment, auditPlan.ResponsibleDepartment));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanStatus, auditPlan.Status));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanPriority, auditPlan.Priority));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanRecurrencePattern, auditPlan.RecurrencePattern));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanRequiresApproval, auditPlan.RequiresApproval));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanExpectedDurationHours, auditPlan.ExpectedDurationHours));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanNotes, auditPlan.Notes));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, auditPlan.UpdatedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, auditPlan.UpdatedDate ?? DateTime.UtcNow));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            var rowsAffected = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            if (rowsAffected > 0)
            {
                return Result.Success(auditPlan);
            }

            return Result<SMSAuditPlan>.Failure<SMSAuditPlan>(DomainErrors.SMSAuditPlanError.NotFound);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result.Failure<SMSAuditPlan>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<bool>> DeleteSMSAuditPlanAsync(string code, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureDeleteItem($"{_logheader} {StoredProcs.pr_SMSAuditPlan_Delete} ID:{code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSAuditPlan_Delete, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanCode, code));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            var rowsAffected = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            if (rowsAffected > 0)
            {
                return Result.Success(true);
            }

            return Result<bool>.Failure<bool>(DomainErrors.SMSAuditPlanError.NotFound);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureDeleteItemError($"{_logheader} {ex.Message}", null);
            return Result.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    #endregion
}