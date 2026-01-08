namespace SMS_Domain.Entities;

/// <summary>
/// SMS Audit Finding entity for managing individual audit findings and non-conformances
/// </summary>
public class SMSAuditFinding : BaseAuditableEntity
{
    public SMSAuditFinding(SMSAuditFindingID id, string createdBy) : base(id, createdBy, DateTime.UtcNow)
    {
        Code = id.Value;
    }

    // Basic Information
    public string Code { get; set; }
    public string AuditCode { get; set; } = string.Empty;
    public string FindingNumber { get; set; } = string.Empty;
    
    // ADDED MISSING PROPERTIES for repository compatibility
    public string Title { get; set; } = string.Empty; // Added for repository compatibility
    public string Description { get; set; } = string.Empty; // Added for repository compatibility
    public string Category { get; set; } = string.Empty; // Added for repository compatibility
    public DateTime DiscoveredDate { get; set; } = DateTime.UtcNow; // Added for repository compatibility
    public DateTime? TargetResolutionDate { get; set; } // Added for repository compatibility
    public DateTime? ActualResolutionDate { get; set; } // Added for repository compatibility
    public string RootCauseAnalysis { get; set; } = string.Empty; // Added for repository compatibility
    public bool VerificationRequired { get; set; } = true; // Added for repository compatibility
    public string Notes { get; set; } = string.Empty; // Added for repository compatibility
    
    public string FindingDescription { get; set; } = string.Empty;
    public string FindingType { get; set; } = string.Empty; // Non-Conformance, Observation, Improvement Opportunity
    public string Severity { get; set; } = string.Empty; // Critical, Major, Minor, Observation
    public string Status { get; set; } = string.Empty; // Open, In Progress, Closed, Verified
    
    // Finding Details
    public DateTime FoundDate { get; set; }
    public string FoundBy { get; set; } = string.Empty;
    public string AffectedArea { get; set; } = string.Empty;
    public string AffectedProcess { get; set; } = string.Empty;
    public string RequirementReference { get; set; } = string.Empty;
    public string StandardReference { get; set; } = string.Empty;
    
    // Evidence and Documentation
    public string EvidenceDescription { get; set; } = string.Empty;
    public string ObjectiveEvidence { get; set; } = string.Empty;
    public string PhotosAttached { get; set; } = string.Empty; // Yes/No or count
    public string DocumentsReferenced { get; set; } = string.Empty;
    
    // Root Cause Analysis
    public string RootCause { get; set; } = string.Empty;
    public string ContributingFactors { get; set; } = string.Empty;
    public string RootCauseMethod { get; set; } = string.Empty; // 5 Whys, Fishbone, FMEA, etc.
    
    // Corrective Action
    public string CorrectiveAction { get; set; } = string.Empty;
    public string ResponsiblePerson { get; set; } = string.Empty;
    public string ResponsibleDepartment { get; set; } = string.Empty;
    public DateTime? TargetCompletionDate { get; set; }
    public DateTime? ActualCompletionDate { get; set; }
    
    // Verification and Closure
    public string VerificationMethod { get; set; } = string.Empty;
    public string VerificationEvidence { get; set; } = string.Empty;
    public DateTime? VerificationDate { get; set; }
    public string VerifiedBy { get; set; } = string.Empty;
    public string ClosureNotes { get; set; } = string.Empty;
    public DateTime? ClosureDate { get; set; }
    public string ClosedBy { get; set; } = string.Empty;
    
    // Follow-up
    public bool RequiresFollowUp { get; set; } = false;
    public DateTime? FollowUpDate { get; set; }
    public string FollowUpNotes { get; set; } = string.Empty;
    public bool IsRecurring { get; set; } = false;
    public string RecurrencePattern { get; set; } = string.Empty;
    
    // Additional Information
    public string ImpactAssessment { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = string.Empty; // High, Medium, Low
    public string FindingCategory { get; set; } = string.Empty;
    public string AdditionalNotes { get; set; } = string.Empty;

    // Business Methods
    public Result AssignCorrectiveAction(string correctiveAction, string responsiblePerson, 
        string responsibleDepartment, DateTime targetDate, string assignedBy)
    {
        try
        {
            if (Status != "Open")
                return Result.Failure(new Error("INVALID_STATUS", "Can only assign corrective actions to open findings"));

            CorrectiveAction = correctiveAction;
            ResponsiblePerson = responsiblePerson;
            ResponsibleDepartment = responsibleDepartment;
            TargetCompletionDate = targetDate;
            Status = "In Progress";
            UpdatedBy = assignedBy;
            UpdatedDate = DateTime.UtcNow;

            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(new Error("ASSIGN_ACTION_FAILED", "Failed to assign corrective action"));
        }
    }

    public Result CompleteCorrectiveAction(string completedBy, DateTime completionDate, string evidence = "")
    {
        try
        {
            if (Status != "In Progress")
                return Result.Failure(new Error("INVALID_STATUS", "Corrective action is not in progress"));

            ActualCompletionDate = completionDate;
            if (!string.IsNullOrEmpty(evidence))
                VerificationEvidence = evidence;
            
            Status = "Closed";
            UpdatedBy = completedBy;
            UpdatedDate = DateTime.UtcNow;

            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(new Error("COMPLETE_ACTION_FAILED", "Failed to complete corrective action"));
        }
    }

    public Result VerifyFinding(string verifiedBy, string verificationMethod, string evidence)
    {
        try
        {
            if (Status != "Closed")
                return Result.Failure(new Error("INVALID_STATUS", "Finding must be closed before verification"));

            VerifiedBy = verifiedBy;
            VerificationMethod = verificationMethod;
            VerificationEvidence = evidence;
            VerificationDate = DateTime.UtcNow;
            Status = "Verified";
            UpdatedBy = verifiedBy;
            UpdatedDate = DateTime.UtcNow;

            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(new Error("VERIFY_FINDING_FAILED", "Failed to verify finding"));
        }
    }
}