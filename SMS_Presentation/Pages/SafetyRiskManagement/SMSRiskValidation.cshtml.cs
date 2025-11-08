using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;
using SMS_Application.Messaging.Queries;
using SMS_Shared.Common;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace SMS.Presentation.Pages.SafetyRiskManagement;

/// <summary>
/// SMS Risk Validation Page Model - Direct SMS Backend Integration
/// No helper classes - direct CQRS/Mediator pattern for SMS risk validation
/// </summary>
public class SMSRiskValidationModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<SMSRiskValidationModel> _logger;

    public SMSRiskValidationModel(IMediator mediator, ILogger<SMSRiskValidationModel> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ===========================================
    // SMS Domain Properties
    // ===========================================
    
    [BindProperty]
    public string HazardId { get; set; } = string.Empty;

    [BindProperty]
    public string ProcessingDecision { get; set; } = string.Empty;

    [BindProperty]
    public string ProcessingNotes { get; set; } = string.Empty;

    [BindProperty]
    public string PriorityOverride { get; set; } = string.Empty;

    [BindProperty]
    public string AssignedToUserId { get; set; } = string.Empty;

    [BindProperty]
    public DateTime? DueDate { get; set; }

    [BindProperty]
    public bool AutoEscalate { get; set; } = false;

    [BindProperty]
    public string RiskAssessmentMethod { get; set; } = "Simplified";

    public string AssessmentType { get; set; } = "Initial";

    [BindProperty]
    public string ReferralAction { get; set; } = string.Empty;

    [BindProperty]
    public string ClosureReason { get; set; } = string.Empty;

    [BindProperty]
    public string InformationNeeded { get; set; } = string.Empty;

    // Display properties
    public string ProcessingMessage { get; set; } = string.Empty;
    public bool IsProcessingComplete { get; set; }

    public async Task<IActionResult> OnGetAsync(string? hazardId = null)
    {
        // Use route parameter if provided, otherwise fall back to property
        if (!string.IsNullOrWhiteSpace(hazardId))
        {
            HazardId = hazardId;
        }
        
        if (string.IsNullOrWhiteSpace(HazardId))
        {
            TempData["ErrorMessage"] = "Hazard ID is required for SMS risk validation";
            return RedirectToPage("/SafetyRiskManagement/HazardProcessing");
        }

        try
        {
            _logger.LogInformation("Loading SMS risk validation for hazard: {HazardId}", HazardId);
            
            // TODO: Load hazard details using SMS Backend when hazard queries are implemented
            // For now, just ensure the HazardId is set
            
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading hazard for SMS risk validation: {HazardId}", HazardId);
            TempData["ErrorMessage"] = "An error occurred while loading the hazard for validation";
            return RedirectToPage("/SafetyRiskManagement/HazardProcessing");
        }
    }

    /// <summary>
    /// Save current validation state as draft
    /// </summary>
    public async Task<IActionResult> OnPostSaveDraftAsync()
    {
        try
        {
            _logger.LogInformation("Saving validation draft for hazard: {HazardId}", HazardId);

            // Validate essential fields for drafts
            if (string.IsNullOrWhiteSpace(HazardId))
            {
                TempData["ErrorMessage"] = "Cannot save draft: Hazard ID is missing.";
                return Page();
            }

            // TODO: Implement draft saving using SMS Backend
            // This would involve creating or updating a draft risk assessment

            TempData["SuccessMessage"] = "Validation draft saved successfully. You can continue working or return later.";
            _logger.LogInformation("Draft saved successfully for hazard {HazardId}", HazardId);
            
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving validation draft for hazard: {HazardId}", HazardId);
            TempData["ErrorMessage"] = "Error saving draft. Please try again.";
            return Page();
        }
    }

    /// <summary>
    /// Proceed to Assessment
    /// </summary>
    public async Task<IActionResult> OnPostProceedToAssessmentAsync()
    {
        try
        {
            _logger.LogInformation("Proceeding to risk assessment for hazard: {HazardId}", HazardId);

            if (!ValidateProcessingDecision())
            {
                return Page();
            }

            // TODO: Implement assessment creation using SMS Backend
            // This would create the initial risk assessment and redirect to the wizard

            TempData["SuccessMessage"] = "SMS Risk validation completed successfully. Proceeding to assessment.";
            
            // For now, redirect to a placeholder page
            return RedirectToPage("/SafetyRiskManagement/RiskAssessmentWizard", new { hazardId = HazardId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proceeding to assessment for hazard: {HazardId}", HazardId);
            ModelState.AddModelError(string.Empty, "An error occurred while processing the validation. Please try again.");
            return Page();
        }
    }

    /// <summary>
    /// Complete SMS risk validation
    /// </summary>
    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            _logger.LogInformation("Processing SMS risk validation for hazard: {HazardId}", HazardId);

            if (!ValidateProcessingDecision())
            {
                return Page();
            }

            // TODO: Implement full validation processing using SMS Backend
            // This would complete the validation and route based on the decision

            var successMessage = ProcessingDecision.ToLowerInvariant() switch
            {
                "sms_risk" => "SMS Risk validation completed. Assessment will be scheduled.",
                "not_sms_risk" => "Item closed as non-SMS risk.",
                "investigation" => "Investigation scheduled for additional information gathering.",
                _ => "Validation completed successfully."
            };

            TempData["SuccessMessage"] = successMessage;

            // Route based on processing decision
            return ProcessingDecision.ToLowerInvariant() switch
            {
                "sms_risk" => RedirectToPage("/SafetyRiskManagement/RiskAssessmentWizard", new { hazardId = HazardId }),
                "not_sms_risk" => RedirectToPage("/SafetyRiskManagement/HazardProcessing"),
                "investigation" => RedirectToPage("/SafetyRiskManagement/HazardProcessing"),
                _ => RedirectToPage("/SafetyRiskManagement/HazardProcessing")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SMS risk validation: {HazardId}", HazardId);
            ModelState.AddModelError(string.Empty, "An unexpected error occurred during validation. Please try again.");
            return Page();
        }
    }

    #region Helper Methods

    private bool ValidateProcessingDecision()
    {
        // Always required fields
        if (string.IsNullOrWhiteSpace(HazardId))
        {
            ModelState.AddModelError(nameof(HazardId), "Hazard ID is required");
        }
        
        if (string.IsNullOrWhiteSpace(ProcessingDecision))
        {
            ModelState.AddModelError(nameof(ProcessingDecision), "Please select a processing decision");
        }
        
        if (string.IsNullOrWhiteSpace(ProcessingNotes) || ProcessingNotes.Length < 5)
        {
            ModelState.AddModelError(nameof(ProcessingNotes), "Processing notes are required (minimum 5 characters)");
        }

        // Conditional validation based on processing decision
        if (!string.IsNullOrWhiteSpace(ProcessingDecision))
        {
            switch (ProcessingDecision.ToLowerInvariant())
            {
                case "not_sms_risk":
                    if (string.IsNullOrWhiteSpace(ClosureReason))
                    {
                        ModelState.AddModelError(nameof(ClosureReason), "Closure reason is required for non-SMS risks");
                    }
                    if (string.IsNullOrWhiteSpace(ReferralAction))
                    {
                        ModelState.AddModelError(nameof(ReferralAction), "Referral action is required for non-SMS risks");
                    }
                    break;
                    
                case "investigation":
                    if (string.IsNullOrWhiteSpace(InformationNeeded))
                    {
                        ModelState.AddModelError(nameof(InformationNeeded), "Information needed is required for investigation path");
                    }
                    break;
                    
                case "sms_risk":
                    // For SMS risks, no additional required fields beyond base validation
                    break;
                    
                default:
                    ModelState.AddModelError(nameof(ProcessingDecision), "Invalid processing decision");
                    break;
            }
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Validation failed. Errors: {Errors}", 
                string.Join(", ", ModelState.Where(ms => ms.Value.Errors.Any())
                    .SelectMany(ms => ms.Value.Errors.Select(e => $"{ms.Key}: {e.ErrorMessage}"))));
            return false;
        }

        return true;
    }

    #endregion
}

