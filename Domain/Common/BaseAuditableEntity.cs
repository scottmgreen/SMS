//-----------------------------------------------------------------------
// <copyright file="BaseAuditableEntity.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Base entity class providing common functionality for SMS domain entities with identity and lifecycle management.
//                  Shared domain infrastructure providing base classes
//                  and common functionality for Domain-Driven Design.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Common;

public abstract class BaseAuditableEntity : BaseEntity, IAuditableEntity
{
    protected BaseAuditableEntity(BaseID<string> id, string createdBy, DateTime createdDate) : base(id)
    {
        CreatedBy = createdBy;
        CreatedDate = createdDate;
    }

    public string? CreatedBy { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
}

