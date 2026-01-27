using Microsoft.Extensions.Logging;

using SMS_Application.Messaging.Queries;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// REPORT QUERY HANDLERS
// =============================================

public class GetReportByCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetReportByCodeQuery, Result<Report>>
{
    private readonly ReportDataService _reportDataService;
    private readonly ILogger<GetReportByCodeQueryHandler> _logger;

    public GetReportByCodeQueryHandler(ReportDataService reportDataService, ILogger<GetReportByCodeQueryHandler> logger)
    {
        _reportDataService = reportDataService ?? throw new ArgumentNullException(nameof(reportDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Report>> HandleAsync(GetReportByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetReportByCodeQuery for Code: {Code}", request.ReportCode);
            var result = await _reportDataService.GetReportByCodeAsync(request.ReportCode, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetReportByCodeQuery for Code: {Code}", ApplicationEventIds.Error, ex);
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