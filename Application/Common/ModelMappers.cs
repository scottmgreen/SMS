using SMS_Application.Messaging.Queries;
using Domain.Models;

namespace SMS_Application.Common;

/// <summary>
/// Mappers between Domain Models and Application Query Models
/// Ensures clean separation between layers while providing conversion
/// </summary>
public static class ModelMappers
{
    /// <summary>
    /// Maps Domain SMSAuditExecutionDashboard to Application SMSAuditExecutionDashboard
    /// </summary>
    public static SMS_Application.Messaging.Queries.SMSAuditExecutionDashboard ToApplicationModel(this Domain.Models.SMSAuditExecutionDashboard domainModel)
    {
        if (domainModel == null) 
            return new SMS_Application.Messaging.Queries.SMSAuditExecutionDashboard();

        return new SMS_Application.Messaging.Queries.SMSAuditExecutionDashboard
        {
            // Map equivalent properties with different names
            TotalAudits = domainModel.TotalAuditsScheduled,
            ScheduledAudits = domainModel.TotalAuditPlans,
            InProgressAudits = domainModel.AuditsInProgress,
            CompletedAudits = domainModel.AuditsCompleted,
            OverdueAudits = domainModel.AuditsOverdue,
            CancelledAudits = domainModel.AuditsCancelled,
            
            // Findings Summary - direct mapping
            TotalFindings = domainModel.TotalFindings,
            CriticalFindings = domainModel.CriticalFindings,
            MajorFindings = domainModel.MajorFindings,
            MinorFindings = domainModel.MinorFindings,
            Observations = domainModel.Observations,
            
            // Performance Metrics - convert types as needed
            AverageAuditDuration = (decimal)domainModel.AverageAuditDurationHours,
            OnTimeCompletionRate = (decimal)domainModel.OnTimeCompletionRate,
            FindingClosureRate = domainModel.FindingsResolved > 0 ? 
                (decimal)(domainModel.FindingsResolved / (double)domainModel.TotalFindings * 100) : 0,
            
            // Breakdowns - direct mapping
            AuditsByType = domainModel.AuditsByType ?? new(),
            AuditsByDepartment = domainModel.AuditsByDepartment ?? new(),
            AuditsByAuditor = new(), // Not in domain model, initialize empty
            FindingsByType = new(), // Not in domain model, initialize empty
            
            // Activity data - map from domain model collections
            RecentActivities = domainModel.RecentActivities?.Select(ra => new SMSAuditActivitySummary
            {
                ActivityType = ra.ActivityType,
                AuditCode = ra.AuditCode,
                AuditName = ra.Description, // Approximate mapping
                ActivityDescription = ra.Description,
                ActivityDate = ra.ActivityDate,
                ActivityBy = ra.ResponsiblePerson
            }).ToList() ?? new(),
            
            OverdueItems = new(), // Would need mapping if domain model had this
            UpcomingAudits = new() // Would need mapping if domain model had this
        };
    }

    /// <summary>
    /// Maps Domain SMSAuditCalendarData to Application SMSAuditCalendarData
    /// </summary>
    public static SMS_Application.Messaging.Queries.SMSAuditCalendarData ToApplicationModel(this Domain.Models.SMSAuditCalendarData domainModel)
    {
        if (domainModel == null)
            return new SMS_Application.Messaging.Queries.SMSAuditCalendarData();

        // The domain model is a single calendar event, but the application model expects collections
        // This suggests the domain model might need to be restructured, but for now we'll adapt
        var calendarEntry = new SMSAuditCalendarEntry
        {
            AuditCode = domainModel.AuditCode ?? string.Empty,
            AuditPlanCode = domainModel.AuditPlanCode ?? string.Empty,
            Title = domainModel.Title,
            Description = domainModel.Description,
            AuditType = domainModel.EventType, // Map event type to audit type
            Status = domainModel.Status,
            StartDate = domainModel.StartDate,
            EndDate = domainModel.EndDate,
            LeadAuditor = domainModel.LeadAuditor,
            Department = domainModel.ResponsibleDepartment,
            Location = domainModel.Location ?? string.Empty,
            ProgressPercentage = 0, // Not in domain model
            Priority = domainModel.Priority
        };

        return new SMS_Application.Messaging.Queries.SMSAuditCalendarData
        {
            // For now, categorize based on status - this logic may need refinement
            ScheduledAudits = domainModel.Status == "Scheduled" ? new List<SMSAuditCalendarEntry> { calendarEntry } : new(),
            InProgressAudits = domainModel.Status == "In Progress" ? new List<SMSAuditCalendarEntry> { calendarEntry } : new(),
            CompletedAudits = domainModel.Status == "Completed" ? new List<SMSAuditCalendarEntry> { calendarEntry } : new(),
            OverdueAudits = domainModel.Status == "Overdue" ? new List<SMSAuditCalendarEntry> { calendarEntry } : new(),
            Summary = new SMSAuditCalendarSummary
            {
                TotalScheduledAudits = domainModel.Status == "Scheduled" ? 1 : 0,
                AuditsThisMonth = 1, // Simplified
                AuditsInProgress = domainModel.Status == "In Progress" ? 1 : 0,
                OverdueAudits = domainModel.Status == "Overdue" ? 1 : 0,
                CompletedThisMonth = domainModel.Status == "Completed" ? 1 : 0,
                AuditsByType = new() { { domainModel.EventType, 1 } },
                AuditsByDepartment = new() { { domainModel.ResponsibleDepartment, 1 } },
                AuditsByStatus = new() { { domainModel.Status, 1 } }
            }
        };
    }
}