namespace SMS_Application.Messaging.Commands;

public class CreateReportCommand : BaseCommandBundle, IRequest<Result<Report>>
{
    public Report Report { get; set; }

    public CreateReportCommand(Report report)
    {
        Report = report ?? throw new ArgumentNullException(nameof(report));
    }
}

public class UpdateReportCommand : BaseCommandBundle, IRequest<Result<Report>>
{
    public Report Report { get; set; }

    public UpdateReportCommand(Report report)
    {
        Report = report ?? throw new ArgumentNullException(nameof(report));
    }
}

public class DeleteReportCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    public ReportID ReportId { get; set; }

    public DeleteReportCommand(ReportID reportId)
    {
        ReportId = reportId ?? throw new ArgumentNullException(nameof(reportId));
    }
}