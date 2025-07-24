using CBT3_Infrastructure.Repositories;

namespace CBT3_Infrastructure.Services;

public class TrainingDataService : BaseDataService<TrainingDataService>
{
    private readonly ILogger<TrainingDataService> _logger;
    private readonly string _logheader;
    private TrainingRepository _repo;

    public TrainingDataService(ILogger<TrainingDataService> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, TrainingRepository repo) : base(logger, serviceScopeFactory, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;

        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader}  {repo.GetType().Name} ");
        _repo = repo;
    }
    public Task<Result<Trainee>> GetTraineeByIdAsync(TraineeID traineeid, CancellationToken ct = default)
    {
        return _repo.GetTraineeByIdAsync(traineeid, ct);
    }
    public Task<Result<Trainee>> AddTraineeAsync(Trainee trainee, CancellationToken ct = default)
    {
        return _repo.AddTraineeAsync(trainee,ct); 
    }
    public Task<Result<bool>> AddTrainingLogEntryAsync(TrainingLogEntry traininglogentry, CancellationToken ct = default)
    {
        return _repo.AddTrainingLogEntryAsync(traininglogentry,ct);
    }

    public Task<Result<bool>> DeleteTrainingLogEntriesAsync(TraineeID  traineeId,CourseID courseId, LessonQuizID lessonquizId, CancellationToken ct = default)
    {
        return _repo.DeleteTrainingLogEntriesAsync(traineeId, courseId, lessonquizId,ct);
    }
    public Task<Result<bool>> CompleteCourseAsync(TraineeID traineeId, CourseID courseId, bool coursepass, CancellationToken ct = default)
    {
        return _repo.CompleteCourseAsync(traineeId,courseId, coursepass,ct);
    }
    public Task<Result<bool>> CheckForPreviousTrainingOnDateAsync(Trainee trainee, CourseID courseId, DateTime date, CancellationToken ct = default)
    {
        return _repo.CheckForPreviousTrainingOnDateAsync(trainee, courseId, date, ct);
    }

}
