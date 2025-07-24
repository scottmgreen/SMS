using CBT3_Application.Services;
namespace CBT3_Application.Messaging;

public class FinishTrainingSessionCommandHandler : BaseCommandBundle, IRequestHandler<FinishTrainingSessionCommand, TrainingSession>
{
    private readonly CBT3_Application.Services.DashboardService _dashboardService;

    public FinishTrainingSessionCommandHandler(CBT3_Application.Services.DashboardService dashboardService)
    {
        _dashboardService = dashboardService;

    }
    public Task<TrainingSession> HandleAsync(FinishTrainingSessionCommand request, CancellationToken ct = default)
    {
        TrainingSession session = request.TrainingSession;


        _dashboardService.FinishTrainingSessionAsync(session,ct);
        return Task.FromResult(request.TrainingSession);
    }
}
