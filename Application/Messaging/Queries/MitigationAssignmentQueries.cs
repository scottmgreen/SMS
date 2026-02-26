//-----------------------------------------------------------------------
// <copyright file="MitigationAssignmentQueries.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query definitions for read operations in the SMS CQRS architecture.
//                  Defines query objects for read operations in the CQRS pattern.
//                  Queries retrieve data without causing side effects.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Messaging.Queries;

// =============================================
// MITIGATION ASSIGNMENT QUERIES
// =============================================

public class GetMitigationAssignmentByIdQuery : BaseQueryBundle, IRequest<Result<MitigationAssignment>>
{
    public MitigationAssignmentID MitigationAssignmentId { get; set; }

    public GetMitigationAssignmentByIdQuery(MitigationAssignmentID mitigationAssignmentId)
    {
        MitigationAssignmentId = mitigationAssignmentId ?? throw new ArgumentNullException(nameof(mitigationAssignmentId));
    }
}

public class GetAllMitigationAssignmentsQuery : BaseQueryBundle, IRequest<Result<List<MitigationAssignment>>>
{
    public GetAllMitigationAssignmentsQuery()
    {
    }
}
