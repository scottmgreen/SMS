using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Domain.ValueObjects;
using SMS_Infrastructure.Common;
using SMS_Infrastructure.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace SMS_Infrastructure.Repositories;

public sealed class InvestigationRepository : BaseRepository<InvestigationRepository, Investigation>
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
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} Investigation Repository Initialized");
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

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationCode, investigation.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationReportCode, investigation.ReportCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationNotes, investigation.InvestigationNotes));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, DateTime.UtcNow));

            var outputParam = new SqlParameter("@pNewID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(outputParam);

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            int newId = (int)outputParam.Value;
            InvestigationID investigationId = new(newId.ToString());

            return await GetInvestigationByIdAsync(investigationId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logheader} {ex.Message}", null);
            return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.CreateFailed);
        }
    }

    public async Task<Result<Investigation>> GetInvestigationByIdAsync(InvestigationID id, CancellationToken ct = default)
    {
        try
        {
            if (id is null)
            {
                return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItem($"{_logheader} {StoredProcs.pr_Investigation_GetById} {id}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Investigation_GetById, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, Convert.ToInt32(id.Value)));

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

    public async Task<Result<Investigation>> UpdateInvestigationAsync(Investigation investigation, CancellationToken ct = default)
    {
        try
        {
            if (investigation is null)
            {
                return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.pr_Investigation_Update} ID:{investigation.Id}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Investigation_Update, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, Convert.ToInt32(investigation.Id.Value)));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationCode, investigation.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationReportCode, investigation.ReportCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmInvestigationNotes, investigation.InvestigationNotes));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, DateTime.UtcNow));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return await GetInvestigationByIdAsync((InvestigationID)investigation.Id, ct).ConfigureAwait(false);
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

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, Convert.ToInt32(id.Value)));

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
}