// =============================================
// SMS DATA SERVICES - COMPLETE SET
// All 10 SMS Data Services for easy copy/paste
// =============================================

// 1. HazardDataService.cs
namespace SMS_Infrastructure.Services;

// 3. InvestigationDataService.cs
public class InvestigationDataService : BaseDataService<InvestigationDataService>
{
    private readonly ILogger<InvestigationDataService> _logger;
    private readonly string _logheader;
    private readonly InvestigationRepository _repo;

    public InvestigationDataService(ILogger<InvestigationDataService> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, InvestigationRepository repo)
        : base(logger, serviceScopeFactory, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _repo = repo;

        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} {repo.GetType().Name}");
    }

    public Task<Result<Investigation>> CreateInvestigationAsync(Investigation investigation, CancellationToken ct = default)
    {
        return _repo.CreateInvestigationAsync(investigation, ct);
    }

    public Task<Result<Investigation>> GetInvestigationByIdAsync(InvestigationID id, CancellationToken ct = default)
    {
        return _repo.GetInvestigationByIdAsync(id, ct);
    }

    public Task<Result<List<Investigation>>> GetAllInvestigationsAsync(CancellationToken ct = default)
    {
        return _repo.GetAllInvestigationsAsync(ct);
    }

    public Task<Result<Investigation>> UpdateInvestigationAsync(Investigation investigation, CancellationToken ct = default)
    {
        return _repo.UpdateInvestigationAsync(investigation, ct);
    }

    public Task<Result<bool>> DeleteInvestigationAsync(InvestigationID id, CancellationToken ct = default)
    {
        return _repo.DeleteInvestigationAsync(id, ct);
    }
}
