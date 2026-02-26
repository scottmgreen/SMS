//-----------------------------------------------------------------------
// <copyright file="SMSAuditEvidenceID.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Strongly-typed identifier for smsauditevidence entities ensuring type safety.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// SMS Audit Evidence unique identifier following the established ID pattern
/// </summary>
public class SMSAuditEvidenceID : BaseID<string>
{
    public SMSAuditEvidenceID(string id) : base(id) { }
}
