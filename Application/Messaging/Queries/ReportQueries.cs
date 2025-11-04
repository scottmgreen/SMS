namespace SMS_Application.Messaging.Queries;

// =============================================
// REPORT QUERIES
// =============================================

public class GetReportByIdQuery : BaseQueryBundle, IRequest<Result<Report>>
{
    public ReportID ReportId { get; set; }

    public GetReportByIdQuery(ReportID reportId)
    {
        ReportId = reportId ?? throw new ArgumentNullException(nameof(reportId));
    }
}

public class GetAllReportsQuery : BaseQueryBundle, IRequest<Result<List<Report>>>
{
    public GetAllReportsQuery()
    {
    }
}