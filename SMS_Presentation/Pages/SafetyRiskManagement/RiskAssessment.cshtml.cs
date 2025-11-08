using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

//using PDXSMS.UseCases.Queries.RiskAssessmentQueries;

using SMS_Infrastructure;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using System.Text.Json;
using System.IO;

namespace PDXSMS_Presentation.Pages.SafetyRiskManagement;

/// <summary>
/// FR-1.2.1 through FR-1.2.5: Five-Step Risk Assessment Process
/// Updated to work with new SMS Risk Validation system
/// </summary>
//public class RiskAssessmentModel : PageModel
//{
//    private readonly RiskAssessmentRepository _riskAssessmentRepository;
//    private readonly ILogger<RiskAssessmentModel> _logger;

//    public RiskAssessmentModel(
//        RiskAssessmentRepository riskAssessmentRepository,
//        ILogger<RiskAssessmentModel> logger)
//    {
//        _riskAssessmentRepository = riskAssessmentRepository ?? throw new ArgumentNullException(nameof(riskAssessmentRepository));
//        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//    }

//    public List<RiskAssessmentSummary> ActiveAssessments { get; set; } = new();
//    public List<RiskAssessmentSummary> CompletedAssessments { get; set; } = new();

//    [BindProperty]
//    public CreateAssessmentViewModel NewAssessment { get; set; } = new();

//    public async Task OnGetAsync()
//    {
//        ViewData["Title"] = "FR-1.2.1-1.2.5: Five-Step Risk Assessment";
//        await LoadAssessmentsFromNewSystemAsync();
//    }

//    public async Task<IActionResult> OnPostCreateAsync()
//    {
//        if (!ModelState.IsValid)
//        {
//            await LoadAssessmentsFromNewSystemAsync();
//            return Page();
//        }

//        try
//        {
//            // Create new assessment using Domain entity factory method
//            var newAssessment = RiskAssessment.Create(
//                assessmentName: NewAssessment.AssessmentName,
//                assessmentScope: NewAssessment.AssessmentScope ?? "",
//                leadAssessorId: new SMSUserID(User?.Identity?.Name ?? "System"),
//                plannedStartDate: DateTime.UtcNow
//            );

//            var assessmentId = await _riskAssessmentRepository.CreateAssessmentAsync(newAssessment);
            
//            TempData["SuccessMessage"] = $"Risk Assessment '{NewAssessment.AssessmentName}' created successfully. Starting Step 1.";
            
//            // Redirect to the NEW wizard implementation
//            return RedirectToPage("/SafetyRiskManagement/RiskAssessmentWizard", 
//                                 new { id = assessmentId, stepNumber = 1 });
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error creating new risk assessment");
//            TempData["ErrorMessage"] = "Error creating risk assessment. Please try again.";
//            await LoadAssessmentsFromNewSystemAsync();
//            return Page();
//        }
//    }

//    /// <summary>
//    /// Load assessments from the new SMS Risk Validation system (risk-assessments.json)
//    /// </summary>
//    private async Task LoadAssessmentsFromNewSystemAsync()
//    {
//        try
//        {
//            var appDataPath = Path.Combine(Directory.GetCurrentDirectory(), "AppData");
//            var assessmentsFilePath = Path.Combine(appDataPath, "risk-assessments.json");
            
//            if (!global::System.IO.File.Exists(assessmentsFilePath))
//            {
//                _logger.LogInformation("No risk-assessments.json file found");
//                ActiveAssessments = new List<RiskAssessmentSummary>();
//                CompletedAssessments = new List<RiskAssessmentSummary>();
//                return;
//            }

//            var json = await global::System.IO.File.ReadAllTextAsync(assessmentsFilePath);
//            var assessments = JsonSerializer.Deserialize<List<Dictionary<string, object?>>>(json, new JsonSerializerOptions
//            {
//                PropertyNameCaseInsensitive = true
//            }) ?? new List<Dictionary<string, object?>>();

//            _logger.LogInformation("?? Loaded {Count} assessments from new system", assessments.Count);

//            var summaries = new List<RiskAssessmentSummary>();
            
//            foreach (var assessment in assessments)
//            {
//                try
//                {
//                    var id = assessment.GetValueOrDefault("id")?.ToString() ?? "";
//                    var name = assessment.GetValueOrDefault("name")?.ToString() ?? "Unknown Assessment";
//                    var status = assessment.GetValueOrDefault("status")?.ToString() ?? "Unknown";
//                    var method = assessment.GetValueOrDefault("method")?.ToString() ?? "SMS5Step";
//                    var assessmentType = assessment.GetValueOrDefault("assessmentType")?.ToString() ?? "Initial";
                    
//                    // Parse dates safely
//                    DateTime createdDate = DateTime.UtcNow;
//                    DateTime lastModified = DateTime.UtcNow;
                    
//                    if (assessment.ContainsKey("createdDate") && assessment["createdDate"] != null)
//                    {
//                        if (DateTime.TryParse(assessment["createdDate"].ToString(), out var parsed))
//                            createdDate = parsed;
//                    }
                    
//                    if (assessment.ContainsKey("lastModifiedDate") && assessment["lastModifiedDate"] != null)
//                    {
//                        if (DateTime.TryParse(assessment["lastModifiedDate"].ToString(), out var parsed))
//                            lastModified = parsed;
//                    }

//                    // Get current step info
//                    var currentStep = 1;
//                    if (assessment.ContainsKey("currentStep") && assessment["currentStep"] != null)
//                    {
//                        if (int.TryParse(assessment["currentStep"].ToString(), out var step))
//                            currentStep = step;
//                    }

//                    // Determine if this should be shown based on status
//                    var shouldShow = ShouldShowAssessment(status, assessmentType);
//                    if (!shouldShow)
//                    {
//                        _logger.LogInformation("?? Skipping assessment {Id} - Status: {Status}, Type: {Type}", id, status, assessmentType);
//                        continue;
//                    }

//                    var summary = new RiskAssessmentSummary
//                    {
//                        Id = id,
//                        Name = name,
//                        LeadAssessor = GetCreatedBy(assessment),
//                        StartDate = createdDate,
//                        LastModified = lastModified,
//                        Status = status,
//                        Method = method,
//                        AssessmentType = assessmentType,
//                        CurrentStep = GetStepDisplay(currentStep, status),
//                        LastActivity = GetLastActivityDescription(status, method),
//                        Progress = CalculateProgress(currentStep, status),
//                        CompletedSteps = Math.Max(0, currentStep - 1),
//                        TotalSteps = method == "Simplified" ? 1 : 5,
//                        CompletedDate = IsCompleted(status) ? lastModified : null,
//                        HazardId = assessment.GetValueOrDefault("hazardId")?.ToString(),
//                        ReportId = assessment.GetValueOrDefault("reportId")?.ToString()
//                    };

//                    summaries.Add(summary);
                    
//                    _logger.LogInformation("? Added assessment {Id}: {Name} - Status: {Status}, Method: {Method}", 
//                        id, name, status, method);
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogError(ex, "Error parsing assessment data");
//                }
//            }

//            // Split into active and completed based on status
//            ActiveAssessments = summaries.Where(s => !IsCompleted(s.Status))
//                                        .OrderByDescending(s => s.LastModified)
//                                        .ToList();
            
//            CompletedAssessments = summaries.Where(s => IsCompleted(s.Status))
//                                           .OrderByDescending(s => s.CompletedDate ?? s.LastModified)
//                                           .ToList();

//            _logger.LogInformation("?? Final results: {Active} active, {Completed} completed assessments", 
//                ActiveAssessments.Count, CompletedAssessments.Count);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error loading assessments from new system");
//            ActiveAssessments = new List<RiskAssessmentSummary>();
//            CompletedAssessments = new List<RiskAssessmentSummary>();
//        }
//    }

//    private bool ShouldShowAssessment(string status, string assessmentType)
//    {
//        // Don't show closed non-SMS risk assessments
//        if (status == "Completed - Not SMS Risk")
//            return false;

//        // Don't show investigation pending assessments  
//        if (status == "Pending Investigation")
//            return false;

//        // Show all others
//        return true;
//    }

//    private string GetCreatedBy(Dictionary<string, object?> assessment)
//    {
//        return assessment.GetValueOrDefault("createdBy")?.ToString() ?? "System";
//    }

//    private bool IsCompleted(string status)
//    {
//        return status == "Completed" || status == "Completed - Not SMS Risk" || status.Contains("Completed");
//    }

//    private string GetStepDisplay(int currentStep, string status)
//    {
//        if (IsCompleted(status))
//        {
//            return "Completed";
//        }
        
//        var stepName = currentStep switch
//        {
//            1 => "System Description",
//            2 => "Hazard Identification", 
//            3 => "Risk Analysis",
//            4 => "Risk Assessment",
//            5 => "Risk Mitigation",
//            _ => "Ready to Start"
//        };
        
//        return $"Step {currentStep}: {stepName}";
//    }

//    private string GetLastActivityDescription(string status, string method)
//    {
//        if (status == "Ready to Start")
//            return $"Assessment created. Ready to begin {method.ToLower()} risk assessment.";
        
//        if (IsCompleted(status))
//            return "Assessment completed.";
            
//        return $"Assessment in progress using {method} method.";
//    }

//    private int CalculateProgress(int currentStep, string status)
//    {
//        if (IsCompleted(status))
//            return 100;
            
//        // For simplified, it's either 0% or 100%
//        if (status.Contains("Simplified"))
//        {
//            return currentStep > 1 ? 100 : 0;
//        }
        
//        // For 5-step process
//        return Math.Min((Math.Max(0, currentStep - 1) * 20), 100); // 20% per completed step
//    }

//    /// <summary>
//    /// Handle continue button - navigate to appropriate assessment page based on method
//    /// </summary>
//    public async Task<IActionResult> OnPostContinueAssessmentAsync(string assessmentId, string method, string hazardId)
//    {
//        try
//        {
//            _logger.LogInformation("?? Continue Assessment: {AssessmentId}, Method: {Method}, HazardId: {HazardId}", 
//                assessmentId, method, hazardId);

//            if (string.IsNullOrEmpty(assessmentId))
//            {
//                TempData["ErrorMessage"] = "Assessment ID is required";
//                return RedirectToPage();
//            }

//            // Navigate based on assessment method
//            return method?.ToLowerInvariant() switch
//            {
//                "simplified" => RedirectToPage("/SafetyRiskManagement/SimplifiedRiskAssessment", 
//                    new { hazardId = hazardId, assessmentId = assessmentId }),
//                "sms5step" or "technical" => RedirectToPage("/SafetyRiskManagement/RiskAssessmentWizard", 
//                    new { id = assessmentId, stepNumber = 1, hazardId = hazardId }),
//                _ => RedirectToPage("/SafetyRiskManagement/RiskAssessmentWizard", 
//                    new { id = assessmentId, stepNumber = 1, hazardId = hazardId })
//            };
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error continuing assessment {AssessmentId}", assessmentId);
//            TempData["ErrorMessage"] = "Error continuing assessment. Please try again.";
//            return RedirectToPage();
//        }
//    }
//}

//public class RiskAssessmentSummary
//{
//    public string Id { get; set; } = string.Empty;
//    public string Name { get; set; } = string.Empty;
//    public string LeadAssessor { get; set; } = string.Empty;
//    public DateTime StartDate { get; set; }
//    public DateTime LastModified { get; set; }
//    public DateTime? CompletedDate { get; set; }
//    public string Status { get; set; } = string.Empty;
//    public string Method { get; set; } = string.Empty;
//    public string AssessmentType { get; set; } = string.Empty;
//    public string CurrentStep { get; set; } = string.Empty;
//    public string LastActivity { get; set; } = string.Empty;
//    public int Progress { get; set; }
//    public int CompletedSteps { get; set; }
//    public int TotalSteps { get; set; } = 5;
//    public string? HazardId { get; set; }
//    public string? ReportId { get; set; }
//}

//public class CreateAssessmentViewModel
//{
//    public string AssessmentName { get; set; } = string.Empty;
//    public string AssessmentScope { get; set; } = string.Empty;
//    public string LeadAssessor { get; set; } = string.Empty;
//    public DateTime? PlannedStartDate { get; set; }
//    public DateTime? PlannedCompletionDate { get; set; }
//}

/// <summary>
/// Session data structure matching the one used in RiskAssessmentWizard
/// </summary>
//public class AssessmentSessionData
//{
//    public string SystemName { get; set; } = string.Empty;
//    public string SystemOverview { get; set; } = string.Empty;
//    public string SystemBoundaries { get; set; } = string.Empty;
//    public string SystemPurpose { get; set; } = string.Empty;
//    public List<IdentifiedHazardDto> IdentifiedHazards { get; set; } = new();
//    public string RiskAnalysisMethod { get; set; } = string.Empty;
//    public string RiskCriteria { get; set; } = string.Empty;
//    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
//}

/// <summary>
/// Simple DTO for session data hazards
/// </summary>
//public class IdentifiedHazardDto
//{
//    public string Id { get; set; } = string.Empty;
//    public string Description { get; set; } = string.Empty;
//    public string Category { get; set; } = string.Empty;
//    public string? RiskLevel { get; set; }
//    public string? Severity { get; set; }
//    public string? Likelihood { get; set; }
//    public string? Tolerability { get; set; }
//    public List<string> ProposedMitigations { get; set; } = new();
//    public List<string> CurrentMitigations { get; set; } = new();
//}

