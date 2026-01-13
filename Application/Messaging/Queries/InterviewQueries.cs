namespace SMS_Application.Messaging.Queries;

// =============================================
// INTERVIEW QUERIES
// =============================================

public class GetInterviewByCodeQuery : BaseQueryBundle, IRequest<Result<Interview>>
{
    public InterviewID InterviewId { get; set; }

    public GetInterviewByCodeQuery(InterviewID interviewId)
    {
        InterviewId = interviewId ?? throw new ArgumentNullException(nameof(interviewId));
    }
}

public class GetAllInterviewsQuery : BaseQueryBundle, IRequest<Result<List<Interview>>>
{
    public GetAllInterviewsQuery()
    {
    }
}