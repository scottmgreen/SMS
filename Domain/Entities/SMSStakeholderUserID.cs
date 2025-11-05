using SMS_Domain.Common;

namespace SMS_Domain.Entities;

/// <summary>
/// Represents a unique identifier for SMS Stakeholder User entities
/// </summary>
public sealed class SMSStakeholderUserID : BaseID<string>
{
    public SMSStakeholderUserID(string value) : base(value) { }
    
    public static implicit operator string(SMSStakeholderUserID id) => id.Value;
    public static implicit operator SMSStakeholderUserID(string value) => new(value);
}