using Microsoft.Extensions.Logging;
using SMS_Application.Messaging.Queries;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// HAZARD QUERY HANDLERS
// =============================================

public class GetHazardByIdQueryHandler : BaseQueryBundle, IRequestHandler<GetHazardByIdQuery, Result<Hazard>>
{
    private readonly HazardDataService _hazardDataService;
    private readonly ILogger<GetHazardByIdQueryHandler> _logger;

    public GetHazardByIdQueryHandler(HazardDataService hazardDataService, ILogger<GetHazardByIdQueryHandler> logger)
    {
        _hazardDataService = hazardDataService ?? throw new ArgumentNullException(nameof(hazardDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Hazard>> HandleAsync(GetHazardByIdQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetHazardByIdQuery for ID: {Id}", request.HazardId);
            var result = await _hazardDataService.GetHazardByIdAsync(request.HazardId, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetHazardByIdQuery for ID: {Id}", request.HazardId);
            return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.NotFound);
        }
    }
}

public class GetAllHazardsQueryHandler : BaseQueryBundle, IRequestHandler<GetAllHazardsQuery, Result<List<Hazard>>>
{
    private readonly HazardDataService _hazardDataService;
    private readonly ILogger<GetAllHazardsQueryHandler> _logger;

    public GetAllHazardsQueryHandler(HazardDataService hazardDataService, ILogger<GetAllHazardsQueryHandler> logger)
    {
        _hazardDataService = hazardDataService ?? throw new ArgumentNullException(nameof(hazardDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<Hazard>>> HandleAsync(GetAllHazardsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetAllHazardsQuery");
            var result = await _hazardDataService.GetAllHazardsAsync(ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetAllHazardsQuery");
            return Result<List<Hazard>>.Failure<List<Hazard>>(DomainErrors.HazardError.NullOrEmpty);
        }
    }
}