namespace SMS_Application.Interfaces;

public interface IEmailNotificationExecutionModeResolver
{
    bool IsEnabled(string? notificationType, bool defaultValue = true);

    EventExecutionMode ResolveExecutionMode(string? notificationType, EventExecutionMode fallbackMode);
}
