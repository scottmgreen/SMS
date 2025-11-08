using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PDXSMS.Services;
using PDXSMS.UseCases.Commands.HazardProcessing;
using PDXSMS.Interfaces;
using PDXSMS_Domain.Common;
using System.Text.Json;
using SMS_Domain.Entities;

namespace PDXSMS_Presentation.Pages.SafetyRiskManagement;

/// <summary>
/// FR-1.1.2: Process Hazard Reports
/// Complete SMS hazard processing workflow supporting validation, risk assessment, 
/// leadership escalation, and mitigation tracking per SMS methodology
/// </summary>
public class HazardProcessingModel : PageModel
{
    private readonly UniversalJsonDataService _dataService;
    private readonly RiskAssessmentRepository _riskAssessmentRepository;
    private readonly IRequestHandler<ValidateSMSRiskWithDatasetCommand, Result<ValidateSMSRiskWithDatasetResponse>> _validateSMSRiskHandler;
    private readonly ILogger<HazardProcessingModel> _logger;

    public HazardProcessingModel(
        UniversalJsonDataService dataService,
        RiskAssessmentRepository riskAssessmentRepository,
        IRequestHandler<ValidateSMSRiskWithDatasetCommand, Result<ValidateSMSRiskWithDatasetResponse>> validateSMSRiskHandler,
        ILogger<HazardProcessingModel> logger)
    {
        _dataService = dataService;
        _riskAssessmentRepository = riskAssessmentRepository;
        _validateSMSRiskHandler = validateSMSRiskHandler;
        _logger = logger;
    }

    public HazardProcessingDashboard Dashboard { get; set; } = new();
    public List<HazardReportSummary> PendingValidation { get; set; } = new();
    public List<HazardReportSummary> PendingRiskAssessment { get; set; } = new();
    public List<HazardReportSummary> PendingTRA { get; set; } = new();
    public List<HazardReportSummary> PendingLeadershipApproval { get; set; } = new();
    public List<HazardReportSummary> InMitigation { get; set; } = new();
    public List<HazardReportSummary> ClosedReferred { get; set; } = new();
    
    // NEW: Investigation tab data
    public List<HazardReportSummary> PendingInvestigation { get; set; } = new();

    [BindProperty]
    public HazardValidationAction ValidationAction { get; set; } = new();

    [BindProperty]
    public RiskAssessmentAction RiskAction { get; set; } = new();

    [BindProperty]
    public LeadershipApprovalAction ApprovalAction { get; set; } = new();

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "SMS Hazard Processing Workflow";
        await LoadHazardProcessingDataAsync();
    }

    public async Task<IActionResult> OnPostValidateHazardAsync()
    {
        try
        {
            _logger.LogInformation("Processing validation decision for hazard: {HazardId}, Decision: {Decision}", 
                ValidationAction.HazardId, ValidationAction.Decision);

            if (!ModelState.IsValid)
            {
                await LoadHazardProcessingDataAsync();
                return Page();
            }

            // Validate required fields
            if (string.IsNullOrEmpty(ValidationAction.HazardId))
            {
                ModelState.AddModelError(nameof(ValidationAction.HazardId), "Hazard ID is required");
                await LoadHazardProcessingDataAsync();
                return Page();
            }

            if (string.IsNullOrEmpty(ValidationAction.Decision))
            {
                ModelState.AddModelError(nameof(ValidationAction.Decision), "Please select a validation decision");
                await LoadHazardProcessingDataAsync();
                return Page();
            }

            if (string.IsNullOrEmpty(ValidationAction.Rationale) || ValidationAction.Rationale.Length < 10)
            {
                ModelState.AddModelError(nameof(ValidationAction.Rationale), "Rationale is required (minimum 10 characters)");
                await LoadHazardProcessingDataAsync();
                return Page();
            }

            // Create validation command using the SMS Risk Validation command handler
            var processingDecision = ValidationAction.Decision switch
            {
                "SMSRisk" => "sms_risk",
                "NoSMSRisk" => "not_sms_risk", 
                "NeedsInvestigation" => "investigation",
                _ => "investigation" // Default fallback
            };

            var command = new ValidateSMSRiskWithDatasetCommand
            {
                HazardId = ValidationAction.HazardId,
                ValidatedById = ValidationAction.ValidatedBy ?? User?.Identity?.Name ?? "System User",
                ProcessingDecision = processingDecision,
                ProcessingNotes = ValidationAction.Rationale,
                RiskAssessmentMethod = "Simplified", // Default for quick processing
                InformationNeeded = processingDecision == "investigation" ? ValidationAction.Rationale : null,
                ClosureReason = processingDecision == "not_sms_risk" ? ValidationAction.Rationale : null,
                ReferralAction = processingDecision == "not_sms_risk" ? "Referred to appropriate department" : null,
                AirportSharedDataset = new AirportSharedDatasetDto() // Empty dataset for quick validation
            };

            // Execute the validation command
            var result = await _validateSMSRiskHandler.HandleAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                var response = result.Value;
                
                // Set appropriate success messages and redirect
                switch (ValidationAction.Decision)
                {
                    case "SMSRisk":
                        TempData["SuccessMessage"] = $"Hazard {ValidationAction.HazardId} validated as SMS Risk and forwarded for risk assessment.";
                        _logger.LogInformation("Hazard {HazardId} validated as SMS Risk", ValidationAction.HazardId);
                        break;
                        
                    case "NoSMSRisk":
                        TempData["InfoMessage"] = $"Hazard {ValidationAction.HazardId} determined as No SMS Risk and closed.";
                        _logger.LogInformation("Hazard {HazardId} determined as No SMS Risk", ValidationAction.HazardId);
                        break;
                        
                    case "NeedsInvestigation":
                        TempData["WarningMessage"] = $"Hazard {ValidationAction.HazardId} requires additional investigation. Check the Investigation tab.";
                        _logger.LogInformation("Hazard {HazardId} requires investigation", ValidationAction.HazardId);
                        // Redirect to Investigation tab
                        return RedirectToPage("/SafetyRiskManagement/HazardProcessing", null, "investigation");
                }
                
                return RedirectToPage();
            }
            else
            {
                _logger.LogError("Failed to validate hazard {HazardId}: {Error}", ValidationAction.HazardId, result.Error.Message);
                ModelState.AddModelError(string.Empty, $"Error processing validation: {result.Error.Message}");
                await LoadHazardProcessingDataAsync();
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing validation for hazard {HazardId}", ValidationAction.HazardId);
            TempData["ErrorMessage"] = $"Error processing validation: {ex.Message}";
            await LoadHazardProcessingDataAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostProcessRiskAssessmentAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadHazardProcessingDataAsync();
            return Page();
        }

        try
        {
            // Process risk assessment results
            switch (RiskAction.RiskScore)
            {
                case "Low":
                case "Medium":
                    TempData["SuccessMessage"] = $"Hazard {RiskAction.HazardId} assessed as {RiskAction.RiskScore} risk. Moving to tracking and documentation.";
                    break;
                case "High":
                case "Critical":
                    TempData["WarningMessage"] = $"Hazard {RiskAction.HazardId} assessed as {RiskAction.RiskScore} risk. Triggering Technical Risk Assessment (TRA).";
                    break;
            }

            // TODO: Implement risk score routing logic
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error processing risk assessment: {ex.Message}";
            await LoadHazardProcessingDataAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostLeadershipApprovalAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadHazardProcessingDataAsync();
            return Page();
        }

        try
        {
            switch (ApprovalAction.Decision)
            {
                case "Approved":
                    TempData["SuccessMessage"] = $"Hazard {ApprovalAction.HazardId} approved by SMS Leadership. Proceeding to mitigation implementation.";
                    break;
                case "Rejected":
                    TempData["WarningMessage"] = $"Hazard {ApprovalAction.HazardId} rejected by SMS Leadership. Returning for revision.";
                    break;
                case "Override":
                    TempData["InfoMessage"] = $"SMS Leadership override applied to {ApprovalAction.HazardId}. Escalating to Technical Risk Assessment.";
                    break;
            }

            // TODO: Implement leadership approval workflow
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error processing leadership approval: {ex.Message}";
            await LoadHazardProcessingDataAsync();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostTriggerTRAAsync(string hazardId)
    {
        try
        {
            // TODO: Implement TRA initiation
            TempData["InfoMessage"] = $"Technical Risk Assessment (TRA) initiated for hazard {hazardId}.";
            return RedirectToPage("/SafetyRiskManagement/TechnicalRiskAssessment", new { hazardId });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error initiating TRA: {ex.Message}";
            return RedirectToPage();
        }
    }

    private async Task LoadHazardProcessingDataAsync()
    {
        try
        {
            // Load all hazard reports from the repository
            var allReports = _dataService.GetAllReports();
            var allHazards = _dataService.GetAllHazards();
            
            // Load assessments to get assessment IDs and methods
            var assessments = await LoadAssessmentsAsync();
            
            // Load mitigation summaries from existing mitigations.json
            var mitigationSummaries = await LoadMitigationsAsync();
            
            Console.WriteLine($"*** DEBUG: Loaded {allReports.Count} reports, {allHazards.Count} hazards, {assessments.Count} assessments, and {mitigationSummaries.Count} mitigation summaries");
            
            // Debug: Log hazard statuses
            foreach (var hazard in allHazards)
            {
                Console.WriteLine($"*** DEBUG: Hazard {hazard.Id} has status: '{hazard.Status}'");
            }
            
            // Convert to HazardReportSummary and categorize by status
            var hazardSummaries = allReports.Select(report => 
            {
                // Get the associated hazard data for more complete information
                var hazard = allHazards.FirstOrDefault(h => h.Id == report.HazardId);
                
                // Get the associated assessment data - FIX: Extract hazard ID from assessment name
                Dictionary<string, object?>? assessment = null;
                string? assessmentMethod = null;
                string? assessmentId = null;
                
                // Try multiple approaches to find the assessment
                foreach (var a in assessments)
                {
                    var aId = a.GetValueOrDefault("id")?.ToString();
                    var aName = a.GetValueOrDefault("name")?.ToString() ?? "";
                    
                    // Check if hazardId field exists (new format)
                    var directHazardId = a.GetValueOrDefault("hazardId")?.ToString();
                    if (directHazardId == report.HazardId)
                    {
                        assessment = a;
                        assessmentId = aId;
                        Console.WriteLine($"*** DEBUG: Found assessment by direct hazardId: {assessmentId} -> {report.HazardId}");
                        break;
                    }
                    
                    // Check if assessment name contains the hazard ID (legacy format)
                    if (aName.Contains(report.HazardId, StringComparison.OrdinalIgnoreCase))
                    {
                        assessment = a;
                        assessmentId = aId;
                        Console.WriteLine($"*** DEBUG: Found assessment by name match: {assessmentId} -> {aName} contains {report.HazardId}");
                        break;
                    }
                }
                
                if (assessment != null)
                {
                    var assessmentName = assessment.GetValueOrDefault("name")?.ToString() ?? "";
                    var assessmentType = assessment.GetValueOrDefault("assessmentType")?.ToString() ?? "";
                    var methodFromAssessment = assessment.GetValueOrDefault("method")?.ToString() ?? "";
                    
                    // ?? IMPROVED: Better method detection logic
                    if (assessmentName.Contains("Simplified", StringComparison.OrdinalIgnoreCase))
                    {
                        assessmentMethod = "Simplified";
                    }
                    else if (assessmentName.Contains("SMS5Step", StringComparison.OrdinalIgnoreCase) || 
                             methodFromAssessment == "SMS5Step" ||
                             assessmentType == "Initial" ||
                             assessmentName.Contains("Technical", StringComparison.OrdinalIgnoreCase))
                    {
                        assessmentMethod = "SMS5Step"; // This will trigger "Start Technical Assessment" button
                    }
                    else if (assessmentType == "Non-SMS Risk Closure")
                    {
                        assessmentMethod = "Documentation";
                    }
                    else
                    {
                        // ?? DEFAULT: If unclear, default to SMS5Step (Technical Assessment)
                        assessmentMethod = "SMS5Step";
                    }
                }
                
                Console.WriteLine($"*** DEBUG: Assessment {assessmentId} -> Method: {assessmentMethod} -> Name: {assessment?.GetValueOrDefault("name")}");

                // Get the associated mitigation summary data from existing mitigations.json
                var mitigationSummary = mitigationSummaries.FirstOrDefault(ms => 
                    ms.RelatedHazard == report.HazardId);
                
                var effectiveStatus = hazard?.Status ?? report.Status;
                Console.WriteLine($"*** DEBUG: Report {report.Id} -> Hazard {report.HazardId} -> Effective Status: '{effectiveStatus}' -> Mitigation: {mitigationSummary?.Id}");
                
                return new HazardReportSummary
                {
                    Id = report.TrackingId ?? report.Id,
                    Type = hazard?.HazardType ?? "Unknown",
                    Location = hazard?.Location ?? "Unknown",
                    Priority = hazard?.Priority ?? "Medium",
                    ReportedBy = report.SubmittedById,
                    ReportedDate = report.SubmittedDate,
                    Status = effectiveStatus, // Use hazard status if available (more current)
                    ProcessStage = GetProcessStageFromStatus(effectiveStatus),
                    DaysInStage = (DateTime.UtcNow - report.SubmittedDate).Days,
                    AssignedTo = GetAssignedToFromStatus(effectiveStatus),
                    RiskScore = GetRiskScoreFromHazard(hazard) ?? GetRiskScoreFromStatus(effectiveStatus),
                    ClosureReason = GetClosureReasonFromStatus(effectiveStatus),
                    // Assessment data for navigation - FIXED: Use proper method detection
                    AssessmentId = assessmentId,
                    AssessmentMethod = assessmentMethod,
                    HazardId = report.HazardId, // Store actual hazard ID for navigation
                    // Mitigation summary data for enhanced tracking using existing mitigations.json
                    MitigationSummaryId = mitigationSummary?.Id,
                    MitigationType = mitigationSummary?.MitigationType,
                    MitigationStatus = mitigationSummary?.Status,
                    NextReviewDate = mitigationSummary?.DueDate,
                    ResponsibleParty = mitigationSummary?.ResponsiblePerson ?? GetAssignedToFromStatus(effectiveStatus),
                    MitigationProgress = mitigationSummary?.Progress
                };
            }).ToList();

            Console.WriteLine($"*** DEBUG: Created {hazardSummaries.Count} hazard summaries");

            // ?? UPDATED CATEGORIZATION: Handle new status values from SMS Risk Validation AND Simplified Risk Assessment
            PendingValidation = hazardSummaries.Where(h => 
                h.Status == "Submitted" || 
                h.Status == "Under Review" || 
                h.Status == "New"
            ).ToList();

            PendingRiskAssessment = hazardSummaries.Where(h => 
                h.Status == "SMS Risk Assessment - In Progress" ||
                h.Status == "Risk Assessment" || 
                h.Status == "Processing"
            ).ToList();

            // NEW: Investigation category for "Need More Info" decisions
            PendingInvestigation = hazardSummaries.Where(h => 
                h.Status == "Under Investigation - Information Needed" ||
                h.Status == "Investigation Required" ||
                h.Status == "Pending Investigation"
            ).ToList();

            PendingTRA = hazardSummaries.Where(h => 
                h.Status == "TRA Required" || 
                h.Status == "Technical Assessment"
            ).ToList();

            PendingLeadershipApproval = hazardSummaries.Where(h => 
                h.Status == "Leadership Review" || 
                h.Status == "Awaiting Approval"
            ).ToList();

            // CRITICAL FIX: Include new statuses from simplified risk assessment completion
            InMitigation = hazardSummaries.Where(h => 
                h.Status == "Mitigation Planning" || 
                h.Status == "In Progress" || 
                h.Status == "Implementation" ||
                h.Status == "Tracking" // NEW: Low risk assessments that need monitoring
            ).ToList();

            // ?? UPDATED CLOSED CATEGORY: Exclude investigations from closed
            ClosedReferred = hazardSummaries.Where(h => 
                h.Status == "Closed" || 
                h.Status == "Referred" || 
                h.Status == "Completed" ||
                h.Status == "Closed - Not SMS Risk"
                // Removed: "Under Investigation - Information Needed" moved to Investigation tab
            ).ToList();

            Console.WriteLine($"*** DEBUG: Categorization Results:");
            Console.WriteLine($"  Pending Validation: {PendingValidation.Count}");
            Console.WriteLine($"  Pending Risk Assessment: {PendingRiskAssessment.Count}");
            Console.WriteLine($"  Pending Investigation: {PendingInvestigation.Count}"); // NEW
            Console.WriteLine($"  Pending TRA: {PendingTRA.Count}");
            Console.WriteLine($"  Pending Leadership: {PendingLeadershipApproval.Count}");
            Console.WriteLine($"  In Mitigation: {InMitigation.Count}");
            Console.WriteLine($"  Closed/Referred: {ClosedReferred.Count}");

            // Log what's in closed
            foreach (var closed in ClosedReferred)
            {
                Console.WriteLine($"  -> Closed item: {closed.Id} ({closed.Status})");
            }

            // Calculate dashboard metrics from actual data
            var activeCount = hazardSummaries.Where(h => 
                !h.Status.Contains("Closed", StringComparison.OrdinalIgnoreCase) && 
                h.Status != "Referred" && 
                h.Status != "Completed"
            ).Count();

            Dashboard = new HazardProcessingDashboard
            {
                TotalActive = activeCount,
                PendingValidation = PendingValidation.Count,
                PendingRiskAssessment = PendingRiskAssessment.Count,
                PendingInvestigation = PendingInvestigation.Count, // NEW
                PendingTRA = PendingTRA.Count,
                PendingLeadershipApproval = PendingLeadershipApproval.Count,
                InMitigation = InMitigation.Count,
                ClosedThisMonth = ClosedReferred.Where(h => h.ReportedDate >= DateTime.UtcNow.AddMonths(-1)).Count(),
                AverageProcessingTime = CalculateAverageProcessingTime(ClosedReferred)
            };

            Console.WriteLine($"*** HAZARD PROCESSING: Loaded {allReports.Count} total reports from JSON files");
            Console.WriteLine($"*** HAZARD PROCESSING: Active: {Dashboard.TotalActive}, Closed this month: {Dashboard.ClosedThisMonth}");
            
            // Log breakdown of statuses for debugging
            foreach (var status in hazardSummaries.GroupBy(h => h.Status))
            {
                Console.WriteLine($"*** HAZARD PROCESSING: Status '{status.Key}': {status.Count()} items");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"*** HAZARD PROCESSING: Error loading data: {ex.Message}");
            
            // Fallback to empty data if repository fails
            Dashboard = new HazardProcessingDashboard
            {
                TotalActive = 0,
                PendingValidation = 0,
                PendingRiskAssessment = 0,
                PendingTRA = 0,
                PendingLeadershipApproval = 0,
                InMitigation = 0,
                ClosedThisMonth = 0,
                AverageProcessingTime = 0.0
            };
            
            PendingValidation = new List<HazardReportSummary>();
            PendingRiskAssessment = new List<HazardReportSummary>();
            PendingTRA = new List<HazardReportSummary>();
            PendingLeadershipApproval = new List<HazardReportSummary>();
            InMitigation = new List<HazardReportSummary>();
            ClosedReferred = new List<HazardReportSummary>();
            PendingInvestigation = new List<HazardReportSummary>();
        }
    }

    /// <summary>
    /// Load assessments from risk-assessments.json to get assessment IDs and methods
    /// </summary>
    private async Task<List<Dictionary<string, object?>>> LoadAssessmentsAsync()
    {
        try
        {
            var appDataPath = Path.Combine(Directory.GetCurrentDirectory(), "AppData");
            var assessmentsFilePath = Path.Combine(appDataPath, "risk-assessments.json");
            
            if (!global::System.IO.File.Exists(assessmentsFilePath))
            {
                Console.WriteLine("*** HAZARD PROCESSING: No risk-assessments.json file found");
                return new List<Dictionary<string, object?>>();
            }

            var json = await global::System.IO.File.ReadAllTextAsync(assessmentsFilePath);
            var assessments = JsonSerializer.Deserialize<List<Dictionary<string, object?>>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<Dictionary<string, object?>>();

            Console.WriteLine($"*** HAZARD PROCESSING: Loaded {assessments.Count} assessments for navigation mapping");
            return assessments;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"*** HAZARD PROCESSING: Error loading assessments: {ex.Message}");
            return new List<Dictionary<string, object?>>();
        }
    }

    /// <summary>
    /// Load mitigation summaries from existing mitigations.json file
    /// </summary>
    private async Task<List<MitigationSummary>> LoadMitigationsAsync()
    {
        try
        {
            var appDataPath = Path.Combine(Directory.GetCurrentDirectory(), "AppData");
            var mitigationsFilePath = Path.Combine(appDataPath, "mitigations.json");
            
            if (!global::System.IO.File.Exists(mitigationsFilePath))
            {
                Console.WriteLine("*** HAZARD PROCESSING: No mitigations.json file found");
                return new List<MitigationSummary>();
            }

            var json = await global::System.IO.File.ReadAllTextAsync(mitigationsFilePath);
            var mitigations = JsonSerializer.Deserialize<List<MitigationSummary>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<MitigationSummary>();

            Console.WriteLine($"*** HAZARD PROCESSING: Loaded {mitigations.Count} mitigations for tracking display");
            return mitigations;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"*** HAZARD PROCESSING: Error loading mitigations: {ex.Message}");
            return new List<MitigationSummary>();
        }
    }

    /// <summary>
    /// Get the correct navigation URL for starting a risk assessment based on method
    /// </summary>
    public static string GetRiskAssessmentNavigationUrl(string? assessmentId, string? method, string? hazardId)
    {
        Console.WriteLine($"*** DEBUG NAV: AssessmentId={assessmentId}, Method={method}, HazardId={hazardId}");
        
        // ?? FIXED: Always go to Step 1 for Technical/SMS5Step assessments
        if (string.IsNullOrEmpty(assessmentId))
        {
            // If no assessment ID, we need to create one first or fallback to wizard
            Console.WriteLine("*** DEBUG NAV: No assessment ID, falling back to RiskAssessment creation");
            return "/SafetyRiskManagement/RiskAssessment";
        }

        var url = method?.ToLowerInvariant() switch
        {
            "simplified" => $"/SafetyRiskManagement/SimplifiedRiskAssessment/{hazardId}/{assessmentId}",
            "sms5step" or "technical" or "initial" => $"/SafetyRiskManagement/RiskAssessmentWizard/{assessmentId}/step/1?hazardId={hazardId}",
            "documentation" => $"/SafetyRiskManagement/RiskAssessment", // Documentation assessments are typically view-only
            // ?? CRITICAL FIX: Default to Step 1 of wizard for any unknown method
            _ => $"/SafetyRiskManagement/RiskAssessmentWizard/{assessmentId}/step/1?hazardId={hazardId}"
        };
        
        Console.WriteLine($"*** DEBUG NAV: Generated URL: {url}");
        return url;
    }

    /// <summary>
    /// Determine the assessment method based on assessment and mitigation data
    /// </summary>
    private static string DetermineAssessmentMethod(Dictionary<string, object?>? assessment, MitigationSummary? mitigationSummary)
    {
        if (assessment != null)
        {
            var assessmentName = assessment.GetValueOrDefault("name")?.ToString() ?? "";
            var assessmentType = assessment.GetValueOrDefault("assessmentType")?.ToString() ?? "";
            
            // Determine method based on name and type
            if (assessmentName.Contains("Simplified", StringComparison.OrdinalIgnoreCase))
            {
                return "Simplified";
            }
            else if (assessmentName.Contains("SMS5Step", StringComparison.OrdinalIgnoreCase) || 
                     assessmentType == "Initial")
            {
                return "SMS5Step";
            }
            else if (assessmentType == "Non-SMS Risk Closure")
            {
                return "Documentation";
            }
        }
        
        // Fallback to mitigation-based method determination
        if (mitigationSummary != null && !string.IsNullOrEmpty(mitigationSummary.EffectivenessRating))
        {
            switch (mitigationSummary.EffectivenessRating)
            {
                case "Effective":
                case "Partially Effective":
                    return "Documentation"; // No further assessment needed
                case "Not Effective":
                    return "TRA Required"; // TRA needed for ineffective mitigations
            }
        }
        
        // Default to SMS5Step if no other method can be determined
        return "SMS5Step";
    }

    private string GetProcessStageFromStatus(string status)
    {
        return status switch
        {
            "Submitted" or "Under Review" or "New" => "Validation",
            "SMS Risk Assessment - In Progress" or "Risk Assessment" => "Risk Assessment",
            "Under Investigation - Information Needed" or "Investigation Required" or "Pending Investigation" => "Investigation", // NEW
            "TRA Required" or "Technical Assessment" => "TRA",
            "Leadership Review" or "Awaiting Approval" => "Leadership Approval",
            "Mitigation Planning" or "In Progress" or "Implementation" => "Mitigation",
            "Tracking" => "Monitoring", // NEW: For low risk items that need tracking
            "Closed" or "Referred" or "Completed" or "Closed - Not SMS Risk" => "Closed",
            _ => "Unknown"
        };
    }

    private string? GetAssignedToFromStatus(string status)
    {
        return status switch
        {
            "SMS Risk Assessment - In Progress" or "Risk Assessment" => "Safety Analyst Team",
            "Under Investigation - Information Needed" or "Investigation Required" or "Pending Investigation" => "Investigation Team", // NEW
            "TRA Required" or "Technical Assessment" => "Technical Risk Assessment Team",
            "Leadership Review" or "Awaiting Approval" => "SMS Leadership",
            "Mitigation Planning" or "In Progress" or "Implementation" => "Operations Team",
            "Tracking" => "Safety Monitoring Team", // NEW: For low risk monitoring
            _ => null
        };
    }

    private string? GetRiskScoreFromStatus(string status)
    {
        return status switch
        {
            "TRA Required" or "Technical Assessment" => "High",
            "Leadership Review" or "Awaiting Approval" => "High", 
            "Mitigation Planning" => "Medium-High", // NEW: Requires active mitigation
            "In Progress" or "Implementation" => "Medium",
            "Tracking" => "Low", // NEW: Low risk items from completed assessments
            "SMS Risk Assessment - In Progress" => "TBD",
            _ => null
        };
    }

    /// <summary>
    /// Enhanced method to get risk score from hazard's risk assessment data if available
    /// </summary>
    private string? GetRiskScoreFromHazard(dynamic? hazard)
    {
        try
        {
            // Check if hazard has riskAssessmentData with overallRisk
            if (hazard != null && hazard.riskAssessmentData != null)
            {
                var riskData = hazard.riskAssessmentData;
                if (riskData.overallRisk != null)
                {
                    return riskData.overallRisk.ToString();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"*** DEBUG: Error extracting risk score from hazard data: {ex.Message}");
        }
        
        return null;
    }

    private string? GetClosureReasonFromStatus(string status)
    {
        return status switch
        {
            "Closed - Not SMS Risk" => "Determined not to be an SMS risk",
            "Under Investigation - Information Needed" => "Additional information required",
            _ => null
        };
    }

    private double CalculateAverageProcessingTime(List<HazardReportSummary> closedReports)
    {
        if (!closedReports.Any()) return 0.0;

        var processingTimes = closedReports.Select(r => 
            (DateTime.UtcNow - r.ReportedDate).Days).ToList();

        return processingTimes.Average();
    }
}

public class HazardProcessingDashboard
{
    public int TotalActive { get; set; }
    public int PendingValidation { get; set; }
    public int PendingRiskAssessment { get; set; }
    public int PendingInvestigation { get; set; } // NEW
    public int PendingTRA { get; set; }
    public int PendingLeadershipApproval { get; set; }
    public int InMitigation { get; set; }
    public int ClosedThisMonth { get; set; }
    public double AverageProcessingTime { get; set; }
}

public class HazardReportSummary
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string ReportedBy { get; set; } = string.Empty;
    public DateTime ReportedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ProcessStage { get; set; } = string.Empty;
    public int DaysInStage { get; set; }
    public string? AssignedTo { get; set; }
    public string? RiskScore { get; set; }
    public bool TRACompleted { get; set; }
    public int? MitigationProgress { get; set; }
    public string? ClosureReason { get; set; }
    
    // Assessment navigation properties
    public string? AssessmentId { get; set; }
    public string? AssessmentMethod { get; set; }
    public string? HazardId { get; set; }
    
    // NEW: Mitigation summary tracking properties
    public string? MitigationSummaryId { get; set; }
    public string? MitigationType { get; set; }
    public string? MitigationStatus { get; set; }
    public DateTime? NextReviewDate { get; set; }
    public string? ResponsibleParty { get; set; }
    
    /// <summary>
    /// Get the correct navigation URL for starting this assessment
    /// </summary>
    public string GetAssessmentNavigationUrl()
    {
        return HazardProcessingModel.GetRiskAssessmentNavigationUrl(AssessmentId, AssessmentMethod, HazardId);
    }
}

public class HazardValidationAction
{
    public string HazardId { get; set; } = string.Empty;
    public string Decision { get; set; } = string.Empty; // SMSRisk, NoSMSRisk, NeedsInvestigation
    public string Rationale { get; set; } = string.Empty;
    public string ValidatedBy { get; set; } = string.Empty;
}

public class RiskAssessmentAction
{
    public string HazardId { get; set; } = string.Empty;
    public string RiskScore { get; set; } = string.Empty; // Low, Medium, High, Critical
    public string AssessmentMethod { get; set; } = string.Empty;
    public string AssessedBy { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public class LeadershipApprovalAction
{
    public string HazardId { get; set; } = string.Empty;
    public string Decision { get; set; } = string.Empty; // Approved, Rejected, Override
    public string ApprovedBy { get; set; } = string.Empty;
    public string Comments { get; set; } = string.Empty;
}

