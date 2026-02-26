//-----------------------------------------------------------------------
// <copyright file="SMSAuditChecklistItemID.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Strongly-typed identifier for smsauditchecklistitem entities ensuring type safety.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// Value object for SMS Audit Checklist Item identifier
/// </summary>
public class SMSAuditChecklistItemID : BaseID<string>
{
    public SMSAuditChecklistItemID(string value) : base(value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("SMS Audit Checklist Item ID cannot be null or empty", nameof(value));
    }
}
