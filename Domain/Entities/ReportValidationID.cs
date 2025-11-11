using SMS_Domain.Common;

namespace SMS_Domain.Entities;

/// <summary>
/// Unique identifier for Report Validation entities
/// </summary>
public sealed class ReportValidationID : BaseID<string>
{
    public ReportValidationID(string id) : base(id) { }

    //public static implicit operator ReportValidationID(string value) => new(value);
    //public static implicit operator string(ReportValidationID id) => id.Value;
}
