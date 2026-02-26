//-----------------------------------------------------------------------
// <copyright file="Error.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Comprehensive error catalog defining structured error handling for SMS domain operations with business-meaningful error codes and messages.
//                  Shared domain infrastructure providing base classes
//                  and common functionality for Domain-Driven Design.
// </copyright>
//-----------------------------------------------------------------------


namespace SMS_Domain.Common;


/// <summary>
/// Represents a concrete domain error.
/// </summary>
public sealed class Error : BaseValueObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Error"/> class.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    /// <summary>
    /// Gets the error code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the error message.
    /// </summary>
    public string Message { get; }

    public static implicit operator string(Error error) => error?.Code ?? string.Empty;

    /// <inheritdoc />
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Code;
        yield return Message;
    }

    /// <summary>
    /// Gets the empty error instance.
    /// </summary>
    internal static Error None => new Error(string.Empty, string.Empty);
}


