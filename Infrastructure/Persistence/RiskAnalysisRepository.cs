using SMS_Domain.Errors;

using SMS_Infrastructure.Interfaces;

namespace SMS_Infrastructure.Persistence;

public sealed class RiskAnalysisRepository : BaseRepository<RiskAnalysisRepository, RiskAnalysis>
{
    private readonly ILogger<RiskAnalysisRepository> _logger;
    private readonly string _logheader;
    private readonly string _connectionString;

    public RiskAnalysisRepository(ILogger<RiskAnalysisRepository> logger, ILogSupport logsupport, IConfiguration configuration)
        : base(logger, logsupport, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} RiskAnalysis Repository Initialized");
    }

    public async Task<Result<RiskAnalysis>> CreateRiskAnalysisAsync(RiskAnalysis riskAnalysis, CancellationToken ct = default)
    {
        try
        {
            if (riskAnalysis is null)
            {
                return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.RiskAnalysisError.NullOrEmpty);
            }

            _logger.LogInfrastructurePostItem($"{_logheader} {StoredProcs.pr_RiskAnalysis_Insert} Code:{riskAnalysis.Code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_RiskAnalysis_Insert, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAnalysisCode, riskAnalysis.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAnalysisHazardCode, riskAnalysis.HazardCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAnalysisRiskAssessmentCode, riskAnalysis.RiskAssessmentCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAnalysisWorstCredibleOutcome, riskAnalysis.WorstCredibleOutcome));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAnalysisRootCause, riskAnalysis.RootCause));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAnalysisAdditionalComments, riskAnalysis.AdditionalComments));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, DateTime.UtcNow));

            var newID = new SqlParameter("@pNewID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var newCode = new SqlParameter("@pNewRiskAnalysisCode", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(newID);
            cmd.Parameters.Add(newCode);

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            int newIdValue = (int)newID.Value;
            string newCodeValue = Convert.ToString(newCode.Value) ?? string.Empty;
            RiskAnalysisID riskAnalysisId = new(newCodeValue);

            return await GetRiskAnalysisByIdAsync(riskAnalysisId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logheader} {ex.Message}", null);
            return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.RiskAnalysisError.CreateFailed);
        }
    }

    public async Task<Result<RiskAnalysis>> GetRiskAnalysisByIdAsync(RiskAnalysisID id, CancellationToken ct = default)
    {
        try
        {
            if (id is null)
            {
                return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.RiskAnalysisError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItem($"{_logheader} {StoredProcs.pr_RiskAnalysis_GetById} {id}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_RiskAnalysis_GetById, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAnalysisCode, id.Value));

            RiskAnalysis? response = null;

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    response = Mappers.MapToRiskAnalysis(reader);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            if (response is not null)
            {
                return Result<RiskAnalysis>.Success(response);
            }
            else
            {
                return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.RiskAnalysisError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logheader} {ex.Message}", null);
            return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<RiskAnalysis>> GetRiskAnalysisByHazardIdAsync(HazardID id, CancellationToken ct = default)
    {
        try
        {
            if (id is null)
            {
                return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.RiskAnalysisError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItem($"{_logheader} {StoredProcs.pr_RiskAnalysis_GetById} {id}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_RiskAnalysis_GetByHazardId, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardId, id.Value));

            RiskAnalysis? response = null;

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    response = Mappers.MapToRiskAnalysis(reader);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            if (response is not null)
            {
                return Result<RiskAnalysis>.Success(response);
            }
            else
            {
                return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.RiskAnalysisError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logheader} {ex.Message}", null);
            return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }



    public async Task<Result<List<RiskAnalysis>>> GetAllRiskAnalysisAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_RiskAnalysis_GetAll}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_RiskAnalysis_GetAll, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            List<RiskAnalysis> response = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var riskAnalysis = Mappers.MapToRiskAnalysis(reader);
                    response.Add(riskAnalysis);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<List<RiskAnalysis>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<List<RiskAnalysis>>.Failure<List<RiskAnalysis>>(DomainErrors.RiskAnalysisError.NullOrEmpty);
        }
    }

    public async Task<Result<RiskAnalysis>> UpdateRiskAnalysisAsync(RiskAnalysis riskAnalysis, CancellationToken ct = default)
    {
        try
        {
            if (riskAnalysis is null)
            {
                return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.RiskAnalysisError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.pr_RiskAnalysis_Update} ID:{riskAnalysis.Id}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_RiskAnalysis_Update, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAnalysisCode, riskAnalysis.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAnalysisHazardCode, riskAnalysis.HazardCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAnalysisRiskAssessmentCode, riskAnalysis.RiskAssessmentCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAnalysisWorstCredibleOutcome, riskAnalysis.WorstCredibleOutcome));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAnalysisRootCause, riskAnalysis.RootCause));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAnalysisAdditionalComments, riskAnalysis.AdditionalComments));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, DateTime.UtcNow));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return await GetRiskAnalysisByIdAsync((RiskAnalysisID)riskAnalysis.Id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.RiskAnalysisError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteRiskAnalysisAsync(RiskAnalysisID id, CancellationToken ct = default)
    {
        try
        {
            if (id is null)
            {
                return Result<bool>.Failure<bool>(DomainErrors.RiskAnalysisError.NullOrEmpty);
            }

            _logger.LogInfrastructureDeleteItem($"{_logheader} {StoredProcs.pr_RiskAnalysis_Delete} ID:{id}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_RiskAnalysis_Delete, sql)
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
            return Result<bool>.Failure<bool>(DomainErrors.RiskAnalysisError.DeleteFailed);
        }
    }
}