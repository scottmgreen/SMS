namespace SMS_Domain.Entities;

public sealed class Investigation : BaseAuditableEntity
{
    public Investigation(InvestigationID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    public string? Code { get; set; }
    public string? ReportCode { get; set; }
    public string? InvestigationNotes { get; set; }
}
