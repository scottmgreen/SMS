//-----------------------------------------------------------------------
// <copyright file="ScoringPanelDataService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Scoring panel data service providing business operations for SMS domain entities.
//                  Infrastructure layer service implementing data access patterns
//                  through repositories while maintaining clean architecture.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Infrastructure.Services;

/// <summary>
/// Scoring Panel Data Service providing business operations for ScoringPanel entities
/// </summary>
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
        return _repo.GetScoringPanelByCodeAsync(id, ct);
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
