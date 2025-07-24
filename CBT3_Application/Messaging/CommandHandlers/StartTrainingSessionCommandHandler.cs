namespace CBT3_Application.Messaging;

public class StartTrainingSessionCommandHandler : BaseCommandBundle, IRequestHandler<StartTrainingSessionCommand, TrainingSession>
{
    private readonly CBT3_Application.Services.DashboardService _dashboardService;

    public StartTrainingSessionCommandHandler(CBT3_Application.Services.DashboardService dashboardService)
    {
        _dashboardService = dashboardService;

    }
    public Task<TrainingSession> HandleAsync(StartTrainingSessionCommand request, CancellationToken ct = default)
    {
        TrainingSession session = request.TrainingSession;


        _dashboardService.StartTrainingSessionAsync(session,ct);
        return Task.FromResult(request.TrainingSession);
    }
}
