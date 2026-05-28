//-----------------------------------------------------------------------
// <copyright file="ApplicationLogMessages.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Provides structured logging extension methods for the Application layer.
//                  Implements high-performance logging using LoggerMessage delegates for
//                  consistent message formatting and optimal performance.
// </copyright>
//-----------------------------------------------------------------------



namespace SMS_Application.Common;

/// <summary>
/// Application Layer Logging Extensions
/// Provides structured logging extension methods with high-performance LoggerMessage delegates.
/// Ensures consistent log message formatting and optimal performance across application services.
/// 
/// Methods:
/// - LogApplicationInformation: Logs informational messages with event ID
/// - LogApplicationDebug: Logs debug messages for troubleshooting
/// - LogApplicationError: Logs error conditions with exception details
/// - LogApplicationCritical: Logs critical system events requiring immediate attention
/// - LogApplicationNone: Logs messages without specific severity level
/// - LogApplicationTrace: Logs detailed execution flow information
/// - LogApplicationWarning: Logs warning conditions that don't halt execution
/// </summary>
public static class ApplicationLogMessages
{
    public static void LogApplicationInformation(this ILogger logger, string message, int? applicationEventId = null, params object[] args)
    {
        LogWithOptionalEventIdFallback(logger, LogLevel.Information, applicationEventId, ApplicationEventIds.Information, null, message, args);
    }

    public static void LogApplicationInformation(this ILogger logger, string message, params object[] args)
    {
        logger.Log(LogLevel.Information, CreateEventId(null, ApplicationEventIds.Information), null, message, args);
    }

    public static void LogApplicationInformation(this ILogger logger, int? applicationEventId, string message, params object[] args)
    {
        logger.Log(LogLevel.Information, CreateEventId(applicationEventId, ApplicationEventIds.Information), null, message, args);
    }

    public static void LogApplicationInformation(this ILogger logger, Exception ex, string message, params object[] args)
    {
        logger.Log(LogLevel.Information, CreateEventId(null, ApplicationEventIds.Information), ex, message, args);
    }

    public static void LogApplicationInformation(this ILogger logger, Exception ex, string message, int? applicationEventId, params object[] args)
    {
        logger.Log(LogLevel.Information, CreateEventId(applicationEventId, ApplicationEventIds.Information), ex, message, args);
    }

    public static void LogApplicationDebug(this ILogger logger, string message, int? applicationEventId = null, params object[] args)
    {
        LogWithOptionalEventIdFallback(logger, LogLevel.Debug, applicationEventId, ApplicationEventIds.Debug, null, message, args);
    }

    public static void LogApplicationDebug(this ILogger logger, string message, params object[] args)
    {
        logger.Log(LogLevel.Debug, CreateEventId(null, ApplicationEventIds.Debug), null, message, args);
    }

    public static void LogApplicationDebug(this ILogger logger, Exception ex, string message, params object[] args)
    {
        logger.Log(LogLevel.Debug, CreateEventId(null, ApplicationEventIds.Debug), ex, message, args);
    }

    public static void LogApplicationDebug(this ILogger logger, Exception ex, string message, int? applicationEventId, params object[] args)
    {
        logger.Log(LogLevel.Debug, CreateEventId(applicationEventId, ApplicationEventIds.Debug), ex, message, args);
    }

    public static void LogApplicationError(this ILogger logger, string message, int? applicationEventId = null, Exception? ex = null, params object[] args)
    {
        LogWithOptionalEventIdFallback(logger, LogLevel.Error, applicationEventId, ApplicationEventIds.Error, ex, message, args);
    }

    public static void LogApplicationError(this ILogger logger, string message, params object[] args)
    {
        logger.Log(LogLevel.Error, CreateEventId(null, ApplicationEventIds.Error), null, message, args);
    }

    public static void LogApplicationError(this ILogger logger, Exception ex, string message, params object[] args)
    {
        logger.Log(LogLevel.Error, CreateEventId(null, ApplicationEventIds.Error), ex, message, args);
    }

    public static void LogApplicationError(this ILogger logger, Exception ex, string message, int? applicationEventId, params object[] args)
    {
        logger.Log(LogLevel.Error, CreateEventId(applicationEventId, ApplicationEventIds.Error), ex, message, args);
    }

    public static void LogApplicationCritical(this ILogger logger, string message, int? applicationEventId = null, Exception? ex = null, params object[] args)
    {
        LogWithOptionalEventIdFallback(logger, LogLevel.Critical, applicationEventId, ApplicationEventIds.Critical, ex, message, args);
    }

    public static void LogApplicationCritical(this ILogger logger, string message, params object[] args)
    {
        logger.Log(LogLevel.Critical, CreateEventId(null, ApplicationEventIds.Critical), null, message, args);
    }

    public static void LogApplicationCritical(this ILogger logger, Exception ex, string message, params object[] args)
    {
        logger.Log(LogLevel.Critical, CreateEventId(null, ApplicationEventIds.Critical), ex, message, args);
    }

    public static void LogApplicationCritical(this ILogger logger, Exception ex, string message, int? applicationEventId, params object[] args)
    {
        logger.Log(LogLevel.Critical, CreateEventId(applicationEventId, ApplicationEventIds.Critical), ex, message, args);
    }

    public static void LogApplicationNone(this ILogger logger, string message, int? applicationEventId = null, params object[] args)
    {
        LogWithOptionalEventIdFallback(logger, LogLevel.None, applicationEventId, ApplicationEventIds.None, null, message, args);
    }

    public static void LogApplicationTrace(this ILogger logger, string message, int? applicationEventId = null, params object[] args)
    {
        LogWithOptionalEventIdFallback(logger, LogLevel.Trace, applicationEventId, ApplicationEventIds.Trace, null, message, args);
    }

    public static void LogApplicationTrace(this ILogger logger, string message, params object[] args)
    {
        logger.Log(LogLevel.Trace, CreateEventId(null, ApplicationEventIds.Trace), null, message, args);
    }

    public static void LogApplicationTrace(this ILogger logger, Exception ex, string message, params object[] args)
    {
        logger.Log(LogLevel.Trace, CreateEventId(null, ApplicationEventIds.Trace), ex, message, args);
    }

    public static void LogApplicationTrace(this ILogger logger, Exception ex, string message, int? applicationEventId, params object[] args)
    {
        logger.Log(LogLevel.Trace, CreateEventId(applicationEventId, ApplicationEventIds.Trace), ex, message, args);
    }

    public static void LogApplicationWarning(this ILogger logger, string message, int? applicationEventId = null, params object[] args)
    {
        LogWithOptionalEventIdFallback(logger, LogLevel.Warning, applicationEventId, ApplicationEventIds.Warning, null, message, args);
    }

    public static void LogApplicationWarning(this ILogger logger, string message, params object[] args)
    {
        logger.Log(LogLevel.Warning, CreateEventId(null, ApplicationEventIds.Warning), null, message, args);
    }

    public static void LogApplicationWarning(this ILogger logger, Exception ex, string message, params object[] args)
    {
        logger.Log(LogLevel.Warning, CreateEventId(null, ApplicationEventIds.Warning), ex, message, args);
    }

    public static void LogApplicationWarning(this ILogger logger, Exception ex, string message, int? applicationEventId, params object[] args)
    {
        logger.Log(LogLevel.Warning, CreateEventId(applicationEventId, ApplicationEventIds.Warning), ex, message, args);
    }

    private static EventId CreateEventId(int? eventId, int fallbackId)
    {
        var effectiveEventId = eventId ?? fallbackId;
        return new EventId(effectiveEventId, nameof(ApplicationLogMessages));
    }

    private static void LogWithOptionalEventIdFallback(
        ILogger logger,
        LogLevel level,
        int? applicationEventId,
        int fallbackEventId,
        Exception? ex,
        string message,
        object[] args)
    {
        var eventId = CreateEventId(applicationEventId, fallbackEventId);
        var safeArgs = args ?? Array.Empty<object>();

        if (applicationEventId.HasValue && HasMorePlaceholdersThanArgs(message, safeArgs.Length))
        {
            var adjustedArgs = new object[safeArgs.Length + 1];
            adjustedArgs[0] = applicationEventId.Value;
            Array.Copy(safeArgs, 0, adjustedArgs, 1, safeArgs.Length);
            logger.Log(level, eventId, ex, message, adjustedArgs);
            return;
        }

        try
        {
            logger.Log(level, eventId, ex, message, safeArgs);
        }
        catch (Exception logException) when (IsFormatException(logException) && applicationEventId.HasValue)
        {
            // Backward compatibility: callers may have intended structured args, but overload
            // resolution bound the first int argument to applicationEventId.
            var adjustedArgs = new object[safeArgs.Length + 1];
            adjustedArgs[0] = applicationEventId.Value;
            Array.Copy(safeArgs, 0, adjustedArgs, 1, safeArgs.Length);

            logger.Log(level, eventId, ex, message, adjustedArgs);
        }
    }

    private static bool IsFormatException(Exception exception)
    {
        if (exception is FormatException)
        {
            return true;
        }

        if (exception is AggregateException aggregateException)
        {
            return aggregateException.Flatten().InnerExceptions.Any(IsFormatException);
        }

        if (exception.InnerException is not null)
        {
            return IsFormatException(exception.InnerException);
        }

        return false;
    }

    private static bool HasMorePlaceholdersThanArgs(string message, int argsCount)
    {
        if (string.IsNullOrEmpty(message))
        {
            return false;
        }

        var placeholders = 0;
        for (var i = 0; i < message.Length; i++)
        {
            if (message[i] == '{')
            {
                // Skip escaped '{{'
                if (i + 1 < message.Length && message[i + 1] == '{')
                {
                    i++;
                    continue;
                }

                // Count only templated placeholders with a closing brace
                var closeIndex = message.IndexOf('}', i + 1);
                if (closeIndex > i + 1)
                {
                    placeholders++;
                    i = closeIndex;
                }
            }
        }

        return placeholders > argsCount;
    }

















    //private static readonly Action<ILogger, int, Exception?> LoggerMessageDebug =
    //    LoggerMessage.Define<int>(LogLevel.Debug, Common.LoggingEventIds.SMS_ApplicationEventIds.Debug, "General Log Debug : O {error} !");

    //public static void LogMessageDebug(this ILogger logger, int error)
    //{
    //    LoggerMessageDebug(logger, error, null);
    //}

    //private static readonly Action<ILogger, int, Exception?> LoggerMessageError =
    //    LoggerMessage.Define<int>(LogLevel.Error, Common.LoggingEventIds.SMS_ApplicationEventIds.Error, "General Log Error : O {error} !");

    //public static void LogMessageError(this ILogger logger, int error)
    //{
    //    LoggerMessageError(logger, error, null);
    //}

    //private static readonly Action<ILogger, int, Exception?> LoggerMessageInformation =
    //    LoggerMessage.Define<int>(LogLevel.Information, Common.LoggingEventIds.SMS_ApplicationEventIds.Information, "General Log Information : O {error} !");

    //public static void LogMessageInformation(this ILogger logger, int error)
    //{
    //    LoggerMessageInformation(logger, error, null);
    //}

    //private static readonly Action<ILogger, int, Exception?> LoggerMessageNone =
    //    LoggerMessage.Define<int>(LogLevel.None, Common.LoggingEventIds.SMS_ApplicationEventIds.None, "General Log None : O {error} !");

    //public static void LogMessageNone(this ILogger logger, int error)
    //{
    //    LoggerMessageNone(logger, error, null);
    //}

    //private static readonly Action<ILogger, int, Exception?> LoggerMessageTrace =
    //    LoggerMessage.Define<int>(LogLevel.Trace, Common.LoggingEventIds.SMS_ApplicationEventIds.Trace, "General Log Trace : O {error} !");

    //public static void LogMessageTrace(this ILogger logger, int error)
    //{
    //    LoggerMessageTrace(logger, error, null);
    //}

    //private static readonly Action<ILogger, int, Exception?> LoggerMessageWarning =
    //    LoggerMessage.Define<int>(LogLevel.Warning, Common.LoggingEventIds.SMS_ApplicationEventIds.Warning, "General Log Warning : O {error} !");

    //public static void LogMessageWarning(this ILogger logger, int error)
    //{
    //    LoggerMessageWarning(logger, error, null);
    //}

}
