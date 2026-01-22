using SMS_Domain.Errors;

using SMS_Infrastructure.Interfaces;

namespace SMS_Infrastructure.Persistence;

public sealed class HazardReportTrackingRepository : BaseRepository<HazardReportTrackingRepository, HazardReportTracking>
{
    private readonly ILogger<HazardReportTrackingRepository> _logger;
    private readonly string _logheader;
    private readonly string _connectionString;

    public HazardReportTrackingRepository(ILogger<HazardReportTrackingRepository> logger, ILogSupport logsupport, IConfiguration configuration)
        : base(logger, logsupport, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} HazardReportTracking Repository Initialized");
    }

    public async Task<Result<HazardReportTracking>> CreateHazardReportTrackingAsync(HazardReportTracking hazardReportTracking, CancellationToken ct = default)
    {
        try
        {
            if (hazardReportTracking is null)
            {
                return Result<HazardReportTracking>.Failure<HazardReportTracking>(DomainErrors.HazardReportTrackingError.NullOrEmpty);
            }

            _logger.LogInfrastructurePostItem($"{_logheader} {StoredProcs.pr_HazardReportTracking_Insert} TrackingCode:{hazardReportTracking.TrackingCode}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_HazardReportTracking_Insert, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Core Properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardCode, hazardReportTracking.HazardCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmReportCode, hazardReportTracking.ReportCode ?? (object)DBNull.Value));

            // Audit Fields
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, hazardReportTracking.CreatedBy ?? "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, hazardReportTracking.CreatedDate));

            var newID = new SqlParameter(ParameterNames.pmNewID, SqlDbType.Int) { Direction = ParameterDirection.Output };
            var newCode = new SqlParameter("@pNewHazardReportTrackingCode", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };

            cmd.Parameters.Add(newID);
            cmd.Parameters.Add(newCode);

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            int newIdValue = (int)newID.Value;
            string newCodeValue = Convert.ToString(newCode.Value) ?? string.Empty;
            HazardReportTrackingID trackingId = new(newCodeValue);

            return await GetHazardReportTrackingByCodeAsync(trackingId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logheader} {ex.Message}", null);
            return Result<HazardReportTracking>.Failure<HazardReportTracking>(DomainErrors.HazardReportTrackingError.CreateFailed);
        }
    }

    public async Task<Result<HazardReportTracking>> GetHazardReportTrackingByCodeAsync(HazardReportTrackingID id, CancellationToken ct = default)
    {
        try
        {
            if (id is null)
            {
                return Result<HazardReportTracking>.Failure<HazardReportTracking>(DomainErrors.HazardReportTrackingError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItem($"{_logheader} {StoredProcs.pr_HazardReportTracking_GetByTrackingCode} {id.Value}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_HazardReportTracking_GetByTrackingCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardReportTrackingTrackingCode, id.Value));

            HazardReportTracking? response = null;

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    response = Mappers.MapToHazardReportTracking(reader);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            if (response is not null)
            {
                return Result<HazardReportTracking>.Success(response);
            }
            else
            {
                return Result<HazardReportTracking>.Failure<HazardReportTracking>(DomainErrors.HazardReportTrackingError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logheader} {ex.Message}", null);
            return Result<HazardReportTracking>.Failure<HazardReportTracking>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<HazardReportTracking>> GetHazardReportTrackingByTrackingCodeAsync(string trackingCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(trackingCode))
            {
                return Result<HazardReportTracking>.Failure<HazardReportTracking>(DomainErrors.HazardReportTrackingError.TrackingCodeRequired);
            }

            _logger.LogInfrastructureGetItem($"{_logheader} {StoredProcs.pr_HazardReportTracking_GetByTrackingCode} TrackingCode:{trackingCode}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_HazardReportTracking_GetByTrackingCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardReportTrackingTrackingCode, trackingCode));

            HazardReportTracking? response = null;

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    response = Mappers.MapToHazardReportTracking(reader);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            if (response is not null)
            {
                return Result<HazardReportTracking>.Success(response);
            }
            else
            {
                return Result<HazardReportTracking>.Failure<HazardReportTracking>(DomainErrors.HazardReportTrackingError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logheader} {ex.Message}", null);
            return Result<HazardReportTracking>.Failure<HazardReportTracking>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<List<HazardReportTracking>>> GetAllHazardReportTrackingAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_HazardReportTracking_GetAll}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_HazardReportTracking_GetAll, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            List<HazardReportTracking> response = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var tracking = Mappers.MapToHazardReportTracking(reader);
                    response.Add(tracking);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<List<HazardReportTracking>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<List<HazardReportTracking>>.Failure<List<HazardReportTracking>>(DomainErrors.HazardReportTrackingError.NullOrEmpty);
        }
    }

    public async Task<Result<List<HazardReportTracking>>> GetHazardReportTrackingByHazardCodeAsync(string hazardCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(hazardCode))
            {
                return Result<List<HazardReportTracking>>.Failure<List<HazardReportTracking>>(DomainErrors.HazardReportTrackingError.HazardCodeRequired);
            }

            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_HazardReportTracking_GetByHazardCode} HazardCode:{hazardCode}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_HazardReportTracking_GetByHazardCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardReportTrackingHazardCode, hazardCode));

            List<HazardReportTracking> response = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var tracking = Mappers.MapToHazardReportTracking(reader);
                    response.Add(tracking);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<List<HazardReportTracking>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<List<HazardReportTracking>>.Failure<List<HazardReportTracking>>(DomainErrors.HazardReportTrackingError.NullOrEmpty);
        }
    }

    public async Task<Result<List<HazardReportTracking>>> GetHazardReportTrackingByReportCodeAsync(string reportCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(reportCode))
            {
                return Result<List<HazardReportTracking>>.Failure<List<HazardReportTracking>>(DomainErrors.HazardReportTrackingError.ReportCodeRequired);
            }

            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_HazardReportTracking_GetByReportCode} ReportCode:{reportCode}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_HazardReportTracking_GetByReportCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardReportTrackingReportCode, reportCode));

            List<HazardReportTracking> response = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var tracking = Mappers.MapToHazardReportTracking(reader);
                    response.Add(tracking);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<List<HazardReportTracking>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<List<HazardReportTracking>>.Failure<List<HazardReportTracking>>(DomainErrors.HazardReportTrackingError.NullOrEmpty);
        }
    }

    public async Task<Result<HazardReportTracking>> UpdateHazardReportTrackingAsync(HazardReportTracking hazardReportTracking, CancellationToken ct = default)
    {
        try
        {
            if (hazardReportTracking is null)
            {
                return Result<HazardReportTracking>.Failure<HazardReportTracking>(DomainErrors.HazardReportTrackingError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.pr_HazardReportTracking_Update} TrackingCode:{hazardReportTracking.TrackingCode}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_HazardReportTracking_Update, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Core Properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardReportTrackingTrackingCode, hazardReportTracking.TrackingCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardCode, hazardReportTracking.HazardCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmReportCode, hazardReportTracking.ReportCode ?? (object)DBNull.Value));

            // Audit Fields
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, hazardReportTracking.UpdatedBy ?? "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, hazardReportTracking.UpdatedDate ?? DateTime.UtcNow));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            var trackingId = new HazardReportTrackingID(hazardReportTracking.TrackingCode);
            return await GetHazardReportTrackingByCodeAsync(trackingId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<HazardReportTracking>.Failure<HazardReportTracking>(DomainErrors.HazardReportTrackingError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteHazardReportTrackingAsync(HazardReportTrackingID id, CancellationToken ct = default)
    {
        try
        {
            if (id is null)
            {
                return Result<bool>.Failure<bool>(DomainErrors.HazardReportTrackingError.NullOrEmpty);
            }

            _logger.LogInfrastructureDeleteItem($"{_logheader} {StoredProcs.pr_HazardReportTracking_Delete} TrackingCode:{id.Value}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_HazardReportTracking_Delete, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardReportTrackingTrackingCode, id.Value));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureDeleteItemError($"{_logheader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.HazardReportTrackingError.DeleteFailed);
        }
    }
}