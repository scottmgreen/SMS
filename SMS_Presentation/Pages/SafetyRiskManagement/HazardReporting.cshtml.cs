using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;

using SMS_Domain.Entities;
using SMS_Domain.Interfaces;
using SMS_Domain.ValueObjects;

using SMS_Infrastructure.Common;
using SMS_Infrastructure.Services;

using SMS_Shared.Common;

namespace SMS_Presentation.Pages.SafetyRiskManagement;

/// <summary>
/// SMS Backend Hazard Reporting Page - SIMPLE Implementation
/// Basic Hazard creation with HazardLocation and HazardFiles
/// </summary>
public class HazardReportingModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<HazardReportingModel> _logger;

    public HazardReportingModel(IMediator mediator, ILogger<HazardReportingModel> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Bound Properties
    
    [BindProperty]
    public HazardReportForm HazardReport { get; set; } = new();

    [BindProperty]
    public bool ShowPreview { get; set; }

    [BindProperty]
    public bool ShowCustomLocation { get; set; }

    [BindProperty]
    public bool ShowConfidentialInfo { get; set; }

    [BindProperty]
    public bool ShowMapModal { get; set; }

    [BindProperty]
    public GeoLocationData SelectedGeoLocation { get; set; } = new();

    [BindProperty]
    public bool ShowSubmissionConfirmation { get; set; }
    
    [BindProperty]
    public bool ShowFinalSuccessConfirmation { get; set; }

    [BindProperty]
    public string? GeneratedReportId { get; set; }
    
    [BindProperty]
    public string? GeneratedHazardId { get; set; }

    [BindProperty]
    public string? GeneratedHazardReportId { get; set; }

    [BindProperty]
    public DateTime? SubmissionDateTime { get; set; }

    [BindProperty]
    public List<IFormFile> UploadedFiles { get; set; } = new();

    #endregion

    #region UI Properties

    public SelectList HazardTypeOptions { get; private set; } = null!;
    public SelectList DepartmentOptions { get; private set; } = null!;
    
    public int DescriptionCharacterCount => HazardReport?.Description?.Length ?? 0;
    public bool DescriptionNearLimit => DescriptionCharacterCount > 1800;
    public bool DescriptionAtLimit => DescriptionCharacterCount > 1950;
    public List<SelectedFile> SelectedFiles { get; private set; } = new();

    public bool IsFormValidForSubmission => 
        !string.IsNullOrEmpty(HazardReport?.HazardType) &&
        !string.IsNullOrEmpty(HazardReport?.ReportedBy) &&
        !string.IsNullOrEmpty(HazardReport?.Description);

    public bool HasGeoLocation => SelectedGeoLocation?.IsValid == true;
    public string GeoLocationDisplay => HasGeoLocation ? 
        $"Lat: {SelectedGeoLocation.Latitude:F6}, Lng: {SelectedGeoLocation.Longitude:F6}" : 
        "No coordinates selected";
    public string GeoLocationDescription => HasGeoLocation ? 
        (SelectedGeoLocation.Description ?? "Custom map location") : "";

    public double AirportCenterLatitude => 45.5898;
    public double AirportCenterLongitude => -122.5951;
    public int DefaultZoomLevel => 15;

    #endregion

    #region Page Handlers

    public void OnGet()
    {
        ViewData["Title"] = "Submit Hazard Report";
        
        var tenMinutesAgo = DateTime.Now.AddMinutes(-10);
        HazardReport.ReportedOn = new DateTime(tenMinutesAgo.Year, tenMinutesAgo.Month, tenMinutesAgo.Day, 
            tenMinutesAgo.Hour, tenMinutesAgo.Minute, 0);
        
        // Set default ReportedBy from session or provide a placeholder
        if (string.IsNullOrEmpty(HazardReport.ReportedBy))
        {
            HazardReport.ReportedBy = HttpContext.Session.GetString("SMS_UserDisplayName") ?? 
                                    HttpContext.Session.GetString("SMS_Email") ?? 
                                    ""; // Leave empty to force user to enter it
        }
        
        InitializeDropdowns();
        InitializeUIState();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        return await OnPostSubmitConfirmed();
    }

    public async Task<IActionResult> OnPostSubmitConfirmed()
    {
        InitializeDropdowns();
        
        try
        {
            // Basic validation
            if (!ModelState.IsValid)
            {
                ShowSubmissionConfirmation = true;
                InitializeUIState();
                return Page();
            }

            _logger.LogInformation("Creating hazard report for user {User}", HazardReport.ReportedBy);

            
            // Create hazard from form data
            var hazardResult = Hazard.CreateFromHazardReport(
                HazardReport.Description,
                HazardReport.HazardType,  // This should be category (the first HazardType parameter)
                HazardReport.ReportedBy,
                HazardReport.ReportingDepartment,
                HazardReport.HazardType,  // This is the actual hazardType parameter
                null, ///location 
                HazardReport.IsConfidential,
                false
            );

            if (hazardResult.IsFailure)
            {
                ModelState.AddModelError("", $"Failed to create hazard: {hazardResult.Error.Message}");
                ShowSubmissionConfirmation = true;
                InitializeUIState();
                return Page();
            }

            var hazard = hazardResult.Value;
            hazard.ReportedOn = HazardReport.ReportedOn;

            _logger.LogInformation("Hazard created: Name = '{name}', HazardType = '{type}', Category = '{category}'", 
                hazard.Name, hazard.HazardType, hazard.Category);

            // Set location from form if no geo location
            if (!HasGeoLocation && !string.IsNullOrEmpty(HazardReport.Location))
            {
                hazard.Location = HazardReport.Location;
            }

            // Process file uploads
            await ProcessFileUploads(hazard);

            // Phase 1: Create Report
            Report report = new Report(new ReportID("RP-0000"));
            report.Code = "RP-0000";
            report.CreatedBy = HazardReport.CreatedBy;
            report.Name = HazardReport.HazardType;
            report.Description = hazard.Description;
            report.Stage = "Initial";
            report.Status = "Initial";

            

            var reportResult = await _mediator.SendAsync(new CreateReportCommand(report),new CancellationToken());
            var reportCode = reportResult.Value.Code;

            hazard.ReportCode = reportCode;

            hazard.ScoringPanelCode = null;

            // Save hazard
            var createHazardCommand = new CreateHazardCommand(hazard);
            var createdHazardResult = await _mediator.SendAsync(createHazardCommand, CancellationToken.None);

            Hazard createdHazard = createdHazardResult.Value;

            var scoringPanel = new ScoringPanel(new ScoringPanelID("SP-0000"));
            scoringPanel.HazardCode = createdHazard.Code;
            var createdPanel = await _mediator.SendAsync(new CreateScoringPanelCommand(scoringPanel),new CancellationToken());

            createdHazard.ScoringPanelCode = createdPanel.Value.Code;


           var hazardLocation = new HazardLocation(new HazardLocationID("HL-0000"));

            // Create hazard location if coordinates are available
            //HazardLocation? hazardLocation = null;
            if (HasGeoLocation)
            {
                hazardLocation.Latitude = SelectedGeoLocation.Latitude;
                hazardLocation.Longitude = SelectedGeoLocation.Longitude;
                hazardLocation.Description = SelectedGeoLocation.Description;


              

                //if (locationResult.IsSuccess)
                //{
                //    hazardLocation = locationResult.Value;
                //    hazardLocation.UpdateLocationInfo(null, null, SelectedGeoLocation.Description, SelectedGeoLocation.Description);
                //}
                //var createHazardLocationCommand = new CreateHazardLocationCommand(hazardLocation);
                
            }
            var createdHazardLocation = await _mediator.SendAsync(new CreateHazardLocationCommand(hazardLocation), new CancellationToken());

            createdHazard.HazardLocation = createdHazardLocation.Value;

            var updatedHazardResult = await _mediator.SendAsync(new UpdateHazardCommand(createdHazard),new CancellationToken());




            if (updatedHazardResult.IsFailure)
            {
                ModelState.AddModelError("", $"Failed to save hazard: {updatedHazardResult.Error.Message}");
                ShowSubmissionConfirmation = true;
                InitializeUIState();
                return Page();
            }

            createdHazard = updatedHazardResult.Value;
            GeneratedHazardId = createdHazard.Code;
            GeneratedReportId = createdHazard.ReportCode;
            //GeneratedHazardReportId = createdHazard.Code;

            _logger.LogInformation("Hazard created successfully - {HazardCode}", createdHazard.Code);

            SubmissionDateTime = DateTime.Now;
            ShowSubmissionConfirmation = false;
            ShowFinalSuccessConfirmation = true;
            InitializeUIState();
            
            TempData["SuccessMessage"] = $"Hazard report submitted successfully! Hazard ID: {GeneratedHazardId}";
            
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during hazard report submission");
            ModelState.AddModelError("", "An error occurred while saving your report.");
            ShowSubmissionConfirmation = false;
            InitializeUIState();
            return Page();
        }
    }

    // Simple action handlers
    public IActionResult OnPostOpenMapSelector()
    {
        InitializeDropdowns();
        ShowMapModal = true;
        InitializeUIState();
        return Page();
    }

    public IActionResult OnPostCloseMapSelector()
    {
        InitializeDropdowns();
        ShowMapModal = false;
        InitializeUIState();
        return Page();
    }

    public async Task<IActionResult> OnPostSetManualLocation(decimal selectedLatitude, decimal selectedLongitude, string locationDescription = "")
    {
        _logger.LogInformation("OnPostSetManualLocation called with lat: {lat}, lng: {lng}, desc: {desc}", selectedLatitude, selectedLongitude, locationDescription);
        
        try
        {
            InitializeDropdowns();
            
            // Validate coordinates
            if (selectedLatitude == 0 && selectedLongitude == 0)
            {
                _logger.LogWarning("Coordinates are zero - validation failed");
                TempData["ErrorMessage"] = "Please select a location on the map first.";
                ShowMapModal = true;
                InitializeUIState();
                return Page();
            }
            
            _logger.LogInformation("Setting geo location with valid coordinates");
            
            // Set the geolocation data
            SelectedGeoLocation = new GeoLocationData
            {
                Latitude = selectedLatitude,
                Longitude = selectedLongitude,
                Description = string.IsNullOrEmpty(locationDescription) ? 
                    $"Map Location ({selectedLatitude:F6}, {selectedLongitude:F6})" : locationDescription,
                SelectedDateTime = DateTime.UtcNow
            };
            
            // Update the form location to indicate map location is selected
            HazardReport.Location = "MAP_LOCATION";
            
            ShowMapModal = false;
            InitializeUIState();
            
            _logger.LogInformation("Location set successfully, modal should close. ShowMapModal = {showModal}", ShowMapModal);
            TempData["SuccessMessage"] = $"Map location selected: {SelectedGeoLocation.Latitude:F6}, {SelectedGeoLocation.Longitude:F6}";
            
            // Add a small delay to ensure proper state
            await Task.Delay(100);
            
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnPostSetManualLocation");
            TempData["ErrorMessage"] = "An error occurred while setting the location. Please try again.";
            ShowMapModal = true;
            InitializeUIState();
            return Page();
        }
    }

    public IActionResult OnPostSelectMapLocation(double latitude, double longitude, string? description)
    {
        InitializeDropdowns();
        
        // Set the geo location data  
        SelectedGeoLocation = new GeoLocationData
        {
            Latitude = (decimal)latitude,
            Longitude = (decimal)longitude,
            Description = description ?? "",
            SelectedDateTime = DateTime.UtcNow
        };
        
        // Update the form location to indicate map location is selected
        HazardReport.Location = "MAP_LOCATION";
        
        ShowMapModal = false;
        InitializeUIState();
        
        TempData["InfoMessage"] = $"Map location selected: {GeoLocationDisplay}";
        return Page();
    }

    public IActionResult OnPostClearGeoLocation()
    {
        InitializeDropdowns();
        SelectedGeoLocation = new GeoLocationData();
        HazardReport.Location = "";
        ShowMapModal = false;
        
        var keysToRemove = ModelState.Keys.Where(k => k.Contains("Location")).ToList();
        foreach (var key in keysToRemove)
        {
            ModelState.Remove(key);
        }
        
        InitializeUIState();
        TempData["InfoMessage"] = "Location cleared.";
        return Page();
    }

    public IActionResult OnPostToggleConfidential()
    {
        InitializeDropdowns();
        ShowConfidentialInfo = HazardReport.IsConfidential;
        InitializeUIState();
        return Page();
    }

    public IActionResult OnPostShowPreview()
    {
        InitializeDropdowns();
        ShowPreview = true;
        InitializeUIState();
        return Page();
    }

    public IActionResult OnPostHidePreview()
    {
        InitializeDropdowns();
        ShowPreview = false;
        InitializeUIState();
        return Page();
    }

    public IActionResult OnPostShowSubmissionConfirmation()
    {
        InitializeDropdowns();
        
        _logger.LogInformation("OnPostShowSubmissionConfirmation: HasGeoLocation = {hasGeo}, Location = {location}", 
            HasGeoLocation, HazardReport.Location);
        
        if (HasGeoLocation)
        {
            _logger.LogInformation("GeoLocation Data: Lat = {lat}, Lng = {lng}, Desc = {desc}", 
                SelectedGeoLocation.Latitude, SelectedGeoLocation.Longitude, SelectedGeoLocation.Description);
        }
        
        // Preserve all form data when showing confirmation
        var currentLocation = HazardReport.Location;
        var currentGeoLocation = SelectedGeoLocation;
        var currentUploadedFiles = UploadedFiles;
        
        ShowSubmissionConfirmation = true;
        ShowPreview = false;
        
        // Restore preserved data
        HazardReport.Location = currentLocation;
        SelectedGeoLocation = currentGeoLocation;
        UploadedFiles = currentUploadedFiles;
        
        InitializeUIState(); // This will process the file attachments
        
        _logger.LogInformation("After preservation: HasGeoLocation = {hasGeo}, Location = {location}", 
            HasGeoLocation, HazardReport.Location);
        
        return Page();
    }

    public IActionResult OnPostCancelSubmission()
    {
        InitializeDropdowns();
        
        _logger.LogInformation("OnPostCancelSubmission: Before cancellation - HasGeoLocation = {hasGeo}, Location = {location}", 
            HasGeoLocation, HazardReport.Location);
        
        if (HasGeoLocation)
        {
            _logger.LogInformation("Cancel: GeoLocation Data: Lat = {lat}, Lng = {lng}, Desc = {desc}", 
                SelectedGeoLocation.Latitude, SelectedGeoLocation.Longitude, SelectedGeoLocation.Description);
        }
        
        ShowSubmissionConfirmation = false;
        InitializeUIState();
        
        _logger.LogInformation("OnPostCancelSubmission: After cancellation - HasGeoLocation = {hasGeo}, Location = {location}", 
            HasGeoLocation, HazardReport.Location);
        
        return Page();
    }

    public IActionResult OnPostCloseFinalConfirmation()
    {
        HazardReport = new HazardReportForm();
        ResetForm();
        return Page();
    }

    public IActionResult OnPostClearForm()
    {
        HazardReport = new HazardReportForm();
        ResetForm();
        return Page();
    }

    #endregion

    #region Helper Methods

    private void InitializeUIState()
    {
        ShowConfidentialInfo = HazardReport.IsConfidential;
        ProcessFileAttachments();
    }

    private void ProcessFileAttachments()
    {
        SelectedFiles.Clear();
        
        // Process files from UploadedFiles (IFormFile)
        if (UploadedFiles?.Any() == true)
        {
            foreach (var file in UploadedFiles.Where(f => f?.Length > 0))
            {
                SelectedFiles.Add(new SelectedFile 
                { 
                    FileName = file.FileName, 
                    FileSizeBytes = file.Length,
                    SizeDisplay = GetFileSizeDisplay(file.Length)
                });
            }
        }
        
        // Also process files from HazardReport.HazardFiles if they exist
        if (HazardReport.HazardFiles?.Any() == true)
        {
            foreach (var file in HazardReport.HazardFiles.Where(f => f?.Length > 0))
            {
                // Avoid duplicates
                if (!SelectedFiles.Any(sf => sf.FileName == file.FileName))
                {
                    SelectedFiles.Add(new SelectedFile 
                    { 
                        FileName = file.FileName, 
                        FileSizeBytes = file.Length,
                        SizeDisplay = GetFileSizeDisplay(file.Length)
                    });
                }
            }
        }
    }
    
    private static string GetFileSizeDisplay(long bytes)
    {
        if (bytes < 1024) return $"{bytes} bytes";
        if (bytes < 1048576) return $"{bytes / 1024} KB";
        return $"{bytes / 1048576:F1} MB";
    }

    private async Task ProcessFileUploads(Hazard hazard)
    {
        if (UploadedFiles?.Any() == true)
        {
            foreach (var file in UploadedFiles.Where(f => f?.Length > 0))
            {
                try
                {
                    using var memoryStream = new MemoryStream();
                    await file.CopyToAsync(memoryStream);
                    var fileData = memoryStream.ToArray();

                    var hazardFileResult = HazardFile.CreateForDatabase(
                        hazard.Code,
                        file.FileName,
                        Path.GetExtension(file.FileName).TrimStart('.'),
                        fileData,
                        "SYSTEM",
                        hazard.ReportCode
                    );

                    if (hazardFileResult.IsSuccess)
                    {
                        hazard.AddFile(hazardFileResult.Value);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to process file {FileName}", file.FileName);
                }
            }
        }
    }

    private void InitializeDropdowns()
    {
        // Simple hazard types
        HazardTypeOptions = new SelectList(new[]
        {
            new { Value = "RWY_INCURSION", Text = "Runway Incursion" },
            new { Value = "ACFT_DAMAGE", Text = "Aircraft Damage" },
            new { Value = "GROUND_VEHICLE", Text = "Ground Vehicle" },
            new { Value = "WILDLIFE_STRIKE", Text = "Wildlife Strike" },
            new { Value = "FOD", Text = "Foreign Object Debris" },
            new { Value = "EQUIPMENT_FAIL", Text = "Equipment Failure" },
            new { Value = "PERSONNEL_INJURY", Text = "Personnel Injury" },
            new { Value = "OTHER", Text = "Other" }
        }, "Value", "Text");

        // Simple departments
        DepartmentOptions = new SelectList(new[]
        {
            new { Value = "OPERATIONS", Text = "Airport Operations" },
            new { Value = "SAFETY", Text = "Safety Management" },
            new { Value = "MAINTENANCE", Text = "Maintenance" },
            new { Value = "FIRE_RESCUE", Text = "Fire & Rescue" },
            new { Value = "SECURITY", Text = "Security" },
            new { Value = "ATC", Text = "Air Traffic Control" },
            new { Value = "OTHER", Text = "Other" }
        }, "Value", "Text");
    }

    private void ResetForm()
    {
        ShowPreview = false;
        ShowCustomLocation = false;
        ShowConfidentialInfo = false;
        ShowMapModal = false;
        ShowSubmissionConfirmation = false;
        ShowFinalSuccessConfirmation = false;
        SelectedGeoLocation = new GeoLocationData();
        UploadedFiles = new();
        
        var tenMinutesAgo = DateTime.Now.AddMinutes(-10);
        HazardReport.ReportedOn = new DateTime(tenMinutesAgo.Year, tenMinutesAgo.Month, tenMinutesAgo.Day,
            tenMinutesAgo.Hour, tenMinutesAgo.Minute, 0);
        
        ModelState.Clear();
        InitializeDropdowns();
        InitializeUIState();
    }

    // Helper methods for Razor page
    public string GetSelectedHazardTypeText() 
    {
        if (string.IsNullOrEmpty(HazardReport.HazardType)) return "Not selected";
        
        // Find the matching hazard type for display
        var hazardTypeOptions = new[]
        {
            new { Value = "RWY_INCURSION", Text = "Runway Incursion" },
            new { Value = "ACFT_DAMAGE", Text = "Aircraft Damage" },
            new { Value = "GROUND_VEHICLE", Text = "Ground Vehicle" },
            new { Value = "WILDLIFE_STRIKE", Text = "Wildlife Strike" },
            new { Value = "FOD", Text = "Foreign Object Debris" },
            new { Value = "EQUIPMENT_FAIL", Text = "Equipment Failure" },
            new { Value = "PERSONNEL_INJURY", Text = "Personnel Injury" },
            new { Value = "OTHER", Text = "Other" }
        };
        
        var selected = hazardTypeOptions.FirstOrDefault(h => h.Value == HazardReport.HazardType);
        return selected?.Text ?? HazardReport.HazardType;
    }
    
    public string GetSelectedDepartmentText() 
    {
        if (string.IsNullOrEmpty(HazardReport.ReportingDepartment)) return "Not specified";
        
        // Find the matching department for display
        var departmentOptions = new[]
        {
            new { Value = "OPERATIONS", Text = "Airport Operations" },
            new { Value = "SAFETY", Text = "Safety Management" },
            new { Value = "MAINTENANCE", Text = "Maintenance" },
            new { Value = "FIRE_RESCUE", Text = "Fire & Rescue" },
            new { Value = "SECURITY", Text = "Security" },
            new { Value = "ATC", Text = "Air Traffic Control" },
            new { Value = "OTHER", Text = "Other" }
        };
        
        var selected = departmentOptions.FirstOrDefault(d => d.Value == HazardReport.ReportingDepartment);
        return selected?.Text ?? HazardReport.ReportingDepartment;
    }
    
    public string GetSelectedLocationText() 
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
        
        if (!string.IsNullOrEmpty(HazardReport.Location) && HazardReport.Location != "MAP_LOCATION")
        {
            return HazardReport.Location;
        }
        
        return "No location selected";
    }
    
    public string GetSelectedPriorityText() => "Medium Priority"; // Default for simplicity

    // Add missing properties that the UI expects
    public string UserEmailAddress => HttpContext.Session.GetString("SMS_Email") ?? "user@flypdx.com";
    public string PriorityBadgeClass => "badge bg-warning"; // Default styling
    public string PriorityDisplayText => "Medium Priority"; // Default text

    #endregion
}

#region Helper Classes

/// <summary>
/// Simple form DTO for hazard reporting - has parameterless constructor for model binding
/// </summary>
public class HazardReportForm
{
    public string? HazardType { get; set; }
    public string? Description { get; set; }
    public string? ReportedBy { get; set; }
    public DateTime ReportedOn { get; set; } = DateTime.Now;
    public DateTime CreatedDate { get; set; } = DateTime.Now; // For backward compatibility
    public string? ReportingDepartment { get; set; }
    public string? Location { get; set; }
    public bool IsConfidential { get; set; }
    public string? CreatedBy { get; set; }
    public string Priority { get; set; } = "MEDIUM"; // Default priority
    public List<IFormFile> HazardFiles { get; set; } = new(); // For file uploads
}

public class GeoLocationData
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime SelectedDateTime { get; set; }
    public bool IsValid => Latitude != 0 && Longitude != 0;
}

public class SelectedFile
{
    public string FileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string SizeDisplay { get; set; } = string.Empty;
}

#endregion