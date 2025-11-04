namespace SMS_Domain.Interfaces;

public interface IRiskAssessment
{
    RiskAssessmentID Id { get; set; }
    string? Code { get; set; }
    string? Name { get; set; }
    string? Description { get; set; }
    string? HazardCode { get; set; }
    string? AssessmentType { get; set; }
    string? Status { get; set; }
    string? Stage { get; set; }
}
