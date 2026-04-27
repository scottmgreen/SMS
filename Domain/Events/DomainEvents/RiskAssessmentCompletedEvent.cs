//-----------------------------------------------------------------------
// <copyright file="RiskAssessmentCompletedEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain event triggered when a risk assessment is completed.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Common;
using SMS_Domain.Interfaces;
using SMS_Domain.Enums;

namespace SMS_Domain.Events;

/// <summary>
/// Domain event triggered when a risk assessment is completed
/// </summary>
public class RiskAssessmentCompletedEvent : BaseDomainEvent
{
    public override string EventType => "RiskAssessment.Completed";

    public string AssessmentId { get; set; } = string.Empty;
    public string AssessmentCode { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime TargetCompletionDate { get; set; }
    public DateTime CompletedDate { get; set; }
    public string HazardId { get; set; } = string.Empty;
    public string ReportId { get; set; } = string.Empty;
    public RiskLevel RiskLevel { get; set; }
    public decimal RiskScore { get; set; }
    public string AssessmentType { get; set; } = string.Empty;

    public RiskAssessmentCompletedEvent(string assessmentId, string assessmentCode, DateTime startDate,
        DateTime targetCompletionDate, DateTime completedDate, string aggregateId)
    {
        AssessmentId = assessmentId;
        AssessmentCode = assessmentCode;
        StartDate = startDate;
        TargetCompletionDate = targetCompletionDate;
        CompletedDate = completedDate;
        AggregateId = aggregateId;
    }
}