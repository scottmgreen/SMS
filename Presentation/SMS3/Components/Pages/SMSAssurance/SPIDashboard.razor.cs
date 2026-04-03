using SMS3.Components.Shared.UIHelpers;
using SMS3.Configuration.Extensions;

namespace SMS3.Components.Pages.SMSAssurance;

public partial class SPIDashboard : ComponentBase
{
    #region Injected Services
    [Inject] private IMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<SPIDashboard> _logger { get; set; } = default!;
    [Inject] private INotificationHelper _notificationHelper { get; set; } = default!;
    [Inject] private NavigationManager _navigation { get; set; } = default!;
    #endregion

    #region Component State
    private bool IsLoading { get; set; } = true;
    private SPIDashboardData? DashboardData { get; set; }

    // Filter State
    private string? SelectedSPIType { get; set; } = "All";
    private string? SelectedDepartment { get; set; } = "All";
    private string? SelectedTimePeriod { get; set; } = "Last 12 Months";
    private string? SelectedTrendSPI { get; set; }

    // Chart Data
    private List<SPIDataPointSummary>? TrendData { get; set; }
    private List<SPIDataPointSummary>? TrendTargetData { get; set; }
    private List<CategoryDataPoint>? CategoryData { get; set; }
    #endregion

    #region Filter Options
    public List<string> SPITypeOptions { get; } = new()
    {
        "All", "Leading", "Lagging", "Process", "Compliance"
    };

    public List<string> DepartmentOptions { get; } = new()
    {
        "All", "Airport Operations", "Security", "Maintenance", "Ground Handling", "Air Traffic Control"
    };

    public List<string> TimePeriodOptions { get; } = new()
    {
        "Last 3 Months", "Last 6 Months", "Last 12 Months", "Year to Date", "Custom Range"
    };

    public List<string> TrendSPIOptions { get; set; } = new();
    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        await LoadDashboardDataAsync();
    }
    #endregion

    #region Data Loading
    private async Task LoadDashboardDataAsync()
    {
        try
        {
            IsLoading = true;
            StateHasChanged();

            _logger.LogInformation("Loading SPI Dashboard data with filters - Type: {Type}, Department: {Department}, Period: {Period}",
                SelectedSPIType, SelectedDepartment, SelectedTimePeriod);

            var (startDate, endDate) = GetDateRange();
            var typeFilters = GetTypeFilters();
            var departmentFilters = GetDepartmentFilters();

            var query = new GetSPIDashboardDataQuery(
                startDate: startDate,
                endDate: endDate,
                spiIds: null,
                departmentFilters: departmentFilters,
                typeFilters: typeFilters,
                includeTrends: true,
                includeAlerts: true
            );

            var result = await _mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                DashboardData = result.Value;
                await LoadTrendSPIOptionsAsync();
                await LoadCategoryDataAsync();

                _logger.LogInformation("SPI Dashboard data loaded successfully - Total SPIs: {TotalSPIs}, Active Alerts: {ActiveAlerts}",
                    DashboardData.TotalSPIs, DashboardData.ActiveAlerts.Count);
            }
            else
            {
                _logger.LogError("Failed to load SPI Dashboard data: {Error}", result.Error?.Message);
                await _notificationHelper.ShowErrorAsync("Failed to load SPI dashboard data");
                DashboardData = new SPIDashboardData(); // Initialize empty
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading SPI Dashboard data");
            await _notificationHelper.ShowErrorAsync("Error loading SPI dashboard data");
            DashboardData = new SPIDashboardData();
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task LoadTrendSPIOptionsAsync()
    {
        if (DashboardData?.SPICards?.Any() == true)
        {
            TrendSPIOptions = DashboardData.SPICards
                .Where(spi => spi.Status == "Active")
                .Select(spi => spi.Name)
                .OrderBy(name => name)
                .ToList();

            // Auto-select first SPI for trend analysis
            if (TrendSPIOptions.Any() && string.IsNullOrEmpty(SelectedTrendSPI))
            {
                SelectedTrendSPI = TrendSPIOptions.First();
                await LoadTrendDataAsync();
            }
        }
    }

    private async Task LoadTrendDataAsync()
    {
        if (string.IsNullOrEmpty(SelectedTrendSPI) || DashboardData == null)
        {
            TrendData = null;
            TrendTargetData = null;
            return;
        }

        try
        {
            var selectedSPI = DashboardData.SPICards.FirstOrDefault(spi => spi.Name == SelectedTrendSPI);
            if (selectedSPI == null) return;

            var trendAnalysis = DashboardData.TrendAnalysis?.FirstOrDefault(t => t.SPIId == selectedSPI.SPIId);
            if (trendAnalysis != null && trendAnalysis.DataPoints?.Any() == true)
            {
                // Filter out invalid data points that could cause chart issues
                TrendData = trendAnalysis.DataPoints
                    .Where(dp => dp.MeasurementDate != default && dp.Value >= 0)
                    .OrderBy(dp => dp.MeasurementDate)
                    .ToList();

                // Only include target data if targets exist and are valid
                TrendTargetData = trendAnalysis.DataPoints
                    .Where(dp => dp.Target.HasValue && dp.Target.Value >= 0 && dp.MeasurementDate != default)
                    .Select(dp => new SPIDataPointSummary
                    {
                        Period = dp.Period,
                        MeasurementDate = dp.MeasurementDate,
                        Value = dp.Target!.Value  // We know it's not null due to the Where clause
                    })
                    .OrderBy(dp => dp.MeasurementDate)
                    .ToList();

                // If no valid data, clear the collections
                if (!TrendData.Any())
                {
                    TrendData = null;
                    TrendTargetData = null;
                }
                else if (!TrendTargetData.Any())
                {
                    TrendTargetData = null;
                }
            }
            else
            {
                TrendData = null;
                TrendTargetData = null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading trend data for SPI: {SPIName}", SelectedTrendSPI);
            TrendData = null;
            TrendTargetData = null;
        }
    }

    private async Task LoadCategoryDataAsync()
    {
        try
        {
            if (DashboardData?.PerformanceSummary?.SPIsByType?.Any() == true)
            {
                CategoryData = DashboardData.PerformanceSummary.SPIsByType
                    .Where(kvp => !string.IsNullOrEmpty(kvp.Key) && kvp.Value > 0)
                    .Select(kvp => new CategoryDataPoint(kvp.Key, kvp.Value))
                    .ToList();

                // If no valid data, clear the collection
                if (!CategoryData.Any())
                {
                    CategoryData = null;
                }
            }
            else
            {
                CategoryData = null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading category data");
            CategoryData = null;
        }
    }

    private async Task RefreshDashboard()
    {
        await LoadDashboardDataAsync();
        await _notificationHelper.ShowSuccessAsync("SPI Dashboard refreshed successfully");
    }
    #endregion

    #region Filter Methods
    private (DateTime? StartDate, DateTime? EndDate) GetDateRange()
    {
        var endDate = DateTime.UtcNow;
        var startDate = SelectedTimePeriod switch
        {
            "Last 3 Months" => endDate.AddMonths(-3),
            "Last 6 Months" => endDate.AddMonths(-6),
            "Last 12 Months" => endDate.AddMonths(-12),
            "Year to Date" => new DateTime(endDate.Year, 1, 1),
            _ => endDate.AddMonths(-12)
        };

        return (startDate, endDate);
    }

    private List<string>? GetTypeFilters()
    {
        if (SelectedSPIType == "All") return null;
        return new List<string> { SelectedSPIType };
    }

    private List<string>? GetDepartmentFilters()
    {
        if (SelectedDepartment == "All") return null;
        return new List<string> { SelectedDepartment };
    }

    private List<SPIDashboardCard> GetFilteredSPICards()
    {
        if (DashboardData?.SPICards == null) return new List<SPIDashboardCard>();

        var filtered = DashboardData.SPICards.AsEnumerable();

        if (SelectedSPIType != "All")
        {
            filtered = filtered.Where(spi => spi.IndicatorType.Contains(SelectedSPIType));
        }

        if (SelectedDepartment != "All")
        {
            filtered = filtered.Where(spi => spi.ResponsibleDepartment == SelectedDepartment);
        }

        return filtered.OrderBy(spi => spi.Name).ToList();
    }
    #endregion

    #region Event Handlers
    private async Task OnSPITypeChanged(object args)
    {
        SelectedSPIType = args?.ToString();
        StateHasChanged();
    }

    private async Task OnDepartmentChanged(object args)
    {
        SelectedDepartment = args?.ToString();
        StateHasChanged();
    }

    private async Task OnTimePeriodChanged(object args)
    {
        SelectedTimePeriod = args?.ToString();
        await LoadDashboardDataAsync();
    }

    private async Task OnTrendSPIChanged(object args)
    {
        SelectedTrendSPI = args?.ToString();
        await LoadTrendDataAsync();
        StateHasChanged();
    }
    #endregion

    #region _navigation Methods
    private void NavigateToConfiguration()
    {
        _navigation.NavigateToSecure("/SMSAssurance/SPIConfiguration");
    }

    private void NavigateToSPIDetail(string spiId)
    {
        _navigation.NavigateToSecure($"/SMSAssurance/SPIDetail/{spiId}");
    }

    private void ShowAllAlerts()
    {
        _navigation.NavigateToSecure("/SMSAssurance/SPIAlerts");
    }
    #endregion

    #region Helper Methods
    private string GetComplianceRate()
    {
        if (DashboardData?.PerformanceSummary == null) return "0";
        return DashboardData.PerformanceSummary.OverallComplianceRate.ToString("F1");
    }

    private int GetCriticalAlerts()
    {
        return DashboardData?.ActiveAlerts?.Count(alert => alert.AlertType == "Critical") ?? 0;
    }

    private string GetAlertCardStyle(string alertType)
    {
        return alertType switch
        {
            "Critical" => "border-left: 4px solid var(--rz-danger);",
            "Warning" => "border-left: 4px solid var(--rz-warning);",
            _ => "border-left: 4px solid var(--rz-info);"
        };
    }

    private string GetAlertIcon(string alertType)
    {
        return alertType switch
        {
            "Critical" => "error",
            "Warning" => "warning",
            _ => "info"
        };
    }

    private string GetAlertColor(string alertType)
    {
        return alertType switch
        {
            "Critical" => "var(--rz-danger)",
            "Warning" => "var(--rz-warning)",
            _ => "var(--rz-info)"
        };
    }

    private string GetTrendIcon(string direction)
    {
        return direction switch
        {
            "IMPROVING" or "Improving" => "trending_up",
            "DECLINING" or "Declining" => "trending_down",
            _ => "trending_flat"
        };
    }

    private string GetTrendColor(string direction)
    {
        return direction switch
        {
            "IMPROVING" or "Improving" => "#28a745",
            "DECLINING" or "Declining" => "#dc3545",
            _ => "#6c757d"
        };
    }

    private string GetSPICardStyle(SPIDashboardCard spiCard)
    {
        var styleClass = "spi-card";

        if (spiCard.IsOverThreshold)
            styleClass += " critical";
        else if (spiCard.IsAtWarningLevel)
            styleClass += " warning";
        else if (spiCard.CurrentValue.HasValue && spiCard.TargetValue.HasValue &&
                 spiCard.CurrentValue.Value >= spiCard.TargetValue.Value)
            styleClass += " compliant";

        return styleClass;
    }

    private BadgeStyle GetStatusBadgeStyle(string status)
    {
        return status switch
        {
            "ACTIVE" or "Active" => BadgeStyle.Success,
            "INACTIVE" or "Inactive" => BadgeStyle.Secondary,
            "UNDER_REVIEW" or "Under Review" => BadgeStyle.Warning,
            "DEPRECATED" or "Deprecated" => BadgeStyle.Danger,
            _ => BadgeStyle.Light
        };
    }

    private string GetPerformanceText(SPIDashboardCard spiCard)
    {
        if (!spiCard.CurrentValue.HasValue || !spiCard.TargetValue.HasValue)
            return "";

        // Handle division by zero case
        if (spiCard.TargetValue.Value == 0)
        {
            return spiCard.CurrentValue.Value == 0 ? "Target met" : "Target not set";
        }

        var percentage = (spiCard.CurrentValue.Value / spiCard.TargetValue.Value * 100m);
        return $"{percentage:F1}% of target";
    }

    private double GetProgressValue(SPIDashboardCard spiCard)
    {
        if (!spiCard.CurrentValue.HasValue || !spiCard.TargetValue.HasValue || spiCard.TargetValue.Value == 0)
            return 0;

        var progress = (double)(spiCard.CurrentValue.Value / spiCard.TargetValue.Value * 100m);
        return Math.Min(Math.Max(progress, 0), 100); // Clamp between 0 and 100
    }

    private ProgressBarStyle GetProgressStyle(SPIDashboardCard spiCard)
    {
        var progress = GetProgressValue(spiCard);

        if (progress >= 95) return ProgressBarStyle.Success;
        if (progress >= 80) return ProgressBarStyle.Info;
        if (progress >= 60) return ProgressBarStyle.Warning;
        return ProgressBarStyle.Danger;
    }

    private string GetLastUpdateText(SPIDashboardCard spiCard)
    {
        if (!spiCard.LastMeasurementDate.HasValue) return "No data";

        var timeAgo = DateTime.UtcNow - spiCard.LastMeasurementDate.Value;

        if (timeAgo.TotalDays < 1)
            return "Today";
        else if (timeAgo.TotalDays < 7)
            return $"{(int)timeAgo.TotalDays}d ago";
        else if (timeAgo.TotalDays < 30)
            return $"{(int)(timeAgo.TotalDays / 7)}w ago";
        else
            return spiCard.LastMeasurementDate.Value.ToString("MMM dd");
    }
    #endregion

    #region Supporting Types
    public class CategoryDataPoint
    {
        public string Category { get; set; }
        public int Count { get; set; }

        public CategoryDataPoint(string category, int count)
        {
            Category = category;
            Count = count;
        }
    }
    #endregion
}