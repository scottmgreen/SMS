using CBT3_Infrastructure;
using CBT3_Infrastructure.Interfaces;

namespace CBT3_Application.Services;
public class DashboardService : IDashboard
{
    private DashboardDataService _dashboardService;
    public DashboardService(DashboardDataService dashboardService)
    {
        _dashboardService = dashboardService;
    }
    public Task<Result<List<TrainingStation>>> ResetTrainingStationsAsync(CancellationToken ct = default)
    {
        return _dashboardService.ResetTrainingStationsAsync(ct);
    }

    
    public Task<Result<TrainingStation>> GetTrainingStationByHostNameAsync(string hostName, CancellationToken ct = default)
    {
        return _dashboardService.GetTrainingStationByHostNameAsync(hostName,ct);
    }

    public Task<Result<TrainingStation>> UpdateTrainingStationAsync(ITrainingStation trainingmachine, CancellationToken ct = default)
    {
        return _dashboardService.UpdateTrainingStationAsync(trainingmachine,ct);
    }

    public Task<Result<TrainingSession>> UpdateTrainingSessionAsync(TrainingSession trainingsession, CancellationToken ct = default)
    {
        return _dashboardService.UpdateTrainingSessionAsync(trainingsession,ct);
    }

    public Task<Result<TrainingSession>> StartTrainingSessionAsync(TrainingSession trainingsession, CancellationToken ct = default)
    {
        return _dashboardService.StartTrainingSessionAsync(trainingsession,ct);
    }

    public Task<Result<TrainingSession>> FinishTrainingSessionAsync(TrainingSession trainingsession, CancellationToken ct = default)
    {
        return _dashboardService.FinishTrainingSessionAsync(trainingsession, ct);
    }

    public Task<Result<List<TrainingRecord>>> GetTrainingRecordsAsync(CancellationToken ct = default)
    {
        return _dashboardService.GetTrainingRecordsAsync(ct);
    }
    public Task<Result<List<TrainingRecord>>> GetMismatchedTrainingRecordsAsync(CancellationToken ct = default)
    {
        return _dashboardService.GetMismatchedTrainingRecordsAsync(ct);
    }
    public Task<Result<List<TrainingStation>>> GetTrainingStationsAsync(CancellationToken ct = default)
    {
        return _dashboardService.GetTrainingStationsAsync(ct);
    }
    public Task<Result<TrainingStation>> GetTrainingStationAsync(string hostname, CancellationToken ct = default)
    {
        return _dashboardService.GetTrainingStationAsync(hostname,ct);
    }
}
