//-----------------------------------------------------------------------
// <copyright file="MitigationAssignmentQueries.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query definitions for read operations in the SMS CQRS architecture.
//                  Defines query objects for read operations in the CQRS pattern.
//                  Queries retrieve data without causing side effects.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Application.Messaging.Queries;

// =============================================
// MITIGATION ASSIGNMENT QUERIES WITH AUDIT TRACKING
// =============================================

public class GetMitigationAssignmentByIdQuery : BaseQueryBundle, IRequest<Result<MitigationAssignment>>, IReadQuery
{
    public MitigationAssignmentID MitigationAssignmentId { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetMitigationAssignmentByIdQuery(MitigationAssignmentID mitigationAssignmentId)
    {
        MitigationAssignmentId = mitigationAssignmentId ?? throw new ArgumentNullException(nameof(mitigationAssignmentId));
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"MitigationAssignment:{MitigationAssignmentId?.Value ?? "Unknown"}";
    }

    public string GetAccessType()
    {
        return "GetById";
    }
}

public class GetAllMitigationAssignmentsQuery : BaseQueryBundle, IRequest<Result<List<MitigationAssignment>>>, IReadQuery
{
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetAllMitigationAssignmentsQuery()
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
        return "MitigationAssignment:All";
    }

    public string GetAccessType()
    {
        return "GetAll";
    }
}
