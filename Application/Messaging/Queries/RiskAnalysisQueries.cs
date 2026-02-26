//-----------------------------------------------------------------------
// <copyright file="RiskAnalysisQueries.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query definitions for SMS risk assessment data retrieval and reporting.
//                  Defines query objects for read operations in the CQRS pattern.
//                  Queries retrieve data without causing side effects.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Messaging.Queries;

// =============================================
// RISK ANALYSIS QUERIES
// =============================================

public class GetRiskAnalysisByCodeQuery : BaseQueryBundle, IRequest<Result<RiskAnalysis>>
{
    public RiskAnalysisID RiskAnalysisCode { get; set; }

    public GetRiskAnalysisByCodeQuery(RiskAnalysisID code)
    {
        RiskAnalysisCode = code ?? throw new ArgumentNullException(nameof(code));
    }
}

public class GetRiskAnalysisByIdQuery : BaseQueryBundle, IRequest<Result<RiskAnalysis>>
{
    public RiskAnalysisID RiskAnalysisId { get; set; }

    public GetRiskAnalysisByIdQuery(RiskAnalysisID id)
    {
        RiskAnalysisId = id ?? throw new ArgumentNullException(nameof(id));
    }
}

public class GetRiskAnalysisByHazardCodeQuery : BaseQueryBundle, IRequest<Result<RiskAnalysis>>
{
    public HazardID HazardCode { get; set; }

    public GetRiskAnalysisByHazardCodeQuery(HazardID hazardcode)
    {
        HazardCode = hazardcode ?? throw new ArgumentNullException(nameof(hazardcode));
    }
}

public class GetRiskAnalysisByHazardAndAssessmentQuery : BaseQueryBundle, IRequest<Result<RiskAnalysis>>
{
    public string HazardCode { get; }
    public string RiskAssessmentCode { get; }

    public GetRiskAnalysisByHazardAndAssessmentQuery(string hazardCode, string riskAssessmentCode)
    {
        HazardCode = hazardCode ?? throw new ArgumentNullException(nameof(hazardCode));
        RiskAssessmentCode = riskAssessmentCode ?? throw new ArgumentNullException(nameof(riskAssessmentCode));
    }
}

public class GetAllRiskAnalysisQuery : BaseQueryBundle, IRequest<Result<List<RiskAnalysis>>>
{
    public GetAllRiskAnalysisQuery()
    {
    }
}
