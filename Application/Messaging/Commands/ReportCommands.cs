using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Application.Common;
using SMS_Application.Interfaces;
using SMS_Shared.Common;

namespace SMS_Application.Messaging.Commands;

public class CreateReportCommand : BaseCommandBundle, IRequest<Result<Report>>, ICreateCommand
{
    public Report Report { get; set; }

    public CreateReportCommand(Report report)
    {
        Report = report ?? throw new ArgumentNullException(nameof(report));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        Report.CreatedBy = userId;
        Report.CreatedDate = timestamp;
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        // For create commands, we typically don't set UpdatedBy
    }
}

public class UpdateReportCommand : BaseCommandBundle, IRequest<Result<Report>>, IUpdateCommand
{
    public Report Report { get; set; }

    public UpdateReportCommand(Report report)
    {
        Report = report ?? throw new ArgumentNullException(nameof(report));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // For update commands, we typically don't modify CreatedBy
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        Report.UpdatedBy = userId;
        Report.UpdatedDate = timestamp;
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