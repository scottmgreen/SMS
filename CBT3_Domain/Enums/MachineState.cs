namespace CBT3_Domain.Enums;

public abstract class MachineState : BaseEnum<MachineState>
{
    protected MachineState(string value, string name) : base(value, name) { }

    public static readonly MachineState CourseMachinePaused = new CourseMachinePausedState();
    public static readonly MachineState LessonMachinePaused = new LessonMachinePausedState();
    public static readonly MachineState LessonPageMachinePaused = new LessonPageMachinePausedState();

    private sealed class CourseMachinePausedState : MachineState
    {
        public CourseMachinePausedState() : base("CourseMachinePaused", "Course Machine Paused")
        {
        }
    }
    private sealed class LessonMachinePausedState : MachineState
    {
        public LessonMachinePausedState() : base("LessonMachinePaused", "Lesson Machine Paused")
        {
        }
    }
    private sealed class LessonPageMachinePausedState : MachineState
    {
        public LessonPageMachinePausedState() : base("LessonPageMachinePaused", "Lesson Page Machine Paused")
        {
        }
    }

}
