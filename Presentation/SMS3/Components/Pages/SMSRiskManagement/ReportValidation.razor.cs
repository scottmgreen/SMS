using SMS_Domain.Enums;
using SMS_Application.Services;
using SMS_Domain.Errors;

using SMS_Shared.Configuration;

using SMS3.Components.Shared.UIHelpers;
using SMS3.Configuration.Extensions;

namespace SMS3.Components.Pages.SMSRiskManagement;

public partial class ReportValidation : ComponentBase
{
    [Parameter] public string ReportId { get; set; } = "";

    [Inject] private ICurrentUserService _currentUserService { get; set; } = default!;
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<ReportValidation> _logger { get; set; } = default!;
    [Inject] private NavigationManager _navigation { get; set; } = default!;
    [Inject] private SPIEventCoordinator _spiCoordinator { get; set; } = default!;

    [Inject] private INotificationHelper _notificationHelper { get; set; } = default!;
    [Inject] private DialogService _dialogService { get; set; } = default!;

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
    private bool IsUpdate => ExistingValidation is not null;
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
            _logger.LogInformation("Loading SMS report validation for report: {ReportId}", ReportId);

            if (string.IsNullOrWhiteSpace(ReportId))
            {
                await _notificationHelper.ShowErrorAsync("Report ID is required for SMS report validation");
                _navigation.NavigateToSecure("/SMSRiskManagement/ReportProcessing");
                return;
            }

            var reportCode = new ReportID(ReportId);

            // Load report details
            var reportResult = await _mediator.SendAsync(new GetReportByCodeQuery(reportCode), CancellationToken.None);
            if (reportResult.IsSuccess)
            {
                ReportDetails = reportResult.Value;

                // Load associated hazard
                var hazardResult = await _mediator.SendAsync(new GetHazardsByReportCodeQuery(reportCode), CancellationToken.None);
                if (hazardResult.IsSuccess)
                {
                    ReportHazard = hazardResult.Value?.FirstOrDefault(h => h.ReportCode.Trim() == reportCode.Value.Trim());
                }
            }
            

            // Check for existing validation
            var existingValidationQuery = new GetReportValidationByReportIdQuery(reportCode);
            var validationResult = await _mediator.SendAsync(existingValidationQuery, CancellationToken.None);

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

                _logger.LogInformation("Found existing ReportValidation for report {ReportId} - Decision: {Decision}",
                    ReportId, SelectedValidationDecision?.Name ?? "None");
            }
            else
            {
                _logger.LogInformation("No existing ReportValidation found for report: {ReportId}", ReportId);

                // Set defaults for new validation
                SelectedValidationDecision = null;
                ValidatedBy = _currentUserService?.UserDisplayName ?? string.Empty;
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
            _logger.LogError(ex, "Error loading report for SMS validation: {ReportId}", ReportId);
            await _notificationHelper.ShowErrorAsync("An error occurred while loading the report for validation");
            _navigation.NavigateToSecure("/SMSRiskManagement/ReportProcessing");
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
            _logger.LogWarning(ex, "Could not load available assessors");
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
            var usersResult = await _mediator.SendAsync(usersQuery, CancellationToken.None);
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
            _logger.LogWarning(ex, "Could not load available assessors");
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

            _logger.LogInformation("HandleSubmit called for ReportId: {ReportId}, Decision: {Decision}", ReportId, SelectedValidationDecision?.Value);

            // Manual validation
            if (SelectedValidationDecision is null)
            {
                await _notificationHelper.ShowErrorAsync("Please select a validation decision");
                return;
            }

            if (string.IsNullOrWhiteSpace(ValidationComments))
            {
                await _notificationHelper.ShowErrorAsync("Validation comments are required");
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
            _logger.LogError(ex, "Error in HandleSubmit for ReportId: {ReportId}", ReportId);
            await _notificationHelper.ShowErrorAsync("Error processing validation. Please try again.");
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
        var getReportQueryResult = await _mediator.SendAsync(getReportQuery, CancellationToken.None);

        var cmd = new UpdateReportStatusCommand(reportId, status, _currentUserService?.UserDisplayName ?? "SYSTEM");
        var cmdResult = await _mediator.SendAsync(cmd, CancellationToken.None);
        if (!cmdResult.IsSuccess)
        {
            await _notificationHelper.ShowErrorAsync($"Report{reportId} Status Was not Updated");
            return false;
        }
        return true;
    }
    
    private async Task CreateValidationRecord()
    {
        try
        {
            _logger.LogInformation("Smart validation record processing for ReportId: {ReportId}, HasExisting: {HasExisting}",ReportId, ExistingValidation is not null);

            if (ExistingValidation is not null)
            {
                // ? UPDATE EXISTING VALIDATION
                _logger.LogInformation("Updating existing ReportValidation: {ValidationCode}", ExistingValidation.Code);

                // Update the existing validation with new values
                ExistingValidation.ValidationDecision = ValidationDecisionValue;
                ExistingValidation.ValidationComments = ValidationComments;
                ExistingValidation.ValidationType = RiskAssessmentCategory.Technical;
                ExistingValidation.ValidatedBy = ValidatedBy;
                ExistingValidation.Status = ReportValidationStatus.Revised;
                ExistingValidation.Stage = "COMPLETE";
                ExistingValidation.ValidatedDate = DateTime.UtcNow;

                ExistingValidation.UpdatedBy = _currentUserService?.UserDisplayName; 
                ExistingValidation.UpdatedDate = DateTime.UtcNow;

                var updateCommand = new UpdateReportValidationCommand(ExistingValidation);
                var result = await _mediator.SendAsync(updateCommand, CancellationToken.None);

                if (!result.IsSuccess)
                {
                    throw new Exception($"Failed to update existing validation: {result.Error?.Message ?? DomainErrors.ReportValidationError.UpdateFailed.Message}");
                }
                bool flowControl = await UpdateReportStatus(ReportId, ReportStatus.ValidationRevised);
                if (!flowControl)
                {
                    throw new Exception($"Failed to Update Report Status during Update Validation: {result.Error?.Message ?? DomainErrors.ReportValidationError.UpdateFailed.Message}");
                }

                _logger.LogInformation("Successfully updated existing ReportValidation: {ValidationCode}", ExistingValidation.Code);
                await _notificationHelper.ShowSuccessAsync($"Validation updated successfully. Decision: {SelectedValidationDecision?.Name}");

                // NEW: SPI AUTOMATION - Trigger validation decision event ??
                await TriggerValidationSPIAutomation(ExistingValidation.Code, ValidationDecisionValue, ExistingValidation.ValidatedDate ?? DateTime.UtcNow);
            }
            else
            {
                // ? CREATE NEW VALIDATION (only if none exists)
                _logger.LogInformation("Creating new ReportValidation for ReportId: {ReportId}", ReportId);

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
                    CreatedBy = _currentUserService?.UserDisplayName,
                    CreatedDate = DateTime.UtcNow
                };

                var createCommand = new CreateReportValidationCommand(validation);
                var result = await _mediator.SendAsync(createCommand, CancellationToken.None);

                if (!result.IsSuccess)
                {
                    throw new Exception($"Failed to create new validation: {result.Error?.Message ?? DomainErrors.ReportValidationError.CreateFailed.Message}");
                }
                bool flowControl = await UpdateReportStatus(ReportId, ReportStatus.ValidationCompleted);
                if (!flowControl)
                {
                    throw new Exception($"Failed to Update Report Status during Create new validation: {result.Error?.Message ?? DomainErrors.ReportValidationError.CreateFailed.Message}");
                }
                _logger.LogInformation("Successfully created new ReportValidation: {ValidationCode}", result.Value.Code);
                await _notificationHelper.ShowSuccessAsync($"Validation recorded successfully. Decision: {SelectedValidationDecision?.Name}");

                // NEW: SPI AUTOMATION - Trigger validation decision event ??
                await TriggerValidationSPIAutomation(result.Value.Code, ValidationDecisionValue, validation.ValidatedDate ?? DateTime.UtcNow);
            }

            

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in smart validation record processing for ReportId: {ReportId}", ReportId);
            throw; // Re-throw to be handled by HandleSubmit
        }
    }

    /// <summary>
    /// Triggers SPI automation when validation decision is made
    /// Updates risk identification effectiveness and validation-related SPIs
    /// </summary>
    private async Task TriggerValidationSPIAutomation(string validationCode, string validationDecision, DateTime validatedDate)
    {
        try
        {
            _logger.LogInformation("SPI Automation: Triggering validation decision event for {ValidationCode} - Decision: {Decision}", 
                validationCode, validationDecision);

            await _spiCoordinator.OnValidationDecisionMade(
                reportId: ReportId,
                reportCode: ReportId,
                validationDecision: validationDecision,
                validatedDate: validatedDate,
                validatedBy: ValidatedBy,
                validationComments: ValidationComments);

            _logger.LogInformation("SPI Automation: Successfully processed validation decision event for {ValidationCode}", validationCode);
        }
        catch (Exception spiEx)
        {
            // Don't fail the validation process if SPI automation fails
            _logger.LogWarning(spiEx, "SPI Automation: Failed to process validation decision event for {ValidationCode} - continuing with validation", validationCode);
        }
    }

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
            _logger.LogError(ex, "Error in NavigateToRiskAssessment for Report: {ReportId}", ReportId);
            await _notificationHelper.ShowErrorAsync("Error navigating to risk assessment. Please try again.");
        }
    }

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
            _logger.LogInformation("User skipped Airport Shared Dataset creation, navigating to Risk Registry for Report: {ReportId}", ReportId);
            
            string navigationUrl = "/SMSAssurance/RiskRegistry";
            await DelayAndNavigate(navigationUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in NavigateToRiskRegistry for Report: {ReportId}", ReportId);
            await _notificationHelper.ShowErrorAsync("Error navigating to risk registry. Please try again.");
        }
    }

    private async Task NavigateToInvestigation()
    {
        try
        {
            if (ReportHazard is null)
            {
                await _notificationHelper.ShowErrorAsync("Cannot create investigation - hazard information not found");
                return;
            }

            if (string.IsNullOrWhiteSpace(ReportHazard.Code))
            {
                await _notificationHelper.ShowErrorAsync("Cannot create investigation - invalid hazard code");
                return;
            }

            // Check for existing investigation first
            var existingInvestigationsQuery = new GetAllInvestigationsQuery();
            var existingResult = await _mediator.SendAsync(existingInvestigationsQuery, CancellationToken.None);

            Investigation? existingInvestigation = null;
            if (existingResult.IsSuccess && existingResult.Value is not null)
            {
                existingInvestigation = existingResult.Value.FirstOrDefault(inv =>
                    !string.IsNullOrWhiteSpace(inv.HazardCode) && inv.HazardCode.Equals(ReportHazard.Code, StringComparison.OrdinalIgnoreCase) ||
                    !string.IsNullOrWhiteSpace(inv.ReportCode) && inv.ReportCode.Equals(ReportId, StringComparison.OrdinalIgnoreCase));
            }

            if (existingInvestigation is not null)
            {
                // Navigate to existing investigation
                await _notificationHelper.ShowSuccessAsync($"Loading existing investigation {existingInvestigation.Code}");
                var navigationUrl = $"/SMSRiskManagement/Investigations/{existingInvestigation.Code}/{ReportHazard.Code}";
                _logger.LogInformation("Navigating to existing investigation: {Url}", navigationUrl);
                bool flowControl = await UpdateReportStatus(ReportId, ReportStatus.UnderInvestigation);
                if (!flowControl)
                {
                    throw new Exception($"Failed to Update Report Status during exsiting investigation: {DomainErrors.ReportValidationError.CreateFailed.Message}");
                }
                await Task.Delay(1500);
                // ?? SECURE NAVIGATION - Navigate to existing investigation
                _navigation.NavigateToSecure(navigationUrl);
            }
            else
            {
                // Create new investigation
                var investigationCode = $"IN-0000";
                var investigationId = new InvestigationID(investigationCode);
                Investigation investigation = new Investigation(investigationId);
                investigation.HazardCode = ReportHazard.Code;
                investigation.Status = InvestigationStatus.InvestigatorAssigned;
                investigation.CreatedBy = _currentUserService?.UserDisplayName;
                investigation.ReportCode = ReportId;
                investigation.AssignedInvestigatorId = LeadInvestigator;
                investigation.InvestigationObjectives = $"Investigation required based on validation decision for hazard {ReportHazard.Code}";
                investigation.InvestigationNotes = $"Investigation initiated from report validation. Validation comments: {ValidationComments}";

                CreateInvestigationCommand command = new CreateInvestigationCommand(investigation);
                var createResult = await _mediator.SendAsync(command, CancellationToken.None);

                if (createResult.IsSuccess)
                {
                    var newInvestigation = createResult.Value;
                    await _notificationHelper.ShowSuccessAsync($"Investigation {newInvestigation.Code} created successfully");
                    var navigationUrl = $"/SMSRiskManagement/Investigations/{newInvestigation.Code}/{ReportHazard.Code}";
                    _logger.LogInformation("Navigating to new investigation: {Url}", navigationUrl);

                    bool flowControl = await UpdateReportStatus(ReportId, ReportStatus.UnderInvestigation);
                    if (!flowControl)
                    {
                        throw new Exception($"Failed to Update Report Status during Create new Investigation: {DomainErrors.ReportValidationError.CreateFailed.Message}");
                    }
                    await Task.Delay(1500);
                    // ?? SECURE NAVIGATION - Navigate to new investigation
                    _navigation.NavigateToSecure(navigationUrl);
                }
                else
                {
                    throw new Exception($"Failed to create investigation: {createResult.Error?.Message ?? DomainErrors.InvestigationError.CreateFailed.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling investigation for ReportId: {ReportId}", ReportId);
            await _notificationHelper.ShowErrorAsync("Error handling investigation. Please try again.");
        }
    }

    private async Task HandleNotSmsRisk()
    {
        var confirmed = await _dialogService.Confirm(
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
            await _notificationHelper.ShowSuccessAsync("Validation completed. Report remains open for further review.");
            await Task.Delay(1500);
           // Navigation.NavigateTo("/SMSRiskManagement/ReportProcessing");
        }
    }

    private async Task CloseReport()
    {
        try
        {
            if (ReportDetails is null)
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
                await _notificationHelper.ShowSuccessAsync("Report has been closed successfully");
                _logger.LogInformation("Report {ReportId} closed due to NOT_SMS_RISK validation", ReportId);
                await Task.Delay(1500);
                // ?? SECURE NAVIGATION - Navigate to Report Processing
                _navigation.NavigateToSecure("/SMSRiskManagement/ReportProcessing");
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing report {ReportId}", ReportId);
            await _notificationHelper.ShowErrorAsync($"Error closing report: {ex.Message}");
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
        return await _dialogService.Confirm(
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
        _logger.LogInformation("User chose to create Airport Shared Dataset for Report: {ReportId}", ReportId);
        
        var datasetUrl = $"/SMSRiskManagement/AirportSharedDataset/{ReportId}";
        if (!string.IsNullOrEmpty(ReportHazard?.Code))
        {
            datasetUrl += $"/{ReportHazard.Code}";
        }

        // Use secure navigation for consistency
        _navigation.NavigateToSecure(datasetUrl);
    }

    /// <summary>
    /// Handle existing risk assessment or create new one and navigate
    /// </summary>
    private async Task HandleRiskAssessmentNavigation()
    {
        _logger.LogInformation("User skipped Airport Shared Dataset creation for Report: {ReportId}", ReportId);

        // Find existing risk assessment for this hazard
        var existingRiskAssessment = await FindExistingRiskAssessment();

        if (existingRiskAssessment is not null)
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
        var result = await _mediator.SendAsync(query, CancellationToken.None);

        if (!result.IsSuccess || result.Value is null || result.Value.Count == 0)
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
            await _mediator.SendAsync(updateCmd, CancellationToken.None);
        }

        // Update report status
        await UpdateReportStatusWithValidation(ReportStatus.RiskAssessmentInProgress, 
            "Failed to update report status for existing risk assessment");

        await _notificationHelper.ShowSuccessAsync($"Loading existing Risk Assessment {existingRiskAssessment.Code}");
        
        // Fix: Use the hazard code from the existing risk assessment if ReportHazard is null
        var hazardCode = ReportHazard?.Code ?? existingRiskAssessment.HazardCode ?? existingRiskAssessment.PrimaryHazardId;
        var navigationUrl = $"/SMSRiskManagement/TechnicalAssessment/{ReportId}/{hazardCode}/1";
        _logger.LogInformation("Navigating to existing Risk Assessment: {Url}", navigationUrl);

        await DelayAndNavigate(navigationUrl);
    }

    /// <summary>
    /// Create new risk assessment and navigate to it
    /// </summary>
    private async Task CreateAndNavigateToNewRiskAssessment()
    {
        if (ReportHazard is null)
        {
            throw new InvalidOperationException("Cannot create risk assessment - hazard information not found");
        }

        // Create new risk assessment
        var riskAssessment = CreateRiskAssessmentEntity();
        
        var command = new CreateRiskAssessmentCommand(riskAssessment);
        var createResult = await _mediator.SendAsync(command, CancellationToken.None);

        if (!createResult.IsSuccess)
        {
            throw new Exception($"Failed to create risk assessment: {createResult.Error?.Message ?? "Unknown error"}");
        }

        // Update report status
        await UpdateReportStatusWithValidation(ReportStatus.RiskAssessmentInProgress,
            "Failed to update report status for new risk assessment");

        var newRiskAssessment = createResult.Value;
        await _notificationHelper.ShowSuccessAsync($"Risk Assessment {newRiskAssessment.Code} created successfully");

        // Fix: Ensure hazard code is properly passed - use the ReportHazard.Code which we validated exists above
        var navigationUrl = $"/SMSRiskManagement/TechnicalAssessment/{ReportId}/{ReportHazard.Code}/1";
        _logger.LogInformation("Navigating to new risk assessment: {Url}", navigationUrl);

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
            UpdatedBy = _currentUserService?.UserDisplayName
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
        _navigation.NavigateToSecure(url); // Use secure navigation consistently
    }
}
