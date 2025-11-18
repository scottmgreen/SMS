using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;
using SMS_Application.Messaging.Queries;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Domain.Common;
using SMS_Shared.Common;
using SMS_Application.Services;

namespace SMS.Presentation.Pages.SafetyRiskManagement;

/// <summary>
/// SIMPLIFIED Risk Assessment Wizard - Refactored for cleaner step handling
/// Uses internal step models for better organization while keeping same UI
/// </summary>

#region Helper Classes - Defined First for Reference

/// <summary>
/// Individual Hazard Risk Analysis - Much better name than AssessmentDataWrapper!
/// Represents the risk analysis for a single hazard within a report
/// </summary>
public class HazardRiskAnalysis
{
    public string HazardId { get; set; } = string.Empty;
    public string HazardDescription { get; set; } = string.Empty;
    public string HazardCategory { get; set; } = string.Empty;

    /// <summary>
    /// The worst credible (realistic) outcome that could result from this hazard
    /// </summary>
    [Required(ErrorMessage = "Worst credible outcome is required for risk analysis.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Worst credible outcome must be between 10 and 1000 characters.")]
    public string WorstCredibleOutcome { get; set; } = string.Empty;

    /// <summary>
    /// Root cause analysis identifying underlying causes that could lead to this hazard
    /// </summary>
    [Required(ErrorMessage = "Root cause analysis is required for risk analysis.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Root cause analysis must be between 10 and 1000 characters.")]
    public string RootCauseAnalysis { get; set; } = string.Empty;

    /// <summary>
    /// Additional comments, assumptions, or observations about this hazard's risk analysis
    /// </summary>
    public string AdditionalComments { get; set; } = string.Empty;

    /// <summary>
    /// When this analysis was last updated
    /// </summary>
    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Checks if this hazard analysis is complete
    /// </summary>
    public bool IsComplete => 
        !string.IsNullOrWhiteSpace(WorstCredibleOutcome) && WorstCredibleOutcome.Length >= 10 &&
        !string.IsNullOrWhiteSpace(RootCauseAnalysis) && RootCauseAnalysis.Length >= 10;
}

/// <summary>
/// Validation status for a single hazard in Step 3
/// Used for providing detailed UI feedback
/// </summary>
public class HazardValidationStatus
{
    public string HazardId { get; set; } = string.Empty;
    public string HazardDescription { get; set; } = string.Empty;
    public bool HasWorstOutcome { get; set; }
    public bool HasRootCause { get; set; }
    public int WorstOutcomeLength { get; set; }
    public int RootCauseLength { get; set; }
    public bool IsWorstOutcomeValid { get; set; }
    public bool IsRootCauseValid { get; set; }
    public bool IsComplete { get; set; }

    public string WorstOutcomeValidationMessage =>
        !HasWorstOutcome ? "Required" :
        !IsWorstOutcomeValid ? $"{WorstOutcomeLength}/10 characters (minimum 10 required)" :
        $"{WorstOutcomeLength} characters ✓";

    public string RootCauseValidationMessage =>
        !HasRootCause ? "Required" :
        !IsRootCauseValid ? $"{RootCauseLength}/10 characters (minimum 10 required)" :
        $"{RootCauseLength} characters ✓";

    public string ValidationCssClass =>
        IsComplete ? "is-valid" :
        (HasWorstOutcome || HasRootCause) ? "is-invalid" :
        "";
}

#endregion

public class RiskAssessmentWizardModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly RiskAssessmentService _riskAssessmentService;
    private readonly ILogger<RiskAssessmentWizardModel> _logger;

    public RiskAssessmentWizardModel(
        IMediator mediator, 
        RiskAssessmentService riskAssessmentService,
        ILogger<RiskAssessmentWizardModel> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _riskAssessmentService = riskAssessmentService ?? throw new ArgumentNullException(nameof(riskAssessmentService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Route Properties

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public int StepNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string? HazardId { get; set; }

    #endregion

    #region Core Data

    public RiskAssessment? Assessment { get; set; }
    public Hazard? PrimaryHazard { get; set; }
    public Report? SourceReport { get; set; }
    public AirportSharedDataset? SharedDataset { get; set; }
    public List<Hazard> RelatedHazards { get; set; } = new();

    #endregion

    #region Step Models - Properties for UI Binding

    [BindProperty]
    public Step1Model Step1 { get; set; } = new();

    [BindProperty] 
    public Step2Model Step2 { get; set; } = new();

    [BindProperty]
    public Step3Model Step3 { get; set; } = new();

    [BindProperty]
    public Step4Model Step4 { get; set; } = new();

    [BindProperty]
    public Step5Model Step5 { get; set; } = new();

    #endregion

    #region UI Helper Properties - CLEANED UP

    // Essential UI properties that the views expect
    public string AssessmentName => GetAssessmentName();
    public string AssessmentId => Id;
    public string LeadAssessorName => AvailableAssessors.FirstOrDefault(a => a.Id.Value == Step1.LeadAssessor)?.DisplayName ?? Step1.LeadAssessor;
    public int CurrentStep => StepNumber;
    public string CurrentStepName => GetStepName(StepNumber);
    //public int IdentifiedHazardsCount => Assessment?.IdentifiedHazardIds.Count ?? RelatedHazards.Count;

    // Step 5 compatibility properties (now that Step5 exists)
    public string SavedImplementationStrategy => Step5.ImplementationStrategy;
    public string SavedOverallTargetDate => Step5.OverallTargetDate?.ToString("yyyy-MM-dd") ?? string.Empty;
    public string SavedImplementationNotes => Step5.ImplementationNotes;
    public Dictionary<string, List<string>> SavedMitigationStrategies => Step5.SavedMitigationStrategies;

    #endregion

    #region Reference Data for UI

    public List<SMSApplicationUser> AvailableAssessors { get; set; } = new();
    public List<SMSStakeholderUser> AvailableStakeholders { get; set; } = new();
    public List<StakeholderGroup> StakeholderGroups_Data { get; set; } = new();
    public List<string> SelectedStakeholderIds { get; set; } = new();
    public List<Hazard> AvailableHazards => RelatedHazards;
    public RiskAssessmentDataWrapper? AssessmentData { get; set; }
    public int Count => RelatedHazards.Count;

    // Additional properties for specific step functionality
    [BindProperty]
    public List<string> AdditionalComments { get; set; } = new();
    
    public List<string> SelectedPanelMembers { get; set; } = new();
    public Dictionary<string, List<PanelMemberScoreData>> PanelScores { get; set; } = new();
    public Dictionary<string, double> HazardAverageScores { get; set; } = new();
    public Dictionary<string, List<string>> HazardPanelMembers { get; set; } = new();
    
    // SMS Users alias for Step 4 and Step 5
    public List<SMSApplicationUser> AvailableSMSUsers => AvailableAssessors;

    // Additional compatibility properties
    public List<object> CompletedScores { get; set; } = new();
    public List<object> PendingScores { get; set; } = new();

    #endregion

    #region SIMPLIFIED Validation Methods - Using Step Models

    /// <summary>
    /// Validates all steps in the wizard
    /// Simplified to use the Validate() methods of each step model in order
    /// </summary>
    public (bool isValid, string message) ValidateAllSteps()
    {
        // Step 1 validation
        var step1Validation = Step1.Validate();
        if (!step1Validation.isValid)
        {
            return (false, $"Step 1: {step1Validation.message}");
        }

        // Step 2 validation
        var step2Validation = Step2.Validate();
        if (!step2Validation.isValid)
        {
            return (false, $"Step 2: {step2Validation.message}");
        }

        // Step 3 validation
        var step3Validation = Step3.Validate(RelatedHazards);
        if (!step3Validation.isValid)
        {
            return (false, $"Step 3: {step3Validation.message}");
        }

        // All steps valid
        return (true, "All steps are valid");
    }

    #endregion

    #region SIMPLIFIED Apply Methods - Using Step Models

    /// <summary>
    /// Applies all steps to the assessment
    /// Simplified to use the ApplyToAssessment() methods of each step model in order
    /// </summary>
    public void ApplyAllSteps(RiskAssessment assessment)
    {
        // Step 1
        Step1.ApplyToAssessment(assessment);

        // Step 2
        Step2.ApplyToAssessment(assessment);

        // Step 3
        Step3.ApplyToAssessment(assessment);

        // Step 4 - Risk Scoring Panel
        Step4.ApplyToAssessment(assessment);

        // Step 5 - Implementation Planning
        Step5.ApplyToAssessment(assessment);
    }

    #endregion

    #region Helper Classes - Shared for All Steps

    /// <summary>
    /// RENAMED: Better name for what this actually represents
    /// Wrapper for assessment data used across multiple steps
    /// Contains identified hazards and implementation planning data
    /// </summary>
    public class RiskAssessmentDataWrapper
    {
        public List<IdentifiedHazardDto> IdentifiedHazards { get; set; } = new();
        public string ImplementationPlan { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data Transfer Object for an identified hazard
    /// Used to pass hazard data between steps and to/from UI
    /// </summary>
    public class IdentifiedHazardDto
    {
        public string Id { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string WorstCredibleOutcome { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Likelihood { get; set; } = string.Empty;
        public string Tolerability { get; set; } = string.Empty;
        public List<string> ProposedMitigations { get; set; } = new();
        public List<string> CurrentMitigations { get; set; } = new();
    }

    public class StakeholderGroup
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Members { get; set; } = new();
        public int MemberCount => Members.Count;
    }

    public class PanelMemberScoreData
    {
        public string PanelMemberId { get; set; } = string.Empty;
        public string MemberId { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;
        public int SeverityScore { get; set; }
        public int LikelihoodScore { get; set; }
        public double CalculatedScore => SeverityScore * LikelihoodScore;
        public string RiskLevel { get; set; } = string.Empty;
        public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;
        public bool IsComplete => SeverityScore > 0 && LikelihoodScore > 0;

        public PanelMemberScoreData()
        {
            MemberId = PanelMemberId;
        }
    }

    #endregion

    #region UI Helper Methods

    public string GetStepName(int stepNumber)
    {
        return stepNumber switch
        {
            1 => "System Description",
            2 => "Hazard Identification", 
            3 => "Risk Analysis",
            4 => "Risk Assessment",
            5 => "Risk Mitigation",
            _ => "Unknown Step"
        };
    }

    public string GetStepIcon(int stepNumber)
    {
        return stepNumber switch
        {
            1 => "fas fa-cogs",
            2 => "fas fa-exclamation-triangle",
            3 => "fas fa-chart-line",
            4 => "fas fa-balance-scale",
            5 => "fas fa-shield-alt",
            _ => "fas fa-question"
        };
    }

    public bool CanSkipToStep(int targetStep)
    {
        return Assessment?.IsStepCompleted(targetStep - 1) ?? (targetStep == 1);
    }

    public int GetLastCompletedStep()
    {
        return Assessment?.CompletedSteps.LastOrDefault() ?? 0;
    }

    public int GetRecommendedStep()
    {
        return Assessment?.GetNextRecommendedStep() ?? 1;
    }

    public string GetAssessmentName()
    {
        return Assessment?.Name ?? "Risk Assessment";
    }

    public string GetRiskLevelClass(string riskLevel)
    {
        return riskLevel?.ToLowerInvariant() switch
        {
            "high" or "5" or "4" => "bg-danger text-white",
            "medium" or "3" => "bg-warning text-dark",
            "low" or "2" or "1" => "bg-success text-white",
            _ => "bg-secondary text-white"
        };
    }

    #endregion

    #region Auto-Save Handlers

    /// <summary>
    /// Handles auto-save requests from the client
    /// </summary>
    public async Task<IActionResult> OnPostAutoSaveStep3Async()
    {
        try
        {
            if (Assessment == null)
            {
                await LoadAssessmentDataAsync();
                if (Assessment == null)
                {
                    return new JsonResult(new { success = false, message = "Assessment not found" });
                }
            }

            // Get form data from request
            var formData = new Dictionary<string, string>();
            foreach (var key in Request.Form.Keys)
            {
                formData[key] = Request.Form[key];
            }

            // Auto-save to Step3 model
            Step3.AutoSave(Id, formData);

            // Optionally save to database for persistence
            if (Step3.HazardAnalyses.Values.Any(ha => !string.IsNullOrWhiteSpace(ha.WorstCredibleOutcome) || !string.IsNullOrWhiteSpace(ha.RootCauseAnalysis)))
            {
                Step3.ApplyToAssessment(Assessment);
                await SaveAssessmentToDatabaseAsync();
            }

            _logger.LogInformation("Auto-saved Step 3 data for Assessment {AssessmentId}", Id);
            
            return new JsonResult(new { 
                success = true, 
                message = "Auto-saved", 
                savedAt = DateTime.Now.ToString("HH:mm:ss"),
                hasUnsavedChanges = Step3.HasUnsavedChanges()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error auto-saving Step 3 data for Assessment {AssessmentId}", Id);
            return new JsonResult(new { success = false, message = "Auto-save failed" });
        }
    }

    /// <summary>
    /// Gets validation status for all hazards in Step 3
    /// </summary>
    public async Task<IActionResult> OnGetStep3ValidationStatusAsync()
    {
        try
        {
            var validationStatuses = Step3.GetHazardValidationStatuses(RelatedHazards);
            var (isValid, message, errors) = Step3.ValidateDetailed(RelatedHazards);
            
            return new JsonResult(new { 
                success = true,
                isValid = isValid,
                message = message,
                validationErrors = errors,
                hazardStatuses = validationStatuses,
                completionPercentage = Step3.GetCompletionPercentage(RelatedHazards)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Step 3 validation status for Assessment {AssessmentId}", Id);
            return new JsonResult(new { success = false, message = "Validation check failed" });
        }
    }

    #endregion

    #region Data Loading and Saving Methods

    /// <summary>
    /// Loads assessment data from the database using Application layer
    /// </summary>
    private async Task LoadAssessmentDataAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(Id))
            {
                _logger.LogWarning("Cannot load assessment data - ID is empty");
                return;
            }

            _logger.LogInformation("Loading assessment data for ID: {AssessmentId}", Id);
            
            // Use the enhanced RiskAssessmentService to load data
            var riskAssessmentId = new RiskAssessmentID(Id);
            var result = await _riskAssessmentService.GetRiskAssessmentByIdAsync(riskAssessmentId);
            
            if (result.IsSuccess)
            {
                Assessment = result.Value;
                LoadStepDataFromAssessment();
                _logger.LogInformation("Successfully loaded assessment data for ID: {AssessmentId}", Id);
            }
            else
            {
                _logger.LogWarning("Assessment not found for ID: {AssessmentId}. Error: {Error}", Id, result.Error?.Message);
                Assessment = null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading assessment data for ID: {AssessmentId}", Id);
        }
    }

    /// <summary>
    /// Creates a new risk assessment using Application layer
    /// </summary>
    private async Task<RiskAssessment?> CreateNewRiskAssessmentAsync()
    {
        try
        {
            _logger.LogInformation("Creating new risk assessment for HazardId: {HazardId}", HazardId);

            // Generate a proper RiskAssessment ID if Id is "new"
            var actualId = Id;
            if (string.IsNullOrEmpty(actualId) || actualId == "new")
            {
                actualId = $"RA-0000";
                Id = actualId; // Update the page property
            }

            // Create new RiskAssessment entity with proper defaults
            var riskAssessmentId = new RiskAssessmentID(actualId);
            
            // Use HazardId from the URL if available, otherwise default name
            var assessmentName = !string.IsNullOrEmpty(HazardId) 
                ? $"Risk Assessment for Hazard {HazardId}" 
                : "New Risk Assessment";
            
            // Use a default lead assessor if Step1 is empty
            var leadAssessor = !string.IsNullOrEmpty(Step1?.LeadAssessor) 
                ? Step1.LeadAssessor 
                : "SYSTEM";

            var createResult = RiskAssessment.CreateInitial(
                riskAssessmentId,
                assessmentName,
                leadAssessor,
                RiskAssessmentCategory.FiveStep,
                HazardId, // primaryHazardId
                HazardId  // hazardCode
            );

            if (createResult.IsFailure)
            {
                _logger.LogError("Failed to create RiskAssessment entity: {Error}", createResult.Error.Message);
                return null;
            }

            var newAssessment = createResult.Value;

            // Save to database using Application layer
            var saveResult = await _riskAssessmentService.CreateRiskAssessmentAsync(newAssessment);
            
            if (saveResult.IsSuccess)
            {
                _logger.LogInformation("Successfully created new risk assessment with ID: {Id}", saveResult.Value?.Id);
                return saveResult.Value;
            }
            else
            {
                _logger.LogError("Failed to save new risk assessment: {Error}", saveResult.Error?.Message);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating new risk assessment");
            return null;
        }
    }

    /// <summary>
    /// Saves assessment data to database using Application layer
    /// </summary>
    private async Task SaveAssessmentToDatabaseAsync()
    {
        try
        {
            if (Assessment == null)
            {
                _logger.LogWarning("Cannot save assessment data - Assessment is null");
                return;
            }

            _logger.LogInformation("Saving assessment data for ID: {AssessmentId}", Assessment.Id);
            
            // Use the enhanced RiskAssessmentService to save data
            var result = await _riskAssessmentService.UpdateRiskAssessmentAsync(Assessment);
            
            if (result.IsSuccess)
            {
                // Update our cached assessment with the latest data
                Assessment = result.Value;
                _logger.LogInformation("Successfully saved assessment data for ID: {AssessmentId}", Assessment.Id);
            }
            else
            {
                _logger.LogError("Failed to save assessment data: {Error}", result.Error?.Message);
                throw new InvalidOperationException($"Failed to save assessment: {result.Error?.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving assessment data for ID: {AssessmentId}", Assessment?.Id?.Value ?? "Unknown");
            throw; // Re-throw so calling method knows save failed
        }
    }

    /// <summary>
    /// Loads step model data from the assessment entity
    /// </summary>
    private void LoadStepDataFromAssessment()
    {
        if (Assessment == null) return;

        try
        {
            // Load data into step models from assessment
            Step1.LoadFromAssessment(Assessment);
            //Step2.LoadFromAssessment(Assessment); // If needed
            Step3.LoadFromAssessment(Assessment, RelatedHazards);
            // Step4.LoadFromAssessment(Assessment); // If needed  
            // Step5.LoadFromAssessment(Assessment); // If needed

            _logger.LogInformation("Step data loaded from assessment {AssessmentId}", Assessment.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading step data from assessment {AssessmentId}", Assessment?.Id?.Value ?? "Unknown");
        }
    }

    /// <summary>
    /// Loads the related hazard data for the current HazardId
    /// </summary>
    private async Task LoadRelatedHazardAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(HazardId))
            {
                _logger.LogInformation("No HazardId provided - skipping hazard load");
                return;
            }

            _logger.LogInformation("Loading hazard data for HazardId: {HazardId}", HazardId);

            // Load the primary hazard
            var hazardQuery = new GetHazardByIdQuery(new HazardID(HazardId));
            var hazardResult = await _mediator.SendAsync(hazardQuery, CancellationToken.None);
            
            if (hazardResult.IsSuccess)
            {
                PrimaryHazard = hazardResult.Value;
                _logger.LogInformation("Successfully loaded primary hazard: {HazardCode}", PrimaryHazard.Code);

                // Add the primary hazard to RelatedHazards for Step 3 analysis
                RelatedHazards = new List<Hazard> { PrimaryHazard };

                // Try to load the source report if available
                if (!string.IsNullOrEmpty(PrimaryHazard.ReportCode))
                {
                    var reportQuery = new GetReportByIdQuery(new ReportID(PrimaryHazard.ReportCode));
                    var reportResult = await _mediator.SendAsync(reportQuery, CancellationToken.None);
                    if (reportResult.IsSuccess)
                    {
                        SourceReport = reportResult.Value;
                        _logger.LogInformation("Successfully loaded source report: {ReportCode}", SourceReport.Code);
                    }
                }
            }
            else
            {
                _logger.LogWarning("Failed to load hazard with HazardId: {HazardId}. Error: {Error}", 
                    HazardId, hazardResult.Error?.Message);
                
                RelatedHazards = new List<Hazard>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading related hazard for HazardId: {HazardId}", HazardId);
            RelatedHazards = new List<Hazard>();
        }
    }

    #endregion

    #region Page Handler Methods - Converted from JavaScript

    /// <summary>
    /// Handles "Save and Navigate Next" button click - using modular step save approach
    /// Much easier to debug and test individual steps
    /// </summary>
    public async Task<IActionResult> OnPostSaveAndNavigateNextAsync()
    {
        try
        {
            _logger.LogInformation("Save and Navigate Next: Step {CurrentStep} for Assessment {AssessmentId}, HazardId: {HazardId}", StepNumber, Id, HazardId);

            // Use step-specific save method
            var saveResult = await SaveCurrentStepAsync();

            if (!saveResult.success)
            {
                TempData["ErrorMessage"] = $"Step {StepNumber} save failed: {saveResult.message}";
                return Page();
            }

            // Determine next step
            var nextStep = StepNumber + 1;
            if (nextStep > 5)
            {
                // If we're on step 5, complete the assessment instead
                TempData["SuccessMessage"] = "Assessment completed successfully!";
                return RedirectToPage("/SafetyRiskManagement/RiskAssessment", new { assessmentId = Id });
            }

            // ✅ FIXED: Navigate to next step and preserve HazardId
            TempData["SuccessMessage"] = saveResult.message;
            return RedirectToPage("/SafetyRiskManagement/RiskAssessmentWizard", 
                new { id = Id, stepNumber = nextStep, hazardId = HazardId });

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Save and Navigate Next for Assessment {AssessmentId}, Step {StepNumber}", Id, StepNumber);
            TempData["ErrorMessage"] = "An unexpected error occurred while saving and navigating. Please try again.";
            return Page();
        }
    }

    ///// <summary>
    ///// Handles "Manual Save Current Step" button click - using modular approach
    ///// </summary>
    //public async Task<IActionResult> OnPostManualSaveCurrentStepAsync()
    //{
    //    try
    //    {
    //        _logger.LogInformation("Manual Save: Step {CurrentStep} for Assessment {AssessmentId}", StepNumber, Id);

    //        // Use step-specific save method (no validation required for manual save)
    //        var saveResult = await SaveCurrentStepAsync();

    //        if (saveResult.success)
    //        {
    //            TempData["SuccessMessage"] = saveResult.message;
    //        }
    //        else
    //        {
    //            TempData["ErrorMessage"] = saveResult.message;
    //        }

    //        return Page();

    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error in Manual Save for Assessment {AssessmentId}, Step {StepNumber}", Id, StepNumber);
    //        TempData["ErrorMessage"] = "An error occurred while saving progress. Please try again.";
    //        return Page();
    //    }
    //}

    ///// <summary>
    ///// Handles navigation to a specific step - converted from step navigation JavaScript
    ///// </summary>
    //public async Task<IActionResult> OnPostNavigateToStepAsync(int targetStep)
    //{
    //    try
    //    {
    //        _logger.LogInformation("Navigate to Step: {TargetStep} from {CurrentStep} for Assessment {AssessmentId}", targetStep, StepNumber, Id);

    //        // Validate target step is within range
    //        if (targetStep < 1 || targetStep > 5)
    //        {
    //            TempData["ErrorMessage"] = "Invalid step number.";
    //            return Page();
    //        }

    //        // Check if user can navigate to target step
    //        if (targetStep > StepNumber && !CanSkipToStep(targetStep))
    //        {
    //            TempData["ErrorMessage"] = $"Please complete Step {StepNumber} before proceeding to Step {targetStep}.";
    //            return Page();
    //        }

    //        // If navigating forward, validate and save current step
    //        if (targetStep > StepNumber)
    //        {
    //            var validationResult = ValidateCurrentStep();
    //            if (!validationResult.isValid)
    //            {
    //                TempData["ErrorMessage"] = $"Please complete Step {StepNumber} before proceeding: {validationResult.message}";
    //                return Page();
    //            }

    //            // Save current step before navigation
    //            await SaveCurrentStepAsync();
    //            TempData["SuccessMessage"] = $"Step {StepNumber} saved successfully!";
    //        }

    //        // Navigate to target step
    //        return RedirectToPage("/SafetyRiskManagement/RiskAssessmentWizard", new { id = Id, stepNumber = targetStep });

    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error navigating to step {TargetStep} for Assessment {AssessmentId}", targetStep, Id);
    //        TempData["ErrorMessage"] = "An error occurred during navigation. Please try again.";
    //        return Page();
    //    }
    //}

    ///// <summary>
    ///// Handles "Previous Step" navigation - converted from JavaScript
    ///// </summary>
    //public async Task<IActionResult> OnPostPreviousStepAsync()
    //{
    //    try
    //    {
    //        var previousStep = StepNumber - 1;
    //        if (previousStep < 1)
    //        {
    //            previousStep = 1;
    //        }

    //        _logger.LogInformation("Navigate to Previous Step: {PreviousStep} from {CurrentStep}", previousStep, StepNumber);

    //        // Auto-save current progress before navigating back
    //        await SaveCurrentStepAsync();

    //        return RedirectToPage("/SafetyRiskManagement/RiskAssessmentWizard", new { id = Id, stepNumber = previousStep });
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error navigating to previous step for Assessment {AssessmentId}", Id);
    //        TempData["ErrorMessage"] = "An error occurred during navigation. Please try again.";
    //        return Page();
    //    }
    //}

    /// <summary>
    /// Handles final assessment completion - converted from JavaScript
    /// </summary>
    public async Task<IActionResult> OnPostCompleteAssessmentAsync()
    {
        try
        {
            _logger.LogInformation("Complete Assessment: {AssessmentId}", Id);

            // Load current assessment data if needed
            if (Assessment == null)
            {
                await LoadAssessmentDataAsync();
            }

            // Validate all steps are complete
            var allStepsValidation = ValidateAllSteps();
            if (!allStepsValidation.isValid)
            {
                TempData["ErrorMessage"] = $"Assessment cannot be completed: {allStepsValidation.message}";
                return Page();
            }

            // Save final step and mark assessment as complete
            await SaveCurrentStepAsync();
            await CompleteAssessmentAsync();

            TempData["SuccessMessage"] = "Risk Assessment completed successfully!";
            return RedirectToPage("/SafetyRiskManagement/RiskAssessment", new { assessmentId = Id });

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing Assessment {AssessmentId}", Id);
            TempData["ErrorMessage"] = "An error occurred while completing the assessment. Please try again.";
            return Page();
        }
    }

    #endregion

    #region Supporting Methods for Page Handlers

    /// <summary>
    /// Validates the current step based on the StepNumber
    /// </summary>
    private (bool isValid, string message) ValidateCurrentStep()
    {
        return StepNumber switch
        {
            1 => Step1?.Validate() ?? (false, "Step 1 data not available"),
            2 => Step2?.Validate() ?? (false, "Step 2 data not available"),
            3 => Step3?.Validate(RelatedHazards) ?? (false, "Step 3 data not available"),
            4 => Step4?.Validate() ?? (false, "Step 4 data not available"),
            5 => Step5?.Validate() ?? (false, "Step 5 data not available"),
            _ => (false, "Invalid step number")
        };
    }

    /// <summary>
    /// Completes the entire assessment process
    /// </summary>
    private async Task CompleteAssessmentAsync()
    {
        try
        {
            if (Assessment == null)
            {
                _logger.LogWarning("Cannot complete assessment - Assessment is null");
                return;
            }

            // Apply all step data to ensure everything is saved
            ApplyAllSteps(Assessment);

            // Mark assessment as complete
            // TODO: Add method to RiskAssessment to mark as complete
            // Assessment.MarkAsComplete();

            // Save final state to database
            await SaveAssessmentToDatabaseAsync();
            
            _logger.LogInformation("Assessment {AssessmentId} completed successfully", Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing Assessment {AssessmentId}", Id);
            throw;
        }
    }

    #endregion

    #region Step-Specific Save Methods - Modular Approach

    /// <summary>
    /// Saves Step 1 data using Application layer service
    /// </summary>
    private async Task<(bool success, string message)> SaveStep1Async()
    {
        try
        {
            _logger.LogInformation("Saving Step 1 for Assessment {AssessmentId}", Id);

            // Validate Step 1 model binding
            if (Step1 == null)
            {
                return (false, "Step 1 data is not properly bound");
            }

            // Validate Step 1 data
            var validation = Step1.Validate();
            if (!validation.isValid)
            {
                return (false, validation.message);
            }

            // Create or load assessment
            if (Assessment == null)
            {
                // Create new assessment for Step 1
                Assessment = await CreateNewRiskAssessmentAsync();
                if (Assessment == null)
                {
                    return (false, "Failed to create new risk assessment");
                }
            }

            // Use Application layer service to save Step 1 data
            var saveResult = await _riskAssessmentService.SaveStep1Async(
                Assessment.Code,
                Step1.LeadAssessor,
                Step1.SystemDescription,
                Step1.SystemBoundaries,
                Step1.SystemPurpose,
                Step1.FiveMPersonnel,
                Step1.FiveMEquipment,
                Step1.FiveMProcedures,
                Step1.FiveMResources,
                Step1.FiveMPhysicalEnvironment,
                Step1.FiveMOperationalEnvironment,
                "WIZARD_USER"
            );

            if (saveResult.IsSuccess)
            {
                // Update our cached assessment
                Assessment = saveResult.Value;
                _logger.LogInformation("Step 1 saved successfully for Assessment {AssessmentId}", Assessment.Id);
                return (true, "Step 1 saved successfully");
            }
            else
            {
                _logger.LogError("Failed to save Step 1: {Error}", saveResult.Error?.Message);
                return (false, $"Error saving Step 1: {saveResult.Error?.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving Step 1 for Assessment {AssessmentId}", Id);
            return (false, $"Error saving Step 1: {ex.Message}");
        }
    }

    /// <summary>
    /// Saves Step 2 data specifically - easier to debug and test
    /// </summary>
    private async Task<(bool success, string message)> SaveStep2Async()
    {
        try
        {
            _logger.LogInformation("Saving Step 2 for Assessment {AssessmentId}", Id);

            // Validate Step 2 model binding
            if (Step2 == null)
            {
                return (false, "Step 2 data is not properly bound");
            }

            // Validate Step 2 data
            var validation = Step2.Validate();
            if (!validation.isValid)
            {
                return (false, validation.message);
            }

            // Ensure we have assessment loaded
            if (Assessment == null)
            {
                await LoadAssessmentDataAsync();
                if (Assessment == null)
                {
                    return (false, "Assessment not found - please complete Step 1 first");
                }
            }

            // Apply Step 2 data to assessment
            Step2.ApplyToAssessment(Assessment);
            
            // Save to database
            await SaveAssessmentToDatabaseAsync();

            _logger.LogInformation("Step 2 saved successfully for Assessment {AssessmentId}", Assessment.Id);
            return (true, "Step 2 saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving Step 2 for Assessment {AssessmentId}", Id);
            return (false, $"Error saving Step 2: {ex.Message}");
        }
    }

    /// <summary>
    /// Saves Step 3 data using Application layer service
    /// </summary>
    private async Task<(bool success, string message)> SaveStep3Async()
    {
        try
        {
            _logger.LogInformation("Saving Step 3 for Assessment {AssessmentId}", Id);

            // Validate Step 3 model binding
            if (Step3 == null)
            {
                return (false, "Step 3 data is not properly bound");
            }

            // Validate Step 3 data
            var validation = Step3.Validate(RelatedHazards);
            if (!validation.isValid)
            {
                return (false, validation.message);
            }

            // Ensure we have assessment loaded
            if (Assessment == null)
            {
                await LoadAssessmentDataAsync();
                if (Assessment == null)
                {
                    return (false, "Assessment not found - please complete previous steps first");
                }
            }

            // Use Application layer service to save Step 3 data
            var saveResult = await _riskAssessmentService.SaveStep3Async(
                Id,
                Step3.RiskAnalysisMethod,
                Step3.RiskCriteria,
                "WIZARD_USER"
            );

            if (saveResult.IsSuccess)
            {
                // Apply Step 3 data to assessment (for hazard-specific analysis)
                Step3.ApplyToAssessment(Assessment);
                
                // Update our cached assessment
                Assessment = saveResult.Value;
                _logger.LogInformation("Step 3 saved successfully for Assessment {AssessmentId}", Assessment.Id);
                return (true, "Step 3 saved successfully");
            }
            else
            {
                _logger.LogError("Failed to save Step 3: {Error}", saveResult.Error?.Message);
                return (false, $"Error saving Step 3: {saveResult.Error?.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving Step 3 for Assessment {AssessmentId}", Id);
            return (false, $"Error saving Step 3: {ex.Message}");
        }
    }

    /// <summary>
    /// Saves Step 4 data specifically - easier to debug and test
    /// </summary>
    private async Task<(bool success, string message)> SaveStep4Async()
    {
        try
        {
            _logger.LogInformation("Saving Step 4 for Assessment {AssessmentId}", Id);

            // Validate Step 4 model binding
            if (Step4 == null)
            {
                return (false, "Step 4 data is not properly bound");
            }

            // Validate Step 4 data
            var validation = Step4.Validate();
            if (!validation.isValid)
            {
                return (false, validation.message);
            }

            // Ensure we have assessment loaded
            if (Assessment == null)
            {
                await LoadAssessmentDataAsync();
                if (Assessment == null)
                {
                    return (false, "Assessment not found - please complete previous steps first");
                }
            }

            // Apply Step 4 data to assessment
            Step4.ApplyToAssessment(Assessment);
            
            // Save to database
            await SaveAssessmentToDatabaseAsync();

            _logger.LogInformation("Step 4 saved successfully for Assessment {AssessmentId}", Assessment.Id);
            return (true, "Step 4 saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving Step 4 for Assessment {AssessmentId}", Id);
            return (false, $"Error saving Step 4: {ex.Message}");
        }
    }

    /// <summary>
    /// Saves Step 5 data specifically - easier to debug and test
    /// </summary>
    private async Task<(bool success, string message)> SaveStep5Async()
    {
        try
        {
            _logger.LogInformation("Saving Step 5 for Assessment {AssessmentId}", Id);

            // Validate Step 5 model binding
            if (Step5 == null)
            {
                return (false, "Step 5 data is not properly bound");
            }

            // Validate Step 5 data
            var validation = Step5.Validate();
            if (!validation.isValid)
            {
                return (false, validation.message);
            }

            // Ensure we have assessment loaded
            if (Assessment == null)
            {
                await LoadAssessmentDataAsync();
                if (Assessment == null)
                {
                    return (false, "Assessment not found - please complete previous steps first");
                }
            }

            // Apply Step 5 data to assessment
            Step5.ApplyToAssessment(Assessment);
            
            // Save to database
            await SaveAssessmentToDatabaseAsync();

            _logger.LogInformation("Step 5 saved successfully for Assessment {AssessmentId}", Assessment.Id);
            return (true, "Step 5 saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving Step 5 for Assessment {AssessmentId}", Id);
            return (false, $"Error saving Step 5: {ex.Message}");
        }
    }

    /// <summary>
    /// Generic step save dispatcher - calls the appropriate step save method
    /// </summary>
    private async Task<(bool success, string message)> SaveCurrentStepAsync()
    {
        return StepNumber switch
        {
            1 => await SaveStep1Async(),
            2 => await SaveStep2Async(),
            3 => await SaveStep3Async(),
            4 => await SaveStep4Async(),
            5 => await SaveStep5Async(),
            _ => (false, $"Invalid step number: {StepNumber}")
        };
    }

    #endregion

    #region Page Load and Initialization

    /// <summary>
    /// Handles GET requests - loads assessment data and initializes step models
    /// </summary>
    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            _logger.LogInformation("Loading Risk Assessment Wizard - Step {StepNumber}, Assessment {AssessmentId}, HazardId: {HazardId}", StepNumber, Id, HazardId);

            // Validate required parameters
            if (StepNumber < 1 || StepNumber > 5)
            {
                _logger.LogWarning("Invalid step number {StepNumber}", StepNumber);
                return RedirectToPage("/SafetyRiskManagement/RiskAssessmentWizard", 
                    new { id = Id, stepNumber = 1, hazardId = HazardId });
            }

            // ✅ FIXED: Load the related hazard first if we have a HazardId
            if (!string.IsNullOrEmpty(HazardId))
            {
                await LoadRelatedHazardAsync();
            }

            // Load assessment data if we have an ID
            if (!string.IsNullOrEmpty(Id))
            {
                await LoadAssessmentDataAsync();
                
                // If assessment loaded, populate step models
                if (Assessment != null)
                {
                    LoadStepDataFromAssessment();
                }
            }

            // Initialize any required reference data
            await LoadReferenceDataAsync();

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Risk Assessment Wizard page");
            TempData["ErrorMessage"] = "Error loading the assessment wizard. Please try again.";
            return RedirectToPage("/SafetyRiskManagement/RiskAssessment");
        }
    }

    /// <summary>
    /// Loads reference data needed for dropdowns and selections using Application layer
    /// </summary>
    private async Task LoadReferenceDataAsync()
    {
        try
        {
            // TODO: Implement queries for reference data when available
            // Example for when queries are implemented:
            // var assessorsQuery = new GetSMSApplicationUsersQuery();
            // var assessorsResult = await _mediator.SendAsync(assessorsQuery, CancellationToken.None);
            // if (assessorsResult.IsSuccess)
            // {
            //     AvailableAssessors = assessorsResult.Value.ToList();
            // }
            
            // var stakeholdersQuery = new GetSMSStakeholderUsersQuery();
            // var stakeholdersResult = await _mediator.SendAsync(stakeholdersQuery, CancellationToken.None);
            // if (stakeholdersResult.IsSuccess)
            // {
            //     AvailableStakeholders = stakeholdersResult.Value.ToList();
            // }

            // For now, initialize to empty lists to prevent null reference errors
            AvailableAssessors = new List<SMSApplicationUser>();
            AvailableStakeholders = new List<SMSStakeholderUser>();
            StakeholderGroups_Data = new List<StakeholderGroup>();
            RelatedHazards = new List<Hazard>();

            _logger.LogInformation("Reference data initialized for Assessment {AssessmentId}", Id);
            await Task.CompletedTask; // Placeholder
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading reference data");
            // Don't throw - just log the error and continue with empty lists
        }
    }

    #endregion
}

public class Step1Model 
{
    #region System Overview Properties

    [Required(ErrorMessage = "Lead Assessor is required.")]
    [Display(Name = "Lead Assessor")]
    public string LeadAssessor { get; set; } = string.Empty;

    [Required(ErrorMessage = "System Description is required.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "System Description must be between 10 and 1000 characters.")]
    [Display(Name = "System Description")]
    public string SystemDescription { get; set; } = string.Empty;

    [Required(ErrorMessage = "System Boundaries are required.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "System Boundaries must be between 10 and 1000 characters.")]
    [Display(Name = "System Boundaries")]
    public string SystemBoundaries { get; set; } = string.Empty;

    [Required(ErrorMessage = "System Purpose is required.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "System Purpose must be between 10 and 1000 characters.")]
    [Display(Name = "System Purpose")]
    public string SystemPurpose { get; set; } = string.Empty;

    #endregion

    #region 5M Framework Properties

    [Required(ErrorMessage = "Personnel Factors (5M People) are required.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Personnel Factors must be between 10 and 1000 characters.")]
    [Display(Name = "Personnel Factors")]
    public string FiveMPersonnel { get; set; } = string.Empty;

    [Required(ErrorMessage = "Equipment Factors (5M Equipment) are required.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Equipment Factors must be between 10 and 1000 characters.")]
    [Display(Name = "Equipment Factors")]
    public string FiveMEquipment { get; set; } = string.Empty;

    [Required(ErrorMessage = "Procedure Factors (5M Procedures) are required.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Procedure Factors must be between 10 and 1000 characters.")]
    [Display(Name = "Procedure Factors")]
    public string FiveMProcedures { get; set; } = string.Empty;

    [Required(ErrorMessage = "Resource Factors (5M Resources) are required.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Resource Factors must be between 10 and 1000 characters.")]
    [Display(Name = "Resource Factors")]
    public string FiveMResources { get; set; } = string.Empty;

    [Required(ErrorMessage = "Physical Environment Factors (5M Environment) are required.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Physical Environment must be between 10 and 1000 characters.")]
    [Display(Name = "Physical Environment")]
    public string FiveMPhysicalEnvironment { get; set; } = string.Empty;

    [Required(ErrorMessage = "Operational Environment Factors are required.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Operational Environment must be between 10 and 1000 characters.")]
    [Display(Name = "Operational Environment")]
    public string FiveMOperationalEnvironment { get; set; } = string.Empty;

    #endregion

    #region Stakeholder Properties

    [Display(Name = "Stakeholder Groups")]
    public string StakeholderGroups { get; set; } = string.Empty;

    [Display(Name = "Individual Stakeholders")]
    public string SelectedIndividualStakeholders { get; set; } = string.Empty;

    #endregion

    #region Step 1 Methods

    public (bool isValid, string message) Validate()
    {
        var step1Fields = new Dictionary<string, string>
        {
            { nameof(LeadAssessor), LeadAssessor },
            { nameof(SystemDescription), SystemDescription },
            { nameof(SystemBoundaries), SystemBoundaries },
            { nameof(SystemPurpose), SystemPurpose },
            { nameof(FiveMPersonnel), FiveMPersonnel },
            { nameof(FiveMEquipment), FiveMEquipment },
            { nameof(FiveMProcedures), FiveMProcedures },
            { nameof(FiveMResources), FiveMResources },
            { nameof(FiveMPhysicalEnvironment), FiveMPhysicalEnvironment },
            { nameof(FiveMOperationalEnvironment), FiveMOperationalEnvironment }
        };
        
        int fieldsWithData = 0;
        var missingFields = new List<string>();
        
        foreach (var field in step1Fields)
        {
            var value = field.Value?.Trim() ?? string.Empty;
            
            if (string.IsNullOrEmpty(value))
            {
                missingFields.Add(field.Key);
            }
            else if (value.Length >= 10)
            {
                fieldsWithData++;
            }
        }
        
        if (fieldsWithData < 4)
        {
            return (false, $"Need at least 4 complete fields (found {fieldsWithData}). Missing: {string.Join(", ", missingFields)}");
        }
        
        return (true, $"Step 1 validation passed with {fieldsWithData} complete fields");
    }

    public void ApplyToAssessment(RiskAssessment assessment)
    {
        assessment.SystemDescription = SystemDescription.Trim();
        assessment.SystemBoundaries = SystemBoundaries.Trim();
        assessment.SystemPurpose = SystemPurpose.Trim();
        assessment.FiveMPersonnel = FiveMPersonnel.Trim();
        assessment.FiveMEquipment = FiveMEquipment.Trim();
        assessment.FiveMProcedures = FiveMProcedures.Trim();
        assessment.FiveMResources = FiveMResources.Trim();
        assessment.FiveMOperationalEnvironment = FiveMOperationalEnvironment.Trim();
        assessment.FiveMPhysicalEnvironment = FiveMPhysicalEnvironment.Trim();
             
        assessment.LeadAssessorId = LeadAssessor.Trim();

        if (!string.IsNullOrEmpty(StakeholderGroups.Trim()))
        {
            var groups = StakeholderGroups.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(g => g.Trim())
                .Where(g => !string.IsNullOrEmpty(g));

            foreach (var group in groups)
            {
                assessment.AddStakeholder(group);
            }
        }

        assessment.CompleteStep(1);
    }

    public void LoadFromAssessment(RiskAssessment assessment)
    {
        if (assessment == null) return;

        if (string.IsNullOrEmpty(LeadAssessor)) LeadAssessor = assessment.LeadAssessorId ?? string.Empty;
        if (string.IsNullOrEmpty(SystemDescription)) SystemDescription = assessment.SystemDescription ?? string.Empty;
        if (string.IsNullOrEmpty(SystemBoundaries)) SystemBoundaries = assessment.SystemBoundaries ?? string.Empty;
        if (string.IsNullOrEmpty(SystemPurpose)) SystemPurpose = assessment.SystemPurpose ?? string.Empty;
        
        // CORRECTED: Use the proper 5M property names from the updated RiskAssessment entity
        if (string.IsNullOrEmpty(FiveMPersonnel)) FiveMPersonnel = assessment.FiveMPersonnel ?? string.Empty;
        if (string.IsNullOrEmpty(FiveMEquipment)) FiveMEquipment = assessment.FiveMEquipment ?? string.Empty;
        if (string.IsNullOrEmpty(FiveMProcedures)) FiveMProcedures = assessment.FiveMProcedures ?? string.Empty;
        if (string.IsNullOrEmpty(FiveMResources)) FiveMResources = assessment.FiveMResources ?? string.Empty;
        if (string.IsNullOrEmpty(FiveMPhysicalEnvironment)) FiveMPhysicalEnvironment = assessment.FiveMPhysicalEnvironment ?? string.Empty;
        if (string.IsNullOrEmpty(FiveMOperationalEnvironment)) FiveMOperationalEnvironment = assessment.FiveMOperationalEnvironment ?? string.Empty;
    }

    #endregion
}

/// <summary>
/// Step 2 Model - Hazard Identification
/// </summary>
public class Step2Model 
{
    #region Hazard Properties

    public List<string> HazardIds { get; set; } = new();
    public List<string> HazardDescriptions { get; set; } = new();
    public List<string> HazardCategories { get; set; } = new();

    #endregion

    #region Step 2 Methods

    public (bool isValid, string message) Validate()
    {
        var validHazards = HazardDescriptions.Where(h => !string.IsNullOrWhiteSpace(h)).Count();
        
        if (validHazards < 1)
        {
            return (false, "At least 1 hazard is required");
        }
        
        return (true, $"Step 2 validation passed with {validHazards} hazards");
    }

    public void ApplyToAssessment(RiskAssessment assessment)
    {
        assessment.ClearIdentifiedHazards();

        for (int i = 0; i < HazardIds.Count && i < HazardDescriptions.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(HazardDescriptions[i]))
            {
                assessment.AddIdentifiedHazard(HazardIds[i], HazardDescriptions[i]);
            }
        }

        assessment.CompleteStep(2);
    }

    #endregion
}

/// <summary>
/// Step 3 Model - Risk Analysis for Multiple Hazards
/// Supports multiple hazards per report where each hazard has its own risk analysis
/// </summary>
public class Step3Model 
{
    #region Risk Analysis Method Properties

    public string RiskAnalysisMethod { get; set; } = "SMS Risk Matrix";
    public string RiskCriteria { get; set; } = string.Empty;

    #endregion

    #region Multiple Hazard Risk Analysis Properties

    /// <summary>
    /// Collection of risk analyses for each identified hazard
    /// Key = HazardId, Value = HazardRiskAnalysis
    /// </summary>
    public Dictionary<string, HazardRiskAnalysis> HazardAnalyses { get; set; } = new();

    
    #endregion

    #region Step 3 Methods

    /// <summary>
    /// Validates that each identified hazard has completed risk analysis
    /// </summary>
    public (bool isValid, string message) Validate(List<Hazard> availableHazards = null)
    {
        if (availableHazards == null || !availableHazards.Any())
        {
            return (false, "No hazards available for risk analysis. Please complete Step 2 first.");
        }

        var incompleteHazards = new List<string>();
        var analysisCount = 0;

        foreach (var hazard in availableHazards)
        {
            if (HazardAnalyses.TryGetValue(hazard.Code, out var analysis))
            {
                if (string.IsNullOrWhiteSpace(analysis.WorstCredibleOutcome) || analysis.WorstCredibleOutcome.Length < 10)
                {
                    incompleteHazards.Add($"{hazard.Code} (missing worst outcome)");
                }
                else if (string.IsNullOrWhiteSpace(analysis.RootCauseAnalysis) || analysis.RootCauseAnalysis.Length < 10)
                {
                    incompleteHazards.Add($"{hazard.Code} (missing root cause analysis)");
                }
                else
                {
                    analysisCount++;
                }
            }
            else
            {
                incompleteHazards.Add($"{hazard.Code} (no analysis)");
            }
        }

        if (incompleteHazards.Any())
        {
            return (false, $"Incomplete risk analysis for hazards: {string.Join(", ", incompleteHazards)}. All hazards need both worst credible outcome and root cause analysis (minimum 10 characters each).");
        }

        return (true, $"Step 3 validation passed - {analysisCount} hazards have complete risk analysis");
    }

    /// <summary>
    /// Ensures all hazards have risk analysis entries initialized
    /// </summary>
    public void InitializeHazardAnalyses(List<Hazard> availableHazards)
    {
        if (availableHazards == null) return;

        foreach (var hazard in availableHazards)
        {
            if (!HazardAnalyses.ContainsKey(hazard.Code))
            {
                HazardAnalyses[hazard.Code] = new HazardRiskAnalysis
                {
                    HazardId = hazard.Code,
                    HazardDescription = hazard.Description,
                    HazardCategory = hazard.HazardType
                };
            }
        }
    }

    /// <summary>
    /// Gets the risk analysis for a specific hazard
    /// </summary>
    public HazardRiskAnalysis GetHazardAnalysis(string hazardId)
    {
        if (!HazardAnalyses.TryGetValue(hazardId, out var analysis))
        {
            analysis = new HazardRiskAnalysis { HazardId = hazardId };
            HazardAnalyses[hazardId] = analysis;
        }
        return analysis;
    }

    /// <summary>
    /// Updates the risk analysis for a specific hazard
    /// </summary>
    public void UpdateHazardAnalysis(string hazardId, string worstOutcome, string rootCause, string additionalComments = "")
    {
        var analysis = GetHazardAnalysis(hazardId);
        analysis.WorstCredibleOutcome = worstOutcome?.Trim() ?? string.Empty;
        analysis.RootCauseAnalysis = rootCause?.Trim() ?? string.Empty;
        analysis.AdditionalComments = additionalComments?.Trim() ?? string.Empty;
        analysis.AnalysisDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Applies the Step 3 risk analysis data to the RiskAssessment entity
    /// </summary>
    public void ApplyToAssessment(RiskAssessment assessment)
    {
        // Update the overall risk analysis method
        //assessment.UpdateRiskAnalysisMethod(RiskAnalysisMethod, RiskCriteria);

        // Update individual hazard analyses
        foreach (var hazardAnalysis in HazardAnalyses.Values)
        {
           // assessment.UpdateHazardWorstOutcome(hazardAnalysis.HazardId, hazardAnalysis.WorstCredibleOutcome);
           // assessment.UpdateHazardRootCause(hazardAnalysis.HazardId, hazardAnalysis.RootCauseAnalysis);
        }

        assessment.CompleteStep(3);
    }

    /// <summary>
    /// Loads risk analysis data from the assessment entity
    /// </summary>
    public void LoadFromAssessment(RiskAssessment assessment, List<Hazard> availableHazards)
    {
        if (assessment == null) return;

        // Load overall method and criteria
        if (string.IsNullOrEmpty(RiskAnalysisMethod) && !string.IsNullOrEmpty(assessment.RiskAnalysisMethod))
        {
            RiskAnalysisMethod = assessment.RiskAnalysisMethod;
        }

        if (string.IsNullOrEmpty(RiskCriteria) && !string.IsNullOrEmpty(assessment.RiskCriteria))
        {
            RiskCriteria = assessment.RiskCriteria;
        }

        // Initialize hazard analyses
        InitializeHazardAnalyses(availableHazards);

        // Load existing analysis data if available
        // Note: This depends on how the RiskAssessment entity stores hazard-specific analysis
        // You may need to add methods to RiskAssessment to retrieve this data
    }

    /// <summary>
    /// Gets completion percentage for Step 3
    /// </summary>
    public int GetCompletionPercentage(List<Hazard> availableHazards)
    {
        if (availableHazards == null || !availableHazards.Any()) return 0;

        var completedAnalyses = availableHazards.Count(h => 
            HazardAnalyses.TryGetValue(h.Code, out var analysis) &&
            !string.IsNullOrWhiteSpace(analysis.WorstCredibleOutcome) && 
            analysis.WorstCredibleOutcome.Length >= 10 &&
            !string.IsNullOrWhiteSpace(analysis.RootCauseAnalysis) && 
            analysis.RootCauseAnalysis.Length >= 10);

        return (int)((double)completedAnalyses / availableHazards.Count * 100);
    }

    #endregion

    #region Step 3 Enhanced Methods

    /// <summary>
    /// Validates that each identified hazard has completed risk analysis
    /// Enhanced with detailed validation messages
    /// </summary>
    public (bool isValid, string message, List<string> validationErrors) ValidateDetailed(List<Hazard> availableHazards = null)
    {
        if (availableHazards == null || !availableHazards.Any())
        {
            return (false, "No hazards available for risk analysis. Please complete Step 2 first.", new List<string>());
        }

        var incompleteHazards = new List<string>();
        var validationErrors = new List<string>();
        var analysisCount = 0;

        foreach (var hazard in availableHazards)
        {
            if (HazardAnalyses.TryGetValue(hazard.Code, out var analysis))
            {
                var worstOutcome = analysis.WorstCredibleOutcome?.Trim() ?? string.Empty;
                var rootCause = analysis.RootCauseAnalysis?.Trim() ?? string.Empty;

                // Check if fields are empty
                if (string.IsNullOrWhiteSpace(worstOutcome) || string.IsNullOrWhiteSpace(rootCause))
                {
                    incompleteHazards.Add(hazard.Code);
                }
                else
                {
                    // Check minimum length requirements
                    if (worstOutcome.Length < 10)
                    {
                        validationErrors.Add($"{hazard.Code}: Worst Credible Outcome must be at least 10 characters (currently {worstOutcome.Length})");
                    }
                    
                    if (rootCause.Length < 10)
                    {
                        validationErrors.Add($"{hazard.Code}: Root Cause Analysis must be at least 10 characters (currently {rootCause.Length})");
                    }

                    if (worstOutcome.Length >= 10 && rootCause.Length >= 10)
                    {
                        analysisCount++;
                    }
                }
            }
            else
            {
                incompleteHazards.Add($"{hazard.Code} (no analysis)");
            }
        }

        // Build comprehensive error message
        var errorMessages = new List<string>();
        
        if (incompleteHazards.Any())
        {
            errorMessages.Add($"Please complete the risk analysis for the following hazards: {string.Join(", ", incompleteHazards)}");
            errorMessages.Add("Both 'Worst Credible Outcome' and 'Root Cause Analysis' are required for each hazard.");
        }

        if (validationErrors.Any())
        {
            errorMessages.Add("Please provide more detailed analysis:");
            errorMessages.AddRange(validationErrors);
            errorMessages.Add("Both fields require at least 10 characters for meaningful analysis.");
        }

        var isValid = !incompleteHazards.Any() && !validationErrors.Any();
        var message = isValid 
            ? $"Step 3 validation passed - {analysisCount} hazards have complete risk analysis"
            : string.Join("\n", errorMessages);

        return (isValid, message, validationErrors);
    }

    /// <summary>
    /// Auto-saves the current step data (server-side implementation)
    /// </summary>
    public void AutoSave(string assessmentId, Dictionary<string, string> formData)
    {
        try
        {
            // Update hazard analyses from form data
            foreach (var kvp in formData)
            {
                var key = kvp.Key;
                var value = kvp.Value?.Trim() ?? string.Empty;

                // Parse form field names like "Step3.HazardAnalyses[HAZ-001].WorstCredibleOutcome"
                if (key.Contains("HazardAnalyses[") && key.Contains("]"))
                {
                    var startIndex = key.IndexOf("[") + 1;
                    var endIndex = key.IndexOf("]");
                    var hazardId = key.Substring(startIndex, endIndex - startIndex);
                    
                    var analysis = GetHazardAnalysis(hazardId);
                    
                    if (key.Contains("WorstCredibleOutcome"))
                    {
                        analysis.WorstCredibleOutcome = value;
                    }
                    else if (key.Contains("RootCauseAnalysis"))
                    {
                        analysis.RootCauseAnalysis = value;
                    }
                    else if (key.Contains("AdditionalComments"))
                    {
                        analysis.AdditionalComments = value;
                    }
                }
                else if (key.Contains("RiskCriteria"))
                {
                    RiskCriteria = value;
                }
            }
            
            // Mark as auto-saved
            LastAutoSaved = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            // Log error but don't throw - auto-save should be resilient
            System.Diagnostics.Debug.WriteLine($"Auto-save error: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets validation status for each hazard (for UI feedback)
    /// </summary>
    public Dictionary<string, HazardValidationStatus> GetHazardValidationStatuses(List<Hazard> availableHazards)
    {
        var statuses = new Dictionary<string, HazardValidationStatus>();
        
        if (availableHazards == null) return statuses;

        foreach (var hazard in availableHazards)
        {
            var status = new HazardValidationStatus
            {
                HazardId = hazard.Code,
                HazardDescription = hazard.Description
            };

            if (HazardAnalyses.TryGetValue(hazard.Code, out var analysis))
            {
                var worstOutcome = analysis.WorstCredibleOutcome?.Trim() ?? string.Empty;
                var rootCause = analysis.RootCauseAnalysis?.Trim() ?? string.Empty;

                status.HasWorstOutcome = !string.IsNullOrWhiteSpace(worstOutcome);
                status.HasRootCause = !string.IsNullOrWhiteSpace(rootCause);
                status.WorstOutcomeLength = worstOutcome.Length;
                status.RootCauseLength = rootCause.Length;
                status.IsWorstOutcomeValid = worstOutcome.Length >= 10;
                status.IsRootCauseValid = rootCause.Length >= 10;
                status.IsComplete = status.IsWorstOutcomeValid && status.IsRootCauseValid;
            }

            statuses[hazard.Code] = status;
        }

        return statuses;
    }

    /// <summary>
    /// When this step was last auto-saved
    /// </summary>
    public DateTime? LastAutoSaved { get; set; }

    /// <summary>
    /// Checks if there are unsaved changes since the last auto-save
    /// </summary>
    public bool HasUnsavedChanges()
    {
        // Simple check - could be enhanced with change tracking
        return LastAutoSaved == null || 
               HazardAnalyses.Values.Any(ha => ha.AnalysisDate > LastAutoSaved);
    }

    #endregion
}

/// <summary>
/// Step 4 Model - Risk Assessment & Scoring
/// Supports multiple hazards with individual risk scoring panels
/// </summary>
public class Step4Model 
{
    #region Risk Assessment Properties

    public string TolerabilityFramework { get; set; } = "PDX-SMS Default";
    public string RiskAcceptanceCriteria { get; set; } = string.Empty;

    #endregion

    #region Step 4 Methods

    public (bool isValid, string message) Validate()
    {
        if (string.IsNullOrWhiteSpace(TolerabilityFramework))
        {
            return (false, "Tolerability framework is required");
        }
        
        return (true, "Step 4 validation passed");
    }

    public void ApplyToAssessment(RiskAssessment assessment)
    {
        //assessment.UpdateTolerabilityFramework(TolerabilityFramework, RiskAcceptanceCriteria);
        assessment.CompleteStep(4);
    }

    #endregion
}

/// <summary>
/// Step 5 Model - Risk Mitigation
/// Supports mitigation strategies for multiple hazards
/// </summary>
public class Step5Model 
{
    #region Risk Mitigation Properties

    public string ImplementationStrategy { get; set; } = string.Empty;
    public DateTime? OverallTargetDate { get; set; }
    public string ImplementationNotes { get; set; } = string.Empty;
    public Dictionary<string, List<string>> SavedMitigationStrategies { get; set; } = new();

    #endregion

    #region Step 5 Methods

    public (bool isValid, string message) Validate()
    {
        bool hasImplementation = !string.IsNullOrWhiteSpace(ImplementationStrategy);
        
        if (!hasImplementation)
        {
            return (false, "Implementation strategy is required");
        }
        
        return (true, "Step 5 validation passed");
    }

    public void ApplyToAssessment(RiskAssessment assessment)
    {
        //assessment.UpdateImplementationStrategy(ImplementationStrategy, OverallTargetDate, ImplementationNotes);
        assessment.CompleteStep(5);
    }

    #endregion
}
