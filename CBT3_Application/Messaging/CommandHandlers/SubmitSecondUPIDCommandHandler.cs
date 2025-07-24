
namespace CBT3_Application.Messaging.CommandHandlers;

public class SubmitSecondUPIDCommandHandler : BaseCommandBundle, IRequestHandler<SubmitSecondUPIDCommand, Result<UPID>>
{
    private readonly RegistrationService _registrationService;
    public SubmitSecondUPIDCommandHandler(RegistrationService registrationService)
    {
        _registrationService = registrationService;
    }


    public Task<Result<UPID>> HandleAsync(SubmitSecondUPIDCommand request, CancellationToken ct = default)
    {
        return _registrationService.SubmitSecondUPIDAsync(request.SecondUPID,request.FirstUPID,ct);
    }


}
