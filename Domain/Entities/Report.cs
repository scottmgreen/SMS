namespace SMS_Domain.Entities;

public sealed class Report : BaseAuditableEntity
{
    public Report(ReportID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    public string Code { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? Stage { get; set; }
}
