//-----------------------------------------------------------------------
// <copyright file="AuditLogEntryID.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Strongly-typed identifier for auditlogentry entities ensuring type safety.
//                  Immutable value object encapsulating domain concepts with
//                  business logic and validation rules.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;
public class AuditLogEntryID : BaseID<string>
{
    public AuditLogEntryID(string id) : base(id) { }
}
