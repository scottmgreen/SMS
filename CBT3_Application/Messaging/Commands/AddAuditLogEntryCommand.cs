namespace CBT3_Application.Messaging;


    public class AddAuditLogEntryCommand : BaseCommandBundle, IRequest<Result<bool>>
    {
        public AuditLogEntry AuditLogEntry { get; set; }

    }

