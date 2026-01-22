using Microsoft.Extensions.Logging;

using SMS_Application.Messaging.Queries;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// MITIGATION QUERY HANDLERS
// =============================================

public class GetMitigationByCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetMitigationByCodeQuery, Result<Mitigation>>
{
    private readonly MitigationService _appService;
    private readonly ILogger<GetMitigationByCodeQueryHandler> _logger;

    public GetMitigationByCodeQueryHandler(MitigationService appService, ILogger<GetMitigationByCodeQueryHandler> logger)
    {
        _appService = appService ?? throw new ArgumentNullException(nameof(appService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Mitigation>> HandleAsync(GetMitigationByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            if (request?.MitigationId is null)
            {
                _logger.LogApplicationError("GetMitigationByIdQuery received with null MitigationId", ApplicationEventIds.Error, null);
                return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.NotFound);
            }

            _logger.LogInformation("Processing GetMitigationByIdQuery for ID: {Id}", request.MitigationId.Value);

            var result = await _appService.GetMitigationByCodeAsync(request.MitigationId, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved Mitigation with ID: {Id}", request.MitigationId.Value);
            }
            else
            {
                _logger.LogApplicationError("Failed to retrieve Mitigation with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetMitigationByIdQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while retrieving Mitigation", ApplicationEventIds.Error, ex);
            return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.NotFound);
        }
    }
}

public class GetAllMitigationsQueryHandler : BaseQueryBundle, IRequestHandler<GetAllMitigationsQuery, Result<List<Mitigation>>>
{
    private readonly MitigationDataService _dataService;
    private readonly ILogger<GetAllMitigationsQueryHandler> _logger;

    public GetAllMitigationsQueryHandler(MitigationDataService dataService, ILogger<GetAllMitigationsQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<Mitigation>>> HandleAsync(GetAllMitigationsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetAllMitigationsQuery");

            var result = await _dataService.GetAllMitigationsAsync(ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} Mitigations", result.Value?.Count ?? 0);
            }
            else
            {
                _logger.LogApplicationError("Failed to retrieve Mitigations. Error: {Error}", ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetAllMitigationsQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while retrieving all Mitigations", ApplicationEventIds.Error, ex);
            return Result<List<Mitigation>>.Failure<List<Mitigation>>(DomainErrors.MitigationError.NotFound);
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
            _logger.LogApplicationError("Error processing GetMitigationsByHazardCodeQuery for HazardCode: {HazardCode}", ApplicationEventIds.Error, ex);
            return Result<List<Mitigation>>.Failure<List<Mitigation>>(DomainErrors.MitigationError.NullOrEmpty);
        }
    }
}