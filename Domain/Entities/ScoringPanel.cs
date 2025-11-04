namespace SMS_Domain.Entities;

public sealed class ScoringPanel : BaseAuditableEntity
{
    public ScoringPanel(ScoringPanelID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    public string? Code { get; set; }
    public string? HazardCode { get; set; }
    public string? SMSUserCode { get; set; }
    public string? Likelihood { get; set; }
    public string? Severity { get; set; }
    public string? Score { get; set; }
}