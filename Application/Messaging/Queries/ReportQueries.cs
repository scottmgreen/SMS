//-----------------------------------------------------------------------
// <copyright file="ReportQueries.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query definitions for SMS report data retrieval and search operations.
//                  Defines query objects for read operations in the CQRS pattern.
//                  Queries retrieve data without causing side effects.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Messaging.Queries;

// =============================================
// REPORT QUERIES
// =============================================

public class GetReportByCodeQuery : BaseQueryBundle, IRequest<Result<Report>>
{
    public ReportID ReportCode { get; set; }

    public GetReportByCodeQuery(ReportID reportcode)
    {
        ReportCode = reportcode ?? throw new ArgumentNullException(nameof(reportcode));
    }
}

public class GetAllReportsQuery : BaseQueryBundle, IRequest<Result<List<Report>>>
{
    public GetAllReportsQuery()
    {
    }
}
