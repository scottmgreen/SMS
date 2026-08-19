using SMS_Domain.Enums;

namespace SMS_Application.Commands;

public class CreateSMSOrganizationCommand : BaseCommandBundle, IRequest<Result<SMSOrganization>>, ICreateCommand
{
    public SMSOrganization Organization { get; set; }
    public string CreatedBy { get; private set; } = string.Empty;

    public CreateSMSOrganizationCommand(SMSOrganization organization)
    {
        Organization = organization ?? throw new ArgumentNullException(nameof(organization));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        CreatedBy = userId;
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
    }
}

public class UpdateSMSOrganizationCommand : BaseCommandBundle, IRequest<Result<SMSOrganization>>, IUpdateCommand
{
    public string OriginalCode { get; set; }
    public SMSOrganization Organization { get; set; }
    public string UpdatedBy { get; private set; } = string.Empty;

    public UpdateSMSOrganizationCommand(string originalCode, SMSOrganization organization)
    {
        if (string.IsNullOrWhiteSpace(originalCode))
            throw new ArgumentException("Organization code cannot be null or empty", nameof(originalCode));

        OriginalCode = originalCode;
        Organization = organization ?? throw new ArgumentNullException(nameof(organization));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        UpdatedBy = userId;
    }
}

public class DeleteSMSOrganizationCommand : BaseCommandBundle, IRequest<Result<bool>>, IDeleteCommand
{
    public string OrganizationCode { get; set; }
    public string DeletedBy { get; private set; } = string.Empty;

    public DeleteSMSOrganizationCommand(string organizationCode)
    {
        if (string.IsNullOrWhiteSpace(organizationCode))
            throw new ArgumentException("Organization code cannot be null or empty", nameof(organizationCode));

        OrganizationCode = organizationCode;
    }

    public void SetDeletedBy(string userId, DateTime timestamp)
    {
        DeletedBy = userId;
    }
}
