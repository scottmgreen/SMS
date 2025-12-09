using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Domain.Enums;
using SMS_Application.Messaging.Commands;
using SMS_Application.Interfaces;
using SMS_Shared.Common;
using Radzen;

namespace SMS3.Components.Pages.SMSRiskManagement.Components;

public partial class EditInterviewDialog : ComponentBase
{
    #region Injected Services
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private ILogger<EditInterviewDialog> Logger { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    #endregion

    #region Parameters
    [Parameter] public Interview Interview { get; set; } = default!;
    #endregion

    #region State
    private bool IsSaving { get; set; } = false;
    public EditInterviewModel Model { get; set; } = new();
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

    #region Lifecycle
    protected override void OnInitialized()
    {
        if (Interview != null)
        {
            Model = new EditInterviewModel
            {
                PersonInterviewed = Interview.PersonInterviewed,
                PersonInterviewedRole = Interview.PersonInterviewedRole,
                PersonInterviewedDepartment = Interview.PersonInterviewedDepartment,
                Type = Interview.Type,
                InterviewDate = Interview.InterviewDate,
                InterviewLocation = Interview.InterviewLocation,
                PreparationNotes = Interview.PreparationNotes,
                PersonInterviewedNotes = Interview.PersonInterviewedNotes,
                InvestigatorNotes = Interview.InvestigatorNotes,
                IsConfidential = Interview.IsConfidential
            };
        }
    }
    #endregion

    #region Methods
    private async Task UpdateInterview(EditInterviewModel model)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(model.PersonInterviewed))
            {
                ShowErrorNotification("Person interviewed is required");
                return;
            }

            IsSaving = true;
            StateHasChanged();

            // Update interview properties
            Interview.PersonInterviewed = model.PersonInterviewed;
            Interview.PersonInterviewedRole = model.PersonInterviewedRole;
            Interview.PersonInterviewedDepartment = model.PersonInterviewedDepartment;
            Interview.Type = model.Type;
            Interview.InterviewLocation = model.InterviewLocation;
            Interview.PreparationNotes = model.PreparationNotes;
            Interview.PersonInterviewedNotes = model.PersonInterviewedNotes;
            Interview.InvestigatorNotes = model.InvestigatorNotes;

            // Update confidentiality
            var confidentialResult = Interview.SetConfidentiality(model.IsConfidential);
            if (confidentialResult.IsFailure)
            {
                ShowErrorNotification($"Failed to update confidentiality: {confidentialResult.Error?.Message}");
                return;
            }

            // Schedule if date/time changed
            if (model.InterviewDate.HasValue && 
                model.InterviewDate != Interview.InterviewDate &&
                Interview.Status == InterviewStatus.Planned)
            {
                var scheduleResult = Interview.ScheduleInterview(
                    model.InterviewDate.Value,
                    model.InterviewLocation ?? "TBD");
                
                if (scheduleResult.IsFailure)
                {
                    ShowErrorNotification($"Failed to reschedule interview: {scheduleResult.Error?.Message}");
                    return;
                }
            }

            // Save interview
            var updateCommand = new UpdateInterviewCommand(Interview);
            var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                Logger.LogInformation("Interview updated successfully: {Code}", Interview.Code);
                DialogService.Close(true);
            }
            else
            {
                ShowErrorNotification($"Failed to update interview: {result.Error?.Message}");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating interview");
            ShowErrorNotification("Error updating interview");
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
    public class EditInterviewModel
    {
        public string PersonInterviewed { get; set; } = "";
        public string? PersonInterviewedRole { get; set; }
        public string? PersonInterviewedDepartment { get; set; }
        public InterviewType Type { get; set; } = InterviewType.Witness;
        public DateTime? InterviewDate { get; set; }
        public string? InterviewLocation { get; set; }
        public string? PreparationNotes { get; set; }
        public string? PersonInterviewedNotes { get; set; }
        public string? InvestigatorNotes { get; set; }
        public bool IsConfidential { get; set; } = false;
    }

    public class DropdownOption
    {
        public object Value { get; set; } = default!;
        public string Text { get; set; } = "";
    }
    #endregion
}