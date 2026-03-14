using System.Runtime.Intrinsics.X86;
using SMS_Application.Interfaces;
using SMS_Application.Services;
using SMS3.Components.Pages.SMSRiskManagement.Models;
using SMS3.Components.Shared;
using SMS3.Components.Shared.UIHelpers;

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
    [Inject] private ICurrentUserService CurrentUserService { get; set; } = default!;
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<TechnicalAssessment> Logger { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private INotificationHelper NotificationHelper { get; set; } = default!;
    


    #endregion

    #region State Properties

    private bool IsLoading { get; set; } = true;
    private bool IsSaving { get; set; } = false;
    public int CurrentStep => int.TryParse(StepNumber, out int step) && step >= 1 && step <= 5 ? step : 1;

    #endregion

    #region Core Assessment Data

    public RiskAssessment? TechRiskAssessment { get; set; }
    
    public RiskAnalysis? TechRiskAnalysis { get; set; }
    


    public Hazard? PrimaryHazard { get; set; }
    public Report? SourceReport { get; set; }
    public List<Hazard> ReportHazards { get; set; } = new();

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

    public string LeadInvestigatorName => AvailableInvestigators.FirstOrDefault()?.DisplayName;

    // CRITICAL: Make this a property that can trigger change detection
    public List<Hazard> ReportedHazards { get; private set; } = new();

    
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

        // For Steps 1-4, show Initial stage
        return $"Hazard Report: {ReportId} Risk Assessment: {TechRiskAssessment?.Code} - Initial Stage";
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

    public List<SMSApplicationUser> AvailableInvestigators { get; set; } = new();
    public List<SMSStakeholderUser> AvailableStakeholders { get; set; } = new();
    public List<SMSApplicationUser> AvailableSMSUsers { get; set; } = new();
    public List<SMSStakeholderGroup> StakeholderGroups { get; set; } = new();

    #endregion

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        Logger.LogInformation("TechnicalAssessment OnInitializedAsync - ReportId: {ReportId}, StepNumber: {StepNumber}, HazardId: {HazardId}", ReportId, StepNumber, HazardId);

        // Initialize step models that require dependency injection
        Step3 = new Step3Model(Mediator, CurrentUserService);
        Step4 = new Step4Model(Mediator, CurrentUserService);
        Step5 = new Step5Model(Mediator, CurrentUserService);



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
        Logger.LogInformation("TechnicalAssessment OnParametersSetAsync - ReportId: {ReportId}, StepNumber: {StepNumber}, HazardId: {HazardId}", ReportId, StepNumber, HazardId);

        // Handle route parameter changes
        var currentStep = CurrentStep;
        if (currentStep < 1 || currentStep > 5)
        {
            Logger.LogWarning("Invalid step {CurrentStep}, redirecting to step 1", currentStep);
            await NavigateToStep(1);
            return;
        }
        
        if (currentStep == 3)
        {
            Logger.LogInformation("Navigating to Step 3 - reloading Step3 data to ensure HazardRiskAnalyses is complete");
            // ✅ FIRST: Refresh ReportHazards to include any newly added hazards from Step 2
            await LoadReportHazardsAsync();

            if (TechRiskAssessment != null && ReportHazards?.Any() == true)
            {
                await Step3.LoadFromAssessmentAsync(TechRiskAssessment, ReportHazards);
            }
            else
            {
                Logger.LogWarning("Cannot reload Step3 data - missing TechRiskAssessment or ReportHazards");
            }
        }
        // Log the step change
        if (StepNumber != null)
        {
            Logger.LogInformation("Parameter change detected - Step: {StepNumber}", StepNumber);
        }

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

            Logger.LogInformation("Loading Technical Assessment - Step {StepNumber}, ReportId: {ReportId}, HazardId: {HazardId}",CurrentStep, ReportId, HazardId);

            await LoadCoreAssessmentDataAsync();
            await LoadReportHazardsAsync(); // ✅ FIXED: Load hazards BEFORE loading step data
            await LoadStepDataFromAssessment(); // Step models need ReportHazards to be populated
            await LoadReferenceDataAsync();

            Logger.LogInformation("Successfully loaded Technical Assessment data");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading Technical Assessment data");
            await NotificationHelper.ShowErrorAsync("Failed to load assessment data. Please try again.");
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
        Logger.LogInformation("Loading assessments for HazardId: {HazardId}", HazardId);

        var getAllAssessmentsQuery = new GetRiskAssessmentsByHazardCodeQuery(new HazardID(HazardId));
        var allAssessmentsResult = await Mediator.SendAsync(getAllAssessmentsQuery, CancellationToken.None);

        if (allAssessmentsResult.IsSuccess && allAssessmentsResult.Value?.Any() == true)
        {
            var assessments = allAssessmentsResult.Value.ToList();

            TechRiskAssessment = assessments.FirstOrDefault(x => x.RiskAssessmentCategory == RiskAssessmentCategory.Technical);

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

            TechRiskAnalysis = anlysis.FirstOrDefault(x => x.HazardCode == HazardId & x.RiskAssessmentCode.Trim() == TechRiskAssessment.Code);
            
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
        Logger.LogInformation("Creating new assessment for ReportId: {ReportId}, Hazard: {HazardId}", ReportId, HazardId);

        try
        {
            // Generate Placeholder ID - WILL BEGENERATED IN THE DATABASE 
            var assessmentId = $"RS-0000";

            // Create Technical assessment using the public constructor
            var technicalAssessment = new RiskAssessment(new RiskAssessmentID(assessmentId))
            {
                Name = $"Technical Risk Assessment for Report {ReportId}",
                LeadAssessorId = LeadAssessorName,
                AssessmentType = RiskAssessmentType.Initial, // Start with Initial, Step 5 will use Residual stage
                RiskAssessmentCategory = RiskAssessmentCategory.Technical,
                HazardCode = HazardId,
                PrimaryHazardId = HazardId,
                Description = $"Created from Report {ReportId}",
                Stage = DetermineRiskAssessmentStageFromStep(1),
                Code = assessmentId,
                Status = RiskAssessmentStatus.AssessmentCreate,
                CurrentStep = 1,
                UpdatedDate = DateTime.UtcNow,
                UpdatedBy = CurrentUserService?.UserDisplayName
            };

            // Save Technical assessment
            var createCommand = new CreateRiskAssessmentCommand(technicalAssessment);
            var result = await Mediator.SendAsync(createCommand, CancellationToken.None);

            if (!result.IsSuccess)
            {
                throw new Exception($"Failed to create Technical assessment: {result.Error?.Message}");
            }

            TechRiskAssessment = result.Value;

            Logger.LogInformation("Created Technical assessment: {AssessmentId}", assessmentId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating assessment for ReportId: {ReportId}", ReportId);
            throw;
        }
    }
    
    private async Task LoadAssessmentsByReportCodeAsync()
    {
        Logger.LogInformation("Loading assessments via ReportId: {ReportId}", ReportId);

        // First, find hazards for this report
        var reportHazardQuery = new GetHazardsByReportCodeQuery(new ReportID(ReportId));
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

                    // Take the first Technical assessment we find
                    if (TechRiskAssessment == null)
                    {
                        TechRiskAssessment = assessments.FirstOrDefault(x => x.RiskAssessmentCategory == RiskAssessmentCategory.Technical);
                        HazardId = hazard.Code; // Update HazardId for consistency
                    }

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
                var hazardQuery = new GetHazardByCodeQuery(new HazardID(HazardId));
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
                var reportHazardQuery = new GetHazardsByReportCodeQuery(new ReportID(ReportId.Trim()));
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
                        var hazardQuery = new GetHazardByCodeQuery(new HazardID(hazardIdString));
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
            ReportedHazards = allHazards.ToList(); // Create a new list to trigger change detection

            Logger.LogInformation("Loaded {Count} hazards for assessment", allHazards.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading report hazards");
            ReportHazards = new List<Hazard>();
            ReportedHazards = new List<Hazard>();
        }
    }

    private async Task LoadStepDataFromAssessment()
    {
        if (TechRiskAssessment == null) return;

        try
        {
            Step1.LoadFromAssessment(TechRiskAssessment);
            Step2.LoadFromAssessment(TechRiskAssessment);

            await Step3.LoadFromAssessmentAsync(TechRiskAssessment,ReportHazards);
            
            Step4.LoadFromAssessment(TechRiskAssessment);
            await Step4.LoadExistingScoringPanelsAsync(Mediator, ReportHazards);

            // Step 5 uses the same assessment - the step models will determine Initial vs Residual properties
            await Step5.LoadFromAssessmentAsync(TechRiskAssessment, ReportHazards);

            Logger.LogInformation("Step models loaded from assessment, including RiskAnalysis entities");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading step models from assessment");
        }
    }
    private async Task LoadAvailableAssessorsAsync()
    {
        try
        {
            var usersQuery = new GetUsersByApplicationGroupCodeQuery("AG-0007");
            var usersResult = await Mediator.SendAsync(usersQuery, CancellationToken.None);
            if (usersResult.IsSuccess)
            {
                AvailableAssessors = usersResult.Value?.ToList() ?? new List<SMSApplicationUser>();

               
                
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Could not load available assessors");
           
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

               
            }
            // Load Investigators Users
            var investigatorsQuery = new GetUsersByApplicationGroupCodeQuery("AG-0006");
            var investigatorsResult = await Mediator.SendAsync(investigatorsQuery, CancellationToken.None);

            if (investigatorsResult.IsSuccess && investigatorsResult.Value != null)
            {
                AvailableInvestigators = investigatorsResult.Value.ToList();                
            }

            // Load Assessors Users
            var usersQuery = new GetUsersByApplicationGroupCodeQuery("AG-0007");
            var usersResult = await Mediator.SendAsync(usersQuery, CancellationToken.None);
            if (usersResult.IsSuccess)
            {
                AvailableAssessors = usersResult.Value?.ToList() ?? new List<SMSApplicationUser>();
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
                await NotificationHelper.ShowErrorAsync($"Please complete Step {CurrentStep} before proceeding to Step {targetStep}");
                return;
            }

            // Save current step before jumping
            var saveResult = await SaveCurrentStepAsync();
            if (!saveResult.success)
            {
                await NotificationHelper.ShowErrorAsync($"Please save Step {CurrentStep} before proceeding to Step {targetStep}");
                return;
            }
            


        }
        if (targetStep == 3)
        {
            Logger.LogInformation("Navigating to Step 3 - reloading Step3 data to ensure HazardRiskAnalyses is complete");
            // ✅ FIRST: Refresh ReportHazards to include any newly added hazards from Step 2
            await LoadReportHazardsAsync();

            if (TechRiskAssessment != null && ReportHazards?.Any() == true)
            {
                await Step3.LoadFromAssessmentAsync(TechRiskAssessment, ReportHazards);
            }
            else
            {
                Logger.LogWarning("Cannot reload Step3 data - missing TechRiskAssessment or ReportHazards");
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
                await NotificationHelper.ShowErrorAsync($"Step {CurrentStep} validation failed: {validationResult.message}");
                return;
            }

            // Save current step
            var saveResult = await SaveCurrentStepAsync();
            IsSaving = !saveResult.success;
            if (!saveResult.success)
            {
                await NotificationHelper.ShowErrorAsync($"Failed to save Step {CurrentStep}: {saveResult.message}");
                return;
            }

            // Navigate to next step
            if (CurrentStep < 5)
            {
                await NavigateToStep(CurrentStep + 1);
                await NotificationHelper.ShowSuccessAsync("Step saved successfully");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in NextStep");
            await NotificationHelper.ShowErrorAsync("Error proceeding to next step");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    private async Task SubmitAssessment()
    {
        try
        {
            IsSaving = true;
            StateHasChanged();

            // Validate all steps
            var allStepsValid = ValidateAllSteps();
            if (!allStepsValid.isValid)
            {
                await NotificationHelper.ShowErrorAsync($"Assessment cannot be completed: {allStepsValid.message}");
                return;
            }

            // Save final step
            var saveResult = await SaveCurrentStepAsync();
            if (!saveResult.success)
            {
                await NotificationHelper.ShowErrorAsync($"Failed to save final step: {saveResult.message}");
                return;
            }

            // Mark assessment as complete and save
            await CompleteAssessmentProcess();

            await NotificationHelper.ShowSuccessAsync("Technical Assessment completed successfully!");

            // Navigate back to report processing
            Navigation.NavigateTo("/SMSRiskManagement/ReportProcessing");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error completing assessment");
            await NotificationHelper.ShowErrorAsync("Error completing assessment");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    #endregion

    #region Save Methods

    

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
            
            // Update assessment status (enum) based on current step
            TechRiskAssessment.Status = DetermineRiskAssessmentStatusFromStep(CurrentStep); 
            TechRiskAssessment.Stage = DetermineRiskAssessmentStageFromStep(CurrentStep +1);

            // Update last modified info
            TechRiskAssessment.UpdatedDate = DateTime.UtcNow;
            TechRiskAssessment.UpdatedBy = CurrentUserService?.UserDisplayName;
            if (CurrentStep == 5)
            {
                TechRiskAssessment.CompletedBy = CurrentUserService?.UserDisplayName;
                TechRiskAssessment.CompletedDate = DateTime.UtcNow;
            }
            
            // Save to database
            var updateCommand = new UpdateRiskAssessmentCommand(TechRiskAssessment);
            var initalresult = await Mediator.SendAsync(updateCommand, CancellationToken.None);
            
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

            var cmd = new UpdateReportStatusCommand(ReportId, status, CurrentUserService?.UserDisplayName);
            var cmdResult = await Mediator.SendAsync(cmd, CancellationToken.None);
            


            if (initalresult.IsSuccess)
            {
                TechRiskAssessment = initalresult.Value; // Update with latest data

                await UpdateHazardStatusForProgress();

                Logger.LogInformation("Step {CurrentStep} saved successfully for assessment {AssessmentCode}", CurrentStep, TechRiskAssessment.Code);

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
    private RiskAssessmentStage DetermineRiskAssessmentStageFromStep(int step)
    {
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
            var hazardResult = await Mediator.SendAsync(hazardQuery, CancellationToken.None);

            if (hazardResult.IsSuccess && hazardResult.Value != null)
            {
                var hazard = hazardResult.Value;

                // ✅ Use shared method to load scoring panels and get current risk level
                var scoringPanels = await LoadScoringPanelsForHazard(HazardId, CurrentStep, TechRiskAssessment?.Code);
                var (averageScore, matrixCode, riskLevel) = CalculateHazardScoringData(scoringPanels, HazardId);

                // Update hazard status based on assessment progress
                var originalStatus = hazard.Status?.ToString();
                hazard.Status = DetermineHazardStatusFromStep(CurrentStep);
                
                // ✅ Use calculated risk level from scoring panels
                hazard.HazardRiskLevel = riskLevel;

                hazard.UpdatedBy = CurrentUserService?.UserDisplayName;  
                hazard.UpdatedDate = DateTime.UtcNow;   

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
            Logger.LogWarning("Invalid hazard code provided: {HazardCode}", hazardCode);
            return new List<ScoringPanel>();
        }

        try
        {
            Logger.LogInformation("Loading scoring panels for hazard {HazardCode}, Step {CurrentStep}", hazardCode, currentStep);

            // Use the provided risk assessment code or fall back to current assessment
            var targetAssessmentCode = !string.IsNullOrEmpty(riskAssessmentCode) 
                ? riskAssessmentCode.Trim() 
                : TechRiskAssessment?.Code?.Trim();

            if (string.IsNullOrEmpty(targetAssessmentCode))
            {
                Logger.LogWarning("No risk assessment code available for scoring panel filtering");
                return new List<ScoringPanel>();
            }

            // Load all panels for the hazard
            var query = new GetScoringPanelsByHazardCodeQuery(hazardCode);
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (!result.IsSuccess || result.Value.Count == 0)
            {
                Logger.LogWarning("No scoring panels found for hazard {HazardCode}", hazardCode);
                return new List<ScoringPanel>();
            }

            // Copy Step 4 scores to Step 5 if needed (only when loading Step 5)
            if (currentStep == 5)
            {
                await CopyStep4ScoresToStep5IfNeeded(result.Value, targetAssessmentCode);
            }

            // Filter panels by risk assessment code
            var filteredPanels = result.Value
                .Where(p => p.RiskAssessmentCode.Trim() == targetAssessmentCode)
                .ToList();

            // Map properties based on current step for all loaded panels
            foreach (var panel in filteredPanels)
            {
                MapScoringPanelPropertiesBasedOnStep(panel, currentStep);
            }

            Logger.LogInformation("Loaded {Count} scoring panels for hazard {HazardCode}, Step {CurrentStep}", 
                filteredPanels.Count, hazardCode, currentStep);

            return filteredPanels;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading scoring panels for hazard {HazardCode}, Step {CurrentStep}", hazardCode, currentStep);
            return new List<ScoringPanel>();
        }
    }

    /// <summary>
    /// Map scoring panel properties based on assessment step
    /// Step 4 = Initial properties, Step 5 = Residual properties
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
            // Step 4 (default): Map from Initial properties
            panel.Likelihood = panel.InitialLikelihood;
            panel.Severity = panel.InitialSeverity;
            panel.Score = panel.InitialScore;
            panel.Rationale = panel.InitialRationale;
        }
    }

    /// <summary>
    /// Update actual entity properties from mapped properties before saving
    /// Step 4 updates Initial properties, Step 5 updates Residual properties
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
            // Step 4: Update Initial properties from mapped properties
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
            var existingPanels = allPanels
                .Where(p => p.RiskAssessmentCode.Trim() == targetAssessmentCode)
                .ToList();

            if (!existingPanels.Any())
            {
                Logger.LogInformation("No panels found for assessment {AssessmentCode} to copy scores", targetAssessmentCode);
                return;
            }

            // Find panels that have Initial scores but EMPTY Residual scores
            var panelsNeedingCopy = existingPanels
                .Where(p =>
                    // Has Initial scores from Step 4
                    p.InitialSeverity.HasValue && p.InitialLikelihood.HasValue && p.InitialScore.HasValue &&
                    // AND Residual scores are empty (haven't been set in Step 5 yet)
                    !p.ResidualSeverity.HasValue && !p.ResidualLikelihood.HasValue && !p.ResidualScore.HasValue)
                .ToList();

            if (!panelsNeedingCopy.Any())
            {
                Logger.LogInformation("No panels need score copying for assessment {AssessmentCode} - either no Initial scores or Residual scores already exist", targetAssessmentCode);
                return;
            }

            Logger.LogInformation("Copying Initial scores to empty Residual scores for {Count} panels in assessment {AssessmentCode}", panelsNeedingCopy.Count, targetAssessmentCode);

            bool anyUpdated = false;

            foreach (var panel in panelsNeedingCopy)
            {
                // Copy Initial scores to Residual as starting point
                panel.ResidualSeverity = panel.InitialSeverity;
                panel.ResidualLikelihood = panel.InitialLikelihood;
                panel.ResidualScore = panel.InitialScore;
                panel.ResidualRationale = $"Initial assessment: {panel.InitialRationale ?? "No rationale provided"}";

                Logger.LogInformation("Copying Initial scores to empty Residual for panel {PanelCode}: {Sev}x{Like}={Score}", 
                    panel.Code, panel.InitialSeverity, panel.InitialLikelihood, panel.InitialScore);

                // Save the updated panel
                var updateCommand = new UpdateScoringPanelCommand(panel);
                var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

                if (result.IsSuccess)
                {
                    Logger.LogInformation("✅ Successfully copied Initial scores to empty Residual for panel {PanelCode}", panel.Code);
                    anyUpdated = true;
                }
                else
                {
                    Logger.LogError("❌ Failed to copy scores for panel {PanelCode}: {Error}", panel.Code, result.Error?.Message ?? "Unknown error");
                }
            }

            if (anyUpdated)
            {
                Logger.LogInformation("✅ Completed copying Initial scores to empty Residual scores for assessment {AssessmentCode}", targetAssessmentCode);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error copying Step 4 scores to Step 5 for assessment {AssessmentCode}", targetAssessmentCode);
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
            Logger.LogInformation("No completed scoring panels for hazard {HazardCode}", hazardCode);
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

            Logger.LogInformation("Calculated hazard {HazardCode} scoring: AvgSev={Severity:F2}→{RoundedSev}, AvgLike={Likelihood:F2}→{RoundedLike}, Matrix={MatrixCode}, Risk={RiskLevel}", 
                hazardCode, averageSeverity, roundedSeverity, averageLikelihood, roundedLikelihood, matrixCode, riskLevel?.Value ?? "Unknown");

            return (averageScore, matrixCode, riskLevel);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error calculating scoring data for hazard {HazardCode}", hazardCode);
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

    private (bool isValid, string message) ValidateCurrentStep()
    {
        return CurrentStep switch
        {
            1 => Step1.Validate(),
            2 => ValidateStep2(),
            3 => Step3.Validate(ReportedHazards, CurrentStep),
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

        var step3Result = Step3.Validate(ReportedHazards, 3); // Use step 3 for Initial validation
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

    private async Task CompleteAssessmentProcess()
    {
        if (TechRiskAssessment == null) return;

        // Apply all steps to ensure everything is saved - ENHANCED: Use async method
        await ApplyCurrentStepToAssessmentAsync();

        
        var updateCommand = new UpdateRiskAssessmentCommand(TechRiskAssessment);
        await Mediator.SendAsync(updateCommand, CancellationToken.None);

        var cmd = new UpdateReportStatusCommand(ReportId, ReportStatus.ValidationCompleted, CurrentUserService?.UserDisplayName);
        var cmdResult = await Mediator.SendAsync(cmd, CancellationToken.None);


    }

   
    private async Task ApplyCurrentStepToAssessmentAsync()
    {
        TechRiskAssessment.UpdatedDate = DateTime.UtcNow;
        TechRiskAssessment.UpdatedBy = CurrentUserService?.UserDisplayName;


        switch (CurrentStep)
        {
            case 1:
                Step1.ApplyToAssessment(TechRiskAssessment!);
                break;
            case 2:
                Step2.ApplyToAssessment(TechRiskAssessment!);
                break;
            case 3:
                await Step3.ApplyToAssessmentAsync(TechRiskAssessment!, ReportedHazards, CurrentStep);
                break;
            case 4:
                await Step4.ApplyToAssessmentAsync(TechRiskAssessment!, ReportedHazards);
                break;
            case 5:
                await Step5.ApplyToAssessmentAsync(TechRiskAssessment!, ReportedHazards);
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
                await NotificationHelper.ShowErrorAsync("A hazard with this description already exists.");
                return;
            }

            // The hazard was already created in the modal - we just need to add it to our collections
            Logger.LogInformation("Hazard {HazardCode} was successfully created in modal, adding to collections", newHazard.Code);

            // Add to collections (no database call needed here - already done in modal)
            ReportHazards.Add(newHazard);

            // Create a completely new list to force parameter change detection
            ReportedHazards = ReportHazards.ToList();

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

            await NotificationHelper.ShowSuccessAsync($"Hazard {newHazard.Code} added successfully");
            Logger.LogInformation("Successfully added hazard to collections: {HazardCode} - Total hazards: {Count}",newHazard.Code, ReportedHazards.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in AddHazard method for hazard: {Description}", newHazard.Description);
            await NotificationHelper.ShowErrorAsync("Error adding hazard");
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

                await NotificationHelper.ShowSuccessAsync($"Hazard {updatedHazard.Code} updated successfully");
                Logger.LogInformation("Successfully updated hazard via CQRS: {HazardCode}", updatedHazard.Code);
            }
            else
            {
                await NotificationHelper.ShowErrorAsync($"Failed to update hazard: {result.Error?.Message}");
                Logger.LogError("CQRS UpdateHazardCommand failed: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating hazard via CQRS");
            await NotificationHelper.ShowErrorAsync("Error updating hazard");
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
                await NotificationHelper.ShowErrorAsync("Cannot delete the initial hazard from the report");
                return;
            }

            // Use proper CQRS DeleteHazardCommand
            var deleteCommand = new DeleteHazardCommand(new HazardID(hazardToDelete.Code));
            var result = await Mediator.SendAsync(deleteCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                // Remove from local collections
                ReportHazards.RemoveAll(h => h.Code == hazardToDelete.Code);
                ReportedHazards = ReportHazards.ToList();

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

                await NotificationHelper.ShowSuccessAsync($"Hazard {hazardToDelete.Code} deleted successfully");
                Logger.LogInformation("Successfully deleted hazard: {HazardCode} - Remaining hazards: {Count}",hazardToDelete.Code, ReportedHazards.Count);
            }
            else
            {
                await NotificationHelper.ShowErrorAsync($"Failed to delete hazard: {result.Error?.Message}");
                Logger.LogError("DeleteHazardCommand failed: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting hazard");
            await NotificationHelper.ShowErrorAsync("Error deleting hazard");
        }
    }

    #endregion
}