namespace SMS_Domain.Entities;

/// <summary>
/// SMS Audit Evidence unique identifier following the established ID pattern
/// </summary>
public class SMSAuditEvidenceID : BaseID<string>
{
    public SMSAuditEvidenceID(string id) : base(id) { }
}