//-----------------------------------------------------------------------
// <copyright file="EmailNotificationEventHandler.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Integration event handler for email notification delivery in SMS system.
//                  Processes email notification events and coordinates with external email services
//                  for reliable delivery of safety-critical communications and workflow notifications.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SMS_Application.Interfaces;
using SMS_Application.Queries;
using SMS_Application.Services;
using SMS_Domain.Events;
using SMS_Domain.Common;
using SMS_Domain.Entities;

namespace SMS_Application.EventHandlers;

/// <summary>
/// Integration event handler for email notification delivery
/// Processes email events and coordinates with external email services
/// Phase 3: Provides reliable email delivery for safety-critical communications
/// </summary>
public class EmailNotificationEventHandler : BaseIntegrationEventHandler<EmailNotificationEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailNotificationEventHandler> _logger;
    private readonly IBaseMediator _mediator;
    private readonly ISMSApplicationGroupService _applicationGroupService;
    private readonly ISMSStakeholderGroupService _stakeholderGroupService;
    private readonly ISMSOrganizationalGroupService _organizationalGroupService;
    private readonly bool _useSimulation;

    public EmailNotificationEventHandler(
        ILogger<EmailNotificationEventHandler> logger,
        IEmailService emailService,
        IBaseMediator mediator,
        ISMSApplicationGroupService applicationGroupService,
        ISMSStakeholderGroupService stakeholderGroupService,
        ISMSOrganizationalGroupService organizationalGroupService,
        IOptions<SmtpEmailConfiguration> smtpConfig)
        : base(logger)
    {
        _logger = logger;
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _applicationGroupService = applicationGroupService ?? throw new ArgumentNullException(nameof(applicationGroupService));
        _stakeholderGroupService = stakeholderGroupService ?? throw new ArgumentNullException(nameof(stakeholderGroupService));
        _organizationalGroupService = organizationalGroupService ?? throw new ArgumentNullException(nameof(organizationalGroupService));

        // Use configuration-based simulation setting
        _useSimulation = smtpConfig?.Value?.UseSimulation ?? false;

        _logger.LogApplicationInformation("[EMAIL HANDLER] Email handler initialized with UseSimulation: {UseSimulation}", _useSimulation);
    }

    /// <summary>
    /// Processes email notification events for external delivery
    /// Phase 3: Implements both real and simulated email delivery
    /// </summary>
    protected override async Task<Result> ProcessIntegrationEventAsync(EmailNotificationEvent integrationEvent, CancellationToken cancellationToken)
    {
        try
        {
            await ResolveGroupContactRecipientsAsync(integrationEvent, cancellationToken);

            _logger.LogApplicationInformation("[EMAIL HANDLER] Processing email notification: '{Subject}' to {RecipientCount} recipients (Priority: {Priority}) - UseSimulation: {UseSimulation}",
                integrationEvent.Subject, integrationEvent.ToRecipients.Count, integrationEvent.Priority, _useSimulation);

            // Validate email event before processing
            if (!ValidateEmailEvent(integrationEvent))
            {
                _logger.LogApplicationError("[EMAIL HANDLER] Email event validation failed for: {Subject}", integrationEvent.Subject);
                return Result.Failure(new Error("INVALID_EMAIL_EVENT", "Email event validation failed"));
            }

            Result deliveryResult;

            if (_useSimulation)
            {
                _logger.LogApplicationInformation("[EMAIL HANDLER] Using simulation mode for email: {Subject}", integrationEvent.Subject);
                // Use simulation for development/testing
                deliveryResult = await SimulateEmailDelivery(integrationEvent, cancellationToken);
            }
            else
            {
                _logger.LogApplicationInformation("[EMAIL HANDLER] Using real email delivery for: {Subject}", integrationEvent.Subject);
                // Use real email delivery for production
                deliveryResult = await SendRealEmail(integrationEvent, cancellationToken);
            }

            if (deliveryResult.IsSuccess)
            {
                _logger.LogApplicationInformation("[EMAIL HANDLER] Email notification sent successfully: '{Subject}' to {RecipientCount} recipients", integrationEvent.Subject, integrationEvent.ToRecipients.Count);
            }
            else
            {
                _logger.LogApplicationError("[EMAIL HANDLER] Email notification failed: '{Subject}' - {Error}",integrationEvent.Subject, deliveryResult.Error.Message);
            }

            return deliveryResult;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "[EMAIL HANDLER] Failed to process email notification: '{Subject}'", integrationEvent.Subject);
            return Result.Failure(new Error("EMAIL_NOTIFICATION_FAILED", $"Email notification failed: {ex.Message}"));
        }
    }

    // Helper methods...
    private async Task<Result> SendRealEmail(EmailNotificationEvent emailEvent, CancellationToken cancellationToken)
    {
        var deliveryResult = await _emailService.SendEmailAsync(emailEvent, cancellationToken);
        if (deliveryResult.IsFailure || deliveryResult.Value is null)
        {
            _logger.LogApplicationError("[EMAIL HANDLER] Email service failed for '{Subject}'. Error: {Error}",
                emailEvent.Subject,
                deliveryResult.Error?.Message ?? "Unknown email service error");

            return Result.Failure(deliveryResult.Error ?? new Error("EMAIL_SEND_FAILED", "Email delivery failed."));
        }

        if (!deliveryResult.Value.IsDelivered)
        {
            _logger.LogApplicationError("[EMAIL HANDLER] Email service reported undelivered message for '{Subject}'. Status: {Status}. Error: {Error}",
                emailEvent.Subject,
                deliveryResult.Value.DeliveryStatus,
                deliveryResult.Value.ErrorMessage);

            return Result.Failure(new Error("EMAIL_SEND_FAILED", deliveryResult.Value.ErrorMessage ?? "Email delivery was not successful."));
        }

        _logger.LogApplicationInformation("[EMAIL HANDLER] Email provider confirmed send for '{Subject}' (MessageId: {MessageId})",
            emailEvent.Subject,
            deliveryResult.Value.MessageId);

        return Result.Success();
    }

    private async Task<Result> SimulateEmailDelivery(EmailNotificationEvent emailEvent, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogApplicationInformation("[EMAIL SIM] Starting email simulation for: {Subject}", emailEvent.Subject);

            // Use original simulation directory
            var simulationDir = @"C:\temp\sms_emails";
            Directory.CreateDirectory(simulationDir);

            _logger.LogApplicationInformation("[EMAIL SIM] Using simulation directory: {Directory}", simulationDir);

            // Generate unique filename with timestamp in .eml format (email message format)
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var safeSubject = string.Join("_", emailEvent.Subject.Split(Path.GetInvalidFileNameChars()));
            // Limit subject length for filename safety
            if (safeSubject.Length > 50)
                safeSubject = safeSubject.Substring(0, 50);
            var filename = $"SMS_Email_{timestamp}_{safeSubject}.eml";
            var filePath = Path.Combine(simulationDir, filename);

            // Create .eml format content (RFC 5322 format)
            var emlContent = GenerateEmlContent(emailEvent);

            // Write to file
            await File.WriteAllTextAsync(filePath, emlContent, System.Text.Encoding.UTF8, cancellationToken);

            _logger.LogApplicationInformation("[EMAIL SIM] SIMULATED EMAIL: '{Subject}' to {RecipientCount} recipients - EML file saved to: {FilePath}", 
                emailEvent.Subject, emailEvent.ToRecipients.Count, filePath);

            // Also log key details to console for immediate feedback
            _logger.LogApplicationInformation("[EMAIL SIM] Email Details: To: {Recipients} | Subject: {Subject} | Priority: {Priority}", 
                string.Join(", ", emailEvent.ToRecipients), emailEvent.Subject, emailEvent.Priority);

            _logger.LogApplicationInformation("[EMAIL SIM] File written successfully: {FileName} ({FileSize} bytes)", 
                Path.GetFileName(filePath), emlContent.Length);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Failed to simulate email delivery for: {Subject}", emailEvent.Subject);
            return Result.Failure(new Error("EMAIL_SIMULATION_FAILED", $"Email simulation failed: {ex.Message}"));
        }
    }

    /// <summary>
    /// Generates RFC 5322 compliant .eml file content that can be opened by email clients
    /// </summary>
    private string GenerateEmlContent(EmailNotificationEvent emailEvent)
    {
        var content = new System.Text.StringBuilder();

        // RFC 5322 Email Headers
        content.AppendLine("From: SMS Safety Management System <noreply@pdxairport.com>");
        content.AppendLine($"To: {string.Join(", ", emailEvent.ToRecipients)}");

        if (emailEvent.CcRecipients?.Any() == true)
            content.AppendLine($"Cc: {string.Join(", ", emailEvent.CcRecipients)}");

        if (emailEvent.BccRecipients?.Any() == true)
            content.AppendLine($"Bcc: {string.Join(", ", emailEvent.BccRecipients)}");

        content.AppendLine($"Subject: {emailEvent.Subject}");
        content.AppendLine($"Date: {emailEvent.OccurredOn:R}"); // RFC 1123 date format
        content.AppendLine($"Message-ID: <{emailEvent.EventId}@sms.pdxairport.com>");

        // Priority header
        var priority = emailEvent.Priority switch
        {
            EmailPriority.Low => "5 (Lowest)",
            EmailPriority.Normal => "3 (Normal)", 
            EmailPriority.High => "1 (Highest)",
            EmailPriority.Urgent => "1 (Highest)",
            _ => "3 (Normal)"
        };
        content.AppendLine($"X-Priority: {priority}");

        // SMS-specific headers for tracking
        content.AppendLine($"X-SMS-Event-ID: {emailEvent.EventId}");
        content.AppendLine($"X-SMS-Event-Type: {emailEvent.EventType}");
        if (!string.IsNullOrWhiteSpace(emailEvent.ReportId))
            content.AppendLine($"X-SMS-Report-ID: {emailEvent.ReportId}");
        if (!string.IsNullOrEmpty(emailEvent.WorkflowType))
            content.AppendLine($"X-SMS-Workflow: {emailEvent.WorkflowType}");
        if (!string.IsNullOrEmpty(emailEvent.RelatedEntityId))
            content.AppendLine($"X-SMS-Entity: {emailEvent.RelatedEntityType}:{emailEvent.RelatedEntityId}");

        // Content headers
        content.AppendLine("MIME-Version: 1.0");
        if (emailEvent.IsHtmlContent)
        {
            content.AppendLine("Content-Type: text/html; charset=UTF-8");
        }
        else
        {
            content.AppendLine("Content-Type: text/plain; charset=UTF-8");
        }
        content.AppendLine("Content-Transfer-Encoding: 8bit");

        // Empty line separates headers from body (required by RFC 5322)
        content.AppendLine();

        // Email body
        if (emailEvent.IsHtmlContent)
        {
            // Add HTML wrapper if not already present
            if (!emailEvent.Body.TrimStart().StartsWith("<html", StringComparison.OrdinalIgnoreCase) &&
                !emailEvent.Body.TrimStart().StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase))
            {
                content.AppendLine("<!DOCTYPE html>");
                content.AppendLine("<html>");
                content.AppendLine("<head>");
                content.AppendLine($"<title>{emailEvent.Subject}</title>");
                content.AppendLine("<meta charset=\"UTF-8\">");
                content.AppendLine("</head>");
                content.AppendLine("<body>");
                content.AppendLine(emailEvent.Body);
                content.AppendLine("</body>");
                content.AppendLine("</html>");
            }
            else
            {
                content.AppendLine(emailEvent.Body);
            }
        }
        else
        {
            content.AppendLine(emailEvent.Body);
        }

        // Add simulation footer in plain text
        content.AppendLine();
        content.AppendLine("---");
        content.AppendLine("SMS EMAIL SIMULATION");
        content.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        content.AppendLine($"Event ID: {emailEvent.EventId}");
        content.AppendLine($"Delivery Mode: {emailEvent.DeliveryMode}");
        if (emailEvent.EmailMetadata?.Any() == true)
        {
            content.AppendLine("Metadata:");
            foreach (var kvp in emailEvent.EmailMetadata)
            {
                content.AppendLine($"  {kvp.Key}: {kvp.Value}");
            }
        }

        return content.ToString();
    }

    private bool ValidateEmailEvent(EmailNotificationEvent emailEvent)
    {
        return !string.IsNullOrEmpty(emailEvent.Subject) && emailEvent.ToRecipients.Any();
    }

    private async Task ResolveGroupContactRecipientsAsync(EmailNotificationEvent emailEvent, CancellationToken cancellationToken)
    {
        var resolvedTo = await ResolveRecipientsAsync(emailEvent.ToRecipients, cancellationToken, enforceGroupContactOnly: true);
        var resolvedCc = await ResolveRecipientsAsync(emailEvent.CcRecipients, cancellationToken, enforceGroupContactOnly: false);
        var resolvedBcc = await ResolveRecipientsAsync(emailEvent.BccRecipients, cancellationToken, enforceGroupContactOnly: false);

        emailEvent.ToRecipients.Clear();
        emailEvent.ToRecipients.AddRange(resolvedTo);

        emailEvent.CcRecipients.Clear();
        emailEvent.CcRecipients.AddRange(resolvedCc);

        emailEvent.BccRecipients.Clear();
        emailEvent.BccRecipients.AddRange(resolvedBcc);
    }

    private async Task<List<string>> ResolveRecipientsAsync(
        IEnumerable<string> recipients,
        CancellationToken cancellationToken,
        bool enforceGroupContactOnly)
    {
        var resolved = new List<string>();
        var unresolved = new List<string>();

        foreach (var recipient in recipients.Where(r => !string.IsNullOrWhiteSpace(r)))
        {
            var normalizedRecipient = recipient.Trim();
            var groupContactEmails = await ResolveGroupContactEmailsForRecipientAsync(normalizedRecipient, cancellationToken);

            if (groupContactEmails.Count > 0)
            {
                resolved.AddRange(groupContactEmails);
                continue;
            }

            if (enforceGroupContactOnly && IsValidEmail(normalizedRecipient))
            {
                var directGroupContactMatch = await ResolveGroupContactEmailsByContactAddressAsync(normalizedRecipient, cancellationToken);
                if (directGroupContactMatch.Count > 0)
                {
                    resolved.AddRange(directGroupContactMatch);
                    continue;
                }
            }

            if (enforceGroupContactOnly)
            {
                unresolved.Add(normalizedRecipient);
            }
            else
            {
                resolved.Add(normalizedRecipient);
            }
        }

        if (enforceGroupContactOnly && unresolved.Count > 0)
        {
            _logger.LogApplicationWarning(
                "[EMAIL HANDLER] Group-contact-only routing dropped {UnresolvedCount} recipient token(s): {Recipients}",
                unresolved.Count,
                string.Join(", ", unresolved));
        }

        return resolved
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private async Task<List<string>> ResolveGroupContactEmailsByContactAddressAsync(string emailAddress, CancellationToken cancellationToken)
    {
        var normalized = emailAddress.Trim();
        var resolvedGroupEmails = new List<string>();

        await AppendGroupContactEmailsByAddressAsync(
            () => _applicationGroupService.GetAllSMSApplicationGroupsAsync(cancellationToken),
            normalized,
            resolvedGroupEmails);

        await AppendGroupContactEmailsByAddressAsync(
            () => _stakeholderGroupService.GetAllSMSStakeholderGroupsAsync(cancellationToken),
            normalized,
            resolvedGroupEmails);

        await AppendGroupContactEmailsByAddressAsync(
            () => _organizationalGroupService.GetAllSMSOrganizationalGroupsAsync(cancellationToken),
            normalized,
            resolvedGroupEmails);

        return resolvedGroupEmails
            .Where(IsValidEmail)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private async Task AppendOrganizationalAuthorityLevelContactEmailsByRecipientAsync(
        string recipient,
        List<string> resolvedGroupEmails,
        CancellationToken cancellationToken)
    {
        var organizationalUser = await ResolveOrganizationalUserFromRecipientAsync(recipient, cancellationToken);
        if (organizationalUser is null || string.IsNullOrWhiteSpace(organizationalUser.Code))
        {
            return;
        }

        var groupsResult = await _organizationalGroupService.GetSMSOrganizationalGroupsByUserCodeAsync(organizationalUser.Code.Trim(), cancellationToken);
        if (groupsResult.IsFailure || groupsResult.Value is null)
        {
            return;
        }

        var userAuthorityLevel = ResolveAuthorityLevelName(organizationalUser.AuthorityLevel);

        var matchingAuthorityEmails = groupsResult.Value
            .Where(g => g.IsActive)
            .Where(g => !string.IsNullOrWhiteSpace(g.ContactEmail))
            .Where(g => string.Equals(g.AuthorityLevel ?? string.Empty, userAuthorityLevel, StringComparison.OrdinalIgnoreCase))
            .Select(g => g.ContactEmail!.Trim())
            .Where(IsValidEmail)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (matchingAuthorityEmails.Count > 0)
        {
            resolvedGroupEmails.AddRange(matchingAuthorityEmails);
            return;
        }

        var fallbackEmails = groupsResult.Value
            .Where(g => g.IsActive)
            .Where(g => !string.IsNullOrWhiteSpace(g.ContactEmail))
            .Select(g => g.ContactEmail!.Trim())
            .Where(IsValidEmail)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        resolvedGroupEmails.AddRange(fallbackEmails);
    }

    private async Task<SMSOrganizationalUser?> ResolveOrganizationalUserFromRecipientAsync(string recipient, CancellationToken cancellationToken)
    {
        var normalized = recipient.Trim();

        if (!normalized.Contains('@'))
        {
            var byCode = await _mediator.SendAsync(new GetSMSOrganizationalUserByCodeQuery(normalized), cancellationToken);
            if (byCode.IsSuccess && byCode.Value is not null)
            {
                return byCode.Value;
            }
        }

        var candidateUserNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { normalized };
        var atSymbolIndex = normalized.IndexOf('@');
        if (atSymbolIndex > 0)
        {
            candidateUserNames.Add(normalized[..atSymbolIndex]);
        }

        foreach (var userName in candidateUserNames)
        {
            var byUserName = await _mediator.SendAsync(new GetSMSOrganizationalUserByUserNameQuery(userName), cancellationToken);
            if (byUserName.IsSuccess && byUserName.Value is not null)
            {
                return byUserName.Value;
            }
        }

        return null;
    }

    private static string ResolveAuthorityLevelName(int? authorityLevel)
    {
        if (!authorityLevel.HasValue)
        {
            return "Standard";
        }

        return authorityLevel.Value switch
        {
            >= 9 => "Executive",
            >= 7 => "Strategic",
            >= 5 => "Operational",
            >= 3 => "Process",
            >= 1 => "Support",
            _ => "Standard"
        };
    }

    private async Task<List<string>> ResolveGroupContactEmailsForRecipientAsync(string recipient, CancellationToken cancellationToken)
    {
        var resolvedGroupEmails = new List<string>();

        await AppendGroupContactEmailByGroupCodeAsync(_applicationGroupService.GetSMSApplicationGroupByCodeAsync, recipient, resolvedGroupEmails, cancellationToken);
        await AppendGroupContactEmailByGroupCodeAsync(_stakeholderGroupService.GetSMSStakeholderGroupByCodeAsync, recipient, resolvedGroupEmails, cancellationToken);
        await AppendGroupContactEmailByGroupCodeAsync(_organizationalGroupService.GetSMSOrganizationalGroupByCodeAsync, recipient, resolvedGroupEmails, cancellationToken);

        // Treat recipient token as a user code first (e.g., OU-0001 / SU-0001 / AU-0001)
        await AppendGroupContactEmailsByUserCodeAsync(
            () => _applicationGroupService.GetSMSApplicationGroupsByUserCodeAsync(recipient, cancellationToken),
            resolvedGroupEmails);

        await AppendGroupContactEmailsByUserCodeAsync(
            () => _stakeholderGroupService.GetSMSStakeholderGroupsByUserCodeAsync(recipient, cancellationToken),
            resolvedGroupEmails);

        await AppendGroupContactEmailsByUserCodeAsync(
            () => _organizationalGroupService.GetSMSOrganizationalGroupsByUserCodeAsync(recipient, cancellationToken),
            resolvedGroupEmails);

        await AppendOrganizationalAuthorityLevelContactEmailsByRecipientAsync(recipient, resolvedGroupEmails, cancellationToken);

        var candidateUserNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { recipient };

        var atSymbolIndex = recipient.IndexOf('@');
        if (atSymbolIndex > 0)
        {
            candidateUserNames.Add(recipient[..atSymbolIndex]);
        }

        foreach (var candidateUserName in candidateUserNames)
        {
            var userCode = await ResolveUserCodeFromUserNameAsync(candidateUserName, cancellationToken);
            if (string.IsNullOrWhiteSpace(userCode))
            {
                continue;
            }

            await AppendGroupContactEmailsByUserCodeAsync(
                () => _applicationGroupService.GetSMSApplicationGroupsByUserCodeAsync(userCode, cancellationToken),
                resolvedGroupEmails);

            await AppendGroupContactEmailsByUserCodeAsync(
                () => _stakeholderGroupService.GetSMSStakeholderGroupsByUserCodeAsync(userCode, cancellationToken),
                resolvedGroupEmails);

            await AppendGroupContactEmailsByUserCodeAsync(
                () => _organizationalGroupService.GetSMSOrganizationalGroupsByUserCodeAsync(userCode, cancellationToken),
                resolvedGroupEmails);

            await AppendOrganizationalAuthorityLevelContactEmailsByRecipientAsync(userCode, resolvedGroupEmails, cancellationToken);
        }

        return resolvedGroupEmails
            .Where(IsValidEmail)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private async Task AppendGroupContactEmailByGroupCodeAsync<TGroup>(
        Func<string, CancellationToken, Task<Result<TGroup>>> groupLookup,
        string groupCode,
        List<string> resolvedGroupEmails,
        CancellationToken cancellationToken)
        where TGroup : BaseUserGroup
    {
        var groupResult = await groupLookup(groupCode, cancellationToken);
        if (groupResult.IsSuccess && !string.IsNullOrWhiteSpace(groupResult.Value?.ContactEmail))
        {
            resolvedGroupEmails.Add(groupResult.Value.ContactEmail.Trim());
        }
    }

    private static async Task AppendGroupContactEmailsByUserCodeAsync<TGroup>(
        Func<Task<Result<IEnumerable<TGroup>>>> groupLookup,
        List<string> resolvedGroupEmails)
        where TGroup : BaseUserGroup
    {
        var groupsResult = await groupLookup();
        if (groupsResult.IsFailure || groupsResult.Value is null)
        {
            return;
        }

        foreach (var group in groupsResult.Value)
        {
            if (!string.IsNullOrWhiteSpace(group.ContactEmail))
            {
                resolvedGroupEmails.Add(group.ContactEmail.Trim());
            }
        }
    }

    private static async Task AppendGroupContactEmailsByAddressAsync<TGroup>(
        Func<Task<Result<IEnumerable<TGroup>>>> groupLookup,
        string emailAddress,
        List<string> resolvedGroupEmails)
        where TGroup : BaseUserGroup
    {
        var groupsResult = await groupLookup();
        if (groupsResult.IsFailure || groupsResult.Value is null)
        {
            return;
        }

        foreach (var group in groupsResult.Value)
        {
            if (string.IsNullOrWhiteSpace(group.ContactEmail))
            {
                continue;
            }

            if (string.Equals(group.ContactEmail.Trim(), emailAddress, StringComparison.OrdinalIgnoreCase))
            {
                resolvedGroupEmails.Add(group.ContactEmail.Trim());
            }
        }
    }

    private async Task<string?> ResolveUserCodeFromUserNameAsync(string userName, CancellationToken cancellationToken)
    {
        try
        {
            var appUserResult = await _mediator.SendAsync(new GetSMSApplicationUserByUserNameQuery(userName), cancellationToken);
            if (appUserResult.IsSuccess && !string.IsNullOrWhiteSpace(appUserResult.Value?.Code))
            {
                return appUserResult.Value.Code.Trim();
            }

            var stakeholderUserResult = await _mediator.SendAsync(new GetSMSStakeholderUserByUserNameQuery(userName), cancellationToken);
            if (stakeholderUserResult.IsSuccess && !string.IsNullOrWhiteSpace(stakeholderUserResult.Value?.Code))
            {
                return stakeholderUserResult.Value.Code.Trim();
            }

            var organizationalUserResult = await _mediator.SendAsync(new GetSMSOrganizationalUserByUserNameQuery(userName), cancellationToken);
            if (organizationalUserResult.IsSuccess && !string.IsNullOrWhiteSpace(organizationalUserResult.Value?.Code))
            {
                return organizationalUserResult.Value.Code.Trim();
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationDebug(ex, "[EMAIL HANDLER] User name lookup failed for recipient token '{UserName}'", userName);
            return null;
        }
    }

    private static bool IsValidEmail(string value)
    {
        try
        {
            _ = new System.Net.Mail.MailAddress(value);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

