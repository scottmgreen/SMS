
namespace CBT3_Application.Messaging.CommandHandlers;
public class SubmitLastNameCommandHandler : BaseCommandBundle, IRequestHandler<SubmitLastNameCommand, Result<LastName>>
{
    private readonly RegistrationService _registrationService;
    public SubmitLastNameCommandHandler(RegistrationService registrationService)
    {
        _registrationService = registrationService;
    }


    public Task<Result<LastName>> HandleAsync(SubmitLastNameCommand request, CancellationToken ct = default)
    {
        return _registrationService.SubmitLastNameAsync(request.LastName,ct);

    }


}
