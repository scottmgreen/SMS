using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SMS_Application.Interfaces;
using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Domain.Events;

namespace SMS3.Components.Pages.SMSSystem.Services
{
    public sealed class EventBusEmailSender : IEmailSender
    {
        private const string AllowDirectToRecipientsMetadataKey = "AllowDirectToRecipients";
        private readonly IBaseEventBus _eventBus;

        public EventBusEmailSender(IBaseEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public async Task SendAsync(EmailNotification notification)
        {
            var toRecipients = NormalizeRecipients(notification.To);
            var ccRecipients = NormalizeRecipients(notification.Cc);
            var bccRecipients = NormalizeRecipients(notification.Bcc);

            if (toRecipients.Count == 0)
            {
                var message = string.IsNullOrWhiteSpace(notification.ToValidationMessage)
                    ? "Cannot send email: no valid To recipients were provided."
                    : $"Cannot send email: {notification.ToValidationMessage}";

                throw new InvalidOperationException(message);
            }

            var emailMetadata = new Dictionary<string, object>
            {
                [AllowDirectToRecipientsMetadataKey] = notification.AllowDirectToRecipients
            };

            var emailEvent = new EmailNotificationEvent(
                toRecipients: toRecipients,
                subject: notification.Subject ?? string.Empty,
                body: notification.BodyHtml ?? string.Empty,
                isHtmlContent: true,
                priority: EmailPriority.Normal,
                workflowType: "ManualComposeDialog",
                relatedEntityType: "EmailComposeDialog",
                ccRecipients: ccRecipients,
                bccRecipients: bccRecipients,
                emailMetadata: emailMetadata,
                attachments: notification.Attachments?.Select(a => new EmailAttachment
                {
                    FileName = a.FileName,
                    Content = a.Content,
                    ContentType = a.ContentType
                }).ToList());

            var result = await _eventBus.PublishIntegrationEventAsync(emailEvent, EventExecutionMode.Immediate, CancellationToken.None).ConfigureAwait(false);
            if (result.IsFailure)
            {
                throw new InvalidOperationException(result.Error?.Message ?? "Email send failed.");
            }
        }

        private static List<string> NormalizeRecipients(IEnumerable<string>? recipients)
        {
            if (recipients is null)
            {
                return new List<string>();
            }

            return recipients
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Select(r => r.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
