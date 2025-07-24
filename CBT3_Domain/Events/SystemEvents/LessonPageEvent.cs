namespace CBT3_Domain.Events.SystemEvents;

public class LessonPageEvent : BaseDomainEvent
{
    public LessonPage Page { get; set; }
    public string Text { get; set; }
    public LessonPageEvent(DateTime datetime, LessonPage page, string message) : base(datetime)
    {
        Page = page;
        Text = message;
        OccurredOn = DateTime.Now;
    }
}
