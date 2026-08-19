using SMS_Domain.Enums;

namespace SMS_Application.Queries;

public class GetAllSMSOrganizationsQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSOrganization>>>, IReadQuery
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
        return "SMSOrganization:All";
    }

    public string GetAccessType()
    {
        return "GetAll";
    }
}

public class GetSMSOrganizationByCodeQuery : BaseQueryBundle, IRequest<Result<SMSOrganization>>, IReadQuery
{
    public string OrganizationCode { get; set; }
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetSMSOrganizationByCodeQuery(string organizationCode)
    {
        if (string.IsNullOrWhiteSpace(organizationCode))
            throw new ArgumentException("Organization code cannot be null or empty", nameof(organizationCode));

        OrganizationCode = organizationCode;
    }

    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"SMSOrganization:Code:{OrganizationCode}";
    }

    public string GetAccessType()
    {
        return "GetByCode";
    }
}
