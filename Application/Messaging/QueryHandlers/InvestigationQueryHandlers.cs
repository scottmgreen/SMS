using Microsoft.Extensions.Logging;
using SMS_Application.Messaging.Queries;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// INVESTIGATION QUERY HANDLERS
// =============================================

public class GetInvestigationByCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetInvestigationByCodeQuery, Result<Investigation>>
{
    private readonly InvestigationDataService _investigationDataService;
    private readonly ILogger<GetInvestigationByCodeQueryHandler> _logger;

    public GetInvestigationByCodeQueryHandler(InvestigationDataService investigationDataService, ILogger<GetInvestigationByCodeQueryHandler> logger)
    {
        _investigationDataService = investigationDataService ?? throw new ArgumentNullException(nameof(investigationDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Investigation>> HandleAsync(GetInvestigationByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetInvestigationByCodeQuery for Code: {Code}", request.InvestigationId.Value);
            // Use the string overload since that's what the service has implemented
            var result = await _investigationDataService.GetInvestigationByCodeAsync(request.InvestigationId.Value, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError($"Error processing GetInvestigationByCodeQuery for Code: {request.InvestigationId.Value}", ApplicationEventIds.Error, ex);
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
            _logger.LogApplicationError("Error processing GetAllInvestigationsQuery", ApplicationEventIds.Error, ex);
            return Result<List<Investigation>>.Failure<List<Investigation>>(DomainErrors.InvestigationError.NullOrEmpty);
        }
    }
}