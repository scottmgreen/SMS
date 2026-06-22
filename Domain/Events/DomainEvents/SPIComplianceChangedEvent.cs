//-----------------------------------------------------------------------
// <copyright file="SPIComplianceChangedEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain event triggered when SPI compliance status changes in the SMS system.
//                  Supports compliance monitoring workflows, audit trail requirements,
//                  and regulatory reporting automation.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Common;
using SMS_Domain.Entities;
using SMS_Domain.Interfaces;

namespace SMS_Domain.Events;

/// <summary>
/// Domain event triggered when SPI compliance status changes
/// Supports regulatory reporting and compliance monitoring workflows
/// </summary>
public class SPIComplianceChangedEvent : BaseDomainEvent, IEventSource
{
    public override string EventType => SMS_Domain.Enums.EventType.SPIComplianceChanged.Value;

    /// <summary>
    /// Display name for SPI configuration dropdowns
    /// </summary>
    public string EventSourceDisplayName => SMS_Domain.Enums.EventType.SPIComplianceChanged;

    /// <summary>
    /// Category for grouping in UI
    /// </summary>
    public string EventSourceCategory => "SMS Domain Event";

    /// <summary>
    /// Description of data provided for SPI calculations
    /// </summary>
    public string EventSourceDescription => "Provides SPI compliance transitions for threshold, at-risk, and regulatory monitoring metrics";

    /// <summary>
    /// Indicates this event is automatically generated from SPI compliance evaluation
    /// </summary>
    public bool IsAutomaticDataSource => true;

    /// <summary>
    /// Display ordering priority in SPI source selection
    /// </summary>
    public int DisplayPriority => 5;

    public string SPICode { get; private set; }
    public string SPIName { get; private set; }
    public SPIComplianceStatus PreviousStatus { get; private set; }
    public SPIComplianceStatus NewStatus { get; private set; }
    public decimal CurrentValue { get; private set; }
    public decimal ComplianceThreshold { get; private set; }
    public string ComplianceStandard { get; private set; }
    public string ReportingPeriod { get; private set; }
    public DateTime ComplianceCheckDate { get; private set; }
    public List<string> AffectedStakeholders { get; private set; }
    public Dictionary<string, object> ComplianceMetadata { get; private set; }

    // Regulatory tracking
    public bool RequiresRegulatoryReporting { get; private set; }
    public DateTime? RegulatoryReportingDeadline { get; private set; }
    public string RegulatoryBody { get; private set; }

    public SPIComplianceChangedEvent(
        SMSEventID id,
        string spiCode,
        string spiName,
        SPIComplianceStatus previousStatus,
        SPIComplianceStatus newStatus,
        decimal currentValue,
        decimal complianceThreshold,
        string complianceStandard,
        string reportingPeriod,
        DateTime complianceCheckDate,
        List<string>? affectedStakeholders = null,
        Dictionary<string, object>? complianceMetadata = null,
        bool requiresRegulatoryReporting = false,
        DateTime? regulatoryReportingDeadline = null,
        string regulatoryBody = "") : base(id)
    {
        SPICode = spiCode ?? throw new ArgumentNullException(nameof(spiCode));
        SPIName = spiName ?? throw new ArgumentNullException(nameof(spiName));
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        CurrentValue = currentValue;
        ComplianceThreshold = complianceThreshold;
        ComplianceStandard = complianceStandard ?? throw new ArgumentNullException(nameof(complianceStandard));
        ReportingPeriod = reportingPeriod ?? throw new ArgumentNullException(nameof(reportingPeriod));
        ComplianceCheckDate = complianceCheckDate;
        AffectedStakeholders = affectedStakeholders ?? new List<string>();
        ComplianceMetadata = complianceMetadata ?? new Dictionary<string, object>();
        RequiresRegulatoryReporting = requiresRegulatoryReporting;
        RegulatoryReportingDeadline = regulatoryReportingDeadline;
        RegulatoryBody = regulatoryBody ?? "";
    }
}

/// <summary>
/// SPI compliance status enumeration
/// </summary>
public enum SPIComplianceStatus
{
    Unknown = 0,
    Compliant = 1,
    NonCompliant = 2,
    AtRisk = 3,
    UnderReview = 4
}