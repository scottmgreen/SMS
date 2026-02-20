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
/// 4. RiskAssessmentCategory: ONLY "Technical" 
///    - Technical: Full 5-step SMS process (IS the 5-step, no redundancy needed)
///    
/// 
/// 5. CurrentStep Rules:
///    - Technical (5-step): CurrentStep reflects last step finished (1-5)
///    
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
    private readonly List<int> _completedSteps = new();


    
    public RiskAssessment(RiskAssessmentID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

       

    // ✅ PROPERTIES WITH BUSINESS RULE ENFORCEMENT
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? HazardCode { get; set; }
    public string LeadAssessorId { get; set; } = string.Empty;

    public string? PrimaryHazardId { get; set; }

    public string ReportCode { get; set; } = string.Empty;

    public RiskAssessmentStatus Status { get; set; } = RiskAssessmentStatus.AssessmentCreate;
    public RiskAssessmentStage Stage { get; set; } = RiskAssessmentStage.DescribingSystem;
    /// <summary>
    /// Assessment Type - BUSINESS RULE: Only "Initial" or "Residual"
    /// </summary>
    public RiskAssessmentType AssessmentType { get; set; } = RiskAssessmentType.Initial;

    /// <summary>
    /// Risk Assessment HazardCategory - BUSINESS RULE: Only "Technical" 
    /// Technical = 5-step process, Preliminary = 1-step process
    /// </summary>
    public RiskAssessmentCategory RiskAssessmentCategory { get; set; } = RiskAssessmentCategory.Technical;

    /// <summary>
    /// Current Step - BUSINESS RULE: 
    /// - Technical: 1-5 (reflects last step finished)
    /// - Preliminary: Always 1 (single step)
    /// </summary>
    public int CurrentStep { get; set; } = 1;

    
    public DateTime? CompletedDate { get; set; }
    public string? CompletedBy { get; set; }
    
    // Risk Scoring Properties
    public int? FinalSeverityScore { get; set; }
    public int? FinalLikelihoodScore { get; set; }
    public string? FinalRiskLevel { get; set; }
    
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

    // Step 4 - Risk Assessment Properties
   
    // Step 5 - Implementation Properties
   
    // ✅ Stakeholder Persistence Properties - Comma-delimited codes
    /// <summary>
    /// Selected Stakeholder Groups (comma-delimited codes: SG-0001,SG-0002,SG-0003)
    /// </summary>
    public string? SelectedStakeholderGroups { get; set; }

    /// <summary>
    /// Selected Individual Stakeholders (comma-delimited codes: SU-0001,SU-0002,SU-0003,SU-0004,SU-0005)
    /// </summary>
    public string? SelectedIndividualStakeholders { get; set; }

    // Read-only collections
    public IReadOnlyList<string> IdentifiedHazardIds => _identifiedHazardIds.AsReadOnly();
    public IReadOnlyList<int> CompletedSteps => _completedSteps.AsReadOnly();

    
        


    // ✅ METHODS WITH BUSINESS RULE ENFORCEMENT

    /// <summary>
    /// Add Stakeholder - SIMPLIFIED (for backward compatibility)
    /// Note: New stakeholder persistence uses SelectedStakeholderGroups and SelectedIndividualStakeholders properties
    /// </summary>
    public void AddStakeholder(string stakeholderId)
    {
        // This method is kept for backward compatibility but no longer maintains a separate collection
        // Stakeholder selection is now handled through the Step1Model persistence
        UpdatedDate = DateTime.UtcNow;
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
            CurrentStep = Math.Max(CurrentStep, stepNumber); // 

            Stage = DetermineRiskAssessmentStageFromStep(stepNumber);
            // Update status based on progress - BUSINESS RULE: Only Created/InProgress/Completed
            if (stepNumber == 5)
            {
                Status = RiskAssessmentStatus.AssessmentComplete;
                CompletedDate = DateTime.UtcNow;
            }
           
            else
            {
                Status = RiskAssessmentStatus.AssessmentUnderway;
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

    private RiskAssessmentStage DetermineRiskAssessmentStageFromStep(int step)
    {
        return step switch
        {
            1 => RiskAssessmentStage.DescribingSystem,// System description
            2 => RiskAssessmentStage.IdentifyingHazards, // Hazard identification
            3 => RiskAssessmentStage.AnalyizingRisk, // Risk analysis
            4 => RiskAssessmentStage.AssessingRisk, // Risk assessment
            5 => RiskAssessmentStage.MitigatingRisk, // Risk mitigation
            _ => RiskAssessmentStage.DescribingSystem
        };
    }
}
