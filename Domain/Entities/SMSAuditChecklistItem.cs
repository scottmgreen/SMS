//-----------------------------------------------------------------------
// <copyright file="SMSAuditChecklistItem.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS audit entity representing smsauditchecklistitem for compliance and regulatory requirements.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// Represents an individual checklist item within an SMS audit
/// Used to track completion of specific audit tasks and requirements
/// </summary>
public class SMSAuditChecklistItem : BaseAuditableEntity
{
    #region Constructor
    public SMSAuditChecklistItem(SMSAuditChecklistItemID id, string createdBy) : base(id, createdBy, DateTime.UtcNow)
    {
    }
    #endregion

    #region Properties
    public string? Code { get; set; }
    public string? AuditCode { get; set; }
    public int ItemNumber { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }
    public string? Requirement { get; set; }
    public bool IsRequired { get; set; }
    public string? Status { get; set; } = "Pending"; // Pending, In Progress, Completed, N/A, Skipped

    // Completion Information
    public DateTime? CompletedDate { get; set; }
    public string? CompletedBy { get; set; }
    public string? Notes { get; set; }
    public string? Evidence { get; set; }

    // Review Information
    public DateTime? ReviewedDate { get; set; }
    public string? ReviewedBy { get; set; }
    public string? ReviewNotes { get; set; }
    public bool? IsAcceptable { get; set; }

    // Reference Information
    public string? ReferenceDocument { get; set; }
    public string? RegulatoryReference { get; set; }
    public string? ProcessArea { get; set; }
    public string? ResponsiblePerson { get; set; }
    #endregion

    #region Factory Methods
    public static Result<SMSAuditChecklistItem> Create(
        string code,
        string auditCode,
        int itemNumber,
        string category,
        string description,
        string? requirement,
        bool isRequired,
        string createdBy)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure<SMSAuditChecklistItem>(DomainErrors.SMSAuditChecklistItemError.CodeRequired);

        if (string.IsNullOrWhiteSpace(auditCode))
            return Result.Failure<SMSAuditChecklistItem>(DomainErrors.SMSAuditChecklistItemError.AuditCodeRequired);

        if (itemNumber <= 0)
            return Result.Failure<SMSAuditChecklistItem>(DomainErrors.SMSAuditChecklistItemError.InvalidItemNumber);

        if (string.IsNullOrWhiteSpace(category))
            return Result.Failure<SMSAuditChecklistItem>(DomainErrors.SMSAuditChecklistItemError.CategoryRequired);

        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure<SMSAuditChecklistItem>(DomainErrors.SMSAuditChecklistItemError.DescriptionRequired);

        if (string.IsNullOrWhiteSpace(createdBy))
            return Result.Failure<SMSAuditChecklistItem>(DomainErrors.SMSAuditChecklistItemError.CreatedByRequired);

        var id = new SMSAuditChecklistItemID(code);
        var item = new SMSAuditChecklistItem(id, createdBy)
        {
            Code = code,
            AuditCode = auditCode,
            ItemNumber = itemNumber,
            Category = category,
            Description = description,
            Requirement = requirement,
            IsRequired = isRequired,
            Status = "Pending"
        };

        return Result.Success(item);
    }
    #endregion

    #region Business Methods
    public Result MarkInProgress(string updatedBy, string? notes = null)
    {
        if (Status == "Completed")
            return Result.Failure(DomainErrors.SMSAuditChecklistItemError.AlreadyCompleted);

        if (Status == "N/A")
            return Result.Failure(DomainErrors.SMSAuditChecklistItemError.NotApplicable);

        Status = "In Progress";
        Notes = notes;
        UpdatedBy = updatedBy;
        UpdatedDate = DateTime.UtcNow;

        return Result.Success();
    }

    public Result MarkCompleted(string completedBy, string? notes = null, string? evidence = null)
    {
        if (Status == "N/A")
            return Result.Failure(DomainErrors.SMSAuditChecklistItemError.NotApplicable);

        if (Status == "Skipped")
            return Result.Failure(DomainErrors.SMSAuditChecklistItemError.Skipped);

        Status = "Completed";
        CompletedDate = DateTime.UtcNow;
        CompletedBy = completedBy;
        Notes = notes;
        Evidence = evidence;
        UpdatedBy = completedBy;
        UpdatedDate = DateTime.UtcNow;

        return Result.Success();
    }

    public Result MarkNotApplicable(string updatedBy, string reason)
    {
        if (Status == "Completed")
            return Result.Failure(DomainErrors.SMSAuditChecklistItemError.AlreadyCompleted);

        if (IsRequired)
            return Result.Failure(DomainErrors.SMSAuditChecklistItemError.RequiredItem);

        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(DomainErrors.SMSAuditChecklistItemError.ReasonRequired);

        Status = "N/A";
        Notes = reason;
        UpdatedBy = updatedBy;
        UpdatedDate = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Skip(string updatedBy, string reason)
    {
        if (Status == "Completed")
            return Result.Failure(DomainErrors.SMSAuditChecklistItemError.AlreadyCompleted);

        if (IsRequired)
            return Result.Failure(DomainErrors.SMSAuditChecklistItemError.RequiredItem);

        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(DomainErrors.SMSAuditChecklistItemError.ReasonRequired);

        Status = "Skipped";
        Notes = reason;
        UpdatedBy = updatedBy;
        UpdatedDate = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Review(string reviewedBy, bool isAcceptable, string? reviewNotes = null)
    {
        if (Status != "Completed")
            return Result.Failure(DomainErrors.SMSAuditChecklistItemError.NotCompleted);

        ReviewedDate = DateTime.UtcNow;
        ReviewedBy = reviewedBy;
        ReviewNotes = reviewNotes;
        IsAcceptable = isAcceptable;
        UpdatedBy = reviewedBy;
        UpdatedDate = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Update(
        string category,
        string description,
        string? requirement,
        bool isRequired,
        string? referenceDocument,
        string? regulatoryReference,
        string? processArea,
        string? responsiblePerson,
        string updatedBy)
    {
        if (Status == "Completed")
            return Result.Failure(DomainErrors.SMSAuditChecklistItemError.CannotUpdateCompleted);

        if (string.IsNullOrWhiteSpace(category))
            return Result.Failure(DomainErrors.SMSAuditChecklistItemError.CategoryRequired);

        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure(DomainErrors.SMSAuditChecklistItemError.DescriptionRequired);

        Category = category;
        Description = description;
        Requirement = requirement;
        IsRequired = isRequired;
        ReferenceDocument = referenceDocument;
        RegulatoryReference = regulatoryReference;
        ProcessArea = processArea;
        ResponsiblePerson = responsiblePerson;
        UpdatedBy = updatedBy;
        UpdatedDate = DateTime.UtcNow;

        return Result.Success();
    }

    public Result ResetStatus(string updatedBy, string? reason = null)
    {
        if (IsRequired && Status == "N/A")
            return Result.Failure(DomainErrors.SMSAuditChecklistItemError.CannotResetRequired);

        Status = "Pending";
        CompletedDate = null;
        CompletedBy = null;
        ReviewedDate = null;
        ReviewedBy = null;
        ReviewNotes = null;
        IsAcceptable = null;

        if (!string.IsNullOrWhiteSpace(reason))
        {
            Notes = string.IsNullOrWhiteSpace(Notes)
                ? $"Reset: {reason}"
                : $"{Notes}\nReset: {reason}";
        }

        UpdatedBy = updatedBy;
        UpdatedDate = DateTime.UtcNow;

        return Result.Success();
    }
    #endregion

    #region Helper Methods
    public bool IsCompleted => Status == "Completed";
    public bool IsPending => Status == "Pending";
    public bool IsInProgress => Status == "In Progress";
    public bool IsNotApplicable => Status == "N/A";
    public bool IsSkipped => Status == "Skipped";

    public bool IsReviewed => ReviewedDate.HasValue && !string.IsNullOrWhiteSpace(ReviewedBy);
    public bool IsAcceptableReview => IsReviewed && IsAcceptable == true;
    public bool IsUnacceptableReview => IsReviewed && IsAcceptable == false;

    public TimeSpan? TimeToComplete
    {
        get
        {
            if (CompletedDate.HasValue && CreatedDate.HasValue)
                return CompletedDate.Value - CreatedDate.Value;
            return null;
        }
    }

    public bool IsOverdue => IsRequired && !IsCompleted && !IsNotApplicable && !IsSkipped;

    public string GetStatusDisplayText()
    {
        return Status switch
        {
            "Pending" => "Pending",
            "In Progress" => "In Progress",
            "Completed" => IsAcceptableReview ? "Completed (Approved)" :
                          IsUnacceptableReview ? "Completed (Rejected)" : "Completed",
            "N/A" => "Not Applicable",
            "Skipped" => "Skipped",
            _ => Status ?? "Unknown"
        };
    }

    public string GetStatusColor()
    {
        return Status switch
        {
            "Completed" when IsAcceptableReview => "#28a745", // Success green
            "Completed" when IsUnacceptableReview => "#dc3545", // Danger red
            "Completed" => "#17a2b8", // Info blue
            "In Progress" => "#ffc107", // Warning yellow
            "Pending" when IsRequired => "#dc3545", // Danger red for required pending
            "Pending" => "#6c757d", // Secondary gray
            "N/A" => "#6c757d", // Secondary gray
            "Skipped" => "#fd7e14", // Warning orange
            _ => "#6c757d" // Default gray
        };
    }
    #endregion
}
