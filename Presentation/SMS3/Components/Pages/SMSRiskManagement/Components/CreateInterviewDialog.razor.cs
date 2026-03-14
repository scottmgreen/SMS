using SMS3.Components.Shared.UIHelpers;

namespace SMS3.Components.Pages.SMSRiskManagement.Components;

public partial class CreateInterviewDialog : ComponentBase
{
    #region Injected Services
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ICurrentUserService CurrentUserService { get; set; } = default!;
    [Inject] private INotificationHelper NotificationHelper { get; set; } = default!;
    [Inject] private ILogger<CreateInterviewDialog> Logger { get; set; } = default!;
    [Inject] public DialogService DialogService { get; set; } = default!;
    #endregion

    #region Parameters
    [Parameter] public string InvestigationCode { get; set; } = default!;
    [Parameter] public DateTime? PresetDateTime { get; set; }
    #endregion

    #region State
    private bool IsSaving { get; set; } = false;
    public CreateInterviewModel Model { get; set; } = new();
    #endregion

    #region Lifecycle
    protected override void OnInitialized()
    {
        // Initialize model with preset date/time if provided
        InitializeDropdownOptions();

        Model = new CreateInterviewModel
        {
            InvestigationCode = InvestigationCode == "UNKNOWN" ? "" : InvestigationCode
        };

        if (PresetDateTime.HasValue)
        {
            Model.InterviewDate = PresetDateTime.Value;
        }
    }

    private void InitializeDropdownOptions()
    {
        DepartmentOptions = SMSDepartment.GetAllValues()
                    .Select(hc => new DropdownOption(hc.Value, hc.Name))
                    .ToList();


        InterviewTypeOptions = InterviewType.GetAllValues()
            .Select(hc => new DropdownOption(hc.Value, hc.Name))
            .ToList();
    }
    #endregion

    #region Dropdown Options
    private List<DropdownOption> InterviewTypeOptions { get; set; } = new ();
    public List<DropdownOption> DepartmentOptions { get; set; } = new();

    


    public async Task OnDepartmentChanged(string? departmentValue)
    {
        Model.PersonInterviewedDepartment = departmentValue;

    }
    public async Task OnWitnessInterviewTypeChanged(string? departmentValue)
    {
        Model.InterviewTypeId = departmentValue;
        Model.Type = InterviewType.FromValue(Model.InterviewTypeId);

    }
    #endregion

    #region Methods
    private async Task CreateInterview()
    {
        await CreateInterview(Model);
    }

    private async Task CreateInterview(CreateInterviewModel model)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(model.PersonInterviewed))
            {
                await NotificationHelper.ShowErrorAsync("Person to interview is required");
                return;
            }

            if (string.IsNullOrWhiteSpace(model.InvestigationCode))
            {
                await NotificationHelper.ShowErrorAsync("Investigation code is required");
                return;
            }

            IsSaving = true;
            StateHasChanged();

            // Create interview entity with current user
            var currentUserId = CurrentUserService.UserCode;
            var interviewResult = Interview.CreateForInvestigation(
                model.InvestigationCode,
                model.PersonInterviewed,
                currentUserId,
                model.Type);

            if (interviewResult.IsFailure)
            {
                await NotificationHelper.ShowErrorAsync($"Failed to create interview: {interviewResult.Error?.Message}");
                return;
            }

            var interview = interviewResult.Value;

            // Set additional properties to support all database parameters
            interview.PersonInterviewedRole = model.PersonInterviewedRole;
            interview.PersonInterviewedDepartment = model.PersonInterviewedDepartment;
            interview.PreparationNotes = model.PreparationNotes;
            interview.QuestionsToAsk = model.QuestionsToAsk;
            interview.BackgroundInformation = model.BackgroundInformation;
            interview.IsConfidential = model.IsConfidential;

            // Schedule if date/time provided
            if (model.InterviewDate.HasValue)
            {
                var scheduleResult = interview.ScheduleInterview(
                    model.InterviewDate.Value,
                    model.InterviewLocation ?? "TBD");

                if (scheduleResult.IsFailure)
                {
                    await NotificationHelper.ShowErrorAsync($"Failed to schedule interview: {scheduleResult.Error?.Message}");
                    return;
                }
            }

            // Save interview using CQRS command
            var createCommand = new CreateInterviewCommand(interview);
            var result = await Mediator.SendAsync(createCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                Logger.LogInformation("Interview created successfully: {Code} by user {UserId}",
                    interview.Code, currentUserId);

                await NotificationHelper.ShowSuccessAsync("Interview scheduled successfully");
                DialogService.Close(true);
            }
            else
            {
                await NotificationHelper.ShowErrorAsync($"Failed to save interview: {result.Error?.Message}");
                Logger.LogError("Failed to save interview {Code}: {Error}",
                    interview.Code, result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating interview for investigation {InvestigationCode}", model.InvestigationCode);
            await NotificationHelper.ShowErrorAsync("Error creating interview");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }
    #endregion

    #region Models
    public class CreateInterviewModel
    {
        public string InvestigationCode { get; set; } = "";
        public string PersonInterviewed { get; set; } = "";
        public string? PersonInterviewedRole { get; set; }
        public string? PersonInterviewedDepartment { get; set; }

        public string? InterviewTypeId { get; set; }
        public InterviewType Type { get; set; } = InterviewType.Witness;
        public DateTime? InterviewDate { get; set; }
        public string? InterviewLocation { get; set; }
        public string? PreparationNotes { get; set; }
        public string? QuestionsToAsk { get; set; }
        public string? BackgroundInformation { get; set; }
        public bool IsConfidential { get; set; } = false;
    }

    
    #endregion
}