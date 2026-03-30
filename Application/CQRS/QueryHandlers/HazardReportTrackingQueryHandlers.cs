//-----------------------------------------------------------------------
// <copyright file="HazardReportTrackingQueryHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query handlers implementing SMS hazard data retrieval and analysis logic.
//                  Implements query handlers for processing read operations.
//                  Retrieves and transforms data for presentation layer consumption.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using Microsoft.Extensions.Logging;

using SMS_Application.Messaging.Queries;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// HAZARD REPORT TRACKING QUERY HANDLERS
// =============================================

public class GetHazardReportTrackingByIdQueryHandler : BaseQueryBundle, IRequestHandler<GetHazardReportTrackingByIdQuery, Result<HazardReportTracking>>
{
    private readonly HazardReportTrackingService _service;
    private readonly ILogger<GetHazardReportTrackingByIdQueryHandler> _logger;

    public GetHazardReportTrackingByIdQueryHandler(
        HazardReportTrackingService service,
        ILogger<GetHazardReportTrackingByIdQueryHandler> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<HazardReportTracking>> HandleAsync(GetHazardReportTrackingByIdQuery request, CancellationToken ct = default)
    {
        try
        {
            if (request?.HazardReportTrackingId is null)
            {
                _logger.LogApplicationError("GetHazardReportTrackingByIdQuery received with null HazardReportTrackingId", ApplicationEventIds.Error, null);
                return Result<HazardReportTracking>.Failure<HazardReportTracking>(DomainErrors.HazardReportTrackingError.NullOrEmpty);
            }

            _logger.LogInformation("Processing GetHazardReportTrackingByIdQuery for ID: {Id}", request.HazardReportTrackingId.Value);

            return await _service.GetHazardReportTrackingByIdAsync(request.HazardReportTrackingId, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetHazardReportTrackingByIdQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while retrieving HazardReportTracking by ID", ApplicationEventIds.Error, ex);
            return Result<HazardReportTracking>.Failure<HazardReportTracking>(DomainErrors.HazardReportTrackingError.NotFound);
        }
    }
}

public class GetAllHazardReportTrackingQueryHandler : BaseQueryBundle, IRequestHandler<GetAllHazardReportTrackingQuery, Result<List<HazardReportTracking>>>
{
    private readonly HazardReportTrackingService _service;
    private readonly ILogger<GetAllHazardReportTrackingQueryHandler> _logger;

    public GetAllHazardReportTrackingQueryHandler(
        HazardReportTrackingService service,
        ILogger<GetAllHazardReportTrackingQueryHandler> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<HazardReportTracking>>> HandleAsync(GetAllHazardReportTrackingQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetAllHazardReportTrackingQuery");

            return await _service.GetAllHazardReportTrackingAsync(ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetAllHazardReportTrackingQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while retrieving all HazardReportTracking records", ApplicationEventIds.Error, ex);
            return Result<List<HazardReportTracking>>.Failure<List<HazardReportTracking>>(DomainErrors.HazardReportTrackingError.NullOrEmpty);
        }
    }
}

public class GetHazardReportTrackingByTrackingCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetHazardReportTrackingByTrackingCodeQuery, Result<HazardReportTrackingDetails>>
{
    private readonly HazardReportTrackingService _service;
    private readonly ILogger<GetHazardReportTrackingByTrackingCodeQueryHandler> _logger;

    public GetHazardReportTrackingByTrackingCodeQueryHandler(
        HazardReportTrackingService service,
        ILogger<GetHazardReportTrackingByTrackingCodeQueryHandler> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<HazardReportTrackingDetails>> HandleAsync(GetHazardReportTrackingByTrackingCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request?.TrackingCode))
            {
                _logger.LogApplicationError("GetHazardReportTrackingByTrackingCodeQuery received with null or empty TrackingCode", ApplicationEventIds.Error, null);
                return Result<HazardReportTrackingDetails>.Failure<HazardReportTrackingDetails>(DomainErrors.HazardReportTrackingError.NullOrEmpty);
            }

            _logger.LogInformation("Processing GetHazardReportTrackingByTrackingCodeQuery for TrackingCode: {TrackingCode}", request.TrackingCode);

            return await _service.GetHazardReportTrackingDetailsByTrackingCodeAsync(request.TrackingCode, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetHazardReportTrackingByTrackingCodeQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while retrieving HazardReportTracking by TrackingCode", ApplicationEventIds.Error, ex);
            return Result<HazardReportTrackingDetails>.Failure<HazardReportTrackingDetails>(DomainErrors.HazardReportTrackingError.NotFound);
        }
    }
}

public class GetHazardReportTrackingByHazardCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetHazardReportTrackingByHazardCodeQuery, Result<List<HazardReportTracking>>>
{
    private readonly HazardReportTrackingService _service;
    private readonly ILogger<GetHazardReportTrackingByHazardCodeQueryHandler> _logger;

    public GetHazardReportTrackingByHazardCodeQueryHandler(
        HazardReportTrackingService service,
        ILogger<GetHazardReportTrackingByHazardCodeQueryHandler> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<HazardReportTracking>>> HandleAsync(GetHazardReportTrackingByHazardCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request?.HazardCode))
            {
                _logger.LogApplicationError("GetHazardReportTrackingByHazardCodeQuery received with null or empty HazardCode", ApplicationEventIds.Error, null);
                return Result<List<HazardReportTracking>>.Failure<List<HazardReportTracking>>(DomainErrors.HazardReportTrackingError.NullOrEmpty);
            }

            _logger.LogInformation("Processing GetHazardReportTrackingByHazardCodeQuery for HazardCode: {HazardCode}", request.HazardCode);

            return await _service.GetHazardReportTrackingByHazardCodeAsync(request.HazardCode, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetHazardReportTrackingByHazardCodeQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while retrieving HazardReportTracking by HazardCode", ApplicationEventIds.Error, ex);
            return Result<List<HazardReportTracking>>.Failure<List<HazardReportTracking>>(DomainErrors.HazardReportTrackingError.NullOrEmpty);
        }
    }
}

public class GetHazardReportTrackingByReportCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetHazardReportTrackingByReportCodeQuery, Result<List<HazardReportTracking>>>
{
    private readonly HazardReportTrackingService _service;
    private readonly ILogger<GetHazardReportTrackingByReportCodeQueryHandler> _logger;

    public GetHazardReportTrackingByReportCodeQueryHandler(
        HazardReportTrackingService service,
        ILogger<GetHazardReportTrackingByReportCodeQueryHandler> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<HazardReportTracking>>> HandleAsync(GetHazardReportTrackingByReportCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request?.ReportCode))
            {
                _logger.LogApplicationError("GetHazardReportTrackingByReportCodeQuery received with null or empty ReportCode", ApplicationEventIds.Error, null);
                return Result<List<HazardReportTracking>>.Failure<List<HazardReportTracking>>(DomainErrors.HazardReportTrackingError.NullOrEmpty);
            }

            _logger.LogInformation("Processing GetHazardReportTrackingByReportCodeQuery for ReportCode: {ReportCode}", request.ReportCode);

            return await _service.GetHazardReportTrackingByReportCodeAsync(request.ReportCode, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetHazardReportTrackingByReportCodeQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while retrieving HazardReportTracking by ReportCode", ApplicationEventIds.Error, ex);
            return Result<List<HazardReportTracking>>.Failure<List<HazardReportTracking>>(DomainErrors.HazardReportTrackingError.NullOrEmpty);
        }
    }
}
