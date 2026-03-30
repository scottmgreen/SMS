//-----------------------------------------------------------------------
// <copyright file="SMSAuditActivitySummary.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain model representing audit activity summaries for dashboard and reporting.
//                  Used for tracking and displaying recent audit-related activities.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// SMS Audit Activity Summary for dashboard
/// Represents a single audit-related activity or event
/// </summary>
public class SMSAuditActivitySummary
{
    /// <summary>
    /// Type of activity (Started, Completed, Finding Added, Evidence Collected, etc.)
    /// </summary>
    public string ActivityType { get; set; } = string.Empty;

    /// <summary>
    /// Audit code associated with the activity
    /// </summary>
    public string AuditCode { get; set; } = string.Empty;

    /// <summary>
    /// Name or title of the audit
    /// </summary>
    public string AuditName { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the activity
    /// </summary>
    public string ActivityDescription { get; set; } = string.Empty;

    /// <summary>
    /// Date and time when the activity occurred
    /// </summary>
    public DateTime ActivityDate { get; set; }

    /// <summary>
    /// Person responsible for or who performed the activity
    /// </summary>
    public string ActivityBy { get; set; } = string.Empty;

    /// <summary>
    /// Current status related to the activity
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Priority level of the activity (High, Medium, Low)
    /// </summary>
    public string Priority { get; set; } = string.Empty;

    /// <summary>
    /// Department related to the activity
    /// </summary>
    public string Department { get; set; } = string.Empty;
}