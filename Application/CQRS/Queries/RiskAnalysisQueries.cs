//-----------------------------------------------------------------------
// <copyright file="RiskAnalysisQueries.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query definitions for SMS risk assessment data retrieval and reporting.
//                  Defines query objects for read operations in the CQRS pattern.
//                  Queries retrieve data without causing side effects.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Application.Queries;

// =============================================
// RISK ANALYSIS QUERIES WITH AUDIT TRACKING
// =============================================

public class GetRiskAnalysisByCodeQuery : BaseQueryBundle, IRequest<Result<RiskAnalysis>>, IReadQuery
{
    public RiskAnalysisID RiskAnalysisCode { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetRiskAnalysisByCodeQuery(RiskAnalysisID code)
    {
        RiskAnalysisCode = code ?? throw new ArgumentNullException(nameof(code));
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"RiskAnalysis:{RiskAnalysisCode?.Value ?? "Unknown"}";
    }

    public string GetAccessType()
    {
        return "GetByCode";
    }
}

public class GetRiskAnalysisByIdQuery : BaseQueryBundle, IRequest<Result<RiskAnalysis>>, IReadQuery
{
    public RiskAnalysisID RiskAnalysisId { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetRiskAnalysisByIdQuery(RiskAnalysisID id)
    {
        RiskAnalysisId = id ?? throw new ArgumentNullException(nameof(id));
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"RiskAnalysis:{RiskAnalysisId?.Value ?? "Unknown"}";
    }

    public string GetAccessType()
    {
        return "GetById";
    }
}

public class GetRiskAnalysisByHazardCodeQuery : BaseQueryBundle, IRequest<Result<RiskAnalysis>>, IReadQuery
{
    public HazardID HazardCode { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetRiskAnalysisByHazardCodeQuery(HazardID hazardcode)
    {
        HazardCode = hazardcode ?? throw new ArgumentNullException(nameof(hazardcode));
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"RiskAnalysis:ByHazard:{HazardCode?.Value ?? "Unknown"}";
    }

    public string GetAccessType()
    {
        return "GetByHazard";
    }
}

public class GetRiskAnalysisByHazardAndAssessmentQuery : BaseQueryBundle, IRequest<Result<RiskAnalysis>>, IReadQuery
{
    public string HazardCode { get; }
    public string RiskAssessmentCode { get; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetRiskAnalysisByHazardAndAssessmentQuery(string hazardCode, string riskAssessmentCode)
    {
        HazardCode = hazardCode ?? throw new ArgumentNullException(nameof(hazardCode));
        RiskAssessmentCode = riskAssessmentCode ?? throw new ArgumentNullException(nameof(riskAssessmentCode));
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"RiskAnalysis:ByHazardAndAssessment:{HazardCode}:{RiskAssessmentCode}";
    }

    public string GetAccessType()
    {
        return "GetByHazardAndAssessment";
    }
}

public class GetAllRiskAnalysisQuery : BaseQueryBundle, IRequest<Result<List<RiskAnalysis>>>, IReadQuery
{
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetAllRiskAnalysisQuery()
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
        return "RiskAnalysis:All";
    }

    public string GetAccessType()
    {
        return "GetAll";
    }
}
