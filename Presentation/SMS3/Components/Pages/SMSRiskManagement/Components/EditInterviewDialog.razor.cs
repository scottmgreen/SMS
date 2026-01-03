using Microsoft.AspNetCore.Components;
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
    [Inject] private ICurrentUserService CurrentUserService { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private ILogger<EditInterviewDialog> Logger { get; set; } = default!;
    [Inject] public DialogService DialogService { get; set; } = default!;
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

    private readonly List<DropdownOption> InterviewStatusOptions = new()
    {
        new() { Value = InterviewStatus.Planned, Text = "Planned" },
        new() { Value = InterviewStatus.Scheduled, Text = "Scheduled" },
        new() { Value = InterviewStatus.InProgress, Text = "In Progress" },
        new() { Value = InterviewStatus.Completed, Text = "Completed" },
        new() { Value = InterviewStatus.Cancelled, Text = "Cancelled" }
    };
    #endregion

    #region Lifecycle
    protected override void OnInitialized()
    {
        // Initialize model from Interview entity
        Model = new EditInterviewModel
        {
            PersonInterviewed = Interview.PersonInterviewed,
            PersonInterviewedRole = Interview.PersonInterviewedRole,
            PersonInterviewedDepartment = Interview.PersonInterviewedDepartment,
            PersonInterviewedNotes = Interview.PersonInterviewedNotes,
            InvestigatorNotes = Interview.InvestigatorNotes,
            Type = Interview.Type,
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

    #region Methods
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
                ShowErrorNotification("Person interviewed is required");
                return;
            }

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

            // Handle interview date changes
            if (model.InterviewDate.HasValue && model.InterviewDate != Interview.InterviewDate)
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

            // Set audit fields
            Interview.UpdatedBy = CurrentUserService.UserId;
            Interview.UpdatedDate = DateTime.UtcNow;

            // Save interview using CQRS command
            var updateCommand = new UpdateInterviewCommand(Interview);
            var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                Logger.LogInformation("Interview updated successfully: {Code} by user {UserId}", 
                    Interview.Code, CurrentUserService.UserId);
                
                ShowSuccessNotification("Interview updated successfully");
                DialogService.Close(true);
            }
            else
            {
                ShowErrorNotification($"Failed to update interview: {result.Error?.Message}");
                Logger.LogError("Failed to update interview {Code}: {Error}", 
                    Interview.Code, result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating interview {Code}", Interview.Code);
            ShowErrorNotification("Error updating interview");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

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
    public class EditInterviewModel
    {
        public string PersonInterviewed { get; set; } = "";
        public string? PersonInterviewedRole { get; set; }
        public string? PersonInterviewedDepartment { get; set; }
        public string? PersonInterviewedNotes { get; set; }
        public string? InvestigatorNotes { get; set; }
        public InterviewType Type { get; set; } = InterviewType.Witness;
        public InterviewStatus Status { get; set; } = InterviewStatus.Planned;
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

    public class DropdownOption
    {
        public object Value { get; set; } = default!;
        public string Text { get; set; } = "";
    }
    #endregion
}