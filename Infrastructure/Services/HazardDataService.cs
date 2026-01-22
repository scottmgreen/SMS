// =============================================
// SMS DATA SERVICES - COMPLETE SET
// All 10 SMS Data Services for easy copy/paste
// =============================================

// 1. HazardDataService.cs
using SMS_Infrastructure.Interfaces;

namespace SMS_Infrastructure.Services;

public class HazardDataService : BaseDataService<HazardDataService>, IHazardDataService
{
    private readonly ILogger<HazardDataService> _logger;
    private readonly string _logheader;
    private readonly HazardRepository _repo;

    public HazardDataService(ILogger<HazardDataService> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, HazardRepository repo)
        : base(logger, serviceScopeFactory, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _repo = repo;

        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} {repo.GetType().Name}");
    }

    public Task<Result<Hazard>> CreateHazardAsync(Hazard hazard, CancellationToken ct = default)
    {
        return _repo.CreateHazardAsync(hazard, ct);
    }

    public Task<Result<Hazard>> GetHazardByCodeAsync(HazardID code, CancellationToken ct = default)
    {
        return _repo.GetHazardByCodeAsync(code, ct);
    }
    public Task<Result<List<Hazard>>> GetHazardsByReportIdAsync(ReportID code, CancellationToken ct = default)
    {
        return _repo.GetHazardsByReportCodeAsync(code, ct);
    }

    public Task<Result<List<Hazard>>> GetHazardsByReportIdWithMitigationsAsync(ReportID id, CancellationToken ct = default)
    {
        return _repo.GetHazardsByReportIdWithMitigationsAsync(id, ct);
    }

    public Task<Result<List<Hazard>>> GetAllHazardsAsync(CancellationToken ct = default)
    {
        return _repo.GetAllHazardsAsync(ct);
    }

    public Task<Result<Hazard>> UpdateHazardAsync(Hazard hazard, CancellationToken ct = default)
    {
        return _repo.UpdateHazardAsync(hazard, ct);
    }

    public Task<Result<bool>> DeleteHazardAsync(HazardID id, CancellationToken ct = default)
    {
        return _repo.DeleteHazardAsync(id, ct);
    }
}
