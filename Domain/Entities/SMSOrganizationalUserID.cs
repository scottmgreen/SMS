using SMS_Domain.Common;

namespace SMS_Domain.Entities;

/// <summary>
/// Represents a unique identifier for SMS Organizational User entities
/// </summary>
public sealed class SMSOrganizationalUserID : BaseID<string>
{
    public SMSOrganizationalUserID(string value) : base(value) { }
    
    public static implicit operator string(SMSOrganizationalUserID id) => id.Value;
    public static implicit operator SMSOrganizationalUserID(string value) => new(value);
}