//-----------------------------------------------------------------------
// <copyright file="SafetyPerformanceIndicatorQueryHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query handlers for Safety Performance Indicator operations using CQRS pattern with MediatR.
//                  Application layer query handlers providing clean separation
//                  between presentation and business logic with validation.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Queries;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// SAFETY PERFORMANCE INDICATOR QUERY HANDLERS
// =============================================

/// <summary>
/// Query handler for getting all Safety Performance Indicators
/// </summary>
public class GetAllSafetyPerformanceIndicatorsQueryHandler : BaseQueryBundle, IRequestHandler<GetAllSafetyPerformanceIndicatorsQuery, Result<List<SafetyPerformanceIndicator>>>
{
    private readonly ISafetyPerformanceIndicatorService _spiService;
    private readonly ILogger<GetAllSafetyPerformanceIndicatorsQueryHandler> _logger;

    public GetAllSafetyPerformanceIndicatorsQueryHandler(
        ISafetyPerformanceIndicatorService spiService,
        ILogger<GetAllSafetyPerformanceIndicatorsQueryHandler> logger)
    {
        _spiService = spiService ?? throw new ArgumentNullException(nameof(spiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<SafetyPerformanceIndicator>>> HandleAsync(GetAllSafetyPerformanceIndicatorsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("✅ Clean Architecture: Processing GetAllSafetyPerformanceIndicatorsQuery");

            var result = await _spiService.GetAllSafetyPerformanceIndicatorsAsync(ct);

            if (result.IsSuccess)
            {
                var spis = result.Value.ToList(); // Convert IEnumerable to List

                // Apply filters if specified
                if (!string.IsNullOrEmpty(request.StatusFilter))
                {
                    spis = spis.Where(spi => spi.Status.Value.Equals(request.StatusFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (!string.IsNullOrEmpty(request.TypeFilter))
                {
                    spis = spis.Where(spi => spi.IndicatorType.Value.Equals(request.TypeFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (!string.IsNullOrEmpty(request.DepartmentFilter))
                {
                    spis = spis.Where(spi => spi.ResponsibleDepartment.Equals(request.DepartmentFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                _logger.LogInformation("✅ Clean Architecture: Successfully retrieved {Count} Safety Performance Indicators", spis.Count);
                return Result<List<SafetyPerformanceIndicator>>.Success(spis);
            }

            _logger.LogApplicationError("Failed to retrieve Safety Performance Indicators. Error: {Error}",
                ApplicationEventIds.Error, null);
            return Result<List<SafetyPerformanceIndicator>>.Failure<List<SafetyPerformanceIndicator>>(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetAllSafetyPerformanceIndicatorsQuery", ApplicationEventIds.Error, ex);
            return Result<List<SafetyPerformanceIndicator>>.Failure<List<SafetyPerformanceIndicator>>(DomainErrors.SPIError.NotFound);
        }
    }
}

/// <summary>
/// Query handler for getting SPI by ID
/// </summary>
public class GetSafetyPerformanceIndicatorByIdQueryHandler : BaseQueryBundle, IRequestHandler<GetSafetyPerformanceIndicatorByIdQuery, Result<SafetyPerformanceIndicator>>
{
    private readonly ISafetyPerformanceIndicatorService _spiService;
    private readonly ILogger<GetSafetyPerformanceIndicatorByIdQueryHandler> _logger;

    public GetSafetyPerformanceIndicatorByIdQueryHandler(
        ISafetyPerformanceIndicatorService spiService,
        ILogger<GetSafetyPerformanceIndicatorByIdQueryHandler> logger)
    {
        _spiService = spiService ?? throw new ArgumentNullException(nameof(spiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SafetyPerformanceIndicator>> HandleAsync(GetSafetyPerformanceIndicatorByIdQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("✅ Clean Architecture: Processing GetSafetyPerformanceIndicatorByIdQuery for ID: {Id}", request.SPIId);

            var result = await _spiService.GetSafetyPerformanceIndicatorByIdAsync(request.SPIId, ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully retrieved Safety Performance Indicator with ID: {Id}", request.SPIId);
            }
            else
            {
                _logger.LogApplicationError("Failed to retrieve Safety Performance Indicator with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSafetyPerformanceIndicatorByIdQuery for ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.NotFound);
        }
    }
}

/// <summary>
/// Query handler for getting SPI by Code
/// </summary>
public class GetSafetyPerformanceIndicatorByCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetSafetyPerformanceIndicatorByCodeQuery, Result<SafetyPerformanceIndicator>>
{
    private readonly ISafetyPerformanceIndicatorService _spiService;
    private readonly ILogger<GetSafetyPerformanceIndicatorByCodeQueryHandler> _logger;

    public GetSafetyPerformanceIndicatorByCodeQueryHandler(
        ISafetyPerformanceIndicatorService spiService,
        ILogger<GetSafetyPerformanceIndicatorByCodeQueryHandler> logger)
    {
        _spiService = spiService ?? throw new ArgumentNullException(nameof(spiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SafetyPerformanceIndicator>> HandleAsync(GetSafetyPerformanceIndicatorByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("✅ Clean Architecture: Processing GetSafetyPerformanceIndicatorByCodeQuery for Code: {Code}", request.Code);

            var result = await _spiService.GetSafetyPerformanceIndicatorByCodeAsync(request.Code, ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully retrieved Safety Performance Indicator with Code: {Code}", request.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to retrieve Safety Performance Indicator with Code: {Code}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSafetyPerformanceIndicatorByCodeQuery for Code: {Code}", ApplicationEventIds.Error, ex);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.NotFound);
        }
    }
}


/// <summary>
/// Query handler for getting SPI dashboard data
/// </summary>
public class GetSPIDashboardDataQueryHandler : BaseQueryBundle, IRequestHandler<GetSPIDashboardDataQuery, Result<SPIDashboardData>>
{
    private readonly SafetyPerformanceIndicatorService _spiService;
    private readonly ILogger<GetSPIDashboardDataQueryHandler> _logger;

    public GetSPIDashboardDataQueryHandler(
        SafetyPerformanceIndicatorService spiService,
        ILogger<GetSPIDashboardDataQueryHandler> logger)
    {
        _spiService = spiService ?? throw new ArgumentNullException(nameof(spiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SPIDashboardData>> HandleAsync(GetSPIDashboardDataQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("✅ Clean Architecture: Processing GetSPIDashboardDataQuery");

            var result = await _spiService.GetDashboardDataAsync(
                request.StartDate,
                request.EndDate,
                request.SPIIds,
                request.DepartmentFilters,
                request.TypeFilters,
                request.IncludeTrends,
                request.IncludeAlerts,
                ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully retrieved SPI dashboard data");
            }
            else
            {
                _logger.LogApplicationError("Failed to retrieve SPI dashboard data. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSPIDashboardDataQuery", ApplicationEventIds.Error, ex);
            return Result<SPIDashboardData>.Failure<SPIDashboardData>(DomainErrors.SPIError.NotFound);
        }
    }
}

/// <summary>
/// Query handler for getting SPI trend analysis
/// </summary>
public class GetSPITrendAnalysisQueryHandler : BaseQueryBundle, IRequestHandler<GetSPITrendAnalysisQuery, Result<List<SPITrendAnalysis>>>
{
    private readonly SafetyPerformanceIndicatorService _spiService;
    private readonly ILogger<GetSPITrendAnalysisQueryHandler> _logger;

    public GetSPITrendAnalysisQueryHandler(
        SafetyPerformanceIndicatorService spiService,
        ILogger<GetSPITrendAnalysisQueryHandler> logger)
    {
        _spiService = spiService ?? throw new ArgumentNullException(nameof(spiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<SPITrendAnalysis>>> HandleAsync(GetSPITrendAnalysisQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSPITrendAnalysisQuery");

            var result = await _spiService.GetTrendAnalysisAsync(
                request.SPIIds,
                request.Periods,
                request.EndDate,
                ct);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSPITrendAnalysisQuery", ApplicationEventIds.Error, ex);
            return Result<List<SPITrendAnalysis>>.Failure<List<SPITrendAnalysis>>(DomainErrors.SPIError.NotFound);
        }
    }
}

/// <summary>
/// Query handler for getting SPI performance summary
/// </summary>
public class GetSPIPerformanceSummaryQueryHandler : BaseQueryBundle, IRequestHandler<GetSPIPerformanceSummaryQuery, Result<SPIPerformanceSummary>>
{
    private readonly SafetyPerformanceIndicatorService _spiService;
    private readonly ILogger<GetSPIPerformanceSummaryQueryHandler> _logger;

    public GetSPIPerformanceSummaryQueryHandler(
        SafetyPerformanceIndicatorService spiService,
        ILogger<GetSPIPerformanceSummaryQueryHandler> logger)
    {
        _spiService = spiService ?? throw new ArgumentNullException(nameof(spiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SPIPerformanceSummary>> HandleAsync(GetSPIPerformanceSummaryQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSPIPerformanceSummaryQuery");

            var result = await _spiService.GetPerformanceSummaryAsync(
                request.StartDate,
                request.EndDate,
                request.DepartmentFilters,
                request.TypeFilters,
                ct);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSPIPerformanceSummaryQuery", ApplicationEventIds.Error, ex);
            return Result<SPIPerformanceSummary>.Failure<SPIPerformanceSummary>(DomainErrors.SPIError.NotFound);
        }
    }
}

/// <summary>
/// Query handler for getting SPI alerts
/// </summary>
public class GetSPIAlertsQueryHandler : BaseQueryBundle, IRequestHandler<GetSPIAlertsQuery, Result<List<SPIAlert>>>
{
    private readonly SafetyPerformanceIndicatorService _spiService;
    private readonly ILogger<GetSPIAlertsQueryHandler> _logger;

    public GetSPIAlertsQueryHandler(
        SafetyPerformanceIndicatorService spiService,
        ILogger<GetSPIAlertsQueryHandler> logger)
    {
        _spiService = spiService ?? throw new ArgumentNullException(nameof(spiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<SPIAlert>>> HandleAsync(GetSPIAlertsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSPIAlertsQuery");

            var result = await _spiService.GetAlertsAsync(
                request.SPIIds,
                request.ActiveAlertsOnly,
                request.StartDate,
                request.EndDate,
                ct);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSPIAlertsQuery", ApplicationEventIds.Error, ex);
            return Result<List<SPIAlert>>.Failure<List<SPIAlert>>(DomainErrors.SPIError.NotFound);
        }
    }
}

/// <summary>
/// Query handler for getting SPI compliance status
/// </summary>
public class GetSPIComplianceStatusQueryHandler : BaseQueryBundle, IRequestHandler<GetSPIComplianceStatusQuery, Result<List<SPIComplianceStatus>>>
{
    private readonly SafetyPerformanceIndicatorService _spiService;
    private readonly ILogger<GetSPIComplianceStatusQueryHandler> _logger;

    public GetSPIComplianceStatusQueryHandler(
        SafetyPerformanceIndicatorService spiService,
        ILogger<GetSPIComplianceStatusQueryHandler> logger)
    {
        _spiService = spiService ?? throw new ArgumentNullException(nameof(spiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<SPIComplianceStatus>>> HandleAsync(GetSPIComplianceStatusQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSPIComplianceStatusQuery");

            var result = await _spiService.GetComplianceStatusAsync(
                request.SPIIds,
                request.AsOfDate,
                ct);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSPIComplianceStatusQuery", ApplicationEventIds.Error, ex);
            return Result<List<SPIComplianceStatus>>.Failure<List<SPIComplianceStatus>>(DomainErrors.SPIError.NotFound);
        }
    }
}

/// <summary>
/// Query handler for getting SPI review schedule
/// </summary>
public class GetSPIReviewScheduleQueryHandler : BaseQueryBundle, IRequestHandler<GetSPIReviewScheduleQuery, Result<List<SPIReviewItem>>>
{
    private readonly SafetyPerformanceIndicatorService _spiService;
    private readonly ILogger<GetSPIReviewScheduleQueryHandler> _logger;

    public GetSPIReviewScheduleQueryHandler(
        SafetyPerformanceIndicatorService spiService,
        ILogger<GetSPIReviewScheduleQueryHandler> logger)
    {
        _spiService = spiService ?? throw new ArgumentNullException(nameof(spiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<SPIReviewItem>>> HandleAsync(GetSPIReviewScheduleQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSPIReviewScheduleQuery");

            var result = await _spiService.GetReviewScheduleAsync(
                request.StartDate,
                request.EndDate,
                request.OverdueOnly,
                ct);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSPIReviewScheduleQuery", ApplicationEventIds.Error, ex);
            return Result<List<SPIReviewItem>>.Failure<List<SPIReviewItem>>(DomainErrors.SPIError.NotFound);
        }
    }
}

/// <summary>
/// Query handler for getting SPI data points
/// </summary>
public class GetSPIDataPointsQueryHandler : BaseQueryBundle, IRequestHandler<GetSPIDataPointsQuery, Result<List<SPIDataPoint>>>
{
    private readonly ISafetyPerformanceIndicatorService _spiService;
    private readonly ILogger<GetSPIDataPointsQueryHandler> _logger;

    public GetSPIDataPointsQueryHandler(
        ISafetyPerformanceIndicatorService spiService,
        ILogger<GetSPIDataPointsQueryHandler> logger)
    {
        _spiService = spiService ?? throw new ArgumentNullException(nameof(spiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<SPIDataPoint>>> HandleAsync(GetSPIDataPointsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("✅ Clean Architecture: Processing GetSPIDataPointsQuery for SPI: {SPIId}", request.SPIId);

            var spiResult = await _spiService.GetSafetyPerformanceIndicatorByCodeAsync(request.SPIId, ct);
            if (spiResult.IsFailure)
            {
                _logger.LogApplicationError("Failed to retrieve SPI with Code: {SPIId}. Error: {Error}",
                    ApplicationEventIds.Error, null);
                return Result<List<SPIDataPoint>>.Failure<List<SPIDataPoint>>(spiResult.Error);
            }

            var spi = spiResult.Value;
            var dataPoints = spi.DataPoints.AsQueryable();

            // Apply date filters
            if (request.StartDate.HasValue)
            {
                dataPoints = dataPoints.Where(dp => dp.MeasurementDate >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                dataPoints = dataPoints.Where(dp => dp.MeasurementDate <= request.EndDate.Value);
            }

            // Apply record limit
            var results = dataPoints
                .OrderByDescending(dp => dp.MeasurementDate)
                .Take(request.MaxRecords ?? 1000)
                .ToList();

            _logger.LogInformation("✅ Clean Architecture: Successfully retrieved {Count} data points for SPI: {SPIId}", results.Count, request.SPIId);
            return Result<List<SPIDataPoint>>.Success(results);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSPIDataPointsQuery for SPI: {SPIId}", ApplicationEventIds.Error, ex);
            return Result<List<SPIDataPoint>>.Failure<List<SPIDataPoint>>(DomainErrors.SPIError.NotFound);
        }
    }
}
