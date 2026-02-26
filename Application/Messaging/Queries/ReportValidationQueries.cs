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
// REPORT VALIDATION QUERIES
// =============================================

public class GetReportValidationByIdQuery : BaseQueryBundle, IRequest<Result<ReportValidation>>
{
    public ReportValidationID ReportValidationId { get; set; }

    public GetReportValidationByIdQuery(ReportValidationID reportValidationId)
    {
        ReportValidationId = reportValidationId ?? throw new ArgumentNullException(nameof(reportValidationId));
    }
}

public class GetAllReportValidationsQuery : BaseQueryBundle, IRequest<Result<List<ReportValidation>>>
{
    public GetAllReportValidationsQuery()
    {
    }
}

public class GetReportValidationByReportIdQuery : BaseQueryBundle, IRequest<Result<ReportValidation>>
{
    public ReportID ReportId { get; set; }

    public GetReportValidationByReportIdQuery(ReportID reportId)
    {
        ReportId = reportId ?? throw new ArgumentNullException(nameof(reportId));
    }
}
