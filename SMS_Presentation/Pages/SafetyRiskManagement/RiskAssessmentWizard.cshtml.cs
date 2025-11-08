using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SMS_Application.Interfaces;
using SMS_Application.Services;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Shared.Common;

namespace SMS.Presentation.Pages.SafetyRiskManagement;

/// <summary>
/// MISSION CRITICAL: Risk Assessment Wizard - PURE SMS Backend Integration
/// NO DTOs, NO file operations, NO helper classes - ONLY Domain Entities and SMS Backend
/// Uses SMSRiskAssessmentWorkflowService for ALL business logic
/// </summary>
public class RiskAssessmentWizardModel : PageModel
{
    private readonly ISMSRiskAssessmentWorkflowService _workflowService;
    private readonly IMediator _mediator;
    private readonly ILogger<RiskAssessmentWizardModel> _logger;

    public RiskAssessmentWizardModel(
        ISMSRiskAssessmentWorkflowService workflowService,
        IMediator mediator,
        ILogger<RiskAssessmentWizardModel> logger)
    {
        _workflowService = workflowService;
        _mediator = mediator;
        _logger = logger;
    }

    #region Properties - Direct Domain Entity Data

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public int StepNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string? HazardId { get; set; }

    // The ACTUAL Risk Assessment Domain Entity - NOT a DTO!
    public RiskAssessment? Assessment { get; set; }

    // Form binding for step data
    [BindProperty]
    public string SystemDescription { get; set; } = string.Empty;

    [BindProperty]
    public string SystemBoundaries { get; set; } = string.Empty;

    [BindProperty]
    public string SystemPurpose { get; set; } = string.Empty;

    [BindProperty]
    public string PersonnelFactors { get; set; } = string.Empty;

    [BindProperty]
    public string EquipmentFactors { get; set; } = string.Empty;

    [BindProperty]
    public string ProcedureFactors { get; set; } = string.Empty;

    [BindProperty]
    public string ResourceFactors { get; set; } = string.Empty;

    [BindProperty]
    public string EnvironmentFactors { get; set; } = string.Empty;

    [BindProperty]
    public List<string> StakeholderIds { get; set; } = new();

    // Step 2 - Hazard Management
    [BindProperty]
    public List<string> HazardIds { get; set; } = new();

    [BindProperty]
    public List<string> HazardDescriptions { get; set; } = new();

    [BindProperty]
    public List<string> HazardCategories { get; set; } = new();

    [BindProperty]
    public List<string> HazardFiveMComponents { get; set; } = new();

    // Step 3 - Risk Analysis
    [BindProperty]
    public string RiskAnalysisMethod { get; set; } = "SMS Risk Matrix";

    [BindProperty]
    public string RiskCriteria { get; set; } = string.Empty;

    [BindProperty]
    public Dictionary<string, string> HazardWorstOutcomes { get; set; } = new();

    [BindProperty]
    public Dictionary<string, string> HazardRootCauses { get; set; } = new();

    // Display Properties
    public List<SMSApplicationUser> AvailableAssessors { get; set; } = new();
    public List<Hazard> AvailableHazards { get; set; } = new();

    #endregion

    #region Page Handlers

    public async Task<IActionResult> OnGetAsync()
    {
        _logger.LogInformation("Loading Risk Assessment Wizard: {AssessmentId} - Step {StepNumber}", Id, StepNumber);

        if (string.IsNullOrWhiteSpace(Id))
        {
            _logger.LogWarning("No Assessment ID provided, redirecting to Risk Assessment list");
            return RedirectToPage("/SafetyRiskManagement/RiskAssessment");
        }

        try
        {
            // Load the actual Risk Assessment Domain Entity using SMS Backend
            var assessmentResult = await _workflowService.GetRiskAssessmentAsync(Id);
            if (assessmentResult.IsFailure)
            {
                _logger.LogError("Risk Assessment not found: {AssessmentId}", Id);
                TempData["ErrorMessage"] = $"Risk Assessment {Id} not found.";
                return RedirectToPage("/SafetyRiskManagement/RiskAssessment");
            }

            Assessment = assessmentResult.Value;
            _logger.LogInformation("Successfully loaded Risk Assessment: {Name} (Status: {Status})", 
                Assessment.Name, Assessment.Status);

            // Populate form fields from Domain Entity
            PopulateFormFieldsFromAssessment();

            // Load reference data for current step
            await LoadStepReferenceDataAsync();

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Risk Assessment Wizard: {AssessmentId}", Id);
            TempData["ErrorMessage"] = $"Error loading assessment: {ex.Message}";
            return RedirectToPage("/SafetyRiskManagement/RiskAssessment");
        }
    }

    #endregion

    #region Step Handlers - Direct SMS Backend Integration

    public async Task<IActionResult> OnPostSaveStep1Async()
    {
        try
        {
            _logger.LogInformation("Saving Step 1 for Assessment {AssessmentId}", Id);

            var stepData = new Step1SystemDescriptionData
            {
                SystemDescription = SystemDescription,
                SystemBoundaries = SystemBoundaries,
                SystemPurpose = SystemPurpose,
                PersonnelFactors = PersonnelFactors,
                EquipmentFactors = EquipmentFactors,
                ProcedureFactors = ProcedureFactors,
                ResourceFactors = ResourceFactors,
                EnvironmentFactors = EnvironmentFactors,
                StakeholderIds = StakeholderIds
            };

            // Use SMS Backend Workflow Service - NO file operations!
            var result = await _workflowService.UpdateStep1SystemDescriptionAsync(Id, stepData);
            
            if (result.IsFailure)
            {
                return new JsonResult(new { success = false, message = result.Error.Message });
            }

            Assessment = result.Value; // Update the Domain Entity
            
            _logger.LogInformation("Step 1 saved successfully for Assessment {AssessmentId}", Id);
            return new JsonResult(new { success = true, message = "Step 1 saved successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving Step 1 for Assessment {AssessmentId}", Id);
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostSaveStep2Async()
    {
        try
        {
            _logger.LogInformation("Saving Step 2 for Assessment {AssessmentId}", Id);

            // Create Hazard Domain Entities from form data using proper factory methods
            var hazards = new List<Hazard>();
            
            // Get additional form data for hazards (category, 5M component)
            var categories = Request.Form["HazardCategories"].ToArray();
            var fiveMComponents = Request.Form["HazardFiveMComponents"].ToArray();

            for (int i = 0; i < HazardIds.Count; i++)
            {
                if (i < HazardDescriptions.Count && !string.IsNullOrWhiteSpace(HazardDescriptions[i]))
                {
                    // Get category and 5M component if available
                    var category = i < categories.Length ? categories[i] : "Other";
                    var fiveMComponent = i < fiveMComponents.Length ? fiveMComponents[i] : null;

                    // Use the enhanced factory method
                    var hazardResult = Hazard.CreateFromStep2(
                        HazardIds[i], 
                        HazardDescriptions[i], 
                        category, 
                        fiveMComponent);
                    
                    if (hazardResult.IsFailure)
                    {
                        _logger.LogWarning("Failed to create hazard {HazardId}: {Error}", 
                            HazardIds[i], hazardResult.Error.Message);
                        return new JsonResult(new { success = false, message = $"Invalid hazard data: {hazardResult.Error.Message}" });
                    }

                    hazards.Add(hazardResult.Value);
                }
            }

            if (hazards.Count == 0)
            {
                return new JsonResult(new { success = false, message = "At least one hazard must be identified." });
            }

            // Use SMS Backend Workflow Service - Creates/Updates hazards in SMS Backend
            var result = await _workflowService.UpdateStep2HazardIdentificationAsync(Id, hazards);

            if (result.IsFailure)
            {
                return new JsonResult(new { success = false, message = result.Error.Message });
            }

            Assessment = result.Value; // Update the Domain Entity

            _logger.LogInformation("Step 2 saved successfully with {HazardCount} hazards", hazards.Count);
            return new JsonResult(new { success = true, message = $"Step 2 saved with {hazards.Count} hazards" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving Step 2: {Error}", ex.Message);
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostSaveStep3Async()
    {
        try
        {
            _logger.LogInformation("Saving Step 3 for Assessment {AssessmentId}", Id);

            var stepData = new Step3RiskAnalysisData
            {
                RiskAnalysisMethod = RiskAnalysisMethod,
                RiskCriteria = RiskCriteria,
                HazardWorstOutcomes = HazardWorstOutcomes,
                HazardRootCauses = HazardRootCauses
            };

            // Use SMS Backend Workflow Service
            var result = await _workflowService.UpdateStep3RiskAnalysisAsync(Id, stepData);

            if (result.IsFailure)
            {
                return new JsonResult(new { success = false, message = result.Error.Message });
            }

            Assessment = result.Value; // Update the Domain Entity

            _logger.LogInformation("Step 3 saved successfully for assessment {AssessmentId}", Id);
            return new JsonResult(new { success = true, message = "Step 3 saved successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving Step 3: {Error}", ex.Message);
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostCompleteAssessmentAsync()
    {
        try
        {
            _logger.LogInformation("Completing Assessment {AssessmentId}", Id);

            // Use SMS Backend Workflow Service
            var result = await _workflowService.CompleteRiskAssessmentAsync(Id);

            if (result.IsFailure)
            {
                TempData["ErrorMessage"] = result.Error.Message;
                return Page();
            }

            Assessment = result.Value; // Update the Domain Entity

            _logger.LogInformation("Assessment {AssessmentId} completed successfully", Id);
            TempData["SuccessMessage"] = "Risk Assessment completed successfully! All steps have been saved.";
            
            return RedirectToPage("/SafetyRiskManagement/RiskAssessment");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing Assessment {AssessmentId}", Id);
            TempData["ErrorMessage"] = $"Error completing assessment: {ex.Message}";
            return Page();
        }
    }

    #endregion

    #region Navigation Handlers

    public async Task<IActionResult> OnPostNextStepAsync()
    {
        try
        {
            var nextStep = Math.Min(StepNumber + 1, 5);
            return RedirectToPage("/SafetyRiskManagement/RiskAssessmentWizard", new { id = Id, stepNumber = nextStep });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to next step");
            TempData["ErrorMessage"] = "Error navigating to next step.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostPreviousStepAsync()
    {
        try
        {
            var previousStep = Math.Max(StepNumber - 1, 1);
            return RedirectToPage("/SafetyRiskManagement/RiskAssessmentWizard", new { id = Id, stepNumber = previousStep });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to previous step");
            TempData["ErrorMessage"] = "Error navigating to previous step.";
            return Page();
        }
    }

    #endregion

    #region Helper Methods - Pure Domain Logic

    private void PopulateFormFieldsFromAssessment()
    {
        if (Assessment == null) return;

        // Step 1 Data
        SystemDescription = Assessment.SystemDescription;
        SystemBoundaries = Assessment.SystemBoundaries;
        SystemPurpose = Assessment.SystemPurpose;
        PersonnelFactors = Assessment.PersonnelFactors;
        EquipmentFactors = Assessment.EquipmentFactors;
        ProcedureFactors = Assessment.ProcedureFactors;
        ResourceFactors = Assessment.ResourceFactors;
        EnvironmentFactors = Assessment.EnvironmentFactors;
        
        // Stakeholders from Domain Entity
        StakeholderIds = Assessment.StakeholderIds.ToList();

        // Step 2 Data - Identified Hazards
        HazardIds = Assessment.IdentifiedHazardIds.ToList();

        // Step 3 Data
        RiskAnalysisMethod = Assessment.RiskAnalysisMethod;
        RiskCriteria = Assessment.RiskCriteria;

        _logger.LogDebug("Populated form fields from Assessment Domain Entity");
    }

    private async Task LoadStepReferenceDataAsync()
    {
        try
        {
            if (StepNumber == 1 || StepNumber == 4)
            {
                // Load available assessors for Step 1 or Step 4 panel selection
                var assessorsResult = await _workflowService.GetAvailableAssessorsAsync();
                if (assessorsResult.IsSuccess)
                {
                    AvailableAssessors = assessorsResult.Value;
                }
            }

            if (StepNumber == 2)
            {
                // Load available hazards for Step 2 hazard identification
                var hazardsResult = await _workflowService.GetAvailableHazardsForAssessmentAsync();
                if (hazardsResult.IsSuccess)
                {
                    AvailableHazards = hazardsResult.Value;
                }
            }

            _logger.LogDebug("Loaded reference data for Step {StepNumber}", StepNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading reference data for Step {StepNumber}", StepNumber);
            // Non-critical error - continue without reference data
        }
    }

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
        // Use Domain Entity business logic
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

    public RiskAssessmentStatus GetAssessmentStatus()
    {
        return Assessment?.Status ?? RiskAssessmentStatus.Created;
    }

    public int GetIdentifiedHazardsCount()
    {
        return Assessment?.IdentifiedHazardIds.Count ?? 0;
    }

    #endregion
}
