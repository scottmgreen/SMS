//-----------------------------------------------------------------------
// <copyright file="HazardEscalationEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain event for hazard escalation workflows.
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
/// Escalation levels for hazard management
/// </summary>
public enum EscalationLevel
{
    Level1_Supervisor = 1,
    Level2_Manager = 2,
    Level3_Executive = 3,
    Level4_Emergency = 4
}