using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Domain.ValueObjects;
using SMS_Infrastructure.Common;
using SMS_Infrastructure.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace SMS_Infrastructure.Repositories;

public sealed class MitigationAssignmentRepository : BaseRepository<MitigationAssignmentRepository, MitigationAssignment>
{
    private readonly ILogger<MitigationAssignmentRepository> _logger;
    private readonly string _logheader;
    private readonly string _connectionString;

    public MitigationAssignmentRepository(ILogger<MitigationAssignmentRepository> logger, ILogSupport logsupport, IConfiguration configuration)
        : base(logger, logsupport, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} MitigationAssignment Repository Initialized");
    }

    public async Task<Result<MitigationAssignment>> CreateMitigationAssignmentAsync(MitigationAssignment mitigationAssignment, CancellationToken ct = default)
    {
        try
        {
            if (mitigationAssignment is null)
            {
                return Result<MitigationAssignment>.Failure<MitigationAssignment>(DomainErrors.MitigationAssignmentError.NullOrEmpty);
            }

            _logger.LogInfrastructurePostItem($"{_logheader} {StoredProcs.pr_MitigationAssignment_Insert} Code:{mitigationAssignment.Code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_MitigationAssignment_Insert, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationAssignmentCode, mitigationAssignment.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationAssignmentMitigationCode, mitigationAssignment.MitigationCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationAssignmentDepartmentCode, mitigationAssignment.DepartmentCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, DateTime.UtcNow));

            var outputParam = new SqlParameter("@pNewID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(outputParam);

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            int newId = (int)outputParam.Value;
            MitigationAssignmentID mitigationAssignmentId = new(newId.ToString());

            return await GetMitigationAssignmentByIdAsync(mitigationAssignmentId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logheader} {ex.Message}", null);
            return Result<MitigationAssignment>.Failure<MitigationAssignment>(DomainErrors.MitigationAssignmentError.CreateFailed);
        }
    }

    public async Task<Result<MitigationAssignment>> GetMitigationAssignmentByIdAsync(MitigationAssignmentID id, CancellationToken ct = default)
    {
        try
        {
            if (id is null)
            {
                return Result<MitigationAssignment>.Failure<MitigationAssignment>(DomainErrors.MitigationAssignmentError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItem($"{_logheader} {StoredProcs.pr_MitigationAssignment_GetById} {id}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_MitigationAssignment_GetById, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, Convert.ToInt32(id.Value)));

            MitigationAssignment? response = null;

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    response = Mappers.MapToMitigationAssignment(reader);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            if (response is not null)
            {
                return Result<MitigationAssignment>.Success(response);
            }
            else
            {
                return Result<MitigationAssignment>.Failure<MitigationAssignment>(DomainErrors.MitigationAssignmentError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logheader} {ex.Message}", null);
            return Result<MitigationAssignment>.Failure<MitigationAssignment>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<List<MitigationAssignment>>> GetAllMitigationAssignmentsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_MitigationAssignment_GetAll}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_MitigationAssignment_GetAll, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            List<MitigationAssignment> response = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var mitigationAssignment = Mappers.MapToMitigationAssignment(reader);
                    response.Add(mitigationAssignment);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<List<MitigationAssignment>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<List<MitigationAssignment>>.Failure<List<MitigationAssignment>>(DomainErrors.MitigationAssignmentError.NullOrEmpty);
        }
    }

    public async Task<Result<MitigationAssignment>> UpdateMitigationAssignmentAsync(MitigationAssignment mitigationAssignment, CancellationToken ct = default)
    {
        try
        {
            if (mitigationAssignment is null)
            {
                return Result<MitigationAssignment>.Failure<MitigationAssignment>(DomainErrors.MitigationAssignmentError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.pr_MitigationAssignment_Update} ID:{mitigationAssignment.Id}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_MitigationAssignment_Update, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, Convert.ToInt32(mitigationAssignment.Id.Value)));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationAssignmentCode, mitigationAssignment.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationAssignmentMitigationCode, mitigationAssignment.MitigationCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationAssignmentDepartmentCode, mitigationAssignment.DepartmentCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, DateTime.UtcNow));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return await GetMitigationAssignmentByIdAsync((MitigationAssignmentID)mitigationAssignment.Id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<MitigationAssignment>.Failure<MitigationAssignment>(DomainErrors.MitigationAssignmentError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteMitigationAssignmentAsync(MitigationAssignmentID id, CancellationToken ct = default)
    {
        try
        {
            if (id is null)
            {
                return Result<bool>.Failure<bool>(DomainErrors.MitigationAssignmentError.NullOrEmpty);
            }

            _logger.LogInfrastructureDeleteItem($"{_logheader} {StoredProcs.pr_MitigationAssignment_Delete} ID:{id}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_MitigationAssignment_Delete, sql)
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
            return Result<bool>.Failure<bool>(DomainErrors.MitigationAssignmentError.DeleteFailed);
        }
    }
}