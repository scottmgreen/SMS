//-----------------------------------------------------------------------
// <copyright file="SPIThresholdExceededEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team  
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain event for Safety Performance Indicator threshold violations.
//                  Supports SPI threshold monitoring, escalation workflows, and stakeholder
//                  notification patterns that integrate with existing SMS infrastructure.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Common;
using SMS_Domain.Entities;

namespace SMS_Domain.Events;

/// <summary>
/// Event triggered when an SPI threshold is exceeded
/// Initiates workflow notifications and potential escalation processes
/// </summary>
public class SPIThresholdExceededEvent : BaseDomainEvent, IEventSource
{
    public override string EventType => "SPI_Threshold_Exceeded";

    /// <summary>
    /// Display name for SPI configuration dropdowns
    /// </summary>
    public string EventSourceDisplayName => "SMS Event Bus - SPI Threshold Exceeded";

    /// <summary>
    /// Category for grouping in UI
    /// </summary>
    public string EventSourceCategory => "SMS Domain Event";

    /// <summary>
    /// Description of data provided for SPI calculations
    /// </summary>
    public string EventSourceDescription => "Provides SPI threshold breach signals for alerting, escalation, and dashboard compliance visualization";

    /// <summary>
    /// Indicates this event is automatically generated from SPI threshold evaluation
    /// </summary>
    public bool IsAutomaticDataSource => true;

    /// <summary>
    /// Display ordering priority in SPI source selection
    /// </summary>
    public int DisplayPriority => 7;

    public string SPICode { get; private set; }
    public string SPIName { get; private set; }
    public decimal CurrentValue { get; private set; }
    public decimal ThresholdValue { get; private set; }
    public SPISeverityLevel Severity { get; private set; }
    public List<string> StakeholderGroups { get; private set; }
    public string ReportingPeriod { get; private set; }
    public DateTime DetectedAt { get; private set; }

    public SPIThresholdExceededEvent(
        SMSEventID id,
        string spiCode,
        string spiName,
        decimal currentValue,
        decimal thresholdValue,
        SPISeverityLevel severity,
        List<string> stakeholderGroups,
        string reportingPeriod,
        string aggregateId) : base(id)
    {
        SPICode = spiCode ?? throw new ArgumentNullException(nameof(spiCode));
        SPIName = spiName ?? throw new ArgumentNullException(nameof(spiName));
        CurrentValue = currentValue;
        ThresholdValue = thresholdValue;
        Severity = severity;
        StakeholderGroups = stakeholderGroups ?? new List<string>();
        ReportingPeriod = reportingPeriod ?? string.Empty;
        DetectedAt = DateTime.UtcNow;
        
    }
}

/// <summary>
/// SPI severity levels for threshold violations
/// </summary>
public enum SPISeverityLevel
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}