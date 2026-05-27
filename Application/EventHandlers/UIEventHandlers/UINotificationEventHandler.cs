using SMS_Domain.Events;

namespace SMS_Application.EventHandlers;

/// <summary>
/// UI Event Handler for UINotificationEvent
/// Handles displaying toast/popup notifications in the UI
/// </summary>
public class UINotificationEventHandler : BaseUIEventHandler<UINotificationEvent>
{
    private readonly ILogger<UINotificationEventHandler> _logger;

    public UINotificationEventHandler(ILogger<UINotificationEventHandler> logger)
        : base(logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task<Result> ProcessUIEventAsync(UINotificationEvent uiEvent, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("[UI HANDLER] Processing notification: {Severity} - {Title} - {Message} (ReportId: {ReportId})",
                uiEvent.Severity, uiEvent.Title, uiEvent.Message, uiEvent.ReportId);

            // Here you would call your NotificationService or UI logic
            _logger.LogInformation("[UI HANDLER] Notification Details: Severity={Severity}, Title={Title}, Message={Message}, Duration={Duration}, Category={Category}, Target={TargetComponent}, ReportId={ReportId}",
                uiEvent.Severity, uiEvent.Title, uiEvent.Message, uiEvent.Duration, uiEvent.Category, uiEvent.TargetComponent, uiEvent.ReportId);

            if (uiEvent.Metadata != null && uiEvent.Metadata.Count > 0)
            {
                _logger.LogInformation("[UI HANDLER] Metadata: {Metadata}",
                    System.Text.Json.JsonSerializer.Serialize(uiEvent.Metadata));
            }

            await Task.Delay(100, cancellationToken); // Simulate UI update
            _logger.LogInformation("[UI HANDLER] Successfully processed UI notification for {EventType}", uiEvent.EventType);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UI HANDLER] Failed to process UI notification event: {EventType}", uiEvent.EventType);
            return Result.Failure(new Error("UI_EVENT_PROCESSING_FAILED", $"UI notification event processing failed: {ex.Message}"));
        }
    }
}
