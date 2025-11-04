namespace SMS_Domain.Interfaces;

public interface IHazard
{
    HazardID Id { get; set; }
    string Code { get; set; }
    string? Name { get; set; }
    string? Description { get; set; }
    string ReportCode { get; set; }
    string? ScoringPanelCode { get; set; }
    string? AverageScore { get; set; }
}
