namespace SMS_Domain.Entities;

public sealed class SMSUserRolePermission : BaseAuditableEntity
{
    public SMSUserRolePermission(SMSUserRolePermissionID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    public string? Code { get; set; }
    public string? SMSUserRoleCode { get; set; }
    public string? SMSModule { get; set; }
    public bool Create { get; set; }
    public bool Read { get; set; }
    public bool Update { get; set; }
    public bool Delete { get; set; }
}
