namespace CBT3_Application.Messaging;


    public class AddTraineeCommand : BaseCommandBundle, IRequest<Result<Trainee>>
    {
        public Trainee Trainee { get; init; }

    }

