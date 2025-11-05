using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace PDXSMS_Presentation.Pages.SafetyRiskManagement;

/// <summary>
/// Risk Assessment Details - Individual Assessment Management
/// Shows the detailed assessment process with interactive risk matrix
/// Now loads REAL data from risk-assessments.json supporting both SMS5Step and Simplified assessments
/// </summary>
public class RiskAssessmentDetailsModel : PageModel
{
    private readonly ILogger<RiskAssessmentDetailsModel> _logger;

    public RiskAssessmentDetailsModel(ILogger<RiskAssessmentDetailsModel> logger)
    {
        _logger = logger;
    }

    public RiskAssessmentDetail Assessment { get; set; } = new();
    public List<HazardRiskMapping> IdentifiedHazards { get; set; } = new();
    public RiskMatrix MatrixData { get; set; } = new();

    [BindProperty]
    public string AssessmentId { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            TempData["ErrorMessage"] = "Assessment ID is required";
            return RedirectToPage("/SafetyRiskManagement/RiskAssessment");
        }

        ViewData["Title"] = $"Risk Assessment Details - {id}";
        AssessmentId = id;
        
        var success = await LoadRealAssessmentDataAsync(id);
        if (!success)
        {
            TempData["ErrorMessage"] = $"Assessment {id} not found";
            return RedirectToPage("/SafetyRiskManagement/RiskAssessment");
        }

        return Page();
    }

    private async Task<bool> LoadRealAssessmentDataAsync(string assessmentId)
    {
        try
        {
            _logger.LogInformation("Loading real assessment data for {AssessmentId}", assessmentId);

            // Load from risk-assessments.json
            var appDataPath = Path.Combine(Directory.GetCurrentDirectory(), "AppData");
            var riskAssessmentsFile = Path.Combine(appDataPath, "risk-assessments.json");

            if (!global::System.IO.File.Exists(riskAssessmentsFile))
            {
                _logger.LogError("Risk assessments file not found: {File}", riskAssessmentsFile);
                return false;
            }

            var json = await global::System.IO.File.ReadAllTextAsync(riskAssessmentsFile);
            var assessments = JsonSerializer.Deserialize<List<Dictionary<string, object?>>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<Dictionary<string, object?>>();

            // Find the specific assessment
            var assessmentData = assessments.FirstOrDefault(a => 
                a.ContainsKey("id") && a["id"]?.ToString() == assessmentId);

            if (assessmentData == null)
            {
                _logger.LogWarning("Assessment {AssessmentId} not found in risk-assessments.json", assessmentId);
                return false;
            }

            // Extract assessment details
            Assessment = ExtractAssessmentDetails(assessmentData, assessmentId);
            
            // Extract hazards based on assessment method
            var method = assessmentData.GetValueOrDefault("method")?.ToString() ?? "";
            if (method == "Simplified")
            {
                IdentifiedHazards = ExtractSimplifiedHazards(assessmentData);
            }
            else
            {
                IdentifiedHazards = ExtractSMS5StepHazards(assessmentData);
            }

            // Load risk matrix data
            MatrixData = GetRiskMatrixData();

            _logger.LogInformation("Successfully loaded assessment {AssessmentId}: Method={Method}, Hazards={HazardCount}", 
                assessmentId, method, IdentifiedHazards.Count);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading assessment data for {AssessmentId}", assessmentId);
            return false;
        }
    }

    private RiskAssessmentDetail ExtractAssessmentDetails(Dictionary<string, object?> assessmentData, string assessmentId)
    {
        var method = assessmentData.GetValueOrDefault("method")?.ToString() ?? "Unknown";
        var name = assessmentData.GetValueOrDefault("name")?.ToString() ?? $"Assessment {assessmentId}";
        var status = assessmentData.GetValueOrDefault("status")?.ToString() ?? "Unknown";
        var currentStep = GetSafeInt(assessmentData, "currentStep", 1);
        
        // Calculate progress based on current step
        var progress = method == "Simplified" ? 
            (status == "Completed" ? 100 : 50) : 
            (currentStep * 20); // SMS5Step: 5 steps = 20% each

        // Extract dates
        var createdDate = GetSafeDateTime(assessmentData, "createdDate");
        var completedDate = GetSafeDateTime(assessmentData, "completedDate");

        // Extract lead assessor based on method
        var leadAssessor = "Unknown";
        var systemDescription = "System description not available";
        var scope = "Assessment scope not specified";

        if (method == "Simplified")
        {
            // For simplified assessments, get data from stepData.simplifiedRiskAssessment
            if (assessmentData.ContainsKey("stepData") && assessmentData["stepData"] is JsonElement stepDataElement)
            {
                if (stepDataElement.TryGetProperty("simplifiedRiskAssessment", out var simplifiedElement))
                {
                    leadAssessor = GetStringFromJsonElement(simplifiedElement, "assessedBy") ?? "Unknown";
                    systemDescription = "Simplified assessment - Limited system analysis conducted";
                    scope = $"Single hazard assessment for {assessmentData.GetValueOrDefault("hazardId")}";
                }
            }
        }
        else if (method == "SMS5Step")
        {
            // For SMS5Step assessments, get data from stepData.step1
            if (assessmentData.ContainsKey("stepData") && assessmentData["stepData"] is JsonElement stepDataElement)
            {
                if (stepDataElement.TryGetProperty("step1", out var step1Element))
                {
                    leadAssessor = GetStringFromJsonElement(step1Element, "leadAssessor") ?? "Unknown";
                    systemDescription = GetStringFromJsonElement(step1Element, "systemDescription") ?? "System description not available";
                    scope = GetStringFromJsonElement(step1Element, "systemBoundaries") ?? "Assessment scope not specified";
                }
            }
        }

        return new RiskAssessmentDetail
        {
            Id = assessmentId,
            Name = name,
            CurrentStep = currentStep,
            Progress = progress,
            LeadAssessor = leadAssessor,
            StartDate = createdDate,
            Status = status,
            SystemDescription = systemDescription,
            Scope = scope,
            Method = method,
            CompletedDate = completedDate
        };
    }

    private List<HazardRiskMapping> ExtractSimplifiedHazards(Dictionary<string, object?> assessmentData)
    {
        var hazards = new List<HazardRiskMapping>();

        try
        {
            var hazardId = assessmentData.GetValueOrDefault("hazardId")?.ToString() ?? "Unknown";
            
            if (assessmentData.ContainsKey("stepData") && assessmentData["stepData"] is JsonElement stepDataElement)
            {
                if (stepDataElement.TryGetProperty("simplifiedRiskAssessment", out var simplifiedElement))
                {
                    var likelihood = GetStringFromJsonElement(simplifiedElement, "likelihood") ?? "Unknown";
                    var severity = GetStringFromJsonElement(simplifiedElement, "severity") ?? "Unknown";
                    var existingControls = GetStringFromJsonElement(simplifiedElement, "existingControls") ?? "None specified";
                    var recommendedActions = GetStringFromJsonElement(simplifiedElement, "recommendedActions") ?? "No actions specified";
                    var overallRisk = GetStringFromJsonElement(simplifiedElement, "overallRisk") ?? "Unknown";

                    // Convert severity text to number and likelihood text to letter for risk matrix
                    var severityNum = ConvertSeverityTextToNumber(severity);
                    var likelihoodLetter = ConvertLikelihoodTextToLetter(likelihood);
                    var riskLevel = $"{severityNum}{likelihoodLetter}";

                    // Load hazard description from hazards.json if possible
                    var hazardDescription = LoadHazardDescription(hazardId) ?? "Hazard assessed via simplified method";

                    hazards.Add(new HazardRiskMapping
                    {
                        HazardId = hazardId,
                        Description = hazardDescription,
                        Likelihood = likelihoodLetter,
                        Severity = severityNum.ToString(),
                        RiskLevel = riskLevel,
                        Category = "Simplified Assessment",
                        WorstCredibleOutcome = recommendedActions,
                        CurrentMitigations = new[] { existingControls },
                        OverallRisk = overallRisk,
                        AssessmentMethod = "Simplified"
                    });

                    _logger.LogInformation("Extracted simplified hazard: {HazardId} -> Risk: {RiskLevel} ({OverallRisk})", 
                        hazardId, riskLevel, overallRisk);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting simplified hazards from assessment data");
        }

        return hazards;
    }

    private List<HazardRiskMapping> ExtractSMS5StepHazards(Dictionary<string, object?> assessmentData)
    {
        var hazards = new List<HazardRiskMapping>();

        try
        {
            if (assessmentData.ContainsKey("stepData") && assessmentData["stepData"] is JsonElement stepDataElement)
            {
                // Get hazards from step2 (Identify Hazards)
                if (stepDataElement.TryGetProperty("step2", out var step2Element))
                {
                    if (step2Element.TryGetProperty("identifiedHazards", out var hazardsElement) && 
                        hazardsElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var hazardElement in hazardsElement.EnumerateArray())
                        {
                            var hazardId = GetStringFromJsonElement(hazardElement, "id") ?? "Unknown";
                            var description = GetStringFromJsonElement(hazardElement, "description") ?? "No description";
                            var category = GetStringFromJsonElement(hazardElement, "category") ?? "Unknown";

                            // Get risk analysis from step3 if available
                            var worstOutcome = "To be determined";
                            if (stepDataElement.TryGetProperty("step3", out var step3Element))
                            {
                                if (step3Element.TryGetProperty("hazardAnalysisData", out var analysisElement) && 
                                    analysisElement.ValueKind == JsonValueKind.Array)
                                {
                                    foreach (var analysis in analysisElement.EnumerateArray())
                                    {
                                        var analysisHazardId = GetStringFromJsonElement(analysis, "hazardId");
                                        if (analysisHazardId == hazardId)
                                        {
                                            worstOutcome = GetStringFromJsonElement(analysis, "worstCredibleOutcome") ?? worstOutcome;
                                            break;
                                        }
                                    }
                                }
                            }

                            // Get risk scores from step4 if available
                            var riskLevel = "TBD";
                            var likelihood = "TBD";
                            var severity = "TBD";
                            
                            if (stepDataElement.TryGetProperty("step4", out var step4Element))
                            {
                                if (step4Element.TryGetProperty("hazardAverageScores", out var scoresElement))
                                {
                                    if (scoresElement.TryGetProperty(hazardId, out var scoreElement))
                                    {
                                        var avgScore = scoreElement.GetDouble();
                                        // Convert average score to risk level (simplified conversion)
                                        riskLevel = ConvertScoreToRiskLevel(avgScore);
                                        var parts = ParseRiskLevel(riskLevel);
                                        severity = parts.severity;
                                        likelihood = parts.likelihood;
                                    }
                                }
                            }

                            hazards.Add(new HazardRiskMapping
                            {
                                HazardId = hazardId,
                                Description = description,
                                Likelihood = likelihood,
                                Severity = severity,
                                RiskLevel = riskLevel,
                                Category = category,
                                WorstCredibleOutcome = worstOutcome,
                                CurrentMitigations = new[] { "Standard operational procedures" },
                                OverallRisk = ConvertRiskLevelToOverallRisk(riskLevel),
                                AssessmentMethod = "SMS5Step"
                            });
                        }
                    }
                }
            }

            _logger.LogInformation("Extracted {Count} SMS5Step hazards from assessment data", hazards.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting SMS5Step hazards from assessment data");
        }

        return hazards;
    }

    // Helper methods for data conversion and extraction
    private string? LoadHazardDescription(string hazardId)
    {
        try
        {
            var appDataPath = Path.Combine(Directory.GetCurrentDirectory(), "AppData");
            var hazardsFile = Path.Combine(appDataPath, "hazards.json");

            if (!global::System.IO.File.Exists(hazardsFile)) return null;

            var json = global::System.IO.File.ReadAllText(hazardsFile);
            var hazards = JsonSerializer.Deserialize<List<Dictionary<string, object?>>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<Dictionary<string, object?>>();

            var hazard = hazards.FirstOrDefault(h => 
                h.ContainsKey("id") && h["id"]?.ToString() == hazardId);

            return hazard?.GetValueOrDefault("description")?.ToString();
        }
        catch
        {
            return null;
        }
    }

    private int ConvertSeverityTextToNumber(string severityText)
    {
        return severityText.ToLowerInvariant() switch
        {
            "negligible" or "minor" => 1,
            "moderate" => 2,
            "serious" => 3,
            "major" => 4,
            "catastrophic" => 5,
            _ => 2
        };
    }

    private string ConvertLikelihoodTextToLetter(string likelihoodText)
    {
        return likelihoodText.ToLowerInvariant() switch
        {
            "very low" or "rare" => "A",
            "low" or "unlikely" => "B", 
            "medium" or "possible" => "C",
            "high" or "likely" => "D",
            "very high" or "frequent" => "E",
            _ => "C"
        };
    }

    private string ConvertScoreToRiskLevel(double avgScore)
    {
        // Simple conversion from numeric score to risk level
        // This is a basic approximation - in real implementation you'd use proper scoring matrix
        return avgScore switch
        {
            >= 15 => "5E",
            >= 12 => "4D", 
            >= 9 => "3C",
            >= 6 => "2C",
            >= 3 => "2B",
            _ => "1A"
        };
    }

    private (string severity, string likelihood) ParseRiskLevel(string riskLevel)
    {
        if (string.IsNullOrEmpty(riskLevel) || riskLevel == "TBD") 
            return ("TBD", "TBD");
            
        var severity = riskLevel.Substring(0, 1);
        var likelihood = riskLevel.Length > 1 ? riskLevel.Substring(1, 1) : "C";
        return (severity, likelihood);
    }

    private string ConvertRiskLevelToOverallRisk(string riskLevel)
    {
        if (riskLevel == "TBD") return "To Be Determined";
        
        var severity = int.TryParse(riskLevel.Substring(0, 1), out var sev) ? sev : 2;
        var likelihood = riskLevel.Length > 1 ? (riskLevel[1] - 'A') : 2;

        // Use the same logic as the view functions
        if ((severity == 1 && likelihood <= 2) || (severity == 2 && likelihood == 0))
            return "Very Low Risk";
        else if ((severity == 1 && likelihood <= 4) || (severity == 2 && likelihood <= 1) || (severity == 3 && likelihood == 0))
            return "Low Risk";
        else if ((severity == 1 && likelihood == 4) || (severity == 2 && likelihood <= 3) || (severity == 3 && likelihood == 1) || (severity == 4 && likelihood == 0))
            return "Medium Risk";
        else if ((severity == 2 && likelihood == 4) || (severity == 3 && likelihood <= 3) || (severity == 4 && likelihood <= 2) || (severity == 5 && likelihood == 0))
            return "High Risk";
        else
            return "Very High Risk";
    }

    private string? GetStringFromJsonElement(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var prop) ? prop.GetString() : null;
    }

    private int GetSafeInt(Dictionary<string, object?> data, string key, int defaultValue = 0)
    {
        if (data.ContainsKey(key) && data[key] != null)
        {
            if (data[key] is JsonElement element && element.ValueKind == JsonValueKind.Number)
                return element.GetInt32();
            if (int.TryParse(data[key].ToString(), out var result))
                return result;
        }
        return defaultValue;
    }

    private DateTime GetSafeDateTime(Dictionary<string, object?> data, string key)
    {
        if (data.ContainsKey(key) && data[key] != null)
        {
            if (data[key] is JsonElement element && element.ValueKind == JsonValueKind.String)
            {
                if (DateTime.TryParse(element.GetString(), out var result))
                    return result;
            }
            if (DateTime.TryParse(data[key].ToString(), out var result2))
                return result2;
        }
        return DateTime.Now;
    }

    private RiskMatrix GetRiskMatrixData()
    {
        return new RiskMatrix
        {
            LikelihoodLabels = new[] { "A\nRare", "B\nUnlikely", "C\nPossible", "D\nLikely", "E\nFrequent" },
            LikelihoodDescriptions = new[] 
            { 
                "Less than once per decade", 
                "Occurs once every 5-10 years", 
                "Once/year or a few times every 2-5 yrs", 
                "Once/month or several times/yr", 
                "At least once/week" 
            },
            SeverityLabels = new[] { "1\nMinor", "2\nModerate", "3\nSerious", "4\nMajor", "5\nCatastrophic" },
            SeverityDescriptions = new[]
            {
                "First aid only, <30-minute delay, no coverage",
                "Injury treated on site, <60-minute delay, localized interruption",
                "Off-site medical treatment, Temporary RWY or TWY closure",
                "Significant or Permanent injury, Partial airport closure for extended period",
                "Fatality/Fatalities, Full airport closure, Major infrastructure damage"
            }
        };
    }
}

public class RiskAssessmentDetail
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int CurrentStep { get; set; }
    public int Progress { get; set; }
    public string LeadAssessor { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string SystemDescription { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;  // NEW: Track assessment method
    public DateTime CompletedDate { get; set; }         // NEW: Track completion date
}

public class HazardRiskMapping
{
    public string HazardId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Likelihood { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string WorstCredibleOutcome { get; set; } = string.Empty;
    public string[] CurrentMitigations { get; set; } = Array.Empty<string>();
    public string OverallRisk { get; set; } = string.Empty;        // NEW: Overall risk assessment
    public string AssessmentMethod { get; set; } = string.Empty;   // NEW: Method used for assessment
}

public class RiskMatrix
{
    public string[] LikelihoodLabels { get; set; } = Array.Empty<string>();
    public string[] LikelihoodDescriptions { get; set; } = Array.Empty<string>();
    public string[] SeverityLabels { get; set; } = Array.Empty<string>();
    public string[] SeverityDescriptions { get; set; } = Array.Empty<string>();
}
