using Microsoft.Extensions.Logging;
using SMS_Application.Messaging.Queries;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// SCORING PANEL QUERY HANDLERS
// =============================================

public class GetScoringPanelByIdQueryHandler : BaseQueryBundle, IRequestHandler<GetScoringPanelByIdQuery, Result<ScoringPanel>>
{
    private readonly ScoringPanelDataService _scoringPanelDataService;
    private readonly ILogger<GetScoringPanelByIdQueryHandler> _logger;

    public GetScoringPanelByIdQueryHandler(ScoringPanelDataService scoringPanelDataService, ILogger<GetScoringPanelByIdQueryHandler> logger)
    {
        _scoringPanelDataService = scoringPanelDataService ?? throw new ArgumentNullException(nameof(scoringPanelDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ScoringPanel>> HandleAsync(GetScoringPanelByIdQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetScoringPanelByIdQuery for ID: {Id}", request.ScoringPanelId);
            var result = await _scoringPanelDataService.GetScoringPanelByIdAsync(request.ScoringPanelId, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetScoringPanelByIdQuery for ID: {Id}", request.ScoringPanelId);
            return Result<ScoringPanel>.Failure<ScoringPanel>(DomainErrors.ScoringPanelError.NotFound);
        }
    }
}

public class GetAllScoringPanelsQueryHandler : BaseQueryBundle, IRequestHandler<GetAllScoringPanelsQuery, Result<List<ScoringPanel>>>
{
    private readonly ScoringPanelService _scoringPanelService;
    private readonly ILogger<GetAllScoringPanelsQueryHandler> _logger;

    public GetAllScoringPanelsQueryHandler(ScoringPanelService scoringPanelDataService, ILogger<GetAllScoringPanelsQueryHandler> logger)
    {
        _scoringPanelService = scoringPanelDataService ?? throw new ArgumentNullException(nameof(scoringPanelDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<ScoringPanel>>> HandleAsync(GetAllScoringPanelsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetAllScoringPanelsQuery");
            var result = await _scoringPanelService.GetAllScoringPanelsAsync(ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetAllScoringPanelsQuery");
            return Result<List<ScoringPanel>>.Failure<List<ScoringPanel>>(DomainErrors.ScoringPanelError.NullOrEmpty);
        }
    }
}

public class GetScoringPanelsByHazardCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetScoringPanelsByHazardCodeQuery, Result<List<ScoringPanel>>>
{
    private readonly ScoringPanelDataService _scoringPanelDataService;
    private readonly ILogger<GetScoringPanelsByHazardCodeQueryHandler> _logger;

    public GetScoringPanelsByHazardCodeQueryHandler(ScoringPanelDataService scoringPanelDataService, ILogger<GetScoringPanelsByHazardCodeQueryHandler> logger)
    {
        _scoringPanelDataService = scoringPanelDataService ?? throw new ArgumentNullException(nameof(scoringPanelDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<ScoringPanel>>> HandleAsync(GetScoringPanelsByHazardCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetScoringPanelsByHazardCodeQuery for HazardCode: {HazardCode}", request.HazardCode);
            var result = await _scoringPanelDataService.GetScoringPanelsByHazardCodeAsync(request.HazardCode, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetScoringPanelsByHazardCodeQuery for HazardCode: {HazardCode}", request.HazardCode);
            return Result<List<ScoringPanel>>.Failure<List<ScoringPanel>>(DomainErrors.ScoringPanelError.NullOrEmpty);
        }
    }
}