namespace SMS_Domain.Entities;

public sealed class Hazard : BaseAuditableEntity
{
    public Hazard(HazardID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    public string Code { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string ReportCode { get; set; } = string.Empty;
    public string? ScoringPanelCode { get; set; }
    public string? AverageScore { get; set; }
}
