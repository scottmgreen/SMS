namespace CBT3_Application.Messaging.Commands;

public class SubmitSecondUPIDCommand : BaseCommandBundle, IRequest<Result<UPID>>
{
    public UPID FirstUPID {get;set;}
    public string SecondUPID { get; set; }

}