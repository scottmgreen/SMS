//-----------------------------------------------------------------------
// <copyright file="EmailNotificationEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Integration event for sending email notifications via external email services.
//                  Supports workflow-based email delivery for SPI alerts, hazard notifications,
//                  escalation communications, and mitigation approval processes.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Events;

/// <summary>
/// Integration event for sending email notifications
/// Handles external email delivery through email service providers
/// </summary>
public class EmailNotificationEvent : IBaseIntegrationEvent
{
    public Guid EventId { get; private set; }
    public DateTime OccurredOn { get; private set; }
    public string EventType => SMS_Domain.Enums.EventType.EmailNotification.Value;
    public string ReportId { get; private set; }
    public string TargetSystem { get; private set; }
    public IntegrationDeliveryMode DeliveryMode { get; private set; }
    public int MaxRetryAttempts { get; private set; }

    // Email content
    public List<string> ToRecipients { get; private set; }
    public List<string> CcRecipients { get; private set; }
    public List<string> BccRecipients { get; private set; }
    public string Subject { get; private set; }
    public string Body { get; private set; }
    public bool IsHtmlContent { get; private set; }

    // Email settings
    public EmailPriority Priority { get; private set; }
    public DateTime? ScheduledDelivery { get; private set; }
    public List<EmailAttachment> Attachments { get; private set; }

    // SMS context
    public string? RelatedEntityId { get; private set; }
    public string? RelatedEntityType { get; private set; }
    public string? WorkflowType { get; private set; }
    public Dictionary<string, object> EmailMetadata { get; private set; }

    // Template support
    public string? TemplateName { get; private set; }
    public Dictionary<string, object> TemplateData { get; private set; }

    public EmailNotificationEvent(
        List<string> toRecipients,
        string subject,
        string body,
        bool isHtmlContent = true,
        EmailPriority priority = EmailPriority.Normal,
        IntegrationDeliveryMode deliveryMode = IntegrationDeliveryMode.BestEffort,
        string? reportId = null,
        string? workflowType = null,
        string? relatedEntityType = null,
        string? relatedEntityId = null,
        List<string>? ccRecipients = null,
        List<string>? bccRecipients = null,
        DateTime? scheduledDelivery = null,
        List<EmailAttachment>? attachments = null,
        string? templateName = null,
        Dictionary<string, object>? templateData = null,
        Dictionary<string, object>? emailMetadata = null,
        int maxRetryAttempts = 3)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        ReportId = ResolveReportId(reportId, relatedEntityType, relatedEntityId);
        TargetSystem = "EmailService";

        ToRecipients = toRecipients ?? throw new ArgumentNullException(nameof(toRecipients));
        Subject = subject ?? throw new ArgumentNullException(nameof(subject));
        Body = body ?? throw new ArgumentNullException(nameof(body));
        IsHtmlContent = isHtmlContent;
        Priority = priority;
        DeliveryMode = deliveryMode;
        WorkflowType = workflowType;
        RelatedEntityType = relatedEntityType;
        RelatedEntityId = relatedEntityId;
        CcRecipients = ccRecipients ?? new List<string>();
        BccRecipients = bccRecipients ?? new List<string>();
        ScheduledDelivery = scheduledDelivery;
        Attachments = attachments ?? new List<EmailAttachment>();
        TemplateName = templateName;
        TemplateData = templateData ?? new Dictionary<string, object>();
        EmailMetadata = emailMetadata ?? new Dictionary<string, object>();
        MaxRetryAttempts = maxRetryAttempts;

        if (!ToRecipients.Any())
        {
            throw new ArgumentException("At least one recipient must be specified", nameof(toRecipients));
        }
    }

    private static string ResolveReportId(string? reportId, string? relatedEntityType, string? relatedEntityId)
    {
        if (!string.IsNullOrWhiteSpace(reportId))
        {
            return reportId.Trim();
        }

        if (string.Equals(relatedEntityType, "Report", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(relatedEntityId))
        {
            return relatedEntityId.Trim();
        }

        return string.Empty;
    }
}

/// <summary>
/// Email priority levels
/// </summary>
public enum EmailPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Urgent = 3
}

/// <summary>
/// Email attachment information
/// </summary>
public class EmailAttachment
{
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/octet-stream";
}