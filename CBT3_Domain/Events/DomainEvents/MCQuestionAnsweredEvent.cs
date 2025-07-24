namespace CBT3_Domain.Events.DomainEvents;

public class MCQuestionAnsweredEvent : BaseDomainEvent
{
    public QuizState State;
    public List<Answer> SubmittedAnswers { get; set; }
    public string Text { get; set; }
    public MCQuestionAnsweredEvent(DateTime datetime, List<Answer> submittedanswers, QuizState state, string message) : base(datetime)
    {
        Text = message;
        State = state;
        SubmittedAnswers = submittedanswers;
        OccurredOn = DateTime.Now;
    }
}
