//-----------------------------------------------------------------------
// <copyright file="HazardWorkflowEnums.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Enums for hazard workflow events and status tracking in the SMS system.
//                  Supports hazard lifecycle management, escalation workflows, and mitigation
//                  approval processes integrated with the EventBus architecture.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Common;

namespace SMS_Domain.Enums;

/// <summary>
/// Priority levels for hazard events
/// </summary>
public abstract class HazardPriority : BaseEnum<HazardPriority>
{
    protected HazardPriority(string value, string name, int rank) : base(value, name)
    {
        Rank = rank;
    }

    public int Rank { get; }

    public static readonly HazardPriority Low = new LowPriority();
    public static readonly HazardPriority Medium = new MediumPriority();
    public static readonly HazardPriority High = new HighPriority();
    public static readonly HazardPriority Critical = new CriticalPriority();

    private sealed class LowPriority : HazardPriority
    {
        public LowPriority() : base("LOW", "Low", 1)
        {
        }
    }

    private sealed class MediumPriority : HazardPriority
    {
        public MediumPriority() : base("MEDIUM", "Medium", 2)
        {
        }
    }

    private sealed class HighPriority : HazardPriority
    {
        public HighPriority() : base("HIGH", "High", 3)
        {
        }
    }

    private sealed class CriticalPriority : HazardPriority
    {
        public CriticalPriority() : base("CRITICAL", "Critical", 4)
        {
        }
    }
}


