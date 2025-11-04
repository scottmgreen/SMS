
using SMS_Shared.Common;

using Microsoft.Extensions.Logging;

namespace SMS_Shared.Configuration;



public static class LogMessages
{

    private static readonly Action<ILogger,int, string,  Exception?> _CBTLoggerInformation =
            LoggerMessage.Define<int,string >(LogLevel.Information, LoggingEventIds.SMS_ApplicationEventIds.Information, "{message} {CBTEventId} ");

    public static void CBTLogInformation(this ILogger logger, int CBTEventId, string message, params object[] args )
    {
        _CBTLoggerInformation(logger, CBTEventId, message,  null);
    }


    private static readonly Action<ILogger, string, int, Exception?> _CBTLoggerDebug =
            LoggerMessage.Define<string, int>(LogLevel.Debug, LoggingEventIds.SMS_ApplicationEventIds.Debug, "{message} {CBTEventId} ");

    public static void CBTLoggerDebug(this ILogger logger, string message, int CBTEventId)
    {
        _CBTLoggerDebug(logger, message, CBTEventId, null);
    }


    private static readonly Action<ILogger, string, int, Exception?> _CBTLoggerError =
            LoggerMessage.Define<string, int>(LogLevel.Error, LoggingEventIds.SMS_ApplicationEventIds.Error, "{message} {CBTEventId} ");

    public static void CBTLoggerError(this ILogger logger, string message, int CBTEventId)
    {
        _CBTLoggerError(logger, message, CBTEventId, null);
    }


    private static readonly Action<ILogger, string, int, Exception?> _CBTLoggerNone =
            LoggerMessage.Define<string, int>(LogLevel.None, LoggingEventIds.SMS_ApplicationEventIds.Error, "{message} {CBTEventId} ");

    public static void CBTLoggerNone(this ILogger logger, string message, int CBTEventId)
    {
        _CBTLoggerNone(logger, message, CBTEventId, null);
    }

    private static readonly Action<ILogger, string, int, Exception?> _CBTLoggerTrace =
            LoggerMessage.Define<string, int>(LogLevel.Trace, LoggingEventIds.SMS_ApplicationEventIds.Trace, "{message} {CBTEventId} ");

    public static void CBTLoggerTrace(this ILogger logger, string message, int CBTEventId)
    {
        _CBTLoggerTrace(logger, message, CBTEventId, null);
    }

    private static readonly Action<ILogger, string, int, Exception?> _CBTLoggerWarning =
            LoggerMessage.Define<string, int>(LogLevel.Warning, LoggingEventIds.SMS_ApplicationEventIds.Warning, "{message} {CBTEventId} ");

    public static void CBTLoggerWarning(this ILogger logger, string message, int CBTEventId)
    {
        _CBTLoggerWarning(logger, message, CBTEventId, null);
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
