namespace SMS_Domain.Entities;

public sealed class RiskAssessment : BaseAuditableEntity
{
    public RiskAssessment(RiskAssessmentID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? HazardCode { get; set; }
    public string? AssessmentType { get; set; }
    public string? Status { get; set; }
    public string? Stage { get; set; }
}
