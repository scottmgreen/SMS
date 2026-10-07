using SMS_Application.Interfaces;
using SMS_Domain.Common;
using SMS_Domain.Entities;
using SMS_Domain.Enums;

namespace SMS3.Components.Pages.SMSSystem.Services;

public interface IEmailNotificationSettingsService
{
    IReadOnlyList<NotificationSetting> GetAllNotificationSettings();

    NotificationExecutionMode ResolveExecutionMode(string? notificationType, NotificationExecutionMode fallbackMode);

    Task<Result> SaveNotificationSettingsAsync(IEnumerable<NotificationSetting> settings, CancellationToken cancellationToken = default);
}
