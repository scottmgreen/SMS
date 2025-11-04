namespace SMS_Domain.Entities;

public sealed class RiskAnalysis : BaseAuditableEntity
{
    public RiskAnalysis(RiskAnalysisID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? HazardCode { get; set; }
    public string? Status { get; set; }
    public string? Stage { get; set; }
    public string? WorstCredibleOutcome { get; set; }
    public string? RootCause { get; set; }
}
