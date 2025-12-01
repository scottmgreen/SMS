using SMS_Domain.Common;

namespace SMS_Domain.Entities;

/// <summary>
/// Represents a unique identifier for SMS User Role assignments
/// </summary>
public sealed class SMSApplicationUserRoleID : BaseID<string>
{
    public SMSApplicationUserRoleID(string value) : base(value) { }
    
    public static implicit operator string(SMSApplicationUserRoleID id) => id.Value;
    public static implicit operator SMSApplicationUserRoleID(string value) => new(value);
}