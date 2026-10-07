namespace SMS_Application.Interfaces;

using SMS_Domain.Enums;

public interface IEmailNotificationExecutionModeResolver
{
    bool IsEnabled(string? notificationType, bool defaultValue = true);

    NotificationExecutionMode ResolveExecutionMode(string? notificationType, NotificationExecutionMode fallbackMode);
}
