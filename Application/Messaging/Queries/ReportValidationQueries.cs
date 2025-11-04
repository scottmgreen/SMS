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