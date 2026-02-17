namespace SMS3.Components.Pages.SMSRiskManagement.Components;

public partial class InterviewsManager : ComponentBase
{
    #region Injected Services
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private ILogger<InterviewsManager> Logger { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    #endregion

    #region Parameters
    [Parameter] public string InvestigationCode { get; set; } = default!;
    [Parameter] public EventCallback OnInterviewsChanged { get; set; }
    #endregion

    #region State Properties
    private bool IsLoading { get; set; } = true;
    public List<Interview> Interviews { get; set; } = new();
    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        await LoadInterviews();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (!string.IsNullOrEmpty(InvestigationCode))
        {
            await LoadInterviews();
        }
    }
    #endregion

    #region Public Methods
    public async Task RefreshInterviews()
    {
        await LoadInterviews();

        // Notify parent component of changes
        if (OnInterviewsChanged.HasDelegate)
        {
            await OnInterviewsChanged.InvokeAsync();
        }

        StateHasChanged();
    }
    #endregion

    #region Data Loading
    private async Task LoadInterviews()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(InvestigationCode))
            {
                Logger.LogWarning("LoadInterviews called with empty InvestigationCode");
                return;
            }

            IsLoading = true;

            var query = new GetAllInterviewsQuery();
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                Interviews = result.Value
                    .Where(i => i.InvestigationCode == InvestigationCode)
                    .OrderBy(i => i.InterviewDate ?? DateTime.MaxValue)
                    .ToList();

                Logger.LogInformation("Loaded {Count} interviews for investigation {Code} (Total available: {Total})",
                    Interviews.Count, InvestigationCode, result.Value.Count);
            }
            else
            {
                Logger.LogError("Failed to load interviews: {Error}", result.Error?.Message);
                Interviews = new List<Interview>();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading interviews for investigation: {Code}", InvestigationCode);
            ShowErrorNotification("Error loading interviews");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }
    #endregion

    #region Interview Actions
    private async Task ShowCreateInterviewDialog()
    {
        var options = new DialogOptions()
        {
            Width = "100%",
            Height = "100%",
            Resizable = false,
            Draggable = false,
            CloseDialogOnOverlayClick = true,
            CloseDialogOnEsc = true,
            ShowTitle = false,
            ShowClose = false,
            CssClass = "custom-modal-dialog"
        };

        var parameters = new Dictionary<string, object>
        {
            { "InvestigationCode", InvestigationCode }
        };

        var result = await DialogService.OpenAsync<CreateInterviewDialog>(
            "", // No title since our custom modal has its own header
            parameters,
            options);

        if (result == true)
        {
            await RefreshInterviews();
            ShowSuccessNotification("Interview scheduled successfully");
        }
    }

    private async Task ViewInterview(Interview interview)
    {
        var options = new DialogOptions()
        {
            Width = "100%",
            Height = "100%",
            Resizable = false,
            Draggable = false,
            CloseDialogOnOverlayClick = true,    // ✅ FIXED: Allow overlay click to close
            CloseDialogOnEsc = true,             // ✅ FIXED: Allow ESC to close
            ShowTitle = false,                   // ✅ FIXED: No title (your custom modal has its own)
            ShowClose = false,
            CssClass = "custom-modal-dialog"
        };

        var parameters = new Dictionary<string, object>
        {
            { "Interview", interview }
        };

        await DialogService.OpenAsync<ViewInterviewDialog>(
            "", // No title since our custom modal has its own header
            parameters,
            options);
    }

    private async Task EditInterview(Interview interview)
    {
        var options = new DialogOptions()
        {
            Width = "100%",
            Height = "100%",
            Resizable = false,
            Draggable = false,
            CloseDialogOnOverlayClick = true,
            CloseDialogOnEsc = true,
            ShowTitle = false,
            ShowClose = false,
            CssClass = "custom-modal-dialog"
        };

        var parameters = new Dictionary<string, object>
        {
            { "Interview", interview }
        };

        var result = await DialogService.OpenAsync<EditInterviewDialog>(
            "", // No title since our custom modal has its own header
            parameters,
            options);

        if (result == true)
        {
            await RefreshInterviews();
            ShowSuccessNotification("Interview updated successfully");
        }
    }

    private async Task StartInterview(Interview interview)
    {
        try
        {
            var confirmed = await DialogService.Confirm(
                $"Are you ready to start the interview with {interview.PersonInterviewed}? This will open the interview dialog for conducting the interview.",
                "Start Interview",
                new ConfirmOptions() { OkButtonText = "Start Interview", CancelButtonText = "Cancel" });

            if (confirmed == true)
            {
                var startResult = interview.StartInterview();
                if (startResult.IsSuccess)
                {
                    var updateCommand = new UpdateInterviewCommand(interview);
                    var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

                    if (result.IsSuccess)
                    {
                        await RefreshInterviews();
                        ShowSuccessNotification("Interview started - Opening interview dialog for conducting");

                        // Immediately open the EditInterviewDialog to conduct the interview
                        await OpenConductInterviewDialog(interview);
                    }
                    else
                    {
                        ShowErrorNotification($"Failed to start interview: {result.Error?.Message}");
                    }
                }
                else
                {
                    ShowErrorNotification($"Cannot start interview: {startResult.Error?.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error starting interview: {Code}", interview.Code);
            ShowErrorNotification("Error starting interview");
        }
    }

    private async Task OpenConductInterviewDialog(Interview interview)
    {
        var options = new DialogOptions()
        {
            Width = "100%",
            Height = "100%",
            Resizable = false,
            Draggable = false,
            CloseDialogOnOverlayClick = false, // Don't allow overlay click when conducting
            CloseDialogOnEsc = false, // Don't allow ESC to close when conducting
            ShowTitle = false,
            ShowClose = false,
            CssClass = "custom-modal-dialog"
        };

        var parameters = new Dictionary<string, object>
        {
            { "Interview", interview }
        };

        var result = await DialogService.OpenAsync<EditInterviewDialog>(
            "", // No title since our custom modal has its own header
            parameters,
            options);

        if (result == true)
        {
            await RefreshInterviews();
            ShowSuccessNotification("Interview session completed");
        }
    }

    private async Task CompleteInterview(Interview interview)
    {
        var options = new DialogOptions()
        {
            Width = "100%",
            Height = "100%",
            Resizable = false,
            Draggable = false,
            CloseDialogOnOverlayClick = false, // Don't allow overlay click for completion
            CloseDialogOnEsc = false, // Don't allow ESC to close completion dialog
            ShowTitle = false,
            ShowClose = false,
            CssClass = "custom-modal-dialog"
        };

        var parameters = new Dictionary<string, object>
        {
            { "Interview", interview }
        };

        var result = await DialogService.OpenAsync<CompleteInterviewDialog>(
            "", // No title since our custom modal has its own header
            parameters,
            options);

        if (result == true)
        {
            await RefreshInterviews();
            ShowSuccessNotification("Interview completed successfully");
        }
    }

    private async Task CancelInterview(Interview interview)
    {
        try
        {
            var confirmed = await DialogService.Confirm(
                $"Are you sure you want to cancel the interview with {interview.PersonInterviewed}?",
                "Cancel Interview",
                new ConfirmOptions() { OkButtonText = "Yes, Cancel", CancelButtonText = "Keep Interview" });

            if (confirmed == true)
            {
                // Use a default reason since Radzen doesn't have a built-in prompt
                var reason = "Interview cancelled by investigator";

                var cancelResult = interview.CancelInterview(reason);
                if (cancelResult.IsSuccess)
                {
                    var updateCommand = new DeleteInterviewCommand(new InterviewID(interview.Code));
                    var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

                    if (result.IsSuccess)
                    {
                        await RefreshInterviews();
                        ShowSuccessNotification("Interview cancelled");
                    }
                    else
                    {
                        ShowErrorNotification($"Failed to cancel interview: {result.Error?.Message}");
                    }
                }
                else
                {
                    ShowErrorNotification($"Cannot cancel interview: {cancelResult.Error?.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error cancelling interview: {Code}", interview.Code);
            ShowErrorNotification("Error cancelling interview");
        }
    }
    #endregion

    #region UI Helpers
    private BadgeStyle GetInterviewStatusBadge(Interview interview)
    {
        return interview.Status.Value switch
        {
            "SCHEDULED" => BadgeStyle.Info,
            "IN_PROGRESS" => BadgeStyle.Warning,
            "COMPLETED" => BadgeStyle.Success,
            "CANCELLED" => BadgeStyle.Danger,
            _ => BadgeStyle.Light
        };
    }

    private string GetInterviewCardStyle(Interview interview)
    {
        if (interview.IsScheduled && interview.InterviewDate.HasValue)
        {
            return "border-left: 4px solid var(--rz-success); background-color: rgba(var(--rz-success-rgb), 0.05);";
        }
        else if (interview.IsInProgress)
        {
            return "border-left: 4px solid var(--rz-warning); background-color: rgba(var(--rz-warning-rgb), 0.05);";
        }
        else if (interview.IsCompleted)
        {
            return "border-left: 4px solid var(--rz-info); background-color: rgba(var(--rz-info-rgb), 0.05);";
        }

        return "";
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
}