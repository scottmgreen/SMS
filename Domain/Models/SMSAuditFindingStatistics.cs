//-----------------------------------------------------------------------
// <copyright file="SMSAuditFindingStatistics.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain model representing statistical data and metrics for SMS smsauditfinding reporting.
//                  Domain model representing complex data structures
//                  for business reporting and analytics.
// </copyright>
//-----------------------------------------------------------------------

namespace Domain.Models;

/// <summary>
/// Statistical data model for SMS Audit Findings analysis and reporting
/// Provides comprehensive metrics on audit findings trends and performance
/// </summary>
public class SMSAuditFindingStatistics
{
    /// <summary>
    /// Date range for the statistics
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date for the statistics
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Total number of findings in the period
    /// </summary>
    public int TotalFindings { get; set; }

    /// <summary>
    /// Number of critical severity findings
    /// </summary>
    public int CriticalFindings { get; set; }

    /// <summary>
    /// Number of major severity findings
    /// </summary>
    public int MajorFindings { get; set; }

    /// <summary>
    /// Number of minor severity findings
    /// </summary>
    public int MinorFindings { get; set; }

    /// <summary>
    /// Number of observations (non-findings)
    /// </summary>
    public int ObservationFindings { get; set; }

    /// <summary>
    /// Findings by status
    /// </summary>
    public int OpenFindings { get; set; }
    public int InProgressFindings { get; set; }
    public int ResolvedFindings { get; set; }
    public int VerifiedFindings { get; set; }
    public int ClosedFindings { get; set; }

    /// <summary>
    /// Findings resolution metrics
    /// </summary>
    public int OverdueFindings { get; set; }
    public double AverageResolutionDays { get; set; }
    public double MedianResolutionDays { get; set; }
    public double ResolutionComplianceRate { get; set; }

    /// <summary>
    /// Findings requiring verification
    /// </summary>
    public int FindingsRequiringVerification { get; set; }
    public int UnverifiedFindings { get; set; }

    /// <summary>
    /// HazardCategory breakdown
    /// </summary>
    public Dictionary<string, int> FindingsByCategory { get; set; } = new();

    /// <summary>
    /// Department breakdown
    /// </summary>
    public Dictionary<string, int> FindingsByDepartment { get; set; } = new();

    /// <summary>
    /// Audit type breakdown
    /// </summary>
    public Dictionary<string, int> FindingsByAuditType { get; set; } = new();

    /// <summary>
    /// Responsible person breakdown (top 10)
    /// </summary>
    public Dictionary<string, int> FindingsByResponsiblePerson { get; set; } = new();

    /// <summary>
    /// Monthly trend data for charts
    /// </summary>
    public List<FindingTrendData> MonthlyTrends { get; set; } = new();

    /// <summary>
    /// Severity distribution over time
    /// </summary>
    public List<SeverityTrendData> SeverityTrends { get; set; } = new();

    /// <summary>
    /// Resolution time analysis
    /// </summary>
    public ResolutionTimeAnalysis ResolutionAnalysis { get; set; } = new();

    /// <summary>
    /// Repeat finding analysis
    /// </summary>
    public RepeatFindingAnalysis RepeatAnalysis { get; set; } = new();
}

/// <summary>
/// Monthly trend data for findings
/// </summary>
public class FindingTrendData
{
    public string Period { get; set; } = string.Empty;
    public int TotalFindings { get; set; }
    public int CriticalFindings { get; set; }
    public int MajorFindings { get; set; }
    public int MinorFindings { get; set; }
    public int ResolvedFindings { get; set; }
    public double ResolutionRate { get; set; }
}

/// <summary>
/// Severity trend analysis
/// </summary>
public class SeverityTrendData
{
    public string Period { get; set; } = string.Empty;
    public Dictionary<string, int> SeverityCounts { get; set; } = new();
    public Dictionary<string, double> SeverityPercentages { get; set; } = new();
}

/// <summary>
/// Resolution time analysis
/// </summary>
public class ResolutionTimeAnalysis
{
    public double AverageResolutionDays { get; set; }
    public double MedianResolutionDays { get; set; }
    public double MinResolutionDays { get; set; }
    public double MaxResolutionDays { get; set; }
    public Dictionary<string, double> ResolutionTimesBySeverity { get; set; } = new();
    public Dictionary<string, double> ResolutionTimesByCategory { get; set; } = new();
    public List<ResolutionTimeDistribution> TimeDistribution { get; set; } = new();
}

/// <summary>
/// Resolution time distribution data
/// </summary>
public class ResolutionTimeDistribution
{
    public string TimeRange { get; set; } = string.Empty; // e.g., "0-7 days", "8-14 days"
    public int Count { get; set; }
    public double Percentage { get; set; }
}

/// <summary>
/// Repeat finding analysis
/// </summary>
public class RepeatFindingAnalysis
{
    public int RepeatFindings { get; set; }
    public int UniqueFindings { get; set; }
    public double RepeatFindingRate { get; set; }
    public List<RepeatFindingDetail> TopRepeatFindings { get; set; } = new();
}

/// <summary>
/// Details of repeat findings
/// </summary>
public class RepeatFindingDetail
{
    public string FindingCategory { get; set; } = string.Empty;
    public string FindingDescription { get; set; } = string.Empty;
    public int OccurrenceCount { get; set; }
    public List<string> AffectedDepartments { get; set; } = new();
    public DateTime FirstOccurrence { get; set; }
    public DateTime LastOccurrence { get; set; }
}
