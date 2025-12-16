namespace SMS_Domain.Entities;

public sealed class ScoringPanel : BaseAuditableEntity
{
    public ScoringPanel(ScoringPanelID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    public string? Code { get; set; }
    public string? HazardCode { get; set; }
    public string? RiskAssessmentCode { get; set; }
    public string? SMSUserCode { get; set; }
    public int? Likelihood { get; set; } = 0;
    public int? Severity { get; set; } = 0;
    public decimal? Score { get; set; }
    public string? Rationale { get; set; }
}