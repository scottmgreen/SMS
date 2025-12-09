using SMS_Shared.Common;

namespace SMS_Domain.Entities;

/// <summary>
/// Investigation Domain Entity - Enhanced for comprehensive investigation workflow
/// Represents hazard investigations with full evidence management and decision tracking
/// </summary>
public sealed class Investigation : BaseAuditableEntity
{
    // Private constructor for Entity Framework
    private Investigation() : base(new InvestigationID(Guid.NewGuid().ToString()), "SYSTEM", DateTime.UtcNow) { }

    // Public constructor for domain usage
    public Investigation(InvestigationID id) : base(id, "SYSTEM", DateTime.UtcNow) 
    {
        Status = InvestigationStatus.InProgress;
        CreatedDate = DateTime.UtcNow;
        UpdatedDate = DateTime.UtcNow;
    }

    // Private constructor for creation with validation
    private Investigation(InvestigationID id, string code, string hazardCode, string assignedInvestigatorId)
        : base(id, "SYSTEM", DateTime.UtcNow)
    {
        Code = code;
        HazardCode = hazardCode;
        AssignedInvestigatorId = assignedInvestigatorId;
        Status = InvestigationStatus.InProgress;
        CreatedDate = DateTime.UtcNow;
        UpdatedDate = DateTime.UtcNow;
    }

    #region Core Properties

    public string Code { get; set; } = string.Empty;
    public string? ReportCode { get; set; }
    public string HazardCode { get; set; } = string.Empty; // FK to Hazard
    public string? InvestigationNotes { get; set; }

    #endregion

    #region Investigation Management

    public string AssignedInvestigatorId { get; set; } = string.Empty;
    public InvestigationStatus Status { get; set; } = InvestigationStatus.InProgress;
    public DateTime? CompletedDate { get; set; }
    public string? InvestigationPlan { get; set; }
    public string? InvestigationObjectives { get; set; }

    #endregion

    #region Decision Properties

    public string? DecisionType { get; set; } // SMSRisk, NoSMSRisk, RequiresMoreInvestigation, ReferExternal
    public string? DecisionRationale { get; set; }
    public string? DecisionMaker { get; set; }
    public DateTime? DecisionDate { get; set; }
    public string? NextSteps { get; set; }
    public string? ReferralDetails { get; set; } // If referring externally

    #endregion

    #region Evidence Collections - Use HazardFiles for file attachments

    // Investigation relies on HazardFiles with category "Evidence" for file management
    // Interviews are managed through Interview entities linked by InvestigationCode
    
    #endregion

    #region Factory Methods

    /// <summary>
    /// Create a new investigation for a hazard
    /// </summary>
    public static Result<Investigation> CreateForHazard(string hazardCode, string assignedInvestigatorId, string? reportCode = null)
    {
        if (string.IsNullOrWhiteSpace(hazardCode))
        {
            return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.HazardCodeRequired);
        }

        if (string.IsNullOrWhiteSpace(assignedInvestigatorId))
        {
            return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.InvestigatorRequired);
        }

        var code = GenerateCode(hazardCode);
        var id = new InvestigationID(code);
        var investigation = new Investigation(id, code, hazardCode, assignedInvestigatorId)
        {
            ReportCode = reportCode
        };

        return Result<Investigation>.Success(investigation);
    }

    /// <summary>
    /// Create investigation from existing data (for migration/import)
    /// </summary>
    public static Result<Investigation> CreateFromData(string code, string hazardCode, string assignedInvestigatorId,
        string? reportCode = null, string? investigationNotes = null, InvestigationStatus? status = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.CodeRequired);
        }

        var id = new InvestigationID(code);
        var investigation = new Investigation(id, code, hazardCode, assignedInvestigatorId)
        {
            ReportCode = reportCode,
            InvestigationNotes = investigationNotes,
            Status = status ?? InvestigationStatus.InProgress
        };

        return Result<Investigation>.Success(investigation);
    }

    #endregion

    #region Domain Behavior Methods

    /// <summary>
    /// Update investigation notes and plan
    /// </summary>
    public Result<bool> UpdateInvestigationDetails(string? investigationNotes, string? investigationPlan = null, string? objectives = null)
    {
        if (Status == InvestigationStatus.Completed)
        {
            return Result<bool>.Failure<bool>(DomainErrors.InvestigationError.CannotModifyCompleted);
        }

        InvestigationNotes = investigationNotes;
        InvestigationPlan = investigationPlan;
        InvestigationObjectives = objectives;
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Assign investigator to this investigation
    /// </summary>
    public Result<bool> AssignInvestigator(string investigatorId)
    {
        if (Status == InvestigationStatus.Completed)
        {
            return Result<bool>.Failure<bool>(DomainErrors.InvestigationError.CannotModifyCompleted);
        }

        if (string.IsNullOrWhiteSpace(investigatorId))
        {
            return Result<bool>.Failure<bool>(DomainErrors.InvestigationError.InvestigatorRequired);
        }

        AssignedInvestigatorId = investigatorId;
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Record investigation decision
    /// </summary>
    public Result<bool> RecordDecision(string decisionType, string rationale, string decisionMaker, string? nextSteps = null, string? referralDetails = null)
    {
        if (Status == InvestigationStatus.Completed)
        {
            return Result<bool>.Failure<bool>(DomainErrors.InvestigationError.CannotModifyCompleted);
        }

        if (string.IsNullOrWhiteSpace(decisionType))
        {
            return Result<bool>.Failure<bool>(DomainErrors.InvestigationError.DecisionTypeRequired);
        }

        if (string.IsNullOrWhiteSpace(rationale) || rationale.Length < 20)
        {
            return Result<bool>.Failure<bool>(DomainErrors.InvestigationError.DecisionRationaleRequired);
        }

        if (string.IsNullOrWhiteSpace(decisionMaker))
        {
            return Result<bool>.Failure<bool>(DomainErrors.InvestigationError.DecisionMakerRequired);
        }

        DecisionType = decisionType;
        DecisionRationale = rationale;
        DecisionMaker = decisionMaker;
        DecisionDate = DateTime.UtcNow;
        NextSteps = nextSteps;
        ReferralDetails = referralDetails;
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Complete the investigation
    /// </summary>
    public Result<bool> CompleteInvestigation()
    {
        if (Status == InvestigationStatus.Completed)
        {
            return Result<bool>.Failure<bool>(DomainErrors.InvestigationError.AlreadyCompleted);
        }

        if (string.IsNullOrWhiteSpace(DecisionType))
        {
            return Result<bool>.Failure<bool>(DomainErrors.InvestigationError.DecisionRequired);
        }

        Status = InvestigationStatus.Completed;
        CompletedDate = DateTime.UtcNow;
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Reopen the investigation
    /// </summary>
    public Result<bool> ReopenInvestigation(string reason)
    {
        if (Status != InvestigationStatus.Completed)
        {
            return Result<bool>.Failure<bool>(DomainErrors.InvestigationError.NotCompleted);
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result<bool>.Failure<bool>(DomainErrors.InvestigationError.ReasonRequired);
        }

        Status = InvestigationStatus.InProgress;
        CompletedDate = null;
        UpdatedDate = DateTime.UtcNow;

        // Add reopening reason to notes
        InvestigationNotes = $"{InvestigationNotes}\n\n[REOPENED on {DateTime.UtcNow:yyyy-MM-dd}]: {reason}";

        return Result<bool>.Success(true);
    }

    #endregion

    #region Query Methods

    /// <summary>
    /// Check if investigation has a decision
    /// </summary>
    public bool HasDecision => !string.IsNullOrWhiteSpace(DecisionType);

    /// <summary>
    /// Check if investigation is completed
    /// </summary>
    public bool IsCompleted => Status == InvestigationStatus.Completed;

    /// <summary>
    /// Check if investigation is in progress
    /// </summary>
    public bool IsInProgress => Status == InvestigationStatus.InProgress;

    /// <summary>
    /// Get the next step message based on decision type
    /// </summary>
    public string GetNextStepMessage()
    {
        return DecisionType switch
        {
            "SMSRisk" => "Hazard returned to validation workflow for SMS risk processing.",
            "NoSMSRisk" => "Hazard closed - determined to not constitute an SMS risk.",
            "RequiresMoreInvestigation" => "Investigation continues - additional information gathering required.",
            "ReferExternal" => "Hazard closed - referred to external organization for handling.",
            _ => "Investigation decision pending."
        };
    }

    /// <summary>
    /// Check if this decision type requires external referral
    /// </summary>
    public bool RequiresExternalReferral => DecisionType == "ReferExternal";

    /// <summary>
    /// Check if this decision indicates SMS risk
    /// </summary>
    public bool IndicatesSMSRisk => DecisionType == "SMSRisk";

    #endregion

    #region Private Helper Methods

    private static string GenerateCode(string hazardCode)
    {
        // Extract hazard ID portion for linking
        var hazardId = hazardCode.Replace("HZ-", "").Replace("HAZ-", "");
        return $"INV-{DateTime.UtcNow:yyyyMMdd}-{hazardId}";
    }

    #endregion
}
