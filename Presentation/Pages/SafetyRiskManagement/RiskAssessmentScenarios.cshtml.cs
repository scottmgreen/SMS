using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PDXSMS_Presentation.Pages.SafetyRiskManagement;

/// <summary>
/// Detailed Risk Assessment Scenarios
/// Demonstrates realistic use cases and step-by-step walkthroughs
/// of the five-step risk assessment process
/// </summary>
public class RiskAssessmentScenariosModel : PageModel
{
    public List<AssessmentScenario> Scenarios { get; set; } = new();
    public AssessmentScenario? SelectedScenario { get; set; }

    public void OnGet(string? scenarioId = null)
    {
        ViewData["Title"] = "Risk Assessment Scenarios - Detailed Use Cases";
        LoadScenarios();
        
        if (!string.IsNullOrEmpty(scenarioId))
        {
            SelectedScenario = Scenarios.FirstOrDefault(s => s.Id == scenarioId);
        }
    }

    private void LoadScenarios()
    {
        Scenarios = new List<AssessmentScenario>
        {
            new AssessmentScenario
            {
                Id = "runway-operations",
                Title = "Runway 10L/28R Operations Risk Assessment",
                Category = "Aircraft Operations",
                Description = "Comprehensive risk assessment of primary runway operations including takeoffs, landings, and taxi operations",
                Complexity = "High",
                EstimatedDuration = "3-4 weeks",
                TeamSize = "6-8 SMEs",
                KeyStakeholders = new[] { "Air Traffic Control", "Airport Operations", "Airlines", "Ground Handling" },
                Step1_SystemDescription = new AssessmentStep
                {
                    StepNumber = 1,
                    StepName = "Describe the System",
                    Description = "Define runway system boundaries using 5M methodology",
                    Activities = new[]
                    {
                        "Mission: Primary runway for commercial aircraft operations, handling 200+ daily movements",
                        "Man: ATC controllers, ground crew, pilots, airport operations staff",
                        "Machine: Runway infrastructure, lighting systems, ground radar, communication equipment",
                        "Media: Weather conditions, visibility, wind patterns, noise restrictions",
                        "Management: FAA regulations, airport SOPs, airline procedures, emergency protocols"
                    },
                    Deliverables = new[] { "System boundaries document", "5M analysis chart", "Stakeholder map" },
                    Duration = "3-5 days"
                },
                Step2_IdentifyHazards = new AssessmentStep
                {
                    StepNumber = 2,
                    StepName = "Identify the Hazards",
                    Description = "Systematic identification of all potential hazards within system boundaries",
                    Activities = new[]
                    {
                        "Runway incursion scenarios (aircraft, vehicles, personnel)",
                        "Weather-related hazards (wind shear, low visibility, ice)",
                        "Equipment failures (lighting, communication, navigation aids)",
                        "Human factors (pilot error, controller workload, fatigue)",
                        "Wildlife strikes and foreign object debris (FOD)"
                    },
                    Deliverables = new[] { "Hazard inventory", "Risk register", "Hazard categorization matrix" },
                    Duration = "5-7 days"
                },
                Step3_AnalyzeRisk = new AssessmentStep
                {
                    StepNumber = 3,
                    StepName = "Analyze the Risk",
                    Description = "Determine likelihood and severity for each identified hazard",
                    Activities = new[]
                    {
                        "Historical data analysis of runway incidents",
                        "Consequence modeling for worst-case scenarios",
                        "Likelihood assessment using frequency data",
                        "SME workshops for qualitative analysis",
                        "Risk pathway mapping"
                    },
                    Deliverables = new[] { "Risk analysis worksheets", "Consequence/likelihood matrices", "Risk pathways diagram" },
                    Duration = "7-10 days"
                },
                Step4_AssessRisk = new AssessmentStep
                {
                    StepNumber = 4,
                    StepName = "Assess the Risk",
                    Description = "Apply risk matrix to determine overall risk levels",
                    Activities = new[]
                    {
                        "Apply 5x5 risk matrix to all identified hazards",
                        "Categorize risks as Very Low, Low, Medium, High, Very High",
                        "Prioritize risks requiring immediate attention",
                        "Document risk tolerance and acceptance criteria",
                        "Validate assessments with operational staff"
                    },
                    Deliverables = new[] { "Risk assessment matrix", "Prioritized risk list", "Risk tolerance document" },
                    Duration = "3-5 days"
                },
                Step5_MitigateRisk = new AssessmentStep
                {
                    StepNumber = 5,
                    StepName = "Mitigate the Risk",
                    Description = "Develop comprehensive risk treatment strategies",
                    Activities = new[]
                    {
                        "Engineering controls (runway design, lighting improvements)",
                        "Administrative controls (procedures, training, restrictions)",
                        "Technology solutions (radar systems, collision avoidance)",
                        "Emergency response planning and coordination",
                        "Monitoring and review mechanisms"
                    },
                    Deliverables = new[] { "Risk treatment plan", "Mitigation action items", "Implementation timeline", "Monitoring plan" },
                    Duration = "5-7 days"
                }
            },
            new AssessmentScenario
            {
                Id = "baggage-system",
                Title = "Automated Baggage Handling System",
                Category = "Ground Operations",
                Description = "Risk assessment of the automated baggage sorting and transport system",
                Complexity = "Medium",
                EstimatedDuration = "2-3 weeks",
                TeamSize = "4-6 SMEs",
                KeyStakeholders = new[] { "Baggage Operations", "IT Systems", "Airlines", "TSA" },
                Step1_SystemDescription = new AssessmentStep
                {
                    StepNumber = 1,
                    StepName = "Describe the System",
                    Description = "Map baggage handling system from check-in to aircraft loading",
                    Activities = new[]
                    {
                        "Mission: Automated sorting and transport of passenger baggage",
                        "Man: Baggage handlers, system operators, maintenance technicians",
                        "Machine: Conveyor systems, sorting equipment, RFID scanners, software",
                        "Media: Terminal environment, weather effects, power systems",
                        "Management: Airline procedures, TSA requirements, maintenance schedules"
                    },
                    Deliverables = new[] { "System flow diagram", "Process mapping", "Interface definitions" },
                    Duration = "2-3 days"
                }
            },
            new AssessmentScenario
            {
                Id = "security-checkpoint",
                Title = "Terminal Security Checkpoint Operations",
                Category = "Passenger Operations",
                Description = "Comprehensive assessment of passenger security screening processes",
                Complexity = "High",
                EstimatedDuration = "4-5 weeks",
                TeamSize = "8-10 SMEs",
                KeyStakeholders = new[] { "TSA", "Airport Operations", "Airlines", "Concessionaires" },
                Step1_SystemDescription = new AssessmentStep
                {
                    StepNumber = 1,
                    StepName = "Describe the System",
                    Description = "Define security checkpoint boundaries and processes",
                    Activities = new[]
                    {
                        "Mission: Screen passengers and carry-on items for security threats",
                        "Man: TSA officers, airport police, airline staff, passengers",
                        "Machine: X-ray machines, metal detectors, explosive detection systems",
                        "Media: Terminal layout, passenger flow patterns, threat environment",
                        "Management: TSA directives, airport security program, emergency procedures"
                    },
                    Deliverables = new[] { "Security process flow", "Checkpoint layout diagram", "Threat assessment" },
                    Duration = "4-5 days"
                }
            }
        };
    }
}

public class AssessmentScenario
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Complexity { get; set; } = string.Empty;
    public string EstimatedDuration { get; set; } = string.Empty;
    public string TeamSize { get; set; } = string.Empty;
    public string[] KeyStakeholders { get; set; } = Array.Empty<string>();
    
    public AssessmentStep? Step1_SystemDescription { get; set; }
    public AssessmentStep? Step2_IdentifyHazards { get; set; }
    public AssessmentStep? Step3_AnalyzeRisk { get; set; }
    public AssessmentStep? Step4_AssessRisk { get; set; }
    public AssessmentStep? Step5_MitigateRisk { get; set; }
}

public class AssessmentStep
{
    public int StepNumber { get; set; }
    public string StepName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] Activities { get; set; } = Array.Empty<string>();
    public string[] Deliverables { get; set; } = Array.Empty<string>();
    public string Duration { get; set; } = string.Empty;
}
