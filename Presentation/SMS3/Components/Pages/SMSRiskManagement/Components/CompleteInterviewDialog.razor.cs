using SMS3.Components.Shared.UIHelpers;

namespace SMS3.Components.Pages.SMSRiskManagement.Components;

public partial class CompleteInterviewDialog : ComponentBase
{
    #region Injected Services
    [Inject] private IMediator _mediator { get; set; } = default!;
    [Inject] private INotificationHelper _notificationHelper { get; set; } = default!;
    [Inject] private ILogger<CompleteInterviewDialog> _logger { get; set; } = default!;
    [Inject] private DialogService _dialogService { get; set; } = default!;
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
                await _notificationHelper.ShowErrorAsync("Investigator notes are required to complete the interview");
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
                await _notificationHelper.ShowErrorAsync($"Failed to complete interview: {completeResult.Error?.Message}");
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
            var result = await _mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Interview completed successfully: {Code}", Interview.Code);
                await _notificationHelper.ShowSuccessAsync("Interview completed successfully");
                _dialogService.Close(true);
            }
            else
            {
                await _notificationHelper.ShowErrorAsync($"Failed to save completed interview: {result.Error?.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing interview");
            await _notificationHelper.ShowErrorAsync("Error completing interview");
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
    #endregion

    #region Models
    public class CompleteInterviewModel
    {
        public int Id { get; set; }
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