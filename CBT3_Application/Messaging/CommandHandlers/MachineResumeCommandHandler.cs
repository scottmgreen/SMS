namespace CBT3_Application.Messaging;


    public class MachineResumeCommandHandler : BaseCommandBundle,IRequestHandler<MachineResumeCommand, bool>
    {

        public MachineResumeCommandHandler()
        {
           
        }
        public Task<bool> HandleAsync(MachineResumeCommand request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            request.Machine.MachineResume(); ;
            return Task.FromResult(true);
        }
    }

