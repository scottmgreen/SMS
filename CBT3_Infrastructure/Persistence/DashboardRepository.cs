using CBT3_Domain.Entities;
using CBT3_Domain.Errors;
using CBT3_Domain.Interfaces;
using CBT3_Infrastructure.Common;
using CBT3_Infrastructure.Interfaces;

namespace CBT3_Infrastructure.Repositories;

public sealed class DashboardRepository : BaseRepository<DashboardRepository, Course>
{
    private readonly ILogger<DashboardRepository> _logger;
    private readonly string _logheader;
    private readonly string _connectionString;
    private readonly IConfiguration _configuration;


    public DashboardRepository(ILogger<DashboardRepository> logger, ILogSupport logsupport, IConfiguration configuration) : base(logger, logsupport, configuration)
    {
        _configuration = configuration;
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _connectionString = ConnectionString;
        //_logger.LogInfrastructureInformation(CBT3_InfrastructureEventIds.InfrastructureEvent, $"{_logheader}");
        
    }

    public async Task<Result<List<TrainingStation>>> ResetTrainingStationsAsync(CancellationToken ct = default)
    {
        try
        {

            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.cn_spResetTrainingStations} ", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.cn_spResetTrainingStations, sql)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            List<TrainingStation> response = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);

            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            
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

    public async Task<Result<TrainingSession>> GetTrainingSessionByTrainingSessionIDAsync(TrainingSessionID sessionId, CancellationToken ct = default)
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


    public async Task<Result<TrainingStation>> UpdateTrainingStationAsync(ITrainingStation trainingstation, CancellationToken ct = default)
    {

        try
        {
            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.cn_spUpdateTrainingStation} {trainingstation.HostName}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.cn_spUpdateTrainingStation, sql)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHostName, trainingstation.HostName));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmTrainingStationCircuitId, trainingstation.CircuitId));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmTrainingStationStatus, trainingstation.Status.Value));
            cmd.Parameters.Add(DataAccess.Parameter("@pUpdatedHostName ", trainingstation.HostName));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            var result = cmd.ExecuteScalar();//ABSOLUTELY CAN't BE Async!
            string hostName = new(result.ToString());
            TrainingStation updatedtrainingstation = GetTrainingStationByHostNameAsync(hostName, ct).Result.Value;
            await sql.CloseAsync().ConfigureAwait(false);

            if (updatedtrainingstation is not null)
                return Result<TrainingStation>.Success(updatedtrainingstation);
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

            // Input parameters
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmTrainingSessionId, trainingsession.Id.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHostName, trainingsession.HostName));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmTraineeId, trainingsession.TraineeID.Value ));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCourseId, trainingsession.CourseID != null ? (object)trainingsession.CourseID.Value : DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCourseStartedAt, trainingsession.CourseStartedAt));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCourseEndedAt, trainingsession.CourseEndedAt != null ? (object)trainingsession.CourseEndedAt :DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter("@pUpdatedHostName ", trainingsession.HostName));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            cmd.ExecuteNonQuery();//ABSOLUTELY CAN't BE Async!

            TrainingSessionID sessionId = new TrainingSessionID(trainingsession.Id.Value);
            TrainingSession updatedtrainingsession = GetTrainingSessionByTrainingSessionIDAsync(sessionId, ct).Result.Value;

            await sql.CloseAsync().ConfigureAwait(false);

            if (updatedtrainingsession is not null)
                return Result<TrainingSession>.Success(updatedtrainingsession);
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
            cmd.ExecuteNonQuery();//ABSOLUTELY CAN't BE Async!
            TrainingSessionID sessionId = new TrainingSessionID(trainingsession.Id.Value);
            TrainingSession startedtrainingsession = GetTrainingSessionByTrainingSessionIDAsync(sessionId, ct).Result.Value;
            
            await sql.CloseAsync().ConfigureAwait(false);

            if (startedtrainingsession is not null)
                return Result<TrainingSession>.Success(startedtrainingsession);
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
            TrainingSession finishedtrainingsession = GetTrainingSessionByTrainingSessionIDAsync(sessionId, ct).Result.Value;
            await sql.CloseAsync().ConfigureAwait(false);

            if (finishedtrainingsession is not null)
                return Result<TrainingSession>.Success(finishedtrainingsession);
            else //this should never happen
                return Result<TrainingSession>.Failure<TrainingSession>(DomainErrors.TraininingSessionError.NullOrEmpty); // No rows were affected

        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<TrainingSession>.Failure<TrainingSession>(DomainErrors.TraininingSessionError.NullOrEmpty); // Or provide a more specific error code
        }
    }


    public async Task<Result<List<TrainingRecord>>> GetTrainingRecordsAsync(CancellationToken ct = default)
    {

        try
        {
            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.cn_spGetTrainingRecords}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.cn_spGetTrainingRecords, sql)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            List<TrainingRecord> response = new();
            TrainingRecord? record = null;
            await sql.OpenAsync(ct).ConfigureAwait(false);

            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    record = Mappers.MapToTrainingRecord(reader);
                    response.Add(record);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);




            if (response is not null)
                return Result<List<TrainingRecord>>.Success(response);
            else //this should never happen
                return Result<List<TrainingRecord>>.Failure<List<TrainingRecord>>(DomainErrors.TraininingSessionError.NullOrEmpty); // No rows were affected

        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<List<TrainingRecord>>.Failure<List<TrainingRecord>>(DomainErrors.TraininingSessionError.NullOrEmpty); // Or provide a more specific error code
        }
    }

    public async Task<Result<List<TrainingRecord>>> GetMismatchedTrainingRecordsAsync(CancellationToken ct = default)
    {

        try
        {
            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.cn_spGetTrainingRecords}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.cn_spGetTrainingRecords, sql)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            List<TrainingRecord> response = new();
            TrainingRecord? record = null;
            await sql.OpenAsync(ct).ConfigureAwait(false);

            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    record = Mappers.MapToTrainingRecord(reader);
                    response.Add(record);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);




            if (response is not null)
                return Result<List<TrainingRecord>>.Success(response);
            else //this should never happen
                return Result<List<TrainingRecord>>.Failure<List<TrainingRecord>>(DomainErrors.TraininingSessionError.NullOrEmpty); // No rows were affected

        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<List<TrainingRecord>>.Failure<List<TrainingRecord>>(DomainErrors.TraininingSessionError.NullOrEmpty); // Or provide a more specific error code
        }
    }
}

