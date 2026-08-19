using SMS_Domain.Enums;

namespace SMS_Application.Commands;

public class CreateSMSCompanyCommand : BaseCommandBundle, IRequest<Result<SMSCompany>>, ICreateCommand
{
    public SMSCompany Company { get; set; }
    public string CreatedBy { get; private set; } = string.Empty;

    public CreateSMSCompanyCommand(SMSCompany company)
    {
        Company = company ?? throw new ArgumentNullException(nameof(company));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        CreatedBy = userId;
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
    }
}

public class UpdateSMSCompanyCommand : BaseCommandBundle, IRequest<Result<SMSCompany>>, IUpdateCommand
{
    public SMSCompany Company { get; set; }
    public string UpdatedBy { get; private set; } = string.Empty;

    public UpdateSMSCompanyCommand(SMSCompany company)
    {
        Company = company ?? throw new ArgumentNullException(nameof(company));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        UpdatedBy = userId;
    }
}

public class DeleteSMSCompanyCommand : BaseCommandBundle, IRequest<Result<bool>>, IDeleteCommand
{
    public string CompanyCode { get; set; }
    public string DeletedBy { get; private set; } = string.Empty;

    public DeleteSMSCompanyCommand(string companyCode)
    {
        if (string.IsNullOrWhiteSpace(companyCode))
            throw new ArgumentException("Company code cannot be null or empty", nameof(companyCode));

        CompanyCode = companyCode;
    }

    public void SetDeletedBy(string userId, DateTime timestamp)
    {
        DeletedBy = userId;
    }
}
