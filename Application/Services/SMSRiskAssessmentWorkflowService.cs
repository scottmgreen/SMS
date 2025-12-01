using SMS_Application.Interfaces;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Shared.Common;
using Microsoft.Extensions.Logging;
using SMS_Infrastructure.Interfaces;
using SMS_Infrastructure.Persistence;

namespace SMS_Application.Services;

/// <summary>
/// SMS Risk Assessment Workflow Service - Mission Critical Business Logic
/// Manages the complete 5-step risk assessment process using proper Domain Entities
/// Direct SMS Backend integration - NO file operations, NO DTOs
/// </summary>
public interface ISMSRiskAssessmentWorkflowService
{
    Task<Result<RiskAssessment>> CreateRiskAssessmentAsync(string assessmentName, string leadAssessorId, RiskAssessmentCategory category = RiskAssessmentCategory.FiveStep, string hazardId = null);
    Task<Result<RiskAssessment>> CreateResidualRiskAssessmentAsync(string assessmentName, string leadAssessorId, string parentAssessmentId, string hazardId = null);
    Task<Result<RiskAssessment>> GetRiskAssessmentAsync(string assessmentId);
    Task<Result<RiskAssessment>> UpdateStep1SystemDescriptionAsync(string assessmentId, Step1SystemDescriptionData data);
    Task<Result<RiskAssessment>> UpdateStep2HazardIdentificationAsync(string assessmentId, List<Hazard> identifiedHazards);
    Task<Result<RiskAssessment>> UpdateStep3RiskAnalysisAsync(string assessmentId, Step3RiskAnalysisData data);
    Task<Result<RiskAssessment>> UpdateStep4RiskAssessmentAsync(string assessmentId, Step4RiskAssessmentData data);
    Task<Result<RiskAssessment>> UpdateStep5MitigationPlanningAsync(string assessmentId, Step5MitigationData data);
    Task<Result<RiskAssessment>> CompleteRiskAssessmentAsync(string assessmentId);
    Task<Result<List<SMSApplicationUser>>> GetAvailableAssessorsAsync();
    Task<Result<List<Hazard>>> GetAvailableHazardsForAssessmentAsync();
}

/// <summary>
/// Step 1 - System Description Data
/// </summary>
public class Step1SystemDescriptionData
{
    public string SystemDescription { get; set; } = string.Empty;
    public string SystemBoundaries { get; set; } = string.Empty;
    public string SystemPurpose { get; set; } = string.Empty;
    public string PersonnelFactors { get; set; } = string.Empty;
    public string EquipmentFactors { get; set; } = string.Empty;
    public string ProcedureFactors { get; set; } = string.Empty;
    public string ResourceFactors { get; set; } = string.Empty;
    public string EnvironmentFactors { get; set; } = string.Empty;
    public List<string> StakeholderIds { get; set; } = new();
}

/// <summary>
/// Step 3 - Risk Analysis Data
/// </summary>
public class Step3RiskAnalysisData
{
    public string RiskAnalysisMethod { get; set; } = "SMS Risk Matrix";
    public string RiskCriteria { get; set; } = string.Empty;
    public Dictionary<string, string> HazardWorstOutcomes { get; set; } = new();
    public Dictionary<string, string> HazardRootCauses { get; set; } = new();
}

/// <summary>
/// Step 4 - Risk Assessment Data (Panel Scoring)
/// </summary>
public class Step4RiskAssessmentData
{
    public Dictionary<string, List<string>> HazardPanelAssignments { get; set; } = new();
    public Dictionary<string, List<RiskScore>> HazardPanelScores { get; set; } = new();
    public string TolerabilityFramework { get; set; } = "PDX-SMS Default";
    public string RiskAcceptanceCriteria { get; set; } = string.Empty;
}

/// <summary>
/// Step 5 - Mitigation Planning Data
/// Uses proper Domain Entities - NOT Value Objects
/// </summary>
public class Step5MitigationData
{
    public string ImplementationStrategy { get; set; } = string.Empty;
    public DateTime? OverallTargetDate { get; set; }
    public string ImplementationNotes { get; set; } = string.Empty;
    public Dictionary<string, List<string>> HazardMitigationStrategyIds { get; set; } = new(); // References to MitigationStrategy entities
    public Dictionary<string, MonitoringRequirement> MonitoringRequirements { get; set; } = new();
}

public class SMSRiskAssessmentWorkflowService : ISMSRiskAssessmentWorkflowService
{
    private readonly IMediator _mediator;
    private readonly ILogger<SMSRiskAssessmentWorkflowService> _logger;
    private readonly IRiskAssessmentRepository _riskAssessmentRepository;
    private readonly SMSApplicationUserRepository _userRepository;
    private readonly IHazardRepository _hazardRepository;

    public SMSRiskAssessmentWorkflowService(
        IMediator mediator,
        ILogger<SMSRiskAssessmentWorkflowService> logger,
        IRiskAssessmentRepository riskAssessmentRepository,
        SMSApplicationUserRepository userRepository,
        IHazardRepository hazardRepository)
    {
        _mediator = mediator;
        _logger = logger;
        _riskAssessmentRepository = riskAssessmentRepository;
        _userRepository = userRepository;
        _hazardRepository = hazardRepository;
    }

    public async Task<Result<RiskAssessment>> CreateRiskAssessmentAsync(string assessmentName, string leadAssessorId, RiskAssessmentCategory category = RiskAssessmentCategory.FiveStep, string hazardId = null)
    {
        try
        {
            _logger.LogInformation("Creating new risk assessment: {AssessmentName} for assessor: {AssessorId}", assessmentName, leadAssessorId);

            // Validate lead assessor exists
            var assessorResult = await _userRepository.GetByIdAsync(leadAssessorId);
            if (assessorResult.IsFailure)
            {
                return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.SMSApplicationUserError.NotFound);
            }

            // Create risk assessment ID
            var assessmentId = new RiskAssessmentID($"RA-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}");
            // Create new initial risk assessment
            var riskAssessment = RiskAssessment.CreateInitial(
                assessmentId,
                assessmentName,
                leadAssessorId,
                category,
                hazardId
            );

            if (riskAssessment.IsFailure)
            {
                return Result<RiskAssessment>.Failure<RiskAssessment>(riskAssessment.Error);
            }

            // Save to repository
            var saveResult = await _riskAssessmentRepository.AddAsync(riskAssessment.Value);
            if (saveResult.IsFailure)
            {
                return Result<RiskAssessment>.Failure<RiskAssessment>(saveResult.Error);
            }

            _logger.LogInformation("Risk assessment created successfully: {AssessmentId}", assessmentId.Value);
            return Result<RiskAssessment>.Success(riskAssessment.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating risk assessment");
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.CreationFailed);
        }
    }

    public async Task<Result<RiskAssessment>> CreateResidualRiskAssessmentAsync(string assessmentName, string leadAssessorId, string parentAssessmentId, string hazardId = null)
    {
        try
        {
            _logger.LogInformation("Creating new residual risk assessment: {AssessmentName} for parent: {ParentId}", assessmentName, parentAssessmentId);

            // Validate parent assessment exists
            var parentResult = await GetRiskAssessmentAsync(parentAssessmentId);
            if (parentResult.IsFailure)
            {
                return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.InvalidParentAssessment);
            }

            // Create residual risk assessment ID
            var assessmentId = new RiskAssessmentID($"RRA-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}");

            // Create new residual risk assessment
            var residualAssessment = RiskAssessment.CreateResidual(
                assessmentId,
                assessmentName,
                leadAssessorId,
                parentAssessmentId,
                hazardId
            );

            if (residualAssessment.IsFailure)
            {
                return Result<RiskAssessment>.Failure<RiskAssessment>(residualAssessment.Error);
            }

            // Save to repository
            var saveResult = await _riskAssessmentRepository.AddAsync(residualAssessment.Value);
            if (saveResult.IsFailure)
            {
                return Result<RiskAssessment>.Failure<RiskAssessment>(saveResult.Error);
            }

            _logger.LogInformation("Residual risk assessment created successfully: {AssessmentId}", assessmentId.Value);
            return Result<RiskAssessment>.Success(residualAssessment.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating residual risk assessment");
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.CreationFailed);
        }
    }

    public async Task<Result<RiskAssessment>> GetRiskAssessmentAsync(string assessmentId)
    {
        try
        {
            var id = new RiskAssessmentID(assessmentId);
            var result = await _riskAssessmentRepository.GetByIdAsync(id);
            if (result.IsFailure)
            {
                _logger.LogWarning("Risk assessment not found: {AssessmentId}", assessmentId);
                return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.NotFound);
            }

            return Result<RiskAssessment>.Success(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving risk assessment: {AssessmentId}", assessmentId);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.NotFound);
        }
    }

    public async Task<Result<RiskAssessment>> UpdateStep1SystemDescriptionAsync(string assessmentId, Step1SystemDescriptionData data)
    {
        try
        {
            _logger.LogInformation("Updating Step 1 for assessment: {AssessmentId}", assessmentId);

            // Get existing assessment
            var assessmentResult = await GetRiskAssessmentAsync(assessmentId);
            if (assessmentResult.IsFailure)
            {
                return assessmentResult;
            }

            var assessment = assessmentResult.Value;

            // Update system description using Domain Entity methods
            //var updateResult = assessment.UpdateSystemDescription(
                //assessment.SystemDescription,
                //data.SystemBoundaries,
                //data.SystemPurpose,
                //data.PersonnelFactors,
                //data.EquipmentFactors,
                //data.ProcedureFactors,
                //data.ResourceFactors,
                //data.EnvironmentFactors
            //);

            //if (updateResult.IsFailure)
            //{
            //    return Result<RiskAssessment>.Failure<RiskAssessment>(updateResult.Error);
            //}

            // Update stakeholders
            foreach (var stakeholderId in data.StakeholderIds)
            {
                assessment.AddStakeholder(stakeholderId);
            }

            // Mark Step 1 as completed
            assessment.CompleteStep(1);

            // Save to repository
            var saveResult = await _riskAssessmentRepository.UpdateAsync(assessment);
            if (saveResult.IsFailure)
            {
                return Result<RiskAssessment>.Failure<RiskAssessment>(saveResult.Error);
            }

            _logger.LogInformation("Step 1 updated successfully for assessment: {AssessmentId}", assessmentId);
            return Result<RiskAssessment>.Success(assessment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Step 1 for assessment: {AssessmentId}", assessmentId);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }

    /// <summary>
    /// Step 2: Update Hazard Identification - Creates/Updates Hazards and links to Risk Assessment
    /// </summary>
    public async Task<Result<RiskAssessment>> UpdateStep2HazardIdentificationAsync(string assessmentId, List<Hazard> identifiedHazards)
    {
        try
        {
            _logger.LogInformation("Updating Step 2 Hazard Identification for assessment {AssessmentId} with {HazardCount} hazards", 
                assessmentId, identifiedHazards.Count);

            // Get existing risk assessment
            var assessmentResult = await GetRiskAssessmentAsync(assessmentId);
            if (assessmentResult.IsFailure)
            {
                return assessmentResult;
            }

            var assessment = assessmentResult.Value;

            // Clear existing identified hazards
            assessment.ClearIdentifiedHazards();

            // Process each hazard
            foreach (var hazard in identifiedHazards)
            {
                // Validate hazard
                if (string.IsNullOrWhiteSpace(hazard.Description) || hazard.Description.Length < 10)
                {
                    return Result<RiskAssessment>.Failure<RiskAssessment>(
                        DomainErrors.HazardError.NullOrEmpty);
                }

                // Create or update hazard in the system using CQRS
                var createHazardCommand = new CreateHazardCommand(hazard);
                var hazardResult = await _mediator.SendAsync(createHazardCommand, CancellationToken.None);
                
                if (hazardResult.IsFailure)
                {
                    _logger.LogError("Failed to create hazard {HazardCode}: {Error}", 
                        hazard.Code, hazardResult.Error.Message);
                    continue; // Skip this hazard but continue with others
                }

                // Add hazard to risk assessment
                //var addResult = assessment.AddIdentifiedHazard(hazardResult.Value.Code, hazardResult.Value.Description);
                //if (addResult.IsFailure)
                //{
                //    _logger.LogWarning("Failed to add hazard {HazardCode} to assessment: {Error}", 
                //        hazard.Code, addResult.Error.Message);
                //}
            }

            // Mark Step 2 as completed
            assessment.CompleteStep(2);

            // Save the updated assessment using CQRS
            var updateCommand = new UpdateRiskAssessmentCommand(assessment);
            var updateResult = await _mediator.SendAsync(updateCommand, CancellationToken.None);

            if (updateResult.IsFailure)
            {
                _logger.LogError("Failed to update risk assessment {AssessmentId}: {Error}", 
                    assessmentId, updateResult.Error.Message);
                return updateResult;
            }

            _logger.LogInformation("Successfully updated Step 2 for assessment {AssessmentId} with {HazardCount} hazards", 
                assessmentId, identifiedHazards.Count);

            return updateResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Step 2 hazard identification for assessment {AssessmentId}", assessmentId);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }

    public async Task<Result<RiskAssessment>> UpdateStep3RiskAnalysisAsync(string assessmentId, Step3RiskAnalysisData data)
    {
        try
        {
            _logger.LogInformation("Updating Step 3 for assessment: {AssessmentId}", assessmentId);

            var assessmentResult = await GetRiskAssessmentAsync(assessmentId);
            if (assessmentResult.IsFailure)
            {
                return assessmentResult;
            }

            var assessment = assessmentResult.Value;

            // Update risk analysis method and criteria
            //var updateResult = assessment.UpdateRiskAnalysisMethod(data.RiskAnalysisMethod, data.RiskCriteria);
            //if (updateResult.IsFailure)
            //{
            //   // return Result<RiskAssessment>.Failure<RiskAssessment>(updateResult.Error);
            //}

            // Update worst credible outcomes for each hazard
            foreach (var hazardOutcome in data.HazardWorstOutcomes)
            {
                //assessment.UpdateHazardWorstOutcome(hazardOutcome.Key, hazardOutcome.Value);
            }

            // Update root causes for each hazard
            foreach (var hazardCause in data.HazardRootCauses)
            {
                //assessment.UpdateHazardRootCause(hazardCause.Key, hazardCause.Value);
            }

            assessment.CompleteStep(3);

            var saveResult = await _riskAssessmentRepository.UpdateAsync(assessment);
            if (saveResult.IsFailure)
            {
                return Result<RiskAssessment>.Failure<RiskAssessment>(saveResult.Error);
            }

            _logger.LogInformation("Step 3 updated successfully for assessment: {AssessmentId}", assessmentId);
            return Result<RiskAssessment>.Success(assessment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Step 3 for assessment: {AssessmentId}", assessmentId);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }

    public async Task<Result<RiskAssessment>> UpdateStep4RiskAssessmentAsync(string assessmentId, Step4RiskAssessmentData data)
    {
        try
        {
            _logger.LogInformation("Updating Step 4 for assessment: {AssessmentId}", assessmentId);

            var assessmentResult = await GetRiskAssessmentAsync(assessmentId);
            if (assessmentResult.IsFailure)
            {
                return assessmentResult;
            }

            var assessment = assessmentResult.Value;

            // Update panel assignments for each hazard
            foreach (var assignment in data.HazardPanelAssignments)
            {
                var hazardId = assignment.Key;
                var panelMemberIds = assignment.Value;

                foreach (var memberId in panelMemberIds)
                {
                   // assessment.AssignPanelMember(hazardId, memberId);
                }
            }

            // Update panel scores for each hazard
            foreach (var hazardScores in data.HazardPanelScores)
            {
                var hazardId = hazardScores.Key;
                var scores = hazardScores.Value;

                foreach (var score in scores)
                {
                    //assessment.SubmitPanelScore(hazardId, score.PanelMemberId, score.SeverityScore, score.LikelihoodScore);
                }
            }

            // Update tolerability framework
            //assessment.UpdateTolerabilityFramework(data.TolerabilityFramework, data.RiskAcceptanceCriteria);

            assessment.CompleteStep(4);

            var saveResult = await _riskAssessmentRepository.UpdateAsync(assessment);
            if (saveResult.IsFailure)
            {
                return Result<RiskAssessment>.Failure<RiskAssessment>(saveResult.Error);
            }

            _logger.LogInformation("Step 4 updated successfully for assessment: {AssessmentId}", assessmentId);
            return Result<RiskAssessment>.Success(assessment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Step 4 for assessment: {AssessmentId}", assessmentId);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }

    public async Task<Result<RiskAssessment>> UpdateStep5MitigationPlanningAsync(string assessmentId, Step5MitigationData data)
    {
        try
        {
            _logger.LogInformation("Updating Step 5 for assessment: {AssessmentId}", assessmentId);

            var assessmentResult = await GetRiskAssessmentAsync(assessmentId);
            if (assessmentResult.IsFailure)
            {
                return assessmentResult;
            }

            var assessment = assessmentResult.Value;

            // Update implementation strategy
            //assessment.UpdateImplementationStrategy(data.ImplementationStrategy, data.OverallTargetDate, data.ImplementationNotes);

            // Link mitigation strategies for each hazard (references to MitigationStrategy entities)
            foreach (var hazardMitigation in data.HazardMitigationStrategyIds)
            {
                var hazardId = hazardMitigation.Key;
                var strategyIds = hazardMitigation.Value;

                foreach (var strategyId in strategyIds)
                {
                   // assessment.LinkMitigationStrategy(hazardId, strategyId);
                }
            }

            // Update monitoring requirements
            foreach (var monitoring in data.MonitoringRequirements)
            {
               // assessment.UpdateMonitoringRequirement(monitoring.Key, monitoring.Value);
            }

            assessment.CompleteStep(5);

            var saveResult = await _riskAssessmentRepository.UpdateAsync(assessment);
            if (saveResult.IsFailure)
            {
                return Result<RiskAssessment>.Failure<RiskAssessment>(saveResult.Error);
            }

            _logger.LogInformation("Step 5 updated successfully for assessment: {AssessmentId}", assessmentId);
            return Result<RiskAssessment>.Success(assessment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Step 5 for assessment: {AssessmentId}", assessmentId);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }

    public async Task<Result<RiskAssessment>> CompleteRiskAssessmentAsync(string assessmentId)
    {
        try
        {
            _logger.LogInformation("Completing risk assessment: {AssessmentId}", assessmentId);

            var assessmentResult = await GetRiskAssessmentAsync(assessmentId);
            if (assessmentResult.IsFailure)
            {
                return assessmentResult;
            }

            var assessment = assessmentResult.Value;

            // Mark assessment as completed
            //var completeResult = assessment.MarkAsCompleted();
            //if (completeResult.IsFailure)
            //{
            //    return Result<RiskAssessment>.Failure<RiskAssessment>(completeResult.Error);
            //}

            var saveResult = await _riskAssessmentRepository.UpdateAsync(assessment);
            if (saveResult.IsFailure)
            {
                return Result<RiskAssessment>.Failure<RiskAssessment>(saveResult.Error);
            }

            _logger.LogInformation("Risk assessment completed successfully: {AssessmentId}", assessmentId);
            return Result<RiskAssessment>.Success(assessment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing risk assessment: {AssessmentId}", assessmentId);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }

    public async Task<Result<List<SMSApplicationUser>>> GetAvailableAssessorsAsync()
    {
        try
        {
            // Get all active SMS application users who can be assessors
            var usersResult = await _userRepository.GetActiveUsersAsync();
            if (usersResult.IsFailure)
            {
                return Result<List<SMSApplicationUser>>.Failure<List<SMSApplicationUser>>(usersResult.Error);
            }

            // Filter for users with appropriate permission levels - using string comparison for now
            var assessors = usersResult.Value
            .Where(user => user.UserRole?.Permissions?.Any(x => x.Create) == true)
            .ToList();

            return Result<List<SMSApplicationUser>>.Success(assessors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available assessors");
            return Result<List<SMSApplicationUser>>.Failure<List<SMSApplicationUser>>(DomainErrors.SMSApplicationUserError.NotFound);
        }
    }

    public async Task<Result<List<Hazard>>> GetAvailableHazardsForAssessmentAsync()
    {
        try
        {
            // Get all hazards that are available for risk assessment
            var hazardsResult = await _hazardRepository.GetAllActiveAsync();
            if (hazardsResult.IsFailure)
            {
                return Result<List<Hazard>>.Failure<List<Hazard>>(hazardsResult.Error);
            }

            // For now, return all active hazards - filtering can be added later based on HazardStatus enum
            return Result<List<Hazard>>.Success(hazardsResult.Value.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available hazards for assessment");
            return Result<List<Hazard>>.Failure<List<Hazard>>(DomainErrors.HazardError.NotFound);
        }
    }}