//-----------------------------------------------------------------------
// <copyright file="SMSStakeholderUserID.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Strongly-typed identifier for smsstakeholderuser entities ensuring type safety.
//                  Immutable value object encapsulating domain concepts with
//                  business logic and validation rules.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// SMS Stakeholder User unique identifier
/// </summary>
public sealed class SMSStakeholderUserID : BaseUserID
{
    public SMSStakeholderUserID(string id) : base(id) { }

    // Implicit conversion from string for convenience
    public static implicit operator SMSStakeholderUserID(string value) => new(value);
}
