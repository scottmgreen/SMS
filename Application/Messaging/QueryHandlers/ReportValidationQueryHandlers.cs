using Microsoft.Extensions.Logging;
using SMS_Application.Messaging.Queries;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// REPORT VALIDATION QUERY HANDLERS
// =============================================

public class GetReportValidationByIdQueryHandler : BaseQueryBundle, IRequestHandler<GetReportValidationByIdQuery, Result<ReportValidation>>
{
    private readonly ReportValidationDataService _reportValidationDataService;
    private readonly ILogger<GetReportValidationByIdQueryHandler> _logger;

    public GetReportValidationByIdQueryHandler(ReportValidationDataService reportValidationDataService, ILogger<GetReportValidationByIdQueryHandler> logger)
    {
        _reportValidationDataService = reportValidationDataService ?? throw new ArgumentNullException(nameof(reportValidationDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ReportValidation>> HandleAsync(GetReportValidationByIdQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetReportValidationByIdQuery for ID: {Id}", request.ReportValidationId);
            var result = await _reportValidationDataService.GetReportValidationByIdAsync(request.ReportValidationId, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetReportValidationByIdQuery for ID: {Id}", request.ReportValidationId);
            return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportError.NotFound);
        }
    }
}

public class GetAllReportValidationsQueryHandler : BaseQueryBundle, IRequestHandler<GetAllReportValidationsQuery, Result<List<ReportValidation>>>
{
    private readonly ReportValidationDataService _reportValidationDataService;
    private readonly ILogger<GetAllReportValidationsQueryHandler> _logger;

    public GetAllReportValidationsQueryHandler(ReportValidationDataService reportValidationDataService, ILogger<GetAllReportValidationsQueryHandler> logger)
    {
        _reportValidationDataService = reportValidationDataService ?? throw new ArgumentNullException(nameof(reportValidationDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<ReportValidation>>> HandleAsync(GetAllReportValidationsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetAllReportValidationsQuery");
            var result = await _reportValidationDataService.GetAllReportValidationsAsync(ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetAllReportValidationsQuery");
            return Result<List<ReportValidation>>.Failure<List<ReportValidation>>(DomainErrors.ReportError.NullOrEmpty);
        }
    }
}