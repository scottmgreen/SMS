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
    [Inject] private AuthenticationService AuthService { get; set; } = default!;
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
    //private readonly List<DropdownOption> StatusOptions = new()
    //{
    //    new() { Value = "Assigned", Text = "Assigned" },
    //    new() { Value = "InProgress", Text = "In Progress" },
    //    new() { Value = "OnHold", Text = "On Hold" },
    //    new() { Value = "Completed", Text = "Completed" },
    //    new() { Value = "Cancelled", Text = "Cancelled" }
    //};
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
                InvestigationEntity.DecisionType = "UNKNOWN";


                Logger.LogInformation("Successfully loaded investigation: {Code} with Status: {Status}",InvestigationEntity.Code, InvestigationEntity.Status);
            }
            else
            {
                Logger.LogError("Investigation {InvestigationId} not found: {Error}",
                    InvestigationId, investigationResult.Error?.Message);
                ShowErrorNotification($"Investigation {InvestigationId} not found. Please verify the investigation exists and try again.");
                return;
            }

            // If HazardId is provided in route but not set in entity, set it
            if (!string.IsNullOrWhiteSpace(HazardId) && string.IsNullOrWhiteSpace(InvestigationEntity.HazardCode))
            {
                InvestigationEntity.HazardCode = HazardId;
                Logger.LogInformation("Set HazardCode from route parameter: {HazardCode}", HazardId);
            }

            // Check HazardCode validity
            if (!string.IsNullOrWhiteSpace(HazardId) && !string.IsNullOrWhiteSpace(InvestigationEntity.HazardCode))
            {
                if (!InvestigationEntity.HazardCode.Equals(HazardId, StringComparison.OrdinalIgnoreCase))
                {
                    Logger.LogWarning("HazardCode mismatch. Route: {RouteHazardId}, Entity: {EntityHazardCode}",
                        HazardId, InvestigationEntity.HazardCode);
                }
            }

            // Validate that HazardCode is set
            if (string.IsNullOrWhiteSpace(InvestigationEntity.HazardCode) || InvestigationEntity.HazardCode == "HAZ-UNKNOWN")
            {
                ShowErrorNotification("Investigation has invalid or missing HazardCode. Please check the investigation setup.");
                Logger.LogError("Investigation {InvestigationId} has invalid HazardCode: {HazardCode}",
                    InvestigationId, InvestigationEntity.HazardCode ?? "NULL");
            }

            showDecisionForm = InvestigationEntity?.DecisionDate == null; // Only show form if decision hasn't been recorded yet

            Logger.LogInformation("Successfully loaded investigation: {Code} with HazardCode: {HazardCode}, Status: {Status}",
                InvestigationEntity?.Code, InvestigationEntity?.HazardCode, InvestigationEntity?.Status);

            // Load available investigators
            await LoadAvailableInvestigators();

            // Load interviews for this investigation
            await LoadInterviews();

            // Load evidence files
            await LoadEvidenceFiles();

        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading investigation data for: {InvestigationId}", InvestigationId);
            ShowErrorNotification($"Error loading investigation data: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
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
                InvestigationEntity.UpdatedBy = AuthService.CurrentUserDisplayName;
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
                        
            InvestigationEntity.DecisionMaker = AuthService.CurrentUserDisplayName;
            InvestigationEntity.Status = InvestigationStatus.FromValue(InvestigationStatusId);

            if (InvestigationEntity.Status !=InvestigationStatus.InvestigationComplete)
            {
                InvestigationEntity.DecisionType = "UNDER_REVIEW";
            }


            // Handle special workflow for "Return to Validation" decision
            if (InvestigationEntity.DecisionType == "ReturnToValidation")
            {
                await HandleReturnToValidation();
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
            var message = "Investigation has been completed and the report has been returned to the validation workflow.\n\n";// +
                         //"Next Steps:\n" +
                         //"• Investigation is now marked as Completed\n" +
                         //"• Report validation has been reset to Initial stage\n" +
                         //"• Report will appear in the Validations listing\n" +
                         //"• Report will no longer appear in Investigations listing";

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


        //try
        //{
        //    Logger.LogInformation("Resetting ReportValidation for ReportCode: {ReportCode}", reportCode);

        //    // Find the existing ReportValidation for this report
        //    var reportId = new ReportID(reportCode);
        //    var validationQuery = new GetReportValidationByReportIdQuery(reportId);
        //    var validationResult = await Mediator.SendAsync(validationQuery, CancellationToken.None);

        //    if (validationResult.IsSuccess && validationResult.Value != null)
        //    {
        //        var validation = validationResult.Value;

        //        // CRITICAL: Reset the validation to make it appear in Validation tab again
        //        validation.Status = ReportValidationStatus.ValidationNeeded;
        //        validation.Stage = "INITIAL";
        //        validation.UpdatedDate = DateTime.UtcNow;

        //        // IMPORTANT: Clear these key fields so the validation appears as needing re-validation
        //        validation.ValidationType = null;        // Clear validation type - this is key!
        //        validation.ValidationDecision = null;    // Clear validation decision - this is key!
        //        validation.ValidatedDate = null;         // Clear validated date
        //        validation.ValidatedBy = null;          // Clear who validated it

        //        // Add a comment about the reset
        //        validation.ValidationComments = $"Reset from Investigation on {DateTime.UtcNow:yyyy-MM-dd HH:mm} - Returned for re-validation due to investigation findings";

        //        Logger.LogInformation("Resetting ReportValidation {ValidationCode}: Status=InProgress, Stage=Initial, ValidationType=NULL, ValidationDecision=NULL",
        //            validation.Code);

        //        // Update the ReportValidation
        //        var updateCommand = new UpdateReportValidationCommand(validation);
        //        var updateResult = await Mediator.SendAsync(updateCommand, CancellationToken.None);

        //        if (updateResult.IsSuccess)
        //        {
        //            bool flowControl = await UpdateReportStatus(reportCode, ReportStatus.NeedsValidation);
        //            if (!flowControl)
        //            {
        //                throw new Exception($"Failed to Update Report Status during Create new Risk Assessment: {DomainErrors.ReportValidationError.CreateFailed.Message}");
        //            }

        //            Logger.LogInformation("Successfully reset ReportValidation {ValidationCode} for ReportCode: {ReportCode}", validation.Code, reportCode);
        //        }
        //        else
        //        {

        //            throw new InvalidOperationException($"Failed to reset ReportValidation: {updateResult.Error?.Message}");
        //        }
        //    }
        //    else
        //    {
        //        Logger.LogWarning("No ReportValidation found for ReportCode: {ReportCode}. Creating new validation...", reportCode);

        //        // If no existing validation found, create a new one
        //        await CreateNewReportValidation(reportCode);
        //    }
        //}
        //catch (Exception ex)
        //{
        //    Logger.LogError(ex, "Error resetting ReportValidation for ReportCode: {ReportCode}", reportCode);
        //    throw; // Re-throw to be handled by the calling method
        //}


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
                var validation = SMS_Domain.Entities.ReportValidation.Create(reportCode, AuthService.CurrentUserDisplayName);
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
        var updatestatuscmd = new UpdateReportStatusCommand(reportcode, status, AuthService.CurrentUserDisplayName);
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
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = "Error",
            Detail = message,
            Duration = 4000
        });
    }

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
    #endregion

    #region Tab Handling
    private void OnTabSelect(int index)
    {
        selectedTabIndex = index;
        StateHasChanged();
    }
    #endregion

    #region Decision Editing
    private async Task OnInterviewsChanged()
    {
        // Refresh the interviews count for the tab
        await LoadInterviews();
        StateHasChanged();
    }

    private void NavigateToListings()
    {
        Navigation.NavigateTo("/Listings/Investigations");
    }

    private string GetFileCountText()
    {
        return EvidenceFiles.Count.ToString();
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
}