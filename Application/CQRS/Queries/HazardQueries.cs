//-----------------------------------------------------------------------
// <copyright file="HazardQueries.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query definitions for SMS hazard data retrieval and analysis operations.
//                  Defines query objects for read operations in the CQRS pattern.
//                  Queries retrieve data without causing side effects.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using SMS_Domain.Entities;

namespace SMS_Application.Queries;

// =============================================
// HAZARD QUERIES WITH AUDIT TRACKING
// =============================================

public class GetHazardByCodeQuery : BaseQueryBundle, IRequest<Result<Hazard>>, IReadQuery
{
    public HazardID HazardId { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetHazardByCodeQuery(HazardID hazardId)
    {
        HazardId = hazardId ?? throw new ArgumentNullException(nameof(hazardId));
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"Hazard:{HazardId?.Value ?? "Unknown"}";
    }

    public string GetAccessType()
    {
        return this.GetType().Name.Replace("Query", ""); // GetHazardByCode
    }
}

public class GetAllHazardsQuery : BaseQueryBundle, IRequest<Result<List<Hazard>>>, IReadQuery
{
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetAllHazardsQuery()
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
        return "Hazard:All";
    }

    public string GetAccessType()
    {
        return this.GetType().Name.Replace("Query", ""); // GetAllHazards
    }
}

public class GetHazardsByReportCodeQuery : BaseQueryBundle, IRequest<Result<List<Hazard>>>, IReadQuery
{
    public ReportID ReportId { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetHazardsByReportCodeQuery(ReportID reportId)
    {
        ReportId = reportId ?? throw new ArgumentNullException(nameof(reportId));
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"Hazard:ByReport:{ReportId?.Value ?? "Unknown"}";
    }

    public string GetAccessType()
    {
        return this.GetType().Name.Replace("Query", ""); // GetHazardsByReportCode
    }
}


