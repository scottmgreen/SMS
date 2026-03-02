//-----------------------------------------------------------------------
// <copyright file="Interview.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS domain entity representing interview with business rules and lifecycle management.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// Interview Domain Entity - Enhanced for comprehensive investigation interviews
/// Represents witness interviews and stakeholder discussions during hazard investigations
/// </summary>
public sealed class Interview : BaseAuditableEntity
{

    // Public constructor for domain usage
    public Interview(InterviewID id) : base(id, "SYSTEM", DateTime.UtcNow)
    {
        Status = InterviewStatus.IntervieweeIdentified; // ✅ UPDATED: Default to IntervieweeIdentified (first step in new workflow)
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
        Status = InterviewStatus.IntervieweeIdentified; // ✅ UPDATED: Default to IntervieweeIdentified (first step in new workflow)
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

    public InterviewStatus Status { get; set; } = InterviewStatus.IntervieweeIdentified; // ✅ UPDATED: Default to IntervieweeIdentified

    /// <summary>
    /// Interview date and time. This is required for all interviews.
    /// When this property is set, the interview is considered scheduled.
    /// </summary>
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

        var code = "INV-0000";
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
        //if (!Status.AllowsModifications())
        //{
        //    return Result<bool>.Failure<bool>(DomainErrors.InterviewError.CannotModifyCompleted);
        //}

        if (interviewDate <= DateTime.UtcNow)
        {
            return Result<bool>.Failure<bool>(DomainErrors.InterviewError.InterviewDateMustBeFuture);
        }

        InterviewDate = interviewDate;
        InterviewLocation = location;
        DurationMinutes = estimatedDurationMinutes ?? Type.GetRecommendedMinimumDurationMinutes();
        Status = InterviewStatus.InterviewScheduled; // ✅ UPDATED: Set to InterviewScheduled
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Update interview date and time (used for rescheduling)
    /// </summary>
    public Result<bool> UpdateDateTime(DateTime newInterviewDate, int? newDurationMinutes = null)
    {
        //if (!Status.AllowsModifications() && !Status.Equals(InterviewStatus.InterviewScheduled)) // ✅ UPDATED: Use InterviewScheduled
        //{
        //    return Result<bool>.Failure<bool>(DomainErrors.InterviewError.CannotModifyCompleted);
        //}

        // Allow past dates for rescheduling if already scheduled
        if (newInterviewDate <= DateTime.UtcNow && !Status.Equals(InterviewStatus.InterviewScheduled)) // ✅ UPDATED: Use InterviewScheduled
        {
            return Result<bool>.Failure<bool>(DomainErrors.InterviewError.InterviewDateMustBeFuture);
        }

        InterviewDate = newInterviewDate;

        if (newDurationMinutes.HasValue)
        {
            DurationMinutes = newDurationMinutes.Value;
        }

        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    
    /// <summary>
    /// Start the interview
    /// </summary>
    public Result<bool> StartInterview()
    {
        //if (!Status.CanConduct()) // ✅ UPDATED: Use CanConduct() method from new enum
        //{
        //    return Result<bool>.Failure<bool>(DomainErrors.InterviewError.MustBeScheduled);
        //}

        // ✅ NOTE: In the new workflow, there's no "InProgress" status - interview goes directly from Scheduled to Complete
        // So we'll keep the interview as InterviewScheduled until completion
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Complete the interview with results
    /// </summary>
    public Result<bool> CompleteInterview(string? personInterviewedNotes, string? investigatorNotes,
        string? keyFindings = null, string? followUpRequired = null, string? additionalWitnesses = null)
    {
        //if (!Status.IsActiveStatus) // ✅ FIXED: Use IsActiveStatus as property, not method
        //{
        //    return Result<bool>.Failure<bool>(DomainErrors.InterviewError.CannotComplete);
        //}

        PersonInterviewedNotes = personInterviewedNotes;
        InvestigatorNotes = investigatorNotes;
        KeyFindings = keyFindings;
        FollowUpRequired = followUpRequired;
        AdditionalWitnesses = additionalWitnesses;

        Status = InterviewStatus.InterviewComplete; // ✅ UPDATED: Use InterviewComplete
        CompletedDate = DateTime.UtcNow;
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Cancel the interview
    /// </summary>
    public Result<bool> CancelInterview(string reason)
    {
        //if (!Status.CanMarkUnableToConduct()) // ✅ UPDATED: Use CanMarkUnableToConduct() from new enum
        //{
        //    return Result<bool>.Failure<bool>(DomainErrors.InterviewError.CannotModifyCompleted);
        //}

        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result<bool>.Failure<bool>(DomainErrors.InterviewError.CancellationReasonRequired);
        }

        Status = InterviewStatus.UnableToConduct; // ✅ UPDATED: Use UnableToConduct instead of Cancelled
        InvestigatorNotes = $"{InvestigatorNotes}\n\n[UNABLE TO CONDUCT]: {reason}"; // ✅ UPDATED: Change message text
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

   

    

    #endregion

    #region Query Methods

    /// <summary>
    /// Check if interview is completed
    /// </summary>
    public bool IsCompleted => Status == InterviewStatus.InterviewComplete;

    /// <summary>
    /// Check if interview is scheduled
    /// </summary>
    public bool IsScheduled => Status == InterviewStatus.InterviewScheduled; // ✅ UPDATED: Use IsScheduled() method from new enum

    /// <summary>
    /// Check if interview is in progress
    /// </summary>
    public bool IsInProgress => Status == InterviewStatus.InterviewScheduled; // ✅ UPDATED: Map to InterviewScheduled (closest equivalent)

    /// <summary>
    /// Check if interview is cancelled
    /// </summary>
    public bool IsCancelled => Status == InterviewStatus.UnableToConduct; // ✅ UPDATED: Use IsUnableToConduct() method from new enum


   

   
    

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

