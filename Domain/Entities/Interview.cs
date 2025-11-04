namespace SMS_Domain.Entities;

public sealed class Interview : BaseAuditableEntity
{
    public Interview(InterviewID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    public string? Code { get; set; }
    public string? InvestigationCode { get; set; }
    public string? SMSInvestigatorCode { get; set; }
    public string? PersonInterviewed { get; set; }
    public string? PersonInterviewedNotes { get; set; }
    public string? InvestigatorNotes { get; set; }
}
