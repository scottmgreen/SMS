using Microsoft.AspNetCore.Components;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Application.Messaging.Queries;
using SMS_Application.Messaging.Commands;
using SMS_Application.Interfaces;
using SMS_Shared.Common;
using Radzen;

namespace SMS3.Components.Pages.SMSRiskManagement;

/// <summary>
/// Code-behind for Airport Shared Dataset creation page
/// Based on ADAM (Airport Data Aggregation and Monitoring) template
/// Handles comprehensive dataset creation for SMS compliance
/// </summary>
public partial class AirportSharedDataset : ComponentBase
{
    #region Injected Services
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private ILogger<AirportSharedDataset> Logger { get; set; } = default!;
    #endregion

    #region Parameters
    [Parameter] public string ReportId { get; set; } = default!;
    [Parameter] public string? HazardId { get; set; }
    #endregion

    #region State Properties
    private bool IsLoading { get; set; } = true;
    private bool IsSaving { get; set; } = false;
    
    public AirportSharedDatasetModel Model { get; set; } = new();
    public Report? ReportDetails { get; set; }
    public Hazard? HazardDetails { get; set; }
    #endregion

    #region Dropdown Options
    private readonly List<DropdownOption> LocationOptions = new()
    {
        new() { Value = "Movement area", Text = "Movement area" },
        new() { Value = "Ramp", Text = "Ramp" },
        new() { Value = "Baggage tunnel", Text = "Baggage tunnel" },
        new() { Value = "Fuel farm", Text = "Fuel farm" },
        new() { Value = "Other", Text = "Other" }
    };

    private readonly List<DropdownOption> YesNoNAOptions = new()
    {
        new() { Value = "N/A", Text = "N/A" },
        new() { Value = "Unknown", Text = "Unknown" },
        new() { Value = "No", Text = "No" },
        new() { Value = "Yes", Text = "Yes" }
    };
    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }
    #endregion

    #region Data Loading
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;

            if (string.IsNullOrWhiteSpace(ReportId))
            {
                ShowErrorNotification("Report ID is required for dataset creation");
                Navigation.NavigateTo("/SMSRiskManagement/ReportProcessing");
                return;
            }

            // Load report details
            var reportQuery = new GetReportByIdQuery(new ReportID(ReportId));
            var reportResult = await Mediator.SendAsync(reportQuery, CancellationToken.None);
            
            if (reportResult.IsSuccess)
            {
                ReportDetails = reportResult.Value;
                
                // Initialize model with report data
                Model.DateTime = ReportDetails.CreatedDate ?? DateTime.Now;
                Model.PrivateNarrative = ReportDetails.Description;
                // Set other relevant fields from report
            }

            // Load hazard details if HazardId provided
            if (!string.IsNullOrWhiteSpace(HazardId))
            {
                var hazardQuery = new GetHazardByIdQuery(new HazardID(HazardId));
                var hazardResult = await Mediator.SendAsync(hazardQuery, CancellationToken.None);
                
                if (hazardResult.IsSuccess)
                {
                    HazardDetails = hazardResult.Value;
                    
                    // Initialize model with hazard data
                    Model.Location = HazardDetails.LocationArea ?? "";
                    Model.SharedNarrative = HazardDetails.Description ?? "";
                    // Set other relevant fields from hazard
                }
            }

            Logger.LogInformation("Loaded dataset creation page for Report: {ReportId}, Hazard: {HazardId}", 
                ReportId, HazardId ?? "None");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading dataset creation page for Report: {ReportId}", ReportId);
            ShowErrorNotification("Error loading dataset creation page");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }
    #endregion

    #region Form Events
    private void OnLocationChange(object value)
    {
        if (value?.ToString() != "Other")
        {
            Model.LocationOther = null;
        }
        StateHasChanged();
    }

    private void OnFlightDelayChange(object value)
    {
        if (value?.ToString() != "Yes")
        {
            Model.FlightDelayDetails = null;
        }
        StateHasChanged();
    }

    private void OnEquipmentRemovedChange(object value)
    {
        if (value?.ToString() != "Yes")
        {
            Model.EquipmentRemovalDetails = null;
        }
        StateHasChanged();
    }

    private void OnPoliceReportChange(object value)
    {
        if (value?.ToString() != "Yes")
        {
            Model.PoliceReportDetails = null;
        }
        StateHasChanged();
    }
    #endregion

    #region Form Actions
    private async Task HandleFormSubmit(AirportSharedDatasetModel model)
    {
        await SaveDataset();
    }

    private async Task SaveDataset()
    {
        try
        {
            IsSaving = true;
            StateHasChanged();

            // Validate required fields
            if (!ValidateForm())
            {
                return;
            }

            // Create AirportSharedDataset entity
            var datasetId = new AirportSharedDatasetID($"ADAM-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}");
            var dataset = new SMS_Domain.Entities.AirportSharedDataset(datasetId)
            {
                Code = datasetId.Value,
                ReportCode = ReportId,
                HazardCode = HazardId,
                PrivateNarrative = Model.PrivateNarrative,
                SharedNarrative = Model.SharedNarrative,
                LocationArea = Model.Location == "Other" ? null : Model.Location,
                LocationOther = Model.Location == "Other" ? Model.LocationOther : null,
                Weather = Model.Weather,
                TriggeringEvent = Model.TriggeringEvent,
                AirlineCompanyOperator = Model.AirlineCompanyOperator,
                OperatorsAuthorized = Model.OperatorsAuthorized,
                FlightDelay = Model.FlightDelay,
                FlightDelayDetails = Model.FlightDelayDetails,
                EquipmentRemovedFromService = Model.EquipmentRemovedFromService,
                EquipmentRemovalDetails = Model.EquipmentRemovalDetails,
                PoliceReport = Model.PoliceReport,
                PoliceReportDetails = Model.PoliceReportDetails,
                ContributingFactors = BuildContributingFactorsString(),
                FactorsOtherDescription = Model.OtherFactor ? Model.FactorsOtherDescription : null,
                AircraftInvolved = Model.AircraftInvolved,
                PoweredEquipmentInvolved = Model.PoweredEquipmentInvolved,
                NonPoweredEquipmentInvolved = Model.NonPoweredEquipmentInvolved,
                PedestrianInvolved = Model.PedestrianInvolved,
                OtherInvolved = Model.OtherInvolved,
                OtherDescription = Model.OtherInvolved ? Model.OtherDescription : null,
                PropertyDamage = Model.PropertyDamage,
                PropertyDamageComments = Model.PropertyDamage ? Model.PropertyDamageComments : null,
                PersonalInjury = Model.PersonalInjury,
                PersonalInjuryComments = Model.PersonalInjury ? Model.PersonalInjuryComments : null,
                Fatality = Model.Fatality,
                FatalityComments = Model.Fatality ? Model.FatalityComments : null,
                OtherIssues = Model.OtherIssues,
                OtherIssuesDescription = Model.OtherIssues ? Model.OtherIssuesDescription : null
            };

            // Save using CQRS
            var createCommand = new CreateAirportSharedDatasetCommand(dataset);
            var result = await Mediator.SendAsync(createCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification($"Airport Shared Dataset {dataset.Code} created successfully!");
                Logger.LogInformation("Created Airport Shared Dataset: {DatasetId} for Report: {ReportId}", 
                    dataset.Code, ReportId);
            }
            else
            {
                ShowErrorNotification($"Failed to create dataset: {result.Error?.Message}");
                Logger.LogError("Failed to create Airport Shared Dataset for Report: {ReportId}, Error: {Error}", 
                    ReportId, result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving Airport Shared Dataset for Report: {ReportId}", ReportId);
            ShowErrorNotification("Error saving dataset");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    private async Task SaveAndContinue()
    {
        await SaveDataset();
        
        // Navigate to appropriate assessment after successful save
        if (!IsSaving) // Only navigate if save was successful
        {
            await NavigateToAssessment();
        }
    }

    private async Task CancelAndReturnToAssessment()
    {
        await NavigateToAssessment();
    }

    private async Task NavigateToAssessment()
    {
        // Determine assessment type from route or default to Technical
        var assessmentType = "TechnicalAssessment"; // Default - could be passed as parameter
        
        string navigationUrl;
        if (!string.IsNullOrEmpty(HazardId))
        {
            navigationUrl = $"/SMSRiskManagement/{assessmentType}/{ReportId}/{HazardId}";
        }
        else
        {
            navigationUrl = $"/SMSRiskManagement/{assessmentType}/{ReportId}";
        }

        Logger.LogInformation("Navigating to assessment: {Url}", navigationUrl);
        Navigation.NavigateTo(navigationUrl);
    }
    #endregion

    #region Validation
    private bool ValidateForm()
    {
        if (Model.DateTime == default)
        {
            ShowErrorNotification("Date and Time is required");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Model.Location))
        {
            ShowErrorNotification("Location is required");
            return false;
        }

        if (Model.Location == "Other" && string.IsNullOrWhiteSpace(Model.LocationOther))
        {
            ShowErrorNotification("Please specify the other location");
            return false;
        }

        return true;
    }
    #endregion

    #region Helper Methods
    private string BuildContributingFactorsString()
    {
        var factors = new List<string>();
        
        if (Model.Fatigue) factors.Add("Fatigue");
        if (Model.Speed) factors.Add("Speed");
        if (Model.WeatherFactor) factors.Add("Weather");
        if (Model.Lighting) factors.Add("Lighting");
        if (Model.Signage) factors.Add("Signage/Markings");
        if (Model.Training) factors.Add("Training");
        if (Model.Construction) factors.Add("Construction");
        if (Model.Administrative) factors.Add("Administrative Controls");
        if (Model.HumanFactors) factors.Add("Human Factors");
        if (Model.Engineering) factors.Add("Engineering");
        if (Model.Mechanical) factors.Add("Mechanical");
        if (Model.OtherFactor) factors.Add("Other");

        return string.Join(", ", factors);
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

    #region Data Models
    public class AirportSharedDatasetModel
    {
        // General Information
        public DateTime DateTime { get; set; } = DateTime.Now;
        public string Location { get; set; } = "";
        public string? LocationOther { get; set; }
        public string? Weather { get; set; }
        public string? PrivateNarrative { get; set; }
        public string? SharedNarrative { get; set; }

        // Triggering Event
        public string? TriggeringEvent { get; set; }

        // Contributing Factors
        public bool Fatigue { get; set; }
        public bool Speed { get; set; }
        public bool WeatherFactor { get; set; }
        public bool Lighting { get; set; }
        public bool Signage { get; set; }
        public bool Training { get; set; }
        public bool Construction { get; set; }
        public bool Administrative { get; set; }
        public bool HumanFactors { get; set; }
        public bool Engineering { get; set; }
        public bool Mechanical { get; set; }
        public bool OtherFactor { get; set; }
        public string? FactorsOtherDescription { get; set; }

        // Operational Impact
        public string? AirlineCompanyOperator { get; set; }
        public string OperatorsAuthorized { get; set; } = "N/A";
        public string FlightDelay { get; set; } = "N/A";
        public string? FlightDelayDetails { get; set; }
        public string EquipmentRemovedFromService { get; set; } = "N/A";
        public string? EquipmentRemovalDetails { get; set; }
        public string PoliceReport { get; set; } = "N/A";
        public string? PoliceReportDetails { get; set; }

        // Advanced Options - Involved Components
        public bool AircraftInvolved { get; set; }
        public bool PoweredEquipmentInvolved { get; set; }
        public bool NonPoweredEquipmentInvolved { get; set; }
        public bool PedestrianInvolved { get; set; }
        public bool OtherInvolved { get; set; }
        public string? OtherDescription { get; set; }

        // Advanced Options - Resulting Issues
        public bool PropertyDamage { get; set; }
        public string? PropertyDamageComments { get; set; }
        public bool PersonalInjury { get; set; }
        public string? PersonalInjuryComments { get; set; }
        public bool Fatality { get; set; }
        public string? FatalityComments { get; set; }
        public bool OtherIssues { get; set; }
        public string? OtherIssuesDescription { get; set; }
    }

    public class DropdownOption
    {
        public string Value { get; set; } = "";
        public string Text { get; set; } = "";
    }
    #endregion
}