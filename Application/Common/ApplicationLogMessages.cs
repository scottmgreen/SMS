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

    private static readonly Action<ILogger, int, string, Exception?> _LogInformation =
            LoggerMessage.Define<int, string>(LogLevel.Information, LoggingEventIds.SMS_ApplicationEventIds.Information, "{message} {ApplicationEventId} ");

    public static void LogApplicationInformation(this ILogger logger, int? ApplicationEventId, string message, params object[] args)
    {
        _LogInformation(logger, LoggingEventIds.SMS_ApplicationEventIds.Information, message, null);
    }


    private static readonly Action<ILogger, string, int, Exception?> _LogDebug =
            LoggerMessage.Define<string, int>(LogLevel.Debug, LoggingEventIds.SMS_ApplicationEventIds.Debug, "{message} {ApplicationEventId} ");

    public static void LogApplicationDebug(this ILogger logger, string message, int? ApplicationEventId)
    {
        _LogDebug(logger, message, LoggingEventIds.SMS_ApplicationEventIds.Debug, null);
    }


    private static readonly Action<ILogger, string, int, Exception?> _LogError =
            LoggerMessage.Define<string, int>(LogLevel.Error, LoggingEventIds.SMS_ApplicationEventIds.Error, "{message} {ApplicationEventId} ");

    public static void LogApplicationError(this ILogger logger, string message, int? ApplicationEventId, Exception ex)
    {
        _LogError(logger, message, LoggingEventIds.SMS_ApplicationEventIds.Error, ex);
    }

    private static readonly Action<ILogger, string, int, Exception?> _LogCritical =
            LoggerMessage.Define<string, int>(LogLevel.Error, LoggingEventIds.SMS_ApplicationEventIds.Error, "{message} {ApplicationEventId} ");

    public static void LogApplicationCritical(this ILogger logger, string message, int? ApplicationEventId, Exception ex)
    {
        _LogCritical(logger, message, LoggingEventIds.SMS_ApplicationEventIds.Error, ex);
    }
    private static readonly Action<ILogger, string, int, Exception?> _LogNone =
            LoggerMessage.Define<string, int>(LogLevel.None, LoggingEventIds.SMS_ApplicationEventIds.None, "{message} {ApplicationEventId} ");

    public static void LogApplicationNone(this ILogger logger, string message, int? ApplicationEventId)
    {
        _LogNone(logger, message, LoggingEventIds.SMS_ApplicationEventIds.None, null);
    }

    private static readonly Action<ILogger, string, int, Exception?> _LogTrace =
            LoggerMessage.Define<string, int>(LogLevel.Trace, LoggingEventIds.SMS_ApplicationEventIds.Trace, "{message} {ApplicationEventId} ");

    public static void LogApplicationTrace(this ILogger logger, string message, int? ApplicationEventId)
    {
        _LogTrace(logger, message, LoggingEventIds.SMS_ApplicationEventIds.Trace, null);
    }

    private static readonly Action<ILogger, string, int, Exception?> _LogWarning =
            LoggerMessage.Define<string, int>(LogLevel.Warning, LoggingEventIds.SMS_ApplicationEventIds.Warning, "{message} {ApplicationEventId} ");

    public static void LogApplicationWarning(this ILogger logger, string message, int? ApplicationEventId)
    {
        _LogWarning(logger, message, LoggingEventIds.SMS_ApplicationEventIds.Warning, null);
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
