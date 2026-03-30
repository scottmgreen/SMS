//-----------------------------------------------------------------------
// <copyright file="SMSAuditCalendarData.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain model representing calendar and scheduling data for SMS planning and coordination.
//                  Contains collections of audit events organized by status and summary information.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// SMS Audit Calendar Data for dashboard and calendar views
/// Aggregate model containing all audit events organized by status with summary statistics
/// </summary>
public class SMSAuditCalendarData
{
    /// <summary>
    /// List of audits that are scheduled but not yet started
    /// </summary>
    public List<SMSAuditCalendarEntry> ScheduledAudits { get; set; } = new();

    /// <summary>
    /// List of audits currently in progress
    /// </summary>
    public List<SMSAuditCalendarEntry> InProgressAudits { get; set; } = new();

    /// <summary>
    /// List of audits that have been completed
    /// </summary>
    public List<SMSAuditCalendarEntry> CompletedAudits { get; set; } = new();

    /// <summary>
    /// List of audits that are overdue
    /// </summary>
    public List<SMSAuditCalendarEntry> OverdueAudits { get; set; } = new();

    /// <summary>
    /// Summary statistics for the calendar data
    /// </summary>
    public SMSAuditCalendarSummary Summary { get; set; } = new();
}
