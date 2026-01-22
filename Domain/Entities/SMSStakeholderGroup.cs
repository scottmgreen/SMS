namespace SMS_Domain.Entities;

/// <summary>
/// Represents an SMS stakeholder group entity for organizing stakeholders into functional groups
/// </summary>
public sealed class SMSStakeholderGroup : BaseAuditableEntity
{
    public SMSStakeholderGroup(SMSStakeholderGroupID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    public string Code { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    public List<SMSStakeholderUser> GroupMembers { get; set; }
}
