namespace SMS_Domain.Entities;

public sealed class Mitigation : BaseAuditableEntity
{
    public Mitigation(MitigationID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    public string? Code { get; set; }
    public string? HazardCode { get; set; }
}
