using System.Threading;
using System.Threading.Tasks;

using SMS_Application.Interfaces;
using SMS_Domain.Enums;
using SMS_Domain.Events;
using SMS3.Components.Pages.SMSSystem.Models;

namespace SMS3.Components.Pages.SMSSystem.Services
{
    public class MockEmailSender : IEmailSender
    {
        private readonly IBaseEventBus _eventBus;

        public MockEmailSender(IBaseEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public async Task SendAsync(MailRequest request)
        {
            var emailEvent = new EmailNotificationEvent(
                toRecipients: request.To,
                subject: request.Subject,
                body: request.BodyHtml,
                isHtmlContent: true,
                priority: EmailPriority.Normal,
                workflowType: "ManualComposeDialog",
                relatedEntityType: "EmailComposeDialog",
                attachments: request.Attachments?.Select(a => new EmailAttachment
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
