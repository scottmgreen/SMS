namespace SMS3.Components.Pages.SMSRiskManagement.Components;

public partial class CompleteInterviewDialog : ComponentBase
{
    #region Injected Services
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private ILogger<CompleteInterviewDialog> Logger { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    #endregion

    #region Parameters
    [Parameter] public Interview Interview { get; set; } = default!;
    #endregion

    #region State
    private bool IsSaving { get; set; } = false;
    public CompleteInterviewModel Model { get; set; } = new();
    #endregion

    #region Lifecycle
    protected override void OnInitialized()
    {
        if (Interview != null)
        {
            // Pre-populate with existing data if any
            Model = new CompleteInterviewModel
            {
                PersonInterviewedNotes = Interview.PersonInterviewedNotes,
                InvestigatorNotes = Interview.InvestigatorNotes,
                KeyFindings = Interview.KeyFindings,
                FollowUpRequired = Interview.FollowUpRequired,
                AdditionalWitnesses = Interview.AdditionalWitnesses,
                IntervieweeCooperative = true,
                InformationReliable = true,
                RequiresFollowUp = !string.IsNullOrWhiteSpace(Interview.FollowUpRequired)
            };
        }
    }
    #endregion

    #region Methods
    private async Task CompleteInterview()
    {
        await CompleteInterview(Model);
    }

    private async Task CompleteInterview(CompleteInterviewModel model)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(model.InvestigatorNotes))
            {
                ShowErrorNotification("Investigator notes are required to complete the interview");
                return;
            }

            IsSaving = true;
            StateHasChanged();

            // Complete the interview using domain method
            var completeResult = Interview.CompleteInterview(
                model.PersonInterviewedNotes,
                model.InvestigatorNotes,
                model.KeyFindings,
                model.FollowUpRequired,
                model.AdditionalWitnesses);

            if (completeResult.IsFailure)
            {
                ShowErrorNotification($"Failed to complete interview: {completeResult.Error?.Message}");
                return;
            }

            // Add assessment notes to investigator notes
            var assessmentNotes = BuildAssessmentNotes(model);
            if (!string.IsNullOrWhiteSpace(assessmentNotes))
            {
                Interview.InvestigatorNotes = $"{Interview.InvestigatorNotes}\n\n{assessmentNotes}";
            }

            // Save interview
            var updateCommand = new UpdateInterviewCommand(Interview);
            var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                Logger.LogInformation("Interview completed successfully: {Code}", Interview.Code);
                ShowSuccessNotification("Interview completed successfully");
                DialogService.Close(true);
            }
            else
            {
                ShowErrorNotification($"Failed to save completed interview: {result.Error?.Message}");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error completing interview");
            ShowErrorNotification("Error completing interview");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    private string BuildAssessmentNotes(CompleteInterviewModel model)
    {
        var notes = new List<string>();

        notes.Add("[INTERVIEW ASSESSMENT]");
        notes.Add($"- Interviewee Cooperation: {(model.IntervieweeCooperative ? "Cooperative" : "Uncooperative or resistant")}");
        notes.Add($"- Information Reliability: {(model.InformationReliable ? "Reliable and accurate" : "Questionable or inconsistent")}");
        notes.Add($"- Follow-up Required: {(model.RequiresFollowUp ? "Yes" : "No")}");
        notes.Add($"- Completed: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC");

        return string.Join("\n", notes);
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
    public class CompleteInterviewModel
    {
        public string? PersonInterviewedNotes { get; set; }
        public string? InvestigatorNotes { get; set; }
        public string? KeyFindings { get; set; }
        public string? FollowUpRequired { get; set; }
        public string? AdditionalWitnesses { get; set; }

        // Assessment flags
        public bool IntervieweeCooperative { get; set; } = true;
        public bool InformationReliable { get; set; } = true;
        public bool RequiresFollowUp { get; set; } = false;
    }
    #endregion
}