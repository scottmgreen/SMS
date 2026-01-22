namespace SMS_Infrastructure.Common;


public static class InfrastructureLogMessages
{

    private static readonly Action<ILogger, int, string, Exception?> _LogInformation =
            LoggerMessage.Define<int, string>(LogLevel.Information, InfrastructureEventIds.Information, "{message} {EventId} ");

    public static void LogInfrastructureInformation(this ILogger logger, int? EventId, string message, params object[] args)
    {
        var eventId = EventId ?? InfrastructureEventIds.Information;
        _LogInformation(logger, eventId, message, null);
    }


    private static readonly Action<ILogger, string, int, Exception?> _LogDebug =
            LoggerMessage.Define<string, int>(LogLevel.Debug, InfrastructureEventIds.Debug, "{message} {EventId} ");

    public static void LogInfrastructureDebug(this ILogger logger, string message, int? EventId)
    {
        _LogDebug(logger, message, InfrastructureEventIds.Debug, null);
    }


    private static readonly Action<ILogger, string, int, Exception?> _LogError =
            LoggerMessage.Define<string, int>(LogLevel.Error, InfrastructureEventIds.Error, "{message} {EventId} ");

    public static void LogInfrastructureError(this ILogger logger, string message, int? EventId, Exception ex)
    {
        var eventId = EventId ?? InfrastructureEventIds.Error;
        _LogError(logger, message, eventId, ex);
    }
    private static readonly Action<ILogger, string, int, Exception?> _LogCritical =
            LoggerMessage.Define<string, int>(LogLevel.Error, InfrastructureEventIds.Critical, "{message} {EventId} ");

    public static void LogInfrastructureCritical(this ILogger logger, string message, int? EventId, Exception ex)
    {
        var eventId = EventId ?? InfrastructureEventIds.Critical;
        _LogCritical(logger, message, eventId, ex);
    }

    private static readonly Action<ILogger, string, int, Exception?> _LogNone =
            LoggerMessage.Define<string, int>(LogLevel.None, InfrastructureEventIds.Error, "{message} {EventId} ");

    public static void LogInfrastructureNone(this ILogger logger, string message, int? EventId)
    {
        var eventId = EventId ?? InfrastructureEventIds.None;
        _LogNone(logger, message, eventId, null);
    }

    private static readonly Action<ILogger, string, int, Exception?> _LogTrace =
            LoggerMessage.Define<string, int>(LogLevel.Trace, InfrastructureEventIds.Trace, "{message} {EventId} ");

    public static void LogInfrastructureTrace(this ILogger logger, string message, int? EventId)
    {
        var eventId = EventId ?? InfrastructureEventIds.Trace;
        _LogTrace(logger, message, eventId, null);
    }

    private static readonly Action<ILogger, string, int, Exception?> _LogWarning =
            LoggerMessage.Define<string, int>(LogLevel.Warning, InfrastructureEventIds.Warning, "{message} {EventId} ");

    public static void LogInfrastructureWarning(this ILogger logger, string message, int? EventId)
    {
        var eventId = EventId ?? InfrastructureEventIds.Warning;
        _LogWarning(logger, message, eventId, null);
    }



    private static readonly Action<ILogger, string, int, Exception?> _InfrastructureEvent =
            LoggerMessage.Define<string, int>(LogLevel.Information, InfrastructureEventIds.InfrastructureEvent, "{message} {EventId}");

    public static void LogInfrastructureEvent(this ILogger logger, string message, int? EventId)
    {
        _InfrastructureEvent(logger, message, InfrastructureEventIds.InfrastructureEvent, null);
    }

    private static readonly Action<ILogger, string, int, Exception?> _GetItems =
        LoggerMessage.Define<string, int>(LogLevel.Information, InfrastructureEventIds.GetItems, "{message} {EventId}");

    public static void LogInfrastructureGetItems(this ILogger logger, string message, int? EventId)
    {
        _GetItems(logger, message, InfrastructureEventIds.GetItems, null);
    }

    private static readonly Action<ILogger, string, int, Exception?> _GetItem =
        LoggerMessage.Define<string, int>(LogLevel.Information, InfrastructureEventIds.GetItem, "{message} {EventId}");

    public static void LogInfrastructureGetItem(this ILogger logger, string message, int? EventId)
    {
        _GetItem(logger, message, InfrastructureEventIds.GetItem, null);
    }
    private static readonly Action<ILogger, string, int, Exception?> _PostItem =
        LoggerMessage.Define<string, int>(LogLevel.Information, InfrastructureEventIds.PostItem, "{message} {EventId}");

    public static void LogInfrastructurePostItem(this ILogger logger, string message, int? EventId)
    {
        _PostItem(logger, message, InfrastructureEventIds.PostItem, null);
    }
    private static readonly Action<ILogger, string, int, Exception?> _PutItem =
        LoggerMessage.Define<string, int>(LogLevel.Information, InfrastructureEventIds.PutItem, "{message} {EventId}");

    public static void LogInfrastructurePutItem(this ILogger logger, string message, int? EventId)
    {
        _PutItem(logger, message, InfrastructureEventIds.PutItem, null);
    }
    private static readonly Action<ILogger, string, int, Exception?> _DeleteItem =
        LoggerMessage.Define<string, int>(LogLevel.Information, InfrastructureEventIds.DeleteItem, "{message} {EventId}");

    public static void LogInfrastructureDeleteItem(this ILogger logger, string message, int? EventId)
    {
        _DeleteItem(logger, message, InfrastructureEventIds.DeleteItem, null);
    }
    private static readonly Action<ILogger, string, int, Exception?> _PutStatus =
        LoggerMessage.Define<string, int>(LogLevel.Information, InfrastructureEventIds.PutStatus, "{message} {EventId}");

    public static void LogInfrastructurePutStatus(this ILogger logger, string message, int? eventId)
    {
        _PutStatus(logger, message, InfrastructureEventIds.PutStatus, null);
    }
    private static readonly Action<ILogger, string, int, Exception?> _GetItemError =
        LoggerMessage.Define<string, int>(LogLevel.Error, InfrastructureEventIds.GetItemError, "{message} {EventId}");

    public static void LogInfrastructureGetItemError(this ILogger logger, string message, int? eventId)
    {
        _GetItemError(logger, message, InfrastructureEventIds.GetItemError, null);
    }
    private static readonly Action<ILogger, string, int, Exception?> _GetItemsError =
            LoggerMessage.Define<string, int>(LogLevel.Error, InfrastructureEventIds.GetItemsError, "{message} {EventId}");

    public static void LogInfrastructureGetItemsError(this ILogger logger, string message, int? eventId)
    {
        _GetItemsError(logger, message, InfrastructureEventIds.GetItemsError, null);
    }

    private static readonly Action<ILogger, string, int, Exception?> _PutItemError =
            LoggerMessage.Define<string, int>(LogLevel.Error, InfrastructureEventIds.PutItemError, "{message} {EventId}");

    public static void LogInfrastructurePutItemError(this ILogger logger, string message, int? eventId)
    {
        _PutItemError(logger, message, InfrastructureEventIds.PutItemError, null);
    }
    private static readonly Action<ILogger, string, int, Exception?> _PostItemError =
            LoggerMessage.Define<string, int>(LogLevel.Error, InfrastructureEventIds.PostItemError, "{message} {EventId}");

    public static void LogInfrastructurePostItemError(this ILogger logger, string message, int? eventId)
    {
        _PostItemError(logger, message, InfrastructureEventIds.PostItemError, null);
    }
    private static readonly Action<ILogger, string, int, Exception?> _PutStatusError =
            LoggerMessage.Define<string, int>(LogLevel.Error, InfrastructureEventIds.PutStatusError, "{message} {EventId}");

    public static void LogInfrastructurePutStatusError(this ILogger logger, string message, int? eventId)
    {
        _PutStatusError(logger, message, InfrastructureEventIds.PutStatusError, null);
    }
    private static readonly Action<ILogger, string, int, Exception?> _DeleteItemError =
            LoggerMessage.Define<string, int>(LogLevel.Error, InfrastructureEventIds.DeleteItemError, "{message} {EventId}");

    public static void LogInfrastructureDeleteItemError(this ILogger logger, string message, int? EventId)
    {
        _DeleteItemError(logger, message, InfrastructureEventIds.DeleteItemError, null);
    }
    //private static readonly Action<ILogger, string, int, Exception?> _InfrastructureError =
    //        LoggerMessage.Define<string, int>(LogLevel.Warning, LoggingEventIds.SMS_InfrastructureEventIds.ApplicationError, "Infrastructure Warning: {message} {EventId}");

    ////public static void LogInfrastructureError(this ILogger logger, string message, int eventId)
    //{
    //    _InfrastructureError(logger, message, eventId, null);
    //}












}
