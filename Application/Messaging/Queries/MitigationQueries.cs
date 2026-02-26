//-----------------------------------------------------------------------
// <copyright file="MitigationQueries.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query definitions for read operations in the SMS CQRS architecture.
//                  Defines query objects for read operations in the CQRS pattern.
//                  Queries retrieve data without causing side effects.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Messaging.Queries;

// =============================================
// MITIGATION QUERIES
// =============================================

public class GetMitigationByCodeQuery : BaseQueryBundle, IRequest<Result<Mitigation>>
{
    public MitigationID MitigationId { get; set; }

    public GetMitigationByCodeQuery(MitigationID mitigationId)
    {
        MitigationId = mitigationId ?? throw new ArgumentNullException(nameof(mitigationId));
    }
}

public class GetAllMitigationsQuery : BaseQueryBundle, IRequest<Result<List<Mitigation>>>
{
    public GetAllMitigationsQuery()
    {
    }
}

/// <summary>
/// NEW: Query to get all mitigations for a specific hazard
/// </summary>
public class GetMitigationsByHazardCodeQuery : BaseQueryBundle, IRequest<Result<List<Mitigation>>>
{
    public string HazardCode { get; set; }

    public GetMitigationsByHazardCodeQuery(string hazardCode)
    {
        HazardCode = hazardCode ?? throw new ArgumentNullException(nameof(hazardCode));
    }
}
