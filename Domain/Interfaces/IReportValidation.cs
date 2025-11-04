namespace SMS_Domain.Interfaces;

public interface IReportValidation
{
    ReportValidationID Id { get; set; }
    string? Code { get; set; }
    string? ReportCode { get; set; }
    string? ValidationDecision { get; set; }
    string? Status { get; set; }
    string? Stage { get; set; }
}
