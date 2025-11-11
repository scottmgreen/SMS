using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SMS_Application.Interfaces;
using SMS_Application.Services;
using SMS_Application.Messaging.Queries;
using SMS_Domain.Entities;
using SMS_Shared.Common;
using System.ComponentModel.DataAnnotations;

namespace PDXSMS_Presentation.Pages.SafetyRiskManagement;

/// <summary>
/// FR-HZ: Hazard Investigation
/// Provides comprehensive investigation workflow for hazards requiring additional information
/// Uses proper Domain Entity (Investigation) and SMSInvestigationWorkflowService with CQRS
/// </summary>
public class HazardInvestigationModel : PageModel
{
    private readonly ISMSInvestigationWorkflowService _investigationWorkflowService;
    private readonly IMediator _mediator;
    private readonly ILogger<HazardInvestigationModel> _logger;

    public HazardInvestigationModel(
        ISMSInvestigationWorkflowService investigationWorkflowService,
        IMediator mediator,
        ILogger<HazardInvestigationModel> logger)
    {
        _investigationWorkflowService = investigationWorkflowService;
        _mediator = mediator;
        _logger = logger;
    }

    #region Properties - Direct Domain Entity Data

    // Route parameters
    [BindProperty(SupportsGet = true)]
    public string HazardId { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string ReportId { get; set; } = string.Empty;

    // The ACTUAL Investigation Domain Entity - NOT a DTO!
    public Investigation? Investigation { get; set; }

    // Form binding properties
    [BindProperty]
    public string AssignedInvestigatorId { get; set; } = string.Empty;

    [BindProperty]
    public string InvestigatorNotes { get; set; } = string.Empty;

    [BindProperty]
    public InvestigationDecisionType DecisionType { get; set; }

    [BindProperty]
    public string DecisionRationale { get; set; } = string.Empty;

    [BindProperty]
    public string DecisionMaker { get; set; } = string.Empty;

    // Display Properties
    public List<SMSApplicationUser> AvailableInvestigators { get; set; } = new();
    public Hazard? HazardInfo { get; set; }

    #endregion

    #region Page Handlers

    public async Task<IActionResult> OnGetAsync()
    {
        _logger.LogInformation("Loading Investigation page for Hazard: {HazardId}", HazardId);

        if (string.IsNullOrWhiteSpace(HazardId))
        {
            _logger.LogWarning("HazardId is required for investigation");
            TempData["ErrorMessage"] = "Hazard ID is required for investigation.";
            return RedirectToPage("/SafetyRiskManagement/HazardProcessing");
        }

        try
        {
            // Load available investigators using workflow service
            var investigatorsResult = await _investigationWorkflowService.GetAvailableInvestigatorsAsync();
            if (investigatorsResult.IsSuccess)
            {
                AvailableInvestigators = investigatorsResult.Value;
            }

            // Load existing investigation for this hazard using workflow service
            var investigationResult = await _investigationWorkflowService.GetInvestigationByHazardCodeAsync(HazardId);
            if (investigationResult.IsSuccess)
            {
                Investigation = investigationResult.Value;
                // Populate form fields from domain entity
                PopulateFormFieldsFromInvestigation();
            }

            // Load hazard information using CQRS
            await LoadHazardInfoAsync();

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading investigation for hazard {HazardId}", HazardId);
            TempData["ErrorMessage"] = "Error loading investigation data. Please try again.";
            return Page();
        }
    }

    #endregion

    #region Handler Methods - Using SMS Workflow Service

    /// <summary>
    /// Create or update investigation using SMS Workflow Service
    /// </summary>
    public async Task<IActionResult> OnPostSaveInvestigationAsync()
    {
        try
        {
            _logger.LogInformation("Saving investigation for hazard {HazardId}", HazardId);

            if (string.IsNullOrWhiteSpace(HazardId))
            {
                return new JsonResult(new { success = false, message = "Hazard ID is required" });
            }

            if (string.IsNullOrWhiteSpace(AssignedInvestigatorId))
            {
                return new JsonResult(new { success = false, message = "Please select an investigator" });
            }

            if (string.IsNullOrWhiteSpace(InvestigatorNotes) || InvestigatorNotes.Length < 10)
            {
                return new JsonResult(new { success = false, message = "Investigator notes are required (minimum 10 characters)" });
            }

            Result<Investigation> result;

            // Check if investigation already exists
            var existingResult = await _investigationWorkflowService.GetInvestigationByHazardCodeAsync(HazardId);
            
            if (existingResult.IsSuccess)
            {
                // Update existing investigation
                result = await _investigationWorkflowService.UpdateInvestigationNotesAsync(
                    existingResult.Value.Id.Value,
                    InvestigatorNotes);
            }
            else
            {
                // Create new investigation
                result = await _investigationWorkflowService.CreateInvestigationAsync(
                    HazardId,
                    AssignedInvestigatorId,
                    InvestigatorNotes);
            }

            if (result.IsFailure)
            {
                return new JsonResult(new { success = false, message = result.Error.Message });
            }

            Investigation = result.Value;

            _logger.LogInformation("Investigation {InvestigationId} saved for hazard {HazardId}", 
                result.Value.Id.Value, HazardId);

            return new JsonResult(new { 
                success = true, 
                message = "Investigation saved successfully",
                investigationId = result.Value.Id.Value
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving investigation for hazard {HazardId}", HazardId);
            return new JsonResult(new { success = false, message = "Error saving investigation setup" });
        }
    }

    /// <summary>
    /// Save investigation decision using SMS Workflow Service
    /// </summary>
    public async Task<IActionResult> OnPostSaveDecisionAsync()
    {
        try
        {
            _logger.LogInformation("Saving decision for hazard {HazardId}", HazardId);

            // Validate decision inputs
            if (string.IsNullOrWhiteSpace(DecisionRationale) || DecisionRationale.Length < 20)
            {
                return new JsonResult(new { success = false, message = "Decision rationale is required (minimum 20 characters)" });
            }

            if (string.IsNullOrWhiteSpace(DecisionMaker))
            {
                return new JsonResult(new { success = false, message = "Decision maker name is required" });
            }

            // Get existing investigation
            var investigationResult = await _investigationWorkflowService.GetInvestigationByHazardCodeAsync(HazardId);
            if (investigationResult.IsFailure)
            {
                return new JsonResult(new { success = false, message = "Investigation not found. Please save basic setup first." });
            }

            // Complete investigation with decision using workflow service
            var result = await _investigationWorkflowService.CompleteInvestigationAsync(
                investigationResult.Value.Id.Value,
                DecisionType,
                DecisionRationale,
                DecisionMaker);

            if (result.IsFailure)
            {
                return new JsonResult(new { success = false, message = result.Error.Message });
            }

            Investigation = result.Value;

            _logger.LogInformation("Investigation decision saved for {InvestigationId}", result.Value.Id.Value);

            return new JsonResult(new { 
                success = true, 
                message = "Investigation decision saved successfully",
                nextStep = GetNextStepMessage(DecisionType)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving investigation decision for hazard {HazardId}", HazardId);
            return new JsonResult(new { success = false, message = "Error saving investigation decision" });
        }
    }

    /// <summary>
    /// Complete the investigation and return to workflow
    /// </summary>
    public async Task<IActionResult> OnPostCompleteInvestigationAsync()
    {
        try
        {
            _logger.LogInformation("Completing investigation for hazard {HazardId}", HazardId);

            // Get existing investigation
            var investigationResult = await _investigationWorkflowService.GetInvestigationByHazardCodeAsync(HazardId);
            if (investigationResult.IsFailure)
            {
                TempData["ErrorMessage"] = "Investigation not found.";
                return RedirectToPage();
            }

            var investigation = investigationResult.Value;

            // Validate that decision has been made (check investigation notes for completion)
            if (string.IsNullOrWhiteSpace(investigation.InvestigationNotes) || 
                !investigation.InvestigationNotes.Contains("INVESTIGATION COMPLETED"))
            {
                TempData["ErrorMessage"] = "Please save an investigation decision before completing the investigation.";
                return RedirectToPage();
            }

            var nextStep = GetNextStepMessageFromNotes(investigation.InvestigationNotes);
            TempData["SuccessMessage"] = $"Investigation {investigation.Id.Value} completed successfully. {nextStep}";
            
            return RedirectToPage("/SafetyRiskManagement/HazardProcessing", null, "validation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing investigation for hazard {HazardId}", HazardId);
            TempData["ErrorMessage"] = "Error completing investigation.";
            return RedirectToPage();
        }
    }

    #endregion

    #region Helper Methods - Pure Domain Logic

    private void PopulateFormFieldsFromInvestigation()
    {
        if (Investigation == null) return;

        InvestigatorNotes = Investigation.InvestigationNotes ?? string.Empty;
        
        // Extract decision info from notes if completed
        if (!string.IsNullOrEmpty(Investigation.InvestigationNotes) && 
            Investigation.InvestigationNotes.Contains("INVESTIGATION COMPLETED"))
        {
            // Parse decision type from notes
            if (Investigation.InvestigationNotes.Contains("Decision: SMSRisk"))
                DecisionType = InvestigationDecisionType.SMSRisk;
            else if (Investigation.InvestigationNotes.Contains("Decision: NoSMSRisk"))
                DecisionType = InvestigationDecisionType.NoSMSRisk;
            else if (Investigation.InvestigationNotes.Contains("Decision: RequiresMoreInvestigation"))
                DecisionType = InvestigationDecisionType.RequiresMoreInvestigation;
            else if (Investigation.InvestigationNotes.Contains("Decision: ReferExternal"))
                DecisionType = InvestigationDecisionType.ReferExternal;
        }

        _logger.LogDebug("Populated form fields from Investigation Domain Entity");
    }

    private async Task LoadHazardInfoAsync()
    {
        try
        {
            // Use CQRS to get hazard information
            var query = new GetHazardByIdQuery(new HazardID(HazardId));
            var hazardResult = await _mediator.SendAsync(query);
            
            if (hazardResult.IsSuccess)
            {
                HazardInfo = hazardResult.Value;
                _logger.LogDebug("Loaded hazard info for {HazardId}", HazardId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading hazard info for {HazardId}", HazardId);
            // Non-critical error - continue without hazard info
        }
    }

    private static string GetNextStepMessage(InvestigationDecisionType decisionType) => decisionType switch
    {
        InvestigationDecisionType.SMSRisk => "Hazard returned to validation workflow for SMS risk processing.",
        InvestigationDecisionType.NoSMSRisk => "Hazard closed - determined to not constitute an SMS risk.",
        InvestigationDecisionType.RequiresMoreInvestigation => "Investigation continues for additional information gathering.",
        InvestigationDecisionType.ReferExternal => "Hazard closed - referred to external organization for handling.",
        _ => "Hazard returned to validation workflow."
    };

    private static string GetNextStepMessageFromNotes(string notes)
    {
        if (notes.Contains("Decision: SMSRisk"))
            return GetNextStepMessage(InvestigationDecisionType.SMSRisk);
        if (notes.Contains("Decision: NoSMSRisk"))
            return GetNextStepMessage(InvestigationDecisionType.NoSMSRisk);
        if (notes.Contains("Decision: RequiresMoreInvestigation"))
            return GetNextStepMessage(InvestigationDecisionType.RequiresMoreInvestigation);
        if (notes.Contains("Decision: ReferExternal"))
            return GetNextStepMessage(InvestigationDecisionType.ReferExternal);
        
        return "Hazard returned to validation workflow.";
    }

    // Display helper methods
    public string GetInvestigationStatus()
    {
        if (Investigation?.InvestigationNotes?.Contains("INVESTIGATION COMPLETED") == true)
            return "Completed";
        if (Investigation != null)
            return "In Progress";
        return "Not Started";
    }

    public bool IsInvestigationCompleted()
    {
        return Investigation?.InvestigationNotes?.Contains("INVESTIGATION COMPLETED") == true;
    }

    public string GetInvestigationId()
    {
        return Investigation?.Id?.Value ?? "Not Created";
    }

    public string GetHazardDescription()
    {
        return HazardInfo?.Description ?? "Hazard information not available";
    }

    public string GetHazardStatus()
    {
        return HazardInfo?.Status ?? "Unknown";
    }

    #endregion
}