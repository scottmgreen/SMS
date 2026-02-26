//-----------------------------------------------------------------------
// <copyright file="HazardLocationQueries.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query definitions for SMS hazard data retrieval and analysis operations.
//                  Defines query objects for read operations in the CQRS pattern.
//                  Queries retrieve data without causing side effects.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Messaging.Queries;

// =============================================
// HAZARD LOCATION QUERIES
// =============================================

public class GetHazardLocationByCodeQuery : BaseQueryBundle, IRequest<Result<HazardLocation>>
{
    public HazardLocationID HazardLocationId { get; set; }

    public GetHazardLocationByCodeQuery(HazardLocationID hazardLocationId)
    {
        HazardLocationId = hazardLocationId ?? throw new ArgumentNullException(nameof(hazardLocationId));
    }
}

public class GetAllHazardLocationsQuery : BaseQueryBundle, IRequest<Result<List<HazardLocation>>>
{
    public GetAllHazardLocationsQuery()
    {
    }
}

public class GetHazardLocationsByHazardCodeQuery : BaseQueryBundle, IRequest<Result<List<HazardLocation>>>
{
    public string HazardCode { get; set; }

    public GetHazardLocationsByHazardCodeQuery(string hazardCode)
    {
        HazardCode = hazardCode ?? throw new ArgumentNullException(nameof(hazardCode));
    }
}
