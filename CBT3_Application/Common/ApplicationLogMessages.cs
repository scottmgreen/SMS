using CBT3_Shared.Common;

using Microsoft.Extensions.Logging;

namespace CBT3_Application.Common;



public static class ApplicationLogMessages
{

    private static readonly Action<ILogger, int, string, Exception?> _LogInformation =
            LoggerMessage.Define<int, string>(LogLevel.Information, LoggingEventIds.CBT3_ApplicationEventIds.Information, "{message} {CBTEventId} ");

    public static void LogApplicationInformation(this ILogger logger, int? CBTEventId, string message, params object[] args)
    {
        _LogInformation(logger, LoggingEventIds.CBT3_ApplicationEventIds.Information, message, null);
    }


    private static readonly Action<ILogger, string, int, Exception?> _LogDebug =
            LoggerMessage.Define<string, int>(LogLevel.Debug, LoggingEventIds.CBT3_ApplicationEventIds.Debug, "{message} {CBTEventId} ");

    public static void LogApplicationDebug(this ILogger logger, string message, int? CBTEventId)
    {
        _LogDebug(logger, message, LoggingEventIds.CBT3_ApplicationEventIds.Debug, null);
    }


    private static readonly Action<ILogger, string, int, Exception?> _LogError =
            LoggerMessage.Define<string, int>(LogLevel.Error, LoggingEventIds.CBT3_ApplicationEventIds.Error, "{message} {CBTEventId} ");

    public static void LogApplicationError(this ILogger logger, string message, int? CBTEventId, Exception ex)
    {
        _LogError(logger, message, LoggingEventIds.CBT3_ApplicationEventIds.Error, ex);
    }

    private static readonly Action<ILogger, string, int, Exception?> _LogCritical =
            LoggerMessage.Define<string, int>(LogLevel.Error, LoggingEventIds.CBT3_ApplicationEventIds.Error, "{message} {CBTEventId} ");

    public static void LogApplicationCritical(this ILogger logger, string message, int? CBTEventId, Exception ex)
    {
        _LogCritical(logger, message, LoggingEventIds.CBT3_ApplicationEventIds.Error, ex);
    }
    private static readonly Action<ILogger, string, int, Exception?> _LogNone =
            LoggerMessage.Define<string, int>(LogLevel.None, LoggingEventIds.CBT3_ApplicationEventIds.None, "{message} {CBTEventId} ");

    public static void LogApplicationNone(this ILogger logger, string message, int? CBTEventId)
    {
        _LogNone(logger, message, LoggingEventIds.CBT3_ApplicationEventIds.None, null);
    }

    private static readonly Action<ILogger, string, int, Exception?> _LogTrace =
            LoggerMessage.Define<string, int>(LogLevel.Trace, LoggingEventIds.CBT3_ApplicationEventIds.Trace, "{message} {CBTEventId} ");

    public static void LogApplicationTrace(this ILogger logger, string message, int? CBTEventId)
    {
        _LogTrace(logger, message, LoggingEventIds.CBT3_ApplicationEventIds.Trace, null);
    }

    private static readonly Action<ILogger, string, int, Exception?> _LogWarning =
            LoggerMessage.Define<string, int>(LogLevel.Warning, LoggingEventIds.CBT3_ApplicationEventIds.Warning, "{message} {CBTEventId} ");

    public static void LogApplicationWarning(this ILogger logger, string message, int? CBTEventId)
    {
        _LogWarning(logger, message, LoggingEventIds.CBT3_ApplicationEventIds.Warning, null);
    }












    //private static readonly Action<ILogger, int, Exception?> LoggerMessageDebug =
    //    LoggerMessage.Define<int>(LogLevel.Debug, Common.LoggingEventIds.CBT3_ApplicationEventIds.Debug, "General Log Debug : O {error} !");

    //public static void LogMessageDebug(this ILogger logger, int error)
    //{
    //    LoggerMessageDebug(logger, error, null);
    //}

    //private static readonly Action<ILogger, int, Exception?> LoggerMessageError =
    //    LoggerMessage.Define<int>(LogLevel.Error, Common.LoggingEventIds.CBT3_ApplicationEventIds.Error, "General Log Error : O {error} !");

    //public static void LogMessageError(this ILogger logger, int error)
    //{
    //    LoggerMessageError(logger, error, null);
    //}

    //private static readonly Action<ILogger, int, Exception?> LoggerMessageInformation =
    //    LoggerMessage.Define<int>(LogLevel.Information, Common.LoggingEventIds.CBT3_ApplicationEventIds.Information, "General Log Information : O {error} !");

    //public static void LogMessageInformation(this ILogger logger, int error)
    //{
    //    LoggerMessageInformation(logger, error, null);
    //}

    //private static readonly Action<ILogger, int, Exception?> LoggerMessageNone =
    //    LoggerMessage.Define<int>(LogLevel.None, Common.LoggingEventIds.CBT3_ApplicationEventIds.None, "General Log None : O {error} !");

    //public static void LogMessageNone(this ILogger logger, int error)
    //{
    //    LoggerMessageNone(logger, error, null);
    //}

    //private static readonly Action<ILogger, int, Exception?> LoggerMessageTrace =
    //    LoggerMessage.Define<int>(LogLevel.Trace, Common.LoggingEventIds.CBT3_ApplicationEventIds.Trace, "General Log Trace : O {error} !");

    //public static void LogMessageTrace(this ILogger logger, int error)
    //{
    //    LoggerMessageTrace(logger, error, null);
    //}

    //private static readonly Action<ILogger, int, Exception?> LoggerMessageWarning =
    //    LoggerMessage.Define<int>(LogLevel.Warning, Common.LoggingEventIds.CBT3_ApplicationEventIds.Warning, "General Log Warning : O {error} !");

    //public static void LogMessageWarning(this ILogger logger, int error)
    //{
    //    LoggerMessageWarning(logger, error, null);
    //}

}
