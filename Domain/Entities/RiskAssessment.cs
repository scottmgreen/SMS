using SMS_Shared.Common;
using SMS_Domain.ValueObjects;

namespace SMS_Domain.Entities;

/// <summary>
/// Risk Assessment Aggregate Root - CLEANED UP VERSION
/// Manages the complete 5-step risk assessment workflow with simplified properties
/// NO PRIVATE SETTERS - Direct property access for simplicity
/// </summary>
public sealed class RiskAssessment : BaseAuditableEntity
{
    // Simplified collections - remove complexity
    private readonly List<string> _identifiedHazardIds = new();
    private readonly List<string> _stakeholderIds = new();
    private readonly List<int> _completedSteps = new();

        
    // Public constructor following the pattern
    public RiskAssessment(RiskAssessmentID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    // Private constructor for creation
    private RiskAssessment(RiskAssessmentID id, string name, string leadAssessorId, RiskAssessmentType assessmentType = RiskAssessmentType.Initial, string? primaryHazardId = null, string? hazardCode = null) 
        : base(id, "SYSTEM", DateTime.UtcNow)
    {
        Name = name;
        LeadAssessorId = leadAssessorId;
        PrimaryHazardId = primaryHazardId;
        HazardCode = hazardCode;
        Status = RiskAssessmentStatus.Created;
        AssessmentType = assessmentType;
        RiskAssessmentCategory = assessmentType == RiskAssessmentType.Initial ? RiskAssessmentCategory.FiveStep : RiskAssessmentCategory.Residual;
        CurrentStep = 1;
        UpdatedDate = DateTime.UtcNow;
    }

    // ✅ SIMPLIFIED PROPERTIES - NO PRIVATE SETTERS!
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? HazardCode { get; set; }
    public string LeadAssessorId { get; set; } = string.Empty;
    public string? PrimaryHazardId { get; set; }
    public RiskAssessmentStatus Status { get; set; }
    public RiskAssessmentType AssessmentType { get; set; }
    public RiskAssessmentCategory RiskAssessmentCategory { get; set; }
    public int CurrentStep { get; set; } = 1;
    public string? Stage { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? CompletedBy { get; set; }
    public string? ParentAssessmentId { get; set; }

    // Risk Scoring Properties
    public int? FinalSeverityScore { get; set; }
    public int? FinalLikelihoodScore { get; set; }
    public string? FinalRiskLevel { get; set; }
    public string? RiskTolerability { get; set; } = "ALARP";
    public string? AssessmentRationale { get; set; }

    // Step 1 - System Description Properties - ✅ PUBLIC SETTERS
    public string SystemDescription { get; set; } = string.Empty;
    public string SystemBoundaries { get; set; } = string.Empty;
    public string SystemPurpose { get; set; } = string.Empty;

    // 5M Framework Fields - ✅ PUBLIC SETTERS
    public string FiveMPersonnel { get; set; } = string.Empty;
    public string FiveMEquipment { get; set; } = string.Empty;
    public string FiveMProcedures { get; set; } = string.Empty;
    public string FiveMResources { get; set; } = string.Empty;
    public string FiveMPhysicalEnvironment { get; set; } = string.Empty;
    public string FiveMOperationalEnvironment { get; set; } = string.Empty;

    // Step 3 - Risk Analysis Properties - ✅ PUBLIC SETTERS
    public string RiskAnalysisMethod { get; set; } = "SMS Risk Matrix";
    public string RiskCriteria { get; set; } = string.Empty;

    // Step 4 - Risk Assessment Properties - ✅ PUBLIC SETTERS
    public string TolerabilityFramework { get; set; } = "PDX-SMS Default";
    public string RiskAcceptanceCriteria { get; set; } = string.Empty;

    // Step 5 - Implementation Properties - ✅ PUBLIC SETTERS
    public string ImplementationStrategy { get; set; } = string.Empty;
    public DateTime? OverallTargetDate { get; set; }
    public string ImplementationNotes { get; set; } = string.Empty;

    // Read-only collections
    public IReadOnlyList<string> IdentifiedHazardIds => _identifiedHazardIds.AsReadOnly();
    public IReadOnlyList<string> StakeholderIds => _stakeholderIds.AsReadOnly();
    public IReadOnlyList<int> CompletedSteps => _completedSteps.AsReadOnly();

    /// <summary>
    /// Factory method to create new Initial Risk Assessment
    /// </summary>
    public static Result<RiskAssessment> CreateInitial(RiskAssessmentID id, string name, string leadAssessorId, RiskAssessmentCategory category = RiskAssessmentCategory.FiveStep, string? primaryHazardId = null, string? hazardCode = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.InvalidName);
        }

        if (string.IsNullOrWhiteSpace(leadAssessorId))
        {
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.InvalidLeadAssessor);
        }

        var assessment = new RiskAssessment(id, name, leadAssessorId, RiskAssessmentType.Initial, primaryHazardId, hazardCode);
        assessment.RiskAssessmentCategory = category;
        assessment.Stage = "Created";
        assessment.Code = $"RA-0000";
        
        return Result<RiskAssessment>.Success(assessment);
    }

    /// <summary>
    /// Factory method to create new Residual Risk Assessment (after mitigations)
    /// </summary>
    public static Result<RiskAssessment> CreateResidual(RiskAssessmentID id, string name, string leadAssessorId, string parentAssessmentId, string? primaryHazardId = null, string? hazardCode = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.InvalidName);
        }

        if (string.IsNullOrWhiteSpace(leadAssessorId))
        {
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.InvalidLeadAssessor);
        }

        if (string.IsNullOrWhiteSpace(parentAssessmentId))
        {
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.InvalidParentAssessment);
        }

        var assessment = new RiskAssessment(id, name, leadAssessorId, RiskAssessmentType.Residual, primaryHazardId, hazardCode);
        assessment.ParentAssessmentId = parentAssessmentId;
        assessment.RiskAssessmentCategory = RiskAssessmentCategory.Residual;
        assessment.Stage = "Created";
        assessment.Code = $"RRA-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        
        return Result<RiskAssessment>.Success(assessment);
    }

    /// <summary>
    /// Factory method for backward compatibility - defaults to Initial assessment
    /// </summary>
    public static Result<RiskAssessment> Create(RiskAssessmentID id, string name, string leadAssessorId, string? primaryHazardId = null)
    {
        return CreateInitial(id, name, leadAssessorId, RiskAssessmentCategory.FiveStep, primaryHazardId);
    }

    // ✅ SIMPLIFIED METHODS - ONLY THE ONES ACTUALLY BEING USED!

    /// <summary>
    /// Add Stakeholder - SIMPLIFIED
    /// </summary>
    public void AddStakeholder(string stakeholderId)
    {
        if (!string.IsNullOrWhiteSpace(stakeholderId) && !_stakeholderIds.Contains(stakeholderId))
        {
            _stakeholderIds.Add(stakeholderId);
            UpdatedDate = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Add Identified Hazard - SIMPLIFIED
    /// </summary>
    public void AddIdentifiedHazard(string hazardId, string description)
    {
        if (!string.IsNullOrWhiteSpace(hazardId) && !_identifiedHazardIds.Contains(hazardId))
        {
            _identifiedHazardIds.Add(hazardId);
            UpdatedDate = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Clear all identified hazards - SIMPLIFIED
    /// </summary>
    public void ClearIdentifiedHazards()
    {
        _identifiedHazardIds.Clear();
        UpdatedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Complete a step - SIMPLIFIED
    /// </summary>
    public void CompleteStep(int stepNumber)
    {
        if (stepNumber >= 1 && stepNumber <= 5 && !_completedSteps.Contains(stepNumber))
        {
            _completedSteps.Add(stepNumber);
            _completedSteps.Sort();
            CurrentStep = Math.Max(CurrentStep, stepNumber);
            UpdatedDate = DateTime.UtcNow;

            // Update status based on progress
            if (_completedSteps.Count == 5)
            {
                Status = RiskAssessmentStatus.ReadyForReview;
                Stage = "Ready for Review";
            }
            else
            {
                Status = RiskAssessmentStatus.InProgress;
                Stage = $"Step {stepNumber} Complete";
            }
        }
    }

    /// <summary>
    /// Check if step is completed - USED BY WIZARD
    /// </summary>
    public bool IsStepCompleted(int stepNumber)
    {
        return _completedSteps.Contains(stepNumber);
    }

    /// <summary>
    /// Get next recommended step - USED BY WIZARD
    /// </summary>
    public int GetNextRecommendedStep()
    {
        for (int i = 1; i <= 5; i++)
        {
            if (!_completedSteps.Contains(i))
            {
                return i;
            }
        }
        return 5; // All completed
    }
}

/// <summary>
/// Risk Assessment Status Enumeration
/// </summary>
public enum RiskAssessmentStatus
{
    Created,
    InProgress,
    ReadyForReview,
    UnderReview,
    Completed,
    Cancelled,
    OnHold
}

/// <summary>
/// Risk Assessment Type Enumeration
/// </summary>
public enum RiskAssessmentType
{
    Initial,   // Original risk assessment before mitigations
    Residual   // Risk assessment after mitigations have been implemented
}

/// <summary>
/// Risk Assessment Category Enumeration
/// </summary>
public enum RiskAssessmentCategory
{
    FiveStep,    // Full 5-step SMS process
    Simplified,  // Simplified risk assessment
    Technical,   // Technical risk assessment (TRA)
    Emergency,   // Emergency assessment
    Residual     // Residual risk after mitigation
}
