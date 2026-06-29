//-----------------------------------------------------------------------
// <copyright file="EventBusQueueManager.razor.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Code-behind for EventBus Queue Manager page.
//                  Provides management and control of queued events for testing and manual execution.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System.Text;
using Radzen;
using Radzen.Blazor;
using SMS_Application.Interfaces;
using SMS_Application.Commands;
using SMS_Application.Queries;

using SMS_Domain.Enums;
using SMS_Domain.Events;
using SMS_Domain.ValueObjects;

namespace SMS3.Components.Pages.SMSSystem.EventBus;

/// <summary>
/// Code-behind for EventBus Queue Manager page
/// Provides management interface for queued events
/// </summary>
public partial class EventBusQueueManager
{
    #region Private Fields

    [Inject] private IBaseMediator _mediator { get; set; } = default!;

    private IEnumerable<QueuedEvent> _queuedEvents = new List<QueuedEvent>();
    private QueueStatistics? _statistics;
    private RadzenDataGrid<QueuedEvent>? _eventGrid;

    private bool _isLoading = true;
    private bool _isProcessing = false;

    private QueuedEventStatus? _statusFilter;
    private EventCategory? _eventTypeFilter = EventCategory.IntegrationEvent;

    #endregion
    // Helper to extract ReportId from any event type
    private string GetEventReportIdentifier(QueuedEvent queuedEvent)
    {
        if (!string.IsNullOrWhiteSpace(queuedEvent.ReportId))
        {
            return queuedEvent.ReportId.Trim();
        }

        if (!string.IsNullOrWhiteSpace(queuedEvent.EventData))
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(queuedEvent.EventData);
                var root = doc.RootElement;

                foreach (var property in root.EnumerateObject())
                {
                    if (string.Equals(property.Name, "ReportId", StringComparison.OrdinalIgnoreCase)
                        && property.Value.ValueKind == System.Text.Json.JsonValueKind.String)
                    {
                        return property.Value.GetString()?.Trim() ?? string.Empty;
                    }
                }
            }
            catch
            {
                // Ignore deserialization errors
            }
        }
        return string.Empty;
    }

    #region Filter Options

    private readonly List<FilterOption<QueuedEventStatus?>> _statusOptions = new()
    {
        new(null, "All Statuses"),
        new(QueuedEventStatus.Pending, "Pending"),
        new(QueuedEventStatus.Processing, "Processing"),
        new(QueuedEventStatus.Processed, "Processed"),
        new(QueuedEventStatus.Failed, "Failed"),
        new(QueuedEventStatus.Cancelled, "Cancelled")
    };

    private readonly List<FilterOption<EventCategory?>> _eventTypeOptions = new()
    {
        new(null, "All Types"),
        new(EventCategory.DomainEvent, "Domain Events"),
        new(EventCategory.IntegrationEvent, "Integration Events"),
        new(EventCategory.UIEvent, "UI Events")
    };

    #endregion

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        try
        {
            await LoadQueuedEvents();
            await LoadStatistics();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to initialize EventBus Queue Manager");
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Initialization Error",
                Detail = "Failed to load queue data. Please refresh the page.",
                Duration = 4000
            });
        }
        finally
        {
            _isLoading = false;
        }
    }

    #endregion

    #region Data Loading Methods

    private async Task LoadQueuedEvents()
    {
        try
        {
            _isLoading = true;
            StateHasChanged();

            var result = await _mediator.SendAsync(
                new GetQueuedEventsQuery(_statusFilter, _eventTypeFilter, 1000),
                CancellationToken.None);

            if (result.IsSuccess)
            {
                _queuedEvents = result.Value;
                Logger.LogDebug("Loaded {EventCount} queued events", _queuedEvents.Count());
            }
            else
            {
                Logger.LogWarning("Failed to load queued events: {Error}", result.Error.Message);
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Warning,
                    Summary = "Load Error",
                    Detail = result.Error.Message,
                    Duration = 4000
                });
                _queuedEvents = new List<QueuedEvent>();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception while loading queued events");
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Load Error",
                Detail = "An unexpected error occurred while loading events.",
                Duration = 4000
            });
            _queuedEvents = new List<QueuedEvent>();
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task LoadStatistics()
    {
        try
        {
            var result = await _mediator.SendAsync(
                new GetEventQueueStatisticsQuery(),
                CancellationToken.None);

            if (result.IsSuccess)
            {
                _statistics = result.Value;
                Logger.LogDebug("Loaded queue statistics: {Pending} pending, {Processed} processed", 
                    _statistics.PendingCount, _statistics.ProcessedCount);
            }
            else
            {
                Logger.LogWarning("Failed to load queue statistics: {Error}", result.Error.Message);
                _statistics = null;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception while loading queue statistics");
            _statistics = null;
        }

        StateHasChanged();
    }

    #endregion

    #region Event Handling Methods

    private async Task OnFilterChanged()
    {
        Logger.LogDebug("Filters changed - Status: {Status}, Type: {Type}", _statusFilter, _eventTypeFilter);
        await LoadQueuedEvents();
        await LoadStatistics();
    }

    private async Task ExecuteEvent(Guid eventId)
    {
        try
        {
            _isProcessing = true;
            StateHasChanged();

            Logger.LogInformation("Manually executing event {EventId}", eventId);

            // Get the event details before execution
            var eventResult = await _mediator.SendAsync(
                new GetQueuedEventByIdQuery(eventId),
                CancellationToken.None);
            if (!eventResult.IsSuccess)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Event Not Found",
                    Detail = eventResult.Error?.Message ?? "Event not found.",
                    Duration = 4000
                });
                return;
            }

            var queuedEvent = eventResult.Value;
            var isEmailPreviewEvent = queuedEvent.EventCategory == EventCategory.IntegrationEvent
                && queuedEvent.EventType == SMS_Domain.Enums.EventType.EmailNotification.Value;

            // Check for IntegrationEvent and Email Notification
            if (isEmailPreviewEvent)
            {
                try
                {
                    var emailEvent = System.Text.Json.JsonSerializer.Deserialize<SMS_Domain.Events.EmailNotificationEvent>(queuedEvent.EventData);
                    if (emailEvent != null)
                    {
                        var subject = emailEvent.Subject ?? string.Empty;
                        var body = emailEvent.Body ?? string.Empty;

                        AppendEmailPreviewDetails(queuedEvent, emailEvent, ref subject, ref body);

                        var model = new SMS3.Components.Pages.SMSSystem.Models.EmailComposeModel
                        {
                            To = emailEvent.ToRecipients ?? new List<string>(),
                            Cc = emailEvent.CcRecipients ?? new List<string>(),
                            Bcc = emailEvent.BccRecipients ?? new List<string>(),
                            Subject = subject,
                            BodyHtml = emailEvent.IsHtmlContent ? ((MarkupString)body).Value : body.Replace("\n", "<br />")
                        };

                        var dialogResult = await DialogService.OpenAsync<Components.EmailComposeDialog>(
                            "SMS Notification",
                            new Dictionary<string, object?> { { "InitialModel", model } },
                            new DialogOptions { Width = "950px", Height = "700px", Resizable = true, Draggable = true, ShowClose = false }
                        );

                        var emailSent = dialogResult is bool sent && sent;

                        if (!emailSent)
                        {
                            Logger.LogInformation("Email dialog cancelled for event {EventId}; queue status remains unchanged.", eventId);
                            return;
                        }

                        if (queuedEvent.Status != QueuedEventStatus.Pending)
                        {
                            NotificationService.Notify(new NotificationMessage
                            {
                                Severity = NotificationSeverity.Success,
                                Summary = "Email Re-Sent",
                                Detail = "Email was sent again without changing the original queue record.",
                                Duration = 3000
                            });

                            Logger.LogInformation("Email event {EventId} re-sent from queue manager for status {Status}", eventId, queuedEvent.Status);
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Failed to preview EmailNotificationEvent for event {EventId}; continuing with execution", eventId);
                }
            }

            // UI Event logic (existing)
            var isUIEvent = queuedEvent.EventCategory == EventCategory.UIEvent;
            var uiEventData = isUIEvent ? DeserializeUIEventData(queuedEvent.EventData) : null;

            var result = await _mediator.SendAsync(new ExecuteQueuedEventCommand(eventId, "ManualUI"), CancellationToken.None);

            if (result.IsSuccess)
            {
                // Show actual UI notification if this was a UI event
                if (isUIEvent && uiEventData != null)
                {
                    ShowUIEventNotification(uiEventData);
                }

                Logger.LogInformation("Successfully executed event {EventId}", eventId);
            }
            else
            {
                // Only show notifications for actual failures
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Execution Failed",
                    Detail = result.Error.Message,
                    Duration = 4000
                });

                Logger.LogWarning("Failed to execute event {EventId}: {Error}", eventId, result.Error.Message);
            }

            // Refresh the data
            await LoadQueuedEvents();
            await LoadStatistics();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception while executing event {EventId}", eventId);
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Execution Error",
                Detail = "An unexpected error occurred during execution.",
                Duration = 4000
            });
        }
        finally
        {
            _isProcessing = false;
            StateHasChanged();
        }
    }

    private static void AppendEmailPreviewDetails(QueuedEvent queuedEvent, EmailNotificationEvent emailEvent, ref string subject, ref string body)
    {
        if (string.IsNullOrWhiteSpace(queuedEvent.EventData))
        {
            return;
        }

        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(queuedEvent.EventData);
            var root = doc.RootElement;

            if (IsHazardEmail(emailEvent))
            {
                var reportCode = FirstString(root, "ReportId", "reportId", "ReportCode", "reportCode");
                var hazardCode = FirstString(root, "RelatedEntityId", "relatedEntityId", "HazardCode", "hazardCode");
                var hazardCategory = FirstString(root, "HazardCategory", "hazardCategory", "Category", "category");
                var hazardType = FirstString(root, "HazardType", "hazardType", "Type", "type");

                if (!string.IsNullOrWhiteSpace(reportCode))
                {
                    subject = EnsureToken(subject, reportCode.Trim());
                }

                if (!string.IsNullOrWhiteSpace(hazardCode))
                {
                    subject = EnsureToken(subject, hazardCode.Trim());
                }

                var details = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(hazardCategory))
                {
                    details.AppendLine($"Hazard Category: {hazardCategory}");
                }

                if (!string.IsNullOrWhiteSpace(hazardType))
                {
                    details.AppendLine($"Hazard Type: {hazardType}");
                }

                if (details.Length > 0)
                {
                    body = AppendTextBlock(body, details.ToString().TrimEnd());
                }
            }
            else if (IsMitigationEmail(emailEvent))
            {
                var mitigationCode = FirstString(root, "RelatedEntityId", "relatedEntityId", "MitigationCode", "mitigationCode");
                var mitigationName = FirstString(root, "MitigationName", "mitigationName", "Name", "name");

                if (!string.IsNullOrWhiteSpace(mitigationCode))
                {
                    subject = EnsureToken(subject, mitigationCode.Trim());
                }

                if (!string.IsNullOrWhiteSpace(mitigationName))
                {
                    body = AppendTextBlock(body, $"Mitigation Name: {mitigationName}");
                }
            }
        }
        catch
        {
            // Leave original preview content unchanged if payload parsing fails.
        }
    }

    private static bool IsHazardEmail(EmailNotificationEvent emailEvent)
    {
        return string.Equals(emailEvent.RelatedEntityType, "Hazard", StringComparison.OrdinalIgnoreCase)
               || string.Equals(emailEvent.WorkflowType, "HazardNotification", StringComparison.OrdinalIgnoreCase)
               || (emailEvent.Subject?.Contains("Hazard", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    private static bool IsMitigationEmail(EmailNotificationEvent emailEvent)
    {
        return string.Equals(emailEvent.RelatedEntityType, "Mitigation", StringComparison.OrdinalIgnoreCase)
               || (emailEvent.WorkflowType?.Contains("Mitigation", StringComparison.OrdinalIgnoreCase) ?? false)
               || (emailEvent.Subject?.Contains("Mitigation", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    private static string FirstString(System.Text.Json.JsonElement root, params string[] names)
    {
        foreach (var name in names)
        {
            if (TryGetStringValue(root, name, out var value) && !string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return string.Empty;
    }

    private static bool TryGetStringValue(System.Text.Json.JsonElement element, string propertyName, out string value)
    {
        value = string.Empty;

        if (element.ValueKind != System.Text.Json.JsonValueKind.Object)
        {
            return false;
        }

        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                if (property.Value.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    value = property.Value.GetString() ?? string.Empty;
                    return true;
                }

                if (property.Value.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    if (TryGetStringValue(property.Value, "Value", out var nested))
                    {
                        value = nested;
                        return true;
                    }

                    if (TryGetStringValue(property.Value, "Name", out nested))
                    {
                        value = nested;
                        return true;
                    }
                }

                return false;
            }

            if (property.Value.ValueKind == System.Text.Json.JsonValueKind.Object
                && TryGetStringValue(property.Value, propertyName, out value))
            {
                return true;
            }
        }

        return false;
    }

    private static string EnsureToken(string subject, string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return subject;
        }

        if (subject.Contains(token, StringComparison.OrdinalIgnoreCase))
        {
            return subject;
        }

        return string.IsNullOrWhiteSpace(subject)
            ? token
            : $"{subject} - {token}";
    }

    private static string AppendTextBlock(string existingBody, string details)
    {
        if (string.IsNullOrWhiteSpace(details))
        {
            return existingBody;
        }

        if (string.IsNullOrWhiteSpace(existingBody))
        {
            return details;
        }

        if (existingBody.Contains(details, StringComparison.OrdinalIgnoreCase))
        {
            return existingBody;
        }

        return $"{existingBody}\n\n{details}";
    }

    private async Task CancelEvent(Guid eventId)
    {
        try
        {
            _isProcessing = true;
            StateHasChanged();

            Logger.LogInformation("Cancelling event {EventId}", eventId);

            var result = await _mediator.SendAsync(
                new CancelQueuedEventCommand(eventId, "ManualUI"),
                CancellationToken.None);

            if (result.IsSuccess)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Info,
                    Summary = "Event Cancelled",
                    Detail = "Event cancelled successfully.",
                    Duration = 3000
                });

                Logger.LogInformation("Successfully cancelled event {EventId}", eventId);
            }
            else
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Cancellation Failed",
                    Detail = result.Error.Message,
                    Duration = 4000
                });

                Logger.LogWarning("Failed to cancel event {EventId}: {Error}", eventId, result.Error.Message);
            }

            // Refresh the data
            await LoadQueuedEvents();
            await LoadStatistics();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception while cancelling event {EventId}", eventId);
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Cancellation Error",
                Detail = "An unexpected error occurred during cancellation.",
                Duration = 4000
            });
        }
        finally
        {
            _isProcessing = false;
            StateHasChanged();
        }
    }

    private async Task ExecuteAllPending()
    {
        try
        {
            _isProcessing = true;
            StateHasChanged();

            Logger.LogInformation("Executing all pending events (Type filter: {EventType})", _eventTypeFilter);

            var result = await _mediator.SendAsync(
                new ExecuteAllPendingQueuedEventsCommand(_eventTypeFilter, "ManualUI"),
                CancellationToken.None);

            if (result.IsSuccess)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Batch Execution Complete",
                    Detail = $"Successfully executed {result.Value} pending events.",
                    Duration = 4000
                });

                Logger.LogInformation("Successfully executed {EventCount} pending events", result.Value);
            }
            else
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Warning,
                    Summary = "Batch Execution Issues",
                    Detail = result.Error.Message,
                    Duration = 5000
                });

                Logger.LogWarning("Batch execution had issues: {Error}", result.Error.Message);
            }

            // Refresh the data
            await LoadQueuedEvents();
            await LoadStatistics();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception while executing all pending events");
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Batch Execution Error",
                Detail = "An unexpected error occurred during batch execution.",
                Duration = 4000
            });
        }
        finally
        {
            _isProcessing = false;
            StateHasChanged();
        }
    }

    private async Task ClearCompleted()
    {
        try
        {
            _isProcessing = true;
            StateHasChanged();

            Logger.LogInformation("Clearing completed events");

            var result = await _mediator.SendAsync(
                new ClearCompletedQueuedEventsCommand(),
                CancellationToken.None);

            if (result.IsSuccess)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Info,
                    Summary = "Cleanup Complete",
                    Detail = $"Cleared {result.Value} completed events.",
                    Duration = 3000
                });

                Logger.LogInformation("Successfully cleared {EventCount} completed events", result.Value);
            }
            else
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Cleanup Failed",
                    Detail = result.Error.Message,
                    Duration = 4000
                });

                Logger.LogWarning("Failed to clear completed events: {Error}", result.Error.Message);
            }

            // Refresh the data
            await LoadQueuedEvents();
            await LoadStatistics();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception while clearing completed events");
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Cleanup Error",
                Detail = "An unexpected error occurred during cleanup.",
                Duration = 4000
            });
        }
        finally
        {
            _isProcessing = false;
            StateHasChanged();
        }
    }

    private async Task ClearQueue()
    {
        try
        {
            var confirmed = await DialogService.Confirm(
                "Are you sure you want to clear ALL events from the queue? This will remove all pending, processed, and failed events.",
                "Clear All Events",
                new ConfirmOptions()
                {
                    OkButtonText = "Yes, Clear All",
                    CancelButtonText = "Cancel"
                });

            if (confirmed != true) return;

            _isProcessing = true;
            StateHasChanged();

            Logger.LogWarning("Clearing ALL queue events - Database truncate/reimport scenario");

            var result = await _mediator.SendAsync(
                new ClearAllQueuedEventsCommand(),
                CancellationToken.None);

            if (result.IsSuccess)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Queue Cleared",
                    Detail = $"Cleared all {result.Value} events from queue.",
                    Duration = 3000
                });

                Logger.LogInformation("Successfully cleared all {EventCount} events from queue", result.Value);
            }
            else
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Clear Failed",
                    Detail = result.Error.Message,
                    Duration = 4000
                });

                Logger.LogWarning("Failed to clear queue: {Error}", result.Error.Message);
            }

            // Refresh the data
            await LoadQueuedEvents();
            await LoadStatistics();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception while clearing queue");
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Clear Error",
                Detail = "An unexpected error occurred during queue clear.",
                Duration = 4000
            });
        }
        finally
        {
            _isProcessing = false;
            StateHasChanged();
        }
    }

    #endregion

    #region UI Methods

    private async Task ShowEventDetails(QueuedEvent queuedEvent)
    {
        try
        {
            await DialogService.OpenAsync<EventDetailsDialog>("Event Details", 
                new Dictionary<string, object?> { { "Event", queuedEvent } },
                new DialogOptions()
                {
                    Width = "800px",
                    Height = "600px",
                    Resizable = true,
                    Draggable = true
                });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to show event details for event {EventId}", queuedEvent.Id);
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Display Error",
                Detail = "Could not display event details.",
                Duration = 3000
            });
        }
    }

    private void CloseEventDetails()
    {
        // This method is no longer needed with DialogService
    }

    private static BadgeStyle GetStatusBadgeStyle(QueuedEventStatus status)
    {
        return status switch
        {
            QueuedEventStatus.Pending => BadgeStyle.Warning,
            QueuedEventStatus.Processing => BadgeStyle.Base,
            QueuedEventStatus.Processed => BadgeStyle.Success,
            QueuedEventStatus.Failed => BadgeStyle.Danger,
            QueuedEventStatus.Cancelled => BadgeStyle.Secondary,
            _ => BadgeStyle.Light
        };
    }

    private static BadgeStyle GetCategoryBadgeStyle(EventCategory category)
    {
        return category switch
        {
            var c when c == EventCategory.DomainEvent => BadgeStyle.Primary,
            var c when c == EventCategory.IntegrationEvent => BadgeStyle.Warning,
            var c when c == EventCategory.UIEvent => BadgeStyle.Base,
            _ => BadgeStyle.Light
        };
    }

    private static BadgeStyle GetPriorityBadgeStyle(EventPriority priority)
    {
        return priority switch
        {
            EventPriority.Critical => BadgeStyle.Danger,
            EventPriority.High => BadgeStyle.Warning,
            EventPriority.Normal => BadgeStyle.Primary,
            EventPriority.Low => BadgeStyle.Secondary,
            _ => BadgeStyle.Light
        };
    }

    #endregion

    #region UI Event Handling

    /// <summary>
    /// Deserializes UI event data for notification processing
    /// </summary>
    private Dictionary<string, object>? DeserializeUIEventData(string eventData)
    {
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(eventData);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to deserialize UI event data");
            return null;
        }
    }

    /// <summary>
    /// Shows actual NotificationService popup for UI events
    /// DEMONSTRATION: This is where UI events trigger real user notifications!
    /// </summary>
    private void ShowUIEventNotification(Dictionary<string, object> uiEventData)
    {
        try
        {
            // Extract notification details from UI event
            var message = uiEventData.ContainsKey("Message") ? uiEventData["Message"]?.ToString() : "UI Event Executed";
            var priority = uiEventData.ContainsKey("Priority") ? uiEventData["Priority"]?.ToString() : "Normal";
            var eventType = uiEventData.ContainsKey("EventType") ? uiEventData["EventType"]?.ToString() : "UIEvent";

            var priorityValue = priority ?? "Normal";
            var eventTypeValue = eventType ?? "UIEvent";

            var severity = GetNotificationSeverityFromPriority(priorityValue);
            var summary = GetNotificationSummaryFromEventType(eventTypeValue, priorityValue);
            var duration = GetNotificationDurationFromPriority(priorityValue);

            Logger.LogInformation("[UI NOTIFICATION] Showing notification: {Summary} - {Message}", summary, message);

            NotificationService.Notify(new NotificationMessage
            {
                Severity = severity,
                Summary = summary,
                Detail = message ?? "UI Event notification",
                Duration = duration
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to show UI event notification");
        }
    }

    /// <summary>
    /// Maps UI event priority to notification severity
    /// </summary>
    private NotificationSeverity GetNotificationSeverityFromPriority(string priority)
    {
        return priority switch
        {
            "Critical" => NotificationSeverity.Error,
            "High" => NotificationSeverity.Warning,
            "Normal" => NotificationSeverity.Info,
            "Low" => NotificationSeverity.Success,
            _ => NotificationSeverity.Info
        };
    }

    /// <summary>
    /// Generates notification summary based on event type and priority
    /// </summary>
    private string GetNotificationSummaryFromEventType(string eventType, string priority)
    {
        return eventType switch
        {
            "HazardCreatedNotification" => priority switch
            {
                "Critical" => "?? Critical Hazard Alert",
                "High" => "? High Priority Hazard",
                "Normal" => "?? New Hazard Reported",
                _ => "? Hazard Notification"
            },
            "TestUIEvent" => "?? UI Test Event",
            _ => $"?? {eventType} Notification"
        };
    }

    /// <summary>
    /// Gets notification duration based on priority
    /// </summary>
    private int GetNotificationDurationFromPriority(string priority)
    {
        return priority switch
        {
            "Critical" => 10000, // 10 seconds for critical
            "High" => 7000,      // 7 seconds for high  
            "Normal" => 5000,    // 5 seconds for normal
            "Low" => 3000,       // 3 seconds for low
            _ => 5000            // Default 5 seconds
        };
    }

    #endregion

    #region Helper Classes

    private record FilterOption<T>(T Value, string Text);

    #endregion
}

