// =============================================
// SMS DATA SERVICES - COMPLETE SET
// All 10 SMS Data Services for easy copy/paste
// =============================================

// 1. HazardDataService.cs
namespace SMS_Infrastructure.Services;

// 6. RiskAssessmentDataService.cs
public class RiskAssessmentDataService : BaseDataService<RiskAssessmentDataService>
{
    private readonly ILogger<RiskAssessmentDataService> _logger;
    private readonly string _logheader;
    private readonly RiskAssessmentRepository _repo;

    public RiskAssessmentDataService(ILogger<RiskAssessmentDataService> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, RiskAssessmentRepository repo)
        : base(logger, serviceScopeFactory, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _repo = repo;

        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} {repo.GetType().Name}");
    }

    public Task<Result<RiskAssessment>> CreateRiskAssessmentAsync(RiskAssessment riskAssessment, CancellationToken ct = default)
    {
        return _repo.CreateRiskAssessmentAsync(riskAssessment, ct);
    }

    public Task<Result<RiskAssessment>> GetRiskAssessmentByIdAsync(RiskAssessmentID id, CancellationToken ct = default)
    {
        return _repo.GetRiskAssessmentByIdAsync(id, ct);
    }

    public Task<Result<List<RiskAssessment>>> GetAllRiskAssessmentsAsync(CancellationToken ct = default)
    {
        return _repo.GetAllRiskAssessmentsAsync(ct);
    }

    public Task<Result<RiskAssessment>> UpdateRiskAssessmentAsync(RiskAssessment riskAssessment, CancellationToken ct = default)
    {
        return _repo.UpdateRiskAssessmentAsync(riskAssessment, ct);
    }

    public Task<Result<bool>> DeleteRiskAssessmentAsync(RiskAssessmentID id, CancellationToken ct = default)
    {
        return _repo.DeleteRiskAssessmentAsync(id, ct);
    }
}
