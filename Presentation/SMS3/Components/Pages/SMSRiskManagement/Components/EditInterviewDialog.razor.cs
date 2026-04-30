
using SMS_Application.Interfaces;
using SMS_Domain.Events.UIEvents;
using SMS_Shared.Configuration;

using SMS3.Components.Shared.UIHelpers;

namespace SMS3.Components.Pages.SMSRiskManagement.Components;

public partial class EditInterviewDialog : ComponentBase
{
    #region Injected Services
    [Inject] private IBaseMediator Mediator { get; set; } = default!;
    [Inject] private ICurrentUserService CurrentUserService { get; set; } = default!;

    [Inject] private IBaseEventBus EventBus { get; set; } = default!;
    [Inject] private ILogger<EditInterviewDialog> Logger { get; set; } = default!;
    [Inject] public DialogService DialogService { get; set; } = default!;
    #endregion

    #region Parameters
    [Parameter] public Interview Interview { get; set; } = default!;
    #endregion

    #region State
    private bool IsSaving { get; set; } = false;
    private int selectedTabIndex = 0;
    public EditInterviewModel Model { get; set; } = new();
    #endregion

    #region Workflow Properties
    private bool CanStartInterview => Model.Status.Equals(InterviewStatus.InterviewScheduled) && Model.InterviewDate.HasValue;
    private bool CanCompleteInterview => Model.Status.Equals(InterviewStatus.InterviewInProgress) && !string.IsNullOrWhiteSpace(Model.KeyFindings);
    private bool IsInterviewInProgress => Model.Status.Equals(InterviewStatus.InterviewInProgress);
    private bool IsInterviewCompleted => Model.Status.Equals(InterviewStatus.InterviewComplete);
    #endregion

    #region Dropdown Options
    private List<DropdownOption> InterviewTypeOptions { get; set; } = new();
    public List<DropdownOption> DepartmentOptions { get; set; } = new();
    private List<InterviewStatusOption> InterviewStatusOptions { get; set; } = new();
    
    private void InitializeDropdownOptions()
    {
        DepartmentOptions = SMSDepartment.GetAllValues()
                    .Select(hc => new DropdownOption(hc.Value, hc.Name))
                    .ToList();


        InterviewTypeOptions = InterviewType.GetAllValues()
            .Select(hc => new DropdownOption(hc.Value, hc.Name))
            .ToList();

        // Initialize Interview Status Options from the Domain Enum
        InterviewStatusOptions = InterviewStatus.GetAllValues()
            .Select(status => new InterviewStatusOption 
            { 
                Status = status, 
                Name = status.Name 
            })
            .ToList();
    }
   


    public async Task OnDepartmentChanged(string? departmentValue)
    {
        Model.PersonInterviewedDepartment = departmentValue;

    }
    public async Task OnWitnessInterviewTypeChanged(string? departmentValue)
    {
        Model.InterviewTypeId = departmentValue;
        Model.Type = InterviewType.FromValue(Model.InterviewTypeId ?? "");

    }
    #endregion

    #region Lifecycle
    protected override void OnInitialized()
    {
        InitializeDropdownOptions();
        // Initialize model from Interview entity
        Model = new EditInterviewModel
        {
            InterviewCode = Interview.Code,
            PersonInterviewed = Interview.PersonInterviewed,
            PersonInterviewedRole = Interview.PersonInterviewedRole,
            PersonInterviewedDepartmentId = Interview.PersonInterviewedDepartment,
            //PersonInterviewedDepartment = Interview.PersonInterviewedDepartment,
            PersonInterviewedNotes = Interview.PersonInterviewedNotes,
            InvestigatorNotes = Interview.InvestigatorNotes,
            InterviewTypeId = Interview.Type.Value,
            //Type = Interview.Type,
            Status = Interview.Status,
            InterviewDate = Interview.InterviewDate,
            DurationMinutes = Interview.DurationMinutes,
            InterviewLocation = Interview.InterviewLocation,
            IsConfidential = Interview.IsConfidential,
            PreparationNotes = Interview.PreparationNotes,
            QuestionsToAsk = Interview.QuestionsToAsk,
            BackgroundInformation = Interview.BackgroundInformation,
            KeyFindings = Interview.KeyFindings,
            FollowUpRequired = Interview.FollowUpRequired,
            AdditionalWitnesses = Interview.AdditionalWitnesses,
            CompletedDate = Interview.CompletedDate
        };
    }
    #endregion

    #region UI Helper Methods
    private string GetDialogTitle()
    {
        //if (Model.Status.Equals(InterviewStatus.Planned))
        //    return $"Plan Interview: {Model.PersonInterviewed}";
        if (Model.Status.Equals(InterviewStatus.InterviewScheduled))
            return $"Scheduled Interview: {Model.InterviewCode} - {Model.PersonInterviewed}";
        if (Model.Status.Equals(InterviewStatus.InterviewInProgress))
            return $"Conducting Interview: {Model.InterviewCode} - {Model.PersonInterviewed}";
        if (Model.Status.Equals(InterviewStatus.InterviewComplete))
            return $"Interview Complete: {Model.InterviewCode} - {Model.PersonInterviewed}";
        if (Model.Status.Equals(InterviewStatus.InterviewCanceled))
            return $"Cancelled Interview: {Model.InterviewCode} - {Model.PersonInterviewed}";

        return $"Edit Interview: {Model.PersonInterviewed}";
    }

    private string GetInterviewIcon()
    {
        if (Model.Status.Equals(InterviewStatus.InterviewScheduled))
            return "schedule";
        if (Model.Status.Equals(InterviewStatus.InterviewInProgress))
            return "record_voice_over";
        if (Model.Status.Equals(InterviewStatus.InterviewComplete))
            return "check_circle";
        if (Model.Status.Equals(InterviewStatus.InterviewCanceled))
            return "cancel";

        return "edit";
    }

    private BadgeStyle GetStatusBadgeStyle()
    {
        if (Model.Status.Equals(InterviewStatus.InterviewScheduled))
            return BadgeStyle.Primary;
        if (Model.Status.Equals(InterviewStatus.InterviewInProgress))
            return BadgeStyle.Warning;
        if (Model.Status.Equals(InterviewStatus.InterviewComplete))
            return BadgeStyle.Success;
        if (Model.Status.Equals(InterviewStatus.InterviewCanceled))
            return BadgeStyle.Danger;

        return BadgeStyle.Light;
    }

    private string GetConductingTabText()
    {
        if (Model.Status.Equals(InterviewStatus.InterviewInProgress))
            return "Conducting Interview";
        if (Model.Status.Equals(InterviewStatus.InterviewComplete))
            return "Interview Results";

        return "Conduct Interview";
    }
    #endregion

    #region Workflow Methods
    private async Task StartInterview()
    {
        try
        {
            var result = Interview.StartInterview();
            if (result.IsSuccess)
            {
                Model.Status = InterviewStatus.InterviewInProgress;
                selectedTabIndex = 2; // Switch to conducting tab
                await UpdateInterview();
                ShowSuccessAsyncNotification("Interview started successfully. You can now begin recording notes and findings.");
                StateHasChanged();
            }
            else
            {
                ShowErrorAsyncNotification($"Cannot start interview: {result.Error?.Message}");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error starting interview {Code}", Interview.Code);
            ShowErrorAsyncNotification("Error starting interview");
        }
    }

    private async Task CompleteInterview()
    {
        try
        {
            //if (string.IsNullOrWhiteSpace(Model.KeyFindings))
            //{
            //    ShowErrorAsyncNotification("Key findings are required to complete the interview");
            //    return;
            //}

            var result = Interview.CompleteInterview(
                Model.PersonInterviewedNotes,
                Model.InvestigatorNotes,
                Model.KeyFindings,
                Model.FollowUpRequired,
                Model.AdditionalWitnesses);

            if (result.IsSuccess)
            {
                Model.Status = InterviewStatus.InterviewComplete;
                Model.CompletedDate = DateTime.UtcNow;
                await UpdateInterview();
                ShowSuccessAsyncNotification("Interview completed successfully!");
                StateHasChanged();
            }
            else
            {
                ShowErrorAsyncNotification($"Cannot complete interview: {result.Error?.Message}");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error completing interview {Code}", Interview.Code);
            ShowErrorAsyncNotification("Error completing interview");
        }
    }

    private async Task CancelInterview()
    {
        try
        {
            var confirmed = await DialogService.Confirm(
                "Are you sure you want to cancel this interview? This action cannot be undone.",
                "Cancel Interview",
                new ConfirmOptions()
                {
                    OkButtonText = "Yes, Cancel Interview",
                    CancelButtonText = "No, Keep Interview"
                });

            if (confirmed == true)
            {
                var reason = "Interview cancelled by user";
                var result = Interview.CancelInterview(reason);
                if (result.IsSuccess)
                {
                    Model.Status = InterviewStatus.InterviewCanceled;
                    await UpdateInterview();
                    ShowSuccessAsyncNotification("Interview cancelled successfully");
                    StateHasChanged();
                }
                else
                {
                    ShowErrorAsyncNotification($"Cannot cancel interview: {result.Error?.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error cancelling interview {Code}", Interview.Code);
            ShowErrorAsyncNotification("Error cancelling interview");
        }
    }
    #endregion

    #region CRUD Methods
    private async Task UpdateInterview()
    {
        await UpdateInterview(Model);
    }

    private async Task UpdateInterview(EditInterviewModel model)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(model.PersonInterviewed))
            {
                ShowErrorAsyncNotification("Person interviewed is required");
                return;
            }

            // Validate interview completion requirements
            

            IsSaving = true;
            StateHasChanged();

            // Update interview entity with all Enhanced parameters
            Interview.PersonInterviewed = model.PersonInterviewed;
            Interview.PersonInterviewedRole = model.PersonInterviewedRole;
            Interview.PersonInterviewedDepartment = model.PersonInterviewedDepartment;
            Interview.PersonInterviewedNotes = model.PersonInterviewedNotes;
            Interview.InvestigatorNotes = model.InvestigatorNotes;
            Interview.Type = model.Type;
            Interview.Status = model.Status;
            Interview.DurationMinutes = model.DurationMinutes;
            Interview.InterviewLocation = model.InterviewLocation;
            Interview.IsConfidential = model.IsConfidential;
            Interview.PreparationNotes = model.PreparationNotes;
            Interview.QuestionsToAsk = model.QuestionsToAsk;
            Interview.BackgroundInformation = model.BackgroundInformation;
            Interview.KeyFindings = model.KeyFindings;
            Interview.FollowUpRequired = model.FollowUpRequired;
            Interview.AdditionalWitnesses = model.AdditionalWitnesses;
            Interview.CompletedDate = model.CompletedDate;

            // Handle interview date changes with domain logic
            if (model.InterviewDate.HasValue && model.InterviewDate != Interview.InterviewDate)
            {
                // Use the domain method for scheduling/rescheduling
                var scheduleResult = Interview.ScheduleInterview(
                    model.InterviewDate.Value,
                    model.InterviewLocation ?? "TBD",
                    model.DurationMinutes);

                if (scheduleResult.IsFailure)
                {
                    // Try update method for rescheduling
                    var updateResult = Interview.UpdateDateTime(model.InterviewDate.Value, model.DurationMinutes);

                    if (updateResult.IsFailure)
                    {
                        ShowErrorAsyncNotification($"Failed to update interview: {updateResult.Error?.Message}");
                        return;
                    }

                    // Update location separately
                    Interview.InterviewLocation = model.InterviewLocation;
                }
            }
            else
            {
                // No date change, just update the location and duration
                Interview.InterviewLocation = model.InterviewLocation;
                Interview.DurationMinutes = model.DurationMinutes;
            }

            // Set audit fields
            Interview.UpdatedBy = CurrentUserService.UserCode;
            Interview.UpdatedDate = DateTime.UtcNow;

            // Save interview using CQRS command
            var updateCommand = new UpdateInterviewCommand(Interview);
            var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                Logger.LogInformation("Interview updated successfully: {Code} by user {UserId}",
                    Interview.Code, CurrentUserService.UserCode);

                ShowSuccessAsyncNotification("Interview updated successfully");

                // Close the dialog and return true to indicate success
                // This will trigger the calendar to refresh
                DialogService.Close(true);
            }
            else
            {
                ShowErrorAsyncNotification($"Failed to update interview: {result.Error?.Message}");
                Logger.LogError("Failed to update interview {Code}: {Error}",
                    Interview.Code, result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating interview {Code}", Interview.Code);
            ShowErrorAsyncNotification("Error updating interview");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }
    #endregion

    #region Notification Methods (EventBus-Driven)
    private async Task ShowSuccessAsyncNotification(string message)
    {
        await EventBus.PublishUIEventAsync(UINotificationEvent.Success("Success", message));
    }

    private async Task ShowErrorAsyncNotification(string message)
    {
        await EventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", message));
    }
    #endregion

    #region Models
    public class EditInterviewModel
    {
        public string InterviewCode { get; set; }
        public string PersonInterviewed { get; set; } = "";
        public string? PersonInterviewedRole { get; set; }
        public string? PersonInterviewedDepartmentId { get; set; }
        public string? PersonInterviewedDepartment { get; set; }
        public string? PersonInterviewedNotes { get; set; }
        public string? InvestigatorNotes { get; set; }

        public string? InterviewTypeId { get; set; }
        public InterviewType Type { get; set; } = InterviewType.Witness;
        public InterviewStatus Status { get; set; } = InterviewStatus.InterviewScheduled;
        public DateTime? InterviewDate { get; set; }
        public int? DurationMinutes { get; set; }
        public string? InterviewLocation { get; set; }
        public bool IsConfidential { get; set; } = false;
        public string? PreparationNotes { get; set; }
        public string? QuestionsToAsk { get; set; }
        public string? BackgroundInformation { get; set; }
        public string? KeyFindings { get; set; }
        public string? FollowUpRequired { get; set; }
        public string? AdditionalWitnesses { get; set; }
        public DateTime? CompletedDate { get; set; }
    }

    public class InterviewStatusOption
    {
        public InterviewStatus Status { get; set; } = default!;
        public string Name { get; set; } = "";
    }

    
    #endregion
}