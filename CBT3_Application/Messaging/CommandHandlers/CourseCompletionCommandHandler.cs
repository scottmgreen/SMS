
using CBT3_Application.Services;

namespace CBT3_Application.Messaging.CommandHandlers;

public class CourseCompletionCommandHandler : BaseCommandBundle, IRequestHandler<CourseCompletionCommand, Result<bool>>
{
    private readonly TrainingService _trainingService;
    private readonly CBT3_Application.Services.DashboardService _dashboardService;
    public CourseCompletionCommandHandler(TrainingService trainingService, CBT3_Application.Services.DashboardService dashboardService)
    {
        _trainingService = trainingService;
        _dashboardService = dashboardService;
    }

    

    public async Task<Result<bool>> HandleAsync(CourseCompletionCommand request, CancellationToken ct = default)
    {
        TraineeID trainee = request.TraineeID;
        CourseID course = request.CourseID;
        bool coursepass = request.CoursePass;
        TrainingSession session = request.TrainingSession;
        session.CourseEndedAt = DateTime.Now;

        _ = await _dashboardService.UpdateTrainingSessionAsync(session,ct);

        Result<bool> result = await _trainingService.CompleteCourseAsync(trainee, course, coursepass,ct).ConfigureAwait(false);
        return result;
    }
}
