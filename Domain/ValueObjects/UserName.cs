//-----------------------------------------------------------------------
// <copyright file="UserName.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Value object representing username with validation rules and business constraints.
//                  Immutable value object encapsulating domain concepts with
//                  business logic and validation rules.
// </copyright>
//-----------------------------------------------------------------------

using System.Text.RegularExpressions;

namespace SMS_Domain.ValueObjects;

/// <summary>
/// Represents a user name value object with validation
/// </summary>
public sealed class UserName : BaseValueObject
{
    public const int MaxLength = 50;
    public const int MinLength = 3;
    private static readonly Regex StakeholderUserNameFormatRegex = new(
        @"^s_[A-Za-z0-9]+\.[A-Za-z0-9]+_[A-Fa-f0-9]{8}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string Value { get; }

    private UserName(string value)
    {
        Value = value;
    }

    public static Result<UserName> Create(string userName) =>
        Result.Create(userName.Trim(), DomainErrors.UserNameError.NullOrEmpty)
            .Ensure(u => !string.IsNullOrWhiteSpace(u), DomainErrors.UserNameError.NullOrEmpty)
            .Ensure(u => u.Length >= MinLength, DomainErrors.UserNameError.TooShort)
            .Ensure(u => u.Length <= MaxLength, DomainErrors.UserNameError.TooLong)
            .Ensure(u => IsValidFormat(u), DomainErrors.UserNameError.InvalidFormat)
            .Map(u => new UserName(u.Trim().ToLowerInvariant()));

    /// <summary>
    /// Validates username format - alphanumeric, dots, hyphens, underscores, and @ symbol allowed
    /// </summary>
    private static bool IsValidFormat(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return false;

        // Allow alphanumeric characters, dots, hyphens, underscores, and @ symbol
        // Common for email-like usernames or simple usernames
        return userName.All(c => char.IsLetterOrDigit(c) ||
                                c == '.' ||
                                c == '-' ||
                                c == '_' ||
                                c == '@');
    }

    public static bool IsValidStakeholderUserNameFormat(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return false;
        }

        return StakeholderUserNameFormatRegex.IsMatch(userName.Trim());
    }

    /// <summary>
    /// Checks if the username appears to be an email address
    /// </summary>
    public bool IsEmail => Value.Contains('@') && Value.Contains('.');

    /// <summary>
    /// Gets the domain part if this is an email-style username
    /// </summary>
    public string? GetDomain()
    {
        if (!IsEmail) return null;
        var parts = Value.Split('@');
        return parts.Length == 2 ? parts[1] : null;
    }

    public static implicit operator string(UserName userName) => userName.Value;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
