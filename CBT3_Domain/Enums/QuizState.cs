
namespace CBT3_Domain.Enums;

public abstract class QuizState : BaseEnum<QuizState>
{
    protected QuizState(string value, string name) : base(value, name) { }

    public static readonly QuizState QuestionAsked = new QuestionAskedState();
    public static readonly QuizState QuestionAnswered = new QuestionAnsweredState();
    public static readonly QuizState QuizStarted = new QuizStartedState();
    public static readonly QuizState QuizFinished = new QuizFinishedState();



    private sealed class QuestionAskedState : QuizState
    {
        public QuestionAskedState() : base("QuestionAsked", "Question Asked")
        {
        }
    }
    private sealed class QuestionAnsweredState : QuizState
    {
        public QuestionAnsweredState() : base("QuestionAnswered", "Question Answered")
        {
        }
    }
    private sealed class QuizFinishedState : QuizState
    {
        public QuizFinishedState() : base("LessonQuizFinished", "Lesson Quiz Finished")
        {
        }
    }
    private sealed class QuizStartedState : QuizState
    {
        public QuizStartedState() : base("LessonQuizStartedState", "Lesson Quiz Started")
        {
        }
    }

}
