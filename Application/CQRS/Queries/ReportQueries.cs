//-----------------------------------------------------------------------
// <copyright file="ReportQueries.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query definitions for SMS report data retrieval and search operations.
//                  Defines query objects for read operations in the CQRS pattern.
//                  Queries retrieve data without causing side effects.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Application.Queries;

// =============================================
// REPORT QUERIES WITH AUDIT TRACKING
// =============================================

public class GetReportByCodeQuery : BaseQueryBundle, IRequest<Result<Report>>, IReadQuery
{
    public ReportID ReportCode { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetReportByCodeQuery(ReportID reportcode)
    {
        ReportCode = reportcode ?? throw new ArgumentNullException(nameof(reportcode));
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"Report:{ReportCode?.Value ?? "Unknown"}";
    }

    public string GetAccessType()
    {
        return this.GetType().Name.Replace("Query", ""); // GetReportByCode
    }
}

public class GetTrackingIDByReportCodeQuery : BaseQueryBundle, IRequest<Result<string>>, IReadQuery
{
    public ReportID ReportCode { get; set; }

    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetTrackingIDByReportCodeQuery(ReportID reportCode)
    {
        ReportCode = reportCode ?? throw new ArgumentNullException(nameof(reportCode));
    }

    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"ReportTracking:{ReportCode?.Value ?? "Unknown"}";
    }

    public string GetAccessType()
    {
        return this.GetType().Name.Replace("Query", string.Empty);
    }
}

public class GetAllReportsQuery : BaseQueryBundle, IRequest<Result<List<Report>>>, IReadQuery
{
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetAllReportsQuery()
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
        return "Report:All";
    }

    public string GetAccessType()
    {
        return this.GetType().Name.Replace("Query", ""); // GetAllReports
    }
}
