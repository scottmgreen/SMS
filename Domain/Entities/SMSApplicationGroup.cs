namespace SMS_Domain.Entities;

public sealed class SMSApplicationGroup : BaseAuditableEntity
{
    public SMSApplicationGroup(SMSApplicationGroupID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    public string Code { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    public List<SMSApplicationUser> GroupMembers { get; set; }
}
