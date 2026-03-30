//-----------------------------------------------------------------------
// <copyright file="BaseUserID.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Strongly-typed identifier for baseuser entities ensuring type safety.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// Represents a unique identifier for BaseUser entities
/// Base class for all user ID types in the SMS system
/// </summary>
public class BaseUserID : BaseID<string>
{
    public BaseUserID(string value) : base(value) { }

    public static implicit operator string(BaseUserID id) => id.Value;
    public static implicit operator BaseUserID(string value) => new(value);
}
