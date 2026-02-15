namespace SMS_Domain.Entities;

/// <summary>
/// Investigation Domain Entity
/// Maps to tbld_Investigations table
/// </summary>
public sealed class Investigation : BaseAuditableEntity
{
    // Public constructor for instantiation
    public Investigation(InvestigationID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

      

    #region Properties

    public string Code { get; set; } = string.Empty;
    public string? ReportCode { get; set; }
    public string? InvestigationNotes { get; set; }
    public string HazardCode { get; set; } = "HAZ-UNKNOWN";
    public string AssignedInvestigatorId { get; set; } = "UNASSIGNED";
    public InvestigationStatus Status { get; set; } = InvestigationStatus.InvestigatorAssigned;
    public DateTime? CompletedDate { get; set; }
    public string? InvestigationPlan { get; set; }
    public string? InvestigationObjectives { get; set; }
    public string? DecisionType { get; set; } = string.Empty;
    public string? DecisionRationale { get; set; } = string.Empty;
    public string? DecisionMaker { get; set; } = string.Empty;
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


    
    

    

    #endregion

    #region Query Properties

    public bool HasDecision => !string.IsNullOrWhiteSpace(DecisionType);
    

    

    
    

    #endregion
}
