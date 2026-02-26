//-----------------------------------------------------------------------
// <copyright file="RiskAssessmentQueries.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query definitions for SMS risk assessment data retrieval and reporting.
//                  Defines query objects for read operations in the CQRS pattern.
//                  Queries retrieve data without causing side effects.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Messaging.Queries;

// =============================================
// RISK ASSESSMENT QUERIES
// =============================================

public class GetRiskAssessmentByCodeQuery : BaseQueryBundle, IRequest<Result<RiskAssessment>>
{
    public RiskAssessmentID RiskAssessmentId { get; set; }

    public GetRiskAssessmentByCodeQuery(RiskAssessmentID riskAssessmentId)
    {
        RiskAssessmentId = riskAssessmentId ?? throw new ArgumentNullException(nameof(riskAssessmentId));
    }
}
public class GetRiskAssessmentsByHazardCodeQuery : BaseQueryBundle, IRequest<Result<List<RiskAssessment>>>
{
    public HazardID HazardCode { get; set; }

    public GetRiskAssessmentsByHazardCodeQuery(HazardID hazardCode)
    {
        HazardCode = hazardCode ?? throw new ArgumentNullException(nameof(hazardCode));
    }
}
public class GetAllRiskAssessmentsQuery : BaseQueryBundle, IRequest<Result<List<RiskAssessment>>>
{
    public GetAllRiskAssessmentsQuery()
    {
    }
}
