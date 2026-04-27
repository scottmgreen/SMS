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
/// </summary>
public class MitigationCompletedEvent : BaseDomainEvent
{
    public override string EventType => "Mitigation.Completed";

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
}