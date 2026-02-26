//-----------------------------------------------------------------------
// <copyright file="HazardLocationQueryHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query handlers implementing SMS hazard data retrieval and analysis logic.
//                  Implements query handlers for processing read operations.
//                  Retrieves and transforms data for presentation layer consumption.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;

using SMS_Application.Messaging.Queries;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// HAZARD LOCATION QUERY HANDLERS - Following Exact SMS Pattern
// =============================================

public class GetHazardLocationByCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetHazardLocationByCodeQuery, Result<HazardLocation>>
{
    private readonly HazardLocationDataService _hazardLocationDataService;
    private readonly ILogger<GetHazardLocationByCodeQueryHandler> _logger;

    public GetHazardLocationByCodeQueryHandler(HazardLocationDataService hazardLocationDataService, ILogger<GetHazardLocationByCodeQueryHandler> logger)
    {
        _hazardLocationDataService = hazardLocationDataService ?? throw new ArgumentNullException(nameof(hazardLocationDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<HazardLocation>> HandleAsync(GetHazardLocationByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetHazardLocationByIdQuery for HazardLocationId: {HazardLocationId}", request.HazardLocationId);
            var result = await _hazardLocationDataService.GetHazardLocationByCodeAsync(request.HazardLocationId, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetHazardLocationByIdQuery for HazardLocationId: {HazardLocationId}", ApplicationEventIds.Error, ex);
            return Result<HazardLocation>.Failure<HazardLocation>(DomainErrors.HazardLocationError.NotFound);
        }
    }
}

public class GetAllHazardLocationsQueryHandler : BaseQueryBundle, IRequestHandler<GetAllHazardLocationsQuery, Result<List<HazardLocation>>>
{
    private readonly HazardLocationDataService _hazardLocationDataService;
    private readonly ILogger<GetAllHazardLocationsQueryHandler> _logger;

    public GetAllHazardLocationsQueryHandler(HazardLocationDataService hazardLocationDataService, ILogger<GetAllHazardLocationsQueryHandler> logger)
    {
        _hazardLocationDataService = hazardLocationDataService ?? throw new ArgumentNullException(nameof(hazardLocationDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<HazardLocation>>> HandleAsync(GetAllHazardLocationsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetAllHazardLocationsQuery");
            var result = await _hazardLocationDataService.GetAllHazardLocationsAsync(ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetAllHazardLocationsQuery", ApplicationEventIds.Error, ex);
            return Result<List<HazardLocation>>.Failure<List<HazardLocation>>(DomainErrors.HazardLocationError.NullOrEmpty);
        }
    }
}

public class GetHazardLocationsByHazardCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetHazardLocationsByHazardCodeQuery, Result<List<HazardLocation>>>
{
    private readonly HazardLocationDataService _hazardLocationDataService;
    private readonly ILogger<GetHazardLocationsByHazardCodeQueryHandler> _logger;

    public GetHazardLocationsByHazardCodeQueryHandler(HazardLocationDataService hazardLocationDataService, ILogger<GetHazardLocationsByHazardCodeQueryHandler> logger)
    {
        _hazardLocationDataService = hazardLocationDataService ?? throw new ArgumentNullException(nameof(hazardLocationDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<HazardLocation>>> HandleAsync(GetHazardLocationsByHazardCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetHazardLocationsByHazardCodeQuery for HazardCode: {HazardCode}", request.HazardCode);
            var result = await _hazardLocationDataService.GetHazardLocationsByHazardCodeAsync(request.HazardCode, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetHazardLocationsByHazardCodeQuery for HazardCode: {HazardCode}", ApplicationEventIds.Error, ex);
            return Result<List<HazardLocation>>.Failure<List<HazardLocation>>(DomainErrors.HazardLocationError.NullOrEmpty);
        }
    }
}
