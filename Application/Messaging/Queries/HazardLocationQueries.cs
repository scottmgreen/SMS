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
// HAZARD LOCATION QUERIES WITH AUDIT TRACKING
// =============================================

public class GetHazardLocationByCodeQuery : BaseQueryBundle, IRequest<Result<HazardLocation>>, IReadQuery
{
    public HazardLocationID HazardLocationId { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetHazardLocationByCodeQuery(HazardLocationID hazardLocationId)
    {
        HazardLocationId = hazardLocationId ?? throw new ArgumentNullException(nameof(hazardLocationId));
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"HazardLocation:{HazardLocationId?.Value ?? "Unknown"}";
    }

    public string GetAccessType()
    {
        return "GetById";
    }
}

public class GetAllHazardLocationsQuery : BaseQueryBundle, IRequest<Result<List<HazardLocation>>>, IReadQuery
{
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetAllHazardLocationsQuery()
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
        return "HazardLocation:All";
    }

    public string GetAccessType()
    {
        return "GetAll";
    }
}

public class GetHazardLocationsByHazardCodeQuery : BaseQueryBundle, IRequest<Result<List<HazardLocation>>>, IReadQuery
{
    public string HazardCode { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetHazardLocationsByHazardCodeQuery(string hazardCode)
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
        return $"HazardLocation:ByHazard:{HazardCode}";
    }

    public string GetAccessType()
    {
        return "GetByHazard";
    }
}
