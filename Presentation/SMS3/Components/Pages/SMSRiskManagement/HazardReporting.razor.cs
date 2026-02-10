using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.JSInterop;

using SMS3.Components.Pages.SMSRiskManagement.Models;

namespace SMS3.Components.Pages.SMSRiskManagement;

/// <summary>
/// Hazard Reporting page - Full functionality with JavaScript mapping integration
/// Supports both CREATE and EDIT modes for comprehensive hazard reporting
/// </summary>
public partial class HazardReporting : ComponentBase, IDisposable
{
    #region Dependencies
    [Inject] private AuthenticationService AuthService { get; set; } = default!;
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<HazardReporting> Logger { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    #endregion

    #region Properties and Fields

    /// <summary>
    /// Main form data object
    /// </summary>
    public HazardReportForm HazardReport { get; set; } = new();

    /// <summary>
    /// Geographic location data
    /// </summary>
    public GeoLocationData SelectedGeoLocation { get; set; } = new();

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

    /// <summary>
    /// Show preview modal
    /// </summary>
    public bool ShowPreview { get; set; }

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
    public int SubmittingDepartmentCharacterCount => HazardReport?.SubmittingDepartment?.Length ?? 0;
    public bool IsSubmittingDepartmentValid => SubmittingDepartmentCharacterCount <= 200;
    
    public bool IsFormValidForPreview =>
        !string.IsNullOrEmpty(HazardReport.HazardType) &&
        !string.IsNullOrEmpty(HazardReport.SubmittedBy) &&
        !string.IsNullOrEmpty(HazardReport.Description);

    public bool IsFormValidForSubmission =>
        IsFormValidForPreview &&
        (HasGeoLocation || !string.IsNullOrEmpty(HazardReport.Location)) &&
        DescriptionCharacterCount <= 2000;

    public string PageTitle => IsEditMode ? $"Edit Report - {EditReportCode}" : "Submit Hazard Report";
    public string PageSubtitle => IsEditMode ? "Modify existing hazard report information" : "Report safety hazards and incidents for SMS processing and risk assessment";

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
            Console.Write(AuthService.CurrentUserDisplayName);
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
                _mapModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "/js/hazard-map.js");
                Logger.LogInformation("Map module loaded successfully");
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Could not load JavaScript map module");
            }
        }

        // Don't auto-initialize the map here - let OpenMapSelector handle it
        // This prevents conflicts between automatic and manual initialization
    }

    public void Dispose()
    {
        _mapModule?.DisposeAsync();
        _dotNetRef?.Dispose();
    }

    #endregion

    #region Edit Mode Methods

    /// <summary>
    /// Check if we're in edit mode based on query parameters
    /// </summary>
    private async Task CheckForEditModeAsync()
    {
        try
        {
            var uri = new Uri(Navigation.Uri);
            var queryParams = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);

            if (queryParams.TryGetValue("mode", out var mode) && mode == "edit" &&
                queryParams.TryGetValue("reportCode", out var reportCode) && !string.IsNullOrEmpty(reportCode))
            {
                IsEditMode = true;
                EditReportCode = reportCode;

                Logger.LogInformation("Edit mode detected for report: {ReportCode}", reportCode);

                // Load the existing report data
                await LoadReportForEditingAsync(reportCode);
            }
            else
            {
                IsEditMode = false;
                EditReportCode = null;
                EditingReport = null;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error checking for edit mode");
            IsEditMode = false;

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Edit Mode Error",
                Detail = "Unable to determine edit mode. Defaulting to create mode.",
                Duration = 3000
            });
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

            Logger.LogInformation("Loading report {ReportCode} for editing", reportCode);

            // Get the report details
            var reportQuery = new GetReportByCodeQuery(new ReportID(reportCode));
            var reportResult = await Mediator.SendAsync(reportQuery, CancellationToken.None);

            if (reportResult.IsFailure || reportResult.Value == null)
            {
                throw new InvalidOperationException($"Report {reportCode} not found");
            }

            EditingReport = reportResult.Value;

            // Get associated hazards to populate the form
            var hazardsQuery = new GetHazardsByReportCodeQuery(new ReportID(reportCode));
            var hazardsResult = await Mediator.SendAsync(hazardsQuery, CancellationToken.None);

            if (hazardsResult.IsSuccess && hazardsResult.Value?.Any() == true)
            {
                // Use the first hazard to populate the form (primary hazard)
                var primaryHazard = hazardsResult.Value.OrderBy(h => h.IsInitialHazard).First();

                // Store the editing hazard for update operations
                EditingHazard = primaryHazard;
                EditHazardCode = primaryHazard.Code;

                Logger.LogInformation("Found primary hazard {HazardCode} with type: {HazardType}",
                    primaryHazard.Code, primaryHazard.HazardType);

                // Try to determine category from hazard type
                var hazardType = HazardType.FromValue(primaryHazard.HazardType ?? "");
                var category = hazardType != null ? HazardCategory.FromValue(hazardType.Category) : null;

                Logger.LogInformation("Determined category: {Category} from hazard type: {HazardType}",
                    category?.Value ?? "NULL", primaryHazard.HazardType);

                //Step 0 Basic Reporting details.
                



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
                    HazardType = primaryHazard.HazardType,
                    Description = primaryHazard.Description,
                    Location = primaryHazard.LocationArea
                };


                // STEP 3: Load the hazard types for the category (this will populate HazardTypeOptions)
                if (category != null)
                {
                    Logger.LogInformation("Loading hazard types for category: {Category}", category.Value);

                    // This will populate HazardTypeOptions and NOT clear HazardReport.HazardType
                    await LoadHazardTypesForCategory(category.Value, preserveSelectedType: true);

                    Logger.LogInformation("Loaded {Count} hazard types for category {Category}. Current type: {Type}",
                        HazardTypeOptions.Count, category.Value, HazardReport.HazardType);
                }
                else
                {
                    Logger.LogWarning("No category found for hazard type: {HazardType}", primaryHazard.HazardType);
                    // Clear hazard types if no category
                    HazardTypeOptions.Clear();
                }

                // STEP 4: Handle geographic location data
                if (primaryHazard.HazardLocation != null)
                {
                    SelectedGeoLocation = new GeoLocationData
                    {
                        Latitude = primaryHazard.HazardLocation.Latitude ?? 0,
                        Longitude = primaryHazard.HazardLocation.Longitude ?? 0,
                        Description = primaryHazard.HazardLocation.Description,
                        SelectedDateTime = DateTime.UtcNow
                    };

                    SelectedLatitude = SelectedGeoLocation.Latitude;
                    SelectedLongitude = SelectedGeoLocation.Longitude;
                    LocationDescription = SelectedGeoLocation.Description ?? "";

                    HazardReport.Location = "MAP_LOCATION";

                    Logger.LogInformation("Loaded geographic location: {Lat}, {Lng}",
                        SelectedGeoLocation.Latitude, SelectedGeoLocation.Longitude);
                }

                Logger.LogInformation("Successfully loaded report {ReportCode} with hazard {HazardCode} - Category: {Category}, Type: {Type}",
                    reportCode, primaryHazard.Code, SelectedHazardCategory, HazardReport.HazardType);
            }
            else
            {
                // No hazards found, use report data
                Logger.LogWarning("No hazards found for report {ReportCode}, using report data", reportCode);

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

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Info,
                Summary = "Report Loaded",
                Detail = $"Loaded report {reportCode} for editing.",
                Duration = 3000
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading report for editing: {ReportCode}", reportCode);

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Load Failed",
                Detail = "Failed to load report for editing. Redirecting to Reports page.",
                Duration = 5000
            });

            // Redirect back to reports on failure
            Navigation.NavigateTo("/SMSRiskManagement/Reports");
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
        if (category != null)
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
                    Logger.LogWarning("Selected type {Type} not found in category {Category}",
                        currentSelectedType, category.Value);
                }
            }

            Logger.LogInformation("Loaded {Count} hazard types for category: {Category}",
                HazardTypeOptions.Count, category.Name);
        }
        else
        {
            HazardTypeOptions.Clear();
            Logger.LogWarning("Category not found: {CategoryValue}", categoryValue);
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
            Logger.LogInformation("Form submit triggered with data: HazardType={HazardType}, SubmittedBy={SubmittedBy}", formData.HazardType, formData.SubmittedBy);

            if (!IsFormValidForSubmission)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Warning,
                    Summary = "Form Validation",
                    Detail = "Please complete all required fields before submitting.",
                    Duration = 4000
                });
                return;
            }

            await ShowSubmissionConfirmationDialog();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during form submission");

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Submission Error",
                Detail = "An error occurred while submitting your report. Please try again.",
                Duration = 5000
            });
        }
    }

       
    /// <summary>
    /// Handle InputFile change event - this will accumulate files properly
    /// </summary>
    public async Task OnInputFileChange(UploadChangeEventArgs args)
    {
        var newFiles = args.Files; // Allow up to 10 files at once
        Logger.LogInformation("🔄 OnInputFileChange called with {Count} new files", newFiles?.Count() ?? 0);

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
                        Logger.LogInformation("⚠️ Skipped duplicate file: {FileName}", newFile.Name);
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
                    Logger.LogInformation("✅ Successfully processed file: {FileName} ({Size} bytes)", newFile.Name, newFile.Size);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "❌ Error processing file: {FileName}", newFile.Name);
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
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Warning,
                    Summary = "Partial Success",
                    Detail = $"Added {successfullyProcessedFiles.Count} file(s). Failed to process {failedFiles.Count} file(s). Total: {AttachedFiles.Count} files queued.",
                    Duration = 4000
                });
            }
            else if (successfullyProcessedFiles.Any())
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Files Added",
                    Detail = $"Added {successfullyProcessedFiles.Count} file(s) to the queue. Total: {AttachedFiles.Count} files.",
                    Duration = 3000
                });
            }
            else if (failedFiles.Any())
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "File Processing Failed",
                    Detail = $"Failed to process {failedFiles.Count} file(s). This may be due to file size limits or browser restrictions.",
                    Duration = 5000
                });
            }

            Logger.LogInformation("📁 File processing completed: {Success} successful, {Failed} failed. Total queued: {Total}",
                successfullyProcessedFiles.Count, failedFiles.Count, AttachedFiles.Count);
        }
        else
        {
            Logger.LogInformation("⚠️ No files provided to OnInputFileChange");
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
        await Task.Delay(300);

        // Always try to initialize the map when modal opens
        if (_mapModule != null)
        {
            try
            {
                // Always reinitialize the map since the DOM element is recreated
                await _mapModule.InvokeVoidAsync("initializeMap",
                    AirportCenterLatitude, AirportCenterLongitude, DefaultZoomLevel, _dotNetRef);

                Logger.LogInformation("Map reinitialized for modal opening");

                // Restore existing location if we have one
                if (HasGeoLocation)
                {
                    await Task.Delay(100); // Give map time to initialize

                    await _mapModule.InvokeVoidAsync("setLocationFromCoordinates",
                        (double)SelectedGeoLocation.Latitude, (double)SelectedGeoLocation.Longitude,
                        SelectedGeoLocation.Description);

                    // Update the form fields to match the restored location
                    SelectedLatitude = SelectedGeoLocation.Latitude;
                    SelectedLongitude = SelectedGeoLocation.Longitude;
                    LocationDescription = SelectedGeoLocation.Description ?? "";

                    Logger.LogInformation("Existing location restored: {Lat}, {Lng}",
                        SelectedGeoLocation.Latitude, SelectedGeoLocation.Longitude);

                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error initializing map in OpenMapSelector");

                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Warning,
                    Summary = "Map Error",
                    Detail = "Could not initialize map. Please try refreshing the page.",
                    Duration = 5000
                });
            }
        }

        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Info,
            Summary = "Map Selector",
            Detail = "Click on the map to select the hazard location.",
            Duration = 3000
        });
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
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Warning,
                Summary = "No Location Selected",
                Detail = "Please click on the map to select a location first.",
                Duration = 3000
            });
            return;
        }

        // Set the geolocation data
        SelectedGeoLocation = new GeoLocationData
        {
            Latitude = SelectedLatitude,
            Longitude = SelectedLongitude,
            Description = LocationDescription,// string.IsNullOrEmpty(LocationDescription) ? 
                                              //$"Map Location ({SelectedLatitude:F6}, {SelectedLongitude:F6})" : LocationDescription,
            SelectedDateTime = DateTime.UtcNow
        };

        // Update the form location to indicate map location is selected
        HazardReport.Location = "MAP_LOCATION";

        ShowMapModal = false;
        StateHasChanged();

        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = "Location Set",
            Detail = $"Location selected: {GeoLocationDisplay}",
            Duration = 3000
        });
    }

    /// <summary>
    /// Clear map selection
    /// </summary>
    public async Task ClearMapSelection()
    {
        SelectedLatitude = 0;
        SelectedLongitude = 0;
        LocationDescription = string.Empty;
        SelectedGeoLocation = new GeoLocationData();
        HazardReport.Location = "";

        if (_mapModule != null)
        {
            try
            {
                await _mapModule.InvokeVoidAsync("clearSelection");
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Error clearing map selection");
            }
        }

        StateHasChanged();

        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Info,
            Summary = "Selection Cleared",
            Detail = "Map selection has been cleared.",
            Duration = 2000
        });
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
            Logger?.LogWarning("Received zero coordinates from map click");
        }

        Logger?.LogInformation("Map location received from JS: Lat={Lat}, Lng={Lng}, Description={Desc}",
            latitude, longitude, description);

        await InvokeAsync(StateHasChanged);

        Logger?.LogInformation("Map location selected: {Lat}, {Lng} - HasValidCoordinates: {HasValid}",
            latitude, longitude, HasValidCoordinates);
    }


    #endregion

    #region Modal Methods

    /// <summary>
    /// Show preview modal
    /// </summary>
    public void ShowPreviewModal()
    {
        if (!IsFormValidForPreview)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Warning,
                Summary = "Incomplete Form",
                Detail = "Please complete all required fields before previewing.",
                Duration = 3000
            });
            return;
        }

        ShowPreview = true;
        StateHasChanged();
    }

    /// <summary>
    /// Hide preview modal
    /// </summary>
    public void HidePreview()
    {
        ShowPreview = false;
        StateHasChanged();
    }

    /// <summary>
    /// Show submission confirmation modal
    /// </summary>
    public async Task ShowSubmissionConfirmationDialog()
    {
        if (!IsFormValidForSubmission)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Warning,
                Summary = "Form Validation",
                Detail = "Please complete all required fields before submitting.",
                Duration = 4000
            });
            return;
        }

        Logger.LogInformation("🚀 Preparing for submission with {Count} cached files ready", AttachedFiles?.Count ?? 0);
        if (AttachedFiles?.Any() == true)
        {
            foreach (var file in AttachedFiles)
            {
                Logger.LogInformation("📄 Cached file ready for submission: {FileName} ({Size} bytes, {DataSize} bytes cached)",
                    file.FileName, file.Size, file.Data?.Length ?? 0);
            }
        }

        ShowSubmissionConfirmation = true;
        ShowPreview = false;
        StateHasChanged(); // Force UI update to hide buttons
    }

    /// <summary>
    /// Cancel submission
    /// </summary>
    public void CancelSubmission()
    {
        ShowSubmissionConfirmation = false;
        StateHasChanged(); // Force UI update to show buttons again

        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Info,
            Summary = "Submission Cancelled",
            Detail = "You can continue editing your report.",
            Duration = 3000
        });
    }

    /// <summary>
    /// Toggle confidential information display
    /// </summary>
    public void ToggleConfidentialInfo()
    {
        ShowConfidentialInfo = !ShowConfidentialInfo;
        StateHasChanged();
    }

    private string GetTrackingUrl()
    {
        if (string.IsNullOrEmpty(GeneratedTrackingId))
            return string.Empty;

        var baseUri = Navigation.BaseUri.TrimEnd('/');
        return $"{baseUri}/ConfidentialReporting/TrackStatus/{GeneratedTrackingId}";
    }

    /// <summary>
    /// Close final confirmation and handle navigation based on mode
    /// </summary>
    public void CloseFinalConfirmation()
    {
        if (IsEditMode)
        {
            // In edit mode, redirect back to Reports page
            Navigation.NavigateTo("/SMSRiskManagement/ReportProcessing");
        }
        else
        {
            // In create mode, clear form and redirect to Report Processing
            // Clear all form data after successful submission
            HazardReport = new HazardReportForm();
            SelectedGeoLocation = new GeoLocationData();

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
            ShowPreview = false;
            ShowConfidentialInfo = false;
            ShowMapModal = false;
            ShowSubmissionConfirmation = false;
            ShowFinalSuccessConfirmation = false;

            Navigation.NavigateTo("/SMSRiskManagement/ReportProcessing");
        }
    }
    private async Task<string> GenerateQRCodeDataUrl(string url)
    {
        try
        {
            // Use JavaScript to generate QR code and return as data URL
            var qrDataUrl = await JSRuntime.InvokeAsync<string>("generateQRCodeDataUrl", url);
            return qrDataUrl ?? string.Empty;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to generate QR code data URL");
            return string.Empty;
        }
    }
    private async Task PrintConfirmation()
    {
        try
        {
            // First, generate the QR code as a data URL using JavaScript
            var trackingUrl = GetTrackingUrl();
            var qrCodeDataUrl = await GenerateQRCodeDataUrl(trackingUrl);

            // Create printable content with embedded QR code
            var printContent = GeneratePrintableContentWithQRData(qrCodeDataUrl);

            // Use standard print function since QR is now embedded
            await JSRuntime.InvokeVoidAsync("printContent", printContent);

            Logger.LogInformation("Print confirmation with embedded QR code requested for tracking ID: {TrackingId}", GeneratedTrackingId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error printing confirmation for tracking ID: {TrackingId}", GeneratedTrackingId);

            // Fallback: print without QR code
            var printContentFallback = GeneratePrintableContentFallback();
            await JSRuntime.InvokeVoidAsync("printContent", printContentFallback);

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Warning,
                Summary = "Print Warning",
                Detail = "Printed confirmation without QR code. Full tracking URL is included.",
                Duration = 5000
            });
        }
    }

    /// <summary>
    /// Copy tracking information to clipboard
    /// </summary>
    private async Task CopyTrackingInfo()
    {
        try
        {
            var trackingInfo = $"Tracking ID: {GeneratedTrackingId}\nTracking URL: {GetTrackingUrl()}\nSubmitted: {SubmissionDateTime?.ToString("MM/dd/yyyy HH:mm")}";

            await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", trackingInfo);
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Success",
                Detail = "Tracking information copied to clipboard!",
                Duration = 4000
            });

            Logger.LogInformation("Tracking information copied to clipboard for: {TrackingId}", GeneratedTrackingId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error copying tracking info for: {TrackingId}", GeneratedTrackingId);

            // Fallback: Show alert with the information
            //await ShowTrackingInfoAlert();
        }
    }


    private string GeneratePrintableContentWithQRData(string qrCodeDataUrl)
    {
        var trackingUrl = GetTrackingUrl();
        var submissionDate = SubmissionDateTime?.ToString("dddd, MMMM dd, yyyy 'at' h:mm tt") ?? "Unknown";
        var hasQRCode = !string.IsNullOrEmpty(qrCodeDataUrl);

        return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <title>SMS Confidential Report Confirmation</title>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        margin: 20px;
                        line-height: 1.4;
                    }}
                    .header {{
                        text-align: center;
                        border-bottom: 2px solid #212e61;
                        padding-bottom: 20px;
                        margin-bottom: 30px;
                    }}
                    .logo {{
                        font-size: 24px;
                        font-weight: bold;
                        color: #212e61;
                    }}
                    .subtitle {{
                        color: #666;
                        margin-top: 5px;
                    }}
                    .confirmation {{
                        background-color: #d4edda;
                        border: 1px solid #c3e6cb;
                        border-radius: 5px;
                        padding: 15px;
                        margin-bottom: 20px;
                    }}
                    .content-row {{
                        display: table;
                        width: 100%;
                        margin-bottom: 20px;
                    }}
                    .details-column {{
                        display: table-cell;
                        vertical-align: top;
                        width: 70%;
                        padding-right: 20px;
                    }}
                    .qr-column {{
                        display: table-cell;
                        vertical-align: top;
                        width: 30%;
                        text-align: center;
                        border: 1px solid #ddd;
                        padding: 15px;
                        border-radius: 5px;
                        background-color: #f9f9f9;
                    }}
                    .details-table {{
                        width: 100%;
                        border-collapse: collapse;
                        margin-bottom: 20px;
                    }}
                    .details-table th,
                    .details-table td {{
                        border: 1px solid #ddd;
                        padding: 10px;
                        text-align: left;
                    }}
                    .details-table th {{
                        background-color: #f8f9fa;
                        font-weight: bold;
                        color: #212e61;
                    }}
                    .tracking-id {{
                        font-family: monospace;
                        font-size: 16px;
                        font-weight: bold;
                        background-color: #fff3cd;
                        padding: 5px;
                        border-radius: 3px;
                    }}
                    .qr-code-img {{
                        max-width: 150px;
                        max-height: 150px;
                        margin: 10px 0;
                    }}
                    .qr-label {{
                        font-weight: bold;
                        color: #212e61;
                        margin-bottom: 10px;
                    }}
                    .qr-instruction {{
                        font-size: 12px;
                        color: #666;
                        margin-top: 10px;
                    }}
                    .important {{
                        background-color: #fff3cd;
                        border: 1px solid #ffeaa7;
                        border-radius: 5px;
                        padding: 15px;
                        margin-bottom: 20px;
                    }}
                    .footer {{
                        border-top: 1px solid #ddd;
                        padding-top: 20px;
                        margin-top: 30px;
                        text-align: center;
                        color: #666;
                        font-size: 12px;
                    }}
                    @media print {{
                        body {{ margin: 0; }}
                        .no-print {{ display: none; }}
                        .content-row {{ 
                            display: table;
                            width: 100%;
                        }}
                        .details-column {{
                            display: table-cell;
                            width: 70%;
                        }}
                        .qr-column {{ 
                            display: table-cell;
                            width: 30%;
                        }}
                    }}
                </style>
            </head>
            <body>
                <div class='header'>
                    <div class='logo'>🛩️ PDX SMS</div>
                    <div class='subtitle'>Safety Management System - Confidential Reporting</div>
                </div>

                <div class='confirmation'>
                    <h2 style='margin: 0; color: #0d8944;'>✅ Report Successfully Submitted</h2>
                    <p style='margin: 5px 0 0 0;'>Your confidential safety report has been received and will be reviewed by authorized personnel.</p>
                </div>

                <div class='content-row'>
                    <div class='details-column'>
                        <table class='details-table'>
                            <tr>
                                <th>Tracking ID</th>
                                <td class='tracking-id'>{GeneratedTrackingId}</td>
                            </tr>
                            <tr>
                                <th>Hazard ID</th>
                                <td>{GeneratedHazardId}</td>
                            </tr>
                            <tr>
                                <th>Submission Date</th>
                                <td>{submissionDate}</td>
                            </tr>
                            <tr>
                                <th>Report Type</th>
                                <td>Confidential Safety Report</td>
                            </tr>
                            <tr>
                                <th>Status</th>
                                <td><strong>Submitted</strong> - Under Review</td>
                            </tr>
                            <tr>
                                <th>Tracking URL</th>
                                <td style='word-break: break-all; font-size: 11px;'>{trackingUrl}</td>
                            </tr>
                        </table>
                    </div>
                    
                    <div class='qr-column'>
                        <div class='qr-label'>📱 Quick Track</div>
                        {(hasQRCode ?
                            $"<img src='{qrCodeDataUrl}' alt='QR Code for {trackingUrl}' class='qr-code-img' />" :
                            "<div style='font-size: 12px; color: #999; padding: 20px;'>QR Code<br>Not Available</div>"
                        )}
                        <div class='qr-instruction'>
                            {(hasQRCode ? "Scan with mobile device<br>to track your report" : "Use the tracking URL above")}
                        </div>
                    </div>
                </div>

                <div class='important'>
                    <h3 style='margin: 0 0 10px 0; color: #212e61;'>🔒 Important Information</h3>
                    <ul style='margin: 0; padding-left: 20px;'>
                        <li><strong>Save this information:</strong> Your tracking ID is the only way to check your report status</li>
                        <li><strong>Anonymous protection:</strong> Your identity is protected and will not be disclosed</li>
                        <li><strong>Follow-up:</strong> Use the tracking URL to check your report status at any time</li>
                        {(hasQRCode ? "<li><strong>QR Code:</strong> Scan the QR code above with your mobile device for quick access</li>" : "")}
                        <li><strong>Questions:</strong> Call our confidential hotline at (503) 555-0199</li>
                    </ul>
                </div>

                <div class='footer'>
                    <p>Port of Portland - Safety Management System</p>
                    <p>This document was generated on {DateTime.Now:dddd, MMMM dd, yyyy 'at' h:mm tt}</p>
                    <p>Keep this confirmation for your records</p>
                </div>
            </body>
            </html>
        ";
    }

    /// <summary>
    /// Generate printable content without QR code (fallback)
    /// </summary>
    private string GeneratePrintableContentFallback()
    {
        var trackingUrl = GetTrackingUrl();
        var submissionDate = SubmissionDateTime?.ToString("dddd, MMMM dd, yyyy 'at' h:mm tt") ?? "Unknown";

        return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <title>SMS Confidential Report Confirmation</title>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        margin: 20px;
                        line-height: 1.4;
                    }}
                    .header {{
                        text-align: center;
                        border-bottom: 2px solid #212e61;
                        padding-bottom: 20px;
                        margin-bottom: 30px;
                    }}
                    .logo {{
                        font-size: 24px;
                        font-weight: bold;
                        color: #212e61;
                    }}
                    .subtitle {{
                        color: #666;
                        margin-top: 5px;
                    }}
                    .confirmation {{
                        background-color: #d4edda;
                        border: 1px solid #c3e6cb;
                        border-radius: 5px;
                        padding: 15px;
                        margin-bottom: 20px;
                    }}
                    .details-table {{
                        width: 100%;
                        border-collapse: collapse;
                        margin-bottom: 20px;
                    }}
                    .details-table th,
                    .details-table td {{
                        border: 1px solid #ddd;
                        padding: 10px;
                        text-align: left;
                    }}
                    .details-table th {{
                        background-color: #f8f9fa;
                        font-weight: bold;
                        color: #212e61;
                    }}
                    .tracking-id {{
                        font-family: monospace;
                        font-size: 16px;
                        font-weight: bold;
                        background-color: #fff3cd;
                        padding: 5px;
                        border-radius: 3px;
                    }}
                    .important {{
                        background-color: #fff3cd;
                        border: 1px solid #ffeaa7;
                        border-radius: 5px;
                        padding: 15px;
                        margin-bottom: 20px;
                    }}
                    .footer {{
                        border-top: 1px solid #ddd;
                        padding-top: 20px;
                        margin-top: 30px;
                        text-align: center;
                        color: #666;
                        font-size: 12px;
                    }}
                </style>
            </head>
            <body>
                <div class='header'>
                    <div class='logo'>??? PDX SMS</div>
                    <div class='subtitle'>Safety Management System - Confidential Reporting</div>
                </div>

                <div class='confirmation'>
                    <h2 style='margin: 0; color: #0d8944;'>? Report Successfully Submitted</h2>
                    <p style='margin: 5px 0 0 0;'>Your confidential safety report has been received and will be reviewed by authorized personnel.</p>
                </div>

                <table class='details-table'>
                    <tr>
                        <th>Tracking ID</th>
                        <td class='tracking-id'>{GeneratedTrackingId}</td>
                    </tr>
                    <tr>
                        <th>Hazard ID</th>
                        <td>{GeneratedHazardId}</td>
                    </tr>
                    <tr>
                        <th>Submission Date</th>
                        <td>{submissionDate}</td>
                    </tr>
                    <tr>
                        <th>Report Type</th>
                        <td>Confidential Safety Report</td>
                    </tr>
                    <tr>
                        <th>Status</th>
                        <td><strong>Submitted</strong> - Under Review</td>
                    </tr>
                    <tr>
                        <th>Tracking URL</th>
                        <td style='word-break: break-all;'>{trackingUrl}</td>
                    </tr>
                </table>

                <div class='important'>
                    <h3 style='margin: 0 0 10px 0; color: #212e61;'>?? Important Information</h3>
                    <ul style='margin: 0; padding-left: 20px;'>
                        <li><strong>Save this information:</strong> Your tracking ID is the only way to check your report status</li>
                        <li><strong>Anonymous protection:</strong> Your identity is protected and will not be disclosed</li>
                        <li><strong>Follow-up:</strong> Use the tracking URL to check your report status at any time</li>
                        <li><strong>Questions:</strong> Call our confidential hotline at (503) 555-0199</li>
                    </ul>
                </div>

                <div class='footer'>
                    <p>Port of Portland - Safety Management System</p>
                    <p>This document was generated on {DateTime.Now:dddd, MMMM dd, yyyy 'at' h:mm tt}</p>
                    <p>Keep this confirmation for your records</p>
                </div>
            </body>
            </html>
        ";
    }

    #endregion

    #region Business Logic Methods

    /// <summary>
    /// Submit the hazard report - handles both CREATE and EDIT modes
    /// </summary>
    public async Task SubmitReportConfirmed()
    {
        try
        {
            // Validation
            if (!IsFormValidForSubmission)
            {
                ShowSubmissionConfirmation = true;
                StateHasChanged();

                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Warning,
                    Summary = "Form Validation",
                    Detail = "Please complete all required fields before submitting.",
                    Duration = 4000
                });
                return;
            }

            IsLoading = true;
            ShowSubmissionConfirmation = false;
            StateHasChanged();

            if (IsEditMode && EditingReport != null && EditingHazard != null)
            {
                // ===============================
                // EDIT MODE - Update existing report and hazard
                // ===============================
                Logger.LogInformation("Starting EDIT mode submission for Report: {ReportCode}, Hazard: {HazardCode}",
                    EditReportCode, EditHazardCode);

                await UpdateExistingReportAndHazard();
            }
            else
            {
                // ===============================
                // CREATE MODE - Create new report and hazard  
                // ===============================
                Logger.LogInformation("Starting CREATE mode submission for new hazard report");

                await CreateNewReportAndHazard();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "❌ Error during hazard report submission in {Mode} mode",
                IsEditMode ? "EDIT" : "CREATE");

            ShowSubmissionConfirmation = false;

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Submission Failed",
                Detail = $"An error occurred while {(IsEditMode ? "updating" : "saving")} your report. Please try again.",
                Duration = 5000
            });
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
        Logger.LogInformation("Updating existing Report: {ReportCode} and Hazard: {HazardCode}",
            EditReportCode, EditHazardCode);

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

        EditingReport.UpdatedDate = DateTime.UtcNow;
        EditingReport.UpdatedBy = AuthService.CurrentUserDisplayName;

        var updateReportCommand = new UpdateReportCommand(EditingReport);
        var reportUpdateResult = await Mediator.SendAsync(updateReportCommand, CancellationToken.None);

        if (reportUpdateResult.IsFailure)
        {
            throw new Exception($"Failed to update report: {reportUpdateResult.Error?.Message}");
        }

        Logger.LogInformation("✅ Report {ReportCode} updated successfully", EditReportCode);

        // ===============================
        // STEP 2: Update the existing Hazard
        // ===============================
        EditingHazard!.Name = $"{HazardReport.HazardCategory} - {HazardReport.HazardType}";
        EditingHazard.Description = HazardReport.Description;
        EditingHazard.HazardCategory = HazardReport.HazardCategory;
        EditingHazard.HazardType = HazardReport.HazardType; // This is the actual selected hazard type, not "Initial"
        
        EditingHazard.UpdatedDate = DateTime.UtcNow;
        EditingHazard.UpdatedBy = AuthService.CurrentUserDisplayName;

        // Handle location updates
        await UpdateHazardLocation(EditingHazard);

        var updateHazardCommand = new UpdateHazardCommand(EditingHazard);
        var hazardUpdateResult = await Mediator.SendAsync(updateHazardCommand, CancellationToken.None);

        if (hazardUpdateResult.IsFailure)
        {
            throw new Exception($"Failed to update hazard: {hazardUpdateResult.Error?.Message}");
        }

        var updatedHazard = hazardUpdateResult.Value;
        Logger.LogInformation("✅ Hazard {HazardCode} updated successfully", EditHazardCode);

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

        Logger.LogInformation("✅ EDIT mode completed - Report: {ReportCode}, Hazard: {HazardCode}",
            updatedHazard.ReportCode, updatedHazard.Code);

        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = "Report Updated Successfully",
            Detail = $"Report {updatedHazard.ReportCode} and hazard {updatedHazard.Code} have been updated.",
            Duration = 5000
        });
    }

    /// <summary>
    /// Create new report and hazard (CREATE MODE)
    /// </summary>
    private async Task CreateNewReportAndHazard()
    {
        Logger.LogInformation("Creating new report and hazard");

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
            Stage = "Initial",
            Status = "Initial",
            CreatedBy = AuthService.CurrentUserDisplayName,
            CreatedDate = DateTime.UtcNow
        };

        var reportResult = await Mediator.SendAsync(new CreateReportCommand(report), CancellationToken.None);

        if (reportResult.IsFailure)
        {
            throw new Exception($"Failed to create report: {reportResult.Error?.Message}");
        }

        var actualReportCode = reportResult.Value.Code;
        Logger.LogInformation("✅ Report created with Code: {ReportCode}", actualReportCode);

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
            CreatedBy = AuthService.CurrentUserDisplayName,
            CreatedDate = DateTime.UtcNow
        };

        var createHazardCommand = new CreateHazardCommand(hazard);
        var createdHazardResult = await Mediator.SendAsync(createHazardCommand, CancellationToken.None);

        if (createdHazardResult.IsFailure)
        {
            throw new Exception($"Failed to create hazard: {createdHazardResult.Error?.Message}");
        }

        var createdHazard = createdHazardResult.Value;

        // Handle location for confidential hazard AFTER NEW Hazard Has Been Recorded !! 
        await UpdateHazardLocation(createdHazard);

        Logger.LogInformation("✅ Hazard created with Code: {HazardCode}, linked to Report: {ReportCode}", createdHazard.Code, actualReportCode);
        Result<HazardReportTracking> createdtrackingcodeResult = await GenerateTracking(createdHazard);
        var createdTracking = createdtrackingcodeResult.Value;
        GeneratedTrackingId = createdTracking.TrackingCode;
        GeneratedHazardId = createdHazard.Code;
        GeneratedReportId = createdHazard.ReportCode;

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

        Logger.LogInformation("✅ CREATE mode completed - Report: {ReportCode}, Hazard: {HazardCode} Tracking: { TrackingCode} ",

            createdHazard.ReportCode, createdHazard.Code, createdTracking.TrackingCode);

        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = "Report Created Successfully",
            Detail = $"Hazard report {createdHazard.Code} has been created and linked to report {createdHazard.ReportCode} with Tracking ID {createdTracking.TrackingCode}.",
            Duration = 5000
        });
    }


    private async Task<Result<HazardReportTracking>> GenerateTracking(Hazard createdHazard)
    {
        HazardReportTracking hazardreporttracking = new HazardReportTracking(new HazardReportTrackingID("HT-0000"));
        hazardreporttracking.HazardCode = createdHazard.Code;
        hazardreporttracking.ReportCode = createdHazard.ReportCode;
        hazardreporttracking.TrackingCode = "HT-0000";
        var trackingcodeCommand = new CreateHazardReportTrackingCommand(hazardreporttracking);
        var createdtrackingcodeResult = await Mediator.SendAsync(trackingcodeCommand, CancellationToken.None);
        return createdtrackingcodeResult;
    }

    /// <summary>
    /// Update hazard location (common for both CREATE and EDIT modes) - FIXED: Create new HazardLocation properly
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
                // Check if HazardLocation already exists for this hazard
                var hazardLocationResult = await Mediator.SendAsync(new GetHazardLocationsByHazardCodeQuery(hazard.Code), CancellationToken.None);

                HazardLocation? hazardLocation = null;

                if (hazardLocationResult.IsSuccess && hazardLocationResult.Value.Any())
                {
                    // UPDATE existing HazardLocation
                    hazardLocation = hazardLocationResult.Value.FirstOrDefault();
                    if (hazardLocation != null)
                    {
                        hazardLocation.HazardCode = hazard.Code;
                        hazardLocation.Latitude = SelectedGeoLocation.Latitude;
                        hazardLocation.Longitude = SelectedGeoLocation.Longitude;
                        hazardLocation.Description = SelectedGeoLocation.Description ?? "Map selected location";
                        hazardLocation.UpdatedBy = AuthService.CurrentUserDisplayName;
                        hazardLocation.UpdatedDate = DateTime.UtcNow;
                        hazard.HazardLocation = hazardLocation;
                        
                        var locationUpdateResult = await Mediator.SendAsync(new UpdateHazardLocationCommand(hazardLocation), CancellationToken.None);

                        if (locationUpdateResult.IsSuccess)
                        {
                            Logger.LogInformation("✅ HazardLocation updated with Code: {LocationCode}, Coordinates: ({Lat}, {Lng})",
                                hazardLocation.Code, SelectedGeoLocation.Latitude, SelectedGeoLocation.Longitude);
                        }
                    }
                }
                else
                {
                    // CREATE new HazardLocation
                    var hazardLocationCode = $"HL-{hazard.Code}-{DateTime.UtcNow:yyyyMMdd}";
                    var newHazardLocation = new HazardLocation(new HazardLocationID(hazardLocationCode))
                    {
                        Code = hazardLocationCode,
                        HazardCode = hazard.Code,
                        Latitude = SelectedGeoLocation.Latitude,
                        Longitude = SelectedGeoLocation.Longitude,
                        Description = SelectedGeoLocation.Description ?? "Map selected location",
                        CreatedBy = AuthService.CurrentUserDisplayName,
                        CreatedDate = DateTime.UtcNow,
                        IsValid = true
                    };

                    // Create the new HazardLocation
                    var createLocationCommand = new CreateHazardLocationCommand(newHazardLocation);
                    var locationCreateResult = await Mediator.SendAsync(createLocationCommand, CancellationToken.None);

                    if (locationCreateResult.IsSuccess)
                    {
                        var createdLocation = locationCreateResult.Value;
                        hazard.HazardLocation = createdLocation;

                        Logger.LogInformation("✅ HazardLocation created with Code: {LocationCode}, Coordinates: ({Lat}, {Lng})",
                            createdLocation.Code, SelectedGeoLocation.Latitude, SelectedGeoLocation.Longitude);
                    }
                    else
                    {
                        Logger.LogError("❌ Failed to create HazardLocation: {Error}", locationCreateResult.Error?.Message);
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
                Logger.LogWarning(locationEx, "Failed to create/update hazard location, but continuing with hazard update");

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
                Logger.LogInformation("📎 Processing {Count} cached files for Hazard: {HazardCode}",
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
                            UploadedBy = HazardReport.SubmittedBy ?? AuthService.CurrentUserDisplayName,
                            UploadedDate = DateTime.UtcNow,
                            IsActive = true,
                            IsConfidential = HazardReport.IsAnonymous
                        };

                        var createHazardFileCommand = new CreateHazardFileCommand(hazardFile);
                        var hazardFileResult = await Mediator.SendAsync(createHazardFileCommand, CancellationToken.None);

                        if (hazardFileResult.IsSuccess)
                        {
                            var createdFileId = hazardFileResult.Value.Code;
                            hazard.AddHazardFile(new HazardFileID(createdFileId));

                            Logger.LogInformation("✅ Created HazardFile: {FileName} with ID: {FileId} for Hazard: {HazardCode}",
                                attachedFile.FileName, createdFileId, hazard.Code);
                        }
                        else
                        {
                            Logger.LogError("❌ Failed to create HazardFile: {FileName} for Hazard: {HazardCode}. Error: {Error}",
                                attachedFile.FileName, hazard.Code, hazardFileResult.Error?.Message);
                        }
                    }
                    catch (Exception fileEx)
                    {
                        Logger.LogError(fileEx, "❌ Exception creating HazardFile: {FileName} for Hazard: {HazardCode}",
                            attachedFile.FileName, hazard.Code);
                    }
                }
            }
            else
            {
                Logger.LogInformation("📎 No files to process for Hazard: {HazardCode}", hazard.Code);
            }
        }
        catch (Exception fileEx)
        {
            Logger.LogError(fileEx, "📎 Error processing files, but continuing with hazard operation");
        }
    }


    #endregion

    #region Dropdown and Smart Enum Methods

    /// <summary>
    /// Initialize dropdown options using Smart Enums
    /// </summary>
    private void InitializeDropdownOptions()
    {
        // Hazard category options from Smart Enum
        HazardCategoryOptions = HazardCategory.GetAllValues()
            .Select(hc => new DropdownOption(hc.Value, hc.Name))
            .ToList();

        // Initially show all hazard types (will be filtered when category is selected)
        HazardTypeOptions = new List<DropdownOption>();

        DepartmentOptions = SMSDepartment.GetAllValues()
            .Select(hc => new DropdownOption(hc.Value, hc.Name))
            .ToList();
    }

    public async Task OnDepartmentChanged(string? departmentValue)
    {
        Logger.LogInformation("Submitting Department changed to: {Department}", departmentValue);

        SelectedDepartment = departmentValue;

    }

    /// <summary>
    /// Handle hazard category selection change
    /// </summary>
    public async Task OnHazardCategoryChanged(string? categoryValue)
    {
        Logger.LogInformation("Hazard category changed to: {Category}", categoryValue);

        SelectedHazardCategory = categoryValue;

        // Clear selected hazard type when category changes (normal user interaction)
        HazardReport.HazardType = null;

        // Load hazard types for the new category
        await LoadHazardTypesForCategory(categoryValue ?? "", preserveSelectedType: false);
    }

    /// <summary>
    /// Handle hazard type selection change
    /// </summary>
    public async Task OnHazardTypeChanged(string? hazardTypeValue)
    {
        HazardReport.HazardType = hazardTypeValue;

        if (!string.IsNullOrEmpty(hazardTypeValue))
        {
            var hazardType = HazardType.FromValue(hazardTypeValue);
            if (hazardType != null)
            {
                Logger.LogInformation("Hazard type changed to: {HazardType}, requires regulatory: {RequiresRegulatory}",
                    hazardType.Name, hazardType.RequiresRegulatoryReporting);

                // Could show regulatory warning if required
                if (hazardType.RequiresRegulatoryReporting)
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Info,
                        Summary = "Regulatory Reporting Required",
                        Detail = $"This hazard type ({hazardType.Name}) requires regulatory reporting to appropriate authorities.",
                        Duration = 5000
                    });
                }
            }
        }

        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Get guidance text for selected hazard type
    /// </summary>
    public string GetHazardTypeGuidance(string? hazardTypeValue)
    {
        if (string.IsNullOrEmpty(hazardTypeValue))
            return string.Empty;

        var hazardType = HazardType.FromValue(hazardTypeValue);
        return hazardType?.GuidanceText ?? string.Empty;
    }

    /// <summary>
    /// Check if regulatory reporting is required for selected hazard type
    /// </summary>
    public bool RequiresRegulatoryReporting(string? hazardTypeValue)
    {
        if (string.IsNullOrEmpty(hazardTypeValue))
            return false;

        var hazardType = HazardType.FromValue(hazardTypeValue);
        return hazardType?.RequiresRegulatoryReporting ?? false;
    }

    /// <summary>
    /// Get hazard category description for display
    /// </summary>
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
    /// Initialize form defaults for new reports
    /// </summary>
    private void InitializeFormDefaults()
    {
        var currentUser = AuthService.CurrentUserDisplayName;
        var tenMinutesAgo = DateTime.Now.AddMinutes(-10);
        
        HazardReport = new HazardReportForm
        {
            SubmittedBy = AuthService.CurrentUserDisplayName ?? "Unknown",
            SubmittedDate = new DateTime(tenMinutesAgo.Year, tenMinutesAgo.Month, tenMinutesAgo.Day,tenMinutesAgo.Hour, tenMinutesAgo.Minute, 0),
            IncidentDateTime = new DateTime(tenMinutesAgo.Year, tenMinutesAgo.Month, tenMinutesAgo.Day, tenMinutesAgo.Hour, tenMinutesAgo.Minute, 0),
        };

        SelectedGeoLocation = new GeoLocationData
        {
            Latitude = 0,
            Longitude = 0,
            Description = "Not set",
            SelectedDateTime = DateTime.UtcNow
        };

        // Initialize empty file collections
        SelectedFiles = new List<IBrowserFile>().AsReadOnly();
        AttachedFiles.Clear();

        // Reset dropdown selections
        SelectedHazardCategory = null;
        HazardTypeOptions.Clear();

        // Reset UI state
        ShowPreview = false;
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

            if (selectedFileToRemove != null)
            {
                selectedFilesList.Remove(selectedFileToRemove);
                SelectedFiles = selectedFilesList.AsReadOnly();
            }

            Logger.LogInformation("Removed file: {FileName} from queue", fileToRemove.FileName);
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
        Logger.LogInformation("Cleared all files from queue");
        StateHasChanged();
    }

    /// <summary>
    /// Clear form data with confirmation
    /// </summary>
    public async Task ClearForm()
    {
        var confirmed = await DialogService.Confirm(
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
            SelectedGeoLocation = new GeoLocationData();

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
            ShowPreview = false;
            ShowConfidentialInfo = false;
            ShowMapModal = false;
            ShowSubmissionConfirmation = false;
            ShowFinalSuccessConfirmation = false;

            InitializeFormDefaults();
            StateHasChanged();

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Info,
                Summary = "Form Cleared",
                Detail = "All form data has been cleared.",
                Duration = 2000
            });
        }
    }

    #endregion
}