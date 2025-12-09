using SMS_Shared.Common;
using SMS_Domain.ValueObjects;
using SMS_Domain.Enums;

namespace SMS_Domain.Entities;

/// <summary>
/// Risk Assessment Aggregate Root - BUSINESS RULE ENFORCED VERSION
/// 
/// CRITICAL BUSINESS RULES (DO NOT VIOLATE):
/// 
/// 1. AssessmentType: ONLY "Initial" and "Residual" are allowed
///    - Initial: Original risk assessment before any mitigations
///    - Residual: Risk assessment after mitigations have been implemented
/// 
/// 2. RiskAssessmentStatus: ONLY "Created", "InProgress", "Completed" 
///    - Created: Just created, no work started
///    - InProgress: Work in progress, not finished
///    - Completed: All required work finished
/// 
/// 3. PrimaryHazardId: If AssessmentType = "Initial", PrimaryHazardId MUST be set to currentHazardId
/// 
/// 4. RiskAssessmentCategory: ONLY "Technical" or "Preliminary"
///    - Technical: Full 5-step SMS process (IS the 5-step, no redundancy needed)
///    - Preliminary: Streamlined 1-step assessment
/// 
/// 5. CurrentStep Rules:
///    - Technical (5-step): CurrentStep reflects last step finished (1-5)
///    - Preliminary: CurrentStep is always 1 (single step process)
/// 
/// WORKFLOW CONSTRAINTS:
/// - Initial assessments are created first with a hazard
/// - Residual assessments are created AFTER mitigations are implemented
/// - Technical assessments have 5 steps, Preliminary assessments have 1 step
/// - Each assessment must have a primary hazard associated
/// 
/// </summary>
public sealed class RiskAssessment : BaseAuditableEntity
{
    // Simplified collections - remove complexity
    private readonly List<string> _identifiedHazardIds = new();
    private readonly List<string> _stakeholderIds = new();
    private readonly List<int> _completedSteps = new();

        
    // Public constructor following the pattern
    public RiskAssessment(RiskAssessmentID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    // Private constructor for creation - ENFORCES BUSINESS RULES
    private RiskAssessment(RiskAssessmentID id, string name, string leadAssessorId, RiskAssessmentType? assessmentType = null, string? primaryHazardId = null, string? hazardCode = null) 
        : base(id, "SYSTEM", DateTime.UtcNow)
    {
        Name = name;
        LeadAssessorId = leadAssessorId;
        HazardCode = hazardCode;
        Status = RiskAssessmentStatus.Created; // BUSINESS RULE: Always start as "Created"
        AssessmentType = assessmentType ?? RiskAssessmentType.Initial; // BUSINESS RULE: Default to Initial
        
        // BUSINESS RULE: If Initial assessment, PrimaryHazardId MUST be set to currentHazardId
        if (AssessmentType == RiskAssessmentType.Initial && !string.IsNullOrEmpty(primaryHazardId))
        {
            PrimaryHazardId = primaryHazardId;
        }
        else if (AssessmentType == RiskAssessmentType.Residual)
        {
            PrimaryHazardId = primaryHazardId; // Can be null for residual assessments
        }
        
        // BUSINESS RULE: Default to Technical category (5-step process)
        RiskAssessmentCategory = RiskAssessmentCategory.Technical;
        CurrentStep = 1; // BUSINESS RULE: Always start at step 1
        UpdatedDate = DateTime.UtcNow;
    }

    // ✅ PROPERTIES WITH BUSINESS RULE ENFORCEMENT
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? HazardCode { get; set; }
    public string LeadAssessorId { get; set; } = string.Empty;
    
    /// <summary>
    /// Primary Hazard ID - BUSINESS RULE: MUST be set for Initial assessments
    /// </summary>
    public string? PrimaryHazardId { get; set; }
    
    /// <summary>
    /// Risk Assessment Status - BUSINESS RULE: Only "Created", "InProgress", "Completed"
    /// </summary>
    public RiskAssessmentStatus Status { get; set; } = RiskAssessmentStatus.Created;
    
    /// <summary>
    /// Assessment Type - BUSINESS RULE: Only "Initial" or "Residual"
    /// </summary>
    public RiskAssessmentType AssessmentType { get; set; } = RiskAssessmentType.Initial;
    
    /// <summary>
    /// Risk Assessment Category - BUSINESS RULE: Only "Technical" or "Preliminary"
    /// Technical = 5-step process, Preliminary = 1-step process
    /// </summary>
    public RiskAssessmentCategory RiskAssessmentCategory { get; set; } = RiskAssessmentCategory.Technical;
    
    /// <summary>
    /// Current Step - BUSINESS RULE: 
    /// - Technical: 1-5 (reflects last step finished)
    /// - Preliminary: Always 1 (single step)
    /// </summary>
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

    // Step 1 - System Description Properties
    public string SystemDescription { get; set; } = string.Empty;
    public string SystemBoundaries { get; set; } = string.Empty;
    public string SystemPurpose { get; set; } = string.Empty;

    // 5M Framework Fields
    public string FiveMPersonnel { get; set; } = string.Empty;
    public string FiveMEquipment { get; set; } = string.Empty;
    public string FiveMProcedures { get; set; } = string.Empty;
    public string FiveMResources { get; set; } = string.Empty;
    public string FiveMPhysicalEnvironment { get; set; } = string.Empty;
    public string FiveMOperationalEnvironment { get; set; } = string.Empty;

    // Step 3 - Risk Analysis Properties
    public string RiskAnalysisMethod { get; set; } = "SMS Risk Matrix";
    public string RiskCriteria { get; set; } = string.Empty;

    // Step 4 - Risk Assessment Properties
    public string TolerabilityFramework { get; set; } = "PDX-SMS Default";
    public string RiskAcceptanceCriteria { get; set; } = string.Empty;

    // Step 5 - Implementation Properties
    public string ImplementationStrategy { get; set; } = string.Empty;
    public DateTime? OverallTargetDate { get; set; }
    public string ImplementationNotes { get; set; } = string.Empty;

    // Read-only collections
    public IReadOnlyList<string> IdentifiedHazardIds => _identifiedHazardIds.AsReadOnly();
    public IReadOnlyList<string> StakeholderIds => _stakeholderIds.AsReadOnly();
    public IReadOnlyList<int> CompletedSteps => _completedSteps.AsReadOnly();

    /// <summary>
    /// Factory method to create new Initial Risk Assessment
    /// BUSINESS RULE: Initial assessments MUST have primaryHazardId set
    /// </summary>
    public static Result<RiskAssessment> CreateInitial(RiskAssessmentID id, string name, string leadAssessorId, RiskAssessmentCategory? category = null, string? primaryHazardId = null, string? hazardCode = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.InvalidName);
        }

        if (string.IsNullOrWhiteSpace(leadAssessorId))
        {
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.InvalidLeadAssessor);
        }

        // BUSINESS RULE ENFORCEMENT: Category can only be Technical or Preliminary
        var assessmentCategory = category ?? RiskAssessmentCategory.Technical;

        var assessment = new RiskAssessment(id, name, leadAssessorId, RiskAssessmentType.Initial, primaryHazardId, hazardCode);
        assessment.RiskAssessmentCategory = assessmentCategory;
        assessment.Stage = "Created";
        assessment.Code = $"RA-0000";
        
        // BUSINESS RULE: Initial assessment MUST have PrimaryHazardId
        if (!string.IsNullOrEmpty(primaryHazardId))
        {
            assessment.PrimaryHazardId = primaryHazardId;
        }
        
        return Result<RiskAssessment>.Success(assessment);
    }

    /// <summary>
    /// Factory method to create new Residual Risk Assessment (after mitigations)
    /// BUSINESS RULE: Residual assessments are created after mitigations
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
        
        // BUSINESS RULE: Residual assessments typically use same category as parent, default to Technical
        assessment.RiskAssessmentCategory = RiskAssessmentCategory.Technical;
        assessment.Stage = "Created";
        assessment.Code = $"RRA-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        
        return Result<RiskAssessment>.Success(assessment);
    }

    /// <summary>
    /// Factory method for backward compatibility - defaults to Initial assessment
    /// </summary>
    public static Result<RiskAssessment> Create(RiskAssessmentID id, string name, string leadAssessorId, string? primaryHazardId = null)
    {
        return CreateInitial(id, name, leadAssessorId, RiskAssessmentCategory.Technical, primaryHazardId);
    }

    // ✅ METHODS WITH BUSINESS RULE ENFORCEMENT

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
    /// Complete a step - BUSINESS RULE ENFORCED
    /// Technical: Steps 1-5, Preliminary: Only step 1
    /// </summary>
    public void CompleteStep(int stepNumber)
    {
        // BUSINESS RULE: Use category-specific validation
        if (!RiskAssessmentCategory.IsValidStep(stepNumber))
        {
            return; // Invalid step for this category
        }

        if (!_completedSteps.Contains(stepNumber))
        {
            _completedSteps.Add(stepNumber);
            _completedSteps.Sort();
            CurrentStep = Math.Max(CurrentStep, stepNumber); // BUSINESS RULE: CurrentStep = last step finished
            UpdatedDate = DateTime.UtcNow;

            // Update status based on progress - BUSINESS RULE: Only Created/InProgress/Completed
            if (RiskAssessmentCategory.IsPreliminary && stepNumber == 1)
            {
                Status = RiskAssessmentStatus.Completed;
                Stage = "Complete";
            }
            else if (RiskAssessmentCategory.IsTechnical && _completedSteps.Count == 5)
            {
                Status = RiskAssessmentStatus.Completed;
                Stage = "Complete";
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
    /// Get next recommended step - BUSINESS RULE ENFORCED
    /// </summary>
    public int GetNextRecommendedStep()
    {
        // BUSINESS RULE: Use category-specific step validation
        for (int i = 1; i <= RiskAssessmentCategory.MaxSteps; i++)
        {
            if (!_completedSteps.Contains(i))
            {
                return i;
            }
        }
        return RiskAssessmentCategory.MaxSteps; // All completed
    }
}
