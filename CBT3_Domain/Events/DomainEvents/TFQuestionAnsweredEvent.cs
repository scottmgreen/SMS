namespace CBT3_Domain.Events.DomainEvents;

public class TFQuestionAnsweredEvent : BaseDomainEvent
{
    public QuizState State;
    public Answer Answer { get; set; }
    public string Text { get; set; }
    public TFQuestionAnsweredEvent(DateTime datetime, Answer answer, QuizState state, string message) : base(datetime)
    {
        Text = message;
        State = state;
        Answer = answer;
        OccurredOn = DateTime.Now;
    }
}
