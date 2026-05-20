using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;
using SMS_Domain.Common;
using SMS_Domain.Events.UIEvents;
using SMS_Domain.Enums;
using SMS_Domain.Interfaces;

namespace SMS_Application.EventHandlers.UIEventHandlers;

/// <summary>
/// UI Event Handler for Hazard Created Notifications
/// DEMONSTRATION: Shows how UI events use NotificationService for actual user notifications
/// </summary>
public class HazardCreatedNotificationHandler : BaseUIEventHandler<UINotificationEvent>
{
    private readonly ILogger<HazardCreatedNotificationHandler> _logger;

    public HazardCreatedNotificationHandler(ILogger<HazardCreatedNotificationHandler> logger)
        : base(logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes UI events by triggering actual user interface notifications
    /// THIS IS WHERE NOTIFICATIONSERVICE SHOULD BE USED - for actual UI events!
    /// </summary>
    protected override async Task<Result> ProcessUIEventAsync(UINotificationEvent uiEvent, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("[UI HANDLER] Processing notification: {EventType} - {Message}", uiEvent.EventType, uiEvent.Message);

            // This is where you'd use NotificationService in a real implementation
            // For demonstration, we'll show what would happen
            var severity = GetNotificationSeverity(uiEvent.Priority);
            var summary = GetNotificationSummary(uiEvent.EventType, uiEvent.Priority);
            var detail = !string.IsNullOrWhiteSpace(uiEvent.ReportId)
                ? $"{uiEvent.Message} (Report: {uiEvent.ReportId})"
                : uiEvent.Message;

            _logger.LogInformation("[UI HANDLER] NOTIFICATION:");
            _logger.LogInformation("Severity: {Severity}", severity);
            _logger.LogInformation("Summary: {Summary}", summary);
            _logger.LogInformation("Detail: {Detail}", detail);
            _logger.LogInformation("Target: {TargetComponent}", uiEvent.TargetComponent);
            _logger.LogInformation("ReportId: {ReportId}", uiEvent.ReportId);

            // Log the full notification data for demonstration
            if (uiEvent.Metadata != null)
            {
                _logger.LogInformation("[UI HANDLER] Notification Data: {Data}", System.Text.Json.JsonSerializer.Serialize(uiEvent.Metadata, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
            }

            // Simulate notification processing
            await Task.Delay(100, cancellationToken); // Simulate UI update time

            _logger.LogInformation("[UI HANDLER] Successfully processed UI notification for {EventType}", uiEvent.EventType);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UI HANDLER] Failed to process UI event: {EventType}", uiEvent.EventType);
            return Result.Failure(new Error("UI_EVENT_PROCESSING_FAILED", $"UI event processing failed: {ex.Message}"));
        }
    }

    #region Helper Methods

    /// <summary>
    /// Maps UI event priority to notification severity
    /// </summary>
    private string GetNotificationSeverity(UIEventPriority priority)
    {
        return priority switch
        {
            UIEventPriority.Critical => "Error", // Red, urgent
            UIEventPriority.High => "Warning",   // Orange, important
            UIEventPriority.Normal => "Info",    // Blue, informational
            UIEventPriority.Low => "Success",    // Green, low priority
            _ => "Info"
        };
    }

    /// <summary>
    /// Generates appropriate notification summary based on event type and priority
    /// </summary>
    private string GetNotificationSummary(string eventType, UIEventPriority priority)
    {
        return eventType switch
        {
            "HazardCreatedNotification" => priority switch
            {
                UIEventPriority.Critical => "?? Critical Hazard Alert",
                UIEventPriority.High => "?? High Priority Hazard",
                UIEventPriority.Normal => "?? New Hazard Reported",
                _ => "?? Hazard Notification"
            },
            "HazardEscalationAlert" => "?? Hazard Escalation Required",
            "TestUIEvent" => "?? UI Test Event",
            _ => $"?? {eventType}"
        };
    }

    #endregion
}