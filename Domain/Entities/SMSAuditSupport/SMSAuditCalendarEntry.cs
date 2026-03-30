//-----------------------------------------------------------------------
// <copyright file="SMSAuditCalendarEntry.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain model representing individual calendar entries for SMS audit events.
//                  Used for displaying specific audit events in calendar interfaces.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// SMS Audit Calendar Entry for calendar display
/// Represents a single audit event or milestone in the calendar view
/// </summary>
public class SMSAuditCalendarEntry
{
    /// <summary>
    /// Audit code identifier
    /// </summary>
    public string AuditCode { get; set; } = string.Empty;

    /// <summary>
    /// Associated audit plan code
    /// </summary>
    public string AuditPlanCode { get; set; } = string.Empty;

    /// <summary>
    /// Title to display in calendar
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the audit event
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Type of audit (Internal, External, Compliance, etc.)
    /// </summary>
    public string AuditType { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the audit
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Start date and time of the audit
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date and time of the audit
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Lead auditor responsible for this audit
    /// </summary>
    public string LeadAuditor { get; set; } = string.Empty;

    /// <summary>
    /// Department being audited
    /// </summary>
    public string Department { get; set; } = string.Empty;

    /// <summary>
    /// Physical location of the audit
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Progress percentage (0-100)
    /// </summary>
    public int ProgressPercentage { get; set; }

    /// <summary>
    /// Priority level (High, Medium, Low)
    /// </summary>
    public string Priority { get; set; } = string.Empty;
}