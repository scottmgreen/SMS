using SMS_Domain.Common;

namespace SMS_Domain.Entities;

/// <summary>
/// Represents a unique identifier for SMS Application User entities
/// </summary>
public sealed class SMSApplicationUserID : BaseID<string>
{
    public SMSApplicationUserID(string value) : base(value) { }
    
    public static implicit operator string(SMSApplicationUserID id) => id.Value;
    public static implicit operator SMSApplicationUserID(string value) => new(value);
}