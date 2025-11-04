namespace SMS_Application.Messaging.Queries;

// =============================================
// RISK ANALYSIS QUERIES
// =============================================

public class GetRiskAnalysisByIdQuery : BaseQueryBundle, IRequest<Result<RiskAnalysis>>
{
    public RiskAnalysisID RiskAnalysisId { get; set; }

    public GetRiskAnalysisByIdQuery(RiskAnalysisID riskAnalysisId)
    {
        RiskAnalysisId = riskAnalysisId ?? throw new ArgumentNullException(nameof(riskAnalysisId));
    }
}

public class GetAllRiskAnalysisQuery : BaseQueryBundle, IRequest<Result<List<RiskAnalysis>>>
{
    public GetAllRiskAnalysisQuery()
    {
    }
}