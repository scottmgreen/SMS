//-----------------------------------------------------------------------
// <copyright file="GetSPIDashboardDataQueryHandler.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: CQRS query handler for SPI dashboard data providing comprehensive
//                  performance metrics, trend analysis, and automated SPI integration.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Messaging.Queries;

/// <summary>
/// Handler for retrieving comprehensive SPI dashboard data
/// </summary>
public class GetSPIDashboardDataQueryHandler : IBaseRequestHandler<GetSPIDashboardDataQuery, Result<SPIDashboard>>
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

    public async Task<Result<SPIDashboard>> HandleAsync(GetSPIDashboardDataQuery query, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("?? SPI Dashboard: Loading dashboard data for period {StartDate} to {EndDate}", 
                query.StartDate, query.EndDate);

            // Get all SPIs
            var spiResult = await _spiService.GetAllSafetyPerformanceIndicatorsAsync(cancellationToken);
            if (spiResult.IsFailure)
            {
                _logger.LogError("? SPI Dashboard: Failed to load SPIs - {Error}", spiResult.Error?.Message);
                return Result<SPIDashboard>.Failure<SPIDashboard>(spiResult.Error);
            }

            var allSPIs = spiResult.Value?.ToList() ?? new List<SafetyPerformanceIndicator>();

            // Apply filters
            var filteredSPIs = ApplyFilters(allSPIs, query);

            // Build dashboard data
            var dashboardData = new SPIDashboard
            {
                TotalSPIs = allSPIs.Count,
                ActiveSPIs = allSPIs.Count(spi => spi.Status == SPIStatus.Active),
                SPICards = await BuildSPICards(filteredSPIs, query.StartDate ?? DateTime.UtcNow.AddMonths(-12), query.EndDate ?? DateTime.UtcNow, cancellationToken),
                LastUpdated = DateTime.UtcNow
            };

            // Build performance summary
            dashboardData.PerformanceSummary = BuildPerformanceSummary(dashboardData.SPICards);

            // Build trend analysis if requested
            if (query.IncludeTrends)
            {
                dashboardData.TrendAnalysis = await BuildTrendAnalysis(filteredSPIs, query.StartDate ?? DateTime.UtcNow.AddMonths(-12), query.EndDate ?? DateTime.UtcNow, cancellationToken);
            }

            // Build alerts if requested
            if (query.IncludeAlerts)
            {
                dashboardData.ActiveAlerts = BuildActiveAlerts(dashboardData.SPICards);
            }

            _logger.LogInformation("? SPI Dashboard: Successfully loaded dashboard data - {TotalSPIs} SPIs, {ActiveSPIs} active, {Alerts} alerts",
                dashboardData.TotalSPIs, dashboardData.ActiveSPIs, dashboardData.ActiveAlerts.Count);

            return Result<SPIDashboard>.Success(dashboardData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? SPI Dashboard: Error loading dashboard data");
            return Result<SPIDashboard>.Failure<SPIDashboard>(DomainErrors.SPIError.DataValidationFailed);
        }
    }

    #region Helper Methods

    /// <summary>
    /// Apply filters to SPI collection
    /// </summary>
    private List<SafetyPerformanceIndicator> ApplyFilters(
        List<SafetyPerformanceIndicator> spis, 
        GetSPIDashboardDataQuery query)
    {
        var filtered = spis.AsEnumerable();

        // Filter by SPI IDs if specified
        if (query.SPIIds?.Any() == true)
        {
            filtered = filtered.Where(spi => query.SPIIds.Contains(spi.Code));
        }

        // Filter by department if specified
        if (query.DepartmentFilters?.Any() == true && !query.DepartmentFilters.Contains("All"))
        {
            filtered = filtered.Where(spi => 
                query.DepartmentFilters.Contains(spi.ResponsibleDepartment) ||
                string.IsNullOrEmpty(spi.ResponsibleDepartment));
        }

        // Filter by type if specified  
        if (query.TypeFilters?.Any() == true && !query.TypeFilters.Contains("All"))
        {
            filtered = filtered.Where(spi => 
                query.TypeFilters.Any(filter => 
                    spi.IndicatorType.Category.Equals(filter, StringComparison.OrdinalIgnoreCase)));
        }

        return filtered.ToList();
    }

    /// <summary>
    /// Build SPI cards with current data
    /// </summary>
    private async Task<List<SPIDashboardCard>> BuildSPICards(
        List<SafetyPerformanceIndicator> spis, 
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken)
    {
        var cards = new List<SPIDashboardCard>();

        foreach (var spi in spis)
        {
            try
            {
                var card = new SPIDashboardCard
                {
                    SPIId = spi.Code,
                    Name = spi.Name,
                    Description = spi.Description,
                    Type = spi.IndicatorType.Name,
                    Category = spi.IndicatorType.Category,
                    Status = spi.Status.Name,
                    TargetValue = spi.TargetValue,
                    WarningThreshold = spi.WarningThreshold,
                    CriticalThreshold = spi.CriticalThreshold,
                    MeasurementUnit = spi.MeasurementUnit,
                    ResponsibleDepartment = spi.ResponsibleDepartment,
                    DataOwner = spi.DataOwner
                };

                // Get current value and trend from data points
                await PopulateCurrentData(card, spi, startDate, endDate);

                cards.Add(card);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "?? SPI Dashboard: Error building card for SPI {SPIId}", spi.Code);
            }
        }

        return cards.OrderBy(c => c.Name).ToList();
    }

    /// <summary>
    /// Populate current data and trends for SPI card
    /// </summary>
    private async Task PopulateCurrentData(
        SPIDashboardCard card, 
        SafetyPerformanceIndicator spi, 
        DateTime startDate, 
        DateTime endDate)
    {
        if (spi.DataPoints?.Any() == true)
        {
            var recentDataPoints = spi.DataPoints
                .Where(dp => dp.MeasurementDate >= startDate && dp.MeasurementDate <= endDate)
                .OrderByDescending(dp => dp.MeasurementDate)
                .ToList();

            if (recentDataPoints.Any())
            {
                var latest = recentDataPoints.First();
                card.CurrentValue = latest.Value;
                card.LastMeasurementDate = latest.MeasurementDate;
                card.DataPointCount = recentDataPoints.Count;

                // Calculate performance status
                card.PerformanceStatus = DeterminePerformanceStatus(latest.Value, spi.TargetValue, spi.WarningThreshold, spi.CriticalThreshold);

                // Calculate trend
                if (recentDataPoints.Count >= 2)
                {
                    var previous = recentDataPoints.Skip(1).First();
                    var trendChange = latest.Value - previous.Value;
                    var trendPercentage = previous.Value != 0 ? (trendChange / previous.Value) * 100 : 0;

                    card.TrendDirection = trendChange > 0 ? "Improving" : trendChange < 0 ? "Declining" : "Stable";
                    card.TrendPercentage = trendPercentage;
                }
            }
        }
    }

    /// <summary>
    /// Determine performance status based on thresholds
    /// </summary>
    private string DeterminePerformanceStatus(decimal currentValue, decimal? target, decimal? warning, decimal? critical)
    {
        if (!target.HasValue) return "Unknown";

        if (critical.HasValue && Math.Abs(currentValue - target.Value) >= critical.Value)
            return "Critical";

        if (warning.HasValue && Math.Abs(currentValue - target.Value) >= warning.Value)
            return "Warning";

        return "Meeting";
    }

    /// <summary>
    /// Build performance summary
    /// </summary>
    private SPIPerformanceSummary BuildPerformanceSummary(List<SPIDashboardCard> cards)
    {
        var summary = new SPIPerformanceSummary();

        foreach (var card in cards)
        {
            // Count by performance status
            switch (card.PerformanceStatus)
            {
                case "Meeting":
                    summary.SPIsMeetingTarget++;
                    break;
                case "Warning":
                    summary.SPIsInWarning++;
                    break;
                case "Critical":
                    summary.SPIsInCritical++;
                    break;
            }

            // Count by type
            summary.SPIsByType[card.Category] = summary.SPIsByType.GetValueOrDefault(card.Category, 0) + 1;

            // Count by department
            if (!string.IsNullOrEmpty(card.ResponsibleDepartment))
            {
                summary.SPIsByDepartment[card.ResponsibleDepartment] = 
                    summary.SPIsByDepartment.GetValueOrDefault(card.ResponsibleDepartment, 0) + 1;
            }
        }

        // Calculate overall compliance rate
        var totalWithTargets = cards.Count(c => !string.IsNullOrEmpty(c.PerformanceStatus) && c.PerformanceStatus != "Unknown");
        summary.OverallComplianceRate = totalWithTargets > 0 
            ? (decimal)summary.SPIsMeetingTarget / totalWithTargets * 100 
            : 0;

        return summary;
    }

    /// <summary>
    /// Build trend analysis data
    /// </summary>
    private async Task<List<SPITrendAnalysis>> BuildTrendAnalysis(
        List<SafetyPerformanceIndicator> spis, 
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken)
    {
        var trends = new List<SPITrendAnalysis>();

        foreach (var spi in spis.Take(10)) // Limit for performance
        {
            if (spi.DataPoints?.Any() == true)
            {
                var dataPoints = spi.DataPoints
                    .Where(dp => dp.MeasurementDate >= startDate && dp.MeasurementDate <= endDate)
                    .OrderBy(dp => dp.MeasurementDate)
                    .Select(dp => new SPIDataPointSummary
                    {
                        Period = dp.Period,
                        MeasurementDate = dp.MeasurementDate,
                        Value = dp.Value,
                        Target = spi.TargetValue,
                        DataSource = dp.DataSource,
                        IsVerified = dp.IsVerified,
                        Notes = dp.Notes ?? string.Empty
                    })
                    .ToList();

                if (dataPoints.Count >= 2)
                {
                    var trend = new SPITrendAnalysis
                    {
                        SPIId = spi.Code,
                        SPIName = spi.Name,
                        DataPoints = dataPoints,
                        TrendAnalysisPeriod = $"{startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}"
                    };

                    // Simple trend calculation
                    var first = dataPoints.First().Value;
                    var last = dataPoints.Last().Value;
                    trend.TrendDirection = last > first ? "Improving" : last < first ? "Declining" : "Stable";
                    trend.TrendSlope = dataPoints.Count > 1 ? (last - first) / dataPoints.Count : 0;

                    trends.Add(trend);
                }
            }
        }

        return trends;
    }

    /// <summary>
    /// Build active alerts
    /// </summary>
    private List<SPIAlert> BuildActiveAlerts(List<SPIDashboardCard> cards)
    {
        var alerts = new List<SPIAlert>();

        foreach (var card in cards)
        {
            if (card.PerformanceStatus == "Critical" && card.CurrentValue.HasValue)
            {
                alerts.Add(new SPIAlert
                {
                    SPIId = card.SPIId,
                    SPIName = card.Name,
                    AlertType = "Critical Threshold Exceeded",
                    AlertMessage = $"{card.Name} has exceeded critical threshold",
                    AlertDate = card.LastMeasurementDate ?? DateTime.UtcNow,
                    CurrentValue = card.CurrentValue.Value,
                    ThresholdValue = card.CriticalThreshold ?? 0,
                    Severity = "Critical",
                    IsAcknowledged = false
                });
            }
            else if (card.PerformanceStatus == "Warning" && card.CurrentValue.HasValue)
            {
                alerts.Add(new SPIAlert
                {
                    SPIId = card.SPIId,
                    SPIName = card.Name,
                    AlertType = "Warning Threshold Exceeded",
                    AlertMessage = $"{card.Name} has exceeded warning threshold",
                    AlertDate = card.LastMeasurementDate ?? DateTime.UtcNow,
                    CurrentValue = card.CurrentValue.Value,
                    ThresholdValue = card.WarningThreshold ?? 0,
                    Severity = "Medium",
                    IsAcknowledged = false
                });
            }
        }

        return alerts.OrderByDescending(a => a.AlertDate).ToList();
    }

    #endregion
}