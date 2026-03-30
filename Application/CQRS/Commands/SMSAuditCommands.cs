//-----------------------------------------------------------------------
// <copyright file="SMSAuditCommands.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command definitions for SMS audit management operations and workflow actions.
//                  Defines command objects for write operations in the CQRS pattern.
//                  Commands represent business intentions and trigger state changes.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Application.Messaging.Commands;

/// <summary>
/// SMS Audit Commands for CQRS pattern
/// </summary>

// CREATE SMS AUDIT (Manual Creation)
public class CreateSMSAuditCommand : BaseCommandBundle, IRequest<Result<SMSAudit>>
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? AuditPlanCode { get; set; }
    public string AuditType { get; set; }
    public string Scope { get; set; }
    public string? Objectives { get; set; }
    public DateTime ScheduledStartDate { get; set; }
    public DateTime ScheduledEndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public string LeadAuditor { get; set; }
    public string? AuditorTeam { get; set; }
    public string ResponsibleDepartment { get; set; }
    public string? ContactPerson { get; set; }
    public string? AuditLocation { get; set; }
    public string Status { get; set; }
    public string Priority { get; set; }
    public string? ExecutiveSummary { get; set; }
    public string? Notes { get; set; }
    public string CreatedBy { get; set; }

    // Full constructor (existing)
    public CreateSMSAuditCommand(string code, string name, string? description, string? auditPlanCode,
        string auditType, string scope, string? objectives, DateTime scheduledStartDate, DateTime scheduledEndDate,
        DateTime? actualStartDate, DateTime? actualEndDate, string leadAuditor, string? auditorTeam,
        string responsibleDepartment, string? contactPerson, string? auditLocation, string status, string priority,
        string? executiveSummary, string? notes, string createdBy)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        AuditPlanCode = auditPlanCode;
        AuditType = auditType ?? throw new ArgumentNullException(nameof(auditType));
        Scope = scope ?? throw new ArgumentNullException(nameof(scope));
        Objectives = objectives;
        ScheduledStartDate = scheduledStartDate;
        ScheduledEndDate = scheduledEndDate;
        ActualStartDate = actualStartDate;
        ActualEndDate = actualEndDate;
        LeadAuditor = leadAuditor ?? throw new ArgumentNullException(nameof(leadAuditor));
        AuditorTeam = auditorTeam;
        ResponsibleDepartment = responsibleDepartment ?? throw new ArgumentNullException(nameof(responsibleDepartment));
        ContactPerson = contactPerson;
        AuditLocation = auditLocation;
        Status = status ?? throw new ArgumentNullException(nameof(status));
        Priority = priority ?? throw new ArgumentNullException(nameof(priority));
        ExecutiveSummary = executiveSummary;
        Notes = notes;
        CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));
    }

    // Simplified constructor for creating from audit plan
    public CreateSMSAuditCommand(string auditPlanCode, string name, string description, string auditType,
        DateTime scheduledStartDate, DateTime scheduledEndDate, string leadAuditor,
        string responsibleDepartment, string createdBy)
    {
        Code = "AUTO"; // Will be auto-generated
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        AuditPlanCode = auditPlanCode;
        AuditType = auditType ?? throw new ArgumentNullException(nameof(auditType));
        Scope = "TBD"; // Will be set from audit plan
        Objectives = "TBD"; // Will be set from audit plan
        ScheduledStartDate = scheduledStartDate;
        ScheduledEndDate = scheduledEndDate;
        ActualStartDate = null;
        ActualEndDate = null;
        LeadAuditor = leadAuditor ?? throw new ArgumentNullException(nameof(leadAuditor));
        AuditorTeam = null;
        ResponsibleDepartment = responsibleDepartment ?? throw new ArgumentNullException(nameof(responsibleDepartment));
        ContactPerson = null;
        AuditLocation = "TBD";
        Status = "Scheduled"; // Default status when created from plan
        Priority = "Medium"; // Default priority
        ExecutiveSummary = null;
        Notes = null;
        CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));
    }
}

// UPDATE SMS AUDIT
public class UpdateSMSAuditCommand : BaseCommandBundle, IRequest<Result<SMSAudit>>
{
    public SMSAudit Audit { get; set; }

    //public string Code { get; set; }
    //public string Name { get; set; }
    //public string? Description { get; set; }
    //public string? AuditPlanCode { get; set; }
    //public string AuditType { get; set; }
    //public string Scope { get; set; }
    //public string? Objectives { get; set; }
    //public DateTime ScheduledStartDate { get; set; }
    //public DateTime ScheduledEndDate { get; set; }
    //public DateTime? ActualStartDate { get; set; }
    //public DateTime? ActualEndDate { get; set; }
    //public string LeadAuditor { get; set; }
    //public string? AuditorTeam { get; set; }
    //public string ResponsibleDepartment { get; set; }
    //public string? ContactPerson { get; set; }
    //public string? AuditLocation { get; set; }
    //public string Status { get; set; }
    //public string Priority { get; set; }
    //public string? ExecutiveSummary { get; set; }
    //public string? Notes { get; set; }
    //public string UpdatedBy { get; set; }
    //public DateTime UpdatedDate { get; set; }

    public UpdateSMSAuditCommand(SMSAudit audit)
    { this.Audit = audit; }
    //{
    //    Code = code ?? throw new ArgumentNullException(nameof(code));
    //    Name = name ?? throw new ArgumentNullException(nameof(name));
    //    Description = description;
    //    AuditPlanCode = auditPlanCode;
    //    AuditType = auditType ?? throw new ArgumentNullException(nameof(auditType));
    //    Scope = scope ?? throw new ArgumentNullException(nameof(scope));
    //    Objectives = objectives;
    //    ScheduledStartDate = scheduledStartDate;
    //    ScheduledEndDate = scheduledEndDate;
    //    ActualStartDate = actualStartDate;
    //    ActualEndDate = actualEndDate;
    //    LeadAuditor = leadAuditor ?? throw new ArgumentNullException(nameof(leadAuditor));
    //    AuditorTeam = auditorTeam;
    //    ResponsibleDepartment = responsibleDepartment ?? throw new ArgumentNullException(nameof(responsibleDepartment));
    //    ContactPerson = contactPerson;
    //    AuditLocation = auditLocation;
    //    Status = status ?? throw new ArgumentNullException(nameof(status));
    //    Priority = priority ?? throw new ArgumentNullException(nameof(priority));
    //    ExecutiveSummary = executiveSummary;
    //    Notes = notes;
    //    UpdatedBy = updatedBy ?? throw new ArgumentNullException(nameof(updatedBy));
    //    UpdatedDate = updatedDate;
    //}
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
