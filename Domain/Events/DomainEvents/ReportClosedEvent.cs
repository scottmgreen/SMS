// <copyright file="ReportClosedEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain event triggered when a report is closed in the SMS system.
// </copyright>
//-----------------------------------------------------------------------

using Domain.Enums;
using SMS_Domain.Common;
using SMS_Domain.Entities;
using SMS_Domain.Enums;

namespace SMS_Domain.Events;

/// <summary>
/// Domain event triggered when a report is closed
/// </summary>
public class ReportClosedEvent : BaseDomainEvent
{
    public override string EventType => Domain.Enums.EventType.ReportClosed.Value;
    public string EventCategory => Domain.Enums.EventCategogy.DomainEvent.Value;

    public string ReportId { get; private set; }
    public string ClosedBy { get; private set; }
    public DateTime ClosedDate { get; private set; }

    public ReportClosedEvent(SMSEventID id,string reportId, string closedBy, DateTime closedDate) : base(id)
    {
        ReportId = reportId ?? throw new ArgumentNullException(nameof(reportId));
        ClosedBy = closedBy ?? "SYSTEM";
        ClosedDate = closedDate;
    }
}
