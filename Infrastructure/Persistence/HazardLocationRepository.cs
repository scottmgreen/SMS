using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Domain.ValueObjects;
using SMS_Infrastructure.Common;
using SMS_Infrastructure.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace SMS_Infrastructure.Persistence;

public sealed class HazardLocationRepository : BaseRepository<HazardLocationRepository, HazardLocation>, IHazardLocationRepository
{
    private readonly ILogger<HazardLocationRepository> _logger;
    private readonly string _logheader;
    private readonly string _connectionString;

    public HazardLocationRepository(ILogger<HazardLocationRepository> logger, ILogSupport logsupport, IConfiguration configuration)
        : base(logger, logsupport, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} HazardLocation Repository Initialized");
    }

    #region Interface Implementation

    public async Task<Result<HazardLocation>> GetByIdAsync(HazardLocationID id)
    {
        return await GetHazardLocationByIdAsync(id);
    }

    public async Task<Result<HazardLocation>> AddAsync(HazardLocation hazardLocation)
    {
        return await CreateHazardLocationAsync(hazardLocation);
    }

    public async Task<Result<bool>> UpdateAsync(HazardLocation hazardLocation)
    {
        var result = await UpdateHazardLocationAsync(hazardLocation);
        return result.IsSuccess ? Result<bool>.Success(true) : Result<bool>.Failure<bool>(result.Error);
    }

    public async Task<Result<bool>> DeleteAsync(HazardLocationID id)
    {
        return await DeleteHazardLocationAsync(id);
    }

    public async Task<Result<IEnumerable<HazardLocation>>> GetAllAsync()
    {
        var result = await GetAllHazardLocationsAsync();
        return result.IsSuccess 
            ? Result<IEnumerable<HazardLocation>>.Success(result.Value.AsEnumerable())
            : Result<IEnumerable<HazardLocation>>.Failure<IEnumerable<HazardLocation>>(result.Error);
    }

    public async Task<Result<IEnumerable<HazardLocation>>> GetByHazardCodeAsync(string hazardCode)
    {
        var result = await GetHazardLocationsByHazardCodeAsync(hazardCode);
        return result.IsSuccess 
            ? Result<IEnumerable<HazardLocation>>.Success(result.Value.AsEnumerable())
            : Result<IEnumerable<HazardLocation>>.Failure<IEnumerable<HazardLocation>>(result.Error);
    }

    #endregion

    #region CRUD Implementation Methods

    public async Task<Result<HazardLocation>> CreateHazardLocationAsync(HazardLocation hazardLocation, CancellationToken ct = default)
    {
        try
        {
            if (hazardLocation is null)
            {
                return Result<HazardLocation>.Failure<HazardLocation>(DomainErrors.HazardLocationError.NullOrEmpty);
            }

            _logger.LogInfrastructurePostItem($"{_logheader} {StoredProcs.pr_HazardLocation_Insert} Code:{hazardLocation.Code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_HazardLocation_Insert, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardLocationCode, hazardLocation.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardLocationHazardCode, hazardLocation.HazardCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardLocationLatitude, hazardLocation.Latitude));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardLocationLongitude, hazardLocation.Longitude));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardLocationDescription, hazardLocation.Description ?? (object)DBNull.Value));
            //cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardLocationDateSelected, hazardLocation.DateSelected));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, hazardLocation.CreatedBy ?? "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, DateTime.UtcNow));

            var newID = new SqlParameter("@pNewID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var newCode = new SqlParameter("@pNewHazardLocationCode", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(newID);
            cmd.Parameters.Add(newCode);

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            int newIdValue = (int)newID.Value;
            string newCodeValue = Convert.ToString(newCode.Value) ?? string.Empty;
            HazardLocationID hazardLocationId = new(newCodeValue);

            return await GetHazardLocationByIdAsync(hazardLocationId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logheader} {ex.Message}", null);
            return Result<HazardLocation>.Failure<HazardLocation>(DomainErrors.HazardLocationError.CreateFailed);
        }
    }

    public async Task<Result<HazardLocation>> GetHazardLocationByIdAsync(HazardLocationID id, CancellationToken ct = default)
    {
        try
        {
            if (id is null)
            {
                return Result<HazardLocation>.Failure<HazardLocation>(DomainErrors.HazardLocationError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItem($"{_logheader} {StoredProcs.pr_HazardLocation_GetById} {id}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_HazardLocation_GetById, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, id.Value));

            HazardLocation? response = null;

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    response = Mappers.MapToHazardLocation(reader);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            if (response is not null)
            {
                return Result<HazardLocation>.Success(response);
            }
            else
            {
                return Result<HazardLocation>.Failure<HazardLocation>(DomainErrors.HazardLocationError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logheader} {ex.Message}", null);
            return Result<HazardLocation>.Failure<HazardLocation>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<List<HazardLocation>>> GetAllHazardLocationsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_HazardLocation_GetAll}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_HazardLocation_GetAll, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            List<HazardLocation> response = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var hazardLocation = Mappers.MapToHazardLocation(reader);
                    response.Add(hazardLocation);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<List<HazardLocation>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<List<HazardLocation>>.Failure<List<HazardLocation>>(DomainErrors.HazardLocationError.NullOrEmpty);
        }
    }

    public async Task<Result<List<HazardLocation>>> GetHazardLocationsByHazardCodeAsync(string hazardCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(hazardCode))
            {
                return Result<List<HazardLocation>>.Failure<List<HazardLocation>>(DomainErrors.HazardLocationError.InvalidCode);
            }

            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_HazardLocation_GetByHazardCode} HazardCode:{hazardCode}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_HazardLocation_GetByHazardCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardLocationHazardCode, hazardCode));

            List<HazardLocation> response = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var hazardLocation = Mappers.MapToHazardLocation(reader);
                    response.Add(hazardLocation);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<List<HazardLocation>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<List<HazardLocation>>.Failure<List<HazardLocation>>(DomainErrors.HazardLocationError.NullOrEmpty);
        }
    }

    public async Task<Result<HazardLocation>> UpdateHazardLocationAsync(HazardLocation hazardLocation, CancellationToken ct = default)
    {
        try
        {
            if (hazardLocation is null)
            {
                return Result<HazardLocation>.Failure<HazardLocation>(DomainErrors.HazardLocationError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.pr_HazardLocation_Update} ID:{hazardLocation.Id}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_HazardLocation_Update, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, hazardLocation.Id.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardLocationCode, hazardLocation.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardLocationHazardCode, hazardLocation.HazardCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardLocationLatitude, hazardLocation.Latitude));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardLocationLongitude, hazardLocation.Longitude));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardLocationDescription, hazardLocation.Description ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, hazardLocation.UpdatedBy ?? "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, DateTime.UtcNow));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return await GetHazardLocationByIdAsync((HazardLocationID)hazardLocation.Id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<HazardLocation>.Failure<HazardLocation>(DomainErrors.HazardLocationError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteHazardLocationAsync(HazardLocationID id, CancellationToken ct = default)
    {
        try
        {
            if (id is null)
            {
                return Result<bool>.Failure<bool>(DomainErrors.HazardLocationError.NullOrEmpty);
            }

            _logger.LogInfrastructureDeleteItem($"{_logheader} {StoredProcs.pr_HazardLocation_Delete} ID:{id}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_HazardLocation_Delete, sql)
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
            return Result<bool>.Failure<bool>(DomainErrors.HazardLocationError.DeleteFailed);
        }
    }

    #endregion
}