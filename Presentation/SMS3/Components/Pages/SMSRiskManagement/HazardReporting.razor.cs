
using Microsoft.JSInterop;

using SMS_Application.Services;
using SMS_Domain.Entities;
using SMS_Domain.Errors;

using SMS_Shared.Configuration;

using SMS3.Components.Pages.SMSRiskManagement.Models;
using SMS3.Components.Shared.UIHelpers;
using SMS3.Configuration.Extensions;

// NEW: EventBus integration for Phase 3 workflow automation
using SMS_Application.Interfaces;
using SMS_Domain.Events;
using SMS_Domain.Enums;

namespace SMS3.Components.Pages.SMSRiskManagement;

/// <summary>
/// Hazard Reporting page - Full functionality with JavaScript mapping integration
/// Supports both CREATE and EDIT modes for comprehensive hazard reporting
/// </summary>
public partial class HazardReporting : ComponentBase, IDisposable
{
    #region Dependencies
    [Inject] private ICurrentUserService CurrentUserService { get; set; } = default!;
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<HazardReporting> _logger { get; set; } = default!;
    [Inject] private DialogService _dialogService { get; set; } = default!;
    [Inject] private SPIEventCoordinator _spiCoordinator { get; set; } = default!;

    [Inject] private INotificationHelper  _notificationHelper { get; set; } = default!;
    [Inject] private IJSRuntime _jsRuntime { get; set; } = default!;
    [Inject] private NavigationManager _navigation { get; set; } = default!;

    // NEW: EventBus integration for Phase 3 workflow automation
    [Inject] private IBaseEventBus _eventBus { get; set; } = default!;
    #endregion

    #region Route Parameters

    /// <summary>
    /// Optional hazard code parameter for editing existing hazards
    /// </summary>
    [Parameter] public string? HazardCode { get; set; }

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
    public string LocationDescription { get; set; } = string.Empty;

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
    public bool HasGeoLocation => SelectedGeoLocation?.IsValid == true;
    public bool HasValidCoordinates => SelectedLatitude != 0 && SelectedLongitude != 0;
    public string GeoLocationDisplay => HasGeoLocation ? $"Lat: {SelectedGeoLocation.Latitude:F6}, Lng: {SelectedGeoLocation.Longitude:F6}" : "No coordinates selected";
    public int DescriptionCharacterCount => HazardReport?.Description?.Length ?? 0;
        
    
    public bool IsFormValidForPreview =>
        !string.IsNullOrEmpty(HazardReport.HazardType) && !string.IsNullOrEmpty(HazardReport.HazardCategory) &&
        !string.IsNullOrEmpty(HazardReport.SubmittedBy) && !string.IsNullOrEmpty(HazardReport.SubmittingDepartment) &&
        !string.IsNullOrEmpty(HazardReport.Description) && !string.IsNullOrEmpty(HazardReport.IncidentDateTime.ToString());

    public bool IsFormValidForSubmission ()
    {
        if (!HazardReport.IsAnonymous)
            {
                return IsFormValidForPreview && 
                !string.IsNullOrEmpty(HazardReport.ReportContactName) && 
                !string.IsNullOrEmpty(HazardReport.ReportContactEmail) && 
                        (HasGeoLocation || !string.IsNullOrEmpty(HazardReport.Location)) && DescriptionCharacterCount <= 2000;
            }

        else
            {
            return IsFormValidForPreview &&
                (HasGeoLocation || !string.IsNullOrEmpty(HazardReport.Location)) && DescriptionCharacterCount <= 2000;
            }
                    
    }
        

    public string PageTitle => IsEditMode ? $"Edit Report - {EditReportCode}" : "Submit Hazard Report";
    public string PageSubtitle => IsEditMode ? "Modify existing hazard report information" : "Report safety hazards and incidents for SMS processing and risk assessment";

    /// <summary>
    /// Visual validation helpers for default values requiring attention
    /// </summary>
    public bool IsHazardCategoryDefault => HazardReport?.HazardCategory == HazardCategory.Default.Value;
    public bool IsHazardTypeDefault => HazardReport?.HazardType == HazardType.Default.Value;
    public bool HasDefaultHazardClassification => IsHazardCategoryDefault || IsHazardTypeDefault;

    // Airport coordinates //GOLDKEY
    private double AirportCenterLatitude => 45.58808;
    private double AirportCenterLongitude => -122.592430;
    private int DefaultZoomLevel => 20;

    private IJSObjectReference? _mapModule;
    private DotNetObjectReference<HazardReporting>? _dotNetRef;

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
            Console.Write(CurrentUserService?.UserDisplayName);
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
                IsEditMode = true;
                EditReportCode = reportCode;

                _logger.LogInformation("Edit mode detected for report: {ReportCode} (via query parameter)", reportCode);

                // Load the existing report data
                await LoadReportForEditingAsync(reportCode);
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
            await _notificationHelper.ShowErrorAsync( "Unable to determine edit mode. Defaulting to create mode.", 5000);

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
                if (primaryHazard.HazardLocation is not null)
                {
                    SelectedGeoLocation = new HazardLocation
                    {
                        Latitude = primaryHazard.HazardLocation.Latitude ?? 0,
                        Longitude = primaryHazard.HazardLocation.Longitude ?? 0,
                        Description = primaryHazard.HazardLocation.Description ?? string.Empty,
                        DateSelected = DateTime.UtcNow,
                        IsValid = (primaryHazard.HazardLocation.Latitude ?? 0) != 0 && (primaryHazard.HazardLocation.Longitude ?? 0) != 0
                    };

                    SelectedLatitude = SelectedGeoLocation.Latitude ?? 0;
                    SelectedLongitude = SelectedGeoLocation.Longitude ?? 0;
                    LocationDescription = SelectedGeoLocation.Description ?? "";

                    HazardReport.Location = "MAP_LOCATION";

                    _logger.LogInformation("Loaded geographic location: {Lat}, {Lng}",
                        SelectedGeoLocation.Latitude, SelectedGeoLocation.Longitude);
                }

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
                    IsAnonymous = EditingReport.IsAnonymous
                };

                // Clear category-related fields
                SelectedHazardCategory = null;
                HazardTypeOptions.Clear();
                DepartmentOptions.Clear();
                SelectedDepartment =null;
            }
            await _notificationHelper.ShowInfoAsync($"Loaded report {reportCode} for editing.", 5000);
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading report for editing: {ReportCode}", reportCode);
            await _notificationHelper.ShowErrorAsync("Failed to load report for editing. Redirecting to Reports page.", 5000);
            
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
                    EditingHazard.HazardLocation = locationResult.Value.FirstOrDefault();
                    _logger.LogInformation("Successfully loaded HazardLocation for hazard {HazardCode}: Lat={Lat}, Lng={Lng}", 
                        hazardCode, EditingHazard.HazardLocation?.Latitude, EditingHazard.HazardLocation?.Longitude);
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
            var category = hazardType is not null ? HazardCategory.FromValue(hazardType.Category) : null;

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

                // Use report data if available, otherwise use defaults
                IncidentDateTime = EditingReport?.IncidentDateTime ?? DateTime.Now,
                SubmittedBy = EditingReport?.SubmittedBy ?? CurrentUserService?.UserDisplayName ?? "Unknown User",
                SubmittedDate = EditingReport?.SubmittedDate ?? DateTime.Now,
                SubmittingDepartment = EditingReport?.SubmittingDepartment ?? "",
                SubmittingDepartmentJobFunction = EditingReport?.SubmittingDepartmentJobFunction ?? "",
                ReportContactName = EditingReport?.ReportContactName ?? "",
                ReportContactCell = EditingReport?.ReportContactCell ?? "",
                ReportContactEmail = EditingReport?.ReportContactEmail ?? "",
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
                    Latitude = EditingHazard.HazardLocation.Latitude ?? 0,
                    Longitude = EditingHazard.HazardLocation.Longitude ?? 0,
                    Description = EditingHazard.HazardLocation.Description ?? string.Empty,
                    DateSelected = DateTime.UtcNow,
                    IsValid = (EditingHazard.HazardLocation.Latitude ?? 0) != 0 && (EditingHazard.HazardLocation.Longitude ?? 0) != 0
                };

                SelectedLatitude = SelectedGeoLocation.Latitude ?? 0;
                SelectedLongitude = SelectedGeoLocation.Longitude ?? 0;
                LocationDescription = SelectedGeoLocation.Description ?? "";

                HazardReport.Location = "MAP_LOCATION";

                _logger.LogInformation("Loaded geographic location: {Lat}, {Lng}",SelectedGeoLocation.Latitude, SelectedGeoLocation.Longitude);
            }

            _logger.LogInformation("Successfully loaded hazard {HazardCode} for editing - Category: {Category}, Type: {Type}",
                hazardCode, SelectedHazardCategory, HazardReport.HazardType);

            await _notificationHelper.ShowInfoAsync($"Loaded hazard {hazardCode} for editing.", 5000);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading hazard for editing: {HazardCode}", hazardCode);
            await _notificationHelper.ShowErrorAsync("Failed to load hazard for editing. Redirecting to Hazards page.", 5000);

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
                    _logger.LogWarning("Selected type {Type} not found in category {Category}",
                        currentSelectedType, category.Value);
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
                await _notificationHelper.ShowWarningAsync(  "Please complete all required fields before submitting.", 5000);
                return;
            }

            await ShowSubmissionConfirmationDialog();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during form submission");
            await _notificationHelper.ShowErrorAsync("An error occurred while submitting your report. Please try again.", 5000);
            
        }
    }

       
    /// <summary>
    /// Handle InputFile change event - this will accumulate files properly
    /// </summary>
    public async Task OnInputFileChange(UploadChangeEventArgs args)
    {
        var newFiles = args.Files; // Allow up to 10 files at once
        _logger.LogInformation("?? OnInputFileChange called with {Count} new files", newFiles?.Count() ?? 0);

        if (newFiles?.Any() == true)
        {
            // Process files immediately to avoid the "file list may have changed" error
            var successfullyProcessedFiles = new List<AttachedFile>();
            var failedFiles = new List<string>();

            foreach (var newFile in args.Files)
            {
                try
                {
                    // Check for duplicate first (before processing)
                    var isDuplicate = AttachedFiles.Any(existing =>
                        existing.FileName.Equals(newFile.Name, StringComparison.OrdinalIgnoreCase) &&
                        existing.Size == newFile.Size);

                    if (isDuplicate)
                    {
                        _logger.LogInformation("?? Skipped duplicate file: {FileName}", newFile.Name);
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
                    _logger.LogInformation("? Successfully processed file: {FileName} ({Size} bytes)", newFile.Name, newFile.Size);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "? Error processing file: {FileName}", newFile.Name);
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
                await _notificationHelper.ShowWarningAsync($"Added {successfullyProcessedFiles.Count} file(s). Failed to process {failedFiles.Count} file(s). Total: {AttachedFiles.Count} files queued.", 5000);
            } else if (successfullyProcessedFiles.Any())
            {
                await _notificationHelper.ShowSuccessAsync($"Added {successfullyProcessedFiles.Count} file(s) to the queue. Total: {AttachedFiles.Count} files.", 5000);
            }
            else if (failedFiles.Any())
            {
                await _notificationHelper.ShowErrorAsync($"Failed to process {failedFiles.Count} file(s). This may be due to file size limits or browser restrictions.", 5000);
            }

            _logger.LogInformation("?? File processing completed: {Success} successful, {Failed} failed. Total queued: {Total}",successfullyProcessedFiles.Count, failedFiles.Count, AttachedFiles.Count);
        }
        else
        {
            _logger.LogInformation("?? No files provided to OnInputFileChange");
        }

        StateHasChanged();
    }

    #endregion

    #region Location Methods

    /// <summary>
    /// Open map selector modal
    /// </summary>
    public async Task OpenMapSelector()
    {
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
                await _mapModule.InvokeVoidAsync("initializeMap",AirportCenterLatitude, AirportCenterLongitude, DefaultZoomLevel, _dotNetRef);

                _logger.LogInformation("Map reinitialized for modal opening");

                // Restore existing location if we have one
                if (HasGeoLocation)
                {
                    await Task.Delay(500); // Give map time to initialize

                    await _mapModule.InvokeVoidAsync("setLocationFromCoordinates",(double)SelectedGeoLocation.Latitude, (double)SelectedGeoLocation.Longitude,SelectedGeoLocation.Description);

                    // Update the form fields to match the restored location
                    SelectedLatitude = SelectedGeoLocation.Latitude ?? 0;
                    SelectedLongitude = SelectedGeoLocation.Longitude ?? 0;
                    LocationDescription = SelectedGeoLocation.Description ?? "";

                    _logger.LogInformation("Existing location restored: {Lat}, {Lng}", SelectedGeoLocation.Latitude, SelectedGeoLocation.Longitude);

                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing map in OpenMapSelector");
                await _notificationHelper.ShowWarningAsync("Could not initialize map. Please try refreshing the page.", 5000);
            }
        }
        await _notificationHelper.ShowInfoAsync("Click on the map to select the hazard location.", 3000);
    }

    /// <summary>
    /// Close map selector modal
    /// </summary>
    public void CloseMapSelector()
    {
        ShowMapModal = false;
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

            await _notificationHelper.ShowWarningAsync("Please click on the map to select a location first.", 5000);
            return;
        }

        // Set the geolocation data
        SelectedGeoLocation = new HazardLocation
        {
            Latitude = SelectedLatitude,
            Longitude = SelectedLongitude,
            Description = LocationDescription,// string.IsNullOrEmpty(LocationDescription) ? 
                                              //$"Map Location ({SelectedLatitude:F6}, {SelectedLongitude:F6})" : LocationDescription,
            DateSelected = DateTime.UtcNow
        };

        // Update the form location to indicate map location is selected
        HazardReport.Location = "MAP_LOCATION";

        ShowMapModal = false;
        StateHasChanged();

        await _notificationHelper.ShowSuccessAsync( $"Location selected: {GeoLocationDisplay}", 5000);
        
    }

    /// <summary>
    /// Clear map selection
    /// </summary>
    public async Task ClearMapSelection()
    {
        SelectedLatitude = 0;
        SelectedLongitude = 0;
        LocationDescription = string.Empty;
        SelectedGeoLocation = new HazardLocation();
        HazardReport.Location = "";

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
        await _notificationHelper.ShowInfoAsync( "Map selection has been cleared.", 5000);
        
    }

    [JSInvokable("OnMapLocationSelected")]
    public async Task OnMapLocationSelected(double latitude, double longitude, string description)
    {
        SelectedLatitude = (decimal)latitude;
        SelectedLongitude = (decimal)longitude;
        LocationDescription = description; // This will set the description automatically!

        // Validate that we received proper coordinates
        if (latitude == 0 && longitude == 0)
        {
            _logger?.LogWarning("Received zero coordinates from map click");
        }

        _logger?.LogInformation("Map location received from JS: Lat={Lat}, Lng={Lng}, Description={Desc}", latitude, longitude, description);

        await InvokeAsync(StateHasChanged);

        _logger?.LogInformation("Map location selected: {Lat}, {Lng} - HasValidCoordinates: {HasValid}", latitude, longitude, HasValidCoordinates);
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
            await _notificationHelper.ShowWarningAsync( "Please complete all required fields before submitting.", 5000);
            return;
        }

        _logger.LogInformation("?? Preparing for submission with {Count} cached files ready", AttachedFiles?.Count ?? 0);
        if (AttachedFiles?.Any() == true)
        {
            foreach (var file in AttachedFiles)
            {
                _logger.LogInformation("?? Cached file ready for submission: {FileName} ({Size} bytes, {DataSize} bytes cached)", file.FileName, file.Size, file.Data?.Length ?? 0);
            }
        }

        ShowSubmissionConfirmation = true;
        StateHasChanged(); // Force UI update to hide buttons
    }

    /// <summary>
    /// Cancel submission
    /// </summary>
    public void CancelSubmission()
    {
        ShowSubmissionConfirmation = false;
        StateHasChanged(); // Force UI update to show buttons again
        _notificationHelper.ShowInfoAsync( "You can continue editing your report.", 5000);
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
            LocationDescription = string.Empty;

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
                await _notificationHelper.ShowWarningAsync( "No report information available to print.", 5000);
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

            await _notificationHelper.ShowErrorAsync( "Failed to print confirmation. Please try again or save the page.", 5000);
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
        try
        {
            // Validation
            if (!IsFormValidForSubmission())
            {
                ShowSubmissionConfirmation = true;
                StateHasChanged();
                await _notificationHelper.ShowWarningAsync( "Please complete all required fields before submitting.", 5000);
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
            _logger.LogError(ex, "? Error during hazard report submission in {Mode} mode",
                IsEditMode ? "EDIT" : "CREATE");

            ShowSubmissionConfirmation = false;
            await _notificationHelper.ShowErrorAsync( $"An error occurred while {(IsEditMode ? "updating" : "saving")} your report. Please try again.", 5000);
            
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
        EditingReport.Status = ReportStatus.ReadyForProcessing;
        EditingReport.UpdatedDate = DateTime.UtcNow;
        EditingReport.UpdatedBy = CurrentUserService.UserCode;

        var updateReportCommand = new UpdateReportCommand(EditingReport);
        var reportUpdateResult = await _mediator.SendAsync(updateReportCommand, CancellationToken.None);

        if (reportUpdateResult.IsFailure)
        {
            throw new Exception($"Failed to update report: {reportUpdateResult.Error?.Message}");
        }

        _logger.LogInformation("? Report {ReportCode} updated successfully", EditReportCode);

        // ===============================
        // STEP 2: Update the existing Hazard
        // ===============================
        EditingHazard!.Name = $"{HazardReport.HazardCategory} - {HazardReport.HazardType}";
        EditingHazard.Description = HazardReport.Description;
        EditingHazard.HazardCategory = HazardReport.HazardCategory;
        EditingHazard.HazardType = HazardReport.HazardType; // This is the actual selected hazard type, not "Initial"
        
        EditingHazard.UpdatedDate = DateTime.UtcNow;
        EditingHazard.UpdatedBy = CurrentUserService.UserDisplayName;

        // Handle location updates
        await UpdateHazardLocation(EditingHazard);

        var updateHazardCommand = new UpdateHazardCommand(EditingHazard);
        var hazardUpdateResult = await _mediator.SendAsync(updateHazardCommand, CancellationToken.None);

        if (hazardUpdateResult.IsFailure)
        {
            throw new Exception($"Failed to update hazard: {hazardUpdateResult.Error?.Message}");
        }

        var updatedHazard = hazardUpdateResult.Value;
        _logger.LogInformation("? Hazard {HazardCode} updated successfully", EditHazardCode);

        // ===============================
        // STEP 3: Handle file updates (if any new files)
        // ===============================
        await ProcessFileUpdates(updatedHazard);

        // ===============================
        // SUCCESS - Show completion message for EDIT
        // ===============================
        GeneratedHazardId = updatedHazard.Code;
        GeneratedReportId = updatedHazard.ReportCode;
        SubmissionDateTime = DateTime.Now;
        ShowSubmissionConfirmation = false;
        ShowFinalSuccessConfirmation = true;

        _logger.LogInformation("? EDIT mode completed - Report: {ReportCode}, Hazard: {HazardCode}",
            updatedHazard.ReportCode, updatedHazard.Code);

        await _notificationHelper.ShowSuccessAsync( $"Report {updatedHazard.ReportCode} and hazard {updatedHazard.Code} have been updated.", 5000);

        
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

            Description = HazardReport.Description,
            Stage = "INITIAL",
            Status = ReportStatus.ReadyForProcessing, //needs validation
            CreatedBy = CurrentUserService.UserDisplayName,
            CreatedDate = DateTime.UtcNow
        };

        var reportResult = await _mediator.SendAsync(new CreateReportCommand(report), CancellationToken.None);

        if (reportResult.IsFailure)
        {
            throw new Exception($"Failed to create report: {reportResult.Error?.Message}");
        }

        var actualReportCode = reportResult.Value.Code;
        _logger.LogInformation("? Report created with Code: {ReportCode}", actualReportCode);

        // ===============================
        // STEP 2: Create new Hazard
        // ===============================
        var hazard = new Hazard(new HazardID("HZ-0000"))
        {
            Code = "HZ-0000",
            Name = $"{HazardReport.HazardCategory} - {HazardReport.HazardType}",
            Description = HazardReport.Description,
            HazardCategory = HazardReport.HazardCategory,
            HazardType = HazardReport.HazardType,
            
            ReportCode = actualReportCode,
            IsInitialHazard = true ,
            CreatedBy = CurrentUserService.UserDisplayName,
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

        _logger.LogInformation("? Hazard created with Code: {HazardCode}, linked to Report: {ReportCode}", createdHazard.Code, actualReportCode);

        // NEW: PHASE 3 - EventBus Integration for Complete Workflow Automation ??
        try
        {
            _logger.LogInformation("?? Phase 3: Publishing HazardCreatedEvent for complete workflow automation");

            // Determine hazard priority based on type/category
            var hazardPriority = DetermineHazardPriority(createdHazard.HazardType, createdHazard.HazardCategory);

            // Create and publish HazardCreatedEvent for complete workflow automation
            var hazardCreatedEvent = new HazardCreatedEvent(
                hazardId: createdHazard.Code,
                hazardCode: createdHazard.Code,
                hazardName: createdHazard.Name ?? "Unnamed Hazard",
                hazardType: createdHazard.HazardType ?? "Unknown",
                hazardCategory: createdHazard.HazardCategory ?? "Unknown",
                description: createdHazard.Description ?? "No description",
                locationArea: createdHazard.LocationArea ?? HazardReport.Location ?? "Unknown Location",
                reportCode: actualReportCode,
                createdBy: createdHazard.CreatedBy ?? CurrentUserService.UserDisplayName,
                createdDate: createdHazard.CreatedDate ?? DateTime.UtcNow,
                isInitialHazard: createdHazard.IsInitialHazard,
                priority: hazardPriority,
                latitude: createdHazard.HazardLocation?.Latitude,
                longitude: createdHazard.HazardLocation?.Longitude
            );

            // Publish the domain event - this triggers the complete workflow
            var eventResult = await _eventBus.PublishDomainEventAsync(hazardCreatedEvent);

            if (eventResult.IsSuccess)
            {
                _logger.LogInformation("? Phase 3: HazardCreatedEvent published successfully for {HazardCode} - Complete workflow initiated", createdHazard.Code);
            }
            else
            {
                _logger.LogWarning("?? Phase 3: Failed to publish HazardCreatedEvent for {HazardCode}: {Error} - continuing with submission", 
                    createdHazard.Code, eventResult.Error.Message);
            }
        }
        catch (Exception eventEx)
        {
            // Don't fail the entire submission if EventBus fails
            _logger.LogWarning(eventEx, "?? Phase 3: EventBus integration failed for {HazardCode} - continuing with submission", createdHazard.Code);
        }

        // NOTE: SPI automation now handled by EventBus SPIAutomationEventHandler - no direct calls needed

        Result<HazardReportTracking> createdtrackingcodeResult = await GenerateTracking(createdHazard);
        var createdTracking = createdtrackingcodeResult.Value;
        GeneratedTrackingId = createdTracking.TrackingCode;

        // ===============================
        // STEP 3: Process files for new hazard
        // ===============================
        await ProcessFileUpdates(createdHazard);

        // ===============================
        // SUCCESS - Show completion message for CREATE
        // ===============================



        SubmissionDateTime = DateTime.Now;
        ShowSubmissionConfirmation = false;
        ShowFinalSuccessConfirmation = true;

        _logger.LogInformation("? CREATE mode completed - Report: {ReportCode}, Hazard: {HazardCode} Tracking: { TrackingCode} ", createdHazard.ReportCode, createdHazard.Code, createdTracking.TrackingCode);

        await _notificationHelper.ShowSuccessAsync( $"Hazard report {createdHazard.Code} has been created and linked to report {createdHazard.ReportCode} with Tracking ID {createdTracking.TrackingCode}.", 5000);

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
                CreatedBy = CurrentUserService?.UserDisplayName,
                CreatedDate = DateTime.UtcNow
            };

            var trackingCommand = new CreateHazardReportTrackingCommand(hazardReportTracking);
            var createdTrackingResult = await _mediator.SendAsync(trackingCommand, CancellationToken.None);

            if (createdTrackingResult.IsSuccess)
            {
                _logger.LogInformation("? Tracking code generated: {TrackingCode} for Hazard: {HazardCode}", 
                    createdTrackingResult.Value.TrackingCode, createdHazard.Code);
            }
            else
            {
                _logger.LogError("? Failed to generate tracking code for Hazard: {HazardCode}. Error: {Error}", 
                    createdHazard.Code, createdTrackingResult.Error?.Message);
            }

            return createdTrackingResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Exception generating tracking code for Hazard: {HazardCode}", createdHazard.Code);
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
                // ?? FIXED: Always create new location since CreateHazardCommandHandler no longer auto-creates default locations
                var hazardLocationCode = "HL-0000"; // Database will generate actual code
                var newHazardLocation = new HazardLocation(new HazardLocationID(hazardLocationCode))
                {
                    Code = hazardLocationCode,
                    HazardCode = hazard.Code,
                    Latitude = SelectedGeoLocation.Latitude,
                    Longitude = SelectedGeoLocation.Longitude,
                    Description = SelectedGeoLocation.Description ?? "Map selected location",
                    CreatedBy = CurrentUserService?.UserDisplayName,
                    CreatedDate = DateTime.UtcNow,
                    IsValid = true
                };

                // Create the new HazardLocation
                var createLocationCommand = new CreateHazardLocationCommand(newHazardLocation);
                var locationCreateResult = await _mediator.SendAsync(createLocationCommand, CancellationToken.None);

                if (locationCreateResult.IsSuccess)
                {
                    var createdLocation = locationCreateResult.Value;
                    hazard.HazardLocation = createdLocation;

                    _logger.LogInformation("? HazardLocation created with Code: {LocationCode}, Coordinates: ({Lat}, {Lng})",
                        createdLocation.Code, SelectedGeoLocation.Latitude, SelectedGeoLocation.Longitude);
                }
                else
                {
                    _logger.LogError("? Failed to create HazardLocation: {Error}", locationCreateResult.Error?.Message);
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
                _logger.LogInformation("?? Processing {Count} cached files for Hazard: {HazardCode}",
                    AttachedFiles.Count, hazard.Code);

                foreach (var attachedFile in AttachedFiles.Where(f => f?.Data?.Length > 0))
                {
                    try
                    {
                        var fileData = attachedFile.Data;
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
                            FileData = fileData,
                            UploadedBy = HazardReport.SubmittedBy ?? CurrentUserService?.UserDisplayName,
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

                            _logger.LogInformation("? Created HazardFile: {FileName} with ID: {FileId} for Hazard: {HazardCode}",
                                attachedFile.FileName, createdFileId, hazard.Code);
                        }
                        else
                        {
                            _logger.LogError("? Failed to create HazardFile: {FileName} for Hazard: {HazardCode}. Error: {Error}",
                                attachedFile.FileName, hazard.Code, hazardFileResult.Error?.Message);
                        }
                    }
                    catch (Exception fileEx)
                    {
                        _logger.LogError(fileEx, "? Exception creating HazardFile: {FileName} for Hazard: {HazardCode}",
                            attachedFile.FileName, hazard.Code);
                    }
                }
            }
            else
            {
                _logger.LogInformation("?? No files to process for Hazard: {HazardCode}", hazard.Code);
            }
        }
        catch (Exception fileEx)
        {
            _logger.LogError(fileEx, "?? Error processing files, but continuing with hazard operation");
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
        SelectedHazardCategory = categoryValue ?? string.Empty;
        HazardReport.HazardType = string.Empty;
        HazardTypeOptions = DropdownHelper.HandleCategoryChange(categoryValue);

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
                    _notificationHelper.ShowInfoAsync( $"This hazard type ({hazardType.Name}) requires regulatory reporting to appropriate authorities.", 5000);
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
        //var currentUser = CurrentUserService.UserCode;
        var theDate = DateTime.Now; //.AddMinutes(-10);
        
        HazardReport = new HazardReportForm
        {
            SubmittedBy = CurrentUserService.UserDisplayName ?? "Unknown",
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
        if (string.IsNullOrEmpty(key)) return "UNKNOWN";
        var hazardType = HazardType.FromValue(key);
        return hazardType?.Name ?? key;
    }

    /// <summary>
    /// Get display text for hazard category
    /// </summary>
    private string GetHazardCategoryDisplay(string? key)
    {
        if (string.IsNullOrEmpty(key)) return "UNKNOWN";
        var category = HazardCategory.FromValue(key);
        return category?.Name ?? key;
    }

    /// <summary>
    /// NEW: Phase 3 - Determine hazard priority based on type and category for EventBus workflow
    /// Business logic to assign priority levels for automated workflow routing
    /// </summary>
    private HazardPriority DetermineHazardPriority(string? hazardType, string? hazardCategory)
    {
        try
        {
            // Business rules for priority determination
            var type = hazardType?.ToUpper() ?? "";
            var category = hazardCategory?.ToUpper() ?? "";

            // Critical priority conditions
            if (type.Contains("STRUCTURAL") || type.Contains("FIRE") || type.Contains("EXPLOSIVE") ||
                category.Contains("SAFETY_CRITICAL") || category.Contains("REGULATORY"))
            {
                return HazardPriority.Critical;
            }

            // High priority conditions  
            if (type.Contains("EQUIPMENT") || type.Contains("MAINTENANCE") || type.Contains("OPERATIONAL") ||
                category.Contains("OPERATIONAL") || category.Contains("MAINTENANCE"))
            {
                return HazardPriority.High;
            }

            // Medium priority conditions
            if (type.Contains("ENVIRONMENTAL") || type.Contains("DOCUMENTATION") ||
                category.Contains("ENVIRONMENTAL") || category.Contains("PROCESS"))
            {
                return HazardPriority.Medium;
            }

            // Default to Medium for unknown types
            return HazardPriority.Medium;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error determining hazard priority for Type: {Type}, Category: {Category} - defaulting to Medium",
                hazardType, hazardCategory);
            return HazardPriority.Medium;
        }
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
            LocationDescription = string.Empty;

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

            _notificationHelper.ShowInfoAsync( "All form data has been cleared.", 5000);

        }
    }

    #endregion
}