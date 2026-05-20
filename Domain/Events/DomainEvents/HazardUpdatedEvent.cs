// <copyright file="HazardUpdatedEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain event triggered when a hazard is updated in the SMS system.
// </copyright>
//-----------------------------------------------------------------------

using Domain.Enums;
using SMS_Domain.Common;
using SMS_Domain.Entities;
using SMS_Domain.Enums;

namespace SMS_Domain.Events;

/// <summary>
/// Domain event triggered when a hazard is updated
/// </summary>
public class HazardUpdatedEvent : BaseDomainEvent
{
    public override string EventType => Domain.Enums.EventType.HazardUpdated.Value;
    public string EventCategory => Domain.Enums.EventCategogy.DomainEvent.Value;

    public string EventId { get; set; }
    public string HazardId { get; set; }
    
    public HazardUpdatedEvent(SMSEventID id):base(id)
    {
        
    }
}
