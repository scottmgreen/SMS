using SMS_Application.Interfaces;
using SMS_Domain.Common;
using SMS3.Components.Pages.SMSSystem.Models;

namespace SMS3.Components.Pages.SMSSystem.Services;

public interface IEmailNotificationSettingsService
{
    IReadOnlyList<EmailNotificationSetting> GetAllNotificationSettings();

    EventExecutionMode ResolveExecutionMode(string? notificationType, EventExecutionMode fallbackMode);

    Task<Result> SaveNotificationSettingsAsync(IEnumerable<EmailNotificationSetting> settings, CancellationToken cancellationToken = default);
}
