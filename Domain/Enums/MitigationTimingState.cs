namespace SMS_Domain.Enums;

using SMS_Domain.Common;

/// <summary>
/// Timing states used for mitigation target-date notifications.
/// </summary>
public abstract class MitigationTimingState : BaseEnum<MitigationTimingState>
{
    protected MitigationTimingState(string value, string name, int rank) : base(value, name)
    {
        Rank = rank;
    }

    public int Rank { get; }

    public static readonly MitigationTimingState None = new NoneState();
    public static readonly MitigationTimingState DueInAdvanceWindow = new DueInAdvanceWindowState();
    public static readonly MitigationTimingState DueInFinalWindow = new DueInFinalWindowState();
    public static readonly MitigationTimingState Overdue = new OverdueState();

    private sealed class NoneState : MitigationTimingState
    {
        public NoneState() : base("NONE", "None", 0)
        {
        }
    }

    private sealed class DueInAdvanceWindowState : MitigationTimingState
    {
        public DueInAdvanceWindowState() : base("DUE_IN_ADVANCE_WINDOW", "DueInAdvanceWindow", 1)
        {
        }
    }

    private sealed class DueInFinalWindowState : MitigationTimingState
    {
        public DueInFinalWindowState() : base("DUE_IN_FINAL_WINDOW", "DueInFinalWindow", 2)
        {
        }
    }

    private sealed class OverdueState : MitigationTimingState
    {
        public OverdueState() : base("OVERDUE", "Overdue", 3)
        {
        }
    }
}
