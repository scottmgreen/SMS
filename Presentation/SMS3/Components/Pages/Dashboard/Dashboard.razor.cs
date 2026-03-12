using SMS3.Components.Shared.UIHelpers;
using SMS3.Extensions;

namespace SMS3.Components.Pages.Dashboard;

public partial class Dashboard : ComponentBase
{
    #region Injected Services
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<Dashboard> Logger { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    #endregion

    #region Component State
    private bool IsLoading { get; set; } = true;
    private DashboardStatisticsResponse? Statistics { get; set; }
    private string? SelectedStatusFilter { get; set; } = "All";
    private string? SelectedEntityType { get; set; } = "All";
    #endregion

    #region Dashboard Metrics - Computed Properties
    public int TotalReports => Statistics?.TotalReports ?? 0;
    public int TotalHazards => Statistics?.TotalHazards ?? 0;
    public int TotalRiskAssessments => Statistics?.TotalRiskAssessments ?? 0;
    public int TotalInvestigations => Statistics?.TotalInvestigations ?? 0;
    public int TotalMitigations => Statistics?.TotalMitigations ?? 0;

    // Status-filtered counts
    public int ActiveItems => GetFilteredCount("Active");
    public int PendingItems => GetFilteredCount("Pending");
    public int CompletedItems => GetFilteredCount("Completed");
    public int OverdueItems => Statistics?.ItemsOverdue ?? 0;
    #endregion

    #region Chart Data
    public List<DataPoint> ReportsByMonth => Statistics?.ReportsByMonth?
        .Select(kvp => new DataPoint(kvp.Key, kvp.Value))
        .OrderBy(dp => dp.Label)
        .ToList() ?? new List<DataPoint>();

    public List<DataPoint> HazardsByType => Statistics?.HazardsByType?
        .Select(kvp => new DataPoint(kvp.Key, kvp.Value))
        .OrderByDescending(dp => dp.Value)
        .ToList() ?? new List<DataPoint>();

    public List<DataPoint> ItemsByStatus => GetStatusDistribution();

    public List<DataPoint> AssessmentsByType => Statistics?.RiskAssessmentsByType?
        .Select(kvp => new DataPoint(kvp.Key, kvp.Value))
        .ToList() ?? new List<DataPoint>();
    #endregion

    #region Filter Options
    public List<string> EntityTypeOptions { get; } = new()
    {
        "All", "Reports", "Hazards", "Risk Assessments", "Investigations", "Mitigations"
    };

    public List<string> StatusFilterOptions { get; } = new()
    {
        "All", "Active", "Pending", "In Progress", "Completed", "Overdue"
    };
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

            Logger.LogInformation("Loading comprehensive dashboard statistics");

            var query = new GetDashboardStatisticsQuery();
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                Statistics = result.Value;
                Logger.LogInformation("Dashboard statistics loaded successfully - Reports: {Reports}, Hazards: {Hazards}, Assessments: {Assessments}",
                    Statistics.TotalReports, Statistics.TotalHazards, Statistics.TotalRiskAssessments);
            }
            else
            {
                Logger.LogError("Failed to load dashboard statistics: {Error}", result.Error?.Message);
                ShowErrorNotification("Failed to load dashboard data");
                Statistics = new DashboardStatisticsResponse(); // Initialize empty
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading dashboard data");
            ShowErrorNotification("Error loading dashboard data");
            Statistics = new DashboardStatisticsResponse();
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task RefreshDashboardData()
    {
        await LoadDashboardDataAsync();
        ShowSuccessNotification("Dashboard data refreshed");
    }
    #endregion

    #region Data Processing Methods
    private int GetFilteredCount(string filterType)
    {
        if (Statistics == null) return 0;

        return filterType switch
        {
            "Active" => GetActiveCount(),
            "Pending" => GetPendingCount(),
            "Completed" => GetCompletedCount(),
            _ => 0
        };
    }

    private int GetActiveCount()
    {
        if (Statistics == null) return 0;

        return Statistics.ReportsByStatus.Where(kvp => !IsCompletedStatus(kvp.Key)).Sum(kvp => kvp.Value) +
               Statistics.HazardsByStatus.Where(kvp => !IsCompletedStatus(kvp.Key)).Sum(kvp => kvp.Value) +
               Statistics.RiskAssessmentsByStatus.Where(kvp => !IsCompletedStatus(kvp.Key)).Sum(kvp => kvp.Value) +
               Statistics.InvestigationsByStatus.Where(kvp => !IsCompletedStatus(kvp.Key)).Sum(kvp => kvp.Value) +
               Statistics.MitigationsByStatus.Where(kvp => !IsCompletedStatus(kvp.Key)).Sum(kvp => kvp.Value);
    }

    private int GetPendingCount()
    {
        if (Statistics == null) return 0;

        return Statistics.ReportsByStatus.Where(kvp => IsPendingStatus(kvp.Key)).Sum(kvp => kvp.Value) +
               Statistics.HazardsByStatus.Where(kvp => IsPendingStatus(kvp.Key)).Sum(kvp => kvp.Value) +
               Statistics.RiskAssessmentsByStatus.Where(kvp => IsPendingStatus(kvp.Key)).Sum(kvp => kvp.Value) +
               Statistics.InvestigationsByStatus.Where(kvp => IsPendingStatus(kvp.Key)).Sum(kvp => kvp.Value) +
               Statistics.MitigationsByStatus.Where(kvp => IsPendingStatus(kvp.Key)).Sum(kvp => kvp.Value);
    }

    private int GetCompletedCount()
    {
        if (Statistics == null) return 0;

        return Statistics.ReportsByStatus.Where(kvp => IsCompletedStatus(kvp.Key)).Sum(kvp => kvp.Value) +
               Statistics.HazardsByStatus.Where(kvp => IsCompletedStatus(kvp.Key)).Sum(kvp => kvp.Value) +
               Statistics.RiskAssessmentsByStatus.Where(kvp => IsCompletedStatus(kvp.Key)).Sum(kvp => kvp.Value) +
               Statistics.InvestigationsByStatus.Where(kvp => IsCompletedStatus(kvp.Key)).Sum(kvp => kvp.Value) +
               Statistics.MitigationsByStatus.Where(kvp => IsCompletedStatus(kvp.Key)).Sum(kvp => kvp.Value);
    }

    private List<DataPoint> GetStatusDistribution()
    {
        if (Statistics == null) return new List<DataPoint>();

        var statusCounts = new Dictionary<string, int>();

        // Aggregate all status counts
        foreach (var kvp in Statistics.ReportsByStatus)
            statusCounts[kvp.Key] = statusCounts.GetValueOrDefault(kvp.Key, 0) + kvp.Value;

        foreach (var kvp in Statistics.HazardsByStatus)
            statusCounts[kvp.Key] = statusCounts.GetValueOrDefault(kvp.Key, 0) + kvp.Value;

        foreach (var kvp in Statistics.RiskAssessmentsByStatus)
            statusCounts[kvp.Key] = statusCounts.GetValueOrDefault(kvp.Key, 0) + kvp.Value;

        foreach (var kvp in Statistics.InvestigationsByStatus)
            statusCounts[kvp.Key] = statusCounts.GetValueOrDefault(kvp.Key, 0) + kvp.Value;

        foreach (var kvp in Statistics.MitigationsByStatus)
            statusCounts[kvp.Key] = statusCounts.GetValueOrDefault(kvp.Key, 0) + kvp.Value;

        return statusCounts
            .Select(kvp => new DataPoint(kvp.Key, kvp.Value))
            .OrderByDescending(dp => dp.Value)
            .Take(10) // Top 10 statuses
            .ToList();
    }

    private bool IsCompletedStatus(string status)
    {
        return status.ToLowerInvariant().Contains("completed") ||
               status.ToLowerInvariant().Contains("closed") ||
               status.ToLowerInvariant().Contains("resolved") ||
               status.ToLowerInvariant().Contains("done");
    }

    private bool IsPendingStatus(string status)
    {
        return status.ToLowerInvariant().Contains("pending") ||
               status.ToLowerInvariant().Contains("new") ||
               status.ToLowerInvariant().Contains("submitted") ||
               status.ToLowerInvariant().Contains("under review");
    }
    #endregion

    #region Event Handlers
    private async Task OnStatusFilterChanged(string newFilter)
    {
        SelectedStatusFilter = newFilter;
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnEntityTypeChanged(string newEntityType)
    {
        SelectedEntityType = newEntityType;
        await InvokeAsync(StateHasChanged);
    }

    private void NavigateToReports()
    {
        // ?? SECURE NAVIGATION - Navigate to Report Processing with encrypted URL
        Navigation.NavigateToSecure("/SMSRiskManagement/ReportProcessing");
    }

    private void NavigateToHazards()
    {
        // ?? SECURE NAVIGATION - Navigate to Hazard Reporting with encrypted URL
        Navigation.NavigateToSecure("/SMSRiskManagement/HazardReporting");
    }

    private void NavigateToInvestigations()
    {
        // ?? SECURE NAVIGATION - Navigate to Investigations with encrypted URL
        Navigation.NavigateToSecure("/SMSRiskManagement/Investigations");
    }

    private void NavigateToRiskAssessments()
    {
        // ?? SECURE NAVIGATION - Navigate to Risk Assessment with encrypted URL
        Navigation.NavigateToSecure("/SMSRiskManagement/RiskAssessment");
    }

    private void NavigateToActivity(DashboardActivityItem activity)
    {
        if (!string.IsNullOrEmpty(activity.NavigationUrl))
        {
            // ?? SECURE NAVIGATION - Navigate to activity with encrypted URL
            Navigation.NavigateToSecure(activity.NavigationUrl);
        }
    }
    #endregion

    #region Notification Methods
    private void ShowSuccessNotification(string message)
    {
        NotificationHelper.ShowSuccess(NotificationService, message);
    }

    private void ShowErrorNotification(string message)
    {
        NotificationHelper.ShowError(NotificationService, message);
    }
    #endregion

    #region Supporting Types
    public class DataPoint
    {
        public string Label { get; set; }
        public int Value { get; set; }

        public DataPoint(string label, int value)
        {
            Label = label;
            Value = value;
        }
    }
    #endregion
}