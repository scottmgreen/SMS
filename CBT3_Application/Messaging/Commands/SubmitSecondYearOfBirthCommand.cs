namespace CBT3_Application.Messaging.Commands;

public class SubmitSecondYearOfBirthCommand : BaseCommandBundle, IRequest<Result<YearOfBirth>>
{
    public string SecondYearOfBirth { get; set; }
    public YearOfBirth FirstYearOfBirth { get; set; }

}