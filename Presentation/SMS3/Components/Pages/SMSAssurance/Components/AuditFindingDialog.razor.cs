namespace SMS3.Components.Pages.SMSAssurance.Components;

public partial class AuditFindingDialog : ComponentBase
{
    #region Parameters
    [Parameter] public string AuditCode { get; set; } = string.Empty;
    [Parameter] public SMSAuditFinding? Finding { get; set; }
    [Parameter] public bool IsNew { get; set; } = true;
    #endregion

    #region Injected Services
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<AuditFindingDialog> Logger { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    #endregion

    #region Component State
    private bool IsSubmitting { get; set; } = false;
    private FindingViewModel ViewModel { get; set; } = new();
    #endregion

    #region Lifecycle Methods
    protected override void OnInitialized()
    {
        if (IsNew)
        {
            ViewModel = new FindingViewModel
            {
                DiscoveredDate = DateTime.Today,
                TargetResolutionDate = DateTime.Today.AddDays(30),
                Severity = "Minor",
                Category = "Process",
                Status = "Open"
            };
        }
        else if (Finding != null)
        {
            ViewModel = new FindingViewModel
            {
                Code = Finding.Code!,
                Title = Finding.Title!,
                Description = Finding.Description!,
                Severity = Finding.Severity!,
                Category = Finding.Category!,
                Status = Finding.Status!,
                DiscoveredDate = Finding.DiscoveredDate,
                TargetResolutionDate = Finding.TargetResolutionDate,
                ActualResolutionDate = Finding.ActualResolutionDate,
                ResponsiblePerson = Finding.ResponsiblePerson,
                CorrectiveAction = Finding.CorrectiveAction,
                RootCauseAnalysis = Finding.RootCauseAnalysis,
                VerificationRequired = Finding.VerificationRequired,
                VerifiedBy = Finding.VerifiedBy,
                VerificationDate = Finding.VerificationDate,
                Notes = Finding.Notes
            };
        }
    }
    #endregion

    #region Event Handlers
    private async Task OnSubmit()
    {
        try
        {
            IsSubmitting = true;
            StateHasChanged();

            if (IsNew)
            {
                await CreateFinding();
            }
            else
            {
                await UpdateFinding();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error submitting finding");
            ShowErrorNotification("Error saving finding");
        }
        finally
        {
            IsSubmitting = false;
            StateHasChanged();
        }
    }

    private void OnCancel()
    {
        DialogService.Close(null);
    }
    #endregion

    #region CRUD Operations
    private async Task CreateFinding()
    {
        if (!ValidateForm()) return;

        try
        {
            // Use the existing CreateSMSAuditFindingCommand structure that matches the command handler
            var command = new CreateSMSAuditFindingCommand(
                AuditCode,
                ViewModel.Description,  // findingDescription
                ViewModel.Severity,     // severity
                ViewModel.Category,     // findingType
                "",                     // affectedArea - could map from additional field
                "",                     // requirementReference - could map from additional field
                "",                     // evidenceDescription - could map from additional field
                "CURRENT_USER"          // createdBy
            );

            var result = await Mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification("Finding created successfully");
                DialogService.Close(result.Value);
            }
            else
            {
                ShowErrorNotification($"Failed to create finding: {result.Error?.Message}");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating finding");
            ShowErrorNotification("Error creating finding");
        }
    }

    private async Task UpdateFinding()
    {
        if (!ValidateForm()) return;

        try
        {
            // Use the existing UpdateSMSAuditFindingCommand structure
            var command = new UpdateSMSAuditFindingCommand(
                ViewModel.Code,         // findingCode
                ViewModel.Description,  // findingDescription
                ViewModel.Severity,     // severity
                ViewModel.Category,     // findingType
                "",                     // affectedArea
                "",                     // requirementReference
                "",                     // evidenceDescription
                ViewModel.RootCauseAnalysis ?? "", // rootCause
                "CURRENT_USER"          // updatedBy
            );

            var result = await Mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification("Finding updated successfully");
                DialogService.Close(result.Value);
            }
            else
            {
                ShowErrorNotification($"Failed to update finding: {result.Error?.Message}");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating finding");
            ShowErrorNotification("Error updating finding");
        }
    }
    #endregion

    #region Validation
    private bool ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(ViewModel.Title))
        {
            ShowErrorNotification("Finding title is required");
            return false;
        }

        if (string.IsNullOrWhiteSpace(ViewModel.Description))
        {
            ShowErrorNotification("Finding description is required");
            return false;
        }

        if (ViewModel.TargetResolutionDate.HasValue &&
            ViewModel.TargetResolutionDate.Value < ViewModel.DiscoveredDate)
        {
            ShowErrorNotification("Target resolution date cannot be before discovered date");
            return false;
        }

        return true;
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
}