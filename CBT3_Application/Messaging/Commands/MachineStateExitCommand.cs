namespace CBT3_Application.Messaging;


    public class MachineStateExitCommand : BaseCommandBundle, IRequest<IBaseMachine>
    {
        public MachineStateExitCommand(IBaseMachine machine)
        {
            Machine = machine;
        }
        public IBaseMachine Machine { get; init; }
    }

