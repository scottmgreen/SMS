//-----------------------------------------------------------------------
// <copyright file="SMSAuditExecutionDashboard.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain model representing dashboard data structures for SMS smsauditexecution visualization.
//                  Domain model representing complex data structures
//                  for business reporting and analytics.
// </copyright>
//-----------------------------------------------------------------------

namespace Domain.Models;

/// <summary>
/// Dashboard model for SMS Audit execution tracking and metrics
/// Provides comprehensive overview of audit activities and performance
/// </summary>
public class SMSAuditExecutionDashboard
{
    /// <summary>
    /// Date range for the dashboard data
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date for the dashboard data
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Total number of audit plans in the period
    /// </summary>
    public int TotalAuditPlans { get; set; }

    /// <summary>
    /// Total number of audits scheduled
    /// </summary>
    public int TotalAuditsScheduled { get; set; }

    /// <summary>
    /// Number of audits completed
    /// </summary>
    public int AuditsCompleted { get; set; }

    /// <summary>
    /// Number of audits currently in progress
    /// </summary>
    public int AuditsInProgress { get; set; }

    /// <summary>
    /// Number of audits that are overdue
    /// </summary>
    public int AuditsOverdue { get; set; }

    /// <summary>
    /// Number of audits cancelled
    /// </summary>
    public int AuditsCancelled { get; set; }

    /// <summary>
    /// Total number of findings discovered
    /// </summary>
    public int TotalFindings { get; set; }

    /// <summary>
    /// Number of critical findings
    /// </summary>
    public int CriticalFindings { get; set; }

    /// <summary>
    /// Number of major findings
    /// </summary>
    public int MajorFindings { get; set; }

    /// <summary>
    /// Number of minor findings
    /// </summary>
    public int MinorFindings { get; set; }

    /// <summary>
    /// Number of observations (non-finding items)
    /// </summary>
    public int Observations { get; set; }

    /// <summary>
    /// Number of findings resolved
    /// </summary>
    public int FindingsResolved { get; set; }

    /// <summary>
    /// Number of findings still open
    /// </summary>
    public int FindingsOpen { get; set; }

    /// <summary>
    /// Number of findings overdue for resolution
    /// </summary>
    public int FindingsOverdue { get; set; }

    /// <summary>
    /// Average time to resolve findings (in days)
    /// </summary>
    public double AverageResolutionTimeDays { get; set; }

    /// <summary>
    /// Percentage of audits completed on time
    /// </summary>
    public double OnTimeCompletionRate { get; set; }

    /// <summary>
    /// Average audit duration in hours
    /// </summary>
    public double AverageAuditDurationHours { get; set; }

    /// <summary>
    /// Number of pieces of evidence collected
    /// </summary>
    public int TotalEvidence { get; set; }

    /// <summary>
    /// Number of evidence items verified
    /// </summary>
    public int EvidenceVerified { get; set; }

    /// <summary>
    /// Number of evidence items archived
    /// </summary>
    public int EvidenceArchived { get; set; }

    /// <summary>
    /// Breakdown by audit type
    /// </summary>
    public Dictionary<string, int> AuditsByType { get; set; } = new();

    /// <summary>
    /// Breakdown by department
    /// </summary>
    public Dictionary<string, int> AuditsByDepartment { get; set; } = new();

    /// <summary>
    /// Monthly trend data for charts
    /// </summary>
    public List<AuditTrendData> MonthlyTrends { get; set; } = new();

    /// <summary>
    /// Top performing auditors
    /// </summary>
    public List<AuditorPerformanceData> TopAuditors { get; set; } = new();

    /// <summary>
    /// Recent audit activities
    /// </summary>
    public List<RecentAuditActivity> RecentActivities { get; set; } = new();
}

/// <summary>
/// Trend data for chart visualization
/// </summary>
public class AuditTrendData
{
    public string Period { get; set; } = string.Empty;
    public int PlannedAudits { get; set; }
    public int CompletedAudits { get; set; }
    public int TotalFindings { get; set; }
    public int ResolvedFindings { get; set; }
}

/// <summary>
/// Auditor performance metrics
/// </summary>
public class AuditorPerformanceData
{
    public string AuditorName { get; set; } = string.Empty;
    public int AuditsCompleted { get; set; }
    public int FindingsDiscovered { get; set; }
    public double AverageAuditScore { get; set; }
    public double OnTimeCompletionRate { get; set; }
}

/// <summary>
/// Recent audit activity for dashboard feed
/// </summary>
public class RecentAuditActivity
{
    public DateTime ActivityDate { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AuditCode { get; set; } = string.Empty;
    public string ResponsiblePerson { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
