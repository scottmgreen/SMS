//-----------------------------------------------------------------------
// <copyright file="HazardCreatedEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain event triggered when a new hazard is created in the SMS system.
//                  Integrates with existing hazard reporting workflow and triggers SPI calculations,
//                  stakeholder notifications, and workflow coordination.
// </copyright>
//-----------------------------------------------------------------------


using Microsoft.Extensions.Logging;

using SMS_Domain.Common;
using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Domain.Interfaces;

namespace SMS_Domain.Events;

/// <summary>
/// Domain event triggered when a new hazard is created
/// Initiates hazard workflow including SPI updates, stakeholder notifications, and escalation checks
/// </summary>
/// <summary>
/// Domain event representing a new hazard being created in the SMS system
/// Triggers notifications, SPI calculations, and workflow initiation
/// IMPLEMENTS: IEventSource for automatic SPI data source discovery
/// </summary>
public class HazardCreatedEvent : BaseDomainEvent, IEventSource
{
    public override string EventType => SMS_Domain.Enums.EventType.HazardCreated.Value;

    #region IEventDataSource Implementation

    /// <summary>
    /// Display name for SPI configuration dropdowns
    /// </summary>
    public string EventSourceDisplayName => SMS_Domain.Enums.EventType.HazardCreated;

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

    #region Event Properties
    public string HazardId { get;  set; }
    public string HazardCode { get;  set; }
    public string HazardName { get;  set; }
    public string HazardType { get;  set; }
    public string HazardCategory { get;  set; }
    public string Description { get;  set; }
    public string LocationArea { get;  set; }
    public string ReportCode { get;  set; }
    public string CreatedBy { get;  set; }
    public DateTime CreatedDate { get;  set; }
    public bool IsInitialHazard { get;  set; }
    public SMS_Domain.Enums.HazardPriority Priority { get;  set; }

    // Location information if available
    public decimal? Latitude { get;  set; }
    public decimal? Longitude { get;  set; }

    #endregion

    #region Constructor
    public HazardCreatedEvent(SMSEventID id) : base(id) { }


    #endregion
}