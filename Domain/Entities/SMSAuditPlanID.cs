namespace SMS_Domain.Entities;

/// <summary>
/// SMS Audit Plan unique identifier following the established ID pattern
/// </summary>
public class SMSAuditPlanID : BaseID<string>
{
    public SMSAuditPlanID(string id) : base(id) { }
}