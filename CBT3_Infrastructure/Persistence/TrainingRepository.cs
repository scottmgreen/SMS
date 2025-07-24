using System.Diagnostics;

using CBT3_Domain.Entities;
using CBT3_Domain.Errors;
using CBT3_Domain.Interfaces;
using CBT3_Domain.ValueObjects;
using CBT3_Infrastructure.Common;
using CBT3_Infrastructure.Interfaces;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CBT3_Infrastructure.Repositories;

public sealed class TrainingRepository : BaseRepository<TrainingRepository, Course>
{
    private readonly ILogger<TrainingRepository> _logger;
    private readonly string _logheader;
    private readonly string _connectionString;

    public TrainingRepository(ILogger<TrainingRepository> logger, ILogSupport logsupport, IConfiguration configuration) : base(logger, logsupport,  configuration)
    {

        _logger = base.Logger;
        _logheader = base.LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(CBT3_Shared.Common.LoggingEventIds.CBT3_InfrastructureEventIds.InfrastructureEvent, "{logheader}  Training Repository ", _logheader);

    }

    public async Task<Result<Trainee>> AddTraineeAsync(Trainee trainee, CancellationToken ct = default)
    {

        try
        { 
            if (trainee is null )
            {
                return Result<Trainee>.Failure<Trainee>(DomainErrors.TraineeError.NullOrEmpty);
            }
            
            _logger.LogInfrastructurePostItem($"{_logheader} {StoredProcs.cn_spAddTrainee} Id:{trainee.Id} First Name:{trainee.FirstName} Last Name:{trainee.LastName} UPID:{trainee.UPID} YOB:{trainee.YearOfBirth}", null);


            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.cn_spAddTrainee, sql)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmTraineeId, trainee.Id.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmFirstName, trainee.FirstName.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmLastName, trainee.LastName.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUPID, trainee.UPID.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmYearOfBirth, trainee.YearOfBirth.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSubscribeToEmailNewsletter, trainee.SubscribeToEmailNewsletter.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSubscribeToTextNewsletter, trainee.SubscribeToTextNewsletter.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSubscribeToOperationalTexts, trainee.SubscribeToOperationalTexts.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, trainee.CreatedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, trainee.CreatedDate.Value));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            var result = cmd.ExecuteScalar();//ABSOLUTELY CAN't BE Async!
            TraineeID traineeid = new(result.ToString());
            Trainee newtrainee = GetTraineeByIdAsync(traineeid, ct).Result.Value;
            await sql.CloseAsync().ConfigureAwait(false);

            if (newtrainee is not null)
                return Result<Trainee>.Success(newtrainee);
            else //this should never happen
                return Result<Trainee>.Failure<Trainee>(DomainErrors.TraineeError.NullOrEmpty); // No rows were affected

        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logheader} {ex.Message}", null);
            return Result<Trainee>.Failure<Trainee>(DomainErrors.TraineeError.NullOrEmpty); // Or provide a more specific error code
        }
    }
    
    public async Task<Result<bool>> AddTrainingLogEntryAsync(TrainingLogEntry trainingLog, CancellationToken ct = default)
    {

        try
        {
            if (trainingLog is null)
            {
                return Result<bool>.Failure<bool>(DomainErrors.TrainingLogEntryError.NullOrEmptyParam);
            }
            
            _logger.LogInfrastructurePostItem($"{_logheader} {StoredProcs.cn_spInsertTrainingLogEntry} " +
                $"LessonId:{trainingLog.LessonId.Value} " +
                $"LessonQuizId:{trainingLog.LessonQuizId.Value} " +
                $"QuestionPoolId:{trainingLog.QuestionPoolId.Value} " +
                $"QuestionId:{trainingLog.QuestionId.Value} " +
                $"AnswerId:{trainingLog.AnswerId.Value} ",null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.cn_spInsertTrainingLogEntry, sql)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };


            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmTraineeId, trainingLog.TraineeId.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCourseId, trainingLog.CourseId.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmLessonId, trainingLog.LessonId.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmLessonQuizId, trainingLog.LessonQuizId.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmQuestionPoolId, trainingLog.QuestionPoolId.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmQuestionId, trainingLog.QuestionId.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmAnswerId, trainingLog.AnswerId.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmIsCorrect, trainingLog.IsCorrect));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRecordedAt, trainingLog.RecordedAt));
            cmd.Parameters.Add(DataAccess.Parameter("@ReturnVal", 0));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            int rowsAffected = (int)await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);

            await sql.CloseAsync().ConfigureAwait(false);

            if (rowsAffected > 0)
                return Result<bool>.Success(true);
            else //this should never happen
                return (Result<bool>)Result<bool>.Failure(DomainErrors.TrainingLogEntryError.AddTrainingLogEntry); // No rows were affected
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logheader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.TrainingLogEntryError.AddTrainingLogEntry); // Or provide a more specific error code
        }
    }
    
    public async Task<Result<bool>> DeleteTrainingLogEntriesAsync(TraineeID traineeId, CourseID courseId, LessonQuizID lessonquizId, CancellationToken ct = default)
    {

        try
        {
            if (traineeId is null || courseId is null)
            {
                return Result<bool>.Failure<bool>(DomainErrors.TrainingLogEntryError.NullOrEmptyParam);
            }
            
            _logger.LogInfrastructureDeleteItem($"{_logheader} {StoredProcs.cn_spDeleteTrainingLogEntries} TraineeId:{traineeId.Value} CourseID {courseId.Value} LessonQuizId{lessonquizId.Value}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.cn_spDeleteTrainingLogEntries, sql)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmTraineeId, traineeId.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCourseId, courseId.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmLessonQuizId, lessonquizId.Value));
            cmd.Parameters.Add(DataAccess.Parameter("@ReturnVal", 0));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            int rowsAffected = (int)await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);

            await sql.CloseAsync().ConfigureAwait(false);

            if (rowsAffected >= 0)
                return Result<bool>.Success(true);
            else
                return Result<bool>.Failure<bool>(DomainErrors.TrainingLogEntryError.DeleteTrainingLog);// No rows were affected
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureDeleteItemError($"{_logheader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.TrainingLogEntryError.DeleteTrainingLog); // Or provide a more specific error code
        }
    }
    
    public async Task<Result<bool>> CompleteCourseAsync(TraineeID traineeId, CourseID courseId, bool coursepass, CancellationToken ct = default)
    {

        try
        {
            if (traineeId is null || courseId is null)
            {
                return Result<bool>.Failure<bool>(DomainErrors.TrainingLogEntryError.NullOrEmptyParam);
            }

            _logger.LogInfrastructurePostItem($"{_logheader} {StoredProcs.cn_spCompleteCourse} TraineeId:{traineeId.Value} CourseID:{courseId.Value} Course Pass:{coursepass.ToString()}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.cn_spCompleteCourse, sql)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };


            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmTraineeId, traineeId.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCourseId, courseId.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCoursePass, coursepass));
            cmd.Parameters.Add(DataAccess.Parameter("@ReturnVal", 0));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            int rowsAffected = (int)await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);

            await sql.CloseAsync().ConfigureAwait(false);
             

            bool success = rowsAffected > -1;

            if (success)
                return Result<bool>.Success(true);
            else
                return Result<bool>.Failure<bool>(DomainErrors.TrainingLogEntryError.CourseCompletion);// No rows were affected


        }
        catch (Exception ex)
        {

            _logger.LogInfrastructurePostItemError($"{_logheader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.TrainingLogEntryError.CourseCompletion);
        }
    }

    public async Task<Result<Trainee>> GetTraineeByIdAsync(TraineeID traineeId, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureGetItem($"{_logheader} {StoredProcs.cn_spGetTrainee} TraineeID:{traineeId.Value}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.cn_spGetTrainee, sql)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmTraineeId, traineeId.Value));

            Trainee response = null;

            await sql.OpenAsync(ct).ConfigureAwait(false);

            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var trainee = Mappers.MapToTrainee(reader);

                    response = trainee;
                }
            }

            await sql.CloseAsync().ConfigureAwait(false);

            if (response is not null)
                return Result<Trainee>.Success(response);
            else //this should never happen
                return (Result<Trainee>)Result<Trainee>.Failure(DomainErrors.TraineeError.NullOrEmpty); 
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logheader} {ex.Message}", null);
            return (Result<Trainee>)Result<Trainee>.Failure(DomainErrors.TraineeError.NullOrEmpty);
        }
    }

    public async Task<Result<bool>> CheckForPreviousTrainingOnDateAsync(Trainee trainee, CourseID courseID, DateTime datetocheck, CancellationToken ct = default)
    {
     
        try
        {
            if (trainee is null)
            {
                return Result<bool>.Failure<bool>(DomainErrors.CourseError.CourseCheck);
            }

            _logger.LogInfrastructureGetItem($"{_logheader} {StoredProcs.cn_spCheckForPreviousTrainingOnDate} TraineeID:{trainee.Id.Value} CourseID {courseID.Value} DateToCheck {datetocheck.Date.ToString("MM/dd/yyyy")}", null);
            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.cn_spCheckForPreviousTrainingOnDate, sql)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmFirstName, trainee.FirstName.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmLastName, trainee.LastName.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUPID, trainee.UPID.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmYearOfBirth, trainee.YearOfBirth.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCourseCode, courseID.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmDate, datetocheck.ToString("yyyy-MM-dd HH:mm:ss")));
            cmd.Parameters.Add(DataAccess.Parameter("@pTrainingExists", 0));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            bool trainingexists = (bool)await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);

            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(trainingexists); // No rows were affected
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logheader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.CourseError.CourseCheck); // Or provide a more specific error code
        }
    }
}

