namespace SMS_Application.Messaging.Queries;

// =============================================
// REPORT VALIDATION QUERIES
// =============================================

public class GetReportValidationByIdQuery : BaseQueryBundle, IRequest<Result<ReportValidation>>
{
    public ReportValidationID ReportValidationId { get; set; }

    public GetReportValidationByIdQuery(ReportValidationID reportValidationId)
    {
        ReportValidationId = reportValidationId ?? throw new ArgumentNullException(nameof(reportValidationId));
    }
}

public class GetAllReportValidationsQuery : BaseQueryBundle, IRequest<Result<List<ReportValidation>>>
{
    public GetAllReportValidationsQuery()
    {
    }
}

public class GetReportValidationByReportIdQuery : BaseQueryBundle, IRequest<Result<ReportValidation>>
{
    public ReportID ReportId { get; set; }

    public GetReportValidationByReportIdQuery(ReportID reportId)
    {
        ReportId = reportId ?? throw new ArgumentNullException(nameof(reportId));
    }
}