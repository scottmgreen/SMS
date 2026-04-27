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
using SMS_Domain.Interfaces;

namespace SMS_Domain.Events;

/// <summary>
/// Domain event triggered when SPI compliance status changes
/// Supports regulatory reporting and compliance monitoring workflows
/// </summary>
public class SPIComplianceChangedEvent : BaseDomainEvent
{
    public override string EventType => "SPI.ComplianceChanged";

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
        string regulatoryBody = "")
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