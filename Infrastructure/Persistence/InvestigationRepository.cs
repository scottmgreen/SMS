//-----------------------------------------------------------------------
// <copyright file="InvestigationRepository.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Repository implementing data access operations for SMS investigation entities with investigation workflow support.
//                  Repository implementation providing data access operations
//                  with stored procedure integration and entity mapping.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Errors;

using SMS_Infrastructure.Interfaces;

namespace SMS_Infrastructure.Persistence;

/// <summary>
/// Enhanced Investigation Repository with comprehensive investigation management capabilities
/// </summary>
public sealed class InvestigationRepository : BaseRepository<InvestigationRepository, Investigation>, IInvestigationRepository
{
    private readonly ILogger<InvestigationRepository> _logger;
    private readonly string _logheader;
    private readonly string _connectionString;

    public InvestigationRepository(ILogger<InvestigationRepository> logger, ILogSupport logsupport, IConfiguration configuration)
        : base(logger, logsupport, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} Enhanced Investigation Repository Initialized");
    }

    public async Task<Result<Investigation>> CreateInvestigationAsync(Investigation investigation, CancellationToken ct = default)
    {
        try
        {
            if (investigation is null)
            {
                return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.NullOrEmpty);
            }

            _logger.LogInfrastructurePostItem($"{_logheader} {StoredProcs.pr_Investigation_Insert} Code:{investigation.Code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Investigation_Insert, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Core properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationCode, investigation.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationReportCode, investigation.ReportCode ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationHazardCode, investigation.HazardCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationNotes, investigation.InvestigationNotes ?? (object)DBNull.Value));

            // Management properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationAssignedInvestigatorId, investigation.AssignedInvestigatorId));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationStatus, investigation.Status.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationPlan, investigation.InvestigationPlan ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationObjectives, investigation.InvestigationObjectives ?? (object)DBNull.Value));

            // Decision properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationDecisionType, investigation.DecisionType ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationDecisionRationale, investigation.DecisionRationale ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationDecisionMaker, investigation.DecisionMaker ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationDecisionDate, investigation.DecisionDate ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationNextSteps, investigation.NextSteps ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationReferralDetails, investigation.ReferralDetails ?? (object)DBNull.Value));

            // Audit properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, investigation.CreatedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, investigation.CreatedDate));

            var newID = new SqlParameter("@pNewID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var newCode = new SqlParameter("@pNewInvestigationCode", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(newID);
            cmd.Parameters.Add(newCode);

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            int newIdValue = (int)newID.Value;
            string newCodeValue = Convert.ToString(newCode.Value) ?? string.Empty;
            InvestigationID investigationId = new(newCodeValue);

            return await GetInvestigationByCodeAsync(investigationId.Value, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logheader} {ex.Message}", null);
            return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.CreateFailed);
        }
    }

    //public async Task<Result<Investigation>> GetInvestigationByIdAsync(InvestigationID id, CancellationToken ct = default)
    //{
    //    try
    //    {
    //        if (id is null)
    //        {
    //            return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.NullOrEmpty);
    //        }

    //        _logger.LogInfrastructureGetItem($"{_logheader} {StoredProcs.pr_Investigation_GetById} {id}", null);

    //        using SqlConnection sql = new(_connectionString);
    //        using SqlCommand cmd = new(StoredProcs.pr_Investigation_GetById, sql)
    //        {
    //            CommandType = CommandType.StoredProcedure
    //        };

    //        cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, id.Value));

    //        Investigation? response = null;

    //        await sql.OpenAsync(ct).ConfigureAwait(false);
    //        using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
    //        {
    //            while (await reader.ReadAsync().ConfigureAwait(false))
    //            {
    //                response = Mappers.MapToInvestigation(reader);
    //            }
    //        }
    //        await sql.CloseAsync().ConfigureAwait(false);

    //        if (response is not null)
    //        {
    //            return Result<Investigation>.Success(response);
    //        }
    //        else
    //        {
    //            return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.NotFound);
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogInfrastructureGetItemError($"{_logheader} {ex.Message}", null);
    //        return Result<Investigation>.Failure<Investigation>(DomainErrors.GeneralError.UnProcessableRequest);
    //    }
    //}

    public async Task<Result<Investigation>> GetInvestigationByCodeAsync(string code, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.CodeRequired);
            }

            _logger.LogInfrastructureGetItem($"{_logheader} {StoredProcs.pr_Investigation_GetByCode} Code:{code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Investigation_GetByCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationCode, code));

            Investigation? response = null;

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    response = Mappers.MapToInvestigation(reader);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            if (response is not null)
            {
                return Result<Investigation>.Success(response);
            }
            else
            {
                return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logheader} {ex.Message}", null);
            return Result<Investigation>.Failure<Investigation>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<List<Investigation>>> GetAllInvestigationsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_Investigation_GetAll}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Investigation_GetAll, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            List<Investigation> response = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var investigation = Mappers.MapToInvestigation(reader);
                    response.Add(investigation);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<List<Investigation>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<List<Investigation>>.Failure<List<Investigation>>(DomainErrors.InvestigationError.NullOrEmpty);
        }
    }

    public async Task<Result<IEnumerable<Investigation>>> GetByHazardCodeAsync(string hazardCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(hazardCode))
            {
                return Result<IEnumerable<Investigation>>.Failure<IEnumerable<Investigation>>(DomainErrors.InvestigationError.HazardCodeRequired);
            }

            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_Investigation_GetByHazardCode} HazardCode:{hazardCode}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Investigation_GetByHazardCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationHazardCode, hazardCode));

            List<Investigation> response = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var investigation = Mappers.MapToInvestigation(reader);
                    response.Add(investigation);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<Investigation>>.Success(response.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<IEnumerable<Investigation>>.Failure<IEnumerable<Investigation>>(DomainErrors.InvestigationError.NotFound);
        }
    }

    public async Task<Result<IEnumerable<Investigation>>> GetByInvestigatorAsync(string investigatorId, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(investigatorId))
            {
                return Result<IEnumerable<Investigation>>.Failure<IEnumerable<Investigation>>(DomainErrors.InvestigationError.InvestigatorRequired);
            }

            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_Investigation_GetByInvestigator} InvestigatorId:{investigatorId}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Investigation_GetByInvestigator, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationAssignedInvestigatorId, investigatorId));

            List<Investigation> response = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var investigation = Mappers.MapToInvestigation(reader);
                    response.Add(investigation);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<Investigation>>.Success(response.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<IEnumerable<Investigation>>.Failure<IEnumerable<Investigation>>(DomainErrors.InvestigationError.NotFound);
        }
    }

    public async Task<Result<IEnumerable<Investigation>>> GetByStatusAsync(InvestigationStatus status, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_Investigation_GetByStatus} Status:{status}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Investigation_GetByStatus, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationStatus, status.ToString()));

            List<Investigation> response = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var investigation = Mappers.MapToInvestigation(reader);
                    response.Add(investigation);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<Investigation>>.Success(response.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<IEnumerable<Investigation>>.Failure<IEnumerable<Investigation>>(DomainErrors.InvestigationError.NotFound);
        }
    }

    public async Task<Result<Investigation>> UpdateInvestigationAsync(Investigation investigation, CancellationToken ct = default)
    {
        try
        {
            if (investigation is null)
            {
                return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.pr_Investigation_Update} Code:{investigation.Code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Investigation_Update, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Core properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationCode, investigation.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationReportCode, investigation.ReportCode ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationHazardCode, investigation.HazardCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationNotes, investigation.InvestigationNotes ?? (object)DBNull.Value));

            // Management properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationAssignedInvestigatorId, investigation.AssignedInvestigatorId));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationStatus, investigation.Status.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationCompletedDate, investigation.CompletedDate ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationPlan, investigation.InvestigationPlan ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationObjectives, investigation.InvestigationObjectives ?? (object)DBNull.Value));

            // Decision properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationDecisionType, investigation.DecisionType ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationDecisionRationale, investigation.DecisionRationale ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationDecisionMaker, investigation.DecisionMaker ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationDecisionDate, investigation.DecisionDate ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationNextSteps, investigation.NextSteps ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationReferralDetails, investigation.ReferralDetails ?? (object)DBNull.Value));

            // Audit properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, investigation.UpdatedBy ?? "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, investigation.UpdatedDate ?? DateTime.UtcNow));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return await GetInvestigationByCodeAsync(investigation.Code, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteInvestigationAsync(InvestigationID id, CancellationToken ct = default)
    {
        try
        {
            if (id is null)
            {
                return Result<bool>.Failure<bool>(DomainErrors.InvestigationError.NullOrEmpty);
            }

            _logger.LogInfrastructureDeleteItem($"{_logheader} {StoredProcs.pr_Investigation_Delete} ID:{id}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Investigation_Delete, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, id.Value));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureDeleteItemError($"{_logheader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.InvestigationError.DeleteFailed);
        }
    }

    public async Task<Result<bool>> UpdateStatusAsync(string investigationCode, InvestigationStatus status, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(investigationCode))
            {
                return Result<bool>.Failure<bool>(DomainErrors.InvestigationError.CodeRequired);
            }

            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.pr_Investigation_UpdateStatus} Code:{investigationCode}, Status:{status}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Investigation_UpdateStatus, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationCode, investigationCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationStatus, status.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, DateTime.UtcNow));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.InvestigationError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> RecordDecisionAsync(string investigationCode, string decisionType, string rationale,
        string decisionMaker, string? nextSteps = null, string? referralDetails = null, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(investigationCode))
            {
                return Result<bool>.Failure<bool>(DomainErrors.InvestigationError.CodeRequired);
            }

            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.pr_Investigation_RecordDecision} Code:{investigationCode}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Investigation_RecordDecision, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationCode, investigationCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationDecisionType, decisionType));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationDecisionRationale, rationale));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationDecisionMaker, decisionMaker));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationDecisionDate, DateTime.UtcNow));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationNextSteps, nextSteps ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationReferralDetails, referralDetails ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, DateTime.UtcNow));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.InvestigationError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> CompleteInvestigationAsync(string investigationCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(investigationCode))
            {
                return Result<bool>.Failure<bool>(DomainErrors.InvestigationError.CodeRequired);
            }

            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.pr_Investigation_Complete} Code:{investigationCode}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Investigation_Complete, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationCode, investigationCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationStatus, InvestigationStatus.InvestigationComplete.ToString()));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationCompletedDate, DateTime.UtcNow));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, DateTime.UtcNow));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.InvestigationError.UpdateFailed);
        }
    }
}
