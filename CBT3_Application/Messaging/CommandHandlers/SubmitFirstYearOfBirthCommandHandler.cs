
namespace CBT3_Application.Messaging.CommandHandlers;

public class SubmitFirstYearOfBirthCommandHandler : BaseCommandBundle, IRequestHandler<SubmitFirstYearOfBirthCommand, Result<YearOfBirth>>
{
    private readonly RegistrationService _registrationService;
    public SubmitFirstYearOfBirthCommandHandler(RegistrationService registrationService)
    {
        _registrationService = registrationService;
    }


    public Task<Result<YearOfBirth>> HandleAsync(SubmitFirstYearOfBirthCommand request, CancellationToken ct = default)
    {
        return _registrationService.SubmitFirstYearOfBirthAsync(request.FirstYearOfBirth,ct);
    }


}
