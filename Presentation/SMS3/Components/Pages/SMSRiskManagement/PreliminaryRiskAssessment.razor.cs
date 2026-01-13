using Microsoft.AspNetCore.Components;
using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Domain.ValueObjects;
using SMS_Application.Messaging.Queries;
using SMS_Application.Messaging.Commands;
using SMS_Application.Interfaces;
using SMS_Shared.Common;
using Radzen;

namespace SMS3.Components.Pages.SMSRiskManagement;

/// <summary>
/// Code-behind for Preliminary Risk Assessment page
/// Handles the streamlined 1-step risk assessment process for moderate complexity hazards
/// </summary>
public partial class PreliminaryRiskAssessment : ComponentBase
{
    #region Injected Services
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private ILogger<PreliminaryRiskAssessment> Logger { get; set; } = default!;
    #endregion

    #region Parameters
    [Parameter] public string? AssessmentId { get; set; }
    [Parameter] public string? HazardId { get; set; }
    [Parameter] public string? ReportId { get; set; }
    [Parameter] public int? Step { get; set; }
    #endregion

    #region State Properties
    private bool IsLoading { get; set; } = true;
    private bool IsSaving { get; set; } = false;
    
    public int ProgressPercentage { get; set; } = 0;
    public string CurrentSection { get; set; } = "Risk Level Assessment";
    public Hazard? Hazard { get; set; }
    public RiskAssessment? Assessment { get; set; }

    // Form Model
    public PreliminaryAssessmentModel Model { get; set; } = new();
    #endregion

    #region Dropdown Options
    private readonly List<string> LikelihoodOptions = new()
    {
        "Very Low (< 1% chance)",
        "Low (1-10% chance)", 
        "Medium (10-50% chance)",
        "High (50-90% chance)",
        "Very High (> 90% chance)"
    };

    private readonly List<string> SeverityOptions = new()
    {
        "Negligible (Minor inconvenience)",
        "Minor (Temporary disruption)",
        "Moderate (Significant impact)", 
        "Major (Serious consequences)",
        "Catastrophic (Severe/fatal consequences)"
    };

    private readonly List<string> EffectivenessOptions = new()
    {
        "Highly Effective (90%+ risk reduction)",
        "Effective (70-90% risk reduction)",
        "Moderately Effective (50-70% risk reduction)",
        "Limited Effectiveness (30-50% risk reduction)",
        "Ineffective (< 30% risk reduction)"
    };

    private readonly List<string> ResidualRiskOptions = new()
    {
        "Low (Acceptable with monitoring)",
        "Medium (Requires additional controls)",
        "High (Requires immediate action)",
        "Critical (Stop operations until mitigated)"
    };

    private readonly List<string> PriorityOptions = new()
    {
        "Immediate (Within 24 hours)",
        "Urgent (Within 1 week)",
        "High (Within 1 month)",
        "Medium (Within 3 months)",
        "Low (Within 6 months)"
    };

    private readonly List<string> CostOptions = new()
    {
        "Minimal (< $1,000)",
        "Low ($1,000 - $10,000)",
        "Medium ($10,000 - $50,000)",
        "High ($50,000 - $100,000)",
        "Very High (> $100,000)"
    };

    private readonly List<string> ConfidenceOptions = new()
    {
        "High (Sufficient data, clear analysis)",
        "Medium (Some uncertainty, additional data helpful)",
        "Low (Limited data, requires further investigation)"
    };

    private readonly List<string> FollowUpOptions = new()
    {
        "No - Assessment complete",
        "Monitoring - Track implementation",
        "Review - Reassess after actions",
        "TRA - Requires Technical Risk Assessment"
    };
    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        await LoadAssessment();
    }
    #endregion

    #region Data Loading Methods
    private async Task LoadAssessment()
    {
        // If we don't have an AssessmentId, we can't proceed
        if (string.IsNullOrWhiteSpace(AssessmentId))
        {
            ShowErrorNotification("Assessment ID is required for preliminary risk assessment");
            Navigation.NavigateTo("/SMSRiskManagement/ReportProcessing");
            return;
        }

        try
        {
            IsLoading = true;
            
            // Load or create assessment first
            await LoadOrCreateAssessment();
            
            // If we have an assessment but no HazardId parameter, get it from the assessment
            if (Assessment != null && string.IsNullOrWhiteSpace(HazardId))
            {
                HazardId = Assessment.HazardCode;
            }
            
            // Load hazard if we have a HazardId
            if (!string.IsNullOrWhiteSpace(HazardId))
            {
                var hazardQuery = new GetHazardByIdQuery(new HazardID(HazardId));
                var hazardResult = await Mediator.SendAsync(hazardQuery, CancellationToken.None);
                
                if (hazardResult.IsSuccess)
                {
                    Hazard = hazardResult.Value;
                    ReportId = Hazard.ReportCode;
                }
            }
            
            CalculateProgress();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading preliminary risk assessment: AssessmentId={AssessmentId}, HazardId={HazardId}", AssessmentId, HazardId);
            ShowErrorNotification("An error occurred while loading the risk assessment");
            Navigation.NavigateTo("/SMSRiskManagement/ReportProcessing");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadOrCreateAssessment()
    {
        // Try to find existing assessment by AssessmentId first
        var assessmentQuery = new GetRiskAssessmentByIdQuery(new RiskAssessmentID(AssessmentId!));
        var assessmentResult = await Mediator.SendAsync(assessmentQuery, CancellationToken.None);
        
        if (assessmentResult.IsSuccess)
        {
            Assessment = assessmentResult.Value;
            LoadFormFromAssessment(Assessment);
            Logger.LogInformation("Found existing RiskAssessment with ID: {AssessmentId}", AssessmentId);
        }
        else
        {
            // No existing assessment found, create a new one
            // Use Report ID as basis for new assessment ID
            await CreateNewAssessment();
        }
    }

    private async Task CreateNewAssessment()
    {
        // Generate a proper RiskAssessment ID based on the Report ID
        // If AssessmentId is RP-0269, create RS-0269 for the risk assessment
        var riskAssessmentId = AssessmentId!.StartsWith("RP-") 
            ? AssessmentId.Replace("RP-", "RS-")
            : $"RS-{AssessmentId}";

        var createResult = RiskAssessment.CreateInitial(
            new RiskAssessmentID(riskAssessmentId),
            $"Preliminary Risk Assessment for Report {AssessmentId}",
            "System User",
            RiskAssessmentCategory.Preliminary,
            HazardId ?? string.Empty,
            HazardId);

        if (createResult.IsSuccess)
        {
            Assessment = createResult.Value;
            // Set the report code for linking
            Assessment.Description = $"Created from Report {AssessmentId}";
            
            var createCommand = new CreateRiskAssessmentCommand(Assessment);
            var result = await Mediator.SendAsync(createCommand, CancellationToken.None);
            
            if (!result.IsSuccess)
            {
                Logger.LogError("Failed to create new assessment: {Error}", result.Error?.Message);
                throw new Exception($"Failed to create assessment: {result.Error?.Message}");
            }

            Logger.LogInformation("Created new RiskAssessment {RiskAssessmentId} from Report {ReportId}", 
                riskAssessmentId, AssessmentId);
        }
        else
        {
            Logger.LogError("Failed to create assessment object: {Error}", createResult.Error?.Message);
            throw new Exception($"Failed to create assessment: {createResult.Error?.Message}");
        }
    }

    private void LoadFormFromAssessment(RiskAssessment assessment)
    {
        if (!string.IsNullOrEmpty(assessment.Description))
        {
            try
            {
                var data = global::System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(assessment.Description);
                if (data != null)
                {
                    Model.Likelihood = data.GetValueOrDefault("likelihood", "");
                    Model.Severity = data.GetValueOrDefault("severity", "");
                    Model.ExistingControls = data.GetValueOrDefault("existingControls", "");
                    Model.ControlEffectiveness = data.GetValueOrDefault("controlEffectiveness", "");
                    Model.ResidualRisk = data.GetValueOrDefault("residualRisk", "");
                    Model.RecommendedActions = data.GetValueOrDefault("recommendedActions", "");
                    Model.Priority = data.GetValueOrDefault("priority", "");
                    Model.EstimatedCost = data.GetValueOrDefault("estimatedCost", "");
                    Model.AssessorComments = data.GetValueOrDefault("assessorComments", "");
                    Model.AssessmentConfidence = data.GetValueOrDefault("assessmentConfidence", "");
                    Model.RequiresFollowUp = data.GetValueOrDefault("requiresFollowUp", "No - Assessment complete");
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Error loading preliminary assessment data");
            }
        }
    }
    #endregion

    #region Form Action Methods
    private async Task SaveDraft()
    {
        IsSaving = true;
        try
        {
            await SaveAssessment(isDraft: true);
            ShowSuccessNotification("Assessment saved as draft successfully.");
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task CompleteAssessment()
    {
        if (!ValidateForm())
        {
            ShowErrorNotification("Please fill in all required fields before completing the assessment.");
            return;
        }

        IsSaving = true;
        try
        {
            await SaveAssessment(isDraft: false);
            
            var overallRisk = DetermineOverallRisk();
            var nextAction = DetermineNextAction();
            
            ShowSuccessNotification($"Preliminary risk assessment {AssessmentId} completed successfully. Risk level: {overallRisk}. Next: {nextAction}.");
            Navigation.NavigateTo("/SMSRiskManagement/ReportProcessing");
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task SaveAssessment(bool isDraft = false)
    {
        if (Assessment == null) return;

        var data = new Dictionary<string, string>
        {
            ["likelihood"] = Model.Likelihood ?? "",
            ["severity"] = Model.Severity ?? "",
            ["existingControls"] = Model.ExistingControls ?? "",
            ["controlEffectiveness"] = Model.ControlEffectiveness ?? "",
            ["residualRisk"] = Model.ResidualRisk ?? "",
            ["recommendedActions"] = Model.RecommendedActions ?? "",
            ["priority"] = Model.Priority ?? "",
            ["estimatedCost"] = Model.EstimatedCost ?? "",
            ["assessorComments"] = Model.AssessorComments ?? "",
            ["assessmentConfidence"] = Model.AssessmentConfidence ?? "",
            ["requiresFollowUp"] = Model.RequiresFollowUp ?? "No"
        };

        Assessment.Description = global::System.Text.Json.JsonSerializer.Serialize(data);
        Assessment.Status = isDraft ? RiskAssessmentStatus.InProgress : RiskAssessmentStatus.Completed;
        Assessment.UpdatedDate = DateTime.UtcNow;

        if (!isDraft)
        {
            Assessment.CompletedDate = DateTime.UtcNow;
            Assessment.CompletedBy = "System User";
            Assessment.FinalRiskLevel = DetermineOverallRisk();
            Assessment.AssessmentRationale = $"Preliminary risk assessment completed. Overall risk: {Assessment.FinalRiskLevel}";
        }

        var updateCommand = new UpdateRiskAssessmentCommand(Assessment);
        var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

        if (!result.IsSuccess)
        {
            throw new Exception($"Failed to save assessment: {result.Error?.Message}");
        }
    }
    #endregion

    #region Validation and Calculation Methods
    private bool ValidateForm()
    {
        return !string.IsNullOrWhiteSpace(Model.Likelihood) &&
               !string.IsNullOrWhiteSpace(Model.Severity) &&
               !string.IsNullOrWhiteSpace(Model.ResidualRisk) &&
               !string.IsNullOrWhiteSpace(Model.RecommendedActions) &&
               !string.IsNullOrWhiteSpace(Model.Priority);
    }

    private void CalculateProgress()
    {
        var completedFields = 0;
        var totalFields = 8;

        if (!string.IsNullOrWhiteSpace(Model.Likelihood)) completedFields++;
        if (!string.IsNullOrWhiteSpace(Model.Severity)) completedFields++;
        if (!string.IsNullOrWhiteSpace(Model.ExistingControls)) completedFields++;
        if (!string.IsNullOrWhiteSpace(Model.ControlEffectiveness)) completedFields++;
        if (!string.IsNullOrWhiteSpace(Model.ResidualRisk)) completedFields++;
        if (!string.IsNullOrWhiteSpace(Model.RecommendedActions)) completedFields++;
        if (!string.IsNullOrWhiteSpace(Model.Priority)) completedFields++;
        if (!string.IsNullOrWhiteSpace(Model.AssessmentConfidence)) completedFields++;

        ProgressPercentage = (int)Math.Round((double)completedFields / totalFields * 100);
    }

    public int GetSectionProgress(int section)
    {
        return section switch
        {
            1 => (!string.IsNullOrWhiteSpace(Model.Likelihood) && !string.IsNullOrWhiteSpace(Model.Severity)) ? 100 : 0,
            2 => (!string.IsNullOrWhiteSpace(Model.ExistingControls) && !string.IsNullOrWhiteSpace(Model.ResidualRisk)) ? 100 : 0,
            3 => (!string.IsNullOrWhiteSpace(Model.RecommendedActions) && !string.IsNullOrWhiteSpace(Model.Priority)) ? 100 : 0,
            4 => !string.IsNullOrWhiteSpace(Model.AssessmentConfidence) ? 100 : 0,
            _ => 0
        };
    }

    private string DetermineOverallRisk()
    {
        var severity = Model.Severity?.ToLowerInvariant();
        var likelihood = Model.Likelihood?.ToLowerInvariant();

        if (severity?.Contains("catastrophic") == true && (likelihood?.Contains("high") == true))
            return "Critical";
        if (severity?.Contains("major") == true || (severity?.Contains("catastrophic") == true && likelihood?.Contains("medium") == true))
            return "High";
        if (severity?.Contains("moderate") == true || (severity?.Contains("major") == true && likelihood?.Contains("low") == true))
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
    #endregion

    #region Notification Methods
    private void ShowSuccessNotification(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = "Success",
            Detail = message,
            Duration = 4000
        });
    }

    private void ShowErrorNotification(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = "Error", 
            Detail = message,
            Duration = 6000
        });
    }
    #endregion

    #region Data Model
    public class PreliminaryAssessmentModel
    {
        public string Likelihood { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string ExistingControls { get; set; } = string.Empty;
        public string ControlEffectiveness { get; set; } = string.Empty;
        public string ResidualRisk { get; set; } = string.Empty;
        public string RecommendedActions { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string EstimatedCost { get; set; } = string.Empty;
        public string AssessorComments { get; set; } = string.Empty;
        public string AssessmentConfidence { get; set; } = string.Empty;
        public string RequiresFollowUp { get; set; } = "No - Assessment complete";
    }
    #endregion
}