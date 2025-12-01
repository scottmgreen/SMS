using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;
using SMS_Application.Messaging.Queries;
using SMS_Application.Services;

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

    // Replace StoredFileKeys with TempFileIds for disk-based temp storage
    [BindProperty]
    public List<string> TempFileIds { get; set; } = new();

    private bool _isProcessingFiles = false;

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

    /// <summary>
    /// Handles the final submission of the hazard report after user confirmation.
    /// Creates the complete hazard workflow: Report → Hazard → ScoringPanel → HazardLocation → HazardFiles ORIGINAL
    /// </summary>
    public async Task<IActionResult> OnPostSubmitConfirmed()
    {
        InitializeDropdowns();
        
        try
        {
            // ═══════════════════════════════════════════════════════════════════
            // STEP 1: VALIDATION
            // ═══════════════════════════════════════════════════════════════════
            if (!ModelState.IsValid)
            {
                ShowSubmissionConfirmation = true;
                InitializeUIState();
                return Page();
            }

            _logger.LogInformation("Starting hazard report submission for user: {User}", HazardReport.ReportedBy);

            // ═══════════════════════════════════════════════════════════════════
            // STEP 2: CREATE INITIAL HAZARD OBJECT FROM FORM DATA
            // ═══════════════════════════════════════════════════════════════════
            var hazardCode = $"HZ-0000";
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

            _logger.LogInformation("Initial hazard object created - Name: '{Name}', Type: '{Type}', Category: '{Category}'",
                hazard.Name, hazard.HazardType, hazard.Category);

            // ═══════════════════════════════════════════════════════════════════
            // PHASE 1: CREATE PARENT REPORT
            // ═══════════════════════════════════════════════════════════════════
            var report = new Report(new ReportID("RP-0000"))
            {
                Code = "RP-0000",
                CreatedBy = HazardReport.CreatedBy ?? "SYSTEM",
                Name = HazardReport.HazardType,
                Description = hazard.Description,
                Stage = "Initial",
                Status = "Initial"
            };

            var reportResult = await _mediator.SendAsync(new CreateReportCommand(report), CancellationToken.None);
            var actualReportCode = reportResult.Value.Code;
            
            _logger.LogInformation("Report created with Code: {ReportCode}", actualReportCode);

            // ═══════════════════════════════════════════════════════════════════
            // PHASE 2: CREATE HAZARD WITH REPORT LINKAGE
            // ═══════════════════════════════════════════════════════════════════
            hazard.ReportCode = actualReportCode;
            hazard.ScoringPanelCode = null;

            var createHazardCommand = new CreateHazardCommand(hazard);
            var createdHazardResult = await _mediator.SendAsync(createHazardCommand, CancellationToken.None);
            var createdHazard = createdHazardResult.Value;
            
            _logger.LogInformation("Hazard created with Code: {HazardCode}, linked to Report: {ReportCode}", 
                createdHazard.Code, actualReportCode);

            // ═══════════════════════════════════════════════════════════════════
            // PHASE 3: CREATE SCORING PANEL FOR RISK ASSESSMENT -- NOW DONE WHEN HAZARD IS CREATED
            // ═══════════════════════════════════════════════════════════════════
            ////////////var scoringPanel = new ScoringPanel(new ScoringPanelID("SP-0000"))
            ////////////{
            ////////////    HazardCode = createdHazard.Code
            ////////////};
            
            ////////////var createdPanelResult = await _mediator.SendAsync(new CreateScoringPanelCommand(scoringPanel), CancellationToken.None);
            ////////////var actualScoringPanelCode = createdPanelResult.Value.Code;
            
            ////////////createdHazard.ScoringPanelCode = actualScoringPanelCode;

            ////////////_logger.LogInformation("ScoringPanel created with Code: {ScoringPanelCode}, linked to Hazard: {HazardCode}",
            ////////////    actualScoringPanelCode, createdHazard.Code);

            // ═══════════════════════════════════════════════════════════════════
            // PHASE 4: CREATE GEOGRAPHIC LOCATION (IF PROVIDED)
            // ═══════════════════════════════════════════════════════════════════
            if (HasGeoLocation)
            {
                var hazardLocationResult = await _mediator.SendAsync(new GetHazardLocationsByHazardCodeQuery(createdHazard.Code), CancellationToken.None);

                HazardLocation hazardlocation = hazardLocationResult.Value.FirstOrDefault();
                hazardlocation.HazardCode = createdHazard.Code;
                hazardlocation.Latitude = SelectedGeoLocation.Latitude;
                hazardlocation.Longitude = SelectedGeoLocation.Longitude;
                hazardlocation.Description = SelectedGeoLocation.Description ?? "Map selected location";
                createdHazard.HazardLocation = hazardlocation;

                var locationUpdateResult = await _mediator.SendAsync(new UpdateHazardLocationCommand(hazardlocation), CancellationToken.None);

                _logger.LogInformation("HazardLocation created with Code: {LocationCode}, Coordinates: ({Lat}, {Lng})",
                    hazardlocation.Code, SelectedGeoLocation.Latitude, SelectedGeoLocation.Longitude);
            }

            // ═══════════════════════════════════════════════════════════════════
            // PHASE 5: PROCESS FILES - RESTORE FROM TEMP STORAGE & CREATE HAZARDFILES
            // ═══════════════════════════════════════════════════════════════════
            try
            {
                // 🔥 STEP 1: Restore files from temp storage if we have temp file IDs
                if (TempFileIds?.Any() == true)
                {
                    _logger.LogInformation("📤 Restoring {Count} files from temp disk storage", TempFileIds.Count);
                    
                    var tempFileInfos = await LoadTempFiles(TempFileIds);
                    HazardReport.HazardFiles = tempFileInfos.Select(CreateIFormFileFromTempFile).ToList();
                    
                    _logger.LogInformation("✅ Restored {Count} files for processing", HazardReport.HazardFiles.Count);
                }

                // 🔥 STEP 2: Process each file and create HazardFile entities
                if (HazardReport.HazardFiles?.Any() == true)
                {
                    _logger.LogInformation("📎 Processing {Count} files for Hazard: {HazardCode}", 
                        HazardReport.HazardFiles.Count, createdHazard.Code);

                    var createdFileIds = new List<string>();

                    foreach (var formFile in HazardReport.HazardFiles.Where(f => f?.Length > 0))
                    {
                        try
                        {
                            // Read file data
                            byte[] fileData;
                            using (var memoryStream = new MemoryStream())
                            {
                                await formFile.CopyToAsync(memoryStream);
                                fileData = memoryStream.ToArray();
                            }

                            // Generate unique file code
                            var fileCode = $"HF-0000";

                            // Create HazardFile entity
                            var hazardFile = new HazardFile(new HazardFileID(fileCode))
                            {
                                Code = fileCode,
                                HazardCode = createdHazard.Code,
                                ReportCode = createdHazard.ReportCode ?? string.Empty,
                                FileName = formFile.FileName,
                                FileType = Path.GetExtension(formFile.FileName)?.TrimStart('.') ?? "unknown",
                                ContentType = formFile.ContentType ?? "application/octet-stream",
                                FileSizeBytes = formFile.Length,
                                StorageType = "Database",
                                FileData = fileData,
                                UploadedBy = HazardReport.ReportedBy ?? "SYSTEM",
                                UploadedDate = DateTime.UtcNow,
                                IsActive = true,
                                IsConfidential = HazardReport.IsConfidential
                            };

                            // 🔥 Send CreateHazardFileCommand for each file
                            _logger.LogInformation("Creating HazardFile: {FileName} with Code: {FileCode}", 
                                formFile.FileName, fileCode);

                            var createHazardFileCommand = new CreateHazardFileCommand(hazardFile);
                            var hazardFileResult = await _mediator.SendAsync(createHazardFileCommand, CancellationToken.None);

                            if (hazardFileResult.IsSuccess)
                            {
                                var createdFileId = hazardFileResult.Value.Code;
                                createdFileIds.Add(createdFileId);

                                // Add HazardFile ID to Hazard's collection
                                createdHazard.AddHazardFile(new HazardFileID(createdFileId));

                                _logger.LogInformation("✅ Created HazardFile: {FileName} with ID: {FileId} for Hazard: {HazardCode}", 
                                    formFile.FileName, createdFileId, createdHazard.Code);
                            }
                            else
                            {
                                _logger.LogError("❌ Failed to create HazardFile: {FileName} for Hazard: {HazardCode}. Error: {Error}", 
                                    formFile.FileName, createdHazard.Code, hazardFileResult.Error?.Message);
                            }
                        }
                        catch (Exception fileEx)
                        {
                            _logger.LogError(fileEx, "❌ Exception creating HazardFile: {FileName} for Hazard: {HazardCode}", 
                                formFile.FileName, createdHazard.Code);
                        }
                    }

                    _logger.LogInformation("✅ File processing completed: {CreatedCount} HazardFiles created for Hazard: {HazardCode}", 
                        createdFileIds.Count, createdHazard.Code);
                }

                // Clean up temp files
                await CleanupTempFiles(TempFileIds);
            }
            catch (Exception fileEx)
            {
                _logger.LogError(fileEx, "❌ Error processing files, but continuing with hazard creation");
                await CleanupTempFiles(TempFileIds);
            }

            // ═══════════════════════════════════════════════════════════════════
            // PHASE 6: FINAL UPDATE
            // ═══════════════════════════════════════════════════════════════════
            var updatedHazardResult = await _mediator.SendAsync(new UpdateHazardCommand(createdHazard), CancellationToken.None);

            if (updatedHazardResult.IsFailure)
            {
                ModelState.AddModelError("", $"Failed to save hazard: {updatedHazardResult.Error.Message}");
                ShowSubmissionConfirmation = true;
                InitializeUIState();
                return Page();
            }

            // ═══════════════════════════════════════════════════════════════════
            // SUCCESS
            // ═══════════════════════════════════════════════════════════════════
            var finalHazard = updatedHazardResult.Value;
            GeneratedHazardId = finalHazard.Code;
            GeneratedReportId = finalHazard.ReportCode;
            SubmissionDateTime = DateTime.Now;
            ShowSubmissionConfirmation = false;
            ShowFinalSuccessConfirmation = true;
            
            TempFileIds.Clear();
            
            InitializeUIState();
            
            _logger.LogInformation("✅ Hazard report submission completed successfully - HazardCode: {HazardCode}, ReportCode: {ReportCode}", 
                finalHazard.Code, finalHazard.ReportCode);
            
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during hazard report submission");
            
            await CleanupTempFiles(TempFileIds);
            
            ModelState.AddModelError("", "An error occurred while saving your report.");
            ShowSubmissionConfirmation = false;
            InitializeUIState();
            return Page();
        }
    }

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
        
        // 🔥 CRITICAL: Save files to temp storage BEFORE showing preview
        if (HazardReport.HazardFiles?.Any() == true)
        {
            _logger.LogInformation("💾 Saving {Count} files to temp storage before preview", HazardReport.HazardFiles.Count);
            
            try
            {
                var tempFileInfos = SaveFilesTemporarily().GetAwaiter().GetResult();
                TempFileIds = tempFileInfos.Select(f => f.TempFileId).ToList();
                
                _logger.LogInformation("✅ Saved {Count} files to temp storage with IDs: {TempIds}", 
                    TempFileIds.Count, string.Join(", ", TempFileIds));
                
                // Update SelectedFiles for display in preview
                UpdateSelectedFilesFromTempFileInfo(tempFileInfos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to save files temporarily during preview");
                // Continue with preview even if file saving fails
            }
        }
        
        ShowPreview = true;
        InitializeUIState();
        return Page();
    }

    public IActionResult OnPostHidePreview()
    {
        InitializeDropdowns();
        
        // Restore files from temp storage when hiding preview
        if (TempFileIds?.Any() == true)
        {
            try
            {
                var tempFileInfos = LoadTempFiles(TempFileIds).GetAwaiter().GetResult();
                HazardReport.HazardFiles = tempFileInfos.Select(CreateIFormFileFromTempFile).ToList();
                
                _logger.LogInformation("✅ Restored {Count} files from temp storage", HazardReport.HazardFiles.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to restore files from temp storage");
                HazardReport.HazardFiles = new List<IFormFile>();
            }
        }
        
        ShowPreview = false;
        InitializeUIState();
        return Page();
    }

    // 🔥 UPDATE: Use temp file approach for confirmation too
    public async Task<IActionResult> OnPostShowSubmissionConfirmation()
    {
        InitializeDropdowns();
        
        _logger.LogInformation("OnPostShowSubmissionConfirmation: HasGeoLocation = {hasGeo}, Location = {location}", 
            HasGeoLocation, HazardReport.Location);
        
        // 🔥 CRITICAL FIX: Save files to disk immediately when user clicks "Submit"
        if (HazardReport.HazardFiles?.Any() == true)
        {
            _logger.LogInformation("💾 Saving {Count} files to temp disk storage before confirmation", HazardReport.HazardFiles.Count);
            
            try
            {
                var tempFileInfos = await SaveFilesTemporarily();
                TempFileIds = tempFileInfos.Select(f => f.TempFileId).ToList();
                
                _logger.LogInformation("✅ Saved {Count} files to temp storage with IDs: {TempIds}", 
                    TempFileIds.Count, string.Join(", ", TempFileIds));
                
                // Update SelectedFiles for display in confirmation modal
                UpdateSelectedFilesFromTempFileInfo(tempFileInfos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to save files temporarily");
                ModelState.AddModelError("", "Failed to process file uploads. Please try again.");
                InitializeUIState();
                return Page();
            }
        }
        
        ShowSubmissionConfirmation = true;
        ShowPreview = false;
        
        InitializeUIState();
        
        _logger.LogInformation("After confirmation setup: TempFiles = {fileCount}", TempFileIds?.Count ?? 0);
        
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
        // Clean up session files when cancelling
        
        TempFileIds.Clear();
        
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

    #region Temp File Management Methods

    private async Task<List<TempFileInfo>> SaveFilesTemporarily()
    {
        var tempFiles = new List<TempFileInfo>();
        
        if (HazardReport.HazardFiles?.Any() != true)
            return tempFiles;
        
        var tempPath = Path.Combine(Path.GetTempPath(), "HazardReports", HttpContext.Session.Id ?? Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempPath);
        
        foreach (var file in HazardReport.HazardFiles.Where(f => f?.Length > 0))
        {
            try
            {
                var tempFileId = Guid.NewGuid().ToString("N");
                var tempFileName = $"{tempFileId}_{file.FileName}";
                var tempFilePath = Path.Combine(tempPath, tempFileName);
                
                using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                
                tempFiles.Add(new TempFileInfo
                {
                    TempFileId = tempFileId,
                    OriginalFileName = file.FileName,
                    TempFilePath = tempFilePath,
                    ContentType = file.ContentType,
                    FileSize = file.Length
                });
                
                _logger.LogInformation("✅ Saved temp file: {FileName} as {TempPath}", file.FileName, tempFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to save temp file: {FileName}", file.FileName);
            }
        }
        
        return tempFiles;
    }

    private async Task<List<TempFileInfo>> LoadTempFiles(List<string> tempFileIds)
    {
        var tempFiles = new List<TempFileInfo>();
        
        var tempPath = Path.Combine(Path.GetTempPath(), "HazardReports", HttpContext.Session.Id ?? "unknown");
        
        if (!Directory.Exists(tempPath))
            return tempFiles;
        
        foreach (var tempFileId in tempFileIds)
        {
            try
            {
                var matchingFiles = Directory.GetFiles(tempPath, $"{tempFileId}_*");
                
                if (matchingFiles.Any())
                {
                    var tempFilePath = matchingFiles.First();
                    var originalFileName = Path.GetFileName(tempFilePath).Substring(tempFileId.Length + 1);
                    
                    var fileInfo = new FileInfo(tempFilePath);
                    
                    tempFiles.Add(new TempFileInfo
                    {
                        TempFileId = tempFileId,
                        OriginalFileName = originalFileName,
                        TempFilePath = tempFilePath,
                        ContentType = GetContentType(originalFileName),
                        FileSize = fileInfo.Length
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to load temp file: {TempFileId}", tempFileId);
            }
        }
        
        return tempFiles;
    }

    private IFormFile CreateIFormFileFromTempFile(TempFileInfo tempFile)
    {
        var fileData = global::System.IO.File.ReadAllBytes(tempFile.TempFilePath);
        return new TempFormFile(tempFile.OriginalFileName, tempFile.ContentType, fileData);
    }

    private async Task CleanupTempFiles(List<string> tempFileIds)
    {
        if (!tempFileIds?.Any() == true) return;
        
        var tempPath = Path.Combine(Path.GetTempPath(), "HazardReports", HttpContext.Session.Id ?? "unknown");
        
        if (!Directory.Exists(tempPath)) return;
        
        foreach (var tempFileId in tempFileIds)
        {
            try
            {
                var matchingFiles = Directory.GetFiles(tempPath, $"{tempFileId}_*");
                
                foreach (var file in matchingFiles)
                {
                    global::System.IO.File.Delete(file);
                    _logger.LogDebug("🗑️ Cleaned up temp file: {TempFile}", file);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Failed to cleanup temp file: {TempFileId}", tempFileId);
            }
        }
        
        try
        {
            if (Directory.Exists(tempPath) && !Directory.GetFiles(tempPath).Any())
            {
                Directory.Delete(tempPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Could not remove temp directory: {TempPath}", tempPath);
        }
    }

    private void UpdateSelectedFilesFromTempFileInfo(List<TempFileInfo> tempFiles)
    {
        SelectedFiles.Clear();
        
        foreach (var tempFile in tempFiles)
        {
            SelectedFiles.Add(new SelectedFile
            {
                FileName = tempFile.OriginalFileName,
                FileSizeBytes = tempFile.FileSize,
                SizeDisplay = GetFileSizeDisplay(tempFile.FileSize)
            });
        }
    }

    private static string GetFileSizeDisplay(long bytes)
    {
        if (bytes < 1024) return $"{bytes} bytes";
        if (bytes < 1048576) return $"{bytes / 1024} KB";
        return $"{bytes / 1048576:F1} MB";
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
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
        if (_isProcessingFiles)
        {
            _logger.LogDebug("ProcessFileAttachments: Already processing, skipping duplicate call");
            return;
        }

        try
        {
            _isProcessingFiles = true;
            SelectedFiles.Clear();
            
            // Process current uploaded files for display using existing HazardFiles property
            if (HazardReport.HazardFiles?.Any() == true)
            {
                _logger.LogInformation("ProcessFileAttachments: Found {Count} hazard files", HazardReport.HazardFiles.Count);
                
                foreach (var file in HazardReport.HazardFiles.Where(f => f?.Length > 0))
                {
                    SelectedFiles.Add(new SelectedFile 
                    { 
                        FileName = file.FileName, 
                        FileSizeBytes = file.Length,
                        SizeDisplay = GetFileSizeDisplay(file.Length)
                    });
                }
            }
            else
            {
                _logger.LogDebug("ProcessFileAttachments: No hazard files found");
            }

            _logger.LogDebug("ProcessFileAttachments completed: {SelectedFileCount} files ready for display", SelectedFiles.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ProcessFileAttachments");
        }
        finally
        {
            _isProcessingFiles = false;
        }
    }
    
    private void InitializeDropdowns()
    {
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

public class SavedFileInfo
{
    public string OriginalFileName { get; set; } = string.Empty;
    public string SavedFileName { get; set; } = string.Empty;
    public string SavedFilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
}

// 🔥 NEW: Temp file classes for reliable file handling
public class TempFileInfo
{
    public string TempFileId { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string TempFilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
}

public class TempFormFile : IFormFile
{
    private readonly byte[] _fileData;
    
    public TempFormFile(string fileName, string contentType, byte[] fileData)
    {
        FileName = fileName;
        ContentType = contentType;
        _fileData = fileData;
        Length = fileData.Length;
    }
    
    public string ContentType { get; }
    public string ContentDisposition => $"form-data; name=\"file\"; filename=\"{FileName}\"";
    public IHeaderDictionary Headers => new HeaderDictionary();
    public long Length { get; }
    public string Name => "file";
    public string FileName { get; }

    public Stream OpenReadStream() => new MemoryStream(_fileData);

    public void CopyTo(Stream target) => target.Write(_fileData, 0, _fileData.Length);

    public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
    {
        return target.WriteAsync(_fileData, 0, _fileData.Length, cancellationToken);
    }
}