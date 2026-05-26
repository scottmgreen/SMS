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

namespace SMS_Application.Messaging.Commands;

public class ExecuteQueuedEventCommand : BaseCommandBundle, IRequest<Result>
{
    public Guid EventId { get; set; }
    public string? ExecutedBy { get; set; }

    public ExecuteQueuedEventCommand(Guid eventId, string? executedBy = null)
    {
        EventId = eventId;
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
    public Guid EventId { get; set; }
    public string? CancelledBy { get; set; }

    public CancelQueuedEventCommand(Guid eventId, string? cancelledBy = null)
    {
        EventId = eventId;
        CancelledBy = cancelledBy;
    }
}

public class ClearCompletedQueuedEventsCommand : BaseCommandBundle, IRequest<Result<int>>
{
}

public class ClearAllQueuedEventsCommand : BaseCommandBundle, IRequest<Result<int>>
{
}
