//-----------------------------------------------------------------------
// <copyright file="InvestigationQueries.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query definitions for SMS investigation data retrieval and status tracking.
//                  Defines query objects for read operations in the CQRS pattern.
//                  Queries retrieve data without causing side effects.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Application.Queries;

// =============================================
// INVESTIGATION QUERIES WITH AUDIT TRACKING
// =============================================

public class GetInvestigationByCodeQuery : BaseQueryBundle, IRequest<Result<Investigation>>, IReadQuery
{
    public InvestigationID InvestigationId { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetInvestigationByCodeQuery(InvestigationID code)
    {
        InvestigationId = code ?? throw new ArgumentNullException(nameof(code));
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"Investigation:{InvestigationId?.Value ?? "Unknown"}";
    }

    public string GetAccessType()
    {
        return "GetById";
    }
}

public class GetAllInvestigationsQuery : BaseQueryBundle, IRequest<Result<List<Investigation>>>, IReadQuery
{
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetAllInvestigationsQuery()
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
        return "Investigation:All";
    }

    public string GetAccessType()
    {
        return "GetAll";
    }
}
