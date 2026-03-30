//-----------------------------------------------------------------------
// <copyright file="AirportSharedDatasetDataService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Airport shared dataset data service providing business operations for SMS domain entities.
//                  Infrastructure layer service implementing data access patterns
//                  through repositories while maintaining clean architecture.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using SMS_Infrastructure.Interfaces;

namespace SMS_Infrastructure.Services;

/// <summary>
/// Airport Shared Dataset Data Service providing business operations for AirportSharedDataset entities
/// </summary>
public class AirportSharedDatasetDataService : BaseDataService<AirportSharedDatasetDataService>, IAirportSharedDatasetDataService
{
    private readonly ILogger<AirportSharedDatasetDataService> _logger;
    private readonly string _logheader;
    private readonly AirportSharedDatasetRepository _repo;

    public AirportSharedDatasetDataService(ILogger<AirportSharedDatasetDataService> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, AirportSharedDatasetRepository repo)
        : base(logger, serviceScopeFactory, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _repo = repo;

        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} {repo.GetType().Name}");
    }

    public Task<Result<AirportSharedDataset>> CreateAirportSharedDatasetAsync(AirportSharedDataset airportSharedDataset, CancellationToken ct = default)
    {
        return _repo.CreateAirportSharedDatasetAsync(airportSharedDataset, ct);
    }

    public Task<Result<AirportSharedDataset>> GetAirportSharedDatasetByCodeAsync(AirportSharedDatasetID code, CancellationToken ct = default)
    {
        return _repo.GetAirportSharedDatasetByCodeAsync(code, ct);
    }

    public Task<Result<List<AirportSharedDataset>>> GetAllAirportSharedDatasetsAsync(CancellationToken ct = default)
    {
        return _repo.GetAllAirportSharedDatasetsAsync(ct);
    }

    public Task<Result<AirportSharedDataset>> UpdateAirportSharedDatasetAsync(AirportSharedDataset airportSharedDataset, CancellationToken ct = default)
    {
        return _repo.UpdateAirportSharedDatasetAsync(airportSharedDataset, ct);
    }

    public Task<Result<bool>> DeleteAirportSharedDatasetAsync(AirportSharedDatasetID id, CancellationToken ct = default)
    {
        return _repo.DeleteAirportSharedDatasetAsync(id, ct);
    }
}

