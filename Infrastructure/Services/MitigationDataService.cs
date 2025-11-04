// =============================================
// SMS DATA SERVICES - COMPLETE SET
// All 10 SMS Data Services for easy copy/paste
// =============================================

// 1. HazardDataService.cs
namespace SMS_Infrastructure.Services;

// 7. MitigationDataService.cs
public class MitigationDataService : BaseDataService<MitigationDataService>
{
    private readonly ILogger<MitigationDataService> _logger;
    private readonly string _logheader;
    private readonly MitigationRepository _repo;

    public MitigationDataService(ILogger<MitigationDataService> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, MitigationRepository repo)
        : base(logger, serviceScopeFactory, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _repo = repo;

        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} {repo.GetType().Name}");
    }

    public Task<Result<Mitigation>> CreateMitigationAsync(Mitigation mitigation, CancellationToken ct = default)
    {
        return _repo.CreateMitigationAsync(mitigation, ct);
    }

    public Task<Result<Mitigation>> GetMitigationByIdAsync(MitigationID id, CancellationToken ct = default)
    {
        return _repo.GetMitigationByIdAsync(id, ct);
    }

    public Task<Result<List<Mitigation>>> GetAllMitigationsAsync(CancellationToken ct = default)
    {
        return _repo.GetAllMitigationsAsync(ct);
    }

    public Task<Result<Mitigation>> UpdateMitigationAsync(Mitigation mitigation, CancellationToken ct = default)
    {
        return _repo.UpdateMitigationAsync(mitigation, ct);
    }

    public Task<Result<bool>> DeleteMitigationAsync(MitigationID id, CancellationToken ct = default)
    {
        return _repo.DeleteMitigationAsync(id, ct);
    }
}
