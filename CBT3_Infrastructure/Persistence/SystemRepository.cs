using CBT3_Domain.Errors;
using CBT3_Domain.Interfaces;
using CBT3_Infrastructure.Common;
using CBT3_Infrastructure.Interfaces;

using Microsoft.FeatureManagement;

namespace CBT3_Infrastructure.Repositories;

public sealed class SystemRepository : BaseRepository<SystemRepository, Course>
{
    private readonly ILogger<SystemRepository> _logger;
    private readonly string _logheader;
    private readonly string _connectionString;
    private readonly IConfiguration _configuration;
    private readonly IFeatureManager _featureManager;

    public SystemRepository(ILogger<SystemRepository> logger, ILogSupport logsupport, IConfiguration configuration, IFeatureManager featureManager) : base(logger, logsupport, configuration)
    {
        _configuration = configuration;
        _featureManager = featureManager;
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader}");
    }

    public async Task<Result<bool>> AddAuditLogEntryAsync(AuditLogEntry auditLogEntry, CancellationToken ct = default)
    {

        try
        {
            _logger.LogInfrastructurePostItem($"{_logheader} {StoredProcs.cn_spAddAuditLogEntry}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.cn_spAddAuditLogEntry, sql)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };


            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUserID, auditLogEntry.UserID));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMessageType, auditLogEntry.MessageType));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSeverity, auditLogEntry.Severity));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmModule, auditLogEntry.Module));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmFunction, auditLogEntry.Function));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmDescription, auditLogEntry.Description));
            //cmd.Parameters.Add(DataAccess.Parameter("@ReturnVal", course.Id));


            await sql.OpenAsync(ct).ConfigureAwait(false);
            int rowsAffected = (int)await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            bool success = rowsAffected > 0;

            if (success)
                return Result<bool>.Success(true);
            else
                return Result<bool>.Failure<bool>(DomainErrors.SystemError.AuditLogEntryError);// No rows were affected
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logheader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.SystemError.AuditLogEntryError);
        }
    }

    public Task<Result<int>> GetAdminPasscodeAsync()
    {
        _logger.LogInfrastructureGetItem($"{_logheader} Get Admin Passcode", null);
        return Task.FromResult<Result<int>>(Convert.ToInt32(_configuration["AdminPasscode"]));
    }

    public async Task<Result<List<TrainingStation>>> GetTrainingStationsAsync(CancellationToken ct = default)
    {
        try
        {

            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.cn_spGetTrainingStations} ", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.cn_spGetTrainingStations, sql)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            List<TrainingStation> response = new();
                           
            await sql.OpenAsync(ct).ConfigureAwait(false);

            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    //Course course = new();
                    var trainingmachine = Mappers.MapToTrainingStation(reader);
                    response.Add(trainingmachine);

                }
            }

            await sql.CloseAsync().ConfigureAwait(false);

            if (response is not null)
            {
                return Result<Course>.Success(response);
            }
            else
            {
                return Result<List<TrainingStation>>.Failure<List<TrainingStation>>(DomainErrors.TraininingStationError.NullOrEmpty);
            }
        }

        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<List<TrainingStation>>.Failure<List<TrainingStation>>(DomainErrors.TraininingStationError.NullOrEmpty);

        }

    }

    public async Task<Result<TrainingSession>> GetTrainingSessionByHostNameAsync(TrainingSessionID sessionId, CancellationToken ct = default)
    {
        try
        {
            if (sessionId is null)
            {
                return Result<TrainingSession>.Failure<TrainingSession>(DomainErrors.TraininingSessionError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItem($"{_logheader} {StoredProcs.cn_spGetTrainingSession} {sessionId.Value}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.cn_spGetTrainingSession, sql)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };


            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmTrainingSessionId, sessionId.Value));

            TrainingSession? response = null;

            await sql.OpenAsync(ct).ConfigureAwait(false); ;
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    response = Mappers.MapToTrainingSession(reader);
                }
            }

            await sql.CloseAsync().ConfigureAwait(false);
            if (response is not null)
            {
                return Result<TrainingSession>.Success(response);
            }
            else
            {
                return Result<TrainingSession>.Failure<TrainingSession>(DomainErrors.TraininingSessionError.NullOrEmpty);
            }

        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logheader} {ex.Message}", null);
            return Result<TrainingSession>.Failure<TrainingSession>(DomainErrors.GeneralError.UnProcessableRequest);
        }

    }

    public async Task<Result<TrainingStation>> GetTrainingStationByHostNameAsync(string hostName, CancellationToken ct = default)
    {
        try
        {
            if (hostName is null)
            {
                return Result<TrainingStation>.Failure<TrainingStation>(DomainErrors.TraininingStationError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItem($"{_logheader} {StoredProcs.cn_spGetTrainingStation} {hostName}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.cn_spGetTrainingStation, sql)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHostName, hostName));

            TrainingStation? response = null;

            await sql.OpenAsync(ct).ConfigureAwait(false); ;
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    response = Mappers.MapToTrainingStation(reader);
                }
            }

            await sql.CloseAsync().ConfigureAwait(false);
            if (response is not null)
            {
                return Result<TrainingStation>.Success(response);
            }
            else
            {
                return Result<TrainingStation>.Failure<TrainingStation>(DomainErrors.TraininingStationError.NullOrEmpty);
            }

        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logheader} {ex.Message}", null);
            return Result<TrainingStation>.Failure<TrainingStation>(DomainErrors.GeneralError.UnProcessableRequest);
        }

    }

    public async Task<Result<TrainingStation>> UpdateTrainingStationAsync(ITrainingStation trainingmachine, CancellationToken ct = default)
    {

        try
        {
            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.cn_spUpdateTrainingStation} {trainingmachine.HostName}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.cn_spUpdateTrainingStation, sql)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHostName, trainingmachine.HostName));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmTrainingStationStatus, trainingmachine.Status));
            cmd.Parameters.Add(DataAccess.Parameter("@pUpdatedHostName ", trainingmachine.HostName));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            var result = cmd.ExecuteScalar();//ABSOLUTELY CAN't BE Async!
            string hostName = new(result.ToString());
            TrainingStation updatedMachine = GetTrainingStationByHostNameAsync(hostName, ct).Result.Value;
            await sql.CloseAsync().ConfigureAwait(false);

            if (updatedMachine is not null)
                return Result<TrainingStation>.Success(updatedMachine);
            else //this should never happen
                return Result<TrainingStation>.Failure<TrainingStation>(DomainErrors.TraininingStationError.NullOrEmpty); // No rows were affected

        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<TrainingStation>.Failure<TrainingStation>(DomainErrors.TraininingStationError.NullOrEmpty); // Or provide a more specific error code
        }
    }

    public async Task<Result<TrainingSession>> UpdateTrainingSessionAsync(TrainingSession trainingsession, CancellationToken ct = default)
    {

        try
        {
            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.cn_spUpdateTrainingSession} {trainingsession.HostName}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.cn_spUpdateTrainingSession, sql)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue(ParameterNames.pmTrainingSessionId, trainingsession.Id.Value);
            cmd.Parameters.AddWithValue(ParameterNames.pmHostName, trainingsession.HostName);
            cmd.Parameters.AddWithValue(ParameterNames.pmTraineeId, (object?)trainingsession.TraineeID?.Value ?? DBNull.Value);
            cmd.Parameters.AddWithValue(ParameterNames.pmCourseId, (object?)trainingsession.CourseID?.Value ?? DBNull.Value);
            cmd.Parameters.AddWithValue(ParameterNames.pmCourseStartedAt, trainingsession.CourseStartedAt);
            cmd.Parameters.AddWithValue(ParameterNames.pmCourseEndedAt, (object?)trainingsession.CourseEndedAt ?? DBNull.Value);

            // Output parameter
            var outputParam = new SqlParameter("@pUpdatedHostName", SqlDbType.VarChar, 50)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(outputParam);

            await sql.OpenAsync(ct).ConfigureAwait(false);
            cmd.ExecuteNonQuery();  // Use ExecuteNonQuery for stored procedures with OUTPUT params

            // Retrieve output value
            TrainingSessionID sessionId = new TrainingSessionID(outputParam.Value.ToString());
            TrainingSession updatedMachine = GetTrainingSessionByHostNameAsync(sessionId, ct).Result.Value;

            await sql.CloseAsync().ConfigureAwait(false);

            if (updatedMachine is not null)
                return Result<TrainingSession>.Success(updatedMachine);
            else //this should never happen
                return Result<TrainingSession>.Failure<TrainingSession>(DomainErrors.TraininingSessionError.NullOrEmpty); // No rows were affected

        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<TrainingSession>.Failure<TrainingSession>(DomainErrors.TraininingSessionError.NullOrEmpty); // Or provide a more specific error code
        }
    }

    public async Task<Result<TrainingSession>> StartTrainingSessionAsync(TrainingSession trainingsession, CancellationToken ct = default)
    {


        try
        {
            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.cn_spStartTrainingSession} {trainingsession.HostName}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.cn_spStartTrainingSession, sql)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmTrainingSessionId, trainingsession.Id.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHostName, trainingsession.HostName));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCourseStartedAt, trainingsession.CourseStartedAt));
            cmd.Parameters.Add(DataAccess.Parameter("@pUpdatedHostName ", trainingsession.HostName));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            var result = cmd.ExecuteNonQuery();//ABSOLUTELY CAN't BE Async!
            TrainingSessionID sessionId = new TrainingSessionID(result.ToString());
            TrainingSession updatedMachine = GetTrainingSessionByHostNameAsync(sessionId, ct).Result.Value;
            
            await sql.CloseAsync().ConfigureAwait(false);

            if (updatedMachine is not null)
                return Result<TrainingSession>.Success(updatedMachine);
            else //this should never happen
                return Result<TrainingSession>.Failure<TrainingSession>(DomainErrors.TraininingSessionError.NullOrEmpty); // No rows were affected

        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<TrainingSession>.Failure<TrainingSession>(DomainErrors.TraininingSessionError.NullOrEmpty); // Or provide a more specific error code
        }
    }

    public async Task<Result<TrainingSession>> FinishTrainingSessionAsync(TrainingSession trainingsession, CancellationToken ct = default)
    {

        try
        {
            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.cn_spFinishTrainingSession} {trainingsession.HostName}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.cn_spFinishTrainingSession, sql)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmTrainingSessionId, trainingsession.Id.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHostName, trainingsession.HostName));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmTraineeId, trainingsession.TraineeID.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCourseId, trainingsession.CourseID.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCourseStartedAt, trainingsession.CourseStartedAt));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCourseEndedAt, trainingsession.CourseEndedAt));
            cmd.Parameters.Add(DataAccess.Parameter("@pUpdatedHostName ", trainingsession.HostName));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            
            var result = cmd.ExecuteNonQuery();//ABSOLUTELY CAN't BE Async!
            TrainingSessionID sessionId = new TrainingSessionID(result.ToString());
            TrainingSession updatedMachine = GetTrainingSessionByHostNameAsync(sessionId, ct).Result.Value;
            await sql.CloseAsync().ConfigureAwait(false);

            if (updatedMachine is not null)
                return Result<TrainingSession>.Success(updatedMachine);
            else //this should never happen
                return Result<TrainingSession>.Failure<TrainingSession>(DomainErrors.TraininingSessionError.NullOrEmpty); // No rows were affected

        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<TrainingSession>.Failure<TrainingSession>(DomainErrors.TraininingSessionError.NullOrEmpty); // Or provide a more specific error code
        }
    }

    public async Task<Result<bool>> GetFeatureEnabledAsync(string featurename)
    {
        var result = await _featureManager.IsEnabledAsync(featurename);

        return Task.FromResult<Result<bool>>(result).Result;
    }


}

