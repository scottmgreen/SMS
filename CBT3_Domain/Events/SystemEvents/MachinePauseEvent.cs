namespace CBT3_Domain.Events.SystemEvents;

public class MachinePauseEvent : BaseDomainEvent
{
    public MachineState State { get; set; }
    public IBaseEntity Entity { get; set; }
    public string Text { get; set; }
    public MachinePauseEvent(DateTime datetime, IBaseEntity entity, MachineState state, string message) : base(datetime)
    {
        State = state;
        Entity = entity;
        Text = message;
        OccurredOn = DateTime.Now;
    }
}
