// =============================================
// SMS DATA SERVICES - COMPLETE SET
// All 10 SMS Data Services for easy copy/paste
// =============================================

// 1. HazardDataService.cs
namespace SMS_Infrastructure.Services;

using SMS_Infrastructure.Interfaces;
using SMS_Infrastructure.Persistence;

/// <summary>
/// Enhanced Investigation Data Service - follows the exact same pattern as HazardDataService
/// Provides business logic layer between CQRS handlers and repository
/// </summary>
public class InvestigationDataService : BaseDataService<InvestigationDataService>, IInvestigationDataService
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

    // Core CRUD Operations
    public Task<Result<Investigation>> CreateInvestigationAsync(Investigation investigation, CancellationToken ct = default)
    {
        return _repo.CreateInvestigationAsync(investigation, ct);
    }

    public Task<Result<Investigation>> GetInvestigationByCodeAsync(string code, CancellationToken ct = default)
    {
        return _repo.GetInvestigationByCodeAsync(code, ct);
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

    // Query Operations
    public Task<Result<IEnumerable<Investigation>>> GetByHazardCodeAsync(string hazardCode, CancellationToken ct = default)
    {
        return _repo.GetByHazardCodeAsync(hazardCode, ct);
    }

    public Task<Result<IEnumerable<Investigation>>> GetByInvestigatorAsync(string investigatorId, CancellationToken ct = default)
    {
        return _repo.GetByInvestigatorAsync(investigatorId, ct);
    }

    public Task<Result<IEnumerable<Investigation>>> GetByStatusAsync(InvestigationStatus status, CancellationToken ct = default)
    {
        return _repo.GetByStatusAsync(status, ct);
    }

    // Status Management Operations
    public Task<Result<bool>> UpdateStatusAsync(string investigationCode, InvestigationStatus status, CancellationToken ct = default)
    {
        return _repo.UpdateStatusAsync(investigationCode, status, ct);
    }

    public Task<Result<bool>> RecordDecisionAsync(string investigationCode, string decisionType, string rationale,
        string decisionMaker, string? nextSteps = null, string? referralDetails = null, CancellationToken ct = default)
    {
        return _repo.RecordDecisionAsync(investigationCode, decisionType, rationale, decisionMaker, nextSteps, referralDetails, ct);
    }

    public Task<Result<bool>> CompleteInvestigationAsync(string investigationCode, CancellationToken ct = default)
    {
        return _repo.CompleteInvestigationAsync(investigationCode, ct);
    }
}
