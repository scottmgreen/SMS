using System.Text.RegularExpressions;

namespace SMS_Domain.ValueObjects;

public sealed class URL : BaseValueObject
{
    /// <summary>
    /// The URL maximum and minimum length.
    /// </summary>
    //public const int RequiredLength = 7;
    

    /// <summary>
    /// Initializes a new instance of the <see cref="UPID"/> class.
    /// </summary>
    /// <param name="value">The first name value.</param>
    private URL(string value) => Value = value;

    /// <summary>
    /// Gets the upid value.
    /// </summary>
    public string Value { get; }

    public static implicit operator string(URL url) => url.Value;

    /// <summary>
    /// Creates a new <see cref="UPID"/> instance based on the specified value.
    /// </summary>
    /// <param name="upid">The upid value.</param>
    /// <returns>The result of the first name creation process containing the upid or an error.</returns>
    //public static Result<URL> Create(string url) =>
    //    Result.Create(url, DomainErrors.URLError.NullOrEmpty)
    //        //.Ensure(f => !string.IsNullOrWhiteSpace(f), DomainErrors.UPIDError.NullOrEmpty)
    //        //.Ensure(f => f.Length == RequiredLength, DomainErrors.UPIDError.RequiredLength)
    //        .Ensure(f => IsUrl(f), DomainErrors.URLError.InValid)
    //        //.Ensure(f => IsWithinRange(f), DomainErrors.UPIDError.OutOfAllowedRange)
    //        .Map(f => new URL(f));


    public static Result<URL> Create(string url)
    {
        //no validation for now!
        return new URL(url);
    }


    //private static bool IsUrl(string url)
    //{
    //    return Regex.IsMatch(url, @"^(https?|ftp)://[^\s/$.?#].[^\s]*$");
    //} 

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <inheritdoc />
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
