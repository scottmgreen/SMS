namespace CBT3_Application.Messaging.Commands;

public class SubmitFirstUPIDCommand : BaseCommandBundle, IRequest<Result<UPID>>
{
    public string FirstUPID { get; set; }

}
