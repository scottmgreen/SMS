namespace SMS_Domain.Interfaces;

public interface IInterview
{
    InterviewID Id { get; set; }
    string? Code { get; set; }
    string? InvestigationCode { get; set; }
    string? SMSInvestigatorCode { get; set; }
    string? PersonInterviewed { get; set; }
    string? PersonInterviewedNotes { get; set; }
    string? InvestigatorNotes { get; set; }
}
