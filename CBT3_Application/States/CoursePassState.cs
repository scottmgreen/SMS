
using CBT3_Application.Interfaces;

namespace CBT3_Application.States;

public class CoursePassState : BaseState
{
    private LessonPage _lessonpage;
    private Lesson _lesson;
    private LessonPageMachine _parentMachine;

    public CoursePassState(IMediator mediator, IMessenger messenger, LessonPage lessonpage, Lesson lesson, LessonPageMachine parentMachine) : base(mediator, messenger)
    {
        _parentMachine = parentMachine;
        _lesson = lesson;
        _lessonpage = lessonpage;
    }
    public override void HandleState(IBaseEntity cbt_entity)
    {
        Messenger.Publish(new LessonPageEvent(DateTime.Now, _lessonpage, $"{_lessonpage.Id} {_lessonpage.LessonPageType}"));
        ExitState(true);
    }

    public override void ExitState(bool completed)
    {
        _parentMachine.LessonFail = false;
        _parentMachine.MachinePause(string.Empty);
    }

    public override void EnterState(IBaseMachine machine)
    {
        _parentMachine = (LessonPageMachine)machine;
    }


}
