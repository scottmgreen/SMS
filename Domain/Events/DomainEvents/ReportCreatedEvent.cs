// <copyright file="ReportCreatedEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain event triggered when a new report is created in the SMS system.
// </copyright>
//-----------------------------------------------------------------------

using Domain.Enums;
using SMS_Domain.Common;
using SMS_Domain.Entities;
using SMS_Domain.Enums;

namespace SMS_Domain.Events;

/// <summary>
/// Domain event triggered when a new report is created
/// </summary>
public class ReportCreatedEvent : BaseDomainEvent
{
    public override string EventType => Domain.Enums.EventType.ReportCreated.Value;
    public string EventCategory => Domain.Enums.EventCategogy.DomainEvent.Value;

    // Example properties (expand as needed)
    public string ReportId { get; private set; }
    public string CreatedBy { get; private set; }
    public DateTime CreatedDate { get; private set; }

    public ReportCreatedEvent(SMSEventID id,string reportId, string createdBy, DateTime createdDate) : base(id)
    {
        ReportId = reportId ?? throw new ArgumentNullException(nameof(reportId));
        CreatedBy = createdBy ?? "SYSTEM";
        CreatedDate = createdDate;
    }
}
