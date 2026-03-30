//-----------------------------------------------------------------------
// <copyright file="SMSAuditOverdueItem.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain model representing overdue audit-related items for tracking and alerting.
//                  Used for dashboard alerts and overdue audit management.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// SMS Audit Overdue Item for dashboard
/// Represents audits and activities that are overdue and require immediate attention
/// </summary>
public class SMSAuditOverdueItem
{
    /// <summary>
    /// Audit code associated with the overdue item
    /// </summary>
    public string AuditCode { get; set; } = string.Empty;

    /// <summary>
    /// Name or title of the audit
    /// </summary>
    public string AuditName { get; set; } = string.Empty;

    /// <summary>
    /// Type of overdue item (Audit, Finding, Action, Evidence, etc.)
    /// </summary>
    public string ItemType { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the overdue item
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Original due date for the item
    /// </summary>
    public DateTime DueDate { get; set; }

    /// <summary>
    /// Number of days the item is overdue
    /// </summary>
    public int DaysOverdue { get; set; }

    /// <summary>
    /// Person responsible for completing the overdue item
    /// </summary>
    public string ResponsiblePerson { get; set; } = string.Empty;

    /// <summary>
    /// Priority level (High, Medium, Low)
    /// </summary>
    public string Priority { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the overdue item
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Department responsible for the item
    /// </summary>
    public string Department { get; set; } = string.Empty;

    /// <summary>
    /// Escalation level based on how overdue the item is
    /// </summary>
    public string EscalationLevel { get; set; } = string.Empty;
}