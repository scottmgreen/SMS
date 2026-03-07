//-----------------------------------------------------------------------
// <copyright file="ReportValidationQueries.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query definitions for SMS report data retrieval and search operations.
//                  Defines query objects for read operations in the CQRS pattern.
//                  Queries retrieve data without causing side effects.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Messaging.Queries;

// =============================================
// REPORT VALIDATION QUERIES WITH AUDIT TRACKING
// =============================================

public class GetReportValidationByIdQuery : BaseQueryBundle, IRequest<Result<ReportValidation>>, IReadQuery
{
    public ReportValidationID ReportValidationId { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetReportValidationByIdQuery(ReportValidationID reportValidationId)
    {
        ReportValidationId = reportValidationId ?? throw new ArgumentNullException(nameof(reportValidationId));
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"ReportValidation:{ReportValidationId?.Value ?? "Unknown"}";
    }

    public string GetAccessType()
    {
        return "GetById";
    }
}

public class GetAllReportValidationsQuery : BaseQueryBundle, IRequest<Result<List<ReportValidation>>>, IReadQuery
{
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetAllReportValidationsQuery()
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
        return "ReportValidation:All";
    }

    public string GetAccessType()
    {
        return "GetAll";
    }
}

public class GetReportValidationByReportIdQuery : BaseQueryBundle, IRequest<Result<ReportValidation>>, IReadQuery
{
    public ReportID ReportId { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetReportValidationByReportIdQuery(ReportID reportId)
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
        return $"ReportValidation:ByReport:{ReportId?.Value ?? "Unknown"}";
    }

    public string GetAccessType()
    {
        return "GetByReport";
    }
}
