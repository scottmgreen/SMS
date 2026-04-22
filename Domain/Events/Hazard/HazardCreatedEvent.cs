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

namespace SMS_Domain.Events.Hazard;

/// <summary>
/// Domain event triggered when a new hazard is created
/// Initiates hazard workflow including SPI updates, stakeholder notifications, and escalation checks
/// </summary>
public class HazardCreatedEvent : BaseDomainEvent
{
    public override string EventType => "Hazard.Created";

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
        bool isInitialHazard = true,
        SMS_Domain.Enums.HazardPriority priority = SMS_Domain.Enums.HazardPriority.Medium,
        decimal? latitude = null,
        decimal? longitude = null)
    {
        HazardId = hazardId ?? throw new ArgumentNullException(nameof(hazardId));
        HazardCode = hazardCode ?? throw new ArgumentNullException(nameof(hazardCode));
        HazardName = hazardName ?? throw new ArgumentNullException(nameof(hazardName));
        HazardType = hazardType ?? throw new ArgumentNullException(nameof(hazardType));
        HazardCategory = hazardCategory ?? throw new ArgumentNullException(nameof(hazardCategory));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        LocationArea = locationArea ?? string.Empty;
        ReportCode = reportCode ?? throw new ArgumentNullException(nameof(reportCode));
        CreatedBy = createdBy ?? throw new ArgumentNullException(nameof(createdBy));
        CreatedDate = createdDate;
        IsInitialHazard = isInitialHazard;
        Priority = priority;
        Latitude = latitude;
        Longitude = longitude;

        AggregateId = hazardId;
    }
}