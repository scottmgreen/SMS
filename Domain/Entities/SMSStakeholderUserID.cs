using SMS_Domain.Common;

namespace SMS_Domain.Entities;

/// <summary>
/// Represents a unique identifier for SMS Stakeholder User entities
/// </summary>
public sealed class SMSStakeholderUserID : BaseUserID
{
    public SMSStakeholderUserID(string id) : base(id) { }
    
    // Implicit conversion from string for convenience
    public static implicit operator SMSStakeholderUserID(string value) => new(value);
}