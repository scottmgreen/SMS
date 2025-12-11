using SMS_Domain.Entities;
using SMS_Domain.ValueObjects; 
using SMS_Application.Messaging.Queries;
using SMS_Application.Messaging.Commands;
using SMS_Application.Interfaces;
using SMS_Shared.Common;
using Radzen;

namespace SMS3.Components.Pages.SMSRiskManagement;

public partial class Investigations : ComponentBase
{
    #region Injected Services
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private ILogger<Investigations> Logger { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    #endregion

    #region Parameters
    [Parameter] public string InvestigationId { get; set; } = default!;
    [Parameter] public string? HazardId { get; set; }
    #endregion

    #region State Properties
    private bool IsLoading { get; set; } = true;
    private bool IsSaving { get; set; } = false;
    private int selectedTabIndex { get; set; } = 0;
    private bool showDecisionForm { get; set; } = false;

    public SMS_Domain.Entities.Investigation? InvestigationEntity { get; set; }
    public List<Interview> Interviews { get; set; } = new();
    public List<SMSApplicationUser> AvailableInvestigators { get; set; } = new();
    public List<HazardFile> EvidenceFiles { get; set; } = new();
    #endregion

    #region Child Component References
    private SMS3.Components.Pages.SMSRiskManagement.Components.InterviewsManager? interviewsManager;
    private SMS3.Components.Pages.SMSRiskManagement.Components.EvidenceFilesManager? evidenceFilesManager;
    #endregion

    #region Dropdown Options
    private readonly List<DropdownOption> StatusOptions = new()
    {
        new() { Value = "Assigned", Text = "Assigned" },
        new() { Value = "InProgress", Text = "In Progress" },
        new() { Value = "OnHold", Text = "On Hold" },
        new() { Value = "Completed", Text = "Completed" },
        new() { Value = "Cancelled", Text = "Cancelled" }
    };

    private readonly List<DropdownOption> DecisionTypeOptions = new()
    {
        new() { Value = "NoFurtherAction", Text = "No Further Action" },
        new() { Value = "ContinueMonitoring", Text = "Continue Monitoring" },
        new() { Value = "RequiresMitigation", Text = "Requires Mitigation" },
        new() { Value = "EscalateToRiskAssessment", Text = "Escalate to Risk Assessment" },
        new() { Value = "ReferToExternalAgency", Text = "Refer to External Agency" }
    };
    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        await LoadInvestigationData();
    }
    #endregion

    #region Data Loading
    private async Task LoadInvestigationData()
    {
        try
        {
            IsLoading = true;

            if (string.IsNullOrWhiteSpace(InvestigationId))
            {
                ShowErrorNotification("Investigation ID is required");
                Navigation.NavigateTo("/Listings/Investigations");
                return;
            }

            Logger.LogInformation("Loading investigation: {InvestigationId} with HazardId: {HazardId}", InvestigationId, HazardId);

            // Load investigation
            var investigationQuery = new GetInvestigationByIdQuery(new InvestigationID(InvestigationId));
            var investigationResult = await Mediator.SendAsync(investigationQuery, CancellationToken.None);
            
            if (investigationResult.IsSuccess)
            {
                InvestigationEntity = investigationResult.Value;
                
                // If HazardId is provided in route but not set in entity, set it
                if (!string.IsNullOrWhiteSpace(HazardId) && string.IsNullOrWhiteSpace(InvestigationEntity.HazardCode))
                {
                    InvestigationEntity.HazardCode = HazardId;
                    Logger.LogInformation("Set HazardCode from route parameter: {HazardCode}", HazardId);
                }
                
                // Validate that HazardCode is set
                if (string.IsNullOrWhiteSpace(InvestigationEntity.HazardCode) || InvestigationEntity.HazardCode == "HAZ-UNKNOWN")
                {
                    ShowErrorNotification("Investigation has invalid or missing HazardCode. Please check the investigation setup.");
                    Logger.LogError("Investigation {InvestigationId} has invalid HazardCode: {HazardCode}", 
                        InvestigationId, InvestigationEntity.HazardCode ?? "NULL");
                }
                
                showDecisionForm = InvestigationEntity?.HasDecision == true;
                
                Logger.LogInformation("Loaded investigation: {Code} with HazardCode: {HazardCode}", 
                    InvestigationEntity?.Code, InvestigationEntity?.HazardCode);
            }
            else
            {
                ShowErrorNotification($"Failed to load investigation: {investigationResult.Error?.Message}");
                Logger.LogError("Failed to load investigation {InvestigationId}: {Error}", 
                    InvestigationId, investigationResult.Error?.Message);
                return;
            }

            // Load available investigators
            await LoadAvailableInvestigators();

            // Load interviews for this investigation
            await LoadInterviews();

            // Load evidence files
            await LoadEvidenceFiles();

        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading investigation data for: {InvestigationId}", InvestigationId);
            ShowErrorNotification("Error loading investigation data");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task LoadAvailableInvestigators()
    {
        try
        {
            var usersQuery = new GetAllSMSApplicationUsersQuery();
            var usersResult = await Mediator.SendAsync(usersQuery, CancellationToken.None);
            
            if (usersResult.IsSuccess && usersResult.Value != null)
            {
                AvailableInvestigators = usersResult.Value.ToList();
                Logger.LogInformation("Loaded {Count} available investigators", AvailableInvestigators.Count);
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Could not load available investigators");
        }
    }

    private async Task LoadInterviews()
    {
        try
        {
            var interviewsQuery = new GetAllInterviewsQuery();
            var interviewsResult = await Mediator.SendAsync(interviewsQuery, CancellationToken.None);
            
            if (interviewsResult.IsSuccess && interviewsResult.Value != null)
            {
                Interviews = interviewsResult.Value
                    .Where(i => i.InvestigationCode == InvestigationEntity?.Code)
                    .ToList();
                
                Logger.LogInformation("Loaded {Count} interviews for investigation", Interviews.Count);
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Could not load interviews");
        }
    }

    private async Task LoadEvidenceFiles()
    {
        try
        {
            if (InvestigationEntity?.HazardCode != null)
            {
                var filesQuery = new GetHazardFilesByHazardCodeQuery(InvestigationEntity.HazardCode, false, "Evidence");
                var filesResult = await Mediator.SendAsync(filesQuery, CancellationToken.None);
                
                if (filesResult.IsSuccess && filesResult.Value != null)
                {
                    EvidenceFiles = filesResult.Value.ToList();
                    Logger.LogInformation("Loaded {Count} evidence files", EvidenceFiles.Count);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Could not load evidence files");
        }
    }
    #endregion

    #region Investigation Actions
    private async Task SaveInvestigation()
    {
        try
        {
            if (InvestigationEntity == null) return;

            IsSaving = true;
            StateHasChanged();

            var updateCommand = new UpdateInvestigationCommand(InvestigationEntity);
            var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification("Investigation updated successfully");
                Logger.LogInformation("Investigation {Code} updated successfully", InvestigationEntity.Code);
                
                // Refresh the investigation data
                await LoadInvestigationData();
            }
            else
            {
                ShowErrorNotification($"Failed to update investigation: {result.Error?.Message}");
                Logger.LogError("Failed to update investigation {Code}: {Error}", 
                    InvestigationEntity.Code, result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving investigation");
            ShowErrorNotification("Error saving investigation");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    private async Task CompleteInvestigation()
    {
        if (InvestigationEntity == null) return;

        try
        {
            // Validate that decision is recorded
            if (!InvestigationEntity.HasDecision)
            {
                ShowErrorNotification("Investigation decision must be recorded before completion");
                return;
            }

            var confirmed = await DialogService.Confirm(
                "Are you sure you want to complete this investigation? This action cannot be undone.",
                "Complete Investigation",
                new ConfirmOptions() { OkButtonText = "Yes, Complete", CancelButtonText = "Cancel" });

            if (confirmed == true)
            {
                InvestigationEntity.Complete();
                await SaveInvestigation();
                ShowSuccessNotification("Investigation completed successfully");
                
                // Navigate based on decision type
                await HandleInvestigationCompletion();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error completing investigation");
            ShowErrorNotification($"Error completing investigation: {ex.Message}");
        }
    }

    private async Task HandleInvestigationCompletion()
    {
        if (InvestigationEntity?.DecisionType == null) return;

        // Show completion dialog with next steps
        var nextStepMessage = InvestigationEntity.NextStepsMessage;
        
        await DialogService.Alert(nextStepMessage, "Investigation Completed", new AlertOptions() { OkButtonText = "OK" });

        // Navigate based on decision
        switch (InvestigationEntity.DecisionType)
        {
            case "EscalateToRiskAssessment":
                // Navigate back to validation workflow
                if (!string.IsNullOrEmpty(InvestigationEntity.HazardCode))
                {
                    Navigation.NavigateTo($"/SMSRiskManagement/ReportValidation/{InvestigationEntity.ReportCode}");
                }
                break;
            
            case "NoFurtherAction":
            case "ReferToExternalAgency":
                // Navigate to hazard details to show closure
                if (!string.IsNullOrEmpty(InvestigationEntity.HazardCode))
                {
                    Navigation.NavigateTo($"/SMSRiskManagement/Hazards/{InvestigationEntity.HazardCode}");
                }
                break;
            
            case "ContinueMonitoring":
            case "RequiresMitigation":
                // Stay on investigation page but refresh data
                await LoadInvestigationData();
                break;
            
            default:
                Navigation.NavigateTo("/Listings/Investigations");
                break;
        }
    }

    private async Task UpdateInvestigationDetails()
    {
        if (InvestigationEntity == null) return;

        try
        {
            InvestigationEntity.UpdateDetails(
                InvestigationEntity.InvestigationNotes,
                InvestigationEntity.InvestigationPlan,
                InvestigationEntity.InvestigationObjectives);

            await SaveInvestigation();
            ShowSuccessNotification("Investigation details updated");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating investigation details");
            ShowErrorNotification($"Error updating details: {ex.Message}");
        }
    }

    private async Task RecordDecision()
    {
        if (InvestigationEntity == null) return;

        try
        {
            if (string.IsNullOrWhiteSpace(InvestigationEntity.DecisionType) ||
                string.IsNullOrWhiteSpace(InvestigationEntity.DecisionRationale) ||
                string.IsNullOrWhiteSpace(InvestigationEntity.DecisionMaker))
            {
                ShowErrorNotification("Decision type, rationale, and decision maker are required");
                return;
            }

            InvestigationEntity.RecordDecision(
                InvestigationEntity.DecisionType,
                InvestigationEntity.DecisionRationale,
                InvestigationEntity.DecisionMaker,
                InvestigationEntity.NextSteps,
                InvestigationEntity.ReferralDetails);

            await SaveInvestigation();
            showDecisionForm = true;
            ShowSuccessNotification("Investigation decision recorded");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error recording investigation decision");
            ShowErrorNotification($"Error recording decision: {ex.Message}");
        }
    }
    #endregion

    #region UI Helpers
    private string GetStatusBadgeText()
    {
        return InvestigationEntity?.StatusDisplay ?? "Unknown";
    }

    private BadgeStyle GetStatusBadgeStyle()
    {
        return InvestigationEntity?.Status switch
        {
            "Assigned" => BadgeStyle.Secondary,
            "InProgress" => BadgeStyle.Info,
            "Completed" => BadgeStyle.Success,
            "OnHold" => BadgeStyle.Warning,
            "Cancelled" => BadgeStyle.Danger,
            _ => BadgeStyle.Light
        };
    }

    private void NavigateToListings()
    {
        Navigation.NavigateTo("/Listings/Investigations");
    }

    private string GetFileCountText()
    {
        var fileCount = EvidenceFiles.Count;
        return fileCount == 0 ? "No Files" : $"{fileCount} File{(fileCount == 1 ? "" : "s")}";
    }

    private string GetCurrentUserCode()
    {
        // Implement your user identification logic here
        // For now, return a placeholder
        return "SYSTEM_USER"; // Replace with actual user identification logic
    }
    #endregion

    #region Notifications
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

    #region Models
    public class DropdownOption
    {
        public object Value { get; set; } = default!;
        public string Text { get; set; } = "";
    }
    #endregion
}