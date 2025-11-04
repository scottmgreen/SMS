namespace SMS_Domain.Enums;

public abstract class CourseState : BaseEnum<CourseState>
{
    protected CourseState(string value, string name) : base(value, name) { }

    public static readonly CourseState CourseStarted = new CourseStartedState();
    public static readonly CourseState CourseFinished = new CourseFinishedState();
    //public static readonly CourseState LessonStarted = new LessonStartedState();
    //public static readonly CourseState LessonFinished = new LessonFinishedState();
    //public static readonly CourseState LessonQuizStarted = new LessonQuizStartedState();
    //public static readonly CourseState LessonQuizFinished = new LessonQuizFinishedState();


    private sealed class CourseStartedState : CourseState
    {
        public CourseStartedState() : base("CourseStarted", "Course Started")
        {
        }
    }
    private sealed class CourseFinishedState : CourseState
    {
        public CourseFinishedState() : base("CourseFinished", "Course Finished")
        {
        }
    }
    //private sealed class LessonStartedState : CourseState
    //{
    //    public LessonStartedState() : base("LessonStartedState", "Lesson Started")
    //    {
    //    }
    //}
    //private sealed class LessonFinishedState : CourseState
    //{
    //    public LessonFinishedState() : base("LessonFinishedState", "Lesson Finished")
    //    {
    //    }
    //}
    //private sealed class LessonQuizStartedState : CourseState
    //{
    //    public LessonQuizStartedState() : base("LessonQuizStartedState", "Lesson Quiz Started")
    //    {
    //    }
    //}
    //private sealed class LessonQuizFinishedState : CourseState
    //{
    //    public LessonQuizFinishedState() : base("LessonQuizFinishedState", "Lesson Quiz Finished")
    //    {
    //    }
    //}
}

