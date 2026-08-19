using SMS_Domain.Enums;

namespace SMS_Application.Commands;

public class CreateSMSJobTitleCommand : BaseCommandBundle, IRequest<Result<SMSJobTitle>>, ICreateCommand
{
    public SMSJobTitle JobTitle { get; set; }
    public string CreatedBy { get; private set; } = string.Empty;

    public CreateSMSJobTitleCommand(SMSJobTitle jobTitle)
    {
        JobTitle = jobTitle ?? throw new ArgumentNullException(nameof(jobTitle));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        CreatedBy = userId;
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
    }
}

public class UpdateSMSJobTitleCommand : BaseCommandBundle, IRequest<Result<SMSJobTitle>>, IUpdateCommand
{
    public string OriginalCode { get; set; }
    public SMSJobTitle JobTitle { get; set; }
    public string UpdatedBy { get; private set; } = string.Empty;

    public UpdateSMSJobTitleCommand(string originalCode, SMSJobTitle jobTitle)
    {
        if (string.IsNullOrWhiteSpace(originalCode))
            throw new ArgumentException("Title code cannot be null or empty", nameof(originalCode));

        OriginalCode = originalCode;
        JobTitle = jobTitle ?? throw new ArgumentNullException(nameof(jobTitle));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        UpdatedBy = userId;
    }
}

public class DeleteSMSJobTitleCommand : BaseCommandBundle, IRequest<Result<bool>>, IDeleteCommand
{
    public string JobTitleCode { get; set; }
    public string DeletedBy { get; private set; } = string.Empty;

    public DeleteSMSJobTitleCommand(string jobTitleCode)
    {
        if (string.IsNullOrWhiteSpace(jobTitleCode))
            throw new ArgumentException("Title code cannot be null or empty", nameof(jobTitleCode));

        JobTitleCode = jobTitleCode;
    }

    public void SetDeletedBy(string userId, DateTime timestamp)
    {
        DeletedBy = userId;
    }
}
