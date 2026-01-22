namespace SMS_Application.Messaging.Commands;

public class CreateInterviewCommand : BaseCommandBundle, IRequest<Result<Interview>>, ICreateCommand
{
    public Interview Interview { get; set; }

    public CreateInterviewCommand(Interview interview)
    {
        Interview = interview ?? throw new ArgumentNullException(nameof(interview));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        Interview.CreatedBy = userId;
        Interview.CreatedDate = timestamp;
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        // For create commands, we typically don't set UpdatedBy
    }
}

public class UpdateInterviewCommand : BaseCommandBundle, IRequest<Result<Interview>>, IUpdateCommand
{
    public Interview Interview { get; set; }

    public UpdateInterviewCommand(Interview interview)
    {
        Interview = interview ?? throw new ArgumentNullException(nameof(interview));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // For update commands, we typically don't modify CreatedBy
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        Interview.UpdatedBy = userId;
        Interview.UpdatedDate = timestamp;
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