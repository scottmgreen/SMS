namespace CBT3_Application.Messaging;

    public class MachineStartCommandHandler : BaseCommandBundle,IRequestHandler<MachineStartCommand, IBaseMachine>
    {
    private readonly SystemDataService _systemdataservice;
    public MachineStartCommandHandler(SystemDataService systemdataservice)
        {
            _systemdataservice = systemdataservice;
        }
        public Task<IBaseMachine> HandleAsync(MachineStartCommand request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            request.Machine.Start();
            return Task.FromResult(request.Machine);
        }
    }

