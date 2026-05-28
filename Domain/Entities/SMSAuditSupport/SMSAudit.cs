//-----------------------------------------------------------------------
// <copyright file="SMSAudit.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS audit entity representing smsaudit for compliance and regulatory requirements.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// SMS Audit entity for managing individual audit executions and calendar entries
/// </summary>
public class SMSAudit : BaseAuditableEntity
{
    public SMSAudit(SMSAuditID id, string createdBy) : base(id, createdBy, DateTime.UtcNow)
    {
        Code = id.Value;
        Findings = new List<SMSAuditFinding>();
        Evidence = new List<SMSAuditEvidence>();
    }

    // Basic Information
    public string Code { get; set; }
    public string AuditPlanCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AuditType { get; set; } = string.Empty; // Internal, External, Regulatory, Management
    public string Status { get; set; } = string.Empty; // Scheduled, In Progress, Completed, Cancelled, Postponed

    // Schedule Information
    public DateTime ScheduledStartDate { get; set; }
    public DateTime ScheduledEndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }

    // Audit Team
    public string LeadAuditor { get; set; } = string.Empty;
    public string AuditorTeam { get; set; } = string.Empty; // JSON array of auditor names/IDs
    public string ResponsibleDepartment { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;

    // Execution Details - ADDED MISSING PROPERTIES
    public string Scope { get; set; } = string.Empty; // Added for repository compatibility
    public string Objectives { get; set; } = string.Empty; // Added for repository compatibility
    public string AuditScope { get; set; } = string.Empty;
    public string AuditObjectives { get; set; } = string.Empty;
    public string AuditCriteria { get; set; } = string.Empty;
    public string AuditMethodology { get; set; } = string.Empty;

    // Location and Logistics
    public string AuditLocation { get; set; } = string.Empty;
    public string LocationArea { get; set; } = string.Empty;
    public string LocationSubArea { get; set; } = string.Empty;
    public string SpecialRequirements { get; set; } = string.Empty;

    // Progress Tracking
    public int ProgressPercentage { get; set; } = 0;
    public string CurrentPhase { get; set; } = string.Empty; // Planning, Opening, Fieldwork, Reporting, Closure
    public string ExecutionNotes { get; set; } = string.Empty;

    // Results Summary
    public int TotalFindings { get; set; } = 0;
    public int CriticalFindings { get; set; } = 0;
    public int MajorFindings { get; set; } = 0;
    public int MinorFindings { get; set; } = 0;
    public int Observations { get; set; } = 0;

    // Report Information - ADDED MISSING PROPERTIES
    public string Priority { get; set; } = string.Empty; // Added for repository compatibility
    public string ExecutiveSummary { get; set; } = string.Empty; // Added for repository compatibility
    public string Notes { get; set; } = string.Empty; // Added for repository compatibility
    public string AuditSummary { get; set; } = string.Empty;
    public string KeyFindings { get; set; } = string.Empty;
    public string Recommendations { get; set; } = string.Empty;
    public string Conclusions { get; set; } = string.Empty;
    public DateTime? ReportSubmittedDate { get; set; }
    public string ReportSubmittedBy { get; set; } = string.Empty;

    // Follow-up
    public DateTime? FollowUpDate { get; set; }
    public string FollowUpRequired { get; set; } = string.Empty; // Yes, No, Conditional
    public string FollowUpNotes { get; set; } = string.Empty;

    // Navigation Properties
    public List<SMSAuditFinding> Findings { get; set; }
    public List<SMSAuditEvidence> Evidence { get; set; }

    // Business Methods - PIPELINE APPROACH: Removed manual audit field setting
    public Result StartAudit(string startedBy)
    {
        try
        {
            if (Status != "Scheduled")
                return Result.Failure(new Error("INVALID_STATUS", "Can only start scheduled audits"));

            Status = "In Progress";
            ActualStartDate = DateTime.UtcNow;
            CurrentPhase = "Opening";
            
            // REMOVED: Manual audit field setting - pipeline handles this
            // UpdatedBy = startedBy;
            // UpdatedDate = DateTime.UtcNow;

            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(new Error("START_AUDIT_FAILED", "Failed to start audit"));
        }
    }

    public Result CompleteAudit(string completedBy, string auditSummary, string keyFindings)
    {
        try
        {
            if (Status != "In Progress")
                return Result.Failure(new Error("INVALID_STATUS", "Can only complete audits in progress"));

            Status = "Completed";
            ActualEndDate = DateTime.UtcNow;
            CurrentPhase = "Reporting";
            AuditSummary = auditSummary;
            KeyFindings = keyFindings;
            ProgressPercentage = 100;
            
            // REMOVED: Manual audit field setting - pipeline handles this
            // UpdatedBy = completedBy;
            // UpdatedDate = DateTime.UtcNow;

            // Update finding counts
            UpdateFindingCounts();

            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(new Error("COMPLETE_AUDIT_FAILED", "Failed to complete audit"));
        }
    }

    public Result AddFinding(string findingDescription, string severity, string foundBy)
    {
        try
        {
            var finding = new SMSAuditFinding(
                new SMSAuditFindingID($"FND-{DateTime.UtcNow:yyyyMMddHHmmss}-{Findings.Count + 1:D3}"),
                foundBy)
            {
                AuditCode = Code,
                FindingDescription = findingDescription,
                Severity = severity,
                Status = "Open",
                FoundDate = DateTime.UtcNow
            };

            Findings.Add(finding);
            UpdateFindingCounts();
            
            // REMOVED: Manual audit field setting - pipeline handles this
            // UpdatedBy = foundBy;
            // UpdatedDate = DateTime.UtcNow;

            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(new Error("ADD_FINDING_FAILED", "Failed to add audit finding"));
        }
    }

    private void UpdateFindingCounts()
    {
        TotalFindings = Findings.Count;
        CriticalFindings = Findings.Count(f => f.Severity == "Critical");
        MajorFindings = Findings.Count(f => f.Severity == "Major");
        MinorFindings = Findings.Count(f => f.Severity == "Minor");
        Observations = Findings.Count(f => f.Severity == "Observation");
    }
}

