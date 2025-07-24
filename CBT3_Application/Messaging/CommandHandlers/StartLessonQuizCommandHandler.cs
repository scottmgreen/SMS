namespace CBT3_Application.Messaging.CommandHandlers;

public class StartLessonQuizCommandHandler : BaseCommandBundle, IRequestHandler<StartLessonQuizCommand, Result<bool>>
{
    private readonly LessonQuizService _lessonquizService;
    private readonly TrainingService _trainingService;
   
    public StartLessonQuizCommandHandler(LessonQuizService lessonquizService,TrainingService trainingdataService)
    {
        _lessonquizService = lessonquizService;
        _trainingService = trainingdataService;
        
    }
    
    public async Task<Result<bool>> HandleAsync(StartLessonQuizCommand request, CancellationToken ct = default)
    {
        
        TraineeID traineeId = request.TraineeId;
        CourseID courseId = request.CourseId;
        LessonQuizID lessonquizId = request.LessonQuizId;
        
        var result = (Result<bool>) await _trainingService.DeleteTrainingLogEntriesAsync(traineeId, courseId, lessonquizId,ct).ConfigureAwait(false);              
        return result;
    }
}