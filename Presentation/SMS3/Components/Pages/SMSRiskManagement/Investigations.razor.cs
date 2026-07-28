
using SMS_Application.Interfaces;
using SMS_Domain.Enums;
using SMS_Domain.Errors;
using SMS_Domain.Events;

using SMS_Shared.Configuration;

using SMS3.Components.Shared.UIHelpers;
using SMS3.Configuration.Extensions;

namespace SMS3.Components.Pages.SMSRiskManagement;

public partial class Investigations : ComponentBase
{
    #region Helper Classes
   
    #endregion

    #region Injected Services
    [Inject] private ICurrentUserService _currentUserService { get; set; } = default!;
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private NavigationManager _navigation { get; set; } = default!;
    [Inject] private IBaseEventBus _eventBus { get; set; } = default!;

    [Inject] private ILogger<Investigations> _logger { get; set; } = default!;
    [Inject] private DialogService _dialogService { get; set; } = default!;
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
    private bool _showReturnToValidationModal = false;
    private string _returnToValidationMessage = string.Empty;

    private string InvestigationStatusId { get; set; } = string.Empty;

    public Investigation? InvestigationEntity { get; set; }
    public List<Interview> Interviews { get; set; } = new();
    public List<SMSApplicationUser> AvailableInvestigators { get; set; } = new();
    public List<HazardFile> EvidenceFiles { get; set; } = new();
    #endregion

    #region Child Component References
    private SMS3.Components.Pages.SMSRiskManagement.Components.InterviewsManager? _interviewsManager;
    private SMS3.Components.Pages.SMSRiskManagement.Components.EvidenceFilesManager? _evidenceFilesManager;
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
    
    
    private readonly List<DropdownOption> _decisionTypeOptions = DropdownHelper.GetInvestigationDecisionOptions();
    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {

        await LoadInvestigationData();
        await LoadAvailableInvestigators();
        await LoadInterviews();
        await LoadEvidenceFiles();
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
                await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Investigation ID is required"));
                _logger.LogError("Investigation ID is null or empty");
                _navigation.NavigateToSecure("/Listings/Investigations");
                return;
            }

            _logger.LogInformation("Loading investigation: {InvestigationId} with HazardId: {HazardId}", InvestigationId, HazardId);

            // Use direct query instead of loading all investigations
            // Using the string-based overload since the InvestigationID version has interface issues
            var investigationQuery = new GetInvestigationByCodeQuery(new InvestigationID(InvestigationId));
            var investigationResult = await _mediator.SendAsync(investigationQuery, CancellationToken.None);

            if (investigationResult.IsSuccess && investigationResult.Value is not null)
            {

                InvestigationEntity = investigationResult.Value;
                InvestigationStatusId = InvestigationEntity.Status.Value;
                
                // Log the loaded AssignedInvestigatorId
                _logger.LogInformation("Loaded investigation with AssignedInvestigatorId: {AssignedId}", InvestigationEntity.AssignedInvestigatorId ?? "NULL");
                
                // Only set to UNKNOWN if DecisionType is null or empty
                if (string.IsNullOrEmpty(InvestigationEntity.DecisionType))
                {
                    InvestigationEntity.DecisionType = "UNKNOWN";
                }

                _logger.LogInformation("Successfully loaded investigation: {Code} with Status: {Status}",InvestigationEntity.Code, InvestigationEntity.Status);
            }
            else
            {
                _logger.LogError("Investigation {InvestigationId} not found: {Error}",InvestigationId, investigationResult.Error?.Message);
                await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", $"Investigation not found: {investigationResult.Error?.Message}"));
                _navigation.NavigateToSecure("/Listings/Investigations");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading investigation data");
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Error loading investigation data"));
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

            var usersResult = await _mediator.SendAsync(usersQuery, CancellationToken.None);

            if (usersResult.IsSuccess && usersResult.Value is not null)
            {
                AvailableInvestigators = usersResult.Value.ToList();
                _logger.LogInformation("Loaded {Count} available investigators", AvailableInvestigators.Count);
                
                // Log the current AssignedInvestigatorId for debugging
                _logger.LogInformation("Current AssignedInvestigatorId: {AssignedId}", InvestigationEntity?.AssignedInvestigatorId ?? "NULL");
                
                // Verify if the assigned investigator exists in the list
                if (!string.IsNullOrEmpty(InvestigationEntity?.AssignedInvestigatorId))
                {
                    var assignedInvestigator = AvailableInvestigators.FirstOrDefault(i => i.Code == InvestigationEntity.AssignedInvestigatorId);
                    if (assignedInvestigator is not null)
                    {
                        _logger.LogInformation("Found assigned investigator: {Name} ({Code})", assignedInvestigator.DisplayName, assignedInvestigator.Code);
                    }
                    else
                    {
                        _logger.LogWarning("Assigned investigator {AssignedId} not found in available investigators list", InvestigationEntity.AssignedInvestigatorId);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load available investigators");
        }
    }

    private async Task LoadInterviews()
    {
        try
        {
            var interviewsQuery = new GetAllInterviewsQuery();
            var interviewsResult = await _mediator.SendAsync(interviewsQuery, CancellationToken.None);

            if (interviewsResult.IsSuccess && interviewsResult.Value is not null)
            {
                Interviews = interviewsResult.Value
                    .Where(i => i.InvestigationCode.Trim() == InvestigationEntity?.Code)
                    .ToList();

                _logger.LogInformation("Loaded {Count} interviews for investigation", Interviews.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load interviews");
        }
    }

    private async Task LoadEvidenceFiles()
    {
        try
        {
            if (InvestigationEntity?.HazardCode is not null)
            {
                // Load ALL files for this hazard, not just those with HazardCategory = "Evidence"
                // This will include both files uploaded during initial reporting and investigation
                var filesQuery = new GetHazardFilesByHazardCodeQuery(InvestigationEntity.HazardCode, false, null);
                var filesResult = await _mediator.SendAsync(filesQuery, CancellationToken.None);

                if (filesResult.IsSuccess && filesResult.Value is not null)
                {
                    EvidenceFiles = filesResult.Value
                        .Where(f => f.IsActive) // Only show active files
                        .OrderByDescending(f => f.UploadedDate)
                        .ToList();

                    _logger.LogInformation("Loaded {Count} evidence files for hazard {HazardCode}",
                        EvidenceFiles.Count, InvestigationEntity.HazardCode);
                }
                else
                {
                    EvidenceFiles = new List<HazardFile>();
                    _logger.LogWarning("No evidence files found for hazard {HazardCode}: {Error}",
                        InvestigationEntity.HazardCode, filesResult.Error?.Message);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load evidence files for hazard {HazardCode}", InvestigationEntity?.HazardCode);
            EvidenceFiles = new List<HazardFile>();
        }
    }
    #endregion

    #region Investigation Actions
    private async Task SaveInvestigation()
    {
        try
        {
            if (InvestigationEntity is null) return;

            IsSaving = true;
            StateHasChanged();

            if(InvestigationEntity.Status == InvestigationStatus.InvestigationComplete)
            {
                InvestigationEntity.CompletedDate = DateTime.UtcNow;
                InvestigationEntity.UpdatedBy = _currentUserService?.UserCode;
                InvestigationEntity.UpdatedDate = DateTime.UtcNow;  
            }

            var updateCommand = new UpdateInvestigationCommand(InvestigationEntity);
            var result = await _mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                await _eventBus.PublishUIEventAsync(UINotificationEvent.Success("Success", "Investigation updated successfully"));
                _logger.LogInformation("Investigation {Code} updated successfully", InvestigationEntity.Code);

                // Refresh the investigation data
                await LoadInvestigationData();
            }
            else
            {
                await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", $"Failed to update investigation: {result.Error?.Message}"));
                _logger.LogError("Failed to update investigation {Code}: {Error}",
                    InvestigationEntity.Code, result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving investigation");
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Error saving investigation"));
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    private async Task CompleteInvestigation()
    {
        if (InvestigationEntity is null) return;

        try
        {
            // Validate that decision is recorded
            if (!InvestigationEntity.HasDecision)
            {
                await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Investigation decision must be recorded before completion"));
                return;
            }

            // Check for incomplete interviews before allowing investigation completion
            var incompleteInterviews = await ValidateInterviewsComplete();
            if (incompleteInterviews.Any())
            {
                var incompleteCount = incompleteInterviews.Count;
                var incompleteList = string.Join(", ", incompleteInterviews.Select(i => $"{i.Code} ({i.Status.Name})"));
                
                await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", $"Cannot complete investigation. {incompleteCount} interview(s) are still incomplete: {incompleteList}. Please complete or close all interviews first."));
                
                // Switch to interviews tab to show the incomplete interviews
                selectedTabIndex = 1; // Assuming interviews tab is index 1
                StateHasChanged();
                return;
            }

            var confirmed = await _dialogService.Confirm(
                "Are you sure you want to complete this investigation? This action cannot be undone.",
                "Complete Investigation",
                new ConfirmOptions() { OkButtonText = "Yes, Complete", CancelButtonText = "Cancel" });

            if (confirmed == true)
            {
                InvestigationEntity.Status = InvestigationStatus.InvestigationComplete;
                await SaveInvestigation();
                await _eventBus.PublishUIEventAsync(UINotificationEvent.Success("Success", "Investigation completed successfully"));

                // Navigate based on decision type
                await HandleInvestigationCompletion();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing investigation");
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", $"Error completing investigation: {ex.Message}"));
        }
    }

    private async Task HandleInvestigationCompletion()
    {
        if (InvestigationEntity?.DecisionType is null) return;

        var nextStepMessage = "Next Step...";

        await _dialogService.Alert(nextStepMessage, "Investigation Completed", new AlertOptions() { OkButtonText = "OK" });

        // Navigate based on decision
        switch (InvestigationEntity.DecisionType)
        {
            case "EscalateToRiskAssessment":
                // Navigate back to validation workflow
                if (!string.IsNullOrEmpty(InvestigationEntity.HazardCode))
                {
                    // ?? SECURE NAVIGATION - Navigate to Report Validation with encrypted URL
                    _navigation.NavigateToSecure($"/SMSRiskManagement/ReportValidation/{InvestigationEntity.ReportCode}");
                }
                break;
        }
    }

    
    private async Task HandleSave()
    {
        if (InvestigationEntity is null) return;

        try
        {
            InvestigationEntity.DecisionDate = DateTime.UtcNow;
            if (string.IsNullOrWhiteSpace(InvestigationEntity.DecisionType) )
            {
                await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Decision type, rationale, and decision maker are required"));
                return;
            }
                        
            InvestigationEntity.DecisionMaker = _currentUserService.UserCode;
            InvestigationEntity.Status = InvestigationStatus.FromValue(InvestigationStatusId) ?? InvestigationStatus.StatusUnknown;

            // Check if user is trying to set status to complete
            if (InvestigationEntity.Status == InvestigationStatus.InvestigationComplete)
            {
                // Validate that all interviews are completed before allowing investigation completion
                var incompleteInterviews = await ValidateInterviewsComplete();
                if (incompleteInterviews.Any())
                {
                    var incompleteCount = incompleteInterviews.Count;
                    var incompleteList = string.Join(", ", incompleteInterviews.Select(i => $"{i.Code} ({i.Status.Name})"));
                    
                    await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", $"Cannot set investigation status to Complete. {incompleteCount} interview(s) are still incomplete: {incompleteList}. Please complete or close all interviews first."));
                    
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
                _returnToValidationMessage = BuildReturnToValidationConfirmationMessage();
                _showReturnToValidationModal = true;
                StateHasChanged();
                return; // Exit here to wait for user confirmation
            }
            else
            {
                // For other decisions, just save normally
                await SaveInvestigation();
                showDecisionForm = false;
                await _eventBus.PublishUIEventAsync(UINotificationEvent.Success("Success", "Investigation decision recorded"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording investigation decision");
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", $"Error recording decision: {ex.Message}"));
        }
    }

    private async Task HandleReturnToValidation()
    {
        if (InvestigationEntity is null) return;

        try
        {
            // Check for incomplete interviews before allowing investigation completion
            var incompleteInterviews = await ValidateInterviewsComplete();
            if (incompleteInterviews.Any())
            {
                var incompleteCount = incompleteInterviews.Count;
                var incompleteList = string.Join(", ", incompleteInterviews.Select(i => $"{i.Code} ({i.Status.Name})"));
                
                await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", $"Cannot complete investigation and return to validation. {incompleteCount} interview(s) are still incomplete: {incompleteList}. Please complete or close all interviews first."));
                
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
                var hazardResult = await _mediator.SendAsync(hazardQuery, CancellationToken.None);

                if (hazardResult.IsSuccess && hazardResult.Value is not null)
                {
                    var hazard = hazardResult.Value.FirstOrDefault(h => h.Code == InvestigationEntity.HazardCode);

                    if (hazard is not null && !string.IsNullOrEmpty(hazard.ReportCode))
                    {
                        
                        await ResetReportValidation(hazard.ReportCode);
                    }
                    else
                    {
                        _logger.LogError("Could not find ReportCode for Investigation {Code} with HazardCode {HazardCode}",
                            InvestigationEntity.Code, InvestigationEntity.HazardCode);
                        await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Error: Could not find associated report for validation reset. Please contact administrator."));
                        return;
                    }
                }
                else
                {
                    _logger.LogError("Could not load hazards for Investigation {Code} with HazardCode {HazardCode}",
                        InvestigationEntity.Code, InvestigationEntity.HazardCode);
                    await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Error: Could not find associated report for validation reset. Please contact administrator."));
                    return;
                }
            }
            else
            {
                _logger.LogError("Cannot reset ReportValidation: Investigation {Code} has no ReportCode or HazardCode", InvestigationEntity.Code);
                await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Warning: Investigation has no associated report code or hazard code. Manual validation reset may be required."));
            }

            showDecisionForm = false;
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Success("Success", "Investigation completed and returned to validation workflow"));

            // Show completion dialog with next steps
            var message = "Investigation has been completed and the report has been returned to the validation workflow.\n\n";

            await _dialogService.Alert(message, "Returned to Validation", new AlertOptions() { OkButtonText = "OK" });

            // Navigate to validations listing to show where the report went
            // ?? SECURE NAVIGATION - Navigate to Report Validation with encrypted URL
            _navigation.NavigateToSecure($"/SMSRiskManagement/ReportValidation/{InvestigationEntity.ReportCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Return to Validation workflow for Investigation: {Code}", InvestigationEntity.Code);
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", $"Error processing return to validation: {ex.Message}"));
        }
    }
    private async Task ResetReportValidation(string reportCode)
    {
        try
        {
            _logger.LogInformation("Resetting ReportValidation for ReportCode: {ReportCode}", reportCode);
            var reportId = new ReportID(reportCode);

            
            var queryHazard = new GetHazardsByReportCodeQuery(new ReportID(reportCode));
            var hazardResult = await _mediator.SendAsync(queryHazard, CancellationToken.None);

            if (hazardResult.IsSuccess && hazardResult.Value is not null) 
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
                    var hazardResetResult = await _mediator.SendAsync(cmdHazardReset, CancellationToken.None);

                    if (!hazardResetResult.IsSuccess)
                    {
                        throw new InvalidOperationException($"Failed to reset hazard scores for {hazard.Code}: {hazardResetResult.Error?.Message}");
                    }

                   
                }
            }

            var validationQuery = new GetReportValidationByReportIdQuery(reportId);
            var validationResult = await _mediator.SendAsync(validationQuery, CancellationToken.None);
            if (validationResult.IsSuccess && validationResult.Value is not null)
            {
                var validation = validationResult.Value;
                var cmd = new ResetReportValidationCommand(new ReportValidationID(validation.Code));
                var cmdResult = await _mediator.SendAsync(cmd, CancellationToken.None);

                if (cmdResult.IsSuccess)
                {
                    bool flowControl = await UpdateReportStatus(reportCode, ReportStatus.NeedsValidation);
                    if (!flowControl)
                    {
                        throw new Exception($"Failed to Update Report Status during Create new Risk Assessment: {DomainErrors.ReportValidationError.CreateFailed.Message}");
                    }

                    _logger.LogInformation("Successfully reset ReportValidation {ValidationCode} for ReportCode: {ReportCode}", validation.Code, reportCode);
                }
                else
                {
                    throw new InvalidOperationException($"Failed to reset ReportValidation: {cmdResult.Error?.Message}");
                }
            }
            else
            {
                _logger.LogWarning("No ReportValidation found for ReportCode: {ReportCode}. Creating new validation...", reportCode);
                // If no existing validation found, create a new one
                await CreateNewReportValidation(reportCode);
            }

        }
        catch (Exception ex) 
        {
            _logger.LogError(ex, "Error resetting ReportValidation for ReportCode: {ReportCode}", reportCode);
            throw; // Re-throw to be handled by the calling method
        }
    }

    private async Task CreateNewReportValidation(string reportCode)
    {
        try
        {
            _logger.LogInformation("Creating new ReportValidation for ReportCode: {ReportCode}", reportCode);

            // Get the report details first
            var reportQuery = new GetReportByCodeQuery(new ReportID(reportCode));
            var reportResult = await _mediator.SendAsync(reportQuery, CancellationToken.None);

            if (reportResult.IsSuccess && reportResult.Value is not null)
            {
                var report = reportResult.Value;

                // Create new ReportValidation using the static factory method
                var validation = SMS_Domain.Entities.ReportValidation.Create(reportCode, _currentUserService.UserCode);
                validation.ValidationComments = $"Created from Investigation return to validation workflow on {DateTime.UtcNow:yyyy-MM-dd HH:mm}";

                var createCommand = new CreateReportValidationCommand(validation);
                var createResult = await _mediator.SendAsync(createCommand, CancellationToken.None);

                if (createResult.IsSuccess)
                {

                    bool flowControl = await UpdateReportStatus(reportCode, ReportStatus.NeedsValidation);
                    if (!flowControl)
                    {
                        throw new Exception($"Failed to Update Report Status during Create new Risk Assessment: {DomainErrors.ReportValidationError.CreateFailed.Message}");
                    }

                    _logger.LogInformation("Successfully created new ReportValidation {ValidationCode} for ReportCode: {ReportCode}",
                        createResult.Value.Code, reportCode);
                }
                else
                {
                    _logger.LogError("Failed to create new ReportValidation for ReportCode: {ReportCode}, Error: {Error}",
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
            _logger.LogError(ex, "Error creating new ReportValidation for ReportCode: {ReportCode}", reportCode);
            throw;
        }
    }
    #endregion

    private async Task<bool> UpdateReportStatus(string reportcode, ReportStatus status)
    {
        var updatestatuscmd = new UpdateReportStatusCommand(reportcode, status, _currentUserService.UserCode);
        var getupdateResult = await _mediator.SendAsync(updatestatuscmd, CancellationToken.None);
        if (!getupdateResult.IsSuccess)
        {
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", $"Report{reportcode} Status Was not Updated"));
            return false;
        }
        return true;
    }

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
                   "Complete and close this investigation\n" +
                   "Reset the report validation status\n" +
                   "Clear any validation history\n" +
                   "Return the report to the validation workflow\n" +
                   "Require re-validation of the entire report";

        return message;
    }

    /// <summary>
    /// Handle the actual return to validation confirmation from custom modal
    /// </summary>
    private async Task HandleReturnToValidationConfirmation()
    {
        try
        {
            _showReturnToValidationModal = false;
            IsSaving = true;
            StateHasChanged();

            _logger.LogInformation("User confirmed return to validation for investigation {Code}", InvestigationEntity?.Code);

            // Perform the return to validation operation
            await HandleReturnToValidation();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during return to validation for investigation {Code}", InvestigationEntity?.Code);
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", $"An unexpected error occurred: {ex.Message}"));
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
            _showReturnToValidationModal = false;
            _returnToValidationMessage = string.Empty;
        StateHasChanged();
        
        _logger.LogInformation("User cancelled return to validation for investigation {Code}", InvestigationEntity?.Code);
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
        _logger.LogInformation("Interview status changed for investigation {Code}. {CompletedCount} completed, {IncompleteCount} incomplete", 
            InvestigationEntity?.Code, GetCompletedInterviewsCount(), GetIncompleteInterviewsCount());
    }

    private async Task EditDecision()
    {
        if (InvestigationEntity is null) return;

        // Clear the decision date to allow editing
        InvestigationEntity.DecisionDate = null;
        StateHasChanged();

        await _eventBus.PublishUIEventAsync(UINotificationEvent.Success("Success", "Decision opened for editing. Make your changes and click 'Record Decision' to save."));
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

            _logger.LogInformation("Investigation {Code}: Found {TotalInterviews} total interviews, {IncompleteCount} incomplete", 
                InvestigationEntity?.Code, Interviews.Count, incompleteInterviews.Count);

            if (incompleteInterviews.Any())
            {
                _logger.LogWarning("Investigation {Code} has incomplete interviews: {IncompleteInterviews}", 
                    InvestigationEntity?.Code, 
                    string.Join(", ", incompleteInterviews.Select(i => $"{i.Code}={i.Status.Name}")));
            }

            return incompleteInterviews;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating interview completeness for investigation {Code}", InvestigationEntity?.Code);
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Error checking interview status. Please try again."));
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
        if (InvestigationEntity is null) return false;
        
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
        
        if (InvestigationEntity is null) 
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
        // ?? SECURE NAVIGATION - Navigate to Investigations listing with encrypted URL
        _navigation.NavigateToSecure("/Listings/Investigations");
    }
    #endregion
}
