using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PDXSMS_Presentation.Pages.SafetyRiskManagement;

/// <summary>
/// 5M Methodology Visualization
/// Interactive graphical presentation of the 5M human factors model
/// </summary>
public class FiveMMethodologyModel : PageModel
{
    public FiveMModelData ModelData { get; set; } = new();
    public string? SelectedComponent { get; set; }

    public void OnGet(string? component = null)
    {
        ViewData["Title"] = "5M Methodology - Human Factors Framework";
        SelectedComponent = component;
        LoadFiveMModelData();
    }

    private void LoadFiveMModelData()
    {
        ModelData = new FiveMModelData
        {
            Mission = new FiveMComponent
            {
                Id = "mission",
                Name = "Mission",
                Description = "The purpose, objectives, and intended outcomes of the system",
                Color = "#e74c3c", // Red
                Icon = "fas fa-bullseye",
                KeyQuestions = new[]
                {
                    "What is the primary purpose of this system?",
                    "What are the intended outcomes and success criteria?",
                    "What objectives must be achieved?",
                    "What constraints limit mission accomplishment?"
                },
                Examples = new[]
                {
                    "Safe aircraft takeoffs and landings",
                    "Efficient passenger flow through security",
                    "Reliable baggage handling operations",
                    "Emergency response readiness"
                },
                RiskFactors = new[]
                {
                    "Unclear or conflicting objectives",
                    "Unrealistic performance expectations",
                    "Changing mission requirements",
                    "Resource limitations affecting mission"
                }
            },
            Man = new FiveMComponent
            {
                Id = "man",
                Name = "Man (Personnel)",
                Description = "Human resources including roles, responsibilities, and capabilities",
                Color = "#3498db", // Blue
                Icon = "fas fa-users",
                KeyQuestions = new[]
                {
                    "Who are the key personnel involved?",
                    "What are their roles and responsibilities?",
                    "What training and qualifications are required?",
                    "How do human factors affect performance?"
                },
                Examples = new[]
                {
                    "Air traffic controllers managing runway operations",
                    "Ground crew performing aircraft servicing",
                    "Security personnel screening passengers",
                    "Maintenance technicians servicing equipment"
                },
                RiskFactors = new[]
                {
                    "Inadequate training or qualifications",
                    "Fatigue and workload issues",
                    "Communication breakdowns",
                    "Complacency and routine violations"
                }
            },
            Machine = new FiveMComponent
            {
                Id = "machine",
                Name = "Machine",
                Description = "Equipment, technology, and physical infrastructure",
                Color = "#f39c12", // Orange
                Icon = "fas fa-cogs",
                KeyQuestions = new[]
                {
                    "What equipment and technology is involved?",
                    "How reliable and maintainable are the systems?",
                    "What are the failure modes and effects?",
                    "How do humans interact with the technology?"
                },
                Examples = new[]
                {
                    "Runway lighting and navigation systems",
                    "Baggage handling conveyor systems",
                    "Security screening equipment",
                    "Ground support equipment and vehicles"
                },
                RiskFactors = new[]
                {
                    "Equipment failures and malfunctions",
                    "Poor human-machine interface design",
                    "Inadequate maintenance procedures",
                    "Technology limitations and constraints"
                }
            },
            Media = new FiveMComponent
            {
                Id = "media",
                Name = "Media (Environment)",
                Description = "Operating environment and external conditions",
                Color = "#27ae60", // Green
                Icon = "fas fa-globe",
                KeyQuestions = new[]
                {
                    "What environmental conditions affect operations?",
                    "How do external factors influence the system?",
                    "What are the physical workspace constraints?",
                    "How do conditions vary over time?"
                },
                Examples = new[]
                {
                    "Weather conditions affecting runway operations",
                    "Terminal noise levels and lighting",
                    "Airside security restrictions",
                    "Peak traffic periods and congestion"
                },
                RiskFactors = new[]
                {
                    "Adverse weather conditions",
                    "Poor workspace design and ergonomics",
                    "High stress and pressure environments",
                    "Distractions and interruptions"
                }
            },
            Management = new FiveMComponent
            {
                Id = "management",
                Name = "Management",
                Description = "Policies, procedures, and organizational controls",
                Color = "#9b59b6", // Purple
                Icon = "fas fa-sitemap",
                KeyQuestions = new[]
                {
                    "What policies and procedures govern operations?",
                    "How is oversight and supervision provided?",
                    "What are the organizational structures?",
                    "How are decisions made and communicated?"
                },
                Examples = new[]
                {
                    "FAA regulations and airport operating procedures",
                    "Emergency response protocols and training",
                    "Quality control and safety management systems",
                    "Maintenance schedules and compliance monitoring"
                },
                RiskFactors = new[]
                {
                    "Inadequate or unclear procedures",
                    "Poor supervision and oversight",
                    "Conflicting organizational priorities",
                    "Insufficient resources or support"
                }
            }
        };
    }
}

public class FiveMModelData
{
    public FiveMComponent Mission { get; set; } = new();
    public FiveMComponent Man { get; set; } = new();
    public FiveMComponent Machine { get; set; } = new();
    public FiveMComponent Media { get; set; } = new();
    public FiveMComponent Management { get; set; } = new();
}

public class FiveMComponent
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string[] KeyQuestions { get; set; } = Array.Empty<string>();
    public string[] Examples { get; set; } = Array.Empty<string>();
    public string[] RiskFactors { get; set; } = Array.Empty<string>();
}
