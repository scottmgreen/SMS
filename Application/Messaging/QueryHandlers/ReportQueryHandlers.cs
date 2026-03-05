//-----------------------------------------------------------------------
// <copyright file="ReportQueryHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query handlers implementing SMS report data retrieval and search logic.
//                  Implements query handlers for processing read operations.
//                  Retrieves and transforms data for presentation layer consumption.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.Messaging.Queries;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// REPORT QUERY HANDLERS - Clean Architecture Pattern
// =============================================

public class GetReportByCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetReportByCodeQuery, Result<Report>>
{
    private readonly ReportService _reportService;
    private readonly ILogger<GetReportByCodeQueryHandler> _logger;

    public GetReportByCodeQueryHandler(ReportService reportService, ILogger<GetReportByCodeQueryHandler> logger)
    {
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Report>> HandleAsync(GetReportByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("✅ Clean Architecture: Processing GetReportByCodeQuery for Code: {Code}", request.ReportCode);
            var result = await _reportService.GetReportByCodeAsync(request.ReportCode, ct).ConfigureAwait(false);
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
    private readonly ReportService _reportService;
    private readonly ILogger<GetAllReportsQueryHandler> _logger;

    public GetAllReportsQueryHandler(ReportService reportService, ILogger<GetAllReportsQueryHandler> logger)
    {
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<Report>>> HandleAsync(GetAllReportsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("✅ Clean Architecture: Processing GetAllReportsQuery");
            var result = await _reportService.GetAllReportsAsync(ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetAllReportsQuery", ApplicationEventIds.Error, ex);
            return Result<List<Report>>.Failure<List<Report>>(DomainErrors.ReportError.NullOrEmpty);
        }
    }
}
