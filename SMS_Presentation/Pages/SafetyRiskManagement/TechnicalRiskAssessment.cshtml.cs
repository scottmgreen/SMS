using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PDXSMS_Presentation.Pages.SafetyRiskManagement;

/// <summary>
/// Technical Risk Assessment (TRA) - Formal risk assessment for High/Critical hazards
/// Part of SMS methodology for comprehensive risk evaluation
/// </summary>
public class TechnicalRiskAssessmentModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string HazardId { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string AssessmentId { get; set; } = string.Empty;

    public TRADetails TRA { get; set; } = new();
    public TRASection[] TRASections { get; set; } = Array.Empty<TRASection>();
    public TRATeamMember[] TRATeam { get; set; } = Array.Empty<TRATeamMember>();

    [BindProperty]
    public TRASectionUpdate SectionUpdate { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrWhiteSpace(HazardId) || string.IsNullOrWhiteSpace(AssessmentId))
        {
            TempData["ErrorMessage"] = "Hazard ID and Assessment ID are required for Technical Risk Assessment";
            return RedirectToPage("/SafetyRiskManagement/HazardProcessing");
        }

        ViewData["Title"] = "Technical Risk Assessment (TRA)";
        LoadTRAData();
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateSectionAsync()
    {
        try
        {
            // TODO: Implement section update logic
            TempData["SuccessMessage"] = $"TRA section '{SectionUpdate.SectionName}' updated successfully.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error updating TRA section: {ex.Message}";
            LoadTRAData();
            return Page();
        }
    }

    public async Task<IActionResult> OnPostCompleteTRAAsync()
    {
        try
        {
            // TODO: Implement TRA completion logic
            TempData["SuccessMessage"] = "TRA completed and forwarded to SMS Leadership for approval.";
            return RedirectToPage("/SafetyRiskManagement/HazardProcessing");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error completing TRA: {ex.Message}";
            return RedirectToPage();
        }
    }

    private void LoadTRAData()
    {
        TRA = new TRADetails
        {
            Id = $"TRA-2024-{HazardId}",
            HazardId = HazardId,
            Title = GetTRATitle(HazardId),
            Status = "In Progress",
            Priority = "Critical",
            InitiatedDate = DateTime.Now.AddDays(-3),
            TargetCompletionDate = DateTime.Now.AddDays(4),
            LeadAssessor = "Dr. Sarah Chen - Chief Safety Officer",
            OverallProgress = 65,
            RequiredSections = 8,
            CompletedSections = 5
        };

        TRASections = new[]
        {
            new TRASection
            {
                Id = "TRA-001-01",
                Name = "Hazard Description and Context",
                Status = "Completed",
                Progress = 100,
                AssignedTo = "Safety Analyst",
                CompletedDate = DateTime.Now.AddDays(-2),
                Comments = "Comprehensive hazard description with supporting documentation completed."
            },
            new TRASection
            {
                Id = "TRA-001-02", 
                Name = "Threat and Error Analysis",
                Status = "Completed",
                Progress = 100,
                AssignedTo = "Operations Specialist",
                CompletedDate = DateTime.Now.AddDays(-2),
                Comments = "SHELL model analysis identifying human factors and system threats."
            },
            new TRASection
            {
                Id = "TRA-001-03",
                Name = "Failure Modes Analysis",
                Status = "Completed",
                Progress = 100,
                AssignedTo = "Technical Specialist",
                CompletedDate = DateTime.Now.AddDays(-1),
                Comments = "FMEA completed identifying potential failure modes and effects."
            },
            new TRASection
            {
                Id = "TRA-001-04",
                Name = "Probability Assessment",
                Status = "Completed",
                Progress = 100,
                AssignedTo = "Data Analyst",
                CompletedDate = DateTime.Now.AddDays(-1),
                Comments = "Statistical analysis using historical incident data and expert judgment."
            },
            new TRASection
            {
                Id = "TRA-001-05",
                Name = "Consequence Assessment",
                Status = "Completed",
                Progress = 100,
                AssignedTo = "Safety Engineer",
                CompletedDate = DateTime.Now.AddHours(-6),
                Comments = "Comprehensive consequence modeling including worst-case scenarios."
            },
            new TRASection
            {
                Id = "TRA-001-06",
                Name = "Risk Characterization",
                Status = "In Progress",
                Progress = 75,
                AssignedTo = "Risk Analyst",
                Comments = "Integrating probability and consequence assessments into risk matrix."
            },
            new TRASection
            {
                Id = "TRA-001-07",
                Name = "Mitigation Options Analysis",
                Status = "In Progress", 
                Progress = 40,
                AssignedTo = "Engineering Team",
                Comments = "Evaluating various mitigation strategies and their effectiveness."
            },
            new TRASection
            {
                Id = "TRA-001-08",
                Name = "Recommendations and Implementation Plan",
                Status = "Pending",
                Progress = 0,
                AssignedTo = "TRA Team Lead",
                Comments = "Awaiting completion of mitigation analysis."
            }
        };

        TRATeam = new[]
        {
            new TRATeamMember
            {
                Name = "Dr. Sarah Chen",
                Role = "TRA Team Lead / Chief Safety Officer",
                Expertise = "Aviation Safety, Risk Assessment",
                Status = "Active",
                LastActivity = DateTime.Now.AddHours(-2)
            },
            new TRATeamMember
            {
                Name = "Mike Rodriguez",
                Role = "Operations Specialist",
                Expertise = "Airport Operations, Human Factors",
                Status = "Active", 
                LastActivity = DateTime.Now.AddHours(-6)
            },
            new TRATeamMember
            {
                Name = "Jennifer Park",
                Role = "Technical Specialist",
                Expertise = "Systems Engineering, FMEA",
                Status = "Active",
                LastActivity = DateTime.Now.AddHours(-8)
            },
            new TRATeamMember
            {
                Name = "Robert Kim",
                Role = "Data Analyst",
                Expertise = "Statistical Analysis, Modeling",
                Status = "Active",
                LastActivity = DateTime.Now.AddDays(-1)
            }
        };
    }

    private string GetTRATitle(string hazardId)
    {
        return hazardId switch
        {
            "H-2024-156" => "Ground Vehicle Near Miss - Technical Risk Assessment",
            "H-2024-154" => "Jet Bridge Hydraulic Failure - Technical Risk Assessment", 
            "H-2024-001" => "FOD on Runway 10L/28R - Technical Risk Assessment",
            _ => $"Technical Risk Assessment for {hazardId}"
        };
    }
}

public record TRADetails
{
    public string Id { get; init; } = "";
    public string HazardId { get; init; } = "";
    public string Title { get; init; } = "";
    public string Status { get; init; } = "";
    public string Priority { get; init; } = "";
    public DateTime InitiatedDate { get; init; }
    public DateTime TargetCompletionDate { get; init; }
    public string LeadAssessor { get; init; } = "";
    public int OverallProgress { get; init; }
    public int RequiredSections { get; init; }
    public int CompletedSections { get; init; }
}

public record TRASection
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Status { get; init; } = "";
    public int Progress { get; init; }
    public string AssignedTo { get; init; } = "";
    public DateTime? CompletedDate { get; init; }
    public string Comments { get; init; } = "";
}

public record TRATeamMember
{
    public string Name { get; init; } = "";
    public string Role { get; init; } = "";
    public string Expertise { get; init; } = "";
    public string Status { get; init; } = "";
    public DateTime LastActivity { get; init; }
}

public record TRASectionUpdate
{
    public string SectionId { get; set; } = "";
    public string SectionName { get; set; } = "";
    public string Status { get; set; } = "";
    public int Progress { get; set; }
    public string Comments { get; set; } = "";
}

