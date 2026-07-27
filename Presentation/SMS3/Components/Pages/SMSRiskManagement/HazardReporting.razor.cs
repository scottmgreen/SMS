
using Microsoft.JSInterop;
using Microsoft.AspNetCore.WebUtilities;

using SMS_Application.Common;

using SMS_Domain.Errors;
using SMS_Domain.Events;

using SMS3.Components.Pages.SMSRiskManagement.Models;
using SMS3.Components.Pages.SMSSystem.Components;
using SMS3.Components.Pages.SMSSystem.Models;
using SMS3.Components.Shared.UIHelpers;
using SMS3.Configuration.Extensions;

using System.Net;

namespace SMS3.Components.Pages.SMSRiskManagement;

/// <summary>
/// Hazard Reporting page - Full functionality with JavaScript mapping integration
/// Supports both CREATE and EDIT modes for comprehensive hazard reporting
/// </summary>
public partial class HazardReporting : ComponentBase, IDisposable
{
    #region Dependencies
    [Inject] private ICurrentUserService _currentUserService { get; set; } = default!;
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<HazardReporting> _logger { get; set; } = default!;
    [Inject] private DialogService _dialogService { get; set; } = default!;
    [Inject] private IBaseEventBus _eventBus { get; set; } = default!;

    [Inject] private IJSRuntime _jsRuntime { get; set; } = default!;
    [Inject] private NavigationManager _navigation { get; set; } = default!;
    [Inject] private IConfiguration _configuration { get; set; } = default!;
    #endregion

    #region Route Parameters

    /// <summary>
    /// Optional hazard code parameter for editing existing hazards
    /// </summary>
    [Parameter] public string? HazardCode { get; set; }
    [Parameter]
    [SupplyParameterFromQuery(Name = "returnTo")]
    public string? ReturnTo { get; set; }

    #endregion

    #region Properties and Fields

    /// <summary>
    /// Main form data object
    /// </summary>
    public HazardReportForm HazardReport { get; set; } = new();

    /// <summary>
    /// Geographic location data
    /// </summary>
    public HazardLocation SelectedGeoLocation { get; set; } = new();

    /// <summary>
    /// File selection for attachments
    /// </summary>
    public IReadOnlyList<IBrowserFile> SelectedFiles { get; set; } = new List<IBrowserFile>();

    /// <summary>
    /// Processed file attachments for display
    /// </summary>
    public List<AttachedFile> AttachedFiles { get; set; } = new();

    /// <summary>
    /// Loading state indicator
    /// </summary>
    public bool IsLoading { get; set; }

    public string EmailValidationMessage { get; set; } = string.Empty;

    /// <summary>
    /// Show map modal
    /// </summary>
    public bool ShowMapModal { get; set; }

    /// <summary>
    /// Show submission confirmation modal
    /// </summary>
    public bool ShowSubmissionConfirmation { get; set; }

    /// <summary>
    /// Success confirmation display state
    /// </summary>
    public bool ShowFinalSuccessConfirmation { get; set; }

    /// <summary>
    /// Show confidential reporting details
    /// </summary>
    public bool ShowConfidentialInfo { get; set; }

    /// <summary>
    /// Generated Report ID after successful submission
    /// </summary>
    public string? GeneratedReportId { get; set; }

    /// <summary>
    /// Generated Hazard ID after successful submission
    /// </summary>
    public string? GeneratedHazardId { get; set; }

    /// <summary>
    /// Generated Tracking ID after successful submission
    /// </summary>
    public string? GeneratedTrackingId { get; set; }

    /// <summary>
    /// Submission timestamp
    /// </summary>
    public DateTime? SubmissionDateTime { get; set; }

    /// <summary>
    /// Map coordinates
    /// </summary>
    public decimal SelectedLatitude { get; set; }
    public decimal SelectedLongitude { get; set; }
    public string SelectedLocationDescription { get; set; } = string.Empty;

    /// <summary>
    /// Hazard category dropdown options
    /// </summary>
    public List<DropdownOption> HazardCategoryOptions { get; set; } = new();

    /// <summary>
    /// Hazard type dropdown options (filtered by selected category)
    /// </summary>
    public List<DropdownOption> HazardTypeOptions { get; set; } = new();


    public List<DropdownOption> DepartmentOptions { get; set; } = new();    

    ///<summary>
    /// Department List
    /// </summary>
    public string? SelectedDepartment { get; set; } 

    /// <summary>
    /// Currently selected hazard category
    /// </summary>
    public string? SelectedHazardCategory { get; set; }

    /// <summary>
    /// Edit mode flag - true when editing an existing report
    /// </summary>
    public bool IsEditMode { get; set; }

    /// <summary>
    /// Report code when in edit mode
    /// </summary>
    public string? EditReportCode { get; set; }

    /// <summary>
    /// Original report being edited (if in edit mode)
    /// </summary>
    public Report? EditingReport { get; set; }

    /// <summary>
    /// Original hazard being edited (if in edit mode)
    /// </summary>
    public Hazard? EditingHazard { get; set; }

    /// <summary>
    /// Hazard code when in edit mode
    /// </summary>
    public string? EditHazardCode { get; set; }

    // Computed Properties
    public string SelectedLatitudeText => SelectedLatitude != 0 ? SelectedLatitude.ToString("F6") : "";
    public string SelectedLongitudeText => SelectedLongitude != 0 ? SelectedLongitude.ToString("F6") : "";
    public string LocationDisplayText => HasGeoLocation ? GetSelectedLocationText() : "No location selected";
    public bool HasGeoLocation =>
        SelectedGeoLocation is not null &&
        SelectedGeoLocation.Latitude.HasValue &&
        SelectedGeoLocation.Longitude.HasValue &&
        SelectedGeoLocation.IsValidated;
    private bool HasSelectedCoordinates => SelectedLatitude != 0 && SelectedLongitude != 0;
    private bool HasEditingCoordinates =>
        (EditingHazard?.HazardLocation?.Latitude ?? 0) != 0 &&
        (EditingHazard?.HazardLocation?.Longitude ?? 0) != 0;

    private HazardLocation? ActiveLocationForValidation =>
        HasSelectedCoordinates ? SelectedGeoLocation :
        (IsEditMode && HasEditingCoordinates ? EditingHazard?.HazardLocation : null);

    public bool RequiresLocationValidation => ActiveLocationForValidation?.IsValidated == false;
    public bool IsMapReadOnlyMode => IsEditMode && HasGeoLocation && !RequiresLocationValidation && !_isMapEditModeEnabled;
    public string LocationSelectionButtonText =>
        IsMapReadOnlyMode
            ? "View Location"
            : (RequiresLocationValidation ? "Validate Location" : "Select Location");
    public bool IsLocationValidationMode => IsEditMode && RequiresLocationValidation;
    public string MapModalTitle =>
        IsMapReadOnlyMode
            ? $"View Hazard Location {ActiveLocationForValidation?.Code}"
            : (IsLocationValidationMode ? $"Confirm Hazard Location {ActiveLocationForValidation?.Code}" : "Select Hazard Location");
    public string MapModalConfirmButtonText => IsLocationValidationMode ? "Confirm Location" : "Use Selected Location";
    public bool CanClearMapSelection => !IsLocationValidationMode && !IsMapReadOnlyMode;
    public string? MapModalLocationCaption
    {
        get
        {
            
            var locationCode = ActiveLocationForValidation?.Code;
            return !string.IsNullOrWhiteSpace(locationCode)
                ? $"Hazard Location: {locationCode}"
                : null;
        }
    }
    public bool HasValidCoordinates => SelectedLatitude != 0 && SelectedLongitude != 0;
    public string GeoLocationDisplay => HasGeoLocation ? $"Lat: {SelectedGeoLocation.Latitude:F6}, Lng: {SelectedGeoLocation.Longitude:F6}" : "No coordinates selected";
    public int HazardTitleCharacterCount => HazardReport?.HazardTitle?.Length ?? 0;
    public int DescriptionCharacterCount => HazardReport?.Description?.Length ?? 0;
        
    
    public bool IsFormValidForPreview =>
        !string.IsNullOrEmpty(HazardReport.HazardType) && !string.IsNullOrEmpty(HazardReport.HazardCategory) &&
        !string.IsNullOrEmpty(HazardReport.SubmittedBy) &&
        !string.IsNullOrWhiteSpace(HazardReport.HazardTitle) &&
        !string.IsNullOrEmpty(HazardReport.Description) && !string.IsNullOrEmpty(HazardReport.IncidentDateTime.ToString());

    public bool IsFormValidForSubmission ()
    {
        if (!HazardReport.IsAnonymous)
        {
            return IsFormValidForPreview &&
                !string.IsNullOrEmpty(HazardReport.ReportContactName) &&
                !string.IsNullOrEmpty(HazardReport.ReportContactEmail) &&
                (HasGeoLocation || !string.IsNullOrEmpty(HazardReport.Location)) && DescriptionCharacterCount <= 3000;
        }

        return IsFormValidForPreview &&
            (HasGeoLocation || !string.IsNullOrEmpty(HazardReport.Location)) && DescriptionCharacterCount <= 3000;
    }

    public void CancelEdit()
    {
        var returnTarget = ReturnTo;
        if (string.IsNullOrWhiteSpace(returnTarget))
        {
            var uri = _navigation.ToAbsoluteUri(_navigation.Uri);
            var query = QueryHelpers.ParseQuery(uri.Query);
            if (query.TryGetValue("returnTo", out var returnValues))
            {
                returnTarget = returnValues.FirstOrDefault();
            }
        }

        if (string.Equals(returnTarget, "hazard-listing", StringComparison.OrdinalIgnoreCase))
        {
            _navigation.NavigateToSecure("/SMSListings/Hazards");
            return;
        }

        if (string.Equals(returnTarget, "report-listing", StringComparison.OrdinalIgnoreCase))
        {
            _navigation.NavigateToSecure("/SMSListings/Reports");
            return;
        }

        if (string.Equals(returnTarget, "report-processing", StringComparison.OrdinalIgnoreCase))
        {
            _navigation.NavigateToSecure("/SMSRiskManagement/ReportProcessing");
            return;
        }

        if (string.Equals(returnTarget, "risk-registry", StringComparison.OrdinalIgnoreCase))
        {
            _navigation.NavigateToSecure("/SMSAssurance/RiskRegistry");
            return;
        }

        _navigation.NavigateToSecure("/SMSListings/Reports");
    }
        

    public string PageTitle => IsEditMode ? $"Edit Report - {EditReportCode}" : "Submit Hazard Report";
    public string PageSubtitle => IsEditMode ? "Modify existing hazard report information" : "Report safety hazards and incidents for SMS processing and risk assessment";

    /// <summary>
    /// Visual validation helpers for default values requiring attention
    /// </summary>
    public bool IsHazardCategoryDefault => HazardReport?.HazardCategory == HazardCategory.Default.Value;
    public bool IsHazardTypeDefault => HazardReport?.HazardType == HazardType.Default.Value;
    public bool IsHazardTitleMissing => string.IsNullOrWhiteSpace(HazardReport?.HazardTitle);
    public bool IsHazardTitleValidationAlert => IsEditMode && IsHazardTitleMissing;
    public bool HasDefaultHazardClassification => IsHazardCategoryDefault || IsHazardTypeDefault;
    public bool IsUpdateBlockedByValidation => IsEditMode && (RequiresLocationValidation || HasDefaultHazardClassification || IsHazardTitleMissing);
    public bool ShowEditValidationAlert => IsEditMode && (HasDefaultHazardClassification || RequiresLocationValidation || IsHazardTitleMissing);
    public string EditValidationAlertTitle =>
        HasDefaultHazardClassification && RequiresLocationValidation && IsHazardTitleMissing
            ? "⚠️ HAZARD CLASSIFICATION + LOCATION + TITLE VALIDATION REQUIRED"
            : HasDefaultHazardClassification && RequiresLocationValidation
                ? "⚠️ HAZARD CLASSIFICATION + LOCATION VALIDATION REQUIRED"
                : HasDefaultHazardClassification && IsHazardTitleMissing
                    ? "⚠️ HAZARD CLASSIFICATION + TITLE VALIDATION REQUIRED"
                    : RequiresLocationValidation && IsHazardTitleMissing
                        ? "⚠️ LOCATION + TITLE VALIDATION REQUIRED"
                        : HasDefaultHazardClassification
                            ? "⚠️ DEFAULT HAZARD CLASSIFICATION DETECTED"
                            : IsHazardTitleMissing
                                ? "⚠️ HAZARD TITLE REQUIRED"
                                : "⚠️ LOCATION VALIDATION REQUIRED";
    public string EditValidationAlertMessage =>
        HasDefaultHazardClassification && RequiresLocationValidation && IsHazardTitleMissing
            ? "Update Hazard Category/Type from defaults, provide a Hazard Title, and validate the selected location before updating this report."
            : HasDefaultHazardClassification && RequiresLocationValidation
                ? "Update Hazard Category/Type from defaults and validate the selected location before updating this report."
                : HasDefaultHazardClassification && IsHazardTitleMissing
                    ? "Update Hazard Category/Type from defaults and provide a Hazard Title before proceeding."
                    : RequiresLocationValidation && IsHazardTitleMissing
                        ? "Provide a Hazard Title and validate the selected location before updating this report."
                        : HasDefaultHazardClassification
                            ? "Please update the Hazard Category and Type to proper values before proceeding."
                            : IsHazardTitleMissing
                                ? "Please provide a Hazard Title before proceeding."
                                : "The selected map location is not validated yet. Click Validate Location and confirm to continue.";
    public bool IsExternalSystemSubmittedReport => IsEditMode && string.Equals(HazardReport?.SubmittedBy?.Trim(), "EXTERNAL_API_SOURCE", StringComparison.OrdinalIgnoreCase);

    // Airport coordinates //GOLDKEY
    private double _airportCenterLatitude => 45.58808;
    private double _airportCenterLongitude => -122.592430;
    private int _defaultZoomLevel => 14;

    private IJSObjectReference? _mapModule;
    private DotNetObjectReference<HazardReporting>? _dotNetRef;
    private bool _isMapEditModeEnabled;
    private decimal _mapOriginalLatitude;
    private decimal _mapOriginalLongitude;
    private string _mapOriginalLocationDescription = string.Empty;
    private HazardLocation _mapOriginalGeoLocation = new() { IsValid = false, IsValidated = false };
    private bool _mapSelectionConfirmed;
    private bool _mapSelectionCleared;

    #endregion

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        InitializeDropdownOptions();

        // Check for edit mode parameters
        await CheckForEditModeAsync();

        if (!IsEditMode)
        {
            InitializeFormDefaults();
            Console.Write(_currentUserService?.UserDisplayName);
        }

        // Create DotNet reference for JavaScript callbacks
        _dotNetRef = DotNetObjectReference.Create(this);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                // Initialize JavaScript mapping module only once
                _mapModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "/js/hazard-map.js");
                _logger.LogInformation("Map module loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load JavaScript map module");
            }
        }

        
    }

    public void Dispose()
    {
        _mapModule?.DisposeAsync();
        _dotNetRef?.Dispose();
    }

    #endregion

    #region Edit Mode Methods

    /// <summary>
    /// Check if we're in edit mode based on route parameters or query parameters
    /// </summary>
    private async Task CheckForEditModeAsync()
    {
        try
        {
            // Check if we have a route parameter (HazardCode)
            if (!string.IsNullOrEmpty(HazardCode))
            {
                // Special case: "new" means create mode
                if (HazardCode.Equals("new", StringComparison.OrdinalIgnoreCase))
                {
                    IsEditMode = false;
                    EditHazardCode = null;
                    EditingHazard = null;
                    EditReportCode = null;
                    EditingReport = null;
                    _logger.LogInformation("Create mode detected via route parameter 'new'");
                    return;
                }

                // Otherwise, treat as edit mode
                IsEditMode = true;
                EditHazardCode = HazardCode;

                _logger.LogInformation("Edit mode detected for hazard: {HazardCode} (via route parameter)", HazardCode);

                // Load the existing hazard data
                await LoadHazardForEditingAsync(HazardCode);
                return;
            }

            // Fallback to existing query parameter logic for backward compatibility
            var uri = new Uri(_navigation.Uri);
            var queryParams = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);

            if (queryParams.TryGetValue("mode", out var mode) && mode == "edit" &&
                queryParams.TryGetValue("reportCode", out var reportCode) && !string.IsNullOrEmpty(reportCode))
            {
                var reportCodeValue = reportCode.ToString();
                IsEditMode = true;
                EditReportCode = reportCodeValue;

                _logger.LogInformation("Edit mode detected for report: {ReportCode} (via query parameter)", reportCodeValue);

                // Load the existing report data
                await LoadReportForEditingAsync(reportCodeValue);
            }
            else
            {
                IsEditMode = false;
                EditReportCode = null;
                EditingReport = null;
                EditHazardCode = null;
                EditingHazard = null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking for edit mode");
            IsEditMode = false;
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Unable to determine edit mode. Defaulting to create mode."));

        }
    }

    /// <summary>
    /// Load report data for editing
    /// </summary>
    private async Task LoadReportForEditingAsync(string reportCode)
    {
        try
        {
            IsLoading = true;
            StateHasChanged();

            _logger.LogInformation("Loading report {ReportCode} for editing", reportCode);

            // Get the report details
            var reportQuery = new GetReportByCodeQuery(new ReportID(reportCode));
            var reportResult = await _mediator.SendAsync(reportQuery, CancellationToken.None);

            if (reportResult.IsFailure || reportResult.Value is null)
            {
                throw new InvalidOperationException($"Report {reportCode} not found");
            }

            EditingReport = reportResult.Value;

            // Get associated hazards to populate the form
            var hazardsQuery = new GetHazardsByReportCodeQuery(new ReportID(reportCode));
            var hazardsResult = await _mediator.SendAsync(hazardsQuery, CancellationToken.None);

            if (hazardsResult.IsSuccess && hazardsResult.Value?.Any() == true)
            {
                // Use the first hazard to populate the form (primary hazard)
                var primaryHazard = hazardsResult.Value.OrderBy(h => h.IsInitialHazard).First();

                // Store the editing hazard for update operations
                EditingHazard = primaryHazard;
                EditHazardCode = primaryHazard.Code;

                _logger.LogInformation("Found primary hazard {HazardCode} with type: {HazardType}", primaryHazard.Code, primaryHazard.HazardType);

                // Try to determine category from hazard type
                var hazardType = HazardType.FromValue(primaryHazard.HazardType ?? "");
                var category = hazardType is not null ? HazardCategory.FromValue(hazardType.Category) : null;

                _logger.LogInformation("Determined category: {Category} from hazard type: {HazardType}", category?.Value ?? "NULL", primaryHazard.HazardType);

                


                // STEP 1: Set the category first
                SelectedHazardCategory = category?.Value;

                // STEP 2: Populate form with basic hazard data
                HazardReport = new HazardReportForm
                {
                    IncidentDateTime = EditingReport.IncidentDateTime,
                    SubmittedBy = EditingReport.SubmittedBy,
                    SubmittedDate = EditingReport.SubmittedDate,
                    SubmittingDepartment = EditingReport.SubmittingDepartment,
                    SubmittingDepartmentJobFunction = EditingReport.SubmittingDepartmentJobFunction,
                    ReportContactName = EditingReport.ReportContactName,
                    ReportContactCell   = EditingReport.ReportContactCell,
                    ReportContactEmail = EditingReport.ReportContactEmail,
                    ReportContactCompany = EditingReport.ReportContactCompany,
                    IsAnonymous =   EditingReport.IsAnonymous,
                    HazardCategory = category?.Value,
                    HazardType = hazardType?.Value , //primaryHazard.HazardType,
                    Description = primaryHazard.Description,
                    Location = primaryHazard.LocationArea
                };


                // STEP 3: Load the hazard types for the category (this will populate HazardTypeOptions)
                if (category is not null)
                {
                    _logger.LogInformation("Loading hazard types for category: {Category}", category.Value);

                    // This will populate HazardTypeOptions and NOT clear HazardReport.HazardType
                    await LoadHazardTypesForCategory(category.Value, preserveSelectedType: true);

                    _logger.LogInformation("Loaded {Count} hazard types for category {Category}. Current type: {Type}",
                        HazardTypeOptions.Count, category.Value, HazardReport.HazardType);
                }
                else
                {
                    _logger.LogWarning("No category found for hazard type: {HazardType}", hazardType); // primaryHazard.HazardType);
                    // Clear hazard types if no category
                    HazardTypeOptions.Clear();
                }

                // STEP 4: Handle geographic location data
                try
                {
                    var locationQuery = new GetHazardLocationsByHazardCodeQuery(primaryHazard.Code);
                    var locationResult = await _mediator.SendAsync(locationQuery, CancellationToken.None);

                    if (locationResult.IsSuccess && locationResult.Value?.Any() == true)
                    {
                        primaryHazard.HazardLocation = locationResult.Value
                            .OrderByDescending(l => l.UpdatedDate ?? DateTime.MinValue)
                            .ThenByDescending(l => l.DateSelected)
                            .ThenByDescending(l => l.CreatedDate ?? DateTime.MinValue)
                            .FirstOrDefault();

                        _logger.LogInformation("Loaded latest HazardLocation for report edit hazard {HazardCode}: IsValidated={IsValidated}",
                            primaryHazard.Code,
                            primaryHazard.HazardLocation?.IsValidated);
                    }
                }
                catch (Exception locationEx)
                {
                    _logger.LogWarning(locationEx, "Failed to load latest HazardLocation for report edit hazard {HazardCode}", primaryHazard.Code);
                }

                if (primaryHazard.HazardLocation is not null)
                {
                    SelectedGeoLocation = new HazardLocation
                    {
                        Code = primaryHazard.HazardLocation.Code,
                        HazardCode = primaryHazard.HazardLocation.HazardCode,
                        Latitude = primaryHazard.HazardLocation.Latitude ?? 0,
                        Longitude = primaryHazard.HazardLocation.Longitude ?? 0,
                        Description = primaryHazard.HazardLocation.Description ?? string.Empty,
                        DateSelected = DateTime.UtcNow,
                        IsValidated = primaryHazard.HazardLocation.IsValidated,
                        IsValid = (primaryHazard.HazardLocation.Latitude ?? 0) != 0 && (primaryHazard.HazardLocation.Longitude ?? 0) != 0
                    };

                if (EditingHazard is not null)
                {
                    EditingHazard.HazardLocation = SelectedGeoLocation;
                }

                    SelectedLatitude = SelectedGeoLocation.Latitude ?? 0;
                    SelectedLongitude = SelectedGeoLocation.Longitude ?? 0;
                    SelectedLocationDescription = SelectedGeoLocation.Description ?? "";

                    HazardReport.Location = "MAP_LOCATION";

                    _logger.LogInformation("Loaded geographic location: {Lat}, {Lng}",
                        SelectedGeoLocation.Latitude, SelectedGeoLocation.Longitude);
                }

                await LoadExistingHazardFilesAsync(primaryHazard.Code);

                _logger.LogInformation("Successfully loaded report {ReportCode} with hazard {HazardCode} - Category: {Category}, Type: {Type}",
                    reportCode, primaryHazard.Code, SelectedHazardCategory, HazardReport.HazardType);
            }
            else
            {
                // No hazards found, use report data
                _logger.LogWarning("No hazards found for report {ReportCode}, using report data", reportCode);

                HazardReport = new HazardReportForm
                {
                    HazardType = EditingReport.Name,
                    Description = EditingReport.Description,
                    IncidentDateTime = EditingReport.IncidentDateTime,
                    SubmittedBy = EditingReport.SubmittedBy,
                    SubmittedDate = EditingReport.SubmittedDate,
                    SubmittingDepartment = EditingReport.SubmittingDepartment,
                    SubmittingDepartmentJobFunction = EditingReport.SubmittingDepartmentJobFunction,
                    ReportContactName = EditingReport.ReportContactName,
                    ReportContactCell = EditingReport.ReportContactCell,
                    ReportContactEmail = EditingReport.ReportContactEmail,
                    ReportContactCompany  = EditingReport.ReportContactCompany,
                    IsAnonymous = EditingReport.IsAnonymous
                };

                // Clear category-related fields
                SelectedHazardCategory = null;
                HazardTypeOptions.Clear();
                DepartmentOptions.Clear();
                SelectedDepartment =null;
            }
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Info("Information", $"Loaded report {reportCode} for editing."));
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading report for editing: {ReportCode}", reportCode);
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Failed to load report for editing. Redirecting to Reports page."));
            
            // Redirect back to reports on failure
            _navigation.NavigateToSecure("/SMSRiskManagement/Reports");
        }
        finally
        {
            IsLoading = false;
            // Force a complete UI refresh after loading
            await InvokeAsync(StateHasChanged);

            // Add a small delay and refresh again to ensure binding
            await Task.Delay(100);
            await InvokeAsync(StateHasChanged);
        }
    }

    /// <summary>
    /// Load hazard data directly for editing (when using HazardCode route parameter)
    /// </summary>
    private async Task LoadHazardForEditingAsync(string hazardCode)
    {
        try
        {
            IsLoading = true;
            StateHasChanged();

            _logger.LogInformation("Loading hazard {HazardCode} directly for editing", hazardCode);

            // Get the hazard details
            var hazardQuery = new GetHazardByCodeQuery(new HazardID(hazardCode));
            var hazardResult = await _mediator.SendAsync(hazardQuery, CancellationToken.None);

            if (hazardResult.IsFailure || hazardResult.Value is null)
            {
                throw new InvalidOperationException($"Hazard {hazardCode} not found");
            }

            EditingHazard = hazardResult.Value;
            EditHazardCode = hazardCode;

            // **CRITICAL FIX**: Load the associated HazardLocation separately since MapToHazard doesn't include it
            try 
            {
                var locationQuery = new GetHazardLocationsByHazardCodeQuery(hazardCode);
                var locationResult = await _mediator.SendAsync(locationQuery, CancellationToken.None);

                if (locationResult.IsSuccess && locationResult.Value?.Any() == true)
                {
                    // Select the most recent location record to align UI state with latest validation action.
                    EditingHazard.HazardLocation = locationResult.Value
                        .OrderByDescending(l => l.UpdatedDate ?? DateTime.MinValue)
                        .ThenByDescending(l => l.DateSelected)
                        .ThenByDescending(l => l.CreatedDate ?? DateTime.MinValue)
                        .FirstOrDefault();

                    _logger.LogInformation("Successfully loaded HazardLocation for hazard {HazardCode}: Lat={Lat}, Lng={Lng}, IsValidated={IsValidated}",hazardCode,
                        EditingHazard.HazardLocation?.Latitude,
                        EditingHazard.HazardLocation?.Longitude,
                        EditingHazard.HazardLocation?.IsValidated);
                }
                else
                {
                    _logger.LogWarning("No HazardLocation found for hazard {HazardCode}", hazardCode);
                }
            }
            catch (Exception locationEx)
            {
                _logger.LogWarning(locationEx, "Failed to load HazardLocation for hazard {HazardCode}, continuing without location data", hazardCode);
            }

            // Get the associated report
            if (!string.IsNullOrEmpty(EditingHazard.ReportCode))
            {
                var reportQuery = new GetReportByCodeQuery(new ReportID(EditingHazard.ReportCode));
                var reportResult = await _mediator.SendAsync(reportQuery, CancellationToken.None);

                if (reportResult.IsSuccess && reportResult.Value is not null)
                {
                    EditingReport = reportResult.Value;
                    EditReportCode = EditingReport.Code;
                }
            }

            _logger.LogInformation("Found hazard {HazardCode} with type: {HazardType}", EditingHazard.Code, EditingHazard.HazardType);

            // Try to determine category from hazard type
            var hazardType = HazardType.FromValue(EditingHazard.HazardType ?? "");
            var category = HazardCategory.FromValue(EditingHazard.HazardCategory ?? "") ;

            _logger.LogInformation("Determined category: {Category} from hazard type: {HazardType}", 
                category?.Value ?? "NULL", EditingHazard.HazardType);

            // STEP 1: Set the category first
            SelectedHazardCategory = category?.Value;

            // STEP 2: Populate form with hazard data and report data (if available)
            HazardReport = new HazardReportForm
            {
                HazardCategory = category?.Value,
                HazardType = hazardType?.Value,
                Description = EditingHazard.Description,
                Location = EditingHazard.LocationArea,
                HazardTitle = EditingHazard.HazardTitle,
                // Use report data if available, otherwise use defaults
                IncidentDateTime = EditingReport?.IncidentDateTime ?? DateTime.Now,
                SubmittedBy = EditingReport?.SubmittedBy ?? _currentUserService?.UserDisplayName ?? "Unknown User",
                SubmittedDate = EditingReport?.SubmittedDate ?? DateTime.Now,
                SubmittingDepartment = EditingReport?.SubmittingDepartment ?? "",
                SubmittingDepartmentJobFunction = EditingReport?.SubmittingDepartmentJobFunction ?? "",
                ReportContactName = EditingReport?.ReportContactName ?? "",
                ReportContactCell = EditingReport?.ReportContactCell ?? "",
                ReportContactEmail = EditingReport?.ReportContactEmail ?? "",
                ReportContactCompany = EditingReport?.ReportContactCompany ?? "",
                IsAnonymous = EditingReport?.IsAnonymous ?? false
            };

            // STEP 3: Load the hazard types for the category
            if (category is not null)
            {
                _logger.LogInformation("Loading hazard types for category: {Category}", category.Value);
                await LoadHazardTypesForCategory(category.Value, preserveSelectedType: true);
                _logger.LogInformation("Loaded {Count} hazard types for category {Category}. Current type: {Type}",
                    HazardTypeOptions.Count, category.Value, HazardReport.HazardType);
            }
            else
            {
                _logger.LogWarning("No category found for hazard type: {HazardType}", hazardType);
                HazardTypeOptions.Clear();
            }

            // STEP 4: Handle geographic location data
            if (EditingHazard.HazardLocation is not null)
            {
                SelectedGeoLocation = new HazardLocation
                {
                    Code = EditingHazard.HazardLocation.Code,
                    HazardCode = EditingHazard.HazardLocation.HazardCode,
                    Latitude = EditingHazard.HazardLocation.Latitude ?? 0,
                    Longitude = EditingHazard.HazardLocation.Longitude ?? 0,
                    Description = EditingHazard.HazardLocation.Description ?? string.Empty,
                    DateSelected = DateTime.UtcNow,
                    IsValidated = EditingHazard.HazardLocation.IsValidated,
                    IsValid = (EditingHazard.HazardLocation.Latitude ?? 0) != 0 && (EditingHazard.HazardLocation.Longitude ?? 0) != 0
                };

                EditingHazard.HazardLocation = SelectedGeoLocation;

                // Keep scalar UI fields in sync before first render-dependent validations
                SelectedLatitude = SelectedGeoLocation.Latitude ?? 0;
                SelectedLongitude = SelectedGeoLocation.Longitude ?? 0;
                SelectedLocationDescription = SelectedGeoLocation.Description ?? string.Empty;

                SelectedLatitude = SelectedGeoLocation.Latitude ?? 0;
                SelectedLongitude = SelectedGeoLocation.Longitude ?? 0;
                SelectedLocationDescription = SelectedGeoLocation.Description ?? "";

                HazardReport.Location = "MAP_LOCATION";

                _logger.LogInformation("Loaded geographic location: {Lat}, {Lng}",SelectedGeoLocation.Latitude, SelectedGeoLocation.Longitude);
            }

            await LoadExistingHazardFilesAsync(EditingHazard.Code);

            _logger.LogInformation("Successfully loaded hazard {HazardCode} for editing - Category: {Category}, Type: {Type}",
                hazardCode, SelectedHazardCategory, HazardReport.HazardType);

            await _eventBus.PublishUIEventAsync(UINotificationEvent.Info("Information", $"Loaded hazard {hazardCode} for editing."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading hazard for editing: {HazardCode}", hazardCode);
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Failed to load hazard for editing. Redirecting to Hazards page."));

            // Redirect back to hazards listing on failure
            _navigation.NavigateToSecure("/Listings/HazardListing");
        }
        finally
        {
            IsLoading = false;
            // Force a complete UI refresh after loading
            await InvokeAsync(StateHasChanged);

            // Add a small delay and refresh again to ensure binding
            await Task.Delay(100);
            await InvokeAsync(StateHasChanged);
        }
    }

    /// <summary>
    /// Load hazard types for a specific category without clearing the selected type
    /// </summary>
    private async Task LoadHazardTypesForCategory(string categoryValue, bool preserveSelectedType = false)
    {
        if (string.IsNullOrEmpty(categoryValue))
        {
            HazardTypeOptions.Clear();
            return;
        }

        var category = HazardCategory.FromValue(categoryValue);
        if (category is not null)
        {
            // Store the current selected type if we want to preserve it
            var currentSelectedType = preserveSelectedType ? HazardReport.HazardType : null;

            // Filter hazard types by selected category
            HazardTypeOptions = HazardType.GetByCategory(category)
                .Select(ht => new DropdownOption(ht.Value, ht.Name))
                .ToList();

            // Restore the selected type if preserving and it exists in the new options
            if (preserveSelectedType && !string.IsNullOrEmpty(currentSelectedType))
            {
                var typeExists = HazardTypeOptions.Any(ht => ht.Value == currentSelectedType);
                if (typeExists)
                {
                    HazardReport.HazardType = currentSelectedType;
                }
                else
                {
                    HazardReport.HazardType = HazardType.Default.Value;
                    _logger.LogWarning("Selected type {Type} not found in category {Category}",currentSelectedType, category.Value);
                }
            }

                _logger.LogInformation("Loaded {Count} hazard types for category: {Category}",
                HazardTypeOptions.Count, category.Name);
        }
        else
        {
            HazardTypeOptions.Clear();
            _logger.LogWarning("Category not found: {CategoryValue}", categoryValue);
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadExistingHazardFilesAsync(string hazardCode)
    {
        if (string.IsNullOrWhiteSpace(hazardCode))
        {
            AttachedFiles = new List<AttachedFile>();
            return;
        }

        try
        {
            var filesResult = await _mediator.SendAsync(new GetHazardFilesByHazardCodeQuery(hazardCode, includeFileData: false), CancellationToken.None);

            if (filesResult.IsSuccess && filesResult.Value is not null)
            {
                AttachedFiles = filesResult.Value
                    .Where(f => !string.IsNullOrWhiteSpace(f.FileName))
                    .Select(f => new AttachedFile
                    {
                        FileName = f.FileName,
                        FileSizeBytes = f.FileSizeBytes,
                        Size = f.FileSizeBytes,
                        SizeDisplay = FormatFileSize(f.FileSizeBytes),
                        ContentType = f.ContentType ?? "application/octet-stream",
                        FilePath = f.FilePath,
                        StorageType = f.StorageType,
                        Data = Array.Empty<byte>()
                    })
                    .ToList();

                _logger.LogInformation("Loaded {Count} existing hazard files for hazard {HazardCode}", AttachedFiles.Count, hazardCode);
            }
            else
            {
                AttachedFiles = new List<AttachedFile>();
            }
        }
        catch (Exception ex)
        {
            AttachedFiles = new List<AttachedFile>();
            _logger.LogWarning(ex, "Failed to load hazard files for hazard {HazardCode}", hazardCode);
        }
    }

    #endregion

    #region Form Event Handlers

    /// <summary>
    /// Handle form submission
    /// </summary>
    public async Task HandleFormSubmit(HazardReportForm formData)
    {
        try
        {
            _logger.LogInformation("Form submit triggered with data: HazardType={HazardType}, SubmittedBy={SubmittedBy}", formData.HazardType, formData.SubmittedBy);

            if (!IsFormValidForSubmission())
            {
                await _eventBus.PublishUIEventAsync(UINotificationEvent.Warning("Warning", "Please complete all required fields before submitting."));
                return;
            }

            await ShowSubmissionConfirmationDialog();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during form submission");
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "An error occurred while submitting your report. Please try again."));
            
        }
    }

       
    /// <summary>
    /// Handle InputFile change event - this will accumulate files properly
    /// </summary>
    public async Task OnInputFileChange(UploadChangeEventArgs args)
    {
        var newFiles = args.Files; // Allow up to 10 files at once
        _logger.LogInformation("OnInputFileChange called with {Count} new files", newFiles?.Count() ?? 0);

        if (newFiles?.Any() == true)
        {
            // Process files immediately to avoid the "file list may have changed" error
            var successfullyProcessedFiles = new List<AttachedFile>();
            var failedFiles = new List<string>();

            foreach (var newFile in newFiles)
            {
                try
                {
                    // Check for duplicate first (before processing)
                    var isDuplicate = AttachedFiles.Any(existing =>
                        existing.FileName.Equals(newFile.Name, StringComparison.OrdinalIgnoreCase) &&
                        existing.Size == newFile.Size);

                    if (isDuplicate)
                    {
                        _logger.LogInformation("Skipped duplicate file: {FileName}", newFile.Name);
                        continue;
                    }

                    // Read file data immediately to avoid JavaScript interop issues
                    byte[] fileData;
                    using (var stream = newFile.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024))
                    using (var memoryStream = new MemoryStream())
                    {
                        await stream.CopyToAsync(memoryStream);
                        fileData = memoryStream.ToArray();
                    }

                    // Create the attached file object with cached data
                    var attachedFile = new AttachedFile
                    {
                        FileName = newFile.Name,
                        ContentType = newFile.ContentType ?? "application/octet-stream",
                        Size = newFile.Size,
                        Data = fileData,
                        SizeDisplay = FormatFileSize(newFile.Size)
                    };

                    successfullyProcessedFiles.Add(attachedFile);
                    _logger.LogInformation("Successfully processed file: {FileName} ({Size} bytes)", newFile.Name, newFile.Size);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing file: {FileName}", newFile.Name);
                    failedFiles.Add(newFile.Name);
                }
            }

            // Add successfully processed files to the collection
            if (successfullyProcessedFiles.Any())
            {
                AttachedFiles.AddRange(successfullyProcessedFiles);

                // Update SelectedFiles to maintain compatibility (though we won't use it for reading data)
                var allFiles = SelectedFiles?.ToList() ?? new List<IBrowserFile>();
                foreach (var file in newFiles.Where(f => successfullyProcessedFiles.Any(sf => sf.FileName == f.Name && sf.Size == f.Size)))
                {
                    allFiles.Add(file);
                }
                SelectedFiles = allFiles.AsReadOnly();
            }

            // Show notification about results
            if (successfullyProcessedFiles.Any() && failedFiles.Any())
            {
                await _eventBus.PublishUIEventAsync(UINotificationEvent.Warning("Warning", $"Added {successfullyProcessedFiles.Count} file(s). Failed to process {failedFiles.Count} file(s). Total: {AttachedFiles.Count} files queued."));
            } else if (successfullyProcessedFiles.Any())
            {
                await _eventBus.PublishUIEventAsync(UINotificationEvent.Success("Success", $"Added {successfullyProcessedFiles.Count} file(s) to the queue. Total: {AttachedFiles.Count} files."));
            }
            else if (failedFiles.Any())
            {
                await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", $"Failed to process {failedFiles.Count} file(s). This may be due to file size limits or browser restrictions."));
            }

            _logger.LogInformation("File processing completed: {Success} successful, {Failed} failed. Total queued: {Total}",successfullyProcessedFiles.Count, failedFiles.Count, AttachedFiles.Count);
        }
        else
        {
            _logger.LogInformation("No files provided to OnInputFileChange");
        }

        StateHasChanged();
    }

    private bool ShouldUseStoredExternalPath(AttachedFile file)
    {
        if (string.IsNullOrWhiteSpace(file.FilePath))
        {
            return false;
        }

        var storageType = file.StorageType ?? string.Empty;
        if (storageType.Equals("Cloud", StringComparison.OrdinalIgnoreCase) ||
            storageType.Equals("FileSystem", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return Uri.TryCreate(file.FilePath, UriKind.Absolute, out _);
    }

    private static string ResolveExternalFilePath(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return string.Empty;
        }

        if (Uri.TryCreate(filePath, UriKind.Absolute, out _))
        {
            return filePath;
        }

        if (filePath.StartsWith("//", StringComparison.Ordinal))
        {
            return $"https:{filePath}";
        }

        if (!filePath.Contains('/') && !filePath.Contains('\\'))
        {
            return filePath;
        }

        return filePath.TrimStart('~');
    }

    #endregion

    #region Location Methods

    /// <summary>
    /// Open map selector modal
    /// </summary>
    public async Task OpenMapSelector()
    {
        _mapOriginalLatitude = SelectedLatitude;
        _mapOriginalLongitude = SelectedLongitude;
        _mapOriginalLocationDescription = SelectedLocationDescription;
        _mapOriginalGeoLocation = new HazardLocation
        {
            Code = SelectedGeoLocation?.Code ?? string.Empty,
            HazardCode = SelectedGeoLocation?.HazardCode ?? string.Empty,
            Latitude = SelectedGeoLocation?.Latitude,
            Longitude = SelectedGeoLocation?.Longitude,
            Description = SelectedGeoLocation?.Description,
            IsValidated = SelectedGeoLocation?.IsValidated ?? false,
            IsValid = SelectedGeoLocation?.IsValid ?? false,
            DateSelected = SelectedGeoLocation?.DateSelected ?? DateTime.UtcNow
        };
        _mapSelectionConfirmed = false;
        _mapSelectionCleared = false;

        _isMapEditModeEnabled = false;
        ShowMapModal = true;
        StateHasChanged();

        // Give DOM time to render the modal
        await Task.Delay(500);

        // Always try to initialize the map when modal opens
        if (_mapModule is not null)
        {
            try
            {
                // Always reinitialize the map since the DOM element is recreated
            await _mapModule.InvokeVoidAsync("initializeMap",_airportCenterLatitude, _airportCenterLongitude, _defaultZoomLevel, _dotNetRef);

                _logger.LogInformation("Map reinitialized for modal opening");

                // Restore existing location pin for both validated and pending-validation locations
                var locationForMap = ActiveLocationForValidation;
                if (locationForMap?.Latitude is decimal mapLat &&
                    locationForMap.Longitude is decimal mapLng &&
                    mapLat != 0 &&
                    mapLng != 0)
                {
                    await Task.Delay(500); // Give map time to initialize

                    await _mapModule.InvokeVoidAsync(
                        "setLocationFromCoordinates",
                        (double)mapLat,
                        (double)mapLng,
                        locationForMap.Description ?? string.Empty);

                    // Update the form fields to match the restored location
                    SelectedLatitude = mapLat;
                    SelectedLongitude = mapLng;
                    SelectedLocationDescription = locationForMap.Description ?? string.Empty;

                    _logger.LogInformation("Existing location restored: {Lat}, {Lng}", mapLat, mapLng);

                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing map in OpenMapSelector");
                await _eventBus.PublishUIEventAsync(UINotificationEvent.Warning("Warning", "Could not initialize map. Please try refreshing the page."));
            }
        }
        if (IsMapReadOnlyMode)
        {
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Info("Information", "Viewing validated location. Click Edit Location to modify and revalidate."));
        }
        else
        {
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Info("Information", "Click on the map to select the hazard location."));
        }
    }

    public void EnableLocationEditMode()
    {
        _isMapEditModeEnabled = true;
        StateHasChanged();
    }

    /// <summary>
    /// Close map selector modal
    /// </summary>
    public void CloseMapSelector()
    {
        if (!_mapSelectionConfirmed && !_mapSelectionCleared)
        {
            SelectedLatitude = _mapOriginalLatitude;
            SelectedLongitude = _mapOriginalLongitude;
            SelectedLocationDescription = _mapOriginalLocationDescription;
            SelectedGeoLocation = new HazardLocation
            {
                Code = _mapOriginalGeoLocation.Code,
                HazardCode = _mapOriginalGeoLocation.HazardCode,
                Latitude = _mapOriginalGeoLocation.Latitude,
                Longitude = _mapOriginalGeoLocation.Longitude,
                Description = _mapOriginalGeoLocation.Description,
                IsValidated = _mapOriginalGeoLocation.IsValidated,
                IsValid = _mapOriginalGeoLocation.IsValid,
                DateSelected = _mapOriginalGeoLocation.DateSelected
            };
        }

        ShowMapModal = false;
        _isMapEditModeEnabled = false;
        // Don't reset map initialization state here to preserve the pin
        StateHasChanged();
    }

    /// <summary>
    /// Use selected location from map
    /// </summary>
    public async Task UseSelectedLocation()
    {
        if (!HasValidCoordinates)
        {

            await _eventBus.PublishUIEventAsync(UINotificationEvent.Warning("Warning", "Please click on the map to select a location first."));
            return;
        }

        var locationWasPreviouslyValidated = ActiveLocationForValidation?.IsValidated == true;
        var locationWasModified = locationWasPreviouslyValidated && IsLocationChangedFromActiveLocation();

        // Set the geolocation data
        SelectedGeoLocation = new HazardLocation
        {
            Code = SelectedGeoLocation.Code,
            HazardCode = SelectedGeoLocation.HazardCode,
            Latitude = SelectedLatitude,
            Longitude = SelectedLongitude,
            Description = SelectedLocationDescription,// string.IsNullOrEmpty(SelectedLocationDescription) ? 
                                              //$"Map Location ({SelectedLatitude:F6}, {SelectedLongitude:F6})" : SelectedLocationDescription,
            DateSelected = DateTime.UtcNow,
            IsValidated = locationWasModified ? false : SelectedGeoLocation.IsValidated,
            IsValid = true
        };

        _mapSelectionConfirmed = true;

        if (!SelectedGeoLocation.IsValidated)
        {
            var locationCode = string.IsNullOrWhiteSpace(SelectedGeoLocation.Code)
                ? "selected location"
                : SelectedGeoLocation.Code;

            var confirmResult = await _dialogService.Confirm(
                //$"Validate location '{locationCode}'? This will set IsValidated to true.",
                "",
                "Hazard Location Confirmed",
                new ConfirmOptions
                {
                    OkButtonText = "Valid",
                    CancelButtonText = "Cancel"
                });

            if (confirmResult != true)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(SelectedGeoLocation.Code))
            {
                SelectedGeoLocation.IsValidated = true;
                var updateLocationResult = await _mediator.SendAsync(new UpdateHazardLocationCommand(SelectedGeoLocation), CancellationToken.None);

                if (updateLocationResult.IsFailure)
                {
                    SelectedGeoLocation.IsValidated = false;
                    await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", updateLocationResult.Error?.Message ?? "Failed to validate selected location."));
                    return;
                }
            }
            else
            {
                // New unsaved location; keep validation state in memory and it will persist on submit.
                SelectedGeoLocation.IsValidated = true;
            }

            if (EditingHazard is not null)
            {
                EditingHazard.HazardLocation = SelectedGeoLocation;
            }
        }

        // Update the form location to indicate map location is selected
        HazardReport.Location = "MAP_LOCATION";

        ShowMapModal = false;
        _isMapEditModeEnabled = false;
        StateHasChanged();

        await _eventBus.PublishUIEventAsync(UINotificationEvent.Success("Success", $"Location selected: {GeoLocationDisplay}"));
        
    }

    /// <summary>
    /// Clear map selection
    /// </summary>
    public async Task ClearMapSelection()
    {
        if (!CanClearMapSelection)
        {
            return;
        }

        SelectedLatitude = 0;
        SelectedLongitude = 0;
        SelectedLocationDescription = string.Empty;
        SelectedGeoLocation = new HazardLocation
        {
            IsValid = false,
            IsValidated = false
        };
        HazardReport.Location = "";
        _mapSelectionCleared = true;
        _mapSelectionConfirmed = false;

        if (_mapModule is not null)
        {
            try
            {
                await _mapModule.InvokeVoidAsync("clearSelection");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error clearing map selection");
            }
        }

        StateHasChanged();
        await _eventBus.PublishUIEventAsync(UINotificationEvent.Info("Information", "Map selection has been cleared."));
        
    }

    [JSInvokable("OnMapLocationSelected")]
    public async Task OnMapLocationSelected(double latitude, double longitude, string description)
    {
        if (IsMapReadOnlyMode)
        {
            return;
        }

        SelectedLatitude = (decimal)latitude;
        SelectedLongitude = (decimal)longitude;
        SelectedLocationDescription = description; // This will set the description automatically!

        // Validate that we received proper coordinates
        if (latitude == 0 && longitude == 0)
        {
            _logger?.LogWarning("Received zero coordinates from map click");
        }

        _logger?.LogInformation("Map location received from JS: Lat={Lat}, Lng={Lng}, Description={Desc}", latitude, longitude, description);

        await InvokeAsync(StateHasChanged);

        _logger?.LogInformation("Map location selected: {Lat}, {Lng} - HasValidCoordinates: {HasValid}", latitude, longitude, HasValidCoordinates);
    }

    private bool IsLocationChangedFromActiveLocation()
    {
        var activeLocation = ActiveLocationForValidation;
        if (activeLocation is null)
        {
            return false;
        }

        var activeLatitude = activeLocation.Latitude ?? 0;
        var activeLongitude = activeLocation.Longitude ?? 0;
        var activeDescription = activeLocation.Description?.Trim() ?? string.Empty;
        var selectedDescription = SelectedLocationDescription?.Trim() ?? string.Empty;

        return activeLatitude != SelectedLatitude ||
               activeLongitude != SelectedLongitude ||
               !string.Equals(activeDescription, selectedDescription, StringComparison.Ordinal);
    }


    #endregion

    #region Modal Methods

    /// <summary>
    /// Show submission confirmation modal
    /// </summary>
    public async Task ShowSubmissionConfirmationDialog()
    {
        if (!IsFormValidForSubmission())
        {
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Warning("Warning", "Please complete all required fields before submitting."));
            return;
        }

        _logger.LogInformation("Preparing for submission with {Count} cached files ready", AttachedFiles?.Count ?? 0);
        if (AttachedFiles?.Any() == true)
        {
            foreach (var file in AttachedFiles)
            {
                _logger.LogInformation("Cached file ready for submission: {FileName} ({Size} bytes, {DataSize} bytes cached)", file.FileName, file.Size, file.Data?.Length ?? 0);
            }
        }

        ShowSubmissionConfirmation = true;
        StateHasChanged(); // Force UI update to hide buttons
    }

    /// <summary>
    /// Cancel submission
    /// </summary>
    public async Task CancelSubmission()
    {
        ShowSubmissionConfirmation = false;
        StateHasChanged(); // Force UI update to show buttons again
        await _eventBus.PublishUIEventAsync(UINotificationEvent.Info("Information", "You can continue editing your report."));
    }

    

    private string GetTrackingUrl()
    {
        if (string.IsNullOrEmpty(GeneratedTrackingId))
            return string.Empty;

        var baseUri = _navigation.BaseUri.TrimEnd('/');
        return $"{baseUri}/SMSRiskManagement/HazardReportSearch/TrackStatus/{GeneratedTrackingId}";
    }

    /// <summary>
    /// Close final confirmation and handle navigation based on mode
    /// </summary>
    public void CloseFinalConfirmation()
    {
        if (IsEditMode)
        {
            // In edit mode, redirect back to Reports page
            _navigation.NavigateToSecure("/SMSRiskManagement/ReportProcessing");
        }
        else
        {
            // In create mode, clear form and redirect to Report Processing
            // Clear all form data after successful submission
            HazardReport = new HazardReportForm();
            SelectedGeoLocation = new HazardLocation();

            // Clear files properly
            SelectedFiles = new List<IBrowserFile>().AsReadOnly();
            AttachedFiles.Clear();

            GeneratedHazardId = null;
            GeneratedReportId = null;
            SubmissionDateTime = null;

            // Reset coordinates
            SelectedLatitude = 0;
            SelectedLongitude = 0;
            SelectedLocationDescription = string.Empty;

            // Reset category selection
            SelectedHazardCategory = null;
            HazardTypeOptions.Clear();

            // Reset edit mode state
            IsEditMode = false;
            EditReportCode = null;
            EditingReport = null;
            EditingHazard = null;
            EditHazardCode = null;

            // Reset all UI state flags
            ShowConfidentialInfo = false;
            ShowMapModal = false;
            ShowSubmissionConfirmation = false;
            ShowFinalSuccessConfirmation = false;

            _navigation.NavigateToSecure("/SMSRiskManagement/ReportProcessing");
        }
    }
    
    /// <summary>
    /// Print the confirmation details with QR code captured from RadzenQRCode component
    /// </summary>
    public async Task PrintConfirmation()
    {
        try
        {
            if (string.IsNullOrEmpty(GeneratedTrackingId) || string.IsNullOrEmpty(GeneratedReportId))
            {
                await _eventBus.PublishUIEventAsync(UINotificationEvent.Warning("Warning", "No report information available to print."));
                return;
            }

            var trackingUrl = GetTrackingUrl();
            var submissionDate = SubmissionDateTime?.ToString("MMMM dd, yyyy 'at' h:mm tt") ?? DateTime.Now.ToString("MMMM dd, yyyy 'at' h:mm tt");

            // Call the NEW JavaScript function that captures the actual RadzenQRCode
            await _jsRuntime.InvokeVoidAsync("printReportConfirmation",
                GeneratedReportId,
                GeneratedHazardId,
                GeneratedTrackingId,
                submissionDate,
                trackingUrl);

            _logger?.LogInformation("Print confirmation initiated for Tracking ID: {TrackingId}", GeneratedTrackingId);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error printing confirmation");

            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Failed to print confirmation. Please try again or save the page."));
        }
    }



    #endregion

    #region Business Logic Methods
    /// <summary>
    /// RFC 5322 compliant email validation
    /// </summary>
    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        // RFC 5322 compliant regex pattern
        var pattern = @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$";

        try
        {
            return global::System.Text.RegularExpressions.Regex.IsMatch(email, pattern,
                global::System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
        catch
        {
            return false;
        }
    }
    /// <summary>
    /// Submit the hazard report - handles both CREATE and EDIT modes
    /// </summary>
    public async Task SubmitReportConfirmed()
    {
        if (IsLoading)
        {
            _logger.LogWarning("SubmitReportConfirmed ignored because a submission is already in progress.");
            return;
        }

        try
        {
            // Validation
            if (!IsFormValidForSubmission())
            {
                ShowSubmissionConfirmation = true;
                StateHasChanged();
                await _eventBus.PublishUIEventAsync(UINotificationEvent.Warning("Warning", "Please complete all required fields before submitting."));
                return;
            }

            IsLoading = true;
            ShowSubmissionConfirmation = false;
            StateHasChanged();

            if (IsEditMode && EditingReport is not null && EditingHazard is not null)
            {
                // ===============================
                // EDIT MODE - Update existing report and hazard
                // ===============================
                _logger.LogInformation("Starting EDIT mode submission for Report: {ReportCode}, Hazard: {HazardCode}", EditReportCode, EditHazardCode);

                await UpdateExistingReportAndHazard();
            }
            else
            {
                // ===============================
                // CREATE MODE - Create new report and hazard  
                // ===============================
                _logger.LogInformation("Starting CREATE mode submission for new hazard report");

                await CreateNewReportAndHazard();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during hazard report submission in {Mode} mode",
                IsEditMode ? "EDIT" : "CREATE");

            ShowSubmissionConfirmation = false;
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", $"An error occurred while {(IsEditMode ? "updating" : "saving")} your report. Please try again."));
            
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Update existing report and hazard (EDIT MODE)
    /// </summary>
    private async Task UpdateExistingReportAndHazard()
    {
        _logger.LogInformation("Updating existing Report: {ReportCode} and Hazard: {HazardCode}", EditReportCode, EditHazardCode);

        // ===============================
        // STEP 1: Update the existing Report
        // ===============================
        EditingReport!.Name = $"{HazardReport.HazardCategory} - {HazardReport.HazardType}";
        EditingReport.Description = HazardReport.Description;
        EditingReport.IncidentDateTime = HazardReport.IncidentDateTime;
        EditingReport.SubmittedBy = HazardReport.SubmittedBy;
        EditingReport.SubmittedDate = HazardReport.SubmittedDate;
        EditingReport.SubmittingDepartment = HazardReport.SubmittingDepartment;
        EditingReport.SubmittingDepartmentJobFunction = HazardReport.SubmittingDepartmentJobFunction;
        EditingReport.ReportContactName = HazardReport.ReportContactName;
        EditingReport.ReportContactCell = HazardReport.ReportContactCell;
        EditingReport.ReportContactEmail = HazardReport.ReportContactEmail;
        EditingReport.ReportContactCompany = HazardReport.ReportContactCompany;
        EditingReport.IsAnonymous = HazardReport.IsAnonymous;
        EditingReport.Status = ReportStatus.ReadyForProcessing;
        EditingReport.UpdatedDate = DateTime.UtcNow;
        EditingReport.UpdatedBy = _currentUserService.UserCode;

        var updateReportCommand = new UpdateReportCommand(EditingReport);
        var reportUpdateResult = await _mediator.SendAsync(updateReportCommand, CancellationToken.None);

        if (reportUpdateResult.IsFailure)
        {
            throw new Exception($"Failed to update report: {reportUpdateResult.Error?.Message}");
        }

        _logger.LogInformation("Report {ReportCode} updated successfully", EditReportCode);

        // ===============================
        // STEP 2: Update the existing Hazard
        // ===============================
        EditingHazard!.Name = $"{HazardReport.HazardCategory} - {HazardReport.HazardType}";
        EditingHazard.HazardTitle = $"{HazardReport.HazardTitle}";
        EditingHazard.Description = HazardReport.Description ?? string.Empty;
        EditingHazard.HazardCategory = HazardReport.HazardCategory ?? string.Empty;
        EditingHazard.HazardType = HazardReport.HazardType; // This is the actual selected hazard type, not "Technical"
        
        EditingHazard.UpdatedDate = DateTime.UtcNow;
        EditingHazard.UpdatedBy = _currentUserService.UserDisplayName;

        // Handle location updates
        await UpdateHazardLocation(EditingHazard);

        var updateHazardCommand = new UpdateHazardCommand(EditingHazard);
        var hazardUpdateResult = await _mediator.SendAsync(updateHazardCommand, CancellationToken.None);

        if (hazardUpdateResult.IsFailure)
        {
            throw new Exception($"Failed to update hazard: {hazardUpdateResult.Error?.Message}");
        }

        var updatedHazard = hazardUpdateResult.Value;
        _logger.LogInformation("Hazard {HazardCode} updated successfully", EditHazardCode);

        // ===============================
        // STEP 3: Handle file updates (if any new files)
        // ===============================
        await ProcessFileUpdates(updatedHazard);

        var trackingCode = await ResolveTrackingCodeForHazardAsync(updatedHazard);

        if (!string.IsNullOrWhiteSpace(trackingCode))
        {
            GeneratedTrackingId = trackingCode;
        }

        // ===============================
        // SUCCESS - EDIT MODE returns to origin page (no modal/email flow)
        // ===============================
        GeneratedHazardId = updatedHazard.Code;
        GeneratedReportId = updatedHazard.ReportCode;
        SubmissionDateTime = DateTime.Now;
        ShowSubmissionConfirmation = false;
        ShowFinalSuccessConfirmation = false;

        _logger.LogInformation("EDIT mode completed - Report: {ReportCode}, Hazard: {HazardCode}",
            updatedHazard.ReportCode, updatedHazard.Code);

        await _eventBus.PublishUIEventAsync(UINotificationEvent.Success(
            "Success",
            $"Report {updatedHazard.ReportCode} and hazard {updatedHazard.Code} have been updated."));

        CancelEdit();

        
    }

    private async Task<string?> ResolveTrackingCodeForHazardAsync(Hazard hazard)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(hazard.Code))
            {
                var byHazardResult = await _mediator.SendAsync(new GetHazardReportTrackingByHazardCodeQuery(hazard.Code), CancellationToken.None);
                if (byHazardResult.IsSuccess && byHazardResult.Value?.Any() == true)
                {
                    var byHazardTrackingCode = byHazardResult.Value
                        .Where(x => !string.IsNullOrWhiteSpace(x.TrackingCode))
                        .OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate)
                        .Select(x => x.TrackingCode)
                        .FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(byHazardTrackingCode))
                    {
                        return byHazardTrackingCode;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(hazard.ReportCode))
            {
                var byReportResult = await _mediator.SendAsync(new GetHazardReportTrackingByReportCodeQuery(hazard.ReportCode), CancellationToken.None);
                if (byReportResult.IsSuccess && byReportResult.Value?.Any() == true)
                {
                    return byReportResult.Value
                        .Where(x => !string.IsNullOrWhiteSpace(x.TrackingCode))
                        .OrderByDescending(x => x.UpdatedDate ?? x.CreatedDate)
                        .Select(x => x.TrackingCode)
                        .FirstOrDefault();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to resolve tracking code for hazard {HazardCode}", hazard.Code);
        }

        return null;
    }

    /// <summary>
    /// Create new report and hazard (CREATE MODE)
    /// </summary>
    private async Task CreateNewReportAndHazard()
    {
        _logger.LogInformation("Creating new report and hazard");

        // ===============================
        // STEP 1: Create new Report
        // ===============================
        var report = new Report(new ReportID("RP-0000"))
        {
            Code = "RP-0000",
            Name = $"{HazardReport.HazardCategory} - {HazardReport.HazardType}",
            IncidentDateTime = HazardReport.IncidentDateTime,
            SubmittedBy = HazardReport.SubmittedBy,
            SubmittedDate = HazardReport.SubmittedDate,
            SubmittingDepartment = HazardReport.SubmittingDepartment,
            SubmittingDepartmentJobFunction = HazardReport.SubmittingDepartmentJobFunction,
            ReportContactName = HazardReport.ReportContactName,
            ReportContactCell = HazardReport.ReportContactCell,
            ReportContactEmail = HazardReport.ReportContactEmail,
            ReportContactCompany = HazardReport.ReportContactCompany,
            Description = HazardReport.Description,
            Stage = "INITIAL",
            Status = ReportStatus.ReadyForProcessing, //needs validation
            CreatedBy = _currentUserService.UserDisplayName,
            CreatedDate = DateTime.UtcNow
        };

        var reportResult = await _mediator.SendAsync(new CreateReportCommand(report), CancellationToken.None);

        if (reportResult.IsFailure)
        {
            throw new Exception($"Failed to create report: {reportResult.Error?.Message}");
        }

        var actualReportCode = reportResult.Value.Code;
        _logger.LogInformation("Report created with Code: {ReportCode}", actualReportCode);

        // ===============================
        // STEP 2: Create new Hazard
        // ===============================
        var hazard = new Hazard(new HazardID("HZ-0000"))
        {
            Code = "HZ-0000",
            Name = $"{HazardReport.HazardCategory} - {HazardReport.HazardType}",
            HazardTitle = HazardReport.HazardTitle ?? string.Empty,
            Description = HazardReport.Description ?? string.Empty,
            HazardCategory = HazardReport.HazardCategory ?? string.Empty,
            HazardType = HazardReport.HazardType,
            
            ReportCode = actualReportCode,
            IsInitialHazard = true ,
            CreatedBy = _currentUserService.UserDisplayName,
            CreatedDate = DateTime.UtcNow
        };

        var createHazardCommand = new CreateHazardCommand(hazard);
        var createdHazardResult = await _mediator.SendAsync(createHazardCommand, CancellationToken.None);

        if (createdHazardResult.IsFailure)
        {
            throw new Exception($"Failed to create hazard: {createdHazardResult.Error?.Message}");
        }

        var createdHazard = createdHazardResult.Value;

        // Handle location for confidential hazard AFTER NEW Hazard Has Been Recorded !! 
        await UpdateHazardLocation(createdHazard);

        GeneratedHazardId = createdHazard.Code;
        GeneratedReportId = createdHazard.ReportCode;

        _logger.LogInformation("Hazard created with Code: {HazardCode}, linked to Report: {ReportCode} - Event publishing now handled by Command Handler", 
            createdHazard.Code, actualReportCode);

        // NOTE: Event publishing moved to CreateHazardCommandHandler for Clean Architecture compliance
        // This ensures events are published regardless of how the hazard is created (UI, API, External, etc.)

        // NOTE: SPI automation now handled by EventBus SPIAutomationEventHandler - no direct calls needed

        Result<HazardReportTracking> createdtrackingcodeResult = await GenerateTracking(createdHazard);
        var createdTracking = createdtrackingcodeResult.Value;
        GeneratedTrackingId = createdTracking.TrackingCode;

        var hasContactEmail = !HazardReport.IsAnonymous && !string.IsNullOrWhiteSpace(HazardReport.ReportContactEmail);

        await SendSubmissionConfirmationEmailIfApplicable(createdHazard, createdTracking.TrackingCode);

        // ===============================
        // STEP 3: Process files for new hazard
        // ===============================
        await ProcessFileUpdates(createdHazard);

        // ===============================
        // SUCCESS - Show completion message for CREATE
        // ===============================



        SubmissionDateTime = DateTime.Now;
        ShowSubmissionConfirmation = false;
        ShowFinalSuccessConfirmation = !hasContactEmail;

        //This is the point where we can show the email compose dialog if applicable, but only if the user has provided a contact email and is not anonymous
        //additionally, this replicates the workflow when the report is submitted from the FlyPDX website using the API submission.

        await ShowSubmissionEmailComposeDialogIfApplicable(createdHazard, createdTracking.TrackingCode);

        _logger.LogInformation("CREATE mode completed - Report: {ReportCode}, Hazard: {HazardCode} Tracking: { TrackingCode} ", createdHazard.ReportCode, createdHazard.Code, createdTracking.TrackingCode);

        await _eventBus.PublishUIEventAsync(UINotificationEvent.Success(
            "Success",
            hasContactEmail
                ? $"Hazard report {createdHazard.Code} has been created and linked to report {createdHazard.ReportCode} with Tracking ID {createdTracking.TrackingCode}. A confirmation email was sent to {HazardReport.ReportContactEmail}."
                : $"Hazard report {createdHazard.Code} has been created and linked to report {createdHazard.ReportCode} with Tracking ID {createdTracking.TrackingCode}."));

    }

    private async Task SendSubmissionConfirmationEmailIfApplicable(Hazard createdHazard, string trackingCode)
    {
        if (HazardReport.IsAnonymous || string.IsNullOrWhiteSpace(HazardReport.ReportContactEmail))
        {
            return;
        }

        var subject = $"PDX Hazard Report Submission Confirmation - {createdHazard.ReportCode} / {createdHazard.Code}";
        var body = BuildHazardSubmissionConfirmationEmailHtml(createdHazard, trackingCode.Trim());

        var emailEvent = new EmailNotificationEvent(
            toRecipients: new List<string> { HazardReport.ReportContactEmail.Trim() },
            subject: subject,
            body: body,
            isHtmlContent: true,
            priority: EmailPriority.Normal,
            reportId: createdHazard.ReportCode,
            workflowType: "HazardSubmissionConfirmation",
            relatedEntityType: "Report",
            relatedEntityId: createdHazard.ReportCode,
            emailMetadata: new Dictionary<string, object>
            {
                { "HazardCode", createdHazard.Code },
                { "TrackingCode", trackingCode },
                { "SubmittedBy", HazardReport.ReportContactName ?? string.Empty }
            });

        var publishResult = await _eventBus.PublishIntegrationEventAsync(emailEvent, EventExecutionMode.Queued);
        if (publishResult.IsFailure)
        {
            _logger.LogWarning("Failed to queue hazard submission confirmation email for report {ReportCode}: {Error}",
                createdHazard.ReportCode,
                publishResult.Error?.Message ?? "Unknown publish error");
        }
    }

    private async Task ShowSubmissionEmailComposeDialogIfApplicable(Hazard hazard, string trackingCode)
    {
        if (HazardReport.IsAnonymous || string.IsNullOrWhiteSpace(HazardReport.ReportContactEmail))
        {
            return;
        }

        IsLoading = false;
        await InvokeAsync(StateHasChanged);

        var emailModel = new EmailComposeModel
        {
            To = new List<string> { HazardReport.ReportContactEmail.Trim() },
            Subject = $"PDX Hazard Report Submission Confirmation - {hazard.ReportCode} / {hazard.Code}",
            BodyHtml = BuildHazardSubmissionConfirmationEmailHtml(hazard, trackingCode)
        };

        var dialogResult = await _dialogService.OpenAsync<EmailComposeDialog>(
            "",
            new Dictionary<string, object?>
            {
                { "InitialModel", emailModel },
                { "DialogTitleOverride", "Hazard Submission Email Preview" }
            },
            new DialogOptions
            {
                Width = "1200px",
                Height = "760px",
                Resizable = true,
                Draggable = true,
                CloseDialogOnOverlayClick = false,
                CloseDialogOnEsc = true,
                ShowClose = true
            });

        if (dialogResult is bool sent && sent)
        {
            CloseFinalConfirmation();
        }
    }

    private string BuildHazardSubmissionConfirmationEmailHtml(Hazard createdHazard, string trackingCode)
    {
        var trackingUrl = GetTrackingUrl();
        var logoUrl = $"{_navigation.BaseUri.TrimEnd('/')}/images/PDX_SMSEmailLogo.png";

        return SMSEmailTemplateBuilder.BuildStandardEmail(
            title: "Hazard Report Confirmation",
            introHtml: "Thank you for submitting a hazard report. A copy of your report details is included below for your records.",
            summaryFields:
            [
                new SMSEmailField { Label = "Tracking Link", Value = $"<a href='{WebUtility.HtmlEncode(trackingUrl)}'>{WebUtility.HtmlEncode(trackingUrl)}</a>", ValueIsHtml = true },
                new SMSEmailField { Label = "PIN / Tracking ID", Value = trackingCode },
                new SMSEmailField { Label = "Report ID", Value = createdHazard.ReportCode },
                new SMSEmailField { Label = "Hazard ID", Value = createdHazard.Code }
            ],
            sections:
            [
                new SMSEmailSection
                {
                    Title = "Reporter Information",
                    Fields =
                    [
                        new SMSEmailField { Label = "Date Submitted", Value = HazardReport.SubmittedDate.ToString("MMMM dd, yyyy h:mm tt") },
                        new SMSEmailField { Label = "Name", Value = HazardReport.ReportContactName ?? string.Empty },
                        new SMSEmailField { Label = "Email", Value = HazardReport.ReportContactEmail ?? string.Empty },
                        new SMSEmailField { Label = "Phone", Value = HazardReport.ReportContactCell ?? string.Empty },
                        new SMSEmailField { Label = "Company", Value = HazardReport.ReportContactCompany ?? string.Empty }
                    ]
                },
                new SMSEmailSection
                {
                    Title = "Hazard Information",
                    Fields =
                    [
                        new SMSEmailField { Label = "Hazard Title", Value = HazardReport.HazardTitle ?? string.Empty },
                        new SMSEmailField { Label = "Hazard Category", Value = HazardReport.HazardCategory ?? string.Empty },
                        new SMSEmailField { Label = "Hazard Type", Value = HazardReport.HazardType ?? string.Empty },
                        new SMSEmailField { Label = "Date and Time of Event", Value = HazardReport.IncidentDateTime.ToString("MMMM dd, yyyy h:mm tt") },
                        new SMSEmailField { Label = "Location Description", Value = SelectedLocationDescription ?? HazardReport.Location ?? string.Empty },
                        new SMSEmailField { Label = "Hazard Description", Value = HazardReport.Description ?? string.Empty, IsFullWidth = true }
                    ]
                }
            ],
            footerHtml: "If any information is missing or incorrect, please reply to this message or contact <a href='mailto:SMS@flypdx.com'>SMS@flypdx.com</a>.",
            logoUrl: logoUrl);
    }


    private async Task<Result<HazardReportTracking>> GenerateTracking(Hazard createdHazard)
    {
        try
        {
            // Create tracking entity - Database will generate the actual tracking code
            HazardReportTracking hazardReportTracking = new HazardReportTracking(new HazardReportTrackingID("HT-TEMP"))
            {
                HazardCode = createdHazard.Code,
                ReportCode = createdHazard.ReportCode,
                TrackingCode = "HT-0000", // This will be replaced by database
                CreatedBy = _currentUserService?.UserDisplayName,
                CreatedDate = DateTime.UtcNow
            };

            var trackingCommand = new CreateHazardReportTrackingCommand(hazardReportTracking);
            var createdTrackingResult = await _mediator.SendAsync(trackingCommand, CancellationToken.None);

            if (createdTrackingResult.IsSuccess)
            {
                _logger.LogInformation("Tracking code generated: {TrackingCode} for Hazard: {HazardCode}", 
                    createdTrackingResult.Value.TrackingCode, createdHazard.Code);
            }
            else
            {
                _logger.LogError("Failed to generate tracking code for Hazard: {HazardCode}. Error: {Error}", 
                    createdHazard.Code, createdTrackingResult.Error?.Message);
            }

            return createdTrackingResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception generating tracking code for Hazard: {HazardCode}", createdHazard.Code);
            return Result<HazardReportTracking>.Failure<HazardReportTracking>(DomainErrors.HazardReportTrackingError.CreateFailed);
        }
    }

    /// <summary>
    /// Update hazard location (common for both CREATE and EDIT modes) - FIXED: Always create new HazardLocation since no default is created
    /// </summary>
    private async Task UpdateHazardLocation(Hazard hazard)
    {
        // Set text-based location if no geographic coordinates
        if (!HasGeoLocation && !string.IsNullOrEmpty(HazardReport.Location))
        {
            hazard.LocationArea = HazardReport.Location;
            return;
        }

        // Handle geographic location if provided
        if (HasGeoLocation)
        {
            try
            {
                HazardLocation? locationToPersist = null;

                if (!string.IsNullOrWhiteSpace(SelectedGeoLocation.Code))
                {
                    locationToPersist = new HazardLocation(new HazardLocationID(SelectedGeoLocation.Code))
                    {
                        Code = SelectedGeoLocation.Code,
                        HazardCode = hazard.Code,
                        Latitude = SelectedGeoLocation.Latitude,
                        Longitude = SelectedGeoLocation.Longitude,
                        Description = SelectedGeoLocation.Description ?? "Map selected location",
                        IsValidated = SelectedGeoLocation.IsValidated,
                        UpdatedBy = _currentUserService?.UserDisplayName,
                        UpdatedDate = DateTime.UtcNow,
                        IsValid = true
                    };
                }
                else
                {
                    var existingLocationResult = await _mediator.SendAsync(new GetHazardLocationsByHazardCodeQuery(hazard.Code), CancellationToken.None);
                    if (existingLocationResult.IsSuccess && existingLocationResult.Value?.Any() == true)
                    {
                        var existingLocation = existingLocationResult.Value
                            .OrderByDescending(l => l.UpdatedDate ?? DateTime.MinValue)
                            .ThenByDescending(l => l.DateSelected)
                            .ThenByDescending(l => l.CreatedDate ?? DateTime.MinValue)
                            .First();

                        locationToPersist = new HazardLocation(new HazardLocationID(existingLocation.Code))
                        {
                            Code = existingLocation.Code,
                            HazardCode = hazard.Code,
                            Latitude = SelectedGeoLocation.Latitude,
                            Longitude = SelectedGeoLocation.Longitude,
                            Description = SelectedGeoLocation.Description ?? "Map selected location",
                            IsValidated = SelectedGeoLocation.IsValidated,
                            UpdatedBy = _currentUserService?.UserDisplayName,
                            UpdatedDate = DateTime.UtcNow,
                            IsValid = true
                        };
                    }
                }

                if (locationToPersist is not null)
                {
                    var updateLocationResult = await _mediator.SendAsync(new UpdateHazardLocationCommand(locationToPersist), CancellationToken.None);

                    if (updateLocationResult.IsSuccess)
                    {
                        hazard.HazardLocation = updateLocationResult.Value;
                        _logger.LogInformation("HazardLocation updated with Code: {LocationCode}, Coordinates: ({Lat}, {Lng})",
                            updateLocationResult.Value.Code, SelectedGeoLocation.Latitude, SelectedGeoLocation.Longitude);
                    }
                    else
                    {
                        _logger.LogError("Failed to update HazardLocation: {Error}", updateLocationResult.Error?.Message);
                    }
                }
                else
                {
                    var hazardLocationCode = "HL-0000"; // Database will generate actual code
                    var newHazardLocation = new HazardLocation(new HazardLocationID(hazardLocationCode))
                    {
                        Code = hazardLocationCode,
                        HazardCode = hazard.Code,
                        Latitude = SelectedGeoLocation.Latitude,
                        Longitude = SelectedGeoLocation.Longitude,
                        Description = SelectedGeoLocation.Description ?? "Map selected location",
                        IsValidated = SelectedGeoLocation.IsValidated,
                        CreatedBy = _currentUserService?.UserDisplayName,
                        CreatedDate = DateTime.UtcNow,
                        IsValid = true
                    };

                    var createLocationCommand = new CreateHazardLocationCommand(newHazardLocation);
                    var locationCreateResult = await _mediator.SendAsync(createLocationCommand, CancellationToken.None);

                    if (locationCreateResult.IsSuccess)
                    {
                        var createdLocation = locationCreateResult.Value;
                        hazard.HazardLocation = createdLocation;

                        _logger.LogInformation("HazardLocation created with Code: {LocationCode}, Coordinates: ({Lat}, {Lng})",
                            createdLocation.Code, SelectedGeoLocation.Latitude, SelectedGeoLocation.Longitude);
                    }
                    else
                    {
                        _logger.LogError("Failed to create HazardLocation: {Error}", locationCreateResult.Error?.Message);
                    }
                }

                // Set coordinate information in hazard fields for backward compatibility
                hazard.LocationArea = $"Lat: {SelectedGeoLocation.Latitude:F6}, Lng: {SelectedGeoLocation.Longitude:F6}";
                if (!string.IsNullOrEmpty(SelectedGeoLocation.Description))
                {
                    hazard.LocationSubArea = SelectedGeoLocation.Description;
                }
            }
            catch (Exception locationEx)
            {
                _logger.LogWarning(locationEx, "Failed to create hazard location, but continuing with hazard creation");

                // Set location in hazard fields as fallback
                hazard.LocationArea = $"Lat: {SelectedGeoLocation.Latitude:F6}, Lng: {SelectedGeoLocation.Longitude:F6}";
                if (!string.IsNullOrEmpty(SelectedGeoLocation.Description))
                {
                    hazard.LocationSubArea = SelectedGeoLocation.Description;
                }
            }
        }
    }

    /// <summary>
    /// Process file attachments (common for both CREATE and EDIT modes)
    /// </summary>
    private async Task ProcessFileUpdates(Hazard hazard)
    {
        try
        {
            if (AttachedFiles?.Any() == true)
            {
                _logger.LogInformation("Processing {Count} cached files for Hazard: {HazardCode}",
                    AttachedFiles.Count, hazard.Code);

                foreach (var attachedFile in AttachedFiles.Where(f => f?.Data?.Length > 0))
                {
                    try
                    {
                        var fileCode = "HF-0000";

                        var hazardFile = new HazardFile(new HazardFileID(fileCode))
                        {
                            Code = fileCode,
                            HazardCode = hazard.Code,
                            ReportCode = hazard.ReportCode ?? string.Empty,
                            FileName = attachedFile.FileName,
                            FileType = Path.GetExtension(attachedFile.FileName)?.TrimStart('.') ?? "unknown",
                            ContentType = attachedFile.ContentType ?? "application/octet-stream",
                            FileSizeBytes = attachedFile.Size,
                            StorageType = "Database",
                            FilePath = null,
                            FileData = attachedFile.Data,
                            UploadedBy = HazardReport.SubmittedBy ?? _currentUserService?.UserDisplayName ?? "SYSTEM",
                            UploadedDate = DateTime.UtcNow,
                            IsActive = true,
                            IsConfidential = HazardReport.IsAnonymous
                        };

                        var createHazardFileCommand = new CreateHazardFileCommand(hazardFile);
                        var hazardFileResult = await _mediator.SendAsync(createHazardFileCommand, CancellationToken.None);

                        if (hazardFileResult.IsSuccess)
                        {
                            var createdFileId = hazardFileResult.Value.Code;
                            hazard.AddHazardFile(new HazardFileID(createdFileId));

                            _logger.LogInformation("Created {StorageType} HazardFile: {FileName} with ID: {FileId} for Hazard: {HazardCode}. FileUri: {FileUri}",
                                hazardFile.StorageType, hazardFile.FileName, createdFileId, hazard.Code, hazardFile.FilePath);
                        }
                        else
                        {
                            _logger.LogError("Failed to create HazardFile: {FileName} for Hazard: {HazardCode}. Error: {Error}",
                                attachedFile.FileName, hazard.Code, hazardFileResult.Error?.Message);
                        }
                    }
                    catch (Exception fileEx)
                    {
                        _logger.LogError(fileEx, "Exception creating HazardFile: {FileName} for Hazard: {HazardCode}",
                            attachedFile.FileName, hazard.Code);
                    }
                }
            }
            else
            {
                _logger.LogInformation("No files to process for Hazard: {HazardCode}", hazard.Code);
            }
        }
        catch (Exception fileEx)
        {
            _logger.LogError(fileEx, "Error processing files, but continuing with hazard operation");
        }
    }


    #endregion

    #region Dropdown and Smart Enum Methods

    private void InitializeDropdownOptions()
    {
        var (categories, types, departments) = DropdownHelper.InitializeHazardReportingDropdowns();
        HazardCategoryOptions = categories;
        HazardTypeOptions = types;
        DepartmentOptions = departments;
    }

    public async Task OnDepartmentChanged(string? departmentValue)
    {
        _logger.LogInformation("Submitting Department changed to: {Department}", departmentValue);

        SelectedDepartment = departmentValue;

    }

    
    public async Task OnHazardCategoryChanged(string? categoryValue)
    {
        var normalizedCategory = categoryValue ?? string.Empty;
        var previousCategory = SelectedHazardCategory ?? string.Empty;

        SelectedHazardCategory = normalizedCategory;
        HazardTypeOptions = DropdownHelper.HandleCategoryChange(normalizedCategory);

        // Clear hazard type only when category actually changed and selected type is not valid for new category.
        if (!string.Equals(previousCategory, normalizedCategory, StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrEmpty(HazardReport.HazardType) &&
            !HazardTypeOptions.Any(x => string.Equals(x.Value, HazardReport.HazardType, StringComparison.OrdinalIgnoreCase)))
        {
            HazardReport.HazardType = string.Empty;
        }

        await InvokeAsync(StateHasChanged);
    }
   
    public async Task OnHazardTypeChanged(string? hazardTypeValue)
    {
        HazardReport.HazardType = hazardTypeValue;

        if (!string.IsNullOrEmpty(hazardTypeValue))
        {
            var hazardType = HazardType.FromValue(hazardTypeValue);
            if (hazardType is not null)
            {
                _logger.LogInformation("Hazard type changed to: {HazardType}, requires regulatory: {RequiresRegulatory}",hazardType.Name, hazardType.RequiresRegulatoryReporting);

                // Could show regulatory warning if required
                if (hazardType.RequiresRegulatoryReporting)
                {
                    await _eventBus.PublishUIEventAsync(UINotificationEvent.Info("Information", $"This hazard type ({hazardType.Name}) requires regulatory reporting to appropriate authorities."));
                }
            }
        }

        await InvokeAsync(StateHasChanged);
    }

    
    public string GetHazardTypeGuidance(string? hazardTypeValue)
    {
        if (string.IsNullOrEmpty(hazardTypeValue))
            return string.Empty;

        var hazardType = HazardType.FromValue(hazardTypeValue);
        return hazardType?.GuidanceText ?? string.Empty;
    }

    
    public bool RequiresRegulatoryReporting(string? hazardTypeValue)
    {
        if (string.IsNullOrEmpty(hazardTypeValue))
            return false;

        var hazardType = HazardType.FromValue(hazardTypeValue);
        return hazardType?.RequiresRegulatoryReporting ?? false;
    }

    
    public string GetHazardCategoryDescription(string? categoryValue)
    {
        if (string.IsNullOrEmpty(categoryValue))
            return string.Empty;

        var category = HazardCategory.FromValue(categoryValue);
        return category?.Description ?? string.Empty;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Handle email input changes with validation
    /// </summary>
    public void OnEmailInput(ChangeEventArgs args)
    {
        var email = args.Value?.ToString() ?? string.Empty;
        HazardReport.ReportContactEmail = email;

        if (!string.IsNullOrWhiteSpace(email) && !IsValidEmail(email))
        {
            EmailValidationMessage = "Please enter a valid email address (RFC 5322 compliant)";
        }
        else
        {
            EmailValidationMessage = string.Empty;
        }

        StateHasChanged();
    }
    /// <summary>
    /// Initialize form defaults for new reports
    /// </summary>
    private void InitializeFormDefaults()
    {
        //var currentUser = _currentUserService.UserCode;
        var theDate = DateTime.Now; //.AddMinutes(-10);
        
        HazardReport = new HazardReportForm
        {
            SubmittedBy = "APPLICATION_SOURCE", //_currentUserService.UserDisplayName ?? "Unknown",
            SubmittedDate = new DateTime(theDate.Year, theDate.Month, theDate.Day, theDate.Hour, theDate.Minute, 0),
            IncidentDateTime = new DateTime(theDate.Year, theDate.Month, theDate.Day, theDate.Hour, theDate.Minute, 0),
        };

        SelectedGeoLocation = new HazardLocation
        {
            Latitude = 0,
            Longitude = 0,
            Description = "Not set",
            DateSelected = DateTime.UtcNow
        };

        // Initialize empty file collections
        SelectedFiles = new List<IBrowserFile>().AsReadOnly();
        AttachedFiles.Clear();

        // Reset dropdown selections
        SelectedHazardCategory = null;
        HazardTypeOptions.Clear();

        // Reset UI state
        ShowConfidentialInfo = false;
        ShowMapModal = false;
        ShowSubmissionConfirmation = false;
        ShowFinalSuccessConfirmation = false;
    }

    /// <summary>
    /// Get display text for selected location
    /// </summary>
    private string GetSelectedLocationText()
    {
        if (!HasGeoLocation) return "No location selected";

        var lat = SelectedGeoLocation.Latitude;
        var lng = SelectedGeoLocation.Longitude;
        return $"Lat: {lat:F6}, Lng: {lng:F6} - {SelectedGeoLocation.Description}";
    }

    /// <summary>
    /// Format file size for display
    /// </summary>
    private string FormatFileSize(long bytes)
    {
        const int scale = 1024;
        string[] orders = { "GB", "MB", "KB", "Bytes" };
        long max = (long)Math.Pow(scale, orders.Length - 1);

        foreach (string order in orders)
        {
            if (bytes > max)
                return $"{decimal.Divide(bytes, max):##.##} {order}";
            max /= scale;
        }
        return "0 Bytes";
    }

    /// <summary>
    /// Get display text for hazard type
    /// </summary>
    private string GetHazardTypeDisplay(string? key)
    {
        if (string.IsNullOrEmpty(key)) return HazardType.Default.Value;
        var hazardType = HazardType.FromValue(key);
        return hazardType?.Name ?? key;
    }

    /// <summary>
    /// Get display text for hazard category
    /// </summary>
    private string GetHazardCategoryDisplay(string? key)
    {
        if (string.IsNullOrEmpty(key)) return HazardCategory.Default.Value;
        var category = HazardCategory.FromValue(key);
        return category?.Name ?? key;
    }

    private string GetDepartmentDisplay(string? key)
    {
        if (string.IsNullOrEmpty(key)) return "UNKNOWN";
        var department = SMSDepartment.FromValue(key);
        return department?.Name ?? key;
    }
    #endregion

    #region File Management Methods

    /// <summary>
    /// Remove a specific file from the queue
    /// </summary>
    public async Task RemoveFile(int index)
    {
        if (index >= 0 && index < AttachedFiles.Count)
        {
            var fileToRemove = AttachedFiles[index];
            AttachedFiles.RemoveAt(index);

            // Also remove from SelectedFiles for consistency
            var selectedFilesList = SelectedFiles.ToList();
            var selectedFileToRemove = selectedFilesList.FirstOrDefault(sf =>
                sf.Name == fileToRemove.FileName && sf.Size == fileToRemove.Size);

            if (selectedFileToRemove is not null)
            {
                selectedFilesList.Remove(selectedFileToRemove);
                SelectedFiles = selectedFilesList.AsReadOnly();
            }

            _logger.LogInformation("Removed file: {FileName} from queue", fileToRemove.FileName);
            StateHasChanged();
        }
    }

    /// <summary>
    /// Clear all queued files
    /// </summary>
    public async Task ClearAllFiles()
    {
        AttachedFiles.Clear();
        SelectedFiles = new List<IBrowserFile>().AsReadOnly();
        _logger.LogInformation("Cleared all files from queue");
        StateHasChanged();
    }

    /// <summary>
    /// Clear form data with confirmation
    /// </summary>
    public async Task ClearForm()
    {
        var confirmed = await _dialogService.Confirm(
            "Are you sure you want to clear all form data?",
            "Clear Form",
            new ConfirmOptions()
            {
                OkButtonText = "Yes, Clear",
                CancelButtonText = "Cancel"
            });

        if (confirmed == true)
        {
            // Clear all form data
            HazardReport = new HazardReportForm();
            SelectedGeoLocation = new HazardLocation();

            // Clear files
            SelectedFiles = new List<IBrowserFile>().AsReadOnly();
            AttachedFiles.Clear();

            // Reset coordinates
            SelectedLatitude = 0;
            SelectedLongitude = 0;
            SelectedLocationDescription = string.Empty;

            // Reset category selection
            SelectedHazardCategory = null;
            HazardTypeOptions.Clear();

            // Reset UI state
            ShowConfidentialInfo = false;
            ShowMapModal = false;
            ShowSubmissionConfirmation = false;
            ShowFinalSuccessConfirmation = false;

            InitializeFormDefaults();
            StateHasChanged();

           await _eventBus.PublishUIEventAsync(UINotificationEvent.Info("Information", "All form data has been cleared."));

        }
    }

    #endregion
}
