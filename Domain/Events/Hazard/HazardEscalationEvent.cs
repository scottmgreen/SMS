//-----------------------------------------------------------------------
// <copyright file="HazardEscalationEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain event triggered when a hazard requires escalation in the SMS system.
//                  Supports escalation workflows, deadline management, and stakeholder notification
//                  coordination for hazards requiring elevated attention and response.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Common;
using SMS_Domain.Enums;

namespace SMS_Domain.Events.Hazard;

/// <summary>
/// Domain event triggered when a hazard requires escalation
/// Supports escalation workflows and deadline management
/// </summary>
public class HazardEscalationEvent : BaseDomainEvent
{
    public override string EventType => "Hazard.Escalation";

    public string HazardId { get; private set; }
    public string HazardCode { get; private set; }
    public SMS_Domain.Enums.EscalationLevel EscalationLevel { get; private set; }
    public string EscalationReason { get; private set; }
    public string AssignedToGroup { get; private set; }
    public string AssignedBy { get; private set; }
    public DateTime EscalationDeadline { get; private set; }
    public HazardStatus CurrentStatus { get; private set; }

    public HazardEscalationEvent(
        string hazardId,
        string hazardCode,
        SMS_Domain.Enums.EscalationLevel escalationLevel,
        string escalationReason,
        string assignedToGroup,
        string assignedBy,
        DateTime escalationDeadline,
        HazardStatus currentStatus,
        string aggregateId)
    {
        HazardId = hazardId ?? throw new ArgumentNullException(nameof(hazardId));
        HazardCode = hazardCode ?? throw new ArgumentNullException(nameof(hazardCode));
        EscalationLevel = escalationLevel;
        EscalationReason = escalationReason ?? throw new ArgumentNullException(nameof(escalationReason));
        AssignedToGroup = assignedToGroup ?? throw new ArgumentNullException(nameof(assignedToGroup));
        AssignedBy = assignedBy ?? throw new ArgumentNullException(nameof(assignedBy));
        EscalationDeadline = escalationDeadline;
        CurrentStatus = currentStatus ?? throw new ArgumentNullException(nameof(currentStatus));

        AggregateId = aggregateId ?? throw new ArgumentNullException(nameof(aggregateId));
    }
}