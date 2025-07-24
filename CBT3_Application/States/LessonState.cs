
namespace CBT3_Application.States;

public class LessonState : BaseState, IBaseState
{
    private LessonMachine _parentMachine;
    public LessonPageMachine ChildMachine;
    public Lesson Lesson;

    public LessonState(IMediator mediator, IMessenger messenger, Lesson lesson, LessonMachine parentMachine) : base(mediator, messenger)
    {
        _parentMachine = parentMachine;
        ChildMachine = new LessonPageMachine(Mediator, Messenger);
        Lesson = lesson;
    }

    public override void HandleState(IBaseEntity cbt_entity)
    {
        ChildMachineStartRequest();
    }

    public override void EnterState(IBaseMachine machine)
    {
        _parentMachine = (LessonMachine)machine;
        
        if (_parentMachine.LessonFail)
        {
            //override and only present the pages for the fail
            Lesson.LessonPages = Lesson.LessonPages.Where(page => page.LessonPageType == PageType.PT_BASE_08).ToList();
        }


        ChildMachine.InitializeMachine(_parentMachine.Trainee, _parentMachine.Course, Lesson.LessonPages, Lesson, _parentMachine);
        _parentMachine.ChildMachine = ChildMachine;


    }

    public override void ExitState(bool completed)
    {
        //BECAREFUL MESS AROUND AT YOU'RE OWN PERIL
        _parentMachine.StateChange();
    }

    private void ChildMachineStartRequest()
    {
        CancellationToken ct = new();
        var message = new MachineStartCommand(ChildMachine);
        Mediator.SendAsync(message,ct);
    }


}




