namespace CBT3_Application.Messaging;

    public class MachineStateChangeCommandHandler : BaseCommandBundle, IRequestHandler<MachineStateChangeCommand, IBaseMachine>
    {
        public MachineStateChangeCommandHandler()
        {
           
        }
       
        public Task<IBaseMachine> HandleAsync(MachineStateChangeCommand request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            request.Machine.StateChange();
            return Task.FromResult(request.Machine);
        }
    }

