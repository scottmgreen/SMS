namespace CBT3_Application.Messaging;


    public class MachineStateChangeCommand : BaseCommandBundle, IRequest<IBaseMachine>
    {
        public MachineStateChangeCommand(IBaseMachine machine)
        {
            Machine = machine;
        }
        public IBaseMachine Machine { get; init; }
    }

