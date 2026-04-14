//-----------------------------------------------------------------------
// <copyright file="SMSAuditPlanRepository.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Repository implementing data access operations for SMS smsauditplan entities supporting compliance and audit workflows.
//                  Repository implementation providing data access operations
//                  with stored procedure integration and entity mapping.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using SMS_Domain.Errors;

using SMS_Infrastructure.Interfaces;

namespace SMS_Infrastructure.Persistence;

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

            // Add parameters - CORRECTED to match stored procedure exactly
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, auditPlan.Code)); // Keep original pattern
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
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanApprovedBy, auditPlan.ApprovedBy)); // MISSING!
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanApprovedDate, auditPlan.ApprovedDate)); // MISSING!
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanExpectedDurationHours, auditPlan.ExpectedDurationHours));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanNotes, auditPlan.Notes));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, auditPlan.CreatedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, auditPlan.CreatedDate));

            // Output parameters
            var newID = new SqlParameter("@pNewID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var newCode = new SqlParameter("@pNewAuditPlanCode", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
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
            _logger.LogInfrastructureGetItem($"{_logheader} {StoredProcs.pr_SMSAuditPlan_GetByCode} ID:{code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSAuditPlan_GetByCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, code));

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

            // Add parameters - COMPLETE LIST to match stored procedure
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, auditPlan.Code.Trim()));
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
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanApprovedBy, auditPlan.ApprovedBy)); // MISSING!
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanApprovedDate, auditPlan.ApprovedDate)); // MISSING!
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanExpectedDurationHours, auditPlan.ExpectedDurationHours));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSAuditPlanNotes, auditPlan.Notes));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, auditPlan.UpdatedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, auditPlan.UpdatedDate ?? DateTime.UtcNow));

            // DEBUG: Log the actual parameter value being sent
            foreach (SqlParameter param in cmd.Parameters)
            {
                _logger.LogInformation("DEBUG: Parameter {Name} = {Value}", param.ParameterName, param.Value);
            }

            await sql.OpenAsync(ct).ConfigureAwait(false);

            // DEBUG: Add a test to see what's actually happening
            _logger.LogInformation("DEBUG: About to execute stored procedure");

            try
            {
                var rowsAffected = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                _logger.LogInformation("DEBUG: Stored procedure returned: {RowsAffected}", rowsAffected);

                // Even if it returns -1, let's check if the record was actually updated
                if (rowsAffected == -1)
                {
                    _logger.LogWarning("DEBUG: Got -1, but let's check if record was actually updated");
                    // Try to get the record to see if it was updated
                    var checkResult = await GetSMSAuditPlanByCodeAsync(auditPlan.Code, ct).ConfigureAwait(false);
                    if (checkResult.IsSuccess && checkResult.Value?.Status == auditPlan.Status)
                    {
                        _logger.LogInformation("DEBUG: Record was actually updated despite -1 return");
                        await sql.CloseAsync().ConfigureAwait(false);
                        return Result.Success(auditPlan);
                    }
                }

                await sql.CloseAsync().ConfigureAwait(false);

                if (rowsAffected > 0)
                {
                    _logger.LogInformation("DEBUG: Update successful, returning success");
                    return Result.Success(auditPlan);
                }

                _logger.LogWarning("DEBUG: No rows affected, returning not found");
                return Result<SMSAuditPlan>.Failure<SMSAuditPlan>(DomainErrors.SMSAuditPlanError.NotFound);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "DEBUG: SqlException during ExecuteNonQueryAsync: {Message}, ErrorNumber: {ErrorNumber}", ex.Message, ex.Number);
                await sql.CloseAsync().ConfigureAwait(false);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DEBUG: Exception during ExecuteNonQueryAsync: {Message}", ex.Message);
                await sql.CloseAsync().ConfigureAwait(false);
                throw;
            }
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

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, code));

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
