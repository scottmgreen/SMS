using SMS_Domain.Common;

namespace SMS_Domain.Enums;

/// <summary>
/// Priority level for event processing
/// </summary>
public abstract class EventPriority : BaseEnum<EventPriority>
{
    protected EventPriority(string value, string name, int id) : base(value, name)
    {
        Id = id;
    }

    public int Id { get; }

    /// <summary>
    /// Low priority - UI updates, non-critical notifications
    /// </summary>
    public static readonly EventPriority Low = new LowPriority();

    /// <summary>
    /// Normal priority - Standard business events
    /// </summary>
    public static readonly EventPriority Normal = new NormalPriority();

    /// <summary>
    /// High priority - Integration events, critical notifications
    /// </summary>
    public static readonly EventPriority High = new HighPriority();

    /// <summary>
    /// Critical priority - Security alerts, system failures
    /// </summary>
    public static readonly EventPriority Critical = new CriticalPriority();

    public static EventPriority FromId(int id)
    {
        return GetAllValues().FirstOrDefault(x => x.Id == id) ?? Normal;
    }

    private sealed class LowPriority : EventPriority
    {
        public LowPriority() : base("LOW", "Low", 0)
        {
        }
    }

    private sealed class NormalPriority : EventPriority
    {
        public NormalPriority() : base("NORMAL", "Normal", 1)
        {
        }
    }

    private sealed class HighPriority : EventPriority
    {
        public HighPriority() : base("HIGH", "High", 2)
        {
        }
    }

    private sealed class CriticalPriority : EventPriority
    {
        public CriticalPriority() : base("CRITICAL", "Critical", 3)
        {
        }
    }
}
