using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Radzen;
using SMS3.Components.Shared.UIHelpers;
using SMS3.Configuration;

namespace SMS3.Components.Shared;

/// <summary>
/// Base component class that provides easy notification methods with configuration support
/// </summary>
public class BaseNotificationComponent : ComponentBase
{
    [Inject] protected NotificationService NotificationService { get; set; } = default!;
    [Inject] protected IOptions<NotificationSettings> NotificationOptions { get; set; } = default!;

    protected NotificationSettings Settings => NotificationOptions.Value;

    /// <summary>
    /// Show success notification (respects configuration)
    /// </summary>
    protected void ShowSuccessNotification(string message, int duration = 4000)
    {
        NotificationHelper.ShowSuccess(NotificationService, message, duration, Settings);
    }

    /// <summary>
    /// Show error notification (respects configuration)
    /// </summary>
    protected void ShowErrorNotification(string message, int duration = 6000)
    {
        NotificationHelper.ShowError(NotificationService, message, duration, Settings);
    }

    /// <summary>
    /// Show info notification (respects configuration)
    /// </summary>
    protected void ShowInfoNotification(string message, int duration = 4000)
    {
        NotificationHelper.ShowInfo(NotificationService, message, duration, Settings);
    }

    /// <summary>
    /// Show warning notification (respects configuration)
    /// </summary>
    protected void ShowWarningNotification(string message, int duration = 5000)
    {
        NotificationHelper.ShowWarning(NotificationService, message, duration, Settings);
    }
}