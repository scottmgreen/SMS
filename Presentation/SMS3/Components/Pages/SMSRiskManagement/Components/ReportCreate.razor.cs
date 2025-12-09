using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;
using SMS_Application.Messaging.Queries;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Shared.Common;
using Radzen;
using Radzen.Blazor;

namespace SMS3.Components.Pages.SMSRiskManagement.Components;

/// <summary>
/// ReportCreate - Code-behind for ReportCreate.razor
/// Handles creation of new hazard reports with full SMS workflow integration
/// Based on the original HazardReporting page but adapted for Blazor with Radzen components
/// </summary>
public partial class ReportCreate : ComponentBase
{
    #region Dependencies
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ISMSSessionService SessionService { get; set; } = default!;
    [Inject] private ILogger<ReportCreate> Logger { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    #endregion

    #region Parameters
    [Parameter] public EventCallback OnReportCreated { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    #endregion

    #region Properties and Fields
    /// <summary>
    /// Main form data object
    /// </summary>
    public ReportHazardData HazardReportData { get; set; } = new();

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
    public bool ShowSuccessConfirmation { get; set; }

    /// <summary>
    /// Show confidential reporting details
    /// </summary>
    public bool ShowConfidentialDetails { get; set; }

    /// <summary>
    /// Generated Report ID after successful submission
    /// </summary>
    public string? GeneratedReportId { get; set; }

    /// <summary>
    /// Generated Hazard ID after successful submission
    /// </summary>
    public string? GeneratedHazardId { get; set; }

    /// <summary>
    /// Hazard type dropdown options
    /// </summary>
    public List<DropdownOption> HazardTypeOptions { get; set; } = new();

    /// <summary>
    /// Department dropdown options
    /// </summary>
    public List<DropdownOption> DepartmentOptions { get; set; } = new();

    // Location Selection Properties - Updated for compact selector
    private bool ShowLocationSelector { get; set; } = false;
    private string ManualLocationDescription { get; set; } = string.Empty;
    private string SelectedQuickLocation { get; set; } = string.Empty;

    /// <summary>
    /// Display text for selected location
    /// </summary>
    public string LocationDisplayText 
    { 
        get 
        { 
            if (!string.IsNullOrEmpty(SelectedQuickLocation))
            {
                return SelectedQuickLocation;
            }
            if (!string.IsNullOrEmpty(ManualLocationDescription))
            {
                return ManualLocationDescription.Length > 50 
                    ? ManualLocationDescription[..47] + "..." 
                    : ManualLocationDescription;
            }
            return "No location selected";
        } 
    }

    /// <summary>
    /// Check if geographic location has been selected
    /// </summary>
    public bool HasGeoLocation => !string.IsNullOrEmpty(SelectedQuickLocation) || !string.IsNullOrEmpty(ManualLocationDescription);

    /// <summary>
    /// Character count for description field
    /// </summary>
    public int DescriptionCharacterCount => HazardReportData.Description?.Length ?? 0;

    /// <summary>
    /// Check if form has minimum required fields for preview
    /// </summary>
    public bool IsFormValidForPreview => 
        !string.IsNullOrEmpty(HazardReportData.HazardType) &&
        !string.IsNullOrEmpty(HazardReportData.ReportedBy) &&
        !string.IsNullOrEmpty(HazardReportData.Description);

    /// <summary>
    /// Check if form is valid for submission
    /// </summary>
    public bool IsFormValidForSubmission => 
        IsFormValidForPreview && 
        DescriptionCharacterCount <= 2000;

    /// <summary>
    /// Shows submission confirmation dialog
    /// </summary>
    public bool ShowSubmissionConfirmation { get; set; }
    #endregion

    #region Lifecycle Methods
    /// <summary>
    /// Component initialization
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        InitializeDropdownOptions();
        InitializeFormDefaults();
    }
    #endregion

    #region Form Event Handlers
    /// <summary>
    /// Handle form submission - this is the main entry point for form submit
    /// </summary>
    /// <param name="formData">Form data</param>
    public async Task HandleFormSubmit(ReportHazardData formData)
    {
        try
        {
            Logger.LogInformation("Form submit triggered with data: HazardType={HazardType}, ReportedBy={ReportedBy}", 
                formData.HazardType, formData.ReportedBy);

            // Validate form first
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

            // Show confirmation dialog
            var confirmed = await DialogService.Confirm(
                "Are you sure you want to submit this hazard report?", 
                "Confirm Submission", 
                new ConfirmOptions() 
                { 
                    OkButtonText = "Yes, Submit", 
                    CancelButtonText = "Cancel" 
                });

            if (confirmed == true)
            {
                await SubmitReportAsync();
            }
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
    /// Legacy method - keep for backward compatibility but redirect to main handler
    /// </summary>
    /// <param name="formData">Form data</param>
    public async Task OnSubmit(ReportHazardData formData)
    {
        await HandleFormSubmit(formData);
    }

    /// <summary>
    /// Reference to the HazardLocation component
    /// </summary>
    private SMS3.Components.Pages.SMSRiskManagement.Components.HazardLocationOpenLayers hazardLocationComponent = default!;

    /// <summary>
    /// Show location selector dialog - Updated for compact selector
    /// </summary>
    public void ShowLocationSelectorPanel()
    {
        try
        {
            ShowLocationSelector = true;
            StateHasChanged();
            
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Info,
                Summary = "Location Selector",
                Detail = "Choose a location method below.",
                Duration = 3000
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error showing location selector");
            
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Location Selector",
                Detail = "Failed to open location selector. Please try again.",
                Duration = 5000
            });
        }
    }

    /// <summary>
    /// Handle location selected callback from HazardLocation component
    /// </summary>
    /// <param name="locationData">Selected location data</param>
    public void OnLocationSelectedCallback(GeoLocationData locationData)
    {
        SelectedGeoLocation = locationData;
        StateHasChanged();
        
        Logger.LogInformation("Location selected: {Location}", LocationDisplayText);
        
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = "Location Set",
            Detail = $"Location has been set: {LocationDisplayText}",
            Duration = 3000
        });
    }

    /// <summary>
    /// Handle location selection callback
    /// </summary>
    public async Task OnLocationSelected(SMS3.Components.Pages.SMSRiskManagement.Components.HazardLocationOpenLayers.GeoLocation location)
    {
        try
        {
            Logger.LogInformation("Location selected: {Latitude}, {Longitude}", location.Latitude, location.Longitude);
            
            HazardReportData.Latitude = location.Latitude;
            HazardReportData.Longitude = location.Longitude;
            HazardReportData.GeoLocation = location.ToString();
            
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing location selection");
        }
    }

    /// <summary>
    /// Get initial location for the map component
    /// </summary>
    public SMS3.Components.Pages.SMSRiskManagement.Components.HazardLocationOpenLayers.GeoLocation? GetInitialLocation()
    {
        if (HazardReportData.Latitude != 0 && HazardReportData.Longitude != 0)
        {
            return new SMS3.Components.Pages.SMSRiskManagement.Components.HazardLocationOpenLayers.GeoLocation
            {
                Latitude = HazardReportData.Latitude,
                Longitude = HazardReportData.Longitude,
                Description = HazardReportData.GeoLocation ?? ""
            };
        }
        return null;
    }

    /// <summary>
    /// Clear selected location
    /// </summary>
    public void ClearLocation()
    {
        SelectedGeoLocation = new GeoLocationData();
        StateHasChanged();
        
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Info,
            Summary = "Location Cleared",
            Detail = "Geographic location has been cleared.",
            Duration = 2000
        });
    }

    /// <summary>
    /// Show preview dialog
    /// </summary>
    public async Task ShowPreview()
    {
        try
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

            // Generate preview content
            var previewContent = GeneratePreviewContent();
            
            await DialogService.Alert(previewContent, "Report Preview");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error showing preview");
        }
    }

    /// <summary>
    /// Show confidential reporting information
    /// </summary>
    public void ShowConfidentialInfo()
    {
        ShowConfidentialDetails = !ShowConfidentialDetails;
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
            ResetForm();
            
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Info,
                Summary = "Form Cleared",
                Detail = "All form data has been cleared.",
                Duration = 2000
            });
        }
    }

    /// <summary>
    /// Handle close callback
    /// </summary>
    public async Task OnCloseCallback()
    {
        if (OnClose.HasDelegate)
        {
            await OnClose.InvokeAsync();
        }
    }
    #endregion

    #region Business Logic Methods
    /// <summary>
    /// Submit the hazard report to the SMS system
    /// </summary>
    private async Task SubmitReportAsync()
    {
        try
        {
            IsLoading = true;
            StateHasChanged();

            Logger.LogInformation("Starting hazard report submission for user: {User}", HazardReportData.ReportedBy);

            // ???????????????????????????????????????????????????????????????????
            // PHASE 1: CREATE PARENT REPORT
            // ???????????????????????????????????????????????????????????????????
            var report = new Report(new ReportID("RP-0000"))
            {
                Code = "RP-0000",
                Name = GetHazardTypeDisplay(HazardReportData.HazardType),
                Description = HazardReportData.Description,
                Stage = "Initial",
                Status = "Active"
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
            var hazardCode = "HZ-0000";
            var hazard = new Hazard(new HazardID(hazardCode))
            {
                Code = hazardCode,
                Name = GetHazardTypeDisplay(HazardReportData.HazardType),
                Description = HazardReportData.Description,
                Category = HazardReportData.HazardType ?? "OTHER",
                HazardType = HazardReportData.HazardType,
                ReportedBy = HazardReportData.ReportedBy,
                ReportedOn = HazardReportData.ReportedOn,
                ReportingDepartment = HazardReportData.ReportingDepartment,
                IsConfidential = HazardReportData.IsConfidential,
                IsAnonymous = false,
                ReportCode = actualReportCode
            };

            // Set location information
            if (HasGeoLocation)
            {
                hazard.LocationArea = $"Lat: {SelectedGeoLocation.Latitude:F6}, Lng: {SelectedGeoLocation.Longitude:F6}";
                if (!string.IsNullOrEmpty(SelectedGeoLocation.Description))
                {
                    hazard.LocationSubArea = SelectedGeoLocation.Description;
                }
            }

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
            // PHASE 3: HANDLE FILE ATTACHMENTS (if any)
            // ???????????????????????????????????????????????????????????????????
            if (SelectedFiles.Any())
            {
                await ProcessFileAttachments(createdHazard);
            }

            // ???????????????????????????????????????????????????????????????????
            // PHASE 4: SUCCESS
            // ???????????????????????????????????????????????????????????????????
            GeneratedReportId = actualReportCode;
            GeneratedHazardId = createdHazard.Code;
            ShowSuccessConfirmation = true;

            Logger.LogInformation("? Hazard report submission completed successfully - HazardCode: {HazardCode}, ReportCode: {ReportCode}", 
                createdHazard.Code, actualReportCode);

            // Notify parent component
            if (OnReportCreated.HasDelegate)
            {
                await OnReportCreated.InvokeAsync();
            }

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Report Created",
                Detail = $"Hazard report {createdHazard.Code} has been successfully created.",
                Duration = 5000
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during hazard report submission");
            
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Submission Failed",
                Detail = "An error occurred while creating your report. Please try again.",
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
    /// Process file attachments and create HazardFile entities
    /// </summary>
    /// <param name="hazard">Associated hazard entity</param>
    private async Task ProcessFileAttachments(Hazard hazard)
    {
        try
        {
            Logger.LogInformation("?? Processing {Count} files for Hazard: {HazardCode}", 
                SelectedFiles.Count, hazard.Code);

            foreach (var file in SelectedFiles)
            {
                try
                {
                    if (file.Size > 10485760) // 10MB limit
                    {
                        Logger.LogWarning("File {FileName} exceeds size limit", file.Name);
                        continue;
                    }

                    // Read file data
                    using var memoryStream = new MemoryStream();
                    await file.OpenReadStream().CopyToAsync(memoryStream);
                    var fileData = memoryStream.ToArray();

                    // Create HazardFile entity
                    var hazardFile = new HazardFile(new HazardFileID("HF-0000"))
                    {
                        Code = "HF-0000",
                        HazardCode = hazard.Code,
                        ReportCode = hazard.ReportCode ?? string.Empty,
                        FileName = file.Name,
                        FileType = Path.GetExtension(file.Name)?.TrimStart('.') ?? "unknown",
                        ContentType = file.ContentType,
                        FileSizeBytes = file.Size,
                        StorageType = "Database",
                        FileData = fileData,
                        UploadedBy = HazardReportData.ReportedBy ?? "SYSTEM",
                        UploadedDate = DateTime.UtcNow,
                        IsActive = true,
                        IsConfidential = HazardReportData.IsConfidential
                    };

                    var createHazardFileCommand = new CreateHazardFileCommand(hazardFile);
                    var hazardFileResult = await Mediator.SendAsync(createHazardFileCommand, CancellationToken.None);

                    if (hazardFileResult.IsSuccess)
                    {
                        Logger.LogInformation("? Created HazardFile: {FileName} with ID: {FileId}", 
                            file.Name, hazardFileResult.Value.Code);
                    }
                    else
                    {
                        Logger.LogError("? Failed to create HazardFile: {FileName}. Error: {Error}", 
                            file.Name, hazardFileResult.Error?.Message);
                    }
                }
                catch (Exception fileEx)
                {
                    Logger.LogError(fileEx, "? Exception creating HazardFile: {FileName}", file.Name);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "? Error processing file attachments");
        }
    }
    #endregion

    #region Helper Methods
    /// <summary>
    /// Initialize dropdown options
    /// </summary>
    private void InitializeDropdownOptions()
    {
        HazardTypeOptions = new List<DropdownOption>
        {
            new() { Value = "RWY_INCURSION", Text = "Runway Incursion" },
            new() { Value = "ACFT_DAMAGE", Text = "Aircraft Damage" },
            new() { Value = "GROUND_VEHICLE", Text = "Ground Vehicle" },
            new() { Value = "WILDLIFE_STRIKE", Text = "Wildlife Strike" },
            new() { Value = "FOD", Text = "Foreign Object Debris" },
            new() { Value = "EQUIPMENT_FAIL", Text = "Equipment Failure" },
            new() { Value = "PERSONNEL_INJURY", Text = "Personnel Injury" },
            new() { Value = "OTHER", Text = "Other" }
        };

        DepartmentOptions = new List<DropdownOption>
        {
            new() { Value = "OPERATIONS", Text = "Airport Operations" },
            new() { Value = "SAFETY", Text = "Safety Management" },
            new() { Value = "MAINTENANCE", Text = "Maintenance" },
            new() { Value = "FIRE_RESCUE", Text = "Fire & Rescue" },
            new() { Value = "SECURITY", Text = "Security" },
            new() { Value = "ATC", Text = "Air Traffic Control" },
            new() { Value = "OTHER", Text = "Other" }
        };
    }

    /// <summary>
    /// Initialize form with default values
    /// </summary>
    private void InitializeFormDefaults()
    {
        var tenMinutesAgo = DateTime.Now.AddMinutes(-10);
        HazardReportData.ReportedOn = new DateTime(tenMinutesAgo.Year, tenMinutesAgo.Month, tenMinutesAgo.Day,
            tenMinutesAgo.Hour, tenMinutesAgo.Minute, 0);

        // Set default ReportedBy from session
        try
        {
            var currentUser = SessionService.GetCurrentUserDisplayName();
            if (!string.IsNullOrEmpty(currentUser))
            {
                HazardReportData.ReportedBy = currentUser;
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Could not get current user for ReportedBy field");
        }
    }

    /// <summary>
    /// File input change handler
    /// </summary>
    /// <param name="files">Selected files</param>
    private void OnFilesChanged(IReadOnlyList<IBrowserFile> files)
    {
        SelectedFiles = files;
        ProcessAttachedFiles();
    }

    /// <summary>
    /// Process attached files for display
    /// </summary>
    private void ProcessAttachedFiles()
    {
        AttachedFiles.Clear();
        
        foreach (var file in SelectedFiles)
        {
            AttachedFiles.Add(new AttachedFile
            {
                FileName = file.Name,
                FileSizeBytes = file.Size,
                SizeDisplay = GetFileSizeDisplay(file.Size)
            });
        }

        StateHasChanged();
    }

    /// <summary>
    /// Get display text for hazard type
    /// </summary>
    /// <param name="value">Hazard type value</param>
    /// <returns>Display text</returns>
    private string GetHazardTypeDisplay(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "Unknown";
        
        var option = HazardTypeOptions.FirstOrDefault(h => h.Value == value);
        return option?.Text ?? value;
    }

    /// <summary>
    /// Get file size display string
    /// </summary>
    /// <param name="bytes">File size in bytes</param>
    /// <returns>Formatted size string</returns>
    private static string GetFileSizeDisplay(long bytes)
    {
        if (bytes < 1024) return $"{bytes} bytes";
        if (bytes < 1048576) return $"{bytes / 1024} KB";
        return $"{bytes / 1048576:F1} MB";
    }

    /// <summary>
    /// Generate preview content for dialog
    /// </summary>
    /// <returns>Preview content string</returns>
    private string GeneratePreviewContent()
    {
        return $@"Report Preview:

Hazard Type: {GetHazardTypeDisplay(HazardReportData.HazardType)}
Reported By: {HazardReportData.ReportedBy}
Reported On: {HazardReportData.ReportedOn:MM/dd/yyyy HH:mm}
Department: {(!string.IsNullOrEmpty(HazardReportData.ReportingDepartment) ? GetDepartmentDisplay(HazardReportData.ReportingDepartment) : "Not specified")}
Location: {LocationDisplayText}
Confidential: {(HazardReportData.IsConfidential ? "Yes" : "No")}

Description:
{HazardReportData.Description}

Attachments: {AttachedFiles.Count} file(s)";
    }

    /// <summary>
    /// Get display text for department
    /// </summary>
    /// <param name="value">Department value</param>
    /// <returns>Display text</returns>
    private string GetDepartmentDisplay(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "Not specified";
        
        var option = DepartmentOptions.FirstOrDefault(d => d.Value == value);
        return option?.Text ?? value;
    }

    /// <summary>
    /// Reset form to initial state
    /// </summary>
    private void ResetForm()
    {
        HazardReportData = new ReportHazardData();
        SelectedGeoLocation = new GeoLocationData();
        SelectedFiles = new List<IBrowserFile>();
        AttachedFiles.Clear();
        ShowSuccessConfirmation = false;
        ShowConfidentialDetails = false;
        GeneratedReportId = null;
        GeneratedHazardId = null;
        
        InitializeFormDefaults();
        StateHasChanged();
    }
    #endregion

    #region Submission Confirmation
    /// <summary>
    /// Show submission confirmation dialog
    /// </summary>
    public async Task ShowSubmissionConfirmationDialog()
    {
        try
        {
            Logger.LogInformation("Opening submission confirmation dialog");
            
            // Validate form first
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
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error showing submission confirmation");
            
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Error",
                Detail = "Failed to show confirmation dialog. Please try again.",
                Duration = 5000
            });
        }
    }

    /// <summary>
    /// Handle confirmed submission
    /// </summary>
    public async Task SubmitReportConfirmed()
    {
        try
        {
            Logger.LogInformation("User confirmed report submission");
            
            ShowSubmissionConfirmation = false;
            await SubmitReportAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during confirmed submission");
            
            ShowSubmissionConfirmation = false;
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
    /// Cancel submission and return to editing
    /// </summary>
    public void CancelSubmission()
    {
        Logger.LogInformation("User cancelled submission");
        
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
    #endregion

    #region Location Selection Methods
    /// <summary>
    /// Set a quick location option
    /// </summary>
    /// <param name="location">Quick location name</param>
    private void SetQuickLocation(string location)
    {
        SelectedQuickLocation = location;
        ManualLocationDescription = string.Empty; // Clear manual description when quick location is selected
        
        // Update form data
        HazardReportData.GeoLocation = location;
        
        Logger.LogInformation("Quick location selected: {Location}", location);
        StateHasChanged();
    }

    /// <summary>
    /// Show detailed location selector for advanced options
    /// </summary>
    private void ShowDetailedLocationSelector()
    {
        // This would open a more detailed location selector dialog
        // For now, just focus on the manual description
        ManualLocationDescription = string.Empty;
        SelectedQuickLocation = string.Empty;
        StateHasChanged();
        
        Logger.LogInformation("Detailed location selector opened");
    }

    /// <summary>
    /// Confirm the selected location and close the selector
    /// </summary>
    private void ConfirmLocationSelection()
    {
        if (!string.IsNullOrEmpty(ManualLocationDescription))
        {
            // Use manual description
            SelectedQuickLocation = string.Empty;
            
            // Update the form data
            HazardReportData.GeoLocation = ManualLocationDescription;
            
            Logger.LogInformation("Manual location confirmed: {Location}", ManualLocationDescription);
        }
        else if (!string.IsNullOrEmpty(SelectedQuickLocation))
        {
            // Update the form data with quick location
            HazardReportData.GeoLocation = SelectedQuickLocation;
            
            Logger.LogInformation("Quick location confirmed: {Location}", SelectedQuickLocation);
        }
        
        ShowLocationSelector = false;
        StateHasChanged();
        
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = "Location Set",
            Detail = $"Location has been set: {LocationDisplayText}",
            Duration = 3000
        });
    }
    #endregion
}

#region Data Classes
/// <summary>
/// Form data for hazard report creation
/// </summary>
public class ReportHazardData
{
    public string? HazardType { get; set; }
    public string? Description { get; set; }
    public string? ReportedBy { get; set; }
    public DateTime ReportedOn { get; set; } = DateTime.Now;
    public string? ReportingDepartment { get; set; }
    public bool IsConfidential { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string GeoLocation { get; set; } = string.Empty;
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
}

/// <summary>
/// File attachment information
/// </summary>
public class AttachedFile
{
    public string FileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string SizeDisplay { get; set; } = string.Empty;
}
#endregion