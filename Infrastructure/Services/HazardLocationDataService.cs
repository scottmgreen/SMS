using SMS_Infrastructure.Interfaces;
using SMS_Infrastructure.Persistence;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Shared.Common;

namespace SMS_Infrastructure.Services;

public class HazardLocationDataService : BaseDataService<HazardLocationDataService>, IHazardLocationDataService
{
    private readonly ILogger<HazardLocationDataService> _logger;
    private readonly string _logheader;
    private readonly HazardLocationRepository _repo;

    public HazardLocationDataService(ILogger<HazardLocationDataService> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, HazardLocationRepository repo)
        : base(logger, serviceScopeFactory, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _repo = repo;

        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} {repo.GetType().Name}");
    }

    public Task<Result<HazardLocation>> CreateHazardLocationAsync(HazardLocation hazardLocation, CancellationToken ct = default)
    {
        return _repo.CreateHazardLocationAsync(hazardLocation, ct);
    }

    public Task<Result<HazardLocation>> GetHazardLocationByIdAsync(HazardLocationID id, CancellationToken ct = default)
    {
        return _repo.GetHazardLocationByIdAsync(id, ct);
    }

    public Task<Result<List<HazardLocation>>> GetAllHazardLocationsAsync(CancellationToken ct = default)
    {
        return _repo.GetAllHazardLocationsAsync(ct);
    }

    public Task<Result<List<HazardLocation>>> GetHazardLocationsByHazardCodeAsync(string hazardCode, CancellationToken ct = default)
    {
        return _repo.GetHazardLocationsByHazardCodeAsync(hazardCode, ct);
    }

    public Task<Result<HazardLocation>> UpdateHazardLocationAsync(HazardLocation hazardLocation, CancellationToken ct = default)
    {
        return _repo.UpdateHazardLocationAsync(hazardLocation, ct);
    }

    public Task<Result<bool>> DeleteHazardLocationAsync(HazardLocationID id, CancellationToken ct = default)
    {
        return _repo.DeleteHazardLocationAsync(id, ct);
    }
}