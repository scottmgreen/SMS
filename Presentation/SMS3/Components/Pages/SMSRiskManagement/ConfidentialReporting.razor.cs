using Microsoft.JSInterop;

using SMS3.Components.Pages.SMSRiskManagement.Models;

namespace SMS3.Components.Pages.SMSRiskManagement;

/// <summary>
/// Confidential Reporting page - Enhanced security for sensitive safety reports
/// Based on HazardReporting but adapted for confidential submissions with identical layout
/// Removed Report Type and Urgency level, added cascading Hazard Category/Type dropdowns
/// </summary>
public partial class ConfidentialReporting : ComponentBase, IDisposable
{
    #region Dependencies
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ISMSSessionService SessionService { get; set; } = default!;
    [Inject] private ILogger<ConfidentialReporting> Logger { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    #endregion

    #region Properties and Fields

    /// <summary>
    /// Formatted latitude text for display
    /// </summary>
    public string SelectedLatitudeText => SelectedLatitude != 0 ? SelectedLatitude.ToString("F6") : "";

    /// <summary>
    /// Formatted longitude text for display
    /// </summary>
    public string SelectedLongitudeText => SelectedLongitude != 0 ? SelectedLongitude.ToString("F6") : "";

    /// <summary>
    /// Main form data object - using HazardReportForm like HazardReporting
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
    /// Success confirmation display state
    /// </summary>
    public bool ShowFinalSuccessConfirmation { get; set; }

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
    /// Generated Tracking ID after successful submission
    /// </summary>
    public string? GeneratedTrackingId { get; set; }

    /// <summary>
    /// Generated Hazard ID after successful submission
    /// </summary>
    public string? GeneratedHazardId { get; set; }


    /// <summary>
    /// Generated Hazard ID after successful submission
    /// </summary>
    public string? GeneratedReportId { get; set; }

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

    /// <summary>
    /// Currently selected hazard category
    /// </summary>
    public string? SelectedHazardCategory { get; set; }

    /// <summary>
    /// Display text for selected location
    /// </summary>
    public string LocationDisplayText => HasGeoLocation ? GetSelectedLocationText() : "No location selected";

    /// <summary>
    /// Check if geographic location has been selected
    /// </summary>
    public bool HasGeoLocation => SelectedGeoLocation?.IsValid == true;

    /// <summary>
    /// Check if valid coordinates are selected
    /// </summary>
    public bool HasValidCoordinates => SelectedLatitude != 0 && SelectedLongitude != 0;

    /// <summary>
    /// Geographic location display string
    /// </summary>
    public string GeoLocationDisplay => HasGeoLocation ?
        $"Lat: {SelectedGeoLocation.Latitude:F6}, Lng: {SelectedGeoLocation.Longitude:F6}" :
        "No coordinates selected";

    /// <summary>
    /// Character count for description field
    /// </summary>
    public int DescriptionCharacterCount => HazardReport?.Description?.Length ?? 0;

    /// <summary>
    /// Check if form has minimum required fields for preview
    /// </summary>
    public bool IsFormValidForPreview =>
        !string.IsNullOrEmpty(HazardReport.HazardType) && !string.IsNullOrEmpty(HazardReport.HazardCategory) &&
        !string.IsNullOrEmpty(HazardReport.SubmittedBy) && !string.IsNullOrEmpty(HazardReport.Description) && 
        !string.IsNullOrEmpty(HazardReport.IncidentDateTime.ToString());

    /// <summary>
    /// Check if form is valid for submission
    /// </summary>
    public bool IsFormValidForSubmission()
    {
        if (!HazardReport.IsAnonymous)
        {
            return IsFormValidForPreview && !string.IsNullOrEmpty(HazardReport.ReportContactName) && !string.IsNullOrEmpty(HazardReport.ReportContactCell) &&
            !string.IsNullOrEmpty(HazardReport.ReportContactEmail) &&
                    (HasGeoLocation || !string.IsNullOrEmpty(HazardReport.Location)) &&
                    DescriptionCharacterCount <= 2000;
        }

        else
        {
            return IsFormValidForPreview &&
                (HasGeoLocation || !string.IsNullOrEmpty(HazardReport.Location)) &&
                DescriptionCharacterCount <= 2000;
        }

    }

    /// <summary>
    /// Page title
    /// </summary>
    public string PageTitle => "Submit Confidential Report";

    /// <summary>
    /// Page subtitle
    /// </summary>
    public string PageSubtitle => "Enhanced protection for sensitive safety reports with reporter anonymity";

    // Airport coordinates
    private double AirportCenterLatitude => 45.58808;
    private double AirportCenterLongitude => -122.592430;
    private int DefaultZoomLevel => 20;

    private IJSObjectReference? _mapModule;
    private DotNetObjectReference<ConfidentialReporting>? _dotNetRef;

    #endregion

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        InitializeDropdownOptions();
        InitializeFormDefaults();

        // Create DotNet reference for JavaScript callbacks
        _dotNetRef = DotNetObjectReference.Create(this);

        Logger.LogInformation("Confidential reporting page initialized for user: {User}",
            SessionService.GetCurrentUserDisplayName() ?? "Anonymous");
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                // Initialize JavaScript mapping module only once
                _mapModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "/js/hazard-map.js");
                Logger.LogInformation("Map module loaded successfully for confidential reporting");
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Could not load JavaScript map module for confidential reporting");
            }
        }
    }

    public void Dispose()
    {
        _mapModule?.DisposeAsync();
        _dotNetRef?.Dispose();
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
            Logger.LogInformation("Confidential form submit triggered with data: HazardType={HazardType}, SubmittedBy={SubmittedBy}",
                formData.HazardType, formData.SubmittedBy);

            if (!IsFormValidForSubmission())
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
            Logger.LogError(ex, "Error during confidential form submission");

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
    /// Handle file selection with proper event callback signature - now accumulates files
    /// </summary>
    public async Task OnFilesSelected(IReadOnlyList<IBrowserFile> newFiles)
    {
        Logger.LogInformation("?? OnFilesSelected called with {Count} new files", newFiles?.Count ?? 0);

        if (newFiles?.Any() == true)
        {
            // Process files immediately to avoid JavaScript interop issues
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
                        Logger.LogInformation("?? Skipped duplicate file: {FileName}", newFile.Name);
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
                    Logger.LogInformation("? Successfully processed file: {FileName} ({Size} bytes)", newFile.Name, newFile.Size);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "? Error processing file: {FileName}", newFile.Name);
                    failedFiles.Add(newFile.Name);
                }
            }

            // Add successfully processed files to the collection
            if (successfullyProcessedFiles.Any())
            {
                AttachedFiles.AddRange(successfullyProcessedFiles);

                // Update SelectedFiles to maintain compatibility
                var allFiles = SelectedFiles?.ToList() ?? new List<IBrowserFile>();
                foreach (var file in newFiles.Where(f => successfullyProcessedFiles.Any(sf => sf.FileName == f.Name && sf.Size == f.Size)))
                {
                    allFiles.Add(file);
                }
                SelectedFiles = allFiles.AsReadOnly();
            }

            // Show notification about results
            if (successfullyProcessedFiles.Any())
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Files Added",
                    Detail = $"Added {successfullyProcessedFiles.Count} file(s) to the queue. Total: {AttachedFiles.Count} files.",
                    Duration = 3000
                });
            }
        }
        else
        {
            Logger.LogInformation("?? No files provided to OnFilesSelected");
        }

        StateHasChanged();
    }

    /// <summary>
    /// Handle InputFile change event - this will accumulate files properly
    /// </summary>
    public async Task OnInputFileChange(InputFileChangeEventArgs args)
    {
        var newFiles = args.GetMultipleFiles(10); // Allow up to 10 files at once
        Logger.LogInformation("?? OnInputFileChange called with {Count} new files", newFiles?.Count() ?? 0);

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
                        Logger.LogInformation("?? Skipped duplicate file: {FileName}", newFile.Name);
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
                    Logger.LogInformation("? Successfully processed file: {FileName} ({Size} bytes)", newFile.Name, newFile.Size);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "? Error processing file: {FileName}", newFile.Name);
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

            Logger.LogInformation("?? File processing completed: {Success} successful, {Failed} failed. Total queued: {Total}",
                successfullyProcessedFiles.Count, failedFiles.Count, AttachedFiles.Count);
        }
        else
        {
            Logger.LogInformation("?? No files provided to OnInputFileChange");
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

        if (_mapModule != null)
        {
            try
            {
                await _mapModule.InvokeVoidAsync("initializeMap",
                    AirportCenterLatitude, AirportCenterLongitude, DefaultZoomLevel, _dotNetRef);

                Logger.LogInformation("Map reinitialized for confidential reporting modal");

                if (HasGeoLocation)
                {
                    await Task.Delay(100);

                    await _mapModule.InvokeVoidAsync("setLocationFromCoordinates",
                        (double)SelectedGeoLocation.Latitude, (double)SelectedGeoLocation.Longitude,
                        SelectedGeoLocation.Description);

                    SelectedLatitude = SelectedGeoLocation.Latitude;
                    SelectedLongitude = SelectedGeoLocation.Longitude;
                    LocationDescription = SelectedGeoLocation.Description ?? "";

                    Logger.LogInformation("Existing location restored in confidential reporting: {Lat}, {Lng}",
                        SelectedGeoLocation.Latitude, SelectedGeoLocation.Longitude);

                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error initializing map in confidential reporting OpenMapSelector");

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
            Detail = "Click on the map to select the incident location.",
            Duration = 3000
        });
    }

    /// <summary>
    /// Close map selector modal
    /// </summary>
    public void CloseMapSelector()
    {
        ShowMapModal = false;
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

        SelectedGeoLocation = new GeoLocationData
        {
            Latitude = SelectedLatitude,
            Longitude = SelectedLongitude,
            Description = LocationDescription,
            SelectedDateTime = DateTime.UtcNow
        };

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
                Logger.LogWarning(ex, "Error clearing map selection in confidential reporting");
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

    /// <summary>
    /// JavaScript callback for map location selection
    /// </summary>
    [JSInvokable]
    public async Task OnMapLocationSelected(double latitude, double longitude, string description)
    {
        SelectedLatitude = (decimal)latitude;
        SelectedLongitude = (decimal)longitude;
        LocationDescription = description;

        await InvokeAsync(StateHasChanged);

        Logger.LogInformation("Confidential reporting map location selected: {Lat}, {Lng}, {Desc}", latitude, longitude, description);
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
        if (!IsFormValidForSubmission())
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

        ShowSubmissionConfirmation = true;
        ShowPreview = false;
        StateHasChanged();
    }

    /// <summary>
    /// Close final confirmation and navigate back to home page for anonymous users
    /// </summary>
    public void CloseFinalConfirmation()
    {
        // Clear all form data after successful submission
        InitializeFormDefaults();

        // Clear the form changed flag to prevent browser warning
        try
        {
            JSRuntime.InvokeVoidAsync("clearFormChanged");
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Could not clear form changed flag");
        }

        StateHasChanged();

        // Navigate to home page for anonymous users (not ReportProcessing)
        Navigation.NavigateTo("/", forceLoad: true);
    }

    #endregion

    #region Business Logic Methods

    /// <summary>
    /// Submit the confidential report - Following same pattern as HazardReporting
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

            Logger.LogInformation("Starting confidential report submission");

            // ===============================
            // STEP 1: Create new Report
            // ===============================
            var report = new Report(new ReportID("RP-0000"))
            {
                Code = "RP-0000",
                Name = $"{HazardReport.HazardCategory} - {HazardReport.HazardType}",
                SubmittedBy = "CONFIDENTIAL_USER",
                SubmittedDate = HazardReport.SubmittedDate,
                SubmittingDepartment = "CONFIDENTIAL",
                Description = HazardReport.Description,
                IncidentDateTime = HazardReport.IncidentDateTime,
                ReportContactName = HazardReport.ReportContactName, 
                ReportContactCell = HazardReport.ReportContactCell, 
                ReportContactEmail  = HazardReport.ReportContactEmail,
                Stage = "Initial",
                Status = "Initial"
                
            };

            var reportResult = await Mediator.SendAsync(new CreateReportCommand(report), CancellationToken.None);

            if (reportResult.IsFailure)
            {
                throw new Exception($"Failed to create report: {reportResult.Error?.Message}");
            }

            var actualReportCode = reportResult.Value.Code;
            GeneratedReportId = actualReportCode;

            Logger.LogInformation("? Confidential report created with Code: {ReportCode}", actualReportCode);

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
                IsInitialHazard = true // Mark as the initial hazard for this report
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


            GeneratedHazardId = createdHazard.Code;
            Logger.LogInformation("? Confidential hazard created with Code: {HazardCode}, linked to Report: {ReportCode}",
                createdHazard.Code, actualReportCode);

            // ===============================
            // STEP 3: Create Tracking Code
            // ===============================
            Result<HazardReportTracking> createdtrackingcodeResult = await GenerateTracking(createdHazard);
            var createdTracking = createdtrackingcodeResult.Value;

            GeneratedTrackingId = createdTracking.TrackingCode;
            Logger.LogInformation("? Tracking code created: {TrackingCode}", createdTracking.TrackingCode);

            // ===============================
            // STEP 4: Process files for confidential hazard
            // ===============================
            await ProcessFileUpdates(createdHazard);

            // ===============================
            // SUCCESS - Show completion message for confidential submission
            // ===============================
            SubmissionDateTime = DateTime.Now;
            ShowSubmissionConfirmation = false;
            ShowFinalSuccessConfirmation = true;

            Logger.LogInformation("? Confidential report submission completed - Report: {ReportCode}, Hazard: {HazardCode}, Tracking: {TrackingCode}",
                createdHazard.ReportCode, createdHazard.Code, createdTracking.TrackingCode);

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Confidential Report Submitted Successfully",
                Detail = $"Your confidential report has been securely submitted with tracking ID: {createdTracking.TrackingCode}",
                Duration = 5000
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "? Error during confidential report submission");

            ShowSubmissionConfirmation = false;

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Submission Failed",
                Detail = "An error occurred while submitting your confidential report. Please try again.",
                Duration = 5000
            });
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
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
    /// Update hazard location (common for both CREATE and EDIT modes) - Same as HazardReporting
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
                var hazardLocationResult = await Mediator.SendAsync(
                    new GetHazardLocationsByHazardCodeQuery(hazard.Code), CancellationToken.None);

                if (hazardLocationResult.IsSuccess && hazardLocationResult.Value.Any())
                {
                    var hazardLocation = hazardLocationResult.Value.FirstOrDefault();
                    if (hazardLocation != null)
                    {
                        hazardLocation.HazardCode = hazard.Code;
                        hazardLocation.Latitude = SelectedGeoLocation.Latitude;
                        hazardLocation.Longitude = SelectedGeoLocation.Longitude;
                        hazardLocation.Description = SelectedGeoLocation.Description ?? "Map selected location";
                        hazard.HazardLocation = hazardLocation;

                        var locationUpdateResult = await Mediator.SendAsync(
                            new UpdateHazardLocationCommand(hazardLocation), CancellationToken.None);

                        if (locationUpdateResult.IsSuccess)
                        {
                            Logger.LogInformation("✅ HazardLocation updated with Code: {LocationCode}, Coordinates: ({Lat}, {Lng})",
                                hazardLocation.Code, SelectedGeoLocation.Latitude, SelectedGeoLocation.Longitude);
                        }
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
                Logger.LogWarning(locationEx, "Failed to create/update hazard location for confidential report, but continuing with hazard update");

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
    /// Process file attachments (common for both CREATE and EDIT modes) - Same as HazardReporting
    /// </summary>
    private async Task ProcessFileUpdates(Hazard hazard)
    {
        try
        {
            if (AttachedFiles?.Any() == true)
            {
                Logger.LogInformation("?? Processing {Count} cached files for confidential Hazard: {HazardCode}",
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
                            UploadedBy = "CONFIDENTIAL_USER", // Anonymous for confidential reports
                            UploadedDate = DateTime.UtcNow,
                            IsActive = true,
                            IsConfidential = true // Always confidential
                        };

                        var createHazardFileCommand = new CreateHazardFileCommand(hazardFile);
                        var hazardFileResult = await Mediator.SendAsync(createHazardFileCommand, CancellationToken.None);

                        if (hazardFileResult.IsSuccess)
                        {
                            var createdFileId = hazardFileResult.Value.Code;
                            hazard.AddHazardFile(new HazardFileID(createdFileId));

                            Logger.LogInformation("? Created confidential HazardFile: {FileName} with ID: {FileId} for Hazard: {HazardCode}",
                                attachedFile.FileName, createdFileId, hazard.Code);
                        }
                        else
                        {
                            Logger.LogError("? Failed to create confidential HazardFile: {FileName} for Hazard: {HazardCode}. Error: {Error}",
                                attachedFile.FileName, hazard.Code, hazardFileResult.Error?.Message);
                        }
                    }
                    catch (Exception fileEx)
                    {
                        Logger.LogError(fileEx, "? Exception creating confidential HazardFile: {FileName} for Hazard: {HazardCode}",
                            attachedFile.FileName, hazard.Code);
                    }
                }
            }
            else
            {
                Logger.LogInformation("?? No files to process for confidential Hazard: {HazardCode}", hazard.Code);
            }
        }
        catch (Exception fileEx)
        {
            Logger.LogError(fileEx, "?? Error processing confidential files, but continuing with hazard operation");
        }
    }

    #endregion

    #region Helper Methods and Dropdown Logic

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
    /// Get display text for hazard types
    /// </summary>
    private string GetHazardTypeDisplay(string? hazardTypeValue)
    {
        return hazardTypeValue ?? "Unknown";
    }

    /// <summary>
    /// Get display text for hazard categories
    /// </summary>
    private string GetHazardCategoryDisplay(string? categoryValue)
    {
        return categoryValue ?? "Unknown";
    }

    /// <summary>
    /// Get selected location text
    /// </summary>
    private string GetSelectedLocationText()
    {
        if (!HasGeoLocation) return "No location selected";

        var lat = SelectedGeoLocation.Latitude;
        var lng = SelectedGeoLocation.Longitude;
        return $"Lat: {lat:F6}, Lng: {lng:F6} - {SelectedGeoLocation.Description}";
    }

    /// <summary>
    /// Initialize dropdown options - Simplified for confidential reporting
    /// </summary>
    private void InitializeDropdownOptions()
    {
        // Basic hazard category options - can be expanded later
        HazardCategoryOptions = new List<DropdownOption>
        {
            new("AIRCRAFT", "Aircraft Operations"),
            new("GROUND", "Ground Operations"),
            new("FACILITY", "Facility & Infrastructure"),
            new("EQUIPMENT", "Equipment & Maintenance"),
            new("PERSONNEL", "Personnel Safety"),
            new("SECURITY", "Security Related"),
            new("ENVIRONMENTAL", "Environmental"),
            new("OTHER", "Other")
        };

        HazardTypeOptions = new List<DropdownOption>();
    }

    /// <summary>
    /// Initialize form defaults for confidential reporting
    /// </summary>
    private void InitializeFormDefaults()
    {
        var tenMinutesAgo = DateTime.Now.AddMinutes(-10);
        HazardReport = new HazardReportForm
        {
            SubmittedBy = "Anonymous Reporter",
            SubmittedDate = new DateTime(tenMinutesAgo.Year, tenMinutesAgo.Month, tenMinutesAgo.Day,
                tenMinutesAgo.Hour, tenMinutesAgo.Minute, 0),
            IsAnonymous = true // Always true for confidential reporting
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

        ShowPreview = false;
        ShowMapModal = false;
        ShowSubmissionConfirmation = false;
        ShowFinalSuccessConfirmation = false;
    }

    /// <summary>
    /// Handle hazard category change
    /// </summary>
    public async Task OnHazardCategoryChanged(string? categoryValue)
    {
        Logger.LogInformation("Confidential reporting: Hazard category changed to: {Category}", categoryValue);

        SelectedHazardCategory = categoryValue;

        // Clear selected hazard type when category changes
        HazardReport.HazardType = null;

        // Load hazard types for the new category
        HazardTypeOptions = categoryValue switch
        {
            "AIRCRAFT" => new List<DropdownOption>
            {
                new("RWY_INCURSION", "Runway Incursion"),
                new("ACFT_DAMAGE", "Aircraft Damage"),
                new("NEAR_MISS", "Near Miss")
            },
            "GROUND" => new List<DropdownOption>
            {
                new("GROUND_VEHICLE", "Ground Vehicle"),
                new("GSE_MALFUNCTION", "GSE Malfunction"),
                new("FOD", "Foreign Object Debris")
            },
            "PERSONNEL" => new List<DropdownOption>
            {
                new("PERSONNEL_INJURY", "Personnel Injury"),
                new("UNSAFE_PRACTICE", "Unsafe Practice"),
                new("TRAINING_ISSUE", "Training Issue")
            },
            "FACILITY" => new List<DropdownOption>
            {
                new("INFRASTRUCTURE", "Infrastructure Issue"),
                new("LIGHTING", "Lighting Problem"),
                new("SIGNAGE", "Signage Issue")
            },
            "EQUIPMENT" => new List<DropdownOption>
            {
                new("EQUIPMENT_FAIL", "Equipment Failure"),
                new("MAINTENANCE_ISSUE", "Maintenance Issue")
            },
            "SECURITY" => new List<DropdownOption>
            {
                new("SECURITY_BREACH", "Security Breach"),
                new("UNAUTHORIZED_ACCESS", "Unauthorized Access")
            },
            "ENVIRONMENTAL" => new List<DropdownOption>
            {
                new("WILDLIFE_STRIKE", "Wildlife Strike"),
                new("WEATHER_RELATED", "Weather Related")
            },
            _ => new List<DropdownOption>()
        };

        StateHasChanged();
    }

    /// <summary>
    /// Handle hazard type change
    /// </summary>
    public async Task OnHazardTypeChanged(string? hazardTypeValue)
    {
        HazardReport.HazardType = hazardTypeValue;

        if (!string.IsNullOrEmpty(hazardTypeValue))
        {
            Logger.LogInformation("Confidential reporting: Hazard type changed to: {HazardType}", hazardTypeValue);

            // Show regulatory notification if required
            if (RequiresRegulatoryReporting(hazardTypeValue))
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Info,
                    Summary = "Regulatory Reporting Required",
                    Detail = $"This hazard type may require regulatory reporting to appropriate authorities.",
                    Duration = 5000
                });
            }
        }

        StateHasChanged();
    }

    /// <summary>
    /// Get hazard type guidance
    /// </summary>
    public string GetHazardTypeGuidance(string? hazardTypeValue)
    {
        return hazardTypeValue switch
        {
            "RWY_INCURSION" => "Report any unauthorized presence on a runway or failure to comply with ATC clearances.",
            "ACFT_DAMAGE" => "Document any damage to aircraft including ground handling incidents.",
            "GROUND_VEHICLE" => "Report incidents involving ground support equipment or vehicles.",
            "PERSONNEL_INJURY" => "Report any injury to personnel including slips, trips, and falls.",
            "EQUIPMENT_FAIL" => "Report any equipment malfunction or failure that could impact safety.",
            "SECURITY_BREACH" => "Report any breach of security protocols or unauthorized access.",
            "WILDLIFE_STRIKE" => "Report any wildlife collision or near-miss with aircraft.",
            _ => "Provide detailed description of the hazard or incident."
        };
    }

    /// <summary>
    /// Check if hazard type requires regulatory reporting
    /// </summary>
    public bool RequiresRegulatoryReporting(string? hazardTypeValue)
    {
        return hazardTypeValue switch
        {
            "RWY_INCURSION" => true,
            "ACFT_DAMAGE" => true,
            "WILDLIFE_STRIKE" => true,
            "SECURITY_BREACH" => true,
            _ => false
        };
    }

    /// <summary>
    /// Get hazard category description
    /// </summary>
    public string GetHazardCategoryDescription(string? categoryValue)
    {
        return categoryValue switch
        {
            "AIRCRAFT" => "Aircraft operations, movements, and related incidents",
            "GROUND" => "Ground operations, equipment, and vehicle-related events",
            "PERSONNEL" => "Personnel safety, training, and procedure-related issues",
            "FACILITY" => "Facility infrastructure, lighting, and physical plant issues",
            "EQUIPMENT" => "Equipment malfunctions, maintenance, and technical problems",
            "SECURITY" => "Security breaches, unauthorized access, and safety-security interface issues",
            "ENVIRONMENTAL" => "Wildlife, weather, and environmental safety concerns",
            _ => "General safety hazards and incidents"
        };
    }

    /// <summary>
    /// Clear all files
    /// </summary>
    public void ClearAllFiles()
    {
        AttachedFiles.Clear();
        SelectedFiles = new List<IBrowserFile>().AsReadOnly();
        StateHasChanged();
    }

    /// <summary>
    /// Remove specific file
    /// </summary>
    public void RemoveFile(int index)
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

            Logger.LogInformation("Removed confidential file: {FileName} from queue", fileToRemove.FileName);
            StateHasChanged();
        }
    }

    /// <summary>
    /// Clear form
    /// </summary>
    public void ClearForm()
    {
        InitializeFormDefaults();
        SelectedHazardCategory = null;
        HazardTypeOptions.Clear();
        StateHasChanged();
    }

    /// <summary>
    /// Cancel submission
    /// </summary>
    public void CancelSubmission()
    {
        ShowSubmissionConfirmation = false;
        StateHasChanged();

        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Info,
            Summary = "Submission Cancelled",
            Detail = "You can continue editing your confidential report.",
            Duration = 3000
        });
    }

    #endregion

    #region URL and Print Functionality

    /// <summary>
    /// Get the full tracking URL for the submitted report
    /// </summary>
    /// <returns>Complete URL for tracking the report</returns>
    private string GetTrackingUrl()
    {
        if (string.IsNullOrEmpty(GeneratedTrackingId))
            return string.Empty;

        var baseUri = Navigation.BaseUri.TrimEnd('/');
        return $"{baseUri}/ConfidentialReporting/TrackStatus/{GeneratedTrackingId}";
    }

    /// <summary>
    /// Print the confirmation details with QR code
    /// </summary>
    public async Task PrintConfirmation()
    {
        try
        {
            if (string.IsNullOrEmpty(GeneratedTrackingId) || string.IsNullOrEmpty(GeneratedReportId))
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Warning,
                    Summary = "Print Error",
                    Detail = "No report information available to print.",
                    Duration = 3000
                });
                return;
            }

            var trackingUrl = GetTrackingUrl();
            var submissionDate = SubmissionDateTime?.ToString("MMMM dd, yyyy 'at' h:mm tt") ?? DateTime.Now.ToString("MMMM dd, yyyy 'at' h:mm tt");

            // Call the NEW JavaScript function that captures the actual RadzenQRCode
            await JSRuntime.InvokeVoidAsync("printReportConfirmation",
                GeneratedReportId,
                GeneratedHazardId,
                GeneratedTrackingId,
                submissionDate,
                trackingUrl);

            Logger?.LogInformation("Print confirmation initiated for Tracking ID: {TrackingId}", GeneratedTrackingId);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error printing confirmation");

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Print Error",
                Detail = "Failed to print confirmation. Please try again or save the page.",
                Duration = 5000
            });
        }
    }

        
    /// <summary>
    /// Show tracking info alert as fallback if clipboard access fails
    /// </summary>
    private async Task ShowTrackingInfoAlert()
    {
        var trackingInfo = $"Tracking ID: {GeneratedTrackingId}\\n\\nTracking URL: {GetTrackingUrl()}\\n\\nSubmitted: {SubmissionDateTime?.ToString("MM/dd/yyyy HH:mm")}";

        // Use JavaScript alert as fallback
        await JSRuntime.InvokeVoidAsync("alert", $"Please copy and save this tracking information:\\n\\n{trackingInfo}");
    }

    #endregion
}