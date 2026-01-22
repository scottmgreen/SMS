namespace SMS_Domain.Entities;

/// <summary>
/// SMS Audit Plan entity for managing audit planning and scheduling
/// </summary>
public class SMSAuditPlan : BaseAuditableEntity
{
    public SMSAuditPlan(SMSAuditPlanID id, string createdBy) : base(id, createdBy, DateTime.UtcNow)
    {
        Code = id.Value;
        AuditCalendarEntries = new List<SMSAudit>();
    }

    // Basic Information
    public string Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AuditType { get; set; } = string.Empty; // Internal, External, Regulatory, Management
    public string Status { get; set; } = string.Empty; // Draft, Approved, Active, Completed, Cancelled

    // Planning Details - ADDED MISSING PROPERTIES
    public DateTime PlannedStartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public string Scope { get; set; } = string.Empty; // Added for repository compatibility
    public string Objectives { get; set; } = string.Empty; // Added for repository compatibility
    public string AuditScope { get; set; } = string.Empty;
    public string AuditObjectives { get; set; } = string.Empty;
    public string AuditCriteria { get; set; } = string.Empty;

    // Resource Assignment
    public string LeadAuditor { get; set; } = string.Empty;
    public string AuditorTeam { get; set; } = string.Empty; // JSON array of auditor names/IDs
    public string ResponsibleDepartment { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;

    // Approval and Authorization - ADDED MISSING PROPERTIES
    public string Priority { get; set; } = string.Empty; // Added for repository compatibility
    public bool RequiresApproval { get; set; } = true; // Added for repository compatibility
    public int ExpectedDurationHours { get; set; } = 8; // Added for repository compatibility
    public string Notes { get; set; } = string.Empty; // Added for repository compatibility
    public string ApprovedBy { get; set; } = string.Empty;
    public DateTime? ApprovedDate { get; set; }
    public string ApprovalNotes { get; set; } = string.Empty;

    // Planning Documentation
    public string AuditChecklist { get; set; } = string.Empty;
    public string RequiredDocuments { get; set; } = string.Empty;
    public string SpecialRequirements { get; set; } = string.Empty;
    public string RiskAreas { get; set; } = string.Empty;

    // Schedule Management
    public int EstimatedDurationHours { get; set; }
    public string RecurrencePattern { get; set; } = string.Empty; // None, Annual, Quarterly, Monthly
    public DateTime? NextScheduledDate { get; set; }

    // Navigation Properties
    public List<SMSAudit> AuditCalendarEntries { get; set; }

    // Business Methods
    public Result ScheduleAudit(DateTime scheduledDate, string scheduledBy)
    {
        try
        {
            if (Status != "Approved")
                return Result.Failure(new Error("AUDIT_PLAN_NOT_APPROVED", "Audit plan must be approved before scheduling"));

            if (scheduledDate < DateTime.UtcNow.Date)
                return Result.Failure(new Error("INVALID_SCHEDULE_DATE", "Cannot schedule audit in the past"));

            var audit = new SMSAudit(
                new SMSAuditID($"AUD-{DateTime.UtcNow:yyyyMMddHHmmss}"),
                scheduledBy)
            {
                AuditPlanCode = this.Code,
                Name = this.Name,
                AuditType = this.AuditType,
                ScheduledStartDate = scheduledDate,
                ScheduledEndDate = scheduledDate.AddHours(EstimatedDurationHours),
                Status = "Scheduled",
                LeadAuditor = this.LeadAuditor,
                AuditorTeam = this.AuditorTeam,
                ResponsibleDepartment = this.ResponsibleDepartment
            };

            AuditCalendarEntries.Add(audit);
            UpdatedBy = scheduledBy;
            UpdatedDate = DateTime.UtcNow;

            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(new Error("AUDIT_SCHEDULE_FAILED", "Failed to schedule audit"));
        }
    }

    public Result ApproveAuditPlan(string approvedBy, string approvalNotes = "")
    {
        try
        {
            if (Status == "Approved")
                return Result.Failure(new Error("ALREADY_APPROVED", "Audit plan is already approved"));

            Status = "Approved";
            ApprovedBy = approvedBy;
            ApprovedDate = DateTime.UtcNow;
            ApprovalNotes = approvalNotes;
            UpdatedBy = approvedBy;
            UpdatedDate = DateTime.UtcNow;

            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(new Error("APPROVAL_FAILED", "Failed to approve audit plan"));
        }
    }

    public Result CompleteAuditPlan(string completedBy, string completionNotes = "")
    {
        try
        {
            if (Status == "Completed")
                return Result.Failure(new Error("ALREADY_COMPLETED", "Audit plan is already completed"));

            if (Status != "Scheduled" && Status != "Approved")
                return Result.Failure(new Error("INVALID_STATUS_FOR_COMPLETION", "Audit plan must be scheduled or approved to be completed"));

            Status = "Completed";
            UpdatedBy = completedBy;
            UpdatedDate = DateTime.UtcNow;

            // Add completion notes to existing notes
            if (!string.IsNullOrEmpty(completionNotes))
            {
                Notes = string.IsNullOrEmpty(Notes)
                    ? $"Completed: {completionNotes}"
                    : $"{Notes}\n\nCompleted: {completionNotes}";
            }

            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(new Error("COMPLETION_FAILED", "Failed to complete audit plan"));
        }
    }

    public bool IsEditable()
    {
        // Audit plans should not be editable once completed
        return Status != "Completed";
    }
}