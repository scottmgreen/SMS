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

using SMS_Shared.Common;

namespace SMS3.Components.Pages.SMSRiskManagement;

/// <summary>
/// Confidential Reporting page - Enhanced security for sensitive safety reports
/// Provides anonymous/confidential reporting with special protection measures
/// Based on HazardReporting but adapted for confidential submissions
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
    /// Main form data object
    /// </summary>
    public ConfidentialReportForm ConfidentialReport { get; set; } = new();

    /// <summary>
    /// File selection for attachments
    /// </summary>
    public IReadOnlyList<IBrowserFile> SelectedFiles { get; set; } = new List<IBrowserFile>();

    /// <summary>
    /// Processed file attachments for display
    /// </summary>
    public List<ConfidentialAttachedFile> AttachedFiles { get; set; } = new();

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
    /// Show submission confirmation modal
    /// </summary>
    public bool ShowSubmissionConfirmation { get; set; }

    /// <summary>
    /// Generated Tracking ID after successful submission
    /// </summary>
    public string? GeneratedTrackingId { get; set; }

    /// <summary>
    /// Submission timestamp
    /// </summary>
    public DateTime? SubmissionDateTime { get; set; }

    /// <summary>
    /// Report type dropdown options
    /// </summary>
    public List<ConfidentialDropdownOption> ReportTypeOptions { get; set; } = new();

    /// <summary>
    /// Priority dropdown options
    /// </summary>
    public List<ConfidentialDropdownOption> PriorityOptions { get; set; } = new();

    /// <summary>
    /// Character count for description field
    /// </summary>
    public int DescriptionCharacterCount => ConfidentialReport?.Description?.Length ?? 0;

    /// <summary>
    /// Check if form has minimum required fields for preview
    /// </summary>
    public bool IsFormValidForPreview => 
        !string.IsNullOrEmpty(ConfidentialReport.ReportType) &&
        !string.IsNullOrEmpty(ConfidentialReport.Location) &&
        !string.IsNullOrEmpty(ConfidentialReport.Description);

    /// <summary>
    /// Check if form is valid for submission
    /// </summary>
    public bool IsFormValidForSubmission => 
        IsFormValidForPreview && 
        DescriptionCharacterCount <= 2000;

    /// <summary>
    /// Page title
    /// </summary>
    public string PageTitle => "Submit Confidential Report";

    /// <summary>
    /// Page subtitle
    /// </summary>
    public string PageSubtitle => "Enhanced protection for sensitive safety reports with reporter anonymity";

    #endregion

    #region Map/Location Properties

    /// <summary>
    /// Show map modal for location selection
    /// </summary>
    public bool ShowMapModal { get; set; }

    /// <summary>
    /// Selected latitude from map
    /// </summary>
    public decimal SelectedLatitude { get; set; }

    /// <summary>
    /// Selected longitude from map  
    /// </summary>
    public decimal SelectedLongitude { get; set; }

    /// <summary>
    /// Location description for map selection
    /// </summary>
    public string LocationDescription { get; set; } = string.Empty;

    /// <summary>
    /// Geographic location data
    /// </summary>
    public GeoLocationData SelectedGeoLocation { get; set; } = new();

    /// <summary>
    /// Check if we have valid coordinates
    /// </summary>
    public bool HasValidCoordinates => SelectedLatitude != 0 && SelectedLongitude != 0;

    /// <summary>
    /// Check if we have geographic location data
    /// </summary>
    public bool HasGeoLocation => SelectedGeoLocation?.IsValid == true;

    /// <summary>
    /// Display text for geographic location
    /// </summary>
    public string GeoLocationDisplay => HasGeoLocation ? 
        $"Lat: {SelectedGeoLocation.Latitude:F6}, Lng: {SelectedGeoLocation.Longitude:F6}" : 
        "No coordinates selected";

    /// <summary>
    /// Display text for location field
    /// </summary>
    public string LocationDisplayText
    {
        get
        {
            if (HasGeoLocation)
            {
                var locationText = GeoLocationDisplay;
                if (!string.IsNullOrEmpty(SelectedGeoLocation.Description))
                {
                    locationText += $" - {SelectedGeoLocation.Description}";
                }
                return locationText;
            }

            if (!string.IsNullOrEmpty(ConfidentialReport.Location) && ConfidentialReport.Location != "MAP_LOCATION")
            {
                return ConfidentialReport.Location;
            }

            return "No location selected - click 'Select on Map'";
        }
    }

    // CRITICAL MISSING PROPERTIES FOR MAP FUNCTIONALITY:
    // Airport coordinates
    private double AirportCenterLatitude => 45.5898;
    private double AirportCenterLongitude => -122.5951;
    private int DefaultZoomLevel => 15;

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
    public async Task HandleFormSubmit(ConfidentialReportForm formData)
    {
        try
        {
            Logger.LogInformation("Confidential form submit triggered with data: ReportType={ReportType}, Location={Location}", 
                formData.ReportType, formData.Location);

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
    /// Handle file selection
    /// </summary>
    public void OnFilesSelected(IReadOnlyList<IBrowserFile> files)
    {
        SelectedFiles = files;
        ProcessAttachedFiles();
        StateHasChanged();
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
    /// Close final confirmation and reset form
    /// </summary>
    public void CloseFinalConfirmation()
    {
        // Clear all form data after successful submission
        ConfidentialReport = new ConfidentialReportForm();
        SelectedFiles = new List<IBrowserFile>();
        AttachedFiles.Clear();
        GeneratedTrackingId = null;
        SubmissionDateTime = null;
        
        // Reset all UI state flags
        ShowPreview = false;
        ShowSubmissionConfirmation = false;
        ShowFinalSuccessConfirmation = false;

        InitializeFormDefaults();
        StateHasChanged();

        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Info,
            Summary = "Form Cleared",
            Detail = "Form cleared successfully. Ready for new confidential report.",
            Duration = 3000
        });
    }

    #endregion

    #region Business Logic Methods

    /// <summary>
    /// Submit the confidential report confirmed
    /// </summary>
    public async Task SubmitReportConfirmed()
    {
        try
        {
            // ??????????????????????????????????????????????????????????????????????????
            // STEP 1: VALIDATION
            // ??????????????????????????????????????????????????????????????????????????
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

            var currentUser = SessionService.GetCurrentUserDisplayName() ?? "Anonymous User";
            Logger.LogInformation("Starting confidential report submission for user: {User}", currentUser);

            // Generate anonymous tracking ID
            GeneratedTrackingId = GenerateAnonymousTrackingId();

            // ??????????????????????????????????????????????????????????????????????????
            // STEP 2: CREATE CONFIDENTIAL HAZARD OBJECT
            // ??????????????????????????????????????????????????????????????????????????
            var hazardCode = "HZ-0000";
            var hazard = new Hazard(new HazardID(hazardCode))
            {
                Code = hazardCode,
                Name = $"{ConfidentialReport.ReportType}",
                Description = ConfidentialReport.Description,
                HazardCategory = ConfidentialReport.ReportType,
                HazardType = ConfidentialReport.ReportType,
                ReportedBy = "CONFIDENTIAL REPORTER",  // Anonymize the reporter
                ReportedOn = DateTime.Now,
                ReportingDepartment = "Confidential",
                IsConfidential = true,  // Always confidential
                IsAnonymous = true,     // Always anonymous
                LocationArea = ConfidentialReport.Location,
                // Store additional confidential data in appropriate fields
                AdditionalComments = $"Priority: {ConfidentialReport.Priority}"
            };

            // Add special circumstances to the description if any are selected
            var circumstances = GetSpecialCircumstancesText();
            if (!string.IsNullOrEmpty(circumstances))
            {
                hazard.Description += $"\n\nSpecial Circumstances: {circumstances}";
            }

            if (!string.IsNullOrEmpty(ConfidentialReport.AdditionalProtection))
            {
                hazard.Description += $"\n\nAdditional Protection Needed: {ConfidentialReport.AdditionalProtection}";
            }

            Logger.LogInformation("Confidential hazard object created - Type: '{Type}', Tracking: '{TrackingId}'",
                hazard.HazardType, GeneratedTrackingId);

            // ??????????????????????????????????????????????????????????????????????????
            // PHASE 1: CREATE PARENT REPORT (CONFIDENTIAL)
            // ??????????????????????????????????????????????????????????????????????????
            var report = new Report(new ReportID("RP-0000"))
            {
                Code = "RP-0000",
                Name = $"Confidential Report - {ConfidentialReport.ReportType}",
                ReportedBy = "CONFIDENTIAL REPORTER",
                ReportedOn = DateTime.Now,
                Department = "Confidential",  
                Description = hazard.Description,
                Stage = "Confidential",
                Status = "Confidential"
            };

            var reportResult = await Mediator.SendAsync(new CreateReportCommand(report), CancellationToken.None);
            
            if (reportResult.IsFailure)
            {
                throw new Exception($"Failed to create confidential report: {reportResult.Error?.Message}");
            }
            
            var actualReportCode = reportResult.Value.Code;
            Logger.LogInformation("Confidential report created with Code: {ReportCode}, Tracking: {TrackingId}", 
                actualReportCode, GeneratedTrackingId);

            // ??????????????????????????????????????????????????????????????????????????
            // PHASE 2: CREATE CONFIDENTIAL HAZARD WITH REPORT LINKAGE
            // ??????????????????????????????????????????????????????????????????????????

            hazard.ReportCode = actualReportCode;
            hazard.RiskMatrixCode = null;
            hazard.HazardType = "Confidential";
            
            // Store tracking ID in a field for reference (without exposing user identity)
            hazard.AdditionalComments = $"Tracking ID: {GeneratedTrackingId}; Priority: {ConfidentialReport.Priority}";

            var createHazardCommand = new CreateHazardCommand(hazard);
            var createdHazardResult = await Mediator.SendAsync(createHazardCommand, CancellationToken.None);
            
            if (createdHazardResult.IsFailure)
            {
                throw new Exception($"Failed to create confidential hazard: {createdHazardResult.Error?.Message}");
            }
            
            var createdHazard = createdHazardResult.Value;
            Logger.LogInformation("Confidential hazard created with Code: {HazardCode}, linked to Report: {ReportCode}, Tracking: {TrackingId}", 
                createdHazard.Code, actualReportCode, GeneratedTrackingId);

            // ??????????????????????????????????????????????????????????????????????????
            // PHASE 3: PROCESS CONFIDENTIAL FILES (IF PROVIDED)
            // ??????????????????????????????????????????????????????????????????????????
            try
            {
                if (SelectedFiles?.Any() == true)
                {
                    Logger.LogInformation("?? Processing {Count} confidential files for Hazard: {HazardCode}, Tracking: {TrackingId}", 
                        SelectedFiles.Count, createdHazard.Code, GeneratedTrackingId);

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

                            // Create HazardFile entity for confidential file
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
                                UploadedBy = "CONFIDENTIAL REPORTER", // Anonymize uploader
                                UploadedDate = DateTime.UtcNow,
                                IsActive = true,
                                IsConfidential = true // Always confidential
                            };

                            // Send CreateHazardFileCommand for each file
                            Logger.LogInformation("Creating confidential HazardFile: {FileName} with Code: {FileCode}, Tracking: {TrackingId}", 
                                browserFile.Name, fileCode, GeneratedTrackingId);

                            var createHazardFileCommand = new CreateHazardFileCommand(hazardFile);
                            var hazardFileResult = await Mediator.SendAsync(createHazardFileCommand, CancellationToken.None);

                            if (hazardFileResult.IsSuccess)
                            {
                                var createdFileId = hazardFileResult.Value.Code;
                                createdFileIds.Add(createdFileId);

                                // Add HazardFile ID to Hazard's collection
                                createdHazard.AddHazardFile(new HazardFileID(createdFileId));

                                Logger.LogInformation("? Created confidential HazardFile: {FileName} with ID: {FileId} for Hazard: {HazardCode}", 
                                    browserFile.Name, createdFileId, createdHazard.Code);
                            }
                            else
                            {
                                Logger.LogError("? Failed to create confidential HazardFile: {FileName} for Hazard: {HazardCode}. Error: {Error}", 
                                    browserFile.Name, createdHazard.Code, hazardFileResult.Error?.Message);
                            }
                        }
                        catch (Exception fileEx)
                        {
                            Logger.LogError(fileEx, "? Exception creating confidential HazardFile: {FileName} for Hazard: {HazardCode}", 
                                browserFile.Name, createdHazard.Code);
                        }
                    }

                    Logger.LogInformation("?? Confidential file processing completed: {CreatedCount} HazardFiles created for Hazard: {HazardCode}", 
                        createdFileIds.Count, createdHazard.Code);
                }
            }
            catch (Exception fileEx)
            {
                Logger.LogError(fileEx, "?? Error processing confidential files, but continuing with hazard creation");
            }

            // ??????????????????????????????????????????????????????????????????????????
            // PHASE 4: FINAL UPDATE
            // ??????????????????????????????????????????????????????????????????????????
            var updatedHazardResult = await Mediator.SendAsync(new UpdateHazardCommand(createdHazard), CancellationToken.None);

            if (updatedHazardResult.IsFailure)
            {
                throw new Exception($"Failed to update confidential hazard: {updatedHazardResult.Error?.Message}");
            }

            // ??????????????????????????????????????????????????????????????????????????
            // SUCCESS
            // ??????????????????????????????????????????????????????????????????????????
            SubmissionDateTime = DateTime.Now;
            ShowSubmissionConfirmation = false;
            ShowFinalSuccessConfirmation = true;
            
            // Clear the form changed flag to prevent navigation warnings
            try
            {
                await JSRuntime.InvokeVoidAsync("clearFormChanged");
            }
            catch (Exception jsEx)
            {
                Logger.LogWarning(jsEx, "Could not clear form changed flag via JavaScript");
            }
            
            Logger.LogInformation("? Confidential report submission completed successfully - HazardCode: {HazardCode}, ReportCode: {ReportCode}, Tracking: {TrackingId}", 
                createdHazard.Code, createdHazard.ReportCode, GeneratedTrackingId);

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Confidential Report Submitted Successfully",
                Detail = $"Your confidential report has been submitted with tracking ID {GeneratedTrackingId}.",
                Duration = 5000
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "? Error during confidential report submission, Tracking: {TrackingId}", GeneratedTrackingId);
            
            ShowSubmissionConfirmation = false;
            
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Submission Failed",
                Detail = "An error occurred while saving your confidential report. Please try again or contact the confidential hotline.",
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
        // Report type options for confidential reports
        ReportTypeOptions = new List<ConfidentialDropdownOption>
        {
            new ConfidentialDropdownOption("Safety Concern", "General Safety Concern"),
            new ConfidentialDropdownOption("Retaliation", "Retaliation or Intimidation"),
            new ConfidentialDropdownOption("Personnel Issue", "Personnel Safety Issue"),
            new ConfidentialDropdownOption("Compliance Violation", "Regulatory Compliance Violation"),
            new ConfidentialDropdownOption("Management Issue", "Management Safety Issue"),
            new ConfidentialDropdownOption("System Failure", "Safety System Failure"),
            new ConfidentialDropdownOption("Cover-up", "Attempted Cover-up"),
            new ConfidentialDropdownOption("Other", "Other Sensitive Matter")
        };

        // Priority options
        PriorityOptions = new List<ConfidentialDropdownOption>
        {
            new ConfidentialDropdownOption("Low", "Low - General concern"),
            new ConfidentialDropdownOption("Medium", "Medium - Moderate safety impact"),
            new ConfidentialDropdownOption("High", "High - Significant safety risk"),
            new ConfidentialDropdownOption("Critical", "Critical - Imminent danger")
        };
    }

    private void InitializeFormDefaults()
    {
        ConfidentialReport = new ConfidentialReportForm
        {
            Priority = "Medium" // Default priority
        };

        SelectedFiles = new List<IBrowserFile>();
        AttachedFiles.Clear();

        ShowPreview = false;
        ShowSubmissionConfirmation = false;
        ShowFinalSuccessConfirmation = false;
    }

    private async Task ProcessAttachedFiles()
    {
        AttachedFiles = new List<ConfidentialAttachedFile>();

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
                    
                    AttachedFiles.Add(new ConfidentialAttachedFile
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
                    Logger.LogError(ex, "Error processing confidential file: {FileName}", file.Name);
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

    /// <summary>
    /// Generate anonymous tracking ID for confidential reports
    /// </summary>
    private string GenerateAnonymousTrackingId()
    {
        // Generate a secure anonymous tracking ID with timestamp and random component
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomComponent = Random.Shared.Next(1000, 9999);
        var checksum = (timestamp.GetHashCode() + randomComponent).ToString().Substring(0, 2);
        
        return $"CONF-{timestamp}-{randomComponent}-{checksum}";
    }

    /// <summary>
    /// Get special circumstances text for display
    /// </summary>
    private string GetSpecialCircumstancesText()
    {
        var circumstances = new List<string>();
        
        if (ConfidentialReport.IsRetaliation)
            circumstances.Add("Retaliation Concern");
            
        if (ConfidentialReport.IsPersonnelIssue)
            circumstances.Add("Personnel Issue");
            
        if (ConfidentialReport.IsComplianceViolation)
            circumstances.Add("Compliance Violation");

        return circumstances.Any() ? string.Join(", ", circumstances) : "None";
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
            ConfidentialReport = new ConfidentialReportForm();
            SelectedFiles = new List<IBrowserFile>();
            AttachedFiles.Clear();
            
            // Reset UI state
            ShowPreview = false;
            ShowSubmissionConfirmation = false;
            ShowFinalSuccessConfirmation = false;
            
            InitializeFormDefaults();
            StateHasChanged();
            
            // Clear the form changed flag to prevent navigation warnings
            try
            {
                await JSRuntime.InvokeVoidAsync("clearFormChanged");
            }
            catch (Exception jsEx)
            {
                Logger.LogWarning(jsEx, "Could not clear form changed flag via JavaScript during form clear");
            }
            
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

    #region Map/Location Methods

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
                
                Logger.LogInformation("Map reinitialized for confidential modal opening");
                
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
    /// Get current location
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
    /// Clear map selection
    /// </summary>
    public async Task ClearMapSelection()
    {
        SelectedLatitude = 0;
        SelectedLongitude = 0;
        LocationDescription = string.Empty;
        SelectedGeoLocation = new GeoLocationData();
        ConfidentialReport.Location = "";
        
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
        ConfidentialReport.Location = "MAP_LOCATION";
        
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
}