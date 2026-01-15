using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

using Radzen;
using Radzen.Blazor;

using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;
using SMS_Application.Messaging.Queries;

using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Domain.Enums; // <-- Added using for Smart Enums

using SMS_Shared.Common;
using SMS3.Components.Pages.SMSRiskManagement.Models; // <-- Updated to use the correct shared models namespace

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
        !string.IsNullOrEmpty(HazardReport.HazardType) &&
        !string.IsNullOrEmpty(HazardReport.ReportedBy) &&
        !string.IsNullOrEmpty(HazardReport.Description);

    /// <summary>
    /// Check if form is valid for submission
    /// </summary>
    public bool IsFormValidForSubmission => 
        IsFormValidForPreview && 
        (HasGeoLocation || !string.IsNullOrEmpty(HazardReport.Location)) &&
        DescriptionCharacterCount <= 2000;

    /// <summary>
    /// Page title
    /// </summary>
    public string PageTitle => "Submit Confidential Report";

    /// <summary>
    /// Page subtitle
    /// </summary>
    public string PageSubtitle => "Enhanced protection for sensitive safety reports with reporter anonymity";

    // Airport coordinates
    private double AirportCenterLatitude => 45.5898;
    private double AirportCenterLongitude => -122.5951;
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
            Logger.LogInformation("Confidential form submit triggered with data: HazardType={HazardType}, ReportedBy={ReportedBy}", 
                formData.HazardType, formData.ReportedBy);

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
    /// Submit the confidential report
    /// </summary>
    public async Task SubmitReportConfirmed()
    {
        try
        {
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

            Logger.LogInformation("Starting confidential report submission");

            // Generate tracking ID
            GeneratedTrackingId = GenerateAnonymousTrackingId();
            
            // ===============================
            // STEP 1: CREATE PARENT REPORT
            // ===============================
            var report = new Report(new ReportID("RP-0000"))
            {
                Code = "RP-0000",
                Name = GetHazardTypeDisplay(HazardReport.HazardType),
                Description = HazardReport.Description,
                Stage = "Initial",
                Status = "Active",
                ReportedBy = "CONFIDENTIAL_USER",
                Department = "CONFIDENTIAL"
            };

            var reportResult = await Mediator.SendAsync(new CreateReportCommand(report), CancellationToken.None);
            
            if (reportResult.IsFailure)
            {
                Logger.LogError("Failed to create report for confidential submission: {Error}", reportResult.Error?.Message);
                throw new Exception($"Failed to create report: {reportResult.Error?.Message}");
            }

            var actualReportCode = reportResult.Value.Code;
            Logger.LogInformation("Confidential report created with Code: {ReportCode}", actualReportCode);

            // ===============================
            // STEP 2: CREATE CONFIDENTIAL HAZARD
            // ===============================
            var hazardCode = "HZ-0000";
            var hazard = new Hazard(new HazardID(hazardCode))
            {
                Code = hazardCode,
                Name = GetHazardTypeDisplay(HazardReport.HazardType),
                Description = HazardReport.Description,
                HazardCategory = HazardReport.HazardType ?? "OTHER",
                HazardType = HazardReport.HazardType,
                ReportedBy = "CONFIDENTIAL_USER",
                ReportedOn = HazardReport.ReportedOn,
                ReportingDepartment = "CONFIDENTIAL",
                IsConfidential = true, // Always confidential
                IsAnonymous = true,
                ReportCode = actualReportCode,
                IsInitialHazard = true
            };

            // Handle location data
            if (HasGeoLocation)
            {
                hazard.LocationArea = "Coordinates Provided";
                hazard.LocationSubArea = $"Lat: {SelectedGeoLocation.Latitude:F6}, Lng: {SelectedGeoLocation.Longitude:F6}";
            }
            else if (!string.IsNullOrEmpty(HazardReport.Location))
            {
                hazard.LocationArea = HazardReport.Location;
            }

            var createHazardCommand = new CreateHazardCommand(hazard);
            var createdHazardResult = await Mediator.SendAsync(createHazardCommand, CancellationToken.None);
            
            if (createdHazardResult.IsFailure)
            {
                Logger.LogError("Failed to create hazard for confidential submission: {Error}", createdHazardResult.Error?.Message);
                throw new Exception($"Failed to create hazard: {createdHazardResult.Error?.Message}");
            }
            
            var createdHazard = createdHazardResult.Value;
            GeneratedHazardId = createdHazard.Code;
            Logger.LogInformation("Confidential hazard created with Code: {HazardCode}", createdHazard.Code);

            // ===============================
            // STEP 3: PROCESS FILE ATTACHMENTS
            // ===============================
            if (AttachedFiles.Any())
            {
                foreach (var attachedFile in AttachedFiles)
                {
                    try
                    {
                        var hazardFile = new HazardFile(new HazardFileID("HF-0000"))
                        {
                            Code = "HF-0000",
                            HazardCode = createdHazard.Code,
                            ReportCode = actualReportCode,
                            FileName = attachedFile.FileName,
                            FileType = Path.GetExtension(attachedFile.FileName)?.TrimStart('.') ?? "unknown",
                            ContentType = attachedFile.ContentType,
                            FileSizeBytes = (int)attachedFile.Size,
                            StorageType = "Database",
                            FileData = attachedFile.Data,
                            UploadedBy = "CONFIDENTIAL_USER",
                            UploadedDate = DateTime.UtcNow,
                            IsActive = true,
                            IsConfidential = true
                        };

                        var createFileCommand = new CreateHazardFileCommand(hazardFile);
                        await Mediator.SendAsync(createFileCommand, CancellationToken.None);
                        
                        Logger.LogInformation("Confidential file attachment processed: {FileName}", attachedFile.FileName);
                    }
                    catch (Exception fileEx)
                    {
                        Logger.LogWarning(fileEx, "Failed to process confidential attachment: {FileName}", attachedFile.FileName);
                        // Continue processing other files
                    }
                }
            }

            // ===============================
            // SUCCESS - SHOW CONFIRMATION
            // ===============================
            SubmissionDateTime = DateTime.Now;
            ShowFinalSuccessConfirmation = true;

            // Clear the form changed flag to prevent browser warning
            try
            {
                await JSRuntime.InvokeVoidAsync("clearFormChanged");
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Could not clear form changed flag");
            }

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Confidential Report Submitted",
                Detail = $"Your confidential report has been securely submitted. Tracking ID: {GeneratedTrackingId}",
                Duration = 8000
            });

            Logger.LogInformation("? Confidential report successfully submitted - TrackingId: {TrackingId}, HazardId: {HazardId}, ReportId: {ReportId}", 
                GeneratedTrackingId, GeneratedHazardId, actualReportCode);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "? Error during confidential report submission");
            
            ShowSubmissionConfirmation = false;
            
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Submission Failed",
                Detail = "An error occurred while submitting your confidential report. Please try again or contact support.",
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

    #region Dropdown and Smart Enum Methods

    private void InitializeDropdownOptions()
    {
        HazardCategoryOptions = HazardCategory.GetAllValues()
            .Select(hc => new DropdownOption(hc.Value, hc.Name))
            .ToList();
        HazardTypeOptions = new List<DropdownOption>();
    }

    public async Task OnHazardCategoryChanged(string? categoryValue)
    {
        Logger.LogInformation("Hazard category changed to: {Category}", categoryValue);
        SelectedHazardCategory = categoryValue;
        HazardReport.HazardType = null;
        await LoadHazardTypesForCategory(categoryValue ?? "", preserveSelectedType: false);
    }

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
            var currentSelectedType = preserveSelectedType ? HazardReport.HazardType : null;
            
            HazardTypeOptions = HazardType.GetByCategory(category)
                .Select(ht => new DropdownOption(ht.Value, ht.Name))
                .ToList();
            
            if (preserveSelectedType && !string.IsNullOrEmpty(currentSelectedType))
            {
                var typeExists = HazardTypeOptions.Any(ht => ht.Value == currentSelectedType);
                if (typeExists)
                {
                    HazardReport.HazardType = currentSelectedType;
                }
            }
            
            Logger.LogInformation("Loaded {Count} hazard types for category: {Category}", 
                HazardTypeOptions.Count, category.Name);
        }
        else
        {
            HazardTypeOptions.Clear();
        }
        
        await InvokeAsync(StateHasChanged);
    }

    public string GetHazardTypeGuidance(string? hazardTypeValue)
    {
        if (string.IsNullOrEmpty(hazardTypeValue)) return string.Empty;
        var hazardType = HazardType.FromValue(hazardTypeValue);
        return hazardType?.GuidanceText ?? string.Empty;
    }

    public bool RequiresRegulatoryReporting(string? hazardTypeValue)
    {
        if (string.IsNullOrEmpty(hazardTypeValue)) return false;
        var hazardType = HazardType.FromValue(hazardTypeValue);
        return hazardType?.RequiresRegulatoryReporting ?? false;
    }

    public string GetHazardCategoryDescription(string? categoryValue)
    {
        if (string.IsNullOrEmpty(categoryValue)) return string.Empty;
        var category = HazardCategory.FromValue(categoryValue);
        return category?.Description ?? string.Empty;
    }

    #endregion

    #region Helpers and Utility Methods

    private void InitializeFormDefaults()
    {
        var currentUser = SessionService.GetCurrentUserDisplayName();
        var tenMinutesAgo = DateTime.Now.AddMinutes(-10);
        
        HazardReport = new HazardReportForm
        {
            ReportedBy = currentUser ?? "Anonymous User",
            ReportedOn = new DateTime(tenMinutesAgo.Year, tenMinutesAgo.Month, tenMinutesAgo.Day,
                tenMinutesAgo.Hour, tenMinutesAgo.Minute, 0),
            IsConfidential = true
        };

        SelectedGeoLocation = new GeoLocationData
        {
            Latitude = 0,
            Longitude = 0,
            Description = "Not set",
            SelectedDateTime = DateTime.UtcNow
        };

        SelectedFiles = new List<IBrowserFile>().AsReadOnly();
        AttachedFiles.Clear();
        SelectedHazardCategory = null;
        HazardTypeOptions.Clear();
        ShowPreview = false;
        ShowMapModal = false;
        ShowSubmissionConfirmation = false;
        ShowFinalSuccessConfirmation = false;
    }

    private string GetSelectedLocationText()
    {
        if (!HasGeoLocation) return "No location selected";
        var lat = SelectedGeoLocation.Latitude;
        var lng = SelectedGeoLocation.Longitude;
        return $"Lat: {lat:F6}, Lng: {lng:F6} - {SelectedGeoLocation.Description}";
    }

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

    private string GetHazardTypeDisplay(string? key)
    {
        if (string.IsNullOrEmpty(key)) return "UNKNOWN";
        var hazardType = HazardType.FromValue(key);
        return hazardType?.Name ?? key;
    }

    private string GetHazardCategoryDisplay(string? key)
    {
        if (string.IsNullOrEmpty(key)) return "UNKNOWN";
        var category = HazardCategory.FromValue(key);
        return category?.Name ?? key;
    }

    private string GenerateAnonymousTrackingId()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomComponent = Random.Shared.Next(1000, 9999);
        var checksum = (timestamp.GetHashCode() + randomComponent).ToString().Substring(0, 2);
        return $"CONF-{timestamp}-{randomComponent}-{checksum}";
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
            
            var selectedFilesList = SelectedFiles.ToList();
            var selectedFileToRemove = selectedFilesList.FirstOrDefault(sf => 
                sf.Name == fileToRemove.FileName && sf.Size == fileToRemove.Size);
            
            if (selectedFileToRemove != null)
            {
                selectedFilesList.Remove(selectedFileToRemove);
                SelectedFiles = selectedFilesList.AsReadOnly();
            }
            
            Logger.LogInformation("Removed file from confidential report: {FileName}", fileToRemove.FileName);
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
        
        Logger.LogInformation("Cleared all files from confidential report queue");
        StateHasChanged();
    }

    /// <summary>
    /// Clear form data
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