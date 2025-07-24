using CBT3_Domain.Interfaces;

using CBT3_Infrastructure.Interfaces;

using Microsoft.FeatureManagement;

namespace CBT3_Infrastructure.Services;
public class DashboardDataService : BaseDataService<DashboardDataService> ,IDashboard
{
    private readonly ILogger<DashboardDataService> _logger;
    private DashboardRepository _repo;
    private readonly string _logheader;
    
    public DashboardDataService(ILogger<DashboardDataService> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, DashboardRepository repo) : base(logger, serviceScopeFactory, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _repo = repo;
    }

         
    public Task<Result<TrainingStation>> GetTrainingStationByHostNameAsync(string hostName, CancellationToken ct = default)
    {
        return _repo.GetTrainingStationByHostNameAsync(hostName, ct);
    }

    public Task<Result<TrainingStation>> UpdateTrainingStationAsync(ITrainingStation trainingmachine, CancellationToken ct = default)
    {
        return _repo.UpdateTrainingStationAsync(trainingmachine, ct);
    }
    public Task<Result<TrainingSession>> UpdateTrainingSessionAsync(TrainingSession trainingsession, CancellationToken ct = default)
    {
        return _repo.UpdateTrainingSessionAsync(trainingsession, ct);
    }
    public Task<Result<TrainingSession>> StartTrainingSessionAsync(TrainingSession trainingsession, CancellationToken ct = default)
    {
        return _repo.StartTrainingSessionAsync(trainingsession, ct);
    }
    public Task<Result<TrainingSession>> FinishTrainingSessionAsync(TrainingSession trainingsession, CancellationToken ct = default)
    {
        return _repo.FinishTrainingSessionAsync(trainingsession, ct);
    }
    public Task<Result<List<TrainingRecord>>> GetTrainingRecordsAsync(CancellationToken ct = default)
    {
        return _repo.GetTrainingRecordsAsync(ct);
    }
    public Task<Result<List<TrainingRecord>>> GetMismatchedTrainingRecordsAsync(CancellationToken ct = default)
    {
        return _repo.GetMismatchedTrainingRecordsAsync(ct);
    }
    public Task<Result<List<TrainingStation>>> ResetTrainingStationsAsync(CancellationToken ct = default)
    {
        return _repo.ResetTrainingStationsAsync(ct);

    }
    public Task<Result<List<TrainingStation>>> GetTrainingStationsAsync(CancellationToken ct = default)
    {
        return _repo.GetTrainingStationsAsync(ct);

    }
    public Task<Result<TrainingStation>> GetTrainingStationAsync(string hostname ,CancellationToken ct = default)
    {
        return _repo.GetTrainingStationByHostNameAsync(hostname,ct);

    }
}