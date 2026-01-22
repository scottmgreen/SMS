namespace SMS_Domain.Common;

public abstract class BaseDomainEvent : IBaseDomainEvent //, INotification
{
    public BaseDomainEvent(DateTime datetime)
    {
        OccurredOn = datetime;
    }
    public DateTime OccurredOn { get; protected set; } = DateTime.UtcNow;
}
