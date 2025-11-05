using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PDXSMS_Presentation.Pages.SafetyPromotion;

/// <summary>
/// Safety Training - Manage training programs, track completion, and ensure competency
/// </summary>
public class SafetyTrainingModel : PageModel
{
    public TrainingDashboard Dashboard { get; set; } = new();
    public TrainingProgram[] Programs { get; set; } = Array.Empty<TrainingProgram>();
    public TrainingRecord[] RecentCompletions { get; set; } = Array.Empty<TrainingRecord>();
    public TrainingRequirement[] UpcomingRequirements { get; set; } = Array.Empty<TrainingRequirement>();

    public void OnGet()
    {
        ViewData["Title"] = "Safety Training - Safety Promotion";
        LoadTrainingData();
    }

    private void LoadTrainingData()
    {
        Dashboard = new TrainingDashboard
        {
            TotalEmployees = 245,
            TrainingComplianceRate = 94.2,
            ActivePrograms = 12,
            CompletedThisMonth = 28,
            OverdueTraining = 8,
            CertificationsExpiring = 5
        };

        Programs = new[]
        {
            new TrainingProgram
            {
                Id = "TRN001",
                Title = "SMS Overview and Fundamentals",
                Category = "Foundational",
                Duration = "4 hours",
                Status = "Active",
                EnrolledCount = 145,
                CompletionRate = 96.5,
                NextSession = DateTime.Now.AddDays(7),
                Description = "Introduction to Safety Management System principles and PDX implementation"
            },
            new TrainingProgram
            {
                Id = "TRN002", 
                Title = "Hazard Identification and Risk Assessment",
                Category = "Risk Management",
                Duration = "6 hours",
                Status = "Active",
                EnrolledCount = 89,
                CompletionRate = 92.1,
                NextSession = DateTime.Now.AddDays(14),
                Description = "Advanced training on systematic hazard identification and risk assessment methodologies"
            },
            new TrainingProgram
            {
                Id = "TRN003",
                Title = "Safety Audit and Investigation Techniques", 
                Category = "Assurance",
                Duration = "8 hours",
                Status = "Active",
                EnrolledCount = 34,
                CompletionRate = 88.2,
                NextSession = DateTime.Now.AddDays(21),
                Description = "Comprehensive training for safety auditors and investigation team members"
            },
            new TrainingProgram
            {
                Id = "TRN004",
                Title = "Emergency Response Procedures",
                Category = "Emergency",
                Duration = "3 hours", 
                Status = "Active",
                EnrolledCount = 245,
                CompletionRate = 98.8,
                NextSession = DateTime.Now.AddDays(3),
                Description = "Critical emergency response procedures and coordination protocols"
            }
        };

        RecentCompletions = new[]
        {
            new TrainingRecord
            {
                EmployeeName = "Jennifer Wu",
                ProgramTitle = "SMS Overview and Fundamentals",
                CompletionDate = DateTime.Now.AddDays(-2),
                Score = 95,
                Status = "Passed",
                CertificationExpiry = DateTime.Now.AddYears(2)
            },
            new TrainingRecord
            {
                EmployeeName = "Mike Chen",
                ProgramTitle = "Hazard Identification and Risk Assessment", 
                CompletionDate = DateTime.Now.AddDays(-1),
                Score = 89,
                Status = "Passed",
                CertificationExpiry = DateTime.Now.AddYears(1)
            },
            new TrainingRecord
            {
                EmployeeName = "Sarah Johnson",
                ProgramTitle = "Emergency Response Procedures",
                CompletionDate = DateTime.Now.AddDays(-3),
                Score = 98,
                Status = "Passed",
                CertificationExpiry = DateTime.Now.AddYears(1)
            }
        };

        UpcomingRequirements = new[]
        {
            new TrainingRequirement
            {
                EmployeeName = "Robert Kim",
                ProgramTitle = "SMS Overview and Fundamentals",
                DueDate = DateTime.Now.AddDays(14),
                Priority = "High",
                Department = "Operations"
            },
            new TrainingRequirement
            {
                EmployeeName = "Lisa Garcia",
                ProgramTitle = "Safety Audit and Investigation Techniques",
                DueDate = DateTime.Now.AddDays(21),
                Priority = "Medium", 
                Department = "Safety"
            },
            new TrainingRequirement
            {
                EmployeeName = "David Park",
                ProgramTitle = "Emergency Response Procedures",
                DueDate = DateTime.Now.AddDays(7),
                Priority = "High",
                Department = "Ground Services"
            }
        };
    }
}

public record TrainingDashboard
{
    public int TotalEmployees { get; init; }
    public double TrainingComplianceRate { get; init; }
    public int ActivePrograms { get; init; }
    public int CompletedThisMonth { get; init; }
    public int OverdueTraining { get; init; }
    public int CertificationsExpiring { get; init; }
}

public record TrainingProgram
{
    public string Id { get; init; } = "";
    public string Title { get; init; } = "";
    public string Category { get; init; } = "";
    public string Duration { get; init; } = "";
    public string Status { get; init; } = "";
    public int EnrolledCount { get; init; }
    public double CompletionRate { get; init; }
    public DateTime NextSession { get; init; }
    public string Description { get; init; } = "";
}

public record TrainingRecord
{
    public string EmployeeName { get; init; } = "";
    public string ProgramTitle { get; init; } = "";
    public DateTime CompletionDate { get; init; }
    public int Score { get; init; }
    public string Status { get; init; } = "";
    public DateTime CertificationExpiry { get; init; }
}

public record TrainingRequirement
{
    public string EmployeeName { get; init; } = "";
    public string ProgramTitle { get; init; } = "";
    public DateTime DueDate { get; init; }
    public string Priority { get; init; } = "";
    public string Department { get; init; } = "";
}
