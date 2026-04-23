//-----------------------------------------------------------------------
// <copyright file="EmailNotificationEventHandler.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Integration event handler for email notification delivery in SMS system.
//                  Processes email notification events and coordinates with external email services
//                  for reliable delivery of safety-critical communications and workflow notifications.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SMS_Application.Interfaces;
using SMS_Application.Services;
using SMS_Domain.Events.Integration;
using SMS_Domain.Common;

namespace SMS_Application.EventHandlers.Integration;

/// <summary>
/// Integration event handler for email notification delivery
/// Processes email events and coordinates with external email services
/// Phase 3: Provides reliable email delivery for safety-critical communications
/// </summary>
public class EmailNotificationEventHandler : BaseIntegrationEventHandler<EmailNotificationEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailNotificationEventHandler> _logger;
    private readonly bool _useSimulation;

    public EmailNotificationEventHandler(
        ILogger<EmailNotificationEventHandler> logger,
        IEmailService emailService,
        IOptions<SmtpEmailConfiguration> smtpConfig)
        : base(logger)
    {
        _logger = logger;
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));

        // Use configuration-based simulation setting
        _useSimulation = smtpConfig?.Value?.UseSimulation ?? false;

        _logger.LogInformation("📧 Email handler initialized with UseSimulation: {UseSimulation}", _useSimulation);
    }

    /// <summary>
    /// Processes email notification events for external delivery
    /// Phase 3: Implements both real and simulated email delivery
    /// </summary>
    protected override async Task<Result> ProcessIntegrationEventAsync(EmailNotificationEvent integrationEvent, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("📧 Processing email notification: '{Subject}' to {RecipientCount} recipients (Priority: {Priority})",
                integrationEvent.Subject, integrationEvent.ToRecipients.Count, integrationEvent.Priority);

            // Validate email event before processing
            if (!ValidateEmailEvent(integrationEvent))
            {
                return Result.Failure(new Error("INVALID_EMAIL_EVENT", "Email event validation failed"));
            }

            Result deliveryResult;

            if (_useSimulation)
            {
                // Use simulation for development/testing
                deliveryResult = await SimulateEmailDelivery(integrationEvent, cancellationToken);
            }
            else
            {
                // Use real email delivery for production
                deliveryResult = await SendRealEmail(integrationEvent, cancellationToken);
            }

            if (deliveryResult.IsSuccess)
            {
                _logger.LogInformation("✅ Email notification sent successfully: '{Subject}' to {RecipientCount} recipients",integrationEvent.Subject, integrationEvent.ToRecipients.Count);
            }
            else
            {
                _logger.LogError("❌ Email notification failed: '{Subject}' - {Error}", integrationEvent.Subject, deliveryResult.Error.Message);
            }

            return deliveryResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to process email notification: '{Subject}'", integrationEvent.Subject);
            return Result.Failure(new Error("EMAIL_NOTIFICATION_FAILED", $"Email notification failed: {ex.Message}"));
        }
    }

    /// <summary>
    /// Sends real email using configured email service
    /// </summary>
    private async Task<Result> SendRealEmail(EmailNotificationEvent emailEvent, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("📤 REAL EMAIL: Sending '{Subject}' via {ServiceType}", emailEvent.Subject, _emailService.GetType().Name);

            var deliveryResult = await _emailService.SendEmailAsync(emailEvent, cancellationToken);

            if (deliveryResult.IsSuccess)
            {
                var result = deliveryResult.Value;
                _logger.LogInformation("✅ REAL EMAIL: Delivered successfully - MessageId: {MessageId}, Status: {Status}", 
                    result.MessageId, result.DeliveryStatus);

                // Log delivery details
                if (result.SuccessfulRecipients.Any())
                {
                    _logger.LogInformation("📧 Successful recipients: [{Recipients}]", 
                        string.Join(", ", result.SuccessfulRecipients));
                }

                if (result.FailedRecipients.Any())
                {
                    _logger.LogWarning("⚠️ Failed recipients: [{Recipients}]", 
                        string.Join(", ", result.FailedRecipients));
                }

                return Result.Success();
            }
            else
            {
                return Result.Failure(deliveryResult.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ REAL EMAIL: Delivery failed for '{Subject}'", emailEvent.Subject);
            return Result.Failure(new Error("REAL_EMAIL_FAILED", $"Real email delivery failed: {ex.Message}"));
        }
    }

    /// <summary>
    /// Phase 3: Simulates email delivery for testing and development
    /// Replace this with actual email service integration
    /// </summary>
    private async Task<Result> SimulateEmailDelivery(EmailNotificationEvent emailEvent, CancellationToken cancellationToken)
    {
        try
        {
            // Simulate processing delay based on priority
            var delay = emailEvent.Priority switch
            {
                EmailPriority.Urgent => TimeSpan.FromMilliseconds(100),
                EmailPriority.High => TimeSpan.FromMilliseconds(200),
                EmailPriority.Normal => TimeSpan.FromMilliseconds(300),
                EmailPriority.Low => TimeSpan.FromMilliseconds(500),
                _ => TimeSpan.FromMilliseconds(300)
            };

            await Task.Delay(delay, cancellationToken);

            // Log email details for development/testing
            _logger.LogInformation("?? SIMULATED EMAIL DELIVERY:");
            _logger.LogInformation("   Subject: {Subject}", emailEvent.Subject);
            _logger.LogInformation("   To: [{Recipients}]", string.Join(", ", emailEvent.ToRecipients));

            if (emailEvent.CcRecipients.Any())
                _logger.LogInformation("   CC: [{Recipients}]", string.Join(", ", emailEvent.CcRecipients));

            if (emailEvent.BccRecipients.Any())
                _logger.LogInformation("   BCC: [{Recipients}]", string.Join(", ", emailEvent.BccRecipients));

            _logger.LogInformation("   Priority: {Priority}", emailEvent.Priority);
            _logger.LogInformation("   Delivery Mode: {DeliveryMode}", emailEvent.DeliveryMode);

            if (!string.IsNullOrEmpty(emailEvent.RelatedEntityType))
            {
                _logger.LogInformation("   Related: {EntityType} {EntityId}", emailEvent.RelatedEntityType, emailEvent.RelatedEntityId);
            }

            if (!string.IsNullOrEmpty(emailEvent.WorkflowType))
            {
                _logger.LogInformation("   Workflow: {WorkflowType}", emailEvent.WorkflowType);
            }

            // Log email body preview (first 200 characters)
            var bodyPreview = emailEvent.Body.Length > 200 
                ? emailEvent.Body.Substring(0, 200) + "..." 
                : emailEvent.Body;
            _logger.LogInformation("   Body Preview: {BodyPreview}", bodyPreview.Replace("\n", " ").Replace("\r", ""));

            // Simulate potential delivery outcomes based on delivery mode
            if (emailEvent.DeliveryMode == IntegrationDeliveryMode.Synchronous)
            {
                // Synchronous mode - simulate immediate confirmation
                _logger.LogInformation("?? Synchronous delivery confirmed for: {Subject}", emailEvent.Subject);
            }
            else
            {
                // Async modes - just queue for delivery
                _logger.LogInformation("?? Email queued for delivery: {Subject}", emailEvent.Subject);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error simulating email delivery for: {Subject}", emailEvent.Subject);
            return Result.Failure(new Error("SIMULATION_FAILED", $"Email simulation failed: {ex.Message}"));
        }
    }

    /// <summary>
    /// Phase 3: Formats recipient list for logging
    /// </summary>
    private string FormatRecipientList(List<string> recipients)
    {
        if (!recipients.Any()) return "none";
        if (recipients.Count == 1) return recipients[0];
        return $"{recipients[0]} +{recipients.Count - 1} others";
    }

    /// <summary>
    /// Phase 3: Validates email event before processing
    /// </summary>
    private bool ValidateEmailEvent(EmailNotificationEvent emailEvent)
    {
        if (!emailEvent.ToRecipients.Any())
        {
            _logger.LogWarning("Email event has no recipients: {Subject}", emailEvent.Subject);
            return false;
        }

        if (string.IsNullOrEmpty(emailEvent.Subject))
        {
            _logger.LogWarning("Email event has empty subject");
            return false;
        }

        if (string.IsNullOrEmpty(emailEvent.Body))
        {
            _logger.LogWarning("Email event has empty body: {Subject}", emailEvent.Subject);
            return false;
        }

        return true;
    }
}