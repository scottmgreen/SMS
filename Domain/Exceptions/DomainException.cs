//-----------------------------------------------------------------------
// <copyright file="DomainException.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain-specific exception for SMS business rule violations and exceptional conditions requiring special handling.
//                  Domain-specific exception for business rule violations
//                  and exceptional conditions within the domain layer.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Exceptions;

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    { }
}

