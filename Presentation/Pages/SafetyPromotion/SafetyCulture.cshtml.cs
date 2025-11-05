using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PDXSMS_Presentation.Pages.SafetyPromotion;

/// <summary>
/// Safety Culture - Assess, monitor, and promote positive safety culture across the organization
/// </summary>
public class SafetyCultureModel : PageModel
{
    public CultureDashboard Dashboard { get; set; } = new();
    public CultureAssessment CurrentAssessment { get; set; } = new();
    public CultureInitiative[] Initiatives { get; set; } = Array.Empty<CultureInitiative>();
    public CultureMetric[] Metrics { get; set; } = Array.Empty<CultureMetric>();
    public CultureFeedback[] RecentFeedback { get; set; } = Array.Empty<CultureFeedback>();

    public void OnGet()
    {
        ViewData["Title"] = "Safety Culture - Safety Promotion";
        LoadCultureData();
    }

    private void LoadCultureData()
    {
        Dashboard = new CultureDashboard
        {
            OverallCultureScore = 4.2,
            ParticipationRate = 78.5,
            PositiveTrend = 12.3,
            InitiativesActive = 8,
            SurveyResponseRate = 85.2,
            ReportingCulture = 4.1
        };

        CurrentAssessment = new CultureAssessment
        {
            AssessmentPeriod = "Q2 2024",
            StartDate = DateTime.Now.AddDays(-90),
            EndDate = DateTime.Now.AddDays(-1),
            ResponseCount = 209,
            CompletionRate = 85.2,
            Status = "Completed",
            KeyFindings = new[]
            {
                "Strong leadership commitment to safety demonstrated across all levels",
                "Increased willingness to report safety concerns and near misses",
                "Positive trend in cross-departmental safety collaboration",
                "Opportunity for improvement in safety training participation",
                "Enhanced trust in management's response to safety issues"
            }
        };

        Initiatives = new[]
        {
            new CultureInitiative
            {
                Id = "CULT001",
                Title = "Safety Leadership Development Program",
                Category = "Leadership",
                Status = "Active",
                Progress = 65,
                StartDate = DateTime.Now.AddDays(-45),
                TargetDate = DateTime.Now.AddDays(30),
                Participants = 24,
                Description = "Developing safety leadership capabilities across management levels"
            },
            new CultureInitiative
            {
                Id = "CULT002",
                Title = "Peer Safety Mentorship Network",
                Category = "Engagement",
                Status = "Active", 
                Progress = 80,
                StartDate = DateTime.Now.AddDays(-60),
                TargetDate = DateTime.Now.AddDays(15),
                Participants = 156,
                Description = "Establishing peer-to-peer safety mentoring relationships"
            },
            new CultureInitiative
            {
                Id = "CULT003",
                Title = "Safety Recognition and Rewards Program",
                Category = "Recognition",
                Status = "Planning",
                Progress = 25,
                StartDate = DateTime.Now.AddDays(14),
                TargetDate = DateTime.Now.AddDays(90),
                Participants = 0,
                Description = "Implementing comprehensive safety recognition system"
            },
            new CultureInitiative
            {
                Id = "CULT004",
                Title = "Open Communication Channels",
                Category = "Communication",
                Status = "Active",
                Progress = 90,
                StartDate = DateTime.Now.AddDays(-120),
                TargetDate = DateTime.Now.AddDays(-30),
                Participants = 245,
                Description = "Enhanced communication pathways for safety concerns"
            }
        };

        Metrics = new[]
        {
            new CultureMetric
            {
                Name = "Voluntary Reporting Rate",
                CurrentValue = 4.2,
                PreviousValue = 3.8,
                Target = 4.5,
                Unit = "reports per employee",
                Trend = "Increasing",
                LastUpdated = DateTime.Now.AddDays(-7)
            },
            new CultureMetric
            {
                Name = "Safety Participation Score",
                CurrentValue = 78.5,
                PreviousValue = 72.1,
                Target = 85.0,
                Unit = "percentage",
                Trend = "Increasing",
                LastUpdated = DateTime.Now.AddDays(-7)
            },
            new CultureMetric
            {
                Name = "Trust in Management",
                CurrentValue = 4.1,
                PreviousValue = 3.9,
                Target = 4.3,
                Unit = "scale 1-5",
                Trend = "Increasing", 
                LastUpdated = DateTime.Now.AddDays(-30)
            },
            new CultureMetric
            {
                Name = "Safety Communication Effectiveness",
                CurrentValue = 3.8,
                PreviousValue = 3.7,
                Target = 4.2,
                Unit = "scale 1-5",
                Trend = "Stable",
                LastUpdated = DateTime.Now.AddDays(-30)
            }
        };

        RecentFeedback = new[]
        {
            new CultureFeedback
            {
                Source = "Anonymous Survey",
                Category = "Positive",
                Feedback = "Management's response to safety concerns has improved significantly. We feel heard and valued.",
                Date = DateTime.Now.AddDays(-2),
                Department = "Operations"
            },
            new CultureFeedback
            {
                Source = "Safety Committee",
                Category = "Suggestion",
                Feedback = "Consider implementing safety moments at the start of all meetings across departments.",
                Date = DateTime.Now.AddDays(-5),
                Department = "Safety"
            },
            new CultureFeedback
            {
                Source = "Employee Focus Group",
                Category = "Concern",
                Feedback = "Need more visible safety leadership presence on the ground during shift changes.",
                Date = DateTime.Now.AddDays(-8),
                Department = "Ground Services"
            },
            new CultureFeedback
            {
                Source = "Exit Interview",
                Category = "Positive",
                Feedback = "PDX has one of the strongest safety cultures I've experienced in aviation.",
                Date = DateTime.Now.AddDays(-12),
                Department = "Maintenance"
            }
        };
    }
}

public record CultureDashboard
{
    public double OverallCultureScore { get; init; }
    public double ParticipationRate { get; init; }
    public double PositiveTrend { get; init; }
    public int InitiativesActive { get; init; }
    public double SurveyResponseRate { get; init; }
    public double ReportingCulture { get; init; }
}

public record CultureAssessment
{
    public string AssessmentPeriod { get; init; } = "";
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public int ResponseCount { get; init; }
    public double CompletionRate { get; init; }
    public string Status { get; init; } = "";
    public string[] KeyFindings { get; init; } = Array.Empty<string>();
}

public record CultureInitiative
{
    public string Id { get; init; } = "";
    public string Title { get; init; } = "";
    public string Category { get; init; } = "";
    public string Status { get; init; } = "";
    public int Progress { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime TargetDate { get; init; }
    public int Participants { get; init; }
    public string Description { get; init; } = "";
}

public record CultureMetric
{
    public string Name { get; init; } = "";
    public double CurrentValue { get; init; }
    public double PreviousValue { get; init; }
    public double Target { get; init; }
    public string Unit { get; init; } = "";
    public string Trend { get; init; } = "";
    public DateTime LastUpdated { get; init; }
}

public record CultureFeedback
{
    public string Source { get; init; } = "";
    public string Category { get; init; } = "";
    public string Feedback { get; init; } = "";
    public DateTime Date { get; init; }
    public string Department { get; init; } = "";
}
