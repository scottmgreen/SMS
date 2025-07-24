using CBT3_Application.Services;

using CBT3_Infrastructure.Interfaces;

namespace CBT3_Application.Messaging;

public class UpdateTrainingStationCommandHandler : BaseCommandBundle, IRequestHandler<UpdateTrainingStationCommand, ITrainingStation>
{
    private readonly CBT3_Application.Services.DashboardService _dashboardService;

    public UpdateTrainingStationCommandHandler(CBT3_Application.Services.DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
                
    }
    public Task<ITrainingStation> HandleAsync(UpdateTrainingStationCommand request, CancellationToken ct = default)
    {
        ITrainingStation station = request.TrainingStation;


        _dashboardService.UpdateTrainingStationAsync(station,ct);
        return Task.FromResult(request.TrainingStation);
    }
}
