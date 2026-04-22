//-----------------------------------------------------------------------
// <copyright file="HazardWorkflowEvents.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain events for hazard escalation and mitigation assignment workflows.
//                  Supports hazard lifecycle management, escalation processes, and stakeholder
//                  notification patterns that integrate with existing SMS hazard infrastructure.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Common;
using SMS_Domain.Enums;

namespace SMS_Domain.Events;

/// <summary>
/// Event triggered when a hazard requires escalation
/// Initiates escalation workflow and stakeholder notification processes
/// </summary>
public class HazardEscalationEvent : BaseDomainEvent
{
    public override string EventType => "Hazard.Escalation";

    public string HazardId { get; private set; }
    public string HazardCode { get; private set; }
    public EscalationLevel EscalationLevel { get; private set; }
    public string EscalationReason { get; private set; }
    public string AssignedToGroup { get; private set; }
    public string AssignedBy { get; private set; }
    public DateTime EscalationDeadline { get; private set; }
    public HazardStatus CurrentStatus { get; private set; }
    public Dictionary<string, string> EscalationMetadata { get; private set; }

    public HazardEscalationEvent(
        string hazardId,
        string hazardCode,
        EscalationLevel escalationLevel,
        string escalationReason,
        string assignedToGroup,
        string assignedBy,
        DateTime escalationDeadline,
        HazardStatus currentStatus,
        string aggregateId,
        Dictionary<string, string>? escalationMetadata = null)
    {
        HazardId = hazardId ?? throw new ArgumentNullException(nameof(hazardId));
        HazardCode = hazardCode ?? throw new ArgumentNullException(nameof(hazardCode));
        EscalationLevel = escalationLevel;
        EscalationReason = escalationReason ?? string.Empty;
        AssignedToGroup = assignedToGroup ?? throw new ArgumentNullException(nameof(assignedToGroup));
        AssignedBy = assignedBy ?? throw new ArgumentNullException(nameof(assignedBy));
        EscalationDeadline = escalationDeadline;
        CurrentStatus = currentStatus;
        AggregateId = aggregateId ?? throw new ArgumentNullException(nameof(aggregateId));
        EscalationMetadata = escalationMetadata ?? new Dictionary<string, string>();
    }
}

/// <summary>
/// Event triggered when a mitigation is assigned
/// Initiates assignment notification and tracking workflows
/// </summary>
public class MitigationAssignmentEvent : BaseDomainEvent
{
    public override string EventType => "Mitigation.Assignment";

    public string MitigationId { get; private set; }
    public string MitigationCode { get; private set; }
    public string HazardId { get; private set; }
    public string AssignedTo { get; private set; }
    public string AssignedBy { get; private set; }
    public DateTime DueDate { get; private set; }
    public MitigationPriority Priority { get; private set; }
    public string AssignmentNotes { get; private set; }
    public List<string> NotificationRecipients { get; private set; }

    public MitigationAssignmentEvent(
        string mitigationId,
        string mitigationCode,
        string hazardId,
        string assignedTo,
        string assignedBy,
        DateTime dueDate,
        MitigationPriority priority,
        string assignmentNotes,
        string aggregateId,
        List<string>? notificationRecipients = null)
    {
        MitigationId = mitigationId ?? throw new ArgumentNullException(nameof(mitigationId));
        MitigationCode = mitigationCode ?? throw new ArgumentNullException(nameof(mitigationCode));
        HazardId = hazardId ?? throw new ArgumentNullException(nameof(hazardId));
        AssignedTo = assignedTo ?? throw new ArgumentNullException(nameof(assignedTo));
        AssignedBy = assignedBy ?? throw new ArgumentNullException(nameof(assignedBy));
        DueDate = dueDate;
        Priority = priority;
        AssignmentNotes = assignmentNotes ?? string.Empty;
        AggregateId = aggregateId ?? throw new ArgumentNullException(nameof(aggregateId));
        NotificationRecipients = notificationRecipients ?? new List<string>();
    }
}

/// <summary>
/// Event triggered when a mitigation is completed
/// Integrates with existing SPIEventCoordinator patterns
/// </summary>
public class MitigationCompletedEvent : BaseDomainEvent
{
    public override string EventType => "Mitigation.Completed";

    public string MitigationId { get; private set; }
    public string MitigationCode { get; private set; }
    public string HazardId { get; private set; }
    public string CompletedBy { get; private set; }
    public DateTime CompletedDate { get; private set; }
    public DateTime TargetCompletionDate { get; private set; }
    public string CompletionNotes { get; private set; }
    public bool IsOverdue { get; private set; }

    public MitigationCompletedEvent(
        string mitigationId,
        string mitigationCode,
        string hazardId,
        string completedBy,
        DateTime completedDate,
        DateTime targetCompletionDate,
        string completionNotes,
        string aggregateId)
    {
        MitigationId = mitigationId ?? throw new ArgumentNullException(nameof(mitigationId));
        MitigationCode = mitigationCode ?? throw new ArgumentNullException(nameof(mitigationCode));
        HazardId = hazardId ?? throw new ArgumentNullException(nameof(hazardId));
        CompletedBy = completedBy ?? throw new ArgumentNullException(nameof(completedBy));
        CompletedDate = completedDate;
        TargetCompletionDate = targetCompletionDate;
        CompletionNotes = completionNotes ?? string.Empty;
        AggregateId = aggregateId ?? throw new ArgumentNullException(nameof(aggregateId));
        IsOverdue = completedDate > targetCompletionDate;
    }
}

/// <summary>
/// Escalation levels for hazard workflows
/// </summary>
public enum EscalationLevel
{
    Level1 = 1,
    Level2 = 2, 
    Level3 = 3,
    Executive = 4
}

/// <summary>
/// Priority levels for mitigation assignments
/// </summary>
public enum MitigationPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Urgent = 4
}