using Microsoft.Extensions.Logging;
using SMS_Application.Messaging.Queries;
using SMS_Application.Services;
using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Infrastructure.Services;
using SMS_Shared.Common;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// SAFETY PERFORMANCE INDICATOR QUERY HANDLERS
// =============================================

/// <summary>
/// Query handler for getting all Safety Performance Indicators
/// </summary>
public class GetAllSafetyPerformanceIndicatorsQueryHandler : BaseQueryBundle, IRequestHandler<GetAllSafetyPerformanceIndicatorsQuery, Result<List<SafetyPerformanceIndicator>>>
{
    private readonly SafetyPerformanceIndicatorDataService _dataService;
    private readonly ILogger<GetAllSafetyPerformanceIndicatorsQueryHandler> _logger;

    public GetAllSafetyPerformanceIndicatorsQueryHandler(
        SafetyPerformanceIndicatorDataService dataService,
        ILogger<GetAllSafetyPerformanceIndicatorsQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<SafetyPerformanceIndicator>>> HandleAsync(GetAllSafetyPerformanceIndicatorsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetAllSafetyPerformanceIndicatorsQuery");

            var result = await _dataService.GetAllSafetyPerformanceIndicatorsAsync(ct);

            if (result.IsSuccess)
            {
                var spis = result.Value;

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

                return Result<List<SafetyPerformanceIndicator>>.Success(spis);
            }

            return result;
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
    private readonly SafetyPerformanceIndicatorDataService _dataService;
    private readonly ILogger<GetSafetyPerformanceIndicatorByIdQueryHandler> _logger;

    public GetSafetyPerformanceIndicatorByIdQueryHandler(
        SafetyPerformanceIndicatorDataService dataService,
        ILogger<GetSafetyPerformanceIndicatorByIdQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SafetyPerformanceIndicator>> HandleAsync(GetSafetyPerformanceIndicatorByIdQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSafetyPerformanceIndicatorByIdQuery for ID: {Id}", request.SPIId);

            var result = await _dataService.GetSafetyPerformanceIndicatorByIdAsync(request.SPIId, ct);

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
    private readonly SafetyPerformanceIndicatorDataService _dataService;
    private readonly ILogger<GetSafetyPerformanceIndicatorByCodeQueryHandler> _logger;

    public GetSafetyPerformanceIndicatorByCodeQueryHandler(
        SafetyPerformanceIndicatorDataService dataService,
        ILogger<GetSafetyPerformanceIndicatorByCodeQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SafetyPerformanceIndicator>> HandleAsync(GetSafetyPerformanceIndicatorByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSafetyPerformanceIndicatorByCodeQuery for Code: {Code}", request.Code);

            var result = await _dataService.GetSafetyPerformanceIndicatorByCodeAsync(request.Code, ct);

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
/// Query handler for getting SPIs by type
/// </summary>
public class GetSPIsByTypeQueryHandler : BaseQueryBundle, IRequestHandler<GetSPIsByTypeQuery, Result<List<SafetyPerformanceIndicator>>>
{
    private readonly SafetyPerformanceIndicatorDataService _dataService;
    private readonly ILogger<GetSPIsByTypeQueryHandler> _logger;

    public GetSPIsByTypeQueryHandler(
        SafetyPerformanceIndicatorDataService dataService,
        ILogger<GetSPIsByTypeQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<SafetyPerformanceIndicator>>> HandleAsync(GetSPIsByTypeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSPIsByTypeQuery for Type: {Type}", request.IndicatorType);

            var result = await _dataService.GetSafetyPerformanceIndicatorsByTypeAsync(request.IndicatorType, ct);

            if (result.IsSuccess && !string.IsNullOrEmpty(request.StatusFilter))
            {
                var filteredSpis = result.Value.Where(spi => spi.Status.Value.Equals(request.StatusFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                return Result<List<SafetyPerformanceIndicator>>.Success(filteredSpis);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSPIsByTypeQuery for Type: {Type}", ApplicationEventIds.Error, ex);
            return Result<List<SafetyPerformanceIndicator>>.Failure<List<SafetyPerformanceIndicator>>(DomainErrors.SPIError.NotFound);
        }
    }
}

/// <summary>
/// Query handler for getting SPIs by department
/// </summary>
public class GetSPIsByDepartmentQueryHandler : BaseQueryBundle, IRequestHandler<GetSPIsByDepartmentQuery, Result<List<SafetyPerformanceIndicator>>>
{
    private readonly SafetyPerformanceIndicatorDataService _dataService;
    private readonly ILogger<GetSPIsByDepartmentQueryHandler> _logger;

    public GetSPIsByDepartmentQueryHandler(
        SafetyPerformanceIndicatorDataService dataService,
        ILogger<GetSPIsByDepartmentQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<SafetyPerformanceIndicator>>> HandleAsync(GetSPIsByDepartmentQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSPIsByDepartmentQuery for Department: {Department}", request.Department);

            var result = await _dataService.GetSafetyPerformanceIndicatorsByDepartmentAsync(request.Department, ct);

            if (result.IsSuccess && !string.IsNullOrEmpty(request.StatusFilter))
            {
                var filteredSpis = result.Value.Where(spi => spi.Status.Value.Equals(request.StatusFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                return Result<List<SafetyPerformanceIndicator>>.Success(filteredSpis);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSPIsByDepartmentQuery for Department: {Department}", ApplicationEventIds.Error, ex);
            return Result<List<SafetyPerformanceIndicator>>.Failure<List<SafetyPerformanceIndicator>>(DomainErrors.SPIError.NotFound);
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
            _logger.LogInformation("Processing GetSPIDashboardDataQuery");

            var result = await _spiService.GetDashboardDataAsync(
                request.StartDate,
                request.EndDate,
                request.SPIIds,
                request.DepartmentFilters,
                request.TypeFilters,
                request.IncludeTrends,
                request.IncludeAlerts,
                ct);

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
    private readonly SafetyPerformanceIndicatorDataService _dataService;
    private readonly ILogger<GetSPIDataPointsQueryHandler> _logger;

    public GetSPIDataPointsQueryHandler(
        SafetyPerformanceIndicatorDataService dataService,
        ILogger<GetSPIDataPointsQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<SPIDataPoint>>> HandleAsync(GetSPIDataPointsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSPIDataPointsQuery for SPI: {SPIId}", request.SPIId);

            var spiResult = await _dataService.GetSafetyPerformanceIndicatorByCodeAsync(request.SPIId, ct);
            if (spiResult.IsFailure)
            {
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

            return Result<List<SPIDataPoint>>.Success(results);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSPIDataPointsQuery for SPI: {SPIId}", ApplicationEventIds.Error, ex);
            return Result<List<SPIDataPoint>>.Failure<List<SPIDataPoint>>(DomainErrors.SPIError.NotFound);
        }
    }
}