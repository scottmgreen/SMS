namespace CBT3_Application.Messaging;


    public class AddTrainingLogEntryCommand : BaseCommandBundle, IRequest<TrainingLogEntry>
    {
        public TrainingLogEntry TrainingLogEntry { get; init; }

    }





