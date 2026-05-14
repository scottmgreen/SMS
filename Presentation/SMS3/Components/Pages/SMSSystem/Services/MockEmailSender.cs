using System.Threading.Tasks;
using SMS3.Components.Pages.SMSSystem.Models;

namespace SMS3.Components.Pages.SMSSystem.Services
{
    public class MockEmailSender : IEmailSender
    {
        public Task SendAsync(MailRequest request)
        {
            // Simulate sending email (no-op)
            return Task.CompletedTask;
        }
    }
}
