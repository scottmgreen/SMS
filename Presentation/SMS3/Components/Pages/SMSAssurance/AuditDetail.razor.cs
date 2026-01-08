using Microsoft.AspNetCore.Components;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Queries;
using SMS_Application.Messaging.Commands;
using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Shared.Common;
using Radzen;
using Radzen.Blazor;

namespace SMS3.Components.Pages.SMSAssurance;

public partial class AuditDetail : ComponentBase
{
    #region Parameters
    [Parameter] public string? AuditCode { get; set; }
    #endregion

    #region Injected Services
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<AuditDetail> Logger { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    #endregion

    #region Component State
    private bool IsLoading { get; set; } = true;
    private bool IsUpdating { get; set; } = false;
    private SMSAudit? Audit { get; set; }
    private List<SMSAuditFinding> Findings { get; set; } = new();
    private List<SMSAuditEvidence> Evidence { get; set; } = new();
    private List<SMSAuditChecklistItem> ChecklistItems { get; set; } = new();
    private int ActiveTabIndex { get; set; } = 0;
    
    // Statistics
    private AuditDetailStats Stats { get; set; } = new();
    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrWhiteSpace(AuditCode))
        {
            Navigation.NavigateTo("/SMSAssurance/AuditManagement");
            return;
        }

        await LoadAuditDetailAsync();
    }
    #endregion

    #region Data Loading
    private async Task LoadAuditDetailAsync()
    {
        try
        {
            IsLoading = true;
            StateHasChanged();

            Logger.LogInformation("Loading audit detail for code: {AuditCode}", AuditCode);

            await Task.WhenAll(
                LoadAuditAsync(),
                LoadFindingsAsync(),
                LoadEvidenceAsync(),
                LoadChecklistItemsAsync()
            );

            CalculateStatistics();

            Logger.LogInformation("Audit detail loaded successfully for: {AuditCode}", AuditCode);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading audit detail for code: {AuditCode}", AuditCode);
            ShowErrorNotification("Error loading audit details");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task LoadAuditAsync()
    {
        try
        {
            var query = new GetSMSAuditByCodeQuery(AuditCode!);
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                Audit = result.Value;
            }
            else
            {
                Logger.LogWarning("Audit not found: {AuditCode}", AuditCode);
                ShowErrorNotification("Audit not found");
                Navigation.NavigateTo("/SMSAssurance/AuditManagement");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading audit: {AuditCode}", AuditCode);
            throw;
        }
    }

    private async Task LoadFindingsAsync()
    {
        try
        {
            var query = new GetSMSAuditFindingsByAuditCodeQuery(AuditCode!);
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                Findings = result.Value.OrderByDescending(f => f.DiscoveredDate).ToList();
            }
            else
            {
                Findings = new List<SMSAuditFinding>();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading findings for audit: {AuditCode}", AuditCode);
            Findings = new List<SMSAuditFinding>();
        }
    }

    private async Task LoadEvidenceAsync()
    {
        try
        {
            var query = new GetSMSAuditEvidenceByAuditCodeQuery(AuditCode!);
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                Evidence = result.Value.OrderByDescending(e => e.CollectionDate).ToList();
            }
            else
            {
                Evidence = new List<SMSAuditEvidence>();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading evidence for audit: {AuditCode}", AuditCode);
            Evidence = new List<SMSAuditEvidence>();
        }
    }

    private async Task LoadChecklistItemsAsync()
    {
        try
        {
            var query = new GetSMSAuditChecklistItemsByAuditCodeQuery(AuditCode!);
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                ChecklistItems = result.Value.OrderBy(c => c.ItemNumber).ToList();
            }
            else
            {
                ChecklistItems = new List<SMSAuditChecklistItem>();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading checklist items for audit: {AuditCode}", AuditCode);
            ChecklistItems = new List<SMSAuditChecklistItem>();
        }
    }

    private void CalculateStatistics()
    {
        Stats = new AuditDetailStats
        {
            TotalFindings = Findings.Count,
            CriticalFindings = Findings.Count(f => f.Severity == "Critical"),
            MajorFindings = Findings.Count(f => f.Severity == "Major"),
            MinorFindings = Findings.Count(f => f.Severity == "Minor"),
            ObservationFindings = Findings.Count(f => f.Severity == "Observation"),
            OpenFindings = Findings.Count(f => f.Status == "Open"),
            InProgressFindings = Findings.Count(f => f.Status == "In Progress"),
            ClosedFindings = Findings.Count(f => f.Status == "Closed"),
            TotalEvidence = Evidence.Count,
            DocumentEvidence = Evidence.Count(e => e.EvidenceType == "Document"),
            PhotoEvidence = Evidence.Count(e => e.EvidenceType == "Photo"),
            VideoEvidence = Evidence.Count(e => e.EvidenceType == "Video"),
            TotalChecklistItems = ChecklistItems.Count,
            CompletedChecklistItems = ChecklistItems.Count(c => c.Status == "Completed"),
            PendingChecklistItems = ChecklistItems.Count(c => c.Status == "Pending"),
            NotApplicableItems = ChecklistItems.Count(c => c.Status == "N/A")
        };
    }

    private async Task RefreshData()
    {
        await LoadAuditDetailAsync();
        ShowSuccessNotification("Audit details refreshed successfully");
    }
    #endregion

    #region Audit Actions
    private async Task EditAudit()
    {
        if (Audit == null) return;

        try
        {
            var result = await DialogService.OpenAsync<Components.AuditDialog>("Edit Audit",
                new Dictionary<string, object>()
                {
                    { "Audit", Audit },
                    { "IsNew", false }
                },
                new DialogOptions() { Width = "900px", Height = "700px", Resizable = true });

            if (result != null)
            {
                await LoadAuditAsync();
                ShowSuccessNotification("Audit updated successfully");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error editing audit");
            ShowErrorNotification("Error editing audit");
        }
    }

    private async Task StartAudit()
    {
        if (Audit == null || Audit.Status != "Scheduled") return;

        try
        {
            IsUpdating = true;
            StateHasChanged();

            var command = new UpdateSMSAuditCommand(
                Audit.Code!,
                Audit.Name!,
                Audit.Description,
                Audit.AuditPlanCode,
                Audit.AuditType!,
                Audit.Scope!,
                Audit.Objectives,
                Audit.ScheduledStartDate,
                Audit.ScheduledEndDate,
                DateTime.Now, // ActualStartDate
                null, // ActualEndDate
                Audit.LeadAuditor!,
                Audit.AuditorTeam,
                Audit.ResponsibleDepartment!,
                Audit.ContactPerson,
                Audit.AuditLocation,
                "In Progress", // Status
                Audit.Priority!,
                Audit.ExecutiveSummary,
                Audit.Notes,
                "CURRENT_USER", // UpdatedBy
                DateTime.Now
            );

            var result = await Mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                await LoadAuditAsync();
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
        finally
        {
            IsUpdating = false;
            StateHasChanged();
        }
    }

    private async Task CompleteAudit()
    {
        if (Audit == null || Audit.Status != "In Progress") return;

        try
        {
            var confirm = await DialogService.Confirm(
                "Are you sure you want to complete this audit? This action cannot be undone.",
                "Complete Audit",
                new ConfirmOptions() { OkButtonText = "Yes, Complete", CancelButtonText = "Cancel" });

            if (confirm != true) return;

            IsUpdating = true;
            StateHasChanged();

            var command = new UpdateSMSAuditCommand(
                Audit.Code!,
                Audit.Name!,
                Audit.Description,
                Audit.AuditPlanCode,
                Audit.AuditType!,
                Audit.Scope!,
                Audit.Objectives,
                Audit.ScheduledStartDate,
                Audit.ScheduledEndDate,
                Audit.ActualStartDate,
                DateTime.Now, // ActualEndDate
                Audit.LeadAuditor!,
                Audit.AuditorTeam,
                Audit.ResponsibleDepartment!,
                Audit.ContactPerson,
                Audit.AuditLocation,
                "Completed", // Status
                Audit.Priority!,
                Audit.ExecutiveSummary,
                Audit.Notes,
                "CURRENT_USER", // UpdatedBy
                DateTime.Now
            );

            var result = await Mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                await LoadAuditAsync();
                ShowSuccessNotification("Audit completed successfully");
            }
            else
            {
                ShowErrorNotification($"Failed to complete audit: {result.Error?.Message}");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error completing audit");
            ShowErrorNotification("Error completing audit");
        }
        finally
        {
            IsUpdating = false;
            StateHasChanged();
        }
    }
    #endregion

    #region Finding Management
    private async Task CreateFinding()
    {
        if (Audit == null) return;

        try
        {
            // For now, just show a simple notification since the dialog components don't exist yet
            ShowSuccessNotification("Finding creation feature will be implemented soon");
            
            // TODO: Implement finding creation when AuditFindingDialog is created
            /*
            var newFindingId = new SMSAuditFindingID($"FND-{DateTime.UtcNow:yyyyMMddHHmmss}");
            var newFinding = new SMSAuditFinding(newFindingId, "CURRENT_USER")
            {
                AuditCode = Audit.Code,
                DiscoveredDate = DateTime.Now,
                Status = "Open"
            };

            var result = await DialogService.OpenAsync<Components.AuditFindingDialog>("Create Finding",
                new Dictionary<string, object>()
                {
                    { "Finding", newFinding },
                    { "IsNew", true }
                },
                new DialogOptions() { Width = "800px", Height = "600px", Resizable = true });

            if (result != null)
            {
                await LoadFindingsAsync();
                CalculateStatistics();
                ShowSuccessNotification("Finding created successfully");
            }
            */
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating finding");
            ShowErrorNotification("Error creating finding");
        }
    }

    private async Task EditFinding(SMSAuditFinding finding)
    {
        try
        {
            // For now, just show a simple notification since the dialog components don't exist yet
            ShowSuccessNotification("Finding editing feature will be implemented soon");
            
            // TODO: Implement finding editing when AuditFindingDialog is created
            /*
            var result = await DialogService.OpenAsync<Components.AuditFindingDialog>("Edit Finding",
                new Dictionary<string, object>()
                {
                    { "Finding", finding },
                    { "IsNew", false }
                },
                new DialogOptions() { Width = "800px", Height = "600px", Resizable = true });

            if (result != null)
            {
                await LoadFindingsAsync();
                CalculateStatistics();
                ShowSuccessNotification("Finding updated successfully");
            }
            */
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error editing finding");
            ShowErrorNotification("Error editing finding");
        }
    }

    private void ViewFindingDetail(string findingCode)
    {
        Navigation.NavigateTo($"/SMSAssurance/FindingDetail/{findingCode}");
    }
    #endregion

    #region Evidence Management
    private async Task UploadEvidence()
    {
        if (Audit == null) return;

        try
        {
            // For now, just show a simple notification since the dialog components don't exist yet
            ShowSuccessNotification("Evidence upload feature will be implemented soon");
            
            // TODO: Implement evidence upload when UploadEvidenceDialog is created
            /*
            var result = await DialogService.OpenAsync<Components.UploadEvidenceDialog>("Upload Evidence",
                new Dictionary<string, object>()
                {
                    { "AuditCode", Audit.Code! }
                },
                new DialogOptions() { Width = "600px", Height = "500px", Resizable = true });

            if (result != null)
            {
                await LoadEvidenceAsync();
                CalculateStatistics();
                ShowSuccessNotification("Evidence uploaded successfully");
            }
            */
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error uploading evidence");
            ShowErrorNotification("Error uploading evidence");
        }
    }

    private async Task ViewEvidence(SMSAuditEvidence evidence)
    {
        try
        {
            // For now, just show a simple notification since the dialog components don't exist yet
            ShowSuccessNotification($"Evidence viewing feature will be implemented soon. Evidence: {evidence.Title}");
            
            // TODO: Implement evidence viewing when ViewEvidenceDialog is created
            /*
            await DialogService.OpenAsync<Components.ViewEvidenceDialog>("View Evidence",
                new Dictionary<string, object>()
                {
                    { "Evidence", evidence }
                },
                new DialogOptions() { Width = "800px", Height = "600px", Resizable = true });
            */
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error viewing evidence");
            ShowErrorNotification("Error viewing evidence");
        }
    }
    #endregion

    #region Navigation Methods
    private void NavigateToAuditManagement()
    {
        Navigation.NavigateTo("/SMSAssurance/AuditManagement");
    }

    private void NavigateToFindings()
    {
        Navigation.NavigateTo("/SMSAssurance/AuditFindings");
    }

    private void NavigateToEvidence()
    {
        Navigation.NavigateTo("/SMSAssurance/AuditEvidence");
    }
    #endregion

    #region Helper Methods
    private string GetProgressPercentage()
    {
        if (Audit?.Status == "Completed") return "100";
        if (Audit?.Status == "In Progress" && Audit.ActualStartDate.HasValue)
        {
            var totalDays = (Audit.ScheduledEndDate - Audit.ScheduledStartDate).TotalDays;
            var elapsedDays = (DateTime.Now - Audit.ActualStartDate.Value).TotalDays;
            var percentage = Math.Min(100, Math.Max(0, (elapsedDays / totalDays) * 100));
            return percentage.ToString("F0");
        }
        return "0";
    }

    private double GetChecklistProgress()
    {
        if (Stats.TotalChecklistItems == 0) return 0;
        return (double)Stats.CompletedChecklistItems / Stats.TotalChecklistItems * 100;
    }

    private double GetFindingResolutionRate()
    {
        if (Stats.TotalFindings == 0) return 100;
        return (double)Stats.ClosedFindings / Stats.TotalFindings * 100;
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

    private BadgeStyle GetFindingStatusBadgeStyle(string? status)
    {
        return status switch
        {
            "Closed" => BadgeStyle.Success,
            "In Progress" => BadgeStyle.Warning,
            "Open" => BadgeStyle.Danger,
            _ => BadgeStyle.Secondary
        };
    }

    private BadgeStyle GetStatusBadgeStyle(string? status)
    {
        return status switch
        {
            "Completed" => BadgeStyle.Success,
            "In Progress" => BadgeStyle.Primary,
            "Scheduled" => BadgeStyle.Info,
            "Cancelled" => BadgeStyle.Danger,
            "On Hold" => BadgeStyle.Warning,
            _ => BadgeStyle.Secondary
        };
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
    public class AuditDetailStats
    {
        public int TotalFindings { get; set; }
        public int CriticalFindings { get; set; }
        public int MajorFindings { get; set; }
        public int MinorFindings { get; set; }
        public int ObservationFindings { get; set; }
        public int OpenFindings { get; set; }
        public int InProgressFindings { get; set; }
        public int ClosedFindings { get; set; }
        public int TotalEvidence { get; set; }
        public int DocumentEvidence { get; set; }
        public int PhotoEvidence { get; set; }
        public int VideoEvidence { get; set; }
        public int TotalChecklistItems { get; set; }
        public int CompletedChecklistItems { get; set; }
        public int PendingChecklistItems { get; set; }
        public int NotApplicableItems { get; set; }
    }
    #endregion
}