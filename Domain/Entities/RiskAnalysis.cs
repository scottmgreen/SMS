namespace SMS_Domain.Entities;

public sealed class RiskAnalysis : BaseAuditableEntity
{
    public RiskAnalysis(RiskAnalysisID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    public string? Code { get; set; }
    public string? HazardCode { get; set; }
    public string? RiskAssessmentCode { get; set; }
    public string? WorstCredibleOutcome { get; set; }
    public string? RootCause { get; set; }
    public string? AdditionalComments { get; set; }
}
