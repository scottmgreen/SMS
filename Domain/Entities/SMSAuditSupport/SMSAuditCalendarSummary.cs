//-----------------------------------------------------------------------
// <copyright file="SMSAuditCalendarSummary.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain model representing summary statistics for SMS audit calendar data.
//                  Provides aggregate information for dashboard and reporting views.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// SMS Audit Calendar Summary statistics
/// Provides aggregated data for dashboard and reporting purposes
/// </summary>
public class SMSAuditCalendarSummary
{
    /// <summary>
    /// Total number of scheduled audits in the time period
    /// </summary>
    public int TotalScheduledAudits { get; set; }

    /// <summary>
    /// Number of audits scheduled for this month
    /// </summary>
    public int AuditsThisMonth { get; set; }

    /// <summary>
    /// Number of audits currently in progress
    /// </summary>
    public int AuditsInProgress { get; set; }

    /// <summary>
    /// Number of overdue audits
    /// </summary>
    public int OverdueAudits { get; set; }

    /// <summary>
    /// Number of audits completed this month
    /// </summary>
    public int CompletedThisMonth { get; set; }

    /// <summary>
    /// Breakdown of audits by type
    /// </summary>
    public Dictionary<string, int> AuditsByType { get; set; } = new();

    /// <summary>
    /// Breakdown of audits by department
    /// </summary>
    public Dictionary<string, int> AuditsByDepartment { get; set; } = new();

    /// <summary>
    /// Breakdown of audits by current status
    /// </summary>
    public Dictionary<string, int> AuditsByStatus { get; set; } = new();
}