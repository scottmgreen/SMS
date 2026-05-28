//------------------------------------------------------------------------
// <copyright file="MitigationQueries.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query definitions for read operations in the SMS CQRS architecture.
//                  Defines query objects for read operations in the CQRS pattern.
//                  Queries retrieve data without causing side effects.
// </copyright>
//------------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Application.Queries;

// =============================================
// MITIGATION QUERIES WITH AUDIT TRACKING
// =============================================

public class GetMitigationByCodeQuery : BaseQueryBundle, IRequest<Result<Mitigation>>, IReadQuery
{
    public MitigationID MitigationId { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetMitigationByCodeQuery(MitigationID mitigationId)
    {
        MitigationId = mitigationId ?? throw new ArgumentNullException(nameof(mitigationId));
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"Mitigation:{MitigationId?.Value ?? "Unknown"}";
    }

    public string GetAccessType()
    {
        return this.GetType().Name.Replace("Query", ""); // GetMitigationByCode
    }
}

public class GetAllMitigationsQuery : BaseQueryBundle, IRequest<Result<List<Mitigation>>>, IReadQuery
{
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetAllMitigationsQuery()
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
        return "Mitigation:All";
    }

    public string GetAccessType()
    {
        return this.GetType().Name.Replace("Query", ""); // GetAllMitigations
    }
}

/// <summary>
/// NEW: Query to get all mitigations for a specific hazard
/// </summary>
public class GetMitigationsByHazardCodeQuery : BaseQueryBundle, IRequest<Result<List<Mitigation>>>, IReadQuery
{
    public string HazardCode { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetMitigationsByHazardCodeQuery(string hazardCode)
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
        return $"Mitigation:ByHazard:{HazardCode}";
    }

    public string GetAccessType()
    {
        return this.GetType().Name.Replace("Query", ""); // GetMitigationsByHazardCode
    }
}
