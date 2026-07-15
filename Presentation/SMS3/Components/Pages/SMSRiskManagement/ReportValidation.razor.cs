using SMS_Domain.Enums;
using SMS_Domain.Events;
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
    [Inject] private IBaseEventBus _eventBus { get; set; } = default!;

    [Inject] private INotificationHelper _notificationHelper { get; set; } = default!;
    [Inject] private DialogService _dialogService { get; set; } = default!;

    // Form Data Properties - Using Smart Enum
    private ValidationDecision? _selectedValidationDecision { get; set; }
    private string _validationComments { get; set; } = "";
    private RiskAssessmentCategory _validationType { get; set; } = RiskAssessmentCategory.Technical;
    private string _currentValidationType => _riskRegistryOnly ? RiskAssessmentType.RiskRegistryOnly.Value : _validationType.Value;
    private string _validatedBy { get; set; } = "";

    private string _leadAssessor { get; set; } = "";

    private string _leadInvestigator { get; set; } = "";

    // Helper property for string-based UI binding - renamed to avoid conflicts
    private string ValidationDecisionValue
    {
        get => _selectedValidationDecision?.Value ?? "";
        set => _selectedValidationDecision = string.IsNullOrWhiteSpace(value) ? null : SMS_Domain.Enums.ValidationDecision.FromValue(value);
    }

    // Display Properties
    private bool _isLoading = true;
    private bool _isRiskRegistryCheckboxDisabled => _selectedValidationDecision != ValidationDecision.SmsRisk;
    private Report? _reportDetails { get; set; } = default!;
    private Hazard? _reportHazard { get; set; } = default!;

    private bool _riskRegistryOnly { get; set; }
    private SMS_Domain.Entities.ReportValidation? ExistingValidation { get; set; }
    private List<SMSApplicationUser> AvailableAssessors { get; set; } = new();

    private List<SMSApplicationUser> AvailableInvestigators { get; set; } = new();

    // State Properties
    private bool _isUpdate => ExistingValidation is not null;
    private string _validationCode => ExistingValidation?.Code ?? "New";
    //private string CurrentStatus { get; set;}= string.Empty; // ExistingValidation?.Status ?? "New";
    private bool _isProcessing { get; set; } = false;

    

    private List<DropdownOption> LeadAssessorOptions { get; set; } = new();

    private List<DropdownOption> LeadInvestigatorOptions { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        await LoadDataAsync();
        _isLoading = false;
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
                _reportDetails = reportResult.Value;

                // Load associated hazard
                var hazardResult = await _mediator.SendAsync(new GetHazardsByReportCodeQuery(reportCode), CancellationToken.None);
                if (hazardResult.IsSuccess)
                {
                    _reportHazard = hazardResult.Value?.FirstOrDefault(h => h.ReportCode.Trim() == reportCode.Value.Trim());
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
                        _selectedValidationDecision = decision;
                    }
                }
                _validationComments = ExistingValidation.ValidationComments ?? "";
                _validationType = RiskAssessmentCategory.Technical ;
                _validatedBy = ExistingValidation.ValidatedBy ?? "";

                _logger.LogInformation("Found existing ReportValidation for report {ReportId} - Decision: {Decision}",
                    ReportId, _selectedValidationDecision?.Name ?? "None");
            }
            else
            {
                _logger.LogInformation("No existing ReportValidation found for report: {ReportId}", ReportId);

                // Set defaults for new validation
                _selectedValidationDecision = null;
                _validatedBy = _currentUserService?.UserDisplayName ?? string.Empty;
                _validationType = RiskAssessmentCategory.Technical;
                _validationComments = "";
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
            _selectedValidationDecision = validationDecision;
            if (_selectedValidationDecision != ValidationDecision.SmsRisk)
            {
                this._riskRegistryOnly = false;
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
        if (_isProcessing) return; // Prevent double-click

        try
        {
            _isProcessing = true;
            StateHasChanged();

            _logger.LogInformation("HandleSubmit called for ReportId: {ReportId}, Decision: {Decision}", ReportId, _selectedValidationDecision?.Value);

            // Manual validation
            if (_selectedValidationDecision is null)
            {
                await _notificationHelper.ShowErrorAsync("Please select a validation decision");
                return;
            }

            if (string.IsNullOrWhiteSpace(_validationComments))
            {
                await _notificationHelper.ShowErrorAsync("Validation comments are required");
                return;
            }

            // Create validation record first
            await CreateValidationRecord();

                                               
            var navigationTask = _selectedValidationDecision switch
            {
                _ when _selectedValidationDecision == ValidationDecision.SmsRisk => NavigateToRiskAssessment(),
                _ when _selectedValidationDecision == ValidationDecision.NeedsInvestigation => NavigateToInvestigation(),
                _ when _selectedValidationDecision == ValidationDecision.NotSmsRisk => HandleNotSmsRisk(),
                _ => throw new InvalidOperationException($"Unhandled validation decision: {_selectedValidationDecision.Name}")
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
            _isProcessing = false;
            StateHasChanged();
        }
    }

    private RiskAssessment CreateRiskAssessmentEntity()
    {
        var assessmentId = new RiskAssessmentID("RS-0000"); // Database will generate actual ID
        var initialStep = _riskRegistryOnly ? 4 : 1;
        var initialStage = _riskRegistryOnly ? RiskAssessmentStage.AssessingRisk : RiskAssessmentStage.DescribingSystem;
        var initialStatus = _riskRegistryOnly ? RiskAssessmentStatus.AssessmentUnderway : RiskAssessmentStatus.AssessmentCreate;
        var assessmentType = _riskRegistryOnly ? RiskAssessmentType.RiskRegistryOnly : RiskAssessmentType.Technical;
        var assessmentLabel = _riskRegistryOnly ? RiskAssessmentType.RiskRegistryOnly.Name : RiskAssessmentType.Technical.Name;

        return new RiskAssessment(assessmentId)
        {
            Name = $"{assessmentLabel} Risk Assessment for Report {ReportId}",
            LeadAssessorId = _leadAssessor,
            AssessmentType = assessmentType,
            RiskAssessmentCategory = RiskAssessmentCategory.Technical,
            HazardCode = _reportHazard!.Code,
            PrimaryHazardId = _reportHazard.Code,
            Description = $"{assessmentLabel} assessment created from Report {ReportId}",
            Stage = initialStage,
            Code = assessmentId.Value,
            Status = initialStatus,
            CurrentStep = initialStep,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = _currentUserService?.UserDisplayName,
        };
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
                ExistingValidation.ValidationComments = _validationComments;
                ExistingValidation.ValidationType = _currentValidationType;
                ExistingValidation.ValidatedBy = _validatedBy;
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
                await _notificationHelper.ShowSuccessAsync($"Validation updated successfully. Decision: {_selectedValidationDecision?.Name}");

                // ValidationDecisionMade domain event is published by CQRS command handlers.
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
                    ValidatedBy = _validatedBy,
                    ValidationDecision = ValidationDecisionValue,
                    ValidationComments = _validationComments,
                    ValidationType = _currentValidationType,
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
                await _notificationHelper.ShowSuccessAsync($"Validation recorded successfully. Decision: {_selectedValidationDecision?.Name}");

                // ValidationDecisionMade domain event is published by CQRS command handlers.
            }

            

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in smart validation record processing for ReportId: {ReportId}", ReportId);
            throw; // Re-throw to be handled by HandleSubmit
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

            if (_riskRegistryOnly)
            {
                _logger.LogInformation("_riskRegistryOnly selected. Routing to Technical Assessment Step 4 for Report: {ReportId}", ReportId);

                var (riskAssessment, created) = await EnsureRiskAssessmentForCurrentHazardAsync();
                if (created)
                {
                    await _notificationHelper.ShowSuccessAsync($"Risk Assessment {riskAssessment.Code} created successfully");
                }
                else
                {
                    await _notificationHelper.ShowInfoAsync($"Using existing Risk Assessment {riskAssessment.Code}");
                }

                await UpdateReportStatusWithValidation(ReportStatus.RiskRegistryOnly, "Risk registry only");

                var encodedReportCode = Uri.EscapeDataString((ReportId ?? string.Empty).Trim());
                var hazardCode = _reportHazard?.Code ?? riskAssessment.HazardCode ?? riskAssessment.PrimaryHazardId ?? string.Empty;
                var encodedHazardCode = Uri.EscapeDataString(hazardCode.Trim());
                var navigationUrl = $"/SMSRiskManagement/TechnicalAssessment/{encodedReportCode}/{encodedHazardCode}/4";
                await DelayAndNavigate(navigationUrl);
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

    private async Task NavigateToInvestigation()
    {
        try
        {
            if (_reportHazard is null)
            {
                await _notificationHelper.ShowErrorAsync("Cannot create investigation - hazard information not found");
                return;
            }

            if (string.IsNullOrWhiteSpace(_reportHazard.Code))
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
                    !string.IsNullOrWhiteSpace(inv.HazardCode) && inv.HazardCode.Equals(_reportHazard.Code, StringComparison.OrdinalIgnoreCase) ||
                    !string.IsNullOrWhiteSpace(inv.ReportCode) && inv.ReportCode.Equals(ReportId, StringComparison.OrdinalIgnoreCase));
            }

            if (existingInvestigation is not null)
            {
                // Navigate to existing investigation
                await _notificationHelper.ShowSuccessAsync($"Loading existing investigation {existingInvestigation.Code}");
                var navigationUrl = $"/SMSRiskManagement/Investigations/{existingInvestigation.Code}/{_reportHazard.Code}";
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
                investigation.HazardCode = _reportHazard.Code;
                investigation.Status = InvestigationStatus.InvestigatorAssigned;
                investigation.CreatedBy = _currentUserService?.UserDisplayName;
                investigation.ReportCode = ReportId;
                investigation.AssignedInvestigatorId = _leadInvestigator;
                investigation.InvestigationObjectives = $"Investigation required based on validation decision for hazard {_reportHazard.Code}";
                investigation.InvestigationNotes = $"Investigation initiated from report validation. Validation comments: {_validationComments}";

                CreateInvestigationCommand command = new CreateInvestigationCommand(investigation);
                var createResult = await _mediator.SendAsync(command, CancellationToken.None);

                if (createResult.IsSuccess)
                {
                    var newInvestigation = createResult.Value;
                    await _notificationHelper.ShowSuccessAsync($"Investigation {newInvestigation.Code} created successfully");
                    var navigationUrl = $"/SMSRiskManagement/Investigations/{newInvestigation.Code}/{_reportHazard.Code}";
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
            if (_reportDetails is null)
            {
                throw new Exception("Report details not loaded");
            }

            // Update report status to Closed
            bool flowControl = await UpdateReportStatus(_reportDetails.Code, ReportStatus.Closed);
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

        if (_selectedValidationDecision?.Value == decisionValue)
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
        if (!string.IsNullOrEmpty(_reportHazard?.Code))
        {
            datasetUrl += $"/{_reportHazard.Code}";
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
        if (string.IsNullOrEmpty(_reportHazard?.Code))
            return null;

        var query = new GetAllRiskAssessmentsQuery();
        var result = await _mediator.SendAsync(query, CancellationToken.None);

        if (!result.IsSuccess || result.Value is null || result.Value.Count == 0)
            return null;

        var existingAssessment = result.Value
            .Where(ra => !string.IsNullOrWhiteSpace(ra.HazardCode)
                         && ra.HazardCode.Equals(_reportHazard.Code, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(ra => ra.UpdatedDate ?? ra.CreatedDate ?? DateTime.MinValue)
            .FirstOrDefault();

        if (existingAssessment is null)
        {
            return null;
        }

        if (!_riskRegistryOnly && existingAssessment.AssessmentType == RiskAssessmentType.RiskRegistryOnly)
        {
            _logger.LogInformation(
                "Converting existing RiskAssessment {RiskAssessmentCode} from {CurrentType} to {TargetType} for hazard {HazardCode}",
                existingAssessment.Code,
                existingAssessment.AssessmentType.Value,
                RiskAssessmentType.Technical.Value,
                _reportHazard.Code);

            existingAssessment.AssessmentType = RiskAssessmentType.Technical;
            existingAssessment.CurrentStep = 1;
            existingAssessment.Stage = RiskAssessmentStage.DescribingSystem;
            existingAssessment.Status = RiskAssessmentStatus.AssessmentCreate;
            existingAssessment.CompletedDate = null;
            existingAssessment.CompletedBy = null;
            if (!string.IsNullOrWhiteSpace(_leadAssessor))
            {
                existingAssessment.LeadAssessorId = _leadAssessor;
            }

            existingAssessment.UpdatedBy = _currentUserService?.UserDisplayName ?? "SYSTEM";
            existingAssessment.UpdatedDate = DateTime.UtcNow;

            var updateCmd = new UpdateRiskAssessmentCommand(existingAssessment);
            var updateResult = await _mediator.SendAsync(updateCmd, CancellationToken.None);
            if (updateResult.IsSuccess && updateResult.Value is not null)
            {
                return updateResult.Value;
            }

            _logger.LogWarning(
                "Failed to convert existing RiskAssessment {RiskAssessmentCode} to Technical. Reusing existing assessment without type change. Error: {Error}",
                existingAssessment.Code,
                updateResult.Error?.Message ?? "Unknown error");
        }
        else if (_riskRegistryOnly && existingAssessment.AssessmentType == RiskAssessmentType.Technical)
        {
            _logger.LogInformation(
                "Converting existing RiskAssessment {RiskAssessmentCode} from {CurrentType} to {TargetType} for hazard {HazardCode}",
                existingAssessment.Code,
                existingAssessment.AssessmentType.Value,
                RiskAssessmentType.RiskRegistryOnly.Value,
                _reportHazard.Code);

            existingAssessment.AssessmentType = RiskAssessmentType.RiskRegistryOnly;
            existingAssessment.CurrentStep = 4;
            existingAssessment.Stage = RiskAssessmentStage.AssessingRisk;
            existingAssessment.Status = RiskAssessmentStatus.AssessmentUnderway;
            existingAssessment.CompletedDate = null;
            existingAssessment.CompletedBy = null;
            if (!string.IsNullOrWhiteSpace(_leadAssessor))
            {
                existingAssessment.LeadAssessorId = _leadAssessor;
            }

            existingAssessment.UpdatedBy = _currentUserService?.UserDisplayName ?? "SYSTEM";
            existingAssessment.UpdatedDate = DateTime.UtcNow;

            var updateCmd = new UpdateRiskAssessmentCommand(existingAssessment);
            var updateResult = await _mediator.SendAsync(updateCmd, CancellationToken.None);
            if (updateResult.IsSuccess && updateResult.Value is not null)
            {
                return updateResult.Value;
            }

            _logger.LogWarning(
                "Failed to convert existing RiskAssessment {RiskAssessmentCode} to RiskRegistryOnly. Reusing existing assessment without type change. Error: {Error}",
                existingAssessment.Code,
                updateResult.Error?.Message ?? "Unknown error");
        }

        return existingAssessment;
    }

    /// <summary>
    /// Ensure a risk assessment exists for the current hazard.
    /// Returns the assessment and whether it was created by this call.
    /// </summary>
    private async Task<(RiskAssessment assessment, bool created)> EnsureRiskAssessmentForCurrentHazardAsync()
    {
        var existingRiskAssessment = await FindExistingRiskAssessment();
        if (existingRiskAssessment is not null)
        {
            return (existingRiskAssessment, false);
        }

        if (_reportHazard is null)
        {
            throw new InvalidOperationException("Cannot create risk assessment - hazard information not found");
        }

        var riskAssessment = CreateRiskAssessmentEntity();
        var command = new CreateRiskAssessmentCommand(riskAssessment);
        var createResult = await _mediator.SendAsync(command, CancellationToken.None);

        if (!createResult.IsSuccess)
        {
            throw new Exception($"Failed to create risk assessment: {createResult.Error?.Message ?? "Unknown error"}");
        }

        return (createResult.Value, true);
    }

    /// <summary>
    /// Update existing risk assessment with lead assessor and navigate
    /// </summary>
    private async Task NavigateToExistingRiskAssessment(RiskAssessment existingRiskAssessment)
    {
        // Update lead assessor if provided
        if (!string.IsNullOrEmpty(_leadAssessor))
        {
            existingRiskAssessment.LeadAssessorId = _leadAssessor;
            
            var updateCmd = new UpdateRiskAssessmentCommand(existingRiskAssessment);
            await _mediator.SendAsync(updateCmd, CancellationToken.None);
        }

        // Update report status
        await UpdateReportStatusWithValidation(ReportStatus.RiskAssessmentInProgress, 
            "Failed to update report status for existing risk assessment");

        await _notificationHelper.ShowSuccessAsync($"Loading existing Risk Assessment {existingRiskAssessment.Code}");
        
        // Fix: Use the hazard code from the existing risk assessment if _reportHazard is null
        var hazardCode = _reportHazard?.Code ?? existingRiskAssessment.HazardCode ?? existingRiskAssessment.PrimaryHazardId;
        var navigationUrl = $"/SMSRiskManagement/TechnicalAssessment/{ReportId}/{hazardCode}/1";
        _logger.LogInformation("Navigating to existing Risk Assessment: {Url}", navigationUrl);

        await DelayAndNavigate(navigationUrl);
    }

    /// <summary>
    /// Create new risk assessment and navigate to it
    /// </summary>
    private async Task CreateAndNavigateToNewRiskAssessment()
    {
        var (newRiskAssessment, _) = await EnsureRiskAssessmentForCurrentHazardAsync();

        // Update report status
        await UpdateReportStatusWithValidation(ReportStatus.RiskAssessmentInProgress, "Failed to update report status for new risk assessment");

        await _notificationHelper.ShowSuccessAsync($"Risk Assessment {newRiskAssessment.Code} created successfully");

        // Fix: Ensure hazard code is properly passed - use _reportHazard if available, else assessment hazard
        var hazardCode = _reportHazard?.Code ?? newRiskAssessment.HazardCode ?? newRiskAssessment.PrimaryHazardId;
        var navigationUrl = $"/SMSRiskManagement/TechnicalAssessment/{ReportId}/{hazardCode}/1";
        _logger.LogInformation("Navigating to new risk assessment: {Url}", navigationUrl);

        await DelayAndNavigate(navigationUrl);
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


