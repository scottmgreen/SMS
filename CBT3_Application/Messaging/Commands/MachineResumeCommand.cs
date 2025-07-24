namespace CBT3_Application.Messaging.Commands;


    public class MachineResumeCommand : BaseCommandBundle,IRequest<bool>
    {
        public MachineResumeCommand(IBaseMachine machine)
        {
            Machine = machine;
        }
        public IBaseMachine Machine { get; init; }

    }

