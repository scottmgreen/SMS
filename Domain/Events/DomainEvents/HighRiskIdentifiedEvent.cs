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
using SMS_Domain.Entities;

namespace SMS_Domain.Events;

/// <summary>
/// Domain event triggered when high risk is identified
/// </summary>
public class HighRiskIdentifiedEvent : BaseDomainEvent, IEventSource
{
    public override string EventType => SMS_Domain.Enums.EventType.HighRiskIdentified.Value;

    /// <summary>
    /// Display name for SPI configuration dropdowns
    /// </summary>
    public string EventSourceDisplayName => SMS_Domain.Enums.EventType.HighRiskIdentified;

    /// <summary>
    /// Category for grouping in UI
    /// </summary>
    public string EventSourceCategory => "SMS Domain Event";

    /// <summary>
    /// Description of data provided for SPI calculations
    /// </summary>
    public string EventSourceDescription => "Provides high-risk identification signals for risk exposure and trend SPI metrics";

    /// <summary>
    /// Indicates this event is automatically generated from risk analysis workflow
    /// </summary>
    public bool IsAutomaticDataSource => true;

    /// <summary>
    /// Display ordering priority in SPI source selection
    /// </summary>
    public int DisplayPriority => 3;

    public string AssessmentId { get; set; } = string.Empty;
    public RiskLevel RiskLevel { get; set; }
    public DateTime IdentifiedDate { get; set; }
    public string ReportId { get; set; } = string.Empty;
    public string RiskDescription { get; set; } = string.Empty;
    public decimal RiskScore { get; set; }

    public HighRiskIdentifiedEvent(SMSEventID id):base(id)
    {

    }
    //public HighRiskIdentifiedEvent(string assessmentId, string assessmentCode, RiskLevel riskLevel,
    //    decimal riskScore, DateTime identifiedDate, string identifiedBy)
    //{
    //    AssessmentId = assessmentId;
    //    RiskLevel = riskLevel;
    //    IdentifiedDate = identifiedDate;
    //    RiskScore = riskScore;
        
    //}
}