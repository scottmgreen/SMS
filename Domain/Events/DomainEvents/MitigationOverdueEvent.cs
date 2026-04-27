//-----------------------------------------------------------------------
// <copyright file="MitigationOverdueEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain event triggered when a mitigation becomes overdue.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Common;
using SMS_Domain.Interfaces;

namespace SMS_Domain.Events;

/// <summary>
/// Domain event triggered when a mitigation becomes overdue
/// </summary>
public class MitigationOverdueEvent : BaseDomainEvent
{
    public override string EventType => "Mitigation.Overdue";

    public string MitigationId { get; set; } = string.Empty;
    public string MitigationCode { get; set; } = string.Empty;
    public string HazardId { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public int DaysOverdue { get; set; }
    public string ReportId { get; set; } = string.Empty;
    public string AssignedTo { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;

    public MitigationOverdueEvent(string mitigationId, string mitigationCode, string hazardId, 
        DateTime targetCompletionDate, int daysOverdue)
    {
        MitigationId = mitigationId;
        MitigationCode = mitigationCode;
        HazardId = hazardId;
        DueDate = targetCompletionDate;
        DaysOverdue = daysOverdue;
        AggregateId = mitigationId;
    }
}