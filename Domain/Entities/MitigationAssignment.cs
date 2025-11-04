namespace SMS_Domain.Entities;

public sealed class MitigationAssignment : BaseAuditableEntity
{
    public MitigationAssignment(MitigationAssignmentID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    public string? Code { get; set; }
    public string? MitigationCode { get; set; }
    public string? DepartmentCode { get; set; }
}
