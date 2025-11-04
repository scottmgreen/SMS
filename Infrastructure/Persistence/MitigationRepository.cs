using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Domain.ValueObjects;
using SMS_Infrastructure.Common;
using SMS_Infrastructure.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace SMS_Infrastructure.Repositories;

public sealed class MitigationRepository : BaseRepository<MitigationRepository, Mitigation>
{
    private readonly ILogger<MitigationRepository> _logger;
    private readonly string _logheader;
    private readonly string _connectionString;

    public MitigationRepository(ILogger<MitigationRepository> logger, ILogSupport logsupport, IConfiguration configuration)
        : base(logger, logsupport, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} Mitigation Repository Initialized");
    }

    public async Task<Result<Mitigation>> CreateMitigationAsync(Mitigation mitigation, CancellationToken ct = default)
    {
        try
        {
            if (mitigation is null)
            {
                return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.NullOrEmpty);
            }

            _logger.LogInfrastructurePostItem($"{_logheader} {StoredProcs.pr_Mitigation_Insert} Code:{mitigation.Code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Mitigation_Insert, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationCode, mitigation.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationHazardCode, mitigation.HazardCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, DateTime.UtcNow));

            var outputParam = new SqlParameter("@pNewID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(outputParam);

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            int newId = (int)outputParam.Value;
            MitigationID mitigationId = new(newId.ToString());

            return await GetMitigationByIdAsync(mitigationId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logheader} {ex.Message}", null);
            return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.CreateFailed);
        }
    }

    public async Task<Result<Mitigation>> GetMitigationByIdAsync(MitigationID id, CancellationToken ct = default)
    {
        try
        {
            if (id is null)
            {
                return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItem($"{_logheader} {StoredProcs.pr_Mitigation_GetById} {id}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Mitigation_GetById, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, Convert.ToInt32(id.Value)));

            Mitigation? response = null;

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    response = Mappers.MapToMitigation(reader);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            if (response is not null)
            {
                return Result<Mitigation>.Success(response);
            }
            else
            {
                return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logheader} {ex.Message}", null);
            return Result<Mitigation>.Failure<Mitigation>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<List<Mitigation>>> GetAllMitigationsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_Mitigation_GetAll}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Mitigation_GetAll, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            List<Mitigation> response = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var mitigation = Mappers.MapToMitigation(reader);
                    response.Add(mitigation);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<List<Mitigation>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<List<Mitigation>>.Failure<List<Mitigation>>(DomainErrors.MitigationError.NullOrEmpty);
        }
    }

    public async Task<Result<Mitigation>> UpdateMitigationAsync(Mitigation mitigation, CancellationToken ct = default)
    {
        try
        {
            if (mitigation is null)
            {
                return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.pr_Mitigation_Update} ID:{mitigation.Id}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Mitigation_Update, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, Convert.ToInt32(mitigation.Id.Value)));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationCode, mitigation.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationHazardCode, mitigation.HazardCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, DateTime.UtcNow));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return await GetMitigationByIdAsync((MitigationID)mitigation.Id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteMitigationAsync(MitigationID id, CancellationToken ct = default)
    {
        try
        {
            if (id is null)
            {
                return Result<bool>.Failure<bool>(DomainErrors.MitigationError.NullOrEmpty);
            }

            _logger.LogInfrastructureDeleteItem($"{_logheader} {StoredProcs.pr_Mitigation_Delete} ID:{id}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Mitigation_Delete, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, Convert.ToInt32(id.Value)));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureDeleteItemError($"{_logheader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.MitigationError.DeleteFailed);
        }
    }
}