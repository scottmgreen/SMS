using Microsoft.Extensions.Logging;
using SMS_Application.Messaging.Queries;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// REPORT QUERY HANDLERS
// =============================================

public class GetReportByIdQueryHandler : BaseQueryBundle, IRequestHandler<GetReportByIdQuery, Result<Report>>
{
    private readonly ReportDataService _reportDataService;
    private readonly ILogger<GetReportByIdQueryHandler> _logger;

    public GetReportByIdQueryHandler(ReportDataService reportDataService, ILogger<GetReportByIdQueryHandler> logger)
    {
        _reportDataService = reportDataService ?? throw new ArgumentNullException(nameof(reportDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Report>> HandleAsync(GetReportByIdQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetReportByIdQuery for ID: {Id}", request.ReportId);
            var result = await _reportDataService.GetReportByIdAsync(request.ReportId, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetReportByIdQuery for ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<Report>.Failure<Report>(DomainErrors.ReportError.NotFound);
        }
    }
}

public class GetAllReportsQueryHandler : BaseQueryBundle, IRequestHandler<GetAllReportsQuery, Result<List<Report>>>
{
    private readonly ReportDataService _reportDataService;
    private readonly ILogger<GetAllReportsQueryHandler> _logger;

    public GetAllReportsQueryHandler(ReportDataService reportDataService, ILogger<GetAllReportsQueryHandler> logger)
    {
        _reportDataService = reportDataService ?? throw new ArgumentNullException(nameof(reportDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<Report>>> HandleAsync(GetAllReportsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetAllReportsQuery");
            var result = await _reportDataService.GetAllReportsAsync(ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetAllReportsQuery", ApplicationEventIds.Error, ex);
            return Result<List<Report>>.Failure<List<Report>>(DomainErrors.ReportError.NullOrEmpty);
        }
    }
}