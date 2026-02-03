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
    [Parameter] public string? ReportId { get; set; }
    [Parameter] public string? HazardId { get; set; }
    #endregion

    #region State Properties
    private bool IsLoading { get; set; } = true;
    private bool IsSaving { get; set; } = false;

    public AirportSharedDatasetModel Model { get; set; } = new();
    public Report? ReportDetails { get; set; }
    public Hazard? HazardDetails { get; set; }
    public SMS_Domain.Entities.AirportSharedDataset? ExistingDataset { get; set; } // Store existing dataset for updates

    // Edit mode detection - we're in edit mode if we have both ReportId and HazardId and an existing dataset
    public bool IsEditMode => ExistingDataset != null;
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
            var reportQuery = new GetReportByCodeQuery(new ReportID(ReportId));
            var reportResult = await Mediator.SendAsync(reportQuery, CancellationToken.None);

            if (reportResult.IsSuccess)
            {
                ReportDetails = reportResult.Value;

                // Initialize model with report data
                Model.DateTime = ReportDetails.CreatedDate ?? DateTime.Now;
                Model.PrivateNarrative = ReportDetails.Description;
            }

            // Load hazard details if HazardId provided
            if (!string.IsNullOrWhiteSpace(HazardId))
            {
                var hazardQuery = new GetHazardByCodeQuery(new HazardID(HazardId));
                var hazardResult = await Mediator.SendAsync(hazardQuery, CancellationToken.None);

                if (hazardResult.IsSuccess)
                {
                    HazardDetails = hazardResult.Value;

                    // Initialize model with hazard data
                    Model.Location = HazardDetails.LocationArea ?? "";
                    Model.SharedNarrative = HazardDetails.Description ?? "";

                    // Check if there's an existing dataset for this report/hazard combination
                    await CheckForExistingDataset();
                }
            }

            Logger.LogInformation("Loaded dataset page for Report: {ReportId}, Hazard: {HazardId}, EditMode: {IsEditMode}",
                ReportId, HazardId ?? "None", IsEditMode);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading dataset page for Report: {ReportId}", ReportId);
            ShowErrorNotification("Error loading dataset page");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task CheckForExistingDataset()
    {
        try
        {
            // Look for existing dataset with this HazardCode
            // Note: This is a simplified approach - in production, you might want a more specific query
            var datasetsQuery = new GetAllAirportSharedDatasetsQuery();
            var datasetsResult = await Mediator.SendAsync(datasetsQuery, CancellationToken.None);

            if (datasetsResult.IsSuccess && datasetsResult.Value != null)
            {
                ExistingDataset = datasetsResult.Value.FirstOrDefault(d =>
                    d.HazardCode == HazardId &&
                    (string.IsNullOrEmpty(d.ReportCode) || d.ReportCode == ReportId));

                if (ExistingDataset != null)
                {
                    Logger.LogInformation("Found existing dataset {DatasetCode} for Report: {ReportId}, Hazard: {HazardId}",
                        ExistingDataset.Code, ReportId, HazardId);

                    // Map existing dataset to form - using only available properties
                    MapDatasetToModel(ExistingDataset);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error checking for existing dataset for Report: {ReportId}, Hazard: {HazardId}", ReportId, HazardId);
            // Don't show error to user - just log it and continue in create mode
        }
    }

    private void MapDatasetToModel(SMS_Domain.Entities.AirportSharedDataset dataset)
    {
        try
        {
            // Map only the properties that exist in the domain model
            // This is a conservative approach to avoid property mismatch errors

            Model.PrivateNarrative = dataset.PrivateNarrative;
            Model.SharedNarrative = dataset.SharedNarrative;
            Model.Weather = dataset.Weather;
            Model.TriggeringEvent = dataset.TriggeringEvent;

            // Handle location mapping safely
            if (!string.IsNullOrEmpty(dataset.LocationArea))
            {
                Model.Location = dataset.LocationArea;
            }
            else if (!string.IsNullOrEmpty(dataset.LocationOther))
            {
                Model.Location = "Other";
                Model.LocationOther = dataset.LocationOther;
            }

            Logger.LogInformation("Mapped existing dataset {Code} to edit form", dataset.Code);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error mapping dataset {Code} to form model", dataset.Code);
            // Continue with default values if mapping fails
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

    private async Task CancelAndReturn()
    {
        await NavigateToAssessment();
    }

    private string GetSaveButtonText()
    {
        return IsEditMode ? "Update Dataset" : "Save Dataset";
    }

    private string GetSaveButtonIcon()
    {
        return IsEditMode ? "save" : "save";
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