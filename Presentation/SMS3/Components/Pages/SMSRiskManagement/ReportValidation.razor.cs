using Microsoft.AspNetCore.Components;
using Radzen;
using SMS_Application.Messaging.Queries;
using SMS_Application.Messaging.Commands;
using SMS_Application.Interfaces;
using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Domain.Common;
using SMS_Domain.Errors;
using SMS_Shared.Common;

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
            var usersQuery = new GetAllSMSApplicationUsersQuery();
            var usersResult = await Mediator.SendAsync(usersQuery, CancellationToken.None);
            if (usersResult.IsSuccess)
            {
                AvailableAssessors = usersResult.Value?.ToList() ?? new List<SMSApplicationUser>();
                
                // Build dropdown options
                ValidatedByOptions = new List<DropdownOption>
                {
                    new DropdownOption { Value = "", Text = "Auto-assign based on workload" }
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
                new DropdownOption { Value = "", Text = "Auto-assign based on workload" }
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

            if (SelectedValidationDecision?.ShouldProceedToAssessment == true)
            {
                // This is an SMS Risk - proceed to assessment
                await ProceedToAssessmentAsync();
            }
            else
            {
                // This is Not SMS Risk or Needs Investigation - submit validation
                await SubmitValidationAsync();
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
    /// Submit validation decision (for NOT_SMS_RISK and NEEDS_INVESTIGATION)
    /// </summary>
    private async Task SubmitValidationAsync()
    {
        try
        {
            Logger.LogInformation("Submitting validation decision for ReportId: {ReportId}", ReportId);

            // Validate required fields
            if (string.IsNullOrWhiteSpace(ValidationComments))
            {
                ShowErrorNotification("Validation comments are required");
                return;
            }

            if (SelectedValidationDecision == null)
            {
                ShowErrorNotification("Please select a validation decision");
                return;
            }

            // Create validation record using proper domain entity
            var validationId = new ReportValidationID($"RV-0000");
            
            var validation = new SMS_Domain.Entities.ReportValidation(validationId)
            {
                Code = validationId.Value,
                ReportCode = ReportId,
                ValidatedBy = GetCurrentUserCode(),
                ValidationDecision = ValidationDecisionValue,
                ValidationComments = ValidationComments,
                ValidationType = ValidationType ?? "Standard",
                Status = "Completed",
                Stage = "Complete",
                ValidatedDate = DateTime.UtcNow,
                CreatedBy = GetCurrentUserCode(),
                CreatedDate = DateTime.UtcNow
            };

            var createCommand = new CreateReportValidationCommand(validation);
            var result = await Mediator.SendAsync(createCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                // Check if this needs investigation
                if (SelectedValidationDecision.Value == "NEEDS_INVESTIGATION")
                {
                    await CreateAndNavigateToInvestigation();
                }
                else
                {
                    ShowSuccessNotification($"Validation submitted successfully. Decision: {SelectedValidationDecision.Name}");
                    
                    // Navigate back to processing queue
                    await Task.Delay(1500); // Give user time to see the success message
                    Navigation.NavigateTo("/SMSRiskManagement/ReportProcessing");
                }
            }
            else
            {
                ShowErrorNotification($"Failed to submit validation: {result.Error?.Message}");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error submitting validation for ReportId: {ReportId}", ReportId);
            ShowErrorNotification("Error submitting validation. Please try again.");
        }
    }

    /// <summary>
    /// Create investigation and navigate to Investigation page
    /// </summary>
    private async Task CreateAndNavigateToInvestigation()
    {
        try
        {
            if (ReportHazard == null)
            {
                ShowErrorNotification("Cannot create investigation - hazard information not found");
                return;
            }

            // Create investigation for the hazard
            var investigationResult = SMS_Domain.Entities.Investigation.CreateForHazard(
                ReportHazard.Code, 
                GetCurrentUserCode(), 
                ReportId);

            if (investigationResult.IsSuccess)
            {
                var investigation = investigationResult.Value;
                investigation.InvestigationObjectives = $"Investigation required based on validation decision for hazard {ReportHazard.Code}";
                investigation.InvestigationNotes = $"Investigation initiated from report validation. Validation comments: {ValidationComments}";

                var createCommand = new CreateInvestigationCommand(investigation);
                var result = await Mediator.SendAsync(createCommand, CancellationToken.None);

                if (result.IsSuccess)
                {
                    ShowSuccessNotification($"Investigation {investigation.Code} created successfully. Proceeding to investigation...");
                    
                    // Navigate to investigation page
                    await Task.Delay(1500);
                    var navigationUrl = $"/SMSRiskManagement/Investigation/{investigation.Code}/{ReportHazard.Code}";
                    
                    Logger.LogInformation("Navigating to investigation: {Url}", navigationUrl);
                    Navigation.NavigateTo(navigationUrl);
                }
                else
                {
                    ShowErrorNotification($"Failed to create investigation: {result.Error?.Message}");
                }
            }
            else
            {
                ShowErrorNotification($"Failed to create investigation: {investigationResult.Error?.Message}");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating investigation for ReportId: {ReportId}", ReportId);
            ShowErrorNotification("Error creating investigation. Please try again.");
        }
    }

    /// <summary>
    /// Get current user code from HttpContext
    /// </summary>
    private string GetCurrentUserCode()
    {
        // Implement your user identification logic here
        // For now, return a placeholder
        return "SYSTEM_USER"; // Replace with actual user identification logic
    }

    /// <summary>
    /// Proceed to risk assessment (for SMS_RISK decisions)
    /// </summary>
    private async Task ProceedToAssessmentAsync()
    {
        try
        {
            Logger.LogInformation("Proceeding to risk assessment for ReportId: {ReportId}", ReportId);

            // Validate required fields
            if (string.IsNullOrWhiteSpace(ValidationComments))
            {
                ShowErrorNotification("Validation comments are required");
                return;
            }

            if (SelectedValidationDecision == null)
            {
                ShowErrorNotification("Please select a validation decision");
                return;
            }

            // Create validation record
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

            if (result.IsSuccess)
            {
                ShowSuccessNotification($"Validation completed. Proceeding to {ValidationType} Assessment...");
                
                // Show Airport Shared Dataset dialog before proceeding to assessment
                await ShowAirportSharedDatasetDialog();
            }
            else
            {
                ShowErrorNotification($"Failed to create validation: {result.Error?.Message}");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error proceeding to assessment for ReportId: {ReportId}", ReportId);
            ShowErrorNotification("Error proceeding to assessment. Please try again.");
        }
    }

    /// <summary>
    /// Show dialog asking if user wants to create an Airport Shared Dataset
    /// </summary>
    private async Task ShowAirportSharedDatasetDialog()
    {
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
            await NavigateToAssessment();
        }
    }

    /// <summary>
    /// Navigate to the appropriate risk assessment page
    /// </summary>
    private async Task NavigateToAssessment()
    {
        var assessmentType = ValidationType?.ToLower() switch
        {
            "technical" => "TechnicalAssessment",
            "preliminary" => "PreliminaryAssessment", 
            _ => "TechnicalAssessment"
        };

        // Simple, reliable routing - use Report ID directly
        string navigationUrl;
        if (ReportHazard != null)
        {
            // Pass Report ID as AssessmentId and Hazard ID as second parameter
            // /SMSRiskManagement/PreliminaryAssessment/{AssessmentId}/{HazardId}
            navigationUrl = $"/SMSRiskManagement/{assessmentType}/{ReportId}/{ReportHazard.Code}";
        }
        else
        {
            // Just Report ID if no hazard
            // /SMSRiskManagement/PreliminaryAssessment/{AssessmentId}
            navigationUrl = $"/SMSRiskManagement/{assessmentType}/{ReportId}";
        }

        // Add a slight delay to show the success message
        await Task.Delay(1500);
        
        Logger.LogInformation("Navigating to assessment: {Url} (ReportId: {ReportId}, HazardId: {HazardId})", 
            navigationUrl, ReportId, ReportHazard?.Code ?? "None");
        Navigation.NavigateTo(navigationUrl);
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

    private async Task<Result<SMS_Domain.Entities.ReportValidation>> SaveValidationAsync(SMS_Domain.Entities.ReportValidation validationEntity)
    {
        try
        {
            if (IsUpdate && ExistingValidation != null)
            {
                var updateCommand = new UpdateReportValidationCommand(validationEntity);
                return await Mediator.SendAsync(updateCommand, CancellationToken.None);
            }
            else
            {
                var createCommand = new CreateReportValidationCommand(validationEntity);
                return await Mediator.SendAsync(createCommand, CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving validation entity");
            return Result<SMS_Domain.Entities.ReportValidation>.Failure<SMS_Domain.Entities.ReportValidation>(
                DomainErrors.ReportValidationError.CreateFailed);
        }
    }

    #endregion

    #region UI Helper Methods

    private string GetValidationCardStyle(string decisionValue)
    {
        var baseStyle = "border: 2px solid var(--rz-border-color);";
        
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
            _ => BadgeStyle.Info
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