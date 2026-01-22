// =============================================
// SMS DATA SERVICES - COMPLETE SET
// All 10 SMS Data Services for easy copy/paste
// =============================================

// AirportSharedDatasetDataService.cs
using SMS_Infrastructure.Interfaces;

namespace SMS_Infrastructure.Services;

public class AirportSharedDatasetDataService : BaseDataService<AirportSharedDatasetDataService>, IAirportSharedDatasetDataService
{
    private readonly ILogger<AirportSharedDatasetDataService> _logger;
    private readonly string _logheader;
    private readonly AirportSharedDatasetRepository _repo;

    public AirportSharedDatasetDataService(ILogger<AirportSharedDatasetDataService> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, AirportSharedDatasetRepository repo)
        : base(logger, serviceScopeFactory, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _repo = repo;

        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} {repo.GetType().Name}");
    }

    public Task<Result<AirportSharedDataset>> CreateAirportSharedDatasetAsync(AirportSharedDataset airportSharedDataset, CancellationToken ct = default)
    {
        return _repo.CreateAirportSharedDatasetAsync(airportSharedDataset, ct);
    }

    public Task<Result<AirportSharedDataset>> GetAirportSharedDatasetByCodeAsync(AirportSharedDatasetID code, CancellationToken ct = default)
    {
        return _repo.GetAirportSharedDatasetByCodeAsync(code, ct);
    }

    public Task<Result<List<AirportSharedDataset>>> GetAllAirportSharedDatasetsAsync(CancellationToken ct = default)
    {
        return _repo.GetAllAirportSharedDatasetsAsync(ct);
    }

    public Task<Result<AirportSharedDataset>> UpdateAirportSharedDatasetAsync(AirportSharedDataset airportSharedDataset, CancellationToken ct = default)
    {
        return _repo.UpdateAirportSharedDatasetAsync(airportSharedDataset, ct);
    }

    public Task<Result<bool>> DeleteAirportSharedDatasetAsync(AirportSharedDatasetID id, CancellationToken ct = default)
    {
        return _repo.DeleteAirportSharedDatasetAsync(id, ct);
    }
}
