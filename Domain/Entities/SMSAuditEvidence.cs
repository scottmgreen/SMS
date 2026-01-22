namespace SMS_Domain.Entities;

/// <summary>
/// SMS Audit Evidence entity for managing audit documentation and evidence collection
/// </summary>
public class SMSAuditEvidence : BaseAuditableEntity
{
    public SMSAuditEvidence(SMSAuditEvidenceID id, string createdBy) : base(id, createdBy, DateTime.UtcNow)
    {
        Code = id.Value;
    }

    // Basic Information
    public string Code { get; set; }
    public string AuditCode { get; set; } = string.Empty;
    public string FindingCode { get; set; } = string.Empty; // Optional - if linked to specific finding

    // ADDED MISSING PROPERTIES for repository compatibility
    public string Title { get; set; } = string.Empty; // Added for repository compatibility
    public string Description { get; set; } = string.Empty; // Added for repository compatibility
    public string Source { get; set; } = string.Empty; // Added for repository compatibility
    public DateTime CollectionDate { get; set; } = DateTime.UtcNow; // Added for repository compatibility
    public string ContentType { get; set; } = string.Empty; // Added for repository compatibility
    public string StorageLocation { get; set; } = string.Empty; // Added for repository compatibility
    public int RetentionPeriodMonths { get; set; } = 84; // Added for repository compatibility (7 years)
    public string Notes { get; set; } = string.Empty; // Added for repository compatibility

    public string EvidenceType { get; set; } = string.Empty; // Document, Photo, Interview, Observation, Record
    public string EvidenceTitle { get; set; } = string.Empty;
    public string EvidenceDescription { get; set; } = string.Empty;

    // Evidence Details
    public DateTime EvidenceDate { get; set; }
    public string CollectedBy { get; set; } = string.Empty;
    public string EvidenceLocation { get; set; } = string.Empty;
    public string EvidenceSource { get; set; } = string.Empty;

    // File Information (if applicable)
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; } = 0;
    public string FileHash { get; set; } = string.Empty;

    // Document Properties
    public string DocumentNumber { get; set; } = string.Empty;
    public string DocumentVersion { get; set; } = string.Empty;
    public DateTime? DocumentDate { get; set; }
    public string DocumentAuthor { get; set; } = string.Empty;

    // Interview Evidence (if applicable)
    public string IntervieweeName { get; set; } = string.Empty;
    public string IntervieweePosition { get; set; } = string.Empty;
    public string IntervieweeDepartment { get; set; } = string.Empty;
    public DateTime? InterviewDate { get; set; }
    public int InterviewDurationMinutes { get; set; } = 0;

    // Observation Evidence (if applicable)
    public string ObservationLocation { get; set; } = string.Empty;
    public DateTime? ObservationDate { get; set; }
    public string ObservationConditions { get; set; } = string.Empty;
    public string ObservedPersonnel { get; set; } = string.Empty;

    // Classification and Security
    public string ConfidentialityLevel { get; set; } = string.Empty; // Public, Internal, Confidential, Restricted
    public bool IsObjectiveEvidence { get; set; } = false;
    public string EvidenceQuality { get; set; } = string.Empty; // Excellent, Good, Fair, Poor
    public string Reliability { get; set; } = string.Empty; // High, Medium, Low

    // Verification and Validation
    public bool IsVerified { get; set; } = false;
    public string VerifiedBy { get; set; } = string.Empty;
    public DateTime? VerificationDate { get; set; }
    public string VerificationNotes { get; set; } = string.Empty;

    // Retention and Lifecycle
    public DateTime RetentionDate { get; set; } = DateTime.UtcNow.AddYears(7); // Default 7-year retention
    public string RetentionReason { get; set; } = string.Empty;
    public bool IsArchived { get; set; } = false;
    public DateTime? ArchivedDate { get; set; }

    // Additional Information
    public string Tags { get; set; } = string.Empty; // JSON array or comma-separated
    public string CrossReferences { get; set; } = string.Empty;
    public string AdditionalNotes { get; set; } = string.Empty;

    // Business Methods
    public Result VerifyEvidence(string verifiedBy, string verificationNotes = "")
    {
        try
        {
            IsVerified = true;
            VerifiedBy = verifiedBy;
            VerificationDate = DateTime.UtcNow;
            VerificationNotes = verificationNotes;
            UpdatedBy = verifiedBy;
            UpdatedDate = DateTime.UtcNow;

            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(new Error("VERIFY_EVIDENCE_FAILED", "Failed to verify evidence"));
        }
    }

    public Result ArchiveEvidence(string archivedBy, string reason = "")
    {
        try
        {
            IsArchived = true;
            ArchivedDate = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(reason))
                RetentionReason = reason;
            UpdatedBy = archivedBy;
            UpdatedDate = DateTime.UtcNow;

            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(new Error("ARCHIVE_EVIDENCE_FAILED", "Failed to archive evidence"));
        }
    }

    public Result LinkToFinding(string findingCode, string linkedBy)
    {
        try
        {
            FindingCode = findingCode;
            UpdatedBy = linkedBy;
            UpdatedDate = DateTime.UtcNow;

            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(new Error("LINK_EVIDENCE_FAILED", "Failed to link evidence to finding"));
        }
    }
}