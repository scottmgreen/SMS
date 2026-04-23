//-----------------------------------------------------------------------
// <copyright file="IEmailService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Email service interface for SMS safety-critical communications.
//                  Supports multiple email providers and delivery modes for reliable
//                  delivery of hazard notifications, escalations, and workflow communications.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Events.Integration;
using SMS_Domain.Common;

namespace SMS_Application.Interfaces;

/// <summary>
/// Email service interface for safety-critical communications
/// Supports multiple email providers and delivery modes
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email notification with full delivery tracking
    /// </summary>
    /// <param name="emailEvent">Email notification event with all details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success/failure with delivery details</returns>
    Task<Result<EmailDeliveryResult>> SendEmailAsync(EmailNotificationEvent emailEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates email configuration and connectivity
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating service health</returns>
    Task<Result> ValidateServiceAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets delivery status for a previously sent email
    /// </summary>
    /// <param name="messageId">Unique message identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current delivery status</returns>
    Task<Result<EmailDeliveryStatus>> GetDeliveryStatusAsync(string messageId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Email delivery result information
/// </summary>
public class EmailDeliveryResult
{
    public string MessageId { get; set; } = string.Empty;
    public bool IsDelivered { get; set; }
    public string DeliveryStatus { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public List<string> SuccessfulRecipients { get; set; } = new();
    public List<string> FailedRecipients { get; set; } = new();
    public string ErrorMessage { get; set; } = string.Empty;
    public string ProviderResponse { get; set; } = string.Empty;
}

/// <summary>
/// Email delivery status tracking
/// </summary>
public enum EmailDeliveryStatus
{
    Pending = 0,
    Sent = 1,
    Delivered = 2,
    Failed = 3,
    Bounced = 4,
    Rejected = 5
}