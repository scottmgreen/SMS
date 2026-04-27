//-----------------------------------------------------------------------
// <copyright file="MitigationApprovalRequestedEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain event triggered when a mitigation requires approval in the SMS system.
//                  Supports mitigation approval workflows, escalation processes, and stakeholder
//                  notification coordination for safety-critical mitigation measures.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Common;
using SMS_Domain.Interfaces;
using SMS_Domain.Enums;

namespace SMS_Domain.Events;

/// <summary>
/// Domain event triggered when a mitigation requires approval
/// Initiates approval workflows and stakeholder notification processes
/// </summary>
public class MitigationApprovalRequestedEvent : BaseDomainEvent
{
    public override string EventType => "Mitigation.ApprovalRequested";

    public string MitigationId { get; private set; }
    public string MitigationCode { get; private set; }
    public string HazardId { get; private set; }
    public string HazardCode { get; private set; }
    public string MitigationDescription { get; private set; }
    public MitigationPriority Priority { get; private set; }
    public string RequestedBy { get; private set; }
    public DateTime RequestDate { get; private set; }
    public DateTime ProposedImplementationDate { get; private set; }
    public List<string> RequiredApprovers { get; private set; }
    public string ApprovalJustification { get; private set; }
    public Dictionary<string, object> ApprovalMetadata { get; private set; }

    // Cost and resource information
    public decimal? EstimatedCost { get; private set; }
    public string ResourceRequirements { get; private set; }
    public TimeSpan? EstimatedImplementationTime { get; private set; }

    // Escalation tracking
    public DateTime ApprovalDeadline { get; private set; }
    public bool RequiresExecutiveApproval { get; private set; }
    public string EscalationPath { get; private set; }

    public MitigationApprovalRequestedEvent(
        string mitigationId,
        string mitigationCode,
        string hazardId,
        string hazardCode,
        string mitigationDescription,
        MitigationPriority priority,
        string requestedBy,
        DateTime requestDate,
        DateTime proposedImplementationDate,
        List<string>? requiredApprovers = null,
        string approvalJustification = "",
        Dictionary<string, object>? approvalMetadata = null,
        decimal? estimatedCost = null,
        string resourceRequirements = "",
        TimeSpan? estimatedImplementationTime = null,
        DateTime? approvalDeadline = null,
        bool requiresExecutiveApproval = false,
        string escalationPath = "")
    {
        MitigationId = mitigationId ?? throw new ArgumentNullException(nameof(mitigationId));
        MitigationCode = mitigationCode ?? throw new ArgumentNullException(nameof(mitigationCode));
        HazardId = hazardId ?? throw new ArgumentNullException(nameof(hazardId));
        HazardCode = hazardCode ?? throw new ArgumentNullException(nameof(hazardCode));
        MitigationDescription = mitigationDescription ?? throw new ArgumentNullException(nameof(mitigationDescription));
        Priority = priority;
        RequestedBy = requestedBy ?? throw new ArgumentNullException(nameof(requestedBy));
        RequestDate = requestDate;
        ProposedImplementationDate = proposedImplementationDate;
        RequiredApprovers = requiredApprovers ?? new List<string>();
        ApprovalJustification = approvalJustification ?? "";
        ApprovalMetadata = approvalMetadata ?? new Dictionary<string, object>();
        EstimatedCost = estimatedCost;
        ResourceRequirements = resourceRequirements ?? "";
        EstimatedImplementationTime = estimatedImplementationTime;
        ApprovalDeadline = approvalDeadline ?? proposedImplementationDate.AddDays(-7);
        RequiresExecutiveApproval = requiresExecutiveApproval;
        EscalationPath = escalationPath ?? "";
    }
}

/// <summary>
/// Mitigation priority levels
/// </summary>
public enum MitigationPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3,
    Emergency = 4
}