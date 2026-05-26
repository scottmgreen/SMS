// <copyright file="HazardUpdatedEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain event triggered when a hazard is updated in the SMS system.
// </copyright>
//-----------------------------------------------------------------------


using SMS_Domain.Entities;

namespace SMS_Domain.Events;

/// <summary>
/// Domain event triggered when a hazard is updated
/// </summary>
public class HazardUpdatedEvent : BaseDomainEvent
{
    public override string EventType => SMS_Domain.Enums.EventType.HazardUpdated.Value;
    public string EventCategory => SMS_Domain.Enums.EventCategogy.DomainEvent.Value;

    public string EventId { get; set; }
    public string HazardId { get; set; }
    
    public HazardUpdatedEvent(SMSEventID id):base(id)
    {
        
    }
}
