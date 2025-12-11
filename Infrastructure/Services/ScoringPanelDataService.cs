// =============================================
// SMS DATA SERVICES - COMPLETE SET
// All 10 SMS Data Services for easy copy/paste
// =============================================

// 1. HazardDataService.cs
using SMS_Domain.Entities;
using SMS_Domain.Interfaces;
using SMS_Domain.ValueObjects;
using SMS_Domain.Common;
using SMS_Infrastructure.Persistence;

namespace SMS_Infrastructure.Services;

// 10. ScoringPanelDataService.cs
public class ScoringPanelDataService : BaseDataService<ScoringPanelDataService>
{
    private readonly ILogger<ScoringPanelDataService> _logger;
    private readonly string _logheader;
    private readonly ScoringPanelRepository _repo;

    public ScoringPanelDataService(ILogger<ScoringPanelDataService> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, ScoringPanelRepository repo)
        : base(logger, serviceScopeFactory, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _repo = repo;

        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} {repo.GetType().Name}");
    }

    public Task<Result<ScoringPanel>> CreateScoringPanelAsync(ScoringPanel scoringPanel, CancellationToken ct = default)
    {
        return _repo.CreateScoringPanelAsync(scoringPanel, ct);
    }

    public Task<Result<ScoringPanel>> GetScoringPanelByIdAsync(ScoringPanelID id, CancellationToken ct = default)
    {
        return _repo.GetScoringPanelByIdAsync(id, ct);
    }

    public Task<Result<List<ScoringPanel>>> GetAllScoringPanelsAsync(CancellationToken ct = default)
    {
        return _repo.GetAllScoringPanelsAsync(ct);
    }

    public Task<Result<List<ScoringPanel>>> GetScoringPanelsByHazardCodeAsync(string hazardCode, CancellationToken ct = default)
    {
        return _repo.GetScoringPanelsByHazardCodeAsync(hazardCode, ct);
    }

    public Task<Result<ScoringPanel>> UpdateScoringPanelAsync(ScoringPanel scoringPanel, CancellationToken ct = default)
    {
        return _repo.UpdateScoringPanelAsync(scoringPanel, ct);
    }

    public Task<Result<bool>> DeleteScoringPanelAsync(ScoringPanelID id, CancellationToken ct = default)
    {
        return _repo.DeleteScoringPanelAsync(id, ct);
    }
}