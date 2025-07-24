namespace CBT3_Application.Messaging.Commands;
public class SubmitFirstNameCommand : BaseCommandBundle, IRequest<Result<FirstName>>
{
    public string FirstName { get; set; }
    
}
