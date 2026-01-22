namespace SMS_Application.Messaging.Commands;

/// <summary>
/// SMS Audit Finding Commands for CQRS pattern
/// </summary>

// CREATE SMS AUDIT FINDING (Direct creation)
public class CreateSMSAuditFindingCommand : BaseCommandBundle, IRequest<Result<SMSAuditFinding>>
{
    public string AuditCode { get; set; }
    public string FindingDescription { get; set; }
    public string Severity { get; set; }
    public string FindingType { get; set; }
    public string AffectedArea { get; set; }
    public string RequirementReference { get; set; }
    public string EvidenceDescription { get; set; }
    public string CreatedBy { get; set; }

    public CreateSMSAuditFindingCommand(string auditCode, string findingDescription, string severity,
        string findingType, string affectedArea, string requirementReference,
        string evidenceDescription, string createdBy)
    {
        AuditCode = auditCode ?? throw new ArgumentNullException(nameof(auditCode));
        FindingDescription = findingDescription ?? throw new ArgumentNullException(nameof(findingDescription));
        Severity = severity ?? throw new ArgumentNullException(nameof(severity));
        FindingType = findingType ?? throw new ArgumentNullException(nameof(findingType));
        AffectedArea = affectedArea ?? string.Empty;
        RequirementReference = requirementReference ?? string.Empty;
        EvidenceDescription = evidenceDescription ?? string.Empty;
        CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));
    }
}

// UPDATE SMS AUDIT FINDING
public class UpdateSMSAuditFindingCommand : BaseCommandBundle, IRequest<Result<SMSAuditFinding>>
{
    public string FindingCode { get; set; }
    public string FindingDescription { get; set; }
    public string Severity { get; set; }
    public string FindingType { get; set; }
    public string AffectedArea { get; set; }
    public string RequirementReference { get; set; }
    public string EvidenceDescription { get; set; }
    public string RootCause { get; set; }
    public string UpdatedBy { get; set; }

    public UpdateSMSAuditFindingCommand(string findingCode, string findingDescription, string severity,
        string findingType, string affectedArea, string requirementReference,
        string evidenceDescription, string rootCause, string updatedBy)
    {
        FindingCode = findingCode ?? throw new ArgumentNullException(nameof(findingCode));
        FindingDescription = findingDescription ?? throw new ArgumentNullException(nameof(findingDescription));
        Severity = severity ?? throw new ArgumentNullException(nameof(severity));
        FindingType = findingType ?? throw new ArgumentNullException(nameof(findingType));
        AffectedArea = affectedArea ?? string.Empty;
        RequirementReference = requirementReference ?? string.Empty;
        EvidenceDescription = evidenceDescription ?? string.Empty;
        RootCause = rootCause ?? string.Empty;
        UpdatedBy = updatedBy ?? throw new ArgumentNullException(nameof(updatedBy));
    }
}

// ASSIGN CORRECTIVE ACTION
public class AssignSMSAuditCorrectiveActionCommand : BaseCommandBundle, IRequest<Result<SMSAuditFinding>>
{
    public string FindingCode { get; set; }
    public string CorrectiveAction { get; set; }
    public string ResponsiblePerson { get; set; }
    public string ResponsibleDepartment { get; set; }
    public DateTime TargetCompletionDate { get; set; }
    public string AssignedBy { get; set; }

    public AssignSMSAuditCorrectiveActionCommand(string findingCode, string correctiveAction,
        string responsiblePerson, string responsibleDepartment, DateTime targetCompletionDate,
        string assignedBy)
    {
        FindingCode = findingCode ?? throw new ArgumentNullException(nameof(findingCode));
        CorrectiveAction = correctiveAction ?? throw new ArgumentNullException(nameof(correctiveAction));
        ResponsiblePerson = responsiblePerson ?? throw new ArgumentNullException(nameof(responsiblePerson));
        ResponsibleDepartment = responsibleDepartment ?? string.Empty;
        TargetCompletionDate = targetCompletionDate;
        AssignedBy = assignedBy ?? throw new ArgumentNullException(nameof(assignedBy));
    }
}

// COMPLETE CORRECTIVE ACTION
public class CompleteSMSAuditCorrectiveActionCommand : BaseCommandBundle, IRequest<Result<SMSAuditFinding>>
{
    public string FindingCode { get; set; }
    public DateTime CompletionDate { get; set; }
    public string CompletionEvidence { get; set; }
    public string CompletedBy { get; set; }

    public CompleteSMSAuditCorrectiveActionCommand(string findingCode, DateTime completionDate,
        string completionEvidence, string completedBy)
    {
        FindingCode = findingCode ?? throw new ArgumentNullException(nameof(findingCode));
        CompletionDate = completionDate;
        CompletionEvidence = completionEvidence ?? string.Empty;
        CompletedBy = completedBy ?? throw new ArgumentNullException(nameof(completedBy));
    }
}

// VERIFY SMS AUDIT FINDING
public class VerifySMSAuditFindingCommand : BaseCommandBundle, IRequest<Result<SMSAuditFinding>>
{
    public string FindingCode { get; set; }
    public string VerificationMethod { get; set; }
    public string VerificationEvidence { get; set; }
    public string VerifiedBy { get; set; }

    public VerifySMSAuditFindingCommand(string findingCode, string verificationMethod,
        string verificationEvidence, string verifiedBy)
    {
        FindingCode = findingCode ?? throw new ArgumentNullException(nameof(findingCode));
        VerificationMethod = verificationMethod ?? throw new ArgumentNullException(nameof(verificationMethod));
        VerificationEvidence = verificationEvidence ?? throw new ArgumentNullException(nameof(verificationEvidence));
        VerifiedBy = verifiedBy ?? throw new ArgumentNullException(nameof(verifiedBy));
    }
}

// DELETE SMS AUDIT FINDING
public class DeleteSMSAuditFindingCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    public string FindingCode { get; set; }
    public string DeletedBy { get; set; }
    public string DeletionReason { get; set; }

    public DeleteSMSAuditFindingCommand(string findingCode, string deletedBy, string deletionReason = "")
    {
        FindingCode = findingCode ?? throw new ArgumentNullException(nameof(findingCode));
        DeletedBy = deletedBy ?? throw new ArgumentNullException(nameof(deletedBy));
        DeletionReason = deletionReason ?? string.Empty;
    }
}