//-----------------------------------------------------------------------
// <copyright file="HazardQueryHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query handlers implementing SMS hazard data retrieval and analysis logic.
//                  Implements query handlers for processing read operations.
//                  Retrieves and transforms data for presentation layer consumption.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.Messaging.Queries;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// HAZARD QUERY HANDLERS - Clean Architecture Pattern
// =============================================

public class GetHazardByIdQueryHandler : BaseQueryBundle, IRequestHandler<GetHazardByCodeQuery, Result<Hazard>>
{
    private readonly HazardService _hazardService;
    private readonly ILogger<GetHazardByIdQueryHandler> _logger;

    public GetHazardByIdQueryHandler(HazardService hazardService, ILogger<GetHazardByIdQueryHandler> logger)
    {
        _hazardService = hazardService ?? throw new ArgumentNullException(nameof(hazardService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Hazard>> HandleAsync(GetHazardByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("✅ Clean Architecture: Processing GetHazardByIdQuery for Code: {Code}", request.HazardId);
            var result = await _hazardService.GetHazardByCodeAsync(new HazardID(request.HazardId.Value), ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetHazardByIdQuery for Code: {Code}", ApplicationEventIds.Error, ex);
            return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.NotFound);
        }
    }
}

public class GetAllHazardsQueryHandler : BaseQueryBundle, IRequestHandler<GetAllHazardsQuery, Result<List<Hazard>>>
{
    private readonly HazardService _hazardService;
    private readonly ILogger<GetAllHazardsQueryHandler> _logger;

    public GetAllHazardsQueryHandler(HazardService hazardService, ILogger<GetAllHazardsQueryHandler> logger)
    {
        _hazardService = hazardService ?? throw new ArgumentNullException(nameof(hazardService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<Hazard>>> HandleAsync(GetAllHazardsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("✅ Clean Architecture: Processing GetAllHazardsQuery");
            var result = await _hazardService.GetAllHazardsAsync(ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetAllHazardsQuery", ApplicationEventIds.Error, ex);
            return Result<List<Hazard>>.Failure<List<Hazard>>(DomainErrors.HazardError.NullOrEmpty);
        }
    }
}

//public class GetHazardsByReportIdQueryHandler : BaseQueryBundle, IRequestHandler<GetHazardsByReportCodeQuery, Result<List<Hazard>>>
//{
//    private readonly HazardService _hazardService;
//    private readonly ILogger<GetHazardsByReportIdQueryHandler> _logger;

//    public GetHazardsByReportIdQueryHandler(HazardService hazardService, ILogger<GetHazardsByReportIdQueryHandler> logger)
//    {
//        _hazardService = hazardService ?? throw new ArgumentNullException(nameof(hazardService));
//        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//    }

//    public async Task<Result<List<Hazard>>> HandleAsync(GetHazardsByReportCodeQuery request, CancellationToken ct = default)
//    {
//        try
//        {
//            ReportID reportid = request.ReportId;
//            _logger.LogInformation("✅ Clean Architecture: Processing GetAllHazardsByReportIdQuery");
//            var result = await _hazardService.GetAllHazardsByReportCodeAsync(new ReportID(request.ReportId.Value), ct).ConfigureAwait(false);
//            return result;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogApplicationError("Error processing GetAllHazardsQuery", ApplicationEventIds.Error, ex);
//            return Result<List<Hazard>>.Failure<List<Hazard>>(DomainErrors.HazardError.NullOrEmpty);
//        }
//    }
//}

public class GetHazardsByReportCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetHazardsByReportCodeQuery, Result<List<Hazard>>>
{
    private readonly HazardService _hazardService;
    private readonly ILogger<GetHazardsByReportCodeQueryHandler> _logger;

    public GetHazardsByReportCodeQueryHandler(HazardService hazardService, ILogger<GetHazardsByReportCodeQueryHandler> logger)
    {
        _hazardService = hazardService ?? throw new ArgumentNullException(nameof(hazardService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<Hazard>>> HandleAsync(GetHazardsByReportCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("✅ Clean Architecture: Processing GetHazardsByReportCodeQuery for ReportCode: {ReportCode}", request.ReportId.Value);
            var result = await _hazardService.GetHazardsByReportCodeAsync(request.ReportId, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetHazardsByReportCodeQuery for ReportCode: {ReportCode}", ApplicationEventIds.Error, ex);
            return Result<List<Hazard>>.Failure<List<Hazard>>(DomainErrors.HazardError.NullOrEmpty);
        }
    }
}
