//-----------------------------------------------------------------------
// <copyright file="EventQueueQueries.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query definitions for EventQueue retrieval and monitoring operations.
//                  Defines query objects for read operations in the CQRS pattern.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.ValueObjects;

namespace SMS_Application.Messaging.Queries;

public class GetQueuedEventsQuery : BaseQueryBundle, IRequest<Result<IEnumerable<QueuedEvent>>>, IReadQuery
{
    public QueuedEventStatus? Status { get; set; }
    public EventCategory? EventType { get; set; }
    public int? MaxResults { get; set; }

    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetQueuedEventsQuery(QueuedEventStatus? status = null, EventCategory? eventType = null, int? maxResults = null)
    {
        Status = status;
        EventType = eventType;
        MaxResults = maxResults;
    }

    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier() => "EventQueue:List";

    public string GetAccessType() => "GetQueuedEvents";
}

public class GetQueuedEventByIdQuery : BaseQueryBundle, IRequest<Result<QueuedEvent>>, IReadQuery
{
    public Guid EventId { get; set; }

    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetQueuedEventByIdQuery(Guid eventId)
    {
        EventId = eventId;
    }

    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier() => $"EventQueue:{EventId}";

    public string GetAccessType() => "GetQueuedEventById";
}

public class GetEventQueueStatisticsQuery : BaseQueryBundle, IRequest<Result<QueueStatistics>>, IReadQuery
{
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier() => "EventQueue:Statistics";

    public string GetAccessType() => "GetStatistics";
}
