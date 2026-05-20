// <copyright file="MitigationStatusChangedEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain event triggered when mitigation status is changed in the SMS system.
// </copyright>
//-----------------------------------------------------------------------

using Domain.Enums;

using SMS_Domain.Common;
using SMS_Domain.Entities;
using SMS_Domain.Enums;

namespace SMS_Domain.Events;

/// <summary>
/// Domain event triggered when mitigation status is changed
/// </summary>
public class MitigationStatusChangedEvent : BaseDomainEvent
{
    public override string EventType => Domain.Enums.EventType.MitigationStatusChanged.Value;
    public string EventCategory => Domain.Enums.EventCategogy.DomainEvent.Value;

    public string MitigationId { get; private set; }
    public string Status { get; private set; }
    public string ChangedBy { get; private set; }
    public DateTime ChangedDate { get; private set; }

    public MitigationStatusChangedEvent(SMSEventID id,string mitigationId, string status, string changedBy, DateTime changedDate) : base(id)
    {
        MitigationId = mitigationId ?? throw new ArgumentNullException(nameof(mitigationId));
        Status = status ?? "";
        ChangedBy = changedBy ?? "SYSTEM";
        ChangedDate = changedDate;
    }
}
