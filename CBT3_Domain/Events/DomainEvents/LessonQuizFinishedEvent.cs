namespace CBT3_Domain.Events.DomainEvents;

public class LessonQuizFinishedEvent : BaseDomainEvent
{
    public QuizState State { get; set; }
    public IBaseEntity Entity { get; set; }
    public string Text { get; set; }
    public bool IsFail { get; set; }
    public LessonQuizFinishedEvent(DateTime datetime, IBaseEntity entity, QuizState state, string message, bool isfail) : base(datetime)
    {
        Text = message;
        State = state;
        Entity = entity;
        IsFail = isfail;
        OccurredOn = DateTime.Now;
    }
}
