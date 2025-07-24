using System.Text.RegularExpressions;

namespace CBT3_Domain.ValueObjects;

public sealed class UPID : BaseValueObject
{
    /// <summary>
    /// The UPID maximum and minimum length.
    /// </summary>
    public const int RequiredLength = 7;
    

    /// <summary>
    /// Initializes a new instance of the <see cref="UPID"/> class.
    /// </summary>
    /// <param name="value">The first name value.</param>
    private UPID(string value) => Value = value;

    /// <summary>
    /// Gets the upid value.
    /// </summary>
    public string Value { get; }

    public static implicit operator string(UPID upid) => upid.Value;

    /// <summary>
    /// Creates a new <see cref="UPID"/> instance based on the specified value.
    /// </summary>
    /// <param name="upid">The upid value.</param>
    /// <returns>The result of the first name creation process containing the upid or an error.</returns>
    public static Result<UPID> Create(string upid) =>
        Result.Create(upid, DomainErrors.UPIDError.NullOrEmpty)
            .Ensure(f => !string.IsNullOrWhiteSpace(f), DomainErrors.UPIDError.NullOrEmpty)
            .Ensure(f => f.Length == RequiredLength, DomainErrors.UPIDError.RequiredLength)
            .Ensure(f => IsNumeric(f), DomainErrors.UPIDError.NonNumericCharacters)
            .Ensure(f => IsWithinRange(f), DomainErrors.UPIDError.OutOfAllowedRange)
            .Map(f => new UPID(f));

    public static Result<UPID> Create(string upid,UPID otherupid) =>
        Result.Create(upid, DomainErrors.UPIDError.NullOrEmpty)
            .Ensure(f => !string.IsNullOrWhiteSpace(f), DomainErrors.UPIDError.NullOrEmpty)
            .Ensure(f => f.Length == RequiredLength, DomainErrors.UPIDError.RequiredLength)
            .Ensure(f => IsNumeric(f), DomainErrors.UPIDError.NonNumericCharacters)
            .Ensure(f => IsWithinRange(f), DomainErrors.UPIDError.OutOfAllowedRange)
            .Ensure(f=> f.Equals(otherupid), DomainErrors.UPIDError.MismatchUPID)
            .Map(f => new UPID(f));

    protected static bool IsNumeric(string upid)
    {
        return Regex.IsMatch(upid, @"^\d+$");
    }

    protected static bool IsWithinRange(string upid)
    {
        if (int.TryParse(upid, out int upidValue))
        {
            return upidValue >= 1000000 && upidValue <= 9999999;
        }
        return false;
    }

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <inheritdoc />
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
