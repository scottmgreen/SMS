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
// RISK ASSESSMENT QUERIES WITH AUDIT TRACKING
// =============================================

public class GetRiskAssessmentByCodeQuery : BaseQueryBundle, IRequest<Result<RiskAssessment>>, IReadQuery
{
    public RiskAssessmentID RiskAssessmentId { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetRiskAssessmentByCodeQuery(RiskAssessmentID riskAssessmentId)
    {
        RiskAssessmentId = riskAssessmentId ?? throw new ArgumentNullException(nameof(riskAssessmentId));
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"RiskAssessment:{RiskAssessmentId?.Value ?? "Unknown"}";
    }

    public string GetAccessType()
    {
        return this.GetType().Name.Replace("Query", ""); // GetRiskAssessmentByCode
    }
}

public class GetRiskAssessmentsByHazardCodeQuery : BaseQueryBundle, IRequest<Result<List<RiskAssessment>>>, IReadQuery
{
    public HazardID HazardCode { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetRiskAssessmentsByHazardCodeQuery(HazardID hazardCode)
    {
        HazardCode = hazardCode ?? throw new ArgumentNullException(nameof(hazardCode));
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"RiskAssessment:ByHazard:{HazardCode?.Value ?? "Unknown"}";
    }

    public string GetAccessType()
    {
        return this.GetType().Name.Replace("Query", ""); // GetRiskAssessmentsByHazardCode
    }
}

public class GetAllRiskAssessmentsQuery : BaseQueryBundle, IRequest<Result<List<RiskAssessment>>>, IReadQuery
{
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetAllRiskAssessmentsQuery()
    {
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return "RiskAssessment:All";
    }

    public string GetAccessType()
    {
        return this.GetType().Name.Replace("Query", ""); // GetAllRiskAssessments
    }
}
