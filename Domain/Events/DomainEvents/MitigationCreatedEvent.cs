// <copyright file="MitigationCreatedEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain event triggered when a mitigation is created in the SMS system.
// </copyright>
//-----------------------------------------------------------------------



using SMS_Domain.Entities;

namespace SMS_Domain.Events;

/// <summary>
/// Domain event triggered when a mitigation is created
/// </summary>
public class MitigationCreatedEvent : BaseDomainEvent
{
    public override string EventType => SMS_Domain.Enums.EventType.MitigationCreated.Value;
    public string EventCategory => SMS_Domain.Enums.EventCategory.DomainEvent.Value;

    public string MitigationId { get; private set; }
    public string CreatedBy { get; private set; }
    public DateTime CreatedDate { get; private set; }

    public MitigationCreatedEvent(SMSEventID id,string mitigationId, string createdBy, DateTime createdDate):base(id)
    {
        MitigationId = mitigationId ?? throw new ArgumentNullException(nameof(mitigationId));
        CreatedBy = createdBy ?? string.Empty;
        CreatedDate = createdDate;
    }
}
