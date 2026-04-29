//-----------------------------------------------------------------------
// <copyright file="HazardCreatedEventHandler.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Event handler for hazard creation events integrating with existing SMS infrastructure.
//                  Coordinates SPI updates, stakeholder notifications, and workflow initiation when
//                  new hazards are created in the SMS system.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;
using SMS_Domain.Common;
using SMS_Domain.Events;
using SMS_Domain.Events.Test;
using SMS_Domain.Enums;
using SMS_Domain.Interfaces;

namespace SMS_Application.EventHandlers;

/// <summary>
/// CLEAN EVENTBUS DEMONSTRATION: HazardCreatedEventHandler
/// 
/// Shows the complete EventBus workflow when a hazard is created:
/// 1. DOMAIN EVENT: HazardCreatedEvent (input) 
/// 2. UI EVENT: HazardCreatedNotification → popup notifications
/// 3. INTEGRATION EVENT: EmailNotificationEvent → email simulation (.eml files)
/// 
/// This demonstrates the event cascade pattern:
/// Domain Event → UI Event (immediate user feedback) + Integration Event (external systems)
/// </summary>
public class HazardCreatedEventHandler : BaseDomainEventHandler<SMS_Domain.Events.HazardCreatedEvent>
{
    private readonly IBaseEventBus _eventBus;
    private readonly ILogger<HazardCreatedEventHandler> _logger;

    public HazardCreatedEventHandler(
        ILogger<HazardCreatedEventHandler> logger,
        IBaseEventBus eventBus)
        : base(logger)
    {
        _logger = logger;
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }

    /// <summary>
    /// Processes hazard creation events and coordinates downstream workflows
    /// CLEAN DEMONSTRATION: Shows Domain → Integration → UI event cascade
    /// </summary>
    protected override async Task<Result> ProcessEventAsync(SMS_Domain.Events.HazardCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🚀 [HAZARD HANDLER] Processing hazard creation for {HazardCode} (Type: {HazardType}, Priority: {Priority})",
                domainEvent.HazardCode, domainEvent.HazardType, domainEvent.Priority);

            // STEP 1: UI Event - Immediate dashboard notifications
            await PublishUINotification(domainEvent, cancellationToken);

            // STEP 2: Integration Event - Email notifications (for Medium+ priority)
            await PublishEmailNotification(domainEvent, cancellationToken);

            _logger.LogInformation("✅ [HAZARD HANDLER] Successfully processed hazard creation for {HazardCode}", domainEvent.HazardCode);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [HAZARD HANDLER] Error processing hazard creation event for {HazardCode}", domainEvent.HazardCode);
            return Result.Failure(new Error("HAZARD_CREATION_HANDLER_ERROR", $"Hazard creation processing failed: {ex.Message}"));
        }
    }

    /// <summary>
    /// PERFORMANCE FIX: Static stakeholder determination to avoid authentication loops
    /// Uses predefined stakeholder groups instead of database queries
    /// </summary>
   

    #region Event Publishing Methods

    /// <summary>
    /// Publishes UI events for immediate user notification
    /// DEMONSTRATION: Shows UI event publication for popup notifications
    /// </summary>
    private async Task PublishUINotification(SMS_Domain.Events.HazardCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("📱 [UI EVENT] Publishing popup notification for {HazardCode}", domainEvent.HazardCode);

            // Create UI notification event for popup/toast notifications
            var uiNotificationEvent = new TestUIEvent();
            uiNotificationEvent.SetEventType("HazardCreatedNotification");
            uiNotificationEvent.SetTargetComponent("NotificationCenter");
            uiNotificationEvent.Message = $"New {domainEvent.Priority} priority hazard: {domainEvent.HazardCode}";
            uiNotificationEvent.Priority = GetUINotificationPriority(domainEvent.Priority);
            uiNotificationEvent.TestData = new
            {
                HazardCode = domainEvent.HazardCode,
                HazardType = domainEvent.HazardType,
                Priority = domainEvent.Priority.ToString(),
                CreatedBy = domainEvent.CreatedBy,
                NotificationType = "popup",
                AutoDismiss = domainEvent.Priority <= SMS_Domain.Enums.HazardPriority.Medium
            };

            // Publish UI event for immediate user notification (Manual mode for demonstration)
            var uiResult = await _eventBus.PublishUIEventAsync(uiNotificationEvent, EventExecutionMode.Manual);

            if (uiResult.IsSuccess)
            {
                _logger.LogInformation("✅ [UI EVENT] Popup notification queued for hazard {HazardCode}", domainEvent.HazardCode);
            }
            else
            {
                _logger.LogWarning("⚠️ [UI EVENT] Failed to queue popup notification: {Error}", uiResult.Error.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ [UI EVENT] Failed to publish UI notification for hazard {HazardCode}", domainEvent.HazardCode);
        }
    }

    /// <summary>
    /// Publishes email notifications based on hazard priority
    /// DEMONSTRATION: Shows Integration event publication for external systems
    /// </summary>
    private async Task PublishEmailNotification(SMS_Domain.Events.HazardCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        try
        {
            // Only send emails for Medium priority and above
            if (domainEvent.Priority >= SMS_Domain.Enums.HazardPriority.Medium)
            {
                _logger.LogInformation("📧 [INTEGRATION EVENT] Creating email notification for {Priority} priority hazard {HazardCode}", 
                    domainEvent.Priority, domainEvent.HazardCode);

                var recipients = GetEmailRecipientsByPriority(domainEvent.Priority);
                var emailPriority = GetEmailPriorityFromHazardPriority(domainEvent.Priority);

                // Create rich email notification
                var emailEvent = new EmailNotificationEvent(
                    toRecipients: recipients,
                    subject: $"🚨 {domainEvent.Priority} Priority Hazard: {domainEvent.HazardCode} - {domainEvent.HazardType}",
                    body: CreateHazardNotificationEmailBody(domainEvent),
                    isHtmlContent: true,
                    priority: emailPriority,
                    deliveryMode: IntegrationDeliveryMode.BestEffort,
                    workflowType: "HazardNotification",
                    relatedEntityType: "Hazard",
                    relatedEntityId: domainEvent.HazardCode,
                    templateName: "SMS_HazardCreated_Template",
                    templateData: new Dictionary<string, object>
                    {
                        { "HazardCode", domainEvent.HazardCode },
                        { "HazardType", domainEvent.HazardType },
                        { "Priority", domainEvent.Priority.ToString() },
                        { "Description", domainEvent.Description },
                        { "Location", domainEvent.LocationArea },
                        { "CreatedBy", domainEvent.CreatedBy },
                        { "CreatedDate", domainEvent.CreatedDate.ToString("yyyy-MM-dd HH:mm") }
                    },
                    emailMetadata: new Dictionary<string, object>
                    {
                        { "Source", "HazardCreatedEventHandler" },
                        { "HazardId", domainEvent.HazardId },
                        { "ReportCode", domainEvent.ReportCode },
                        { "EventDemonstration", true }
                    }
                );

                // Publish Integration Event for email delivery (Manual mode for demonstration)
                var emailResult = await _eventBus.PublishIntegrationEventAsync(emailEvent, EventExecutionMode.Manual);

                if (emailResult.IsSuccess)
                {
                    _logger.LogInformation("✅ [INTEGRATION EVENT] Hazard notification email queued for {HazardCode} to {RecipientCount} recipients", 
                        domainEvent.HazardCode, recipients.Count);
                }
                else
                {
                    _logger.LogError("❌ [INTEGRATION EVENT] Failed to queue hazard notification email: {Error}", emailResult.Error.Message);
                }
            }
            else
            {
                _logger.LogInformation("ℹ️ [INTEGRATION EVENT] Skipping email notification for {HazardCode} - Priority {Priority} below Medium threshold", 
                    domainEvent.HazardCode, domainEvent.Priority);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [INTEGRATION EVENT] Failed to send email notifications for {HazardCode}", domainEvent.HazardCode);
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Maps hazard priority to UI notification priority
    /// </summary>
    private UIEventPriority GetUINotificationPriority(SMS_Domain.Enums.HazardPriority hazardPriority)
    {
        return hazardPriority switch
        {
            SMS_Domain.Enums.HazardPriority.Critical => UIEventPriority.Critical,
            SMS_Domain.Enums.HazardPriority.High => UIEventPriority.High,
            SMS_Domain.Enums.HazardPriority.Medium => UIEventPriority.Normal,
            _ => UIEventPriority.Low
        };
    }

    /// <summary>
    /// Gets email recipients based on hazard priority level
    /// </summary>
    private List<string> GetEmailRecipientsByPriority(SMS_Domain.Enums.HazardPriority priority)
    {
        return priority switch
        {
            SMS_Domain.Enums.HazardPriority.Critical => new List<string>
            {
                "safety.manager@pdxairport.com",
                "operations.director@pdxairport.com", 
                "sms.coordinator@pdxairport.com"
            },
            SMS_Domain.Enums.HazardPriority.High => new List<string>
            {
                "safety.manager@pdxairport.com",
                "sms.coordinator@pdxairport.com"
            },
            _ => new List<string>
            {
                "safety.team@pdxairport.com"
            }
        };
    }

    /// <summary>
    /// Maps hazard priority to email priority
    /// </summary>
    private EmailPriority GetEmailPriorityFromHazardPriority(SMS_Domain.Enums.HazardPriority hazardPriority)
    {
        return hazardPriority switch
        {
            SMS_Domain.Enums.HazardPriority.Critical => EmailPriority.Urgent,
            SMS_Domain.Enums.HazardPriority.High => EmailPriority.High,
            SMS_Domain.Enums.HazardPriority.Medium => EmailPriority.Normal,
            _ => EmailPriority.Low
        };
    }

    /// <summary>
    /// Creates clean HTML email body for hazard notifications
    /// </summary>
    private string CreateHazardNotificationEmailBody(SMS_Domain.Events.HazardCreatedEvent domainEvent)
    {
        var priorityColor = GetPriorityColor(domainEvent.Priority);

        return $@"
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; background-color: #f9f9f9; }}
        .container {{ background-color: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        .header {{ background-color: {priorityColor}; color: white; padding: 15px; border-radius: 5px; margin-bottom: 20px; }}
        .details {{ background-color: #f8f9fa; padding: 15px; border-left: 4px solid {priorityColor}; margin: 15px 0; }}
        .footer {{ color: #666; font-size: 12px; margin-top: 30px; border-top: 1px solid #eee; padding-top: 15px; }}
        .priority {{ color: {priorityColor}; font-weight: bold; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>🚨 New Hazard Report: <span class='priority'>{domainEvent.Priority}</span> Priority</h2>
        </div>

        <p>A new hazard has been reported in the SMS system and requires attention.</p>

        <div class='details'>
            <h3>Hazard Information:</h3>
            <ul>
                <li><strong>Hazard Code:</strong> {domainEvent.HazardCode}</li>
                <li><strong>Type:</strong> {domainEvent.HazardType}</li>
                <li><strong>Category:</strong> {domainEvent.HazardCategory}</li>
                <li><strong>Priority:</strong> <span class='priority'>{domainEvent.Priority}</span></li>
                <li><strong>Location:</strong> {domainEvent.LocationArea}</li>
                <li><strong>Reported By:</strong> {domainEvent.CreatedBy}</li>
                <li><strong>Date:</strong> {domainEvent.CreatedDate:yyyy-MM-dd HH:mm} UTC</li>
                <li><strong>Report Code:</strong> {domainEvent.ReportCode}</li>
            </ul>

            <h4>Description:</h4>
            <p>{domainEvent.Description}</p>
        </div>

        <p>Please access the SMS system to review this hazard and take appropriate action.</p>

        <div class='footer'>
            <p><strong>SMS Safety Management System</strong> - Automated Notification<br/>
            Event ID: {domainEvent.EventId} | Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC</p>
        </div>
    </div>
</body>
</html>";
    }

    /// <summary>
    /// Gets color code for hazard priority levels
    /// </summary>
    private string GetPriorityColor(SMS_Domain.Enums.HazardPriority priority)
    {
        return priority switch
        {
            SMS_Domain.Enums.HazardPriority.Critical => "#dc3545", // Bootstrap danger red
            SMS_Domain.Enums.HazardPriority.High => "#fd7e14",     // Bootstrap warning orange  
            SMS_Domain.Enums.HazardPriority.Medium => "#ffc107",   // Bootstrap warning yellow
            _ => "#28a745"                                          // Bootstrap success green
        };
    }

    #endregion
}