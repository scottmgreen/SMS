using System.Threading;
using System.Threading.Tasks;

using SMS_Application.Interfaces;
using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Domain.Events;

namespace SMS3.Components.Pages.SMSSystem.Services
{
    public class MockEmailSender : IEmailSender
    {
        private readonly IBaseEventBus _eventBus;

        public MockEmailSender(IBaseEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public async Task SendAsync(EmailNotification notification)
        {
            var emailEvent = new EmailNotificationEvent(
                toRecipients: notification.To,
                subject: notification.Subject ?? string.Empty,
                body: notification.BodyHtml ?? string.Empty,
                isHtmlContent: true,
                priority: EmailPriority.Normal,
                workflowType: "ManualComposeDialog",
                relatedEntityType: "EmailComposeDialog",
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
    }
}
