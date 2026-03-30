//-----------------------------------------------------------------------
// <copyright file="ModelMappers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Shared utility providing common functionality for Application layer components.
//                  Provides shared utilities, constants, and base classes
//                  for Application layer components.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Application.Common;

/// <summary>
/// Mappers between Domain Models and Application Query Models
/// Ensures clean separation between layers while providing conversion
/// </summary>
public static class ModelMappers
{
    /// <summary>
    /// Maps Domain SMSAuditExecutionDashboard to Domain SMSAuditExecutionDashboard
    /// Since both are now in the domain layer, this might not be needed unless there are different variations
    /// </summary>
    public static SMSAuditExecutionDashboard ToApplicationModel(this SMSAuditExecutionDashboard domainModel)
    {
        // Since the classes are now both in the domain layer and have the same structure,
        // we can return the domain model directly or create a copy if needed for immutability
        return domainModel;
    }

    /// <summary>
    /// Maps the single Domain SMSAuditCalendarEvent to the aggregate SMSAuditCalendarData structure
    /// This handles the architectural difference where individual calendar events need to be 
    /// organized into collections for the application layer
    /// </summary>
    public static SMSAuditCalendarData ToApplicationModel(this SMSAuditCalendarEvent domainModel)
    {
        if (domainModel == null)
            return new SMSAuditCalendarData();

        // Convert the single calendar event to a calendar entry
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
            ProgressPercentage = 0, // Not in the single item domain model
            Priority = domainModel.Priority
        };

        // Create the aggregate calendar data structure
        var result = new SMSAuditCalendarData();

        // Categorize the single entry based on its status
        switch (domainModel.Status?.ToLowerInvariant())
        {
            case "scheduled":
                result.ScheduledAudits.Add(calendarEntry);
                break;
            case "in progress":
            case "inprogress":
                result.InProgressAudits.Add(calendarEntry);
                break;
            case "completed":
                result.CompletedAudits.Add(calendarEntry);
                break;
            case "overdue":
                result.OverdueAudits.Add(calendarEntry);
                break;
            default:
                result.ScheduledAudits.Add(calendarEntry); // Default fallback
                break;
        }

        // Create summary data for this single item
        result.Summary = new SMSAuditCalendarSummary
        {
            TotalScheduledAudits = result.ScheduledAudits.Count,
            AuditsInProgress = result.InProgressAudits.Count,
            OverdueAudits = result.OverdueAudits.Count,
            CompletedThisMonth = result.CompletedAudits.Count,
            AuditsThisMonth = 1,
            AuditsByType = new Dictionary<string, int> { { domainModel.EventType, 1 } },
            AuditsByDepartment = new Dictionary<string, int> { { domainModel.ResponsibleDepartment, 1 } },
            AuditsByStatus = new Dictionary<string, int> { { domainModel.Status, 1 } }
        };

        return result;
    }

    /// <summary>
    /// Maps a collection of Domain SMSAuditCalendarEvent to the aggregate SMSAuditCalendarData structure
    /// This is likely what should be used when getting calendar data from multiple sources
    /// </summary>
    public static SMSAuditCalendarData ToApplicationModel(this IEnumerable<SMSAuditCalendarEvent> domainModels)
    {
        if (domainModels == null)
            return new SMSAuditCalendarData();

        var result = new SMSAuditCalendarData();
        var auditsByType = new Dictionary<string, int>();
        var auditsByDepartment = new Dictionary<string, int>();
        var auditsByStatus = new Dictionary<string, int>();

        foreach (var domainModel in domainModels)
        {
            var calendarEntry = new SMSAuditCalendarEntry
            {
                AuditCode = domainModel.AuditCode ?? string.Empty,
                AuditPlanCode = domainModel.AuditPlanCode ?? string.Empty,
                Title = domainModel.Title,
                Description = domainModel.Description,
                AuditType = domainModel.EventType,
                Status = domainModel.Status,
                StartDate = domainModel.StartDate,
                EndDate = domainModel.EndDate,
                LeadAuditor = domainModel.LeadAuditor,
                Department = domainModel.ResponsibleDepartment,
                Location = domainModel.Location ?? string.Empty,
                ProgressPercentage = 0,
                Priority = domainModel.Priority
            };

            // Categorize entries based on status
            switch (domainModel.Status?.ToLowerInvariant())
            {
                case "scheduled":
                    result.ScheduledAudits.Add(calendarEntry);
                    break;
                case "in progress":
                case "inprogress":
                    result.InProgressAudits.Add(calendarEntry);
                    break;
                case "completed":
                    result.CompletedAudits.Add(calendarEntry);
                    break;
                case "overdue":
                    result.OverdueAudits.Add(calendarEntry);
                    break;
                default:
                    result.ScheduledAudits.Add(calendarEntry);
                    break;
            }

            // Build summary dictionaries
            if (!string.IsNullOrEmpty(domainModel.EventType))
            {
                auditsByType[domainModel.EventType] = auditsByType.GetValueOrDefault(domainModel.EventType, 0) + 1;
            }
            
            if (!string.IsNullOrEmpty(domainModel.ResponsibleDepartment))
            {
                auditsByDepartment[domainModel.ResponsibleDepartment] = auditsByDepartment.GetValueOrDefault(domainModel.ResponsibleDepartment, 0) + 1;
            }
            
            if (!string.IsNullOrEmpty(domainModel.Status))
            {
                auditsByStatus[domainModel.Status] = auditsByStatus.GetValueOrDefault(domainModel.Status, 0) + 1;
            }
        }

        // Create summary
        result.Summary = new SMSAuditCalendarSummary
        {
            TotalScheduledAudits = result.ScheduledAudits.Count,
            AuditsInProgress = result.InProgressAudits.Count,
            OverdueAudits = result.OverdueAudits.Count,
            CompletedThisMonth = result.CompletedAudits.Count,
            AuditsThisMonth = domainModels.Count(),
            AuditsByType = auditsByType,
            AuditsByDepartment = auditsByDepartment,
            AuditsByStatus = auditsByStatus
        };

        return result;
    }
}
