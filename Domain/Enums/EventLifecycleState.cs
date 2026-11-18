using SMS_Domain.Common;

namespace SMS_Domain.Enums;

/// <summary>
/// Lifecycle state for EventBus event-chain readiness.
/// </summary>
public abstract class EventLifecycleState : BaseEnum<EventLifecycleState>
{
    protected EventLifecycleState(string value, string name, int id) : base(value, name)
    {
        Id = id;
    }

    public int Id { get; }

    public static readonly EventLifecycleState Active = new ActiveState();
    public static readonly EventLifecycleState PlannedStub = new PlannedStubState();
    public static readonly EventLifecycleState Deprecated = new DeprecatedState();

    public static EventLifecycleState FromId(int id)
    {
        return GetAllValues().FirstOrDefault(x => x.Id == id) ?? Active;
    }

    private sealed class ActiveState : EventLifecycleState
    {
        public ActiveState() : base("ACTIVE", "Active", 0)
        {
        }
    }

    private sealed class PlannedStubState : EventLifecycleState
    {
        public PlannedStubState() : base("PLANNED_STUB", "PlannedStub", 1)
        {
        }
    }

    private sealed class DeprecatedState : EventLifecycleState
    {
        public DeprecatedState() : base("DEPRECATED", "Deprecated", 2)
        {
        }
    }
}
