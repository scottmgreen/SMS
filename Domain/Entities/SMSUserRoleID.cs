using SMS_Domain.Common;

namespace SMS_Domain.Entities;

/// <summary>
/// Represents a unique identifier for SMS User Role assignments
/// </summary>
public sealed class SMSUserRoleID : BaseID<string>
{
    public SMSUserRoleID(string value) : base(value) { }
    
    public static implicit operator string(SMSUserRoleID id) => id.Value;
    public static implicit operator SMSUserRoleID(string value) => new(value);
}