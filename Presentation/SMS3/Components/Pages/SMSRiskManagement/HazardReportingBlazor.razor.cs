using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;
using SMS_Application.Messaging.Queries;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Shared.Common;
using Radzen;
using Radzen.Blazor;

namespace SMS3.Components.Pages.SMSRiskManagement;

/// <summary>
/// Blazor version of the HazardReporting page - Full functionality with JavaScript mapping integration
/// Converted from CSHTML to provide complete Blazor experience while maintaining all original features
/// </summary>
public partial class HazardReportingBlazor : ComponentBase, IDisposable
{
    #region Dependencies
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ISMSSessionService SessionService { get; set; } = default!;
    [Inject] private ILogger<HazardReportingBlazor> Logger { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
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
    /// Success confirmation display state
    /// </summary>
    public bool ShowFinalSuccessConfirmation { get; set; }

    /// <summary>
    /// Show confidential reporting details
    /// </summary>
    public bool ShowConfidentialInfo { get; set; }

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
    /// Generated Report ID after successful submission
    /// </summary>
    public string? GeneratedReportId { get; set; }

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
    /// Hazard type dropdown options
    /// </summary>
    public List<DropdownOption> HazardTypeOptions { get; set; } = new();

    /// <summary>
    /// Department dropdown options
    /// </summary>
    public List<DropdownOption> DepartmentOptions { get; set; } = new();

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

    // Airport coordinates
    private double AirportCenterLatitude => 45.5898;
    private double AirportCenterLongitude => -122.5951;
    private int DefaultZoomLevel => 15;

    private IJSObjectReference? _mapModule;
    private DotNetObjectReference<HazardReportingBlazor>? _dotNetRef;

    #endregion

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        InitializeDropdownOptions();
        InitializeFormDefaults();
        
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

    #region Form Event Handlers

    /// <summary>
    /// Handle form submission
    /// </summary>
    public async Task HandleFormSubmit(HazardReportForm formData)
    {
        try
        {
            Logger.LogInformation("Form submit triggered with data: HazardType={HazardType}, ReportedBy={ReportedBy}", 
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
    /// Handle file selection
    /// </summary>
    public void OnFilesSelected(IReadOnlyList<IBrowserFile> files)
    {
        SelectedFiles = files;
        ProcessAttachedFiles();
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
            Description = string.IsNullOrEmpty(LocationDescription) ? 
                $"Map Location ({SelectedLatitude:F6}, {SelectedLongitude:F6})" : LocationDescription,
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

    /// <summary>
    /// Get current GPS location
    /// </summary>
    public async Task GetCurrentLocation()
    {
        if (_mapModule != null)
        {
            try
            {
                await _mapModule.InvokeVoidAsync("getCurrentLocation");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error getting current location");
                
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Location Error",
                    Detail = "Unable to get current location. Please check your device settings.",
                    Duration = 3000
                });
            }
        }
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
        
        Logger.LogInformation("Map location selected: {Lat}, {Lng}, {Desc}", latitude, longitude, description);
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

    /// <summary>
    /// Close final confirmation and reset form
    /// </summary>
    public void CloseFinalConfirmation()
    {
        // Clear all form data after successful submission
        HazardReport = new HazardReportForm();
        SelectedGeoLocation = new GeoLocationData();
        SelectedFiles = new List<IBrowserFile>();
        AttachedFiles.Clear();
        GeneratedHazardId = null;
        GeneratedReportId = null;
        SubmissionDateTime = null;
        
        // Reset coordinates
        SelectedLatitude = 0;
        SelectedLongitude = 0;
        LocationDescription = string.Empty;
        
        // Reset all UI state flags
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
            Detail = "Form cleared successfully. Ready for new hazard report.",
            Duration = 3000
        });
    }

    #endregion

    #region Business Logic Methods

    /// <summary>
    /// Submit the hazard report confirmed - Updated to match CSHTML version logic
    /// </summary>
    public async Task SubmitReportConfirmed()
    {
        InitializeDropdownOptions();

        try
        {
            // ???????????????????????????????????????????????????????????????????
            // STEP 1: VALIDATION
            // ???????????????????????????????????????????????????????????????????
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

            Logger.LogInformation("Starting hazard report submission for user: {User}", HazardReport.ReportedBy);

            // ???????????????????????????????????????????????????????????????????
            // STEP 2: CREATE INITIAL HAZARD OBJECT FROM FORM DATA
            // ???????????????????????????????????????????????????????????????????
            var hazardCode = "HZ-0000";
            var hazard = new Hazard(new HazardID(hazardCode))
            {
                Code = hazardCode,
                Name = HazardReport.HazardType,
                Description = HazardReport.Description,
                Category = HazardReport.HazardType,
                HazardType = HazardReport.HazardType,
                ReportedBy = HazardReport.ReportedBy,
                ReportedOn = HazardReport.ReportedOn,
                ReportingDepartment = HazardReport.ReportingDepartment,
                IsConfidential = HazardReport.IsConfidential,
                IsAnonymous = false,
            };

            // Set location information - simplified since Location is now an entity
            if (!HasGeoLocation && !string.IsNullOrEmpty(HazardReport.Location))
            {
                // For text-based location, we'll set it in the LocationArea field instead
                hazard.LocationArea = HazardReport.Location;
            }

            Logger.LogInformation("Initial hazard object created - Name: '{Name}', Type: '{Type}', Category: '{Category}'",
                hazard.Name, hazard.HazardType, hazard.Category);

            // ???????????????????????????????????????????????????????????????????
            // PHASE 1: CREATE PARENT REPORT
            // ???????????????????????????????????????????????????????????????????
            var report = new Report(new ReportID("RP-0000"))
            {
                Code = "RP-0000",
                Name = HazardReport.HazardType,
                Description = hazard.Description,
                Stage = "Initial",
                Status = "Initial"
            };

            var reportResult = await Mediator.SendAsync(new CreateReportCommand(report), CancellationToken.None);
            
            if (reportResult.IsFailure)
            {
                throw new Exception($"Failed to create report: {reportResult.Error?.Message}");
            }
            
            var actualReportCode = reportResult.Value.Code;
            Logger.LogInformation("Report created with Code: {ReportCode}", actualReportCode);

            // ???????????????????????????????????????????????????????????????????
            // PHASE 2: CREATE HAZARD WITH REPORT LINKAGE
            // ???????????????????????????????????????????????????????????????????
            hazard.ReportCode = actualReportCode;
            hazard.ScoringPanelCode = null;

            var createHazardCommand = new CreateHazardCommand(hazard);
            var createdHazardResult = await Mediator.SendAsync(createHazardCommand, CancellationToken.None);
            
            if (createdHazardResult.IsFailure)
            {
                throw new Exception($"Failed to create hazard: {createdHazardResult.Error?.Message}");
            }
            
            var createdHazard = createdHazardResult.Value;
            Logger.LogInformation("Hazard created with Code: {HazardCode}, linked to Report: {ReportCode}", 
                createdHazard.Code, actualReportCode);

            // ???????????????????????????????????????????????????????????????????
            // PHASE 3: CREATE SCORING PANEL FOR RISK ASSESSMENT -- NOW DONE WHEN HAZARD IS CREATED
            // ???????????????????????????????????????????????????????????????????
            // Scoring panel creation is now handled automatically when hazard is created
            Logger.LogInformation("Scoring panel creation handled automatically during hazard creation");

            // ???????????????????????????????????????????????????????????????????
            // PHASE 4: CREATE GEOGRAPHIC LOCATION (IF PROVIDED)
            // ???????????????????????????????????????????????????????????????????
            if (HasGeoLocation)
            {
                try
                {
                    var hazardLocationResult = await Mediator.SendAsync(new GetHazardLocationsByHazardCodeQuery(createdHazard.Code), CancellationToken.None);

                    if (hazardLocationResult.IsSuccess && hazardLocationResult.Value.Any())
                    {
                        var hazardLocation = hazardLocationResult.Value.FirstOrDefault();
                        if (hazardLocation != null)
                        {
                            hazardLocation.HazardCode = createdHazard.Code;
                            hazardLocation.Latitude = SelectedGeoLocation.Latitude;
                            hazardLocation.Longitude = SelectedGeoLocation.Longitude;
                            hazardLocation.Description = SelectedGeoLocation.Description ?? "Map selected location";
                            createdHazard.HazardLocation = hazardLocation;

                            var locationUpdateResult = await Mediator.SendAsync(new UpdateHazardLocationCommand(hazardLocation), CancellationToken.None);

                            if (locationUpdateResult.IsSuccess)
                            {
                                Logger.LogInformation("HazardLocation updated with Code: {LocationCode}, Coordinates: ({Lat}, {Lng})",
                                    hazardLocation.Code, SelectedGeoLocation.Latitude, SelectedGeoLocation.Longitude);
                            }
                        }
                    }

                    // Also set coordinate information in hazard fields for backward compatibility
                    createdHazard.LocationArea = $"Lat: {SelectedGeoLocation.Latitude:F6}, Lng: {SelectedGeoLocation.Longitude:F6}";
                    if (!string.IsNullOrEmpty(SelectedGeoLocation.Description))
                    {
                        createdHazard.LocationSubArea = SelectedGeoLocation.Description;
                    }
                }
                catch (Exception locationEx)
                {
                    Logger.LogWarning(locationEx, "Failed to create/update hazard location, but continuing with hazard creation");
                    
                    // Set location in hazard fields as fallback
                    createdHazard.LocationArea = $"Lat: {SelectedGeoLocation.Latitude:F6}, Lng: {SelectedGeoLocation.Longitude:F6}";
                    if (!string.IsNullOrEmpty(SelectedGeoLocation.Description))
                    {
                        createdHazard.LocationSubArea = SelectedGeoLocation.Description;
                    }
                }
            }

            // ???????????????????????????????????????????????????????????????????
            // PHASE 5: PROCESS FILES - CREATE HAZARDFILES FROM BLAZOR FILES
            // ???????????????????????????????????????????????????????????????????
            try
            {
                if (SelectedFiles?.Any() == true)
                {
                    Logger.LogInformation("?? Processing {Count} files for Hazard: {HazardCode}", 
                        SelectedFiles.Count, createdHazard.Code);

                    var createdFileIds = new List<string>();

                    foreach (var browserFile in SelectedFiles.Where(f => f?.Size > 0))
                    {
                        try
                        {
                            // Read file data from Blazor IBrowserFile
                            byte[] fileData;
                            using (var stream = browserFile.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024))
                            using (var memoryStream = new MemoryStream())
                            {
                                await stream.CopyToAsync(memoryStream);
                                fileData = memoryStream.ToArray();
                            }

                            // Generate unique file code
                            var fileCode = "HF-0000";

                            // Create HazardFile entity
                            var hazardFile = new HazardFile(new HazardFileID(fileCode))
                            {
                                Code = fileCode,
                                HazardCode = createdHazard.Code,
                                ReportCode = createdHazard.ReportCode ?? string.Empty,
                                FileName = browserFile.Name,
                                FileType = Path.GetExtension(browserFile.Name)?.TrimStart('.') ?? "unknown",
                                ContentType = browserFile.ContentType ?? "application/octet-stream",
                                FileSizeBytes = browserFile.Size,
                                StorageType = "Database",
                                FileData = fileData,
                                UploadedBy = HazardReport.ReportedBy ?? "SYSTEM",
                                UploadedDate = DateTime.UtcNow,
                                IsActive = true,
                                IsConfidential = HazardReport.IsConfidential
                            };

                            // Send CreateHazardFileCommand for each file
                            Logger.LogInformation("Creating HazardFile: {FileName} with Code: {FileCode}", 
                                browserFile.Name, fileCode);

                            var createHazardFileCommand = new CreateHazardFileCommand(hazardFile);
                            var hazardFileResult = await Mediator.SendAsync(createHazardFileCommand, CancellationToken.None);

                            if (hazardFileResult.IsSuccess)
                            {
                                var createdFileId = hazardFileResult.Value.Code;
                                createdFileIds.Add(createdFileId);

                                // Add HazardFile ID to Hazard's collection
                                createdHazard.AddHazardFile(new HazardFileID(createdFileId));

                                Logger.LogInformation("? Created HazardFile: {FileName} with ID: {FileId} for Hazard: {HazardCode}", 
                                    browserFile.Name, createdFileId, createdHazard.Code);
                            }
                            else
                            {
                                Logger.LogError("? Failed to create HazardFile: {FileName} for Hazard: {HazardCode}. Error: {Error}", 
                                    browserFile.Name, createdHazard.Code, hazardFileResult.Error?.Message);
                            }
                        }
                        catch (Exception fileEx)
                        {
                            Logger.LogError(fileEx, "? Exception creating HazardFile: {FileName} for Hazard: {HazardCode}", 
                                browserFile.Name, createdHazard.Code);
                        }
                    }

                    Logger.LogInformation("? File processing completed: {CreatedCount} HazardFiles created for Hazard: {HazardCode}", 
                        createdFileIds.Count, createdHazard.Code);
                }
            }
            catch (Exception fileEx)
            {
                Logger.LogError(fileEx, "? Error processing files, but continuing with hazard creation");
            }

            // ???????????????????????????????????????????????????????????????????
            // PHASE 6: FINAL UPDATE
            // ???????????????????????????????????????????????????????????????????
            var updatedHazardResult = await Mediator.SendAsync(new UpdateHazardCommand(createdHazard), CancellationToken.None);

            if (updatedHazardResult.IsFailure)
            {
                throw new Exception($"Failed to update hazard: {updatedHazardResult.Error?.Message}");
            }

            // ???????????????????????????????????????????????????????????????????
            // SUCCESS
            // ???????????????????????????????????????????????????????????????????
            var finalHazard = updatedHazardResult.Value;
            GeneratedHazardId = finalHazard.Code;
            GeneratedReportId = finalHazard.ReportCode;
            SubmissionDateTime = DateTime.Now;
            ShowSubmissionConfirmation = false;
            ShowFinalSuccessConfirmation = true;
            
            Logger.LogInformation("? Hazard report submission completed successfully - HazardCode: {HazardCode}, ReportCode: {ReportCode}", 
                finalHazard.Code, finalHazard.ReportCode);

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Report Submitted Successfully",
                Detail = $"Hazard report {finalHazard.Code} has been created and linked to report {finalHazard.ReportCode}.",
                Duration = 5000
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "? Error during hazard report submission");
            
            ShowSubmissionConfirmation = false;
            
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Submission Failed",
                Detail = "An error occurred while saving your report. Please try again.",
                Duration = 5000
            });
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    #endregion

    #region Helpers

    private void InitializeDropdownOptions()
    {
        // Hazard type options
        HazardTypeOptions = new List<DropdownOption>
        {
            new DropdownOption("Fire", "Fire"),
            new DropdownOption("Flood", "Flood"),
            new DropdownOption("Earthquake", "Earthquake"),
            new DropdownOption("Landslide", "Landslide"),
            new DropdownOption("Other", "Other")
        };

        // Sample static department options
        DepartmentOptions = new List<DropdownOption>
        {
            new DropdownOption("HR", "HR"),
            new DropdownOption("Finance", "Finance"),
            new DropdownOption("IT", "IT"),
            new DropdownOption("Operations", "Operations")
        };
    }

    private void InitializeFormDefaults()
    {
        var tenMinutesAgo = DateTime.Now.AddMinutes(-10);
        HazardReport = new HazardReportForm
        {
            ReportedBy = SessionService.GetCurrentUserDisplayName() ?? "Unknown User",
            ReportedOn = new DateTime(tenMinutesAgo.Year, tenMinutesAgo.Month, tenMinutesAgo.Day,
                tenMinutesAgo.Hour, tenMinutesAgo.Minute, 0)
        };

        SelectedGeoLocation = new GeoLocationData
        {
            Latitude = 0,
            Longitude = 0,
            Description = "Not set",
            SelectedDateTime = DateTime.UtcNow
        };

        SelectedFiles = new List<IBrowserFile>();
        AttachedFiles.Clear();

        ShowPreview = false;
        ShowConfidentialInfo = false;
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

    private string GetHazardTypeDisplay(string? key)
    {
        return string.IsNullOrEmpty(key) ? "UNKNOWN" : key;
    }

    private async Task ProcessAttachedFiles()
    {
        AttachedFiles = new List<AttachedFile>();

        if (SelectedFiles != null)
        {
            foreach (var file in SelectedFiles)
            {
                try
                {
                    var buffer = new byte[file.Size];
                    using (var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024))
                    {
                        await stream.ReadAsync(buffer, 0, (int)file.Size);
                    }
                    
                    AttachedFiles.Add(new AttachedFile
                    {
                        FileName = file.Name,
                        ContentType = file.ContentType,
                        Size = file.Size,
                        Data = buffer,
                        SizeDisplay = FormatFileSize(file.Size)
                    });
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error processing file: {FileName}", file.Name);
                }
            }
        }
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

    private async Task ProcessFileAttachments(Hazard createdHazard)
    {
        if (AttachedFiles == null || !AttachedFiles.Any())
        {
            return;
        }

        // Process file attachments here
        // Implementation would depend on your file storage requirements
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
            // Clear all form data
            HazardReport = new HazardReportForm();
            SelectedGeoLocation = new GeoLocationData();
            SelectedFiles = new List<IBrowserFile>();
            AttachedFiles.Clear();
            
            // Reset coordinates
            SelectedLatitude = 0;
            SelectedLongitude = 0;
            LocationDescription = string.Empty;
            
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

#region Data Classes

/// <summary>
/// Form data for hazard report creation
/// </summary>
public class HazardReportForm
{
    public string? HazardType { get; set; }
    public string? Description { get; set; }
    public string? ReportedBy { get; set; }
    public DateTime ReportedOn { get; set; } = DateTime.Now;
    public string? ReportingDepartment { get; set; }
    public string? Location { get; set; }
    public bool IsConfidential { get; set; }
}

/// <summary>
/// Geographic location data
/// </summary>
public class GeoLocationData
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime SelectedDateTime { get; set; }
    public bool IsValid => Latitude != 0 && Longitude != 0;
}

/// <summary>
/// Dropdown option for form controls
/// </summary>
public class DropdownOption
{
    public string Value { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    
    public DropdownOption() { }
    public DropdownOption(string value, string text)
    {
        Value = value;
        Text = text;
    }
}

/// <summary>
/// File attachment information
/// </summary>
public class AttachedFile
{
    public string FileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string SizeDisplay { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public long Size { get; set; }
}

#endregion