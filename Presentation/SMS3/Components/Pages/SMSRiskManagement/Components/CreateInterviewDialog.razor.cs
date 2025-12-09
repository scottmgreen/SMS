using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Domain.Enums;
using SMS_Application.Messaging.Commands;
using SMS_Application.Interfaces;
using SMS_Shared.Common;
using Radzen;

namespace SMS3.Components.Pages.SMSRiskManagement.Components;

public partial class CreateInterviewDialog : ComponentBase
{
    #region Injected Services
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private ILogger<CreateInterviewDialog> Logger { get; set; } = default!;
    [CascadingParameter] public DialogService DialogService { get; set; } = default!;
    #endregion

    #region Parameters
    [Parameter] public string InvestigationCode { get; set; } = default!;
    #endregion

    #region State
    private bool IsSaving { get; set; } = false;
    public CreateInterviewModel Model { get; set; } = new();
    #endregion

    #region Dropdown Options
    private readonly List<DropdownOption> InterviewTypeOptions = new()
    {
        new() { Value = InterviewType.Witness, Text = "Witness Interview" },
        new() { Value = InterviewType.Expert, Text = "Subject Matter Expert" },
        new() { Value = InterviewType.Stakeholder, Text = "Stakeholder Interview" },
        new() { Value = InterviewType.FollowUp, Text = "Follow-up Interview" }
    };
    #endregion

    #region Methods
    private async Task CreateInterview(CreateInterviewModel model)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(model.PersonInterviewed))
            {
                ShowErrorNotification("Person to interview is required");
                return;
            }

            IsSaving = true;
            StateHasChanged();

            // Create interview entity
            var interviewResult = Interview.CreateForInvestigation(
                InvestigationCode,
                model.PersonInterviewed,
                "CURRENT_USER", // TODO: Get actual current user
                model.Type);

            if (interviewResult.IsFailure)
            {
                ShowErrorNotification($"Failed to create interview: {interviewResult.Error?.Message}");
                return;
            }

            var interview = interviewResult.Value;
            
            // Set additional properties
            interview.PersonInterviewedRole = model.PersonInterviewedRole;
            interview.PersonInterviewedDepartment = model.PersonInterviewedDepartment;
            interview.PreparationNotes = model.PreparationNotes;
            interview.IsConfidential = model.IsConfidential;

            // Schedule if date/time provided
            if (model.InterviewDate.HasValue)
            {
                var scheduleResult = interview.ScheduleInterview(
                    model.InterviewDate.Value,
                    model.InterviewLocation ?? "TBD");
                
                if (scheduleResult.IsFailure)
                {
                    ShowErrorNotification($"Failed to schedule interview: {scheduleResult.Error?.Message}");
                    return;
                }
            }

            // Save interview
            var createCommand = new CreateInterviewCommand(interview);
            var result = await Mediator.SendAsync(createCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                Logger.LogInformation("Interview created successfully: {Code}", interview.Code);
                DialogService.Close(true);
            }
            else
            {
                ShowErrorNotification($"Failed to save interview: {result.Error?.Message}");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating interview");
            ShowErrorNotification("Error creating interview");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
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
    public class CreateInterviewModel
    {
        public string PersonInterviewed { get; set; } = "";
        public string? PersonInterviewedRole { get; set; }
        public string? PersonInterviewedDepartment { get; set; }
        public InterviewType Type { get; set; } = InterviewType.Witness;
        public DateTime? InterviewDate { get; set; }
        public string? InterviewLocation { get; set; }
        public string? PreparationNotes { get; set; }
        public bool IsConfidential { get; set; } = false;
    }

    public class DropdownOption
    {
        public object Value { get; set; } = default!;
        public string Text { get; set; } = "";
    }
    #endregion
}