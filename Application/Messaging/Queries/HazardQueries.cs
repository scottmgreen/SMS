//-----------------------------------------------------------------------
// <copyright file="HazardQueries.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query definitions for SMS hazard data retrieval and analysis operations.
//                  Defines query objects for read operations in the CQRS pattern.
//                  Queries retrieve data without causing side effects.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Messaging.Queries;

// =============================================
// HAZARD QUERIES
// =============================================

public class GetHazardByCodeQuery : BaseQueryBundle, IRequest<Result<Hazard>>
{
    public HazardID HazardId { get; set; }

    public GetHazardByCodeQuery(HazardID hazardId)
    {
        HazardId = hazardId ?? throw new ArgumentNullException(nameof(hazardId));
    }
}

public class GetAllHazardsQuery : BaseQueryBundle, IRequest<Result<List<Hazard>>>
{
    public GetAllHazardsQuery()
    {
    }
}

public class GetHazardsByReportCodeQuery : BaseQueryBundle, IRequest<Result<List<Hazard>>>
{
    public ReportID ReportId { get; set; }

    public GetHazardsByReportCodeQuery(ReportID reportId)
    {
        ReportId = reportId ?? throw new ArgumentNullException(nameof(reportId));
    }
}


