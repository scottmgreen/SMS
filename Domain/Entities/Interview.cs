using SMS_Shared.Common;
using SMS_Domain.Enums;

namespace SMS_Domain.Entities;

/// <summary>
/// Interview Domain Entity - Enhanced for comprehensive investigation interviews
/// Represents witness interviews and stakeholder discussions during hazard investigations
/// </summary>
public sealed class Interview : BaseAuditableEntity
{
    // Private constructor for Entity Framework
    private Interview() : base(new InterviewID(Guid.NewGuid().ToString()), "SYSTEM", DateTime.UtcNow) { }

    // Public constructor for domain usage
    public Interview(InterviewID id) : base(id, "SYSTEM", DateTime.UtcNow) 
    {
        Status = InterviewStatus.Planned;
        Type = InterviewType.Witness;
        CreatedDate = DateTime.UtcNow;
        UpdatedDate = DateTime.UtcNow;
    }

    // Private constructor for creation with validation
    private Interview(InterviewID id, string code, string investigationCode, string personInterviewed, string investigatorCode)
        : base(id, "SYSTEM", DateTime.UtcNow)
    {
        Code = code;
        InvestigationCode = investigationCode;
        PersonInterviewed = personInterviewed;
        SMSInvestigatorCode = investigatorCode;
        Status = InterviewStatus.Planned;
        Type = InterviewType.Witness;
        CreatedDate = DateTime.UtcNow;
        UpdatedDate = DateTime.UtcNow;
    }

    #region Core Properties

    public string Code { get; set; } = string.Empty;
    public string InvestigationCode { get; set; } = string.Empty; // FK to Investigation
    public string SMSInvestigatorCode { get; set; } = string.Empty; // FK to SMS User conducting interview

    #endregion

    #region Interview Details

    public string PersonInterviewed { get; set; } = string.Empty;
    public string? PersonInterviewedRole { get; set; } // Role/position of interviewee
    public string? PersonInterviewedDepartment { get; set; }
    public string? PersonInterviewedNotes { get; set; } // Notes from the interviewee
    public string? InvestigatorNotes { get; set; } // Investigator's observations and analysis

    #endregion

    #region Interview Management

    public InterviewStatus Status { get; set; } = InterviewStatus.Planned;
    public DateTime? InterviewDate { get; set; }
    public int? DurationMinutes { get; set; }
    public string? InterviewLocation { get; set; }
    public InterviewType Type { get; set; } = InterviewType.Witness;
    public bool IsConfidential { get; set; } = false;

    #endregion

    #region Interview Preparation

    public string? PreparationNotes { get; set; }
    public string? QuestionsToAsk { get; set; }
    public string? BackgroundInformation { get; set; }

    #endregion

    #region Interview Results

    public string? KeyFindings { get; set; }
    public string? FollowUpRequired { get; set; }
    public string? AdditionalWitnesses { get; set; } // Names of additional witnesses mentioned
    public DateTime? CompletedDate { get; set; }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Create a new interview for an investigation
    /// </summary>
    public static Result<Interview> CreateForInvestigation(string investigationCode, string personInterviewed, string investigatorCode, InterviewType? type = null)
    {
        if (string.IsNullOrWhiteSpace(investigationCode))
        {
            return Result<Interview>.Failure<Interview>(DomainErrors.InterviewError.InvestigationCodeRequired);
        }

        if (string.IsNullOrWhiteSpace(personInterviewed))
        {
            return Result<Interview>.Failure<Interview>(DomainErrors.InterviewError.PersonInterviewedRequired);
        }

        if (string.IsNullOrWhiteSpace(investigatorCode))
        {
            return Result<Interview>.Failure<Interview>(DomainErrors.InterviewError.InvestigatorRequired);
        }

        var code = GenerateCode(investigationCode);
        var id = new InterviewID(code);
        var interview = new Interview(id, code, investigationCode, personInterviewed, investigatorCode)
        {
            Type = type ?? InterviewType.Witness
        };

        return Result<Interview>.Success(interview);
    }

    /// <summary>
    /// Create interview from existing data (for migration/import)
    /// </summary>
    public static Result<Interview> CreateFromData(string code, string investigationCode, string personInterviewed, 
        string investigatorCode, string? personInterviewedNotes = null, string? investigatorNotes = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result<Interview>.Failure<Interview>(DomainErrors.InterviewError.CodeRequired);
        }

        var id = new InterviewID(code);
        var interview = new Interview(id, code, investigationCode, personInterviewed, investigatorCode)
        {
            PersonInterviewedNotes = personInterviewedNotes,
            InvestigatorNotes = investigatorNotes
        };

        return Result<Interview>.Success(interview);
    }

    #endregion

    #region Domain Behavior Methods

    /// <summary>
    /// Schedule the interview
    /// </summary>
    public Result<bool> ScheduleInterview(DateTime interviewDate, string location, int? estimatedDurationMinutes = null)
    {
        if (!Status.AllowsModifications())
        {
            return Result<bool>.Failure<bool>(DomainErrors.InterviewError.CannotModifyCompleted);
        }

        if (interviewDate <= DateTime.UtcNow)
        {
            return Result<bool>.Failure<bool>(DomainErrors.InterviewError.InterviewDateMustBeFuture);
        }

        InterviewDate = interviewDate;
        InterviewLocation = location;
        DurationMinutes = estimatedDurationMinutes ?? Type.GetRecommendedMinimumDurationMinutes();
        Status = InterviewStatus.Scheduled;
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Add preparation notes and questions
    /// </summary>
    public Result<bool> AddPreparation(string? preparationNotes, string? questionsToAsk, string? backgroundInfo = null)
    {
        if (!Status.AllowsModifications())
        {
            return Result<bool>.Failure<bool>(DomainErrors.InterviewError.CannotModifyCompleted);
        }

        PreparationNotes = preparationNotes;
        QuestionsToAsk = questionsToAsk;
        BackgroundInformation = backgroundInfo;
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Start the interview
    /// </summary>
    public Result<bool> StartInterview()
    {
        if (!Status.CanStart())
        {
            return Result<bool>.Failure<bool>(DomainErrors.InterviewError.MustBeScheduled);
        }

        Status = InterviewStatus.InProgress;
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Complete the interview with results
    /// </summary>
    public Result<bool> CompleteInterview(string? personInterviewedNotes, string? investigatorNotes, 
        string? keyFindings = null, string? followUpRequired = null, string? additionalWitnesses = null)
    {
        if (!Status.CanComplete())
        {
            return Result<bool>.Failure<bool>(DomainErrors.InterviewError.CannotComplete);
        }

        PersonInterviewedNotes = personInterviewedNotes;
        InvestigatorNotes = investigatorNotes;
        KeyFindings = keyFindings;
        FollowUpRequired = followUpRequired;
        AdditionalWitnesses = additionalWitnesses;
        
        Status = InterviewStatus.Completed;
        CompletedDate = DateTime.UtcNow;
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Cancel the interview
    /// </summary>
    public Result<bool> CancelInterview(string reason)
    {
        if (!Status.CanCancel())
        {
            return Result<bool>.Failure<bool>(DomainErrors.InterviewError.CannotModifyCompleted);
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result<bool>.Failure<bool>(DomainErrors.InterviewError.CancellationReasonRequired);
        }

        Status = InterviewStatus.Cancelled;
        InvestigatorNotes = $"{InvestigatorNotes}\n\n[CANCELLED]: {reason}";
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Update interview details
    /// </summary>
    public Result<bool> UpdateDetails(string? personRole, string? personDepartment, string? description = null)
    {
        if (!Status.AllowsModifications())
        {
            return Result<bool>.Failure<bool>(DomainErrors.InterviewError.CannotModifyCompleted);
        }

        PersonInterviewedRole = personRole;
        PersonInterviewedDepartment = personDepartment;
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Set interview confidentiality
    /// </summary>
    public Result<bool> SetConfidentiality(bool isConfidential, string? reason = null)
    {
        IsConfidential = isConfidential;
        
        if (isConfidential && !string.IsNullOrWhiteSpace(reason))
        {
            InvestigatorNotes = $"{InvestigatorNotes}\n\n[CONFIDENTIAL]: {reason}";
        }

        UpdatedDate = DateTime.UtcNow;
        return Result<bool>.Success(true);
    }

    #endregion

    #region Query Methods

    /// <summary>
    /// Check if interview is completed
    /// </summary>
    public bool IsCompleted => Status.IsComplete();

    /// <summary>
    /// Check if interview is scheduled
    /// </summary>
    public bool IsScheduled => Status == InterviewStatus.Scheduled;

    /// <summary>
    /// Check if interview is in progress
    /// </summary>
    public bool IsInProgress => Status == InterviewStatus.InProgress;

    /// <summary>
    /// Check if interview is cancelled
    /// </summary>
    public bool IsCancelled => Status.IsCancelled();

    /// <summary>
    /// Check if interview has findings
    /// </summary>
    public bool HasFindings => !string.IsNullOrWhiteSpace(KeyFindings);

    /// <summary>
    /// Check if follow-up is required
    /// </summary>
    public bool RequiresFollowUp => !string.IsNullOrWhiteSpace(FollowUpRequired);

    /// <summary>
    /// Check if additional witnesses were mentioned
    /// </summary>
    public bool HasAdditionalWitnesses => !string.IsNullOrWhiteSpace(AdditionalWitnesses);

    /// <summary>
    /// Get interview duration in minutes (estimated or actual)
    /// </summary>
    public int? GetDurationMinutes()
    {
        if (DurationMinutes.HasValue)
        {
            return DurationMinutes.Value;
        }

        if (InterviewDate.HasValue && CompletedDate.HasValue)
        {
            return (int)(CompletedDate.Value - InterviewDate.Value).TotalMinutes;
        }

        return null;
    }

    /// <summary>
    /// Get formatted interview summary
    /// </summary>
    public string GetInterviewSummary()
    {
        return $"{PersonInterviewed} ({Type.Name}) - {Status.Name} - {InterviewDate:yyyy-MM-dd}";
    }

    /// <summary>
    /// Get preparation guidelines for this interview type
    /// </summary>
    public string GetPreparationGuidelines()
    {
        return Type.GetPreparationGuidelines();
    }

    /// <summary>
    /// Get status display description
    /// </summary>
    public string GetStatusDescription()
    {
        return Status.GetDisplayDescription();
    }

    #endregion

    #region Private Helper Methods

    private static string GenerateCode(string investigationCode)
    {
        // Extract investigation ID portion for linking
        var investigationId = investigationCode.Replace("INV-", "");
        var timestamp = DateTime.UtcNow.ToString("HHmmss");
        return $"INT-{investigationId}-{timestamp}";
    }

    #endregion
}
