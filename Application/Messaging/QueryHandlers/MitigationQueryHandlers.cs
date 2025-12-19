using Microsoft.Extensions.Logging;
using SMS_Application.Messaging.Queries;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// MITIGATION QUERY HANDLERS
// =============================================

public class GetMitigationByIdQueryHandler : BaseQueryBundle, IRequestHandler<GetMitigationByIdQuery, Result<Mitigation>>
{
    private readonly MitigationDataService _mitigationDataService;
    private readonly ILogger<GetMitigationByIdQueryHandler> _logger;

    public GetMitigationByIdQueryHandler(MitigationDataService mitigationDataService, ILogger<GetMitigationByIdQueryHandler> logger)
    {
        _mitigationDataService = mitigationDataService ?? throw new ArgumentNullException(nameof(mitigationDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Mitigation>> HandleAsync(GetMitigationByIdQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetMitigationByIdQuery for ID: {Id}", request.MitigationId);
            var result = await _mitigationDataService.GetMitigationByIdAsync(request.MitigationId, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetMitigationByIdQuery for ID: {Id}", request.MitigationId);
            return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.NotFound);
        }
    }
}

public class GetAllMitigationsQueryHandler : BaseQueryBundle, IRequestHandler<GetAllMitigationsQuery, Result<List<Mitigation>>>
{
    private readonly MitigationDataService _mitigationDataService;
    private readonly ILogger<GetAllMitigationsQueryHandler> _logger;

    public GetAllMitigationsQueryHandler(MitigationDataService mitigationDataService, ILogger<GetAllMitigationsQueryHandler> logger)
    {
        _mitigationDataService = mitigationDataService ?? throw new ArgumentNullException(nameof(mitigationDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<Mitigation>>> HandleAsync(GetAllMitigationsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetAllMitigationsQuery");
            var result = await _mitigationDataService.GetAllMitigationsAsync(ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetAllMitigationsQuery");
            return Result<List<Mitigation>>.Failure<List<Mitigation>>(DomainErrors.MitigationError.NullOrEmpty);
        }
    }
}

/// <summary>
/// NEW: Handler for getting mitigations by hazard code
/// </summary>
public class GetMitigationsByHazardCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetMitigationsByHazardCodeQuery, Result<List<Mitigation>>>
{
    private readonly MitigationDataService _mitigationDataService;
    private readonly ILogger<GetMitigationsByHazardCodeQueryHandler> _logger;

    public GetMitigationsByHazardCodeQueryHandler(MitigationDataService mitigationDataService, ILogger<GetMitigationsByHazardCodeQueryHandler> logger)
    {
        _mitigationDataService = mitigationDataService ?? throw new ArgumentNullException(nameof(mitigationDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<Mitigation>>> HandleAsync(GetMitigationsByHazardCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetMitigationsByHazardCodeQuery for HazardCode: {HazardCode}", request.HazardCode);
            var result = await _mitigationDataService.GetMitigationsByHazardCodeAsync(request.HazardCode, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetMitigationsByHazardCodeQuery for HazardCode: {HazardCode}", request.HazardCode);
            return Result<List<Mitigation>>.Failure<List<Mitigation>>(DomainErrors.MitigationError.NullOrEmpty);
        }
    }
}