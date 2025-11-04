namespace SMS_Domain.Interfaces;

public interface IScoringPanel
{
    ScoringPanelID Id { get; set; }
    string? Code { get; set; }
    string? HazardCode { get; set; }
    string? SMSUserCode { get; set; }
    string? Likelihood { get; set; }
    string? Severity { get; set; }
    string? Score { get; set; }
}