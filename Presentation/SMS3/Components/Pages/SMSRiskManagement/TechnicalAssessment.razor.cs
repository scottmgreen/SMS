using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using SMS_Application.Messaging.Queries;
using SMS_Application.Messaging.Commands;
using SMS_Application.Interfaces;
using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Domain.ValueObjects;
using SMS_Domain.Common;
using SMS_Shared.Common;

namespace SMS3.Components.Pages.SMSRiskManagement;

public partial class TechnicalAssessment : ComponentBase
{
    #region Parameters and Injection

    [Parameter] public string? Id { get; set; }
    [Parameter] public int StepNumber { get; set; } = 1;
    
    // Add query parameters for HazardId and ReportId
    [SupplyParameterFromQuery(Name = "hazardId")] public string? HazardId { get; set; }
    [SupplyParameterFromQuery(Name = "reportId")] public string? ReportId { get; set; }

    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<TechnicalAssessment> Logger { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;

    #endregion

    #region State Properties

    private bool IsLoading { get; set; } = true;
    private bool IsSaving { get; set; } = false;
    public int CurrentStep => StepNumber;

    #endregion

    #region Core Assessment Data

    public RiskAssessment? InitialRiskAssessment { get; set; }
    public RiskAssessment? ResidualRiskAssessment { get; set; }
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

    public string AssessmentName => InitialRiskAssessment?.Name ?? "Technical Risk Assessment";
    public string AssessmentId => InitialRiskAssessment?.Code ?? Id ?? "New";
    public string LeadAssessorName => AvailableAssessors.FirstOrDefault(a => a.Id.Value == Step1.LeadAssessor)?.DisplayName ?? Step1.LeadAssessor;
    public List<Hazard> AvailableHazards => ReportHazards;
    public List<Step4Model.PanelMemberScoreData> CompletedScores => Step4?.CompletedScores ?? new();

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
        Logger.LogInformation("TechnicalAssessment OnInitializedAsync - Id: {Id}, StepNumber: {StepNumber}, HazardId: {HazardId}, ReportId: {ReportId}",
            Id, StepNumber, HazardId, ReportId);
            
        await LoadAssessmentDataAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        Logger.LogInformation("TechnicalAssessment OnParametersSetAsync - Id: {Id}, StepNumber: {StepNumber}, HazardId: {HazardId}, ReportId: {ReportId}",
            Id, StepNumber, HazardId, ReportId);
            
        // Handle route parameter changes
        if (StepNumber < 1 || StepNumber > 5)
        {
            StepNumber = 1;
            await NavigateToStep(1);
        }
    }

    #endregion

    #region Data Loading Methods

    private async Task LoadAssessmentDataAsync()
    {
        try
        {
            IsLoading = true;
            StateHasChanged();

            Logger.LogInformation("Loading Technical Assessment - Step {StepNumber}, AssessmentId: {AssessmentId}, HazardId: {HazardId}, ReportId: {ReportId}",
                StepNumber, Id, HazardId, ReportId);

            await LoadCoreAssessmentDataAsync();
            await LoadReportHazardsAsync();
            LoadStepDataFromAssessment();
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
            await LoadAssessmentsByHazardIdAsync();
        }
        else if (!string.IsNullOrWhiteSpace(ReportId))
        {
            // Secondary method: Load by ReportId and find hazards, then assessments
            await LoadAssessmentsByReportIdAsync();
        }
        else if (!string.IsNullOrWhiteSpace(Id) && Id != "New")
        {
            // Tertiary method: Load by Assessment ID directly
            await LoadAssessmentByIdAsync();
        }
        else
        {
            throw new InvalidOperationException("Cannot load assessment: No HazardId, ReportId, or Assessment ID provided");
        }

        // Validate that we found assessments
        if (InitialRiskAssessment == null)
        {
            throw new InvalidOperationException($"No Initial Risk Assessment found. HazardId: {HazardId}, ReportId: {ReportId}, Id: {Id}");
        }

        // Update the Id parameter if it wasn't provided but we found an assessment
        if (string.IsNullOrWhiteSpace(Id) || Id == "New")
        {
            Id = InitialRiskAssessment.Code;
        }

        Logger.LogInformation("Successfully loaded assessments - Initial: {InitialCode}, Residual: {ResidualCode}", 
            InitialRiskAssessment.Code, ResidualRiskAssessment?.Code ?? "None");
    }

    private async Task LoadAssessmentsByHazardIdAsync()
    {
        Logger.LogInformation("Loading assessments for HazardId: {HazardId}", HazardId);

        var getAllAssessmentsQuery = new GetRiskAssessmentsByHazardIdQuery(new HazardID(HazardId));
        var allAssessmentsResult = await Mediator.SendAsync(getAllAssessmentsQuery, CancellationToken.None);

        if (allAssessmentsResult.IsSuccess && allAssessmentsResult.Value?.Any() == true)
        {
            var assessments = allAssessmentsResult.Value.ToList();
            
            InitialRiskAssessment = assessments.FirstOrDefault(x => x.AssessmentType == RiskAssessmentType.Initial);
            ResidualRiskAssessment = assessments.FirstOrDefault(x => x.AssessmentType == RiskAssessmentType.Residual);

            Logger.LogInformation("Found {Count} assessments for hazard {HazardId}", assessments.Count, HazardId);
        }
        else
        {
            Logger.LogWarning("No risk assessments found for HazardId: {HazardId}", HazardId);
        }
    }

    private async Task LoadAssessmentsByReportIdAsync()
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
                var assessmentsQuery = new GetRiskAssessmentsByHazardIdQuery(new HazardID(hazard.Code));
                var assessmentsResult = await Mediator.SendAsync(assessmentsQuery, CancellationToken.None);

                if (assessmentsResult.IsSuccess && assessmentsResult.Value?.Any() == true)
                {
                    var assessments = assessmentsResult.Value.ToList();
                    
                    // Take the first valid set of assessments we find
                    if (InitialRiskAssessment == null)
                    {
                        InitialRiskAssessment = assessments.FirstOrDefault(x => x.AssessmentType == RiskAssessmentType.Initial);
                        HazardId = hazard.Code; // Update HazardId for consistency
                    }
                    
                    if (ResidualRiskAssessment == null)
                    {
                        ResidualRiskAssessment = assessments.FirstOrDefault(x => x.AssessmentType == RiskAssessmentType.Residual);
                    }

                    Logger.LogInformation("Found assessments via hazard {HazardCode}", hazard.Code);
                    
                    // If we found what we need, no need to check other hazards
                    if (InitialRiskAssessment != null) break;
                }
            }
        }
        else
        {
            Logger.LogWarning("No hazards found for ReportId: {ReportId}", ReportId);
        }
    }

    private async Task LoadAssessmentByIdAsync()
    {
        Logger.LogInformation("Loading assessment directly by Id: {Id}", Id);

        try
        {
            var assessmentQuery = new GetRiskAssessmentByIdQuery(new RiskAssessmentID(Id));
            var assessmentResult = await Mediator.SendAsync(assessmentQuery, CancellationToken.None);

            if (assessmentResult.IsSuccess && assessmentResult.Value != null)
            {
                var assessment = assessmentResult.Value;
                
                if (assessment.AssessmentType == RiskAssessmentType.Initial)
                {
                    InitialRiskAssessment = assessment;
                    
                    // Try to find the corresponding residual assessment
                    if (!string.IsNullOrEmpty(assessment.HazardCode))
                    {
                        HazardId = assessment.HazardCode;
                        await LoadAssessmentsByHazardIdAsync(); // This will find both Initial and Residual
                    }
                }
                else if (assessment.AssessmentType == RiskAssessmentType.Residual)
                {
                    ResidualRiskAssessment = assessment;
                    
                    // Try to find the corresponding initial assessment
                    if (!string.IsNullOrEmpty(assessment.HazardCode))
                    {
                        HazardId = assessment.HazardCode;
                        await LoadAssessmentsByHazardIdAsync(); // This will find both Initial and Residual
                    }
                }
            }
            else
            {
                Logger.LogWarning("No risk assessment found for Id: {Id}", Id);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading assessment by ID: {Id}", Id);
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
                var reportHazardQuery = new GetHazardsByReportIdQuery(new ReportID(ReportId));
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
            if (InitialRiskAssessment?.IdentifiedHazardIds?.Any() == true)
            {
                foreach (var hazardIdString in InitialRiskAssessment.IdentifiedHazardIds)
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

            ReportHazards = allHazards;
            Logger.LogInformation("Loaded {Count} hazards for assessment", allHazards.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading report hazards");
            ReportHazards = new List<Hazard>();
        }
    }

    private void LoadStepDataFromAssessment()
    {
        if (InitialRiskAssessment == null) return;

        try
        {
            Step1.LoadFromAssessment(InitialRiskAssessment);
            Step2.LoadFromAssessment(InitialRiskAssessment);
            Step3.LoadFromAssessment(InitialRiskAssessment, ReportHazards);
            Step4.LoadFromAssessment(InitialRiskAssessment);
            Step5.LoadFromAssessment(InitialRiskAssessment);

            Logger.LogInformation("Step models loaded from assessment");
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
                AvailableAssessors = AvailableSMSUsers; // For now, assessors are SMS users
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

            Logger.LogInformation("Reference data loaded - SMS Users: {SMS}, Stakeholders: {Stakeholders}, Groups: {Groups}",
                AvailableSMSUsers.Count, AvailableStakeholders.Count, StakeholderGroups.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading reference data");
        }
    }

    #endregion

    #region Navigation Methods

    private async Task NavigateToStep(int targetStep)
    {
        if (targetStep < 1 || targetStep > 5) return;

        // Build the URL with all necessary parameters
        var baseUrl = $"/SMSRiskManagement/TechnicalAssessment/{AssessmentId}/{targetStep}";
        var queryParams = new List<string>();
        
        if (!string.IsNullOrEmpty(HazardId))
            queryParams.Add($"hazardId={Uri.EscapeDataString(HazardId)}");
        
        if (!string.IsNullOrEmpty(ReportId))
            queryParams.Add($"reportId={Uri.EscapeDataString(ReportId)}");
        
        var fullUrl = queryParams.Any() 
            ? $"{baseUrl}?{string.Join("&", queryParams)}" 
            : baseUrl;

        Logger.LogInformation("Navigating to: {Url}", fullUrl);
        Navigation.NavigateTo(fullUrl);
    }

    private async Task PreviousStep()
    {
        if (CurrentStep > 1)
        {
            await NavigateToStep(CurrentStep - 1);
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
        if (InitialRiskAssessment == null)
        {
            return (false, "Assessment not loaded");
        }

        try
        {
            // Apply current step to assessment
            ApplyCurrentStepToAssessment();

            // Save to database
            var updateCommand = new UpdateRiskAssessmentCommand(InitialRiskAssessment);
            var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                InitialRiskAssessment = result.Value; // Update with latest data
                return (true, $"Step {CurrentStep} saved successfully");
            }
            else
            {
                return (false, result.Error?.Message ?? "Save failed");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving step {Step}", CurrentStep);
            return (false, ex.Message);
        }
    }

    private void ApplyCurrentStepToAssessment()
    {
        switch (CurrentStep)
        {
            case 1:
                Step1.ApplyToAssessment(InitialRiskAssessment!);
                break;
            case 2:
                Step2.ApplyToAssessment(InitialRiskAssessment!);
                break;
            case 3:
                Step3.ApplyToAssessment(InitialRiskAssessment!);
                break;
            case 4:
                Step4.ApplyToAssessment(InitialRiskAssessment!);
                break;
            case 5:
                Step5.ApplyToAssessment(InitialRiskAssessment!);
                break;
        }
    }

    private async Task CompleteAssessmentProcess()
    {
        if (InitialRiskAssessment == null) return;

        // Apply all steps to ensure everything is saved
        Step1.ApplyToAssessment(InitialRiskAssessment);
        Step2.ApplyToAssessment(InitialRiskAssessment);
        Step3.ApplyToAssessment(InitialRiskAssessment);
        Step4.ApplyToAssessment(InitialRiskAssessment);
        Step5.ApplyToAssessment(InitialRiskAssessment);

        // Mark as completed (implement this method in RiskAssessment entity if needed)
        // InitialRiskAssessment.MarkAsComplete();

        // Save final state
        var updateCommand = new UpdateRiskAssessmentCommand(InitialRiskAssessment);
        await Mediator.SendAsync(updateCommand, CancellationToken.None);
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
            1 => "settings",
            2 => "warning",
            3 => "analytics",
            4 => "balance",
            5 => "shield",
            _ => "help"
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
            Logger.LogInformation("Adding new hazard via CQRS: {Description}", newHazard.Description);

            // Use proper CQRS CreateHazardCommand
            var createCommand = new CreateHazardCommand(newHazard);
            var result = await Mediator.SendAsync(createCommand, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                // Add to local collection
                ReportHazards.Add(result.Value);
                
                // Update Step2 model
                Step2.HazardIds.Add(result.Value.Code);
                Step2.HazardDescriptions.Add(result.Value.Description);
                Step2.HazardCategories.Add(result.Value.HazardType ?? string.Empty);

                ShowSuccessNotification($"Hazard {result.Value.Code} added successfully");
                Logger.LogInformation("Successfully created hazard via CQRS: {HazardCode}", result.Value.Code);
            }
            else
            {
                ShowErrorNotification($"Failed to add hazard: {result.Error?.Message}");
                Logger.LogError("CQRS CreateHazardCommand failed: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error adding hazard via CQRS");
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

    #endregion
}