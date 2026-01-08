using SMS_Application.Common;
using SMS_Application.Interfaces;
using SMS_Domain.Entities;
using SMS_Shared.Common;

namespace SMS_Application.Messaging.Commands;

/// <summary>
/// SMS Audit Plan Commands for CQRS pattern
/// </summary>

// CREATE SMS AUDIT PLAN
public class CreateSMSAuditPlanCommand : BaseCommandBundle, IRequest<Result<SMSAuditPlan>>
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string AuditType { get; set; }
    public string AuditScope { get; set; }
    public string? AuditObjectives { get; set; }
    public string ResponsibleDepartment { get; set; }
    public string LeadAuditor { get; set; }
    public string? AuditorTeam { get; set; }
    public DateTime PlannedStartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public int? EstimatedHours { get; set; }
    public string Priority { get; set; }
    public string Status { get; set; }
    public string? Notes { get; set; }
    public string? RegulatoryRequirements { get; set; }
    public string? Resources { get; set; }
    public string? Deliverables { get; set; }
    public string? SuccessCriteria { get; set; }
    public string CreatedBy { get; set; }

    public CreateSMSAuditPlanCommand(string code, string name, string? description, string auditType, string auditScope,
        string? auditObjectives, string responsibleDepartment, string leadAuditor, string? auditorTeam,
        DateTime plannedStartDate, DateTime plannedEndDate, int? estimatedHours, string priority, string status,
        string? notes, string? regulatoryRequirements, string? resources, string? deliverables, string? successCriteria,
        string createdBy)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        AuditType = auditType ?? throw new ArgumentNullException(nameof(auditType));
        AuditScope = auditScope ?? throw new ArgumentNullException(nameof(auditScope));
        AuditObjectives = auditObjectives;
        ResponsibleDepartment = responsibleDepartment ?? throw new ArgumentNullException(nameof(responsibleDepartment));
        LeadAuditor = leadAuditor ?? throw new ArgumentNullException(nameof(leadAuditor));
        AuditorTeam = auditorTeam;
        PlannedStartDate = plannedStartDate;
        PlannedEndDate = plannedEndDate;
        EstimatedHours = estimatedHours;
        Priority = priority ?? throw new ArgumentNullException(nameof(priority));
        Status = status ?? throw new ArgumentNullException(nameof(status));
        Notes = notes;
        RegulatoryRequirements = regulatoryRequirements;
        Resources = resources;
        Deliverables = deliverables;
        SuccessCriteria = successCriteria;
        CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));
    }
}

// UPDATE SMS AUDIT PLAN
public class UpdateSMSAuditPlanCommand : BaseCommandBundle, IRequest<Result<SMSAuditPlan>>
{
    public Guid Id { get; set; }
    public string AuditPlanCode { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string AuditType { get; set; }
    public string AuditScope { get; set; }
    public string? AuditObjectives { get; set; }
    public string ResponsibleDepartment { get; set; }
    public string LeadAuditor { get; set; }
    public string? AuditorTeam { get; set; }
    public DateTime PlannedStartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public int? EstimatedHours { get; set; }
    public string Priority { get; set; }
    public string Status { get; set; }
    public string? Notes { get; set; }
    public string? RegulatoryRequirements { get; set; }
    public string? Resources { get; set; }
    public string? Deliverables { get; set; }
    public string? SuccessCriteria { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime UpdatedDate { get; set; }

    public UpdateSMSAuditPlanCommand(Guid id, string auditPlanCode, string name, string? description, string auditType, string auditScope,
        string? auditObjectives, string responsibleDepartment, string leadAuditor, string? auditorTeam,
        DateTime plannedStartDate, DateTime plannedEndDate, int? estimatedHours, string priority, string status,
        string? notes, string? regulatoryRequirements, string? resources, string? deliverables, string? successCriteria,
        string updatedBy, DateTime updatedDate)
    {
        Id = id;
        AuditPlanCode = auditPlanCode ?? throw new ArgumentNullException(nameof(auditPlanCode));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        AuditType = auditType ?? throw new ArgumentNullException(nameof(auditType));
        AuditScope = auditScope ?? throw new ArgumentNullException(nameof(auditScope));
        AuditObjectives = auditObjectives;
        ResponsibleDepartment = responsibleDepartment ?? throw new ArgumentNullException(nameof(responsibleDepartment));
        LeadAuditor = leadAuditor ?? throw new ArgumentNullException(nameof(leadAuditor));
        AuditorTeam = auditorTeam;
        PlannedStartDate = plannedStartDate;
        PlannedEndDate = plannedEndDate;
        EstimatedHours = estimatedHours;
        Priority = priority ?? throw new ArgumentNullException(nameof(priority));
        Status = status ?? throw new ArgumentNullException(nameof(status));
        Notes = notes;
        RegulatoryRequirements = regulatoryRequirements;
        Resources = resources;
        Deliverables = deliverables;
        SuccessCriteria = successCriteria;
        UpdatedBy = updatedBy ?? throw new ArgumentNullException(nameof(updatedBy));
        UpdatedDate = updatedDate;
    }
}

// APPROVE SMS AUDIT PLAN
public class ApproveSMSAuditPlanCommand : BaseCommandBundle, IRequest<Result<SMSAuditPlan>>
{
    public string AuditPlanCode { get; set; }
    public string ApprovedBy { get; set; }
    public string ApprovalNotes { get; set; }

    public ApproveSMSAuditPlanCommand(string auditPlanCode, string approvedBy, string approvalNotes = "")
    {
        AuditPlanCode = auditPlanCode ?? throw new ArgumentNullException(nameof(auditPlanCode));
        ApprovedBy = approvedBy ?? throw new ArgumentNullException(nameof(approvedBy));
        ApprovalNotes = approvalNotes ?? string.Empty;
    }
}

// SCHEDULE SMS AUDIT
public class ScheduleSMSAuditCommand : BaseCommandBundle, IRequest<Result<SMSAudit>>
{
    public string AuditPlanCode { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string ScheduledBy { get; set; }
    public string ContactPerson { get; set; }
    public string AuditLocation { get; set; }

    public ScheduleSMSAuditCommand(string auditPlanCode, DateTime scheduledDate, 
        string scheduledBy, string contactPerson = "", string auditLocation = "")
    {
        AuditPlanCode = auditPlanCode ?? throw new ArgumentNullException(nameof(auditPlanCode));
        ScheduledDate = scheduledDate;
        ScheduledBy = scheduledBy ?? throw new ArgumentNullException(nameof(scheduledBy));
        ContactPerson = contactPerson ?? string.Empty;
        AuditLocation = auditLocation ?? string.Empty;
    }
}

// DELETE SMS AUDIT PLAN
public class DeleteSMSAuditPlanCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    public string AuditPlanCode { get; set; }
    public string DeletedBy { get; set; }
    public string DeletionReason { get; set; }

    public DeleteSMSAuditPlanCommand(string auditPlanCode, string deletedBy, string deletionReason = "")
    {
        AuditPlanCode = auditPlanCode ?? throw new ArgumentNullException(nameof(auditPlanCode));
        DeletedBy = deletedBy ?? throw new ArgumentNullException(nameof(deletedBy));
        DeletionReason = deletionReason ?? string.Empty;
    }
}