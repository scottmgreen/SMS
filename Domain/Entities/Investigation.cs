using SMS_Shared.Common;

namespace SMS_Domain.Entities;

/// <summary>
/// Investigation Domain Entity
/// Maps to tbld_Investigations table
/// </summary>
public sealed class Investigation : BaseAuditableEntity
{
    // Public constructor for instantiation
    public Investigation(InvestigationID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    // Simple constructor pattern as requested
    private Investigation(InvestigationID id, string code) : base(id, "SYSTEM", DateTime.UtcNow) 
    {
        Code = code;
    }

    #region Properties

    public string Code { get; set; } = string.Empty;
    public string? ReportCode { get; set; }
    public string? InvestigationNotes { get; set; }
    public string HazardCode { get; set; } = "HAZ-UNKNOWN";
    public string AssignedInvestigatorId { get; set; } = "UNASSIGNED";
    public string Status { get; set; } = "Assigned";
    public DateTime? CompletedDate { get; set; }
    public string? InvestigationPlan { get; set; }
    public string? InvestigationObjectives { get; set; }
    public string? DecisionType { get; set; }
    public string? DecisionRationale { get; set; }
    public string? DecisionMaker { get; set; }
    public DateTime? DecisionDate { get; set; }
    public string? NextSteps { get; set; }
    public string? ReferralDetails { get; set; }

    #endregion

    #region Factory Methods

    //public static Investigation CreateForHazard(string code, string hazardCode, string assignedInvestigatorId, string? reportCode = null)
    //{
    //    // Validate required parameters
    //    if (string.IsNullOrWhiteSpace(code))
    //        throw new ArgumentException("Investigation code is required", nameof(code));
        
    //    if (string.IsNullOrWhiteSpace(hazardCode))
    //        throw new ArgumentException("HazardCode is required and cannot be null", nameof(hazardCode));
        
    //    if (string.IsNullOrWhiteSpace(assignedInvestigatorId))
    //        throw new ArgumentException("Assigned investigator ID is required", nameof(assignedInvestigatorId));

    //    var id = new InvestigationID(code);
    //    return new Investigation(id, code)
    //    {
    //        HazardCode = hazardCode, // Critical: Set HazardCode explicitly
    //        AssignedInvestigatorId = assignedInvestigatorId,
    //        ReportCode = reportCode,
    //        Status = "Assigned"
    //    };
    //}

    #endregion

    #region Domain Methods

    public void UpdateDetails(string? notes, string? plan = null, string? objectives = null)
    {
        InvestigationNotes = notes;
        InvestigationPlan = plan;
        InvestigationObjectives = objectives;
    }

    public void AssignTo(string investigatorId)
    {
        AssignedInvestigatorId = investigatorId;
    }

    public void Start()
    {
        Status = "InProgress";
    }

    public void PutOnHold()
    {
        Status = "OnHold";
    }

    public void Cancel()
    {
        Status = "Cancelled";
    }

    public void RecordDecision(string decisionType, string rationale, string decisionMaker, string? nextSteps = null, string? referralDetails = null)
    {
        DecisionType = decisionType;
        DecisionRationale = rationale;
        DecisionMaker = decisionMaker;
        DecisionDate = DateTime.UtcNow;
        NextSteps = nextSteps;
        ReferralDetails = referralDetails;
    }

    public void Complete()
    {
        Status = "Completed";
        CompletedDate = DateTime.UtcNow;
    }

    // Alias for backward compatibility
    public void CompleteInvestigation() => Complete();

    #endregion

    #region Query Properties

    public bool HasDecision => !string.IsNullOrWhiteSpace(DecisionType);
    public bool IsCompleted => Status == "Completed";
    public bool IsInProgress => Status == "InProgress";
    public bool IsAssigned => Status == "Assigned";
    public bool IsOnHold => Status == "OnHold";
    public bool IsCancelled => Status == "Cancelled";

    public string StatusDisplay => Status switch
    {
        "Assigned" => "Assigned",
        "InProgress" => "In Progress",
        "OnHold" => "On Hold",
        "Completed" => "Completed",
        "Cancelled" => "Cancelled",
        _ => Status
    };

    public string NextStepsMessage => DecisionType switch
    {
        "NoFurtherAction" => "Investigation closed - no further action required.",
        "ContinueMonitoring" => "Hazard will continue to be monitored.",
        "RequiresMitigation" => "Hazard requires mitigation measures.",
        "EscalateToRiskAssessment" => "Hazard escalated to risk assessment.",
        "ReferToExternalAgency" => "Hazard referred to external agency.",
        _ => "Investigation decision pending."
    };

    // Alias for backward compatibility
    public string GetNextStepMessage() => NextStepsMessage;

    #endregion
}
