
namespace CBT3_Application.States;


public class CourseStartState : BaseState
{
    private CourseMachine _parentMachine;
    private Course _course;
    public CourseStartState(IMediator mediator, IMessenger messenger, Course course) : base(mediator, messenger)
    {
        _course = course;
    }

    public override void HandleState(IBaseEntity _course)
    {
        // CoursePass = false //default
        Messenger.Publish(new CourseStateEvent(DateTime.Now, _course, CourseState.CourseStarted, $"Course First State=> {_course.Id} Course Started", _parentMachine.CoursePass));
        var message = new MachineStartCommand(_parentMachine.ChildMachine);
        CancellationToken ct = new();
        Mediator.SendAsync(message,ct);
        ExitState(true);

    }

    public override void ExitState(bool completed){}

    public override void EnterState(IBaseMachine machine)
    {
        _parentMachine = (CourseMachine)machine;
    }


}

