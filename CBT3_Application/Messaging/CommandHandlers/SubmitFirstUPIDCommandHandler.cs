
namespace CBT3_Application.Messaging.CommandHandlers;

public class SubmitFirstUPIDCommandHandler : BaseCommandBundle, IRequestHandler<SubmitFirstUPIDCommand, Result<UPID>>
{
    private readonly RegistrationService _registrationService;
    public SubmitFirstUPIDCommandHandler(RegistrationService registrationService)
    {
        _registrationService = registrationService;
    }


    public Task<Result<UPID>> HandleAsync(SubmitFirstUPIDCommand request, CancellationToken ct = default)
    {
        return _registrationService.SubmitFirstUPIDAsync(request.FirstUPID,ct);
    }


}
