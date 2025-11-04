namespace SMS_Domain.Interfaces;

public interface IReport
{
    ReportID Id { get; set; }
    string Code { get; set; }
    string? Name { get; set; }
    string? Description { get; set; }
    string? Status { get; set; }
    string? Stage { get; set; }
}
