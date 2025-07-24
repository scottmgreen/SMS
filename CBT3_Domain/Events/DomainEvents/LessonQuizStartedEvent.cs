namespace CBT3_Domain.Events.DomainEvents;

public class LessonQuizStartedEvent : BaseDomainEvent
{
    public QuizState State { get; set; }
    public LessonQuiz LessonQuiz { get; set; }
    public string Text { get; set; }
    public LessonQuizStartedEvent(DateTime datetime, LessonQuiz quiz, QuizState state, string message) : base(datetime)
    {
        LessonQuiz = quiz;
        State = state;
        Text = message;
        OccurredOn = DateTime.Now;
    }
}
