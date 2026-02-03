namespace SMS_Application.Messaging.Queries;

// =============================================
// HAZARD QUERIES
// =============================================

public class GetHazardByCodeQuery : BaseQueryBundle, IRequest<Result<Hazard>>
{
    public HazardID HazardId { get; set; }

    public GetHazardByCodeQuery(HazardID hazardId)
    {
        HazardId = hazardId ?? throw new ArgumentNullException(nameof(hazardId));
    }
}

public class GetAllHazardsQuery : BaseQueryBundle, IRequest<Result<List<Hazard>>>
{
    public GetAllHazardsQuery()
    {
    }
}

public class GetHazardsByReportCodeQuery : BaseQueryBundle, IRequest<Result<List<Hazard>>>
{
    public ReportID ReportId { get; set; }

    public GetHazardsByReportCodeQuery(ReportID reportId)
    {
        ReportId = reportId ?? throw new ArgumentNullException(nameof(reportId));
    }
}

