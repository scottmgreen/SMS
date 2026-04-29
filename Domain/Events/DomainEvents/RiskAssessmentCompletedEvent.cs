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
/// IMPLEMENTS: IEventDataSource for automatic SPI data source discovery
/// </summary>
public class RiskAssessmentCompletedEvent : BaseDomainEvent, IEventDataSource
{
    public override string EventType => "RiskAssessment.Completed";

    #region IEventDataSource Implementation

    /// <summary>
    /// Display name for SPI configuration dropdowns
    /// </summary>
    public string DataSourceDisplayName => "Risk Assessment";

    /// <summary>
    /// Category for grouping in UI
    /// </summary>
    public string DataSourceCategory => "Risk";

    /// <summary>
    /// Description of data provided for SPI calculations
    /// </summary>
    public string DataSourceDescription => "Provides data points for risk assessment completion rates, risk level trends, and assessment timeliness metrics";

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

    #endregion
}