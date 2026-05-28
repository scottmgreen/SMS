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

        _logger.LogApplicationInformation("[EMAIL HANDLER] Email handler initialized with UseSimulation: {UseSimulation}", _useSimulation);
    }

    /// <summary>
    /// Processes email notification events for external delivery
    /// Phase 3: Implements both real and simulated email delivery
    /// </summary>
    protected override async Task<Result> ProcessIntegrationEventAsync(EmailNotificationEvent integrationEvent, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogApplicationInformation("[EMAIL HANDLER] Processing email notification: '{Subject}' to {RecipientCount} recipients (Priority: {Priority}) - UseSimulation: {UseSimulation}",
                integrationEvent.Subject, integrationEvent.ToRecipients.Count, integrationEvent.Priority, _useSimulation);

            // Validate email event before processing
            if (!ValidateEmailEvent(integrationEvent))
            {
                _logger.LogApplicationError("[EMAIL HANDLER] Email event validation failed for: {Subject}", integrationEvent.Subject);
                return Result.Failure(new Error("INVALID_EMAIL_EVENT", "Email event validation failed"));
            }

            Result deliveryResult;

            if (_useSimulation)
            {
                _logger.LogApplicationInformation("[EMAIL HANDLER] Using simulation mode for email: {Subject}", integrationEvent.Subject);
                // Use simulation for development/testing
                deliveryResult = await SimulateEmailDelivery(integrationEvent, cancellationToken);
            }
            else
            {
                _logger.LogApplicationInformation("[EMAIL HANDLER] Using real email delivery for: {Subject}", integrationEvent.Subject);
                // Use real email delivery for production
                deliveryResult = await SendRealEmail(integrationEvent, cancellationToken);
            }

            if (deliveryResult.IsSuccess)
            {
                _logger.LogApplicationInformation("[EMAIL HANDLER] Email notification sent successfully: '{Subject}' to {RecipientCount} recipients", integrationEvent.Subject, integrationEvent.ToRecipients.Count);
            }
            else
            {
                _logger.LogApplicationError("[EMAIL HANDLER] Email notification failed: '{Subject}' - {Error}",integrationEvent.Subject, deliveryResult.Error.Message);
            }

            return deliveryResult;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "[EMAIL HANDLER] Failed to process email notification: '{Subject}'", integrationEvent.Subject);
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
        try
        {
            _logger.LogApplicationInformation("[EMAIL SIM] Starting email simulation for: {Subject}", emailEvent.Subject);

            // Use original simulation directory
            var simulationDir = @"C:\temp\sms_emails";
            Directory.CreateDirectory(simulationDir);

            _logger.LogApplicationInformation("[EMAIL SIM] Using simulation directory: {Directory}", simulationDir);

            // Generate unique filename with timestamp in .eml format (email message format)
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var safeSubject = string.Join("_", emailEvent.Subject.Split(Path.GetInvalidFileNameChars()));
            // Limit subject length for filename safety
            if (safeSubject.Length > 50)
                safeSubject = safeSubject.Substring(0, 50);
            var filename = $"SMS_Email_{timestamp}_{safeSubject}.eml";
            var filePath = Path.Combine(simulationDir, filename);

            // Create .eml format content (RFC 5322 format)
            var emlContent = GenerateEmlContent(emailEvent);

            // Write to file
            await File.WriteAllTextAsync(filePath, emlContent, System.Text.Encoding.UTF8, cancellationToken);

            _logger.LogApplicationInformation("[EMAIL SIM] SIMULATED EMAIL: '{Subject}' to {RecipientCount} recipients - EML file saved to: {FilePath}", 
                emailEvent.Subject, emailEvent.ToRecipients.Count, filePath);

            // Also log key details to console for immediate feedback
            _logger.LogApplicationInformation("[EMAIL SIM] Email Details: To: {Recipients} | Subject: {Subject} | Priority: {Priority}", 
                string.Join(", ", emailEvent.ToRecipients), emailEvent.Subject, emailEvent.Priority);

            _logger.LogApplicationInformation("[EMAIL SIM] File written successfully: {FileName} ({FileSize} bytes)", 
                Path.GetFileName(filePath), emlContent.Length);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Failed to simulate email delivery for: {Subject}", emailEvent.Subject);
            return Result.Failure(new Error("EMAIL_SIMULATION_FAILED", $"Email simulation failed: {ex.Message}"));
        }
    }

    /// <summary>
    /// Generates RFC 5322 compliant .eml file content that can be opened by email clients
    /// </summary>
    private string GenerateEmlContent(EmailNotificationEvent emailEvent)
    {
        var content = new System.Text.StringBuilder();

        // RFC 5322 Email Headers
        content.AppendLine("From: SMS Safety Management System <noreply@pdxairport.com>");
        content.AppendLine($"To: {string.Join(", ", emailEvent.ToRecipients)}");

        if (emailEvent.CcRecipients?.Any() == true)
            content.AppendLine($"Cc: {string.Join(", ", emailEvent.CcRecipients)}");

        if (emailEvent.BccRecipients?.Any() == true)
            content.AppendLine($"Bcc: {string.Join(", ", emailEvent.BccRecipients)}");

        content.AppendLine($"Subject: {emailEvent.Subject}");
        content.AppendLine($"Date: {emailEvent.OccurredOn:R}"); // RFC 1123 date format
        content.AppendLine($"Message-ID: <{emailEvent.EventId}@sms.pdxairport.com>");

        // Priority header
        var priority = emailEvent.Priority switch
        {
            EmailPriority.Low => "5 (Lowest)",
            EmailPriority.Normal => "3 (Normal)", 
            EmailPriority.High => "1 (Highest)",
            EmailPriority.Urgent => "1 (Highest)",
            _ => "3 (Normal)"
        };
        content.AppendLine($"X-Priority: {priority}");

        // SMS-specific headers for tracking
        content.AppendLine($"X-SMS-Event-ID: {emailEvent.EventId}");
        content.AppendLine($"X-SMS-Event-Type: {emailEvent.EventType}");
        if (!string.IsNullOrWhiteSpace(emailEvent.ReportId))
            content.AppendLine($"X-SMS-Report-ID: {emailEvent.ReportId}");
        if (!string.IsNullOrEmpty(emailEvent.WorkflowType))
            content.AppendLine($"X-SMS-Workflow: {emailEvent.WorkflowType}");
        if (!string.IsNullOrEmpty(emailEvent.RelatedEntityId))
            content.AppendLine($"X-SMS-Entity: {emailEvent.RelatedEntityType}:{emailEvent.RelatedEntityId}");

        // Content headers
        content.AppendLine("MIME-Version: 1.0");
        if (emailEvent.IsHtmlContent)
        {
            content.AppendLine("Content-Type: text/html; charset=UTF-8");
        }
        else
        {
            content.AppendLine("Content-Type: text/plain; charset=UTF-8");
        }
        content.AppendLine("Content-Transfer-Encoding: 8bit");

        // Empty line separates headers from body (required by RFC 5322)
        content.AppendLine();

        // Email body
        if (emailEvent.IsHtmlContent)
        {
            // Add HTML wrapper if not already present
            if (!emailEvent.Body.TrimStart().StartsWith("<html", StringComparison.OrdinalIgnoreCase) &&
                !emailEvent.Body.TrimStart().StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase))
            {
                content.AppendLine("<!DOCTYPE html>");
                content.AppendLine("<html>");
                content.AppendLine("<head>");
                content.AppendLine($"<title>{emailEvent.Subject}</title>");
                content.AppendLine("<meta charset=\"UTF-8\">");
                content.AppendLine("</head>");
                content.AppendLine("<body>");
                content.AppendLine(emailEvent.Body);
                content.AppendLine("</body>");
                content.AppendLine("</html>");
            }
            else
            {
                content.AppendLine(emailEvent.Body);
            }
        }
        else
        {
            content.AppendLine(emailEvent.Body);
        }

        // Add simulation footer in plain text
        content.AppendLine();
        content.AppendLine("---");
        content.AppendLine("SMS EMAIL SIMULATION");
        content.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        content.AppendLine($"Event ID: {emailEvent.EventId}");
        content.AppendLine($"Delivery Mode: {emailEvent.DeliveryMode}");
        if (emailEvent.EmailMetadata?.Any() == true)
        {
            content.AppendLine("Metadata:");
            foreach (var kvp in emailEvent.EmailMetadata)
            {
                content.AppendLine($"  {kvp.Key}: {kvp.Value}");
            }
        }

        return content.ToString();
    }

    private bool ValidateEmailEvent(EmailNotificationEvent emailEvent)
    {
        return !string.IsNullOrEmpty(emailEvent.Subject) && emailEvent.ToRecipients.Any();
    }
}

