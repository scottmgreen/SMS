namespace SMS_Application.Messaging.Commands;

public class CreateReportValidationCommand : BaseCommandBundle, IRequest<Result<ReportValidation>>, ICreateCommand
{
    public ReportValidation ReportValidation { get; set; }

    public CreateReportValidationCommand(ReportValidation reportValidation)
    {
        ReportValidation = reportValidation ?? throw new ArgumentNullException(nameof(reportValidation));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        ReportValidation.CreatedBy = userId;
        ReportValidation.CreatedDate = timestamp;
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        // For create commands, we typically don't set UpdatedBy
    }
}

public class UpdateReportValidationCommand : BaseCommandBundle, IRequest<Result<ReportValidation>>, IUpdateCommand
{
    public ReportValidation ReportValidation { get; set; }

    public UpdateReportValidationCommand(ReportValidation reportValidation)
    {
        ReportValidation = reportValidation ?? throw new ArgumentNullException(nameof(reportValidation));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // For update commands, we typically don't modify CreatedBy
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        ReportValidation.UpdatedBy = userId;
        ReportValidation.UpdatedDate = timestamp;
    }
}

public class DeleteReportValidationCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    public ReportValidationID ReportValidationId { get; set; }

    public DeleteReportValidationCommand(ReportValidationID reportValidationId)
    {
        ReportValidationId = reportValidationId ?? throw new ArgumentNullException(nameof(reportValidationId));
    }
}