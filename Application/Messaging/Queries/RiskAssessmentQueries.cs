namespace SMS_Application.Messaging.Queries;

// =============================================
// RISK ASSESSMENT QUERIES
// =============================================

public class GetRiskAssessmentByIdQuery : BaseQueryBundle, IRequest<Result<RiskAssessment>>
{
    public RiskAssessmentID RiskAssessmentId { get; set; }

    public GetRiskAssessmentByIdQuery(RiskAssessmentID riskAssessmentId)
    {
        RiskAssessmentId = riskAssessmentId ?? throw new ArgumentNullException(nameof(riskAssessmentId));
    }
}

public class GetAllRiskAssessmentsQuery : BaseQueryBundle, IRequest<Result<List<RiskAssessment>>>
{
    public GetAllRiskAssessmentsQuery()
    {
    }
}