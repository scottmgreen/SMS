//-----------------------------------------------------------------------
// <copyright file="HazardWorkflowEnums.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Enums for hazard workflow events and status tracking in the SMS system.
//                  Supports hazard lifecycle management, escalation workflows, and mitigation
//                  approval processes integrated with the EventBus architecture.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Enums;

/// <summary>
/// Priority levels for hazard events
/// </summary>
public enum HazardPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

/// <summary>
/// Escalation levels for hazard management
/// </summary>
public enum EscalationLevel
{
    Level1 = 1,
    Level2 = 2,
    Level3 = 3,
    Executive = 4
}

/// <summary>
/// Mitigation priority levels for approval workflows
/// </summary>
public enum MitigationPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4,
    Emergency = 5
}