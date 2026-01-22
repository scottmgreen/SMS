namespace SMS3.Components.Pages.SMSRiskManagement;

/// <summary>
/// Hybrid Risk Assessment - Combines preliminary assessment simplicity with technical assessment power
/// Users can start with streamlined assessment and expand to full technical capabilities as needed
/// </summary>
public partial class HybridRiskAssessment : ComponentBase
{
    #region Injected Services
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private ILogger<HybridRiskAssessment> Logger { get; set; } = default!;
    #endregion

    #region Parameters
    [Parameter] public string? AssessmentId { get; set; }
    [Parameter] public string? HazardId { get; set; }
    [Parameter] public string? ReportId { get; set; }
    #endregion

    #region State Properties
    private bool IsLoading { get; set; } = true;
    private bool IsSaving { get; set; } = false;
    private int ActiveTabIndex { get; set; } = 0;
    private string AssessmentMode { get; set; } = "Preliminary";
    private bool ShowAdvancedSections { get; set; } = false;

    public Hazard? Hazard { get; set; }
    public RiskAssessment? InitialRiskAssessment { get; set; }
    public RiskAssessment? ResidualRiskAssessment { get; set; }
    public List<Hazard> AvailableHazards { get; set; } = new();
    public List<SMSStakeholderUser> AvailableStakeholders { get; set; } = new();
    public List<SMSApplicationUser> AvailableAssessors { get; set; } = new();
    public List<SMSStakeholderGroup> StakeholderGroups { get; set; } = new();

    // Assessment Models
    public PreliminaryAssessmentModel PreliminaryModel { get; set; } = new();
    public Step1Model Step1Data { get; set; } = new();
    public Step3Model Step3Data { get; set; } = new();
    public Step4Model Step4Data { get; set; } = new();
    public Step5Model Step5Data { get; set; } = new();

    // Collapsible sections tracking
    private Dictionary<int, bool> AdvancedSectionsExpanded { get; set; } = new()
    {
        { 1, false }, { 2, false }, { 3, false }, { 4, false }, { 5, false }
    };
    #endregion

    #region Assessment Mode Options
    private readonly List<AssessmentModeOption> AssessmentModeOptions = new()
    {
        new("Preliminary", "Preliminary (Streamlined)"),
        new("Hybrid", "Hybrid (Flexible)"),
        new("Technical", "Technical (Comprehensive)")
    };

    public record AssessmentModeOption(string Value, string Text);
    #endregion

    #region Dropdown Options (same as PreliminaryRiskAssessment)
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
        if (string.IsNullOrWhiteSpace(AssessmentId))
        {
            ShowErrorNotification("Assessment ID is required for hybrid risk assessment");
            Navigation.NavigateTo("/SMSRiskManagement/ReportProcessing");
            return;
        }

        try
        {
            IsLoading = true;

            // Load or create assessments
            await LoadOrCreateAssessments();

            // Load stakeholder and assessor data
            await LoadStakeholderData();

            // Load hazard data
            await LoadHazardData();

        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading hybrid risk assessment: AssessmentId={AssessmentId}, HazardId={HazardId}", AssessmentId, HazardId);
            ShowErrorNotification("An error occurred while loading the risk assessment");
            Navigation.NavigateTo("/SMSRiskManagement/ReportProcessing");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadOrCreateAssessments()
    {
        // Generate proper assessment IDs
        var initialAssessmentId = AssessmentId!.StartsWith("RP-")
            ? AssessmentId.Replace("RP-", "RS-")
            : $"RS-{AssessmentId}";

        var residualAssessmentId = AssessmentId.StartsWith("RP-")
            ? AssessmentId.Replace("RP-", "RRS-")
            : $"RRS-{AssessmentId}";

        // Load or create initial assessment
        var initialQuery = new GetRiskAssessmentByIdQuery(new RiskAssessmentID(initialAssessmentId));
        var initialResult = await Mediator.SendAsync(initialQuery, CancellationToken.None);

        if (initialResult.IsSuccess)
        {
            InitialRiskAssessment = initialResult.Value;
            LoadPreliminaryFromAssessment(InitialRiskAssessment);
        }
        else
        {
            await CreateInitialAssessment(initialAssessmentId);
        }

        // Load or create residual assessment if in technical mode
        if (AssessmentMode == "Technical")
        {
            var residualQuery = new GetRiskAssessmentByIdQuery(new RiskAssessmentID(residualAssessmentId));
            var residualResult = await Mediator.SendAsync(residualQuery, CancellationToken.None);

            if (residualResult.IsSuccess)
            {
                ResidualRiskAssessment = residualResult.Value;
            }
            else if (InitialRiskAssessment != null)
            {
                await CreateResidualAssessment(residualAssessmentId);
            }
        }
    }

    private async Task CreateInitialAssessment(string assessmentId)
    {
        var createResult = RiskAssessment.CreateInitial(
            new RiskAssessmentID(assessmentId),
            $"Hybrid Risk Assessment for Report {AssessmentId}",
            "System User",
            RiskAssessmentCategory.Technical, // Default to technical to allow expansion
            HazardId ?? string.Empty,
            HazardId);

        if (createResult.IsSuccess)
        {
            InitialRiskAssessment = createResult.Value;

            var createCommand = new CreateRiskAssessmentCommand(InitialRiskAssessment);
            await Mediator.SendAsync(createCommand, CancellationToken.None);

            Logger.LogInformation("Created initial assessment {AssessmentId}", assessmentId);
        }
    }

    private async Task CreateResidualAssessment(string assessmentId)
    {
        var createResult = RiskAssessment.CreateResidual(
            new RiskAssessmentID(assessmentId),
            $"Residual Risk Assessment for Report {AssessmentId}",
            "System User",
            InitialRiskAssessment!.Code,
            HazardId ?? string.Empty,
            HazardId);

        if (createResult.IsSuccess)
        {
            ResidualRiskAssessment = createResult.Value;

            var createCommand = new CreateRiskAssessmentCommand(ResidualRiskAssessment);
            await Mediator.SendAsync(createCommand, CancellationToken.None);

            Logger.LogInformation("Created residual assessment {AssessmentId}", assessmentId);
        }
    }

    private async Task LoadStakeholderData()
    {
        try
        {
            // TODO: Find and use correct query names for loading stakeholder data
            // For now, initialize with empty collections to prevent errors
            AvailableStakeholders = new List<SMSStakeholderUser>();
            AvailableAssessors = new List<SMSApplicationUser>();
            StakeholderGroups = new List<SMSStakeholderGroup>();

            Logger.LogInformation("Stakeholder data initialized (queries need to be implemented)");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading stakeholder data");
        }
    }

    private async Task LoadHazardData()
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(HazardId))
            {
                var hazardQuery = new GetHazardByIdQuery(new HazardID(HazardId));
                var hazardResult = await Mediator.SendAsync(hazardQuery, CancellationToken.None);

                if (hazardResult.IsSuccess)
                {
                    Hazard = hazardResult.Value;
                    AvailableHazards = new List<Hazard> { Hazard };
                    ReportId = Hazard.ReportCode;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading hazard data");
        }
    }

    private void LoadPreliminaryFromAssessment(RiskAssessment assessment)
    {
        if (!string.IsNullOrEmpty(assessment.Description))
        {
            try
            {
                var data = global::System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(assessment.Description);
                if (data != null)
                {
                    PreliminaryModel.Likelihood = data.GetValueOrDefault("likelihood", "");
                    PreliminaryModel.Severity = data.GetValueOrDefault("severity", "");
                    PreliminaryModel.ExistingControls = data.GetValueOrDefault("existingControls", "");
                    PreliminaryModel.ControlEffectiveness = data.GetValueOrDefault("controlEffectiveness", "");
                    PreliminaryModel.ResidualRisk = data.GetValueOrDefault("residualRisk", "");
                    PreliminaryModel.RecommendedActions = data.GetValueOrDefault("recommendedActions", "");
                    PreliminaryModel.Priority = data.GetValueOrDefault("priority", "");
                    PreliminaryModel.EstimatedCost = data.GetValueOrDefault("estimatedCost", "");
                    PreliminaryModel.AssessorComments = data.GetValueOrDefault("assessorComments", "");
                    PreliminaryModel.AssessmentConfidence = data.GetValueOrDefault("assessmentConfidence", "");
                    PreliminaryModel.RequiresFollowUp = data.GetValueOrDefault("requiresFollowUp", "No - Assessment complete");
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Error loading assessment data");
            }
        }
    }
    #endregion

    #region UI Event Handlers
    private async Task OnAssessmentModeChanged()
    {
        ShowAdvancedSections = AssessmentMode != "Preliminary";

        if (AssessmentMode == "Technical")
        {
            // Auto-expand all advanced sections
            for (int i = 1; i <= 5; i++)
            {
                AdvancedSectionsExpanded[i] = true;
            }

            // Create residual assessment if it doesn't exist
            if (ResidualRiskAssessment == null && InitialRiskAssessment != null)
            {
                var residualAssessmentId = AssessmentId!.StartsWith("RP-")
                    ? AssessmentId.Replace("RP-", "RRS-")
                    : $"RRS-{AssessmentId}";

                await CreateResidualAssessment(residualAssessmentId);
            }
        }
        else if (AssessmentMode == "Preliminary")
        {
            // Collapse all advanced sections
            for (int i = 1; i <= 5; i++)
            {
                AdvancedSectionsExpanded[i] = false;
            }
        }

        StateHasChanged();
    }

    private void ToggleAdvancedSection(int sectionNumber)
    {
        AdvancedSectionsExpanded[sectionNumber] = !AdvancedSectionsExpanded[sectionNumber];
        StateHasChanged();
    }

    // Step model update handlers
    private async Task UpdateStep1Data(Step1Model data)
    {
        Step1Data = data;
        await InvokeAsync(StateHasChanged);
    }

    private async Task UpdateStep3Data(Step3Model data)
    {
        Step3Data = data;
        await InvokeAsync(StateHasChanged);
    }

    private async Task UpdateStep4Data(Step4Model data)
    {
        Step4Data = data;
        await InvokeAsync(StateHasChanged);
    }

    private async Task UpdateStep5Data(Step5Model data)
    {
        Step5Data = data;
        await InvokeAsync(StateHasChanged);
    }
    #endregion

    #region Technical Assessment Event Handlers
    private async Task OnMitigationUpdated(string hazardId)
    {
        Logger.LogInformation("Mitigation updated for hazard {HazardId}", hazardId);
    }

    private async Task OnResidualHazardScored()
    {
        Logger.LogInformation("Residual hazard scored");
    }

    private async Task OnResidualAnalysisFieldChanged(Hazard hazard, string fieldName, string value)
    {
        Logger.LogInformation("Residual analysis field changed: {Field} = {Value}", fieldName, value);
    }

    // Residual analysis data getters
    private string GetResidualWorstOutcome(Hazard hazard) => "";
    private string GetResidualRootCause(Hazard hazard) => "";
    private string GetResidualAdditionalComments(Hazard hazard) => "";
    #endregion

    #region Form Actions
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

            ShowSuccessNotification($"Hybrid risk assessment completed successfully. Risk level: {overallRisk}. Next: {nextAction}.");
            Navigation.NavigateTo("/SMSRiskManagement/ReportProcessing");
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task SaveAssessment(bool isDraft = false)
    {
        if (InitialRiskAssessment == null) return;

        var data = new Dictionary<string, string>
        {
            ["likelihood"] = PreliminaryModel.Likelihood ?? "",
            ["severity"] = PreliminaryModel.Severity ?? "",
            ["existingControls"] = PreliminaryModel.ExistingControls ?? "",
            ["controlEffectiveness"] = PreliminaryModel.ControlEffectiveness ?? "",
            ["residualRisk"] = PreliminaryModel.ResidualRisk ?? "",
            ["recommendedActions"] = PreliminaryModel.RecommendedActions ?? "",
            ["priority"] = PreliminaryModel.Priority ?? "",
            ["estimatedCost"] = PreliminaryModel.EstimatedCost ?? "",
            ["assessorComments"] = PreliminaryModel.AssessorComments ?? "",
            ["assessmentConfidence"] = PreliminaryModel.AssessmentConfidence ?? "",
            ["requiresFollowUp"] = PreliminaryModel.RequiresFollowUp ?? "No",
            ["assessmentMode"] = AssessmentMode
        };

        InitialRiskAssessment.Description = global::System.Text.Json.JsonSerializer.Serialize(data);
        InitialRiskAssessment.Status = isDraft ? RiskAssessmentStatus.InProgress : RiskAssessmentStatus.Completed;
        InitialRiskAssessment.UpdatedDate = DateTime.UtcNow;

        if (!isDraft)
        {
            InitialRiskAssessment.CompletedDate = DateTime.UtcNow;
            InitialRiskAssessment.CompletedBy = "System User";
            InitialRiskAssessment.FinalRiskLevel = DetermineOverallRisk();
            InitialRiskAssessment.AssessmentRationale = $"Hybrid risk assessment completed in {AssessmentMode} mode. Overall risk: {InitialRiskAssessment.FinalRiskLevel}";
        }

        var updateCommand = new UpdateRiskAssessmentCommand(InitialRiskAssessment);
        var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

        if (!result.IsSuccess)
        {
            throw new Exception($"Failed to save assessment: {result.Error?.Message}");
        }
    }
    #endregion

    #region UI Helper Methods
    private AlertStyle GetModeAlertStyle() => AssessmentMode switch
    {
        "Preliminary" => AlertStyle.Info,
        "Technical" => AlertStyle.Warning,
        "Hybrid" => AlertStyle.Success,
        _ => AlertStyle.Base
    };

    private string GetModeDescription() => AssessmentMode switch
    {
        "Preliminary" => "Streamlined assessment with essential fields only",
        "Technical" => "Full 5-step SMS methodology with all advanced features",
        "Hybrid" => "Start simple, expand sections as needed",
        _ => "Select an assessment mode"
    };

    private string GetTabName(int tabIndex) => tabIndex switch
    {
        0 => "System & Hazards",
        1 => "Risk Analysis",
        2 => "Risk Assessment",
        3 => "Mitigation",
        4 => "Summary",
        _ => "Assessment"
    };

    private double GetTabProgress(int tabIndex) => tabIndex switch
    {
        0 => GetBasicFieldProgress(new[] { Step1Data.SystemPurpose, Step1Data.SystemBoundaries }),
        1 => GetBasicFieldProgress(new[] { PreliminaryModel.Likelihood, PreliminaryModel.Severity }),
        2 => GetBasicFieldProgress(new[] { PreliminaryModel.ExistingControls, PreliminaryModel.ResidualRisk }),
        3 => GetBasicFieldProgress(new[] { PreliminaryModel.RecommendedActions, PreliminaryModel.Priority }),
        4 => GetBasicFieldProgress(new[] { PreliminaryModel.AssessmentConfidence }),
        _ => 0
    };

    private double GetBasicFieldProgress(string[] fields)
    {
        var completedFields = fields.Count(field => !string.IsNullOrWhiteSpace(field));
        return fields.Length > 0 ? (double)completedFields / fields.Length * 100 : 0;
    }

    private double GetOverallProgress()
    {
        var totalTabs = 5;
        var tabProgress = 0.0;
        for (int i = 0; i < totalTabs; i++)
        {
            tabProgress += GetTabProgress(i);
        }
        return tabProgress / totalTabs;
    }

    public string GetOverallRiskAssessment()
    {
        if (string.IsNullOrEmpty(PreliminaryModel.Severity) || string.IsNullOrEmpty(PreliminaryModel.Likelihood))
            return "";
        return DetermineOverallRisk();
    }

    public string GetRecommendedNextAction()
    {
        return DetermineNextAction();
    }
    #endregion

    #region Validation and Risk Calculation
    private bool ValidateForm()
    {
        return !string.IsNullOrWhiteSpace(PreliminaryModel.Likelihood) &&
               !string.IsNullOrWhiteSpace(PreliminaryModel.Severity) &&
               !string.IsNullOrWhiteSpace(PreliminaryModel.ResidualRisk) &&
               !string.IsNullOrWhiteSpace(PreliminaryModel.RecommendedActions) &&
               !string.IsNullOrWhiteSpace(PreliminaryModel.Priority);
    }

    private string DetermineOverallRisk()
    {
        var severity = PreliminaryModel.Severity?.ToLowerInvariant();
        var likelihood = PreliminaryModel.Likelihood?.ToLowerInvariant();

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
            "High" => "Urgent mitigation required - consider full technical assessment",
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

    #region Data Model (same as PreliminaryRiskAssessment)
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