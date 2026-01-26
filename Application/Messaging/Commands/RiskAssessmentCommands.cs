namespace SMS_Application.Messaging.Commands;

// =============================================
// BASIC RISK ASSESSMENT COMMANDS
// =============================================

public class CreateRiskAssessmentCommand : BaseCommandBundle, IRequest<Result<RiskAssessment>>, ICreateCommand
{
    public RiskAssessment RiskAssessment { get; set; }

    public CreateRiskAssessmentCommand(RiskAssessment riskAssessment)
    {
        RiskAssessment = riskAssessment ?? throw new ArgumentNullException(nameof(riskAssessment));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        RiskAssessment.CreatedBy = userId;
        RiskAssessment.CreatedDate = timestamp;
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        // For create commands, we typically don't set UpdatedBy
    }
}

public class UpdateRiskAssessmentCommand : BaseCommandBundle, IRequest<Result<RiskAssessment>>, IUpdateCommand
{
    public RiskAssessment RiskAssessment { get; set; }

    public UpdateRiskAssessmentCommand(RiskAssessment riskAssessment)
    {
        RiskAssessment = riskAssessment ?? throw new ArgumentNullException(nameof(riskAssessment));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // For update commands, we typically don't modify CreatedBy
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        RiskAssessment.UpdatedBy = userId;
        RiskAssessment.UpdatedDate = timestamp;
    }
}

public class DeleteRiskAssessmentCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    public RiskAssessmentID RiskAssessmentId { get; set; }

    public DeleteRiskAssessmentCommand(RiskAssessmentID riskAssessmentId)
    {
        RiskAssessmentId = riskAssessmentId ?? throw new ArgumentNullException(nameof(riskAssessmentId));
    }
}

// =============================================
// STEP-SPECIFIC COMMANDS FOR STEPS 1-5
// =============================================

/// <summary>
/// Command to save Step 1 - System Description data
/// </summary>
public class SaveStep1Command : BaseCommandBundle, IRequest<Result<RiskAssessment>>, IHasAuditFields
{
    public RiskAssessmentID RiskAssessmentId { get; set; }
    public string LeadAssessorId { get; set; }
    public string SystemDescription { get; set; }
    public string SystemBoundaries { get; set; }
    public string SystemPurpose { get; set; }
    public string FiveMPersonnel { get; set; }
    public string FiveMEquipment { get; set; }
    public string FiveMProcedures { get; set; }
    public string FiveMResources { get; set; }
    public string FiveMPhysicalEnvironment { get; set; }
    public string FiveMOperationalEnvironment { get; set; }
    public string UpdatedBy { get; set; }

    public SaveStep1Command(
        RiskAssessmentID riskAssessmentId,
        string leadAssessorId,
        string systemDescription,
        string systemBoundaries,
        string systemPurpose,
        string fiveMPersonnel,
        string fiveMEquipment,
        string fiveMProcedures,
        string fiveMResources,
        string fiveMPhysicalEnvironment,
        string fiveMOperationalEnvironment)
    {
        RiskAssessmentId = riskAssessmentId ?? throw new ArgumentNullException(nameof(riskAssessmentId));
        LeadAssessorId = leadAssessorId ?? throw new ArgumentNullException(nameof(leadAssessorId));
        SystemDescription = systemDescription ?? string.Empty;
        SystemBoundaries = systemBoundaries ?? string.Empty;
        SystemPurpose = systemPurpose ?? string.Empty;
        FiveMPersonnel = fiveMPersonnel ?? string.Empty;
        FiveMEquipment = fiveMEquipment ?? string.Empty;
        FiveMProcedures = fiveMProcedures ?? string.Empty;
        FiveMResources = fiveMResources ?? string.Empty;
        FiveMPhysicalEnvironment = fiveMPhysicalEnvironment ?? string.Empty;
        FiveMOperationalEnvironment = fiveMOperationalEnvironment ?? string.Empty;
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // This is an update operation, not create
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        UpdatedBy = userId;
    }
}

/// <summary>
/// Command to save Step 3 - Risk Analysis data
/// </summary>
public class SaveStep3Command : BaseCommandBundle, IRequest<Result<RiskAssessment>>, IHasAuditFields
{
    public RiskAssessmentID RiskAssessmentId { get; set; }
    public string RiskAnalysisMethod { get; set; }
    public string RiskCriteria { get; set; }
    public string UpdatedBy { get; set; }

    public SaveStep3Command(
        RiskAssessmentID riskAssessmentId,
        string riskAnalysisMethod,
        string riskCriteria)
    {
        RiskAssessmentId = riskAssessmentId ?? throw new ArgumentNullException(nameof(riskAssessmentId));
        RiskAnalysisMethod = riskAnalysisMethod ?? "SMS Risk Matrix";
        RiskCriteria = riskCriteria ?? string.Empty;
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // This is an update operation, not create
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        UpdatedBy = userId;
    }
}

/// <summary>
/// Command to save Step 4 - Risk Assessment data
/// </summary>
public class SaveStep4Command : BaseCommandBundle, IRequest<Result<RiskAssessment>>, IHasAuditFields
{
    public RiskAssessmentID RiskAssessmentId { get; set; }
    
    public int? FinalSeverityScore { get; set; }
    public int? FinalLikelihoodScore { get; set; }
    public string FinalRiskLevel { get; set; }
    public string RiskTolerability { get; set; }
    public string AssessmentRationale { get; set; }
    public string UpdatedBy { get; set; }

    public SaveStep4Command(
        RiskAssessmentID riskAssessmentId,
        int? finalSeverityScore,
        int? finalLikelihoodScore,
        string finalRiskLevel,
        string riskTolerability,
        string assessmentRationale)
    {
        RiskAssessmentId = riskAssessmentId ?? throw new ArgumentNullException(nameof(riskAssessmentId));
        //TolerabilityFramework = tolerabilityFramework ?? "PDX-SMS Default";
        //RiskAcceptanceCriteria = riskAcceptanceCriteria ?? string.Empty;
        FinalSeverityScore = finalSeverityScore;
        FinalLikelihoodScore = finalLikelihoodScore;
        FinalRiskLevel = finalRiskLevel ?? string.Empty;
        RiskTolerability = riskTolerability ?? string.Empty;
        AssessmentRationale = assessmentRationale ?? string.Empty;
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // This is an update operation, not create
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        UpdatedBy = userId;
    }
}

/// <summary>
/// Command to save Step 5 - Implementation data
/// </summary>
public class SaveStep5Command : BaseCommandBundle, IRequest<Result<RiskAssessment>>, IHasAuditFields
{
    public RiskAssessmentID RiskAssessmentId { get; set; }
    
    public string UpdatedBy { get; set; }

    public SaveStep5Command(RiskAssessmentID riskAssessmentId )
    {
        RiskAssessmentId = riskAssessmentId ?? throw new ArgumentNullException(nameof(riskAssessmentId));
        
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // This is an update operation, not create
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        UpdatedBy = userId;
    }
}

/// <summary>
/// Command to update progress tracking data
/// </summary>
public class UpdateProgressCommand : BaseCommandBundle, IRequest<Result<RiskAssessment>>, IHasAuditFields
{
    public RiskAssessmentID RiskAssessmentId { get; set; }
    public int CurrentStep { get; set; }
    public string CompletedSteps { get; set; }
    public int CompletionPercentage { get; set; }
    public string Status { get; set; }
    public string Stage { get; set; }
    public string UpdatedBy { get; set; }

    public UpdateProgressCommand(
        RiskAssessmentID riskAssessmentId,
        int currentStep,
        string completedSteps,
        int completionPercentage,
        string status = null,
        string stage = null)
    {
        RiskAssessmentId = riskAssessmentId ?? throw new ArgumentNullException(nameof(riskAssessmentId));
        CurrentStep = currentStep;
        CompletedSteps = completedSteps ?? string.Empty;
        CompletionPercentage = completionPercentage;
        Status = status;
        Stage = stage;
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // This is an update operation, not create
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        UpdatedBy = userId;
    }
}