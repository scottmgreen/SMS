using SMS_Domain.Entities;

namespace SMS_Application.Interfaces;

public interface IEmailSender
{
    Task SendAsync(EmailNotification notification);
}
