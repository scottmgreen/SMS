using SMS_Domain.Common;

namespace SMS_Domain.Entities;

/// <summary>
/// Represents a unique identifier for SMS Application User entities
/// </summary>
public sealed class SMSApplicationUserID : BaseID<string>
{
    public SMSApplicationUserID(string id) : base(id) { }
}