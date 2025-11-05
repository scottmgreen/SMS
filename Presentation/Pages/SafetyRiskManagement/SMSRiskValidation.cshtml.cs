using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PDXSMS.UseCases.Commands.HazardProcessing;
using PDXSMS.UseCases.Queries.HazardQueries;
using PDXSMS.Interfaces;
using PDXSMS_Domain.Common;
using PDXSMS_Presentation.ViewModels;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using PDXSMS.Services;

namespace PDXSMS_Presentation.Pages.SafetyRiskManagement;

/// <summary>
/// SIMPLIFIED: Direct file-based SMS Risk Validation 
/// NO sessions, NO caching - pure file-based CRUD operations
/// Creates/Updates RiskAssessment directly in risk-assessments.json
/// </summary>
public class SMSRiskValidationModel : PageModel
{
    private readonly IRequestHandler<ValidateSMSRiskWithDatasetCommand, Result<ValidateSMSRiskWithDatasetResponse>> _validateSMSRiskHandler;
    private readonly IRequestHandler<GetHazardSummaryQuery, GetHazardSummaryQueryResponse> _getHazardSummaryHandler;
    private readonly ILogger<SMSRiskValidationModel> _logger;

    public SMSRiskValidationModel(
        IRequestHandler<ValidateSMSRiskWithDatasetCommand, Result<ValidateSMSRiskWithDatasetResponse>> validateSMSRiskHandler,
        IRequestHandler<GetHazardSummaryQuery, GetHazardSummaryQueryResponse> getHazardSummaryHandler,
        ILogger<SMSRiskValidationModel> logger)
    {
        _validateSMSRiskHandler = validateSMSRiskHandler ?? throw new ArgumentNullException(nameof(validateSMSRiskHandler));
        _getHazardSummaryHandler = getHazardSummaryHandler ?? throw new ArgumentNullException(nameof(getHazardSummaryHandler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ===========================================
    // CLEAN PROPERTIES (Domain-Driven)
    // ===========================================
    
    [BindProperty]
    public string HazardId { get; set; } = string.Empty;

    [BindProperty]
    public string ProcessingDecision { get; set; } = string.Empty;

    [BindProperty]
    public string ProcessingNotes { get; set; } = string.Empty;

    // Conditional fields (NOT required for drafts - validation depends on ProcessingDecision)
    [BindProperty]
    public string PriorityOverride { get; set; } = string.Empty;

    [BindProperty]
    public string AssignedToUserId { get; set; } = string.Empty;

    [BindProperty]
    public DateTime? DueDate { get; set; }

    [BindProperty]
    public bool AutoEscalate { get; set; } = false;

    // Conditional fields (simplified) - NOT required for drafts
    [BindProperty]
    public string RiskAssessmentMethod { get; set; } = "Simplified";

    // FIXED: Assessment type is always "Initial" during validation
    // Residual assessments are created later in the process after mitigation implementation
    public string AssessmentType { get; set; } = "Initial";

    [BindProperty]
    public string ReferralAction { get; set; } = string.Empty;

    [BindProperty]
    public string ClosureReason { get; set; } = string.Empty;

    [BindProperty]
    public string InformationNeeded { get; set; } = string.Empty;

    // ===========================================
    // DOMAIN-DRIVEN VIEWMODEL
    // ===========================================

    /// <summary>
    /// Clean ViewModel that maps to domain entities
    /// CRITICAL: Airport Shared Dataset - All SMS compliance data
    /// </summary>
    [BindProperty]
    public AirportSharedDatasetViewModel AirportSharedDataset { get; set; } = new();

    // Display properties
    public GetHazardSummaryQueryResponse? HazardDetails { get; set; }
    public string ProcessingMessage { get; set; } = string.Empty;
    public bool IsProcessingComplete { get; set; }

    // Legacy property for view compatibility
    public GetHazardSummaryQueryResponse? Hazard => HazardDetails;

    // Domain validation results
    public MinimumDatasetValidationResult? DatasetValidationResult { get; private set; }

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

            // Load hazard details
            var query = new GetHazardSummaryQuery(HazardId);
            HazardDetails = await _getHazardSummaryHandler.HandleAsync(query, CancellationToken.None);

            if (HazardDetails == null || !HazardDetails.IsFound)
            {
                TempData["ErrorMessage"] = $"Hazard {HazardId} not found or not available for processing";
                return RedirectToPage("/SafetyRiskManagement/HazardProcessing");
            }

            // Initialize airport shared dataset with hazard information
            if (string.IsNullOrEmpty(AirportSharedDataset.PrivateNarrative))
            {
                AirportSharedDataset.InitializeFromHazard(HazardDetails);
            }

            _logger.LogDebug("Successfully loaded hazard details for validation: {HazardType} at {Location}", 
                HazardDetails.HazardType, HazardDetails.Location);

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
    /// SIMPLIFIED: Save current validation state as draft - DIRECT FILE OPERATION
    /// No more sessions - saves directly to risk-assessments.json as draft
    /// </summary>
    public async Task<IActionResult> OnPostSaveDraftAsync()
    {
        try
        {
            _logger.LogInformation("💾 Saving validation draft directly to file for hazard: {HazardId}", HazardId);

            // Validate essential fields for drafts
            if (string.IsNullOrWhiteSpace(HazardId))
            {
                TempData["ErrorMessage"] = "Cannot save draft: Hazard ID is missing.";
                HazardDetails = await LoadHazardDetails();
                return Page();
            }

            // Create draft command (same structure as validation command)
            var draftCommand = new ValidateSMSRiskWithDatasetCommand
            {
                HazardId = HazardId,
                ValidatedById = HttpContext.Session.GetString("UserId") ?? HttpContext.Session.GetString("UserName") ?? User?.Identity?.Name ?? "current-user-id", // 🆕 Get actual user ID from session
                ProcessingDecision = ProcessingDecision ?? "draft",
                ProcessingNotes = ProcessingNotes ?? "Draft - In Progress",
                RiskAssessmentMethod = RiskAssessmentMethod,
                AssignedToUserId = AssignedToUserId, // 🆕 Store assigned assessor even in drafts
                DueDate = DueDate,
                ReferralAction = ReferralAction,
                ClosureReason = ClosureReason,
                InformationNeeded = InformationNeeded,
                AutoEscalate = AutoEscalate,
                AirportSharedDataset = AirportSharedDataset.ToApplicationDto()
            };

            // Save draft directly to file (will create/update assessment as draft)
            var result = await _validateSMSRiskHandler.HandleAsync(draftCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "📝 Validation draft saved successfully. You can continue working or return later.";
                _logger.LogInformation("✅ Draft saved successfully for hazard {HazardId}", HazardId);
            }
            else
            {
                TempData["ErrorMessage"] = "❌ Error saving draft. Please try again.";
                _logger.LogError("❌ Draft save failed for hazard {HazardId}: {Error}", HazardId, result.Error.Message);
            }
            
            // Reload hazard details for display
            HazardDetails = await LoadHazardDetails();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error saving validation draft for hazard: {HazardId}", HazardId);
            TempData["ErrorMessage"] = "❌ Error saving draft. Please try again.";
            
            HazardDetails = await LoadHazardDetails();
            return Page();
        }
    }

    /// <summary>
    /// SIMPLIFIED: Proceed to Assessment using direct file operations
    /// </summary>
    public async Task<IActionResult> OnPostProceedToAssessmentAsync()
    {
        try
        {
            _logger.LogInformation("🎯 PROCEEDING TO RISK ASSESSMENT via direct file operation");
            _logger.LogInformation("HazardId: {HazardId}, Decision: {Decision}, Method: {RiskAssessmentMethod}", 
                HazardId, ProcessingDecision, RiskAssessmentMethod);

            // 🔍 DEBUG: Log all posted values to identify the issue
            _logger.LogInformation("DEBUG - ProceedToAssessment Posted Values:");
            _logger.LogInformation("  HazardId: '{HazardId}'", HazardId);
            _logger.LogInformation("  ProcessingDecision: '{ProcessingDecision}'", ProcessingDecision);
            _logger.LogInformation("  ProcessingNotes: '{ProcessingNotes}' (Length: {Length})", ProcessingNotes, ProcessingNotes?.Length ?? 0);
            _logger.LogInformation("  ReferralAction: '{ReferralAction}'", ReferralAction);
            _logger.LogInformation("  ClosureReason: '{ClosureReason}'", ClosureReason);
            _logger.LogInformation("  InformationNeeded: '{InformationNeeded}'", InformationNeeded);
            _logger.LogInformation("  PriorityOverride: '{PriorityOverride}'", PriorityOverride);

            // 🔍 DEBUG: Check if ModelState already has errors before we start validation
            _logger.LogInformation("DEBUG - ProceedToAssessment ModelState before validation has {Count} errors", ModelState.ErrorCount);
            foreach (var modelError in ModelState.Where(ms => ms.Value.Errors.Any()))
            {
                foreach (var error in modelError.Value.Errors)
                {
                    _logger.LogWarning("ProceedToAssessment Pre-existing ModelState Error - {Key}: {Error}", modelError.Key, error.ErrorMessage);
                }
            }

            // 🚨 CLEAR ALL EXISTING MODEL STATE ERRORS FIRST
            ModelState.Clear();
            _logger.LogInformation("DEBUG - ProceedToAssessment Cleared all ModelState errors");

            // 🚨 EXPLICIT VALIDATION: Validate all required fields explicitly since we removed [Required] attributes
            
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

            // 🚨 CONDITIONAL VALIDATION: Only validate fields based on processing decision
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
                        // For SMS risks, no additional required fields
                        _logger.LogInformation("DEBUG - ProceedToAssessment SMS Risk path selected, no additional validation required");
                        break;
                        
                    default:
                        ModelState.AddModelError(nameof(ProcessingDecision), "Invalid processing decision");
                        break;
                }
            }

            // 🔍 DEBUG: Log validation results after our explicit validation
            _logger.LogInformation("DEBUG - ProceedToAssessment ModelState after explicit validation has {Count} errors", ModelState.ErrorCount);
            foreach (var modelError in ModelState.Where(ms => ms.Value.Errors.Any()))
            {
                foreach (var error in modelError.Value.Errors)
                {
                    _logger.LogWarning("ProceedToAssessment Our Validation Error - {Key}: {Error}", modelError.Key, error.ErrorMessage);
                }
            }

            // Remove any validation errors for optional fields
            RemoveModelStateError(nameof(PriorityOverride));

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ProceedToAssessment validation failed. Errors: {Errors}", 
                    string.Join(", ", ModelState.Where(ms => ms.Value.Errors.Any())
                        .SelectMany(ms => ms.Value.Errors.Select(e => $"{ms.Key}: {e.ErrorMessage}"))));
                HazardDetails = await LoadHazardDetails();
                return Page();
            }

            // Create and execute the command - will create/update files directly
            var command = new ValidateSMSRiskWithDatasetCommand
            {
                HazardId = HazardId,
                ValidatedById = HttpContext.Session.GetString("UserId") ?? HttpContext.Session.GetString("UserName") ?? User?.Identity?.Name ?? "current-user-id", // 🆕 Get actual user ID from session
                ProcessingDecision = ProcessingDecision,
                ProcessingNotes = ProcessingNotes,
                RiskAssessmentMethod = RiskAssessmentMethod,
                AssignedToUserId = AssignedToUserId, // 🆕 This will be stored for Step 1 Lead Assessor
                DueDate = DueDate,
                ReferralAction = ReferralAction,
                ClosureReason = ClosureReason,
                InformationNeeded = InformationNeeded,
                AutoEscalate = AutoEscalate,
                AirportSharedDataset = AirportSharedDataset.ToApplicationDto()
            };

            // Execute the direct file-based command
            var result = await _validateSMSRiskHandler.HandleAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                var response = result.Value;
                DatasetValidationResult = response.DatasetValidation;

                _logger.LogInformation("✅ Direct file-based SMS RISK VALIDATION SUCCESSFUL");
                _logger.LogInformation("Next Step: {NextStep}", response.NextStep);

                // Set success message
                var successMessage = $"✅ SMS Risk validation completed successfully. " +
                    $"Decision: {response.ProcessingDecision}. " +
                    $"Assessment Type: Initial. " +
                    $"Dataset completeness: {response.DatasetValidation.Metrics.CompletenessScore}%.";

                TempData["SuccessMessage"] = successMessage;

                // Direct redirect to the assessment
                return Redirect(response.RedirectUrl ?? response.NextStep);
            }
            else
            {
                _logger.LogWarning("❌ Direct file-based SMS risk validation failed: {Error}", result.Error.Message);
                ModelState.AddModelError(string.Empty, result.Error.Message);
                
                HazardDetails = await LoadHazardDetails();
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in direct file-based proceed to assessment for hazard: {HazardId}", HazardId);
            ModelState.AddModelError(string.Empty, "An error occurred while processing the validation. Please try again.");
            
            HazardDetails = await LoadHazardDetails();
            return Page();
        }
    }

    /// <summary>
    /// SIMPLIFIED: Direct file-based SMS risk validation
    /// No sessions, no caching - pure file-based CRUD operations
    /// </summary>
    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            _logger.LogInformation("🎯 DIRECT FILE-BASED SMS RISK VALIDATION");
            _logger.LogInformation("HazardId: {HazardId}, Decision: {Decision}", HazardId, ProcessingDecision);

            // 🔍 DEBUG: Log all posted values to identify the issue
            _logger.LogInformation("DEBUG - Posted Values:");
            _logger.LogInformation("  HazardId: '{HazardId}'", HazardId);
            _logger.LogInformation("  ProcessingDecision: '{ProcessingDecision}'", ProcessingDecision);
            _logger.LogInformation("  ProcessingNotes: '{ProcessingNotes}' (Length: {Length})", ProcessingNotes, ProcessingNotes?.Length ?? 0);
            _logger.LogInformation("  ReferralAction: '{ReferralAction}'", ReferralAction);
            _logger.LogInformation("  ClosureReason: '{ClosureReason}'", ClosureReason);
            _logger.LogInformation("  InformationNeeded: '{InformationNeeded}'", InformationNeeded);
            _logger.LogInformation("  PriorityOverride: '{PriorityOverride}'", PriorityOverride);

            // 🔍 DEBUG: Check if ModelState already has errors before we start validation
            _logger.LogInformation("DEBUG - ModelState before validation has {Count} errors", ModelState.ErrorCount);
            foreach (var modelError in ModelState.Where(ms => ms.Value.Errors.Any()))
            {
                foreach (var error in modelError.Value.Errors)
                {
                    _logger.LogWarning("Pre-existing ModelState Error - {Key}: {Error}", modelError.Key, error.ErrorMessage);
                }
            }

            // 🚨 CLEAR ALL EXISTING MODEL STATE ERRORS FIRST
            ModelState.Clear();
            _logger.LogInformation("DEBUG - Cleared all ModelState errors");

            // 🚨 EXPLICIT VALIDATION: Validate all required fields explicitly since we removed [Required] attributes
            
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

            // 🚨 CONDITIONAL VALIDATION: Only validate fields based on processing decision
            if (!string.IsNullOrWhiteSpace(ProcessingDecision))
            {
                switch (ProcessingDecision.ToLowerInvariant())
                {
                    case "not_sms_risk":
                        // For non-SMS risks, require closure reason and referral action
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
                        // For investigation path, require information needed
                        if (string.IsNullOrWhiteSpace(InformationNeeded))
                        {
                            ModelState.AddModelError(nameof(InformationNeeded), "Information needed is required for investigation path");
                        }
                        break;
                        
                    case "sms_risk":
                        // For SMS risks, no additional required fields beyond the base ones
                        // RiskAssessmentMethod defaults to "Simplified" and is optional
                        _logger.LogInformation("DEBUG - SMS Risk path selected, no additional validation required");
                        break;
                        
                    default:
                        ModelState.AddModelError(nameof(ProcessingDecision), "Invalid processing decision");
                        break;
                }
            }

            // 🔍 DEBUG: Log validation results after our explicit validation
            _logger.LogInformation("DEBUG - ModelState after explicit validation has {Count} errors", ModelState.ErrorCount);
            foreach (var modelError in ModelState.Where(ms => ms.Value.Errors.Any()))
            {
                foreach (var error in modelError.Value.Errors)
                {
                    _logger.LogWarning("Our Validation Error - {Key}: {Error}", modelError.Key, error.ErrorMessage);
                }
            }

            // PriorityOverride is always optional - ensure no validation errors
            RemoveModelStateError(nameof(PriorityOverride));

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model validation failed. Errors: {Errors}", 
                    string.Join(", ", ModelState.Where(ms => ms.Value.Errors.Any())
                        .SelectMany(ms => ms.Value.Errors.Select(e => $"{ms.Key}: {e.ErrorMessage}"))));
                HazardDetails = await LoadHazardDetails();
                return Page();
            }

            // Create and execute the command - will create/update files directly
            var command = new ValidateSMSRiskWithDatasetCommand
            {
                HazardId = HazardId,
                ValidatedById = HttpContext.Session.GetString("UserId") ?? HttpContext.Session.GetString("UserName") ?? User?.Identity?.Name ?? "current-user-id", // 🆕 Get actual user ID from session
                ProcessingDecision = ProcessingDecision,
                ProcessingNotes = ProcessingNotes,
                RiskAssessmentMethod = RiskAssessmentMethod,
                AssignedToUserId = AssignedToUserId,
                DueDate = DueDate,
                ReferralAction = ReferralAction,
                ClosureReason = ClosureReason,
                InformationNeeded = InformationNeeded,
                AutoEscalate = AutoEscalate,
                AirportSharedDataset = AirportSharedDataset.ToApplicationDto()
            };

            // Execute direct file-based validation
            var result = await _validateSMSRiskHandler.HandleAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                var response = result.Value;
                DatasetValidationResult = response.DatasetValidation;
                
                _logger.LogInformation("✅ Direct file-based SMS risk validation completed: {HazardId} -> {Decision}", 
                    HazardId, response.ProcessingDecision);

                // Set success message
                TempData["SuccessMessage"] = GenerateSuccessMessage(response);

                // Direct redirect based on processing decision
                return response.ProcessingDecision.ToLowerInvariant() switch
                {
                    "sms_risk" => Redirect(response.RedirectUrl ?? response.NextStep),
                    "not_sms_risk" => RedirectToPage("/SafetyRiskManagement/HazardProcessing"),
                    "investigation" => RedirectToPage("/SafetyRiskManagement/HazardProcessing", null, "investigation"), // Add fragment to open Investigation tab
                    _ => RedirectToPage("/SafetyRiskManagement/HazardProcessing")
                };
            }
            else
            {
                _logger.LogWarning("❌ Direct file-based SMS risk validation failed: {Error}", result.Error.Message);
                ModelState.AddModelError(string.Empty, result.Error.Message);
                
                HazardDetails = await LoadHazardDetails();
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in direct file-based SMS risk validation: {HazardId}", HazardId);
            ModelState.AddModelError(string.Empty, "An unexpected error occurred during validation. Please try again.");
            
            HazardDetails = await LoadHazardDetails();
            return Page();
        }
    }

    #region Helper Methods

    private async Task<GetHazardSummaryQueryResponse?> LoadHazardDetails()
    {
        try
        {
            var query = new GetHazardSummaryQuery(HazardId);
            var details = await _getHazardSummaryHandler.HandleAsync(query, CancellationToken.None);
            
            if (details?.Description != null && string.IsNullOrEmpty(AirportSharedDataset.PrivateNarrative))
            {
                AirportSharedDataset.InitializeFromHazard(details);
            }
            
            return details;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading hazard details: {HazardId}", HazardId);
            return null;
        }
    }

    private string GenerateSuccessMessage(ValidateSMSRiskWithDatasetResponse response)
    {
        var baseMessage = $"SMS Risk validation completed successfully. " +
            $"Decision: {response.ProcessingDecision}. " +
            $"Hazard status: {response.HazardStatus}.";

        // Add dataset information using domain metrics
        if (response.DatasetValidation.IsValid && response.ProcessingDecision.Equals("sms_risk", StringComparison.OrdinalIgnoreCase))
        {
            baseMessage += $" Initial risk assessment created with {response.DatasetValidation.Metrics.CompletenessScore}% dataset completeness.";
        }

        // Add assignment information
        if (response.CreatedAssignments?.Length > 0)
        {
            baseMessage += $" {response.CreatedAssignments.Length} assignment(s) created for processing.";
        }

        // Add timeline information
        if (!string.IsNullOrWhiteSpace(response.ProcessingTimeline))
        {
            baseMessage += $" {response.ProcessingTimeline}";
        }

        return baseMessage;
    }

    /// <summary>
    /// Helper method to remove specific model state errors for conditional validation
    /// </summary>
    private void RemoveModelStateError(string key)
    {
        if (ModelState.ContainsKey(key))
        {
            ModelState.Remove(key);
        }
    }

    #endregion
}

