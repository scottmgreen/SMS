using Microsoft.Extensions.Logging;
using SMS_Infrastructure.Services;
using SMS_Application.Messaging.Queries;
using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Application.Interfaces;
using SMS_Shared.Common;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// HAZARD LOCATION QUERY HANDLERS - Following Exact SMS Pattern
// =============================================

public class GetHazardLocationByIdQueryHandler : BaseQueryBundle, IRequestHandler<GetHazardLocationByIdQuery, Result<HazardLocation>>
{
    private readonly HazardLocationDataService _hazardLocationDataService;
    private readonly ILogger<GetHazardLocationByIdQueryHandler> _logger;

    public GetHazardLocationByIdQueryHandler(HazardLocationDataService hazardLocationDataService, ILogger<GetHazardLocationByIdQueryHandler> logger)
    {
        _hazardLocationDataService = hazardLocationDataService ?? throw new ArgumentNullException(nameof(hazardLocationDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<HazardLocation>> HandleAsync(GetHazardLocationByIdQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetHazardLocationByIdQuery for HazardLocationId: {HazardLocationId}", request.HazardLocationId);
            var result = await _hazardLocationDataService.GetHazardLocationByIdAsync(request.HazardLocationId, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetHazardLocationByIdQuery for HazardLocationId: {HazardLocationId}", request.HazardLocationId);
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
            _logger.LogError(ex, "Error processing GetAllHazardLocationsQuery");
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
            _logger.LogError(ex, "Error processing GetHazardLocationsByHazardCodeQuery for HazardCode: {HazardCode}", request.HazardCode);
            return Result<List<HazardLocation>>.Failure<List<HazardLocation>>(DomainErrors.HazardLocationError.NullOrEmpty);
        }
    }
}