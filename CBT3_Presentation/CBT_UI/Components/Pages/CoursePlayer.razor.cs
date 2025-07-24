using Blazored.Video.Support;

using CBT_UI.Components.Pages.Shared;

using CBT3_Application.Interfaces;
using CBT3_Application.Messaging;
using CBT3_Application.Messaging.Commands;
using CBT3_Application.Messaging.Queries;
using CBT3_Application.Services;

using CBT3_Domain.Common;
using CBT3_Domain.Entities;
using CBT3_Domain.Enums;
using CBT3_Domain.Errors;
using CBT3_Domain.Events.DomainEvents;
using CBT3_Domain.Events.SystemEvents;
using CBT3_Domain.Interfaces;

using CBT3_UI.Components.Shared;

using Microsoft.AspNetCore.Components;

using Radzen;




namespace CBT_UI.Components.Pages;

public partial class CoursePlayer :ComponentBase
{
    [Inject]
    protected SystemService _systemService { get; set; }
    

    
    private Course _course = null;
    private Trainee _trainee = null;
    private MarkupString _lessonPageText;
    private bool? _fileExists;
    private string _videoFileName = string.Empty;
    public string _imageFileName = string.Empty;
    private string _currentEvent = string.Empty;
    private MachineState _currentMachineState;
    
    public string _lessonPage = "display:none";
    public string _continueButton = "display:none";
    public string _customerserviceButton= "display:none";
    public string _startButton = "";
    public string _spinner = "display:none";
    public string _exitsystem_exitcourseButtons = "display:none";


    // Declare fields to hold delegate instances
    private readonly Action<CourseStateEvent> _courseStateHandler;
    private readonly Action<LessonPageEvent> _lessonPageHandler;
    private readonly Action<MachinePauseEvent> _machinePauseHandler;
    private readonly Action<LessonQuizStartedEvent> _quizStartedHandler;
    private readonly Action<LessonQuizFinishedEvent> _quizFinishedHandler;

    private void ShowSpinner() { _spinner = ""; }
    private void HideSpinner() { _spinner = "display:none"; }
    private void ShowStartButton() { _startButton = ""; }
    private void HideStartButton() { _startButton = "display:none"; }
    private void ShowContinueButton() { _continueButton = ""; }
    private void HideContinueButton() { _continueButton = "display:none"; }
    private void ShowCustomerServiceContinueButton() { _customerserviceButton = ""; }
    private void HideCustomerServiceContinueButton() { _customerserviceButton = "display:none"; }
    private void ShowExitSystemExitCourseButtons() { _exitsystem_exitcourseButtons = ""; }
    private void HideExitSystemExitCourseButtons() { _exitsystem_exitcourseButtons = "display:none"; }
    private void ShowLessonPageText() { _lessonPage = "display:inline"; }
    private void HideLessonPageText() { _lessonPage = "display:none"; }
    private void SetLessonPageText(string lessonpagetext)
    {
        _lessonPageText = (MarkupString)$"<h2><strong>{lessonpagetext}</strong></h2>";
    }


    [Parameter]
    public string TraineeId { get; set; }
    private string _traineeId;
    [Parameter]
    public string CourseId { get; set; }
    private string _courseId;

    public CoursePlayer()
    {
        _courseStateHandler = HandleCourseStateEvent;
        _lessonPageHandler = HandleLessonPageEvent;
        _machinePauseHandler = HandleMachinePauseEvent;
        _quizStartedHandler = HandleLessonQuizStartedEvent;
        _quizFinishedHandler = HandleLessonQuizFinishedEvent;
    }
    private async Task InitializeMachine()
    {
        
        //SubscribeToEvents();
        _messenger.Subscribe(_machinePauseHandler);
        _messenger.Subscribe(_courseStateHandler);
        _messenger.Subscribe(_lessonPageHandler);
        _messenger.Subscribe(_quizStartedHandler);
        _messenger.Subscribe(_quizFinishedHandler);

        TraineeID traineeId = new TraineeID(_traineeId);
        GetTraineeQuery traineequery = new GetTraineeQuery(traineeId);
        Result<Trainee> traineeresult = await _mediator.SendAsync(traineequery, default);
        if (traineeresult.IsSuccess)
        {
            _trainee = traineeresult.Value;
            AppState.SetProperty(this, "Trainee", _trainee);
        }



        CourseID courseId = new(_courseId);
        GetCourseQuery courseQuery = new(courseId);
        Result<Course> courseresult = await _mediator.SendAsync(courseQuery, default);

        if (courseresult.IsSuccess)
        {
            _course = courseresult.Value;
            AppState.SetProperty(this, "Course", _course);
        }

        _courseMachine.InitializeMachine(_trainee, _course);

    }

    protected override async Task OnInitializedAsync()
    {
        _courseId = CourseId;
        _traineeId = TraineeId;
        
        await base.OnInitializedAsync();
    }


    public async Task<Result<bool>> ShowVideo(string filename)
    {
        string fileName = filename;

        DialogOptions dialogOptions = new DialogOptions()
        {
            Width = "1300px",
            Height = "1000px",
            Resizable = false,
            Draggable = false,
            ShowClose = true,
            ShowTitle = false,
            CssClass = "radzen-dialog"
        };

        Dictionary<string, object> dialogParams = new Dictionary<string, object> { { "FileName", fileName } };

        bool dialog = await Task.Run(() => _dialogService.OpenAsync<VideoComponent>($"Video",dialogParams, dialogOptions ));

        return dialog
            ? Result.Success(dialog)
            : Result.Failure<bool>(DomainErrors.SystemError.VideoPlaybackError);
    }
    public async Task<Result<bool>> ShowQuiz(Trainee trainee, CourseID courseid, LessonQuiz quiz)
    {
        DialogOptions dialogOptions = new DialogOptions() 
        { 
            Width = "1400px", 
            Height = "1100px", 
            Resizable = false, 
            Draggable = false, 
            ShowClose = true, 
            ShowTitle = false, 
            CssClass = "radzen-dialog" 
        };

        Dictionary<string, object> dialogParams = new Dictionary<string, object>() { { "LessonQuiz", quiz }, { "Trainee", trainee }, { "CourseID", courseid } };

        bool dialog = await _dialogService.OpenAsync<LessonQuizComponent>($"Quiz", dialogParams, dialogOptions);

        //if (dialog)
        //{
        //    ContinueClick();
        //}
        return dialog
            ? Result.Success(dialog)
            : Result.Failure<bool>(DomainErrors.SystemError.LessonQuizError);

    }
    public async Task<Result<bool>> ShowCustom(PageType pageType)
    {
        var parameters = new Dictionary<string, object>();

        DialogOptions customDialogOptions = new DialogOptions() 
        { 
            Width = "1300px", 
            Height = "1000px", 
            Resizable = false, 
            Draggable = false, 
            ShowClose = true, 
            ShowTitle = false, 
            CssClass = "radzen-dialog" 
        };
        bool dialog = false;
        if (pageType == PageType.PT_CUSTOM_01)
        {
            dialog = await Task.Run(() => _dialogService.OpenAsync<PT_CUSTOM_01>($"Custom_01", null, customDialogOptions));
        }
        if (pageType == PageType.PT_CUSTOM_02)
        {
            dialog = await Task.Run(() => _dialogService.OpenAsync<PT_CUSTOM_02>($"Custom_02", null, customDialogOptions));
        }
        if (pageType == PageType.PT_CUSTOM_03)
        {
            dialog = await Task.Run(() => _dialogService.OpenAsync<PT_CUSTOM_03>($"Custom_03", null, customDialogOptions));
        }
        if (pageType == PageType.PT_CUSTOM_04)
        {
            dialog = await Task.Run(() => _dialogService.OpenAsync<PT_CUSTOM_04>($"Custom_04", null, customDialogOptions));
        }
        if (pageType == PageType.PT_CUSTOM_05)
        {
            dialog = await Task.Run(() => _dialogService.OpenAsync<PT_CUSTOM_05>($"Custom_05", null, customDialogOptions));
        }
        if (pageType == PageType.PT_CUSTOM_06)
        {
            dialog = await Task.Run(() => _dialogService.OpenAsync<PT_CUSTOM_06>($"Custom_06", null, customDialogOptions));
        }
        if (pageType == PageType.PT_CUSTOM_07)
        {
            dialog = await Task.Run(() => _dialogService.OpenAsync<PT_CUSTOM_07>($"Custom_07", null, customDialogOptions));
        }

        return dialog
            ? Result.Success(dialog)
            : Result.Failure<bool>(DomainErrors.SystemError.CustomPageError);
    }
    
    private async Task CourseMachineStart()
    {
        ShowSpinner();
        HideStartButton();
        await Task.Run(() => InitializeMachine());
        
        MachineStartCommand request = new MachineStartCommand(_courseMachine);
        _ = await _mediator.SendAsync(request, default);
        
        StateHasChanged();
    }
    private async void ContinueClick()
    {
              
        if (_currentMachineState.Equals(MachineState.CourseMachinePaused))
        {
            CourseMachineResume();
            return;
        }
        if (_currentMachineState.Equals(MachineState.LessonMachinePaused))
        {
            LessonMachineResume();
            return;
        }
        if (_currentMachineState.Equals(MachineState.LessonPageMachinePaused))
        {
            LessonPageMachineResume();
            return;
        }


    }
    private async void CustomerServiceClick()
    {
        HideCustomerServiceContinueButton();
        _videoFileName = $"./video/customer_service.mp4";
        StateHasChanged();
        _ = await ShowVideo(_videoFileName);

        SetLessonPageText(_cbtApp.LessonPage.PageText);
        ShowLessonPageText();
        ShowExitSystemExitCourseButtons();
        StateHasChanged();
        AppState.SetProperty(this, "Course", null);
        AppState.SetProperty(this, "LessonPage", null);
        _messenger.Unsubscribe(_quizStartedHandler);
    }
    
    private void NavigateToCourseSelection()
    {
        _navManager.NavigateTo($"/courseselection/{_cbtApp.Trainee.Id.Value}");
    }
    private void NavigateToSplash()
    {
        TrainingSession session = _cbtApp.TrainingSession;
        session.CourseEndedAt = DateTime.Now;

        FinishTrainingSessionCommand cmd = new FinishTrainingSessionCommand(session);
        _ = _mediator.SendAsync(cmd, default).Result;
        ContinueClick();
        
    }
    

    private void HandleCourseStateEvent(CourseStateEvent @event)
    {
        //Console.WriteLine($"=>{@event.Text} ");
        //If the Course State Event is "CourseFinished", this requires 
        //Using the CQRS / MEDIATOR bus to send a CourseCompletionCommand
        //This command will fire off the _= await _dataService.CompleteCourseAsync(trainee, course, coursepass);
        //Stored proc to record the results and writeout the xml etc.


        HideSpinner();
        _cbtApp.CoursePass = @event.CoursePass;
        if (@event.State.Equals(CourseState.CourseFinished))
        {
            
            if (_cbtApp.CoursePass)
            {
                SetLessonPageText("Course Finished");
                ShowLessonPageText();
                HideContinueButton();
                ShowExitSystemExitCourseButtons();
                StateHasChanged();
                _navManager.NavigateTo("/");
                return;
            }
            
            else if (!_cbtApp.CoursePass)
            {
                SetLessonPageText("Please return to the Front Counter for further instructions");
                ShowLessonPageText();
                HideContinueButton();
                HideExitSystemExitCourseButtons();
                AppState.SetProperty(this, "Trainee", null);
                AppState.SetProperty(this, "Course", null);
                AppState.SetProperty(this, "Session", null);
                AppState.SetProperty(this, "LessonPage", null);
                _messenger.Unsubscribe(_courseStateHandler);
                _messenger.Unsubscribe(_lessonPageHandler);
                _messenger.Unsubscribe(_quizStartedHandler);
                _messenger.Unsubscribe(_machinePauseHandler);
                StateHasChanged();
                return;
            }

            

        }
    }
  
    private async void HandleLessonPageEvent(LessonPageEvent @event)
    {
        //This determines what page strategy to use//Text/Video//Custom Page//Quiz//


        void ShowLessonPageTextAndShowContinueButton()
        {
            ShowLessonPageText();
            ShowContinueButton();
        }
        void ShowLessonPageTextAndHideContinueButton()
        {
            ShowLessonPageText();
            HideContinueButton();
        }

        void FinalizePage()
        {
            StateHasChanged();
            return;
        }

        // Handle Course Pass logic
        async Task HandleCoursePass(LessonPage lessonPage)
        {
            TraineeID traineeId = new(_cbtApp.Trainee.Id.Value);
            CourseID courseId = new(_cbtApp.Course.Id.Value);
            TrainingSession session = _cbtApp.TrainingSession;
            _cbtApp.CoursePass = true;

            CourseCompletionCommand request = new CourseCompletionCommand(session, traineeId, courseId, _cbtApp.CoursePass);
            var result = await _mediator.SendAsync<Result<bool>>(request, default);

            if (_cbtApp.Course.CustomerServiceRequirement)
            {
                await HandleCustomerServiceMessage();
            }
            else
            {
                SetLessonPageText(lessonPage.PageText);
                ShowLessonPageTextAndHideContinueButton();
                ShowExitSystemExitCourseButtons();

                AppState.SetProperty(this, "Course", null);
                AppState.SetProperty(this, "LessonPage", null);
                _messenger.Unsubscribe(_quizStartedHandler);

                FinalizePage();
            }
            
            
        }

        async Task HandleCustomerServiceMessage()
        {
            LessonPageID customerservice_lessonPageID = new LessonPageID(Guid.NewGuid().ToString());
            LessonPage customerservice_lessonpage = new(customerservice_lessonPageID);
            customerservice_lessonpage.PageText = "Please watch this important Customer Service Message";
            customerservice_lessonpage.VideoURL = "customer_service.mp4";
            SetLessonPageText(customerservice_lessonpage.PageText);
            ShowLessonPageText();
            HideContinueButton();
            ShowCustomerServiceContinueButton();
            StateHasChanged();
            
        }
        async Task HandleCustomerServiceVideo(LessonPage lessonpage)
        {
            SetLessonPageText("");
            HideLessonPageText();
            HideContinueButton();
            StateHasChanged();
            _videoFileName = $"./video/{lessonpage.VideoURL}";
            _ = await ShowVideo(_videoFileName);
        }

        // Handle Course Fail logic
        async Task HandleCourseFail(LessonPage lessonPage)
        {
            TraineeID traineeId = new(_cbtApp.Trainee.Id.Value);
            CourseID courseId = new(_cbtApp.Course.Id.Value);
            TrainingSession session = _cbtApp.TrainingSession;
            _cbtApp.CoursePass = false;

            CourseCompletionCommand request = new CourseCompletionCommand(session, traineeId, courseId, _cbtApp.CoursePass);
            var result = await _mediator.SendAsync<Result<bool>>(request, default);

            _cbtApp.TrainingSession.CourseEndedAt = DateTime.Now;

            FinishTrainingSessionCommand cmd = new(_cbtApp.TrainingSession);
            await _mediator.SendAsync(cmd, default);

            SetLessonPageText(lessonPage.PageText);
            ShowLessonPageTextAndShowContinueButton();
            HideExitSystemExitCourseButtons();

            FinalizePage();
        }
        // Handle Custom Page logic
        async Task HandleCustomPage(PageType pt, LessonPage lessonPage)
        {
            SetLessonPageText(lessonPage.PageText);
            ShowLessonPageTextAndHideContinueButton();
            HideExitSystemExitCourseButtons();
            StateHasChanged();
            await ShowCustom(pt);
            ContinueClick();
        }


        async Task HandleVideoPage(LessonPage lessonPage)
        {
            if (lessonPage.VideoURL != null)
            {
                _videoFileName = $"./video/{lessonPage.VideoURL}";

                if (_systemService.IsFileExists(_videoFileName).Result.IsSuccess)
                {
                    SetLessonPageText("");
                    HideLessonPageText();
                    HideContinueButton();
                    StateHasChanged();
                    _ = await ShowVideo(_videoFileName);
                    ContinueClick();
                    return;

                }
                else
                {
                    SetLessonPageText($"Video Missing => {lessonPage.VideoURL}");
                    ShowLessonPageTextAndShowContinueButton();
                    FinalizePage();
                }
            }
            else
            {
                SetLessonPageText($"Video Missing => Check DB ");
                ShowLessonPageTextAndShowContinueButton();
                FinalizePage();
            }
        }

        async Task HandleBasePage (LessonPage lessonPage)
        {
            SetLessonPageText(lessonPage.PageText);
            ShowLessonPageTextAndShowContinueButton();
            FinalizePage();
        }

        PageType pt = @event.Page.LessonPageType;
        LessonPage lessonPage = @event.Page;
        AppState.SetProperty(this, "LessonPage", lessonPage);
        _imageFileName = string.Empty;


        if (pt.Equals(PageType.PT_BASE_01)) //COURSE_INTRO
        {
            await HandleBasePage(lessonPage);
        }
        else if (pt.Equals(PageType.PT_BASE_02)) //LESSON_OVERVIEW
        {
            await HandleBasePage(lessonPage);
        }
        else if (pt.Equals(PageType.PT_BASE_03)) //KNOWLEDGE_CHECK
        {
            await HandleBasePage(lessonPage);
        }
        else if (pt.Equals(PageType.PT_BASE_04)) //COURSE_END
        {
            SetLessonPageText("Course Completed");
            ShowLessonPageTextAndShowContinueButton();
            HideExitSystemExitCourseButtons();
            FinalizePage();
        }
        else if (pt.Equals(PageType.PT_BASE_05)) //VIDEO
        {
            await HandleVideoPage(lessonPage);
        }
        else if (pt.Equals(PageType.PT_BASE_06)) //QUIZ
        {
            SetLessonPageText(lessonPage.PageText);
            ShowLessonPageTextAndHideContinueButton();
            FinalizePage();
        }


        else if (pt.Equals(PageType.PT_BASE_07)) //COURSE_PASS
        {
            await HandleCoursePass(lessonPage);
        }
        else if (pt.Equals(PageType.PT_BASE_08)) //COURSE_FAIL
        {
            await HandleCourseFail(lessonPage);
        }

        else if (pt.PageSubType.Equals("CUSTOM"))
            {
                if (pt.Equals(PageType.PT_CUSTOM_04))
                { 
                _imageFileName = $"./images/{ lessonPage.ImageURL}.jpg";
                }
            await HandleCustomPage(pt, lessonPage);
        }
        
        
        
       
    }

    private void HandleMachinePauseEvent(MachinePauseEvent @event)
    {
        _currentMachineState = @event.State;
    }

    private void HandleLessonQuizFinishedEvent(LessonQuizFinishedEvent @event)
    {
        _courseMachine.ChildMachine.LessonFail = @event.IsFail;
    }

    private async void HandleLessonQuizStartedEvent(LessonQuizStartedEvent @event)
    {
        //Quiz Started State Raises the QuizState.QuizStarted event
        //The client needs to subscribe to  Events"
        _messenger.Subscribe(_quizFinishedHandler);

        SetLessonPageText("");
        HideLessonPageText();
        HideContinueButton();
        StateHasChanged();
        LessonQuiz quiz = @event.LessonQuiz;
        Trainee trainee = _cbtApp.Trainee;
        CourseID courseid = new (_courseId);

        Result<bool> result = await ShowQuiz(trainee, courseid, quiz);

        if (result.IsSuccess && result.Value)
        {
            ContinueClick(); // react to the dialog result
        }
        else if (result.IsFailure)
        {
            // Handle the error, e.g., show message/log
            Console.Error.WriteLine($"Error: {result.Error}");
        }


    }

    private void CourseMachineResume()
    {
        var request = new MachineResumeCommand(_courseMachine)
        {
            Machine = _courseMachine
        };
        _ = _mediator.SendAsync(request,default).Result;

        InvokeAsync(StateHasChanged);
    }
    private void LessonMachineResume()
    {
        var request = new MachineResumeCommand(_courseMachine)
        {
            Machine = _courseMachine.ChildMachine

        };
        _ = _mediator.SendAsync(request,default).Result;

        InvokeAsync(StateHasChanged);
    }
    private void LessonPageMachineResume()
    {
        var request = new MachineResumeCommand(_courseMachine)
        {
            Machine = _courseMachine.ChildMachine.ChildMachine

        };
        _ = _mediator.SendAsync(request,default).Result;

        InvokeAsync(StateHasChanged);
    }

    void OnEnd(VideoState state)
    {
        _dialogService.Close(false); _dialogService.Close(false);
    }

    public void Dispose()
    {
        //UnsubscribeToEvents();
    }
}
