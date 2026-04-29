//-----------------------------------------------------------------------
// <copyright file="HazardCreatedEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain event triggered when a new hazard is created in the SMS system.
//                  Integrates with existing hazard reporting workflow and triggers SPI calculations,
//                  stakeholder notifications, and workflow coordination.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Common;
using SMS_Domain.Interfaces;
using SMS_Domain.Enums;

namespace SMS_Domain.Events;

/// <summary>
/// Domain event triggered when a new hazard is created
/// Initiates hazard workflow including SPI updates, stakeholder notifications, and escalation checks
/// </summary>
/// <summary>
/// Domain event representing a new hazard being created in the SMS system
/// Triggers notifications, SPI calculations, and workflow initiation
/// IMPLEMENTS: IEventDataSource for automatic SPI data source discovery
/// </summary>
public class HazardCreatedEvent : BaseDomainEvent, IEventDataSource
{
    public override string EventType => "Hazard.Created";

    #region IEventDataSource Implementation

    /// <summary>
    /// Display name for SPI configuration dropdowns
    /// </summary>
    public string DataSourceDisplayName => "Hazard Management";

    /// <summary>
    /// Category for grouping in UI
    /// </summary>
    public string DataSourceCategory => "Safety";

    /// <summary>
    /// Description of data provided for SPI calculations
    /// </summary>
    public string DataSourceDescription => "Provides data points for hazard reporting, safety incident trends, and risk identification metrics";

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

    public string HazardId { get; private set; }
    public string HazardCode { get; private set; }
    public string HazardName { get; private set; }
    public string HazardType { get; private set; }
    public string HazardCategory { get; private set; }
    public string Description { get; private set; }
    public string LocationArea { get; private set; }
    public string ReportCode { get; private set; }
    public string CreatedBy { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public bool IsInitialHazard { get; private set; }
    public SMS_Domain.Enums.HazardPriority Priority { get; private set; }

    // Location information if available
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }

    #endregion

    #region Constructor

    public HazardCreatedEvent(
        string hazardId,
        string hazardCode,
        string hazardName,
        string hazardType,
        string hazardCategory,
        string description,
        string locationArea,
        string reportCode,
        string createdBy,
        DateTime createdDate,
        bool isInitialHazard,
        SMS_Domain.Enums.HazardPriority priority,
        decimal? latitude = null,
        decimal? longitude = null)
    {
        HazardId = hazardId ?? throw new ArgumentNullException(nameof(hazardId));
        HazardCode = hazardCode ?? throw new ArgumentNullException(nameof(hazardCode));
        HazardName = hazardName ?? "";
        HazardType = hazardType ?? "";
        HazardCategory = hazardCategory ?? "";
        Description = description ?? "";
        LocationArea = locationArea ?? "";
        ReportCode = reportCode ?? "";
        CreatedBy = createdBy ?? "SYSTEM";
        CreatedDate = createdDate;
        IsInitialHazard = isInitialHazard;
        Priority = priority;
        Latitude = latitude;
        Longitude = longitude;
    }

    #endregion
}