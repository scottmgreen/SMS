using System.Security.Cryptography;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;
using SMS_Application.Messaging.Queries;

using SMS_Domain.Common;
using SMS_Domain.Entities;
using SMS_Domain.Interfaces;

using SMS_Shared.Common;

namespace SMS.Presentation.Pages.SafetyRiskManagement;

/// <summary>
/// SMS Report Validation Page - Proper PageModel Implementation
/// Uses PageModel properties for binding, not direct entity binding
/// Follows proper Razor Pages patterns - NO direct entity binding
/// </summary>
public class ReportValidationModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReportValidationModel> _logger;

    public ReportValidationModel(IMediator mediator, ILogger<ReportValidationModel> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region PageModel Properties for Form Binding - NO ENTITY BINDING!
    
    [BindProperty(SupportsGet = true)]
    public string ReportId { get; set; } = string.Empty;
    
    [BindProperty]
    public string ValidationDecision { get; set; } = string.Empty;
    
    [BindProperty]
    public string ValidationComments { get; set; } = string.Empty;
    
    [BindProperty]
    public string ValidationType { get; set; } = "Standard";
    
    [BindProperty]
    public string ValidatedBy { get; set; } = string.Empty;

    // Add HazardId to preserve across POST
    [BindProperty]
    public string HazardId { get; set; } = string.Empty;

    #endregion

    #region Display Properties - Loaded from Domain Entities
    
    // Display data loaded from SMS Backend - NOT bound
    public Report? ReportDetails { get; set; }
    public Hazard? ReportHazard { get; set; }
    public ReportValidation? ReportValidation { get; set; }
    public List<SMSApplicationUser> AvailableAssessors { get; set; } = new();

    // UI State properties
    public bool IsUpdate => ReportValidation != null;
    public string ValidationCode => ReportValidation?.Code ?? string.Empty;
    public string CurrentStatus => ReportValidation?.Status ?? "New";

    // Hidden property to track if this is an update (persisted through POST)
    [BindProperty]
    public bool IsExistingValidation { get; set; }

    [BindProperty]
    public string ExistingValidationId { get; set; } = string.Empty;

    #endregion

    public async Task<IActionResult> OnGetAsync(string? reportId = null)
    {
        if (string.IsNullOrWhiteSpace(reportId))
        {
            TempData["ErrorMessage"] = "Report ID is required for SMS report validation";
            return RedirectToPage("/SafetyRiskManagement/HazardProcessing");
        }

        ReportId = reportId;

        try
        {
            _logger.LogInformation("Loading SMS report validation for report: {ReportId}", reportId);
            var reportCode = new ReportID(reportId);

            // Load report details
            var reportResult = await _mediator.SendAsync(new GetReportByIdQuery(reportCode), CancellationToken.None);
            if (reportResult.IsSuccess)
            {
                ReportDetails = reportResult.Value;
            }

            // Load associated hazard with better comparison
            var hazardResult = await _mediator.SendAsync(new GetAllHazardsQuery(), CancellationToken.None);
            if (hazardResult.IsSuccess)
            {
                //List<Hazard> test = hazardResult.Value;


                ReportHazard = hazardResult.Value.ToList().FirstOrDefault(h => h.ReportCode.Trim() == reportCode.Value.Trim());
                
                // Preserve HazardId for POST operations
                if (ReportHazard != null)
                {
                    HazardId = ReportHazard.Code;
                }
            }

            // Check if validation already exists
            var existingValidationQuery = new GetReportValidationByReportIdQuery(reportCode);
            var validationResult = await _mediator.SendAsync(existingValidationQuery, CancellationToken.None);
            
            if (validationResult.IsSuccess)
            {
                ReportValidation = validationResult.Value;
                
                // Populate form fields from existing validation
                ValidationDecision = ReportValidation.ValidationDecision ?? string.Empty;
                ValidationComments = ReportValidation.ValidationComments ?? string.Empty;
                ValidationType = ReportValidation.ValidationType ?? "Standard";
                ValidatedBy = ReportValidation.ValidatedBy ?? "SYSTEM"; // Use existing value, not hardcoded

                // Track that this is an existing validation
                IsExistingValidation = true;
                ExistingValidationId = ReportValidation.Id.Value;
                
                _logger.LogInformation("Loaded existing validation for report: {ReportId}", reportId);
            }
            else
            {
                // Set defaults for new validation
                ValidatedBy = "SYSTEM";
                ValidationType = "Standard";

                //Create new validation
                //var validation = ReportValidation.Create(ReportId, ValidatedBy);
                ReportValidationID id = new ReportValidationID("RV-0000");
                ReportValidation newReportValidation = new ReportValidation(id);
                newReportValidation.Code = id.Value;
                newReportValidation.ReportCode = reportCode.Value;
                newReportValidation.ValidatedBy = "SYSTEM";
                newReportValidation.Status = "InProgress";
                newReportValidation.Stage = "Initial";
                newReportValidation.ValidationType = "Standard";

                var createCommand = new CreateReportValidationCommand(newReportValidation);
                var result =  await _mediator.SendAsync(createCommand, CancellationToken.None);

                ReportValidation = result.Value;

                // Track that this is a new validation that now exists
                IsExistingValidation = true;
                ExistingValidationId = ReportValidation.Id.Value;

                _logger.LogInformation("Creating new validation for report: {ReportId}", reportId);
            }

            // Load available assessors
            try
            {
                var usersQuery = new GetAllSMSApplicationUsersQuery();
                var usersResult = await _mediator.SendAsync(usersQuery, CancellationToken.None);
                if (usersResult.IsSuccess)
                {
                    AvailableAssessors = usersResult.Value.ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load available assessors");
                AvailableAssessors = new List<SMSApplicationUser>();
            }
            
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading report for SMS validation: {ReportId}", reportId);
            TempData["ErrorMessage"] = "An error occurred while loading the report for validation";
            return RedirectToPage("/SafetyRiskManagement/HazardProcessing");
        }
    }

    /// <summary>
    /// Save current validation state as draft using SMS Backend
    /// </summary>
    public async Task<IActionResult> OnPostSaveDraftAsync()
    {
        try
        {
            _logger.LogInformation("Saving validation draft for report: {ReportId}", ReportId);

            // FIX: Use the helper method, not the null property
            var validationEntity = CreateOrUpdateValidationEntity();
            validationEntity.SaveAsDraft(ValidationComments);

            var result = await SaveValidationEntity(validationEntity);
            
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Draft saved successfully";
                _logger.LogInformation("Draft saved successfully for report {ReportId}", ReportId);
            }
            else
            {
                TempData["ErrorMessage"] = $"Error saving draft: {result.Error?.Message}";
                _logger.LogError("Draft save failed for report {ReportId}: {Error}", ReportId, result.Error?.Message);
            }
            
            return RedirectToPage(new { reportId = ReportId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving validation draft for report: {ReportId}", ReportId);
            TempData["ErrorMessage"] = "Error saving draft. Please try again.";
            return RedirectToPage(new { reportId = ReportId });
        }
    }

    /// <summary>
    /// Complete validation and proceed to assessment
    /// </summary>
    public async Task<IActionResult> OnPostProceedToAssessmentAsync()
    {
        _logger.LogWarning("🚀 OnPostProceedToAssessmentAsync called! ReportId: {ReportId}, HazardId: {HazardId}, ValidatedBy: {ValidatedBy}", 
            ReportId, HazardId, ValidatedBy);
        
        try
        {
            _logger.LogInformation("Proceeding to risk assessment for report: {ReportId}", ReportId);

            // Simple approach: create entity with form data
            var validationEntity = CreateOrUpdateValidationEntity();
            validationEntity.CompleteValidation("SMS_RISK", ValidationComments ?? "Validated as SMS Risk", ValidationType);

            var result = await SaveValidationEntity(validationEntity);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Report validation completed as SMS_RISK for report: {ReportId}", ReportId);

                // Navigate to RiskAssessmentWizard using "new" as ID to trigger creation
                return RedirectToPage("/SafetyRiskManagement/RiskAssessmentWizard", 
                    new { id = "new", stepNumber = 1, hazardId = HazardId });
            }
            else
            {
                ModelState.AddModelError("", result.Error?.Message ?? "Error processing validation.");
                await LoadDisplayDataAsync(); // Only reload display data, not form data
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proceeding to assessment for report: {ReportId}", ReportId);
            ModelState.AddModelError(string.Empty, "An error occurred while processing the validation. Please try again.");
            await LoadDisplayDataAsync(); // Only reload display data, not form data
            return Page();
        }
    }

    /// <summary>
    /// Complete SMS report validation with specific decision
    /// </summary>
    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            _logger.LogInformation("Processing SMS report validation for report: {ReportId}", ReportId);

            // Validate required fields
            if (string.IsNullOrWhiteSpace(ValidationDecision))
            {
                ModelState.AddModelError(nameof(ValidationDecision), "Please select a validation decision");
            }

            if (string.IsNullOrWhiteSpace(ValidationComments) || ValidationComments.Length < 5)
            {
                ModelState.AddModelError(nameof(ValidationComments), "Validation comments are required (minimum 5 characters)");
            }

            if (string.IsNullOrWhiteSpace(ValidatedBy))
            {
                ModelState.AddModelError(nameof(ValidatedBy), "ValidatedBy field is required");
            }

            if (string.IsNullOrWhiteSpace(HazardId))
            {
                ModelState.AddModelError(nameof(HazardId), "HazardId field is required");
            }

            if (!ModelState.IsValid)
            {
                // Reload display data but preserve form inputs
                await LoadDisplayDataAsync();
                return Page();
            }

            // Simple approach: create entity with form data
            var validationEntity = CreateOrUpdateValidationEntity();
            validationEntity.CompleteValidation(ValidationDecision, ValidationComments, ValidationType);

            var result = await SaveValidationEntity(validationEntity);

            if (result.IsSuccess)
            {
                var successMessage = ValidationDecision?.ToUpperInvariant() switch
                {
                    "SMS_RISK" => "SMS Report validation completed. Assessment will be scheduled.",
                    "NOT_SMS_RISK" => "Report closed as non-SMS risk.",
                    "NEEDS_INVESTIGATION" => "Investigation scheduled for additional information gathering.",
                    _ => "Validation completed successfully."
                };

                TempData["SuccessMessage"] = successMessage;

                // Route based on validation decision - use "new" ID for SMS_RISK creation
                return ValidationDecision?.ToUpperInvariant() switch
                {
                    "SMS_RISK" => RedirectToPage("/SafetyRiskManagement/RiskAssessmentWizard", 
                        new { id = "new", stepNumber = 1, hazardId = HazardId }),
                    "NOT_SMS_RISK" => RedirectToPage("/SafetyRiskManagement/ReportProcessing"),
                    "NEEDS_INVESTIGATION" => RedirectToPage("/SafetyRiskManagement/ReportProcessing"),
                    _ => RedirectToPage("/SafetyRiskManagement/ReportProcessing")
                };
            }
            else
            {
                ModelState.AddModelError("", result.Error?.Message ?? "Error saving validation.");
                await LoadDisplayDataAsync(); // Only reload display data, not form data
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SMS report validation: {ReportId}", ReportId);
            ModelState.AddModelError(string.Empty, "An unexpected error occurred during validation. Please try again.");
            await LoadDisplayDataAsync(); // Only reload display data, not form data
            return Page();
        }
    }

    #region Private Helper Methods

    private ReportValidation CreateOrUpdateValidationEntity()
    {
        // Simple approach: Create entity with form data, use the ID we stored in GET
        if (IsExistingValidation && !string.IsNullOrEmpty(ExistingValidationId))
        {
            // Update existing - create entity with existing ID and current form data
            var existingId = new ReportValidationID(ExistingValidationId);
            var validation = new ReportValidation(existingId)
            {
                Code = ExistingValidationId,
                ReportCode = ReportId,
                ValidationDecision = ValidationDecision,
                ValidationComments = ValidationComments,
                ValidationType = ValidationType,
                ValidatedBy = ValidatedBy,
                Status = "InProgress", // Business logic will update this
                Stage = "Initial"
            };
            
            _logger.LogInformation("Created entity for updating existing validation: {ValidationId}", ExistingValidationId);
            return validation;
        }
        else
        {
            // Create new validation
            var validation = ReportValidation.Create(ReportId, ValidatedBy);
            validation.ValidationDecision = ValidationDecision;
            validation.ValidationComments = ValidationComments;
            validation.ValidationType = ValidationType;
            
            _logger.LogInformation("Created new validation entity for report: {ReportId}", ReportId);
            return validation;
        }
    }

    private async Task<Result<ReportValidation>> SaveValidationEntity(ReportValidation validationEntity)
    {
        try
        {
            if (IsExistingValidation && !string.IsNullOrEmpty(ExistingValidationId))
            {
                // Update existing validation
                var updateCommand = new UpdateReportValidationCommand(validationEntity);
                return await _mediator.SendAsync(updateCommand, CancellationToken.None);
            }
            else
            {
                // Create new validation
                var createCommand = new CreateReportValidationCommand(validationEntity);
                return await _mediator.SendAsync(createCommand, CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving validation entity");
            return Result<ReportValidation>.Failure<ReportValidation>(new SMS_Domain.Common.Error("SAVE_FAILED", "Error saving validation"));
        }
    }

    private async Task LoadDisplayDataAsync()
    {
        try
        {
            // Reload display data for validation errors - DO NOT reload form data
            var reportCode = new ReportID(ReportId);

            var reportResult = await _mediator.SendAsync(new GetReportByIdQuery(reportCode), CancellationToken.None);
            if (reportResult.IsSuccess)
            {
                ReportDetails = reportResult.Value;
            }

            var hazardResult = await _mediator.SendAsync(new GetAllHazardsQuery(), CancellationToken.None);
            if (hazardResult.IsSuccess)
            {
                ReportHazard = hazardResult.Value.FirstOrDefault(h => h.ReportCode == ReportId);
                
                // Preserve HazardId if it's not already set (this happens on validation errors)
                if (string.IsNullOrEmpty(HazardId) && ReportHazard != null)
                {
                    HazardId = ReportHazard.Code;
                }
            }

            var usersQuery = new GetAllSMSApplicationUsersQuery();
            var usersResult = await _mediator.SendAsync(usersQuery, CancellationToken.None);
            if (usersResult.IsSuccess)
            {
                AvailableAssessors = usersResult.Value.ToList();
            }

            // DO NOT reload ReportValidation here - it would overwrite form inputs
            // await LoadReportValidationAsync(); // REMOVED!
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading display data");
        }
    }



    #endregion
}

