using Microsoft.AspNetCore.Components;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Queries;
using SMS_Application.Messaging.Commands;
using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Shared.Common;
using Radzen;
using Radzen.Blazor;
using SMS3.Components.Pages.SMSAssurance.Components;

namespace SMS3.Components.Pages.SMSAssurance;

public partial class AuditManagement : ComponentBase
{
    #region Injected Services
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<AuditManagement> Logger { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    #endregion

    #region Component State
    private bool IsLoading { get; set; } = true;
    private string? SearchText { get; set; }
    private string? SelectedStatus { get; set; } = "All";
    private string? SelectedType { get; set; } = "All";
    private string? SelectedDepartment { get; set; } = "All";
    private DateTime? StartDate { get; set; }
    private DateTime? EndDate { get; set; }
    
    // Data Lists
    private List<SMSAuditPlan> AuditPlans { get; set; } = new();
    private List<SMSAudit> ActiveAudits { get; set; } = new();
    private List<SMSAuditFinding> RecentFindings { get; set; } = new();
    private List<SMSAuditEvidence> RecentEvidence { get; set; } = new();
    
    // Dashboard Statistics
    private AuditDashboardStats DashboardStats { get; set; } = new();
    
    // Grid References
    private RadzenDataGrid<SMSAuditPlan>? auditPlansGrid;
    private RadzenDataGrid<SMSAudit>? activeAuditsGrid;
    #endregion

    #region Filter Options
    public List<string> StatusOptions { get; } = new()
    {
        "All", "Draft", "Approved", "Scheduled", "In Progress", "Completed", "Overdue"
    };

    public List<string> TypeOptions { get; } = new()
    {
        "All", "Internal", "External", "Regulatory", "Management Review", "Process Audit", "Compliance"
    };

    public List<string> DepartmentOptions { get; } = new()
    {
        "All", "Airport Operations", "Security", "Maintenance", "Ground Handling", "Air Traffic Control", "Safety"
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
            ShowErrorNotification("Error loading audit management data");
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
            }
            else
            {
                AuditPlans = new List<SMSAuditPlan>();
                Logger.LogWarning("Failed to load audit plans: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading audit plans");
            AuditPlans = new List<SMSAuditPlan>();
        }
    }

    private async Task LoadActiveAuditsAsync()
    {
        try
        {
            var query = new GetAllSMSAuditsQuery();
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                ActiveAudits = result.Value
                    .Where(a => a.Status == "In Progress" || a.Status == "Scheduled")
                    .OrderBy(a => a.ScheduledStartDate)
                    .ToList();
            }
            else
            {
                ActiveAudits = new List<SMSAudit>();
                Logger.LogWarning("Failed to load active audits: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading active audits");
            ActiveAudits = new List<SMSAudit>();
        }
    }

    private async Task LoadRecentFindingsAsync()
    {
        try
        {
            // Temporarily comment out since query doesn't exist
            // var query = new GetAllSMSAuditFindingsQuery();
            // var result = await Mediator.SendAsync(query, CancellationToken.None);

            // For now just use empty list
            RecentFindings = new List<SMSAuditFinding>();
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
            //var query = new GetAllSMSAuditEvidenceQuery();
            //var result = await Mediator.SendAsync(query, CancellationToken.None);

            //if (result.IsSuccess && result.Value != null)
            //{
            //    RecentEvidence = result.Value
            //        .OrderByDescending(e => e.CollectionDate)
            //        .Take(5)
            //        .ToList();
            //}
            //else
            //{
            //    RecentEvidence = new List<SMSAuditEvidence>();
            //    Logger.LogWarning("Failed to load recent evidence: {Error}", result.Error?.Message);
            //}
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
            // Calculate dashboard statistics from loaded data
            DashboardStats = new AuditDashboardStats
            {
                TotalAuditPlans = AuditPlans.Count,
                ApprovedPlans = AuditPlans.Count(p => p.Status == "Approved"),
                ActiveAudits = ActiveAudits.Count(a => a.Status == "In Progress"),
                CompletedThisMonth = ActiveAudits.Count(a => a.Status == "Completed" && 
                    a.ActualEndDate?.Month == DateTime.Now.Month &&
                    a.ActualEndDate?.Year == DateTime.Now.Year),
                OverdueAudits = ActiveAudits.Count(a => a.Status == "Overdue" || 
                    (a.ScheduledEndDate < DateTime.Now && a.ActualEndDate == null)),
                CriticalFindings = RecentFindings.Count(f => f.Severity == "Critical"),
                MajorFindings = RecentFindings.Count(f => f.Severity == "Major"),
                FindingsAwaitingAction = RecentFindings.Count(f => f.Status == "Open" || f.Status == "In Progress")
            };
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
        ShowSuccessNotification("Audit Management dashboard refreshed successfully");
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
                    { "AuditPlan", new SMSAuditPlan(new SMSAuditPlanID("AP-0000"), "SYSTEM") },
                    { "IsNew", true }
                },
                new DialogOptions() { Width = "800px", Height = "600px", Resizable = true });

            if (result != null)
            {
                await LoadAuditPlansAsync();
                ShowSuccessNotification("Audit plan created successfully");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating audit plan");
            ShowErrorNotification("Error creating audit plan");
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
                new DialogOptions() { Width = "800px", Height = "600px", Resizable = true });

            if (result != null)
            {
                await LoadAuditPlansAsync();
                ShowSuccessNotification("Audit plan updated successfully");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error editing audit plan");
            ShowErrorNotification("Error updating audit plan");
        }
    }

    private async Task ScheduleFromPlan(SMSAuditPlan plan)
    {
        try
        {
            var newAudit = new SMSAudit(new SMSAuditID("AD-0000"),"SYSTEM")
            {
                AuditPlanCode = plan.Code,
                Name = plan.Name,
                Description = plan.Description,
                AuditType = plan.AuditType,
                Scope = plan.Scope,
                Objectives = plan.Objectives,
                ScheduledStartDate = plan.PlannedStartDate,
                ScheduledEndDate = plan.PlannedEndDate,
                LeadAuditor = plan.LeadAuditor,
                AuditorTeam = plan.AuditorTeam,
                ResponsibleDepartment = plan.ResponsibleDepartment,
                Status = "Scheduled"
            };

            var result = await DialogService.OpenAsync<AuditDialog>("Schedule Audit",
                new Dictionary<string, object>()
                {
                    { "Audit", newAudit },
                    { "IsNew", true }
                },
                new DialogOptions() { Width = "900px", Height = "700px", Resizable = true });

            if (result != null)
            {
                await LoadActiveAuditsAsync();
                ShowSuccessNotification("Audit scheduled successfully");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error scheduling audit");
            ShowErrorNotification("Error scheduling audit");
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
                var command = new DeleteSMSAuditPlanCommand(plan.Code,"SYSTEM");
                var result = await Mediator.SendAsync(command, CancellationToken.None);

                if (result.IsSuccess)
                {
                    await LoadAuditPlansAsync();
                    ShowSuccessNotification("Audit plan deleted successfully");
                }
                else
                {
                    ShowErrorNotification($"Failed to delete audit plan: {result.Error?.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting audit plan");
            ShowErrorNotification("Error deleting audit plan");
        }
    }

    private async Task ViewAudit(SMSAudit audit)
    {
        Navigation.NavigateTo($"/SMSAssurance/AuditDetail/{audit.Code}");
    }

    private async Task StartAudit(SMSAudit audit)
    {
        try
        {
            var command = new UpdateSMSAuditCommand(
                audit.Code!,
                audit.Name!,
                audit.Description,
                audit.AuditPlanCode,
                audit.AuditType!,
                audit.Scope!,
                audit.Objectives,
                audit.ScheduledStartDate,
                audit.ScheduledEndDate,
                DateTime.Now, // ActualStartDate
                null, // ActualEndDate
                audit.LeadAuditor!,
                audit.AuditorTeam,
                audit.ResponsibleDepartment!,
                audit.ContactPerson,
                audit.AuditLocation,
                "In Progress", // Status
                audit.Priority!,
                audit.ExecutiveSummary,
                audit.Notes,
                "SYSTEM", // UpdatedBy
                DateTime.Now
            );

            var result = await Mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                await LoadActiveAuditsAsync();
                await LoadDashboardStatsAsync();
                ShowSuccessNotification("Audit started successfully");
            }
            else
            {
                ShowErrorNotification($"Failed to start audit: {result.Error?.Message}");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error starting audit");
            ShowErrorNotification("Error starting audit");
        }
    }
    #endregion

    #region Navigation Methods
    private void NavigateToAuditPlans()
    {
        Navigation.NavigateTo("/SMSAssurance/AuditPlans");
    }

    private void NavigateToActiveAudits()
    {
        Navigation.NavigateTo("/SMSAssurance/ActiveAudits");
    }

    private void NavigateToFindings()
    {
        Navigation.NavigateTo("/SMSAssurance/AuditFindings");
    }

    private void NavigateToEvidence()
    {
        Navigation.NavigateTo("/SMSAssurance/AuditEvidence");
    }

    private void NavigateToAuditReports()
    {
        Navigation.NavigateTo("/SMSAssurance/AuditReports");
    }

    private void NavigateToFindingDetail(string findingCode)
    {
        Navigation.NavigateTo($"/SMSAssurance/FindingDetail/{findingCode}");
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

        if (StartDate.HasValue)
        {
            query = query.Where(p => p.PlannedStartDate >= StartDate.Value);
        }

        if (EndDate.HasValue)
        {
            query = query.Where(p => p.PlannedEndDate <= EndDate.Value);
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

        if (StartDate.HasValue)
        {
            query = query.Where(a => a.ScheduledStartDate >= StartDate.Value);
        }

        if (EndDate.HasValue)
        {
            query = query.Where(a => a.ScheduledEndDate <= EndDate.Value);
        }

        return query;
    }

    private BadgeStyle GetStatusBadgeStyle(string? status)
    {
        return status switch
        {
            "Completed" => BadgeStyle.Success,
            "In Progress" => BadgeStyle.Primary,
            "Scheduled" => BadgeStyle.Info,
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

    #region Notification Methods
    private void ShowSuccessNotification(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = "Success",
            Detail = message,
            Duration = 4000
        });
    }

    private void ShowErrorNotification(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = "Error",
            Detail = message,
            Duration = 6000
        });
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
}