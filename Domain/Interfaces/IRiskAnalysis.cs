namespace SMS_Domain.Interfaces;

public interface IRiskAnalysis
{
    RiskAnalysisID Id { get; set; }
    string? Code { get; set; }
    string? Name { get; set; }
    string? Description { get; set; }
    string? HazardCode { get; set; }
    string? Status { get; set; }
    string? Stage { get; set; }
    string? WorstCredibleOutcome { get; set; }
    string? RootCause { get; set; }
}
