//-----------------------------------------------------------------------
// <copyright file="RiskAnalysisDataService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Risk analysis data service providing business operations for SMS domain entities.
//                  Infrastructure layer service implementing data access patterns
//                  through repositories while maintaining clean architecture.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Infrastructure.Services;

/// <summary>
/// Risk Analysis Data Service providing business operations for RiskAnalysis entities
/// </summary>
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

    public Task<Result<RiskAnalysis>> GetRiskAnalysisByCodeAsync(RiskAnalysisID id, CancellationToken ct = default)
    {
        return _repo.GetRiskAnalysisByCodeAsync(id, ct);
    }
    public Task<Result<RiskAnalysis>> GetRiskAnalysisByHazardCodeAsync(HazardID id, CancellationToken ct = default)
    {
        return _repo.GetRiskAnalysisByHazardCodeAsync(id, ct);
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

