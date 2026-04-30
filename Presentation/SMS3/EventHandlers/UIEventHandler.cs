using Microsoft.Extensions.Logging;
using Radzen;
using SMS_Application.Interfaces;
using SMS_Domain.Common;
using SMS_Domain.Events.UIEvents;
using SMS3.Components.Shared;

namespace SMS3.EventHandlers;

/// <summary>
/// UI Event Handler that uses BlazorNotificationDispatcher for proper UI thread marshaling
/// Follows the InvokeAsync pattern from your example
/// </summary>
public class UIEventHandler : BaseUIEventHandler<UINotificationEvent>
{
    private readonly ILogger<UIEventHandler> _logger;

    public UIEventHandler(ILogger<UIEventHandler> logger)
        : base(logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task<Result> ProcessUIEventAsync(UINotificationEvent uiEvent, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("UINotificationHandler: Processing {Severity} notification - {Title}: {Message}", 
                uiEvent.Severity, uiEvent.Title, uiEvent.Message);

            // Map to Radzen enum
            var severity = uiEvent.Severity switch
            {
                UINotificationSeverity.Success => NotificationSeverity.Success,
                UINotificationSeverity.Error => NotificationSeverity.Error,
                UINotificationSeverity.Warning => NotificationSeverity.Warning,
                UINotificationSeverity.Info => NotificationSeverity.Info,
                _ => NotificationSeverity.Info
            };

            // Use BlazorNotificationDispatcher to properly marshal to UI thread
            await EventBusDispatcher.DispatchNotificationAsync(
                severity, 
                uiEvent.Title, 
                uiEvent.Message, 
                uiEvent.Duration
            );

            _logger.LogInformation("UINotificationHandler: Notification dispatched to Blazor components via InvokeAsync pattern");

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UINotificationHandler: Failed to dispatch notification");
            return Result.Failure(new Error("UI_NOTIFICATION_FAILED", $"Failed to dispatch notification: {ex.Message}"));
        }
    }
}