using SMS_Application.Common;
using SMS_Application.Interfaces;
using SMS_Domain.Entities;
using SMS_Shared.Common;

namespace SMS_Application.Messaging.Commands;

/// <summary>
/// SMS Audit Commands for CQRS pattern
/// </summary>

// CREATE SMS AUDIT (Manual Creation)
public class CreateSMSAuditCommand : BaseCommandBundle, IRequest<Result<SMSAudit>>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string AuditType { get; set; }
    public DateTime ScheduledStartDate { get; set; }
    public DateTime ScheduledEndDate { get; set; }
    public string LeadAuditor { get; set; }
    public string ResponsibleDepartment { get; set; }
    public string CreatedBy { get; set; }

    public CreateSMSAuditCommand(string name, string description, string auditType,
        DateTime scheduledStartDate, DateTime scheduledEndDate, string leadAuditor,
        string responsibleDepartment, string createdBy)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? string.Empty;
        AuditType = auditType ?? throw new ArgumentNullException(nameof(auditType));
        ScheduledStartDate = scheduledStartDate;
        ScheduledEndDate = scheduledEndDate;
        LeadAuditor = leadAuditor ?? throw new ArgumentNullException(nameof(leadAuditor));
        ResponsibleDepartment = responsibleDepartment ?? string.Empty;
        CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));
    }
}

// UPDATE SMS AUDIT
public class UpdateSMSAuditCommand : BaseCommandBundle, IRequest<Result<SMSAudit>>
{
    public string AuditCode { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime ScheduledStartDate { get; set; }
    public DateTime ScheduledEndDate { get; set; }
    public string AuditLocation { get; set; }
    public string ContactPerson { get; set; }
    public string UpdatedBy { get; set; }

    public UpdateSMSAuditCommand(string auditCode, string name, string description,
        DateTime scheduledStartDate, DateTime scheduledEndDate, string auditLocation,
        string contactPerson, string updatedBy)
    {
        AuditCode = auditCode ?? throw new ArgumentNullException(nameof(auditCode));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? string.Empty;
        ScheduledStartDate = scheduledStartDate;
        ScheduledEndDate = scheduledEndDate;
        AuditLocation = auditLocation ?? string.Empty;
        ContactPerson = contactPerson ?? string.Empty;
        UpdatedBy = updatedBy ?? throw new ArgumentNullException(nameof(updatedBy));
    }
}

// START SMS AUDIT
public class StartSMSAuditCommand : BaseCommandBundle, IRequest<Result<SMSAudit>>
{
    public string AuditCode { get; set; }
    public string StartedBy { get; set; }

    public StartSMSAuditCommand(string auditCode, string startedBy)
    {
        AuditCode = auditCode ?? throw new ArgumentNullException(nameof(auditCode));
        StartedBy = startedBy ?? throw new ArgumentNullException(nameof(startedBy));
    }
}

// COMPLETE SMS AUDIT
public class CompleteSMSAuditCommand : BaseCommandBundle, IRequest<Result<SMSAudit>>
{
    public string AuditCode { get; set; }
    public string CompletedBy { get; set; }
    public string AuditSummary { get; set; }
    public string KeyFindings { get; set; }
    public string Recommendations { get; set; }
    public string Conclusions { get; set; }

    public CompleteSMSAuditCommand(string auditCode, string completedBy, string auditSummary,
        string keyFindings, string recommendations = "", string conclusions = "")
    {
        AuditCode = auditCode ?? throw new ArgumentNullException(nameof(auditCode));
        CompletedBy = completedBy ?? throw new ArgumentNullException(nameof(completedBy));
        AuditSummary = auditSummary ?? throw new ArgumentNullException(nameof(auditSummary));
        KeyFindings = keyFindings ?? throw new ArgumentNullException(nameof(keyFindings));
        Recommendations = recommendations ?? string.Empty;
        Conclusions = conclusions ?? string.Empty;
    }
}

// ADD SMS AUDIT FINDING
public class AddSMSAuditFindingCommand : BaseCommandBundle, IRequest<Result<SMSAuditFinding>>
{
    public string AuditCode { get; set; }
    public string FindingDescription { get; set; }
    public string Severity { get; set; }
    public string FindingType { get; set; }
    public string AffectedArea { get; set; }
    public string RequirementReference { get; set; }
    public string FoundBy { get; set; }

    public AddSMSAuditFindingCommand(string auditCode, string findingDescription, string severity,
        string findingType, string affectedArea, string requirementReference, string foundBy)
    {
        AuditCode = auditCode ?? throw new ArgumentNullException(nameof(auditCode));
        FindingDescription = findingDescription ?? throw new ArgumentNullException(nameof(findingDescription));
        Severity = severity ?? throw new ArgumentNullException(nameof(severity));
        FindingType = findingType ?? throw new ArgumentNullException(nameof(findingType));
        AffectedArea = affectedArea ?? string.Empty;
        RequirementReference = requirementReference ?? string.Empty;
        FoundBy = foundBy ?? throw new ArgumentNullException(nameof(foundBy));
    }
}

// CANCEL SMS AUDIT
public class CancelSMSAuditCommand : BaseCommandBundle, IRequest<Result<SMSAudit>>
{
    public string AuditCode { get; set; }
    public string CancelledBy { get; set; }
    public string CancellationReason { get; set; }

    public CancelSMSAuditCommand(string auditCode, string cancelledBy, string cancellationReason)
    {
        AuditCode = auditCode ?? throw new ArgumentNullException(nameof(auditCode));
        CancelledBy = cancelledBy ?? throw new ArgumentNullException(nameof(cancelledBy));
        CancellationReason = cancellationReason ?? throw new ArgumentNullException(nameof(cancellationReason));
    }
}

// DELETE SMS AUDIT
public class DeleteSMSAuditCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    public string AuditCode { get; set; }
    public string DeletedBy { get; set; }
    public string DeletionReason { get; set; }

    public DeleteSMSAuditCommand(string auditCode, string deletedBy, string deletionReason = "")
    {
        AuditCode = auditCode ?? throw new ArgumentNullException(nameof(auditCode));
        DeletedBy = deletedBy ?? throw new ArgumentNullException(nameof(deletedBy));
        DeletionReason = deletionReason ?? string.Empty;
    }
}