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

namespace SMS_Domain.Events.Mitigation;

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
        List<string> requiredApprovers,
        string approvalJustification,
        string aggregateId,
        Dictionary<string, object>? approvalMetadata = null,
        decimal? estimatedCost = null,
        string? resourceRequirements = null,
        TimeSpan? estimatedImplementationTime = null,
        DateTime? approvalDeadline = null,
        bool requiresExecutiveApproval = false,
        string? escalationPath = null)
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
        RequiredApprovers = requiredApprovers ?? throw new ArgumentNullException(nameof(requiredApprovers));
        ApprovalJustification = approvalJustification ?? throw new ArgumentNullException(nameof(approvalJustification));
        ApprovalMetadata = approvalMetadata ?? new Dictionary<string, object>();
        EstimatedCost = estimatedCost;
        ResourceRequirements = resourceRequirements ?? string.Empty;
        EstimatedImplementationTime = estimatedImplementationTime;
        ApprovalDeadline = approvalDeadline ?? DateTime.UtcNow.AddDays(7); // Default 7-day approval window
        RequiresExecutiveApproval = requiresExecutiveApproval;
        EscalationPath = escalationPath ?? string.Empty;

        AggregateId = aggregateId ?? throw new ArgumentNullException(nameof(aggregateId));
    }
}