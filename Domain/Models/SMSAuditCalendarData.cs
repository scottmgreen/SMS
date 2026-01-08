using System;

namespace Domain.Models;

/// <summary>
/// Data model for SMS Audit calendar view representation
/// Used for displaying audit plans and audits in calendar interfaces
/// </summary>
public class SMSAuditCalendarData
{
    /// <summary>
    /// Unique identifier for calendar event
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Title to display in calendar
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Description for detailed view
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of audit event (Plan, Audit, Finding Review, etc.)
    /// </summary>
    public string EventType { get; set; } = string.Empty;
    
    /// <summary>
    /// Audit plan code if applicable
    /// </summary>
    public string? AuditPlanCode { get; set; }
    
    /// <summary>
    /// Audit code if applicable
    /// </summary>
    public string? AuditCode { get; set; }
    
    /// <summary>
    /// Start date/time of the event
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// End date/time of the event
    /// </summary>
    public DateTime EndDate { get; set; }
    
    /// <summary>
    /// Current status of the audit/plan
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// Priority level for visual representation
    /// </summary>
    public string Priority { get; set; } = string.Empty;
    
    /// <summary>
    /// Lead auditor responsible
    /// </summary>
    public string LeadAuditor { get; set; } = string.Empty;
    
    /// <summary>
    /// Department being audited
    /// </summary>
    public string ResponsibleDepartment { get; set; } = string.Empty;
    
    /// <summary>
    /// Location where audit will take place
    /// </summary>
    public string? Location { get; set; }
    
    /// <summary>
    /// Color code for calendar display
    /// </summary>
    public string ColorCode { get; set; } = "#007bff";
    
    /// <summary>
    /// Whether this event can be edited
    /// </summary>
    public bool IsEditable { get; set; } = true;
    
    /// <summary>
    /// Whether this is an all-day event
    /// </summary>
    public bool IsAllDay { get; set; } = false;
    
    /// <summary>
    /// Additional metadata for calendar integration
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}