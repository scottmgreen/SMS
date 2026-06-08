using System.Runtime.Intrinsics.X86;

using SMS_Application.Interfaces;
using SMS_Application.Services;
using SMS_Domain.Events;

using SMS3.Components.Pages.SMSRiskManagement.Models;
using SMS3.Components.Shared;
using SMS3.Components.Shared.UIHelpers;
using SMS3.Configuration.Extensions;

namespace SMS3.Components.Pages.SMSRiskManagement;

/// <summary>
/// Technical Assessment - Comprehensive 5-step SMS risk assessment methodology
/// </summary>
public partial class TechnicalAssessment : ComponentBase
{
    
    #region Parameters and Injection

    [Parameter] public string? ReportId { get; set; }
    [Parameter] public string? HazardId { get; set; }  // Now a route parameter
    [Parameter] public string? StepNumber { get; set; } = "1";
    [Parameter]
    [SupplyParameterFromQuery(Name = "returnTo")]
    public string? ReturnTo { get; set; }

    // Keep query parameters for backward compatibility
    // [SupplyParameterFromQuery(Name = "reportId")] public string? ReportId { get; set; }
    [Inject] private ICurrentUserService _currentUserService { get; set; } = default!;
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<TechnicalAssessment> _logger { get; set; } = default!;
    [Inject] private NavigationManager _navigation { get; set; } = default!;
    [Inject] private INotificationHelper _notificationHelper { get; set; } = default!;
    [Inject] private IBaseEventBus _eventBus { get; set; } = default!;
    [Inject] private IConfiguration _configuration { get; set; } = default!;
    


    #endregion

    #region State Properties

    private bool IsLoading { get; set; } = true;
    private bool IsSaving { get; set; } = false;
    private bool ForceTechnicalAssessmentWorkflow => _configuration.GetValue<bool?>("FeatureManagement:ForceTechnicalAssessmentWorkflow") ?? true;
    private string? LastLoadedReportId { get; set; }
    private string? LastLoadedHazardId { get; set; }
    public int CurrentStep => int.TryParse(StepNumber, out int step) && step >= 1 && step <= 5 ? step : 1;
    private bool IsRiskRegistryOnly =>
        TechRiskAssessment?.AssessmentType == RiskAssessmentType.RiskRegistryOnly ||
        string.Equals(SourceReport?.Status, ReportStatus.RiskRegistryOnly.Value, StringComparison.OrdinalIgnoreCase);
    private int MaxAssessmentStep
    {
        get
        {
            if (TechRiskAssessment?.AssessmentType == RiskAssessmentType.RiskRegistryOnly)
            {
                return 4;
            }

            return TechRiskAssessment?.RiskAssessmentCategory?.GetMaxStep() ?? 5;
        }
    }
    private bool IsAssessmentModeResolved => TechRiskAssessment is not null || SourceReport is not null;

    #endregion

    #region Core Assessment Data

    public RiskAssessment? TechRiskAssessment { get; set; }
    
    public RiskAnalysis? TechRiskAnalysis { get; set; }
    
    public Hazard? PrimaryHazard { get; set; }
    public Report? SourceReport { get; set; }
    public List<Hazard> ReportHazards { get; set; } = new();
    // Snapshot list used for child component parameter change detection.
    public List<Hazard> HazardUiSnapshot { get; private set; } = new();
    #endregion

    #region Step Models

    public Step1Model Step1 { get; set; } = new();
    public Step2Model Step2 { get; set; } = new();
    public Step3Model Step3 { get; set; } = default!;  // Will be initialized in OnInitializedAsync
    public Step4Model Step4 { get; set; } = default!;  // Will be initialized in OnInitializedAsync
    public Step5Model Step5 { get; set; } = default!;  // Will be initialized in OnInitializedAsync

    #endregion

    #region UI Helper Properties

    public string AssessmentName => GetCurrentAssessmentName();
    public string LeadAssessorName => AvailableAssessors.FirstOrDefault(a => a.UserName.Value == Step1.LeadAssessor)?.DisplayName ?? Step1.LeadAssessor;
    public string LeadInvestigatorName => AvailableInvestigators.FirstOrDefault()?.DisplayName ?? "Not Assigned";

    

    
    #endregion

    #region UI Helper Methods
    private string GetCurrentAssessmentName()
    {
        var stepName = GetStepName(CurrentStep);

        // For Step 5, emphasize it's the Residual stage
        if (CurrentStep == 5)
        {
            return $"Hazard Report: {ReportId} Risk Assessment: {TechRiskAssessment?.Code} - Residual Stage";
        }

        // For Steps 1-4, show Technical stage
        return $"Hazard Report: {ReportId} Risk Assessment: {TechRiskAssessment?.Code} - Technical Stage";
    }
    public string GetStepName(int stepNumber)
    {
        return stepNumber switch
        {
            1 => "System Description",
            2 => "Hazard Identification",
            3 => "Risk Analysis",
            4 => "Technical Risk Assessment",
            5 => "Risk Mitigation and Residual Risk Assesment",
            _ => "Unknown Step"
        };
    }
    public string GetStepIcon(int stepNumber)
    {
        return stepNumber switch
        {
            1 => "settings",
            2 => "warning",
            3 => "analytics",
            4 => "balance",
            5 => "shield",
            _ => "help"
        };
    }

    #endregion

    #region Reference Data

    public List<SMSApplicationUser> AvailableAssessors { get; set; } = new();
    public List<SMSApplicationUser> AvailableInvestigators { get; set; } = new();
    public List<SMSStakeholderUser> AvailableStakeholders { get; set; } = new();
    public List<SMSApplicationUser> AvailableSMSUsers { get; set; } = new();
    public List<SMSStakeholderGroup> StakeholderGroups { get; set; } = new();

    #endregion

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        _logger.LogInformation("TechnicalAssessment OnInitializedAsync - ReportId: {ReportId}, StepNumber: {StepNumber}, HazardId: {HazardId}", ReportId, StepNumber, HazardId);

        // Initialize step models that require dependency injection
        Step3 = new Step3Model(_mediator, _currentUserService);
        Step4 = new Step4Model(_mediator, _currentUserService);
        Step5 = new Step5Model(_mediator, _currentUserService);



        // If no step number provided, redirect to step 1
        if (string.IsNullOrEmpty(StepNumber) || CurrentStep < 1 || CurrentStep > 5)
        {
            await NavigateToStep(1);
            return;
        }

        await LoadAssessmentDataAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        _logger.LogInformation("TechnicalAssessment OnParametersSetAsync - ReportId: {ReportId}, StepNumber: {StepNumber}, HazardId: {HazardId}", ReportId, StepNumber, HazardId);

        ReportId = NormalizeRouteSegment(ReportId);
        HazardId = NormalizeRouteSegment(HazardId);
        StepNumber = NormalizeRouteSegment(StepNumber) ?? "1";

        // Handle route parameter changes
        var currentStep = CurrentStep;
        if (currentStep < 1 || currentStep > 5)
        {
            _logger.LogWarning("Invalid step {CurrentStep}, redirecting to step 1", currentStep);
            await NavigateToStep(1);
            return;
        }

        var routeContextChanged = !string.Equals(LastLoadedReportId, ReportId, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(LastLoadedHazardId, HazardId, StringComparison.OrdinalIgnoreCase);

        if (TechRiskAssessment is null || routeContextChanged)
        {
            _logger.LogInformation("Route context changed or assessment missing. Reloading data. LastReportId: {LastReportId}, NewReportId: {NewReportId}, LastHazardId: {LastHazardId}, NewHazardId: {NewHazardId}",
                LastLoadedReportId, ReportId, LastLoadedHazardId, HazardId);

            await LoadAssessmentDataAsync();
            LastLoadedReportId = ReportId;
            LastLoadedHazardId = HazardId;
        }
        
        if (currentStep == 3)
        {
            _logger.LogInformation("Navigating to Step 3 - reloading Step3 data to ensure HazardRiskAnalyses is complete");
            // FIRST: Refresh ReportHazards to include any newly added hazards from Step 2
            await LoadReportHazardsAsync();

            if (TechRiskAssessment is not null && ReportHazards?.Any() == true)
            {
                await Step3.LoadFromAssessmentAsync(TechRiskAssessment, ReportHazards);
            }
            else
            {
                _logger.LogWarning("Cannot reload Step3 data - missing TechRiskAssessment or ReportHazards");
            }
        }
        // Log the step change
        if (StepNumber != null)
        {
            _logger.LogInformation("Parameter change detected - Step: {StepNumber}", StepNumber);
        }

        await InvokeAsync(StateHasChanged);

        _logger.LogInformation("OnParametersSetAsync completed - Current step: {CurrentStep}", CurrentStep);
    }

    #endregion

    #region Data Loading Methods
    private async Task LoadSourceReport()
    {
        SourceReport = null;

        if (string.IsNullOrWhiteSpace(ReportId))
        {
            return;
        }

        try
        {
            var query = new GetReportByCodeQuery(new ReportID(ReportId));
            var result = await _mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value is not null)
            {
                SourceReport = result.Value;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading source report for ReportId: {ReportId}", ReportId);
        }
    }
    private async Task LoadAssessmentDataAsync()
    {
        try
        {
            IsLoading = true;
            StateHasChanged();

            _logger.LogInformation("Loading Technical Assessment - Step {StepNumber}, ReportId: {ReportId}, HazardId: {HazardId}",CurrentStep, ReportId, HazardId);
            if (SourceReport is null || !string.Equals(SourceReport.Code, ReportId, StringComparison.OrdinalIgnoreCase))
            {
                await LoadSourceReport();
            }

            if (IsRiskRegistryOnly && CurrentStep != 4)
            {
                var encodedReportId = Uri.EscapeDataString(ReportId ?? string.Empty);
                var encodedHazardId = Uri.EscapeDataString(HazardId ?? string.Empty);
                var returnToQuery = !string.IsNullOrWhiteSpace(ReturnTo)
                    ? $"?returnTo={Uri.EscapeDataString(ReturnTo)}"
                    : string.Empty;
                var riskRegistryOnlyUrl = $"/SMSRiskManagement/TechnicalAssessment/{encodedReportId}/{encodedHazardId}/4{returnToQuery}";
                _logger.LogInformation("Risk Registry Only report detected. Redirecting to Step 4: {Url}", riskRegistryOnlyUrl);
                _navigation.NavigateToSecure(riskRegistryOnlyUrl);
                return;
            }

            await LoadCoreAssessmentDataAsync();
            await LoadReportHazardsAsync(); // Load hazards BEFORE loading step data
            await LoadStepDataFromAssessment(); // Step models need ReportHazards to be populated
            await LoadReferenceDataAsync();

            _logger.LogInformation("Successfully loaded Technical Assessment data");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Technical Assessment data");
            await _notificationHelper.ShowErrorAsync("Failed to load assessment data. Please try again.");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task LoadCoreAssessmentDataAsync()
    {
        // Start with the strategy of finding assessments based on what parameters we have
        if (!string.IsNullOrWhiteSpace(HazardId))
        {
            // Primary method: Load by HazardId (this is the main approach)
            await LoadAssessmentsByHazardCodeAsync();
            
        }
        else if (!string.IsNullOrWhiteSpace(ReportId))
        {
            // Secondary method: Load by ReportId and find hazards, then assessments
            await LoadAssessmentByReportCodeAsync();
        }
        else
        {
            throw new InvalidOperationException("Cannot load assessment: No HazardId or ReportId provided");
        }

        // Validate that we found assessments
        if (TechRiskAssessment is null)
        {
            throw new InvalidOperationException($"No Technical Risk Assessment found. HazardId: {HazardId}, ReportId: {ReportId}");
        }
        else
        {
            await LoadAnalysisByHazardCodeAsync();
        }

        // Update the AssessmentId parameter if it wasn't provided but we found an assessment
        if (string.IsNullOrWhiteSpace(ReportId) || ReportId == "RS-0000")
        {
            ReportId = TechRiskAssessment.Code;
        }
    }

    private async Task LoadAssessmentsByHazardCodeAsync()
    {
        _logger.LogInformation("Loading assessments for HazardId: {HazardId}", HazardId);

        if (string.IsNullOrEmpty(HazardId))
        {
            _logger.LogWarning("HazardId is null or empty, cannot load assessments");
            return;
        }

        var getAllAssessmentsQuery = new GetRiskAssessmentsByHazardCodeQuery(new HazardID(HazardId));
        var allAssessmentsResult = await _mediator.SendAsync(getAllAssessmentsQuery, CancellationToken.None);

        if (allAssessmentsResult.IsSuccess && allAssessmentsResult.Value?.Any() == true)
        {
            var assessments = allAssessmentsResult.Value.ToList();

            TechRiskAssessment = ResolveAssessmentForCurrentWorkflow(assessments);

            _logger.LogInformation("Found {Count} assessments for hazard {HazardId}", assessments.Count, HazardId);
        }
        else
        {
            _logger.LogWarning("No risk assessments found for HazardId: {HazardId}", HazardId);
        }
    }

    private async Task LoadAnalysisByHazardCodeAsync()
    {
        _logger.LogInformation("Loading assessments for HazardId: {HazardId}", HazardId);

        var getAllAnalysisQuery = new GetAllRiskAnalysisQuery();
        var allAllAnalysisResult = await _mediator.SendAsync(getAllAnalysisQuery, CancellationToken.None);

        if (allAllAnalysisResult.IsSuccess && allAllAnalysisResult.Value?.Any() == true)
        {
            var anlysis = allAllAnalysisResult.Value.ToList();

            TechRiskAnalysis = anlysis.FirstOrDefault(x => x.HazardCode == HazardId && 
                                                           !string.IsNullOrEmpty(x.RiskAssessmentCode) && 
                                                           TechRiskAssessment?.Code != null && 
                                                           !string.IsNullOrEmpty(TechRiskAssessment.Code) && 
                                                           x.RiskAssessmentCode.Trim() == TechRiskAssessment.Code);

            _logger.LogInformation("Found {Count} Risk Analysis for hazard {HazardId}", anlysis.Count, HazardId);
        }
        else
        {
            _logger.LogWarning("No Risk Analysis found for HazardId: {HazardId}", HazardId);
        }
    }

    private async Task LoadAssessmentByReportCodeAsync()
    {
        _logger.LogInformation("Loading assessment by Report ID: {ReportId}", ReportId);

        try
        {
            // If ReportId looks like a Report ID (RP-xxxx), try to find assessments for this report
            if (ReportId?.StartsWith("RP-") == true)
            {
                await LoadAssessmentsByReportCodeAsync();

                // If no assessments found, create them
                if (TechRiskAssessment is null && !string.IsNullOrEmpty(HazardId))
                {
                    await CreateAssessmentsForReport();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading assessment by Report ID: {ReportId}", ReportId);
        }
    }
    
    private async Task CreateAssessmentsForReport()
    {
        _logger.LogInformation("Creating new assessment for ReportId: {ReportId}, Hazard: {HazardId}", ReportId, HazardId);

        try
        {
            // Generate Placeholder ID - WILL BEGENERATED IN THE DATABASE 
            var assessmentId = $"RS-0000";

            var isRiskRegistryAssessment = IsRiskRegistryOnly;
            var assessmentType = isRiskRegistryAssessment ? RiskAssessmentType.RiskRegistryOnly : RiskAssessmentType.Technical;
            var assessmentCategory = RiskAssessmentCategory.Technical;
            var assessmentLabel = assessmentType.Name;
            var initialStep = isRiskRegistryAssessment ? 4 : 1;
            var initialStatus = isRiskRegistryAssessment ? RiskAssessmentStatus.AssessmentUnderway : RiskAssessmentStatus.AssessmentCreate;
            var initialStage = isRiskRegistryAssessment ? RiskAssessmentStage.AssessingRisk : RiskAssessmentStage.DescribingSystem;

            // Create assessment using the public constructor
            var technicalAssessment = new RiskAssessment(new RiskAssessmentID(assessmentId))
            {
                Name = $"{assessmentLabel} Risk Assessment for Report {ReportId}",
                LeadAssessorId = LeadAssessorName,
                AssessmentType = assessmentType,
                RiskAssessmentCategory = assessmentCategory,
                HazardCode = HazardId,
                PrimaryHazardId = HazardId,
                Description = $"{assessmentLabel} assessment created from Report {ReportId}",
                Stage = initialStage,
                Code = assessmentId,
                Status = initialStatus,
                CurrentStep = initialStep,
                CreatedDate = DateTime.UtcNow,
                CreatedBy  = _currentUserService?.UserDisplayName
            };

            // Save Technical assessment
            var createCommand = new CreateRiskAssessmentCommand(technicalAssessment);
            var result = await _mediator.SendAsync(createCommand, CancellationToken.None);

            if (!result.IsSuccess)
            {
                throw new Exception($"Failed to create Technical assessment: {result.Error?.Message}");
            }

            TechRiskAssessment = result.Value;

            _logger.LogInformation("Created {AssessmentType} assessment: {AssessmentId}", assessmentType.Value, assessmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating assessment for ReportId: {ReportId}", ReportId);
            throw;
        }
    }
    
    private async Task LoadAssessmentsByReportCodeAsync()
    {
        _logger.LogInformation("Loading assessments via ReportId: {ReportId}", ReportId);

        if (string.IsNullOrEmpty(ReportId))
        {
            _logger.LogWarning("ReportId is null or empty, cannot load assessments");
            return;
        }

        // First, find hazards for this report
        var reportHazardQuery = new GetHazardsByReportCodeQuery(new ReportID(ReportId));
        var reportHazardResult = await _mediator.SendAsync(reportHazardQuery, CancellationToken.None);

        if (reportHazardResult.IsSuccess && reportHazardResult.Value?.Any() == true)
        {
            var reportHazards = reportHazardResult.Value.ToList();
            _logger.LogInformation("Found {Count} hazards for report {ReportId}", reportHazards.Count, ReportId);

            // Try to load assessments for each hazard until we find one
            foreach (var hazard in reportHazards)
            {
                var assessmentsQuery = new GetRiskAssessmentsByHazardCodeQuery(new HazardID(hazard.Code));
                var assessmentsResult = await _mediator.SendAsync(assessmentsQuery, CancellationToken.None);

                if (assessmentsResult.IsSuccess && assessmentsResult.Value?.Any() == true)
                {
                    var assessments = assessmentsResult.Value.ToList();

                    // Take the first matching assessment we find for this workflow
                    if (TechRiskAssessment is null)
                    {
                        TechRiskAssessment = ResolveAssessmentForCurrentWorkflow(assessments);
                        HazardId = hazard.Code; // Update HazardId for consistency
                    }

                    _logger.LogInformation("Found assessments via hazard {HazardCode}", hazard.Code);

                    // If we found what we need, no need to check other hazards
                    if (TechRiskAssessment is not null) break;
                }
            }
        }
        else
        {
            _logger.LogWarning("No hazards found for ReportId: {ReportId}", ReportId);
        }
    }

    private RiskAssessment? ResolveAssessmentForCurrentWorkflow(IEnumerable<RiskAssessment> assessments)
    {
        var assessmentList = assessments?.ToList() ?? new List<RiskAssessment>();
        if (!assessmentList.Any())
        {
            return null;
        }

        if (IsRiskRegistryOnly)
        {
            return assessmentList.FirstOrDefault(x => x.AssessmentType == RiskAssessmentType.RiskRegistryOnly)
                   ?? assessmentList.FirstOrDefault(x => x.AssessmentType == RiskAssessmentType.Technical)
                   ?? assessmentList.FirstOrDefault();
        }

        return assessmentList.FirstOrDefault(x => x.AssessmentType == RiskAssessmentType.Technical)
               ?? assessmentList.FirstOrDefault(x => x.AssessmentType == RiskAssessmentType.RiskRegistryOnly)
               ?? assessmentList.FirstOrDefault();
    }
    private async Task LoadReportHazardsAsync()
    {
        try
        {
            var allHazards = new List<Hazard>();

            // Load primary hazard
            if (!string.IsNullOrEmpty(HazardId))
            {
                var hazardQuery = new GetHazardByCodeQuery(new HazardID(HazardId));
                var hazardResult = await _mediator.SendAsync(hazardQuery, CancellationToken.None);

                if (hazardResult.IsSuccess && hazardResult.Value is not null)
                {
                    allHazards.Add(hazardResult.Value);
                    PrimaryHazard = hazardResult.Value;
                }

            }

            // Load additional hazards from report
            if (!string.IsNullOrEmpty(ReportId))
            {
                var reportHazardQuery = new GetHazardsByReportCodeQuery(new ReportID(ReportId.Trim()));
                var reportHazardResult = await _mediator.SendAsync(reportHazardQuery, CancellationToken.None);

                if (reportHazardResult.IsSuccess && reportHazardResult.Value?.Any() == true)
                {
                    foreach (var hazard in reportHazardResult.Value)
                    {
                        if (!allHazards.Any(h => h.Code == hazard.Code))
                        {
                            allHazards.Add(hazard);
                        }
                    }
                }
            }

            // Load hazards identified in Step 2
            if (TechRiskAssessment?.IdentifiedHazardIds?.Any() == true)
            {
                foreach (var hazardIdString in TechRiskAssessment.IdentifiedHazardIds)
                {
                    try
                    {
                        var hazardQuery = new GetHazardByCodeQuery(new HazardID(hazardIdString));
                        var hazardResult = await _mediator.SendAsync(hazardQuery, CancellationToken.None);

                        if (hazardResult.IsSuccess && hazardResult.Value is not null
                            && !allHazards.Any(h => h.Code == hazardResult.Value.Code))
                        {
                            allHazards.Add(hazardResult.Value);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to load identified hazard {HazardId}", hazardIdString);
                    }
                }
            }

            // Update source and UI snapshot collections
            ReportHazards = allHazards;
            RefreshHazardUiSnapshot();

            _logger.LogInformation("Loaded {Count} hazards for assessment", allHazards.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading report hazards");
            ReportHazards = new List<Hazard>();
            RefreshHazardUiSnapshot();
        }
    }

    private void RefreshHazardUiSnapshot()
    {
        HazardUiSnapshot = ReportHazards.ToList();
    }

    private async Task LoadStepDataFromAssessment()
    {
        if (TechRiskAssessment is null) return;

        try
        {
            Step1.LoadFromAssessment(TechRiskAssessment);
            Step2.LoadFromAssessment(TechRiskAssessment);

            await Step3.LoadFromAssessmentAsync(TechRiskAssessment,ReportHazards);
            
            Step4.LoadFromAssessment(TechRiskAssessment);
            await Step4.LoadExistingScoringPanelsAsync(_mediator, ReportHazards);

            // Step 5 uses the same assessment - the step models will determine Technical vs Residual properties
            await Step5.LoadFromAssessmentAsync(TechRiskAssessment, ReportHazards);

            _logger.LogInformation("Step models loaded from assessment, including RiskAnalysis entities");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading step models from assessment");
        }
    }
    
    private async Task LoadAvailableAssessorsAsync()
    {
        try
        {
            var usersQuery = new GetUsersByApplicationGroupCodeQuery("AG-0007");
            var usersResult = await _mediator.SendAsync(usersQuery, CancellationToken.None);
            if (usersResult.IsSuccess)
            {
                AvailableAssessors = usersResult.Value?.ToList() ?? new List<SMSApplicationUser>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load available assessors");
        }
    }
    
    private async Task LoadReferenceDataAsync()
    {
        try
        {
            // Load SMS Application Users
            var smsUsersQuery = new GetAllSMSApplicationUsersQuery();
            var smsUsersResult = await _mediator.SendAsync(smsUsersQuery, CancellationToken.None);
            if (smsUsersResult.IsSuccess)
            {
                AvailableSMSUsers = smsUsersResult.Value?.Where(u => u.IsActive).ToList() ?? new List<SMSApplicationUser>();
            }
            
            // Load Investigators Users
            var investigatorsQuery = new GetUsersByApplicationGroupCodeQuery("AG-0006");
            var investigatorsResult = await _mediator.SendAsync(investigatorsQuery, CancellationToken.None);

            if (investigatorsResult.IsSuccess && investigatorsResult.Value != null)
            {
                AvailableInvestigators = investigatorsResult.Value.ToList();                
            }

            // Load Assessors Users
            var usersQuery = new GetUsersByApplicationGroupCodeQuery("AG-0007");
            var usersResult = await _mediator.SendAsync(usersQuery, CancellationToken.None);
            if (usersResult.IsSuccess)
            {
                AvailableAssessors = usersResult.Value?.ToList() ?? new List<SMSApplicationUser>();
            }

            // Load Stakeholder Users
            var stakeholdersQuery = new GetActiveSMSStakeholderUsersQuery();
            var stakeholdersResult = await _mediator.SendAsync(stakeholdersQuery, CancellationToken.None);
            if (stakeholdersResult.IsSuccess)
            {
                AvailableStakeholders = stakeholdersResult.Value?.ToList() ?? new List<SMSStakeholderUser>();
            }

            // Load Stakeholder Groups
            var groupsQuery = new GetAllSMSStakeholderGroupsQuery();
            var groupsResult = await _mediator.SendAsync(groupsQuery, CancellationToken.None);
            if (groupsResult.IsSuccess)
            {
                StakeholderGroups = groupsResult.Value?.ToList() ?? new List<SMSStakeholderGroup>();
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading reference data");
        }
    }

    /// <summary>
    /// Determine if a SMS Application User is a member of the Safety Team
    /// This method checks various criteria to identify Safety Team members
    /// </summary>

    

    #endregion

    #region Navigation Methods

    private async Task NavigateToStep(int targetStep, bool skipWorkflowNavigationValidation = false)
    {
        if (IsRiskRegistryOnly)
        {
            targetStep = 4;
        }

        if (targetStep < 1 || targetStep > MaxAssessmentStep) return;

        // Check if we're trying to jump ahead without saving current progress
        if (targetStep > CurrentStep && IsSaving)
        {
            _logger.LogInformation("Navigation blocked - currently saving step {CurrentStep}", CurrentStep);
            return;
        }

        // In strict workflow mode, any forward navigation requires current step validation and save.
        if (!skipWorkflowNavigationValidation && ForceTechnicalAssessmentWorkflow && targetStep > CurrentStep)
        {
            var validationResult = ValidateCurrentStep();
            if (!validationResult.isValid)
            {
                await _notificationHelper.ShowErrorAsync($"Please complete Step {CurrentStep} before proceeding to Step {targetStep}");
                return;
            }

            // Save current step before jumping
            var saveResult = await SaveCurrentStepAsync();
            if (!saveResult.success)
            {
                await _notificationHelper.ShowErrorAsync($"Please save Step {CurrentStep} before proceeding to Step {targetStep}");
                return;
            }
        }
        
        if (targetStep == 3)
        {
            _logger.LogInformation("Navigating to Step 3 - reloading Step3 data to ensure HazardRiskAnalyses is complete");
            // FIRST: Refresh ReportHazards to include any newly added hazards from Step 2
            await LoadReportHazardsAsync();

            if (TechRiskAssessment is not null && ReportHazards?.Any() == true)
            {
                await Step3.LoadFromAssessmentAsync(TechRiskAssessment, ReportHazards);
            }
            else
            {
                _logger.LogWarning("Cannot reload Step3 data - missing TechRiskAssessment or ReportHazards");
            }
        }
        // Include the step number in the URL
        var encodedReportId = Uri.EscapeDataString(ReportId ?? string.Empty);
        var encodedHazardId = Uri.EscapeDataString(HazardId ?? string.Empty);
        var returnToQuery = !string.IsNullOrWhiteSpace(ReturnTo)
            ? $"?returnTo={Uri.EscapeDataString(ReturnTo)}"
            : string.Empty;
        var navigationUrl = $"/SMSRiskManagement/TechnicalAssessment/{encodedReportId}/{encodedHazardId}/{targetStep}{returnToQuery}";

        _logger.LogInformation("Navigating to: {Url}", navigationUrl);
        _navigation.NavigateToSecure(navigationUrl);
    }

    private static string? NormalizeRouteSegment(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Uri.UnescapeDataString(value).Trim();
    }

    private async Task PreviousStep()
    {
        if (CurrentStep > 1)
        {
            await NavigateToStep(CurrentStep - 1);
        }
        else
        {
            _logger.LogInformation("Already at first step, cannot go to previous step");
        }
    }

    private async Task NextStep()
    {
        try
        {
            if (!ForceTechnicalAssessmentWorkflow)
            {
                if (CurrentStep < MaxAssessmentStep)
                {
                    await NavigateToStep(CurrentStep + 1, skipWorkflowNavigationValidation: true);
                }

                return;
            }

            IsSaving = true;
            StateHasChanged();

            // Validate current step
            var validationResult = ValidateCurrentStep();
            if (!validationResult.isValid)
            {
                await _notificationHelper.ShowErrorAsync($"Step {CurrentStep} validation failed: {validationResult.message}");
                return;
            }

            // Save current step
            var saveResult = await SaveCurrentStepAsync();
            IsSaving = !saveResult.success;
            if (!saveResult.success)
            {
                await _notificationHelper.ShowErrorAsync($"Failed to save Step {CurrentStep}: {saveResult.message}");
                return;
            }

            // Smart progression: if earlier steps are still invalid, point workflow back to earliest invalid step.
            var lowestEarlierInvalidStep = GetLowestInvalidStepUpTo(CurrentStep, includeCurrentStep: false);
            if (lowestEarlierInvalidStep.HasValue && lowestEarlierInvalidStep.Value < CurrentStep)
            {
                await RepointAssessmentProgressToStepAsync(lowestEarlierInvalidStep.Value);
                await _notificationHelper.ShowWarningAsync($"Step {CurrentStep} saved. Continue with Step {lowestEarlierInvalidStep.Value} first.");
                await NavigateToStep(lowestEarlierInvalidStep.Value, skipWorkflowNavigationValidation: true);
                return;
            }

            // Navigate to next step
            if (CurrentStep < MaxAssessmentStep)
            {
                await NavigateToStep(CurrentStep + 1, skipWorkflowNavigationValidation: true);
                await _notificationHelper.ShowSuccessAsync("Step saved successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in NextStep");
            await _notificationHelper.ShowErrorAsync("Error proceeding to next step");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    private async Task RepointAssessmentProgressToStepAsync(int targetStep)
    {
        if (TechRiskAssessment is null)
        {
            return;
        }

        targetStep = Math.Clamp(targetStep, 1, MaxAssessmentStep);

        TechRiskAssessment.CurrentStep = targetStep;
        TechRiskAssessment.Status = DetermineRiskAssessmentStatusFromStep(targetStep);
        TechRiskAssessment.Stage = DetermineRiskAssessmentStageFromStep(targetStep);
        TechRiskAssessment.CompletedBy = null;
        TechRiskAssessment.CompletedDate = null;
        TechRiskAssessment.UpdatedBy = _currentUserService?.UserDisplayName;
        TechRiskAssessment.UpdatedDate = DateTime.UtcNow;

        var updateCommand = new UpdateRiskAssessmentCommand(TechRiskAssessment);
        var updateResult = await _mediator.SendAsync(updateCommand, CancellationToken.None);
        if (updateResult.IsSuccess)
        {
            TechRiskAssessment = updateResult.Value;
        }

        var reportStatusCommand = new UpdateReportStatusCommand(ReportId ?? string.Empty, ReportStatus.RiskAssessmentInProgress, _currentUserService?.UserDisplayName ?? "System");
        await _mediator.SendAsync(reportStatusCommand, CancellationToken.None);
    }

    private async Task SubmitAssessment()
    {
        try
        {
            IsSaving = true;
            StateHasChanged();

            if (IsRiskRegistryOnly)
            {
                var rrOnlyValidation = ValidateRiskRegistryOnlyStep4();
                if (!rrOnlyValidation.isValid)
                {
                    var saveResult = await SaveCurrentStepAsync(forceStayOnCurrentStep: true);
                    if (!saveResult.success)
                    {
                        await _notificationHelper.ShowErrorAsync($"Failed to save assessment: {saveResult.message}");
                        return;
                    }

                    await _notificationHelper.ShowSuccessAsync("Assessment saved at Step 4. Complete scoring to submit.");
                    NavigateAfterAssessmentAction(isCompleted: false);
                    return;
                }
            }

            // RR-only must always complete/close on submit (do not route through save-only branch).
            var shouldCompleteAssessment = ForceTechnicalAssessmentWorkflow || IsRiskRegistryOnly;

            if (shouldCompleteAssessment)
            {
                var allStepsValid = ValidateAllSteps();
                if (!allStepsValid.isValid)
                {
                    await _notificationHelper.ShowErrorAsync($"Assessment cannot be completed: {allStepsValid.message}");
                    return;
                }

                if (!IsRiskRegistryOnly)
                {
                    var saveResult = await SaveCurrentStepAsync();
                    if (!saveResult.success)
                    {
                        await _notificationHelper.ShowErrorAsync($"Failed to save final step: {saveResult.message}");
                        return;
                    }
                }

                // Mark assessment as complete and save
                await CompleteAssessmentProcess();
                var successMessage = IsRiskRegistryOnly
                    ? "Risk Registry Only assessment completed successfully!"
                    : "Technical Assessment completed successfully!";
                await _notificationHelper.ShowSuccessAsync(successMessage);
                NavigateAfterAssessmentAction(isCompleted: true);
            }
            else
            {
                // Save final step
                var saveResult = await SaveCurrentStepAsync();
                if (!saveResult.success)
                {
                    await _notificationHelper.ShowErrorAsync($"Failed to save final step: {saveResult.message}");
                    return;
                }
                var lowestInvalidStep = GetLowestInvalidStep();
                if (lowestInvalidStep.HasValue)
                {
                    await _notificationHelper.ShowSuccessAsync($"Assessment saved. Continue with Step {lowestInvalidStep.Value}.");
                }
                else
                {
                    await _notificationHelper.ShowSuccessAsync("Technical Assessment completed successfully!");
                }
                NavigateAfterAssessmentAction(isCompleted: false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing assessment");
            await _notificationHelper.ShowErrorAsync("Error completing assessment");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    private void NavigateAfterAssessmentAction(bool isCompleted)
    {
        if (isCompleted)
        {
            _navigation.NavigateToSecure("/SMSRiskManagement/ReportProcessing?tab=validation");
            return;
        }

        var returnTarget = ReturnTo;
        if (string.Equals(returnTarget, "risk-assessment-listing", StringComparison.OrdinalIgnoreCase))
        {
            _navigation.NavigateToSecure("/SMSListings/RiskAssessments");
            return;
        }

        if (string.Equals(returnTarget, "report-processing", StringComparison.OrdinalIgnoreCase))
        {
            _navigation.NavigateToSecure("/SMSRiskManagement/ReportProcessing");
            return;
        }

        _navigation.NavigateToSecure("/SMSRiskManagement/ReportProcessing");
    }

    #endregion

    #region Save Methods

    private async Task SaveStep()
    {
        try
        {
            IsSaving = true;
            StateHasChanged();

            var saveResult = await SaveCurrentStepAsync(forceStayOnCurrentStep: true);
            if (!saveResult.success)
            {
                await _notificationHelper.ShowErrorAsync($"Failed to save Step {CurrentStep}: {saveResult.message}");
                return;
            }

            await _notificationHelper.ShowSuccessAsync($"Step {CurrentStep} saved. You can leave and resume from this step.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving current step {CurrentStep}", CurrentStep);
            await _notificationHelper.ShowErrorAsync("Error saving current step");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    private async Task<(bool success, string message)> SaveCurrentStepAsync(bool forceStayOnCurrentStep = false)
    {
        if (TechRiskAssessment is null)
        {
            return (false, "Assessment not loaded");
        }

        try
        {
            // Apply current step to assessment - ENHANCED: Use async method
            await ApplyCurrentStepToAssessmentAsync();

            // ENHANCEMENT: Update the current step in the assessment
            TechRiskAssessment.CurrentStep = CurrentStep;

            var isExplicitSaveOnFinalStep = forceStayOnCurrentStep && CurrentStep >= MaxAssessmentStep;

            // Update assessment status/stage
            if (isExplicitSaveOnFinalStep)
            {
                TechRiskAssessment.Status = RiskAssessmentStatus.AssessmentUnderway;
                TechRiskAssessment.Stage = IsRiskRegistryOnly ? RiskAssessmentStage.AssessingRisk : RiskAssessmentStage.MitigatingRisk;
            }
            else
            {
                TechRiskAssessment.Status = DetermineRiskAssessmentStatusFromStep(CurrentStep);
                TechRiskAssessment.Stage = DetermineRiskAssessmentStageFromStep(CurrentStep);
            }

            // Update last modified info
            TechRiskAssessment.UpdatedDate = DateTime.UtcNow;
            TechRiskAssessment.UpdatedBy = _currentUserService?.UserDisplayName;
            if (CurrentStep == 5 && !isExplicitSaveOnFinalStep)
            {
                TechRiskAssessment.CompletedBy = _currentUserService?.UserDisplayName;
                TechRiskAssessment.CompletedDate = DateTime.UtcNow;
            }
            else
            {
                TechRiskAssessment.CompletedBy = null;
                TechRiskAssessment.CompletedDate = null;
            }
            
            // Save to database
            var updateCommand = new UpdateRiskAssessmentCommand(TechRiskAssessment);
            var initalresult = await _mediator.SendAsync(updateCommand, CancellationToken.None);
            
            //This may change but atleast it's a start//
            ReportStatus status = CurrentStep switch
            {
                1 => ReportStatus.RiskAssessmentInProgress,
                2 => ReportStatus.RiskAssessmentInProgress,
                3 => ReportStatus.RiskAssessmentInProgress,
                4 => ReportStatus.RiskAssessmentInProgress,
                5 => ReportStatus.RiskAssessmentSubmitted,
                _ => ReportStatus.RiskAssessmentInProgress
            };

            if (isExplicitSaveOnFinalStep)
            {
                status = ReportStatus.RiskAssessmentInProgress;
            }

            var cmd = new UpdateReportStatusCommand(ReportId ?? "", status, _currentUserService?.UserDisplayName ?? "System");
            var cmdResult = await _mediator.SendAsync(cmd, CancellationToken.None);


            if (initalresult.IsSuccess)
            {
                TechRiskAssessment = initalresult.Value; // Update with latest data

                await UpdateHazardStatusForProgress();

                _logger.LogInformation("Step {CurrentStep} saved successfully for assessment {AssessmentCode}", CurrentStep, TechRiskAssessment.Code);

                return (true, $"Step {CurrentStep} saved successfully");
            }
            else
            {
                _logger.LogError("Failed to save step {CurrentStep}: {Error}", CurrentStep, initalresult.Error?.Message);
                return (false, initalresult.Error?.Message ?? "Save failed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving step {Step}", CurrentStep);
            return (false, ex.Message);
        }
    }
    private RiskAssessmentStage DetermineRiskAssessmentStageFromStep(int step)
    {
        if (IsRiskRegistryOnly && step >= 4)
        {
            return RiskAssessmentStage.Completed;
        }

        return step switch
        {
            1 => RiskAssessmentStage.DescribingSystem,// System description
            2 => RiskAssessmentStage.IdentifyingHazards, // Hazard identification
            3 => RiskAssessmentStage.AnalyizingRisk, // Risk analysis
            4 => RiskAssessmentStage.AssessingRisk, // Risk assessment
            5 => RiskAssessmentStage.MitigatingRisk, // Risk mitigation
            _ => RiskAssessmentStage.DescribingSystem
        };
    }
    private RiskAssessmentStatus DetermineRiskAssessmentStatusFromStep(int step)
    {
        if (IsRiskRegistryOnly && step >= 4)
        {
            return RiskAssessmentStatus.AssessmentComplete;
        }

        return step switch
        {
            1 => RiskAssessmentStatus.AssignedToAssessor,// System description
            2 => RiskAssessmentStatus.AssessmentUnderway, // Hazard identification
            3 => RiskAssessmentStatus.AssessmentUnderway, // Risk analysis
            4 => RiskAssessmentStatus.AssessmentUnderway, // Risk assessment
            5 => RiskAssessmentStatus.AssessmentComplete, // Risk mitigation
            _ => RiskAssessmentStatus.AssignedToAssessor
        };
    }
    /// <summary>
    /// Update the associated Hazard status to reflect assessment progress
    /// </summary>
    private async Task UpdateHazardStatusForProgress()
    {
        if (string.IsNullOrEmpty(HazardId)) return;

        try
        {
            // Get current hazard
            var hazardQuery = new GetHazardByCodeQuery(new HazardID(HazardId));
            var hazardResult = await _mediator.SendAsync(hazardQuery, CancellationToken.None);

            if (hazardResult.IsSuccess && hazardResult.Value is not null)
            {
                var hazard = hazardResult.Value;

                // Use shared method to load scoring panels and get current risk level
                var scoringPanels = await LoadScoringPanelsForHazard(HazardId, CurrentStep, TechRiskAssessment?.Code);
                var (averageScore, matrixCode, riskLevel) = CalculateHazardScoringData(scoringPanels, HazardId);

                // Update hazard status based on assessment progress
                var originalStatus = hazard.Status?.ToString();
                hazard.Status = DetermineHazardStatusFromStep(CurrentStep);
                
                // Use calculated risk level from scoring panels
                hazard.HazardRiskLevel = riskLevel;

                hazard.UpdatedBy = _currentUserService?.UserDisplayName;  
                hazard.UpdatedDate = DateTime.UtcNow;   

                // Only update if status changed
                if (hazard.Status?.ToString() != originalStatus)
                {
                    var updateHazardCommand = new UpdateHazardCommand(hazard);
                    var updateResult = await _mediator.SendAsync(updateHazardCommand, CancellationToken.None);

                    if (updateResult.IsSuccess)
                    {
                        _logger.LogInformation("Updated hazard {HazardId} status from {OldStatus} to {NewStatus}",
                            HazardId, originalStatus, hazard.Status?.ToString());
                    }
                    else
                    {
                        _logger.LogWarning("Failed to update hazard status: {Error}", updateResult.Error?.Message);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating hazard status for progress");
        }
    }

    /// <summary>
    /// Determine appropriate hazard status based on assessment step
    /// </summary>
    private HazardStatus DetermineHazardStatusFromStep(int step)
    {
        return step switch
        {
            1 => HazardStatus.InitialRiskAssessment, // System description
            2 => HazardStatus.InitialRiskAssessment, // Hazard identification
            3 => HazardStatus.InitialRiskAnalysis, // Risk analysis
            4 => HazardStatus.InitialHazardScoring, // Risk assessment
            5 => HazardStatus.ResidualRiskAnalysis, // Risk mitigation
            _ => HazardStatus.InitialRiskAssessment
        };
    }

   
    public async Task<List<ScoringPanel>> LoadScoringPanelsForHazard(string hazardCode, int currentStep, string? riskAssessmentCode = null)
    {
        if (string.IsNullOrEmpty(hazardCode) || hazardCode == "HZ-0000")
        {
            _logger.LogWarning("Invalid hazard code provided: {HazardCode}", hazardCode);
            return new List<ScoringPanel>();
        }

        try
        {
            _logger.LogInformation("Loading scoring panels for hazard {HazardCode}, Step {CurrentStep}", hazardCode, currentStep);

            // Use the provided risk assessment code or fall back to current assessment
            var targetAssessmentCode = !string.IsNullOrEmpty(riskAssessmentCode) 
                ? riskAssessmentCode.Trim() 
                : TechRiskAssessment?.Code?.Trim();

            if (string.IsNullOrEmpty(targetAssessmentCode))
            {
                _logger.LogWarning("No risk assessment code available for scoring panel filtering");
                return new List<ScoringPanel>();
            }

            // Load all panels for the hazard
            var query = new GetScoringPanelsByHazardCodeQuery(hazardCode);
            var result = await _mediator.SendAsync(query, CancellationToken.None);

            if (!result.IsSuccess || result.Value is null || result.Value.Count == 0)
            {
                _logger.LogWarning("No scoring panels found for hazard {HazardCode}", hazardCode);
                return new List<ScoringPanel>();
            }

            // Copy Step 4 scores to Step 5 if needed (only when loading Step 5)
            if (currentStep == 5)
            {
                await CopyStep4ScoresToStep5IfNeeded(result.Value, targetAssessmentCode);
            }

            // Filter panels by risk assessment code
            var filteredPanels = (result.Value ?? new List<ScoringPanel>())
                .Where(p => string.Equals(p.RiskAssessmentCode?.Trim(), targetAssessmentCode, StringComparison.Ordinal))
                .ToList();

            // Map properties based on current step for all loaded panels
            foreach (var panel in filteredPanels)
            {
                MapScoringPanelPropertiesBasedOnStep(panel, currentStep);
            }

            _logger.LogInformation("Loaded {Count} scoring panels for hazard {HazardCode}, Step {CurrentStep}", 
                filteredPanels.Count, hazardCode, currentStep);

            return filteredPanels;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading scoring panels for hazard {HazardCode}, Step {CurrentStep}", hazardCode, currentStep);
            return new List<ScoringPanel>();
        }
    }

    /// <summary>
    /// Map scoring panel properties based on assessment step
    /// Step 4 = Technical properties, Step 5 = Residual properties
    /// </summary>
    /// <param name="panel">The scoring panel to map properties for</param>
    /// <param name="currentStep">Current assessment step (4 or 5)</param>
    private void MapScoringPanelPropertiesBasedOnStep(ScoringPanel panel, int currentStep)
    {
        if (currentStep == 5)
        {
            // Step 5: Map from Residual properties
            panel.Likelihood = panel.ResidualLikelihood;
            panel.Severity = panel.ResidualSeverity;
            panel.Score = panel.ResidualScore;
            panel.Rationale = panel.ResidualRationale;
        }
        else
        {
            // Step 4 (default): Map from Technical properties
            panel.Likelihood = panel.InitialLikelihood;
            panel.Severity = panel.InitialSeverity;
            panel.Score = panel.InitialScore;
            panel.Rationale = panel.InitialRationale;
        }
    }

    /// <summary>
    /// Update actual entity properties from mapped properties before saving
    /// Step 4 updates Technical properties, Step 5 updates Residual properties
    /// </summary>
    /// <param name="panel">The scoring panel to update</param>
    /// <param name="currentStep">Current assessment step (4 or 5)</param>
    public void UpdateScoringPanelEntityPropertiesFromMapped(ScoringPanel panel, int currentStep)
    {
        if (currentStep == 5)
        {
            // Step 5: Update Residual properties from mapped properties
            panel.ResidualLikelihood = panel.Likelihood;
            panel.ResidualSeverity = panel.Severity;
            panel.ResidualScore = panel.Score;
            panel.ResidualRationale = panel.Rationale;
        }
        else
        {
            // Step 4: Update Technical properties from mapped properties
            panel.InitialLikelihood = panel.Likelihood;
            panel.InitialSeverity = panel.Severity;
            panel.InitialScore = panel.Score;
            panel.InitialRationale = panel.Rationale;
        }
    }

    /// <summary>
    /// Copy Step 4 initial scores to Step 5 residual scores if residual scores are empty
    /// </summary>
    /// <param name="allPanels">All panels for the hazard</param>
    /// <param name="targetAssessmentCode">Target risk assessment code</param>
    private async Task CopyStep4ScoresToStep5IfNeeded(IEnumerable<ScoringPanel> allPanels, string targetAssessmentCode)
    {
        try
        {
            // Get existing panels for the current assessment
            var existingPanels = (allPanels ?? Enumerable.Empty<ScoringPanel>())
                .Where(p => string.Equals(p.RiskAssessmentCode?.Trim(), targetAssessmentCode, StringComparison.Ordinal))
                .ToList();

            if (!existingPanels.Any())
            {
                _logger.LogInformation("No panels found for assessment {AssessmentCode} to copy scores", targetAssessmentCode);
                return;
            }

            // Find panels that have Technical scores but EMPTY Residual scores
            var panelsNeedingCopy = existingPanels
                .Where(p =>
                    // Has Technical scores from Step 4
                    p.InitialSeverity.HasValue && p.InitialLikelihood.HasValue && p.InitialScore.HasValue &&
                    // AND Residual scores are empty (haven't been set in Step 5 yet)
                    !p.ResidualSeverity.HasValue && !p.ResidualLikelihood.HasValue && !p.ResidualScore.HasValue)
                .ToList();

            if (!panelsNeedingCopy.Any())
            {
                _logger.LogInformation("No panels need score copying for assessment {AssessmentCode} - either no Technical scores or Residual scores already exist", targetAssessmentCode);
                return;
            }

            _logger.LogInformation("Copying Technical scores to empty Residual scores for {Count} panels in assessment {AssessmentCode}", panelsNeedingCopy.Count, targetAssessmentCode);

            bool anyUpdated = false;

            foreach (var panel in panelsNeedingCopy)
            {
                // Copy Technical scores to Residual as starting point
                panel.ResidualSeverity = panel.InitialSeverity;
                panel.ResidualLikelihood = panel.InitialLikelihood;
                panel.ResidualScore = panel.InitialScore;
                panel.ResidualRationale = $"Technical assessment: {panel.InitialRationale ?? "No rationale provided"}";

                _logger.LogInformation("Copying Technical scores to empty Residual for panel {PanelCode}: {Sev}x{Like}={Score}", 
                    panel.Code, panel.InitialSeverity, panel.InitialLikelihood, panel.InitialScore);

                // Save the updated panel
                var updateCommand = new UpdateScoringPanelCommand(panel);
                var result = await _mediator.SendAsync(updateCommand, CancellationToken.None);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Successfully copied Technical scores to empty Residual for panel {PanelCode}", panel.Code);
                    anyUpdated = true;
                }
                else
                {
                    _logger.LogError("Failed to copy scores for panel {PanelCode}: {Error}", panel.Code, result.Error?.Message ?? "Unknown error");
                }
            }

            if (anyUpdated)
            {
                _logger.LogInformation("Completed copying Technical scores to empty Residual scores for assessment {AssessmentCode}", targetAssessmentCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying Step 4 scores to Step 5 for assessment {AssessmentCode}", targetAssessmentCode);
        }
    }

    /// <summary>
    /// Calculate hazard average scoring data from completed scoring panels
    /// Returns average score, matrix code, and risk level based on step context
    /// </summary>
    /// <param name="scoringPanels">List of scoring panels (already mapped for current step)</param>
    /// <param name="hazardCode">Hazard code for logging</param>
    /// <returns>Tuple of calculated scoring data</returns>
    public (double? AverageScore, string MatrixCode, RiskLevel RiskLevel) CalculateHazardScoringData(List<ScoringPanel> scoringPanels, string hazardCode)
    {
        var completedPanels = scoringPanels.Where(p => HasScoringPanelScore(p)).ToList();

        if (!completedPanels.Any())
        {
            _logger.LogInformation("No completed scoring panels for hazard {HazardCode}", hazardCode);
            return (null, string.Empty, RiskLevel.Unkonwn);
        }

        try
        {
            // Calculate averages separately for severity and likelihood (aviation standard)
            var averageSeverity = completedPanels.Average(p => (double)p.Severity!.Value);
            var averageLikelihood = completedPanels.Average(p => (double)p.Likelihood!.Value);
            var averageScore = completedPanels.Average(p => (double)p.Score!.Value);

            // Use aviation standard calculation for matrix code
            var matrixCode = AviationRiskMatrixCalculator.GetAverageMatrixCode(averageSeverity, averageLikelihood);
            var roundedSeverity = (int)Math.Round(averageSeverity);
            var roundedLikelihood = (int)Math.Round(averageLikelihood);
            var riskLevel = AviationRiskMatrixCalculator.GetAviationRiskLevel(roundedSeverity, roundedLikelihood);

            _logger.LogInformation("Calculated hazard {HazardCode} scoring: AvgSev={Severity:F2}?{RoundedSev}, AvgLike={Likelihood:F2}?{RoundedLike}, Matrix={MatrixCode}, Risk={RiskLevel}", 
                hazardCode, averageSeverity, roundedSeverity, averageLikelihood, roundedLikelihood, matrixCode, riskLevel?.Value ?? "Unknown");

            return (averageScore, matrixCode, riskLevel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating scoring data for hazard {HazardCode}", hazardCode);
            return (null, string.Empty, RiskLevel.Unkonwn);
        }
    }

    /// <summary>
    /// Check if a scoring panel has complete score data
    /// </summary>
    /// <param name="panel">The scoring panel to check</param>
    /// <returns>True if panel has severity, likelihood, and calculated score</returns>
    private bool HasScoringPanelScore(ScoringPanel panel)
    {
        return panel.Severity.HasValue && panel.Likelihood.HasValue && panel.Score.HasValue;
    }

    #endregion

    #region Validation Methods

    private (bool isValid, string message) GetStepValidationStatus(int stepNumber)
    {
        return stepNumber switch
        {
            1 => Step1.Validate(),
            2 => Step2.Validate(ReportHazards),
            3 => Step3.Validate(HazardUiSnapshot, 3),
            4 => Step4.Validate(),
            5 => Step5.Validate(),
            _ => (false, "Invalid step number")
        };
    }

    private static ButtonStyle GetStepButtonStyle(int stepNumber, bool isCurrentStep, bool isValid)
    {
        if (isCurrentStep)
        {
            //return isValid ? ButtonStyle.Primary : ButtonStyle.Warning;
            return ButtonStyle.Primary;
        }

        return isValid ? ButtonStyle.Success : ButtonStyle.Danger;
    }

    private static string GetStepButtonIcon(bool isCurrentStep, bool isValid)
    {
        if (isValid)
        {
            return isCurrentStep ? "task_alt" : "check_circle";
        }

        return isCurrentStep ? "edit" : "error";
    }

    private int? GetLowestInvalidStep()
    {
        if (IsRiskRegistryOnly)
        {
            return Step4.Validate().isValid ? null : 4;
        }

        for (var step = 1; step <= MaxAssessmentStep; step++)
        {
            var validation = GetStepValidationStatus(step);
            if (!validation.isValid)
            {
                return step;
            }
        }

        return null;
    }

    private int? GetLowestInvalidStepUpTo(int maxStepToCheck, bool includeCurrentStep = true)
    {
        var upperBound = Math.Clamp(maxStepToCheck, 1, MaxAssessmentStep);
        for (var step = 1; step <= upperBound; step++)
        {
            if (!includeCurrentStep && step == CurrentStep)
            {
                continue;
            }

            var validation = GetStepValidationStatus(step);
            if (!validation.isValid)
            {
                return step;
            }
        }

        return null;
    }

    private bool IsAssessmentReadyForCompletion() => !GetLowestInvalidStep().HasValue;

    private bool ShowSubmitAssessmentButton()
    {
        if (IsRiskRegistryOnly)
        {
            // RR-only is a single actionable step (Step 4); always keep final action visible.
            return true;
        }

        return CurrentStep < MaxAssessmentStep || IsAssessmentReadyForCompletion();
    }

    private string GetFinalActionButtonText()
    {
        if (CurrentStep < MaxAssessmentStep)
        {
            return "Next Step";
        }

        if (IsRiskRegistryOnly)
        {
            var rrOnlyStep4Valid = ValidateRiskRegistryOnlyStep4().isValid;
            return rrOnlyStep4Valid ? "Submit Assessment" : "Save Assessment";
        }

        return IsAssessmentReadyForCompletion() ? "Submit Assessment" : "Save Assessment";
    }

    private string GetFinalActionButtonIcon()
    {
        if (CurrentStep < MaxAssessmentStep)
        {
            return "arrow_forward";
        }

        return IsAssessmentReadyForCompletion() ? "check_circle" : "save";
    }

    private (bool isValid, string message) ValidateCurrentStep()
    {
        if (CurrentStep == 3)
        {
            return Step3.Validate(HazardUiSnapshot, CurrentStep);
        }

        return GetStepValidationStatus(CurrentStep);
    }

    private (bool isValid, string message) ValidateAllSteps()
    {
        // Risk Registry Only flow uses Step 4 scoring without requiring Steps 1-3 or Step 5.
        if (IsRiskRegistryOnly)
        {
            var step4OnlyResult = ValidateRiskRegistryOnlyStep4();
            if (!step4OnlyResult.isValid)
            {
                return (false, $"Step 4: {step4OnlyResult.message}");
            }

            return (true, "Risk Registry Only validation passed (Step 4)");
        }

        // Validate each step in sequence
        var step1Result = Step1.Validate();
        if (!step1Result.isValid)
            return (false, $"Step 1: {step1Result.message}");

        var step2Result = Step2.Validate(ReportHazards);
        if (!step2Result.isValid)
            return (false, $"Step 2: {step2Result.message}");

        var step3Result = Step3.Validate(HazardUiSnapshot, 3); 
        if (!step3Result.isValid)
            return (false, $"Step 3: {step3Result.message}");

        var step4Result = Step4.Validate();
        if (!step4Result.isValid)
            return (false, $"Step 4: {step4Result.message}");

        var step5Result = Step5.Validate();
        if (!step5Result.isValid)
            return (false, $"Step 5: {step5Result.message}");

        return (true, "All steps are valid");
    }

    private (bool isValid, string message) ValidateRiskRegistryOnlyStep4()
    {
        if (ReportHazards is null || !ReportHazards.Any())
        {
            return (false, "No hazards available for scoring");
        }

        var unscoredHazards = ReportHazards
            .Where(h =>
            {
                var hasPersistedScore = h.InitialAverageScore.HasValue && h.InitialAverageScore.Value > 0;
                var hasStep4Score = Step4.HazardAverageScores.TryGetValue(h.Code, out var avgScore) && avgScore > 0;
                return !hasPersistedScore && !hasStep4Score;
            })
            .Select(h => h.Code)
            .ToList();

        if (unscoredHazards.Any())
        {
            return (false, $"Scoring incomplete for {unscoredHazards.Count} hazard(s)");
        }

        return (true, "Risk Registry Only validation passed (Step 4)");
    }

    #endregion

    private async Task CompleteAssessmentProcess()
    {
        if (TechRiskAssessment is null) return;

        // Apply all steps to ensure everything is saved - ENHANCED: Use async method
        await ApplyCurrentStepToAssessmentAsync();

        // Capture completion details for SPI automation
        var completedDate = DateTime.UtcNow;
        var assessmentStartDate = TechRiskAssessment.CreatedDate ?? DateTime.UtcNow.AddDays(-7); // Default to 7 days ago if no start date
        var targetCompletionDate = assessmentStartDate.AddDays(14); // Assume 14-day target for technical assessments
        var completedBy = _currentUserService?.UserDisplayName ?? "Unknown User";
        var finalRiskLevel = GetAssessmentFinalRiskLevel(); // Get the determined risk level

        // Explicitly mark assessment completed on submit.
        TechRiskAssessment.CurrentStep = MaxAssessmentStep;
        TechRiskAssessment.Status = RiskAssessmentStatus.AssessmentComplete;
        TechRiskAssessment.Stage = RiskAssessmentStage.Completed;
        TechRiskAssessment.CompletedBy = completedBy;
        TechRiskAssessment.CompletedDate = completedDate;
        TechRiskAssessment.UpdatedBy = completedBy;
        TechRiskAssessment.UpdatedDate = completedDate;

        // Update risk assessment - pipeline will automatically set UpdatedBy/UpdatedDate
        var updateCommand = new UpdateRiskAssessmentCommand(TechRiskAssessment);
        await _mediator.SendAsync(updateCommand, CancellationToken.None);

        var reportCompletionStatus = IsRiskRegistryOnly ? ReportStatus.Closed : ReportStatus.ValidationCompleted;
        var cmd = new UpdateReportStatusCommand(ReportId ?? "", reportCompletionStatus, _currentUserService?.UserDisplayName ?? "System");
        var cmdResult = await _mediator.SendAsync(cmd, CancellationToken.None);

        // NEW: SPI AUTOMATION - Trigger risk assessment completion event ??
        await TriggerRiskAssessmentSPIAutomation(
            TechRiskAssessment?.Code ?? "",
            assessmentStartDate,
            completedDate,
            targetCompletionDate,
            completedBy,
            finalRiskLevel);
    }

   
    private async Task ApplyCurrentStepToAssessmentAsync()
    {
        // FIXED: Removed manual audit field assignments - pipeline handles automatically
        // REMOVED: TechRiskAssessment.UpdatedDate = DateTime.UtcNow;
        // REMOVED: TechRiskAssessment.UpdatedBy = _currentUserService?.UserDisplayName;

        switch (CurrentStep)
        {
            case 1:
                Step1.ApplyToAssessment(TechRiskAssessment!);
                break;
            case 2:
                Step2.ApplyToAssessment(TechRiskAssessment!);
                break;
            case 3:
                await Step3.ApplyToAssessmentAsync(TechRiskAssessment!, HazardUiSnapshot, CurrentStep);
                break;
            case 4:
                await Step4.ApplyToAssessmentAsync(TechRiskAssessment!, HazardUiSnapshot);
                break;
            case 5:
                await Step5.ApplyToAssessmentAsync(TechRiskAssessment!, HazardUiSnapshot);
                break;
        }
    }

    #region Event Handlers for Child Components

    private async Task UpdateStep1(Step1Model updatedStep1)
    {
        Step1 = updatedStep1;
        await InvokeAsync(StateHasChanged);
    }

    private async Task UpdateStep2(Step2Model updatedStep2)
    {
        Step2 = updatedStep2;
        await InvokeAsync(StateHasChanged);
    }

    private async Task UpdateStep3(Step3Model updatedStep3)
    {
        Step3 = updatedStep3;
        await InvokeAsync(StateHasChanged);
    }

    private async Task UpdateStep4(Step4Model updatedStep4)
    {
        Step4 = updatedStep4;
        await InvokeAsync(StateHasChanged);
    }

    private async Task UpdateStep5(Step5Model updatedStep5)
    {
        Step5 = updatedStep5;
        await InvokeAsync(StateHasChanged);
    }

    private async Task AddHazard(Hazard newHazard)
    {
        try
        {
            _logger.LogInformation("AddHazard method called with hazard: {Description} (Code: {Code})", newHazard.Description, newHazard.Code);

            // CRITICAL: Check if hazard already exists to prevent duplication
            if (ReportHazards.Any(h => h.Code == newHazard.Code && h.Code != "HZ-0000"))
            {
                _logger.LogWarning("Hazard {HazardCode} already exists in ReportHazards collection, skipping duplication", newHazard.Code);
                return;
            }

            // CRITICAL: Check if hazard with same description already exists (in case of rapid duplicate submissions)
            if (ReportHazards.Any(h => h.Description?.Trim().Equals(newHazard.Description?.Trim(), StringComparison.OrdinalIgnoreCase) == true))
            {
                _logger.LogWarning("Hazard with description '{Description}' already exists in ReportHazards collection, skipping duplication", newHazard.Description);
                await _notificationHelper.ShowErrorAsync("A hazard with this description already exists.");
                return;
            }

            // The hazard was already created in the modal - we just need to add it to our collections
            _logger.LogInformation("Hazard {HazardCode} was successfully created in modal, adding to collections", newHazard.Code);

            // Add to collections (no database call needed here - already done in modal)
            ReportHazards.Add(newHazard);

            // Create a new list reference to trigger parameter change detection in child components
            RefreshHazardUiSnapshot();

            // Update Step2 model
            if (!Step2.HazardIds.Contains(newHazard.Code))
            {
                Step2.HazardIds.Add(newHazard.Code);
                Step2.HazardDescriptions.Add(newHazard.Description ?? string.Empty);
                Step2.HazardCategories.Add(newHazard.HazardCategory ?? string.Empty);
            }
            else
            {
                _logger.LogWarning("Hazard {HazardCode} already exists in Step2 model, skipping Step2 update", newHazard.Code);
            }

            // FIXED: Also update the assessment's IdentifiedHazardIds list
            if (TechRiskAssessment is not null && !TechRiskAssessment.IdentifiedHazardIds.Contains(newHazard.Code))
            {
                TechRiskAssessment.AddIdentifiedHazard(newHazard.Code, newHazard.Description ?? string.Empty);
                _logger.LogInformation("Added hazard {HazardCode} to assessment's IdentifiedHazardIds", newHazard.Code);
            }

            // CRITICAL: Force complete UI refresh for parent and all children
            await InvokeAsync(() =>
            {
                StateHasChanged();
            });

            await _notificationHelper.ShowSuccessAsync($"Hazard {newHazard.Code} added successfully");
            _logger.LogInformation("Successfully added hazard to collections: {HazardCode} - Total hazards: {Count}",newHazard.Code, HazardUiSnapshot.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AddHazard method for hazard: {Description}", newHazard.Description);
            await _notificationHelper.ShowErrorAsync("Error adding hazard");
        }
    }

    private async Task UpdateHazard(Hazard updatedHazard)
    {
        try
        {
            _logger.LogInformation("Updating hazard via CQRS: {HazardId}", updatedHazard.Code);

            // Use proper CQRS UpdateHazardCommand
            var updateCommand = new UpdateHazardCommand(updatedHazard);
            var result = await _mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess && result.Value is not null)
            {
                // Update local collection
                var existingIndex = ReportHazards.FindIndex(h => h.Code == updatedHazard.Code);
                if (existingIndex >= 0)
                {
                    ReportHazards[existingIndex] = result.Value;
                    RefreshHazardUiSnapshot();
                }

                await _notificationHelper.ShowSuccessAsync($"Hazard {updatedHazard.Code} updated successfully");
                _logger.LogInformation("Successfully updated hazard via CQRS: {HazardCode}", updatedHazard.Code);
            }
            else
            {
                await _notificationHelper.ShowErrorAsync($"Failed to update hazard: {result.Error?.Message}");
                _logger.LogError("CQRS UpdateHazardCommand failed: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating hazard via CQRS");
            await _notificationHelper.ShowErrorAsync("Error updating hazard");
        }
    }

    private async Task DeleteHazard(Hazard hazardToDelete)
    {
        try
        {
            _logger.LogInformation("Deleting hazard: {HazardCode}", hazardToDelete.Code);

            // Check if this is the initial hazard - should not be deleted
            if (hazardToDelete.Code == HazardId)
            {
                await _notificationHelper.ShowErrorAsync("Cannot delete the initial hazard from the report");
                return;
            }

            // Use proper CQRS DeleteHazardCommand
            var deleteCommand = new DeleteHazardCommand(new HazardID(hazardToDelete.Code));
            var result = await _mediator.SendAsync(deleteCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                // Remove from local collections
                ReportHazards.RemoveAll(h => h.Code == hazardToDelete.Code);
                RefreshHazardUiSnapshot();

                // Update Step2 model
                var indexToRemove = Step2.HazardIds.IndexOf(hazardToDelete.Code);
                if (indexToRemove >= 0)
                {
                    Step2.HazardIds.RemoveAt(indexToRemove);
                    if (indexToRemove < Step2.HazardDescriptions.Count)
                        Step2.HazardDescriptions.RemoveAt(indexToRemove);
                    if (indexToRemove < Step2.HazardCategories.Count)
                        Step2.HazardCategories.RemoveAt(indexToRemove);
                }

                // FIXED: Also remove from assessment's IdentifiedHazardIds
                if (TechRiskAssessment is not null)
                {
                    // Clear and re-add all remaining hazards
                    TechRiskAssessment.ClearIdentifiedHazards();
                    foreach (var remainingHazardId in Step2.HazardIds)
                    {
                        var hazardIndex = Step2.HazardIds.IndexOf(remainingHazardId);
                        var hazardDescription = hazardIndex < Step2.HazardDescriptions.Count
                            ? Step2.HazardDescriptions[hazardIndex]
                            : $"Hazard {remainingHazardId}";
                        TechRiskAssessment.AddIdentifiedHazard(remainingHazardId, hazardDescription);
                    }
                    _logger.LogInformation("Removed hazard {HazardCode} from assessment's IdentifiedHazardIds", hazardToDelete.Code);
                }

                // Force UI refresh
                await InvokeAsync(StateHasChanged);

                await _notificationHelper.ShowSuccessAsync($"Hazard {hazardToDelete.Code} deleted successfully");
                _logger.LogInformation("Successfully deleted hazard: {HazardCode} - Remaining hazards: {Count}",hazardToDelete.Code, HazardUiSnapshot.Count);
            }
            else
            {
                await _notificationHelper.ShowErrorAsync($"Failed to delete hazard: {result.Error?.Message}");
                _logger.LogError("DeleteHazardCommand failed: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting hazard");
            await _notificationHelper.ShowErrorAsync("Error deleting hazard");
        }
    }

    #endregion

    #region SPI Automation Integration

    /// <summary>
    /// Triggers SPI automation when risk assessment is completed
    /// Updates Risk Assessment Completion Rate and High Risk Exposure SPIs
    /// </summary>
    private async Task TriggerRiskAssessmentSPIAutomation(string assessmentCode, DateTime startDate, 
        DateTime completedDate, DateTime targetDate, string completedBy, string riskLevel)
    {
        try
        {
            _logger.LogInformation("SPI Automation: Triggering risk assessment completion events for {AssessmentCode}", assessmentCode);

            var resolvedRiskLevel = RiskLevel.GetAllValues().FirstOrDefault(level =>
                    level.Value.Equals(riskLevel, StringComparison.OrdinalIgnoreCase) ||
                    level.Name.Equals(riskLevel, StringComparison.OrdinalIgnoreCase))
                ?? RiskLevel.Low;

            var completionEvent = new RiskAssessmentCompletedEvent(
                new SMSEventID($"EVT-{Guid.NewGuid():N}"),
                TechRiskAssessment?.Id?.Value ?? assessmentCode,
                assessmentCode,
                startDate,
                targetDate,
                completedDate,
                ReportId ?? string.Empty)
            {
                HazardId = HazardId ?? string.Empty,
                ReportId = ReportId ?? string.Empty,
                RiskLevel = resolvedRiskLevel,
                RiskScore = GetAssessmentRiskScore(),
                AssessmentType = "Technical"
            };

            var completionPublishResult = await _eventBus.PublishDomainEventAsync(completionEvent, CancellationToken.None);
            if (completionPublishResult.IsFailure)
            {
                _logger.LogWarning("SPI Automation: Failed to publish RiskAssessmentCompleted event for {AssessmentCode}: {Error}",
                    assessmentCode, completionPublishResult.Error?.Message);
            }

            // If this is a high risk assessment, also trigger High Risk Exposure SPI
            _logger.LogInformation("Checking if {RiskLevel} is high risk for SPI automation", riskLevel);
            if (IsHighRiskLevel(riskLevel))
            {
                _logger.LogInformation("HIGH RISK DETECTED! Triggering High Risk Exposure SPI for {RiskLevel}", riskLevel);

                var highRiskEvent = new HighRiskIdentifiedEvent(new SMSEventID($"EVT-{Guid.NewGuid():N}"))
                {
                    AssessmentId = assessmentCode,
                    ReportId = ReportId ?? string.Empty,
                    RiskLevel = resolvedRiskLevel,
                    RiskScore = GetAssessmentRiskScore(),
                    IdentifiedDate = completedDate,
                    RiskDescription = $"Technical assessment identified {riskLevel} risk level"
                };

                var highRiskPublishResult = await _eventBus.PublishDomainEventAsync(highRiskEvent, CancellationToken.None);
                if (highRiskPublishResult.IsFailure)
                {
                    _logger.LogWarning("SPI Automation: Failed to publish HighRiskIdentified event for {AssessmentCode}: {Error}",
                        assessmentCode, highRiskPublishResult.Error?.Message);
                }

                _logger.LogInformation("SPI Automation: High risk SPI automation completed for {AssessmentCode} - Level: {RiskLevel}", 
                    assessmentCode, riskLevel);
            }
            else
            {
                _logger.LogInformation("Risk level {RiskLevel} is not considered high risk - skipping High Risk Exposure SPI", riskLevel);
            }

            _logger.LogInformation("SPI Automation: Successfully processed risk assessment completion events for {AssessmentCode}", assessmentCode);
        }
        catch (Exception spiEx)
        {
            // Don't fail the assessment completion if SPI automation fails
            _logger.LogWarning(spiEx, "? SPI Automation: Failed to process risk assessment completion events for {AssessmentCode} - continuing with assessment", assessmentCode);
        }
    }

    /// <summary>
    /// Gets the final risk level determined by the assessment
    /// Uses the highest risk level found across all hazards based on matrix codes
    /// </summary>
    private string GetAssessmentFinalRiskLevel()
    {
        try
        {
            if (ReportHazards?.Any() != true)
            {
                _logger.LogInformation("No hazards found, defaulting to Low risk");
                return RiskLevel.Low.Value; // Default to low risk if no hazards
            }

            // Get the highest risk level from all assessed hazards using matrix codes
            var highestRisk = RiskLevel.Low; // Start with lowest

            foreach (var hazard in ReportHazards)
            {
                _logger.LogInformation("Analyzing hazard {HazardCode}: Technical={InitialMatrix}, Residual={ResidualMatrix}", 
                    hazard.Code, hazard.InitialRiskMatrixCode, hazard.ResidualRiskMatrixCode);

                // Check both initial and residual matrix codes to determine risk levels
                var initialRisk = GetRiskLevelFromMatrixCode(hazard.InitialRiskMatrixCode);
                var residualRisk = GetRiskLevelFromMatrixCode(hazard.ResidualRiskMatrixCode);

                _logger.LogInformation("Risk levels for {HazardCode}: Technical={InitialRisk}, Residual={ResidualRisk}", 
                    hazard.Code, initialRisk.Value, residualRisk.Value);

                // Use the higher of initial or residual risk
                var hazardMaxRisk = GetHigherRiskLevel(initialRisk, residualRisk);
                highestRisk = GetHigherRiskLevel(highestRisk, hazardMaxRisk);

                _logger.LogInformation("Hazard {HazardCode} max risk: {MaxRisk}, Overall highest: {HighestRisk}", 
                    hazard.Code, hazardMaxRisk.Value, highestRisk.Value);
            }

            _logger.LogInformation("Assessment {AssessmentCode} final risk level determined: {RiskLevel}", 
                TechRiskAssessment?.Code, highestRisk.Value);

            return highestRisk.Value;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error determining assessment risk level, defaulting to Medium");
            return RiskLevel.Medium.Value;
        }
    }

    /// <summary>
    /// Gets the overall risk score for the assessment
    /// </summary>
    private decimal GetAssessmentRiskScore()
    {
        try
        {
            if (ReportHazards?.Any() != true)
                return 0m;

            // Calculate average risk score across all hazards
            var totalScore = 0m;
            var scoredHazards = 0;

            foreach (var hazard in ReportHazards)
            {
                // Use residual score if available, otherwise initial score
                if (hazard.ResidualAverageScore.HasValue)
                {
                    totalScore += hazard.ResidualAverageScore.Value;
                    scoredHazards++;
                }
                else if (hazard.InitialAverageScore.HasValue)
                {
                    totalScore += hazard.InitialAverageScore.Value;
                    scoredHazards++;
                }
            }

            return scoredHazards > 0 ? totalScore / scoredHazards : 0m;
        }
        catch
        {
            return 0m;
        }
    }

    /// <summary>
    /// Checks if a risk level is considered high risk for SPI tracking
    /// </summary>
    private static bool IsHighRiskLevel(string riskLevel)
    {
        return string.Equals(riskLevel, RiskLevel.Critical.Value, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(riskLevel, RiskLevel.High.Value, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets risk level from a matrix code string
    /// Properly handles aviation matrix codes like "4E", "3C", etc.
    /// </summary>
    private RiskLevel GetRiskLevelFromMatrixCode(string? matrixCode)
    {
        if (string.IsNullOrEmpty(matrixCode))
        {
            _logger.LogDebug("Empty matrix code, returning Low risk");
            return RiskLevel.Low;
        }

        var code = matrixCode.ToUpperInvariant().Trim();
        _logger.LogDebug("Parsing matrix code: '{MatrixCode}' -> '{CleanCode}'", matrixCode, code);

        // Handle aviation matrix codes (e.g., "4E", "3C", "5D")
        if (TryParseAviationMatrixCode(code, out int severity, out int likelihood))
        {
            var riskLevel = GetRiskLevelFromSeverityLikelihood(severity, likelihood);
            _logger.LogDebug("Aviation matrix: {Code} -> Severity:{Severity}, Likelihood:{Likelihood} -> {RiskLevel}", 
                code, severity, likelihood, riskLevel.Value);
            return riskLevel;
        }

        // Handle text-based risk levels
        var textRisk = code switch
        {
            var c when c.Contains("CRITICAL") || c.Contains("RED") => RiskLevel.Critical,
            var c when c.Contains("HIGH") || c.Contains("ORANGE") => RiskLevel.High,
            var c when c.Contains("MEDIUM") || c.Contains("YELLOW") => RiskLevel.Medium,
            var c when c.Contains("LOW") || c.Contains("GREEN") => RiskLevel.Low,
            _ => RiskLevel.Low
        };

        _logger.LogDebug("Text-based matrix: {Code} -> {RiskLevel}", code, textRisk.Value);
        return textRisk;
    }

    /// <summary>
    /// Tries to parse aviation matrix code like "4E" into severity and likelihood
    /// </summary>
    private static bool TryParseAviationMatrixCode(string code, out int severity, out int likelihood)
    {
        severity = 0;
        likelihood = 0;

        if (code.Length < 2) return false;

        // Extract severity (first part - number)
        if (!int.TryParse(code[0].ToString(), out severity) || severity < 1 || severity > 5)
            return false;

        // Extract likelihood (second part - letter or number)
        var likelihoodChar = code[1];
        likelihood = likelihoodChar switch
        {
            'A' or '1' => 1,
            'B' or '2' => 2,
            'C' or '3' => 3,
            'D' or '4' => 4,
            'E' or '5' => 5,
            _ => 0
        };

        return likelihood > 0;
    }

    /// <summary>
    /// Gets risk level based on aviation matrix severity and likelihood values
    /// Based on the aviation matrix color mapping in RiskLevel enum
    /// </summary>
    private static RiskLevel GetRiskLevelFromSeverityLikelihood(int severity, int likelihood)
    {
        // Get the color from the aviation matrix
        var color = RiskLevel.GetAviationMatrixColor(severity, likelihood);

        return color switch
        {
            "#dc3545" => RiskLevel.Critical,  // Red = Critical
            "#fd7e14" => RiskLevel.High,      // Orange = High  
            "#ffc107" => RiskLevel.Medium,    // Yellow = Medium
            "#28a745" => RiskLevel.Low,       // Green = Low
            _ => RiskLevel.Low                // Default
        };
    }

    /// <summary>
    /// Returns the higher of two risk levels based on authority level
    /// </summary>
    private static RiskLevel GetHigherRiskLevel(RiskLevel level1, RiskLevel level2)
    {
        return level1.RequiredAuthorityLevel >= level2.RequiredAuthorityLevel ? level1 : level2;
    }

    #endregion

    #region Report Description Modal

    private bool ShowDescriptionModal = false;
    private string SelectedDescription = string.Empty;
    private string SelectedReportId = string.Empty;

    private void ShowDescriptionDialog()
    {
        try
        {
            SelectedDescription = SourceReport?.Description ?? "No description available";
            SelectedReportId = SourceReport?.Code ?? "Unknown";
            ShowDescriptionModal = true;
            StateHasChanged();
            _logger.LogInformation("Showing description modal for report {ReportId}", SourceReport?.Code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing description modal for report {ReportId}", SourceReport?.Code);
            _notificationHelper.ShowErrorAsync("Error showing description details");
        }
    }

    private void CloseDescriptionModal()
    {
        ShowDescriptionModal = false;
        SelectedDescription = string.Empty;
        SelectedReportId = string.Empty;
        StateHasChanged();
    }

    #endregion
}

