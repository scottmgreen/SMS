//-----------------------------------------------------------------------
// <copyright file="FirstName.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Value object representing firstname with validation rules and business constraints.
//                  Immutable value object encapsulating domain concepts with
//                  business logic and validation rules.
// </copyright>
//-----------------------------------------------------------------------


using System.Text.RegularExpressions;

namespace SMS_Domain.ValueObjects;






/// <summary>
/// Represents the first name value object.
/// </summary>
public sealed class FirstName : BaseValueObject
{
    /// <summary>
    /// The first name maximum length.
    /// </summary>
    public const int MaxLength = 100;

    /// <summary>
    /// Initializes a new instance of the <see cref="FirstName"/> class.
    /// </summary>
    /// <param name="value">The first name value.</param>
    private FirstName(string value) => Value = value;

    /// <summary>
    /// Gets the first name value.
    /// </summary>
    public string Value { get; }

    public static implicit operator string(FirstName firstName) => firstName.Value;

    /// <summary>
    /// Creates a new <see cref="FirstName"/> instance based on the specified value.
    /// </summary>
    /// <param name="firstName">The first name value.</param>
    /// <returns>The result of the first name creation process containing the first name or an error.</returns>
    public static Result<FirstName> Create(string firstName) =>
        Result.Create(firstName.Trim(), DomainErrors.FirstNameError.NullOrEmpty)
            .Ensure(f => !string.IsNullOrWhiteSpace(f), DomainErrors.FirstNameError.NullOrEmpty)
            .Ensure(f => f.Length <= MaxLength, DomainErrors.FirstNameError.LongerThanAllowed)
            .Ensure(f => Regex.IsMatch(f, @"^[a-zA-Z ]+$"), DomainErrors.FirstNameError.ContainsSpecialCharactersOrNumbers)
            .Map(f => new FirstName(f));

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <inheritdoc />
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}


