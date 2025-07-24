
using CBT3_Application.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using static System.Formats.Asn1.AsnWriter;

namespace CBT3_Application.Services;

public sealed class LessonQuizService 
{
    private int _consecutiveAttemptsInPool;
    private CourseID _courseId;
    
    private int _currentPoolIndex;
    private int _currentQuestionIndex;
    private IMessenger _messenger;
    
    private LessonQuiz _lessonQuiz;
    private IMediator _mediator;
    private List<QuestionPool> _questionPools = new();
    private Random _random;
    private Trainee _trainee;
    private bool _isQuizFail { get; set; }

    public QuestionPoolID CurrentPoolId;
    public Question CurrentQuestion;
    public bool IsQuizComplete() => _currentPoolIndex >= _questionPools.Count;
    public bool IsQuizFail() => _isQuizFail;
    
    public int Score { get; set; }
    
    private HashSet<QuestionID> _usedQuestionIds = new HashSet<QuestionID>();

    public LessonQuizService(IMediator mediator, IMessenger messenger)
    {
        _mediator = mediator;
        _messenger = messenger;
    }
    public void InitializeQuiz(Trainee trainee, CourseID courseid, LessonQuiz lessonQuiz)
    {
        Console.WriteLine("Quiz started.");
        
        
        this._lessonQuiz = lessonQuiz;
        this._trainee = trainee;
        this._courseId = courseid;
        this._questionPools = lessonQuiz.QuestionPools;
        this._random = new Random();
        this._currentPoolIndex = -1;
        this._currentQuestionIndex = 0;
        this._consecutiveAttemptsInPool = 0;
        Score = 0;
       var startlessonquizcommand = new StartLessonQuizCommand() 
       { TraineeId = (TraineeID)trainee.Id, CourseId = courseid, LessonQuizId = (LessonQuizID)lessonQuiz.Id };
        _ = _mediator.SendAsync<Result<bool>>(startlessonquizcommand, default);

    }
    
   
    private void HandleCorrectAnswer(Answer selectedAnswer)
    {
        _consecutiveAttemptsInPool = 0;
        AddTrainingLogEntry(selectedAnswer);
    }
    private void HandleCorrectAnswers(List<Answer> selectedAnswers)
    {
        _consecutiveAttemptsInPool = 0;
        foreach (Answer answer in selectedAnswers)
        {
            if (answer.IsCorrect == true)
            {
                AddTrainingLogEntry(answer);
            }
        }
    }
    private void HandleIncorrectAnswer(Answer selectedAnswer)
    {

        _consecutiveAttemptsInPool++;
        _isQuizFail = _consecutiveAttemptsInPool >= _lessonQuiz.AttemptsAllowed;
        
        AddTrainingLogEntry(selectedAnswer);

        if (_isQuizFail)
        {
            FinishQuiz();
        }
        //else
        //{
        //    _isQuizFail = false;
        //}
        
    }
    private void HandleIncorrectAnswers(List<Answer> selectedAnswers)
    {

        _consecutiveAttemptsInPool++;
        _isQuizFail = _consecutiveAttemptsInPool >= _lessonQuiz.AttemptsAllowed;

        foreach (Answer answer in selectedAnswers)
        {
            AddTrainingLogEntry(answer);
        }

        if (_isQuizFail)
        {                       
            FinishQuiz();
        }
        //else
        //{
        //    _isQuizFail = false;  
        //}

        
    }
    

    private void FinishQuiz()
    {
        _messenger.Publish(new LessonQuizFinishedEvent(DateTime.Now, _lessonQuiz, QuizState.QuizFinished, string.Empty, _isQuizFail));
        
    }
    private bool CheckAnswer(Answer selectedAnswer)
    {
        if (selectedAnswer is null)
        {
            FinishQuiz();
            return false; // Indicate quiz failure
        }

        if (selectedAnswer.IsCorrect == true)
        {
            HandleCorrectAnswer(selectedAnswer);
            return true; // Indicate correct answer
        }
        else
        {
            HandleIncorrectAnswer(selectedAnswer);
            return false; // Indicate incorrect answer
        }
    }
    private bool CheckAnswers(List<Answer> selectedAnswers)
    {
        if (selectedAnswers is null || !selectedAnswers.Any())
        {
            FinishQuiz();
            return false; // Indicate quiz failure
        }

        int selectedCorrect = selectedAnswers.Count(a => a.IsCorrect == true);
        int countOfProvidedAnswers = selectedAnswers.Count;

        if (selectedCorrect == countOfProvidedAnswers)
        {
            HandleCorrectAnswers(selectedAnswers);
            return true; // Indicate correct answer
        }
        else
        {
            HandleIncorrectAnswers(selectedAnswers);
            return false; // Indicate incorrect answer
        }
    }

    private void ResetQuestionsInPool(QuestionPool pool)
    {
        // Clear the used question IDs for the pool
        foreach (var question in pool.Questions)
        {
            _usedQuestionIds.Remove((QuestionID)question.Id);
        }

    }

    public Question GetNextQuestion(bool movenext)
    {
        if (movenext)
        {
            _currentPoolIndex++;
        }
        // Check if there are no more question pools
        if (_currentPoolIndex >= _questionPools.Count)
        {
            _isQuizFail = false;
            //FinishQuiz();
            return null; // No more questions, quiz is complete
        }


        // Get the current pool
        var currentPool = _questionPools[_currentPoolIndex];

        // If the current pool has no more questions, move to the next pool
        if (_currentQuestionIndex >= currentPool.Questions.Count)
        {
            if (_currentPoolIndex >= _questionPools.Count)
            {
                _isQuizFail = false;
                FinishQuiz();
                return null; // No more questions, quiz is complete
            }
            currentPool = _questionPools[_currentPoolIndex]; // Get the next pool
        }

        // Select a random question from the current pool
        var randomQuestionIndex = _random.Next(0, currentPool.Questions.Count);
        var randomQuestion = currentPool.Questions[randomQuestionIndex];
        
        CurrentPoolId = randomQuestion.QuestionPoolID;
        CurrentQuestion = randomQuestion;
        
        if (randomQuestion is not null)
        {
            return randomQuestion;
        }
        else
        {
            FinishQuiz();
            return null;
        }
        
    }

    public Question GetNextQuestionFromSamePool(Question currentquestion)
    {
        if (_currentPoolIndex < 0 || _currentPoolIndex >= _questionPools.Count)
        {
            // Invalid current pool index
            FinishQuiz();
            return null;
        }

        // Get the current question pool
        var currentPool = _questionPools[_currentPoolIndex];
        currentPool.Questions.Remove(currentquestion);
        // Filter out questions that have not been used yet
        var unusedQuestions = currentPool.Questions.Where(q => !_usedQuestionIds.Contains(q.Id)).ToList();

        if (unusedQuestions.Count == 0)
        {
            // All questions in the pool have been used, reset them
            ResetQuestionsInPool(currentPool);
            unusedQuestions = currentPool.Questions.ToList();
        }

        // Select a random question from the unused questions
        var randomIndex = _random.Next(0, unusedQuestions.Count);
        var selectedQuestion = unusedQuestions[randomIndex];

        // Mark the selected question as used
        _usedQuestionIds.Add((QuestionID)selectedQuestion.Id);
        CurrentPoolId = selectedQuestion.QuestionPoolID;
        CurrentQuestion = selectedQuestion;
        
        if (selectedQuestion is not null)
        {
            return selectedQuestion;
        }
        else
        {
            FinishQuiz();
            return null;
        }
        
    }

    public Result<Question> SubmitAnswer(Answer selectedAnswer)
    {

        bool isCorrect = CheckAnswer(selectedAnswer);
        
        if (isCorrect)
        {
            Score++;
            CurrentQuestion = this.GetNextQuestion(isCorrect);
            if (CurrentQuestion != null)
            {
                CurrentPoolId = CurrentQuestion.QuestionPoolID;
            }
            return CurrentQuestion;
            
        }
        if (!isCorrect && !IsQuizFail())
        {
            CurrentQuestion = this.GetNextQuestionFromSamePool(CurrentQuestion);
            if (CurrentQuestion != null)
            {
                CurrentPoolId = CurrentQuestion.QuestionPoolID;
            }
            return CurrentQuestion;
            
        }
        else
        {
            CurrentQuestion = null;
            return CurrentQuestion;
        }


        }

    public Result<Question> SubmitAnswers(List<Answer> providedAnswers)
    {
        var selectedAnswers = providedAnswers.Where(a => a.IsSelected).ToList();
        bool isCorrect = CheckAnswers(selectedAnswers);
        
        if (isCorrect)
        {
            Score++;
            CurrentQuestion = this.GetNextQuestion(isCorrect);
            if (CurrentQuestion != null)
            {
                CurrentPoolId = CurrentQuestion.QuestionPoolID;
            }
            return CurrentQuestion;
            
        }
        if (!isCorrect && !IsQuizFail())
        {
            CurrentQuestion = GetNextQuestionFromSamePool(CurrentQuestion);
            if (CurrentQuestion != null)
            {
                CurrentPoolId = CurrentQuestion.QuestionPoolID;
            }
            return CurrentQuestion;
        }
        else
        {
            CurrentQuestion = null;
            return CurrentQuestion;
        }

    }

    public Result<QuestionPool> GetPlaybackQuestionPool ()
    {
        return _questionPools[_currentPoolIndex];
    }

    private TrainingLogEntry GetTrainingLogEntry(Answer selectedAnswer)
    {
        TraineeID traineeID = (TraineeID)_trainee.Id;
        TrainingLogEntryID tleID = new(Guid.NewGuid().ToString());
        TrainingLogEntry traininglogEntry = new(tleID);
        traininglogEntry.TraineeId = traineeID;
        traininglogEntry.CourseId = _courseId;
        traininglogEntry.LessonId = _lessonQuiz.LessonID;
        traininglogEntry.LessonQuizId = new(_lessonQuiz.Id.Value);
        traininglogEntry.QuestionPoolId = CurrentPoolId;
        traininglogEntry.QuestionId = new(CurrentQuestion.Id.Value);
        traininglogEntry.RecordedAt = DateTime.Now;
        traininglogEntry.AnswerId = (AnswerID)selectedAnswer.Id;
        traininglogEntry.IsCorrect = selectedAnswer.IsCorrect ?? false;

        return traininglogEntry;
    }

    private void AddTrainingLogEntry(Answer selectedAnswer)
    {
        TrainingLogEntry logentry = GetTrainingLogEntry(selectedAnswer);
        CancellationToken ct = new();
        
        var addlogentrycommand = new AddTrainingLogEntryCommand() { TrainingLogEntry = logentry };
        _ = _mediator.SendAsync(addlogentrycommand, ct);
    }
    
}


