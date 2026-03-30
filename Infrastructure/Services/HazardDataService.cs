//-----------------------------------------------------------------------
// <copyright file="HazardDataService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Hazard data service providing business operations for SMS domain entities.
//                  Infrastructure layer service implementing data access patterns
//                  through repositories while maintaining clean architecture.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using SMS_Infrastructure.Interfaces;

namespace SMS_Infrastructure.Services;

/// <summary>
/// Hazard Data Service providing business operations for Hazard entities
/// </summary>
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
    public Task<Result<List<Hazard>>> GetHazardsByReportCodeAsync(ReportID code, CancellationToken ct = default)
    {
        return _repo.GetHazardsByReportCodeAsync(code, ct);
    }

    public Task<Result<List<Hazard>>> GetHazardsByReportCodeWithMitigationsAsync(ReportID id, CancellationToken ct = default)
    {
        return _repo.GetHazardsByReportCodeWithMitigationsAsync(id, ct);
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

