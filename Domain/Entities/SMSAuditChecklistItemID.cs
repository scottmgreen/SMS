using SMS_Domain.Common;

namespace SMS_Domain.Entities;

/// <summary>
/// Value object for SMS Audit Checklist Item identifier
/// </summary>
public class SMSAuditChecklistItemID : BaseID<string>
{
    public SMSAuditChecklistItemID(string value) : base(value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("SMS Audit Checklist Item ID cannot be null or empty", nameof(value));
    }
}