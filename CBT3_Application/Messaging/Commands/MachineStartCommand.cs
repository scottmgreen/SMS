namespace CBT3_Application.Messaging;


    public class MachineStartCommand : BaseCommandBundle,IRequest<IBaseMachine>
    {
        public MachineStartCommand(IBaseMachine machine)
        {
            Machine = machine;
        }
        public IBaseMachine Machine { get; init; }

    }

