namespace CBT3_Application.Messaging.Commands;
public class SubmitLastNameCommand : BaseCommandBundle, IRequest<Result<LastName>>
{
    public string LastName { get; set; }

}
