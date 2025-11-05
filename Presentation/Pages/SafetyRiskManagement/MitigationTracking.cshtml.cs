using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PDXSMS.Services;
using System.Text.Json;

namespace PDXSMS_Presentation.Pages.SafetyRiskManagement;

/// <summary>
/// FR-1.3.1: Track Mitigation Implementation
/// Monitor and verify the effectiveness of implemented risk mitigations
/// </summary>
public class MitigationTrackingModel : PageModel
{
    private readonly SMSIdGenerationService _idGenerationService;
    private readonly string _dataDirectory;
    private readonly string _mitigationsFile;
    private readonly JsonSerializerOptions _jsonOptions;

    public MitigationTrackingModel(SMSIdGenerationService idGenerationService)
    {
        _idGenerationService = idGenerationService;
        _dataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "AppData");
        _mitigationsFile = Path.Combine(_dataDirectory, "mitigations.json");
        
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public List<MitigationSummary> ActiveMitigations { get; set; } = new();
    public List<MitigationSummary> CompletedMitigations { get; set; } = new();
    public List<MitigationSummary> OverdueMitigations { get; set; } = new();

    [BindProperty]
    public UpdateMitigationViewModel UpdateMitigation { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? HazardId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? MitigationId { get; set; }

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "FR-1.3.1: Track Mitigation Implementation";
        await LoadMitigationsAsync();
    }

    public async Task<IActionResult> OnPostUpdateAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadMitigationsAsync();
            return Page();
        }

        try
        {
            await UpdateMitigationAsync(UpdateMitigation);
            TempData["SuccessMessage"] = $"Mitigation {UpdateMitigation.MitigationId} updated successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error updating mitigation: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCreateMitigationAsync(string hazardId, string riskAssessmentId, string riskLevel)
    {
        try
        {
            var mitigationId = await CreateMitigationFromAssessmentAsync(hazardId, riskAssessmentId, riskLevel);
            TempData["SuccessMessage"] = $"Mitigation summary {mitigationId} created for hazard {hazardId}";
            return RedirectToPage(new { mitigationId });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error creating mitigation: {ex.Message}";
            return RedirectToPage("/SafetyRiskManagement/HazardProcessing");
        }
    }

    private async Task LoadMitigationsAsync()
    {
        try
        {
            var allMitigations = await LoadMitigationsFromFileAsync();
            
            // Filter by hazard ID if specified
            if (!string.IsNullOrEmpty(HazardId))
            {
                allMitigations = allMitigations.Where(m => m.RelatedHazard == HazardId).ToList();
            }

            // Filter by mitigation ID if specified
            if (!string.IsNullOrEmpty(MitigationId))
            {
                allMitigations = allMitigations.Where(m => m.Id == MitigationId).ToList();
            }

            ActiveMitigations = allMitigations.Where(m => m.Status == "In Progress" || m.Status == "Monitoring").ToList();
            CompletedMitigations = allMitigations.Where(m => m.Status == "Completed").ToList();
            OverdueMitigations = allMitigations.Where(m => m.IsOverdue && m.Status != "Completed").ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading mitigations: {ex.Message}");
            // Fallback to empty lists
        }
    }

    private async Task<List<MitigationSummary>> LoadMitigationsFromFileAsync()
    {
        if (!global::System.IO.File.Exists(_mitigationsFile))
        {
            return new List<MitigationSummary>();
        }

        try
        {
            var json = await global::System.IO.File.ReadAllTextAsync(_mitigationsFile);
            var mitigations = JsonSerializer.Deserialize<List<MitigationSummary>>(json, _jsonOptions) ?? new List<MitigationSummary>();
            
            // Update overdue status
            foreach (var mitigation in mitigations)
            {
                mitigation.IsOverdue = mitigation.DueDate < DateTime.UtcNow && mitigation.Status != "Completed";
            }

            return mitigations;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deserializing mitigations: {ex.Message}");
            return new List<MitigationSummary>();
        }
    }

    private async Task SaveMitigationsAsync(List<MitigationSummary> mitigations)
    {
        try
        {
            Directory.CreateDirectory(_dataDirectory);
            var json = JsonSerializer.Serialize(mitigations, _jsonOptions);
            await global::System.IO.File.WriteAllTextAsync(_mitigationsFile, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving mitigations: {ex.Message}");
            throw;
        }
    }

    private async Task UpdateMitigationAsync(UpdateMitigationViewModel update)
    {
        var mitigations = await LoadMitigationsFromFileAsync();
        var mitigation = mitigations.FirstOrDefault(m => m.Id == update.MitigationId);
        
        if (mitigation != null)
        {
            mitigation.Status = update.Status;
            mitigation.Progress = update.Progress;
            mitigation.EffectivenessRating = update.EffectivenessRating;
            
            if (update.Status == "Completed" && mitigation.CompletedDate == null)
            {
                mitigation.CompletedDate = DateTime.UtcNow;
                mitigation.Progress = 100;
            }

            await SaveMitigationsAsync(mitigations);
        }
        else
        {
            throw new InvalidOperationException($"Mitigation {update.MitigationId} not found");
        }
    }

    public async Task<string> CreateMitigationFromAssessmentAsync(string hazardId, string riskAssessmentId, string riskLevel)
    {
        var mitigationId = _idGenerationService.GenerateMitigationStrategyId();
        var mitigations = await LoadMitigationsFromFileAsync();

        var mitigationType = DetermineMitigationType(riskLevel);
        var dueDate = CalculateDueDate(riskLevel);
        var responsibleParty = DetermineResponsibleParty(riskLevel);

        var newMitigation = new MitigationSummary
        {
            Id = mitigationId,
            Title = $"Risk Mitigation for {hazardId}",
            RelatedHazard = hazardId,
            RelatedAssessment = riskAssessmentId,
            ResponsiblePerson = responsibleParty,
            DueDate = dueDate,
            Status = riskLevel.ToLowerInvariant() == "low" ? "Monitoring" : "In Progress",
            Progress = riskLevel.ToLowerInvariant() == "low" ? 100 : 0, // Monitoring is considered 100% for tracking
            EffectivenessRating = "TBD",
            IsOverdue = false,
            CreatedDate = DateTime.UtcNow,
            MitigationType = mitigationType,
            RiskLevel = riskLevel
        };

        mitigations.Add(newMitigation);
        await SaveMitigationsAsync(mitigations);

        return mitigationId;
    }

    private string DetermineMitigationType(string riskLevel)
    {
        return riskLevel.ToLowerInvariant() switch
        {
            "critical" => "Emergency Response",
            "high" => "Active Mitigation",
            "medium" => "Planned Mitigation",
            "low" => "Monitoring",
            _ => "Monitoring"
        };
    }

    private DateTime CalculateDueDate(string riskLevel)
    {
        return riskLevel.ToLowerInvariant() switch
        {
            "critical" => DateTime.UtcNow.AddDays(1),
            "high" => DateTime.UtcNow.AddDays(7),
            "medium" => DateTime.UtcNow.AddDays(30),
            "low" => DateTime.UtcNow.AddDays(90),
            _ => DateTime.UtcNow.AddDays(30)
        };
    }

    private string DetermineResponsibleParty(string riskLevel)
    {
        return riskLevel.ToLowerInvariant() switch
        {
            "critical" => "Emergency Response Team",
            "high" => "Operations Management",
            "medium" => "Safety Team",
            "low" => "Safety Monitoring Team",
            _ => "Safety Team"
        };
    }
}

public class MitigationSummary
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string RelatedHazard { get; set; } = string.Empty;
    public string? RelatedAssessment { get; set; } // NEW: Link to risk assessment
    public string ResponsiblePerson { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Progress { get; set; }
    public string EffectivenessRating { get; set; } = string.Empty;
    public bool IsOverdue { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string? MitigationType { get; set; } // NEW: Type of mitigation
    public string? RiskLevel { get; set; } // NEW: Original risk level
}

public class UpdateMitigationViewModel
{
    public string MitigationId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Progress { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string EffectivenessRating { get; set; } = string.Empty;
}
