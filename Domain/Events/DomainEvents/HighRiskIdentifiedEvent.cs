//-----------------------------------------------------------------------
// <copyright file="HighRiskIdentifiedEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain event triggered when high risk is identified.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Common;
using SMS_Domain.Interfaces;
using SMS_Domain.Enums;

namespace SMS_Domain.Events;

/// <summary>
/// Domain event triggered when high risk is identified
/// </summary>
public class HighRiskIdentifiedEvent : BaseDomainEvent
{
    public override string EventType => "Risk.HighRiskIdentified";

    public string AssessmentId { get; set; } = string.Empty;
    public RiskLevel RiskLevel { get; set; }
    public DateTime IdentifiedDate { get; set; }
    public string ReportId { get; set; } = string.Empty;
    public string RiskDescription { get; set; } = string.Empty;
    public decimal RiskScore { get; set; }

    public HighRiskIdentifiedEvent(string assessmentId, string assessmentCode, RiskLevel riskLevel, 
        decimal riskScore, DateTime identifiedDate, string identifiedBy)
    {
        AssessmentId = assessmentId;
        RiskLevel = riskLevel;
        IdentifiedDate = identifiedDate;
        RiskScore = riskScore;
        AggregateId = assessmentId;
    }
}