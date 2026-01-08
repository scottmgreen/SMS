namespace SMS_Domain.Entities;

/// <summary>
/// SMS Audit unique identifier following the established ID pattern
/// </summary>
public class SMSAuditID : BaseID<string>
{
    public SMSAuditID(string id) : base(id) { }
}