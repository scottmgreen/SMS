//-----------------------------------------------------------------------
// <copyright file="SMSAuditUpcomingItem.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain model representing upcoming audit-related items for planning and scheduling.
//                  Used for tracking and displaying upcoming audits and related activities.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// SMS Audit Upcoming Item for dashboard
/// Represents audits and activities that are scheduled to occur in the near future
/// </summary>
public class SMSAuditUpcomingItem
{
    /// <summary>
    /// Audit code for the upcoming audit
    /// </summary>
    public string AuditCode { get; set; } = string.Empty;

    /// <summary>
    /// Name or title of the upcoming audit
    /// </summary>
    public string AuditName { get; set; } = string.Empty;

    /// <summary>
    /// Scheduled date and time for the audit
    /// </summary>
    public DateTime ScheduledDate { get; set; }

    /// <summary>
    /// Lead auditor assigned to the audit
    /// </summary>
    public string LeadAuditor { get; set; } = string.Empty;

    /// <summary>
    /// Department to be audited
    /// </summary>
    public string Department { get; set; } = string.Empty;

    /// <summary>
    /// Number of days until the audit is due
    /// </summary>
    public int DaysUntilDue { get; set; }

    /// <summary>
    /// Current status of audit preparation
    /// </summary>
    public string PreparationStatus { get; set; } = string.Empty;

    /// <summary>
    /// Type of audit (Internal, External, Compliance, etc.)
    /// </summary>
    public string AuditType { get; set; } = string.Empty;

    /// <summary>
    /// Priority level of the upcoming audit
    /// </summary>
    public string Priority { get; set; } = string.Empty;

    /// <summary>
    /// Location where the audit will be conducted
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Estimated duration of the audit in hours
    /// </summary>
    public decimal EstimatedDurationHours { get; set; }

    /// <summary>
    /// Percentage of preparation completed
    /// </summary>
    public int PreparationCompletionPercent { get; set; }
}