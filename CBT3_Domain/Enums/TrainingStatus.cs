
namespace CBT3_Domain.Enums;


public abstract class TrainingStatus : BaseEnum<TrainingStatus>
{
    protected TrainingStatus(string value, string name) : base(value, name) { }

    //public const string TrainingStarted = "Started";
    //public const string TrainingStopped = "Stopped";
    //public const string TrainingRunning = "Running";


    public static readonly TrainingStatus TrainingStarted = new Started();
    public static readonly TrainingStatus TrainingStopped = new Stopped();
    public static readonly TrainingStatus TrainingRunning = new Running();



    private sealed class Started : TrainingStatus
    {
        public Started() : base("TrainingStarted", "Training Started")
        {
        }
    }
    private sealed class Stopped : TrainingStatus
    {
        public Stopped() : base("TrainingStopped", "Training Stopped")
        {
        }
    }
    private sealed class Running : TrainingStatus
    {
        public Running() : base("TrainingRunning", "Training Running")
        {
        }
    }
}