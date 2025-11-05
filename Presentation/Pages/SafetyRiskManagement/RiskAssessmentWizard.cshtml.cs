using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PDXSMS.Services;
using SystemFile = System.IO.File;
using SystemPath = System.IO.Path;
using SystemJsonSerializer = System.Text.Json.JsonSerializer;
using SystemJsonSerializerOptions = System.Text.Json.JsonSerializerOptions;
using SystemJsonNamingPolicy = System.Text.Json.JsonNamingPolicy;
using SystemJsonElement = System.Text.Json.JsonElement;
using SystemJsonValueKind = System.Text.Json.JsonValueKind;

namespace PDXSMS_Presentation.Pages.SafetyRiskManagement;

public class RiskAssessmentWizardModel : PageModel
{
    private readonly ILogger<RiskAssessmentWizardModel> _logger;
    private readonly CleanRiskAssessmentService _cleanService;
    private readonly UniversalUserRepository _userRepository;

    public RiskAssessmentWizardModel(
        ILogger<RiskAssessmentWizardModel> logger,
        CleanRiskAssessmentService cleanService,
        UniversalUserRepository userRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cleanService = cleanService ?? throw new ArgumentNullException(nameof(cleanService));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    #region Properties

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public int StepNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string? HazardId { get; set; }

    // Simple DTO for assessment data instead of complex response object
    public class AssessmentDataDto
    {
        public bool IsFound { get; set; } = false;
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string LeadAssessor { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string SystemDescription { get; set; } = string.Empty;
        public string SystemBoundaries { get; set; } = string.Empty;
        public string SystemPurpose { get; set; } = string.Empty;
        public List<IdentifiedHazardDto> IdentifiedHazards { get; set; } = new();
        public int LastCompletedStep { get; set; } = 0;
        public int RecommendedStep { get; set; } = 1;
        public bool CanProceedToStep5 { get; set; } = false;

        // Additional properties for Step 3 and beyond
        public string RiskAnalysisMethod { get; set; } = string.Empty;
        public string RiskCriteria { get; set; } = string.Empty;
        public string ImplementationPlan { get; set; } = string.Empty;
    }

    public class IdentifiedHazardDto
    {
        public string Id { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string FiveMComponent { get; set; } = string.Empty;

        // Additional properties for risk assessment
        public string RiskLevel { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Likelihood { get; set; } = string.Empty;
        public string WorstCredibleOutcome { get; set; } = string.Empty;
        public string Tolerability { get; set; } = string.Empty;
        public List<string> ProposedMitigations { get; set; } = new();
        public List<string> CurrentMitigations { get; set; } = new();
    }

    // Panel member score data
    public class PanelMemberScoreData
    {
        public string MemberId { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;
        public int SeverityScore { get; set; } = 0;
        public int LikelihoodScore { get; set; } = 0;
        public double OverallScore { get; set; } = 0.0;
        public string Comments { get; set; } = string.Empty;
        public DateTime ScoredDate { get; set; } = DateTime.UtcNow;
        public bool IsComplete { get; set; } = false;
        public double CalculatedScore => SeverityScore * LikelihoodScore;
    }

    public AssessmentDataDto AssessmentData { get; set; } = new();

    [BindProperty]
    public string LeadAssessor { get; set; } = string.Empty;

    [BindProperty]
    public string SystemDescription { get; set; } = string.Empty;

    [BindProperty]
    public string SystemBoundaries { get; set; } = string.Empty;

    [BindProperty]
    public string SystemPurpose { get; set; } = string.Empty;

    [BindProperty]
    public string FiveMPersonnel { get; set; } = string.Empty;

    [BindProperty]
    public string FiveMEquipment { get; set; } = string.Empty;

    [BindProperty]
    public string FiveMProcedures { get; set; } = string.Empty;

    [BindProperty]
    public string FiveMResources { get; set; } = string.Empty;

    [BindProperty]
    public string FiveMEnvironment { get; set; } = string.Empty;

    [BindProperty]
    public string FiveMPhysicalEnvironment { get; set; } = string.Empty;

    [BindProperty]
    public string FiveMOperationalEnvironment { get; set; } = string.Empty;

    [BindProperty]
    public string StakeholderGroups { get; set; } = string.Empty;

    [BindProperty]
    public string SelectedIndividualStakeholders { get; set; } = string.Empty;

    // Additional missing properties for stakeholder management
    public List<StakeholderUserData> AvailableStakeholders { get; set; } = new();
    public List<GroupData> StakeholderGroups_Data { get; set; } = new();
    public List<string> SelectedStakeholderIds { get; set; } = new();

    // Additional properties for Step 4 and beyond
    public List<StakeholderUserData> SelectedPanelMembers { get; set; } = new();
    public List<StakeholderUserData> EmployeeStakeholders { get; set; } = new();
    public Dictionary<string, List<PanelMemberScoreData>> PanelScores { get; set; } = new();
    public Dictionary<string, double> HazardAverageScores { get; set; } = new();

    // Additional properties for Step 3  
    public List<string> RootCauses { get; set; } = new();
    public List<string> AdditionalComments { get; set; } = new();

    // Navigation Properties
    public int CurrentStep => StepNumber;
    public string CurrentStepName => GetStepName(CurrentStep);
    public string AssessmentName => AssessmentData?.Name ?? "Risk Assessment";
    public string AssessmentId => Id;

    // Calculated properties for UI
    public int IdentifiedHazardsCount => AssessmentData?.IdentifiedHazards?.Count ?? 0;

    // Step 4 specific properties for scoring panel
    public Dictionary<string, List<string>> HazardPanelMembers { get; set; } = new();

    // Simple SMS user list for the modal (bypasses StakeholderUserData complexity)
    public List<SimpleSMSUser> AvailableSMSUsers { get; set; } = new();

    // Additional properties for Step 5 mitigation data loading
    public Dictionary<string, List<object>> SavedMitigationStrategies { get; set; } = new();
    public Dictionary<string, object?> SavedResidualRiskAssessments { get; set; } = new();
    public Dictionary<string, object?> SavedMonitoringRequirements { get; set; } = new();
    public string SavedImplementationStrategy { get; set; } = string.Empty;
    public string SavedOverallTargetDate { get; set; } = string.Empty;
    public string SavedImplementationNotes { get; set; } = string.Empty;

    #endregion

    #region Page Handlers

    public async Task<IActionResult> OnGetAsync()
    {
        _logger.LogInformation("🚀 WIZARD ENTRY: Assessment ID = {AssessmentId}, Step = {StepNumber}, Hazard = {HazardId}", Id, StepNumber, HazardId);

        if (string.IsNullOrWhiteSpace(Id))
        {
            _logger.LogWarning("❌ WIZARD: No Assessment ID provided, redirecting to RiskAssessment");
            return RedirectToPage("/SafetyRiskManagement/RiskAssessment");
        }

        try
        {
            _logger.LogInformation("Loading Risk Assessment Wizard: {AssessmentId} - Step {StepNumber}", Id, StepNumber);

            // Load assessment data using the improved method
            AssessmentData = await LoadAssessmentDataAsync(Id);

            _logger.LogInformation("🔍 WIZARD: AssessmentData.IsFound = {IsFound}", AssessmentData.IsFound);

            if (!AssessmentData.IsFound)
            {
                _logger.LogError("❌ WIZARD: Assessment {AssessmentId} not found, redirecting to RiskAssessment", Id);
                TempData["ErrorMessage"] = $"Assessment {Id} not found.";
                return RedirectToPage("/SafetyRiskManagement/RiskAssessment");
            }

            _logger.LogInformation("✅ WIZARD: Assessment loaded successfully - Name: {Name}, Status: {Status}", AssessmentData.Name, AssessmentData.Status);

            // Populate bound properties from assessment data
            LeadAssessor = AssessmentData.LeadAssessor ?? "";
            SystemDescription = AssessmentData.SystemDescription ?? "";
            SystemBoundaries = AssessmentData.SystemBoundaries ?? "";
            SystemPurpose = AssessmentData.SystemPurpose ?? "";

            _logger.LogInformation("✅ WIZARD: Bound properties populated");

            // SIMPLE: Only load data for the current step
            await LoadDetailedStepDataAsync(Id, StepNumber);

            // Load stakeholder data for Step 1
            if (StepNumber == 1)
            {
                await LoadStakeholderDataAsync();
                _logger.LogInformation("✅ WIZARD: Step 1 stakeholder data loaded");
            }

            // For Step 2, load Step 1 data to show stakeholders in System Context
            if (StepNumber == 2)
            {
                // Load Step 1 data to get stakeholder information for display
                await LoadDetailedStepDataAsync(Id, 1);
                _logger.LogInformation("✅ WIZARD: Step 2 loaded Step 1 stakeholder data for display");
            }

            // Load SMS users only for Step 4
            if (StepNumber == 4)
            {
                await LoadSMSUsersForScoringPanelsAsync();
                _logger.LogInformation("✅ WIZARD: Step 4 SMS users loaded for scoring panels");
            }

            // Load SMS users only for Step 5  
            if (StepNumber == 5)
            {
                await LoadSMSUsersForScoringPanelsAsync();
                _logger.LogInformation("✅ WIZARD: SMS users loaded for Step 5 display");
                
                // 🔧 DEBUG: Log hazard risk properties for Step 5 troubleshooting
                if (AssessmentData?.IdentifiedHazards != null)
                {
                    _logger.LogInformation("🔍 DEBUG Step 5 - Checking {Count} hazards for risk assessment properties:", AssessmentData.IdentifiedHazards.Count);
                    
                    foreach (var hazard in AssessmentData.IdentifiedHazards)
                    {
                        _logger.LogInformation("📋 Hazard {HazardId}: Severity='{Severity}', Likelihood='{Likelihood}', RiskLevel='{RiskLevel}', Tolerability='{Tolerability}'", 
                            hazard.Id, hazard.Severity ?? "NULL", hazard.Likelihood ?? "NULL", hazard.RiskLevel ?? "NULL", hazard.Tolerability ?? "NULL");
                    }
                    
                    var assessedCount = AssessmentData.IdentifiedHazards.Count(h => 
                        !string.IsNullOrEmpty(h.Tolerability) || 
                        !string.IsNullOrEmpty(h.RiskLevel) || 
                        (!string.IsNullOrEmpty(h.Severity) && !string.IsNullOrEmpty(h.Likelihood)));
                    
                    _logger.LogInformation("📊 Step 5 Assessment Status: {AssessedCount} of {TotalCount} hazards are assessed", assessedCount, AssessmentData.IdentifiedHazards.Count);
                }
            }

            _logger.LogInformation("✅ WIZARD: Successfully loaded, returning Page()");
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ WIZARD: Error loading Risk Assessment Wizard: {AssessmentId}", Id);
            TempData["ErrorMessage"] = $"Error loading assessment data: {ex.Message}";
            return RedirectToPage("/SafetyRiskManagement/RiskAssessment");
        }
    }

    #endregion

    #region Step 4 Scoring Panel Methods

    /// <summary>
    /// Get scoring summary for a specific hazard (useful for Step 5)
    /// </summary>
    public class HazardScoringSummary
    {
        public string HazardId { get; set; } = string.Empty;
        public string HazardDescription { get; set; } = string.Empty;
        public int TotalPanelMembers { get; set; } = 0;
        public int CompletedScores { get; set; } = 0;
        public double? AverageScore { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public string RiskCategory { get; set; } = string.Empty;
        public bool IsComplete { get; set; } = false;
        public List<PanelMemberScoreData> MemberScores { get; set; } = new();
        public DateTime? LastScoreDate { get; set; }
    }

    /// <summary>
    /// Get comprehensive scoring summary for all hazards
    /// </summary>
    public List<HazardScoringSummary> GetHazardScoringSummaries()
    {
        var summaries = new List<HazardScoringSummary>();

        if (AssessmentData?.IdentifiedHazards == null) return summaries;

        foreach (var hazard in AssessmentData.IdentifiedHazards)
        {
            var panelMembers = HazardPanelMembers.GetValueOrDefault(hazard.Id, new List<string>());
            var memberScores = PanelScores.GetValueOrDefault(hazard.Id, new List<PanelMemberScoreData>());
            var completedScores = memberScores.Where(s => s.IsComplete).ToList();
            var averageScore = HazardAverageScores.ContainsKey(hazard.Id) ? (double?)HazardAverageScores[hazard.Id] : null;

            summaries.Add(new HazardScoringSummary
            {
                HazardId = hazard.Id,
                HazardDescription = hazard.Description,
                TotalPanelMembers = panelMembers.Count,
                CompletedScores = completedScores.Count,
                AverageScore = averageScore,
                RiskLevel = averageScore.HasValue ? GetRiskLevelFromScore(averageScore.Value) : "Not Assessed",
                RiskCategory = averageScore.HasValue ? GetRiskCategoryFromScore(averageScore.Value) : "Unknown",
                IsComplete = panelMembers.Count > 0 && completedScores.Count == panelMembers.Count,
                MemberScores = memberScores,
                LastScoreDate = completedScores.Any() ? completedScores.Max(s => s.ScoredDate) : null
            });
        }

        return summaries.OrderByDescending(s => s.AverageScore ?? 0).ToList();
    }

    /// <summary>
    /// Get risk level from numeric score
    /// </summary>
    private string GetRiskLevelFromScore(double score)
    {
        return score switch
        {
            <= 5.0 => "Low",
            <= 10.0 => "Medium",
            <= 15.0 => "High", 
            <= 20.0 => "Very High",
            _ => "Critical"
        };
    }

    /// <summary>
    /// Get risk category from numeric score
    /// </summary>
    private string GetRiskCategoryFromScore(double score)
    {
        return score switch
        {
            <= 5.0 => "Acceptable",
            <= 10.0 => "Tolerable",
            <= 15.0 => "Review Required",
            <= 20.0 => "Unacceptable",
            _ => "Catastrophic"
        };
    }

    /// <summary>
    /// Check if all hazards have completed scoring
    /// </summary>
    public bool IsAllHazardScoringComplete()
    {
        if (AssessmentData?.IdentifiedHazards == null || !AssessmentData.IdentifiedHazards.Any())
            return false;

        foreach (var hazard in AssessmentData.IdentifiedHazards)
        {
            var panelMembers = HazardPanelMembers.GetValueOrDefault(hazard.Id, new List<string>());
            var memberScores = PanelScores.GetValueOrDefault(hazard.Id, new List<PanelMemberScoreData>());
            var completedScores = memberScores.Count(s => s.IsComplete);

            if (panelMembers.Count == 0 || completedScores < panelMembers.Count)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Get overall assessment scoring progress
    /// </summary>
    public double GetOverallScoringProgress()
    {
        if (AssessmentData?.IdentifiedHazards == null || !AssessmentData.IdentifiedHazards.Any())
            return 0.0;

        var totalRequiredScores = 0;
        var totalCompletedScores = 0;

        foreach (var hazard in AssessmentData.IdentifiedHazards)
        {
            var panelMembers = HazardPanelMembers.GetValueOrDefault(hazard.Id, new List<string>());
            var memberScores = PanelScores.GetValueOrDefault(hazard.Id, new List<PanelMemberScoreData>());
            
            totalRequiredScores += panelMembers.Count;
            totalCompletedScores += memberScores.Count(s => s.IsComplete);
        }

        return totalRequiredScores > 0 ? (double)totalCompletedScores / totalRequiredScores : 0.0;
    }

    #endregion

    #region POST Handlers

    public async Task<IActionResult> OnPostSaveStep1Async()
    {
        try
        {
            _logger.LogInformation("💾 Saving Step 1 for Assessment {AssessmentId}", Id);

            // Validate required data
            if (string.IsNullOrWhiteSpace(Id))
            {
                return new JsonResult(new { success = false, message = "Assessment ID is required" });
            }

            // Create Step 1 data structure
            var step1Data = new Dictionary<string, object?>
            {
                ["leadAssessor"] = LeadAssessor ?? "",
                ["systemDescription"] = SystemDescription ?? "",
                ["systemBoundaries"] = SystemBoundaries ?? "",
                ["systemPurpose"] = SystemPurpose ?? "",
                ["fiveMPersonnel"] = FiveMPersonnel ?? "",
                ["fiveMEquipment"] = FiveMEquipment ?? "",
                ["fiveMProcedures"] = FiveMProcedures ?? "",
                ["fiveMResources"] = FiveMResources ?? "",
                ["fiveMPhysicalEnvironment"] = FiveMPhysicalEnvironment ?? "",
                ["fiveMOperationalEnvironment"] = FiveMOperationalEnvironment ?? "",
                ["stakeholderGroups"] = StakeholderGroups ?? "",
                ["selectedIndividualStakeholders"] = SelectedIndividualStakeholders ?? "",
                ["completedDate"] = DateTime.UtcNow,
                ["lastModifiedDate"] = DateTime.UtcNow
            };

            // Save to the assessment system
            await SaveStepDataToAssessmentFile(Id, 1, step1Data);

            _logger.LogInformation("✅ Step 1 saved successfully for Assessment {AssessmentId}", Id);
            return new JsonResult(new { success = true, message = "Step 1 saved successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error saving Step 1 for Assessment {AssessmentId}", Id);
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostSaveStep2Async()
    {
        try
        {
            _logger.LogInformation("💾 Saving Step 2 for Assessment {AssessmentId}", Id);

            // Get hazard data from form
            var hazards = new List<object>();
            var newHazardsForRepository = new List<Dictionary<string, object?>>();

            for (int i = 0; i < Request.Form.Count; i++)
            {
                var idKey = $"AssessmentData.IdentifiedHazards[{i}].Id";
                var descKey = $"AssessmentData.IdentifiedHazards[{i}].Description";
                var catKey = $"AssessmentData.IdentifiedHazards[{i}].Category";
                var fiveMKey = $"AssessmentData.IdentifiedHazards[{i}].FiveMComponent";

                if (Request.Form.ContainsKey(idKey))
                {
                    var hazardId = Request.Form[idKey].ToString();
                    var description = Request.Form[descKey].ToString();
                    var category = Request.Form[catKey].ToString();
                    var fiveMComponent = Request.Form.ContainsKey(fiveMKey) ? Request.Form[fiveMKey].ToString() : "";

                    hazards.Add(new
                    {
                        id = hazardId,
                        description = description,
                        category = category,
                        fiveMComponent = fiveMComponent,
                        createdDate = DateTime.UtcNow
                    });

                    // 🔧 CRITICAL FIX: Check if this is a new hazard that needs to be added to main hazards repository
                    if (!string.IsNullOrEmpty(hazardId) && !await HazardExistsInRepository(hazardId))
                    {
                        _logger.LogInformation("🆕 New hazard {HazardId} needs to be added to main repository", hazardId);

                        newHazardsForRepository.Add(new Dictionary<string, object?>
                        {
                            ["id"] = hazardId,
                            ["hazardType"] = category,
                            ["location"] = "Risk Assessment Generated",
                            ["description"] = description,
                            ["reportedById"] = "RISK ASSESSMENT",
                            ["reportedDate"] = DateTime.UtcNow,
                            ["status"] = "SMS Risk Assessment - In Progress",
                            ["priority"] = "MEDIUM",
                            ["isConfidential"] = false,
                            ["fiveMComponent"] = fiveMComponent,
                            ["createdDate"] = DateTime.UtcNow,
                            ["lastModifiedDate"] = DateTime.UtcNow
                        });
                    }
                }
            }

            // Save to assessment file
            var step2Data = new Dictionary<string, object?>
            {
                ["identifiedHazards"] = hazards,
                ["hazardCount"] = hazards.Count,
                ["completedDate"] = DateTime.UtcNow,
                ["lastModifiedDate"] = DateTime.UtcNow
            };

            await SaveStepDataToAssessmentFile(Id, 2, step2Data);

            // 🔧 CRITICAL FIX: Save new hazards to main hazards repository
            if (newHazardsForRepository.Count > 0)
            {
                await SaveNewHazardsToRepository(newHazardsForRepository);
                _logger.LogInformation("✅ Saved {Count} new hazards to main repository with 5M components", newHazardsForRepository.Count);
            }

            _logger.LogInformation("✅ Step 2 saved successfully with {Count} hazards ({NewCount} new hazards) including 5M components", hazards.Count, newHazardsForRepository.Count);
            return new JsonResult(new { success = true, message = $"Step 2 saved with {hazards.Count} hazards" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error saving Step 2: {Error}", ex.Message);
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostSaveStep3Async()
    {
        try
        {
            _logger.LogInformation("💾 Saving Step 3 for Assessment {AssessmentId}", Id);
            
            // 🔧 CRITICAL FIX: Capture hazard-specific analysis data
            var hazardAnalysisData = new List<object>();
           // var rootCauses = new List<string>();
            //var additionalComments = new List<string>();
            
            // Extract hazard analysis data from form
            for (int i = 0; i < Request.Form.Count; i++)
            {
                var worstOutcomeKey = $"AssessmentData.IdentifiedHazards[{i}].WorstCredibleOutcome";
                var rootCauseKey = $"RootCauses[{i}]";
                var commentKey = $"AdditionalComments[{i}]";
                var hazardIdKey = $"AssessmentData.IdentifiedHazards[{i}].Id";

                if (Request.Form.ContainsKey(worstOutcomeKey))
                {
                    var hazardId = Request.Form[hazardIdKey].ToString();
                    var worstOutcome = Request.Form[worstOutcomeKey].ToString();
                    var rootCause = Request.Form.ContainsKey(rootCauseKey) ? Request.Form[rootCauseKey].ToString() : "";
                    var comment = Request.Form.ContainsKey(commentKey) ? Request.Form[commentKey].ToString() : "";

                    hazardAnalysisData.Add(new
                    {
                        hazardId = hazardId,
                        worstCredibleOutcome = worstOutcome,
                        rootCause = rootCause,
                       // additionalComments = comment,
                        analyzedDate = DateTime.UtcNow
                    });
                    
                    // Also add to separate arrays for backward compatibility
                   // rootCauses.Add(rootCause);
                   // additionalComments.Add(comment);
                }
            }

            var step3Data = new Dictionary<string, object?>
            {
                ["riskAnalysisMethod"] = "SMS Risk Matrix",
                ["riskCriteria"] = Request.Form["AssessmentData.RiskCriteria"].ToString(),
                // 🔧 NEW: Store the critical hazard analysis data
                ["hazardAnalysisData"] = hazardAnalysisData,
                //["rootCauses"] = rootCauses,
                //["additionalComments"] = additionalComments,
                ["analyzedHazardCount"] = hazardAnalysisData.Count,
                ["completedDate"] = DateTime.UtcNow,
                ["lastModifiedDate"] = DateTime.UtcNow
            };

            await SaveStepDataToAssessmentFile(Id, 3, step3Data);

            _logger.LogInformation("✅ Step 3 saved successfully with analysis data for {Count} hazards", hazardAnalysisData.Count);
            return new JsonResult(new { success = true, message = $"Step 3 saved with analysis for {hazardAnalysisData.Count} hazards" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error saving Step 3: {Error}", ex.Message);
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// 🔧 NEW: Handle scoring panel assignments for specific hazards
    /// </summary>
    public async Task<IActionResult> OnPostAssignScoringPanelAsync()
    {
        try
        {
            var hazardId = Request.Form["hazardId"];
            var userIds = Request.Form["userIds"].ToString();
            var assessmentId = Request.Form["Id"];
            
            _logger.LogInformation("Assigning scoring panel - HazardId: {HazardId}, UserIds: {UserIds}, AssessmentId: {AssessmentId}", 
                hazardId, userIds, assessmentId);
            
            if (string.IsNullOrEmpty(hazardId) || string.IsNullOrEmpty(assessmentId))
            {
                return new JsonResult(new { success = false, message = "Missing required parameters" });
            }
            
            // Parse user IDs
            var selectedUserIds = string.IsNullOrEmpty(userIds) 
                ? new List<string>() 
                : userIds.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            
            // Load current assessment data to preserve existing assignments
            AssessmentData = await LoadAssessmentDataAsync(assessmentId);
            await LoadDetailedStepDataAsync(assessmentId, 4);
            
            // Ensure we have the collections initialized
            if (HazardPanelMembers == null)
                HazardPanelMembers = new Dictionary<string, List<string>>();
            
            if (PanelScores == null)
                PanelScores = new Dictionary<string, List<PanelMemberScoreData>>();
            
            // 🔧 CRITICAL FIX: Update ONLY the specific hazard, preserve others
            HazardPanelMembers[hazardId] = selectedUserIds;
            
            _logger.LogInformation("✅ Updated panel for {HazardId}: {UserCount} users. Total hazards with panels: {TotalHazards}", 
                hazardId, selectedUserIds.Count, HazardPanelMembers.Count);
            
            // Log all current assignments for debugging
            foreach (var assignment in HazardPanelMembers)
            {
                _logger.LogInformation("📋 Hazard {HazardId} has {UserCount} panel members: {Users}", 
                    assignment.Key, assignment.Value.Count, string.Join(", ", assignment.Value));
            }
            
            // Save the updated panel assignments to assessment file
            var step4Data = new Dictionary<string, object?>
            {
                ["hazardPanelAssignments"] = HazardPanelMembers.ToDictionary(
                    kvp => kvp.Key, 
                    kvp => kvp.Value.Cast<object>().ToList()
                ),
                ["panelScores"] = PanelScores.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Select(score => new Dictionary<string, object?>
                    {
                        ["memberId"] = score.MemberId,
                        ["severityScore"] = score.SeverityScore,
                        ["likelihoodScore"] = score.LikelihoodScore,
                        ["isComplete"] = score.IsComplete,
                        ["calculatedScore"] = score.CalculatedScore
                    }).Cast<object>().ToList()
                ),
                ["lastPanelUpdateDate"] = DateTime.UtcNow
            };
            
            await SaveStepDataToAssessmentFile(assessmentId, 4, step4Data);
            
            _logger.LogInformation("✅ Successfully assigned {Count} users to hazard {HazardId} scoring panel. Preserved other hazard assignments.", 
                selectedUserIds.Count, hazardId);
            
            return new JsonResult(new { 
                success = true, 
                message = $"Successfully assigned {selectedUserIds.Count} users to {hazardId} scoring panel",
                hazardId = hazardId,
                assignedUsers = selectedUserIds.Count,
                totalHazardsWithPanels = HazardPanelMembers.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning scoring panel");
            return new JsonResult(new { success = false, message = "Error assigning scoring panel: " + ex.Message });
        }
    }

    /// <summary>
    /// Handle individual panel member score submission
    /// </summary>
    public async Task<IActionResult> OnPostSubmitScoreAsync()
    {
        try
        {
            var hazardId = Request.Form["hazardId"].ToString();
            var memberId = Request.Form["memberId"].ToString();
            var severityScore = int.Parse(Request.Form["severityScore"]);
            var likelihoodScore = int.Parse(Request.Form["likelihoodScore"]);
            var assessmentId = Request.Form["Id"].ToString();
            
            _logger.LogInformation("Submitting score - HazardId: {HazardId}, MemberId: {MemberId}, Severity: {Severity}, Likelihood: {Likelihood}", 
                hazardId, memberId, severityScore, likelihoodScore);
            
            if (string.IsNullOrEmpty(hazardId) || string.IsNullOrEmpty(memberId) || string.IsNullOrEmpty(assessmentId))
            {
                return new JsonResult(new { success = false, message = "Missing required parameters" });
            }
            
            if (severityScore < 1 || severityScore > 5 || likelihoodScore < 1 || likelihoodScore > 5)
            {
                return new JsonResult(new { success = false, message = "Invalid severity or likelihood scores" });
            }
            
            // Load current assessment data
            AssessmentData = await LoadAssessmentDataAsync(assessmentId);
            await LoadDetailedStepDataAsync(assessmentId, 4);
            
            // Ensure collections are initialized
            if (PanelScores == null)
                PanelScores = new Dictionary<string, List<PanelMemberScoreData>>();
                
            if (HazardPanelMembers == null)
                HazardPanelMembers = new Dictionary<string, List<string>>();
            
            // Initialize hazard scores if not exists
            if (!PanelScores.ContainsKey(hazardId))
                PanelScores[hazardId] = new List<PanelMemberScoreData>();
            
            // Find or create the member score
            var memberScore = PanelScores[hazardId].FirstOrDefault(s => s.MemberId == memberId);
            if (memberScore == null)
            {
                memberScore = new PanelMemberScoreData
                {
                    MemberId = memberId
                };
                PanelScores[hazardId].Add(memberScore);
            }
            
            // Update the score
            memberScore.SeverityScore = severityScore;
            memberScore.LikelihoodScore = likelihoodScore;
            memberScore.IsComplete = true;
            memberScore.ScoredDate = DateTime.UtcNow;
            
            // Calculate hazard average
            var completedScores = PanelScores[hazardId].Where(s => s.IsComplete).ToList();
            double? hazardAverage = null;
            if (completedScores.Any())
            {
                hazardAverage = completedScores.Average(s => s.CalculatedScore);
                if (HazardAverageScores == null)
                    HazardAverageScores = new Dictionary<string, double>();
                HazardAverageScores[hazardId] = hazardAverage.Value;
            }
            
            // Save updated data
            var step4Data = new Dictionary<string, object?>
            {
                ["hazardPanelAssignments"] = HazardPanelMembers.ToDictionary(
                    kvp => kvp.Key, 
                    kvp => kvp.Value.Cast<object>().ToList()
                ),
                ["panelScores"] = PanelScores.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Select(score => new Dictionary<string, object?>
                    {
                        ["memberId"] = score.MemberId,
                        ["severityScore"] = score.SeverityScore,
                        ["likelihoodScore"] = score.LikelihoodScore,
                        ["isComplete"] = score.IsComplete,
                        ["calculatedScore"] = score.CalculatedScore,
                        ["scoredDate"] = score.ScoredDate
                    }).Cast<object>().ToList()
                ),
                ["hazardAverageScores"] = HazardAverageScores?.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value),
                ["lastScoreUpdateDate"] = DateTime.UtcNow
            };
            
            await SaveStepDataToAssessmentFile(assessmentId, 4, step4Data);
            
            _logger.LogInformation("✅ Score submitted successfully for {MemberId} on {HazardId}. Calculated score: {Score}. Hazard average: {Average}", 
                memberId, hazardId, memberScore.CalculatedScore, hazardAverage);
            
            return new JsonResult(new { 
                success = true, 
                message = $"Score submitted successfully",
                hazardId = hazardId,
                memberId = memberId,
                calculatedScore = memberScore.CalculatedScore,
                hazardAverage = hazardAverage,
                completedScores = completedScores.Count,
                totalPanelMembers = HazardPanelMembers.GetValueOrDefault(hazardId, new List<string>()).Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting score");
            return new JsonResult(new { success = false, message = "Error submitting score: " + ex.Message });
        }
    }

    public async Task<IActionResult> OnPostSaveStep4Async()
    {
        try
        {
            _logger.LogInformation("💾 Saving Step 4 for Assessment {AssessmentId}", Id);

            // Validate required data
            if (string.IsNullOrWhiteSpace(Id))
            {
                return new JsonResult(new { success = false, message = "Assessment ID is required" });
            }

            // Load current assessment data to preserve existing data
            AssessmentData = await LoadAssessmentDataAsync(Id);
            await LoadDetailedStepDataAsync(Id, 4);

            // Ensure collections are initialized
            if (HazardPanelMembers == null)
                HazardPanelMembers = new Dictionary<string, List<string>>();
            
            if (PanelScores == null)
                PanelScores = new Dictionary<string, List<PanelMemberScoreData>>();
            
            if (HazardAverageScores == null)
                HazardAverageScores = new Dictionary<string, double>();

            // Create Step 4 data structure preserving existing panel data
            var step4Data = new Dictionary<string, object?>
            {
                ["hazardPanelAssignments"] = HazardPanelMembers.ToDictionary(
                    kvp => kvp.Key, 
                    kvp => kvp.Value.Cast<object>().ToList()
                ),
                ["panelScores"] = PanelScores.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Select(score => new Dictionary<string, object?>
                    {
                        ["memberId"] = score.MemberId,
                        ["severityScore"] = score.SeverityScore,
                        ["likelihoodScore"] = score.LikelihoodScore,
                        ["isComplete"] = score.IsComplete,
                        ["calculatedScore"] = score.CalculatedScore,
                        ["scoredDate"] = score.ScoredDate
                    }).Cast<object>().ToList()
                ),
                ["hazardAverageScores"] = HazardAverageScores?.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value),
                ["completedDate"] = DateTime.UtcNow,
                ["lastModifiedDate"] = DateTime.UtcNow
            };

            // Save to the assessment system
            await SaveStepDataToAssessmentFile(Id, 4, step4Data);

            var panelCount = HazardPanelMembers.Count;
            var scoreCount = PanelScores.Values.Sum(list => list.Count(s => s.IsComplete));
            var averageCount = HazardAverageScores.Count;

            _logger.LogInformation("✅ Step 4 saved successfully - Panels: {PanelCount}, Scores: {ScoreCount}, Averages: {AverageCount}", 
                panelCount, scoreCount, averageCount);

            return new JsonResult(new { 
                success = true, 
                message = $"Step 4 saved with {panelCount} panel assignments, {scoreCount} scores, {averageCount} averages"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error saving Step 4 for Assessment {AssessmentId}", Id);
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostSaveStep5Async()
    {
        try
        {
            _logger.LogInformation("💾 Saving Step 5 for Assessment {AssessmentId}", Id);

            // Validate required data
            if (string.IsNullOrWhiteSpace(Id))
            {
                return new JsonResult(new { success = false, message = "Assessment ID is required" });
            }

            // Load current assessment data to preserve existing data
            AssessmentData = await LoadAssessmentDataAsync(Id);

            // Extract mitigation planning data from form
            var mitigationPlanning = new Dictionary<string, object>();
            var hazardMitigations = new List<object>();
            var residualRiskAssessments = new List<object>();

            // Extract implementation planning data
            var implementationPlan = Request.Form["AssessmentData.ImplementationPlan"].ToString();
            var overallTargetDate = Request.Form["OverallTargetDate"].ToString();
            var implementationNotes = Request.Form["ImplementationNotes"].ToString();

            _logger.LogInformation("📋 Extracting Step 5 data - Implementation: {Plan}, Target: {Date}", 
                implementationPlan, overallTargetDate);

            // Process each hazard's mitigation strategies and residual risk
            if (AssessmentData?.IdentifiedHazards != null)
            {
                for (int hazardIdx = 0; hazardIdx < AssessmentData.IdentifiedHazards.Count; hazardIdx++)
                {
                    var hazard = AssessmentData.IdentifiedHazards[hazardIdx];
                    
                    // Extract mitigation strategies for this hazard
                    var strategies = ExtractMitigationStrategies(hazardIdx);
                    
                    // Extract residual risk assessment for this hazard
                    var residualRisk = ExtractResidualRiskAssessment(hazardIdx);
                    
                    // Extract monitoring data for acceptable risks
                    var monitoring = ExtractMonitoringData(hazardIdx);

                    if (strategies.Any() || residualRisk != null || monitoring != null)
                    {
                        var hazardMitigation = new Dictionary<string, object?>
                        {
                            ["hazardId"] = hazard.Id,
                            ["hazardDescription"] = hazard.Description,
                            ["initialRiskLevel"] = hazard.RiskLevel,
                            ["tolerability"] = hazard.Tolerability,
                            ["mitigationStrategies"] = strategies,
                            ["residualRiskAssessment"] = residualRisk,
                            ["monitoringRequirements"] = monitoring,
                            ["createdDate"] = DateTime.UtcNow
                        };

                        hazardMitigations.Add(hazardMitigation);
                        
                        _logger.LogInformation("✅ Processed mitigation data for {HazardId}: {StrategyCount} strategies, Residual Risk: {HasResidual}", 
                            hazard.Id, strategies.Count, residualRisk != null);
                    }
                }
            }

            // Create Step 5 data structure
            var step5Data = new Dictionary<string, object?>
            {
                ["implementationStrategy"] = implementationPlan,
                ["overallTargetDate"] = overallTargetDate,
                ["implementationNotes"] = implementationNotes,
                ["hazardMitigations"] = hazardMitigations,
                ["mitigationPlanningComplete"] = hazardMitigations.Count > 0,
                ["totalHazardsWithMitigation"] = hazardMitigations.Count,
                ["completedDate"] = DateTime.UtcNow,
                ["lastModifiedDate"] = DateTime.UtcNow
            };

            // Save to the assessment system
            await SaveStepDataToAssessmentFile(Id, 5, step5Data);

            var strategyCount = hazardMitigations.Sum(hm => 
            {
                var hm_dict = hm as Dictionary<string, object?>;
                var strategies = hm_dict?["mitigationStrategies"] as List<object>;
                return strategies?.Count ?? 0;
            });

            _logger.LogInformation("✅ Step 5 saved successfully - {HazardCount} hazards with mitigation, {StrategyCount} total strategies", 
                hazardMitigations.Count, strategyCount);

            return new JsonResult(new { 
                success = true, 
                message = $"Step 5 saved - {hazardMitigations.Count} hazards with mitigation planning, {strategyCount} total strategies"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error saving Step 5 for Assessment {AssessmentId}", Id);
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Extract mitigation strategies for a specific hazard from form data
    /// </summary>
    private List<object> ExtractMitigationStrategies(int hazardIndex)
    {
        var strategies = new List<object>();
        int strategyIndex = 0;
        
        // Keep looking for strategies until we don't find any more
        while (true)
        {
            var descriptionKey = $"MitigationStrategies[{hazardIndex}][{strategyIndex}][Description]";
            var description = Request.Form[descriptionKey].ToString();
            
            if (string.IsNullOrEmpty(description))
            {
                break; // No more strategies for this hazard
            }
            
            // Extract all strategy fields
            var controlTypeKey = $"MitigationStrategies[{hazardIndex}][{strategyIndex}][ControlType]";
            var priorityKey = $"MitigationStrategies[{hazardIndex}][{strategyIndex}][Priority]";
            var targetDateKey = $"MitigationStrategies[{hazardIndex}][{strategyIndex}][TargetDate]";
            var estimatedCostKey = $"MitigationStrategies[{hazardIndex}][{strategyIndex}][EstimatedCost]";
            var statusKey = $"MitigationStrategies[{hazardIndex}][{strategyIndex}][Status]";
            
            var controlType = Request.Form[controlTypeKey].ToString();
            var priority = Request.Form[priorityKey].ToString();
            var targetDate = Request.Form[targetDateKey].ToString();
            var estimatedCost = Request.Form[estimatedCostKey].ToString();
            var status = Request.Form[statusKey].ToString();
            
            // Extract assignments for this strategy
            var assignments = ExtractStrategyAssignments(hazardIndex, strategyIndex);
            
            // Parse estimated cost
            decimal.TryParse(estimatedCost, out var costValue);
            DateTime.TryParse(targetDate, out var parsedTargetDate);
            
            var strategy = new Dictionary<string, object?>
            {
                ["description"] = description,
                ["controlType"] = controlType,
                ["priority"] = priority,
                ["targetDate"] = parsedTargetDate != default ? parsedTargetDate : null,
                ["estimatedCost"] = costValue,
                ["status"] = status,
                ["assignments"] = assignments,
                ["createdDate"] = DateTime.UtcNow
            };
            
            strategies.Add(strategy);
            strategyIndex++;
        }
        
        return strategies;
    }

    /// <summary>
    /// Extract assignments for a specific mitigation strategy
    /// </summary>
    private List<object> ExtractStrategyAssignments(int hazardIndex, int strategyIndex)
    {
        var assignments = new List<object>();
        int assignmentIndex = 0;
        
        // Keep looking for assignments until we don't find any more
        while (true)
        {
            var departmentKey = $"MitigationStrategies[{hazardIndex}][{strategyIndex}][Assignments][{assignmentIndex}][Department]";
            var personKey = $"MitigationStrategies[{hazardIndex}][{strategyIndex}][Assignments][{assignmentIndex}][Person]";
            var roleKey = $"MitigationStrategies[{hazardIndex}][{strategyIndex}][Assignments][{assignmentIndex}][Role]";
            
            var department = Request.Form[departmentKey].ToString();
            var person = Request.Form[personKey].ToString();
            var role = Request.Form[roleKey].ToString();
            
            // If no department, this assignment doesn't exist
            if (string.IsNullOrEmpty(department))
            {
                break;
            }
            
            var assignment = new Dictionary<string, object?>
            {
                ["department"] = department,
                ["person"] = person,
                ["role"] = role,
                ["assignedDate"] = DateTime.UtcNow
            };
            
            assignments.Add(assignment);
            assignmentIndex++;
        }
        
        return assignments;
    }

    /// <summary>
    /// Extract residual risk assessment for a specific hazard
    /// </summary>
    private Dictionary<string, object?>? ExtractResidualRiskAssessment(int hazardIndex)
    {
        var severityKey = $"ResidualSeverity[{hazardIndex}]";
        var likelihoodKey = $"ResidualLikelihood[{hazardIndex}]";
        var riskLevelKey = $"ResidualRiskLevel[{hazardIndex}]";
        var tolerabilityKey = $"ResidualTolerability[{hazardIndex}]";
        var justificationKey = $"ResidualRiskJustification[{hazardIndex}]";
        
        var severity = Request.Form[severityKey].ToString();
        var likelihood = Request.Form[likelihoodKey].ToString();
        var riskLevel = Request.Form[riskLevelKey].ToString();
        var tolerability = Request.Form[tolerabilityKey].ToString();
        var justification = Request.Form[justificationKey].ToString();
        
        // Only create residual risk assessment if severity and likelihood are provided
        if (!string.IsNullOrEmpty(severity) && !string.IsNullOrEmpty(likelihood))
        {
            return new Dictionary<string, object?>
            {
                ["residualSeverity"] = severity,
                ["residualLikelihood"] = likelihood,
                ["residualRiskLevel"] = riskLevel,
                ["residualTolerability"] = tolerability,
                ["assessmentRationale"] = justification,
                ["assessedDate"] = DateTime.UtcNow
            };
        }
        
        return null;
    }

    /// <summary>
    /// Extract monitoring requirements for acceptable risks
    /// </summary>
    private Dictionary<string, object?>? ExtractMonitoringData(int hazardIndex)
    {
        var frequencyKey = $"MonitoringFrequency[{hazardIndex}]";
        var departmentKey = $"MonitoringDepartment[{hazardIndex}]";
        var triggerKey = $"ReviewTrigger[{hazardIndex}]";
        
        var frequency = Request.Form[frequencyKey].ToString();
        var department = Request.Form[departmentKey].ToString();
        var trigger = Request.Form[triggerKey].ToString();
        
        // Only create monitoring data if at least frequency is provided
        if (!string.IsNullOrEmpty(frequency))
        {
            return new Dictionary<string, object?>
            {
                ["monitoringFrequency"] = frequency,
                ["responsibleDepartment"] = department,
                ["reviewTrigger"] = trigger,
                ["establishedDate"] = DateTime.UtcNow
            };
        }
        
        return null;
    }

    #endregion

    #region Navigation Handlers

    public async Task<IActionResult> OnPostNextStepAsync()
    {
        try
        {
            _logger.LogInformation("🚀 NAVIGATION: Moving from Step {CurrentStep} to next step", StepNumber);

            var nextStep = Math.Min(StepNumber + 1, 5);

            // Redirect to the next step
            return RedirectToPage("/SafetyRiskManagement/RiskAssessmentWizard", new { id = Id, stepNumber = nextStep });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error navigating to next step");
            TempData["ErrorMessage"] = "Error navigating to next step.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostPreviousStepAsync()
    {
        try
        {
            _logger.LogInformation("🚀 NAVIGATION: Moving from Step {CurrentStep} to previous step", StepNumber);

            var previousStep = Math.Max(StepNumber - 1, 1);

            // Redirect to the previous step
            return RedirectToPage("/SafetyRiskManagement/RiskAssessmentWizard", new { id = Id, stepNumber = previousStep });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error navigating to previous step");
            TempData["ErrorMessage"] = "Error navigating to previous step.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostNavigateToStepAsync(int targetStep)
    {
        try
        {
            _logger.LogInformation("🚀 NAVIGATION: Navigating from Step {CurrentStep} to Step {TargetStep}", StepNumber, targetStep);

            // Validate target step is within bounds
            if (targetStep < 1 || targetStep > 5)
            {
                TempData["ErrorMessage"] = "Invalid step number.";
                return Page();
            }

            // Redirect to the target step
            return RedirectToPage("/SafetyRiskManagement/RiskAssessmentWizard", new { id = Id, stepNumber = targetStep });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error navigating to step {TargetStep}", targetStep);
            TempData["ErrorMessage"] = "Error navigating to step.";
            return Page();
        }
    }

    #endregion

    /// <summary>
    /// Convert average score to approximate severity and likelihood values
    /// </summary>
    private static (int severity, int likelihood) ConvertScoreToSeverityLikelihood(double averageScore)
    {
        // This is a simplified conversion based on common scoring patterns
        // In a real implementation, you'd use the actual individual scores
        
        if (averageScore <= 2)
            return (1, 2); // Minor severity, Unlikely
        else if (averageScore <= 4)
            return (2, 2); // Moderate severity, Unlikely  
        else if (averageScore <= 6)
            return (2, 3); // Moderate severity, Possible
        else if (averageScore <= 9)
            return (3, 3); // Serious severity, Possible
        else if (averageScore <= 12)
            return (3, 4); // Serious severity, Likely
        else if (averageScore <= 16)
            return (4, 4); // Major severity, Likely
        else if (averageScore <= 20)
            return (4, 5); // Major severity, Frequent
        else
            return (5, 5); // Catastrophic severity, Frequent
    }

    /// <summary>
    /// Get likelihood code from numeric likelihood value
    /// </summary>
    private static string GetLikelihoodCode(int likelihood)
    {
        return likelihood switch
        {
            1 => "A",
            2 => "B", 
            3 => "C",
            4 => "D",
            5 => "E",
            _ => "C"
        };
    }

    /// <summary>
    /// Determine tolerability from average score
    /// </summary>
    private static string GetTolerabilityFromScore(double averageScore)
    {
        return averageScore switch
        {
            <= 2.0 => "Acceptable",
            <= 6.0 => "ALARP", 
            <= 12.0 => "ALARP",
            _ => "Unacceptable"
        };
    }

    #region Helper Methods

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
        return targetStep >= 1 && targetStep <= 5;
    }

    public int GetLastCompletedStep()
    {
        return AssessmentData?.LastCompletedStep ?? 0;
    }

    public int GetRecommendedStep()
    {
        return AssessmentData?.RecommendedStep ?? 1;
    }

    // Risk calculation helper methods
    public string CalculateRiskLevel(string severity, string likelihood)
    {
        // Simple risk matrix calculation
        if (string.IsNullOrEmpty(severity) || string.IsNullOrEmpty(likelihood))
            return "Unknown";

        // Parse numeric values or use defaults
        if (int.TryParse(severity, out int sev) && int.TryParse(likelihood, out int like))
        {
            var riskScore = sev * like;
            return riskScore switch
            {
                <= 5 => "1A",
                <= 10 => "2B", 
                <= 15 => "3C",
                <= 20 => "4D",
                _ => "5E"
            };
        }

        return "Unknown";
    }

    public string GetRiskLevelClass(string riskLevel)
    {
        return riskLevel?.ToUpper() switch
        {
            "1A" or "1B" or "2A" => "success",
            "2B" or "2C" or "3A" or "3B" => "warning",
            "3C" or "3D" or "4A" or "4B" => "info",
            "4C" or "4D" or "5A" or "5B" or "5C" => "danger",
            "5D" or "5E" => "dark",
            _ => "secondary"
        };
    }

    public string GetRiskLevelText(string riskLevel)
    {
        return riskLevel?.ToUpper() switch
        {
            "1A" or "1B" or "2A" => "Acceptable",
            "2B" or "2C" or "3A" or "3B" => "Tolerable",
            "3C" or "3D" or "4A" or "4B" => "Review Required",
            "4C" or "4D" or "5A" or "5B" or "5C" => "Unacceptable",
            "5D" or "5E" => "Catastrophic",
            _ => "Unknown"
        };
    }

    public string GetSeverityDescription(string severity)
    {
        return severity switch
        {
            "1" => "Negligible",
            "2" => "Minor",
            "3" => "Major", 
            "4" => "Hazardous",
            "5" => "Catastrophic",
            _ => "Unknown"
        };
    }

    public string GetLikelihoodDescription(string likelihood)
    {
        return likelihood switch
        {
            "1" => "Extremely Improbable",
            "2" => "Improbable",
            "3" => "Remote",
            "4" => "Probable", 
            "5" => "Frequent",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Helper method to get string values, handling both string and JsonElement types
    /// </summary>
    private static string? GetStringValue(Dictionary<string, object?> dict, string key)
    {
        if (!dict.ContainsKey(key)) return null;
        
        var value = dict[key];
        if (value == null) return null;
        
        // Handle JsonElement
        if (value is SystemJsonElement jsonElement)
        {
            return jsonElement.ValueKind == SystemJsonValueKind.String ? jsonElement.GetString() : jsonElement.ToString();
        }
        
        return value.ToString();
    }

    /// <summary>
    /// Helper method to get integer values, handling both int and JsonElement types
    /// </summary>
    private static int? GetIntValue(Dictionary<string, object?> dict, string key)
    {
        if (!dict.ContainsKey(key)) return null;
        
        var value = dict[key];
        if (value == null) return null;
        
        // Handle JsonElement
        if (value is SystemJsonElement jsonElement)
        {
            return jsonElement.ValueKind == SystemJsonValueKind.Number ? jsonElement.GetInt32() : null;
        }
        
        // Handle direct integer
        if (value is int intValue) return intValue;
        
        // Try parsing string
        return int.TryParse(value.ToString(), out var parsed) ? parsed : null;
    }

    /// <summary>
    /// Helper method to get double values, handling both double and JsonElement types
    /// </summary>
    private static double? GetDoubleValue(Dictionary<string, object?> dict, string key)
    {
        if (!dict.ContainsKey(key)) return null;
        
        var value = dict[key];
        if (value == null) return null;
        
        // Handle JsonElement
        if (value is SystemJsonElement jsonElement && jsonElement.ValueKind == SystemJsonValueKind.Number)
            return jsonElement.GetDouble();
        
        // Handle direct double
        if (value is double doubleValue) return doubleValue;
        
        // Handle float
        if (value is float floatValue) return (double)floatValue;
        
        // Try parsing string
        return double.TryParse(value.ToString(), out var parsed) ? parsed : null;
    }

    /// <summary>
    /// Helper method to get boolean values, handling both bool and JsonElement types
    /// </summary>
    private static bool? GetBooleanValue(Dictionary<string, object?> dict, string key)
    {
        if (!dict.ContainsKey(key)) return null;
        
        var value = dict[key];
        if (value == null) return null;
        
        // Handle JsonElement
        if (value is SystemJsonElement jsonElement)
            return jsonElement.ValueKind == SystemJsonValueKind.True ? true :
                   jsonElement.ValueKind == SystemJsonValueKind.False ? false : null;
        
        // Handle direct boolean
        if (value is bool boolValue) return boolValue;
        
        // Try parsing string
        return bool.TryParse(value.ToString(), out var parsed) ? parsed : null;
    }

    /// <summary>
    /// Helper method to get DateTime values, handling both DateTime and JsonElement types
    /// </summary>
    private static DateTime? GetDateTimeValue(Dictionary<string, object?> dict, string key)
    {
        if (!dict.ContainsKey(key)) return null;
        
        var value = dict[key];
        if (value == null) return null;
        
        // Handle JsonElement
        if (value is SystemJsonElement jsonElement && jsonElement.ValueKind == SystemJsonValueKind.String)
        {
            var dateString = jsonElement.GetString();
            return DateTime.TryParse(dateString, out var parsed) ? parsed : null;
        }
        
        // Handle direct DateTime
        if (value is DateTime dateTimeValue) return dateTimeValue;
        
        // Try parsing string
        return DateTime.TryParse(value.ToString(), out var parsedDateTime) ? parsedDateTime : null;
    }

    /// <summary>
    /// Helper method to get nested objects, handling JsonElement types
    /// </summary>
    private static object? GetNestedValue(Dictionary<string, object?> dict, string key)
    {
        if (!dict.ContainsKey(key)) return null;
        
        var value = dict[key];
        if (value == null) return null;
        
        // Handle JsonElement for objects
        if (value is SystemJsonElement jsonElement && jsonElement.ValueKind == SystemJsonValueKind.Object)
        {
            var result = new Dictionary<string, object?>();
            foreach (var property in jsonElement.EnumerateObject())
            {
                result[property.Name] = property.Value;
            }
            return result;
        }
        
        return value;
    }

    /// <summary>
    /// Helper method to get array values, handling JsonElement types
    /// </summary>
    private static List<object>? GetArrayValue(Dictionary<string, object?> dict, string key)
    {
        if (!dict.ContainsKey(key)) return null;
        
        var value = dict[key];
        if (value == null) return null;
        
        // Handle JsonElement for arrays
        if (value is SystemJsonElement jsonElement && jsonElement.ValueKind == SystemJsonValueKind.Array)
        {
            var result = new List<object>();
            foreach (var item in jsonElement.EnumerateArray())
            {
                if (item.ValueKind == SystemJsonValueKind.Object)
                {
                    var itemDict = new Dictionary<string, object?>();
                    foreach (var property in item.EnumerateObject())
                    {
                        itemDict[property.Name] = property.Value;
                    }
                    result.Add(itemDict);
                }
                else
                {
                    result.Add(item);
                }
            }
            return result;
        }
        
        // Handle direct List
        if (value is List<object> listValue) return listValue;
        
        return null;
    }

    private async Task LoadStakeholderDataAsync()
    {
        try
        {
            AvailableStakeholders = await _userRepository.GetAllStakeholderUsersAsync();
            var allGroups = await _userRepository.GetAllGroupsAsync();

            // Update member counts for each group
            foreach (var group in allGroups)
            {
                group.MemberCount = AvailableStakeholders.Count(s =>
                    s.StakeholderGroups?.Contains(group.Name) == true);
            }

            // Filter out groups with 0 members
            StakeholderGroups_Data = allGroups.Where(g => g.MemberCount > 0).ToList();

            _logger.LogInformation("Loaded {Count} stakeholders, {GroupCount} groups", 
                AvailableStakeholders.Count, StakeholderGroups_Data.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading stakeholder data");
            AvailableStakeholders = new List<StakeholderUserData>();
            StakeholderGroups_Data = new List<GroupData>();
        }
    }

    /// <summary>
    /// 🔧 CRITICAL FIX: Load SMS users for Step 4 scoring panels
    /// </summary>
    private async Task LoadSMSUsersForScoringPanelsAsync()
    {
        try
        {
            _logger.LogInformation("🚀 STARTING LoadSMSUsersForScoringPanelsAsync for Step {StepNumber}", StepNumber);
            
            // Load both SMS organizational users AND employee stakeholders
            var smsUsers = await _userRepository.GetAllSMSUsersAsync();
            var employeeStakeholders = await _userRepository.GetEmployeeStakeholdersAsync();

            _logger.LogInformation("📊 SMS USER LOADING: Found {SMSUserCount} SMS users, {EmployeeCount} employee stakeholders", 
                smsUsers.Count, employeeStakeholders.Count);

            // Convert SMS users to simple list for modal (bypasses StakeholderUserData issues)
            var employeeSmsUsers = smsUsers.Where(u => u.IsEmployee).ToList();
            _logger.LogInformation("🔍 Found {Count} employee SMS users for scoring panels", employeeSmsUsers.Count);
            
            if (employeeSmsUsers.Count == 0)
            {
                _logger.LogWarning("⚠️ NO EMPLOYEE SMS USERS FOUND! Total SMS users: {Total}, Employee filter result: 0", smsUsers.Count);
                
                // Log the first few users to see their IsEmployee status
                foreach (var user in smsUsers.Take(5))
                {
                    _logger.LogInformation("📋 SMS User: {Id} - {Name} - IsEmployee: {IsEmployee}", 
                        user.Id, user.DisplayName, user.IsEmployee);
                }
            }
            
            // Use simple SMS user class that definitely works
            AvailableSMSUsers = employeeSmsUsers.Select(smsUser => new SimpleSMSUser
            {
                Id = smsUser.Id,
                DisplayName = smsUser.DisplayName ?? smsUser.Id,
                Email = smsUser.Email ?? "",
                OrganizationName = smsUser.Department ?? "SMS Employee"
            }).ToList();
                
            _logger.LogInformation("✅ Successfully created {Count} SimpleSMSUser objects for Step 4 scoring panels", AvailableSMSUsers.Count);
            
            if (AvailableSMSUsers.Count > 0)
            {
                _logger.LogInformation("📋 First SMS User: {Id} - {Name} - {Email}", 
                    AvailableSMSUsers[0].Id, AvailableSMSUsers[0].DisplayName, AvailableSMSUsers[0].Email);
            }
            
            // Also maintain EmployeeStakeholders for backward compatibility, but simplified
            EmployeeStakeholders = AvailableSMSUsers.Select(u => new StakeholderUserData()).ToList();
            
            _logger.LogInformation("✅ COMPLETED LoadSMSUsersForScoringPanelsAsync - AvailableSMSUsers: {Count}, EmployeeStakeholders: {EmployeeCount}", 
                AvailableSMSUsers.Count, EmployeeStakeholders.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ CRITICAL ERROR in LoadSMSUsersForScoringPanelsAsync");
            AvailableSMSUsers = new List<SimpleSMSUser>();
            EmployeeStakeholders = new List<StakeholderUserData>();
            
            // Create fallback test data so we can at least test the modal
            _logger.LogInformation("🔧 Creating fallback test SMS users for debugging");
            AvailableSMSUsers = new List<SimpleSMSUser>
            {
                new SimpleSMSUser { Id = "test-1", DisplayName = "Test User 1", Email = "test1@pdx.com", OrganizationName = "Test Dept" },
                new SimpleSMSUser { Id = "test-2", DisplayName = "Test User 2", Email = "test2@pdx.com", OrganizationName = "Test Dept" }
            };
            _logger.LogInformation("🔧 Created {Count} fallback SMS users", AvailableSMSUsers.Count);
        }
    }

    private async Task<AssessmentDataDto> LoadAssessmentDataAsync(string assessmentId)
    {
        try
        {
            _logger.LogInformation("📂 Loading assessment data for {AssessmentId}", assessmentId);

            var appDataPath = SystemPath.Combine(Directory.GetCurrentDirectory(), "AppData");
            var assessmentsFile = SystemPath.Combine(appDataPath, "risk-assessments.json");

            if (!SystemFile.Exists(assessmentsFile))
            {
                _logger.LogWarning("Risk assessments file not found: {File}", assessmentsFile);
                return new AssessmentDataDto { IsFound = false };
            }

            var json = await SystemFile.ReadAllTextAsync(assessmentsFile);
            var assessments = SystemJsonSerializer.Deserialize<List<Dictionary<string, object?>>>(json, new SystemJsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<Dictionary<string, object?>>();

            var assessment = assessments.FirstOrDefault(a => 
                a.ContainsKey("id") && a["id"]?.ToString() == assessmentId);

            if (assessment == null)
            {
                _logger.LogWarning("❌ Assessment {AssessmentId} not found", assessmentId);
                return new AssessmentDataDto { IsFound = false };
            }

            var stepData = GetNestedValue(assessment, "stepData") as Dictionary<string, object?> ?? new Dictionary<string, object?>();
            var step1Data = GetNestedValue(stepData, "step1") as Dictionary<string, object?>;
            
            var assessmentData = new AssessmentDataDto
            {
                IsFound = true,
                Id = assessmentId,
                Name = GetStringValue(assessment, "name") ?? "Risk Assessment",
                Status = GetStringValue(assessment, "status") ?? "Ready to Start",
                LastCompletedStep = GetIntValue(assessment, "currentStep") ?? 0,
                RecommendedStep = Math.Min((GetIntValue(assessment, "currentStep") ?? 0) + 1, 5),
                IdentifiedHazards = new List<IdentifiedHazardDto>()
            };

            if (step1Data != null)
            {
                assessmentData.LeadAssessor = GetStringValue(step1Data, "leadAssessor") ?? "";
                assessmentData.SystemDescription = GetStringValue(step1Data, "systemDescription") ?? "";
                assessmentData.SystemBoundaries = GetStringValue(step1Data, "systemBoundaries") ?? "";
                assessmentData.SystemPurpose = GetStringValue(step1Data, "systemPurpose") ?? "";
            }

            var step2Data = GetNestedValue(stepData, "step2") as Dictionary<string, object?>;
            if (step2Data != null)
            {
                var hazards = GetArrayValue(step2Data, "identifiedHazards") ?? new List<object>();
                foreach (var hazard in hazards)
                {
                    if (hazard is Dictionary<string, object?> hazardDict)
                    {
                        assessmentData.IdentifiedHazards.Add(new IdentifiedHazardDto
                        {
                            Id = GetStringValue(hazardDict, "id") ?? "",
                            Description = GetStringValue(hazardDict, "description") ?? "",
                            Category = GetStringValue(hazardDict, "category") ?? "",
                            FiveMComponent = GetStringValue(hazardDict, "fiveMComponent") ?? ""
                        });
                    }
                }
            }

            // 🔧 CRITICAL FIX: Populate hazard risk properties from Step 4 scoring data if available
            var step4Data = GetNestedValue(stepData, "step4") as Dictionary<string, object?>;
            if (step4Data != null && assessmentData.IdentifiedHazards.Any())
            {
                _logger.LogInformation("🔍 Found Step 4 scoring data, populating hazard risk properties");
                
                foreach (var hazard in assessmentData.IdentifiedHazards)
                {
                    if (HazardAverageScores.ContainsKey(hazard.Id))
                    {
                        var averageScore = HazardAverageScores[hazard.Id];
                        
                        // Convert average score to severity and likelihood
                        var (severity, likelihood) = ConvertScoreToSeverityLikelihood(averageScore);
                        
                        hazard.Severity = severity.ToString();
                        hazard.Likelihood = GetLikelihoodCode(likelihood);
                        hazard.RiskLevel = $"{severity}{GetLikelihoodCode(likelihood)}";
                        hazard.Tolerability = GetTolerabilityFromScore(averageScore);
                        
                        _logger.LogInformation("✅ Populated risk properties for {HazardId}: Score={Score:F1}, Risk={RiskLevel}, Tolerability={Tolerability}", 
                            hazard.Id, averageScore, hazard.RiskLevel, hazard.Tolerability);
                    }
                }
            }

            return assessmentData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error loading assessment data");
            return new AssessmentDataDto { IsFound = false };
        }
    }

    private async Task LoadDetailedStepDataAsync(string assessmentId, int stepNumber)
    {
        try
        {
            var appDataPath = SystemPath.Combine(Directory.GetCurrentDirectory(), "AppData");
            var assessmentsFile = SystemPath.Combine(appDataPath, "risk-assessments.json");

            if (!SystemFile.Exists(assessmentsFile)) return;

            var json = await SystemFile.ReadAllTextAsync(assessmentsFile);
            var assessments = SystemJsonSerializer.Deserialize<List<Dictionary<string, object?>>>(json, new SystemJsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<Dictionary<string, object?>>();

            var assessment = assessments.FirstOrDefault(a => 
                a.ContainsKey("id") && a["id"]?.ToString() == assessmentId);

            if (assessment == null) return;

            var stepData = GetNestedValue(assessment, "stepData") as Dictionary<string, object?> ?? new Dictionary<string, object?>();
            var currentStepData = GetNestedValue(stepData, $"step{stepNumber}") as Dictionary<string, object?>;

            if (currentStepData == null) 
            {
                _logger.LogInformation("No data found for Step {StepNumber} - this is normal for new steps", stepNumber);
                return;
            }

            // Load Step 1 specific data
            if (stepNumber == 1)
            {
                FiveMPersonnel = GetStringValue(currentStepData, "fiveMPersonnel") ?? "";
                FiveMEquipment = GetStringValue(currentStepData, "fiveMEquipment") ?? "";
                FiveMProcedures = GetStringValue(currentStepData, "fiveMProcedures") ?? "";
                FiveMResources = GetStringValue(currentStepData, "fiveMResources") ?? "";
                FiveMPhysicalEnvironment = GetStringValue(currentStepData, "fiveMPhysicalEnvironment") ?? "";
                FiveMOperationalEnvironment = GetStringValue(currentStepData, "fiveMOperationalEnvironment") ?? "";

                // Parse selected stakeholder groups for UI (Step 1 only)
                StakeholderGroups = GetStringValue(currentStepData, "stakeholderGroups") ?? "";
                SelectedIndividualStakeholders = GetStringValue(currentStepData, "selectedIndividualStakeholders") ?? "";
                
                if (!string.IsNullOrWhiteSpace(StakeholderGroups))
                {
                    SelectedStakeholderIds = StakeholderGroups.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .ToList();
                }

                _logger.LogInformation("✅ Loaded Step 1 detailed data - 5M fields populated");
            }
            
            // ✅ CRITICAL FIX: Load Step 3 specific data (hazard analysis)
            if (stepNumber == 3)
            {
                _logger.LogInformation("🔍 Loading Step 3 data for {AssessmentId}", assessmentId);
                
                // Load Risk Criteria
                AssessmentData.RiskCriteria = GetStringValue(currentStepData, "riskCriteria") ?? "";
                
                // Load hazard analysis data
                var hazardAnalysisData = GetArrayValue(currentStepData, "hazardAnalysisData") ?? new List<object>();
                
                _logger.LogInformation("Found {Count} hazard analysis records in Step 3 data", hazardAnalysisData.Count);
                
                // Initialize lists
                RootCauses = new List<string>();
                AdditionalComments = new List<string>();
                
                // Create dictionaries to map hazard IDs to their analysis data
                var worstOutcomesByHazardId = new Dictionary<string, string>();
                var rootCausesByHazardId = new Dictionary<string, string>();
                var commentsByHazardId = new Dictionary<string, string>();
                
                foreach (var analysisItem in hazardAnalysisData)
                {
                    if (analysisItem is Dictionary<string, object?> analysisDict)
                    {
                        var hazardId = GetStringValue(analysisDict, "hazardId") ?? "";
                        var worstOutcome = GetStringValue(analysisDict, "worstCredibleOutcome") ?? "";
                        var rootCause = GetStringValue(analysisDict, "rootCause") ?? "";
                        var comment = GetStringValue(analysisDict, "additionalComments") ?? "";
                        
                        if (!string.IsNullOrEmpty(hazardId))
                        {
                            worstOutcomesByHazardId[hazardId] = worstOutcome;
                            rootCausesByHazardId[hazardId] = rootCause;
                            commentsByHazardId[hazardId] = comment;
                            
                            _logger.LogInformation("📋 Loaded Step 3 analysis for {HazardId}: WCO={HasWCO}, RC={HasRC}", 
                                hazardId, !string.IsNullOrEmpty(worstOutcome), !string.IsNullOrEmpty(rootCause));
                        }
                    }
                }
                
                // Now populate the hazards with their analysis data AND the arrays for the form
                if (AssessmentData.IdentifiedHazards != null)
                {
                    for (int i = 0; i < AssessmentData.IdentifiedHazards.Count; i++)
                    {
                        var hazard = AssessmentData.IdentifiedHazards[i];
                        
                        // Update hazard object with analysis data
                        if (worstOutcomesByHazardId.ContainsKey(hazard.Id))
                        {
                            hazard.WorstCredibleOutcome = worstOutcomesByHazardId[hazard.Id];
                        }
                        
                        // Add to arrays for backward compatibility (index-based)
                        RootCauses.Add(rootCausesByHazardId.GetValueOrDefault(hazard.Id, ""));
                        AdditionalComments.Add(commentsByHazardId.GetValueOrDefault(hazard.Id, ""));
                    }
                }
                
                _logger.LogInformation("✅ Loaded Step 3 data - Risk Criteria: {HasCriteria}, Hazard Analysis: {Count}, Root Causes: {RootCauseCount}, Comments: {CommentCount}", 
                    !string.IsNullOrEmpty(AssessmentData.RiskCriteria), hazardAnalysisData.Count, RootCauses.Count, AdditionalComments.Count);
            }

            // Load Step 4 specific data - FIXED to load panel assignments and SMS users
            if (stepNumber == 4)
            {
                _logger.LogInformation("🔍 Loading Step 4 data for {AssessmentId}", assessmentId);
                
                // Load hazard panel assignments from the current step data
                if (currentStepData != null)
                {
                    var hazardPanelAssignments = GetNestedValue(currentStepData, "hazardPanelAssignments") as Dictionary<string, object?>;
                    if (hazardPanelAssignments != null)
                    {
                        _logger.LogInformation("📋 Found hazardPanelAssignments with {Count} entries", hazardPanelAssignments.Count);
                        
                        HazardPanelMembers = new Dictionary<string, List<string>>();
                        
                        foreach (var assignment in hazardPanelAssignments)
                        {
                            try
                            {
                                var hazardId = assignment.Key;
                                var userList = new List<string>();
                                
                                // Handle JsonElement array
                                if (assignment.Value is SystemJsonElement jsonArray && jsonArray.ValueKind == SystemJsonValueKind.Array)
                                {
                                    foreach (var userElement in jsonArray.EnumerateArray())
                                    {
                                        if (userElement.ValueKind == SystemJsonValueKind.String)
                                        {
                                            var userId = userElement.GetString();
                                            if (!string.IsNullOrEmpty(userId))
                                            {
                                                userList.Add(userId);
                                            }
                                        }
                                    }
                                }
                                // Handle regular list
                                else if (assignment.Value is IEnumerable<object> enumerable)
                                {
                                    foreach (var item in enumerable)
                                    {
                                        if (item != null)
                                        {
                                            userList.Add(item.ToString() ?? "");
                                        }
                                    }
                                }
                                
                                if (userList.Count > 0)
                                {
                                    HazardPanelMembers[hazardId] = userList;
                                    _logger.LogInformation("✅ Loaded {Count} panel members for hazard {HazardId}: {Users}", 
                                        userList.Count, hazardId, string.Join(", ", userList));
                                }
                            }
                            catch (Exception assignmentEx)
                            {
                                _logger.LogError(assignmentEx, "❌ Error processing assignment for hazard {HazardId}", assignment.Key);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogInformation("📋 No hazardPanelAssignments found in Step 4 data");
                        HazardPanelMembers = new Dictionary<string, List<string>>();
                    }
                    
                    // Load panel scores
                    var panelScores = GetNestedValue(currentStepData, "panelScores") as Dictionary<string, object?>;
                    if (panelScores != null)
                    {
                        PanelScores = new Dictionary<string, List<PanelMemberScoreData>>();
                        
                        foreach (var hazardScores in panelScores)
                        {
                            var hazardId = hazardScores.Key;
                            var scores = new List<PanelMemberScoreData>();
                            
                            // Handle JsonElement array
                            if (hazardScores.Value is SystemJsonElement jsonArray && jsonArray.ValueKind == SystemJsonValueKind.Array)
                            {
                                foreach (var scoreElement in jsonArray.EnumerateArray())
                                {
                                    if (scoreElement.ValueKind == SystemJsonValueKind.Object)
                                    {
                                        var scoreDict = new Dictionary<string, object?>();
                                        foreach (var prop in scoreElement.EnumerateObject())
                                        {
                                            scoreDict[prop.Name] = prop.Value;
                                        }
                                        
                                        var score = new PanelMemberScoreData
                                        {
                                            MemberId = GetStringValue(scoreDict, "memberId") ?? "",
                                            SeverityScore = GetIntValue(scoreDict, "severityScore") ?? 0,
                                            LikelihoodScore = GetIntValue(scoreDict, "likelihoodScore") ?? 0,
                                            IsComplete = GetBooleanValue(scoreDict, "isComplete") ?? false,
                                            ScoredDate = GetDateTimeValue(scoreDict, "scoredDate") ?? DateTime.UtcNow
                                        };
                                        scores.Add(score);
                                    }
                                }
                            }
                            
                            if (scores.Count > 0)
                            {
                                PanelScores[hazardId] = scores;
                                _logger.LogInformation("✅ Loaded {Count} scores for hazard {HazardId}", scores.Count, hazardId);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogInformation("📋 No panelScores found in Step 4 data");
                        PanelScores = new Dictionary<string, List<PanelMemberScoreData>>();
                    }
                    
                    // Load hazard average scores
                    var hazardAverageScores = GetNestedValue(currentStepData, "hazardAverageScores") as Dictionary<string, object?>;
                    if (hazardAverageScores != null)
                    {
                        HazardAverageScores = new Dictionary<string, double>();
                        foreach (var avg in hazardAverageScores)
                        {
                            var avgValue = GetDoubleValue(new Dictionary<string, object?> { { "value", avg.Value } }, "value");
                            if (avgValue.HasValue)
                            {
                                HazardAverageScores[avg.Key] = avgValue.Value;
                                _logger.LogInformation("✅ Loaded average score {Average:F1} for hazard {HazardId}", avgValue.Value, avg.Key);
                            }
                        }
                    }
                    else
                    {
                        // Calculate hazard average scores from existing panel scores
                        HazardAverageScores = new Dictionary<string, double>();
                        foreach (var hazardScores in PanelScores)
                        {
                            var completedScores = hazardScores.Value.Where(s => s.IsComplete).ToList();
                            if (completedScores.Any())
                            {
                                var averageScore = completedScores.Average(s => s.CalculatedScore);
                                HazardAverageScores[hazardScores.Key] = averageScore;
                                _logger.LogInformation("✅ Calculated average score {Average:F1} for hazard {HazardId} from {Count} completed scores", 
                                    averageScore, hazardScores.Key, completedScores.Count);
                            }
                        }
                    }
                    
                    // Load tolerability framework and risk acceptance criteria
                    //TolerabilityFramework = GetStringValue(currentStepData, "tolerabilityFramework") ?? "PDX-SMS Default Tolerability Framework";
                    //RiskAcceptanceCriteria = GetStringValue(currentStepData, "riskAcceptanceCriteria") ?? "PDX-SMS Default Acceptance Criteria";
                    
                    // Log loaded settings
                    //_logger.LogInformation("✅ Loaded Step 4 settings - Tolerability Framework: {Framework}, Risk Acceptance Criteria: {Criteria}", 
                    //    TolerabilityFramework, RiskAcceptanceCriteria);
                }
                else
                {
                    _logger.LogInformation("📋 No Step 4 data found, initializing empty collections");
                    HazardPanelMembers = new Dictionary<string, List<string>>();
                    PanelScores = new Dictionary<string, List<PanelMemberScoreData>>();
                    HazardAverageScores = new Dictionary<string, double>();
                }
                
                _logger.LogInformation("✅ Loaded Step 4 data - Panel Assignments: {PanelCount}, Scores: {ScoreCount}, Averages: {AvgCount}", 
                    HazardPanelMembers.Count, PanelScores.Values.Sum(list => list.Count), HazardAverageScores.Count);
            }

            // Load Step 5 specific data - FIXED to populate hazard risk assessment properties AND mitigation data
            if (stepNumber == 5)
            {
                _logger.LogInformation("🔍 Loading Step 5 data - populating hazard risk properties from Step 4 scoring AND mitigation data");
                
                // 🔧 CRITICAL FIX: ALWAYS load Step 4 data first to ensure hazard risk properties are populated
                var step4Data = GetNestedValue(stepData, "step4") as Dictionary<string, object?>;
                if (step4Data != null)
                {
                    // Load hazard panel assignments to preserve them
                    var hazardPanelAssignments = GetNestedValue(step4Data, "hazardPanelAssignments") as Dictionary<string, object?>;
                    if (hazardPanelAssignments != null)
                    {
                        HazardPanelMembers = new Dictionary<string, List<string>>();
                        
                        foreach (var assignment in hazardPanelAssignments)
                        {
                            try
                            {
                                var hazardId = assignment.Key;
                                var userList = new List<string>();
                                
                                // Handle JsonElement array
                                if (assignment.Value is SystemJsonElement jsonArray && jsonArray.ValueKind == SystemJsonValueKind.Array)
                                {
                                    foreach (var userElement in jsonArray.EnumerateArray())
                                    {
                                        if (userElement.ValueKind == SystemJsonValueKind.String)
                                        {
                                            var userId = userElement.GetString();
                                            if (!string.IsNullOrEmpty(userId))
                                            {
                                                userList.Add(userId);
                                            }
                                        }
                                    }
                                }
                                // Handle regular list
                                else if (assignment.Value is IEnumerable<object> enumerable)
                                {
                                    foreach (var item in enumerable)
                                    {
                                        if (item != null)
                                        {
                                            userList.Add(item.ToString() ?? "");
                                        }
                                    }
                                }
                                
                                if (userList.Count > 0)
                                {
                                    HazardPanelMembers[hazardId] = userList;
                                    _logger.LogInformation("✅ Loaded {Count} panel members for hazard {HazardId}: {Users}", 
                                        userList.Count, hazardId, string.Join(", ", userList));
                                }
                            }
                            catch (Exception assignmentEx)
                            {
                                _logger.LogError(assignmentEx, "❌ Error processing assignment for hazard {HazardId}", assignment.Key);
                            }
                        }
                    }
                    
                    // Load panel scores
                    var panelScores = GetNestedValue(step4Data, "panelScores") as Dictionary<string, object?>;
                    if (panelScores != null)
                    {
                        PanelScores = new Dictionary<string, List<PanelMemberScoreData>>();
                        
                        foreach (var hazardScores in panelScores)
                        {
                            var hazardId = hazardScores.Key;
                            var scores = new List<PanelMemberScoreData>();
                            
                            // Handle JsonElement array
                            if (hazardScores.Value is SystemJsonElement jsonArray && jsonArray.ValueKind == SystemJsonValueKind.Array)
                            {
                                foreach (var scoreElement in jsonArray.EnumerateArray())
                                {
                                    if (scoreElement.ValueKind == SystemJsonValueKind.Object)
                                    {
                                        var scoreDict = new Dictionary<string, object?>();
                                        foreach (var prop in scoreElement.EnumerateObject())
                                        {
                                            scoreDict[prop.Name] = prop.Value;
                                        }
                                        
                                        var score = new PanelMemberScoreData
                                        {
                                            MemberId = GetStringValue(scoreDict, "memberId") ?? "",
                                            SeverityScore = GetIntValue(scoreDict, "severityScore") ?? 0,
                                            LikelihoodScore = GetIntValue(scoreDict, "likelihoodScore") ?? 0,
                                            IsComplete = GetBooleanValue(scoreDict, "isComplete") ?? false,
                                            ScoredDate = GetDateTimeValue(scoreDict, "scoredDate") ?? DateTime.UtcNow
                                        };
                                        scores.Add(score);
                                    }
                                }
                            }
                            
                            if (scores.Count > 0)
                            {
                                PanelScores[hazardId] = scores;
                                _logger.LogInformation("✅ Loaded {Count} scores for hazard {HazardId}", scores.Count, hazardId);
                            }
                        }
                    }
                    
                    // Load hazard average scores
                    var hazardAverageScores = GetNestedValue(step4Data, "hazardAverageScores") as Dictionary<string, object?>;
                    if (hazardAverageScores != null)
                    {
                        HazardAverageScores = new Dictionary<string, double>();
                        foreach (var avg in hazardAverageScores)
                        {
                            var avgValue = GetDoubleValue(new Dictionary<string, object?> { { "value", avg.Value } }, "value");
                            if (avgValue.HasValue)
                            {
                                HazardAverageScores[avg.Key] = avgValue.Value;
                                _logger.LogInformation("✅ Loaded average score {Average:F1} for hazard {HazardId}", avgValue.Value, avg.Key);
                            }
                        }
                    }
                    else
                    {
                        // Calculate hazard average scores from existing panel scores
                        HazardAverageScores = new Dictionary<string, double>();
                        foreach (var hazardScores in PanelScores)
                        {
                            var completedScores = hazardScores.Value.Where(s => s.IsComplete).ToList();
                            if (completedScores.Any())
                            {
                                var averageScore = completedScores.Average(s => s.CalculatedScore);
                                HazardAverageScores[hazardScores.Key] = averageScore;
                                _logger.LogInformation("✅ Calculated average score {Average:F1} for hazard {HazardId} from {Count} completed scores", 
                                    averageScore, hazardScores.Key, completedScores.Count);
                            }
                        }
                    }
                
                    // 🔧 CRITICAL FIX: ALWAYS populate hazard risk properties from scoring data
                    if (AssessmentData?.IdentifiedHazards != null && HazardAverageScores != null)
                    {
                        _logger.LogInformation("🔧 CRITICAL FIX: Populating risk properties for all {Count} hazards", AssessmentData.IdentifiedHazards.Count);
                        
                        foreach (var hazard in AssessmentData.IdentifiedHazards)
                        {
                            if (HazardAverageScores.ContainsKey(hazard.Id))
                            {
                                var averageScore = HazardAverageScores[hazard.Id];
                                
                                // Convert average score to severity and likelihood
                                var (severity, likelihood) = ConvertScoreToSeverityLikelihood(averageScore);
                                
                                hazard.Severity = severity.ToString();
                                hazard.Likelihood = GetLikelihoodCode(likelihood);
                                hazard.RiskLevel = $"{severity}{GetLikelihoodCode(likelihood)}";
                                hazard.Tolerability = GetTolerabilityFromScore(averageScore);
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("📋 No Step 4 data found - hazards may not have risk assessment data");
                    HazardPanelMembers = new Dictionary<string, List<string>>();
                    PanelScores = new Dictionary<string, List<PanelMemberScoreData>>();
                    HazardAverageScores = new Dictionary<string, double>();
                }
                
                // 🔧 CRITICAL FIX: Load Step 5 mitigation data and populate form fields AND JavaScript data structures
                var step5Data = GetNestedValue(stepData, "step5") as Dictionary<string, object?>;
                if (step5Data != null && AssessmentData?.IdentifiedHazards != null)
                {
                    _logger.LogInformation("🔍 Found Step 5 data, loading saved mitigation strategies and residual risk assessments");
                    
                    // 🔧 NEW: Load implementation planning data into form-bound properties
                    AssessmentData.ImplementationPlan = GetStringValue(step5Data, "implementationStrategy") ?? "";
                    SavedImplementationStrategy = GetStringValue(step5Data, "implementationStrategy") ?? "";
                    SavedOverallTargetDate = GetStringValue(step5Data, "overallTargetDate") ?? "";
                    SavedImplementationNotes = GetStringValue(step5Data, "implementationNotes") ?? "";
                    
                    _logger.LogInformation("✅ Loaded implementation planning: Strategy={Strategy}, Target={Target}", 
                        SavedImplementationStrategy, SavedOverallTargetDate);
                    
                    // Initialize collections for JavaScript consumption
                    SavedMitigationStrategies = new Dictionary<string, List<object>>();
                    SavedResidualRiskAssessments = new Dictionary<string, object?>();
                    SavedMonitoringRequirements = new Dictionary<string, object?>();
                    
                    // Load hazard mitigations
                    var hazardMitigations = GetArrayValue(step5Data, "hazardMitigations") ?? new List<object>();
                    
                    foreach (var mitigationItem in hazardMitigations)
                    {
                        if (mitigationItem is Dictionary<string, object?> mitigationDict)
                        {
                            var hazardId = GetStringValue(mitigationDict, "hazardId");
                            if (string.IsNullOrEmpty(hazardId)) continue;
                            
                            // Find the corresponding hazard in AssessmentData
                            var hazard = AssessmentData.IdentifiedHazards.FirstOrDefault(h => h.Id == hazardId);
                            if (hazard == null) continue;
                            
                            // Load mitigation strategies for JavaScript reconstruction
                            var strategies = GetArrayValue(mitigationDict, "mitigationStrategies") ?? new List<object>();
                            if (strategies.Any())
                            {
                                SavedMitigationStrategies[hazardId] = strategies;
                                _logger.LogInformation("✅ Prepared {Count} mitigation strategies for {HazardId} JavaScript loading", strategies.Count, hazardId);
                            }
                            
                            // Load residual risk assessment for JavaScript reconstruction
                            var residualRisk = GetNestedValue(mitigationDict, "residualRiskAssessment") as Dictionary<string, object?>;
                            if (residualRisk != null)
                            {
                                SavedResidualRiskAssessments[hazardId] = residualRisk;
                                _logger.LogInformation("✅ Prepared residual risk assessment for {HazardId} JavaScript loading", hazardId);
                            }
                            
                            // Load monitoring requirements for JavaScript reconstruction
                            var monitoring = GetNestedValue(mitigationDict, "monitoringRequirements") as Dictionary<string, object?>;
                            if (monitoring != null)
                            {
                                SavedMonitoringRequirements[hazardId] = monitoring;
                                _logger.LogInformation("✅ Prepared monitoring requirements for {HazardId} JavaScript loading", hazardId);
                            }
                            
                            // 🔧 CRITICAL FIX: Update hazard object for basic display with saved mitigation strategies
                            var proposedMitigations = new List<string>();
                            foreach (var strategy in strategies)
                            {
                                if (strategy is Dictionary<string, object?> strategyDict)
                                {
                                    var description = GetStringValue(strategyDict, "description");
                                    var controlType = GetStringValue(strategyDict, "controlType");
                                    var priority = GetStringValue(strategyDict, "priority");
                                    
                                    if (!string.IsNullOrEmpty(description))
                                    {
                                        var strategyText = $"{controlType}: {description}";
                                        if (!string.IsNullOrEmpty(priority))
                                        {
                                            strategyText += $" (Priority: {priority})";
                                        }
                                        proposedMitigations.Add(strategyText);
                                    }
                                }
                            }
                            
                            if (proposedMitigations.Any())
                            {
                                hazard.ProposedMitigations = proposedMitigations;
                            }
                        }
                    }
                    
                    _logger.LogInformation("✅ Step 5 mitigation data prepared successfully - {StrategiesCount} hazards with strategies, {ResidualCount} with residual risk, {MonitoringCount} with monitoring", 
                        SavedMitigationStrategies.Count, SavedResidualRiskAssessments.Count, SavedMonitoringRequirements.Count);
                }
                else
                {
                    _logger.LogInformation("📋 No Step 5 mitigation data found - this is normal for new Step 5 entries");
                    
                    // Initialize empty collections for new Step 5
                    SavedMitigationStrategies = new Dictionary<string, List<object>>();
                    SavedResidualRiskAssessments = new Dictionary<string, object?>();
                    SavedMonitoringRequirements = new Dictionary<string, object?>();
                    SavedImplementationStrategy = "";
                    SavedOverallTargetDate = "";
                    SavedImplementationNotes = "";
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error loading detailed step data for Step {StepNumber}", stepNumber);
            // Don't throw - just log the error and continue
        }
    }

    private async Task SaveStepDataToAssessmentFile(string assessmentId, int stepNumber, Dictionary<string, object?> stepData)
    {
        try
        {
            var appDataPath = SystemPath.Combine(Directory.GetCurrentDirectory(), "AppData");
            var assessmentsFile = SystemPath.Combine(appDataPath, "risk-assessments.json");

            if (!SystemFile.Exists(assessmentsFile)) return;

            var json = await SystemFile.ReadAllTextAsync(assessmentsFile);
            var assessments = SystemJsonSerializer.Deserialize<List<Dictionary<string, object?>>>(json, new SystemJsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<Dictionary<string, object?>>();

            var assessment = assessments.FirstOrDefault(a => 
                a.ContainsKey("id") && a["id"]?.ToString() == assessmentId);

            if (assessment == null) return;

            var allStepData = GetNestedValue(assessment, "stepData") as Dictionary<string, object?> ?? new Dictionary<string, object?>();
            allStepData[$"step{stepNumber}"] = stepData;
            assessment["stepData"] = allStepData;

            var currentStepValue = GetIntValue(assessment, "currentStep") ?? 1;
            assessment["currentStep"] = Math.Max(currentStepValue, stepNumber);
            assessment["lastModifiedDate"] = DateTime.UtcNow;
            assessment["status"] = "In Progress";

            var updatedJson = SystemJsonSerializer.Serialize(assessments, new SystemJsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = SystemJsonNamingPolicy.CamelCase
            });

            await SystemFile.WriteAllTextAsync(assessmentsFile, updatedJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error saving step data");
            throw;
        }
    }

    private async Task<bool> HazardExistsInRepository(string hazardId)
    {
        try
        {
            var appDataPath = SystemPath.Combine(Directory.GetCurrentDirectory(), "AppData");
            var hazardsFile = SystemPath.Combine(appDataPath, "hazards.json");

            if (!SystemFile.Exists(hazardsFile)) return false;

            var json = await SystemFile.ReadAllTextAsync(hazardsFile);
            var hazards = SystemJsonSerializer.Deserialize<List<Dictionary<string, object?>>>(json, new SystemJsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<Dictionary<string, object>>();

            return hazards.Any(h => h.ContainsKey("id") && h["id"]?.ToString() == hazardId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking hazard existence");
            return false;
        }
    }

    private async Task SaveNewHazardsToRepository(List<Dictionary<string, object?>> newHazards)
    {
        try
        {
            var appDataPath = SystemPath.Combine(Directory.GetCurrentDirectory(), "AppData");
            var hazardsFile = SystemPath.Combine(appDataPath, "hazards.json");

            var existingHazards = new List<Dictionary<string, object?>>();
            if (SystemFile.Exists(hazardsFile))
            {
                var json = await SystemFile.ReadAllTextAsync(hazardsFile);
                existingHazards = SystemJsonSerializer.Deserialize<List<Dictionary<string, object?>>>(json, new SystemJsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Dictionary<string, object?>>();
            }

            existingHazards.AddRange(newHazards);

            var updatedJson = SystemJsonSerializer.Serialize(existingHazards, new SystemJsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = SystemJsonNamingPolicy.CamelCase
            });

            await SystemFile.WriteAllTextAsync(hazardsFile, updatedJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error saving new hazards to repository");
            throw;
        }
    }

    #endregion

    public async Task<IActionResult> OnPostCompleteAssessmentAsync()
    {
        try
        {
            _logger.LogInformation("🎯 Completing Assessment {AssessmentId}", Id);

            // Validate required data
            if (string.IsNullOrWhiteSpace(Id))
            {
                TempData["ErrorMessage"] = "Assessment ID is required";
                return Page();
            }

            // First, save Step 5 data if we're on Step 5
            if (StepNumber == 5)
            {
                _logger.LogInformation("💾 Auto-saving Step 5 before completion");
                var saveResult = await OnPostSaveStep5Async();
                
                if (saveResult is JsonResult jsonResult)
                {
                    // Check if save was successful
                    var resultValue = jsonResult.Value;
                    var successProperty = resultValue?.GetType().GetProperty("success");
                    var isSuccess = (bool?)successProperty?.GetValue(resultValue) ?? false;
                    
                    if (!isSuccess)
                    {
                        var messageProperty = resultValue?.GetType().GetProperty("message");
                        var message = messageProperty?.GetValue(resultValue)?.ToString() ?? "Unknown error";
                        
                        _logger.LogError("❌ Failed to save Step 5 before completion: {Message}", message);
                        TempData["ErrorMessage"] = $"Failed to save Step 5: {message}";
                        return Page();
                    }
                    
                    _logger.LogInformation("✅ Step 5 auto-saved successfully before completion");
                }
            }

            // Load current assessment data
            var assessmentData = await LoadAssessmentDataAsync(Id);
            
            if (!assessmentData.IsFound)
            {
                _logger.LogError("❌ Assessment {AssessmentId} not found for completion", Id);
                TempData["ErrorMessage"] = "Assessment not found";
                return RedirectToPage("/SafetyRiskManagement/RiskAssessment");
            }

            // Mark assessment as completed by updating status and currentStep
            await UpdateAssessmentCompletionStatus(Id);

            _logger.LogInformation("🎉 Assessment {AssessmentId} completed successfully!", Id);
            
            TempData["SuccessMessage"] = "Risk Assessment completed successfully! All 5 steps have been saved.";
            
            // Redirect to assessment list or summary page
            return RedirectToPage("/SafetyRiskManagement/RiskAssessment");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error completing Assessment {AssessmentId}", Id);
            TempData["ErrorMessage"] = $"Error completing assessment: {ex.Message}";
            return Page();
        }
    }

    /// <summary>
    /// Update assessment completion status in the JSON file
    /// </summary>
    private async Task UpdateAssessmentCompletionStatus(string assessmentId)
    {
        try
        {
            var appDataPath = SystemPath.Combine(Directory.GetCurrentDirectory(), "AppData");
            var assessmentsFile = SystemPath.Combine(appDataPath, "risk-assessments.json");

            if (!SystemFile.Exists(assessmentsFile)) return;

            var json = await SystemFile.ReadAllTextAsync(assessmentsFile);
            var assessments = SystemJsonSerializer.Deserialize<List<Dictionary<string, object?>>>(json, new SystemJsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<Dictionary<string, object?>>();

            var assessment = assessments.FirstOrDefault(a => 
                a.ContainsKey("id") && a["id"]?.ToString() == assessmentId);

            if (assessment != null)
            {
                // Update completion status
                assessment["status"] = "Completed";
                assessment["currentStep"] = 5; // Mark all 5 steps as complete
                assessment["completedDate"] = DateTime.UtcNow;
                assessment["lastModifiedDate"] = DateTime.UtcNow;

                var updatedJson = SystemJsonSerializer.Serialize(assessments, new SystemJsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = SystemJsonNamingPolicy.CamelCase
                });

                await SystemFile.WriteAllTextAsync(assessmentsFile, updatedJson);
                
                _logger.LogInformation("✅ Updated assessment {AssessmentId} status to Completed", assessmentId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error updating assessment completion status");
            throw;
        }
    }

    /// <summary>
    /// Get numeric risk level for calculations - supports residual risk comparisons
    /// </summary>
    public int GetRiskLevelNumeric(string? riskLevel)
    {
        if (string.IsNullOrEmpty(riskLevel)) return 0;

        // Extract the numeric part from risk levels like "1A", "2B", etc.
        if (riskLevel.Length >= 1 && char.IsDigit(riskLevel[0]))
        {
            if (int.TryParse(riskLevel[0].ToString(), out int result))
            {
                return result;
            }
        }

        // Fallback for text-based risk levels
        return riskLevel.ToUpper() switch
        {
            var level when level.Contains("VERY LOW") || level.Contains("1") => 1,
            var level when level.Contains("LOW") || level.Contains("2") => 2,
            var level when level.Contains("MEDIUM") || level.Contains("3") => 3,
            var level when level.Contains("HIGH") || level.Contains("4") => 4,
            var level when level.Contains("VERY HIGH") || level.Contains("5") => 5,
            _ => 0
        };
    }

    /// <summary>
    /// Save individual mitigation strategy (called from JavaScript when strategies are added/modified)
    /// </summary>
    public async Task<IActionResult> OnPostSaveMitigationStrategyAsync()
    {
        try
        {
            var hazardId = Request.Form["hazardId"].ToString();
            var hazardIndex = int.Parse(Request.Form["hazardIndex"].ToString());
            var strategyIndex = int.Parse(Request.Form["strategyIndex"].ToString());
            var strategyDataJson = Request.Form["strategyData"].ToString();
            var assessmentId = Request.Form["Id"].ToString();
            
            _logger.LogInformation("💾 Saving mitigation strategy - Assessment: {AssessmentId}, Hazard: {HazardId}, Strategy: {StrategyIndex}", 
                assessmentId, hazardId, strategyIndex);
            
            if (string.IsNullOrEmpty(hazardId) || string.IsNullOrEmpty(assessmentId) || string.IsNullOrEmpty(strategyDataJson))
            {
                return new JsonResult(new { success = false, message = "Missing required parameters" });
            }
            
            // Parse strategy data
            var strategyData = SystemJsonSerializer.Deserialize<Dictionary<string, object?>>(strategyDataJson, new SystemJsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            if (strategyData == null)
            {
                return new JsonResult(new { success = false, message = "Invalid strategy data" });
            }
            
            // Load current assessment data
            AssessmentData = await LoadAssessmentDataAsync(assessmentId);
            await LoadDetailedStepDataAsync(assessmentId, 5);
            
            if (!AssessmentData.IsFound)
            {
                return new JsonResult(new { success = false, message = "Assessment not found" });
            }
            
            // Get or create Step 5 data structure
            var appDataPath = SystemPath.Combine(Directory.GetCurrentDirectory(), "AppData");
            var assessmentsFile = SystemPath.Combine(appDataPath, "risk-assessments.json");
            
            if (!SystemFile.Exists(assessmentsFile))
            {
                return new JsonResult(new { success = false, message = "Assessment file not found" });
            }
            
            var json = await SystemFile.ReadAllTextAsync(assessmentsFile);
            var assessments = SystemJsonSerializer.Deserialize<List<Dictionary<string, object?>>>(json, new SystemJsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<Dictionary<string, object?>>();

            var assessment = assessments.FirstOrDefault(a => 
                a.ContainsKey("id") && a["id"]?.ToString() == assessmentId);
            
            if (assessment == null)
            {
                return new JsonResult(new { success = false, message = "Assessment not found in file" });
            }
            
            // Get or create stepData and step5
            var stepData = GetNestedValue(assessment, "stepData") as Dictionary<string, object?> ?? new Dictionary<string, object?>();
            var step5Data = GetNestedValue(stepData, "step5") as Dictionary<string, object?> ?? new Dictionary<string, object?>();
            
            // Get or create hazardMitigations array
            var hazardMitigations = GetArrayValue(step5Data, "hazardMitigations") ?? new List<object>();
            
            // Find or create hazard mitigation entry
            Dictionary<string, object?>? hazardMitigation = null;
            foreach (var item in hazardMitigations)
            {
                if (item is Dictionary<string, object?> dict && 
                    GetStringValue(dict, "hazardId") == hazardId)
                {
                    hazardMitigation = dict;
                    break;
                }
            }
            
            // Create new hazard mitigation if not found
            if (hazardMitigation == null)
            {
                hazardMitigation = new Dictionary<string, object?>
                {
                    ["hazardId"] = hazardId,
                    ["hazardDescription"] = GetHazardDescription(hazardId),
                    ["mitigationStrategies"] = new List<object>(),
                    ["createdDate"] = DateTime.UtcNow
                };
                hazardMitigations.Add(hazardMitigation);
            }
            
            // Get or create mitigation strategies array
            var strategies = GetArrayValue(hazardMitigation, "mitigationStrategies") ?? new List<object>();
            
            // Find or create the specific strategy
            Dictionary<string, object?>? strategy = null;
            if (strategyIndex < strategies.Count && strategies[strategyIndex] is Dictionary<string, object?> existingStrategy)
            {
                strategy = existingStrategy;
            }
            else
            {
                // Create new strategy and ensure the list is large enough
                while (strategies.Count <= strategyIndex)
                {
                    strategies.Add(new Dictionary<string, object?>());
                }
                strategy = strategies[strategyIndex] as Dictionary<string, object?> ?? new Dictionary<string, object?>();
                strategies[strategyIndex] = strategy;
            }
            
            // Update strategy with new data
            strategy["description"] = GetStringValue(strategyData, "description") ?? "";
            strategy["controlType"] = GetStringValue(strategyData, "controlType") ?? "";
            strategy["priority"] = GetStringValue(strategyData, "priority") ?? "";
            strategy["targetDate"] = GetStringValue(strategyData, "targetDate") ?? "";
            strategy["estimatedCost"] = GetDoubleValue(strategyData, "estimatedCost") ?? 0.0;
            strategy["status"] = GetStringValue(strategyData, "status") ?? "Planned";
            strategy["lastModifiedDate"] = DateTime.UtcNow;
            
            // Handle assignments
            if (strategyData.ContainsKey("assignments") && strategyData["assignments"] is SystemJsonElement assignmentsElement)
            {
                var assignments = new List<object>();
                foreach (var assignmentElement in assignmentsElement.EnumerateArray())
                {
                    var assignmentDict = new Dictionary<string, object?>();
                    foreach (var prop in assignmentElement.EnumerateObject())
                    {
                        assignmentDict[prop.Name] = prop.Value.GetString();
                    }
                    assignments.Add(assignmentDict);
                }
                strategy["assignments"] = assignments;
            }
            
            // Update the hazard mitigation strategies
            hazardMitigation["mitigationStrategies"] = strategies;
            hazardMitigation["lastModifiedDate"] = DateTime.UtcNow;
            
            // Update step5 data
            step5Data["hazardMitigations"] = hazardMitigations;
            step5Data["totalHazardsWithMitigation"] = hazardMitigations.Count;
            step5Data["mitigationPlanningComplete"] = hazardMitigations.Count > 0;
            step5Data["lastModifiedDate"] = DateTime.UtcNow;
            
            // Update assessment
            stepData["step5"] = step5Data;
            assessment["stepData"] = stepData;
            assessment["lastModifiedDate"] = DateTime.UtcNow;
            
            // Save back to file
            var updatedJson = SystemJsonSerializer.Serialize(assessments, new SystemJsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = SystemJsonNamingPolicy.CamelCase
            });
            
            await SystemFile.WriteAllTextAsync(assessmentsFile, updatedJson);
            
            _logger.LogInformation("✅ Successfully saved mitigation strategy {StrategyIndex} for hazard {HazardId}", 
                strategyIndex, hazardId);
            
            return new JsonResult(new { 
                success = true, 
                message = "Mitigation strategy saved successfully",
                hazardId = hazardId,
                strategyIndex = strategyIndex
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error saving mitigation strategy");
            return new JsonResult(new { success = false, message = "Error saving mitigation strategy: " + ex.Message });
        }
    }
    
    /// <summary>
    /// Helper method to get hazard description for mitigation entries
    /// </summary>
    private string GetHazardDescription(string hazardId)
    {
        try
        {
            if (AssessmentData?.IdentifiedHazards != null)
            {
                var hazard = AssessmentData.IdentifiedHazards.FirstOrDefault(h => h.Id == hazardId);
                if (hazard != null)
                {
                    return hazard.Description;
                }
            }
            return $"Hazard {hazardId}";
        }
        catch
        {
            return $"Hazard {hazardId}";
        }
    }

    /// <summary>
    /// Handle residual risk panel score submission (Step 5 specific)
    /// </summary>
    public async Task<IActionResult> OnPostSubmitResidualScoreAsync()
    {
        try
        {
            var hazardId = Request.Form["hazardId"].ToString(); // This will be "residual-HZ-xxxx"
            var memberId = Request.Form["memberId"].ToString();
            var severityScore = int.Parse(Request.Form["severityScore"]);
            var likelihoodScore = int.Parse(Request.Form["likelihoodScore"]);
            var assessmentId = Request.Form["Id"].ToString();
            
            _logger.LogInformation("📤 Submitting residual score - HazardId: {HazardId}, MemberId: {MemberId}, Severity: {Severity}, Likelihood: {Likelihood}", 
                hazardId, memberId, severityScore, likelihoodScore);
            
            if (string.IsNullOrEmpty(hazardId) || string.IsNullOrEmpty(memberId) || string.IsNullOrEmpty(assessmentId))
            {
                return new JsonResult(new { success = false, message = "Missing required parameters" });
            }
            
            if (severityScore < 1 || severityScore > 5 || likelihoodScore < 1 || likelihoodScore > 5)
            {
                return new JsonResult(new { success = false, message = "Invalid severity or likelihood scores" });
            }
            
            // Load current assessment data
            AssessmentData = await LoadAssessmentDataAsync(assessmentId);
            await LoadDetailedStepDataAsync(assessmentId, 4); // Load Step 4 data where panel scores are stored
            
            // Ensure collections are initialized
            if (PanelScores == null)
                PanelScores = new Dictionary<string, List<PanelMemberScoreData>>();
                
            if (HazardPanelMembers == null)
                HazardPanelMembers = new Dictionary<string, List<string>>();
            
            // Initialize residual hazard scores if not exists
            if (!PanelScores.ContainsKey(hazardId))
                PanelScores[hazardId] = new List<PanelMemberScoreData>();
            
            // Find or create the member score for residual risk
            var memberScore = PanelScores[hazardId].FirstOrDefault(s => s.MemberId == memberId);
            if (memberScore == null)
            {
                memberScore = new PanelMemberScoreData
                {
                    MemberId = memberId
                };
                PanelScores[hazardId].Add(memberScore);
            }
            
            // Update the residual risk score
            memberScore.SeverityScore = severityScore;
            memberScore.LikelihoodScore = likelihoodScore;
            memberScore.IsComplete = true;
            memberScore.ScoredDate = DateTime.UtcNow;
            
            // Calculate residual hazard average
            var completedScores = PanelScores[hazardId].Where(s => s.IsComplete).ToList();
            double? hazardAverage = null;
            if (completedScores.Any())
            {
                hazardAverage = completedScores.Average(s => s.CalculatedScore);
                if (HazardAverageScores == null)
                    HazardAverageScores = new Dictionary<string, double>();
                HazardAverageScores[hazardId] = hazardAverage.Value;
            }
            
            // Save updated data - include residual scores in Step 4 data structure
            var step4Data = new Dictionary<string, object?>
            {
                ["hazardPanelAssignments"] = HazardPanelMembers.ToDictionary(
                    kvp => kvp.Key, 
                    kvp => kvp.Value.Cast<object>().ToList()
                ),
                ["panelScores"] = PanelScores.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Select(score => new Dictionary<string, object?>
                    {
                        ["memberId"] = score.MemberId,
                        ["severityScore"] = score.SeverityScore,
                        ["likelihoodScore"] = score.LikelihoodScore,
                        ["isComplete"] = score.IsComplete,
                        ["calculatedScore"] = score.CalculatedScore,
                        ["scoredDate"] = score.ScoredDate
                    }).Cast<object>().ToList()
                ),
                ["hazardAverageScores"] = HazardAverageScores?.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value),
                ["lastScoreUpdateDate"] = DateTime.UtcNow
            };
            
            await SaveStepDataToAssessmentFile(assessmentId, 4, step4Data);
            
            _logger.LogInformation("✅ Residual score submitted successfully for {MemberId} on {HazardId}. Calculated score: {Score}. Hazard average: {Average}", 
                memberId, hazardId, memberScore.CalculatedScore, hazardAverage);
            
            return new JsonResult(new { 
                success = true, 
                message = $"Residual score submitted successfully",
                hazardId = hazardId,
                memberId = memberId,
                calculatedScore = memberScore.CalculatedScore,
                hazardAverage = hazardAverage,
                completedScores = completedScores.Count,
                totalPanelMembers = HazardPanelMembers.GetValueOrDefault(hazardId, new List<string>()).Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error submitting residual score");
            return new JsonResult(new { success = false, message = "Error submitting residual score: " + ex.Message });
        }
    }
}
 
/// <summary>
/// Simple SMS user class for Step 4 modal (bypasses StakeholderUserData issues)
/// </summary>
