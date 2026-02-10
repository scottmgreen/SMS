namespace SMS_Domain.Entities;

/// <summary>
/// Represents an SMS stakeholder user entity for external partners, airlines, contractors, etc.
/// </summary>
public sealed class SMSStakeholderUser : BaseUser
{
    // Simple constructors

    public SMSStakeholderUser(SMSStakeholderUserID id) : base(id, "SYSTEM", DateTime.UtcNow)
    {
        StakeholderUserId = id;
    }

    // Simple properties with public setters
    public SMSStakeholderUserID StakeholderUserId { get; set; }
    public string StakeholderType { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;

    public bool IsPOPEmployee { get; set; }
    


}