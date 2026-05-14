using System.Collections.Generic;

namespace SMS3.Components.Pages.SMSSystem.Models
{

    public interface IEmailSender
    {
        Task SendAsync(MailRequest request);
    }

    public class EmailComposeModel
    {
        public List<string> To { get; set; } = new();
        public List<string> Cc { get; set; } = new();
        public List<string> Bcc { get; set; } = new();
        public string? Subject { get; set; }
        public string? BodyHtml { get; set; }
    }

    // Backend-agnostic mail request (used by IEmailSender)
    public class MailRequest
    {
        public List<string> To { get; set; } = new();
        public List<string> Cc { get; set; } = new();
        public List<string> Bcc { get; set; } = new();
        public string Subject { get; set; } = string.Empty;
        public string BodyHtml { get; set; } = string.Empty;
    }
}