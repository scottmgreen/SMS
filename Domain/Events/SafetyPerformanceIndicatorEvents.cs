//-----------------------------------------------------------------------
// <copyright file="SafetyPerformanceIndicatorEvents.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain events for Safety Performance Indicator workflow notifications.
//                  Supports SPI threshold monitoring, escalation workflows, and stakeholder
//                  notification patterns that integrate with existing SMS infrastructure.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Common;

namespace SMS_Domain.Events;

/// <summary>
/// Event triggered when an SPI threshold is exceeded
/// Initiates workflow notifications and potential escalation processes
/// </summary>
public class SPIThresholdExceededEvent : BaseDomainEvent
{
    public override string EventType => "SPI.ThresholdExceeded";

    public string SPICode { get; private set; }
    public string SPIName { get; private set; }
    public decimal CurrentValue { get; private set; }
    public decimal ThresholdValue { get; private set; }
    public SPISeverityLevel Severity { get; private set; }
    public List<string> StakeholderGroups { get; private set; }
    public string ReportingPeriod { get; private set; }
    public DateTime DetectedAt { get; private set; }

    public SPIThresholdExceededEvent(
        string spiCode,
        string spiName,
        decimal currentValue,
        decimal thresholdValue,
        SPISeverityLevel severity,
        List<string> stakeholderGroups,
        string reportingPeriod,
        string aggregateId)
    {
        SPICode = spiCode ?? throw new ArgumentNullException(nameof(spiCode));
        SPIName = spiName ?? throw new ArgumentNullException(nameof(spiName));
        CurrentValue = currentValue;
        ThresholdValue = thresholdValue;
        Severity = severity;
        StakeholderGroups = stakeholderGroups ?? new List<string>();
        ReportingPeriod = reportingPeriod ?? string.Empty;
        DetectedAt = DateTime.UtcNow;
        AggregateId = aggregateId ?? throw new ArgumentNullException(nameof(aggregateId));
    }
}

/// <summary>
/// Event triggered when SPI data is updated
/// Can initiate threshold checking and monitoring workflows
/// </summary>
public class SPIDataUpdatedEvent : BaseDomainEvent
{
    public override string EventType => "SPI.DataUpdated";

    public string SPICode { get; private set; }
    public decimal PreviousValue { get; private set; }
    public decimal NewValue { get; private set; }
    public string UpdatedBy { get; private set; }
    public string UpdateReason { get; private set; }
    public Dictionary<string, object> AdditionalData { get; private set; }

    public SPIDataUpdatedEvent(
        string spiCode,
        decimal previousValue,
        decimal newValue,
        string updatedBy,
        string updateReason,
        string aggregateId,
        Dictionary<string, object>? additionalData = null)
    {
        SPICode = spiCode ?? throw new ArgumentNullException(nameof(spiCode));
        PreviousValue = previousValue;
        NewValue = newValue;
        UpdatedBy = updatedBy ?? throw new ArgumentNullException(nameof(updatedBy));
        UpdateReason = updateReason ?? string.Empty;
        AggregateId = aggregateId ?? throw new ArgumentNullException(nameof(aggregateId));
        AdditionalData = additionalData ?? new Dictionary<string, object>();
    }
}

/// <summary>
/// Severity levels for SPI events
/// Simple enum that can be enhanced based on existing SMS severity patterns
/// </summary>
public enum SPISeverityLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}