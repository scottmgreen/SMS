
namespace CBT3_Domain.Common;

public abstract class BaseAggregateRoot
{
    private readonly List<IBaseDomainEvent> _domainEvents = new();

    public Guid Id { get; protected set; }


    public IReadOnlyCollection<IBaseDomainEvent> DomainEvents => _domainEvents.AsReadOnly(); // Expose domain events as IReadOnlyCollection
    public void AddDomainEvent(IBaseDomainEvent domainEvent) => _domainEvents.Add(domainEvent); //// Add domain event to the list
    public void RemoveDomainEvent(IBaseDomainEvent domainEvent) => _domainEvents.Remove(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear(); //// Clear domain events
}
