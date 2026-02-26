//-----------------------------------------------------------------------
// <copyright file="IAuditableEntity.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain contract defining operations and ensuring clean architecture boundaries for SMS business logic.
//                  Domain service contract defining business operations
//                  and ensuring clean architecture boundaries.
// </copyright>
//-----------------------------------------------------------------------



namespace SMS_Domain.Interfaces;

public interface IAuditableEntity : IBaseEntity
{
    string? CreatedBy { get; set; }
    DateTime? CreatedDate { get; set; }
    string? UpdatedBy { get; set; }
    DateTime? UpdatedDate { get; set; }
}

