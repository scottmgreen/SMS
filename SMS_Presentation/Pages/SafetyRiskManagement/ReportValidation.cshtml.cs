using System.Security.Cryptography;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
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
/// SMS Report Validation Page - Fixed ValidationDecision binding
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

    #region PageModel Properties for Form Binding
    
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

    [BindProperty]
    public string HazardId { get; set; } = string.Empty;

    [BindProperty]
    public bool IsExistingValidation { get; set; }

    // Remove any Required attribute and make it nullable for new validations
    [BindProperty]
    public string? ExistingValidationId { get; set; }

    #endregion

    #region SelectLists for Dropdowns
    
    public SelectList ValidationTypeOptions => new SelectList(
        new List<object>
        {
            new { Value = "Preliminary", Text = "Preliminary Accessment" },
            new { Value = "Technical", Text = "Technical Assessment" } //,
            //new { Value = "Complex", Text = "Complex Case Validation" }
        },
        "Value",
        "Text",
        ValidationType
    );

    public SelectList ValidatedByOptions
    {
        get
        {
            var options = new List<object>
            {
                new { Value = "", Text = "Auto-assign based on workload" }
            };

            if (AvailableAssessors?.Any() == true)
            {
                foreach (var assessor in AvailableAssessors)
                {
                    var assessorUserName = assessor.UserName.Value;
                    var assessorText = $"{assessor.DisplayName} ({assessorUserName}) - {assessor.ApplicationRole}";
                    options.Add(new { Value = assessorUserName, Text = assessorText });
                }
            }

            return new SelectList(options, "Value", "Text", ValidatedBy);
        }
    }

    #endregion

    #region Display Properties
    
    public Report? ReportDetails { get; set; }
    public Hazard? ReportHazard { get; set; }
    public ReportValidation? ReportValidation { get; set; }
    public List<SMSApplicationUser> AvailableAssessors { get; set; } = new();

    public bool IsUpdate => IsExistingValidation && ReportValidation != null;
    public string ValidationCode => ReportValidation?.Code ?? "New";
    public string CurrentStatus => ReportValidation?.Status ?? "New";

    #endregion

    #region UI State Properties
    
    public bool ShouldShowSmsRiskFields => ValidationDecision == "SMS_RISK";
    
    // Show Submit button for NOT_SMS_RISK and NEEDS_INVESTIGATION (and also when nothing is selected initially)
    public bool ShouldShowSubmitButton => string.IsNullOrEmpty(ValidationDecision) || ValidationDecision != "SMS_RISK";
    
    // Only show Proceed button when SMS_RISK is specifically selected
    public bool ShouldShowProceedButton => ValidationDecision == "SMS_RISK";

    public string GetValidationCardClass(string decisionValue)
    {
        if (ValidationDecision != decisionValue) 
            return "validation-card";

        return decisionValue switch
        {
            "SMS_RISK" => "validation-card selected-sms-risk",
            "NOT_SMS_RISK" => "validation-card selected-not-sms-risk", 
            "NEEDS_INVESTIGATION" => "validation-card selected-investigation",
            _ => "validation-card"
        };
    }

    public string SmsRiskFieldsDisplayStyle => ShouldShowSmsRiskFields ? "block" : "none";
    public string ProceedButtonDisplayStyle => ShouldShowProceedButton ? "inline-block" : "none";
    public string SubmitButtonDisplayStyle => ShouldShowSubmitButton ? "inline-block" : "none";

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

            // Load associated hazard
            var hazardResult = await _mediator.SendAsync(new GetAllHazardsQuery(), CancellationToken.None);
            if (hazardResult.IsSuccess)
            {
                ReportHazard = hazardResult.Value.ToList().FirstOrDefault(h => h.ReportCode.Trim() == reportCode.Value.Trim());
                
                if (ReportHazard != null)
                {
                    HazardId = ReportHazard.Code;
                }
            }

            // Check for existing validation
            var existingValidationQuery = new GetReportValidationByReportIdQuery(reportCode);
            var validationResult = await _mediator.SendAsync(existingValidationQuery, CancellationToken.None);
            
            if (validationResult.IsSuccess)
            {
                ReportValidation = validationResult.Value;
                
                // Populate form fields from existing validation
                ValidationDecision = ReportValidation.ValidationDecision ?? string.Empty;
                ValidationComments = ReportValidation.ValidationComments ?? string.Empty;
                ValidationType = ReportValidation.ValidationType ?? "Standard";
                ValidatedBy = ReportValidation.ValidatedBy ?? string.Empty;

                // Track that this is an existing validation
                IsExistingValidation = true;
                ExistingValidationId = ReportValidation.Id.Value;
                
                _logger.LogInformation("Found existing ReportValidation {ValidationId} for report {ReportId} - Decision: {Decision}", 
                    ExistingValidationId, reportId, ValidationDecision);
            }
            else
            {
                _logger.LogInformation("No existing ReportValidation found for report: {ReportId}", reportId);
                
                // Set defaults for new validation - EXPLICITLY set to null for new validations
                ValidatedBy = string.Empty;
                ValidationType = "Standard";
                ValidationDecision = string.Empty;
                ValidationComments = string.Empty;
                
                IsExistingValidation = false;
                ExistingValidationId = null; // Explicitly set to null for new validations
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

            _logger.LogInformation("Completed loading ReportValidation page. ValidationDecision: '{Decision}', ValidationType: '{Type}', ValidatedBy: '{ValidatedBy}'", 
                ValidationDecision, ValidationType, ValidatedBy);
            
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading report for SMS validation: {ReportId}", reportId);
            TempData["ErrorMessage"] = "An error occurred while loading the report for validation";
            return RedirectToPage("/SafetyRiskManagement/HazardProcessing");
        }
    }

    public async Task<IActionResult> OnPostSaveDraftAsync()
    {
        try
        {
            _logger.LogInformation("Saving validation draft for report: {ReportId}, Decision: '{Decision}'", ReportId, ValidationDecision);

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

    public async Task<IActionResult> OnPostProceedToAssessmentAsync()
    {
        _logger.LogInformation("🚀 OnPostProceedToAssessmentAsync - ReportId: {ReportId}, ValidationDecision: '{Decision}', ValidatedBy: '{ValidatedBy}'", 
            ReportId, ValidationDecision, ValidatedBy);
        
        try
        {
            // Force ValidationDecision to SMS_RISK for assessment flow
            if (ValidationDecision != "SMS_RISK")
            {
                _logger.LogInformation("Forcing ValidationDecision to SMS_RISK for assessment flow (was: '{Previous}')", ValidationDecision);
                ValidationDecision = "SMS_RISK";
            }

            // Validate required fields
            var validationErrors = ValidateRequiredFields();
            if (validationErrors.Any())
            {
                foreach (var error in validationErrors)
                {
                    ModelState.AddModelError(error.Key, error.Value);
                }

                await LoadDisplayDataAsync();
                return Page();
            }

            var validationEntity = CreateOrUpdateValidationEntity();
            validationEntity.CompleteValidation(ValidationDecision, ValidationComments, ValidationType);

            var result = await SaveValidationEntity(validationEntity);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Report validation completed as {Decision} for report: {ReportId} by {ValidatedBy}", 
                    ValidationDecision, ReportId, ValidatedBy);

                
                return RedirectToPage("/SafetyRiskManagement/RiskAssessmentWizard", new { id = "new", stepNumber = 1, hazardId = HazardId });
            }
            else
            {
                ModelState.AddModelError("", result.Error?.Message ?? "Error processing validation.");
                await LoadDisplayDataAsync();
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proceeding to assessment for report: {ReportId}", ReportId);
            ModelState.AddModelError(string.Empty, "An error occurred while processing the validation. Please try again.");
            await LoadDisplayDataAsync();
            return Page();
        }
    }

    #region Private Helper Methods

    private Dictionary<string, string> ValidateRequiredFields()
    {
        var errors = new Dictionary<string, string>();

        if (string.IsNullOrWhiteSpace(ValidationDecision))
        {
            errors.Add(nameof(ValidationDecision), "Please select a validation decision");
        }
        else if (!IsValidDecision(ValidationDecision))
        {
            errors.Add(nameof(ValidationDecision), "Invalid validation decision selected");
        }

        if (string.IsNullOrWhiteSpace(ValidationComments) || ValidationComments.Length < 5)
        {
            errors.Add(nameof(ValidationComments), "Validation comments are required (minimum 5 characters)");
        }

        // Only validate ValidatedBy if SMS_RISK is selected (assessment assignment required)
        if (ValidationDecision == "SMS_RISK" && string.IsNullOrWhiteSpace(ValidatedBy))
        {
            errors.Add(nameof(ValidatedBy), "Assigned Assessor is required for SMS Risk validations");
        }

        if (string.IsNullOrWhiteSpace(HazardId))
        {
            errors.Add(nameof(HazardId), "HazardId field is required");
        }

        _logger.LogInformation("Validation check - Decision: '{Decision}', Comments length: {Length}, ValidatedBy: '{ValidatedBy}', HazardId: '{HazardId}', Errors: {ErrorCount}", 
            ValidationDecision, ValidationComments?.Length ?? 0, ValidatedBy, HazardId, errors.Count);

        return errors;
    }

    private bool IsValidDecision(string decision)
    {
        var validDecisions = new[] { "SMS_RISK", "NOT_SMS_RISK", "NEEDS_INVESTIGATION" };
        return validDecisions.Contains(decision);
    }

    private ReportValidation CreateOrUpdateValidationEntity()
    {
        if (IsExistingValidation && !string.IsNullOrEmpty(ExistingValidationId))
        {
            var existingId = new ReportValidationID(ExistingValidationId);
            var validation = new ReportValidation(existingId)
            {
                Code = ExistingValidationId,
                ReportCode = ReportId,
                ValidationDecision = ValidationDecision,
                ValidationComments = ValidationComments,
                ValidationType = ValidationType,
                ValidatedBy = string.IsNullOrEmpty(ValidatedBy) ? "SYSTEM" : ValidatedBy,
                Status = "InProgress",
                Stage = "Initial"
            };
            
            _logger.LogInformation("Updated existing validation entity: {ValidationId} with Decision: '{Decision}'", ExistingValidationId, ValidationDecision);
            return validation;
        }
        else
        {
            var validatedByValue = string.IsNullOrEmpty(ValidatedBy) ? "SYSTEM" : ValidatedBy;
            var validation = ReportValidation.Create(ReportId, validatedByValue);
            validation.ValidationDecision = ValidationDecision;
            validation.ValidationComments = ValidationComments;
            validation.ValidationType = ValidationType;
            validation.Status = "InProgress";
            validation.Stage = "Initial";
            
            _logger.LogInformation("Created new validation entity for report: {ReportId} with Decision: '{Decision}'", ReportId, ValidationDecision);
            return validation;
        }
    }

    private async Task<Result<ReportValidation>> SaveValidationEntity(ReportValidation validationEntity)
    {
        try
        {
            if (IsExistingValidation && !string.IsNullOrEmpty(ExistingValidationId))
            {
                var updateCommand = new UpdateReportValidationCommand(validationEntity);
                return await _mediator.SendAsync(updateCommand, CancellationToken.None);
            }
            else
            {
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
            var reportCode = new ReportID(ReportId);

            var reportResult = await _mediator.SendAsync(new GetReportByIdQuery(reportCode), CancellationToken.None);
            if (reportResult.IsSuccess)
            {
                ReportDetails = reportResult.Value;
            }

            var hazardResult = await _mediator.SendAsync(new GetHazardsByReportIdQuery(reportCode), CancellationToken.None);
            if (hazardResult.IsSuccess)
            {
                ReportHazard = hazardResult.Value.FirstOrDefault();
                
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading display data");
        }
    }

    #endregion
}

