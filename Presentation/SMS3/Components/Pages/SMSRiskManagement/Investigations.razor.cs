using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Domain.Errors;

using SMS3.Components.Shared.UIHelpers;

namespace SMS3.Components.Pages.SMSRiskManagement;

public partial class Investigations : ComponentBase
{
    #region Helper Classes
   
    #endregion

    #region Injected Services
    [Inject] private ICurrentUserService CurrentUserService { get; set; } = default!;
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private ILogger<Investigations> Logger { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    #endregion

    #region Parameters
    [Parameter] public string InvestigationId { get; set; } = default!;
    [Parameter] public string? HazardId { get; set; }
    #endregion

    #region State Properties
    private bool IsLoading { get; set; } = true;
    private bool IsSaving { get; set; } = false;
    private int selectedTabIndex { get; set; } = 0;
    private bool showDecisionForm { get; set; } = false;

    // Custom confirmation modal properties for Return to Validation
    private bool showReturnToValidationModal = false;
    private string returnToValidationMessage = string.Empty;

    private string InvestigationStatusId { get; set; } = string.Empty;

    public SMS_Domain.Entities.Investigation? InvestigationEntity { get; set; }
    public List<Interview> Interviews { get; set; } = new();
    public List<SMSApplicationUser> AvailableInvestigators { get; set; } = new();
    public List<HazardFile> EvidenceFiles { get; set; } = new();
    #endregion

    #region Child Component References
    private SMS3.Components.Pages.SMSRiskManagement.Components.InterviewsManager? interviewsManager;
    private SMS3.Components.Pages.SMSRiskManagement.Components.EvidenceFilesManager? evidenceFilesManager;
    #endregion

    #region Dropdown Options
   
    private List<DropdownOption> StatusOptions
    {
        get
        {
            return InvestigationStatus.GetAllValues()
                .OrderBy(dept => dept.Name)
                .Select(dept => new DropdownOption
                {
                    Text = dept.Name,
                    Value = dept.Value
                })
                .ToList();
        }
    }
    
    
    private readonly List<DropdownOption> DecisionTypeOptions = DropdownHelper.GetInvestigationDecisionOptions();
    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {

        await LoadInvestigationData();
        await LoadAvailableInvestigators();
        await LoadInterviews();
    }

    #endregion

        #region Data Loading
    private async Task LoadInvestigationData()
    {
        try
        {
            IsLoading = true;

            if (string.IsNullOrWhiteSpace(InvestigationId))
            {
                ShowErrorNotification("Investigation ID is required");
                Logger.LogError("Investigation ID is null or empty");
                Navigation.NavigateTo("/Listings/Investigations");
                return;
            }

            Logger.LogInformation("Loading investigation: {InvestigationId} with HazardId: {HazardId}", InvestigationId, HazardId);

            // Use direct query instead of loading all investigations
            // Using the string-based overload since the InvestigationID version has interface issues
            var investigationQuery = new GetInvestigationByCodeQuery(new InvestigationID(InvestigationId));
            var investigationResult = await Mediator.SendAsync(investigationQuery, CancellationToken.None);

            if (investigationResult.IsSuccess && investigationResult.Value != null)
            {

                InvestigationEntity = investigationResult.Value;
                InvestigationStatusId = InvestigationEntity.Status.Value;
                
                // Log the loaded AssignedInvestigatorId
                Logger.LogInformation("Loaded investigation with AssignedInvestigatorId: {AssignedId}", InvestigationEntity.AssignedInvestigatorId ?? "NULL");
                
                // Only set to UNKNOWN if DecisionType is null or empty
                if (string.IsNullOrEmpty(InvestigationEntity.DecisionType))
                {
                    InvestigationEntity.DecisionType = "UNKNOWN";
                }

                Logger.LogInformation("Successfully loaded investigation: {Code} with Status: {Status}",InvestigationEntity.Code, InvestigationEntity.Status);
            }
            else
            {
                Logger.LogError("Investigation {InvestigationId} not found: {Error}",InvestigationId, investigationResult.Error?.Message);
                ShowErrorNotification($"Investigation not found: {investigationResult.Error?.Message}");
                Navigation.NavigateTo("/Listings/Investigations");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading investigation data");
            ShowErrorNotification("Error loading investigation data");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadAvailableInvestigators()
    {
        try
        {
            var usersQuery = new GetUsersByApplicationGroupCodeQuery("AG-0006");

            var usersResult = await Mediator.SendAsync(usersQuery, CancellationToken.None);

            if (usersResult.IsSuccess && usersResult.Value != null)
            {
                AvailableInvestigators = usersResult.Value.ToList();
                Logger.LogInformation("Loaded {Count} available investigators", AvailableInvestigators.Count);
                
                // Log the current AssignedInvestigatorId for debugging
                Logger.LogInformation("Current AssignedInvestigatorId: {AssignedId}", InvestigationEntity?.AssignedInvestigatorId ?? "NULL");
                
                // Verify if the assigned investigator exists in the list
                if (!string.IsNullOrEmpty(InvestigationEntity?.AssignedInvestigatorId))
                {
                    var assignedInvestigator = AvailableInvestigators.FirstOrDefault(i => i.UserName == InvestigationEntity.AssignedInvestigatorId);
                    if (assignedInvestigator != null)
                    {
                        Logger.LogInformation("Found assigned investigator: {Name} ({Code})", assignedInvestigator.DisplayName, assignedInvestigator.Code);
                    }
                    else
                    {
                        Logger.LogWarning("Assigned investigator {AssignedId} not found in available investigators list", InvestigationEntity.AssignedInvestigatorId);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Could not load available investigators");
        }
    }

    private async Task LoadInterviews()
    {
        try
        {
            var interviewsQuery = new GetAllInterviewsQuery();
            var interviewsResult = await Mediator.SendAsync(interviewsQuery, CancellationToken.None);

            if (interviewsResult.IsSuccess && interviewsResult.Value != null)
            {
                Interviews = interviewsResult.Value
                    .Where(i => i.InvestigationCode.Trim() == InvestigationEntity?.Code)
                    .ToList();

                Logger.LogInformation("Loaded {Count} interviews for investigation", Interviews.Count);
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Could not load interviews");
        }
    }

    private async Task LoadEvidenceFiles()
    {
        try
        {
            if (InvestigationEntity?.HazardCode != null)
            {
                // Load ALL files for this hazard, not just those with HazardCategory = "Evidence"
                // This will include both files uploaded during initial reporting and investigation
                var filesQuery = new GetHazardFilesByHazardCodeQuery(InvestigationEntity.HazardCode, false, null);
                var filesResult = await Mediator.SendAsync(filesQuery, CancellationToken.None);

                if (filesResult.IsSuccess && filesResult.Value != null)
                {
                    EvidenceFiles = filesResult.Value
                        .Where(f => f.IsActive) // Only show active files
                        .OrderByDescending(f => f.UploadedDate)
                        .ToList();

                    Logger.LogInformation("Loaded {Count} evidence files for hazard {HazardCode}",
                        EvidenceFiles.Count, InvestigationEntity.HazardCode);
                }
                else
                {
                    EvidenceFiles = new List<HazardFile>();
                    Logger.LogWarning("No evidence files found for hazard {HazardCode}: {Error}",
                        InvestigationEntity.HazardCode, filesResult.Error?.Message);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Could not load evidence files for hazard {HazardCode}", InvestigationEntity?.HazardCode);
            EvidenceFiles = new List<HazardFile>();
        }
    }
    #endregion

    #region Investigation Actions
    private async Task SaveInvestigation()
    {
        try
        {
            if (InvestigationEntity == null) return;

            IsSaving = true;
            StateHasChanged();

            if(InvestigationEntity.Status == InvestigationStatus.InvestigationComplete)
            {
                InvestigationEntity.CompletedDate = DateTime.UtcNow;
                InvestigationEntity.UpdatedBy = CurrentUserService?.UserDisplayName;
                InvestigationEntity.UpdatedDate = DateTime.UtcNow;  
            }

            var updateCommand = new UpdateInvestigationCommand(InvestigationEntity);
            var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification("Investigation updated successfully");
                Logger.LogInformation("Investigation {Code} updated successfully", InvestigationEntity.Code);

                // Refresh the investigation data
                await LoadInvestigationData();
            }
            else
            {
                ShowErrorNotification($"Failed to update investigation: {result.Error?.Message}");
                Logger.LogError("Failed to update investigation {Code}: {Error}",
                    InvestigationEntity.Code, result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving investigation");
            ShowErrorNotification("Error saving investigation");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    private async Task CompleteInvestigation()
    {
        if (InvestigationEntity == null) return;

        try
        {
            // Validate that decision is recorded
            if (!InvestigationEntity.HasDecision)
            {
                ShowErrorNotification("Investigation decision must be recorded before completion");
                return;
            }

            // Check for incomplete interviews before allowing investigation completion
            var incompleteInterviews = await ValidateInterviewsComplete();
            if (incompleteInterviews.Any())
            {
                var incompleteCount = incompleteInterviews.Count;
                var incompleteList = string.Join(", ", incompleteInterviews.Select(i => $"{i.Code} ({i.Status.Name})"));
                
                ShowErrorNotification($"Cannot complete investigation. {incompleteCount} interview(s) are still incomplete: {incompleteList}. Please complete or close all interviews first.");
                
                // Switch to interviews tab to show the incomplete interviews
                selectedTabIndex = 1; // Assuming interviews tab is index 1
                StateHasChanged();
                return;
            }

            var confirmed = await DialogService.Confirm(
                "Are you sure you want to complete this investigation? This action cannot be undone.",
                "Complete Investigation",
                new ConfirmOptions() { OkButtonText = "Yes, Complete", CancelButtonText = "Cancel" });

            if (confirmed == true)
            {
                InvestigationEntity.Status = InvestigationStatus.InvestigationComplete;
                await SaveInvestigation();
                ShowSuccessNotification("Investigation completed successfully");

                // Navigate based on decision type
                await HandleInvestigationCompletion();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error completing investigation");
            ShowErrorNotification($"Error completing investigation: {ex.Message}");
        }
    }

    private async Task HandleInvestigationCompletion()
    {
        if (InvestigationEntity?.DecisionType == null) return;

        var nextStepMessage = "Next Step...";

        await DialogService.Alert(nextStepMessage, "Investigation Completed", new AlertOptions() { OkButtonText = "OK" });

        // Navigate based on decision
        switch (InvestigationEntity.DecisionType)
        {
            case "EscalateToRiskAssessment":
                // Navigate back to validation workflow
                if (!string.IsNullOrEmpty(InvestigationEntity.HazardCode))
                {
                    Navigation.NavigateTo($"/SMSRiskManagement/ReportValidation/{InvestigationEntity.ReportCode}");
                }
                break;
        }
    }

    
    private async Task HandleSave()
    {
        if (InvestigationEntity == null) return;

        try
        {
            InvestigationEntity.DecisionDate = DateTime.UtcNow;
            if (string.IsNullOrWhiteSpace(InvestigationEntity.DecisionType) )
            {
                ShowErrorNotification("Decision type, rationale, and decision maker are required");
                return;
            }
                        
            InvestigationEntity.DecisionMaker = CurrentUserService?.UserDisplayName;
            InvestigationEntity.Status = InvestigationStatus.FromValue(InvestigationStatusId);

            // Check if user is trying to set status to complete
            if (InvestigationEntity.Status == InvestigationStatus.InvestigationComplete)
            {
                // Validate that all interviews are completed before allowing investigation completion
                var incompleteInterviews = await ValidateInterviewsComplete();
                if (incompleteInterviews.Any())
                {
                    var incompleteCount = incompleteInterviews.Count;
                    var incompleteList = string.Join(", ", incompleteInterviews.Select(i => $"{i.Code} ({i.Status.Name})"));
                    
                    ShowErrorNotification($"Cannot set investigation status to Complete. {incompleteCount} interview(s) are still incomplete: {incompleteList}. Please complete or close all interviews first.");
                    
                    // Reset the status back to previous value
                    InvestigationStatusId = InvestigationEntity.Status.Value;
                    
                    // Switch to interviews tab to show the incomplete interviews
                    selectedTabIndex = 1; // Assuming interviews tab is index 1
                    StateHasChanged();
                    return;
                }
            }

            if (InvestigationEntity.Status != InvestigationStatus.InvestigationComplete)
            {
                InvestigationEntity.DecisionType = "UNDER_REVIEW";
            }

            // Handle special workflow for "Return to Validation" decision
            if (InvestigationEntity.DecisionType == "RETURN_TO_VALIDATION")
            {
                // Show confirmation modal instead of directly calling HandleReturnToValidation
                returnToValidationMessage = BuildReturnToValidationConfirmationMessage();
                showReturnToValidationModal = true;
                StateHasChanged();
                return; // Exit here to wait for user confirmation
            }
            else
            {
                // For other decisions, just save normally
                await SaveInvestigation();
                showDecisionForm = false;
                ShowSuccessNotification("Investigation decision recorded");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error recording investigation decision");
            ShowErrorNotification($"Error recording decision: {ex.Message}");
        }
    }

    private async Task HandleReturnToValidation()
    {
        if (InvestigationEntity == null) return;

        try
        {
            // Check for incomplete interviews before allowing investigation completion
            var incompleteInterviews = await ValidateInterviewsComplete();
            if (incompleteInterviews.Any())
            {
                var incompleteCount = incompleteInterviews.Count;
                var incompleteList = string.Join(", ", incompleteInterviews.Select(i => $"{i.Code} ({i.Status.Name})"));
                
                ShowErrorNotification($"Cannot complete investigation and return to validation. {incompleteCount} interview(s) are still incomplete: {incompleteList}. Please complete or close all interviews first.");
                
                // Switch to interviews tab to show the incomplete interviews
                selectedTabIndex = 1; // Assuming interviews tab is index 1
                StateHasChanged();
                return;
            }
            
            // Step 1: Complete the investigation (set status to Completed)
            InvestigationEntity.Status = InvestigationStatus.InvestigationComplete;

            // Step 2: Save the completed investigation
            await SaveInvestigation();

            // Step 3: Find and reset the existing ReportValidation
            if (!string.IsNullOrEmpty(InvestigationEntity.ReportCode))
            {
                await ResetReportValidation(InvestigationEntity.ReportCode);
            }
            else if (!string.IsNullOrEmpty(InvestigationEntity.HazardCode))
            {
                
                var hazardQuery = new GetAllHazardsQuery();
                var hazardResult = await Mediator.SendAsync(hazardQuery, CancellationToken.None);

                if (hazardResult.IsSuccess && hazardResult.Value != null)
                {
                    var hazard = hazardResult.Value.FirstOrDefault(h => h.Code == InvestigationEntity.HazardCode);

                    if (hazard != null && !string.IsNullOrEmpty(hazard.ReportCode))
                    {
                        
                        await ResetReportValidation(hazard.ReportCode);
                    }
                    else
                    {
                        Logger.LogError("Could not find ReportCode for Investigation {Code} with HazardCode {HazardCode}",
                            InvestigationEntity.Code, InvestigationEntity.HazardCode);
                        ShowErrorNotification("Error: Could not find associated report for validation reset. Please contact administrator.");
                        return;
                    }
                }
                else
                {
                    Logger.LogError("Could not load hazards for Investigation {Code} with HazardCode {HazardCode}",
                        InvestigationEntity.Code, InvestigationEntity.HazardCode);
                    ShowErrorNotification("Error: Could not find associated report for validation reset. Please contact administrator.");
                    return;
                }
            }
            else
            {
                Logger.LogError("Cannot reset ReportValidation: Investigation {Code} has no ReportCode or HazardCode", InvestigationEntity.Code);
                ShowErrorNotification("Warning: Investigation has no associated report code or hazard code. Manual validation reset may be required.");
            }

            showDecisionForm = false;
            ShowSuccessNotification("Investigation completed and returned to validation workflow");

            // Show completion dialog with next steps
            var message = "Investigation has been completed and the report has been returned to the validation workflow.\n\n";

            await DialogService.Alert(message, "Returned to Validation", new AlertOptions() { OkButtonText = "OK" });

            // Navigate to validations listing to show where the report went
            Navigation.NavigateTo($"/SMSRiskManagement/ReportValidation/{InvestigationEntity.ReportCode}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in Return to Validation workflow for Investigation: {Code}", InvestigationEntity.Code);
            ShowErrorNotification($"Error processing return to validation: {ex.Message}");
        }
    }
    private async Task ResetReportValidation(string reportCode)
    {
        try
        {
            Logger.LogInformation("Resetting ReportValidation for ReportCode: {ReportCode}", reportCode);
            var reportId = new ReportID(reportCode);

            
            var queryHazard = new GetHazardsByReportCodeQuery(new ReportID(reportCode));
            var hazardResult = await Mediator.SendAsync(queryHazard, CancellationToken.None);

            if (hazardResult != null) 
            {
                var hazards = hazardResult.Value;
                foreach (Hazard hazard in hazards) 
                {
                    hazard.HazardRiskLevel = RiskLevel.Unkonwn;
                    hazard.InitialAverageScore = 0; 
                    hazard.ResidualAverageScore = 0;
                    hazard.ResidualRiskMatrixCode = "TBD";
                    hazard.InitialRiskMatrixCode = "TBD";
                    var cmdHazardReset = new ResetHazardScoresCommand(hazard);
                    var hazardResetResult = await Mediator.SendAsync(queryHazard, CancellationToken.None);

                   
                }
            }

            var validationQuery = new GetReportValidationByReportIdQuery(reportId);
            var validationResult = await Mediator.SendAsync(validationQuery, CancellationToken.None);
            if (validationResult.IsSuccess && validationResult.Value != null)
            {
                var validation = validationResult.Value;
                var cmd = new ResetReportValidationCommand(new ReportValidationID(validation.Code));
                var cmdResult = await Mediator.SendAsync(cmd, CancellationToken.None);

                if (cmdResult.IsSuccess)
                {
                    bool flowControl = await UpdateReportStatus(reportCode, ReportStatus.NeedsValidation);
                    if (!flowControl)
                    {
                        throw new Exception($"Failed to Update Report Status during Create new Risk Assessment: {DomainErrors.ReportValidationError.CreateFailed.Message}");
                    }

                    Logger.LogInformation("Successfully reset ReportValidation {ValidationCode} for ReportCode: {ReportCode}", validation.Code, reportCode);
                }
                else
                {
                    throw new InvalidOperationException($"Failed to reset ReportValidation: {cmdResult.Error?.Message}");
                }
            }
            else
            {
                Logger.LogWarning("No ReportValidation found for ReportCode: {ReportCode}. Creating new validation...", reportCode);
                // If no existing validation found, create a new one
                await CreateNewReportValidation(reportCode);
            }

        }
        catch (Exception ex) 
        {
            Logger.LogError(ex, "Error resetting ReportValidation for ReportCode: {ReportCode}", reportCode);
            throw; // Re-throw to be handled by the calling method
        }
    }

    private async Task CreateNewReportValidation(string reportCode)
    {
        try
        {
            Logger.LogInformation("Creating new ReportValidation for ReportCode: {ReportCode}", reportCode);

            // Get the report details first
            var reportQuery = new GetReportByCodeQuery(new ReportID(reportCode));
            var reportResult = await Mediator.SendAsync(reportQuery, CancellationToken.None);

            if (reportResult.IsSuccess && reportResult.Value != null)
            {
                var report = reportResult.Value;

                // Create new ReportValidation using the static factory method
                var validation = SMS_Domain.Entities.ReportValidation.Create(reportCode, CurrentUserService?.UserDisplayName);
                validation.ValidationComments = $"Created from Investigation return to validation workflow on {DateTime.UtcNow:yyyy-MM-dd HH:mm}";

                var createCommand = new CreateReportValidationCommand(validation);
                var createResult = await Mediator.SendAsync(createCommand, CancellationToken.None);

                if (createResult.IsSuccess)
                {

                    bool flowControl = await UpdateReportStatus(reportCode, ReportStatus.NeedsValidation);
                    if (!flowControl)
                    {
                        throw new Exception($"Failed to Update Report Status during Create new Risk Assessment: {DomainErrors.ReportValidationError.CreateFailed.Message}");
                    }

                    Logger.LogInformation("Successfully created new ReportValidation {ValidationCode} for ReportCode: {ReportCode}",
                        createResult.Value.Code, reportCode);
                }
                else
                {
                    Logger.LogError("Failed to create new ReportValidation for ReportCode: {ReportCode}, Error: {Error}",
                        reportCode, createResult.Error?.Message);
                    throw new InvalidOperationException($"Failed to create new ReportValidation: {createResult.Error?.Message}");
                }
            }
            else
            {
                throw new InvalidOperationException($"Report {reportCode} not found, cannot create validation");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating new ReportValidation for ReportCode: {ReportCode}", reportCode);
            throw;
        }
    }
    #endregion

    private async Task<bool> UpdateReportStatus(string reportcode, ReportStatus status)
    {
        var updatestatuscmd = new UpdateReportStatusCommand(reportcode, status, CurrentUserService?.UserDisplayName);
        var getupdateResult = await Mediator.SendAsync(updatestatuscmd, CancellationToken.None);
        if (!getupdateResult.IsSuccess)
        {
            ShowErrorNotification($"Report{reportcode} Status Was not Updated");
            return false;
        }
        return true;
    }

    #region Notification Methods
    private void ShowErrorNotification(string message)
    {
        NotificationHelper.ShowError(NotificationService, message, 7000);
    }

    private void ShowSuccessNotification(string message)
    {
        NotificationHelper.ShowSuccess(NotificationService, message, 5000);
    }

    private void ShowInfoNotification(string message)
    {
        NotificationHelper.ShowInfo(NotificationService, message, 5000);
    }
    #endregion

    #region Tab Handling
    private void OnTabSelect(int index)
    {
        selectedTabIndex = index;
        StateHasChanged();
    }
    #endregion

    /// <summary>
    /// Builds a detailed confirmation message for return to validation
    /// </summary>
    private string BuildReturnToValidationConfirmationMessage()
    {
        var hazardCount = 1; // Since this is tied to a single hazard
        var reportCode = InvestigationEntity?.ReportCode ?? "Unknown";
        
        var message = $"Are you sure you want to return investigation '{InvestigationEntity?.Code}' to the validation workflow?\n\n" +
                       $"Investigation Details:\n" +
                       $"Code: {InvestigationEntity?.Code}\n" +
                       $"Hazard Code: {InvestigationEntity?.HazardCode}\n" +
                       $"Report Code: {reportCode}\n" +
                       $"Status: {InvestigationEntity?.Status?.Name ?? "Unknown"}\n" +
                       $"Assigned To: {InvestigationEntity?.AssignedInvestigatorId ?? "Unassigned"}\n\n";

        if (!string.IsNullOrEmpty(InvestigationEntity?.HazardCode))
        {
            message += "WARNING: This will affect the associated hazard and require re-validation.\n\n";
        }

        message += "This action will:\n" +
                   "✓ Complete and close this investigation\n" +
                   "✓ Reset the report validation status\n" +
                   "✓ Clear any validation history\n" +
                   "✓ Return the report to the validation workflow\n" +
                   "✓ Require re-validation of the entire report";

        return message;
    }

    /// <summary>
    /// Handle the actual return to validation confirmation from custom modal
    /// </summary>
    private async Task HandleReturnToValidationConfirmation()
    {
        try
        {
            showReturnToValidationModal = false;
            IsSaving = true;
            StateHasChanged();

            Logger.LogInformation("User confirmed return to validation for investigation {Code}", InvestigationEntity?.Code);

            // Perform the return to validation operation
            await HandleReturnToValidation();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error during return to validation for investigation {Code}", InvestigationEntity?.Code);
            ShowErrorNotification($"An unexpected error occurred: {ex.Message}");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Cancel the return to validation operation
    /// </summary>
    private void CancelReturnToValidationConfirmation()
    {
        showReturnToValidationModal = false;
        returnToValidationMessage = string.Empty;
        StateHasChanged();
        
        Logger.LogInformation("User cancelled return to validation for investigation {Code}", InvestigationEntity?.Code);
    }

    /// <summary>
    /// Gets the final warning message for display
    /// </summary>
    private string GetReturnToValidationFinalWarning()
    {
        return "THIS ACTION CANNOT BE UNDONE!";
    }

    #region Decision Editing
    private async Task OnInterviewsChanged()
    {
        // Refresh the interviews count for the tab
        await LoadInterviews();
        StateHasChanged();
        
        // Log the interview status change for debugging
        Logger.LogInformation("Interview status changed for investigation {Code}. {CompletedCount} completed, {IncompleteCount} incomplete", 
            InvestigationEntity?.Code, GetCompletedInterviewsCount(), GetIncompleteInterviewsCount());
    }

    private void EditDecision()
    {
        if (InvestigationEntity == null) return;

        // Clear the decision date to allow editing
        InvestigationEntity.DecisionDate = null;
        StateHasChanged();

        ShowSuccessNotification("Decision opened for editing. Make your changes and click 'Record Decision' to save.");
    }
    #endregion

    #region Validation Methods
    /// <summary>
    /// Validates that all interviews associated with this investigation are complete
    /// </summary>
    /// <returns>List of incomplete interviews</returns>
    private async Task<List<Interview>> ValidateInterviewsComplete()
    {
        try
        {
            // Reload interviews to get the most current status
            await LoadInterviews();

            // Filter interviews that are NOT in a completed state
            var incompleteInterviews = Interviews.Where(interview => 
                interview.Status != InterviewStatus.InterviewComplete &&
                interview.Status != InterviewStatus.UnableToConduct &&
                interview.Status != InterviewStatus.InterviewCanceled
            ).ToList();

            Logger.LogInformation("Investigation {Code}: Found {TotalInterviews} total interviews, {IncompleteCount} incomplete", 
                InvestigationEntity?.Code, Interviews.Count, incompleteInterviews.Count);

            if (incompleteInterviews.Any())
            {
                Logger.LogWarning("Investigation {Code} has incomplete interviews: {IncompleteInterviews}", 
                    InvestigationEntity?.Code, 
                    string.Join(", ", incompleteInterviews.Select(i => $"{i.Code}={i.Status.Name}")));
            }

            return incompleteInterviews;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error validating interview completeness for investigation {Code}", InvestigationEntity?.Code);
            ShowErrorNotification("Error checking interview status. Please try again.");
            return new List<Interview>(); // Return empty list to allow operation but log the error
        }
    }
    #endregion

    #region UI Helper Methods
    /// <summary>
    /// Gets the count of incomplete interviews for UI display
    /// </summary>
    /// <returns>Number of incomplete interviews</returns>
    private int GetIncompleteInterviewsCount()
    {
        return Interviews.Count(interview => 
            interview.Status != InterviewStatus.InterviewComplete &&
            interview.Status != InterviewStatus.UnableToConduct &&
            interview.Status != InterviewStatus.InterviewCanceled);
    }

    /// <summary>
    /// Gets the count of completed interviews for UI display
    /// </summary>
    /// <returns>Number of completed interviews</returns>
    private int GetCompletedInterviewsCount()
    {
        return Interviews.Count(interview => 
            interview.Status == InterviewStatus.InterviewComplete ||
            interview.Status == InterviewStatus.UnableToConduct ||
            interview.Status == InterviewStatus.InterviewCanceled);
    }

    /// <summary>
    /// Determines if the investigation can be completed based on interview status
    /// </summary>
    /// <returns>True if all interviews are complete, false otherwise</returns>
    private bool CanCompleteInvestigation()
    {
        if (InvestigationEntity == null) return false;
        
        // Must have decision recorded
        if (!InvestigationEntity.HasDecision) return false;
        
        // All interviews must be in completed state
        return GetIncompleteInterviewsCount() == 0;
    }

    /// <summary>
    /// Gets a summary message about interview completion status
    /// </summary>
    /// <returns>Status message for display</returns>
    private string GetInterviewCompletionStatus()
    {
        var total = Interviews.Count;
        var completed = GetCompletedInterviewsCount();
        var incomplete = GetIncompleteInterviewsCount();

        if (total == 0)
            return "No interviews associated with this investigation";
        
        if (incomplete == 0)
            return $"All {total} interview(s) are completed";
        
        return $"{completed} of {total} interview(s) completed, {incomplete} still pending";
    }

    /// <summary>
    /// Gets the subtitle for the page header showing completion status
    /// </summary>
    /// <returns>Subtitle with completion status</returns>
    private string GetInvestigationSubtitle()
    {
        var baseSubtitle = "Conduct comprehensive investigations into safety incidents and hazard reports";
        
        if (InvestigationEntity == null) 
            return baseSubtitle;

        var total = Interviews.Count;
        if (total == 0) 
            return baseSubtitle;

        var incomplete = GetIncompleteInterviewsCount();
        if (incomplete > 0)
            return $"{baseSubtitle} • {incomplete} of {total} interview(s) still pending completion";
        
        return $"{baseSubtitle} • All {total} interview(s) completed - Ready to close";
    }

    /// <summary>
    /// Gets the file count text for the Evidence tab
    /// </summary>
    /// <returns>File count as string</returns>
    private string GetFileCountText()
    {
        return EvidenceFiles.Count.ToString();
    }

    /// <summary>
    /// Navigate back to investigations listings
    /// </summary>
    private void NavigateToListings()
    {
        Navigation.NavigateTo("/Listings/Investigations");
    }
    #endregion
}