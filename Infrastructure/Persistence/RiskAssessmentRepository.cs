using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Domain.ValueObjects;
using SMS_Infrastructure.Common;
using SMS_Infrastructure.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace SMS_Infrastructure.Repositories;

public sealed class RiskAssessmentRepository : BaseRepository<RiskAssessmentRepository, RiskAssessment>
{
    private readonly ILogger<RiskAssessmentRepository> _logger;
    private readonly string _logheader;
    private readonly string _connectionString;

    public RiskAssessmentRepository(ILogger<RiskAssessmentRepository> logger, ILogSupport logsupport, IConfiguration configuration)
        : base(logger, logsupport, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} RiskAssessment Repository Initialized");
    }

    public async Task<Result<RiskAssessment>> CreateRiskAssessmentAsync(RiskAssessment riskAssessment, CancellationToken ct = default)
    {
        try
        {
            if (riskAssessment is null)
            {
                return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.NullOrEmpty);
            }

            _logger.LogInfrastructurePostItem($"{_logheader} {StoredProcs.pr_RiskAssessment_Insert} Code:{riskAssessment.Code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_RiskAssessment_Insert, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentCode, riskAssessment.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentName, riskAssessment.Name));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentDescription, riskAssessment.Description));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentHazardCode, riskAssessment.HazardCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentType, riskAssessment.AssessmentType));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentStatus, riskAssessment.Status));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentStage, riskAssessment.Stage));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, DateTime.UtcNow));

            var newID = new SqlParameter("@pNewID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var newCode = new SqlParameter("@pNewRiskAssessmentCode", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(newID);
            cmd.Parameters.Add(newCode);

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            int newIdValue = (int)newID.Value;
            string newCodeValue = Convert.ToString(newCode.Value) ?? string.Empty;
            RiskAssessmentID riskAssessmentId = new RiskAssessmentID(newCodeValue);

            return await GetRiskAssessmentByIdAsync(riskAssessmentId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create RiskAssessment: {Code}", riskAssessment.Code);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.CreateFailed);
        }
    }

    public async Task<Result<RiskAssessment>> GetRiskAssessmentByIdAsync(RiskAssessmentID riskAssessmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving RiskAssessment by ID: {Id}", riskAssessmentId);

            var parameters = new DynamicParameters();
            parameters.Add("@RiskAssessmentID", riskAssessmentId.Value);

            var riskAssessment = await _connectionFactory.GetConnection().QuerySingleOrDefaultAsync<RiskAssessment>(
                "GetRiskAssessmentById", 
                parameters, 
                commandType: CommandType.StoredProcedure);

            if (riskAssessment == null)
            {
                _logger.LogWarning("RiskAssessment not found with ID: {Id}", riskAssessmentId);
                return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.NotFound);
            }

            _logger.LogInformation("Successfully retrieved RiskAssessment: {Id}", riskAssessmentId);
            return Result<RiskAssessment>.Success(riskAssessment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve RiskAssessment by ID: {Id}", riskAssessmentId);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.NotFound);
        }
    }

    public async Task<Result<List<RiskAssessment>>> GetAllRiskAssessmentsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_RiskAssessment_GetAll}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_RiskAssessment_GetAll, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            List<RiskAssessment> response = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var riskAssessment = Mappers.MapToRiskAssessment(reader);
                    response.Add(riskAssessment);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<List<RiskAssessment>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<List<RiskAssessment>>.Failure<List<RiskAssessment>>(DomainErrors.RiskAssessmentError.NullOrEmpty);
        }
    }

    public async Task<Result<RiskAssessment>> UpdateRiskAssessmentAsync(RiskAssessment riskAssessment, CancellationToken ct = default)
    {
        try
        {
            if (riskAssessment is null)
            {
                return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.pr_RiskAssessment_Update} ID:{riskAssessment.Id}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_RiskAssessment_Update, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, riskAssessment.Id.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentCode, riskAssessment.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentName, riskAssessment.Name));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentDescription, riskAssessment.Description));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentHazardCode, riskAssessment.HazardCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentType, riskAssessment.AssessmentType));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentStatus, riskAssessment.Status));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentStage, riskAssessment.Stage));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, DateTime.UtcNow));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return await GetRiskAssessmentByIdAsync(new RiskAssessmentID(riskAssessment.Id.Value), ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteRiskAssessmentAsync(RiskAssessmentID id, CancellationToken ct = default)
    {
        try
        {
            if (id is null)
            {
                return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.NullOrEmpty);
            }

            _logger.LogInfrastructureDeleteItem($"{_logheader} {StoredProcs.pr_RiskAssessment_Delete} ID:{id}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_RiskAssessment_Delete, sql)
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
            return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.DeleteFailed);
        }
    }
}