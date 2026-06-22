// <copyright file="ReportCreatedEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain event triggered when a new report is created in the SMS system.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Domain.Events;

/// <summary>
/// Domain event triggered when a new report is created
/// </summary>
public class ReportCreatedEvent : BaseDomainEvent
{
    public override string EventType => SMS_Domain.Enums.EventType.ReportCreated.Value;
    public string EventCategory => SMS_Domain.Enums.EventCategory.DomainEvent.Value;

    // Example properties (expand as needed)
    public string ReportId { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = "SYSTEM";
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public ReportCreatedEvent(SMSEventID id) : base(id)
    {
    }

    public ReportCreatedEvent(SMSEventID id,string reportId, string createdBy, DateTime createdDate) : base(id)
    {
        ReportId = reportId ?? throw new ArgumentNullException(nameof(reportId));
        CreatedBy = createdBy ?? "SYSTEM";
        CreatedDate = createdDate;
    }
}
