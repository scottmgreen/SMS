//-----------------------------------------------------------------------
// <copyright file="HazardLocationDataService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Hazard location data service providing business operations for SMS domain entities.
//                  Infrastructure layer service implementing data access patterns
//                  through repositories while maintaining clean architecture.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using SMS_Infrastructure.Interfaces;

namespace SMS_Infrastructure.Services;

/// <summary>
/// Hazard Location Data Service providing business operations for HazardLocation entities
/// </summary>
public class HazardLocationDataService : BaseDataService<HazardLocationDataService>, IHazardLocationDataService
{
    private readonly ILogger<HazardLocationDataService> _logger;
    private readonly string _logheader;
    private readonly HazardLocationRepository _repo;

    public HazardLocationDataService(ILogger<HazardLocationDataService> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, HazardLocationRepository repo)
        : base(logger, serviceScopeFactory, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _repo = repo;

        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} {repo.GetType().Name}");
    }

    public Task<Result<HazardLocation>> CreateHazardLocationAsync(HazardLocation hazardLocation, CancellationToken ct = default)
    {
        return _repo.CreateHazardLocationAsync(hazardLocation, ct);
    }

    public Task<Result<HazardLocation>> GetHazardLocationByCodeAsync(HazardLocationID code, CancellationToken ct = default)
    {
        return _repo.GetHazardLocationByCodeAsync(code, ct);
    }

    public Task<Result<List<HazardLocation>>> GetAllHazardLocationsAsync(CancellationToken ct = default)
    {
        return _repo.GetAllHazardLocationsAsync(ct);
    }

    public Task<Result<List<HazardLocation>>> GetHazardLocationsByHazardCodeAsync(string hazardCode, CancellationToken ct = default)
    {
        return _repo.GetHazardLocationsByHazardCodeAsync(hazardCode, ct);
    }

    public Task<Result<HazardLocation>> UpdateHazardLocationAsync(HazardLocation hazardLocation, CancellationToken ct = default)
    {
        return _repo.UpdateHazardLocationAsync(hazardLocation, ct);
    }

    public Task<Result<bool>> DeleteHazardLocationAsync(HazardLocationID id, CancellationToken ct = default)
    {
        return _repo.DeleteHazardLocationAsync(id, ct);
    }
}
