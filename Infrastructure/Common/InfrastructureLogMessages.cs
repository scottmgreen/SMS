//-----------------------------------------------------------------------
// <copyright file="InfrastructureLogMessages.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Infrastructure service interface defining nfrastructurelogmessages operations and contracts.
//                  Infrastructure service contract defining data access operations
//                  and external system integration interfaces.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Infrastructure.Common;

public static class InfrastructureLogMessages
{
    public static void LogInfrastructureInformation(this ILogger logger, string message, params object[] args)
        => logger.Log(LogLevel.Information, CreateEventId(null, InfrastructureEventIds.Information), null, message, args);

    public static void LogInfrastructureInformation(this ILogger logger, int? eventId, string message, params object[] args)
        => logger.Log(LogLevel.Information, CreateEventId(eventId, InfrastructureEventIds.Information), null, message, args);

    public static void LogInfrastructureInformation(this ILogger logger, Exception ex, string message, params object[] args)
        => logger.Log(LogLevel.Information, CreateEventId(null, InfrastructureEventIds.Information), ex, message, args);

    public static void LogInfrastructureDebug(this ILogger logger, string message, params object[] args)
        => logger.Log(LogLevel.Debug, CreateEventId(null, InfrastructureEventIds.Debug), null, message, args);

    public static void LogInfrastructureDebug(this ILogger logger, int? eventId, string message, params object[] args)
        => logger.Log(LogLevel.Debug, CreateEventId(eventId, InfrastructureEventIds.Debug), null, message, args);

    public static void LogInfrastructureDebug(this ILogger logger, Exception ex, string message, params object[] args)
        => logger.Log(LogLevel.Debug, CreateEventId(null, InfrastructureEventIds.Debug), ex, message, args);

    public static void LogInfrastructureError(this ILogger logger, string message, params object[] args)
        => logger.Log(LogLevel.Error, CreateEventId(null, InfrastructureEventIds.Error), null, message, args);

    public static void LogInfrastructureError(this ILogger logger, int? eventId, string message, params object[] args)
        => logger.Log(LogLevel.Error, CreateEventId(eventId, InfrastructureEventIds.Error), null, message, args);

    public static void LogInfrastructureError(this ILogger logger, Exception ex, string message, params object[] args)
        => logger.Log(LogLevel.Error, CreateEventId(null, InfrastructureEventIds.Error), ex, message, args);

    public static void LogInfrastructureError(this ILogger logger, string message, int? eventId, Exception ex)
        => logger.Log(LogLevel.Error, CreateEventId(eventId, InfrastructureEventIds.Error), ex, message, Array.Empty<object>());

    public static void LogInfrastructureCritical(this ILogger logger, string message, params object[] args)
        => logger.Log(LogLevel.Critical, CreateEventId(null, InfrastructureEventIds.Critical), null, message, args);

    public static void LogInfrastructureCritical(this ILogger logger, int? eventId, string message, params object[] args)
        => logger.Log(LogLevel.Critical, CreateEventId(eventId, InfrastructureEventIds.Critical), null, message, args);

    public static void LogInfrastructureCritical(this ILogger logger, Exception ex, string message, params object[] args)
        => logger.Log(LogLevel.Critical, CreateEventId(null, InfrastructureEventIds.Critical), ex, message, args);

    public static void LogInfrastructureCritical(this ILogger logger, string message, int? eventId, Exception ex)
        => logger.Log(LogLevel.Critical, CreateEventId(eventId, InfrastructureEventIds.Critical), ex, message, Array.Empty<object>());

    public static void LogInfrastructureNone(this ILogger logger, string message, params object[] args)
        => logger.Log(LogLevel.None, CreateEventId(null, InfrastructureEventIds.None), null, message, args);

    public static void LogInfrastructureNone(this ILogger logger, int? eventId, string message, params object[] args)
        => logger.Log(LogLevel.None, CreateEventId(eventId, InfrastructureEventIds.None), null, message, args);

    public static void LogInfrastructureTrace(this ILogger logger, string message, params object[] args)
        => logger.Log(LogLevel.Trace, CreateEventId(null, InfrastructureEventIds.Trace), null, message, args);

    public static void LogInfrastructureTrace(this ILogger logger, int? eventId, string message, params object[] args)
        => logger.Log(LogLevel.Trace, CreateEventId(eventId, InfrastructureEventIds.Trace), null, message, args);

    public static void LogInfrastructureTrace(this ILogger logger, Exception ex, string message, params object[] args)
        => logger.Log(LogLevel.Trace, CreateEventId(null, InfrastructureEventIds.Trace), ex, message, args);

    public static void LogInfrastructureWarning(this ILogger logger, string message, params object[] args)
        => logger.Log(LogLevel.Warning, CreateEventId(null, InfrastructureEventIds.Warning), null, message, args);

    public static void LogInfrastructureWarning(this ILogger logger, int? eventId, string message, params object[] args)
        => logger.Log(LogLevel.Warning, CreateEventId(eventId, InfrastructureEventIds.Warning), null, message, args);

    public static void LogInfrastructureWarning(this ILogger logger, Exception ex, string message, params object[] args)
        => logger.Log(LogLevel.Warning, CreateEventId(null, InfrastructureEventIds.Warning), ex, message, args);

    public static void LogInfrastructureEvent(this ILogger logger, string message)
        => logger.Log(LogLevel.Information, CreateEventId(null, InfrastructureEventIds.InfrastructureEvent), null, message, Array.Empty<object>());

    public static void LogInfrastructureEvent(this ILogger logger, string message, int? eventId)
        => logger.Log(LogLevel.Information, CreateEventId(eventId, InfrastructureEventIds.InfrastructureEvent), null, message, Array.Empty<object>());

    public static void LogInfrastructureGetItems(this ILogger logger, string message)
        => logger.Log(LogLevel.Information, CreateEventId(null, InfrastructureEventIds.GetItems), null, message, Array.Empty<object>());

    public static void LogInfrastructureGetItems(this ILogger logger, string message, int? eventId)
        => logger.Log(LogLevel.Information, CreateEventId(eventId, InfrastructureEventIds.GetItems), null, message, Array.Empty<object>());

    public static void LogInfrastructureGetItem(this ILogger logger, string message)
        => logger.Log(LogLevel.Information, CreateEventId(null, InfrastructureEventIds.GetItem), null, message, Array.Empty<object>());

    public static void LogInfrastructureGetItem(this ILogger logger, string message, int? eventId)
        => logger.Log(LogLevel.Information, CreateEventId(eventId, InfrastructureEventIds.GetItem), null, message, Array.Empty<object>());

    public static void LogInfrastructurePostItem(this ILogger logger, string message)
        => logger.Log(LogLevel.Information, CreateEventId(null, InfrastructureEventIds.PostItem), null, message, Array.Empty<object>());

    public static void LogInfrastructurePostItem(this ILogger logger, string message, int? eventId)
        => logger.Log(LogLevel.Information, CreateEventId(eventId, InfrastructureEventIds.PostItem), null, message, Array.Empty<object>());

    public static void LogInfrastructurePutItem(this ILogger logger, string message)
        => logger.Log(LogLevel.Information, CreateEventId(null, InfrastructureEventIds.PutItem), null, message, Array.Empty<object>());

    public static void LogInfrastructurePutItem(this ILogger logger, string message, int? eventId)
        => logger.Log(LogLevel.Information, CreateEventId(eventId, InfrastructureEventIds.PutItem), null, message, Array.Empty<object>());

    public static void LogInfrastructureDeleteItem(this ILogger logger, string message)
        => logger.Log(LogLevel.Information, CreateEventId(null, InfrastructureEventIds.DeleteItem), null, message, Array.Empty<object>());

    public static void LogInfrastructureDeleteItem(this ILogger logger, string message, int? eventId)
        => logger.Log(LogLevel.Information, CreateEventId(eventId, InfrastructureEventIds.DeleteItem), null, message, Array.Empty<object>());

    public static void LogInfrastructurePutStatus(this ILogger logger, string message)
        => logger.Log(LogLevel.Information, CreateEventId(null, InfrastructureEventIds.PutStatus), null, message, Array.Empty<object>());

    public static void LogInfrastructurePutStatus(this ILogger logger, string message, int? eventId)
        => logger.Log(LogLevel.Information, CreateEventId(eventId, InfrastructureEventIds.PutStatus), null, message, Array.Empty<object>());

    public static void LogInfrastructureGetItemError(this ILogger logger, string message)
        => logger.Log(LogLevel.Error, CreateEventId(null, InfrastructureEventIds.GetItemError), null, message, Array.Empty<object>());

    public static void LogInfrastructureGetItemError(this ILogger logger, string message, int? eventId)
        => logger.Log(LogLevel.Error, CreateEventId(eventId, InfrastructureEventIds.GetItemError), null, message, Array.Empty<object>());

    public static void LogInfrastructureGetItemsError(this ILogger logger, string message)
        => logger.Log(LogLevel.Error, CreateEventId(null, InfrastructureEventIds.GetItemsError), null, message, Array.Empty<object>());

    public static void LogInfrastructureGetItemsError(this ILogger logger, string message, int? eventId)
        => logger.Log(LogLevel.Error, CreateEventId(eventId, InfrastructureEventIds.GetItemsError), null, message, Array.Empty<object>());

    public static void LogInfrastructurePutItemError(this ILogger logger, string message)
        => logger.Log(LogLevel.Error, CreateEventId(null, InfrastructureEventIds.PutItemError), null, message, Array.Empty<object>());

    public static void LogInfrastructurePutItemError(this ILogger logger, string message, int? eventId)
        => logger.Log(LogLevel.Error, CreateEventId(eventId, InfrastructureEventIds.PutItemError), null, message, Array.Empty<object>());

    public static void LogInfrastructurePostItemError(this ILogger logger, string message)
        => logger.Log(LogLevel.Error, CreateEventId(null, InfrastructureEventIds.PostItemError), null, message, Array.Empty<object>());

    public static void LogInfrastructurePostItemError(this ILogger logger, string message, int? eventId)
        => logger.Log(LogLevel.Error, CreateEventId(eventId, InfrastructureEventIds.PostItemError), null, message, Array.Empty<object>());

    public static void LogInfrastructurePutStatusError(this ILogger logger, string message)
        => logger.Log(LogLevel.Error, CreateEventId(null, InfrastructureEventIds.PutStatusError), null, message, Array.Empty<object>());

    public static void LogInfrastructurePutStatusError(this ILogger logger, string message, int? eventId)
        => logger.Log(LogLevel.Error, CreateEventId(eventId, InfrastructureEventIds.PutStatusError), null, message, Array.Empty<object>());

    public static void LogInfrastructureDeleteItemError(this ILogger logger, string message)
        => logger.Log(LogLevel.Error, CreateEventId(null, InfrastructureEventIds.DeleteItemError), null, message, Array.Empty<object>());

    public static void LogInfrastructureDeleteItemError(this ILogger logger, string message, int? eventId)
        => logger.Log(LogLevel.Error, CreateEventId(eventId, InfrastructureEventIds.DeleteItemError), null, message, Array.Empty<object>());

    private static EventId CreateEventId(int? eventId, int fallbackId)
    {
        var effectiveEventId = eventId ?? fallbackId;
        return new EventId(effectiveEventId, nameof(InfrastructureLogMessages));
    }
}

