namespace CBT3_Application.Messaging;

public class UpdateTrainingSessionCommandHandler : BaseCommandBundle, IRequestHandler<UpdateTrainingSessionCommand, TrainingSession>
{
    private readonly CBT3_Application.Services.DashboardService _dashboardService;

    public UpdateTrainingSessionCommandHandler(CBT3_Application.Services.DashboardService dashboardService)
    {
        _dashboardService = dashboardService;

    }
    public Task<TrainingSession> HandleAsync(UpdateTrainingSessionCommand request, CancellationToken ct = default)
    {
        TrainingSession session = request.TrainingSession;


        _ = _dashboardService.UpdateTrainingSessionAsync(session,ct);
        return Task.FromResult(request.TrainingSession);
    }
}