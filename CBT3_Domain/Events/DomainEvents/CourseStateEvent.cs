namespace CBT3_Domain.Events.DomainEvents;

public class CourseStateEvent : BaseDomainEvent
{
    public CourseState State;
    public IBaseEntity Entity { get; set; }
    public string Text { get; set; }
    public bool CoursePass { get; set; }
    public CourseStateEvent(DateTime datetime, IBaseEntity entity, CourseState state, string message, bool coursepass) : base(datetime)
    {
        State = state;
        Entity = entity;
        Text = message;
        CoursePass = coursepass;
        OccurredOn = DateTime.Now;
    }
}
