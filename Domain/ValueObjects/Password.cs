using SMS_Domain.Common;
using SMS_Shared.Common;
using SMS_Domain.Errors;
using System.Security.Cryptography;
using System.Text;

namespace SMS_Domain.ValueObjects;

/// <summary>
/// Represents a password value object with validation and hashing
/// </summary>
public sealed class Password : BaseValueObject
{
    public const int MinLength = 8;
    public const int MaxLength = 128;

    public string HashedValue { get; }
    public DateTime CreatedDate { get; }
    public bool RequiresChange { get; }

    private Password(string hashedValue, DateTime createdDate, bool requiresChange = false)
    {
        HashedValue = hashedValue;
        CreatedDate = createdDate;
        RequiresChange = requiresChange;
    }

    /// <summary>
    /// Creates a new password from plaintext with validation and hashing
    /// </summary>
    public static Result<Password> Create(string plainTextPassword, bool requiresChange = false) =>
        Result.Create(plainTextPassword, DomainErrors.PasswordError.NullOrEmpty)
            .Ensure(p => !string.IsNullOrWhiteSpace(p), DomainErrors.PasswordError.NullOrEmpty)
            .Ensure(p => p.Length >= MinLength, DomainErrors.PasswordError.TooShort)
            .Ensure(p => p.Length <= MaxLength, DomainErrors.PasswordError.TooLong)
            .Ensure(p => HasUpperCase(p), DomainErrors.PasswordError.MissingUpperCase)
            .Ensure(p => HasLowerCase(p), DomainErrors.PasswordError.MissingLowerCase)
            .Ensure(p => HasDigit(p), DomainErrors.PasswordError.MissingDigit)
            .Ensure(p => HasSpecialChar(p), DomainErrors.PasswordError.MissingSpecialChar)
            .Map(p => new Password(HashPassword(p), DateTime.UtcNow, requiresChange));

    /// <summary>
    /// Creates a password object from an already hashed value (for loading from database)
    /// </summary>
    public static Password FromHash(string hashedValue, DateTime createdDate, bool requiresChange = false)
    {
        if (string.IsNullOrWhiteSpace(hashedValue))
            throw new ArgumentException("Hashed password cannot be null or empty.", nameof(hashedValue));

        return new Password(hashedValue, createdDate, requiresChange);
    }

    /// <summary>
    /// Verifies a plaintext password against this hashed password
    /// </summary>
    public bool Verify(string plainTextPassword)
    {
        if (string.IsNullOrWhiteSpace(plainTextPassword))
            return false;

        return BCrypt.Net.BCrypt.Verify(plainTextPassword, HashedValue);
    }

    /// <summary>
    /// Checks if the password has expired based on policy (90 days)
    /// </summary>
    public bool IsExpired(int maxAgeDays = 90)
    {
        return DateTime.UtcNow > CreatedDate.AddDays(maxAgeDays);
    }

    /// <summary>
    /// Gets the age of the password in days
    /// </summary>
    public int AgeDays => (int)(DateTime.UtcNow - CreatedDate).TotalDays;

    /// <summary>
    /// Creates a password that requires change on next login
    /// </summary>
    public static Result<Password> CreateTemporary(string plainTextPassword) =>
        Create(plainTextPassword, requiresChange: true);

    private static string HashPassword(string plainTextPassword)
    {
        // Use BCrypt with work factor 12 for strong security
        return BCrypt.Net.BCrypt.HashPassword(plainTextPassword, 12);
    }

    private static bool HasUpperCase(string password) => password.Any(char.IsUpper);
    private static bool HasLowerCase(string password) => password.Any(char.IsLower);
    private static bool HasDigit(string password) => password.Any(char.IsDigit);
    private static bool HasSpecialChar(string password) => password.Any(c => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(c));

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return HashedValue;
        yield return CreatedDate;
        yield return RequiresChange;
    }

    public override string ToString() => "[PROTECTED PASSWORD]";
}