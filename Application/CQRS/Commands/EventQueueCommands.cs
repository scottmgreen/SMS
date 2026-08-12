//-----------------------------------------------------------------------
// <copyright file="EventQueueCommands.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command definitions for EventQueue execution and lifecycle operations.
//                  Defines command objects for write operations in the CQRS pattern.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.ValueObjects;
using SMS_Domain.Enums;

namespace SMS_Application.Commands;

public class ExecuteQueuedEventCommand : BaseCommandBundle, IRequest<Result>
{
    public string QueueCode { get; set; }
    public string? ExecutedBy { get; set; }

    public ExecuteQueuedEventCommand(string queueCode, string? executedBy = null)
    {
        QueueCode = queueCode;
        ExecutedBy = executedBy;
    }
}

public class ExecuteAllPendingQueuedEventsCommand : BaseCommandBundle, IRequest<Result<int>>
{
    public EventCategory? EventType { get; set; }
    public string? ExecutedBy { get; set; }

    public ExecuteAllPendingQueuedEventsCommand(EventCategory? eventType = null, string? executedBy = null)
    {
        EventType = eventType;
        ExecutedBy = executedBy;
    }
}

public class CancelQueuedEventCommand : BaseCommandBundle, IRequest<Result>
{
    public string QueueCode { get; set; }
    public string? CancelledBy { get; set; }

    public CancelQueuedEventCommand(string queueCode, string? cancelledBy = null)
    {
        QueueCode = queueCode;
        CancelledBy = cancelledBy;
    }
}

public class ClearCompletedQueuedEventsCommand : BaseCommandBundle, IRequest<Result<int>>
{
}

public class RebuildQueuedEmailEventCommand : BaseCommandBundle, IRequest<Result<string>>
{
    public string QueueCode { get; set; }
    public string? RebuiltBy { get; set; }

    public RebuildQueuedEmailEventCommand(string queueCode, string? rebuiltBy = null)
    {
        QueueCode = queueCode;
        RebuiltBy = rebuiltBy;
    }
}

public class ClearAllQueuedEventsCommand : BaseCommandBundle, IRequest<Result<int>>
{
}
