using SMS_Domain.Enums;

namespace SMS_Application.Interfaces;

public interface IEventLifecycleRegistry
{
    IReadOnlyList<EventLifecycleEntry> GetAll();

    IReadOnlyList<EventLifecycleEntry> GetByCategory(EventCategory eventCategory);

    bool TryGet(string eventClassName, out EventLifecycleEntry entry);

    EventLifecycleState ResolveState(string eventClassName, EventLifecycleState fallbackState);

    bool IsActive(string eventClassName);
}

public sealed record EventLifecycleEntry(
    string EventClassName,
    EventCategory EventCategory,
    EventLifecycleState LifecycleState,
    string Notes);
