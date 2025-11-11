using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SMS_Application.Messaging.Commands;
using SMS_Application.Messaging.Queries;
using SMS_Domain.Entities;
using SMS_Shared.Common;
using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;
using SMS_Domain.Common;

namespace SMS.Presentation.Pages.SafetyRiskManagement;

/// <summary>
/// SMS Report Validation Page - Direct Entity Binding with SMS Backend
/// Uses ReportValidation Domain Entity directly with CQRS Commands/Queries
/// NO PageModel properties - Direct binding to Domain Entity
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

    // DIRECT ENTITY BINDING - NO PageModel properties!
    [BindProperty]
    public ReportValidation ValidationEntity { get; set; } 

    // Additional display data from SMS Backend
    public Report? ReportDetails { get; set; }
    public List<SMSApplicationUser> AvailableAssessors { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string? reportId = null)
    {
        if (string.IsNullOrWhiteSpace(reportId))
        {
            TempData["ErrorMessage"] = "Report ID is required for SMS report validation";
            return RedirectToPage("/SafetyRiskManagement/HazardProcessing");
        }

        try
        {
            _logger.LogInformation("Loading SMS report validation for report: {ReportId}", reportId);
            
            // Check if validation already exists
            var existingValidationQuery = new GetAllReportValidationsQuery();
            var allValidationsResult = await _mediator.SendAsync(existingValidationQuery, new CancellationToken());
            
            ReportValidation? existingValidation = null;
            if (allValidationsResult.IsSuccess)
            {
                existingValidation = allValidationsResult.Value.FirstOrDefault(v => v.ReportCode == reportId);
            }

            if (existingValidation != null)
            {
                // Load existing validation
                ValidationEntity = existingValidation;
                _logger.LogInformation("Loaded existing validation for report: {ReportId}", reportId);
            }
            else
            {
                // Create new validation entity using factory method
                ValidationEntity = ReportValidation.Create(reportId, User?.Identity?.Name ?? "System");
                _logger.LogInformation("Created new validation entity for report: {ReportId}", reportId);
            }

            // Load report details for display (when available)
            try
            {
                // TODO: Add GetReportByIdQuery when available
                // var reportQuery = new GetReportByIdQuery { ReportId = new ReportID(reportId) };
                // var reportResult = await _mediator.Send(reportQuery);
                // if (reportResult.IsSuccess)
                // {
                //     ReportDetails = reportResult.Value;
                // }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load report details for {ReportId}", reportId);
            }

            // Load available assessors
            try
            {
                var usersQuery = new GetAllSMSApplicationUsersQuery();
                var usersResult = await _mediator.SendAsync(usersQuery, new CancellationToken());
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
            _logger.LogInformation("Saving validation draft for report: {ReportId}", ValidationEntity.ReportCode);

            // Use entity business logic to save as draft
            ValidationEntity.SaveAsDraft(ValidationEntity.ValidationComments);

            var result = await SaveValidationEntity();
            
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Validation draft saved successfully. You can continue working or return later.";
                _logger.LogInformation("Draft saved successfully for report {ReportId}", ValidationEntity.ReportCode);
            }
            else
            {
                TempData["ErrorMessage"] = result.Error?.Message ?? "Error saving draft.";
                _logger.LogError("Draft save failed for report {ReportId}: {Error}", ValidationEntity.ReportCode, result.Error?.Message);
            }
            
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving validation draft for report: {ReportId}", ValidationEntity.ReportCode);
            TempData["ErrorMessage"] = "Error saving draft. Please try again.";
            return Page();
        }
    }

    /// <summary>
    /// Complete validation and proceed to assessment
    /// </summary>
    public async Task<IActionResult> OnPostProceedToAssessmentAsync()
    {
        try
        {
            _logger.LogInformation("Proceeding to risk assessment for report: {ReportId}", ValidationEntity.ReportCode);

            // Use entity business logic to complete validation
            ValidationEntity.CompleteValidation("SMS_RISK", ValidationEntity.ValidationComments ?? "Validated as SMS Risk", ValidationEntity.ValidationType);

            var result = await SaveValidationEntity();

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "SMS Report validation completed successfully. Proceeding to assessment.";
                
                return RedirectToPage("/SafetyRiskManagement/RiskAssessmentWizard", 
                    new { hazardId = ValidationEntity.ReportCode });
            }
            else
            {
                ModelState.AddModelError("", result.Error?.Message ?? "Error processing validation.");
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proceeding to assessment for report: {ReportId}", ValidationEntity.ReportCode);
            ModelState.AddModelError(string.Empty, "An error occurred while processing the validation. Please try again.");
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
            _logger.LogInformation("Processing SMS report validation for report: {ReportId}", ValidationEntity.ReportCode);

            // Validate required fields
            if (string.IsNullOrWhiteSpace(ValidationEntity.ValidationDecision))
            {
                ModelState.AddModelError("ValidationEntity.ValidationDecision", "Please select a validation decision");
            }

            if (string.IsNullOrWhiteSpace(ValidationEntity.ValidationComments) || ValidationEntity.ValidationComments.Length < 5)
            {
                ModelState.AddModelError("ValidationEntity.ValidationComments", "Validation comments are required (minimum 5 characters)");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Use entity business logic to complete validation
            ValidationEntity.CompleteValidation(
                ValidationEntity.ValidationDecision,
                ValidationEntity.ValidationComments,
                ValidationEntity.ValidationType);

            var result = await SaveValidationEntity();

            if (result.IsSuccess)
            {
                var successMessage = ValidationEntity.ValidationDecision?.ToUpperInvariant() switch
                {
                    "SMS_RISK" => "SMS Report validation completed. Assessment will be scheduled.",
                    "NOT_SMS_RISK" => "Report closed as non-SMS risk.",
                    "NEEDS_INVESTIGATION" => "Investigation scheduled for additional information gathering.",
                    _ => "Validation completed successfully."
                };

                TempData["SuccessMessage"] = successMessage;

                // Route based on validation decision
                return ValidationEntity.ValidationDecision?.ToUpperInvariant() switch
                {
                    "SMS_RISK" => RedirectToPage("/SafetyRiskManagement/RiskAssessmentWizard", 
                        new { hazardId = ValidationEntity.ReportCode }),
                    "NOT_SMS_RISK" => RedirectToPage("/SafetyRiskManagement/HazardProcessing"),
                    "NEEDS_INVESTIGATION" => RedirectToPage("/SafetyRiskManagement/HazardProcessing"),
                    _ => RedirectToPage("/SafetyRiskManagement/HazardProcessing")
                };
            }
            else
            {
                ModelState.AddModelError("", result.Error?.Message ?? "Error saving validation.");
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SMS report validation: {ReportId}", ValidationEntity.ReportCode);
            ModelState.AddModelError(string.Empty, "An unexpected error occurred during validation. Please try again.");
            return Page();
        }
    }

    private async Task<Result<ReportValidation>> SaveValidationEntity()
    {
        try
        {
            // Check if this is an update or create operation
            if (string.IsNullOrWhiteSpace(ValidationEntity.Code))
            {
                // Create new validation - use constructor with ReportValidation parameter
                var createCommand = new CreateReportValidationCommand(ValidationEntity);
                return await _mediator.SendAsync(createCommand, new CancellationToken());
            }
            else
            {
                // Update existing validation - use constructor with ReportValidation parameter
                var updateCommand = new UpdateReportValidationCommand(ValidationEntity);
                return await _mediator.SendAsync(updateCommand,new CancellationToken());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving validation entity");
            return Result<ReportValidation>.Failure<ReportValidation>(new SMS_Domain.Common.Error("SAVE_FAILED", "Error saving validation"));
        }
    }
}

