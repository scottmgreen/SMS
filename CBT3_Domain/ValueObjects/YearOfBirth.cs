
using System.Text.RegularExpressions;

namespace CBT3_Domain.ValueObjects;

public sealed class YearOfBirth : BaseValueObject
{
    /// <summary>
    /// The UPID maximum and minimum length.
    /// </summary>
    public const int RequiredLength = 4;
    

    /// <summary>
    /// Initializes a new instance of the <see cref="FirstName"/> class.
    /// </summary>
    /// <param name="value">The first name value.</param>
    private YearOfBirth(string value) => Value = value;

    /// <summary>
    /// Gets the first name value.
    /// </summary>
    public string Value { get; }

    public static implicit operator string(YearOfBirth yearofbirth) => yearofbirth.Value;

    /// <summary>
    /// Creates a new <see cref="UPID"/> instance based on the specified value.
    /// </summary>
    /// <param name="upid">The first name value.</param>
    /// <returns>The result of the first name creation process containing the first name or an error.</returns>
    public static Result<YearOfBirth> Create(string upid) =>
        Result.Create(upid, DomainErrors.YearOfBirthError.NullOrEmpty)
            .Ensure(f => !string.IsNullOrWhiteSpace(f), DomainErrors.YearOfBirthError.NullOrEmpty)
            .Ensure(f => f.Length == RequiredLength, DomainErrors.YearOfBirthError.RequiredLength)
            .Ensure(f => IsValidYearOfBirth(f), DomainErrors.YearOfBirthError.OutOfRange)
            .Map(f => new YearOfBirth(f));


    public static Result<YearOfBirth> Create(string upid, YearOfBirth otheryearofbirth) =>
        Result.Create(upid, DomainErrors.YearOfBirthError.NullOrEmpty)
            .Ensure(f => !string.IsNullOrWhiteSpace(f), DomainErrors.YearOfBirthError.NullOrEmpty)
            .Ensure(f => f.Length == RequiredLength, DomainErrors.YearOfBirthError.RequiredLength)
            .Ensure(f => IsNumeric(f), DomainErrors.YearOfBirthError.NonNumericCharacters)
            .Ensure(f => IsValidYearOfBirth(f), DomainErrors.YearOfBirthError.OutOfRange)
            .Ensure(f => f.Equals(otheryearofbirth), DomainErrors.YearOfBirthError.MismatchYearOfBirth)
            .Map(f => new YearOfBirth(f));




    /// <inheritdoc />
    public override string ToString() => Value;

    /// <inheritdoc />
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
    protected static bool IsValidYearOfBirth(string yearOfBirth)
    {
        int year;

        // Check if the input can be parsed as an integer
        if (!int.TryParse(yearOfBirth, out year))
        {
            return false; // Not a valid integer
        }

        // Check if the year falls within a reasonable range (e.g., 1900 to current year)
        int currentYear = DateTime.Now.Year;
        const int minYear = 1900;

        if (year < minYear || year > currentYear)
        {
            return false; // Year is outside the valid range
        }

        return true; // Year is valid
    }

    protected static bool IsNumeric(string upid)
    {
        return Regex.IsMatch(upid, @"^\d+$");
    }
}
