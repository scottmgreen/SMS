using SMS_Domain.Common;
using SMS_Domain.Interfaces;

namespace SMS_Domain.Entities;

/// <summary>
/// ReportValidation Domain Entity
/// Represents validation of Reports to determine if they qualify as SMS Risks
/// Maps to tbld_ReportValidations database table
/// </summary>
public class ReportValidation : BaseAuditableEntity, IReportValidation
{
    
    // Public constructor for domain usage
    public ReportValidation(ReportValidationID id) : base(id, "SYSTEM", DateTime.UtcNow)
    {
        Code = string.Empty;
        ReportCode = string.Empty;
        ValidationDecision = string.Empty;
        Status = string.Empty;
        Stage = string.Empty;
    }

    #region Core Properties (matching database table)

    /// <summary>
    /// Unique validation code
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to the report being validated
    /// </summary>
    public string ReportCode { get; set; } = string.Empty;

    /// <summary>
    /// The validation decision (SMS_RISK, NOT_SMS_RISK, NEEDS_INVESTIGATION)
    /// </summary>
    public string? ValidationDecision { get; set; }

    /// <summary>
    /// Current status of the validation (Draft, InProgress, Completed)
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Current stage of the validation process
    /// </summary>
    public string? Stage { get; set; }

    /// <summary>
    /// Who performed the validation
    /// </summary>
    public string? ValidatedBy { get; set; }

    /// <summary>
    /// When the validation was completed
    /// </summary>
    public DateTime? ValidatedDate { get; set; }

    /// <summary>
    /// Validation comments/notes
    /// </summary>
    public string? ValidationComments { get; set; }

    /// <summary>
    /// Type of validation (Standard, Expedited, Complex)
    /// </summary>
    public string? ValidationType { get; set; }

    #endregion

    #region Interface Implementation

    ReportValidationID IReportValidation.Id 
    { 
        get => (ReportValidationID)Id; 
        set => throw new NotSupportedException("Id cannot be set directly"); 
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Create a new ReportValidation for a report
    /// </summary>
    public static ReportValidation Create(string reportCode, string validatedBy)
    {
        var id = new ReportValidationID($"RV-0000");
        var validation = new ReportValidation(id)
        {
            Code = id.Value,
            ReportCode = reportCode,
            ValidatedBy = validatedBy,
            Status = "InProgress",
            Stage = "Initial",
            ValidationType = "Standard"
        };

        return validation;
    }

    #endregion

    #region Business Logic

    /// <summary>
    /// Complete the validation with a decision
    /// </summary>
    public void CompleteValidation(string decision, string comments, string? validationType = null)
    {
        ValidationDecision = decision;
        ValidationComments = comments;
        ValidatedDate = DateTime.UtcNow;
        Status = "Completed";
        
        if (!string.IsNullOrWhiteSpace(validationType))
        {
            ValidationType = validationType;
        }
    }

    /// <summary>
    /// Save as draft
    /// </summary>
    public void SaveAsDraft(string? comments = null)
    {
        Status = "Draft";
        if (!string.IsNullOrWhiteSpace(comments))
        {
            ValidationComments = comments;
        }
    }

    /// <summary>
    /// Check if validation is completed
    /// </summary>
    public bool IsCompleted => Status == "Completed";

    /// <summary>
    /// Check if validation is in draft state
    /// </summary>
    public bool IsDraft => Status == "Draft";

    #endregion
}