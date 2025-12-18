// =============================================
// SMS DATA SERVICES - COMPLETE SET
// All 10 SMS Data Services for easy copy/paste
// =============================================

// 1. HazardDataService.cs
using SMS_Infrastructure.Persistence;

namespace SMS_Infrastructure.Services;

// 5. RiskAnalysisDataService.cs
public class RiskAnalysisDataService : BaseDataService<RiskAnalysisDataService>
{
    private readonly ILogger<RiskAnalysisDataService> _logger;
    private readonly string _logheader;
    private readonly RiskAnalysisRepository _repo;

    public RiskAnalysisDataService(ILogger<RiskAnalysisDataService> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, RiskAnalysisRepository repo)
        : base(logger, serviceScopeFactory, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _repo = repo;

        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} {repo.GetType().Name}");
    }

    public Task<Result<RiskAnalysis>> CreateRiskAnalysisAsync(RiskAnalysis riskAnalysis, CancellationToken ct = default)
    {
        return _repo.CreateRiskAnalysisAsync(riskAnalysis, ct);
    }

    public Task<Result<RiskAnalysis>> GetRiskAnalysisByIdAsync(RiskAnalysisID id, CancellationToken ct = default)
    {
        return _repo.GetRiskAnalysisByIdAsync(id, ct);
    }
    public Task<Result<RiskAnalysis>> GetRiskAnalysisByHazardIdAsync(HazardID id, CancellationToken ct = default)
    {
        return _repo.GetRiskAnalysisByHazardIdAsync(id, ct);
    }
    public Task<Result<List<RiskAnalysis>>> GetAllRiskAnalysisAsync(CancellationToken ct = default)
    {
        return _repo.GetAllRiskAnalysisAsync(ct);
    }

    public Task<Result<RiskAnalysis>> UpdateRiskAnalysisAsync(RiskAnalysis riskAnalysis, CancellationToken ct = default)
    {
        return _repo.UpdateRiskAnalysisAsync(riskAnalysis, ct);
    }

    public Task<Result<bool>> DeleteRiskAnalysisAsync(RiskAnalysisID id, CancellationToken ct = default)
    {
        return _repo.DeleteRiskAnalysisAsync(id, ct);
    }
}
