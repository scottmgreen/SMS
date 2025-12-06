using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;
using SMS_Application.Messaging.Queries;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Shared.Common;
using System.ComponentModel.DataAnnotations;

namespace SMS_Presentation.Pages.SafetyRiskManagement;

public class SimplifiedRiskAssessmentModel : PageModel
{
    private readonly ILogger<SimplifiedRiskAssessmentModel> _logger;
    private readonly IMediator _mediator;

    public SimplifiedRiskAssessmentModel(
        ILogger<SimplifiedRiskAssessmentModel> logger,
        IMediator mediator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    // Route parameters
    [BindProperty(SupportsGet = true)]
    public string HazardId { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string AssessmentId { get; set; } = string.Empty;

    // Display properties
    public int ProgressPercentage { get; set; } = 0;
    public string CurrentSection { get; set; } = "Risk Level Assessment";
    public Hazard? Hazard { get; set; }
    public RiskAssessment? Assessment { get; set; }

    // Form binding - using simple properties instead of DTOs
    [BindProperty]
    [Required(ErrorMessage = "Likelihood is required")]
    public string Likelihood { get; set; } = string.Empty;
    
    [BindProperty]
    [Required(ErrorMessage = "Severity is required")]
    public string Severity { get; set; } = string.Empty;
    
    [BindProperty]
    public string ExistingControls { get; set; } = string.Empty;
    
    [BindProperty]
    public string ControlEffectiveness { get; set; } = string.Empty;
    
    [BindProperty]
    [Required(ErrorMessage = "Residual risk level is required")]
    public string ResidualRisk { get; set; } = string.Empty;
    
    [BindProperty]
    [Required(ErrorMessage = "Recommended actions are required")]
    public string RecommendedActions { get; set; } = string.Empty;
    
    [BindProperty]
    [Required(ErrorMessage = "Priority is required")]
    public string Priority { get; set; } = string.Empty;
    
    [BindProperty]
    public string EstimatedCost { get; set; } = string.Empty;
    
    [BindProperty]
    public string AssessorComments { get; set; } = string.Empty;
    
    [BindProperty]
    public string AssessmentConfidence { get; set; } = string.Empty;
    
    [BindProperty]
    public string RequiresFollowUp { get; set; } = "No";

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrWhiteSpace(HazardId) || string.IsNullOrWhiteSpace(AssessmentId))
        {
            TempData["ErrorMessage"] = "Hazard ID and Assessment ID are required for simplified risk assessment";
            return RedirectToPage("/SafetyRiskManagement/ReportProcessing");
        }

        try
        {
            _logger.LogInformation("Loading simplified risk assessment for hazard: {HazardId}, assessment: {AssessmentId}", 
                HazardId, AssessmentId);

            // Load hazard using CQRS
            var hazardQuery = new GetHazardByIdQuery(new HazardID(HazardId));
            var hazardResult = await _mediator.SendAsync(hazardQuery, CancellationToken.None);
            
            if (hazardResult.IsSuccess)
            {
                Hazard = hazardResult.Value;
            }
            else
            {
                _logger.LogWarning("Hazard not found: {HazardId}", HazardId);
            }
            
            // Load existing assessment using CQRS
            var assessmentQuery = new GetRiskAssessmentByIdQuery(new RiskAssessmentID(AssessmentId));
            var assessmentResult = await _mediator.SendAsync(assessmentQuery, CancellationToken.None);
            
            if (assessmentResult.IsSuccess)
            {
                Assessment = assessmentResult.Value;
                LoadFormFromAssessment(Assessment);
            }
            else
            {
                // Create new assessment if it doesn't exist
                await CreateNewSimplifiedAssessment();
            }
            
            // Calculate progress
            ProgressPercentage = CalculateProgress();

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading simplified risk assessment: {HazardId}, {AssessmentId}", 
                HazardId, AssessmentId);
            TempData["ErrorMessage"] = "An error occurred while loading the risk assessment";
            return RedirectToPage("/SafetyRiskManagement/ReportProcessing");
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            // Reload hazard data for form
            var hazardQuery = new GetHazardByIdQuery(new HazardID(HazardId));
            var hazardResult = await _mediator.SendAsync(hazardQuery, CancellationToken.None);
            if (hazardResult.IsSuccess)
            {
                Hazard = hazardResult.Value;
            }
            
            ProgressPercentage = CalculateProgress();
            return Page();
        }

        try
        {
            _logger.LogInformation("Saving simplified risk assessment: {AssessmentId} for hazard {HazardId}", 
                AssessmentId, HazardId);

            // Get or create assessment
            var assessmentQuery = new GetRiskAssessmentByIdQuery(new RiskAssessmentID(AssessmentId));
            var assessmentResult = await _mediator.SendAsync(assessmentQuery, CancellationToken.None);
            
            RiskAssessment assessment;
            if (assessmentResult.IsSuccess)
            {
                assessment = assessmentResult.Value;
            }
            else
            {
                // Create new simplified assessment
                var createResult = RiskAssessment.CreateInitial(
                    new RiskAssessmentID(AssessmentId),
                    $"Simplified Risk Assessment for {HazardId}",
                    User.Identity?.Name ?? "System User",
                    RiskAssessmentCategory.Simplified,
                    HazardId);

                if (!createResult.IsSuccess)
                {
                    TempData["ErrorMessage"] = "Failed to create assessment: " + createResult.Error.Message;
                    return RedirectToPage("/SafetyRiskManagement/ReportProcessing");
                }

                assessment = createResult.Value;
                
                // Save the new assessment
                var createCommand = new CreateRiskAssessmentCommand(assessment);
                await _mediator.SendAsync(createCommand, CancellationToken.None);
            }

            // Update assessment with form data
            UpdateAssessmentFromForm(assessment);
            
            // Save updated assessment
            var updateCommand = new UpdateRiskAssessmentCommand(assessment);
            var updateResult = await _mediator.SendAsync(updateCommand, CancellationToken.None);

            if (!updateResult.IsSuccess)
            {
                TempData["ErrorMessage"] = "Failed to save assessment: " + updateResult.Error.Message;
                return Page();
            }

            var overallRisk = DetermineOverallRisk();
            var nextAction = DetermineNextAction();

            TempData["SuccessMessage"] = $"Simplified risk assessment {AssessmentId} completed successfully. " +
                                       $"Risk level: {overallRisk}. Next: {nextAction}.";

            return RedirectToPage("/SafetyRiskManagement/ReportProcessing");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving simplified risk assessment: {AssessmentId}", AssessmentId);
            TempData["ErrorMessage"] = "An error occurred while saving the risk assessment";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostSaveDraftAsync()
    {
        try
        {
            _logger.LogInformation("Saving draft for simplified risk assessment: {AssessmentId} for hazard {HazardId}", 
                AssessmentId, HazardId);

            // Get or create assessment
            var assessmentQuery = new GetRiskAssessmentByIdQuery(new RiskAssessmentID(AssessmentId));
            var assessmentResult = await _mediator.SendAsync(assessmentQuery, CancellationToken.None);
            
            RiskAssessment assessment;
            if (assessmentResult.IsSuccess)
            {
                assessment = assessmentResult.Value;
            }
            else
            {
                // Create new simplified assessment
                var createResult = RiskAssessment.CreateInitial(
                    new RiskAssessmentID(AssessmentId),
                    $"Simplified Risk Assessment for {HazardId}",
                    User.Identity?.Name ?? "System User",
                    RiskAssessmentCategory.Simplified,
                    HazardId);

                if (!createResult.IsSuccess)
                {
                    TempData["ErrorMessage"] = "Failed to create assessment: " + createResult.Error.Message;
                    return Page();
                }

                assessment = createResult.Value;
                
                // Save the new assessment
                var createCommand = new CreateRiskAssessmentCommand(assessment);
                await _mediator.SendAsync(createCommand, CancellationToken.None);
            }

            // Update assessment with form data (no validation for draft)
            UpdateAssessmentFromForm(assessment, isDraft: true);
            
            // Save updated assessment
            var updateCommand = new UpdateRiskAssessmentCommand(assessment);
            var updateResult = await _mediator.SendAsync(updateCommand, CancellationToken.None);

            if (updateResult.IsSuccess)
            {
                TempData["InfoMessage"] = "Assessment saved as draft successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error saving draft: " + updateResult.Error.Message;
            }
            
            // Reload data for form
            var hazardQuery = new GetHazardByIdQuery(new HazardID(HazardId));
            var hazardResult = await _mediator.SendAsync(hazardQuery, CancellationToken.None);
            if (hazardResult.IsSuccess)
            {
                Hazard = hazardResult.Value;
            }
            
            Assessment = assessment;
            ProgressPercentage = CalculateProgress();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving draft: {AssessmentId}", AssessmentId);
            TempData["ErrorMessage"] = "Error saving draft: " + ex.Message;
            
            // Reload data for form
            var hazardQuery = new GetHazardByIdQuery(new HazardID(HazardId));
            var hazardResult = await _mediator.SendAsync(hazardQuery, CancellationToken.None);
            if (hazardResult.IsSuccess)
            {
                Hazard = hazardResult.Value;
            }
            
            ProgressPercentage = CalculateProgress();
            return Page();
        }
    }

    private async Task CreateNewSimplifiedAssessment()
    {
        var createResult = RiskAssessment.CreateInitial(
            new RiskAssessmentID(AssessmentId),
            $"Simplified Risk Assessment for {HazardId}",
            User.Identity?.Name ?? "System User",
            RiskAssessmentCategory.Simplified,
            HazardId);

        if (createResult.IsSuccess)
        {
            Assessment = createResult.Value;
            
            var createCommand = new CreateRiskAssessmentCommand(Assessment);
            await _mediator.SendAsync(createCommand, CancellationToken.None);
        }
    }

    private void LoadFormFromAssessment(RiskAssessment assessment)
    {
        // Load simplified assessment data from assessment description or notes
        // For now, simplified assessments store data in the description field as JSON
        // This could be enhanced to use proper fields in the future
        if (!string.IsNullOrEmpty(assessment.Description))
        {
            try
            {
                // Try to deserialize simplified data from description
                var data = global::System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(assessment.Description);
                if (data != null)
                {
                    Likelihood = data.GetValueOrDefault("likelihood", "");
                    Severity = data.GetValueOrDefault("severity", "");
                    ExistingControls = data.GetValueOrDefault("existingControls", "");
                    ControlEffectiveness = data.GetValueOrDefault("controlEffectiveness", "");
                    ResidualRisk = data.GetValueOrDefault("residualRisk", "");
                    RecommendedActions = data.GetValueOrDefault("recommendedActions", "");
                    Priority = data.GetValueOrDefault("priority", "");
                    EstimatedCost = data.GetValueOrDefault("estimatedCost", "");
                    AssessorComments = data.GetValueOrDefault("assessorComments", "");
                    AssessmentConfidence = data.GetValueOrDefault("assessmentConfidence", "");
                    RequiresFollowUp = data.GetValueOrDefault("requiresFollowUp", "No");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error loading simplified assessment data from description for {AssessmentId}", AssessmentId);
            }
        }
    }

    private void UpdateAssessmentFromForm(RiskAssessment assessment, bool isDraft = false)
    {
        // Store simplified assessment data as JSON in description field
        var data = new Dictionary<string, string>
        {
            ["likelihood"] = Likelihood ?? "",
            ["severity"] = Severity ?? "",
            ["existingControls"] = ExistingControls ?? "",
            ["controlEffectiveness"] = ControlEffectiveness ?? "",
            ["residualRisk"] = ResidualRisk ?? "",
            ["recommendedActions"] = RecommendedActions ?? "",
            ["priority"] = Priority ?? "",
            ["estimatedCost"] = EstimatedCost ?? "",
            ["assessorComments"] = AssessorComments ?? "",
            ["assessmentConfidence"] = AssessmentConfidence ?? "",
            ["requiresFollowUp"] = RequiresFollowUp ?? "No"
        };
        
        assessment.Description = global::System.Text.Json.JsonSerializer.Serialize(data);
        assessment.Status = isDraft ? RiskAssessmentStatus.InProgress : RiskAssessmentStatus.Completed;
        assessment.UpdatedDate = DateTime.UtcNow;
        
        if (!isDraft)
        {
            assessment.CompletedDate = DateTime.UtcNow;
            assessment.CompletedBy = User.Identity?.Name ?? "System User";
            
            // Set risk scores based on simplified assessment
            assessment.FinalRiskLevel = DetermineOverallRisk();
            assessment.AssessmentRationale = $"Simplified risk assessment completed. Overall risk: {assessment.FinalRiskLevel}";
        }
    }

    private int CalculateProgress()
    {
        var completedFields = 0;
        var totalFields = 8; // Key required fields

        if (!string.IsNullOrWhiteSpace(Likelihood)) completedFields++;
        if (!string.IsNullOrWhiteSpace(Severity)) completedFields++;
        if (!string.IsNullOrWhiteSpace(ExistingControls)) completedFields++;
        if (!string.IsNullOrWhiteSpace(ControlEffectiveness)) completedFields++;
        if (!string.IsNullOrWhiteSpace(ResidualRisk)) completedFields++;
        if (!string.IsNullOrWhiteSpace(RecommendedActions)) completedFields++;
        if (!string.IsNullOrWhiteSpace(Priority)) completedFields++;
        if (!string.IsNullOrWhiteSpace(AssessmentConfidence)) completedFields++;

        return (int)Math.Round((double)completedFields / totalFields * 100);
    }

    private string DetermineOverallRisk()
    {
        // Simple risk matrix calculation
        var severity = Severity?.ToLowerInvariant();
        var likelihood = Likelihood?.ToLowerInvariant();

        if (severity == "catastrophic" && (likelihood == "high" || likelihood == "very high"))
            return "Critical";
        if (severity == "major" || (severity == "catastrophic" && likelihood == "medium"))
            return "High";
        if (severity == "moderate" || (severity == "major" && likelihood == "low"))
            return "Medium";

        return "Low";
    }

    private string DetermineNextAction()
    {
        var overallRisk = DetermineOverallRisk();
        return overallRisk switch
        {
            "Critical" => "Immediate action required - operations may need to stop",
            "High" => "Urgent mitigation required - may require TRA",
            "Medium" => "Mitigation planning required within 30 days",
            "Low" => "Monitor and track implementation",
            _ => "Continue with normal process"
        };
    }
}

