namespace SMS_Domain.Entities;

/// <summary>
/// Mitigation Domain Entity - Comprehensive implementation matching tbld_Mitigations schema
/// Represents a risk mitigation strategy with complete lifecycle management
/// </summary>
public sealed class Mitigation : BaseAuditableEntity
{
    #region Constructors

    // Constructor for Entity Framework
    private Mitigation() : base(new MitigationID(Guid.NewGuid().ToString()), "SYSTEM", DateTime.UtcNow) { }

    // Public constructor following domain pattern
    public Mitigation(MitigationID id) : base(id, "SYSTEM", DateTime.UtcNow)
    {
        // Set default values to match database defaults
        Status = "Proposed";
        Progress = 0;
        HasDependencies = false;
        IsPrerequisite = false;
        ValidationRequired = false;
    }

    #endregion

    #region Core Properties (Required fields)

    /// <summary>Business identifier for the mitigation</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Associated hazard code - links to Hazard entity</summary>
    public string HazardCode { get; set; } = string.Empty;

    /// <summary>Descriptive name of the mitigation</summary>
    public string? Name { get; set; }

    /// <summary>Detailed description of the mitigation strategy</summary>
    public string? Description { get; set; }

    #endregion

    #region Classification Properties

    /// <summary>Type of mitigation (Engineering, Administrative, PPE, etc.)</summary>
    public string? Type { get; set; }

    /// <summary>Current status (Proposed, Approved, InProgress, Completed, Cancelled, OnHold)</summary>
    public string Status { get; set; } = "Proposed";

    /// <summary>Priority level (Critical, High, Medium, Low)</summary>
    public string? Priority { get; set; }

    /// <summary>Associated risk assessment code</summary>
    public string? RiskAssessmentCode { get; set; }

    #endregion

    #region Timeline Properties

    /// <summary>Target completion date</summary>
    public DateTime? TargetDate { get; set; }

    /// <summary>Actual implementation start date</summary>
    public DateTime? ImplementationDate { get; set; }

    /// <summary>Actual completion date</summary>
    public DateTime? CompletionDate { get; set; }

    #endregion

    #region Assignment Properties

    /// <summary>Department responsible for implementation</summary>
    public string? AssignedDepartment { get; set; }

    /// <summary>Person assigned to implement the mitigation</summary>
    public string? AssignedTo { get; set; }

    /// <summary>Person who approved this mitigation</summary>
    public string? ApprovedBy { get; set; }

    /// <summary>Date the mitigation was approved</summary>
    public DateTime? ApprovedDate { get; set; }

    #endregion

    #region Progress Tracking

    /// <summary>Implementation progress percentage (0-100)</summary>
    public int Progress { get; set; } = 0;

    /// <summary>Progress notes and updates</summary>
    public string? ProgressNotes { get; set; }

    /// <summary>Date of last progress update</summary>
    public DateTime? LastProgressUpdate { get; set; }

    /// <summary>Person who last updated progress</summary>
    public string? ProgressUpdatedBy { get; set; }

    #endregion

    #region Cost and Resource Properties

    /// <summary>Estimated cost for implementation</summary>
    public decimal? EstimatedCost { get; set; }

    /// <summary>Actual cost incurred</summary>
    public decimal? ActualCost { get; set; }

    /// <summary>Resource requirements description</summary>
    public string? ResourceRequirements { get; set; }

    /// <summary>Estimated hours for implementation</summary>
    public int? EstimatedHours { get; set; }

    /// <summary>Actual hours spent</summary>
    public int? ActualHours { get; set; }

    #endregion

    #region Effectiveness Properties

    /// <summary>Effectiveness rating after implementation</summary>
    public string? EffectivenessRating { get; set; }

    /// <summary>Notes on effectiveness assessment</summary>
    public string? EffectivenessNotes { get; set; }

    /// <summary>Date of effectiveness review</summary>
    public DateTime? EffectivenessReviewDate { get; set; }

    /// <summary>Person who reviewed effectiveness</summary>
    public string? EffectivenessReviewedBy { get; set; }

    #endregion

    #region Monitoring Properties

    /// <summary>Monitoring requirements for ongoing effectiveness</summary>
    public string? MonitoringRequirements { get; set; }

    /// <summary>Frequency of monitoring (Daily, Weekly, Monthly, etc.)</summary>
    public string? MonitoringFrequency { get; set; }

    #endregion

    #region Risk Reduction Properties

    /// <summary>Expected severity reduction (1-5 scale)</summary>
    public int? ExpectedSeverityReduction { get; set; }

    /// <summary>Expected likelihood reduction (1-5 scale)</summary>
    public int? ExpectedLikelihoodReduction { get; set; }

    /// <summary>Actual severity reduction achieved</summary>
    public int? ActualSeverityReduction { get; set; }

    /// <summary>Actual likelihood reduction achieved</summary>
    public int? ActualLikelihoodReduction { get; set; }

    /// <summary>Residual risk level after implementation</summary>
    public string? ResidualRiskLevel { get; set; }

    #endregion

    #region Dependency Properties

    /// <summary>Prerequisites that must be met before implementation</summary>
    public string? Prerequisites { get; set; }

    /// <summary>Dependencies on other mitigations or activities</summary>
    public string? Dependencies { get; set; }

    /// <summary>Indicates if this mitigation has dependencies</summary>
    public bool HasDependencies { get; set; } = false;

    /// <summary>Indicates if this mitigation is a prerequisite for others</summary>
    public bool IsPrerequisite { get; set; } = false;

    #endregion

    #region Implementation Planning

    /// <summary>Detailed implementation plan</summary>
    public string? ImplementationPlan { get; set; }

    /// <summary>Communication plan for the mitigation</summary>
    public string? CommunicationPlan { get; set; }

    /// <summary>Training requirements for implementation</summary>
    public string? TrainingRequirements { get; set; }

    /// <summary>Documentation that needs to be updated</summary>
    public string? DocumentationUpdates { get; set; }

    #endregion

    #region Testing and Validation

    /// <summary>Testing procedure to validate effectiveness</summary>
    public string? TestingProcedure { get; set; }

    /// <summary>Date testing was completed</summary>
    public DateTime? TestingCompletedDate { get; set; }

    /// <summary>Results of testing</summary>
    public string? TestingResults { get; set; }

    /// <summary>Indicates if validation is required</summary>
    public bool ValidationRequired { get; set; } = false;

    /// <summary>Date validation was completed</summary>
    public DateTime? ValidationDate { get; set; }

    /// <summary>Person who validated the mitigation</summary>
    public string? ValidatedBy { get; set; }

    #endregion

    #region Additional Properties

    /// <summary>General notes about the mitigation</summary>
    public string? Notes { get; set; }

    /// <summary>Lessons learned from implementation</summary>
    public string? LessonsLearned { get; set; }

    /// <summary>Recommendations for future similar mitigations</summary>
    public string? RecommendationsForFuture { get; set; }

    #endregion

    #region Domain Methods

    /// <summary>
    /// Factory method to create new mitigation
    /// </summary>
    public static Result<Mitigation> Create(string code, string hazardCode, string name, string description)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.InvalidCode);
        }

        if (string.IsNullOrWhiteSpace(hazardCode))
        {
            return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.InvalidHazardCode);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.InvalidName);
        }

        var id = new MitigationID(code);
        var mitigation = new Mitigation(id)
        {
            Code = code,
            HazardCode = hazardCode,
            Name = name,
            Description = description
        };

        return Result<Mitigation>.Success(mitigation);
    }

    /// <summary>
    /// Update progress and status
    /// </summary>
    public Result<bool> UpdateProgress(int progressPercentage, string? notes = null, string? updatedBy = null)
    {
        if (progressPercentage < 0 || progressPercentage > 100)
        {
            return Result<bool>.Failure<bool>(DomainErrors.MitigationError.InvalidProgress);
        }

        Progress = progressPercentage;
        ProgressNotes = notes;
        LastProgressUpdate = DateTime.UtcNow;
        ProgressUpdatedBy = updatedBy;

        // Auto-update status based on progress
        if (progressPercentage == 100)
        {
            Status = "Completed";
            CompletionDate = DateTime.UtcNow;
        }
        else if (progressPercentage > 0 && Status == "Proposed")
        {
            Status = "InProgress";
            ImplementationDate ??= DateTime.UtcNow;
        }

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Approve the mitigation
    /// </summary>
    public Result<bool> Approve(string approvedBy, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(approvedBy))
        {
            return Result<bool>.Failure<bool>(DomainErrors.MitigationError.InvalidApprover);
        }

        Status = "Approved";
        ApprovedBy = approvedBy;
        ApprovedDate = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(notes))
        {
            Notes = string.IsNullOrEmpty(Notes) ? notes : $"{Notes}\n\nApproval Notes: {notes}";
        }

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Mark as completed
    /// </summary>
    public Result<bool> Complete(string? completedBy = null, string? notes = null)
    {
        Status = "Completed";
        Progress = 100;
        CompletionDate = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(completedBy))
        {
            ProgressUpdatedBy = completedBy;
        }

        if (!string.IsNullOrEmpty(notes))
        {
            ProgressNotes = notes;
        }

        LastProgressUpdate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Cancel the mitigation
    /// </summary>
    public Result<bool> Cancel(string reason, string? cancelledBy = null)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result<bool>.Failure<bool>(DomainErrors.MitigationError.InvalidCancellationReason);
        }

        Status = "Cancelled";
        Notes = string.IsNullOrEmpty(Notes) ? $"Cancelled: {reason}" : $"{Notes}\n\nCancelled: {reason}";

        if (!string.IsNullOrEmpty(cancelledBy))
        {
            ProgressUpdatedBy = cancelledBy;
        }

        LastProgressUpdate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Check if mitigation is overdue
    /// </summary>
    public bool IsOverdue()
    {
        return TargetDate.HasValue &&
               TargetDate < DateTime.UtcNow &&
               Status != "Completed" &&
               Status != "Cancelled";
    }

    /// <summary>
    /// Get days remaining until target date
    /// </summary>
    public int? GetDaysRemaining()
    {
        if (!TargetDate.HasValue || Status == "Completed" || Status == "Cancelled")
            return null;

        return (int)(TargetDate.Value - DateTime.UtcNow).TotalDays;
    }

    /// <summary>
    /// Calculate cost variance
    /// </summary>
    public decimal? GetCostVariance()
    {
        if (!EstimatedCost.HasValue || !ActualCost.HasValue)
            return null;

        return ActualCost.Value - EstimatedCost.Value;
    }

    /// <summary>
    /// Calculate cost variance percentage
    /// </summary>
    public decimal? GetCostVariancePercentage()
    {
        if (!EstimatedCost.HasValue || !ActualCost.HasValue || EstimatedCost.Value == 0)
            return null;

        return (ActualCost.Value - EstimatedCost.Value) / EstimatedCost.Value * 100;
    }

    #endregion
}
