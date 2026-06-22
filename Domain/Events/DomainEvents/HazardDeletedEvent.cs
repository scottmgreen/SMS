// <copyright file="HazardDeletedEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain event triggered when a hazard is deleted in the SMS system.
// </copyright>
//-----------------------------------------------------------------------


using SMS_Domain.Entities;

namespace SMS_Domain.Events;

/// <summary>
/// Domain event triggered when a hazard is deleted
/// </summary>
public class HazardDeletedEvent : BaseDomainEvent, IEventSource
{
    public override string EventType => SMS_Domain.Enums.EventType.HazardDeleted.Value;
    public string EventCategory => SMS_Domain.Enums.EventCategory.DomainEvent.Value;

    public string HazardId { get; set; }
    
    #region IEventDataSource Implementation

    /// <summary>
    /// Display name for SPI configuration dropdowns
    /// </summary>
    public string EventSourceDisplayName => SMS_Domain.Enums.EventType.HazardDeleted;

    /// <summary>
    /// Category for grouping in UI
    /// </summary>
    public string EventSourceCategory => SMS_Domain.Enums.EventCategory.DomainEvent.Value;

    /// <summary>
    /// Description of data provided for SPI calculations
    /// </summary>
    public string EventSourceDescription => "Provides data points for hazard reporting, safety incident trends, and risk identification metrics";

    /// <summary>
    /// This is a primary automatic data source for safety SPIs
    /// </summary>
    public bool IsAutomaticDataSource => true;

    /// <summary>
    /// High priority for safety-related SPIs
    /// </summary>
    public int DisplayPriority => 1;

    #endregion

    public HazardDeletedEvent(SMSEventID id):base(id)
    {
        //HazardId = hazardId ?? throw new ArgumentNullException(nameof(hazardId));
        //UpdatedBy = deletedBy ?? "SYSTEM";
        //UpdatedDate = deletedDate;
    }
}
