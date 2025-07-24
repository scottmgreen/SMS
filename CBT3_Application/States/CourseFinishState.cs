

namespace CBT3_Application.States;

public class CourseFinishState : BaseState
{
    private CourseMachine _parentMachine;
    private Course _course;

    public CourseFinishState(IMediator mediator, IMessenger messenger, Course course) : base(mediator, messenger)
    {
        _course = course;
    }
    public override void HandleState(IBaseEntity cbt_entity)
    {
        ExitState(true);
    }


    public override void ExitState(bool completed)
    {
        Messenger.Publish(new CourseStateEvent(DateTime.Now, _course, CourseState.CourseFinished, $"{_course.Id} Course Complete", _parentMachine.CoursePass));
    }

    public override void EnterState(IBaseMachine machine)
    {
        _parentMachine = (CourseMachine)machine;

    }


}


