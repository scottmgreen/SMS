namespace CBT3_Application.Messaging;


public class StartTrainingSessionCommand : BaseCommandBundle, IRequest<TrainingSession>
{
    public StartTrainingSessionCommand(TrainingSession session)
    {
        TrainingSession = session;
    }
    public TrainingSession TrainingSession { get; init; }

}
