namespace SMS_Domain.Entities;

public sealed class EmailNotification
{
    public List<string> To { get; set; } = new();
    public List<string> Cc { get; set; } = new();
    public List<string> Bcc { get; set; } = new();
    public bool HasToValidationError { get; set; }
    public string? ToValidationMessage { get; set; }
    public string? Subject { get; set; }
    public string? BodyHtml { get; set; }
    public List<MailAttachment> Attachments { get; set; } = new();
}
