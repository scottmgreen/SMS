using SMS_Domain.Common;

namespace SMS_Domain.Entities;

/// <summary>
/// Represents a unique identifier for SMS Application User entities
/// </summary>
public sealed class SMSApplicationUserID : BaseUserID
{
    public SMSApplicationUserID(string id) : base(id) { }
    
    // Implicit conversion from string for convenience
    public static implicit operator SMSApplicationUserID(string value) => new(value);
}