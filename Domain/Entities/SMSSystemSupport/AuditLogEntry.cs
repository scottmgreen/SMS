//-----------------------------------------------------------------------
// <copyright file="AuditLogEntry.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS audit log entry entity representing auditlogentry for system audit and tracking.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

public class AuditLogEntry : BaseEntity
{
    public AuditLogEntry(AuditLogEntryID id) : base(id)
    {
    }
    public string UserID { get; set; }
    public string Workstation { get; set; }
    public string EventDateTime { get; set; }
    public string MessageType { get; set; }
    public string Severity { get; set; }
    public string Module { get; set; }
    public string Function { get; set; }
    public string Description { get; set; }


}

