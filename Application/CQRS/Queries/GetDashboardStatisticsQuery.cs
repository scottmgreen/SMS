//-----------------------------------------------------------------------
// <copyright file="GetDashboardStatisticsQuery.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application layer component providing functionality for the SMS safety management system.
//                  Provides shared utilities, constants, and base classes
//                  for Application layer components.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Queries;

/// <summary>
/// Get comprehensive dashboard statistics query
/// </summary>
public record GetDashboardStatisticsQuery() : IRequest<Result<DashboardStatisticsResponse>>;

/// <summary>
/// Dashboard statistics response with counts by status
/// </summary>
public class DashboardStatisticsResponse
{
    // Report statistics
    public int TotalReports { get; set; }
    public Dictionary<string, int> ReportsByStatus { get; set; } = new();
    public Dictionary<string, int> ReportsByMonth { get; set; } = new();

    // Hazard statistics
    public int TotalHazards { get; set; }
    public Dictionary<string, int> HazardsByStatus { get; set; } = new();
    public Dictionary<string, int> HazardsByType { get; set; } = new();

    // Risk Assessment statistics
    public int TotalRiskAssessments { get; set; }
    public Dictionary<string, int> RiskAssessmentsByStatus { get; set; } = new();
    public Dictionary<string, int> RiskAssessmentsByType { get; set; } = new();

    // Investigation statistics
    public int TotalInvestigations { get; set; }
    public Dictionary<string, int> InvestigationsByStatus { get; set; } = new();

    // Interview statistics (part of investigations)
    public int TotalInterviews { get; set; }
    public Dictionary<string, int> InterviewsByStatus { get; set; } = new();

    // Mitigation statistics
    public int TotalMitigations { get; set; }
    public Dictionary<string, int> MitigationsByStatus { get; set; } = new();
    public Dictionary<string, int> MitigationsByType { get; set; } = new();

    // Time-based analytics
    public double AverageProcessingDays { get; set; }
    public int ItemsCompletedThisMonth { get; set; }
    public int ItemsOverdue { get; set; }

    // Recent activity
    public List<DashboardActivityItem> RecentActivity { get; set; } = new();
}

/// <summary>
/// Dashboard activity item
/// </summary>
public class DashboardActivityItem
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Report, Hazard, Investigation, etc.
    public string Action { get; set; } = string.Empty; // Created, Updated, Completed, etc.
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string NavigationUrl { get; set; } = string.Empty;
}
