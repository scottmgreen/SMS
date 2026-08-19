using SMS_Domain.Enums;

namespace SMS_Application.Queries;

public class GetAllSMSJobTitlesQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSJobTitle>>>, IReadQuery
{
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return "SMSJobTitle:All";
    }

    public string GetAccessType()
    {
        return "GetAll";
    }
}

public class GetSMSJobTitleByCodeQuery : BaseQueryBundle, IRequest<Result<SMSJobTitle>>, IReadQuery
{
    public string JobTitleCode { get; set; }
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetSMSJobTitleByCodeQuery(string jobTitleCode)
    {
        if (string.IsNullOrWhiteSpace(jobTitleCode))
            throw new ArgumentException("Title code cannot be null or empty", nameof(jobTitleCode));

        JobTitleCode = jobTitleCode;
    }

    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"SMSJobTitle:Code:{JobTitleCode}";
    }

    public string GetAccessType()
    {
        return "GetByCode";
    }
}
