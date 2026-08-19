using SMS_Domain.Enums;

namespace SMS_Application.Queries;

public class GetAllSMSCompaniesQuery : BaseQueryBundle, IRequest<Result<IEnumerable<SMSCompany>>>, IReadQuery
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
        return "SMSCompany:All";
    }

    public string GetAccessType()
    {
        return "GetAll";
    }
}

public class GetSMSCompanyByCodeQuery : BaseQueryBundle, IRequest<Result<SMSCompany>>, IReadQuery
{
    public string CompanyCode { get; set; }
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetSMSCompanyByCodeQuery(string companyCode)
    {
        if (string.IsNullOrWhiteSpace(companyCode))
            throw new ArgumentException("Company code cannot be null or empty", nameof(companyCode));

        CompanyCode = companyCode;
    }

    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"SMSCompany:Code:{CompanyCode}";
    }

    public string GetAccessType()
    {
        return "GetByCode";
    }
}
