//-----------------------------------------------------------------------
// <copyright file="MitigationCompletedEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain event triggered when a mitigation is completed.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Common;
using SMS_Domain.Interfaces;

namespace SMS_Domain.Events;

/// <summary>
/// Domain event triggered when a mitigation is completed
/// IMPLEMENTS: IEventDataSource for automatic SPI data source discovery
/// </summary>
public class MitigationCompletedEvent : BaseDomainEvent, IEventDataSource
{
    public override string EventType => "Mitigation.Completed";

    #region IEventDataSource Implementation

    /// <summary>
    /// Display name for SPI configuration dropdowns
    /// </summary>
    public string DataSourceDisplayName => "Mitigation Management";

    /// <summary>
    /// Category for grouping in UI
    /// </summary>
    public string DataSourceCategory => "Compliance";

    /// <summary>
    /// Description of data provided for SPI calculations
    /// </summary>
    public string DataSourceDescription => "Provides data points for mitigation completion rates, timeliness metrics, and effectiveness tracking";

    /// <summary>
    /// This is a primary automatic data source for compliance-related SPIs
    /// </summary>
    public bool IsAutomaticDataSource => true;

    /// <summary>
    /// Medium-high priority for compliance-related SPIs
    /// </summary>
    public int DisplayPriority => 3;

    #endregion

    #region Event Properties

    public string MitigationId { get; set; } = string.Empty;
    public string MitigationCode { get; set; } = string.Empty;
    public string HazardId { get; set; } = string.Empty;
    public DateTime TargetCompletionDate { get; set; }
    public DateTime CompletedDate { get; set; }
    public string ReportId { get; set; } = string.Empty;
    public string CompletionNotes { get; set; } = string.Empty;
    public string EffectivenessRating { get; set; } = string.Empty;

    public MitigationCompletedEvent(string mitigationId, string mitigationCode, string hazardId, 
        DateTime targetCompletionDate, DateTime completedDate, string aggregateId)
    {
        MitigationId = mitigationId;
        MitigationCode = mitigationCode;
        HazardId = hazardId;
        TargetCompletionDate = targetCompletionDate;
        CompletedDate = completedDate;
        AggregateId = aggregateId;
    }

    #endregion
}