namespace SMS_Application.Messaging.Commands;

public class CreateInterviewCommand : BaseCommandBundle, IRequest<Result<Interview>>
{
    public Interview Interview { get; set; }

    public CreateInterviewCommand(Interview interview)
    {
        Interview = interview ?? throw new ArgumentNullException(nameof(interview));
    }
}

public class UpdateInterviewCommand : BaseCommandBundle, IRequest<Result<Interview>>
{
    public Interview Interview { get; set; }

    public UpdateInterviewCommand(Interview interview)
    {
        Interview = interview ?? throw new ArgumentNullException(nameof(interview));
    }
}

public class DeleteInterviewCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    public InterviewID InterviewId { get; set; }

    public DeleteInterviewCommand(InterviewID interviewId)
    {
        InterviewId = interviewId ?? throw new ArgumentNullException(nameof(interviewId));
    }
}