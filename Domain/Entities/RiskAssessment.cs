using SMS_Shared.Common;
using SMS_Domain.ValueObjects;

namespace SMS_Domain.Entities;

/// <summary>
/// Risk Assessment Aggregate Root - Mission Critical
/// Manages the complete 5-step risk assessment workflow with proper Domain logic
/// Supports both Initial and Residual risk assessments - NO separate ResidualRiskAssessment needed!
/// NO DTOs - Pure Domain Entity with rich behavior
/// </summary>
public sealed class RiskAssessment : BaseAuditableEntity
{
    // Private fields for encapsulation
    private readonly List<string> _identifiedHazardIds = new();
    private readonly List<string> _stakeholderIds = new();
    private readonly Dictionary<string, List<string>> _hazardPanelMembers = new();
    private readonly Dictionary<string, List<RiskScore>> _hazardPanelScores = new();
    private readonly Dictionary<string, string> _hazardWorstOutcomes = new();
    private readonly Dictionary<string, string> _hazardRootCauses = new();
    private readonly Dictionary<string, List<string>> _hazardMitigationStrategyIds = new(); // Reference to MitigationStrategy entities
    private readonly Dictionary<string, MonitoringRequirement> _monitoringRequirements = new();
    private readonly List<int> _completedSteps = new();

    // Constructor for Entity Framework
    private RiskAssessment() : base(new RiskAssessmentID(Guid.NewGuid().ToString()), "SYSTEM", DateTime.UtcNow) { }

    // Public constructor following the pattern
    public RiskAssessment(RiskAssessmentID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    // Private constructor for creation
    private RiskAssessment(RiskAssessmentID id, string name, string leadAssessorId, RiskAssessmentType assessmentType = RiskAssessmentType.Initial, string? primaryHazardId = null, string? hazardCode = null) 
        : base(id, "SYSTEM", DateTime.UtcNow)
    {
        Name = name;
        LeadAssessorId = leadAssessorId;
        PrimaryHazardId = primaryHazardId;
        HazardCode = hazardCode; // Set the hazard code
        Status = RiskAssessmentStatus.Created;
        AssessmentType = assessmentType; // Can be Initial or Residual
        RiskAssessmentCategory = assessmentType == RiskAssessmentType.Initial ? RiskAssessmentCategory.FiveStep : RiskAssessmentCategory.Residual;
        CurrentStep = 1;
        UpdatedDate = DateTime.UtcNow;
    }

    // Public Properties (immutable from outside)
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? HazardCode { get; set; } // Primary hazard code for this risk assessment
    public string LeadAssessorId { get; set; } = string.Empty;
    public string? PrimaryHazardId { get; set; }
    public RiskAssessmentStatus Status { get; set; }
    public RiskAssessmentType AssessmentType { get; set; } // Initial or Residual
    public RiskAssessmentCategory RiskAssessmentCategory { get; set; } // FiveStep, Simplified, Technical, Residual
    public int CurrentStep { get; set; } = 1;
    public string? Stage { get; set; } // Simple string property for workflow stage
    public DateTime? CompletedDate { get; set; }
    public string? CompletedBy { get; set; }
    public string? ParentAssessmentId { get; set; } // For Residual assessments - links back to Initial

    // Risk Scoring Properties (shared by Initial and Residual)
    public int? FinalSeverityScore { get; set; }
    public int? FinalLikelihoodScore { get; set; }
    public string? FinalRiskLevel { get; set; }
    public string? RiskTolerability { get; set; } = "ALARP";
    public string? AssessmentRationale { get; set; }

    // Step 1 - System Description Properties
    public string SystemDescription { get; private set; } = string.Empty;
    public string SystemBoundaries { get; private set; } = string.Empty;
    public string SystemPurpose { get; private set; } = string.Empty;
    public string PersonnelFactors { get; private set; } = string.Empty;
    public string EquipmentFactors { get; private set; } = string.Empty;
    public string ProcedureFactors { get; private set; } = string.Empty;
    public string ResourceFactors { get; private set; } = string.Empty;
    public string EnvironmentFactors { get; private set; } = string.Empty;

    // Step 3 - Risk Analysis Properties
    public string RiskAnalysisMethod { get; private set; } = "SMS Risk Matrix";
    public string RiskCriteria { get; private set; } = string.Empty;

    // Step 4 - Risk Assessment Properties
    public string TolerabilityFramework { get; private set; } = "PDX-SMS Default";
    public string RiskAcceptanceCriteria { get; private set; } = string.Empty;

    // Step 5 - Implementation Properties
    public string ImplementationStrategy { get; private set; } = string.Empty;
    public DateTime? OverallTargetDate { get; private set; }
    public string ImplementationNotes { get; private set; } = string.Empty;

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
        assessment.Stage = "Created"; // Initialize stage
        
        // Generate code if not provided
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
        assessment.Stage = "Created"; // Initialize stage
        
        // Generate code for residual assessment
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

    /// <summary>
    /// Update final risk scoring (used for both Initial and Residual assessments)
    /// </summary>
    public Result<bool> UpdateFinalRiskScoring(int severityScore, int likelihoodScore, string? tolerability = null, string? rationale = null)
    {
        if (severityScore < 1 || severityScore > 5)
        {
            return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.InvalidSeverityScore);
        }

        if (likelihoodScore < 1 || likelihoodScore > 5)
        {
            return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.InvalidLikelihoodScore);
        }

        FinalSeverityScore = severityScore;
        FinalLikelihoodScore = likelihoodScore;
        FinalRiskLevel = CalculateRiskLevel(severityScore, likelihoodScore);
        RiskTolerability = tolerability ?? "ALARP";
        AssessmentRationale = rationale;
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Update the primary hazard code for this risk assessment
    /// </summary>
    public Result<bool> UpdateHazardCode(string hazardCode)
    {
        if (string.IsNullOrWhiteSpace(hazardCode))
        {
            return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.InvalidHazard);
        }

        if (Status == RiskAssessmentStatus.Completed)
        {
            return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.CannotModifyCompleted);
        }

        HazardCode = hazardCode;
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Update the workflow stage
    /// </summary>
    public Result<bool> UpdateStage(string stage)
    {
        if (string.IsNullOrWhiteSpace(stage))
        {
            return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.InvalidStage);
        }

        Stage = stage;
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Check if this is a residual risk assessment
    /// </summary>
    public bool IsResidualAssessment => AssessmentType == RiskAssessmentType.Residual;

    /// <summary>
    /// Check if this is an initial risk assessment
    /// </summary>
    public bool IsInitialAssessment => AssessmentType == RiskAssessmentType.Initial;

    /// <summary>
    /// Get the risk reduction achieved (only applicable for residual assessments)
    /// </summary>
    public string? GetRiskReduction(RiskAssessment? initialAssessment = null)
    {
        if (!IsResidualAssessment || initialAssessment == null || !FinalSeverityScore.HasValue || !FinalLikelihoodScore.HasValue)
            return null;

        if (!initialAssessment.FinalSeverityScore.HasValue || !initialAssessment.FinalLikelihoodScore.HasValue)
            return null;

        var initialRisk = initialAssessment.FinalSeverityScore.Value * initialAssessment.FinalLikelihoodScore.Value;
        var residualRisk = FinalSeverityScore.Value * FinalLikelihoodScore.Value;
        var reduction = initialRisk - residualRisk;

        return $"Risk reduced by {reduction} points (from {initialRisk} to {residualRisk})";
    }

    /// <summary>
    /// Step 1 - Update System Description
    /// </summary>
    public Result<bool> UpdateSystemDescription(
        string systemDescription,
        string systemBoundaries, 
        string systemPurpose,
        string personnelFactors,
        string equipmentFactors,
        string procedureFactors,
        string resourceFactors,
        string environmentFactors)
    {
        if (Status == RiskAssessmentStatus.Completed)
        {
            return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.CannotModifyCompleted);
        }

        SystemDescription = systemDescription ?? string.Empty;
        SystemBoundaries = systemBoundaries ?? string.Empty;
        SystemPurpose = systemPurpose ?? string.Empty;
        PersonnelFactors = personnelFactors ?? string.Empty;
        EquipmentFactors = equipmentFactors ?? string.Empty;
        ProcedureFactors = procedureFactors ?? string.Empty;
        ResourceFactors = resourceFactors ?? string.Empty;
        EnvironmentFactors = environmentFactors ?? string.Empty;

        UpdatedDate = DateTime.UtcNow;
        Status = RiskAssessmentStatus.InProgress;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Step 2 - Add Stakeholder
    /// </summary>
    public Result<bool> AddStakeholder(string stakeholderId)
    {
        if (string.IsNullOrWhiteSpace(stakeholderId))
        {
            return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.InvalidStakeholder);
        }

        if (!_stakeholderIds.Contains(stakeholderId))
        {
            _stakeholderIds.Add(stakeholderId);
        }

        UpdatedDate = DateTime.UtcNow;
        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Step 2 - Add Identified Hazard
    /// </summary>
    public Result<bool> AddIdentifiedHazard(string hazardId, string description)
    {
        if (string.IsNullOrWhiteSpace(hazardId))
        {
            return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.InvalidHazard);
        }

        if (!_identifiedHazardIds.Contains(hazardId))
        {
            _identifiedHazardIds.Add(hazardId);
        }

        UpdatedDate = DateTime.UtcNow;
        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Step 2 - Clear all identified hazards (for updates)
    /// </summary>
    public void ClearIdentifiedHazards()
    {
        _identifiedHazardIds.Clear();
        UpdatedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Step 3 - Update Risk Analysis Method
    /// </summary>
    public Result<bool> UpdateRiskAnalysisMethod(string method, string criteria)
    {
        if (Status == RiskAssessmentStatus.Completed)
        {
            return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.CannotModifyCompleted);
        }

        RiskAnalysisMethod = method ?? "SMS Risk Matrix";
        RiskCriteria = criteria ?? string.Empty;
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Step 3 - Update Hazard Worst Outcome
    /// </summary>
    public Result<bool> UpdateHazardWorstOutcome(string hazardId, string worstOutcome)
    {
        if (!_identifiedHazardIds.Contains(hazardId))
        {
            return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.HazardNotFound);
        }

        _hazardWorstOutcomes[hazardId] = worstOutcome ?? string.Empty;
        UpdatedDate = DateTime.UtcNow;
        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Step 3 - Update Hazard Root Cause
    /// </summary>
    public Result<bool> UpdateHazardRootCause(string hazardId, string rootCause)
    {
        if (!_identifiedHazardIds.Contains(hazardId))
        {
            return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.HazardNotFound);
        }

        _hazardRootCauses[hazardId] = rootCause ?? string.Empty;
        UpdatedDate = DateTime.UtcNow;
        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Step 4 - Assign Panel Member to Hazard
    /// </summary>
    public Result<bool> AssignPanelMember(string hazardId, string panelMemberId)
    {
        if (!_identifiedHazardIds.Contains(hazardId))
        {
            return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.HazardNotFound);
        }

        if (string.IsNullOrWhiteSpace(panelMemberId))
        {
            return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.InvalidPanelMember);
        }

        if (!_hazardPanelMembers.ContainsKey(hazardId))
        {
            _hazardPanelMembers[hazardId] = new List<string>();
        }

        if (!_hazardPanelMembers[hazardId].Contains(panelMemberId))
        {
            _hazardPanelMembers[hazardId].Add(panelMemberId);
        }

        UpdatedDate = DateTime.UtcNow;
        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Step 4 - Submit Panel Score
    /// </summary>
    public Result<bool> SubmitPanelScore(string hazardId, string panelMemberId, int severityScore, int likelihoodScore)
    {
        if (!_identifiedHazardIds.Contains(hazardId))
        {
            return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.HazardNotFound);
        }

        if (severityScore < 1 || severityScore > 5 || likelihoodScore < 1 || likelihoodScore > 5)
        {
            return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.InvalidRiskScore);
        }

        if (!_hazardPanelScores.ContainsKey(hazardId))
        {
            _hazardPanelScores[hazardId] = new List<RiskScore>();
        }

        // Remove existing score for this member
        _hazardPanelScores[hazardId].RemoveAll(s => s.PanelMemberId == panelMemberId);

        // Add new score
        var riskScore = RiskScore.Create(panelMemberId, severityScore, likelihoodScore);
        if (riskScore.IsFailure)
        {
            return Result<bool>.Failure<bool>(riskScore.Error);
        }

        _hazardPanelScores[hazardId].Add(riskScore.Value);
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Step 4 - Update Tolerability Framework
    /// </summary>
    public Result<bool> UpdateTolerabilityFramework(string framework, string criteria)
    {
        TolerabilityFramework = framework ?? "PDX-SMS Default";
        RiskAcceptanceCriteria = criteria ?? string.Empty;
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Step 5 - Update Implementation Strategy
    /// </summary>
    public Result<bool> UpdateImplementationStrategy(string strategy, DateTime? targetDate, string notes)
    {
        ImplementationStrategy = strategy ?? string.Empty;
        OverallTargetDate = targetDate;
        ImplementationNotes = notes ?? string.Empty;
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Step 5 - Link Mitigation Strategy to Hazard (references MitigationStrategy entity)
    /// </summary>
    public Result<bool> LinkMitigationStrategy(string hazardId, string mitigationStrategyId)
    {
        if (!_identifiedHazardIds.Contains(hazardId))
        {
            return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.HazardNotFound);
        }

        if (string.IsNullOrWhiteSpace(mitigationStrategyId))
        {
            return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.InvalidMitigationStrategy);
        }

        if (!_hazardMitigationStrategyIds.ContainsKey(hazardId))
        {
            _hazardMitigationStrategyIds[hazardId] = new List<string>();
        }

        if (!_hazardMitigationStrategyIds[hazardId].Contains(mitigationStrategyId))
        {
            _hazardMitigationStrategyIds[hazardId].Add(mitigationStrategyId);
        }

        UpdatedDate = DateTime.UtcNow;
        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Step 5 - Update Monitoring Requirement
    /// </summary>
    public Result<bool> UpdateMonitoringRequirement(string hazardId, MonitoringRequirement requirement)
    {
        if (!_identifiedHazardIds.Contains(hazardId))
        {
            return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.HazardNotFound);
        }

        _monitoringRequirements[hazardId] = requirement;
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Complete a step
    /// </summary>
    public Result<bool> CompleteStep(int stepNumber)
    {
        if (stepNumber < 1 || stepNumber > 5)
        {
            return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.InvalidStep);
        }

        if (!_completedSteps.Contains(stepNumber))
        {
            _completedSteps.Add(stepNumber);
            _completedSteps.Sort();
        }

        CurrentStep = Math.Max(CurrentStep, stepNumber);
        UpdatedDate = DateTime.UtcNow;

        // Update status and stage based on progress
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

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Mark assessment as completed
    /// </summary>
    public Result<bool> MarkAsCompleted()
    {
        if (_completedSteps.Count < 5)
        {
            return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.IncompleteSteps);
        }

        Status = RiskAssessmentStatus.Completed;
        Stage = "Completed";
        CompletedDate = DateTime.UtcNow;
        CompletedBy = LeadAssessorId; // Could be parameterized
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Get panel scores for a hazard
    /// </summary>
    public IReadOnlyList<RiskScore> GetHazardPanelScores(string hazardId)
    {
        return _hazardPanelScores.GetValueOrDefault(hazardId, new List<RiskScore>()).AsReadOnly();
    }

    /// <summary>
    /// Get average risk score for a hazard
    /// </summary>
    public double? GetHazardAverageScore(string hazardId)
    {
        if (!_hazardPanelScores.ContainsKey(hazardId))
            return null;

        var scores = _hazardPanelScores[hazardId];
        if (!scores.Any())
            return null;

        return scores.Average(s => s.CalculatedScore);
    }

    /// <summary>
    /// Get mitigation strategy IDs for a hazard
    /// </summary>
    public IReadOnlyList<string> GetHazardMitigationStrategyIds(string hazardId)
    {
        return _hazardMitigationStrategyIds.GetValueOrDefault(hazardId, new List<string>()).AsReadOnly();
    }

    /// <summary>
    /// Check if step is completed
    /// </summary>
    public bool IsStepCompleted(int stepNumber)
    {
        return _completedSteps.Contains(stepNumber);
    }

    /// <summary>
    /// Get next recommended step
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

    private static string CalculateRiskLevel(int severity, int likelihood)
    {
        var score = severity * likelihood;
        return score switch
        {
            <= 5 => "1A",
            <= 10 => "2B", 
            <= 15 => "3C",
            <= 20 => "4D",
            _ => "5E"
        };
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
/// Risk Assessment Type Enumeration - Initial vs Residual
/// </summary>
public enum RiskAssessmentType
{
    Initial,   // Original risk assessment before mitigations
    Residual   // Risk assessment after mitigations have been implemented
}

/// <summary>
/// Risk Assessment Category Enumeration - Method/Process used
/// </summary>
public enum RiskAssessmentCategory
{
    FiveStep,    // Full 5-step SMS process
    Simplified,  // Simplified risk assessment
    Technical,   // Technical risk assessment (TRA)
    Emergency,   // Emergency assessment
    Residual     // Residual risk after mitigation
}
