

using CBT3_Application.Messaging.Commands;

namespace CBT3_Application.Messaging.CommandHandlers;
public class SubmitFirstNameCommandHandler : BaseCommandBundle, IRequestHandler<SubmitFirstNameCommand, Result<FirstName>>
{
    private readonly RegistrationService _registrationService;
    public SubmitFirstNameCommandHandler(RegistrationService registrationService)
    {
        _registrationService = registrationService;
    }


    public Task<Result<FirstName>> HandleAsync(SubmitFirstNameCommand request, CancellationToken ct = default)
    {
        return _registrationService.SubmitFirstNameAsync(request.FirstName, ct);

    }
}
