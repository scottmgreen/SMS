namespace SMS_Domain.Interfaces;

public interface IInvestigation
{
    InvestigationID Id { get; set; }
    string? Code { get; set; }
    string? ReportCode { get; set; }
    string? InvestigationNotes { get; set; }
}
