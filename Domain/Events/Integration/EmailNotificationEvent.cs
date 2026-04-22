//-----------------------------------------------------------------------
// <copyright file="EmailNotificationEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Integration event for sending email notifications via external email services.
//                  Supports workflow-based email delivery for SPI alerts, hazard notifications,
//                  escalation communications, and mitigation approval processes.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Interfaces;

namespace SMS_Domain.Events.Integration;

/// <summary>
/// Integration event for sending email notifications
/// Handles external email delivery through email service providers
/// </summary>
public class EmailNotificationEvent : IIntegrationEvent
{
    public Guid EventId { get; private set; }
    public DateTime OccurredOn { get; private set; }
    public string EventType => "Integration.Email.Notification";
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
        bool isHtmlContent = false,
        List<string>? ccRecipients = null,
        List<string>? bccRecipients = null,
        EmailPriority priority = EmailPriority.Normal,
        string targetSystem = "EmailService",
        IntegrationDeliveryMode deliveryMode = IntegrationDeliveryMode.BestEffort,
        int maxRetryAttempts = 3,
        DateTime? scheduledDelivery = null,
        List<EmailAttachment>? attachments = null,
        string? relatedEntityId = null,
        string? relatedEntityType = null,
        string? workflowType = null,
        Dictionary<string, object>? emailMetadata = null,
        string? templateName = null,
        Dictionary<string, object>? templateData = null)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        TargetSystem = targetSystem;
        DeliveryMode = deliveryMode;
        MaxRetryAttempts = maxRetryAttempts;

        ToRecipients = toRecipients ?? throw new ArgumentNullException(nameof(toRecipients));
        if (!ToRecipients.Any())
            throw new ArgumentException("At least one recipient is required", nameof(toRecipients));

        CcRecipients = ccRecipients ?? new List<string>();
        BccRecipients = bccRecipients ?? new List<string>();
        Subject = subject ?? throw new ArgumentNullException(nameof(subject));
        Body = body ?? throw new ArgumentNullException(nameof(body));
        IsHtmlContent = isHtmlContent;

        Priority = priority;
        ScheduledDelivery = scheduledDelivery;
        Attachments = attachments ?? new List<EmailAttachment>();

        RelatedEntityId = relatedEntityId;
        RelatedEntityType = relatedEntityType;
        WorkflowType = workflowType;
        EmailMetadata = emailMetadata ?? new Dictionary<string, object>();

        TemplateName = templateName;
        TemplateData = templateData ?? new Dictionary<string, object>();
    }
}

/// <summary>
/// Email priority levels
/// </summary>
public enum EmailPriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Urgent = 4
}

/// <summary>
/// Email attachment information
/// </summary>
public class EmailAttachment
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public long Size { get; set; }
}