namespace CBT3_Application.Messaging.Commands;

public class SubmitFirstYearOfBirthCommand : BaseCommandBundle, IRequest<Result<YearOfBirth>>
{
    public string FirstYearOfBirth { get; set; }

}
