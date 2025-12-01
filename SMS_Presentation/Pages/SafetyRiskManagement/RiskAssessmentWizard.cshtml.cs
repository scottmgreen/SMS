using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;
using SMS_Application.Messaging.Queries;
using SMS_Application.Services;

using SMS_Domain.Common;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;

using SMS_Shared.Common;

namespace SMS.Presentation.Pages.SafetyRiskManagement;


/// <summary>
/// SIMPLIFIED Risk Assessment Wizard - Refactored for cleaner step handling
/// Uses internal step models for better organization while keeping same UI
/// </summary>

public class RiskAssessmentWizardModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<RiskAssessmentWizardModel> _logger;

    public RiskAssessmentWizardModel(
        IMediator mediator,
        RiskAssessmentService riskAssessmentService, HazardService hazardService,
        ILogger<RiskAssessmentWizardModel> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    #region Helper Classes for Risk Assessment

    /// <summary>
    /// Panel Member Score Data for Step 4 assessments
    /// </summary>
    public class PanelMemberScoreData
    {
        public string PanelMemberId { get; set; } = string.Empty;
        public string MemberId { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;
        public string HazardId { get; set; } = string.Empty;
        public int SeverityScore { get; set; }
        public int LikelihoodScore { get; set; }
        public double CalculatedScore => SeverityScore * LikelihoodScore;
        public string RiskLevel { get; set; } = string.Empty;
        public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;
        public bool IsComplete => SeverityScore > 0 && LikelihoodScore > 0;

        public PanelMemberScoreData()
        {
            MemberId = PanelMemberId;
        }
    }

    /// <summary>
    /// Individual Hazard Risk Analysis
    /// </summary>
    public class HazardRiskAnalysis
    {
        public string HazardId { get; set; } = string.Empty;
        public string HazardDescription { get; set; } = string.Empty;
        public string HazardCategory { get; set; } = string.Empty;

        [Required(ErrorMessage = "Worst credible outcome is required for risk analysis.")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Worst credible outcome must be between 10 and 1000 characters.")]
        public string WorstCredibleOutcome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Root cause analysis is required for risk analysis.")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Root cause analysis must be between 10 and 1000 characters.")]
        public string RootCauseAnalysis { get; set; } = string.Empty;

        public string AdditionalComments { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;

        public bool IsComplete =>
            !string.IsNullOrWhiteSpace(WorstCredibleOutcome) && WorstCredibleOutcome.Length >= 10 &&
            !string.IsNullOrWhiteSpace(RootCauseAnalysis) && RootCauseAnalysis.Length >= 10;
    }

    /// <summary>
    /// Validation status for a single hazard in Step 3
    /// </summary>
    public class HazardValidationStatus
    {
        public string HazardId { get; set; } = string.Empty;
        public string HazardDescription { get; set; } = string.Empty;
        public bool HasWorstOutcome { get; set; }
        public bool HasRootCause { get; set; }
        public int WorstOutcomeLength { get; set; }
        public int RootCauseLength { get; set; }
        public bool IsWorstOutcomeValid { get; set; }
        public bool IsRootCauseValid { get; set; }
        public bool IsComplete { get; set; }

        public string WorstOutcomeValidationMessage =>
            !HasWorstOutcome ? "Required" :
            !IsWorstOutcomeValid ? $"{WorstOutcomeLength}/10 characters (minimum 10 required)" :
            $"{WorstOutcomeLength} characters ✓";

        public string RootCauseValidationMessage =>
            !HasRootCause ? "Required" :
            !IsRootCauseValid ? $"{RootCauseLength}/10 characters (minimum 10 required)" :
            $"{RootCauseLength} characters ✓";

        public string ValidationCssClass =>
            IsComplete ? "is-valid" :
            (HasWorstOutcome || HasRootCause) ? "is-invalid" :
            "";
    }

    #endregion

    #region Route Properties

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public int StepNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string? HazardId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ReportId { get; set; }

    #endregion

    #region Core Data

    public RiskAssessment? InitialRiskAssessment { get; set; }

    public RiskAssessment? ResidualRiskAssessment { get; set; }

    public Hazard? PrimaryHazard { get; set; }
    public Report? SourceReport { get; set; }
    public AirportSharedDataset? SharedDataset { get; set; }
    public List<Hazard> ReportHazards { get; set; } = new();

    #endregion

    #region Step Models - Properties for UI Binding

    [BindProperty]
    public Step1Model Step1 { get; set; } = new();

    [BindProperty]
    public Step2Model Step2 { get; set; } = new();

    [BindProperty]
    public Step3Model Step3 { get; set; } = new();

    [BindProperty]
    public Step4Model Step4 { get; set; } = new();

    [BindProperty]
    public Step5Model Step5 { get; set; } = new();

    #endregion

    #region UI Helper Properties - CLEANED UP

    // Essential UI properties that the views expect

    public string InitialAssessmentName => GetInitialAssessmentName();

    public string ResidualAssessmentName => GetResidualAssessmentName();

    // ✅ FIXED: Add missing AssessmentName property
    public string AssessmentName => InitialAssessmentName;

    public string AssessmentId => Id;
    public string LeadAssessorName => AvailableAssessors.FirstOrDefault(a => a.Id.Value == Step1.LeadAssessor)?.DisplayName ?? Step1.LeadAssessor;
    public int CurrentStep => StepNumber;
    public string CurrentStepName => GetStepName(StepNumber);

    // NEW: Missing properties that the views are looking for
    /// <summary>
    /// Gets selected panel members from Step 4
    /// </summary>
    public List<string> SelectedPanelMembers => Step4?.SelectedPanelMembers ?? new List<string>();

    /// <summary>
    /// Gets completed scores from Step 4
    /// </summary>
    public List<PanelMemberScoreData> CompletedScores => Step4?.CompletedScores ?? new List<PanelMemberScoreData>();

    /// <summary>
    /// Gets pending scores from Step 4
    /// </summary>
    public List<PanelMemberScoreData> PendingScores => Step4?.PendingScores ?? new List<PanelMemberScoreData>();

    /// <summary>
    /// Gets panel scores dictionary from Step 4
    /// </summary>
    public Dictionary<string, List<PanelMemberScoreData>> PanelScores => Step4?.PanelScores ?? new Dictionary<string, List<PanelMemberScoreData>>();

    // ✅ FIXED: Add missing Step 4 properties
    /// <summary>
    /// Gets hazard panel members from Step 4
    /// </summary>
    public Dictionary<string, List<string>> HazardPanelMembers => Step4?.HazardPanelMembers ?? new Dictionary<string, List<string>>();

    /// <summary>
    /// Gets hazard average scores from Step 4
    /// </summary>
    public Dictionary<string, double> HazardAverageScores => Step4?.HazardAverageScores ?? new Dictionary<string, double>();

    /// <summary>
    /// Gets identified hazards count for progress display
    /// </summary>
    public int IdentifiedHazardsCount => ReportHazards?.Count ?? 0;

    // Step 5 compatibility properties (now that Step5 exists)
    public string SavedImplementationStrategy => Step5.ImplementationStrategy;
    public string SavedOverallTargetDate => Step5.OverallTargetDate?.ToString("yyyy-MM-dd") ?? string.Empty;
    public string SavedImplementationNotes => Step5.ImplementationNotes;
    public Dictionary<string, List<string>> SavedMitigationStrategies => Step5.SavedMitigationStrategies;

    #endregion

    #region Reference Data for UI

    public List<SMSApplicationUser> AvailableAssessors { get; set; } = new();
    public List<SMSStakeholderUser> AvailableStakeholders { get; set; } = new();
    public List<SMSApplicationUser> AvailableSMSUsers { get; set; }
    public List<SMSStakeholderGroup> StakeholderGroups { get; set; } = new();
    public List<string> SelectedStakeholderIds { get; set; } = new();
    
    /// <summary>
    /// CLARIFIED: This property returns all hazards available for analysis
    /// Includes the initial hazard + any additional hazards identified in Step 2
    /// This is what Step 3 uses for analysis - there should always be at least one hazard
    /// </summary>
    public List<Hazard> AvailableHazards => ReportHazards ?? new List<Hazard>();
    
    /// <summary>
    /// Gets the count of hazards available for analysis
    /// This should always be at least 1 (the initial hazard)
    /// </summary>
    public int AvailableHazardsCount => AvailableHazards.Count;
    
    public RiskAssessmentDataWrapper? AssessmentData { get; set; }
    
    /// <summary>
    /// DEPRECATED: Use AvailableHazardsCount instead
    /// </summary>
    public int Count => ReportHazards.Count;

    /// <summary>
    /// Helper property for UI compatibility - gets selected stakeholder group names
    /// </summary>
    public List<string> SelectedStakeholderGroupNames
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Step1?.StakeholderGroups))
                return new List<string>();

            return Step1.StakeholderGroups.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(g => g.Trim())
                .Where(g => !string.IsNullOrEmpty(g))
                .ToList();
        }
    }

    /// <summary>
    /// Helper property for UI compatibility - gets selected individual stakeholder data
    /// </summary>
    public List<object> SelectedIndividualStakeholderData
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Step1?.SelectedIndividualStakeholders))
                return new List<object>();

            try
            {
                return global::System.Text.Json.JsonSerializer.Deserialize<List<object>>(Step1.SelectedIndividualStakeholders) ?? new List<object>();
            }
            catch
            {
                return new List<object>();
            }
        }
    }

    /// <summary>
    /// Load stakeholder data - USE DOMAIN ENTITIES DIRECTLY
    /// </summary>
    private async Task LoadStakeholdersAsync()
    {
        try
        {
            _logger.LogInformation("Loading stakeholder data for Assessment {AssessmentId}", Id);

            // Load all active SMS Stakeholder Users
            var stakeholdersQuery = new GetActiveSMSStakeholderUsersQuery();
            var stakeholdersResult = await _mediator.SendAsync(stakeholdersQuery, CancellationToken.None);

            if (stakeholdersResult.IsSuccess && stakeholdersResult.Value != null)
            {
                AvailableStakeholders = stakeholdersResult.Value.ToList();
                _logger.LogInformation("Loaded {Count} stakeholder users", AvailableStakeholders.Count);
            }
            else
            {
                AvailableStakeholders = new List<SMSStakeholderUser>();
            }

            // Load Stakeholder Groups - USE DOMAIN ENTITY DIRECTLY
            var stakeholderGroupsQuery = new GetAllSMSStakeholderGroupsQuery();
            var stakeholderGroupsResult = await _mediator.SendAsync(stakeholderGroupsQuery, CancellationToken.None);

            if (stakeholderGroupsResult.IsSuccess && stakeholderGroupsResult.Value != null)
            {
                StakeholderGroups = stakeholderGroupsResult.Value.ToList();
                _logger.LogInformation("Loaded {GroupCount} stakeholder groups", StakeholderGroups.Count);
            }
            else
            {
                StakeholderGroups = new List<SMSStakeholderGroup>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading stakeholder data");
            AvailableStakeholders = new List<SMSStakeholderUser>();
            StakeholderGroups = new List<SMSStakeholderGroup>();
        }
    }

    #endregion

    #region SIMPLIFIED Validation Methods - Using Step Models

    /// <summary>
    /// Validates all steps in the wizard
    /// Simplified to use the Validate() methods of each step model in order
    /// </summary>
    public (bool isValid, string message) ValidateAllSteps()
    {
        // Step 1 validation
        var step1Validation = Step1.Validate();
        if (!step1Validation.isValid)
        {
            return (false, $"Step 1: {step1Validation.message}");
        }

        // Step 2 validation
        var step2Validation = Step2.Validate();
        if (!step2Validation.isValid)
        {
            return (false, $"Step 2: {step2Validation.message}");
        }

        // Step 3 validation
        var step3Validation = Step3.Validate(ReportHazards);
        if (!step3Validation.isValid)
        {
            return (false, $"Step 3: {step3Validation.message}");
        }

        // All steps valid
        return (true, "All steps are valid");
    }

    #endregion

    #region SIMPLIFIED Apply Methods - Using Step Models

    /// <summary>
    /// Applies all steps to the assessment
    /// Simplified to use the ApplyToAssessment() methods of each step model in order
    /// </summary>
    public void ApplyAllSteps(RiskAssessment assessment)
    {
        // Step 1
        Step1.ApplyToAssessment(assessment);

        // Step 2
        Step2.ApplyToAssessment(assessment);

        // Step 3
        Step3.ApplyToAssessment(assessment);

        // Step 4 - Risk Scoring Panel
        Step4.ApplyToAssessment(assessment);

        // Step 5 - Implementation Planning
        Step5.ApplyToAssessment(assessment);
    }

    #endregion

    #region Helper Classes - Shared for All Steps

    /// <summary>
    /// RENAMED: Better name for what this actually represents
    /// Wrapper for assessment data used across multiple steps
    /// Contains identified hazards and implementation planning data
    /// </summary>
    public class RiskAssessmentDataWrapper
    {
        public List<IdentifiedHazardDto> IdentifiedHazards { get; set; } = new();
        public string ImplementationPlan { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data Transfer Object for an identified hazard
    /// Used to pass hazard data between steps and to/from UI
    /// </summary>
    public class IdentifiedHazardDto
    {
        public string Id { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string WorstCredibleOutcome { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Likelihood { get; set; } = string.Empty;
        public string Tolerability { get; set; } = string.Empty;
        public List<string> ProposedMitigations { get; set; } = new();
        public List<string> CurrentMitigations { get; set; } = new();
    }

    #endregion

    #region UI Helper Methods

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
        return InitialRiskAssessment?.IsStepCompleted(targetStep - 1) ?? (targetStep == 1);
    }

    public int GetLastCompletedStep()
    {
        return InitialRiskAssessment?.CompletedSteps.LastOrDefault() ?? 0;
    }

    public int GetRecommendedStep()
    {
        return InitialRiskAssessment?.GetNextRecommendedStep() ?? 1;
    }

    public string GetInitialAssessmentName()
    {
        return InitialRiskAssessment?.Name ?? "Initial Risk Assessment";
    }
    public string GetResidualAssessmentName()
    {
        return ResidualRiskAssessment?.Name ?? "Residual Risk Assessment";
    }
    public string GetRiskLevelClass(string riskLevel)
    {
        return riskLevel?.ToLowerInvariant() switch
        {
            "high" or "5" or "4" => "bg-danger text-white",
            "medium" or "3" => "bg-warning text-dark",
            "low" or "2" or "1" => "bg-success text-white",
            _ => "bg-secondary text-white"
        };
    }

    #endregion



    #region Data Loading and Saving Methods

    /// <summary>
    /// Loads assessment data from the database using Application layer
    /// FIXED: Added missing LoadAssessmentDataAsync method
    /// </summary>
    private async Task LoadAssessmentDataAsync()
    {
        try
        {
            _logger.LogInformation("Loading assessment data for ID: {AssessmentId}", Id);

            // Use the enhanced method that guarantees assessment loading
            var loadResult = await EnsureAssessmentLoadedAsync();
            if (!loadResult.success)
            {
                _logger.LogError("Failed to load assessment: {Error}", loadResult.message);
                throw new InvalidOperationException($"Failed to load assessment: {loadResult.message}");
            }

            // Load related hazard data
            await LoadReportHazardsAsync();

            // Load step data from the assessment
            LoadStepDataFromAssessment();

            // Load reference data
            await LoadReferenceDataAsync();

            _logger.LogInformation("Successfully loaded assessment data for ID: {AssessmentId}", InitialRiskAssessment?.Id?.Value ?? "Unknown");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading assessment data for ID: {AssessmentId}", Id);
            throw;
        }
    }

    /// <summary>
    /// ENHANCED: Guaranteed Assessment Loading
    /// </summary>
    private async Task<(bool success, string message)> EnsureAssessmentLoadedAsync()
    {
        try
        {
            if (InitialRiskAssessment == null)
            {
                _logger.LogWarning("⚠️ Assessment is null, attempting to reload...");

                //if (string.IsNullOrEmpty(Id) || Id == "new")
                //{
                var getRiskAssessmentCommand = new GetRiskAssessmentsByHazardIdQuery(new HazardID(HazardId));
                var riskassessment = _mediator.SendAsync(getRiskAssessmentCommand, CancellationToken.None);
                var initialriskassessment = riskassessment.Result.Value.Where(x => x.AssessmentType == RiskAssessmentType.Initial).FirstOrDefault();
                if (initialriskassessment == null)
                {
                    return (false, "Failed to create new assessment");
                }
                InitialRiskAssessment = initialriskassessment;
                Id = InitialRiskAssessment.Code!; // Update route parameter
                //}
                //else
                //{
                //    // Load existing assessment
                //    var riskAssessmentId = new RiskAssessmentID(Id);
                //    var result = await _riskAssessmentService.GetRiskAssessmentByIdAsync(riskAssessmentId);

                //    if (result.IsSuccess && result.Value != null)
                //    {
                //        InitialRiskAssessment = result.Value;
                //    }
                //    else
                //    {
                //        return (false, $"Assessment {Id} not found");
                //    }
                //}
            }
            else
            {
                // Refresh assessment from database to get latest state
                //var riskAssessmentId = new RiskAssessmentID(Assessment.Code!);
                //var result = await _riskAssessmentService.GetRiskAssessmentByIdAsync(riskAssessmentId);

                //if (result.IsSuccess && result.Value != null)
                //{
                //    Assessment = result.Value;
                //    _logger.LogInformation("✅ Assessment refreshed successfully");
                //}
            }

            return (true, "Assessment loaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error ensuring assessment loaded");
            return (false, $"Assessment loading error: {ex.Message}");
        }
    }



    /// <summary>
    /// Saves assessment data to database using Application layer
    /// </summary>
    private async Task SaveAssessmentToDatabaseAsync()
    {
        try
        {
            if (InitialRiskAssessment == null)
            {
                _logger.LogWarning("Cannot save assessment data - Assessment is null");
                return;
            }

            _logger.LogInformation("Saving assessment data for ID: {AssessmentId}", InitialRiskAssessment.Id);

            // Use the enhanced RiskAssessmentService to save data

            var updateRiskAssessmentCommand = new UpdateRiskAssessmentCommand(InitialRiskAssessment);
            var updateResult = _mediator.SendAsync(updateRiskAssessmentCommand, new CancellationToken());


            if (updateResult.Result.IsSuccess)
            {
                // Update our cached assessment with the latest data
                InitialRiskAssessment = updateResult.Result.Value;
                _logger.LogInformation("Successfully saved assessment data for ID: {AssessmentId}", InitialRiskAssessment.Id);
            }
            else
            {
                _logger.LogError("Failed to save assessment data: {Error}", updateResult.Result.Error?.Message);
                throw new InvalidOperationException($"Failed to save assessment: {updateResult.Result.Error?.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving assessment data for ID: {AssessmentId}", InitialRiskAssessment?.Id?.Value ?? "Unknown");
            throw; // Re-throw so calling method knows save failed
        }
    }

    /// <summary>
    /// Loads step model data from the assessment entity
    /// </summary>
    private void LoadStepDataFromAssessment()
    {
        if (InitialRiskAssessment == null) return;

        try
        {
            // Load data into step models from assessment
            Step1.LoadFromAssessment(InitialRiskAssessment);
            Step2.LoadFromAssessment(InitialRiskAssessment); // ✅ ENABLED - now has overload
            Step3.LoadFromAssessment(InitialRiskAssessment, ReportHazards);
            Step4.LoadFromAssessment(InitialRiskAssessment); // If needed  
            // Step5.LoadFromAssessment(Assessment); // If needed

            // Initialize UI compatibility properties for stakeholder selection
            InitializeStakeholderUIProperties();

            _logger.LogInformation("✅ Step models loaded from assessment {AssessmentId}", InitialRiskAssessment.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error loading step models from assessment {AssessmentId}", InitialRiskAssessment?.Id?.Value ?? "Unknown");
        }
    }

    /// <summary>
    /// Initialize stakeholder UI properties for proper binding with the Razor view
    /// </summary>
    private void InitializeStakeholderUIProperties()
    {
        try
        {
            // Initialize SelectedStakeholderIds for UI compatibility
            SelectedStakeholderIds = new List<string>();

            // Add stakeholder groups to selected IDs
            if (!string.IsNullOrWhiteSpace(Step1?.StakeholderGroups))
            {
                var groupNames = Step1.StakeholderGroups.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(g => g.Trim())
                    .Where(g => !string.IsNullOrEmpty(g));

                SelectedStakeholderIds.AddRange(groupNames);
            }

            // Add individual stakeholder IDs
            if (!string.IsNullOrWhiteSpace(Step1?.SelectedIndividualStakeholders))
            {
                try
                {
                    var individuals = global::System.Text.Json.JsonSerializer.Deserialize<List<Step1Model.StakeholderSelection>>(Step1.SelectedIndividualStakeholders);
                    if (individuals?.Any() == true)
                    {
                        SelectedStakeholderIds.AddRange(individuals.Select(i => i.Id));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not parse individual stakeholder selections for UI");
                }
            }

            _logger.LogInformation("✅ Initialized stakeholder UI properties with {Count} selected stakeholders", SelectedStakeholderIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error initializing stakeholder UI properties");
            SelectedStakeholderIds = new List<string>();
        }
    }
    /// <summary>
    /// Load reference data needed for dropdowns and selections
    /// CRITICAL: DO NOT REMOVE THIS METHOD!!! IT IS REQUIRED FOR PROPER OPERATION!!!
    /// </summary>
    private async Task LoadReferenceDataAsync()
    {
        try
        {
            _logger.LogInformation("Loading reference data for Assessment {AssessmentId}", Id);

            // Load stakeholder data using the enhanced method
            await LoadStakeholdersAsync();

            // Load SMS Application Users for Step 4 scoring panels
            await LoadSMSApplicationUsersAsync();

            // Initialize empty lists for other reference data (can be enhanced later)
            AvailableAssessors = new List<SMSApplicationUser>();

            _logger.LogInformation("Reference data loaded successfully - {StakeholderCount} stakeholders, {GroupCount} groups, {SMSUserCount} SMS users",
                AvailableStakeholders.Count, StakeholderGroups.Count, AvailableSMSUsers?.Count ?? 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading reference data");
            AvailableAssessors = new List<SMSApplicationUser>();
            AvailableStakeholders = new List<SMSStakeholderUser>();
            StakeholderGroups = new List<SMSStakeholderGroup>();
            AvailableSMSUsers = new List<SMSApplicationUser>();
        }
    }

    /// <summary>
    /// Load SMS Application Users for Step 4 scoring panels
    /// </summary>
    private async Task LoadSMSApplicationUsersAsync()
    {
        try
        {
            _logger.LogInformation("Loading SMS Application Users for Step 4 scoring panels");

            // Load all active SMS Application Users
            var smsUsersQuery = new GetAllSMSApplicationUsersQuery();
            var smsUsersResult = await _mediator.SendAsync(smsUsersQuery, CancellationToken.None);

            if (smsUsersResult.IsSuccess && smsUsersResult.Value != null)
            {
                AvailableSMSUsers = smsUsersResult.Value.Where(u => u.IsActive).ToList();
                _logger.LogInformation("Loaded {Count} SMS Application Users for scoring panels", AvailableSMSUsers.Count);
            }
            else
            {
                _logger.LogWarning("Failed to load SMS Application Users or no users found");
                AvailableSMSUsers = new List<SMSApplicationUser>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading SMS Application Users");
            AvailableSMSUsers = new List<SMSApplicationUser>();
        }
    }
    /// <summary>
    /// Loads all hazards associated with this report for risk assessment
    /// This should ALWAYS include the initial hazard plus any additional hazards from Step 2
    /// </summary>
    private async Task LoadReportHazardsAsync()
    {
        try
        {
            _logger.LogInformation("Loading all report hazards for ReportId: {ReportId}, HazardId: {HazardId}", ReportId, HazardId);

            // Initialize empty list
            ReportHazards = new List<Hazard>();

            // STEP 1: Load ALL hazards for this report (includes initial + Step 2 hazards)
            if (!string.IsNullOrEmpty(ReportId))
            {
                var reportHazardsQuery = new GetHazardsByReportIdQuery(new ReportID(ReportId));
                var reportHazardsResult = await _mediator.SendAsync(reportHazardsQuery, CancellationToken.None);

                if (reportHazardsResult.IsSuccess && reportHazardsResult.Value.Any())
                {
                    ReportHazards = reportHazardsResult.Value.ToList();
                    _logger.LogInformation("Loaded {Count} hazards from report {ReportId}: {HazardCodes}", 
                        ReportHazards.Count, ReportId, string.Join(", ", ReportHazards.Select(h => h.Code)));

                    // Set PrimaryHazard to the one matching HazardId, or first one if not found
                    PrimaryHazard = ReportHazards.FirstOrDefault(h => h.Code == HazardId) ?? ReportHazards.First();
                }
            }

            // STEP 2: If we still don't have hazards, try loading just the primary hazard from HazardId
            if (!ReportHazards.Any() && !string.IsNullOrEmpty(HazardId))
            {
                _logger.LogWarning("No hazards found for report {ReportId}, trying to load primary hazard {HazardId}", ReportId, HazardId);
                
                var hazardQuery = new GetHazardByIdQuery(new HazardID(HazardId));
                var hazardResult = await _mediator.SendAsync(hazardQuery, CancellationToken.None);

                if (hazardResult.IsSuccess)
                {
                    PrimaryHazard = hazardResult.Value;
                    ReportHazards = new List<Hazard> { PrimaryHazard };
                    _logger.LogInformation("Loaded primary hazard as fallback: {HazardCode}", PrimaryHazard.Code);
                }
            }

            // STEP 3: Load source report if available
            if (PrimaryHazard != null && !string.IsNullOrEmpty(PrimaryHazard.ReportCode))
            {
                var reportQuery = new GetReportByIdQuery(new ReportID(PrimaryHazard.ReportCode));
                var reportResult = await _mediator.SendAsync(reportQuery, CancellationToken.None);
                if (reportResult.IsSuccess)
                {
                    SourceReport = reportResult.Value;
                }
            }

            _logger.LogInformation("Final report hazards loaded: {Count} hazards available for risk assessment", ReportHazards.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading report hazards for ReportId: {ReportId}, HazardId: {HazardId}", ReportId, HazardId);
            ReportHazards = new List<Hazard>();
        }
    }

    #endregion

    #region Page Handler Methods - Converted from JavaScript

    /// <summary>
    /// Handles "Save and Navigate Next" button click - using modular step save approach
    /// Much easier to debug and test individual steps
    /// </summary>
    /// <summary>
    /// ENHANCED: Save and Navigate with Guaranteed State Preservation
    /// </summary>
    public async Task<IActionResult> OnPostSaveAndNavigateNextAsync()
    {
        try
        {
            _logger.LogInformation("🚀 ENHANCED Save and Navigate: Step {CurrentStep} for Assessment {AssessmentId}",
                StepNumber, Id);

            // CRITICAL: Reload Assessment before save operation
            var reloadResult = await EnsureAssessmentLoadedAsync();
            if (!reloadResult.success)
            {
                TempData["ErrorMessage"] = $"Failed to reload assessment: {reloadResult.message}";
                return await ReloadPageWithErrorAsync();
            }

            // Validate current step
            var validationResult = ValidateCurrentStep();
            if (!validationResult.isValid)
            {
                TempData["ErrorMessage"] = $"Step {StepNumber} validation failed: {validationResult.message}";
                return await ReloadPageWithErrorAsync();
            }

            // Save current step
            var saveResult = await SaveCurrentStepAsync();
            if (!saveResult.success)
            {
                TempData["ErrorMessage"] = $"Step {StepNumber} save failed: {saveResult.message}";
                return await ReloadPageWithErrorAsync();
            }

            // Determine next step
            var nextStep = StepNumber + 1;
            if (nextStep > 5)
            {
                TempData["SuccessMessage"] = "Assessment completed successfully!";
                return RedirectToPage("/SafetyRiskManagement/RiskAssessment", new { assessmentId = Id });
            }

            // Navigate to next step with ALL parameters preserved
            TempData["SuccessMessage"] = saveResult.message;
            return RedirectToPage("/SafetyRiskManagement/RiskAssessmentWizard",
                new { id = InitialRiskAssessment.Code, stepNumber = nextStep, reportId = ReportId, hazardId = HazardId });

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in Save and Navigate Next for Assessment {AssessmentId}, Step {StepNumber}", Id, StepNumber);
            TempData["ErrorMessage"] = "An unexpected error occurred while saving. Please try again.";
            return await ReloadPageWithErrorAsync();
        }
    }

    /// <summary>
    /// Reload the current page with all data after an error
    /// </summary>
    private async Task<IActionResult> ReloadPageWithErrorAsync()
    {
        try
        {
            if (InitialRiskAssessment == null)
            {
                await EnsureAssessmentLoadedAsync();
            }
            await LoadReportHazardsAsync();
            LoadStepDataFromAssessment();
            await LoadReferenceDataAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reloading page data after error");
        }

        return Page();
    }

    /// <summary>
    /// Validate the current step based on the StepNumber
    /// </summary>
    private (bool isValid, string message) ValidateCurrentStep()
    {
        return StepNumber switch
        {
            1 => Step1?.Validate() ?? (false, "Step 1 data not available"),
            2 => Step2?.Validate() ?? (false, "Step 2 data not available"),
            3 => Step3?.Validate(ReportHazards) ?? (false, "Step 3 data not available"),
            4 => Step4?.Validate() ?? (false, "Step 4 data not available"),
            5 => Step5?.Validate() ?? (false, "Step 5 data not available"),
            _ => (false, "Invalid step number")
        };
    }

    /// <summary>
    /// Handles final assessment completion - converted from JavaScript
    /// </summary>
    public async Task<IActionResult> OnPostCompleteAssessmentAsync()
    {
        try
        {
            _logger.LogInformation("Complete Assessment: {AssessmentId}", Id);

            // Load current assessment data if needed
            if (InitialRiskAssessment == null)
            {
                await LoadAssessmentDataAsync();
            }

            // Validate all steps are complete
            var allStepsValidation = ValidateAllSteps();
            if (!allStepsValidation.isValid)
            {
                TempData["ErrorMessage"] = $"Assessment cannot be completed: {allStepsValidation.message}";
                return Page();
            }

            // Save final step and mark assessment as complete
            await SaveCurrentStepAsync();
            await CompleteAssessmentAsync();

            TempData["SuccessMessage"] = "Risk Assessment completed successfully!";
            return RedirectToPage("/SafetyRiskManagement/RiskAssessment", new { assessmentId = Id });

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing Assessment {AssessmentId}", Id);
            TempData["ErrorMessage"] = "An error occurred while completing the assessment. Please try again.";
            return Page();
        }
    }

    #endregion

    #region Step 4 Specific Page Handlers

    /// <summary>
    /// Assign scoring panel members to a specific hazard
    /// </summary>
    public async Task<IActionResult> OnPostAssignScoringPanelAsync(string hazardId, string userIds)
    {
        try
        {
            _logger.LogInformation("Assigning scoring panel to hazard {HazardId}: {UserIds}", hazardId, userIds);

            if (string.IsNullOrWhiteSpace(hazardId))
            {
                return new JsonResult(new { success = false, message = "Hazard ID is required" });
            }

            // Parse user IDs
            var memberIds = userIds?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => id.Trim())
                .Where(id => !string.IsNullOrEmpty(id))
                .ToList() ?? new List<string>();

            if (!memberIds.Any())
            {
                return new JsonResult(new { success = false, message = "At least one panel member is required" });
            }

            // Assign panel members to the hazard
            Step4.AssignPanelMembersToHazard(hazardId, memberIds);

            _logger.LogInformation("Successfully assigned {Count} panel members to hazard {HazardId}", memberIds.Count, hazardId);

            return new JsonResult(new
            {
                success = true,
                message = $"Assigned {memberIds.Count} panel members to hazard {hazardId}",
                hazardId = hazardId,
                memberCount = memberIds.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning scoring panel to hazard {HazardId}", hazardId);
            return new JsonResult(new { success = false, message = "Error assigning scoring panel: " + ex.Message });
        }
    }
    /// <summary>
    /// Submit individual panel member score for a hazard
    /// FIXED: Type consistency issue
    /// </summary>
    public async Task<IActionResult> OnPostSubmitScoreAsync(string hazardId, string memberId, int severityScore, int likelihoodScore)
    {
        try
        {
            _logger.LogInformation("Submitting score for hazard {HazardId}, member {MemberId}: {Severity}x{Likelihood}",
                hazardId, memberId, severityScore, likelihoodScore);

            if (string.IsNullOrWhiteSpace(hazardId) || string.IsNullOrWhiteSpace(memberId))
            {
                return new JsonResult(new { success = false, message = "Hazard ID and Member ID are required" });
            }

            if (severityScore < 1 || severityScore > 5 || likelihoodScore < 1 || likelihoodScore > 5)
            {
                return new JsonResult(new { success = false, message = "Scores must be between 1 and 5" });
            }

            // Create score data using the correct PanelMemberScoreData class
            var scoreData = new PanelMemberScoreData
            {
                HazardId = hazardId,
                MemberId = memberId,
                MemberName = AvailableSMSUsers?.FirstOrDefault(u => u.Id.Value == memberId)?.DisplayName ?? memberId,
                SeverityScore = severityScore,
                LikelihoodScore = likelihoodScore,
                SubmittedDate = DateTime.UtcNow
            };

            // Add score to Step 4 model - FIXED: Now uses the same type
            Step4.AddPanelMemberScore(hazardId, scoreData);

            var calculatedScore = scoreData.CalculatedScore;
            _logger.LogInformation("Score submitted successfully: {Score} for hazard {HazardId} by member {MemberId}",
                calculatedScore, hazardId, memberId);

            return new JsonResult(new
            {
                success = true,
                message = $"Score {calculatedScore} submitted successfully",
                hazardId = hazardId,
                memberId = memberId,
                score = calculatedScore,
                averageScore = Step4.HazardAverageScores.ContainsKey(hazardId) ? Step4.HazardAverageScores[hazardId] : (double?)null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting score for hazard {HazardId}, member {MemberId}", hazardId, memberId);
            return new JsonResult(new { success = false, message = "Error submitting score: " + ex.Message });
        }
    }
    /// <summary>
    /// Completes the entire assessment process
    /// </summary>
    private async Task CompleteAssessmentAsync()
    {
        try
        {
            if (InitialRiskAssessment == null)
            {
                _logger.LogWarning("Cannot complete assessment - Assessment is null");
                return;
            }

            // Apply all step data to ensure everything is saved
            ApplyAllSteps(InitialRiskAssessment);

            // Mark assessment as complete
            // TODO: Add method to RiskAssessment to mark as complete
            // Assessment.MarkAsComplete();

            // Save final state to database
            await SaveAssessmentToDatabaseAsync();

            _logger.LogInformation("Assessment {AssessmentId} completed successfully", Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing Assessment {AssessmentId}", Id);
            throw;
        }
    }

    #endregion

    #region Step-Specific Handlers - Modular Approach

    /// <summary>
    /// Creates a new hazard and persists it to the database using CreateHazardCommand
    /// </summary>
    public async Task<IActionResult> OnPostCreateHazardAsync(
        string hazardDescription,
        string hazardCategory)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(hazardDescription) || hazardDescription.Length < 10)
            {
                TempData["ErrorMessage"] = "Hazard description must be at least 10 characters.";
                return RedirectToPage(new { id = Id, stepNumber = StepNumber, reportId = ReportId, hazardId = HazardId });
            }

            // Generate hazard ID
            var hazardCounter = Step2.HazardIds.Count + 1;
            var hazardId = $"HZ-0000";
            HazardID hazardid = new HazardID(hazardId);
            Hazard hazard = new Hazard(hazardid);
            hazard.Description = hazardDescription;
            hazard.Category = hazardCategory;
            hazard.ReportCode = ReportId;
            hazard.CreatedBy = "WIZARD_USER";
            hazard.ReportedBy = "New Text Area";

            // Create the command
            var createCommand = new CreateHazardCommand(hazard);

            var result = await _mediator.SendAsync(createCommand, CancellationToken.None);


            if (result.IsSuccess)
            {
                hazard = result.Value;
                // Add to Step2 model
                Step2.HazardIds.Add(hazard.Code);
                Step2.HazardDescriptions.Add(hazard.Description);
                Step2.HazardCategories.Add(hazard.Category ?? "");

                TempData["SuccessMessage"] = $"Hazard {hazardid.Value} created successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = $"Failed to create hazard: {result.Error?.Message}";
            }

            return RedirectToPage(new { id = Id, stepNumber = StepNumber, reportId = ReportId, hazardId = HazardId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating hazard");
            TempData["ErrorMessage"] = "An error occurred while creating the hazard.";
            return RedirectToPage(new { id = Id, stepNumber = StepNumber, reportId = ReportId, hazardId = HazardId });
        }
    }

    #endregion

    #region Page Load and Initialization

    /// <summary>
    /// Handles GET requests - loads assessment data and initializes step models
    /// ENHANCED: OnGetAsync with guaranteed Assessment loading
    /// </summary>
    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            _logger.LogInformation("🚀 ENHANCED Loading Risk Assessment Wizard - Step {StepNumber}, Assessment {AssessmentId}, ReportId : {reportid}, HazardId: {HazardId}",
                StepNumber, Id, ReportId, HazardId);

            //ReportID reportid = new ReportID(ReportId);
            //var reportcommand = new GetReportByIdQuery(reportid);
            //var reportResult = await _mediator.SendAsync(reportcommand, CancellationToken.None);

            var allRiskAssessmentCommand = new GetRiskAssessmentsByHazardIdQuery(new HazardID(HazardId));
            var allRiskAssessmentResult = await _mediator.SendAsync(allRiskAssessmentCommand, new CancellationToken());

            if (allRiskAssessmentResult.IsSuccess)
            {
                InitialRiskAssessment = allRiskAssessmentResult.Value.Where(x => x.AssessmentType == RiskAssessmentType.Initial).FirstOrDefault();
                ResidualRiskAssessment = allRiskAssessmentResult.Value.Where(x => x.AssessmentType == RiskAssessmentType.Residual).FirstOrDefault();
            }



            // Validate step number
            if (StepNumber < 1 || StepNumber > 5)
            {
                _logger.LogWarning("Invalid step number {StepNumber}, redirecting to step 1", StepNumber);
                return RedirectToPage("/SafetyRiskManagement/RiskAssessmentWizard",
                    new { id = InitialRiskAssessment.Code, stepNumber = 1, reportId = ReportId, hazardId = HazardId });
            }

            // STEP 1: Ensure Assessment is loaded - GUARANTEED SUCCESS
            //var loadResult = await EnsureAssessmentLoadedAsync();
            //if (!loadResult.success)
            //{
            //    _logger.LogError("❌ Failed to load assessment: {Error}", loadResult.message);
            //    TempData["ErrorMessage"] = loadResult.message;
            //    return RedirectToPage("/SafetyRiskManagement/RiskAssessment");
            //}

            // STEP 2: Load Related Hazard Data  
            await LoadReportHazardsAsync();

            // STEP 3: Load Step Models with Current Data
            LoadStepDataFromAssessment();

            // STEP 4: Load Reference Data for UI
            await LoadReferenceDataAsync();

            _logger.LogInformation("✅ Successfully loaded Risk Assessment Wizard - Assessment: {Name}, Step: {Step}",
                InitialRiskAssessment.Name, StepNumber);

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Critical error loading Risk Assessment Wizard");
            TempData["ErrorMessage"] = "Unable to load the assessment wizard. Please try again.";
            return RedirectToPage("/SafetyRiskManagement/RiskAssessment");
        }
    }

    #endregion

    #region STAKEHOLDER MANAGEMENT POST HANDLERS

    /// <summary>
    /// Create new stakeholder from Step 1 modal
    /// Handles stakeholder creation and returns to the current step
    /// ENHANCED: Added comprehensive validation and logging
    /// </summary>
    public async Task<IActionResult> OnPostCreateStakeholderAsync(
        string stakeholderName,
        string stakeholderEmail,
        string stakeholderOrganization,
        string stakeholderGroup,
        string stakeholderType,
        string stakeholderCategory,
        string stakeholderPhone)
    {
        try
        {
            _logger.LogInformation("🎯 Creating new stakeholder: {Name} from {Organization} for type {Type}", 
                stakeholderName, stakeholderOrganization, stakeholderType);

            // Validate required fields
            if (string.IsNullOrWhiteSpace(stakeholderName))
            {
                TempData["ErrorMessage"] = "Stakeholder name is required";
                return await ReloadCurrentStepAsync();
            }

            if (string.IsNullOrWhiteSpace(stakeholderOrganization))
            {
                TempData["ErrorMessage"] = "Organization is required";
                return await ReloadCurrentStepAsync();
            }

            if (string.IsNullOrWhiteSpace(stakeholderType))
            {
                TempData["ErrorMessage"] = "Stakeholder type is required";
                return await ReloadCurrentStepAsync();
            }

            // Create the stakeholder user using the CreateSMSStakeholderUserCommand
            var stakeholderId = new SMSStakeholderUserID($"SU-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}");
            
            // Parse name into first and last name
            var nameParts = stakeholderName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var firstName = nameParts.Length > 0 ? nameParts[0] : stakeholderName;
            var lastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "";

            var stakeholder = new SMSStakeholderUser(stakeholderId)
            {
                Code = stakeholderId.Value,
                FirstName = FirstName.Create(firstName).Value,
                LastName = LastName.Create(lastName).Value,
                UserName = UserName.Create(stakeholderEmail ?? $"{firstName.ToLower()}.{lastName.ToLower()}@temp.com").Value,
                Password = Password.Create("TempPass123!").Value, // Temporary password
                Organization = stakeholderOrganization,
                StakeholderType = stakeholderType,
                IsActive = true,
                SMSUserType = "Stakeholder",
                CreatedBy = "RISK_ASSESSMENT_WIZARD",
                CreatedDate = DateTime.UtcNow
            };

            var createCommand = new CreateSMSStakeholderUserCommand(stakeholder);
            var result = await _mediator.SendAsync(createCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = $"Stakeholder '{stakeholderName}' created successfully and is available for selection.";
                _logger.LogInformation("✅ Successfully created stakeholder: {StakeholderCode}", stakeholder.Code);
                
                // Auto-select the new stakeholder if Step1 data is available
                if (Step1 != null && !string.IsNullOrWhiteSpace(stakeholderType))
                {
                    var currentGroups = Step1.StakeholderGroups?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(g => g.Trim()).ToList() ?? new List<string>();

                    if (!currentGroups.Contains(stakeholderType))
                    {
                        currentGroups.Add(stakeholderType);
                        Step1.StakeholderGroups = string.Join(", ", currentGroups);
                        _logger.LogInformation("Auto-selected stakeholder type '{Type}' for new stakeholder", stakeholderType);
                    }
                }
            }
            else
            {
                TempData["ErrorMessage"] = $"Failed to create stakeholder: {result.Error?.Message}";
                _logger.LogError("Failed to create stakeholder: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creating stakeholder: {Name}", stakeholderName);
            TempData["ErrorMessage"] = $"Error creating stakeholder: {ex.Message}";
        }

        return await ReloadCurrentStepAsync();
    }

    /// <summary>
    /// Helper method to reload the current step with all data
    /// </summary>
    private async Task<IActionResult> ReloadCurrentStepAsync()
    {
        try
        {
            // Ensure assessment is loaded
            if (InitialRiskAssessment == null)
            {
                await EnsureAssessmentLoadedAsync();
            }
            
            // Reload related data
            await LoadReportHazardsAsync();
            LoadStepDataFromAssessment();
            await LoadReferenceDataAsync(); // This will reload the updated stakeholder list
            
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reloading current step");
            return RedirectToPage("/SafetyRiskManagement/RiskAssessmentWizard",
                new { id = Id, stepNumber = StepNumber, hazardId = HazardId, reportId = ReportId });
        }
    }

    #endregion

    #region Step Models Definitions

    public class Step1Model
    {
        #region System Overview Properties

        [Required(ErrorMessage = "Lead Assessor is required.")]
        [Display(Name = "Lead Assessor")]
        public string LeadAssessor { get; set; } = string.Empty;

        [Required(ErrorMessage = "System Description is required.")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "System Description must be between 10 and 1000 characters.")]
        [Display(Name = "System Description")]
        public string SystemDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "System Boundaries are required.")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "System Boundaries must be between 10 and 1000 characters.")]
        [Display(Name = "System Boundaries")]
        public string SystemBoundaries { get; set; } = string.Empty;

        [Required(ErrorMessage = "System Purpose is required.")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "System Purpose must be between 10 and 1000 characters.")]
        [Display(Name = "System Purpose")]
        public string SystemPurpose { get; set; } = string.Empty;

        #endregion

        #region 5M Framework Properties

        [Required(ErrorMessage = "Personnel Factors (5M People) are required.")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Personnel Factors must be between 10 and 1000 characters.")]
        [Display(Name = "Personnel Factors")]
        public string FiveMPersonnel { get; set; } = string.Empty;

        [Required(ErrorMessage = "Equipment Factors (5M Equipment) are required.")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Equipment Factors must be between 10 and 1000 characters.")]
        [Display(Name = "Equipment Factors")]
        public string FiveMEquipment { get; set; } = string.Empty;

        [Required(ErrorMessage = "Procedure Factors (5M Procedures) are required.")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Procedure Factors must be between 10 and 1000 characters.")]
        [Display(Name = "Procedure Factors")]
        public string FiveMProcedures { get; set; } = string.Empty;

        [Required(ErrorMessage = "Resource Factors (5M Resources) are required.")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Resource Factors must be between 10 and 1000 characters.")]
        [Display(Name = "Resource Factors")]
        public string FiveMResources { get; set; } = string.Empty;

        [Required(ErrorMessage = "Physical Environment Factors (5M Environment) are required.")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Physical Environment must be between 10 and 1000 characters.")]
        [Display(Name = "Physical Environment")]
        public string FiveMPhysicalEnvironment { get; set; } = string.Empty;

        [Required(ErrorMessage = "Operational Environment Factors are required.")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Operational Environment must be between 10 and 1000 characters.")]
        [Display(Name = "Operational Environment")]
        public string FiveMOperationalEnvironment { get; set; } = string.Empty;

        #endregion

        #region Stakeholder Properties

        [Display(Name = "Stakeholder Groups")]
        public string StakeholderGroups { get; set; } = string.Empty;

        [Display(Name = "Individual Stakeholders")]
        public string SelectedIndividualStakeholders { get; set; } = string.Empty;

        #endregion

        #region Helper Classes for Stakeholder Selection

        public class StakeholderSelection
        {
            public string Id { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Organization { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
        }

        #endregion

        #region Step 1 Methods

        public (bool isValid, string message) Validate()
        {
            var step1Fields = new Dictionary<string, string>
        {
            { nameof(LeadAssessor), LeadAssessor },
            { nameof(SystemDescription), SystemDescription },
            { nameof(SystemBoundaries), SystemBoundaries },
            { nameof(SystemPurpose), SystemPurpose },
            { nameof(FiveMPersonnel), FiveMPersonnel },
            { nameof(FiveMEquipment), FiveMEquipment },
            { nameof(FiveMProcedures), FiveMProcedures },
            { nameof(FiveMResources), FiveMResources },
            { nameof(FiveMPhysicalEnvironment), FiveMPhysicalEnvironment },
            { nameof(FiveMOperationalEnvironment), FiveMOperationalEnvironment }
        };

            int fieldsWithData = 0;
            var missingFields = new List<string>();

            foreach (var field in step1Fields)
            {
                var value = field.Value?.Trim() ?? string.Empty;

                if (string.IsNullOrEmpty(value))
                {
                    missingFields.Add(field.Key);
                }
                else if (value.Length >= 10)
                {
                    fieldsWithData++;
                }
            }

            if (fieldsWithData < 4)
            {
                return (false, $"Need at least 4 complete fields (found {fieldsWithData}). Missing: {string.Join(", ", missingFields)}");
            }

            return (true, $"Step 1 validation passed with {fieldsWithData} complete fields");
        }

        public void ApplyToAssessment(RiskAssessment assessment)
        {
            assessment.SystemDescription = SystemDescription.Trim();
            assessment.SystemBoundaries = SystemBoundaries.Trim();
            assessment.SystemPurpose = SystemPurpose.Trim();
            assessment.FiveMPersonnel = FiveMPersonnel.Trim();
            assessment.FiveMEquipment = FiveMEquipment.Trim();
            assessment.FiveMProcedures = FiveMProcedures.Trim();
            assessment.FiveMResources = FiveMResources.Trim();
            assessment.FiveMOperationalEnvironment = FiveMOperationalEnvironment.Trim();
            assessment.FiveMPhysicalEnvironment = FiveMPhysicalEnvironment.Trim();

            assessment.LeadAssessorId = LeadAssessor.Trim();

            if (!string.IsNullOrEmpty(StakeholderGroups.Trim()))
            {
                var groups = StakeholderGroups.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(g => g.Trim())
                    .Where(g => !string.IsNullOrEmpty(g));

                foreach (var group in groups)
                {
                    assessment.AddStakeholder(group);
                }
            }

            assessment.CompleteStep(1);
        }

        public void LoadFromAssessment(RiskAssessment assessment)
        {
            if (assessment == null) return;

            if (string.IsNullOrEmpty(LeadAssessor)) LeadAssessor = assessment.LeadAssessorId ?? string.Empty;
            if (string.IsNullOrEmpty(SystemDescription)) SystemDescription = assessment.SystemDescription ?? string.Empty;
            if (string.IsNullOrEmpty(SystemBoundaries)) SystemBoundaries = assessment.SystemBoundaries ?? string.Empty;
            if (string.IsNullOrEmpty(SystemPurpose)) SystemPurpose = assessment.SystemPurpose ?? string.Empty;

            if (string.IsNullOrEmpty(FiveMPersonnel)) FiveMPersonnel = assessment.FiveMPersonnel ?? string.Empty;
            if (string.IsNullOrEmpty(FiveMEquipment)) FiveMEquipment = assessment.FiveMEquipment ?? string.Empty;
            if (string.IsNullOrEmpty(FiveMProcedures)) FiveMProcedures = assessment.FiveMProcedures ?? string.Empty;
            if (string.IsNullOrEmpty(FiveMResources)) FiveMResources = assessment.FiveMResources ?? string.Empty;
            if (string.IsNullOrEmpty(FiveMPhysicalEnvironment)) FiveMPhysicalEnvironment = assessment.FiveMPhysicalEnvironment ?? string.Empty;
            if (string.IsNullOrEmpty(FiveMOperationalEnvironment)) FiveMOperationalEnvironment = assessment.FiveMOperationalEnvironment ?? string.Empty;
        }

        #endregion
    }

    public class Step2Model
    {
        #region Hazard Properties

        public List<string> HazardIds { get; set; } = new();
        public List<string> HazardDescriptions { get; set; } = new();
        public List<string> HazardCategories { get; set; } = new();

        #endregion

        #region Step 2 Methods

        public (bool isValid, string message) Validate()
        {
            var validHazards = HazardDescriptions.Where(h => !string.IsNullOrWhiteSpace(h)).Count();

            if (validHazards < 1)
            {
                return (false, "At least 1 hazard is required");
            }

            return (true, $"Step 2 validation passed with {validHazards} hazards");
        }

        public void LoadFromAssessment(RiskAssessment assessment)
        {
            if (assessment == null) return;

            HazardIds = assessment.IdentifiedHazardIds.ToList();
            HazardDescriptions = assessment.IdentifiedHazardIds.Select(id => $"Hazard {id}").ToList();
            HazardCategories = assessment.IdentifiedHazardIds.Select(_ => string.Empty).ToList();
        }

        public void ApplyToAssessment(RiskAssessment assessment)
        {
            assessment.ClearIdentifiedHazards();

            for (int i = 0; i < HazardIds.Count && i < HazardDescriptions.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(HazardDescriptions[i]))
                {
                    assessment.AddIdentifiedHazard(HazardIds[i], HazardDescriptions[i]);
                }
            }

            assessment.CompleteStep(2);
        }

        #endregion
    }

    public class Step3Model
    {
        #region Risk Analysis Method Properties

        public string RiskAnalysisMethod { get; set; } = "SMS Risk Matrix";
        public string RiskCriteria { get; set; } = string.Empty;

        #endregion

        #region Multiple Hazard Risk Analysis Properties

        public Dictionary<string, HazardRiskAnalysis> HazardAnalyses { get; set; } = new();

        #endregion

        #region Step 3 Methods

        public (bool isValid, string message) Validate(List<Hazard> availableHazards = null)
        {
            // ENHANCED: Better validation with clear error messages
            if (availableHazards == null || !availableHazards.Any())
            {
                return (false, "No hazards available for risk analysis. There should always be at least one initial hazard. Please check hazard loading.");
            }

            var incompleteHazards = new List<string>();
            var analysisCount = 0;
            var totalHazards = availableHazards.Count;

           // _logger?.LogInformation("Validating Step 3 with {TotalHazards} available hazards", totalHazards);

            foreach (var hazard in availableHazards)
            {
                var hazardCode = hazard.Code;
               // _logger?.LogDebug("Validating hazard analysis for: {HazardCode}", hazardCode);

                if (HazardAnalyses.TryGetValue(hazardCode, out var analysis))
                {
                    var worstOutcomeValid = !string.IsNullOrWhiteSpace(analysis.WorstCredibleOutcome) && analysis.WorstCredibleOutcome.Length >= 10;
                    var rootCauseValid = !string.IsNullOrWhiteSpace(analysis.RootCauseAnalysis) && analysis.RootCauseAnalysis.Length >= 10;

                    if (!worstOutcomeValid && !rootCauseValid)
                    {
                        incompleteHazards.Add($"{hazardCode} (missing both worst outcome and root cause analysis)");
                    }
                    else if (!worstOutcomeValid)
                    {
                        incompleteHazards.Add($"{hazardCode} (worst credible outcome incomplete: {analysis.WorstCredibleOutcome?.Length ?? 0}/10 characters)");
                    }
                    else if (!rootCauseValid)
                    {
                        incompleteHazards.Add($"{hazardCode} (root cause analysis incomplete: {analysis.RootCauseAnalysis?.Length ?? 0}/10 characters)");
                    }
                    else
                    {
                        analysisCount++;
                      //  _logger?.LogDebug("Hazard {HazardCode} analysis complete", hazardCode);
                    }
                }
                else
                {
                    incompleteHazards.Add($"{hazardCode} (no analysis data found)");
                }
            }

            if (incompleteHazards.Any())
            {
                var message = $"Risk analysis incomplete for {incompleteHazards.Count}/{totalHazards} hazards: {string.Join("; ", incompleteHazards)}";
               // _logger?.LogWarning("Step 3 validation failed: {Message}", message);
                return (false, message);
            }

            var successMessage = $"Step 3 validation passed - {analysisCount}/{totalHazards} hazards have complete risk analysis";
          //  _logger?.LogInformation("Step 3 validation successful: {Message}", successMessage);
            return (true, successMessage);
        }

        public HazardRiskAnalysis GetHazardAnalysis(string hazardCode)
        {
            if (string.IsNullOrEmpty(hazardCode))
            {
                return new HazardRiskAnalysis { HazardId = hazardCode ?? string.Empty };
            }

            if (!HazardAnalyses.ContainsKey(hazardCode))
            {
                HazardAnalyses[hazardCode] = new HazardRiskAnalysis
                {
                    HazardId = hazardCode,
                    HazardDescription = $"Analysis for {hazardCode}",
                    HazardCategory = "General"
                };
            }

            return HazardAnalyses[hazardCode];
        }

        public void InitializeHazardAnalyses(List<Hazard> availableHazards)
        {
            if (availableHazards == null) return;

            foreach (var hazard in availableHazards)
            {
                if (!HazardAnalyses.ContainsKey(hazard.Code))
                {
                    HazardAnalyses[hazard.Code] = new HazardRiskAnalysis
                    {
                        HazardId = hazard.Code,
                        HazardDescription = hazard.Description,
                        HazardCategory = hazard.HazardType
                    };
                }
            }
        }

        public void ApplyToAssessment(RiskAssessment assessment)
        {
            assessment.CompleteStep(3);
        }

        public void LoadFromAssessment(RiskAssessment assessment, List<Hazard> reportHazards)
        {
            if (assessment == null) return;

            if (string.IsNullOrEmpty(RiskAnalysisMethod) && !string.IsNullOrEmpty(assessment.RiskAnalysisMethod))
            {
                RiskAnalysisMethod = assessment.RiskAnalysisMethod;
            }

            if (string.IsNullOrEmpty(RiskCriteria) && !string.IsNullOrEmpty(assessment.RiskCriteria))
            {
                RiskCriteria = assessment.RiskCriteria;
            }

            InitializeHazardAnalyses(reportHazards);
        }

        #endregion
    }

    public class Step4Model
    {
        #region Risk Assessment Properties

        public string TolerabilityFramework { get; set; } = "PDX-SMS Default";
        public string RiskAcceptanceCriteria { get; set; } = string.Empty;

        #endregion

        #region Scoring Panel Properties

        public List<string> SelectedPanelMembers { get; set; } = new();
        public Dictionary<string, List<string>> HazardPanelMembers { get; set; } = new();
        public Dictionary<string, List<PanelMemberScoreData>> PanelScores { get; set; } = new();
        public Dictionary<string, double> HazardAverageScores { get; set; } = new();
        public Dictionary<string, string> HazardRiskLevels { get; set; } = new();

        #endregion

        #region Helper Properties for UI

        public List<PanelMemberScoreData> CompletedScores
        {
            get
            {
                return PanelScores.Values
                    .SelectMany(scores => scores)
                    .Where(score => score.IsComplete)
                    .ToList();
            }
        }

        public List<PanelMemberScoreData> PendingScores
        {
            get
            {
                var allExpectedScores = new List<PanelMemberScoreData>();

                foreach (var hazardPanelKvp in HazardPanelMembers)
                {
                    var hazardId = hazardPanelKvp.Key;
                    var panelMemberIds = hazardPanelKvp.Value;

                    foreach (var memberId in panelMemberIds)
                    {
                        var existingScore = PanelScores.ContainsKey(hazardId)
                            ? PanelScores[hazardId].FirstOrDefault(s => s.MemberId == memberId)
                            : null;

                        if (existingScore == null || !existingScore.IsComplete)
                        {
                            allExpectedScores.Add(new PanelMemberScoreData
                            {
                                HazardId = hazardId,
                                MemberId = memberId,
                                MemberName = memberId
                            });
                        }
                    }
                }

                return allExpectedScores;
            }
        }

        #endregion

        #region Step 4 Methods

        public (bool isValid, string message) Validate()
        {
            if (string.IsNullOrWhiteSpace(TolerabilityFramework))
            {
                return (false, "Tolerability framework is required");
            }

            return (true, "Step 4 validation passed");
        }

        public void ApplyToAssessment(RiskAssessment assessment)
        {
            assessment.CompleteStep(4);
        }

        public void LoadFromAssessment(RiskAssessment assessment)
        {
            if (assessment == null) return;

            if (string.IsNullOrEmpty(TolerabilityFramework))
            {
                TolerabilityFramework = "PDX-SMS Default";
            }
        }

        public void AddPanelMemberScore(string hazardId, PanelMemberScoreData score)
        {
            if (!PanelScores.ContainsKey(hazardId))
            {
                PanelScores[hazardId] = new List<PanelMemberScoreData>();
            }

            PanelScores[hazardId].RemoveAll(s => s.MemberId == score.MemberId);
            PanelScores[hazardId].Add(score);
            RecalculateHazardAverage(hazardId);
        }

        private void RecalculateHazardAverage(string hazardId)
        {
            if (!PanelScores.ContainsKey(hazardId))
            {
                return;
            }

            var completedScores = PanelScores[hazardId].Where(s => s.IsComplete).ToList();
            if (completedScores.Any())
            {
                var average = completedScores.Average(s => s.CalculatedScore);
                HazardAverageScores[hazardId] = average;
                HazardRiskLevels[hazardId] = DetermineRiskLevel(average);
            }
            else
            {
                HazardAverageScores.Remove(hazardId);
                HazardRiskLevels.Remove(hazardId);
            }
        }

        private string DetermineRiskLevel(double score)
        {
            return score switch
            {
                >= 15 => "High",
                >= 8 => "Medium",
                _ => "Low"
            };
        }

        public void AssignPanelMembersToHazard(string hazardId, List<string> memberIds)
        {
            HazardPanelMembers[hazardId] = memberIds.ToList();
        }

        #endregion
    }

    public class Step5Model
    {
        #region Risk Mitigation Properties

        public string ImplementationStrategy { get; set; } = string.Empty;
        public DateTime? OverallTargetDate { get; set; }
        public string ImplementationNotes { get; set; } = string.Empty;
        public Dictionary<string, List<string>> SavedMitigationStrategies { get; set; } = new();

        #endregion

        #region Panel-Related Properties for Residual Risk Assessment

        public Dictionary<string, List<string>> HazardPanelMembers { get; set; } = new();
        public Dictionary<string, List<PanelMemberScoreData>> PanelScores { get; set; } = new();
        public Dictionary<string, double> HazardAverageScores { get; set; } = new();

        #endregion

        #region Step 5 Methods

        public (bool isValid, string message) Validate()
        {
            bool hasImplementation = !string.IsNullOrWhiteSpace(ImplementationStrategy);

            if (!hasImplementation)
            {
                return (false, "Implementation strategy is required");
            }

            return (true, "Step 5 validation passed");
        }

        public void ApplyToAssessment(RiskAssessment assessment)
        {
            assessment.CompleteStep(5);
        }

        #endregion
    }


    #endregion

    #region ✅ BLAZOR NAVIGATION HANDLERS

    /// <summary>
    /// Handles navigation to a specific step from Blazor components
    /// </summary>
    public async Task<IActionResult> OnPostNavigateToStepAsync(int targetStep)
    {
        try
        {
            _logger.LogInformation("🔄 Navigation requested to step {TargetStep} from step {CurrentStep}",
                targetStep, StepNumber);

            // Validate target step
            if (targetStep < 1 || targetStep > 5)
            {
                TempData["ErrorMessage"] = "Invalid step number.";
                return Page();
            }

            // Save current step before navigation
            if (StepNumber != targetStep)
            {
                var saveResult = await SaveCurrentStepInternalAsync();
                if (!saveResult.success)
                {
                    TempData["ErrorMessage"] = $"Failed to save current step: {saveResult.message}";
                    return Page();
                }
            }

            // Navigate to target step
            return RedirectToPage("/SafetyRiskManagement/RiskAssessmentWizard",
                new { id = InitialRiskAssessment?.Code ?? Id, stepNumber = targetStep, reportId = ReportId, hazardId = HazardId });

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error navigating to step {TargetStep}", targetStep);
            TempData["ErrorMessage"] = "Navigation error occurred. Please try again.";
            return Page();
        }
    }

    /// <summary>
    /// Handles manual save requests from UI
    /// </summary>
    public async Task<IActionResult> OnPostManualSaveCurrentStepAsync()
    {
        try
        {
            _logger.LogInformation("💾 Manual save requested for Step {StepNumber}", StepNumber);

            var saveResult = await SaveCurrentStepInternalAsync();

            if (saveResult.success)
            {
                TempData["SuccessMessage"] = saveResult.message;
            }
            else
            {
                TempData["ErrorMessage"] = saveResult.message;
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during manual save for Step {StepNumber}", StepNumber);
            TempData["ErrorMessage"] = "Save operation failed. Please try again.";
            return Page();
        }
    }

    /// <summary>
    /// Handles previous step navigation
    /// </summary>
    public async Task<IActionResult> OnPostPreviousStepAsync()
    {
        if (StepNumber > 1)
        {
            return await OnPostNavigateToStepAsync(StepNumber - 1);
        }

        return Page();
    }

    /// <summary>
    /// Internal step save method used by navigation handlers
    /// </summary>
    private async Task<(bool success, string message)> SaveCurrentStepInternalAsync()
    {
        return await SaveCurrentStepAsync();
    }

    #endregion

    #region Supporting Methods for Page Handlers

    /// <summary>
    /// Generic step save dispatcher - calls the appropriate step save method
    /// </summary>
    private async Task<(bool success, string message)> SaveCurrentStepAsync()
    {
        return StepNumber switch
        {
            1 => await SaveStep1Async(),
            2 => await SaveStep2Async(),
            3 => await SaveStep3Async(),
            4 => await SaveStep4Async(),
            5 => await SaveStep5Async(),
            _ => (false, $"Invalid step number: {StepNumber}")
        };
    }

    /// <summary>
    /// Saves Step 1 data
    /// </summary>
    private async Task<(bool success, string message)> SaveStep1Async()
    {
        try
        {
            Step1.ApplyToAssessment(InitialRiskAssessment);
            await SaveAssessmentToDatabaseAsync();
            return (true, "Step 1 saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving Step 1");
            return (false, $"Error saving Step 1: {ex.Message}");
        }
    }

    /// <summary>
    /// Saves Step 2 data
    /// </summary>
    private async Task<(bool success, string message)> SaveStep2Async()
    {
        try
        {
            Step2.ApplyToAssessment(InitialRiskAssessment);
            await SaveAssessmentToDatabaseAsync();
            return (true, "Step 2 saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving Step 2");
            return (false, $"Error saving Step 2: {ex.Message}");
        }
    }

    /// <summary>
    /// Saves Step 3 data
    /// </summary>
    private async Task<(bool success, string message)> SaveStep3Async()
    {
        try
        {
            Step3.ApplyToAssessment(InitialRiskAssessment);
            await SaveAssessmentToDatabaseAsync();
            return (true, "Step 3 saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving Step 3");
            return (false, $"Error saving Step 3: {ex.Message}");
        }
    }

    /// <summary>
    /// Saves Step 4 data
    /// </summary>
    private async Task<(bool success, string message)> SaveStep4Async()
    {
        try
        {
            Step4.ApplyToAssessment(InitialRiskAssessment);
            await SaveAssessmentToDatabaseAsync();
            return (true, "Step 4 saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving Step 4");
            return (false, $"Error saving Step 4: {ex.Message}");
        }
    }

    /// <summary>
    /// Saves Step 5 data
    /// </summary>
    private async Task<(bool success, string message)> SaveStep5Async()
    {
        try
        {
            Step5.ApplyToAssessment(InitialRiskAssessment);
            await SaveAssessmentToDatabaseAsync();
            return (true, "Step 5 saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving Step 5");
            return (false, $"Error saving Step 5: {ex.Message}");
        }
    }

    #endregion
}