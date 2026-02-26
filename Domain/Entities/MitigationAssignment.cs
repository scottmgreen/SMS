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

public sealed class MitigationAssignment : BaseAuditableEntity
{
    public MitigationAssignment(MitigationAssignmentID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    public string? Code { get; set; }
    public string? MitigationCode { get; set; }
    public string? DepartmentCode { get; set; }
}

