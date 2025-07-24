

using CBT3_Application.Interfaces;

using CBT3_Domain.Entities;
using CBT3_Domain.Interfaces;

namespace CBT3_Application.States;

public class LessonQuizState : BaseState
{
    private readonly LessonQuizService _quizservice;
    private readonly IMediator _mediator;
    private readonly IMessenger _messenger;
    private Trainee _trainee;
    private Course _course;
    private LessonPage _lessonpage;
    private LessonPageMachine _parentMachine;
    private LessonQuiz _lessonquiz;

    public Question CurrentQuestion;
    public QuestionPoolID CurrentPoolId; 
    


    public LessonQuizState(IMediator mediator, IMessenger messenger, LessonQuizService quizservice,Trainee trainee, Course course, LessonPage lessonpage, LessonQuiz lessonquiz, LessonPageMachine parentMachine) : base(mediator, messenger)
    {
        _trainee = trainee;
        _course = course;
        _quizservice = quizservice;
        _mediator = mediator;// CBT3_Application.Configuration.DependencyInjection.ServiceProvider.GetRequiredService<IMediator>();
        _messenger = messenger;// CBT3_Application.Configuration.DependencyInjection.ServiceProvider.GetRequiredService<IMessenger>();
        _parentMachine = parentMachine;
        _lessonquiz = lessonquiz;
        _lessonpage = lessonpage;
                //This is leading to a duplicate 
        //_messenger.Subscribe<LessonQuizFinishedEvent>(HandleLessonQuizFinishedEvent);

    }
    private void HandleLessonQuizFinishedEvent(LessonQuizFinishedEvent @event)
    {
        ExitState(@event.IsFail);

    }
    public override void EnterState(IBaseMachine machine)
    {

    }

    public override void ExitState(bool isFail)
    {

        _parentMachine.LessonFail = isFail;
        _parentMachine.MachinePause(string.Empty);

    }

    public override void HandleState(IBaseEntity cbt_entity)
    {
        _messenger.Publish(new LessonQuizStartedEvent(DateTime.Now, _lessonquiz, QuizState.QuizStarted, $"{_lessonpage.Id} {_lessonpage.LessonPageType} {_lessonpage.PageOrder} {_lessonpage.LessonPageSubType} {_lessonquiz.Id}"));

        Debug.WriteLine($"Question Pools { _lessonquiz.QuestionPools.Count}");
        _quizservice.InitializeQuiz(_trainee, (CourseID)_course.Id,(LessonQuiz)_lessonquiz);
        //StartLessonQuiz();
        ///HANDLE QUIZ INTERACTION IN THE QUIZ STATE ///
        //_isFail = _quizservice.IsQuizFail();
        //_eventAggregator.Publish(new LessonQuizFinishedEvent(DateTime.Now, _lessonquiz, QuizState.QuizFinished, string.Empty, _isFail));
        //ExitState(_isFail);
    }

   


    

   
}
