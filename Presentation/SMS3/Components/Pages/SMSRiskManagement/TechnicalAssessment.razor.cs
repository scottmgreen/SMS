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

    // Keep query parameters for backward compatibility
    // [SupplyParameterFromQuery(Name = "reportId")] public string? ReportId { get; set; }
    [Inject] private AuthenticationService AuthService { get; set; } = default!;

    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<TechnicalAssessment> Logger { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;

    #endregion

    #region State Properties

    private bool IsLoading { get; set; } = true;
    private bool IsSaving { get; set; } = false;
    public int CurrentStep => int.TryParse(StepNumber, out int step) && step >= 1 && step <= 5 ? step : 1;

    #endregion

    #region Core Assessment Data

    public RiskAssessment? TechRiskAssessment { get; set; }
    

    public RiskAnalysis? InitialRiskAnalysis { get; set; }
    public RiskAnalysis? ResidualRiskAnalysis { get; set; }


    public Hazard? PrimaryHazard { get; set; }
    public Report? SourceReport { get; set; }
    public List<Hazard> ReportHazards { get; set; } = new();

    #endregion

    #region Step Models

    public Step1Model Step1 { get; set; } = new();
    public Step2Model Step2 { get; set; } = new();
    public Step3Model Step3 { get; set; } = new();
    public Step4Model Step4 { get; set; } = new();
    public Step5Model Step5 { get; set; } = new();

    #endregion

    #region UI Helper Properties

    public string AssessmentName => GetCurrentAssessmentName();
    public string LeadAssessorName => AvailableAssessors.FirstOrDefault(a => a.Id.Value == Step1.LeadAssessor)?.DisplayName ?? Step1.LeadAssessor;

    // CRITICAL: Make this a property that can trigger change detection
    public List<Hazard> AvailableHazards { get; private set; } = new();

    //public List<Step4Model.PanelMemberScoreData> CompletedScores => Step4?.CompletedScores ?? new();

    #endregion

    #region UI Helper Methods
    private RiskAssessment? GetCurrentAssessment()
    {
        return CurrentStep switch
        {
            5 => TechRiskAssessment ?? TechRiskAssessment,
            _ => TechRiskAssessment
        };
    }
    private string GetCurrentAssessmentName()
    {
        var stepName = GetStepName(CurrentStep);
        var currentAssessment = GetCurrentAssessment();

        // For Step 5, emphasize it's the Residual assessment
        if (CurrentStep == 5)
        {
            var residualAssessment = TechRiskAssessment; //ResidualRiskAssessment ?? currentAssessment;
            return $"Hazard Report:{ReportId} Residual Risk Assessment:{residualAssessment?.Code}"; // - {stepName}";
        }

        // For Steps 1-4, show Initial assessment
        return $"Hazard Report:{ReportId} Initial Assessment: {currentAssessment?.Code}"; // - {stepName}";
    }
    public string GetStepName(int stepNumber)
    {
        return stepNumber switch
        {
            1 => "System Description",
            2 => "Hazard Identification",
            3 => "Risk Analysis",
            4 => "Initial Risk Assessment",
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
    public List<SMSStakeholderUser> AvailableStakeholders { get; set; } = new();
    public List<SMSApplicationUser> AvailableSMSUsers { get; set; } = new();
    public List<SMSStakeholderGroup> StakeholderGroups { get; set; } = new();

    #endregion

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        Logger.LogInformation("TechnicalAssessment OnInitializedAsync - ReportId: {ReportId}, StepNumber: {StepNumber}, HazardId: {HazardId}", ReportId, StepNumber, HazardId);

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
        Logger.LogInformation("TechnicalAssessment OnParametersSetAsync - ReportId: {ReportId}, StepNumber: {StepNumber}, HazardId: {HazardId}",
            ReportId, StepNumber, HazardId);

        // Handle route parameter changes
        var currentStep = CurrentStep;
        if (currentStep < 1 || currentStep > 5)
        {
            // Invalid step, redirect to step 1
            Logger.LogWarning("Invalid step {CurrentStep}, redirecting to step 1", currentStep);
            await NavigateToStep(1);
            return;
        }

        // Log the step change
        if (StepNumber != null)
        {
            Logger.LogInformation("Parameter change detected - Step: {StepNumber}", StepNumber);
        }

        // ✅ CRITICAL FIX: Reload Step3 data when navigating to Step 3
        if (currentStep == 3)
        {
            Logger.LogInformation("Navigating to Step 3 - reloading Step3 data to ensure HazardRiskAnalyses is complete");
            
            if (TechRiskAssessment != null && ReportHazards?.Any() == true)
            {
                await Step3.LoadFromAssessmentAsync(TechRiskAssessment, Mediator, ReportHazards);
            }
            else
            {
                Logger.LogWarning("Cannot reload Step3 data - missing InitialRiskAssessment or ReportHazards");
            }
        }

        // If StepNumber parameter changed, we need to refresh the UI
        await InvokeAsync(StateHasChanged);

        Logger.LogInformation("OnParametersSetAsync completed - Current step: {CurrentStep}", CurrentStep);
    }

    #endregion

    #region Data Loading Methods

    private async Task LoadAssessmentDataAsync()
    {
        try
        {
            IsLoading = true;
            StateHasChanged();

            Logger.LogInformation("Loading Technical Assessment - Step {StepNumber}, ReportId: {ReportId}, HazardId: {HazardId}",
                CurrentStep, ReportId, HazardId);

            await LoadCoreAssessmentDataAsync();
            await LoadReportHazardsAsync(); // ✅ FIXED: Load hazards BEFORE loading step data
            await LoadStepDataFromAssessment(); // Step models need ReportHazards to be populated
            await LoadReferenceDataAsync();

            Logger.LogInformation("Successfully loaded Technical Assessment data");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading Technical Assessment data");
            ShowErrorNotification("Failed to load assessment data. Please try again.");
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
        if (TechRiskAssessment == null)
        {
            throw new InvalidOperationException($"No Initial Risk Assessment found. HazardId: {HazardId}, ReportId: {ReportId}");
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

        //Logger.LogInformation("Successfully loaded assessments - Initial: {InitialCode}, Residual: {ResidualCode}", InitialRiskAssessment.Code, ResidualRiskAssessment?.Code ?? "None");
    }

    private async Task LoadAssessmentsByHazardCodeAsync()
    {
        Logger.LogInformation("Loading assessments for HazardId: {HazardId}", HazardId);

        var getAllAssessmentsQuery = new GetRiskAssessmentsByHazardCodeQuery(new HazardID(HazardId));
        var allAssessmentsResult = await Mediator.SendAsync(getAllAssessmentsQuery, CancellationToken.None);

        if (allAssessmentsResult.IsSuccess && allAssessmentsResult.Value?.Any() == true)
        {
            var assessments = allAssessmentsResult.Value.ToList();

            TechRiskAssessment = assessments.FirstOrDefault(x => x.AssessmentType == RiskAssessmentType.Initial);
            //ResidualRiskAssessment = assessments.FirstOrDefault(x => x.AssessmentType == RiskAssessmentType.Residual);

            Logger.LogInformation("Found {Count} assessments for hazard {HazardId}", assessments.Count, HazardId);
        }
        else
        {
            Logger.LogWarning("No risk assessments found for HazardId: {HazardId}", HazardId);
        }
    }


    private async Task LoadAnalysisByHazardCodeAsync()
    {
        Logger.LogInformation("Loading assessments for HazardId: {HazardId}", HazardId);

        var getAllAnalysisQuery = new GetAllRiskAnalysisQuery();
        var allAllAnalysisResult = await Mediator.SendAsync(getAllAnalysisQuery, CancellationToken.None);

        if (allAllAnalysisResult.IsSuccess && allAllAnalysisResult.Value?.Any() == true)
        {
            var anlysis = allAllAnalysisResult.Value.ToList();

            InitialRiskAnalysis = anlysis.FirstOrDefault(x => x.HazardCode == HazardId & x.RiskAssessmentCode.Trim() == TechRiskAssessment.Code);
            ResidualRiskAnalysis = anlysis.FirstOrDefault(x => x.HazardCode == HazardId & x.RiskAssessmentCode.Trim() == TechRiskAssessment.Code);

            Logger.LogInformation("Found {Count} Risk Analysis for hazard {HazardId}", anlysis.Count, HazardId);
        }
        else
        {
            Logger.LogWarning("No Risk Analysis found for HazardId: {HazardId}", HazardId);
        }
    }





    private async Task LoadAssessmentByReportCodeAsync()
    {
        Logger.LogInformation("Loading assessment by Report ID: {ReportId}", ReportId);

        try
        {
            // If ReportId looks like a Report ID (RP-xxxx), try to find assessments for this report
            if (ReportId?.StartsWith("RP-") == true)
            {
                await LoadAssessmentsByReportCodeAsync();

                // If no assessments found, create them
                if (TechRiskAssessment == null && !string.IsNullOrEmpty(HazardId))
                {
                    await CreateAssessmentsForReport();
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading assessment by Report ID: {ReportId}", ReportId);
        }
    }
    private async Task CreateAssessmentsForReport()
    {
        Logger.LogInformation("Creating new assessments for ReportId: {ReportId}, Hazard: {HazardId}", ReportId, HazardId);

        try
        {
            // Generate Placeholder IDs THESE WILL BE GENERATED IN THE DATABASE 
            var initialId = $"RS-0000";
            var residualId = $"RS-0000";

            // Create Initial assessment using the public constructor
            var initialAssessment = new RiskAssessment(new RiskAssessmentID(initialId))
            {
                Name = $"Technical Risk Assessment for Report {ReportId}",
                LeadAssessorId = LeadAssessorName,
                AssessmentType = RiskAssessmentType.Initial,
                RiskAssessmentCategory = RiskAssessmentCategory.Technical,
                HazardCode = HazardId,
                PrimaryHazardId = HazardId,
                Description = $"Created from Report {ReportId}",
                Stage = "Created",
                Code = initialId,
                Status = RiskAssessmentStatus.Created,
                CurrentStep = 1,
                UpdatedDate = DateTime.UtcNow,
                UpdatedBy = AuthService.CurrentUserDisplayName
            };

            // Save Initial assessment
            var createInitialCommand = new CreateRiskAssessmentCommand(initialAssessment);
            var initialResult = await Mediator.SendAsync(createInitialCommand, CancellationToken.None);

            if (!initialResult.IsSuccess)
            {
                throw new Exception($"Failed to create Initial assessment: {initialResult.Error?.Message}");
            }

            TechRiskAssessment = initialResult.Value;

            // Create Residual assessment
            //var residualAssessment = new RiskAssessment(new RiskAssessmentID(residualId))
            //{
            //    Name = $"Residual Risk Assessment for Report {ReportId}",
            //    LeadAssessorId = LeadAssessorName,
            //    AssessmentType = RiskAssessmentType.Residual,
            //    RiskAssessmentCategory = RiskAssessmentCategory.Technical,
            //    HazardCode = HazardId,
            //    PrimaryHazardId = HazardId,
            //    Description = $"Created from Initial Assessment {InitialRiskAssessment.Code}",
            //    Stage = "Created",
            //    Code = residualId,
            //    Status = RiskAssessmentStatus.Created,
            //    CurrentStep = 1,
            //    UpdatedDate = DateTime.UtcNow,
            //    UpdatedBy = AuthService.CurrentUserDisplayName
            //};

            // Save Residual assessment
            //var createResidualCommand = new CreateRiskAssessmentCommand(residualAssessment);
            //var residualResult = await Mediator.SendAsync(createResidualCommand, CancellationToken.None);

            //if (residualResult.IsSuccess)
            //{
            //    ResidualRiskAssessment = residualResult.Value;
            //}

            Logger.LogInformation("Created assessments - Initial: {InitialId}, Residual: {ResidualId}",
                initialId, residualId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating assessments for ReportId: {ReportId}", ReportId);
            throw;
        }
    }
    
    private async Task LoadAssessmentsByReportCodeAsync()
    {
        Logger.LogInformation("Loading assessments via ReportId: {ReportId}", ReportId);

        // First, find hazards for this report
        var reportHazardQuery = new GetHazardsByReportIdQuery(new ReportID(ReportId));
        var reportHazardResult = await Mediator.SendAsync(reportHazardQuery, CancellationToken.None);

        if (reportHazardResult.IsSuccess && reportHazardResult.Value?.Any() == true)
        {
            var reportHazards = reportHazardResult.Value.ToList();
            Logger.LogInformation("Found {Count} hazards for report {ReportId}", reportHazards.Count, ReportId);

            // Try to load assessments for each hazard until we find one
            foreach (var hazard in reportHazards)
            {
                var assessmentsQuery = new GetRiskAssessmentsByHazardCodeQuery(new HazardID(hazard.Code));
                var assessmentsResult = await Mediator.SendAsync(assessmentsQuery, CancellationToken.None);

                if (assessmentsResult.IsSuccess && assessmentsResult.Value?.Any() == true)
                {
                    var assessments = assessmentsResult.Value.ToList();

                    // Take the first valid set of assessments we find
                    if (TechRiskAssessment == null)
                    {
                        TechRiskAssessment = assessments.FirstOrDefault(x => x.AssessmentType == RiskAssessmentType.Initial);
                        HazardId = hazard.Code; // Update HazardId for consistency
                    }

                    //if (ResidualRiskAssessment == null)
                    //{
                    //    ResidualRiskAssessment = assessments.FirstOrDefault(x => x.AssessmentType == RiskAssessmentType.Residual);
                    //}

                    Logger.LogInformation("Found assessments via hazard {HazardCode}", hazard.Code);

                    // If we found what we need, no need to check other hazards
                    if (TechRiskAssessment != null) break;
                }
            }
        }
        else
        {
            Logger.LogWarning("No hazards found for ReportId: {ReportId}", ReportId);
        }
    }

    private async Task LoadReportHazardsAsync()
    {
        try
        {
            var allHazards = new List<Hazard>();

            // Load primary hazard
            if (!string.IsNullOrEmpty(HazardId))
            {
                var hazardQuery = new GetHazardByIdQuery(new HazardID(HazardId));
                var hazardResult = await Mediator.SendAsync(hazardQuery, CancellationToken.None);

                if (hazardResult.IsSuccess && hazardResult.Value != null)
                {
                    allHazards.Add(hazardResult.Value);
                    PrimaryHazard = hazardResult.Value;
                }

            }

            // Load additional hazards from report
            if (!string.IsNullOrEmpty(ReportId))
            {
                var reportHazardQuery = new GetHazardsByReportIdQuery(new ReportID(ReportId.Trim()));
                var reportHazardResult = await Mediator.SendAsync(reportHazardQuery, CancellationToken.None);

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
                        var hazardQuery = new GetHazardByIdQuery(new HazardID(hazardIdString));
                        var hazardResult = await Mediator.SendAsync(hazardQuery, CancellationToken.None);

                        if (hazardResult.IsSuccess && hazardResult.Value != null
                            && !allHazards.Any(h => h.Code == hazardResult.Value.Code))
                        {
                            allHazards.Add(hazardResult.Value);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(ex, "Failed to load identified hazard {HazardId}", hazardIdString);
                    }
                }
            }

            // CRITICAL: Update both collections
            ReportHazards = allHazards;
            AvailableHazards = allHazards.ToList(); // Create a new list to trigger change detection

            Logger.LogInformation("Loaded {Count} hazards for assessment", allHazards.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading report hazards");
            ReportHazards = new List<Hazard>();
            AvailableHazards = new List<Hazard>();
        }
    }

    private async Task LoadStepDataFromAssessment()
    {
        if (TechRiskAssessment == null) return;

        try
        {
            Step1.LoadFromAssessment(TechRiskAssessment);
            Step2.LoadFromAssessment(TechRiskAssessment);

            await Step3.LoadFromAssessmentAsync(TechRiskAssessment, Mediator, ReportHazards);
            
            Step4.LoadFromAssessment(TechRiskAssessment);
            await Step4.LoadExistingScoringPanelsAsync(Mediator, ReportHazards);

            // Step 5 uses ResidualRiskAssessment for loading residual analyses
            var step5Assessment = TechRiskAssessment;//ResidualRiskAssessment ?? InitialRiskAssessment;
            await Step5.LoadFromAssessmentAsync(step5Assessment, Mediator, ReportHazards);

            Logger.LogInformation("Step models loaded from assessment, including RiskAnalysis entities");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading step models from assessment");
        }
    }

    private async Task LoadReferenceDataAsync()
    {
        try
        {
            // Load SMS Application Users
            var smsUsersQuery = new GetAllSMSApplicationUsersQuery();
            var smsUsersResult = await Mediator.SendAsync(smsUsersQuery, CancellationToken.None);
            if (smsUsersResult.IsSuccess)
            {
                AvailableSMSUsers = smsUsersResult.Value?.Where(u => u.IsActive).ToList() ?? new List<SMSApplicationUser>();

                // ENHANCED: Filter for Safety Team members only
                AvailableAssessors = AvailableSMSUsers
                    .Where(u => u.IsActive && IsSafetyTeamMember(u))
                    .ToList();

                Logger.LogInformation("Filtered to {SafetyTeamCount} Safety Team assessors from {TotalCount} total SMS users",
                    AvailableAssessors.Count, AvailableSMSUsers.Count);
            }

            // Load Stakeholder Users
            var stakeholdersQuery = new GetActiveSMSStakeholderUsersQuery();
            var stakeholdersResult = await Mediator.SendAsync(stakeholdersQuery, CancellationToken.None);
            if (stakeholdersResult.IsSuccess)
            {
                AvailableStakeholders = stakeholdersResult.Value?.ToList() ?? new List<SMSStakeholderUser>();
            }

            // Load Stakeholder Groups
            var groupsQuery = new GetAllSMSStakeholderGroupsQuery();
            var groupsResult = await Mediator.SendAsync(groupsQuery, CancellationToken.None);
            if (groupsResult.IsSuccess)
            {
                StakeholderGroups = groupsResult.Value?.ToList() ?? new List<SMSStakeholderGroup>();
            }

            Logger.LogInformation("Reference data loaded - SMS Users: {SMS}, Safety Team Assessors: {Assessors}, Stakeholders: {Stakeholders}, Groups: {Groups}",
                AvailableSMSUsers.Count, AvailableAssessors.Count, AvailableStakeholders.Count, StakeholderGroups.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading reference data");
        }
    }

    /// <summary>
    /// Determine if a SMS Application User is a member of the Safety Team
    /// This method checks various criteria to identify Safety Team members
    /// </summary>
    private bool IsSafetyTeamMember(SMSApplicationUser user)
    {
        if (user == null || !user.IsActive) return false;

        // Check if user has Safety Team role
        if (user.UserRole?.Name != null)
        {
            var roleName = user.UserRole.Name.ToLowerInvariant();
            if (roleName.Contains("safety") ||
                roleName.Contains("assessor") ||
                roleName.Contains("risk") ||
                roleName.Contains("sms") ||
                roleName.Equals("safety team", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        // Check SMSUserType property
        if (!string.IsNullOrEmpty(user.SMSUserType))
        {
            var userType = user.SMSUserType.ToLowerInvariant();
            if (userType.Contains("safety") ||
                userType.Contains("assessor") ||
                userType.Contains("risk") ||
                userType.Contains("sms"))
            {
                return true;
            }
        }

        // Check user code patterns
        if (!string.IsNullOrEmpty(user.Code))
        {
            var userCode = user.Code.ToLowerInvariant();
            if (userCode.Contains("safety") ||
                userCode.Contains("sms") ||
                userCode.StartsWith("st-") // Safety Team prefix
                || userCode.StartsWith("ra-"))   // Risk Assessor prefix
            {
                return true;
            }
        }

        // Check username patterns
        if (user.UserName?.Value != null)
        {
            var username = user.UserName.Value.ToLowerInvariant();
            if (username.Contains("safety") ||
                username.Contains("sms") ||
                username.Contains("risk") ||
                username.Contains("assessor"))
            {
                return true;
            }
        }

        // Fallback: For development/demo purposes, if no specific roles are configured,
        // allow any active user to be considered a potential assessor
        // TODO: Remove this fallback once proper role configuration is in place
        if (user.UserRole?.Name == null || string.IsNullOrEmpty(user.UserRole.Name))
        {
            Logger.LogWarning("User {UserCode} has no role assigned - including in assessors for development purposes", user.Code);
            return true; // Temporarily allow users without roles
        }

        return false;
    }

    #endregion

    #region Navigation Methods

    private async Task NavigateToStep(int targetStep)
    {
        if (targetStep < 1 || targetStep > 5) return;

        // Check if we're trying to jump ahead without saving current progress
        if (targetStep > CurrentStep && IsSaving)
        {
            Logger.LogInformation("Navigation blocked - currently saving step {CurrentStep}", CurrentStep);
            return;
        }

        // If jumping forward by more than one step, validate current step first
        if (targetStep > CurrentStep + 1)
        {
            var validationResult = ValidateCurrentStep();
            if (!validationResult.isValid)
            {
                ShowErrorNotification($"Please complete Step {CurrentStep} before proceeding to Step {targetStep}");
                return;
            }

            // Save current step before jumping
            var saveResult = await SaveCurrentStepAsync();
            if (!saveResult.success)
            {
                ShowErrorNotification($"Please save Step {CurrentStep} before proceeding to Step {targetStep}");
                return;
            }
        }

        // Include the step number in the URL
        var navigationUrl = $"/SMSRiskManagement/TechnicalAssessment/{ReportId}/{HazardId}/{targetStep}";

        Logger.LogInformation("Navigating to: {Url}", navigationUrl);
        Navigation.NavigateTo(navigationUrl);
    }

    private async Task PreviousStep()
    {
        if (CurrentStep > 1)
        {
            await NavigateToStep(CurrentStep - 1);
        }
        else
        {
            Logger.LogInformation("Already at first step, cannot go to previous step");
        }
    }

    private async Task NextStep()
    {
        try
        {
            IsSaving = true;
            StateHasChanged();

            // Validate current step
            var validationResult = ValidateCurrentStep();
            if (!validationResult.isValid)
            {
                ShowErrorNotification($"Step {CurrentStep} validation failed: {validationResult.message}");
                return;
            }

            // Save current step
            var saveResult = await SaveCurrentStepAsync();
            IsSaving = !saveResult.success;
            if (!saveResult.success)
            {
                ShowErrorNotification($"Failed to save Step {CurrentStep}: {saveResult.message}");
                return;
            }

            // Navigate to next step
            if (CurrentStep < 5)
            {
                await NavigateToStep(CurrentStep + 1);
                ShowSuccessNotification("Step saved successfully");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in NextStep");
            ShowErrorNotification("Error proceeding to next step");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    private async Task CompleteAssessment()
    {
        try
        {
            IsSaving = true;
            StateHasChanged();

            // Validate all steps
            var allStepsValid = ValidateAllSteps();
            if (!allStepsValid.isValid)
            {
                ShowErrorNotification($"Assessment cannot be completed: {allStepsValid.message}");
                return;
            }

            // Save final step
            var saveResult = await SaveCurrentStepAsync();
            if (!saveResult.success)
            {
                ShowErrorNotification($"Failed to save final step: {saveResult.message}");
                return;
            }

            // Mark assessment as complete and save
            await CompleteAssessmentProcess();

            ShowSuccessNotification("Technical Assessment completed successfully!");

            // Navigate back to report processing
            Navigation.NavigateTo("/SMSRiskManagement/ReportProcessing");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error completing assessment");
            ShowErrorNotification("Error completing assessment");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    #endregion

    #region Save Methods

    private async Task SaveCurrentStep()
    {
        try
        {
            IsSaving = true;
            StateHasChanged();

            var saveResult = await SaveCurrentStepAsync();
            if (saveResult.success)
            {
                ShowSuccessNotification("Step saved successfully");
            }
            else
            {
                ShowErrorNotification($"Failed to save step: {saveResult.message}");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in SaveCurrentStep");
            ShowErrorNotification("Error saving step");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    private async Task<(bool success, string message)> SaveCurrentStepAsync()
    {
        if (TechRiskAssessment == null)
        {
            return (false, "Assessment not loaded");
        }

        try
        {
            // Apply current step to assessment - ENHANCED: Use async method
            await ApplyCurrentStepToAssessmentAsync();

            // ENHANCEMENT: Update the current step in the assessment
            TechRiskAssessment.CurrentStep = CurrentStep;
            //ResidualRiskAssessment.CurrentStep = CurrentStep;

            // ENHANCEMENT: Update status based on current step
            UpdateAssessmentAndReportStatus();

            // Save to database
            var updateCommand = new UpdateRiskAssessmentCommand(TechRiskAssessment);
            var initalresult = await Mediator.SendAsync(updateCommand, CancellationToken.None);


            //updateCommand = new UpdateRiskAssessmentCommand(ResidualRiskAssessment);
            //var residualresult = await Mediator.SendAsync(updateCommand, CancellationToken.None);


            if (initalresult.IsSuccess)
            {
                TechRiskAssessment = initalresult.Value; // Update with latest data
                //ResidualRiskAssessment = residualresult.Value;

                // ENHANCEMENT: Also update the associated Hazard status
                await UpdateHazardStatusForProgress();

                Logger.LogInformation("Step {CurrentStep} saved successfully for assessment {AssessmentCode}",CurrentStep, TechRiskAssessment.Code);

                return (true, $"Step {CurrentStep} saved successfully");
            }
            else
            {
                Logger.LogError("Failed to save step {CurrentStep}: {Error}", CurrentStep, initalresult.Error?.Message);
                return (false, initalresult.Error?.Message ?? "Save failed");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving step {Step}", CurrentStep);
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// Update the assessment status and related entities based on current progress
    /// </summary>
    private void UpdateAssessmentAndReportStatus()
    {
        if (TechRiskAssessment == null) return;

        // Update assessment status (enum) based on current step
        TechRiskAssessment.Status = CurrentStep > 0 ? RiskAssessmentStatus.InProgress : RiskAssessmentStatus.Created;
        //ResidualRiskAssessment.Status = CurrentStep > 0 ? RiskAssessmentStatus.InProgress : RiskAssessmentStatus.Created;

        // Update last modified info
        TechRiskAssessment.UpdatedDate = DateTime.UtcNow;
        TechRiskAssessment.UpdatedBy = AuthService.CurrentUserDisplayName; 
        //ResidualRiskAssessment.UpdatedDate = DateTime.UtcNow;
        //ResidualRiskAssessment.UpdatedBy = AuthService.CurrentUserDisplayName;

        //Logger.LogInformation("Updated InitialRiskAssessment stage to: {Stage} and status to: {Status} for step {Step}", InitialRiskAssessment.Stage, InitialRiskAssessment.Status.Name, CurrentStep);
        //Logger.LogInformation("Updated ResidualRiskAssessment stage to: {Stage} and status to: {Status} for step {Step}", ResidualRiskAssessment.Stage, ResidualRiskAssessment.Status.Name, CurrentStep);
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
            var hazardQuery = new GetHazardByIdQuery(new HazardID(HazardId));
            var hazardResult = await Mediator.SendAsync(hazardQuery, CancellationToken.None);

            if (hazardResult.IsSuccess && hazardResult.Value != null)
            {
                var hazard = hazardResult.Value;

                // Update hazard status based on assessment progress
                var originalStatus = hazard.Status?.ToString();
                hazard.Status = DetermineHazardStatusFromStep(CurrentStep);

                // Only update if status changed
                if (hazard.Status?.ToString() != originalStatus)
                {
                    var updateHazardCommand = new UpdateHazardCommand(hazard);
                    var updateResult = await Mediator.SendAsync(updateHazardCommand, CancellationToken.None);

                    if (updateResult.IsSuccess)
                    {
                        Logger.LogInformation("Updated hazard {HazardId} status from {OldStatus} to {NewStatus}",
                            HazardId, originalStatus, hazard.Status?.ToString());
                    }
                    else
                    {
                        Logger.LogWarning("Failed to update hazard status: {Error}", updateResult.Error?.Message);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating hazard status for progress");
        }
    }

    /// <summary>
    /// Determine appropriate hazard status based on assessment step
    /// </summary>
    private HazardStatus DetermineHazardStatusFromStep(int step)
    {
        return step switch
        {
            1 => HazardStatus.UnderReview, // System description
            2 => HazardStatus.UnderReview, // Hazard identification
            3 => HazardStatus.UnderReview, // Risk analysis
            4 => HazardStatus.UnderReview, // Risk assessment
            5 => HazardStatus.UnderReview, // Risk mitigation
            _ => HazardStatus.Active
        };
    }

    #endregion

    #region Validation Methods

    private (bool isValid, string message) ValidateCurrentStep()
    {
        return CurrentStep switch
        {
            1 => Step1.Validate(),
            2 => ValidateStep2(),
            3 => Step3.Validate(AvailableHazards),
            4 => Step4.Validate(),
            5 => Step5.Validate(),
            _ => (false, "Invalid step number")
        };
    }

    private (bool isValid, string message) ValidateStep2()
    {
        var hazardCount = ReportHazards?.Count ?? 0;

        if (hazardCount < 1)
        {
            return (false, "At least 1 hazard must be identified before proceeding to Step 3");
        }

        return (true, $"Step 2 validation passed with {hazardCount} hazard(s)");
    }

    private (bool isValid, string message) ValidateAllSteps()
    {
        // Validate each step in sequence
        var step1Result = Step1.Validate();
        if (!step1Result.isValid)
            return (false, $"Step 1: {step1Result.message}");

        var step2Result = ValidateStep2();
        if (!step2Result.isValid)
            return (false, $"Step 2: {step2Result.message}");

        var step3Result = Step3.Validate(AvailableHazards);
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

    #endregion

    #region Notification Methods

    private void ShowErrorNotification(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = "Error",
            Detail = message
        });
    }

    private void ShowSuccessNotification(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = "Success",
            Detail = message
        });
    }

    #endregion

    private async Task CompleteAssessmentProcess()
    {
        if (TechRiskAssessment == null) return;

        // Apply all steps to ensure everything is saved - ENHANCED: Use async method
        await ApplyCurrentStepToAssessmentAsync();

        // Save final state
        var updateCommand = new UpdateRiskAssessmentCommand(TechRiskAssessment);
        await Mediator.SendAsync(updateCommand, CancellationToken.None);
    }

    //private void ApplyCurrentStepToAssessment()
    //{
    //    switch (CurrentStep)
    //    {
    //        case 1:
    //            Step1.ApplyToAssessment(InitialRiskAssessment!);
    //            break;
    //        case 2:
    //            Step2.ApplyToAssessment(InitialRiskAssessment!);
    //            break;
    //        case 3:
    //            Step3.ApplyToAssessment(InitialRiskAssessment!);
    //            break;
    //        case 4:
    //            Step4.ApplyToAssessment(InitialRiskAssessment!);
    //            break;
    //        case 5:
    //            Step5.ApplyToAssessment(InitialRiskAssessment!);
    //            break;
    //    }
    //}

    private async Task ApplyCurrentStepToAssessmentAsync()
    {
        switch (CurrentStep)
        {
            case 1:
                Step1.ApplyToAssessment(TechRiskAssessment!);
                //Step1.ApplyToAssessment(ResidualRiskAssessment!);
                break;
            case 2:
                Step2.ApplyToAssessment(TechRiskAssessment!);
                break;
            case 3:
                await Step3.ApplyToAssessmentAsync(TechRiskAssessment!, Mediator, AvailableHazards);
                break;
            case 4:
                await Step4.ApplyToAssessmentAsync(TechRiskAssessment!, Mediator, AvailableHazards);
                break;
            case 5:
                Step5.ApplyToAssessment(TechRiskAssessment!);
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
            Logger.LogInformation("AddHazard method called with hazard: {Description} (Code: {Code})", newHazard.Description, newHazard.Code);

            // CRITICAL: Check if hazard already exists to prevent duplication
            if (ReportHazards.Any(h => h.Code == newHazard.Code && h.Code != "HZ-0000"))
            {
                Logger.LogWarning("Hazard {HazardCode} already exists in ReportHazards collection, skipping duplication", newHazard.Code);
                return;
            }

            // CRITICAL: Check if hazard with same description already exists (in case of rapid duplicate submissions)
            if (ReportHazards.Any(h => h.Description?.Trim().Equals(newHazard.Description?.Trim(), StringComparison.OrdinalIgnoreCase) == true))
            {
                Logger.LogWarning("Hazard with description '{Description}' already exists in ReportHazards collection, skipping duplication", newHazard.Description);
                ShowErrorNotification("A hazard with this description already exists.");
                return;
            }

            // The hazard was already created in the modal - we just need to add it to our collections
            Logger.LogInformation("Hazard {HazardCode} was successfully created in modal, adding to collections", newHazard.Code);

            // Add to collections (no database call needed here - already done in modal)
            ReportHazards.Add(newHazard);

            // Create a completely new list to force parameter change detection
            AvailableHazards = ReportHazards.ToList();

            // Update Step2 model
            if (!Step2.HazardIds.Contains(newHazard.Code))
            {
                Step2.HazardIds.Add(newHazard.Code);
                Step2.HazardDescriptions.Add(newHazard.Description ?? string.Empty);
                Step2.HazardCategories.Add(newHazard.HazardCategory ?? string.Empty);
            }
            else
            {
                Logger.LogWarning("Hazard {HazardCode} already exists in Step2 model, skipping Step2 update", newHazard.Code);
            }

            // ✅ FIXED: Also update the assessment's IdentifiedHazardIds list
            if (TechRiskAssessment != null && !TechRiskAssessment.IdentifiedHazardIds.Contains(newHazard.Code))
            {
                TechRiskAssessment.AddIdentifiedHazard(newHazard.Code, newHazard.Description ?? string.Empty);
                Logger.LogInformation("Added hazard {HazardCode} to assessment's IdentifiedHazardIds", newHazard.Code);
            }

            // CRITICAL: Force complete UI refresh for parent and all children
            await InvokeAsync(() =>
            {
                StateHasChanged();
            });

            ShowSuccessNotification($"Hazard {newHazard.Code} added successfully");
            Logger.LogInformation("Successfully added hazard to collections: {HazardCode} - Total hazards: {Count}",
                newHazard.Code, AvailableHazards.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in AddHazard method for hazard: {Description}", newHazard.Description);
            ShowErrorNotification("Error adding hazard");
        }
    }

    private async Task UpdateHazard(Hazard updatedHazard)
    {
        try
        {
            Logger.LogInformation("Updating hazard via CQRS: {HazardId}", updatedHazard.Code);

            // Use proper CQRS UpdateHazardCommand
            var updateCommand = new UpdateHazardCommand(updatedHazard);
            var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                // Update local collection
                var existingIndex = ReportHazards.FindIndex(h => h.Code == updatedHazard.Code);
                if (existingIndex >= 0)
                {
                    ReportHazards[existingIndex] = result.Value;
                }

                ShowSuccessNotification($"Hazard {updatedHazard.Code} updated successfully");
                Logger.LogInformation("Successfully updated hazard via CQRS: {HazardCode}", updatedHazard.Code);
            }
            else
            {
                ShowErrorNotification($"Failed to update hazard: {result.Error?.Message}");
                Logger.LogError("CQRS UpdateHazardCommand failed: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating hazard via CQRS");
            ShowErrorNotification("Error updating hazard");
        }
    }

    private async Task DeleteHazard(Hazard hazardToDelete)
    {
        try
        {
            Logger.LogInformation("Deleting hazard: {HazardCode}", hazardToDelete.Code);

            // Check if this is the initial hazard - should not be deleted
            if (hazardToDelete.Code == HazardId)
            {
                ShowErrorNotification("Cannot delete the initial hazard from the report");
                return;
            }

            // Use proper CQRS DeleteHazardCommand
            var deleteCommand = new DeleteHazardCommand(new HazardID(hazardToDelete.Code));
            var result = await Mediator.SendAsync(deleteCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                // Remove from local collections
                ReportHazards.RemoveAll(h => h.Code == hazardToDelete.Code);
                AvailableHazards = ReportHazards.ToList();

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

                // ✅ FIXED: Also remove from assessment's IdentifiedHazardIds
                if (TechRiskAssessment != null)
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
                    Logger.LogInformation("Removed hazard {HazardCode} from assessment's IdentifiedHazardIds", hazardToDelete.Code);
                }

                // Force UI refresh
                await InvokeAsync(StateHasChanged);

                ShowSuccessNotification($"Hazard {hazardToDelete.Code} deleted successfully");
                Logger.LogInformation("Successfully deleted hazard: {HazardCode} - Remaining hazards: {Count}",
                    hazardToDelete.Code, AvailableHazards.Count);
            }
            else
            {
                ShowErrorNotification($"Failed to delete hazard: {result.Error?.Message}");
                Logger.LogError("DeleteHazardCommand failed: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting hazard");
            ShowErrorNotification("Error deleting hazard");
        }
    }

    #endregion
}