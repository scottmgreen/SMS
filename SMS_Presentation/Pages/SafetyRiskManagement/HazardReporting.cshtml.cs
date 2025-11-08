using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PDXSMS.Services;

namespace PDXSMS_Presentation.Pages.SafetyRiskManagement;

/// <summary>
/// FR-1.1.1: Submit Hazard Report
/// Allows users to submit hazard reports with document attachments
/// Enhanced with dynamic dropdown population, pure C# Razor syntax, and geolocation mapping
/// 
/// UPDATED: Now uses UniversalJsonDataService with SMSIdGenerationService for human-readable IDs
/// ENHANCED: Added map thumbnail generation for location visualization
/// </summary>
public class HazardReportingModel : PageModel
{
    private readonly UniversalJsonDataService _dataService;
    private readonly SMSIdGenerationService _idGenerator;
    private readonly MapThumbnailService _mapThumbnailService;

    public HazardReportingModel(
        UniversalJsonDataService dataService, 
        SMSIdGenerationService idGenerator,
        MapThumbnailService mapThumbnailService)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
        _mapThumbnailService = mapThumbnailService ?? throw new ArgumentNullException(nameof(mapThumbnailService));
    }

    [BindProperty]
    public HazardReportViewModel HazardReport { get; set; } = new();

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

    // Submission Confirmation Workflow Properties
    [BindProperty]
    public bool ShowSubmissionConfirmation { get; set; }
    
    [BindProperty]
    public bool ShowFinalSuccessConfirmation { get; set; }

    [BindProperty]
    public string? GeneratedReportId { get; set; }
    
    [BindProperty]
    public string? GeneratedHazardReportId { get; set; }

    [BindProperty]
    public string? UserEmailAddress { get; set; }
    
    [BindProperty]
    public DateTime? SubmissionDateTime { get; set; }

    // Dynamic dropdown data properties
    public SelectList HazardTypeOptions { get; private set; } = null!;
    public SelectList PriorityOptions { get; private set; } = null!;
    public SelectList LocationOptions { get; private set; } = null!;
    public SelectList DepartmentOptions { get; private set; } = null!;

    // UI State Properties
    public string PriorityBadgeClass => GetPriorityBadgeClass();
    public string PriorityDisplayText => GetPriorityDisplayText();
    public int DescriptionCharacterCount => HazardReport.Description?.Length ?? 0;
    public bool DescriptionNearLimit => DescriptionCharacterCount > 1800;
    public bool DescriptionAtLimit => DescriptionCharacterCount > 1950;
    public List<FileInfo> SelectedFiles { get; private set; } = new();

    // Form validation property - determines if the form is ready for submission
    public bool IsFormValidForSubmission => 
        !string.IsNullOrEmpty(HazardReport.HazardType) &&
        !string.IsNullOrEmpty(HazardReport.ReportedBy) &&
        !string.IsNullOrEmpty(HazardReport.Description) &&
        (HasGeoLocation || !string.IsNullOrEmpty(HazardReport.Location)) &&
        HazardReport.ReportedOn <= DateTime.Now &&
        HazardReport.ReportedOn >= DateTime.Now.AddDays(-30);

    // Geolocation Properties
    public bool HasGeoLocation => SelectedGeoLocation.IsValid;
    public string GeoLocationDisplay => HasGeoLocation ? 
        $"Lat: {SelectedGeoLocation.Latitude:F6}, Lng: {SelectedGeoLocation.Longitude:F6}" : 
        "No coordinates selected";
    public string GeoLocationDescription => HasGeoLocation ? 
        (!string.IsNullOrEmpty(SelectedGeoLocation.Description) ? SelectedGeoLocation.Description : "Custom map location") : 
        "";

    // PDX Airport Boundaries for map centering
    public double AirportCenterLatitude => 45.5898; // PDX Airport coordinates
    public double AirportCenterLongitude => -122.5951;
    public int DefaultZoomLevel => 15; // Appropriate for airport-wide view

    // Private lists for dropdown data
    private readonly List<HazardTypeOption> _hazardTypes = new()
    {
        new("RWY_INCURSION", "Runway Incursion", "Aircraft or vehicle unauthorized entry onto runway"),
        new("RWY_EXCURSION", "Runway Excursion", "Aircraft departure from runway surface"),
        new("ACFT_DAMAGE", "Aircraft Damage", "Physical damage to aircraft structures or systems"),
        new("GROUND_VEHICLE", "Ground Vehicle", "Ground support equipment or vehicle incidents"),
        new("WILDLIFE_STRIKE", "Wildlife Strike", "Aircraft collision with birds or wildlife"),
        new("FOD", "Foreign Object Debris", "FOD on runways, taxiways, or ramps"),
        new("WEATHER", "Weather Related", "Weather-related operational hazards"),
        new("EQUIPMENT_FAIL", "Equipment Failure", "Failure of ground or aircraft equipment"),
        new("PERSONNEL_INJURY", "Personnel Injury", "Injury to airport or airline personnel"),
        new("SECURITY_BREACH", "Security Breach", "Unauthorized access or security violations"),
        new("FUEL_SPILL", "Fuel Spill", "Aviation fuel or hazardous material spills"),
        new("FIRE_EMERGENCY", "Fire/Emergency", "Fire or emergency response situations"),
        new("COMMUNICATION", "Communication Failure", "Radio or communication system failures"),
        new("NAVIGATION", "Navigation System", "Navigation aid or system malfunctions"),
        new("PASSENGER", "Passenger Incident", "Passenger-related safety incidents"),
        new("CARGO", "Cargo/Baggage", "Cargo or baggage handling incidents"),
        new("MAINTENANCE", "Maintenance Related", "Maintenance operation hazards"),
        new("PROCEDURE", "Procedural Deviation", "Deviation from established procedures"),
        new("CONSTRUCTION", "Construction Activity", "Construction-related safety hazards"),
        new("OTHER", "Other", "Other safety hazards not listed above")
    };

    private readonly List<PriorityOption> _priorities = new()
    {
        new("LOW", "Low", "Minor safety concern with minimal impact", "text-success"),
        new("MEDIUM", "Medium", "Moderate safety impact requiring attention", "text-warning"),
        new("HIGH", "High", "Significant safety risk requiring immediate action", "text-danger"),
        new("CRITICAL", "Critical", "Imminent safety threat requiring emergency response", "text-danger fw-bold")
    };

    private readonly List<LocationOption> _locations = new()
    {
        // Runways
        new("RWY_10L_28R", "Runway 10L/28R", "Main runway - 11,000 ft"),
        new("RWY_10R_28L", "Runway 10R/28L", "Parallel runway - 8,000 ft"),
        new("RWY_03_21", "Runway 03/21", "Crosswind runway - 6,000 ft"),
        
        // Taxiways
        new("TWY_A", "Taxiway Alpha", "Main parallel taxiway"),
        new("TWY_B", "Taxiway Bravo", "Terminal connector taxiway"),
        new("TWY_C", "Taxiway Charlie", "Cargo area connector"),
        new("TWY_D", "Taxiway Delta", "General aviation connector"),
        new("TWY_E", "Taxiway Echo", "Maintenance area connector"),
        
        // Terminal Areas
        new("TERM_MAIN", "Main Terminal", "Primary passenger terminal"),
        new("CONC_A", "Concourse A", "Domestic gates A1-A20"),
        new("CONC_B", "Concourse B", "Domestic gates B1-B25"),
        new("CONC_C", "Concourse C", "International gates C1-C15"),
        new("CONC_D", "Concourse D", "Regional gates D1-D10"),
        new("CONC_E", "Concourse E", "International gates E1-E12"),
        
        // Gates (sample - can be expanded)
        new("GATE_A1", "Gate A1", "Domestic gate - Concourse A"),
        new("GATE_A5", "Gate A5", "Domestic gate - Concourse A"),
        new("GATE_B10", "Gate B10", "Domestic gate - Concourse B"),
        new("GATE_C3", "Gate C3", "International gate - Concourse C"),
        
        // Ramp Areas
        new("RAMP_MAIN", "Main Ramp", "Primary aircraft parking ramp"),
        new("RAMP_CARGO", "Cargo Ramp", "Freight and cargo operations"),
        new("RAMP_GA", "General Aviation Ramp", "General aviation parking"),
        new("RAMP_MAINT", "Maintenance Ramp", "Aircraft maintenance area"),
        new("RAMP_DEICE", "Deicing Pad", "Aircraft deicing operations"),
        
        // Support Areas
        new("ATC_TOWER", "Control Tower", "Air traffic control tower"),
        new("FIRE_STATION", "Fire Station", "Aircraft rescue and firefighting"),
        new("FUEL_FARM", "Fuel Farm", "Aviation fuel storage facility"),
        new("GSE_YARD", "GSE Yard", "Ground support equipment storage"),
        new("MAINT_HANGAR", "Maintenance Hangar", "Aircraft maintenance facility"),
        new("CARGO_BUILDING", "Cargo Building", "Freight processing facility"),
        
        // Perimeter & Security
        new("PERIMETER", "Airport Perimeter", "Security fence and perimeter areas"),
        new("SECURITY_CHECKPOINT", "Security Checkpoint", "Passenger security screening"),
        new("AOA_ENTRY", "AOA Entry Point", "Airfield access control point"),
        
        // Utilities & Infrastructure
        new("ELECTRICAL", "Electrical Systems", "Power distribution and lighting"),
        new("WATER_SEWER", "Water/Sewer", "Water and sewage systems"),
        new("COMM_EQUIPMENT", "Communication Equipment", "Radio and navigation aids"),
        
        // Other
        new("PARKING_PUBLIC", "Public Parking", "Passenger vehicle parking areas"),
        new("ROADWAY", "Airport Roadway", "Internal airport roadways"),
        new("CONSTRUCTION_ZONE", "Construction Zone", "Active construction areas"),
        new("OTHER", "Other Location", "Location not listed above"),
        new("MAP_LOCATION", "Select on Map", "Use interactive map to select precise location")
    };

    private readonly List<DepartmentOption> _departments = new()
    {
        new("OPERATIONS", "Airport Operations", "Airfield and terminal operations"),
        new("SAFETY", "Safety Management", "SMS and safety oversight"),
        new("MAINTENANCE", "Maintenance & Engineering", "Facility and equipment maintenance"),
        new("FIRE_RESCUE", "Aircraft Rescue & Firefighting", "Emergency response services"),
        new("SECURITY", "Airport Security", "Security operations and access control"),
        new("ATC", "Air Traffic Control", "Tower and approach control"),
        new("GROUND_SERVICES", "Ground Handling Services", "Aircraft servicing and support"),
        new("CARGO", "Cargo Operations", "Freight and cargo handling"),
        new("FUEL_SERVICES", "Fuel Services", "Aircraft fueling operations"),
        new("FACILITIES", "Facilities Management", "Building and infrastructure management"),
        new("IT_TELECOM", "IT & Telecommunications", "Information systems and communications"),
        new("ENVIRONMENTAL", "Environmental Services", "Environmental compliance and cleanup"),
        new("CONSTRUCTION", "Construction Management", "Capital projects and construction"),
        new("AIRLINE_OPS", "Airline Operations", "Airline operational staff"),
        new("TSA", "Transportation Security Administration", "Federal security screening"),
        new("CBP", "Customs & Border Protection", "Federal customs and immigration"),
        new("CONCESSIONS", "Concessions & Retail", "Terminal retail and food service"),
        new("PARKING", "Parking Services", "Vehicle parking operations"),
        new("JANITORIAL", "Janitorial Services", "Cleaning and waste management"),
        new("ADMIN", "Administration", "Executive and administrative staff"),
        new("OTHER", "Other Department", "Department not listed above")
    };

    public void OnGet()
    {
        ViewData["Title"] = "FR-1.1.1: Submit Hazard Report";
        
        // Clear any stale TempData messages
        if (Request.Query.ContainsKey("clearMessages"))
        {
            TempData.Remove("SuccessMessage");
            TempData.Remove("InfoMessage");  
            TempData.Remove("ErrorMessage");
            TempData.Remove("HazardReportId");
        }
        
        // Initialize ReportedOn with current time minus 10 minutes, rounded to the nearest minute (no seconds/milliseconds)
        var tenMinutesAgo = DateTime.Now.AddMinutes(-10);
        HazardReport.ReportedOn = new DateTime(tenMinutesAgo.Year, tenMinutesAgo.Month, tenMinutesAgo.Day, tenMinutesAgo.Hour, tenMinutesAgo.Minute, 0);
        
        InitializeDropdowns();
        InitializeUIState();
        
        // Check if we should open the map modal based on query parameters
        if (Request.Query.ContainsKey("openMap"))
        {
            ShowMapModal = true;
            HazardReport.Location = "MAP_LOCATION";
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // This method is now primarily for fallback/direct submission scenarios
        // Normal workflow should go through OnPostShowSubmissionConfirmation -> OnPostSubmitConfirmed
        
        return await OnPostSubmitConfirmed();
    }

    public async Task<IActionResult> OnPostSubmitConfirmed()
    {
        // This method is called when user clicks "Yes, Submit Report" from the confirmation modal
        // It should perform the actual submission and show success modal
        
        InitializeDropdowns(); // Always reinitialize dropdowns for postback
        
        // Perform the same validation as OnPostAsync
        if (!ModelState.IsValid || (!HasGeoLocation && string.IsNullOrEmpty(HazardReport.Location)) || 
            string.IsNullOrEmpty(HazardReport.ReportedBy) || HazardReport.ReportedOn > DateTime.Now)
        {
            if (!HasGeoLocation && string.IsNullOrEmpty(HazardReport.Location))
            {
                ModelState.AddModelError("HazardReport.Location", "Please select a location using the map.");
            }
            
            if (string.IsNullOrEmpty(HazardReport.ReportedBy))
            {
                ModelState.AddModelError("HazardReport.ReportedBy", "Please specify who is reporting this hazard.");
            }
            
            if (HazardReport.ReportedOn > DateTime.Now)
            {
                ModelState.AddModelError("HazardReport.ReportedOn", "Reported date and time cannot be in the future.");
            }
            
            if (HazardReport.ReportedOn < DateTime.Now.AddDays(-30))
            {
                ModelState.AddModelError("HazardReport.ReportedOn", "Reported date cannot be more than 30 days in the past. Please contact SMS administration for older reports.");
            }
            
            // Return to confirmation modal with errors
            ShowSubmissionConfirmation = true;
            InitializeUIState();
            return Page();
        }

        try
        {
            // Use UniversalJsonDataService with human-readable IDs
            var hazardData = new Hazard
            {
                // Let the UniversalJsonDataService generate the human-readable Hazard ID
                Id = "", // Will be generated as HZ-xxxx format
                HazardType = HazardReport.HazardType,
                Location = HasGeoLocation ? 
                    $"{SelectedGeoLocation.Latitude:F6}, {SelectedGeoLocation.Longitude:F6}" + 
                    (string.IsNullOrEmpty(SelectedGeoLocation.Description) ? "" : $" - {SelectedGeoLocation.Description}") 
                    : HazardReport.Location,
                Description = HazardReport.Description,
                Priority = HazardReport.Priority,
                Status = "Under Review", // Set initial status
                ReportedById = HazardReport.ReportedBy,
                ReportedDate = HazardReport.ReportedOn,
                IsConfidential = HazardReport.IsConfidential,
                CreatedDate = DateTime.UtcNow
            };

            // Save the hazard to JSON storage - this will generate the HZ-xxxx ID
            _dataService.SaveHazard(hazardData);
            
            // Create and save the associated report
            var reportData = new ReportData
            {
                // Let the UniversalJsonDataService generate the human-readable Report ID
                Id = "", // Will be generated as SMS-yyyy-xxxx format
                HazardId = hazardData.Id, // Use the generated hazard ID
                SubmittedById = HazardReport.ReportedBy,
                SubmittedDate = HazardReport.ReportedOn,
                Status = "Submitted", // Set report status
                TrackingId = "", // Will be set to the same as the Report ID
                ValidatedById = null,
                ValidationNotes = null,
                CreatedDate = DateTime.UtcNow
            };

            // Save the report to JSON storage - this will generate the SMS-yyyy-xxxx ID
            _dataService.SaveReport(reportData);
            
            // Generate map thumbnail if geolocation data is available
            string? mapThumbnailFilename = null;
            if (HasGeoLocation)
            {
                try 
                {
                    Console.WriteLine($"*** MAP THUMBNAIL: Generating thumbnail for report {reportData.TrackingId} at {SelectedGeoLocation.Latitude:F6}, {SelectedGeoLocation.Longitude:F6}");
                    
                    mapThumbnailFilename = await _mapThumbnailService.GenerateMapThumbnailAsync(
                        reportData.TrackingId ?? reportData.Id,
                        hazardData.Id, // Pass the hazard ID
                        SelectedGeoLocation.Latitude,
                        SelectedGeoLocation.Longitude,
                        SelectedGeoLocation.Description
                    );
                    
                    Console.WriteLine($"*** MAP THUMBNAIL: Successfully generated thumbnail: {mapThumbnailFilename}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"*** MAP THUMBNAIL: Failed to generate thumbnail for report {reportData.TrackingId}: {ex.Message}");
                    // Don't fail the entire submission if thumbnail generation fails
                }
            }
            
            // Use the tracking ID from the report (which is the same as the report ID)
            GeneratedReportId = reportData.TrackingId;
            
            Console.WriteLine($"*** HAZARD REPORTING: Successfully saved hazard {hazardData.Id} with tracking ID {reportData.TrackingId}");
            if (!string.IsNullOrEmpty(mapThumbnailFilename))
            {
                Console.WriteLine($"*** HAZARD REPORTING: Map thumbnail saved as {mapThumbnailFilename}");
            }
            
            // Show success modal
            SubmissionDateTime = DateTime.Now;
            ShowSubmissionConfirmation = false;
            ShowFinalSuccessConfirmation = true;
            InitializeUIState();
            
            // Set success message
            var successMessage = $"Hazard report submitted successfully with tracking ID: {GeneratedReportId}";
            if (!string.IsNullOrEmpty(mapThumbnailFilename))
            {
                successMessage += $". Location map saved as: {mapThumbnailFilename}";
            }
            TempData["SuccessMessage"] = successMessage;
            
            return Page();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"*** HAZARD REPORTING: Error saving hazard report: {ex.Message}");
            
            // Show error but still allow user to see the form
            TempData["ErrorMessage"] = $"Error saving hazard report: {ex.Message}";
            ModelState.AddModelError("", "An error occurred while saving your report. Please try again.");
            
            ShowSubmissionConfirmation = false;
            InitializeUIState();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostSaveDraftAsync()
    {
        InitializeDropdowns();
        
        try
        {
            // Use UniversalJsonDataService with human-readable IDs for drafts
            var hazardData = new HazardData
            {
                // Let the UniversalJsonDataService generate the human-readable Hazard ID
                Id = "", // Will be generated as HZ-xxxx format
                HazardType = HazardReport.HazardType,
                Location = HasGeoLocation ? 
                    $"{SelectedGeoLocation.Latitude:F6}, {SelectedGeoLocation.Longitude:F6}" + 
                    (string.IsNullOrEmpty(SelectedGeoLocation.Description) ? "" : $" - {SelectedGeoLocation.Description}") 
                    : HazardReport.Location,
                Description = HazardReport.Description,
                Priority = HazardReport.Priority,
                Status = "Draft", // Set as draft status
                ReportedById = User.Identity?.Name ?? "SYSTEM",
                ReportedDate = DateTime.UtcNow,
                IsConfidential = HazardReport.IsConfidential,
                CreatedDate = DateTime.UtcNow
            };

            // Save the hazard to JSON storage - this will generate the HZ-xxxx ID
            _dataService.SaveHazard(hazardData);
            
            // Create and save the associated report as draft
            var reportData = new ReportData
            {
                // Let the UniversalJsonDataService generate the human-readable Report ID
                Id = "", // Will be generated as SMS-yyyy-xxxx format
                HazardId = hazardData.Id, // Use the generated hazard ID
                SubmittedById = User.Identity?.Name ?? "SYSTEM",
                SubmittedDate = DateTime.UtcNow,
                Status = "Draft", // Set report status as draft
                TrackingId = "", // Will be set to the same as the Report ID
                ValidatedById = null,
                ValidationNotes = "Saved as draft",
                CreatedDate = DateTime.UtcNow
            };

            // Save the draft report to JSON storage - this will generate the SMS-yyyy-xxxx ID
            _dataService.SaveReport(reportData);
            
            // Generate map thumbnail for draft if geolocation data is available
            string? mapThumbnailFilename = null;
            if (HasGeoLocation)
            {
                try 
                {
                    Console.WriteLine($"*** MAP THUMBNAIL: Generating thumbnail for draft {reportData.TrackingId} at {SelectedGeoLocation.Latitude:F6}, {SelectedGeoLocation.Longitude:F6}");
                    
                    mapThumbnailFilename = await _mapThumbnailService.GenerateMapThumbnailAsync(
                        reportData.TrackingId ?? reportData.Id,
                        hazardData.Id, // Pass the hazard ID
                        SelectedGeoLocation.Latitude,
                        SelectedGeoLocation.Longitude,
                        SelectedGeoLocation.Description
                    );
                    
                    Console.WriteLine($"*** MAP THUMBNAIL: Successfully generated draft thumbnail: {mapThumbnailFilename}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"*** MAP THUMBNAIL: Failed to generate draft thumbnail: {ex.Message}");
                    // Don't fail the draft save if thumbnail generation fails
                }
            }
            
            Console.WriteLine($"*** HAZARD REPORTING: Successfully saved draft hazard {hazardData.Id} with tracking ID {reportData.TrackingId}");
            
            var infoMessage = $"Hazard report saved as draft. Draft ID: {reportData.TrackingId}";
            if (!string.IsNullOrEmpty(mapThumbnailFilename))
            {
                infoMessage += $". Location map saved as: {mapThumbnailFilename}";
            }
            TempData["InfoMessage"] = infoMessage;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"*** HAZARD REPORTING: Error saving draft: {ex.Message}");
            TempData["ErrorMessage"] = "Failed to save draft. Please try again.";
        }
        
        InitializeUIState();
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateLocation()
    {
        // SIMPLIFIED: No longer needed since we only use map selection
        // This method is kept for compatibility but redirects to map selector
        return OnPostOpenMapSelector();
    }

    public IActionResult OnPostOpenMapSelector()
    {
        // DON'T clear model state - preserve form data
        // ModelState.Clear(); // Removed this line to preserve validation state
        
        InitializeDropdowns();
        ShowMapModal = true;
        
        // Always set to MAP_LOCATION since that's our only option now
        HazardReport.Location = "MAP_LOCATION";
        
        InitializeUIState();
        
        return Page();
    }

    public IActionResult OnPostCloseMapSelector()
    {
        // DON'T clear model state - preserve form data
        // ModelState.Clear(); // Removed this line to preserve validation state
        
        InitializeDropdowns();
        ShowMapModal = false;
        
        // If no geo location was selected, revert to empty selection
        if (!HasGeoLocation)
        {
            HazardReport.Location = "";
        }
        
        InitializeUIState();
        return Page();
    }

    public IActionResult OnPostSelectMapLocation(double latitude, double longitude, string? description)
    {
        // DON'T clear model state - preserve form data
        // ModelState.Clear(); // Removed this line to preserve validation state
        
        InitializeDropdowns();
        
        SelectedGeoLocation = new GeoLocationData
        {
            Latitude = latitude,
            Longitude = longitude,
            Description = description ?? "",
            SelectedDateTime = DateTime.UtcNow
        };
        
        ShowMapModal = false;
        HazardReport.Location = "MAP_LOCATION"; // Keep the map location option selected
        
        InitializeUIState();
        TempData["InfoMessage"] = $"Map location selected: {GeoLocationDisplay}";
        return Page();
    }

    public IActionResult OnPostClearGeoLocation()
    {
        // DON'T clear model state - preserve form data except for location-related fields
        InitializeDropdowns();
        SelectedGeoLocation = new GeoLocationData();
        HazardReport.Location = "";
        ShowMapModal = false;
        
        // Only clear location-related model state errors
        var keysToRemove = ModelState.Keys.Where(k => k.Contains("Location")).ToList();
        foreach (var key in keysToRemove)
        {
            ModelState.Remove(key);
        }
        
        InitializeUIState();
        TempData["InfoMessage"] = "Geo location cleared.";
        return Page();
    }

    public IActionResult OnPostUpdatePriority()
    {
        // DON'T clear model state - preserve form data
        InitializeDropdowns();
        InitializeUIState();
        return Page();
    }

    public IActionResult OnPostToggleConfidential()
    {
        // DON'T clear model state - preserve form data
        InitializeDropdowns();
        ShowConfidentialInfo = HazardReport.IsConfidential;
        InitializeUIState();
        return Page();
    }

    public IActionResult OnPostShowPreview()
    {
        // DON'T clear model state - preserve form data and validation
        // This allows users to see validation errors in the preview
        InitializeDropdowns();
        ShowPreview = true;
        InitializeUIState();
        return Page();
    }

    public IActionResult OnPostHidePreview()
    {
        // DON'T clear model state - preserve form data and validation
        InitializeDropdowns();
        ShowPreview = false;
        InitializeUIState();
        return Page();
    }

    public IActionResult OnPostShowSubmissionConfirmation()
    {
        // ALWAYS show submission confirmation - don't validate here, validate during actual submission
        InitializeDropdowns();
        ShowSubmissionConfirmation = true;
        ShowPreview = false; // Hide preview if it was showing
        InitializeUIState();
        return Page();
    }

    public IActionResult OnPostCancelSubmission()
    {
        // Don't clear model state - preserve form data
        InitializeDropdowns();
        ShowSubmissionConfirmation = false;
        InitializeUIState();
        return Page();
    }

    public IActionResult OnPostCloseFinalConfirmation()
    {
        // Clear the form completely after successful submission acknowledgment
        HazardReport = new HazardReportViewModel();
        ShowPreview = false;
        ShowCustomLocation = false;
        ShowConfidentialInfo = false;
        ShowMapModal = false;
        ShowSubmissionConfirmation = false;
        ShowFinalSuccessConfirmation = false;
        SelectedGeoLocation = new GeoLocationData();
        
        // Clear generated IDs and submission data
        GeneratedReportId = null;
        GeneratedHazardReportId = null;
        UserEmailAddress = null;
        SubmissionDateTime = null;
        
        // Initialize ReportedOn with current time minus 10 minutes, rounded to the nearest minute
        var tenMinutesAgo = DateTime.Now.AddMinutes(-10);
        HazardReport.ReportedOn = new DateTime(tenMinutesAgo.Year, tenMinutesAgo.Month, tenMinutesAgo.Day, tenMinutesAgo.Hour, tenMinutesAgo.Minute, 0);
        
        // Clear any validation errors
        ModelState.Clear();
        
        InitializeDropdowns();
        InitializeUIState();
        
        TempData["InfoMessage"] = "Thank you! Your hazard report has been submitted successfully. The form has been cleared for a new report.";
        return Page();
    }

    public IActionResult OnPostClearForm()
    {
        HazardReport = new HazardReportViewModel();
        ShowPreview = false;
        ShowCustomLocation = false;
        ShowConfidentialInfo = false;
        ShowMapModal = false;
        ShowSubmissionConfirmation = false;
        ShowFinalSuccessConfirmation = false;
        SelectedGeoLocation = new GeoLocationData();
        
        // Initialize ReportedOn with current time minus 10 minutes, rounded to the nearest minute
        var tenMinutesAgo = DateTime.Now.AddMinutes(-10);
        HazardReport.ReportedOn = new DateTime(tenMinutesAgo.Year, tenMinutesAgo.Month, tenMinutesAgo.Day, tenMinutesAgo.Hour, tenMinutesAgo.Minute, 0);
        
        InitializeDropdowns();
        InitializeUIState();
        TempData["InfoMessage"] = "Form has been cleared.";
        return Page();
    }

    private void InitializeUIState()
    {
        // SIMPLIFIED: No longer need to check for "OTHER" since we only use map
        ShowCustomLocation = false; // Always false now since we only use map selection
        ShowConfidentialInfo = HazardReport.IsConfidential;
        ProcessFileAttachments();
        
        // Ensure user email is available for confirmation
        if (ShowSubmissionConfirmation && string.IsNullOrEmpty(UserEmailAddress))
        {
            UserEmailAddress = HttpContext.Session.GetString("UserEmail") ?? 
                              User.Identity?.Name ?? 
                              "user@airport.com";
        }
        
        // Ensure location is set to MAP_LOCATION if we have geolocation data
        if (HasGeoLocation && string.IsNullOrEmpty(HazardReport.Location))
        {
            HazardReport.Location = "MAP_LOCATION";
        }
    }

    private void ProcessFileAttachments()
    {
        SelectedFiles.Clear();
        if (HazardReport.Attachments?.Any() == true)
        {
            foreach (var file in HazardReport.Attachments)
            {
                if (file?.Length > 0)
                {
                    SelectedFiles.Add(new FileInfo 
                    { 
                        Name = file.FileName, 
                        Size = file.Length,
                        SizeDisplay = GetFileSizeDisplay(file.Length)
                    });
                }
            }
        }
    }

    private void InitializeDropdowns()
    {
        // Initialize Hazard Types with grouped options
        var hazardTypeGroups = new List<SelectListGroup>
        {
            new() { Name = "Aircraft Operations" },
            new() { Name = "Ground Operations" },
            new() { Name = "Security & Emergency" },
            new() { Name = "Infrastructure & Systems" },
            new() { Name = "Personnel & Procedures" }
        };

        var hazardTypeItems = new List<SelectListItem>
        {
            new("Select hazard type...", "", false, true), // Default option
        };

        // Group hazard types for better organization
        var aircraftOps = _hazardTypes.Where(h => new[] { "RWY_INCURSION", "RWY_EXCURSION", "ACFT_DAMAGE", "WILDLIFE_STRIKE", "NAVIGATION" }.Contains(h.Value));
        var groundOps = _hazardTypes.Where(h => new[] { "GROUND_VEHICLE", "FOD", "CARGO", "MAINTENANCE", "FUEL_SPILL" }.Contains(h.Value));
        var security = _hazardTypes.Where(h => new[] { "SECURITY_BREACH", "FIRE_EMERGENCY", "PERSONNEL_INJURY", "PASSENGER" }.Contains(h.Value));
        var infrastructure = _hazardTypes.Where(h => new[] { "EQUIPMENT_FAIL", "COMMUNICATION", "WEATHER" }.Contains(h.Value));
        var procedures = _hazardTypes.Where(h => new[] { "PROCEDURE", "CONSTRUCTION", "OTHER" }.Contains(h.Value));

        AddGroupedItems(hazardTypeItems, aircraftOps, hazardTypeGroups[0]);
        AddGroupedItems(hazardTypeItems, groundOps, hazardTypeGroups[1]);
        AddGroupedItems(hazardTypeItems, security, hazardTypeGroups[2]);
        AddGroupedItems(hazardTypeItems, infrastructure, hazardTypeGroups[3]);
        AddGroupedItems(hazardTypeItems, procedures, hazardTypeGroups[4]);

        HazardTypeOptions = new SelectList(hazardTypeItems, "Value", "Text", "Group");

        // Initialize Priority Options
        PriorityOptions = new SelectList(_priorities.Select(p => new SelectListItem
        {
            Value = p.Value,
            Text = $"{p.Text} - {p.Description}",
            Selected = p.Value == "MEDIUM" // Default to Medium
        }), "Value", "Text", "MEDIUM");

        // Initialize Location Options with categories
        var locationGroups = new List<SelectListGroup>
        {
            new() { Name = "Runways & Taxiways" },
            new() { Name = "Terminal & Gates" },
            new() { Name = "Ramp Areas" },
            new() { Name = "Support Facilities" },
            new() { Name = "Other Areas" },
            new() { Name = "Custom Selection" }
        };

        var locationItems = new List<SelectListItem>
        {
            new("Select location or use map...", "", false, true),
        };

        var runwayTaxiway = _locations.Where(l => l.Value.StartsWith("RWY_") || l.Value.StartsWith("TWY_"));
        var terminal = _locations.Where(l => l.Value.StartsWith("TERM_") || l.Value.StartsWith("CONC_") || l.Value.StartsWith("GATE_"));
        var ramp = _locations.Where(l => l.Value.StartsWith("RAMP_"));
        var support = _locations.Where(l => new[] { "ATC_TOWER", "FIRE_STATION", "FUEL_FARM", "GSE_YARD", "MAINT_HANGAR", "CARGO_BUILDING" }.Contains(l.Value));
        var otherAreas = _locations.Where(l => new[] { "PERIMETER", "SECURITY_CHECKPOINT", "AOA_ENTRY", "ELECTRICAL", "WATER_SEWER", "COMM_EQUIPMENT", "PARKING_PUBLIC", "ROADWAY", "CONSTRUCTION_ZONE", "OTHER" }.Contains(l.Value));
        var customSelection = _locations.Where(l => l.Value == "MAP_LOCATION");

        AddGroupedLocationItems(locationItems, runwayTaxiway, locationGroups[0]);
        AddGroupedLocationItems(locationItems, terminal, locationGroups[1]);
        AddGroupedLocationItems(locationItems, ramp, locationGroups[2]);
        AddGroupedLocationItems(locationItems, support, locationGroups[3]);
        AddGroupedLocationItems(locationItems, otherAreas, locationGroups[4]);
        AddGroupedLocationItems(locationItems, customSelection, locationGroups[5]);

        LocationOptions = new SelectList(locationItems, "Value", "Text", "Group");

        // Initialize Department Options
        DepartmentOptions = new SelectList(_departments.Select(d => new SelectListItem
        {
            Value = d.Value,
            Text = $"{d.Text} - {d.Description}"
        }), "Value", "Text");
    }

    private string GetPriorityBadgeClass()
    {
        return HazardReport.Priority switch
        {
            "LOW" => "badge bg-success",
            "MEDIUM" => "badge bg-warning",
            "HIGH" => "badge bg-danger",
            "CRITICAL" => "badge bg-danger",
            _ => "badge bg-secondary"
        };
    }

    private string GetPriorityDisplayText()
    {
        return HazardReport.Priority switch
        {
            "LOW" => "Low Priority",
            "MEDIUM" => "Medium Priority", 
            "HIGH" => "High Priority",
            "CRITICAL" => "Critical Priority",
            _ => "Select Priority"
        };
    }

    private static string GetFileSizeDisplay(long bytes)
    {
        if (bytes < 1024) return $"{bytes} bytes";
        if (bytes < 1048576) return $"{bytes / 1024} KB";
        return $"{bytes / 1048576:F1} MB";
    }

    private static void AddGroupedItems(List<SelectListItem> items, IEnumerable<HazardTypeOption> options, SelectListGroup group)
    {
        foreach (var option in options)
        {
            items.Add(new SelectListItem
            {
                Value = option.Value,
                Text = $"{option.Text} - {option.Description}",
                Group = group
            });
        }
    }

    private static void AddGroupedLocationItems(List<SelectListItem> items, IEnumerable<LocationOption> options, SelectListGroup group)
    {
        foreach (var option in options)
        {
            items.Add(new SelectListItem
            {
                Value = option.Value,
                Text = $"{option.Text} - {option.Description}",
                Group = group
            });
        }
    }

    public string GetSelectedHazardTypeText()
    {
        var selectedType = _hazardTypes.FirstOrDefault(h => h.Value == HazardReport.HazardType);
        return selectedType != null ? $"{selectedType.Text} - {selectedType.Description}" : "Not selected";
    }

    public string GetSelectedDepartmentText()
    {
        var selectedDept = _departments.FirstOrDefault(d => d.Value == HazardReport.Department);
        return selectedDept != null ? $"{selectedDept.Text} - {selectedDept.Description}" : "Not specified";
    }

    public string GetSelectedLocationText()
    {
        // Clean location display logic - removed decorative prefix
        if (HasGeoLocation)
        {
            var locationText = $"{GeoLocationDisplay}";
            if (!string.IsNullOrEmpty(GeoLocationDescription))
            {
                locationText += $" - {GeoLocationDescription}";
            }
            return locationText;
        }
        
        // Fallback for when there's a location value but no geolocation
        if (!string.IsNullOrEmpty(HazardReport.Location) && HazardReport.Location != "MAP_LOCATION")
        {
            var selectedLocation = _locations.FirstOrDefault(l => l.Value == HazardReport.Location);
            return selectedLocation != null ? $"{selectedLocation.Text} - {selectedLocation.Description}" : HazardReport.Location;
        }
        
        return "No location selected - Please use the map to select a location";
    }

    public string GetSelectedPriorityText()
    {
        var selectedPriority = _priorities.FirstOrDefault(p => p.Value == HazardReport.Priority);
        return selectedPriority != null ? $"{selectedPriority.Text} - {selectedPriority.Description}" : "Not selected";
    }

    public IActionResult OnPostSetPresetLocation(double lat, double lng, string desc)
    {
        // DON'T clear model state - preserve form data
        // ModelState.Clear(); // Removed this line to preserve validation state
        
        InitializeDropdowns();
        
        SelectedGeoLocation = new GeoLocationData
        {
            Latitude = lat,
            Longitude = lng,
            Description = desc ?? "",
            SelectedDateTime = DateTime.UtcNow
        };
        
        ShowMapModal = false;
        HazardReport.Location = "MAP_LOCATION";
        
        InitializeUIState();
        TempData["InfoMessage"] = $"Location selected: {desc} ({lat:F6}, {lng:F6})";
        return Page();
    }

    public IActionResult OnPostSetManualLocation(double manualLatitude, double manualLongitude, string locationDescription)
    {
        // DON'T clear model state - preserve form data
        // ModelState.Clear(); // Removed this line to preserve validation state
        
        InitializeDropdowns();
        
        // Validate coordinates are within CORRECTED PDX airport bounds - actual airport property
        if (manualLatitude < 45.580 || manualLatitude > 45.595 || manualLongitude < -122.610 || manualLongitude > -122.580)
        {
            ModelState.AddModelError("", "Coordinates must be within PDX airport area bounds.");
            ShowMapModal = true;
            HazardReport.Location = "MAP_LOCATION";
            InitializeUIState();
            return Page();
        }
        
        SelectedGeoLocation = new GeoLocationData
        {
            Latitude = manualLatitude,
            Longitude = manualLongitude,
            Description = locationDescription ?? "",
            SelectedDateTime = DateTime.UtcNow
        };
        
        ShowMapModal = false;
        HazardReport.Location = "MAP_LOCATION";
        
        InitializeUIState();
        TempData["InfoMessage"] = $"Manual coordinates set: {manualLatitude:F6}, {manualLongitude:F6}";
        return Page();
    }

    private string GenerateHazardReportId()
    {
        // Use the ID generation service for human-readable Hazard IDs
        return _idGenerator.GenerateHazardId();
    }
}

public class HazardReportViewModel
{
    public string HazardType { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string CustomLocation { get; set; } = string.Empty; // For custom location entry
    public string Description { get; set; } = string.Empty;
    public bool IsConfidential { get; set; }
    public string Priority { get; set; } = "MEDIUM";
    public string Department { get; set; } = string.Empty;
    public string ReportedBy { get; set; } = string.Empty; // Added ReportedBy property
    public DateTime ReportedOn { get; set; } = DateTime.Now; // Will be initialized properly in page model
    public List<IFormFile> Attachments { get; set; } = new();
}

// Supporting option classes
public record HazardTypeOption(string Value, string Text, string Description);
public record PriorityOption(string Value, string Text, string Description, string CssClass);
public record LocationOption(string Value, string Text, string Description);
public record DepartmentOption(string Value, string Text, string Description);

// UI Helper classes
public class FileInfo
{
    public string Name { get; set; } = string.Empty;
    public long Size { get; set; }
    public string SizeDisplay { get; set; } = string.Empty;
}

// Geolocation support classes
public class GeoLocationData
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime SelectedDateTime { get; set; }
    
    public bool IsValid => Latitude != 0 && Longitude != 0;
    
    public string FormattedCoordinates => $"{Latitude:F6}, {Longitude:F6}";
    
    public string ToJson() => 
        $"{{\"lat\":{Latitude:F6},\"lng\":{Longitude:F6},\"description\":\"{Description}\",\"selectedAt\":\"{SelectedDateTime:yyyy-MM-ddTHH:mm:ssZ}\"}}";
}