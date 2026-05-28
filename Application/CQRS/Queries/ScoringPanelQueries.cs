//-----------------------------------------------------------------------
// <copyright file="ScoringPanelQueries.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query definitions for read operations in the SMS CQRS architecture.
//                  Defines query objects for read operations in the CQRS pattern.
//                  Queries retrieve data without causing side effects.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Application.Queries;

// =============================================
// SCORING PANEL QUERIES WITH AUDIT TRACKING
// =============================================

public class GetScoringPanelByIdQuery : BaseQueryBundle, IRequest<Result<ScoringPanel>>, IReadQuery
{
    public ScoringPanelID ScoringPanelId { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetScoringPanelByIdQuery(ScoringPanelID scoringPanelId)
    {
        ScoringPanelId = scoringPanelId ?? throw new ArgumentNullException(nameof(scoringPanelId));
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"ScoringPanel:{ScoringPanelId?.Value ?? "Unknown"}";
    }

    public string GetAccessType()
    {
        return "GetById";
    }
}

public class GetAllScoringPanelsQuery : BaseQueryBundle, IRequest<Result<List<ScoringPanel>>>, IReadQuery
{
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetAllScoringPanelsQuery()
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
        return "ScoringPanel:All";
    }

    public string GetAccessType()
    {
        return "GetAll";
    }
}

public class GetScoringPanelsByHazardCodeQuery : BaseQueryBundle, IRequest<Result<List<ScoringPanel>>>, IReadQuery
{
    public string HazardCode { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetScoringPanelsByHazardCodeQuery(string hazardCode)
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
        return $"ScoringPanel:ByHazard:{HazardCode}";
    }

    public string GetAccessType()
    {
        return "GetByHazard";
    }
}
