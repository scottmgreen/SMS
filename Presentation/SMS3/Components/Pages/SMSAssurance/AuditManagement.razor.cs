using SMS3.Components.Shared.UIHelpers;
using SMS3.Configuration.Extensions;

namespace SMS3.Components.Pages.SMSAssurance;

public partial class AuditManagement : ComponentBase
{
    #region Injected Services
    [Inject] private ICurrentUserService CurrentUserService { get; set; } = default!;
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<AuditManagement> Logger { get; set; } = default!;
    [Inject] private INotificationHelper NotificationHelper { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    #endregion

    #region Component State
    private bool IsLoading { get; set; } = true;
    private bool IsUpdating { get; set; } = false;

    // Collapsible sections
    private bool showCategorySummary { get; set; } = true;
    private bool showFilterPanel { get; set; } = false;

    // Filter State
    private string SearchText { get; set; } = string.Empty;
    private string SelectedStatus { get; set; } = "All";
    private string SelectedType { get; set; } = "All";
    private string SelectedDepartment { get; set; } = "All";

    // Filter Options
    private List<string> StatusOptions { get; set; } = new() { "All", "Draft", "Approved", "Scheduled", "In Progress", "Completed", "Cancelled" };
    private List<string> TypeOptions { get; set; } = new() { "All", "Internal", "External", "Management Review", "Compliance", "Follow-up" };
    private List<string> DepartmentOptions { get; set; } = new() { "All", "Operations", "Maintenance", "Safety", "Security", "Management" };

    // Data Collections
    private List<SMSAuditPlan> AuditPlans { get; set; } = new();
    private List<SMSAuditPlan> AllAuditPlans { get; set; } = new();
    private List<SMSAudit> ActiveAudits { get; set; } = new();
    private List<SMSAudit> AllAudits { get; set; } = new(); // Add this for complete audit list
    private List<SMSAuditFinding> RecentFindings { get; set; } = new();
    private List<SMSAuditEvidence> RecentEvidence { get; set; } = new();

    // Grid References
    private RadzenDataGrid<SMSAuditPlan>? auditPlansGrid;
    private RadzenDataGrid<SMSAudit>? activeAuditsGrid;

    // Dashboard Statistics
    private AuditDashboardStats DashboardStats { get; set; } = new();
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

            Logger.LogInformation("Loading Audit Management dashboard data");

            // Load all dashboard data
            await Task.WhenAll(
                LoadAuditPlansAsync(),
                LoadActiveAuditsAsync(),
                LoadRecentFindingsAsync(),
                LoadRecentEvidenceAsync(),
                LoadDashboardStatsAsync()
            );

            Logger.LogInformation("Audit Management dashboard data loaded successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading Audit Management dashboard data");
            await NotificationHelper.ShowErrorAsync("Error loading audit management data");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task LoadAuditPlansAsync()
    {
        try
        {
            var query = new GetAllSMSAuditPlansQuery();
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                AuditPlans = result.Value.ToList();
                AllAuditPlans = result.Value.ToList(); // Also load to AllAuditPlans for filtering
            }
            else
            {
                AuditPlans = new List<SMSAuditPlan>();
                AllAuditPlans = new List<SMSAuditPlan>();
                Logger.LogWarning("Failed to load audit plans: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading audit plans");
            AuditPlans = new List<SMSAuditPlan>();
            AllAuditPlans = new List<SMSAuditPlan>();
        }
    }

    private async Task LoadActiveAuditsAsync()
    {
        try
        {
            // Load ALL audits, not just active ones for proper statistics
            var query = new GetAllSMSAuditsQuery();
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                var allAudits = result.Value.ToList();

                // Store all audits for statistics calculation
                AllAudits = allAudits;

                // Filter for the active audits grid display
                ActiveAudits = allAudits
                    .Where(a => a.Status == "In Progress" || a.Status == "Scheduled")
                    .OrderBy(a => a.ScheduledStartDate)
                    .ToList();
            }
            else
            {
                ActiveAudits = new List<SMSAudit>();
                AllAudits = new List<SMSAudit>();
                Logger.LogWarning("Failed to load audits: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading audits");
            ActiveAudits = new List<SMSAudit>();
            AllAudits = new List<SMSAudit>();
        }
    }

    private async Task LoadRecentFindingsAsync()
    {
        try
        {
            var query = new GetAllSMSAuditFindingsQuery();
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                RecentFindings = result.Value
                    .OrderByDescending(f => f.DiscoveredDate)
                    .Take(10)
                    .ToList();
            }
            else
            {
                RecentFindings = new List<SMSAuditFinding>();
                Logger.LogWarning("Failed to load recent findings: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading recent findings");
            RecentFindings = new List<SMSAuditFinding>();
        }
    }

    private async Task LoadRecentEvidenceAsync()
    {
        try
        {
            var query = new GetAllSMSAuditEvidenceQuery();
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                RecentEvidence = result.Value
                    .OrderByDescending(e => e.CollectionDate)
                    .Take(10)
                    .ToList();
            }
            else
            {
                RecentEvidence = new List<SMSAuditEvidence>();
                Logger.LogWarning("Failed to load recent evidence: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading recent evidence");
            RecentEvidence = new List<SMSAuditEvidence>();
        }
    }

    private async Task LoadDashboardStatsAsync()
    {
        try
        {
            // Calculate dashboard statistics from actual loaded data
            DashboardStats = new AuditDashboardStats
            {
                // Audit Plan Statistics
                TotalAuditPlans = AllAuditPlans.Count,
                ApprovedPlans = AllAuditPlans.Count(p => p.Status == "Approved"),

                // Active Audit Statistics  
                ActiveAudits = AllAudits.Count(a => a.Status == "In Progress"),
                CompletedThisMonth = AllAudits.Count(a => a.Status == "Completed" &&
                    a.ActualEndDate.HasValue &&
                    a.ActualEndDate.Value.Month == DateTime.Now.Month &&
                    a.ActualEndDate.Value.Year == DateTime.Now.Year),

                // Calculate overdue audits (scheduled but past end date and not completed)
                OverdueAudits = AllAudits.Count(a =>
                    a.ScheduledEndDate < DateTime.Now &&
                    a.ActualEndDate == null &&
                    (a.Status == "Scheduled" || a.Status == "In Progress")),

                // Finding Statistics
                CriticalFindings = RecentFindings.Count(f => f.Severity == "Critical"),
                MajorFindings = RecentFindings.Count(f => f.Severity == "Major"),
                FindingsAwaitingAction = RecentFindings.Count(f => f.Status == "Open" || f.Status == "In Progress")
            };

            Logger.LogInformation("Dashboard Stats Calculated: Plans={TotalPlans}, ActiveAudits={Active}, Overdue={Overdue}",
                DashboardStats.TotalAuditPlans, DashboardStats.ActiveAudits, DashboardStats.OverdueAudits);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error calculating dashboard statistics");
            DashboardStats = new AuditDashboardStats();
        }
    }

    private async Task RefreshDashboard()
    {
        await LoadDashboardDataAsync();
        await NotificationHelper.ShowSuccessAsync("Audit Management dashboard refreshed successfully");
    }
    #endregion

    #region Event Handlers
    private async Task OnSearchTextChanged(string value)
    {
        SearchText = value;
        await auditPlansGrid?.Reload();
        await activeAuditsGrid?.Reload();
    }

    private async Task OnStatusFilterChanged(object value)
    {
        SelectedStatus = value?.ToString();
        await auditPlansGrid?.Reload();
        await activeAuditsGrid?.Reload();
    }

    private async Task OnTypeFilterChanged(object value)
    {
        SelectedType = value?.ToString();
        await auditPlansGrid?.Reload();
        await activeAuditsGrid?.Reload();
    }

    private async Task OnDepartmentFilterChanged(object value)
    {
        SelectedDepartment = value?.ToString();
        await auditPlansGrid?.Reload();
        await activeAuditsGrid?.Reload();
    }

    private async Task OnDateRangeChanged()
    {
        await auditPlansGrid?.Reload();
        await activeAuditsGrid?.Reload();
    }
    #endregion

    #region CRUD Operations
    private async Task CreateAuditPlan()
    {
        try
        {
            var result = await DialogService.OpenAsync<Components.AuditPlanDialog>("Create Audit Plan",
                new Dictionary<string, object>()
                {
                    { "AuditPlan", new SMSAuditPlan(new SMSAuditPlanID("AP-0000"), CurrentUserService?.UserDisplayName) },
                    { "IsNew", true }
                },
                new DialogOptions() { Width = "1200px", Height = "900px", Resizable = true });

            if (result != null)
            {
                // Refresh ALL dashboard data after creating a plan
                await LoadDashboardDataAsync();
                await NotificationHelper.ShowSuccessAsync("Audit plan created successfully");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating audit plan");
            await NotificationHelper.ShowErrorAsync("Error creating audit plan");
        }
    }

    private async Task EditAuditPlan(SMSAuditPlan plan)
    {
        try
        {
            var result = await DialogService.OpenAsync<Components.AuditPlanDialog>("Edit Audit Plan",
                new Dictionary<string, object>()
                {
                    { "AuditPlan", plan },
                    { "IsNew", false }
                },
                new DialogOptions() { Width = "1200px", Height = "900px", Resizable = true });

            if (result != null)
            {
                // Refresh ALL dashboard data after editing a plan
                await LoadDashboardDataAsync();
                await NotificationHelper.ShowSuccessAsync("Audit plan updated successfully");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error editing audit plan");
            await NotificationHelper.ShowErrorAsync("Error updating audit plan");
        }
    }

    private async Task ScheduleFromPlan(SMSAuditPlan plan)
    {
        try
        {
            // Simple confirmation dialog instead of full form
            var confirmMessage = $"Schedule audit '{plan.Name}' for {plan.PlannedStartDate:MM/dd/yyyy} - {plan.PlannedEndDate:MM/dd/yyyy}?";

            var confirm = await DialogService.Confirm(
                confirmMessage,
                "Schedule Audit Confirmation",
                new ConfirmOptions()
                {
                    OkButtonText = "Yes, Schedule",
                    CancelButtonText = "Cancel",
                    Width = "500px"
                });

            if (confirm == true)
            {
                // Step 1: Update the audit plan status to "Scheduled"
                plan.Status = "Scheduled";
                plan.UpdatedBy = "CURRENT_USER";
                plan.UpdatedDate = DateTime.UtcNow;

                var updatePlanCommand = new UpdateSMSAuditPlanCommand(plan);
                var updateResult = await Mediator.SendAsync(updatePlanCommand, CancellationToken.None);

                if (updateResult.IsFailure)
                {
                    await NotificationHelper.ShowErrorAsync($"Failed to update audit plan status: {updateResult.Error?.Message}");
                    return;
                }

                // Step 2: Create the actual SMS Audit record
                var createAuditCommand = new CreateSMSAuditCommand(
                    auditPlanCode: plan.Code,
                    name: plan.Name,
                    description: plan.Description ?? "",
                    auditType: plan.AuditType ?? "Internal",
                    scheduledStartDate: plan.PlannedStartDate,
                    scheduledEndDate: plan.PlannedEndDate,
                    leadAuditor: plan.LeadAuditor ?? "TBD",
                    responsibleDepartment: plan.ResponsibleDepartment ?? "Operations",
                    createdBy: "CURRENT_USER"
                );

                var createAuditResult = await Mediator.SendAsync(createAuditCommand, CancellationToken.None);

                if (createAuditResult.IsSuccess)
                {
                    // Refresh ALL dashboard data after scheduling
                    await LoadDashboardDataAsync();
                    await NotificationHelper.ShowSuccessAsync($"Audit '{plan.Name}' has been scheduled successfully and audit record created!");
                }
                else
                {
                    await NotificationHelper.ShowErrorAsync($"Failed to create audit record: {createAuditResult.Error?.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error scheduling audit from plan: {PlanCode}", plan.Code);
            await NotificationHelper.ShowErrorAsync("Error scheduling audit");
        }
    }

    private async Task DeleteAuditPlan(SMSAuditPlan plan)
    {
        try
        {
            var confirm = await DialogService.Confirm(
                $"Are you sure you want to delete the audit plan '{plan.Name}'?",
                "Confirm Delete",
                new ConfirmOptions() { OkButtonText = "Yes", CancelButtonText = "No" });

            if (confirm == true)
            {
                var command = new DeleteSMSAuditPlanCommand(plan.Code, CurrentUserService?.UserDisplayName);
                var result = await Mediator.SendAsync(command, CancellationToken.None);

                if (result.IsSuccess)
                {
                    // Refresh ALL dashboard data after deleting a plan
                    await LoadDashboardDataAsync();
                    await NotificationHelper.ShowSuccessAsync("Audit plan deleted successfully");
                }
                else
                {
                    await NotificationHelper.ShowErrorAsync($"Failed to delete audit plan: {result.Error?.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting audit plan");
            await NotificationHelper.ShowErrorAsync("Error deleting audit plan");
        }
    }

    private async Task ViewAudit(SMSAudit audit)
    {
        Navigation.NavigateToSecure($"/SMSAssurance/AuditDetail/{audit.Code}");
    }

    private async Task StartAudit(SMSAudit audit)
    {
        try
        {
            var command = new StartSMSAuditCommand(audit.Code, "CURRENT_USER");
            var result = await Mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                // Refresh ALL dashboard data after starting an audit
                await LoadDashboardDataAsync();
                await NotificationHelper.ShowSuccessAsync("Audit started successfully");
            }
            else
            {
                await NotificationHelper.ShowErrorAsync($"Failed to start audit: {result.Error?.Message}");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error starting audit");
            await NotificationHelper.ShowErrorAsync("Error starting audit");
        }
    }
    #endregion

    #region Navigation Methods
    private void NavigateToAuditPlans()
    {
        // Navigate to the main audit management page with Audit Plans tab selected
        Navigation.NavigateToSecure("/SMSAssurance/AuditManagement#audit-plans");
    }

    private void NavigateToActiveAudits()
    {
        // Navigate to the main audit management page with Active Audits tab selected
        Navigation.NavigateToSecure("/SMSAssurance/AuditManagement#active-audits");
    }

    private void NavigateToFindings()
    {
        // Navigate to audit finding management page (when implemented)
        Navigation.NavigateToSecure("/SMSAssurance/AuditFindings");
    }

    private void NavigateToEvidence()
    {
        // Navigate to audit evidence management page (when implemented)
        Navigation.NavigateToSecure("/SMSAssurance/AuditEvidence");
    }

    private void NavigateToAuditReports()
    {
        // Navigate to audit reporting page (when implemented)
        Navigation.NavigateToSecure("/SMSAssurance/AuditReports");
    }

    private void NavigateToFindingDetail(string findingCode)
    {
        Navigation.NavigateToSecure($"/SMSAssurance/FindingDetail/{findingCode}");
    }
    #endregion

    #region Helper Methods
    private IQueryable<SMSAuditPlan> GetFilteredAuditPlans(IQueryable<SMSAuditPlan> query)
    {
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            query = query.Where(p => p.Name!.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                   p.Description!.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        if (SelectedStatus != "All")
        {
            query = query.Where(p => p.Status == SelectedStatus);
        }

        if (SelectedType != "All")
        {
            query = query.Where(p => p.AuditType == SelectedType);
        }

        if (SelectedDepartment != "All")
        {
            query = query.Where(p => p.ResponsibleDepartment == SelectedDepartment);
        }

        return query;
    }

    private IQueryable<SMSAudit> GetFilteredAudits(IQueryable<SMSAudit> query)
    {
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            query = query.Where(a => a.Name!.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                   a.Description!.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        if (SelectedStatus != "All")
        {
            query = query.Where(a => a.Status == SelectedStatus);
        }

        if (SelectedType != "All")
        {
            query = query.Where(a => a.AuditType == SelectedType);
        }

        if (SelectedDepartment != "All")
        {
            query = query.Where(a => a.ResponsibleDepartment == SelectedDepartment);
        }

        return query;
    }

    private BadgeStyle GetStatusBadgeStyle(string? status)
    {
        return status switch
        {
            "Completed" => BadgeStyle.Success,
            "In Progress" => BadgeStyle.Primary,
            "Scheduled" => BadgeStyle.Success,
            "Draft" => BadgeStyle.Secondary,
            "Approved" => BadgeStyle.Success,
            "Overdue" => BadgeStyle.Danger,
            _ => BadgeStyle.Light
        };
    }

    private BadgeStyle GetSeverityBadgeStyle(string? severity)
    {
        return severity switch
        {
            "Critical" => BadgeStyle.Danger,
            "Major" => BadgeStyle.Warning,
            "Minor" => BadgeStyle.Info,
            "Observation" => BadgeStyle.Light,
            _ => BadgeStyle.Secondary
        };
    }

    private string GetProgressPercentage(SMSAudit audit)
    {
        if (audit.Status == "Completed") return "100";
        if (audit.Status == "In Progress" && audit.ActualStartDate.HasValue)
        {
            var totalDays = (audit.ScheduledEndDate - audit.ScheduledStartDate).TotalDays;
            var elapsedDays = (DateTime.Now - audit.ActualStartDate.Value).TotalDays;
            var percentage = Math.Min(100, Math.Max(0, (elapsedDays / totalDays) * 100));
            return percentage.ToString("F0");
        }
        return "0";
    }

    private string GetTimeUntilDue(DateTime dueDate)
    {
        var timeSpan = dueDate - DateTime.Now;
        if (timeSpan.TotalDays < 0) return "Overdue";
        if (timeSpan.TotalDays < 1) return "Due today";
        if (timeSpan.TotalDays < 7) return $"{Math.Ceiling(timeSpan.TotalDays)} days";
        return $"{Math.Ceiling(timeSpan.TotalDays / 7)} weeks";
    }
    #endregion

    #region Supporting Types
    public class AuditDashboardStats
    {
        public int TotalAuditPlans { get; set; }
        public int ApprovedPlans { get; set; }
        public int ActiveAudits { get; set; }
        public int CompletedThisMonth { get; set; }
        public int OverdueAudits { get; set; }
        public int CriticalFindings { get; set; }
        public int MajorFindings { get; set; }
        public int FindingsAwaitingAction { get; set; }
    }
    #endregion

    #region Helper Methods for Category Statistics
    private int GetInternalAuditsCount()
    {
        return AllAudits.Count(a => a.AuditType == "Internal" &&
            (a.Status == "Scheduled" || a.Status == "In Progress" || a.Status == "Completed"));
    }

    private int GetExternalAuditsCount()
    {
        return AllAudits.Count(a => a.AuditType == "External" &&
            (a.Status == "Scheduled" || a.Status == "In Progress" || a.Status == "Completed"));
    }

    private int GetManagementReviewsCount()
    {
        return AllAudits.Count(a => a.AuditType == "Management Review" &&
            (a.Status == "Scheduled" || a.Status == "In Progress" || a.Status == "Completed"));
    }

    private int GetCompletedInternalAudits()
    {
        return AllAudits.Count(a => a.AuditType == "Internal" && a.Status == "Completed");
    }

    private int GetPlannedInternalAudits()
    {
        var currentYear = DateTime.Now.Year;
        return AllAuditPlans.Count(ap => ap.AuditType == "Internal" &&
            ap.Status == "Approved" &&
            ap.PlannedStartDate.Year == currentYear);
    }

    private int GetCompletedExternalAudits()
    {
        return AllAudits.Count(a => a.AuditType == "External" && a.Status == "Completed");
    }

    private int GetScheduledExternalAudits()
    {
        var currentYear = DateTime.Now.Year;
        return AllAudits.Count(a => a.AuditType == "External" &&
            (a.Status == "Scheduled" || a.Status == "Completed") &&
            a.ScheduledStartDate.Year == currentYear);
    }

    private int GetCompletedManagementReviews()
    {
        var currentYear = DateTime.Now.Year;
        return AllAudits.Count(a => a.AuditType == "Management Review" &&
            a.Status == "Completed" &&
            a.ActualEndDate.HasValue &&
            a.ActualEndDate.Value.Year == currentYear);
    }

    private int GetRequiredManagementReviews()
    {
        return 1; // Annual requirement per 14 CFR 139.301(e)
    }

    private decimal GetInternalAuditComplianceRate()
    {
        var planned = GetPlannedInternalAudits();
        if (planned == 0) return 100;

        var completed = GetCompletedInternalAudits();
        return Math.Round((decimal)completed / planned * 100, 1);
    }

    private decimal GetExternalAuditComplianceRate()
    {
        var scheduled = GetScheduledExternalAudits();
        if (scheduled == 0) return 100;

        var completed = GetCompletedExternalAudits();
        return Math.Round((decimal)completed / scheduled * 100, 1);
    }

    private decimal GetManagementReviewComplianceRate()
    {
        var required = GetRequiredManagementReviews();
        var completed = GetCompletedManagementReviews();

        return Math.Round((decimal)completed / required * 100, 1);
    }

    private string GetComplianceColor(decimal percentage)
    {
        return percentage switch
        {
            >= 90 => "var(--rz-success)",
            >= 75 => "var(--rz-warning)",
            _ => "var(--rz-danger)"
        };
    }

    private void ClearFilters()
    {
        SearchText = string.Empty;
        SelectedStatus = "All";
        SelectedType = "All";
        SelectedDepartment = "All";
        ApplyFilters();
    }

    private void NavigateToOverdueAudits()
    {
        // Navigate to filtered audit view showing only overdue audits
        Navigation.NavigateToSecure("/SMSAssurance/AuditManagement?status=Overdue");
    }
    #endregion

    #region Apply Filters Method
    private void ApplyFilters()
    {
        try
        {
            var filteredPlans = AllAuditPlans.AsEnumerable();

            // Apply search text filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filteredPlans = filteredPlans.Where(p =>
                    (p.Name?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (p.Code?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (p.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            // Apply status filter
            if (SelectedStatus != "All")
            {
                filteredPlans = filteredPlans.Where(p => p.Status == SelectedStatus);
            }

            // Apply type filter
            if (SelectedType != "All")
            {
                filteredPlans = filteredPlans.Where(p => p.AuditType == SelectedType);
            }

            // Apply department filter
            if (SelectedDepartment != "All")
            {
                filteredPlans = filteredPlans.Where(p => p.ResponsibleDepartment == SelectedDepartment);
            }

            AuditPlans = filteredPlans.ToList();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error applying filters");
            NotificationHelper.ShowErrorAsync("Error applying filters");
        }
    }
    #endregion
}