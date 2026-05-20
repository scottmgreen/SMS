//-----------------------------------------------------------------------
// <copyright file="SMSAuditID.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Strongly-typed identifier for smsaudit entities ensuring type safety.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// SMS Audit unique identifier following the established ID pattern
/// </summary>
public class SMSEventID : BaseID<string>
{
    public SMSEventID(string id) : base(id) { }
}
