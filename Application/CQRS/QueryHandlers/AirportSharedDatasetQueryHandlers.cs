//-----------------------------------------------------------------------
// <copyright file="AirportSharedDatasetQueryHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query handlers implementing data retrieval logic for SMS read operations.
//                  Implements query handlers for processing read operations.
//                  Retrieves and transforms data for presentation layer consumption.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using Microsoft.Extensions.Logging;

using SMS_Application.Messaging.Queries;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// AIRPORT SHARED DATASET QUERY HANDLERS
// =============================================

public class GetAirportSharedDatasetByIdQueryHandler : BaseQueryBundle, IRequestHandler<GetAirportSharedDatasetByCodeQuery, Result<AirportSharedDataset>>
{
    private readonly AirportSharedDatasetDataService _airportSharedDatasetDataService;
    private readonly ILogger<GetAirportSharedDatasetByIdQueryHandler> _logger;

    public GetAirportSharedDatasetByIdQueryHandler(AirportSharedDatasetDataService airportSharedDatasetDataService, ILogger<GetAirportSharedDatasetByIdQueryHandler> logger)
    {
        _airportSharedDatasetDataService = airportSharedDatasetDataService ?? throw new ArgumentNullException(nameof(airportSharedDatasetDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<AirportSharedDataset>> HandleAsync(GetAirportSharedDatasetByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetAirportSharedDatasetByCodeQuery for Code: {Code}", request.AirportSharedDatasetCode);
            var result = await _airportSharedDatasetDataService.GetAirportSharedDatasetByCodeAsync(request.AirportSharedDatasetCode, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetAirportSharedDatasetByCodeQuery for Code: {Code}", ApplicationEventIds.Error, ex);
            return Result<AirportSharedDataset>.Failure<AirportSharedDataset>(DomainErrors.AirportSharedDatasetError.NotFound);
        }
    }
}

public class GetAllAirportSharedDatasetsQueryHandler : BaseQueryBundle, IRequestHandler<GetAllAirportSharedDatasetsQuery, Result<List<AirportSharedDataset>>>
{
    private readonly AirportSharedDatasetDataService _airportSharedDatasetDataService;
    private readonly ILogger<GetAllAirportSharedDatasetsQueryHandler> _logger;

    public GetAllAirportSharedDatasetsQueryHandler(AirportSharedDatasetDataService airportSharedDatasetDataService, ILogger<GetAllAirportSharedDatasetsQueryHandler> logger)
    {
        _airportSharedDatasetDataService = airportSharedDatasetDataService ?? throw new ArgumentNullException(nameof(airportSharedDatasetDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<AirportSharedDataset>>> HandleAsync(GetAllAirportSharedDatasetsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetAllAirportSharedDatasetsQuery");
            var result = await _airportSharedDatasetDataService.GetAllAirportSharedDatasetsAsync(ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetAllAirportSharedDatasetsQuery", ApplicationEventIds.Error, ex);
            return Result<List<AirportSharedDataset>>.Failure<List<AirportSharedDataset>>(DomainErrors.AirportSharedDatasetError.NullOrEmpty);
        }
    }
}
