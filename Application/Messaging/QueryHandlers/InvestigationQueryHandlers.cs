using Microsoft.Extensions.Logging;
using SMS_Application.Messaging.Queries;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// INVESTIGATION QUERY HANDLERS
// =============================================

public class GetInvestigationByIdQueryHandler : BaseQueryBundle, IRequestHandler<GetInvestigationByIdQuery, Result<Investigation>>
{
    private readonly InvestigationDataService _investigationDataService;
    private readonly ILogger<GetInvestigationByIdQueryHandler> _logger;

    public GetInvestigationByIdQueryHandler(InvestigationDataService investigationDataService, ILogger<GetInvestigationByIdQueryHandler> logger)
    {
        _investigationDataService = investigationDataService ?? throw new ArgumentNullException(nameof(investigationDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Investigation>> HandleAsync(GetInvestigationByIdQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetInvestigationByIdQuery for ID: {Id}", request.InvestigationId);
            var result = await _investigationDataService.GetInvestigationByIdAsync(request.InvestigationId, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetInvestigationByIdQuery for ID: {Id}", request.InvestigationId);
            return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.NotFound);
        }
    }
}

public class GetAllInvestigationsQueryHandler : BaseQueryBundle, IRequestHandler<GetAllInvestigationsQuery, Result<List<Investigation>>>
{
    private readonly InvestigationDataService _investigationDataService;
    private readonly ILogger<GetAllInvestigationsQueryHandler> _logger;

    public GetAllInvestigationsQueryHandler(InvestigationDataService investigationDataService, ILogger<GetAllInvestigationsQueryHandler> logger)
    {
        _investigationDataService = investigationDataService ?? throw new ArgumentNullException(nameof(investigationDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<Investigation>>> HandleAsync(GetAllInvestigationsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetAllInvestigationsQuery");
            var result = await _investigationDataService.GetAllInvestigationsAsync(ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetAllInvestigationsQuery");
            return Result<List<Investigation>>.Failure<List<Investigation>>(DomainErrors.InvestigationError.NullOrEmpty);
        }
    }
}