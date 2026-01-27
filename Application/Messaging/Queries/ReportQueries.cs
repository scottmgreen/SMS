namespace SMS_Application.Messaging.Queries;

// =============================================
// REPORT QUERIES
// =============================================

public class GetReportByCodeQuery : BaseQueryBundle, IRequest<Result<Report>>
{
    public ReportID ReportCode { get; set; }

    public GetReportByCodeQuery(ReportID reportcode)
    {
        ReportCode = reportcode ?? throw new ArgumentNullException(nameof(reportcode));
    }
}

public class GetAllReportsQuery : BaseQueryBundle, IRequest<Result<List<Report>>>
{
    public GetAllReportsQuery()
    {
    }
}