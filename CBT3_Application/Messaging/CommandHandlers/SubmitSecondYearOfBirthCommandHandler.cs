
namespace CBT3_Application.Messaging.CommandHandlers;

public class SubmitSecondYearOfBirthCommandHandler : BaseCommandBundle, IRequestHandler<SubmitSecondYearOfBirthCommand, Result<YearOfBirth>>
{
    private readonly RegistrationService _registrationService;
    public SubmitSecondYearOfBirthCommandHandler(RegistrationService registrationService)
    {
        _registrationService = registrationService;
    }


    public Task<Result<YearOfBirth>> HandleAsync(SubmitSecondYearOfBirthCommand request, CancellationToken ct = default)
    {
        return _registrationService.SubmitSecondYearOfBirthAsync(request.SecondYearOfBirth, request.FirstYearOfBirth,ct);
    }


}