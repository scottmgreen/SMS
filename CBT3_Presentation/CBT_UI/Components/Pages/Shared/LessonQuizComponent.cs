using System.Net.NetworkInformation;

using CBT_UI.Components.Pages.Shared;

using CBT3_Application.Interfaces;
using CBT3_Application.Messaging;
using CBT3_Application.Services;

using CBT3_Domain.Common;
using CBT3_Domain.Entities;
using CBT3_Domain.Enums;
using CBT3_Domain.Errors;
using CBT3_Domain.Events.DomainEvents;
using CBT3_Domain.Interfaces;

using Microsoft.AspNetCore.Components;

using Radzen;

namespace CBT3_UI.Components.Shared
{
    public partial class LessonQuizComponent : ComponentBase , IDisposable
    {
        [Inject]
        IMediator _mediator { get; set; }   

        [Inject]
        IMessenger _messenger { get; set; } 

        [Inject]
        SystemService _systemService { get; set; }

        [Inject]
        DialogService _dialogService { get; set; }

        [Inject]
        LessonQuizService _lessonQuizService { get; set; }

        [Parameter]
        public LessonQuiz LessonQuiz { get; set; }

        [Parameter]
        public Trainee Trainee { get; set; }

        [Parameter]
        public CourseID CourseID { get; set; }


        private Answer _parentAnswer;
        public int Score = 0;
        private Question _currentQuestion;
        private bool _isQuizFailure;

        private int _questionAttemptCount = 0;
        //private int _questionNumber = 0;

        protected private LessonQuiz _lessonquiz;
        protected private Trainee _trainee;
        protected private CourseID _courseID;
        protected private Lesson _lesson;
        protected private string _currentPoolId;
        protected private string _quizStyle;


        protected override async Task OnInitializedAsync()
        {
            
            try
            {
                Score = 0;
                
               // _questionAttemptCount ++;
                _lessonquiz = LessonQuiz;
                _trainee = Trainee;
                _courseID = CourseID;
                               
                if (_lessonquiz != null)
                {
                    _lessonQuizService.InitializeQuiz(_trainee, _courseID,_lessonquiz);
                    await LoadQuestion();
                    
                    _currentQuestion = _lessonQuizService.CurrentQuestion;
                    _currentPoolId = _currentQuestion.QuestionPoolID.Value; 
                }
                else
                {
                    // Handle the case where no suitable lesson is found
                    // For example, display an error message or redirect the user
                    // You can also set default values for _lessonquiz, _questionPools, _questions if needed
                }


            }
            catch (Exception ex)
            {
                // Handle exceptions
                // For example, log the exception or display an error message
            }

            await base.OnInitializedAsync();
        }

        private async Task LoadQuestion()
        {
            _currentQuestion = _lessonQuizService.GetNextQuestion(true);
            _currentPoolId = _currentQuestion.QuestionPoolID.Value;
            await InvokeAsync(StateHasChanged); // Ensure the UI updates with the new question
        }

        public async Task SubmitAnswer(Answer selectedAnswer)
        {
            try
            {
                AlertOptions ao = new AlertOptions();
                ao.ShowClose = false;
                ao.OkButtonText = "Ok";

                bool isCorrect = selectedAnswer.IsCorrect ?? false;

                SubmitAnswerCommand cmd = new(selectedAnswer);
                var result = await _mediator.SendAsync(cmd, default);
                _currentQuestion = result?.Value; ;

                _questionAttemptCount = (selectedAnswer.IsCorrect ?? false) ? 0 : _questionAttemptCount + 1;
                await InvokeAsync(StateHasChanged);

                Score = _lessonQuizService.Score;
                _isQuizFailure = _lessonQuizService.IsQuizFail();

                if (!isCorrect && _currentQuestion is not null) // Show the playback..
                {
                    _quizStyle = "display:none";

                    var jumpback = _lessonQuizService.GetPlaybackQuestionPool()?.Value;
                    if (jumpback != null && !string.IsNullOrEmpty(jumpback.JumpBackFilename))
                    {
                        await _dialogService.Alert("", "Your answer was incorrect. Let's Review",ao).ConfigureAwait(false);
                        var basePath = "./video/";
                        var filename = jumpback.JumpBackFilename;
                        bool fileExists = _systemService.IsFileExists(basePath + filename).Result.IsSuccess;
                        if (fileExists)
                        {
                            string fullfilename = $"{basePath}{filename}#t={jumpback.JumpBackStart},{jumpback.JumpBackEnd}";
                            await ShowVideo(fullfilename);
                        }
                        else
                        {
                            await _dialogService.Alert("", "Jumpback Video Missing !",ao).ConfigureAwait(false);
                        }

                        
                    }
                }

                _quizStyle = "";

                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SubmitAnswer: {ex.Message}");
            }
        }
        public async Task SubmitAnswers(List<Answer> providedAnswers)
        {
            try
            {
                AlertOptions ao = new AlertOptions();
                ao.ShowClose = false;
                ao.OkButtonText = "Ok";

                var selectedAnswers = providedAnswers.Where(a => a.IsSelected).ToList();
                if (!selectedAnswers.Any())
                {
                    MarkupString alertmessage = (MarkupString)"";
                    await _dialogService.Alert("", "Please select an answer before submitting", ao);
                    return;
                }
                bool isCorrect = selectedAnswers.All(a => a.IsCorrect ?? false);
                _questionAttemptCount = isCorrect ? 0 : _questionAttemptCount + 1;
               
                await InvokeAsync(StateHasChanged);


                SubmitAnswersCommand cmd = new(selectedAnswers);
                var result = await _mediator.SendAsync(cmd, default);
                _currentQuestion = result?.Value; ;

                Score = _lessonQuizService.Score;
                _isQuizFailure = _lessonQuizService.IsQuizFail();

                if (!isCorrect && _currentQuestion is not null) // Show the playback..
                {
                    _quizStyle = "display:none";

                    var jumpback = _lessonQuizService.GetPlaybackQuestionPool()?.Value;
                    if (jumpback != null && !string.IsNullOrEmpty(jumpback.JumpBackFilename))
                    {
                        await _dialogService.Alert("", "Your answer(s) selected were incorrect. Let's Review",ao).ConfigureAwait(false);
                        var basePath = "./video/";
                        var filename = jumpback.JumpBackFilename;
                        bool fileExists = _systemService.IsFileExists(basePath + filename).Result.IsSuccess;
                        if (fileExists)
                        {
                            string fullfilename = $"{basePath}{filename}#t={jumpback.JumpBackStart},{jumpback.JumpBackEnd}";
                            await ShowVideo(fullfilename);
                        }
                        else
                        {
                            await _dialogService.Alert("", "Jumpback Video Missing !", ao).ConfigureAwait(false);
                        }

                    }
                }

                _quizStyle = "";
                
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SubmitAnswers: {ex.Message}");
            }
        }

        //private bool CheckFileExists(string filePath)
        //{
        //    bool fileExists = _systemService.IsFileExists(filePath).Result.IsSuccess;
        //    return fileExists;
        //}
        

        public async Task<Result<bool>> ShowVideo(string filename)
        {
            string fileName = filename;

            bool dialog = await Task.Run(() => _dialogService.OpenAsync<VideoComponent>($"Video",
                new Dictionary<string, object>() { { "FileName", fileName } },
                new DialogOptions() { Width = "1300px", Height = "1000px", Resizable = false, Draggable = false, ShowClose = true, ShowTitle = false, CssClass = "radzen-dialog" }));


            if (dialog)
            {
                return Result.Success<bool>(dialog);
            }
            else
            {
                return Result.Failure<bool>(DomainErrors.SystemError.VideoPlaybackError);
            }
        }

        protected void Finish()
        {
            _dialogService.Close(true);
        }

        protected void RestartQuiz()
        {
            Score = 0;
        }

        public void ChildFiredEvent(Answer answer)
        {
            _parentAnswer = answer;
            StateHasChanged();
        }

        private async Task ShowDebug()
        {

            ConfirmOptions _confirmOptions = new ConfirmOptions() { OkButtonText = "OK", CancelButtonText = "No", ShowClose = false };
            string dialogMessage = $@"
        <div class='info-grid'>
            <div class='label'>QuestionPool:</div>
            <div class='data'>{_currentQuestion?.QuestionPoolID.Value}</div>
            <div class='label'>Question:</div>    
            <div class='data'>{_currentQuestion?.Id.Value}</div>
            <div class='label'>AttemptCount:</div>    
            <div class='data'>{_questionAttemptCount}</div>
            <div class='label'>Score out of {_lessonquiz.QuestionPools.Count()}</div>    
            <div class='data'>{Score}</div>

             <br>
        </div>";

            
            var dialog = await _dialogService.Alert("", dialogMessage, _confirmOptions);
        }



        private bool disposedValue = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    // This is where you can release any resources like event handlers, timers, etc.
                }

                // Dispose unmanaged resources
                // This is where you can release any unmanaged resources if needed

                disposedValue = true;
            }
        }

        // Finalizer
        ~LessonQuizComponent()
        {
            Dispose(false);
        }
    }
}

