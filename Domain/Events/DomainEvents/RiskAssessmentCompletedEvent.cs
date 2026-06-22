//-----------------------------------------------------------------------
// <copyright file="RiskAssessmentCompletedEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain event triggered when a risk assessment is completed.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Common;
using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Domain.Interfaces;

namespace SMS_Domain.Events;

/// <summary>
/// Domain event triggered when a risk assessment is completed
/// IMPLEMENTS: IEventSource for automatic SPI data source discovery
/// </summary>
public class RiskAssessmentCompletedEvent : BaseDomainEvent, IEventSource
{
    public override string EventType => SMS_Domain.Enums.EventType.RiskAssessmentCompleted.Value;

    /// <summary>
    /// Display name for SPI configuration dropdowns
    /// </summary>
    public string EventSourceDisplayName => SMS_Domain.Enums.EventType.RiskAssessmentCompleted;

    /// <summary>
    /// Category for grouping in UI
    /// </summary>
    public string EventSourceCategory => "SMS Domain Event";

    #region IEventDataSource Implementation

    
    /// <summary>
    /// Description of data provided for SPI calculations
    /// </summary>
    public string EventSourceDescription => "Provides data points for risk assessment completion rates, risk level trends, and assessment timeliness metrics";

    /// <summary>
    /// This is a primary automatic data source for risk-related SPIs
    /// </summary>
    public bool IsAutomaticDataSource => true;

    /// <summary>
    /// High priority for risk-related SPIs
    /// </summary>
    public int DisplayPriority => 2;

    #endregion

    #region Event Properties

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

    public RiskAssessmentCompletedEvent(SMSEventID id,string assessmentId, string assessmentCode, DateTime startDate,
        DateTime targetCompletionDate, DateTime completedDate, string aggregateId) : base(id)
    {
        AssessmentId = assessmentId;
        AssessmentCode = assessmentCode;
        StartDate = startDate;
        TargetCompletionDate = targetCompletionDate;
        CompletedDate = completedDate;
        
    }

    #endregion
}