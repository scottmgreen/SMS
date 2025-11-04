namespace SMS_Application.Messaging.Commands;

public class CreateReportValidationCommand : BaseCommandBundle, IRequest<Result<ReportValidation>>
{
    public ReportValidation ReportValidation { get; set; }

    public CreateReportValidationCommand(ReportValidation reportValidation)
    {
        ReportValidation = reportValidation ?? throw new ArgumentNullException(nameof(reportValidation));
    }
}

public class UpdateReportValidationCommand : BaseCommandBundle, IRequest<Result<ReportValidation>>
{
    public ReportValidation ReportValidation { get; set; }

    public UpdateReportValidationCommand(ReportValidation reportValidation)
    {
        ReportValidation = reportValidation ?? throw new ArgumentNullException(nameof(reportValidation));
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