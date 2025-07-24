
namespace CBT3_Domain.Events.DomainEvents;

public class AskQuestionEvent : BaseDomainEvent
{
    public QuizState State;
    public IBaseEntity Question { get; set; }
    public string Text { get; set; }
    public AskQuestionEvent(DateTime datetime, IBaseEntity question, QuizState state, string message) : base(datetime)
    {
        Text = message;
        State = state;
        Question = question;
        OccurredOn = DateTime.Now;
    }
}
