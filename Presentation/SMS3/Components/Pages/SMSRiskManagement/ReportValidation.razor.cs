
using SMS_Domain.Enums;
using SMS_Domain.Errors;

using SMS_Shared.Configuration;

using SMS3.Components.Shared.UIHelpers;
using SMS3.Configuration.Extensions;

namespace SMS3.Components.Pages.SMSRiskManagement;

public partial class ReportValidation : ComponentBase
{
    [Parameter] public string ReportId { get; set; } = "";

    [Inject] private ICurrentUserService CurrentUserService { get; set; } = default!;
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<ReportValidation> Logger { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    
    [Inject] private INotificationHelper  NotificationHelper { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;

    // Form Data Properties - Using Smart Enum
    private ValidationDecision? SelectedValidationDecision { get; set; }
    private string ValidationComments { get; set; } = "";
    private RiskAssessmentCategory ValidationType { get; set; } = RiskAssessmentCategory.Technical;
    private string ValidatedBy { get; set; } = "";

    private string LeadAssessor { get; set; } = "";

    private string LeadInvestigator { get; set; } = "";

    // Helper property for string-based UI binding - renamed to avoid conflicts
    private string ValidationDecisionValue
    {
        get => SelectedValidationDecision?.Value ?? "";
        set => SelectedValidationDecision = string.IsNullOrWhiteSpace(value) ? null : SMS_Domain.Enums.ValidationDecision.FromValue(value);
    }

    // Display Properties
    private bool IsLoading = true;
    private bool IsRiskRegistryCheckboxDisabled => SelectedValidationDecision != ValidationDecision.SmsRisk;
    private Report? ReportDetails { get; set; } = default!;
    private Hazard? ReportHazard { get; set; } = default!;

    private bool RiskRegistryOnly { get; set; }
    private SMS_Domain.Entities.ReportValidation? ExistingValidation { get; set; }
    private List<SMSApplicationUser> AvailableAssessors { get; set; } = new();

    private List<SMSApplicationUser> AvailableInvestigators { get; set; } = new();

    // State Properties
    private bool IsUpdate => ExistingValidation != null;
    private string ValidationCode => ExistingValidation?.Code ?? "New";
    //private string CurrentStatus { get; set;}= string.Empty; // ExistingValidation?.Status ?? "New";
    private bool IsProcessing { get; set; } = false;

    

    private List<DropdownOption> LeadAssessorOptions { get; set; } = new();

    private List<DropdownOption> LeadInvestigatorOptions { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        await LoadDataAsync();
        IsLoading = false;
    }

    #region Data Loading

    private async Task LoadDataAsync()
    {
        try
        {
            Logger.LogInformation("Loading SMS report validation for report: {ReportId}", ReportId);

            if (string.IsNullOrWhiteSpace(ReportId))
            {
                await NotificationHelper.ShowErrorAsync("Report ID is required for SMS report validation");
                Navigation.NavigateToSecure("/SMSRiskManagement/ReportProcessing");
                return;
            }

            var reportCode = new ReportID(ReportId);

            // Load report details
            var reportResult = await Mediator.SendAsync(new GetReportByCodeQuery(reportCode), CancellationToken.None);
            if (reportResult.IsSuccess)
            {
                ReportDetails = reportResult.Value;

                // Load associated hazard
                var hazardResult = await Mediator.SendAsync(new GetHazardsByReportCodeQuery(reportCode), CancellationToken.None);
                if (hazardResult.IsSuccess)
                {
                    ReportHazard = hazardResult.Value?.FirstOrDefault(h => h.ReportCode.Trim() == reportCode.Value.Trim());
                }
            }
            

            // Check for existing validation
            var existingValidationQuery = new GetReportValidationByReportIdQuery(reportCode);
            var validationResult = await Mediator.SendAsync(existingValidationQuery, CancellationToken.None);

            if (validationResult.IsSuccess)
            {
                ExistingValidation = validationResult.Value;

                // Populate form fields from existing validation using Smart Enum
                if (!string.IsNullOrWhiteSpace(ExistingValidation.ValidationDecision))
                {
                    if (SMS_Domain.Enums.ValidationDecision.TryFromValue(ExistingValidation.ValidationDecision, out var decision))
                    {
                        SelectedValidationDecision = decision;
                    }
                }
                ValidationComments = ExistingValidation.ValidationComments ?? "";
                ValidationType = RiskAssessmentCategory.Technical ;
                ValidatedBy = ExistingValidation.ValidatedBy ?? "";

                Logger.LogInformation("Found existing ReportValidation for report {ReportId} - Decision: {Decision}",
                    ReportId, SelectedValidationDecision?.Name ?? "None");
            }
            else
            {
                Logger.LogInformation("No existing ReportValidation found for report: {ReportId}", ReportId);

                // Set defaults for new validation
                SelectedValidationDecision = null;
                ValidatedBy = CurrentUserService?.UserDisplayName;
                ValidationType = RiskAssessmentCategory.Technical;
                ValidationComments = "";
            }

            // Load available assessors
            await LoadAvailableAssessorsAsync();
            await LoadAvailableInvestigatorsAsync();

            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading report for SMS validation: {ReportId}", ReportId);
            await NotificationHelper.ShowErrorAsync("An error occurred while loading the report for validation");
            Navigation.NavigateToSecure("/SMSRiskManagement/ReportProcessing");
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

                // Build dropdown options
                LeadAssessorOptions = new List<DropdownOption>
                {
                    new DropdownOption { Value = "", Text = "" }
                };

                foreach (var assessor in AvailableAssessors)
                {
                    LeadAssessorOptions.Add(new DropdownOption
                    {
                        Value = assessor.UserName.Value,
                        Text = $"{assessor.DisplayName}" // ({assessor.UserName.Value}) - {assessor.UserRole}" 
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Could not load available assessors");
            LeadAssessorOptions = new List<DropdownOption>
            {
                new DropdownOption { Value = "", Text = "" }
            };
        }
    }

    private async Task LoadAvailableInvestigatorsAsync()
    {
        try
        {
            var usersQuery = new GetUsersByApplicationGroupCodeQuery("AG-0006");
            var usersResult = await Mediator.SendAsync(usersQuery, CancellationToken.None);
            if (usersResult.IsSuccess)
            {
                AvailableInvestigators = usersResult.Value?.ToList() ?? new List<SMSApplicationUser>();

                // Build dropdown options
                LeadInvestigatorOptions = new List<DropdownOption>
                {
                    new DropdownOption { Value = "", Text = "" }
                };

                foreach (var assessor in AvailableInvestigators)
                {
                    LeadInvestigatorOptions.Add(new DropdownOption
                    {
                        Value = assessor.UserName.Value,
                        Text = $"{assessor.DisplayName}" // ({assessor.UserName.Value}) - {assessor.UserRole}" 
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Could not load available assessors");
            LeadInvestigatorOptions = new List<DropdownOption>
            {
                new DropdownOption { Value = "", Text = "" }
            };
        }
    }


    #endregion

    #region Form Handling

    private void SetValidationDecision(string decision)
    {
        if (SMS_Domain.Enums.ValidationDecision.TryFromValue(decision, out var validationDecision))
        {
            SelectedValidationDecision = validationDecision;
            if (SelectedValidationDecision != ValidationDecision.SmsRisk)
            {
                this.RiskRegistryOnly = false;
                // The checkbox will automatically be disabled due to the Disabled binding
            }
            StateHasChanged();
        }
    }

    #endregion

    #region Form Submission Methods

    /// <summary>
    /// Handles the form submission based on the selected validation decision
    /// </summary>
    private async Task HandleSubmit()
    {
        if (IsProcessing) return; // Prevent double-click

        try
        {
            IsProcessing = true;
            StateHasChanged();

            Logger.LogInformation("HandleSubmit called for ReportId: {ReportId}, Decision: {Decision}", ReportId, SelectedValidationDecision?.Value);

            // Manual validation
            if (SelectedValidationDecision == null)
            {
                await NotificationHelper.ShowErrorAsync("Please select a validation decision");
                return;
            }

            if (string.IsNullOrWhiteSpace(ValidationComments))
            {
                await NotificationHelper.ShowErrorAsync("Validation comments are required");
                return;
            }

            // Create validation record first
            await CreateValidationRecord();

            //if (this.RiskRegistryOnly)
            //{
            //    await NavigateToRiskRegistry();
            //    return;
            //}
                                   
            var navigationTask = SelectedValidationDecision switch
            {
                _ when SelectedValidationDecision == ValidationDecision.SmsRisk && this.RiskRegistryOnly => NavigateToRiskRegistry(),
                _ when SelectedValidationDecision == ValidationDecision.SmsRisk => NavigateToRiskAssessment(),
                _ when SelectedValidationDecision == ValidationDecision.NeedsInvestigation => NavigateToInvestigation(),
                _ when SelectedValidationDecision == ValidationDecision.NotSmsRisk => HandleNotSmsRisk(),
                _ => throw new InvalidOperationException($"Unhandled validation decision: {SelectedValidationDecision.Name}")
            };
            await navigationTask;


        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in HandleSubmit for ReportId: {ReportId}", ReportId);
            await NotificationHelper.ShowErrorAsync("Error processing validation. Please try again.");
        }
        finally
        {
            IsProcessing = false;
            StateHasChanged();
        }
    }
    private async Task<bool> UpdateReportStatus(string reportId, ReportStatus status)
    {
        var getReportQuery = new GetReportByCodeQuery(new ReportID(reportId));
        var getReportQueryResult = await Mediator.SendAsync(getReportQuery, CancellationToken.None);

        var cmd = new UpdateReportStatusCommand(reportId, status, CurrentUserService?.UserDisplayName);
        var cmdResult = await Mediator.SendAsync(cmd, CancellationToken.None);
        if (!cmdResult.IsSuccess)
        {
            await NotificationHelper.ShowErrorAsync($"Report{reportId} Status Was not Updated");
            return false;
        }
        return true;
    }
    /// <summary>
    /// Smart validation record creation - reuses existing validation if available, creates new if needed
    /// </summary>
    private async Task CreateValidationRecord()
    {
        try
        {
            Logger.LogInformation("Smart validation record processing for ReportId: {ReportId}, HasExisting: {HasExisting}",ReportId, ExistingValidation != null);

            if (ExistingValidation != null)
            {
                // ? UPDATE EXISTING VALIDATION
                Logger.LogInformation("Updating existing ReportValidation: {ValidationCode}", ExistingValidation.Code);

                // Update the existing validation with new values
                ExistingValidation.ValidationDecision = ValidationDecisionValue;
                ExistingValidation.ValidationComments = ValidationComments;
                ExistingValidation.ValidationType = RiskAssessmentCategory.Technical;
                ExistingValidation.ValidatedBy = ValidatedBy;
                ExistingValidation.Status = ReportValidationStatus.Revised;
                ExistingValidation.Stage = "COMPLETE";
                ExistingValidation.ValidatedDate = DateTime.UtcNow;

                ExistingValidation.UpdatedBy = CurrentUserService?.UserDisplayName; 
                ExistingValidation.UpdatedDate = DateTime.UtcNow;

                var updateCommand = new UpdateReportValidationCommand(ExistingValidation);
                var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

                if (!result.IsSuccess)
                {
                    throw new Exception($"Failed to update existing validation: {result.Error?.Message ?? DomainErrors.ReportValidationError.UpdateFailed.Message}");
                }
                bool flowControl = await UpdateReportStatus(ReportId, ReportStatus.ValidationRevised);
                if (!flowControl)
                {
                    throw new Exception($"Failed to Update Report Status during Update Validation: {result.Error?.Message ?? DomainErrors.ReportValidationError.UpdateFailed.Message}");
                }

                Logger.LogInformation("Successfully updated existing ReportValidation: {ValidationCode}", ExistingValidation.Code);
                await NotificationHelper.ShowSuccessAsync($"Validation updated successfully. Decision: {SelectedValidationDecision?.Name}");
            }
            else
            {
                // ? CREATE NEW VALIDATION (only if none exists)
                Logger.LogInformation("Creating new ReportValidation for ReportId: {ReportId}", ReportId);

                var validationId = new ReportValidationID($"RV-0000");
                var validation = new SMS_Domain.Entities.ReportValidation(validationId)
                {
                    Code = validationId.Value,
                    ReportCode = ReportId,
                    ValidatedBy = ValidatedBy,
                    ValidationDecision = ValidationDecisionValue,
                    ValidationComments = ValidationComments,
                    ValidationType = ValidationType ?? "STANDARD",
                    Status = ReportValidationStatus.ValidationComplete,
                    Stage = "NEW",
                    ValidatedDate = DateTime.UtcNow,
                    CreatedBy = CurrentUserService?.UserDisplayName,
                    CreatedDate = DateTime.UtcNow
                };

                var createCommand = new CreateReportValidationCommand(validation);
                var result = await Mediator.SendAsync(createCommand, CancellationToken.None);

                if (!result.IsSuccess)
                {
                    throw new Exception($"Failed to create new validation: {result.Error?.Message ?? DomainErrors.ReportValidationError.CreateFailed.Message}");
                }
                bool flowControl = await UpdateReportStatus(ReportId, ReportStatus.ValidationCompleted);
                if (!flowControl)
                {
                    throw new Exception($"Failed to Update Report Status during Create new validation: {result.Error?.Message ?? DomainErrors.ReportValidationError.CreateFailed.Message}");
                }
                Logger.LogInformation("Successfully created new ReportValidation: {ValidationCode}", result.Value.Code);
                await NotificationHelper.ShowSuccessAsync($"Validation recorded successfully. Decision: {SelectedValidationDecision?.Name}");
            }

            

        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in smart validation record processing for ReportId: {ReportId}", ReportId);
            throw; // Re-throw to be handled by HandleSubmit
        }
    }


    /// <summary>
    /// Navigate to Risk Assessment with optimized flow control
    /// </summary>
    private async Task NavigateToRiskAssessment()
    {
        try
        {
            // Step 1: Check if user wants to create Airport Shared Dataset
            bool createDataset = await ShowAirportDatasetDialog();
            
            if (createDataset)
            {
                await NavigateToDatasetCreation();
                return;
            }

            // Step 2: Handle existing or create new risk assessment
            await HandleRiskAssessmentNavigation();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in NavigateToRiskAssessment for Report: {ReportId}", ReportId);
            await NotificationHelper.ShowErrorAsync("Error navigating to risk assessment. Please try again.");
        }
    }

    /// <summary>
    /// Navigate to Risk Registry with optimized flow control
    /// </summary>
    private async Task NavigateToRiskRegistry()
    {
        try
        {
            // Step 1: Check if user wants to create Airport Shared Dataset
            bool createDataset = await ShowAirportDatasetDialog();
            
            if (createDataset)
            {
                await NavigateToDatasetCreation();
                return;
            }

            // Step 2: Navigate directly to Risk Registry
            Logger.LogInformation("User skipped Airport Shared Dataset creation, navigating to Risk Registry for Report: {ReportId}", ReportId);
            
            string navigationUrl = "/SMSAssurance/RiskRegistry";
            await DelayAndNavigate(navigationUrl);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in NavigateToRiskRegistry for Report: {ReportId}", ReportId);
            await NotificationHelper.ShowErrorAsync("Error navigating to risk registry. Please try again.");
        }
    }




    /// <summary>
    /// Navigate to Investigation (create investigation first if needed)
    /// </summary>
    private async Task NavigateToInvestigation()
    {
        try
        {
            if (ReportHazard == null)
            {
                await NotificationHelper.ShowErrorAsync("Cannot create investigation - hazard information not found");
                return;
            }

            if (string.IsNullOrWhiteSpace(ReportHazard.Code))
            {
                await NotificationHelper.ShowErrorAsync("Cannot create investigation - invalid hazard code");
                return;
            }

            // Check for existing investigation first
            var existingInvestigationsQuery = new GetAllInvestigationsQuery();
            var existingResult = await Mediator.SendAsync(existingInvestigationsQuery, CancellationToken.None);

            Investigation? existingInvestigation = null;
            if (existingResult.IsSuccess && existingResult.Value != null)
            {
                existingInvestigation = existingResult.Value.FirstOrDefault(inv =>
                    !string.IsNullOrWhiteSpace(inv.HazardCode) && inv.HazardCode.Equals(ReportHazard.Code, StringComparison.OrdinalIgnoreCase) ||
                    !string.IsNullOrWhiteSpace(inv.ReportCode) && inv.ReportCode.Equals(ReportId, StringComparison.OrdinalIgnoreCase));
            }

            if (existingInvestigation != null)
            {
                // Navigate to existing investigation
                await NotificationHelper.ShowSuccessAsync($"Loading existing investigation {existingInvestigation.Code}");
                var navigationUrl = $"/SMSRiskManagement/Investigations/{existingInvestigation.Code}/{ReportHazard.Code}";
                Logger.LogInformation("Navigating to existing investigation: {Url}", navigationUrl);
                bool flowControl = await UpdateReportStatus(ReportId, ReportStatus.UnderInvestigation);
                if (!flowControl)
                {
                    throw new Exception($"Failed to Update Report Status during exsiting investigation: {DomainErrors.ReportValidationError.CreateFailed.Message}");
                }
                await Task.Delay(1500);
                // ?? SECURE NAVIGATION - Navigate to existing investigation
                Navigation.NavigateToSecure(navigationUrl);
            }
            else
            {
                // Create new investigation
                var investigationCode = $"IN-0000";
                var investigationId = new InvestigationID(investigationCode);
                Investigation investigation = new Investigation(investigationId);
                investigation.HazardCode = ReportHazard.Code;
                investigation.Status = InvestigationStatus.InvestigatorAssigned;
                investigation.CreatedBy = CurrentUserService?.UserDisplayName;
                investigation.ReportCode = ReportId;
                investigation.AssignedInvestigatorId = LeadInvestigator;
                investigation.InvestigationObjectives = $"Investigation required based on validation decision for hazard {ReportHazard.Code}";
                investigation.InvestigationNotes = $"Investigation initiated from report validation. Validation comments: {ValidationComments}";

                CreateInvestigationCommand command = new CreateInvestigationCommand(investigation);
                var createResult = await Mediator.SendAsync(command, CancellationToken.None);

                if (createResult.IsSuccess)
                {
                    var newInvestigation = createResult.Value;
                    await NotificationHelper.ShowSuccessAsync($"Investigation {newInvestigation.Code} created successfully");
                    var navigationUrl = $"/SMSRiskManagement/Investigations/{newInvestigation.Code}/{ReportHazard.Code}";
                    Logger.LogInformation("Navigating to new investigation: {Url}", navigationUrl);

                    bool flowControl = await UpdateReportStatus(ReportId, ReportStatus.UnderInvestigation);
                    if (!flowControl)
                    {
                        throw new Exception($"Failed to Update Report Status during Create new Investigation: {DomainErrors.ReportValidationError.CreateFailed.Message}");
                    }
                    await Task.Delay(1500);
                    // ?? SECURE NAVIGATION - Navigate to new investigation
                    Navigation.NavigateToSecure(navigationUrl);
                }
                else
                {
                    throw new Exception($"Failed to create investigation: {createResult.Error?.Message ?? DomainErrors.InvestigationError.CreateFailed.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling investigation for ReportId: {ReportId}", ReportId);
            await NotificationHelper.ShowErrorAsync("Error handling investigation. Please try again.");
        }
    }

    /// <summary>
    /// Handle NOT_SMS_RISK decision - prompt user and close report if confirmed
    /// </summary>
    private async Task HandleNotSmsRisk()
    {
        var confirmed = await DialogService.Confirm(
            message: "This report has been determined to be NOT an SMS Risk. Do you want to close this report?",
            title: "Close Report",
            options: new ConfirmOptions()
            {
                OkButtonText = "Yes, Close Report",
                CancelButtonText = "No, Keep Open",
                Width = "500px"
            });

        if (confirmed == true)
        {
            await CloseReport();
        }
        else
        {
            await NotificationHelper.ShowSuccessAsync("Validation completed. Report remains open for further review.");
            await Task.Delay(1500);
           // Navigation.NavigateTo("/SMSRiskManagement/ReportProcessing");
        }
    }

    /// <summary>
    /// Close the report using CQRS mediator
    /// </summary>
    private async Task CloseReport()
    {
        try
        {
            if (ReportDetails == null)
            {
                throw new Exception("Report details not loaded");
            }

            // Update report status to Closed
            bool flowControl = await UpdateReportStatus(ReportDetails.Code, ReportStatus.Closed);
            if (!flowControl)
            {
                throw new Exception($"Failed to Update Report Status during exsiting investigation: {DomainErrors.ReportValidationError.CreateFailed.Message}");
            }
            else
            {
                await NotificationHelper.ShowSuccessAsync("Report has been closed successfully");
                Logger.LogInformation("Report {ReportId} closed due to NOT_SMS_RISK validation", ReportId);
                await Task.Delay(1500);
                // ?? SECURE NAVIGATION - Navigate to Report Processing
                Navigation.NavigateToSecure("/SMSRiskManagement/ReportProcessing");
            }

        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error closing report {ReportId}", ReportId);
            await NotificationHelper.ShowErrorAsync($"Error closing report: {ex.Message}");
        }
    }

    
    #endregion

    #region UI Helper Methods

    private string GetValidationCardStyle(string decisionValue)
    {
        //var baseStyle = "border: 2px solid var(--rz-border-color);";
        var baseStyle = "border: 2px solid; color:black;height:110px;";

        if (SelectedValidationDecision?.Value == decisionValue)
        {
            return decisionValue switch
            {
                "SMS_RISK" => baseStyle + " border-color: var(--rz-success); background-color: var(--rz-success-lighter);",
                "NOT_SMS_RISK" => baseStyle + " border-color: var(--rz-danger); background-color: var(--rz-danger-lighter);",
                "NEEDS_INVESTIGATION" => baseStyle + " border-color: var(--rz-warning); background-color: var(--rz-warning-lighter);",
                _ => baseStyle
            };
        }

        return baseStyle;
    }

    
    #endregion

    #region Models
    

    #endregion

    /// <summary>
    /// Shows Airport Shared Dataset confirmation dialog
    /// Returns true if user wants to create dataset, false to skip
    /// </summary>
    private async Task<bool> ShowAirportDatasetDialog()
    {
        return await DialogService.Confirm(
            message: "Do you want to create an Airport Shared Dataset for this SMS Risk assessment?",
            title: "Airport Shared Dataset",
            options: new ConfirmOptions()
            {
                OkButtonText = "Yes, Create Dataset",
                CancelButtonText = "No, Skip",
                Width = "500px"
            }) ?? false; // Handle null case
    }

    /// <summary>
    /// Navigate to Airport Shared Dataset creation page
    /// </summary>
    private async Task NavigateToDatasetCreation()
    {
        Logger.LogInformation("User chose to create Airport Shared Dataset for Report: {ReportId}", ReportId);
        
        var datasetUrl = $"/SMSRiskManagement/AirportSharedDataset/{ReportId}";
        if (!string.IsNullOrEmpty(ReportHazard?.Code))
        {
            datasetUrl += $"/{ReportHazard.Code}";
        }

        // Use secure navigation for consistency
        Navigation.NavigateToSecure(datasetUrl);
    }

    /// <summary>
    /// Handle existing risk assessment or create new one and navigate
    /// </summary>
    private async Task HandleRiskAssessmentNavigation()
    {
        Logger.LogInformation("User skipped Airport Shared Dataset creation for Report: {ReportId}", ReportId);

        // Find existing risk assessment for this hazard
        var existingRiskAssessment = await FindExistingRiskAssessment();

        if (existingRiskAssessment != null)
        {
            await NavigateToExistingRiskAssessment(existingRiskAssessment);
        }
        else
        {
            await CreateAndNavigateToNewRiskAssessment();
        }
    }

    /// <summary>
    /// Find existing risk assessment for the current hazard
    /// Returns null if none found
    /// </summary>
    private async Task<RiskAssessment?> FindExistingRiskAssessment()
    {
        if (string.IsNullOrEmpty(ReportHazard?.Code))
            return null;

        var query = new GetAllRiskAssessmentsQuery();
        var result = await Mediator.SendAsync(query, CancellationToken.None);

        if (!result.IsSuccess || result.Value?.Count == 0)
            return null;

        return result.Value.FirstOrDefault(ra => 
            !string.IsNullOrWhiteSpace(ra.HazardCode) && 
            ra.HazardCode.Equals(ReportHazard.Code, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Update existing risk assessment with lead assessor and navigate
    /// </summary>
    private async Task NavigateToExistingRiskAssessment(RiskAssessment existingRiskAssessment)
    {
        // Update lead assessor if provided
        if (!string.IsNullOrEmpty(LeadAssessor))
        {
            existingRiskAssessment.LeadAssessorId = LeadAssessor;
            
            var updateCmd = new UpdateRiskAssessmentCommand(existingRiskAssessment);
            await Mediator.SendAsync(updateCmd, CancellationToken.None);
        }

        // Update report status
        await UpdateReportStatusWithValidation(ReportStatus.RiskAssessmentInProgress, 
            "Failed to update report status for existing risk assessment");

        await NotificationHelper.ShowSuccessAsync($"Loading existing Risk Assessment {existingRiskAssessment.Code}");
        
        // Fix: Use the hazard code from the existing risk assessment if ReportHazard is null
        var hazardCode = ReportHazard?.Code ?? existingRiskAssessment.HazardCode ?? existingRiskAssessment.PrimaryHazardId;
        var navigationUrl = $"/SMSRiskManagement/TechnicalAssessment/{ReportId}/{hazardCode}/1";
        Logger.LogInformation("Navigating to existing Risk Assessment: {Url}", navigationUrl);

        await DelayAndNavigate(navigationUrl);
    }

    /// <summary>
    /// Create new risk assessment and navigate to it
    /// </summary>
    private async Task CreateAndNavigateToNewRiskAssessment()
    {
        if (ReportHazard == null)
        {
            throw new InvalidOperationException("Cannot create risk assessment - hazard information not found");
        }

        // Create new risk assessment
        var riskAssessment = CreateRiskAssessmentEntity();
        
        var command = new CreateRiskAssessmentCommand(riskAssessment);
        var createResult = await Mediator.SendAsync(command, CancellationToken.None);

        if (!createResult.IsSuccess)
        {
            throw new Exception($"Failed to create risk assessment: {createResult.Error?.Message ?? "Unknown error"}");
        }

        // Update report status
        await UpdateReportStatusWithValidation(ReportStatus.RiskAssessmentInProgress,
            "Failed to update report status for new risk assessment");

        var newRiskAssessment = createResult.Value;
        await NotificationHelper.ShowSuccessAsync($"Risk Assessment {newRiskAssessment.Code} created successfully");

        // Fix: Ensure hazard code is properly passed - use the ReportHazard.Code which we validated exists above
        var navigationUrl = $"/SMSRiskManagement/TechnicalAssessment/{ReportId}/{ReportHazard.Code}/1";
        Logger.LogInformation("Navigating to new risk assessment: {Url}", navigationUrl);

        await DelayAndNavigate(navigationUrl);
    }

    /// <summary>
    /// Create risk assessment entity with proper initialization
    /// </summary>
    private RiskAssessment CreateRiskAssessmentEntity()
    {
        var assessmentId = new RiskAssessmentID("RS-0000"); // Database will generate actual ID

        return new RiskAssessment(assessmentId)
        {
            Name = $"Technical Risk Assessment for Report {ReportId}",
            LeadAssessorId = LeadAssessor,
            AssessmentType = RiskAssessmentType.Initial,
            RiskAssessmentCategory = RiskAssessmentCategory.Technical,
            HazardCode = ReportHazard!.Code,
            PrimaryHazardId = ReportHazard.Code,
            Description = $"Created from Report {ReportId}",
            Stage = RiskAssessmentStage.DescribingSystem,
            Code = assessmentId.Value,
            Status = RiskAssessmentStatus.AssessmentCreate,
            CurrentStep = 1,
            UpdatedDate = DateTime.UtcNow,
            UpdatedBy = CurrentUserService?.UserDisplayName
        };
    }

    /// <summary>
    /// Update report status with proper error handling
    /// </summary>
    private async Task UpdateReportStatusWithValidation(ReportStatus status, string errorMessage)
    {
        bool success = await UpdateReportStatus(ReportId, status);
        if (!success)
        {
            throw new Exception($"{errorMessage}: {DomainErrors.ReportValidationError.CreateFailed.Message}");
        }
    }

    /// <summary>
    /// Consistent delay and navigation with secure routing
    /// </summary>
    private async Task DelayAndNavigate(string url)
    {
        await Task.Delay(1000);
        Navigation.NavigateToSecure(url); // Use secure navigation consistently
    }
}