using Microsoft.AspNetCore.Components;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Application.Messaging.Queries;
using SMS_Application.Messaging.Commands;
using SMS_Application.Interfaces;
using SMS_Shared.Common;
using Radzen;
using SMS3.Components.Shared;
using SMS_Domain.Errors;

namespace SMS3.Components.Pages.SMSRiskManagement;

public partial class ReportValidation : ComponentBase
{
    [Parameter] public string ReportId { get; set; } = "";
    
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<ReportValidation> Logger { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;

    // Form Data Properties - Using Smart Enum
    private ValidationDecision? SelectedValidationDecision { get; set; }
    private string ValidationComments { get; set; } = "";
    private string ValidationType { get; set; } = "Technical";
    private string ValidatedBy { get; set; } = "";

    // Helper property for string-based UI binding - renamed to avoid conflicts
    private string ValidationDecisionValue
    {
        get => SelectedValidationDecision?.Value ?? "";
        set => SelectedValidationDecision = string.IsNullOrWhiteSpace(value) ? null : SMS_Domain.Enums.ValidationDecision.FromValue(value);
    }

    // Display Properties
    private Report? ReportDetails { get; set; }
    private Hazard? ReportHazard { get; set; }
    private SMS_Domain.Entities.ReportValidation? ExistingValidation { get; set; }
    private List<SMSApplicationUser> AvailableAssessors { get; set; } = new();

    // State Properties
    private bool IsUpdate => ExistingValidation != null;
    private string ValidationCode => ExistingValidation?.Code ?? "New";
    private string CurrentStatus => ExistingValidation?.Status ?? "New";
    private bool IsProcessing { get; set; } = false;

    // Dropdown Options
    private List<DropdownOption> ValidationTypeOptions { get; set; } = new()
    {
        new DropdownOption { Value = "Preliminary", Text = "Preliminary Assessment" },
        new DropdownOption { Value = "Technical", Text = "Technical Assessment" }
    };

    private List<DropdownOption> ValidatedByOptions { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    #region Data Loading

    private async Task LoadDataAsync()
    {
        try
        {
            Logger.LogInformation("Loading SMS report validation for report: {ReportId}", ReportId);

            if (string.IsNullOrWhiteSpace(ReportId))
            {
                ShowErrorNotification("Report ID is required for SMS report validation");
                Navigation.NavigateTo("/SMSRiskManagement/ReportProcessing");
                return;
            }

            var reportCode = new ReportID(ReportId);

            // Load report details
            var reportResult = await Mediator.SendAsync(new GetReportByIdQuery(reportCode), CancellationToken.None);
            if (reportResult.IsSuccess)
            {
                ReportDetails = reportResult.Value;
            }

            // Load associated hazard
            var hazardResult = await Mediator.SendAsync(new GetAllHazardsQuery(), CancellationToken.None);
            if (hazardResult.IsSuccess)
            {
                ReportHazard = hazardResult.Value?.FirstOrDefault(h => h.ReportCode.Trim() == reportCode.Value.Trim());
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
                ValidationType = ExistingValidation.ValidationType ?? "Technical";
                ValidatedBy = ExistingValidation.ValidatedBy ?? "";
                
                Logger.LogInformation("Found existing ReportValidation for report {ReportId} - Decision: {Decision}", 
                    ReportId, SelectedValidationDecision?.Name ?? "None");
            }
            else
            {
                Logger.LogInformation("No existing ReportValidation found for report: {ReportId}", ReportId);
                
                // Set defaults for new validation
                SelectedValidationDecision = null;
                ValidatedBy = "";
                ValidationType = "Technical";
                ValidationComments = "";
            }

            // Load available assessors
            await LoadAvailableAssessorsAsync();

            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading report for SMS validation: {ReportId}", ReportId);
            ShowErrorNotification("An error occurred while loading the report for validation");
            Navigation.NavigateTo("/SMSRiskManagement/ReportProcessing");
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
                ValidatedByOptions = new List<DropdownOption>
                {
                    new DropdownOption { Value = "", Text = "" }
                };
                
                foreach (var assessor in AvailableAssessors)
                {
                    ValidatedByOptions.Add(new DropdownOption 
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
            ValidatedByOptions = new List<DropdownOption>
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

            Logger.LogInformation("HandleSubmit called for ReportId: {ReportId}, Decision: {Decision}", 
                ReportId, SelectedValidationDecision?.Value);

            // Manual validation
            if (SelectedValidationDecision == null)
            {
                ShowErrorNotification("Please select a validation decision");
                return;
            }

            if (string.IsNullOrWhiteSpace(ValidationComments))
            {
                ShowErrorNotification("Validation comments are required");
                return;
            }

            // Create validation record first
            await CreateValidationRecord();

            // Navigate based on decision
            switch (SelectedValidationDecision.Value)
            {
                case "SMS_RISK":
                    await NavigateToRiskAssessment();
                    break;
                
                case "NEEDS_INVESTIGATION":
                    await NavigateToInvestigation();
                    break;
                
                case "NOT_SMS_RISK":
                    await HandleNotSmsRisk();
                    break;
                
                default:
                    ShowErrorNotification("Invalid validation decision");
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in HandleSubmit for ReportId: {ReportId}", ReportId);
            ShowErrorNotification("Error processing validation. Please try again.");
        }
        finally
        {
            IsProcessing = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Create the validation record using CQRS
    /// </summary>
    private async Task CreateValidationRecord()
    {
        var validationId = new ReportValidationID($"RV-0000");
        
        var validation = new SMS_Domain.Entities.ReportValidation(validationId)
        {
            Code = validationId.Value,
            ReportCode = ReportId,
            ValidatedBy = GetCurrentUserCode(),
            ValidationDecision = ValidationDecisionValue,
            ValidationComments = ValidationComments,
            ValidationType = ValidationType ?? "Technical",
            Status = "Completed",
            Stage = "Complete",
            ValidatedDate = DateTime.UtcNow,
            CreatedBy = GetCurrentUserCode(),
            CreatedDate = DateTime.UtcNow
        };

        var createCommand = new CreateReportValidationCommand(validation);
        var result = await Mediator.SendAsync(createCommand, CancellationToken.None);

        if (!result.IsSuccess)
        {
            throw new Exception($"Failed to create validation: {result.Error?.Message ?? DomainErrors.ReportValidationError.CreateFailed.Message}");
        }

        ShowSuccessNotification($"Validation recorded successfully. Decision: {SelectedValidationDecision.Name}");
    }

    /// <summary>
    /// Navigate to Risk Assessment (Preliminary or Technical)
    /// </summary>
    private async Task NavigateToRiskAssessment()
    {
        // Show Airport Shared Dataset dialog before proceeding to assessment
        var result = await DialogService.Confirm(
            message: "Do you want to create an Airport Shared Dataset for this SMS Risk assessment?",
            title: "Airport Shared Dataset", 
            options: new ConfirmOptions() 
            { 
                OkButtonText = "Yes, Create Dataset", 
                CancelButtonText = "No, Skip",
                Width = "500px"
            });

        if (result == true)
        {
            // User wants to create dataset - navigate to dataset creation page
            Logger.LogInformation("User chose to create Airport Shared Dataset for Report: {ReportId}", ReportId);
            var datasetUrl = $"/SMSRiskManagement/AirportSharedDataset/{ReportId}";
            
            if (!string.IsNullOrEmpty(ReportHazard?.Code))
            {
                datasetUrl += $"/{ReportHazard.Code}";
            }
            
            Navigation.NavigateTo(datasetUrl);
        }
        else
        {
            // User skipped dataset creation - proceed directly to assessment
            Logger.LogInformation("User skipped Airport Shared Dataset creation for Report: {ReportId}", ReportId);
            
            var assessmentType = ValidationType?.ToLower() switch
            {
                "technical" => "TechnicalAssessment",
                "preliminary" => "PreliminaryRiskAssessment", 
                _ => "TechnicalAssessment"
            };

            string navigationUrl;
            if (ReportHazard != null)
            {
                navigationUrl = $"/SMSRiskManagement/{assessmentType}/{ReportId}/{ReportHazard.Code}/1";
            }
            else
            {
                navigationUrl = $"/SMSRiskManagement/{assessmentType}/{ReportId}/1";
            }

            Logger.LogInformation("Navigating to {AssessmentType}: {Url}", assessmentType, navigationUrl);
            
            await Task.Delay(1500);
            Navigation.NavigateTo(navigationUrl);
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
                ShowErrorNotification("Cannot create investigation - hazard information not found");
                return;
            }

            if (string.IsNullOrWhiteSpace(ReportHazard.Code))
            {
                ShowErrorNotification("Cannot create investigation - invalid hazard code");
                return;
            }

            // Check for existing investigation first
            var existingInvestigationQuery = new GetAllInvestigationsQuery();
            var existingResult = await Mediator.SendAsync(existingInvestigationQuery, CancellationToken.None);

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
                ShowSuccessNotification($"Loading existing investigation {existingInvestigation.Code}");
                var navigationUrl = $"/SMSRiskManagement/Investigations/{existingInvestigation.Code}/{ReportHazard.Code}";
                Logger.LogInformation("Navigating to existing investigation: {Url}", navigationUrl);
                await Task.Delay(1500);
                Navigation.NavigateTo(navigationUrl);
            }
            else
            {
                // Create new investigation
                var investigationCode = $"IN-0000";
                var investigationId = new InvestigationID(investigationCode);
                Investigation investigation = new Investigation(investigationId);
                investigation.HazardCode = ReportHazard.Code;
                investigation.CreatedBy = GetCurrentUserCode();
                investigation.ReportCode = ReportId;
                investigation.AssignedInvestigatorId = GetCurrentUserCode();
                investigation.InvestigationObjectives = $"Investigation required based on validation decision for hazard {ReportHazard.Code}";
                investigation.InvestigationNotes = $"Investigation initiated from report validation. Validation comments: {ValidationComments}";

                CreateInvestigationCommand command = new CreateInvestigationCommand(investigation);
                var createResult = await Mediator.SendAsync(command, CancellationToken.None);

                if (createResult.IsSuccess)
                {
                    var newInvestigation = createResult.Value;
                    ShowSuccessNotification($"Investigation {newInvestigation.Code} created successfully");
                    var navigationUrl = $"/SMSRiskManagement/Investigations/{newInvestigation.Code}/{ReportHazard.Code}";
                    Logger.LogInformation("Navigating to new investigation: {Url}", navigationUrl);
                    await Task.Delay(1500);
                    Navigation.NavigateTo(navigationUrl);
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
            ShowErrorNotification("Error handling investigation. Please try again.");
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
            ShowSuccessNotification("Validation completed. Report remains open for further review.");
            await Task.Delay(1500);
            Navigation.NavigateTo("/SMSRiskManagement/ReportProcessing");
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
            ReportDetails.Status = "Closed";
            ReportDetails.UpdatedBy = GetCurrentUserCode();
            ReportDetails.UpdatedDate = DateTime.UtcNow;

            var updateCommand = new UpdateReportCommand(ReportDetails);
            var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification("Report has been closed successfully");
                Logger.LogInformation("Report {ReportId} closed due to NOT_SMS_RISK validation", ReportId);
                await Task.Delay(1500);
                Navigation.NavigateTo("/SMSRiskManagement/ReportProcessing");
            }
            else
            {
                throw new Exception($"Failed to close report: {result.Error?.Message}");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error closing report {ReportId}", ReportId);
            ShowErrorNotification($"Error closing report: {ex.Message}");
        }
    }

    /// <summary>
    /// Get current user code from HttpContext
    /// </summary>
    private string GetCurrentUserCode()
    {
        return "SYSTEM_USER"; // Replace with actual user identification logic
    }

    #endregion

    #region Validation Entity Management

    private SMS_Domain.Entities.ReportValidation CreateValidationEntity()
    {
        if (IsUpdate && ExistingValidation != null)
        {
            var validation = ExistingValidation;
            validation.ValidationDecision = SelectedValidationDecision?.Value; // Store the enum Value
            validation.ValidationComments = ValidationComments;
            validation.ValidationType = ValidationType;
            validation.ValidatedBy = string.IsNullOrEmpty(ValidatedBy) ? "SYSTEM" : ValidatedBy;
            validation.Status = "Completed";
            
            return validation;
        }
        else
        {
            var validatedByValue = string.IsNullOrEmpty(ValidatedBy) ? "SYSTEM" : ValidatedBy;
            var validation = SMS_Domain.Entities.ReportValidation.Create(ReportId, validatedByValue);
            validation.ValidationDecision = SelectedValidationDecision?.Value; // Store the enum Value
            validation.ValidationComments = ValidationComments;
            validation.ValidationType = ValidationType;
            validation.Status = "Completed";
            
            return validation;
        }
    }

    #endregion

    #region UI Helper Methods

    private string GetValidationCardStyle(string decisionValue)
    {
        //var baseStyle = "border: 2px solid var(--rz-border-color);";
        var baseStyle = "border: 2px solid; color:black;";

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

    private BadgeStyle GetStatusBadgeStyle()
    {
        return CurrentStatus switch
        {
            "Completed" => BadgeStyle.Success,
            "Draft" => BadgeStyle.Warning,
            _ => BadgeStyle.Success
        };
    }

    #endregion

    #region Notifications

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

    #region Models

    public class DropdownOption
    {
        public string Value { get; set; } = "";
        public string Text { get; set; } = "";
    }

    #endregion
}