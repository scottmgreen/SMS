using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PDXSMS.Services;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;

namespace PDXSMS_Presentation.Pages.SafetyRiskManagement;

/// <summary>
/// FR-HZ: Hazard Investigation
/// Provides comprehensive investigation workflow for hazards requiring additional information
/// </summary>
public class HazardInvestigationModel : PageModel
{
    private readonly ILogger<HazardInvestigationModel> _logger;
    private readonly UniversalJsonDataService _dataService;
    private readonly ReportDocumentService _reportDocumentService;
    private readonly UniversalUserRepository _userRepository;
    // Route parameters
    [FromRoute]
    public string HazardId { get; set; } = string.Empty;

    [FromQuery]
    public string ReportId { get; set; } = string.Empty;

    public HazardInvestigationModel(ILogger<HazardInvestigationModel> logger, UniversalUserRepository userRepository,UniversalJsonDataService dataService,ReportDocumentService reportDocumentService)
    {
        _logger = logger;
        _dataService = dataService;
        _reportDocumentService = reportDocumentService;
        _userRepository = userRepository;
    }

    // Investigation data
    public InvestigationData? Investigation { get; set; }
    
    // Combined documents from reports
    public CombinedInvestigationDocuments? CombinedDocuments { get; set; }

    // Form binding properties for new items
    [BindProperty]
    public string AssignedInvestigatorId { get; set; } = string.Empty;

    [BindProperty]
    public string InvestigatorNotes { get; set; } = string.Empty;

    [BindProperty]
    public Interview NewInterview { get; set; } = new();

    [BindProperty]
    public Document NewDocument { get; set; } = new();

    [BindProperty]
    public MediaItem NewImage { get; set; } = new();

    [BindProperty]
    public MediaItem NewVideo { get; set; } = new();

    [BindProperty]
    public InvestigationDecision Decision { get; set; } = new();

    // UI state
    public string ActiveTab { get; set; } = "interviews";

    // Data properties
    public HazardSummary? HazardSummary { get; set; }
    public ReportSummary? ReportSummary { get; set; }
    public List<SimpleSMSUser> AvailableInvestigators { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrEmpty(HazardId))
        {
            _logger.LogWarning("HazardId is required for investigation");
            return BadRequest("Hazard ID is required");
        }

        try
        {
            // Load hazard summary
            HazardSummary = await LoadHazardSummaryAsync();
            
            // Load report summary if ReportId provided
            if (!string.IsNullOrEmpty(ReportId))
            {
                ReportSummary = await LoadReportSummaryAsync();
            }
            
            // Load available investigators
            AvailableInvestigators = await LoadAvailableInvestigatorsAsync();
            
            // Load existing investigation
            Investigation = await LoadExistingInvestigationAsync();
            
            // Load combined documents from report-documents.json
            CombinedDocuments = await _reportDocumentService.GetInvestigationDocumentsAsync(HazardId, ReportId);
            
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading investigation for hazard {HazardId}", HazardId);
            TempData["ErrorMessage"] = "Error loading investigation data. Please try again.";
            return Page();
        }
    }

    /// <summary>
    /// Save basic investigation setup (investigator assignment and notes)
    /// </summary>
    public async Task<IActionResult> OnPostSaveInvestigationAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(HazardId))
            {
                return new JsonResult(new { success = false, message = "Hazard ID is required" });
            }

            if (string.IsNullOrEmpty(AssignedInvestigatorId))
            {
                return new JsonResult(new { success = false, message = "Please select an investigator" });
            }

            if (string.IsNullOrEmpty(InvestigatorNotes) || InvestigatorNotes.Length < 10)
            {
                return new JsonResult(new { success = false, message = "Investigator notes are required (minimum 10 characters)" });
            }

            // Load existing investigation or create new one
            var investigation = await LoadExistingInvestigationAsync() ?? new InvestigationData
            {
                Id = $"INV-{DateTime.Now:yyyy-MMdd}-{HazardId}",
                HazardId = HazardId,
                ReportId = ReportId,
                CreatedDate = DateTime.UtcNow
            };

            // Update basic information
            investigation.AssignedInvestigatorId = AssignedInvestigatorId;
            investigation.InvestigatorNotes = InvestigatorNotes;
            investigation.LastModifiedDate = DateTime.UtcNow;

            // Save to investigations.json file
            await SaveInvestigationAsync(investigation);

            _logger.LogInformation("Investigation {InvestigationId} basic setup saved for hazard {HazardId}", investigation.Id, HazardId);

            return new JsonResult(new { 
                success = true, 
                message = "Investigation setup saved successfully",
                investigationId = investigation.Id
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving investigation setup");
            return new JsonResult(new { success = false, message = "Error saving investigation setup" });
        }
    }

    /// <summary>
    /// Save investigation decision - determines next workflow step
    /// </summary>
    public async Task<IActionResult> OnPostSaveDecisionAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(HazardId))
            {
                return new JsonResult(new { success = false, message = "Hazard ID is required" });
            }

            // Validate decision
            if (string.IsNullOrEmpty(Decision.DecisionType))
            {
                return new JsonResult(new { success = false, message = "Please select a decision type" });
            }

            if (string.IsNullOrEmpty(Decision.Rationale) || Decision.Rationale.Length < 20)
            {
                return new JsonResult(new { success = false, message = "Decision rationale is required (minimum 20 characters)" });
            }

            if (string.IsNullOrEmpty(Decision.DecisionMaker))
            {
                return new JsonResult(new { success = false, message = "Decision maker name is required" });
            }

            // Load existing investigation
            var investigation = await LoadExistingInvestigationAsync();
            if (investigation == null)
            {
                return new JsonResult(new { success = false, message = "Investigation not found. Please save basic setup first." });
            }

            // Update decision
            Decision.Id = $"DEC-{DateTime.Now:yyyyMMdd-HHmmss}";
            Decision.DecisionDate = DateTime.UtcNow;
            investigation.Decision = Decision;
            investigation.Status = "Decision Made";
            investigation.LastModifiedDate = DateTime.UtcNow;

            // Save investigation
            await SaveInvestigationAsync(investigation);

            // Update hazard status based on decision
            var newHazardStatus = Decision.DecisionType switch
            {
                "SMSRisk" => "Under Review", // Return to validation
                "NoSMSRisk" => "Closed - No SMS Risk",
                "RequiresMoreInvestigation" => "Under Investigation",
                "ReferExternal" => "Closed - Referred",
                _ => "Under Review"
            };

            await UpdateHazardStatusAsync(newHazardStatus);

            _logger.LogInformation("Investigation decision {DecisionId} saved for investigation {InvestigationId}", Decision.Id, investigation.Id);

            return new JsonResult(new { 
                success = true, 
                message = "Investigation decision saved successfully",
                decisionId = Decision.Id,
                nextStep = GetNextStepMessage(Decision.DecisionType)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving investigation decision");
            return new JsonResult(new { success = false, message = "Error saving investigation decision" });
        }
    }

    /// <summary>
    /// Add a new interview to the investigation
    /// </summary>
    public async Task<IActionResult> OnPostAddInterviewAsync()
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                                      .SelectMany(x => x.Value.Errors.Select(e => e.ErrorMessage))
                                      .ToArray();
                return new JsonResult(new { success = false, message = string.Join(", ", errors) });
            }

            // Load existing investigation
            var investigation = await LoadExistingInvestigationAsync();
            if (investigation == null)
            {
                return new JsonResult(new { success = false, message = "Investigation not found. Please save basic setup first." });
            }

            // Create new interview with ID that relates back to investigation
            NewInterview.Id = $"INT-{DateTime.Now:yyyyMMdd-HHmmss}";
            NewInterview.InvestigationId = investigation.Id; // Link to investigation
            NewInterview.InterviewDate = NewInterview.InterviewDate == DateTime.MinValue ? DateTime.Now : NewInterview.InterviewDate;
            NewInterview.CreatedDate = DateTime.UtcNow;

            // Add to investigation
            investigation.Interviews.Add(NewInterview);
            investigation.LastModifiedDate = DateTime.UtcNow;

            // Save investigation
            await SaveInvestigationAsync(investigation);

            _logger.LogInformation("Interview {InterviewId} added to investigation {InvestigationId}", NewInterview.Id, investigation.Id);

            return new JsonResult(new { 
                success = true, 
                message = "Interview added successfully",
                interviewId = NewInterview.Id,
                activeTab = "interviews"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding interview");
            return new JsonResult(new { success = false, message = "Error adding interview" });
        }
    }

    /// <summary>
    /// Add a new document to the investigation
    /// </summary>
    public async Task<IActionResult> OnPostAddDocumentAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(NewDocument.Title) || string.IsNullOrEmpty(NewDocument.Description))
            {
                return new JsonResult(new { success = false, message = "Document title and description are required" });
            }

            // Load existing investigation
            var investigation = await LoadExistingInvestigationAsync();
            if (investigation == null)
            {
                return new JsonResult(new { success = false, message = "Investigation not found. Please save basic setup first." });
            }

            // Create new document with ID
            NewDocument.Id = $"DOC-{DateTime.Now:yyyyMMdd-HHmms}";
            NewDocument.InvestigationId = investigation.Id; // Link to investigation
            NewDocument.UploadedDate = DateTime.UtcNow;

            // Add to investigation
            investigation.Documents.Add(NewDocument);
            investigation.LastModifiedDate = DateTime.UtcNow;

            // Save investigation
            await SaveInvestigationAsync(investigation);

            _logger.LogInformation("Document {DocumentId} added to investigation {InvestigationId}", NewDocument.Id, investigation.Id);

            return new JsonResult(new { 
                success = true, 
                message = "Document added successfully",
                documentId = NewDocument.Id,
                activeTab = "documents"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding document");
            return new JsonResult(new { success = false, message = "Error adding document" });
        }
    }

    /// <summary>
    /// Add a new image to the investigation
    /// </summary>
    public async Task<IActionResult> OnPostAddImageAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(NewImage.Title) || string.IsNullOrEmpty(NewImage.Description))
            {
                return new JsonResult(new { success = false, message = "Image title and description are required" });
            }

            // Load existing investigation
            var investigation = await LoadExistingInvestigationAsync();
            if (investigation == null)
            {
                return new JsonResult(new { success = false, message = "Investigation not found. Please save basic setup first." });
            }

            // Create new image with ID
            NewImage.Id = $"IMG-{DateTime.Now:yyyyMMdd-HHmmss}";
            NewImage.InvestigationId = investigation.Id; // Link to investigation
            NewImage.UploadedDate = DateTime.UtcNow;
            NewImage.MediaType = "Image";

            // Add to investigation
            investigation.Images.Add(NewImage);
            investigation.LastModifiedDate = DateTime.UtcNow;

            // Save investigation
            await SaveInvestigationAsync(investigation);

            _logger.LogInformation("Image {ImageId} added to investigation {InvestigationId}", NewImage.Id, investigation.Id);

            return new JsonResult(new { 
                success = true, 
                message = "Image added successfully",
                imageId = NewImage.Id,
                activeTab = "images"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding image");
            return new JsonResult(new { success = false, message = "Error adding image" });
        }
    }

    /// <summary>
    /// Add a new video to the investigation
    /// </summary>
    public async Task<IActionResult> OnPostAddVideoAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(NewVideo.Title) || string.IsNullOrEmpty(NewVideo.Description))
            {
                return new JsonResult(new { success = false, message = "Video title and description are required" });
            }

            // Load existing investigation
            var investigation = await LoadExistingInvestigationAsync();
            if (investigation == null)
            {
                return new JsonResult(new { success = false, message = "Investigation not found. Please save basic setup first." });
            }

            // Create new video with ID
            NewVideo.Id = $"VID-{DateTime.Now:yyyyMMdd-HHmmss}";
            NewVideo.InvestigationId = investigation.Id; // Link to investigation
            NewVideo.UploadedDate = DateTime.UtcNow;
            NewVideo.MediaType = "Video";

            // Add to investigation
            investigation.Videos.Add(NewVideo);
            investigation.LastModifiedDate = DateTime.UtcNow;

            // Save investigation
            await SaveInvestigationAsync(investigation);

            _logger.LogInformation("Video {VideoId} added to investigation {InvestigationId}", NewVideo.Id, investigation.Id);

            return new JsonResult(new { 
                success = true, 
                message = "Video added successfully",
                videoId = NewVideo.Id,
                activeTab = "videos"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding video");
            return new JsonResult(new { success = false, message = "Error adding video" });
        }
    }

    /// <summary>
    /// Complete the investigation and return hazard to validation workflow
    /// </summary>
    public async Task<IActionResult> OnPostCompleteInvestigationAsync()
    {
        try
        {
            // Load existing investigation
            var investigation = await LoadExistingInvestigationAsync();
            if (investigation == null)
            {
                TempData["ErrorMessage"] = "Investigation not found";
                return RedirectToPage();
            }

            // Ensure a decision has been made
            if (investigation.Decision == null || string.IsNullOrEmpty(investigation.Decision.DecisionType))
            {
                TempData["ErrorMessage"] = "Please save an investigation decision before completing the investigation";
                return RedirectToPage();
            }

            // Mark investigation as complete
            investigation.Status = "Completed";
            investigation.CompletedDate = DateTime.UtcNow;
            investigation.LastModifiedDate = DateTime.UtcNow;

            // Save investigation
            await SaveInvestigationAsync(investigation);

            _logger.LogInformation("Investigation {InvestigationId} completed for hazard {HazardId}", investigation.Id, HazardId);

            var nextStep = GetNextStepMessage(investigation.Decision.DecisionType);
            TempData["SuccessMessage"] = $"Investigation {investigation.Id} completed successfully. {nextStep}";
            
            return RedirectToPage("/SafetyRiskManagement/HazardProcessing", null, "validation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing investigation");
            TempData["ErrorMessage"] = "Error completing investigation";
            return RedirectToPage();
        }
    }

    #region Helper Methods

    private async Task LoadHazardAndReportDataAsync()
    {
        try
        {
            // Load hazard data
            var hazards = _dataService.GetAllHazards();
            var hazard = hazards.FirstOrDefault(h => h.Id == HazardId);

            if (hazard != null)
            {
                HazardSummary = new HazardSummary
                {
                    Id = hazard.Id,
                    HazardType = hazard.HazardType,
                    Location = hazard.Location,
                    Description = hazard.Description,
                    Priority = hazard.Priority,
                    Status = hazard.Status,
                    ReportedDate = hazard.ReportedDate,
                    ReportedBy = hazard.ReportedById
                };
            }

            // Load report data if ReportId is provided
            if (!string.IsNullOrEmpty(ReportId))
            {
                var reports = _dataService.GetAllReports();
                var report = reports.FirstOrDefault(r => r.Id == ReportId || r.TrackingId == ReportId);

                if (report != null)
                {
                    ReportSummary = new ReportSummary
                    {
                        Id = report.Id,
                        TrackingId = report.TrackingId,
                        HazardId = report.HazardId,
                        SubmittedBy = report.SubmittedById,
                        SubmittedDate = report.SubmittedDate,
                        Status = report.Status
                    };
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading hazard and report data");
            throw;
        }
    }

    private async Task LoadInvestigatorsAsync()
    {
        try
        {
            // Load SMS users who can conduct investigations
            var smsUsers = await _userRepository.GetAllSMSUsersAsync();
            var investigators = smsUsers.Where(u => u.IsEmployee).ToList();

            AvailableInvestigators = investigators.Select(user => new SimpleSMSUser
            {
                Id = user.Id,
                DisplayName = user.DisplayName ?? user.Id,
                Email = user.Email ?? "",
                OrganizationName = user.Department ?? "Investigation Team"
            }).ToList();

            _logger.LogInformation("Loaded {Count} available investigators", AvailableInvestigators.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading investigators");
            AvailableInvestigators = new List<SimpleSMSUser>();
        }
    }

    private async Task<HazardSummary?> LoadHazardSummaryAsync()
    {
        try
        {
            var hazards = _dataService.GetAllHazards();
            var hazard = hazards.FirstOrDefault(h => h.Id == HazardId);

            if (hazard != null)
            {
                return new HazardSummary
                {
                    Id = hazard.Id,
                    HazardType = hazard.HazardType,
                    Location = hazard.Location,
                    Description = hazard.Description,
                    Priority = hazard.Priority,
                    Status = hazard.Status,
                    ReportedDate = hazard.ReportedDate,
                    ReportedBy = hazard.ReportedById
                };
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading hazard summary");
            return null;
        }
    }

    private async Task<ReportSummary?> LoadReportSummaryAsync()
    {
        try
        {
            var reports = _dataService.GetAllReports();
            var report = reports.FirstOrDefault(r => r.Id == ReportId || r.TrackingId == ReportId);

            if (report != null)
            {
                return new ReportSummary
                {
                    Id = report.Id,
                    TrackingId = report.TrackingId,
                    HazardId = report.HazardId,
                    SubmittedBy = report.SubmittedById,
                    SubmittedDate = report.SubmittedDate,
                    Status = report.Status
                };
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading report summary");
            return null;
        }
    }

    private async Task<List<SimpleSMSUser>> LoadAvailableInvestigatorsAsync()
    {
        try
        {
            var smsUsers = _userRepository.GetAllSMSUsersAsync().Result;
            return smsUsers.Where(u => u.IsEmployee).Select(u => new SimpleSMSUser
            {
                Id = u.Id,
                DisplayName = u.DisplayName ?? u.Id,
                Email = u.Email ?? "",
                OrganizationName = u.Department ?? "Investigation Team"
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading available investigators");
            return new List<SimpleSMSUser>();
        }
    }

    private async Task<InvestigationData?> LoadExistingInvestigationAsync()
    {
        try
        {
            var appDataPath = Path.Combine(Directory.GetCurrentDirectory(), "AppData");
            var investigationsFile = Path.Combine(appDataPath, "investigations.json");

            if (!global::System.IO.File.Exists(investigationsFile))
            {
                return null;
            }

            var json = await global::System.IO.File.ReadAllTextAsync(investigationsFile);
            var investigations = JsonSerializer.Deserialize<List<InvestigationData>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<InvestigationData>();

            var investigation = investigations.FirstOrDefault(i => i.HazardId == HazardId);
            
            if (investigation != null)
            {
                Investigation = investigation;
                AssignedInvestigatorId = investigation.AssignedInvestigatorId;
                InvestigatorNotes = investigation.InvestigatorNotes;
                
                // Load existing decision if present
                if (investigation.Decision != null)
                {
                    Decision = investigation.Decision;
                }
            }

            return investigation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading existing investigation");
            return null;
        }
    }

    private async Task SaveInvestigationAsync(InvestigationData investigation)
    {
        try
        {
            var appDataPath = Path.Combine(Directory.GetCurrentDirectory(), "AppData");
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            var investigationsFile = Path.Combine(appDataPath, "investigations.json");
            
            var investigations = new List<InvestigationData>();
            if (global::System.IO.File.Exists(investigationsFile))
            {
                var existingJson = await global::System.IO.File.ReadAllTextAsync(investigationsFile);
                investigations = JsonSerializer.Deserialize<List<InvestigationData>>(existingJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<InvestigationData>();
            }

            // Remove existing investigation with same ID and add updated one
            investigations.RemoveAll(i => i.Id == investigation.Id || i.HazardId == investigation.HazardId);
            investigations.Add(investigation);

            var json = JsonSerializer.Serialize(investigations, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await global::System.IO.File.WriteAllTextAsync(investigationsFile, json);
            Investigation = investigation; // Update display property
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving investigation");
            throw;
        }
    }

    private async Task UpdateHazardStatusAsync(string newStatus)
    {
        try
        {
            var appDataPath = Path.Combine(Directory.GetCurrentDirectory(), "AppData");
            var hazardsFile = Path.Combine(appDataPath, "hazards.json");

            if (!global::System.IO.File.Exists(hazardsFile))
            {
                return;
            }

            var json = await global::System.IO.File.ReadAllTextAsync(hazardsFile);
            var hazards = JsonSerializer.Deserialize<List<Dictionary<string, object?>>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<Dictionary<string, object?>>();

            var hazard = hazards.FirstOrDefault(h => h.ContainsKey("id") && h["id"]?.ToString() == HazardId);
            if (hazard != null)
            {
                hazard["status"] = newStatus;
                hazard["lastModifiedDate"] = DateTime.UtcNow;

                var updatedJson = JsonSerializer.Serialize(hazards, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await global::System.IO.File.WriteAllTextAsync(hazardsFile, updatedJson);
                _logger.LogInformation("Updated hazard {HazardId} status to {Status}", HazardId, newStatus);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating hazard status");
        }
    }

    private static string GetNextStepMessage(string decisionType) => decisionType switch
    {
        "SMSRisk" => "Hazard returned to validation workflow for SMS risk processing.",
        "NoSMSRisk" => "Hazard closed - determined to not constitute an SMS risk.",
        "RequiresMoreInvestigation" => "Hazard remains under investigation for additional information gathering.",
        "ReferExternal" => "Hazard closed - referred to external organization for handling.",
        _ => "Hazard returned to validation workflow."
    };

    #endregion
}

#region Data Models

public class HazardSummary
{
    public string Id { get; set; } = string.Empty;
    public string HazardType { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ReportedDate { get; set; }
    public string ReportedBy { get; set; } = string.Empty;
}

public class ReportSummary
{
    public string Id { get; set; } = string.Empty;
    public string TrackingId { get; set; } = string.Empty;
    public string HazardId { get; set; } = string.Empty;
    public string SubmittedBy { get; set; } = string.Empty;
    public DateTime SubmittedDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class InvestigationData
{
    public string Id { get; set; } = string.Empty;
    public string HazardId { get; set; } = string.Empty;
    public string ReportId { get; set; } = string.Empty;
    public string AssignedInvestigatorId { get; set; } = string.Empty;
    public string InvestigatorNotes { get; set; } = string.Empty;
    public string Status { get; set; } = "In Progress";
    public DateTime CreatedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime LastModifiedDate { get; set; }

    // Investigation Decision
    public InvestigationDecision? Decision { get; set; }

    // Related Evidence Collections - Each item has InvestigationId for relationship
    public List<Interview> Interviews { get; set; } = new();
    public List<Document> Documents { get; set; } = new();
    public List<MediaItem> Images { get; set; } = new();
    public List<MediaItem> Videos { get; set; } = new();
}

public class InvestigationDecision
{
    public string Id { get; set; } = string.Empty;
    
    [Required]
    public string DecisionType { get; set; } = string.Empty; // SMSRisk, NoSMSRisk, RequiresMoreInvestigation, ReferExternal
    
    [Required]
    public string Rationale { get; set; } = string.Empty;
    
    [Required]
    public string DecisionMaker { get; set; } = string.Empty;
    
    public DateTime DecisionDate { get; set; }
    
    public string NextSteps { get; set; } = string.Empty;
    
    public string ReferralDetails { get; set; } = string.Empty; // If referring externally
}

public class Interview
{
    public string Id { get; set; } = string.Empty;
    public string InvestigationId { get; set; } = string.Empty; // Links to Investigation
    
    [Required]
    public string PersonInterviewed { get; set; } = string.Empty;
    
    [Required]
    public DateTime InterviewDate { get; set; } = DateTime.Now;
    
    [Required]
    public string ConductedBy { get; set; } = string.Empty;
    
    [Required]
    public string InterviewNotes { get; set; } = string.Empty;
    
    public string InvestigatorNotes { get; set; } = string.Empty;
    
    public DateTime CreatedDate { get; set; }
}

public class Document
{
    public string Id { get; set; } = string.Empty;
    public string InvestigationId { get; set; } = string.Empty; // Links to Investigation
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    public string DocumentType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedDate { get; set; }
}

public class MediaItem
{
    public string Id { get; set; } = string.Empty;
    public string InvestigationId { get; set; } = string.Empty; // Links to Investigation
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    public string MediaType { get; set; } = string.Empty; // Image or Video
    public string FilePath { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedDate { get; set; }
}

//public class SimpleSMSUser
//{
//    public string Id { get; set; } = string.Empty;
//    public string DisplayName { get; set; } = string.Empty;
//    public string Email { get; set; } = string.Empty;
//    public string OrganizationName { get; set; } = string.Empty;
//}

public class InvestigationInterview
{
    public string Id { get; set; } = string.Empty;
    public string InvestigationId { get; set; } = string.Empty;
    
    [Required]
    public string PersonInterviewed { get; set; } = string.Empty;
    
    [Required]
    public DateTime InterviewDate { get; set; } = DateTime.Now;
    
    [Required]
    public string ConductedBy { get; set; } = string.Empty;
    
    [Required]
    public string InterviewNotes { get; set; } = string.Empty;
    
    public string InvestigatorNotes { get; set; } = string.Empty;
    
    public DateTime CreatedDate { get; set; }
}

public class InvestigationDocument
{
    public string Id { get; set; } = string.Empty;
    public string InvestigationId { get; set; } = string.Empty;
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    public string DocumentType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedDate { get; set; }
}

#endregion