
namespace CBT3_Domain.Enums;

public abstract class TrainingStationStatus : BaseEnum<TrainingStationStatus>
{
    protected TrainingStationStatus(string value, string name) : base(value, name) { }

       
    public static readonly TrainingStationStatus MachineStopped = new Stopped();
    public static readonly TrainingStationStatus MachineIdle = new Idle();
    public static readonly TrainingStationStatus MachineRunning = new Running();


    private sealed class Stopped : TrainingStationStatus
    {
        public Stopped() : base("Stopped", "Training Machine Stopped")
        {
        }
    }
    private sealed class Idle : TrainingStationStatus
    {
        public Idle() : base("Idle", "Training Machine Idle")
        {
        }
    }
    private sealed class Running : TrainingStationStatus
    {
        public Running() : base("Running", "Training Machine Running")
        {
        }
    }

    
}
