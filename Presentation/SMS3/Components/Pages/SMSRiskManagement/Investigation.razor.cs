using SMS_Domain.Entities;
using SMS_Domain.ValueObjects; 
using SMS_Domain.Enums;
using SMS_Application.Messaging.Queries;
using SMS_Application.Messaging.Commands;
using SMS_Application.Interfaces;
using SMS_Shared.Common;
using Radzen;

namespace SMS3.Components.Pages.SMSRiskManagement;

public partial class Investigation : ComponentBase
{
    #region Injected Services
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private ILogger<Investigation> Logger { get; set; } = default!;
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
        new() { Value = InvestigationStatus.InProgress, Text = "In Progress" },
        new() { Value = InvestigationStatus.OnHold, Text = "On Hold" },
        new() { Value = InvestigationStatus.Completed, Text = "Completed" },
        new() { Value = InvestigationStatus.Cancelled, Text = "Cancelled" }
    };

    private readonly List<DropdownOption> DecisionTypeOptions = new()
    {
        new() { Value = "SMSRisk", Text = "SMS Risk - Return to Validation" },
        new() { Value = "NoSMSRisk", Text = "No SMS Risk - Close Hazard" },
        new() { Value = "RequiresMoreInvestigation", Text = "Requires More Investigation" },
        new() { Value = "ReferExternal", Text = "Refer to External Organization" }
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

            // Load investigation
            var investigationQuery = new GetInvestigationByIdQuery(new InvestigationID(InvestigationId));
            var investigationResult = await Mediator.SendAsync(investigationQuery, CancellationToken.None);
            
            if (investigationResult.IsSuccess)
            {
                InvestigationEntity = investigationResult.Value;
                showDecisionForm = InvestigationEntity?.HasDecision == true;
                
                Logger.LogInformation("Loaded investigation: {Code}", InvestigationEntity?.Code);
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
                var completeResult = InvestigationEntity.CompleteInvestigation();
                if (completeResult.IsSuccess)
                {
                    await SaveInvestigation();
                    ShowSuccessNotification("Investigation completed successfully");
                    
                    // Navigate based on decision type
                    await HandleInvestigationCompletion();
                }
                else
                {
                    ShowErrorNotification($"Failed to complete investigation: {completeResult.Error?.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error completing investigation");
            ShowErrorNotification("Error completing investigation");
        }
    }

    private async Task HandleInvestigationCompletion()
    {
        if (InvestigationEntity?.DecisionType == null) return;

        // Show completion dialog with next steps
        var nextStepMessage = InvestigationEntity.GetNextStepMessage();
        
        await DialogService.Alert(nextStepMessage, "Investigation Completed", new AlertOptions() { OkButtonText = "OK" });

        // Navigate based on decision
        switch (InvestigationEntity.DecisionType)
        {
            case "SMSRisk":
                // Navigate back to validation workflow
                if (!string.IsNullOrEmpty(InvestigationEntity.HazardCode))
                {
                    Navigation.NavigateTo($"/SMSRiskManagement/ReportValidation/{InvestigationEntity.ReportCode}");
                }
                break;
            
            case "NoSMSRisk":
            case "ReferExternal":
                // Navigate to hazard details to show closure
                if (!string.IsNullOrEmpty(InvestigationEntity.HazardCode))
                {
                    Navigation.NavigateTo($"/SMSRiskManagement/Hazards/{InvestigationEntity.HazardCode}");
                }
                break;
            
            case "RequiresMoreInvestigation":
                // Stay on investigation page but refresh data
                await LoadInvestigationData();
                break;
            
            default:
                Navigation.NavigateTo("/Listings/Investigations");
                break;
        }
    }
    #endregion

    #region UI Helpers
    private string GetStatusBadgeText()
    {
        return InvestigationEntity?.Status.ToString() ?? "Unknown";
    }

    private BadgeStyle GetStatusBadgeStyle()
    {
        return InvestigationEntity?.Status.Value switch
        {
            "IN_PROGRESS" => BadgeStyle.Info,
            "COMPLETED" => BadgeStyle.Success,
            "ON_HOLD" => BadgeStyle.Warning,
            "CANCELLED" => BadgeStyle.Danger,
            _ => BadgeStyle.Secondary
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