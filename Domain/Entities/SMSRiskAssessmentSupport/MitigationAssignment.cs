//-----------------------------------------------------------------------
// <copyright file="MitigationAssignment.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS mitigation entity representing mitigationassignment for risk mitigation strategies.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// Mitigation Assignment entity for managing mitigation task assignments and tracking
/// </summary>
public class MitigationAssignment : BaseAuditableEntity
{
    public MitigationAssignment(MitigationAssignmentID id) : base(id, string.Empty, DateTime.UtcNow) { }

    public string? Code { get; set; }
    public string? MitigationCode { get; set; }
    public string? DepartmentCode { get; set; }
}

