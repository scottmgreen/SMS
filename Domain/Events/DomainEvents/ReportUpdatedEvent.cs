// <copyright file="ReportUpdatedEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain event triggered when a report is updated in the SMS system.
// </copyright>
//-----------------------------------------------------------------------

using Domain.Enums;

using SMS_Domain.Common;
using SMS_Domain.Entities;
using SMS_Domain.Enums;

namespace SMS_Domain.Events;

/// <summary>
/// Domain event triggered when a report is updated
/// </summary>
public class ReportUpdatedEvent : BaseDomainEvent
{
    public override string EventType => Domain.Enums.EventType.ReportUpdated.Value;
    public string EventCategory => Domain.Enums.EventCategogy.DomainEvent.Value;

    public string ReportId { get; private set; }
    public string UpdatedBy { get; private set; }
    public DateTime UpdatedDate { get; private set; }

    public ReportUpdatedEvent(SMSEventID id,string reportId, string updatedBy, DateTime updatedDate) : base(id)
    {
        ReportId = reportId ?? throw new ArgumentNullException(nameof(reportId));
        UpdatedBy = updatedBy ?? "SYSTEM";
        UpdatedDate = updatedDate;
    }
}
