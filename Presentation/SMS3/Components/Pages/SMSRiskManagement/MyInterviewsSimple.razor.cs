using SMS3.Components.Pages.SMSRiskManagement.Components;

namespace SMS3.Components.Pages.SMSRiskManagement;

public partial class MyInterviewsSimple : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ICurrentUserService CurrentUserService { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private ILogger<MyInterviewsSimple> Logger { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;

    [Inject] private AuthenticationService AuthService { get; set; } = default!;

    private bool IsLoading { get; set; } = true;
    private List<Interview> FilteredInterviews { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadMyInterviews();
    }

    private async Task LoadMyInterviews()
    {
        try
        {
            IsLoading = true;

            var query = new GetAllInterviewsQuery();
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                var currentUserId = CurrentUserService.UserId;
                FilteredInterviews = result.Value
                    .Where(i => i.SMSInvestigatorCode.Equals(currentUserId, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(i => i.InterviewDate ?? DateTime.MaxValue)
                    .ToList();

                Logger.LogInformation("Loaded {Count} interviews for user {UserId}",
                    FilteredInterviews.Count, currentUserId);
            }
            else
            {
                Logger.LogError("Failed to load interviews: {Error}", result.Error?.Message);
                FilteredInterviews = new List<Interview>();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading user interviews");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task RefreshInterviews()
    {
        await LoadMyInterviews();
    }

    private async Task StartInterview(Interview interview)
    {
        try
        {
            var confirmed = await DialogService.Confirm(
                $"Start interview with {interview.PersonInterviewed}?",
                "Start Interview");

            if (confirmed == true)
            {
                var startResult = interview.StartInterview();
                if (startResult.IsSuccess)
                {
                    var updateCommand = new UpdateInterviewCommand(interview);
                    await Mediator.SendAsync(updateCommand, CancellationToken.None);
                    await RefreshInterviews();
                    await ConductInterview(interview);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error starting interview");
        }
    }

    private async Task ConductInterview(Interview interview)
    {
        var options = new DialogOptions()
        {
            Width = "100%",
            Height = "100%",
            ShowTitle = false,
            ShowClose = false,
            CssClass = "custom-modal-dialog"
        };

        var parameters = new Dictionary<string, object>
        {
            { "Interview", interview }
        };

        await DialogService.OpenAsync<EditInterviewDialog>("", parameters, options);
        await RefreshInterviews();
    }

    private async Task EditInterview(Interview interview)
    {
        await ConductInterview(interview);
    }

    private BadgeStyle GetInterviewStatusBadge(Interview interview)
    {
        if (interview.Status.Equals(InterviewStatus.InterviewScheduled))
            return BadgeStyle.Info;
        if (interview.Status.Equals(InterviewStatus.InterviewInProgress))
            return BadgeStyle.Warning;
        if (interview.Status.Equals(InterviewStatus.InterviewComplete))
            return BadgeStyle.Success;
        if (interview.Status.Equals(InterviewStatus.UnableToConduct))
            return BadgeStyle.Danger;

        return BadgeStyle.Light;
    }
}