namespace SMS_Domain.Entities;

public sealed class ReportValidation : BaseAuditableEntity
{
    public ReportValidation(ReportValidationID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    public string? Code { get; set; }
    public string? ReportCode { get; set; }
    public string? ValidationDecision { get; set; }
    public string? Status { get; set; }
    public string? Stage { get; set; }
}
