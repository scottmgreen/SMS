//-----------------------------------------------------------------------
// <copyright file="IMitigationAssignment.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain contract defining operations and ensuring clean architecture boundaries for SMS business logic.
//                  Domain service contract defining business operations
//                  and ensuring clean architecture boundaries.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Interfaces;

public interface IMitigationAssignment
{
    MitigationAssignmentID Id { get; set; }
    string? Code { get; set; }
    string? MitigationCode { get; set; }
    string? DepartmentCode { get; set; }
}

