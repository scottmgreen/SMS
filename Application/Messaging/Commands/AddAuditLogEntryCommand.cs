namespace SMS_Application.Messaging.Commands;

public class AddAuditLogEntryCommand : BaseCommandBundle, IRequest<Result<bool>>, ICreateCommand
{
    public AuditLogEntry AuditLogEntry { get; set; }

    public AddAuditLogEntryCommand(AuditLogEntry auditLogEntry)
    {
        AuditLogEntry = auditLogEntry ?? throw new ArgumentNullException(nameof(auditLogEntry));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // AuditLogEntry doesn't have CreatedBy/CreatedDate in the traditional sense
        // The UserID field serves as the "who created this" and EventDateTime as "when"
        if (string.IsNullOrEmpty(AuditLogEntry.UserID))
        {
            AuditLogEntry.UserID = userId;
        }
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        // For audit log entries, we typically don't set UpdatedBy
    }
}

