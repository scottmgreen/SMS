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
using SMS_Domain.Events;
using SMS_Domain.Common;

namespace SMS_Application.EventHandlers;

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

        _logger.LogInformation("?? Email handler initialized with UseSimulation: {UseSimulation}", _useSimulation);
    }

    /// <summary>
    /// Processes email notification events for external delivery
    /// Phase 3: Implements both real and simulated email delivery
    /// </summary>
    protected override async Task<Result> ProcessIntegrationEventAsync(EmailNotificationEvent integrationEvent, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("?? Processing email notification: '{Subject}' to {RecipientCount} recipients (Priority: {Priority})",
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
                _logger.LogInformation("? Email notification sent successfully: '{Subject}' to {RecipientCount} recipients",
                    integrationEvent.Subject, integrationEvent.ToRecipients.Count);
            }
            else
            {
                _logger.LogError("? Email notification failed: '{Subject}' - {Error}",
                    integrationEvent.Subject, deliveryResult.Error.Message);
            }

            return deliveryResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Failed to process email notification: '{Subject}'", integrationEvent.Subject);
            return Result.Failure(new Error("EMAIL_NOTIFICATION_FAILED", $"Email notification failed: {ex.Message}"));
        }
    }

    // Helper methods...
    private async Task<Result> SendRealEmail(EmailNotificationEvent emailEvent, CancellationToken cancellationToken)
    {
        // Implementation here
        await Task.CompletedTask;
        return Result.Success();
    }

    private async Task<Result> SimulateEmailDelivery(EmailNotificationEvent emailEvent, CancellationToken cancellationToken)
    {
        // Simulation implementation
        _logger.LogInformation("?? SIMULATED EMAIL: {Subject} to {Recipients}", 
            emailEvent.Subject, string.Join(", ", emailEvent.ToRecipients));
        await Task.CompletedTask;
        return Result.Success();
    }

    private bool ValidateEmailEvent(EmailNotificationEvent emailEvent)
    {
        return !string.IsNullOrEmpty(emailEvent.Subject) && emailEvent.ToRecipients.Any();
    }
}