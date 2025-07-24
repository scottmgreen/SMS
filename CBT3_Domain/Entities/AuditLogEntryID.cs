namespace CBT3_Domain.Entities;
public class AuditLogEntryID : BaseID<string>
{
    public AuditLogEntryID(string id) : base(id) { }
}