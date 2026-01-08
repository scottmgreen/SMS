namespace SMS_Domain.Entities;

/// <summary>
/// SMS Audit Finding unique identifier following the established ID pattern
/// </summary>
public class SMSAuditFindingID : BaseID<string>
{
    public SMSAuditFindingID(string id) : base(id) { }
}