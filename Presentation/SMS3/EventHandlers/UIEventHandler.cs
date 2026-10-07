using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Radzen;
using SMS_Application.Interfaces;
using SMS_Domain.Common;
using SMS_Domain.Events;
using SMS3.Components.Shared;
using SMS_Application.Interfaces;

namespace SMS3.EventHandlers;

/// <summary>
/// UI Event Handler that uses BlazorNotificationDispatcher for proper UI thread marshaling
/// Follows the InvokeAsync pattern from your example
/// </summary>
public class UIEventHandler : BaseUIEventHandler<UINotificationEvent>
{
    private readonly ILogger<UIEventHandler> _logger;
    private readonly IConfiguration _configuration;
    private readonly ICurrentUserService _currentUserService;

    public UIEventHandler(ILogger<UIEventHandler> logger, IConfiguration configuration, ICurrentUserService currentUserService)
        : base(logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    protected override async Task<Result> ProcessUIEventAsync(UINotificationEvent uiEvent, CancellationToken cancellationToken)
    {
        try
        {
            if (!IsNotificationEnabled(uiEvent.Severity))
            {
                _logger.LogInformation("UINotificationHandler: Skipped {Severity} notification because FeatureManagement toggle is disabled", uiEvent.Severity);
                return Result.Success();
            }

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

            var targetUserCode = uiEvent.Metadata is not null
                && uiEvent.Metadata.TryGetValue("TargetUserCode", out var targetUserValue)
                ? targetUserValue?.ToString()
                : null;

            var targetSessionKey = uiEvent.Metadata is not null
                && uiEvent.Metadata.TryGetValue("TargetSessionKey", out var targetSessionValue)
                ? targetSessionValue?.ToString()
                : null;

            // Enforce user-specific toasts when target user is supplied.
            // If no target is supplied, default to the publishing user's session identity.
            if (string.IsNullOrWhiteSpace(targetUserCode) && string.IsNullOrWhiteSpace(targetSessionKey))
            {
                targetUserCode = _currentUserService?.UserCode;
            }

            if (string.IsNullOrWhiteSpace(targetUserCode) && string.IsNullOrWhiteSpace(targetSessionKey))
            {
                _logger.LogWarning("UINotificationHandler: Dropping notification {EventId} because TargetUserCode could not be resolved.", uiEvent.EventId);
                return Result.Success();
            }

            // Use dispatcher to marshal to UI thread and limit toast to target user session(s)
            await EventBusDispatcher.DispatchNotificationAsync(
                severity, 
                uiEvent.Title, 
                uiEvent.Message, 
                uiEvent.Duration,
                targetUserCode,
                targetSessionKey
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

    private bool IsNotificationEnabled(UINotificationSeverity severity)
    {
        return severity switch
        {
            UINotificationSeverity.Error => _configuration.GetValue<bool?>("NotificationEvents:UINotificationError:Enabled") ?? true,
            UINotificationSeverity.Success => _configuration.GetValue<bool?>("NotificationEvents:UINotificationSuccess:Enabled") ?? true,
            UINotificationSeverity.Warning => _configuration.GetValue<bool?>("NotificationEvents:UINotificationWarning:Enabled") ?? true,
            UINotificationSeverity.Info => _configuration.GetValue<bool?>("NotificationEvents:UINotificationInfo:Enabled") ?? true,
            _ => true
        };
    }
}