//-----------------------------------------------------------------------
// <copyright file="InvestigationQueries.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query definitions for SMS investigation data retrieval and status tracking.
//                  Defines query objects for read operations in the CQRS pattern.
//                  Queries retrieve data without causing side effects.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Messaging.Queries;

// =============================================
// INVESTIGATION QUERIES
// =============================================

public class GetInvestigationByCodeQuery : BaseQueryBundle, IRequest<Result<Investigation>>
{
    public InvestigationID InvestigationId { get; set; }

    public GetInvestigationByCodeQuery(InvestigationID code)
    {
        InvestigationId = code ?? throw new ArgumentNullException(nameof(code));
    }
}

public class GetAllInvestigationsQuery : BaseQueryBundle, IRequest<Result<List<Investigation>>>
{
    public GetAllInvestigationsQuery()
    {
    }
}
