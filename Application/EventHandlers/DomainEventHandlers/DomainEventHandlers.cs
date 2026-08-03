//-----------------------------------------------------------------------
// <copyright file="DomainEventHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Consolidated domain event handlers for hazard and SPI automation.
// </copyright>
//-----------------------------------------------------------------------



using SMS_Domain.Events;
using SMS_Application.Common;
using SMS_Domain.Entities;
using SMS_Application.Queries;

namespace SMS_Application.EventHandlers
{
       



/// <summary>
/// Handles hazard creation through the unified flow: domain acknowledgement, SPI automation,
/// integration notification, and UI notification.
/// </summary>
public sealed class HazardCreatedEventHandler : BaseDomainEventHandler<SMS_Domain.Events.HazardCreatedEvent>
{
    private readonly IBaseEventBus _eventBus;
    private readonly ISPIAutomationService _spiAutomationService;
    private readonly ILogger<HazardCreatedEventHandler> _logger;

    public HazardCreatedEventHandler(
        ILogger<HazardCreatedEventHandler> logger,
        IBaseEventBus eventBus,
        ISPIAutomationService spiAutomationService)
        : base(logger, eventBus)
    {
        _logger = logger;
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _spiAutomationService = spiAutomationService ?? throw new ArgumentNullException(nameof(spiAutomationService));
    }

    protected override async Task<Result> ProcessEventAsync(SMS_Domain.Events.HazardCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogApplicationInformation("[HAZARD HANDLER] Processing hazard creation for {HazardCode} (Type: {HazardType}, Priority: {Priority})", domainEvent.HazardCode, domainEvent.HazardType, domainEvent.Priority);

            // Unified execution order: domain -> SPI -> integration -> UI.
            var domainResult = await HandleDomainEventAsync(domainEvent, cancellationToken);
            if (domainResult.IsFailure)
            {
                return domainResult;
            }

            var spiAutomationResult = await HandleSPIAutomationAsync(domainEvent, cancellationToken);
            if (spiAutomationResult.IsFailure)
            {
                return spiAutomationResult;
            }

            var integrationResult = await HandleIntegrationEventAsync(domainEvent, cancellationToken);
            if (integrationResult.IsFailure)
            {
                return integrationResult;
            }

            var uiResult = await HandleUIEventAsync(domainEvent, cancellationToken);
            if (uiResult.IsFailure)
            {
                return uiResult;
            }

            _logger.LogApplicationInformation("[HAZARD HANDLER] Successfully processed hazard creation for {HazardCode}", domainEvent.HazardCode);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "[HAZARD HANDLER] Error processing hazard creation event for {HazardCode}", domainEvent.HazardCode);
            return Result.Failure(new Error("HAZARD_CREATION_HANDLER_ERROR", $"Hazard creation processing failed: {ex.Message}"));
        }
    }

    private Task<Result> HandleDomainEventAsync(SMS_Domain.Events.HazardCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogApplicationInformation("[DOMAIN EVENT] HazardCreatedEvent received for {HazardCode}", domainEvent.HazardCode);
        return Task.FromResult(Result.Success());
    }

    private async Task<Result> HandleSPIAutomationAsync(SMS_Domain.Events.HazardCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        try
        {
            var hazardRateResult = await _spiAutomationService.UpdateHazardReportRateAsync(domainEvent.CreatedDate, cancellationToken);
            if (hazardRateResult.IsFailure)
            {
                _logger.LogApplicationWarning(
                    "[SPI AUTOMATION] Failed to update Hazard Report Rate SPI for {HazardCode}: {Error}",
                    ApplicationEventIds.Warning,
                    domainEvent.HazardCode,
                    hazardRateResult.Error?.Message ?? "Unknown SPI automation error");

                return Result.Failure(hazardRateResult.Error ?? new Error("SPI_AUTOMATION_FAILED", "Failed to update Hazard Report Rate SPI"));
            }

            _logger.LogApplicationInformation(
                "[SPI AUTOMATION] Successfully updated Hazard Report Rate SPI for {HazardCode}",
                ApplicationEventIds.Information,
                domainEvent.HazardCode);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(
                ex,
                "[SPI AUTOMATION] Error updating Hazard Report Rate SPI for {HazardCode}",
                ApplicationEventIds.Error,
                domainEvent.HazardCode);

            return Result.Failure(new Error("SPI_AUTOMATION_FAILED", $"Hazard SPI automation failed: {ex.Message}"));
        }
    }

    private async Task<Result> HandleIntegrationEventAsync(SMS_Domain.Events.HazardCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        await PublishEmailNotification(domainEvent, cancellationToken);
        return Result.Success();
    }

    private async Task<Result> HandleUIEventAsync(SMS_Domain.Events.HazardCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        await PublishUINotification(domainEvent, cancellationToken);
        return Result.Success();
    }

    private async Task PublishUINotification(SMS_Domain.Events.HazardCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        try
        {
            var uiNotificationEvent = new UINotificationEvent(
                severity: UINotificationSeverity.Info,
                title: "Hazard Created",
                message: BuildNotificationMessage(domainEvent),
                duration: 5000,
                category: "Hazard",
                sourceLayer: "DomainEventHandler",
                reportId: HandlerHelpers.ResolveReportId(domainEvent),
                targetComponent: "NotificationCenter",
                priority: GetUINotificationPriority(domainEvent.Priority),
                metadata: new Dictionary<string, object>
                {
                    { "HazardCode", domainEvent.HazardCode },
                    { "HazardType", domainEvent.HazardType },
                    { "Priority", domainEvent.Priority.ToString() },
                    { "CreatedBy", domainEvent.CreatedBy },
                    { "NotificationType", "popup" },
                    { "AutoDismiss", domainEvent.Priority <= SMS_Domain.Enums.HazardPriority.Medium }
                });

            var uiResult = await _eventBus.PublishUIEventAsync(uiNotificationEvent, EventExecutionMode.Manual, cancellationToken);

            if (uiResult.IsFailure)
            {
                _logger.LogApplicationWarning("[UI EVENT] Failed to queue popup notification: {Error}", uiResult.Error.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogApplicationWarning(ex, "[UI EVENT] Failed to publish UI notification for hazard {HazardCode}", domainEvent.HazardCode);
        }
    }

    private async Task PublishEmailNotification(SMS_Domain.Events.HazardCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        try
        {
            var recipients = GetEmailRecipientsByPriority(domainEvent.Priority);
            var emailPriority = GetEmailPriorityFromHazardPriority(domainEvent.Priority);

            var emailEvent = new EmailNotificationEvent(
                toRecipients: recipients,
                subject: $"{domainEvent.Priority} Priority Hazard: {domainEvent.HazardCode} - {domainEvent.HazardType}",
                body: CreateHazardNotificationEmailBody(domainEvent),
                isHtmlContent: true,
                priority: emailPriority,
                deliveryMode: IntegrationDeliveryMode.BestEffort,
                reportId: HandlerHelpers.ResolveReportId(domainEvent),
                workflowType: "HazardNotification",
                relatedEntityType: "Hazard",
                relatedEntityId: domainEvent.HazardCode,
                templateName: "SMS_HazardCreated_Template",
                templateData: new Dictionary<string, object>
                {
                    { "HazardCode", domainEvent.HazardCode },
                    { "HazardType", domainEvent.HazardType },
                    { "Priority", domainEvent.Priority.ToString() },
                    { "Description", domainEvent.Description },
                    { "Location", domainEvent.LocationArea },
                    { "CreatedBy", domainEvent.CreatedBy },
                    { "CreatedDate", domainEvent.CreatedDate.ToString("yyyy-MM-dd HH:mm") }
                },
                emailMetadata: new Dictionary<string, object>
                {
                    { "Source", "HazardCreatedEventHandler" },
                    { "HazardId", domainEvent.HazardId },
                    { "ReportCode", domainEvent.ReportCode },
                    { "EventDemonstration", true }
                });

            var emailResult = await _eventBus.PublishIntegrationEventAsync(emailEvent, EventExecutionMode.Manual, cancellationToken);

            if (emailResult.IsFailure)
            {
                _logger.LogApplicationError("[INTEGRATION EVENT] Failed to queue hazard notification email: {Error}", emailResult.Error.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "[INTEGRATION EVENT] Failed to send email notifications for {HazardCode}", domainEvent.HazardCode);
        }
    }

    

    private static string BuildNotificationMessage(SMS_Domain.Events.HazardCreatedEvent domainEvent)
    {
        var baseMessage = $"New {domainEvent.Priority} priority hazard: {domainEvent.HazardCode}";
        var reportId = HandlerHelpers.ResolveReportId(domainEvent);
        return string.IsNullOrWhiteSpace(reportId)
            ? baseMessage
            : $"{baseMessage} (Report: {reportId})";
    }

    private UIEventPriority GetUINotificationPriority(SMS_Domain.Enums.HazardPriority hazardPriority)
    {
        return hazardPriority switch
        {
            SMS_Domain.Enums.HazardPriority.Critical => UIEventPriority.Critical,
            SMS_Domain.Enums.HazardPriority.High => UIEventPriority.High,
            SMS_Domain.Enums.HazardPriority.Medium => UIEventPriority.Normal,
            _ => UIEventPriority.Low
        };
    }

    private List<string> GetEmailRecipientsByPriority(SMS_Domain.Enums.HazardPriority priority)
    {
        return priority switch
        {
            SMS_Domain.Enums.HazardPriority.Critical => new List<string>
            {
                "safety.manager@pdxairport.com",
                "operations.director@pdxairport.com",
                "sms.coordinator@pdxairport.com"
            },
            SMS_Domain.Enums.HazardPriority.High => new List<string>
            {
                "safety.manager@pdxairport.com",
                "sms.coordinator@pdxairport.com"
            },
            _ => new List<string> { "safety.team@pdxairport.com" }
        };
    }

    private EmailPriority GetEmailPriorityFromHazardPriority(SMS_Domain.Enums.HazardPriority hazardPriority)
    {
        return hazardPriority switch
        {
            SMS_Domain.Enums.HazardPriority.Critical => EmailPriority.Urgent,
            SMS_Domain.Enums.HazardPriority.High => EmailPriority.High,
            SMS_Domain.Enums.HazardPriority.Medium => EmailPriority.Normal,
            _ => EmailPriority.Low
        };
    }

    private string CreateHazardNotificationEmailBody(SMS_Domain.Events.HazardCreatedEvent domainEvent)
    {
        var priorityColor = GetPriorityColor(domainEvent.Priority);

        return $@"
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; background-color: #f9f9f9; }}
        .container {{ background-color: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        .header {{ background-color: {priorityColor}; color: white; padding: 15px; border-radius: 5px; margin-bottom: 20px; }}
        .details {{ background-color: #f8f9fa; padding: 15px; border-left: 4px solid {priorityColor}; margin: 15px 0; }}
        .footer {{ color: #666; font-size: 12px; margin-top: 30px; border-top: 1px solid #eee; padding-top: 15px; }}
        .priority {{ color: {priorityColor}; font-weight: bold; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>?? New Hazard Report: <span class='priority'>{domainEvent.Priority}</span> Priority</h2>
        </div>

        <p>A new hazard has been reported in the SMS system and requires attention.</p>

        <div class='details'>
            <h3>Hazard Information:</h3>
            <ul>
                <li><strong>Hazard Code:</strong> {domainEvent.HazardCode}</li>
                <li><strong>Type:</strong> {domainEvent.HazardType}</li>
                <li><strong>Category:</strong> {domainEvent.HazardCategory}</li>
                <li><strong>Priority:</strong> <span class='priority'>{domainEvent.Priority}</span></li>
                <li><strong>Location:</strong> {domainEvent.LocationArea}</li>
                <li><strong>Reported By:</strong> {domainEvent.CreatedBy}</li>
                <li><strong>Date:</strong> {domainEvent.CreatedDate:yyyy-MM-dd HH:mm} UTC</li>
                <li><strong>Report Code:</strong> {domainEvent.ReportCode}</li>
            </ul>

            <h4>Description:</h4>
            <p>{domainEvent.Description}</p>
        </div>

        <p>Please access the SMS system to review this hazard and take appropriate action.</p>

        <div class='footer'>
            <p><strong>SMS Safety Management System</strong> - Automated Notification<br/>
            Event ID: {domainEvent.EventId} | Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GetPriorityColor(SMS_Domain.Enums.HazardPriority priority)
    {
        return priority switch
        {
            SMS_Domain.Enums.HazardPriority.Critical => "#dc3545",
            SMS_Domain.Enums.HazardPriority.High => "#fd7e14",
            SMS_Domain.Enums.HazardPriority.Medium => "#ffc107",
            _ => "#28a745"
        };
    }
}
/// <summary>
/// Handles hazard update notifications for domain logging and UI feedback.
/// </summary>
public sealed class HazardUpdatedEventHandler : BaseDomainEventHandler<HazardUpdatedEvent>
{
    public HazardUpdatedEventHandler(ILogger<HazardUpdatedEventHandler> logger, IBaseEventBus eventBus) : base(logger, eventBus) { }

    protected override Task<Result> HandleDomainEventAsync(HazardUpdatedEvent domainEvent, CancellationToken cancellationToken)
    {
        Logger.LogApplicationInformation("[DOMAIN EVENT] Hazard updated. HazardId: {HazardId}, ReportId: {ReportId}",domainEvent.HazardId,HandlerHelpers.ResolveReportId(domainEvent));

        return Task.FromResult(Result.Success());
    }
}

/// <summary>
/// Reserved handler for hazard deletion events.
/// Intentionally minimal while deletion side-effects are not required.
/// </summary>
public sealed class HazardDeletedEventHandler : BaseDomainEventHandler<HazardDeletedEvent>
{
    public HazardDeletedEventHandler(ILogger<HazardDeletedEventHandler> logger, IBaseEventBus eventBus) : base(logger, eventBus) { }
}

/// <summary>
/// Handles hazard status transitions and triggers SPI closure-time automation when status moves to closed.
/// </summary>
public sealed class HazardStatusChangedEventHandler : BaseDomainEventHandler<HazardStatusChangedEvent>
{
    private readonly ISPIAutomationService _spiAutomationService;
    private readonly ILogger<HazardStatusChangedEventHandler> _logger;

    public HazardStatusChangedEventHandler(
        ILogger<HazardStatusChangedEventHandler> logger,
        IBaseEventBus eventBus,
        ISPIAutomationService spiAutomationService) : base(logger, eventBus)
    {
        _logger = logger;
        _spiAutomationService = spiAutomationService ?? throw new ArgumentNullException(nameof(spiAutomationService));
    }

    protected override async Task<Result> HandleDomainEventAsync(HazardStatusChangedEvent domainEvent, CancellationToken cancellationToken)
    {
        var newStatusValue = domainEvent.NewStatus?.Value ?? string.Empty;
        var newStatusName = domainEvent.NewStatus?.Name ?? string.Empty;

        var isClosedStatus =
            newStatusValue.Contains("CLOSED", StringComparison.OrdinalIgnoreCase) ||
            newStatusName.Contains("Closed", StringComparison.OrdinalIgnoreCase);

        if (!isClosedStatus)
        {
            _logger.LogApplicationInformation(
                "[SPI AUTOMATION] Hazard status change for {HazardCode} does not indicate closure. Status: {Status}",
                domainEvent.HazardCode,
                string.IsNullOrWhiteSpace(newStatusName) ? newStatusValue : newStatusName);

            return Result.Success();
        }

        var closureDate = domainEvent.StatusChangeDate == default ? DateTime.UtcNow : domainEvent.StatusChangeDate;
        var submittedDate = domainEvent.TimeInCurrentStatus.HasValue
            ? closureDate.Subtract(domainEvent.TimeInCurrentStatus.Value)
            : closureDate;

        var hazardIdentifier = !string.IsNullOrWhiteSpace(domainEvent.HazardId)
            ? domainEvent.HazardId
            : domainEvent.HazardCode;

        var result = await _spiAutomationService.UpdateHazardClosureTimeAsync(
            hazardIdentifier,
            submittedDate,
            closureDate,
            cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogApplicationWarning(
                "SPI automation failed for hazard status changed event {HazardCode}: {Error}",
                domainEvent.HazardCode,
                result.Error?.Message);

            return Result.Failure(result.Error ?? new Error("HAZARD_STATUS_SPI_FAILED", "Failed to update hazard closure SPI"));
        }

        return Result.Success();
    }
}

/// <summary>
/// Handles high-risk identification events and updates high-risk exposure SPI metrics.
/// </summary>
public sealed class HighRiskIdentifiedEventHandler : BaseDomainEventHandler<HighRiskIdentifiedEvent>
{
    private readonly ISPIAutomationService _spiAutomationService;
    private readonly ILogger<HighRiskIdentifiedEventHandler> _logger;

    public HighRiskIdentifiedEventHandler(
        ILogger<HighRiskIdentifiedEventHandler> logger,
        IBaseEventBus eventBus,
        ISPIAutomationService spiAutomationService) : base(logger, eventBus)
    {
        _logger = logger;
        _spiAutomationService = spiAutomationService ?? throw new ArgumentNullException(nameof(spiAutomationService));
    }

    protected override async Task<Result> HandleDomainEventAsync(HighRiskIdentifiedEvent domainEvent, CancellationToken cancellationToken)
    {
        var result = await _spiAutomationService.UpdateHighRiskExposureAsync(domainEvent.RiskLevel.Value, domainEvent.IdentifiedDate, cancellationToken);
        if (result.IsFailure)
        {
            _logger.LogApplicationWarning("SPI automation failed for high risk event {AssessmentId}: {Error}", domainEvent.AssessmentId, result.Error?.Message);
            return Result.Failure(result.Error ?? new Error("HIGH_RISK_SPI_FAILED", "Failed to update high risk exposure SPI"));
        }

        return Result.Success();
    }
}

/// <summary>
/// Reserved handler for mitigation approval confirmations.
/// </summary>
public sealed class MitigationApprovalApprovedEventHandler : BaseDomainEventHandler<MitigationApprovalApprovedEvent>
{
    public MitigationApprovalApprovedEventHandler(ILogger<MitigationApprovalApprovedEventHandler> logger, IBaseEventBus eventBus) : base(logger, eventBus) { }
}

/// <summary>
/// Reserved handler for mitigation approval requests.
/// </summary>
public sealed class MitigationApprovalRequestedEventHandler : BaseDomainEventHandler<MitigationApprovalRequestedEvent>
{
    private readonly ILogger<MitigationApprovalRequestedEventHandler> _logger;
    private readonly IBaseMediator _mediator;

    public MitigationApprovalRequestedEventHandler(ILogger<MitigationApprovalRequestedEventHandler> logger, IBaseEventBus eventBus, IBaseMediator mediator)
        : base(logger, eventBus)
    {
        _logger = logger;
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    protected override Task<Result> HandleDomainEventAsync(MitigationApprovalRequestedEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogApplicationInformation(
            "[MITIGATION APPROVAL] Approval requested for mitigation {MitigationCode} (Priority: {Priority}, Deadline: {Deadline})",
            domainEvent.MitigationCode,
            domainEvent.Priority,
            domainEvent.ApprovalDeadline);

        return Task.FromResult(Result.Success());
    }

    protected override async Task<Result> HandleIntegrationEventAsync(MitigationApprovalRequestedEvent domainEvent, CancellationToken cancellationToken)
    {
        var recipients = (domainEvent.RequiredApprovers ?? new List<string>())
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (recipients.Count == 0)
        {
            _logger.LogApplicationWarning(
                "[MITIGATION APPROVAL] No approver recipients resolved for mitigation {MitigationCode}. Email notification will not be sent.",
                domainEvent.MitigationCode);
            return Result.Failure(new Error("MITIGATION_APPROVAL_NO_RECIPIENTS", "No approver recipients resolved for mitigation approval notification."));
        }

        var emailEvent = new EmailNotificationEvent(
            toRecipients: recipients,
            subject: $"Mitigation Approval Required: {domainEvent.MitigationCode}",
            body: await BuildMitigationApprovalRequestedEmailHtmlAsync(domainEvent, cancellationToken),
            isHtmlContent: true,
            priority: EmailPriority.High,
            deliveryMode: IntegrationDeliveryMode.BestEffort,
            reportId: domainEvent.ReportId,
            workflowType: "MitigationApproval",
            relatedEntityType: "Mitigation",
            relatedEntityId: domainEvent.MitigationId,
            emailMetadata: new Dictionary<string, object>
            {
                { "MitigationCode", domainEvent.MitigationCode },
                { "Priority", domainEvent.Priority.ToString() },
                { "RequestedBy", domainEvent.RequestedBy }
            });

        return await EventBus.PublishIntegrationEventAsync(emailEvent, EventExecutionMode.Queued, cancellationToken);
    }

    private async Task<string> BuildMitigationApprovalRequestedEmailHtmlAsync(MitigationApprovalRequestedEvent domainEvent, CancellationToken cancellationToken)
    {
        Hazard? hazard = null;
        Report? report = null;
        ReportValidation? reportValidation = null;
        HazardLocation? latestLocation = null;
        RiskAssessment? currentRiskAssessment = null;
        Mitigation? currentMitigation = null;
        HazardReportTracking? tracking = null;

        if (!string.IsNullOrWhiteSpace(domainEvent.HazardCode))
        {
            var hazardResult = await _mediator.SendAsync(new GetHazardByCodeQuery(new HazardID(domainEvent.HazardCode)), cancellationToken);
            if (hazardResult.IsSuccess)
            {
                hazard = hazardResult.Value;
            }

            var trackingResult = await _mediator.SendAsync(new GetHazardReportTrackingByHazardCodeQuery(domainEvent.HazardCode), cancellationToken);
            if (trackingResult.IsSuccess && trackingResult.Value?.Any() == true)
            {
                tracking = trackingResult.Value
                    .OrderByDescending(t => t.UpdatedDate ?? t.CreatedDate)
                    .FirstOrDefault();
            }

            var locationResult = await _mediator.SendAsync(new GetHazardLocationsByHazardCodeQuery(domainEvent.HazardCode), cancellationToken);
            if (locationResult.IsSuccess && locationResult.Value?.Any() == true)
            {
                latestLocation = locationResult.Value
                    .OrderByDescending(l => l.UpdatedDate ?? l.CreatedDate ?? DateTime.MinValue)
                    .ThenByDescending(l => l.DateSelected)
                    .FirstOrDefault();
            }

            var assessmentsResult = await _mediator.SendAsync(new GetRiskAssessmentsByHazardCodeQuery(new HazardID(domainEvent.HazardCode)), cancellationToken);
            if (assessmentsResult.IsSuccess && assessmentsResult.Value?.Any() == true)
            {
                currentRiskAssessment = assessmentsResult.Value.FirstOrDefault(ra => ra.RiskAssessmentCategory == RiskAssessmentCategory.Technical)
                                     ?? assessmentsResult.Value.FirstOrDefault();
            }

            var mitigationsResult = await _mediator.SendAsync(new GetMitigationsByHazardCodeQuery(domainEvent.HazardCode), cancellationToken);
            if (mitigationsResult.IsSuccess && mitigationsResult.Value?.Any() == true)
            {
                currentMitigation = mitigationsResult.Value.FirstOrDefault(m => string.Equals(m.Code, domainEvent.MitigationCode, StringComparison.OrdinalIgnoreCase))
                                 ?? mitigationsResult.Value.FirstOrDefault();
            }
        }

        var reportCode = hazard?.ReportCode;
        if (!string.IsNullOrWhiteSpace(reportCode))
        {
            var reportResult = await _mediator.SendAsync(new GetReportByCodeQuery(new ReportID(reportCode)), cancellationToken);
            if (reportResult.IsSuccess)
            {
                report = reportResult.Value;
            }

            var validationResult = await _mediator.SendAsync(new GetReportValidationByReportIdQuery(new ReportID(reportCode)), cancellationToken);
            if (validationResult.IsSuccess)
            {
                reportValidation = validationResult.Value;
            }
        }

        var summaryFields = new List<SMSEmailField>
        {
            new() { Label = "Mitigation Code", Value = domainEvent.MitigationCode },
            new() { Label = "Mitigation ID", Value = domainEvent.MitigationId },
            new() { Label = "Hazard ID", Value = domainEvent.HazardCode },
            new() { Label = "Priority", Value = domainEvent.Priority.ToString() },
            new() { Label = "Requested By", Value = domainEvent.RequestedBy },
            new() { Label = "Requested Date", Value = domainEvent.RequestDate.ToString("MMMM dd, yyyy h:mm tt") },
            new() { Label = "Approval Deadline", Value = domainEvent.ApprovalDeadline.ToString("MMMM dd, yyyy h:mm tt") }
        };

        if (!string.IsNullOrWhiteSpace(tracking?.TrackingCode))
        {
            summaryFields.Add(new SMSEmailField { Label = "Tracking ID", Value = tracking.TrackingCode });
        }

        var sections = new List<SMSEmailSection>
        {
            new()
            {
                Title = "Mitigation Details",
                Fields =
                [
                    new SMSEmailField { Label = "Description", Value = domainEvent.MitigationDescription ?? string.Empty, IsFullWidth = true }
                ]
            },
            new()
            {
                Title = "Hazard Context",
                Fields =
                [
                    new SMSEmailField { Label = "Hazard Category", Value = hazard?.HazardCategory ?? string.Empty },
                    new SMSEmailField { Label = "Hazard Type", Value = hazard?.HazardType ?? string.Empty },
                    new SMSEmailField { Label = "Hazard Risk Level", Value = hazard?.HazardRiskLevel?.Name ?? hazard?.HazardRiskLevel?.Value ?? "Unknown" },
                    new SMSEmailField { Label = "Initial Average Score", Value = hazard?.InitialAverageScore?.ToString("0.##") ?? "N/A" },
                    new SMSEmailField { Label = "Residual Average Score", Value = hazard?.ResidualAverageScore?.ToString("0.##") ?? "N/A" },
                    new SMSEmailField { Label = "Hazard Description", Value = hazard?.Description ?? string.Empty, IsFullWidth = true }
                ]
            },
            new()
            {
                Title = "Report Status Context",
                Fields =
                [
                    new SMSEmailField { Label = "Report ID", Value = reportCode ?? string.Empty },
                    new SMSEmailField { Label = "Validation Decision", Value = reportValidation?.ValidationDecision ?? string.Empty },
                    new SMSEmailField { Label = "Validation Date", Value = reportValidation?.CreatedDate?.ToString("MMMM dd, yyyy h:mm tt") ?? string.Empty },
                    new SMSEmailField { Label = "Location", Value = latestLocation?.Description ?? string.Empty },
                    new SMSEmailField { Label = "Location Validation", Value = latestLocation is null ? "No mapped location" : (latestLocation.IsValidated ? "Validated" : "Validation Required") },
                    new SMSEmailField { Label = "Risk Assessment", Value = currentRiskAssessment?.Code ?? string.Empty },
                    new SMSEmailField { Label = "Risk Assessment Status", Value = currentRiskAssessment?.Status ?? string.Empty },
                    new SMSEmailField { Label = "Risk Assessment Stage", Value = currentRiskAssessment?.Stage ?? string.Empty },
                    new SMSEmailField { Label = "Mitigation Status", Value = currentMitigation?.Status ?? string.Empty },
                    new SMSEmailField { Label = "Mitigation Progress", Value = currentMitigation?.Progress is null ? string.Empty : $"{currentMitigation.Progress}%" },
                    new SMSEmailField { Label = "Contact Email", Value = report?.ReportContactEmail ?? string.Empty }
                ]
            }
        };

        return SMSEmailTemplateBuilder.BuildStandardEmail(
            title: "Mitigation Approval Request",
            introHtml: "A mitigation approval request has been submitted and requires your review.",
            summaryFields: summaryFields,
            sections: sections,
            footerHtml: "Please review this mitigation in SMS and take the appropriate approval action.",
            logoUrl: SMSEmailTemplateBuilder.DefaultLogoUrl);
    }

    protected override async Task<Result> HandleUIEventAsync(MitigationApprovalRequestedEvent domainEvent, CancellationToken cancellationToken)
    {
        var uiEvent = new UINotificationEvent(
            severity: UINotificationSeverity.Warning,
            title: "Mitigation Approval Requested",
            message: $"Mitigation {domainEvent.MitigationCode} is awaiting approval.",
            duration: 7000,
            category: "Mitigation",
            sourceLayer: nameof(MitigationApprovalRequestedEventHandler),
            reportId: domainEvent.ReportId,
            targetComponent: "NotificationCenter",
            priority: UIEventPriority.High,
            metadata: new Dictionary<string, object>
            {
                { "MitigationId", domainEvent.MitigationId },
                { "MitigationCode", domainEvent.MitigationCode },
                { "RequestedBy", domainEvent.RequestedBy }
            });

        return await EventBus.PublishUIEventAsync(uiEvent, EventExecutionMode.Manual, cancellationToken);
    }
}

/// <summary>
/// Handles mitigation completion and updates mitigation implementation SPI metrics.
/// </summary>
public sealed class MitigationCompletedEventHandler : BaseDomainEventHandler<MitigationCompletedEvent>
{
    private readonly ISPIAutomationService _spiAutomationService;
    private readonly ILogger<MitigationCompletedEventHandler> _logger;

    public MitigationCompletedEventHandler(
        ILogger<MitigationCompletedEventHandler> logger,
        IBaseEventBus eventBus,
        ISPIAutomationService spiAutomationService) : base(logger, eventBus)
    {
        _logger = logger;
        _spiAutomationService = spiAutomationService ?? throw new ArgumentNullException(nameof(spiAutomationService));
    }

    protected override async Task<Result> HandleDomainEventAsync(MitigationCompletedEvent domainEvent, CancellationToken cancellationToken)
    {
        var result = await _spiAutomationService.UpdateMitigationImplementationRateAsync(
            domainEvent.MitigationId,
            domainEvent.TargetCompletionDate,
            domainEvent.CompletedDate,
            cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogApplicationWarning("SPI automation failed for mitigation completed event {MitigationId}: {Error}", domainEvent.MitigationId, result.Error?.Message);
            return Result.Failure(result.Error ?? new Error("MITIGATION_SPI_FAILED", "Failed to update mitigation implementation SPI"));
        }

        return Result.Success();
    }
}

/// <summary>
/// Reserved handler for mitigation creation events.
/// </summary>
public sealed class MitigationCreatedEventHandler : BaseDomainEventHandler<MitigationCreatedEvent>
{
    public MitigationCreatedEventHandler(ILogger<MitigationCreatedEventHandler> logger, IBaseEventBus eventBus) : base(logger, eventBus) { }
}

/// <summary>
/// Handles mitigation overdue events and updates corrective-action closure SPI metrics.
/// </summary>
public sealed class MitigationOverdueEventHandler : BaseDomainEventHandler<MitigationOverdueEvent>
{
    private readonly ISPIAutomationService _spiAutomationService;
    private readonly ILogger<MitigationOverdueEventHandler> _logger;

    public MitigationOverdueEventHandler(
        ILogger<MitigationOverdueEventHandler> logger,
        IBaseEventBus eventBus,
        ISPIAutomationService spiAutomationService) : base(logger, eventBus)
    {
        _logger = logger;
        _spiAutomationService = spiAutomationService ?? throw new ArgumentNullException(nameof(spiAutomationService));
    }

    protected override async Task<Result> HandleDomainEventAsync(MitigationOverdueEvent domainEvent, CancellationToken cancellationToken)
    {
        var calculationDate = domainEvent.DueDate == default ? DateTime.UtcNow : domainEvent.DueDate;

        var result = await _spiAutomationService.UpdateCorrectiveActionClosureAsync(calculationDate, cancellationToken);
        if (result.IsFailure)
        {
            _logger.LogApplicationWarning(
                "SPI automation failed for mitigation overdue event {MitigationId}: {Error}",
                domainEvent.MitigationId,
                result.Error?.Message);

            return Result.Failure(result.Error ?? new Error("MITIGATION_OVERDUE_SPI_FAILED", "Failed to update corrective action closure SPI"));
        }

        return Result.Success();
    }
}

/// <summary>
/// Reserved handler for mitigation status transitions.
/// Transition publication is centralized in application services.
/// </summary>
public sealed class MitigationStatusChangedEventHandler : BaseDomainEventHandler<MitigationStatusChangedEvent>
{
    private readonly IBaseMediator _mediator;
    private readonly ISPIAutomationService _spiAutomationService;
    private readonly ILogger<MitigationStatusChangedEventHandler> _logger;

    public MitigationStatusChangedEventHandler(
        ILogger<MitigationStatusChangedEventHandler> logger,
        IBaseEventBus eventBus,
        IBaseMediator mediator,
        ISPIAutomationService spiAutomationService) : base(logger, eventBus)
    {
        _logger = logger;
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _spiAutomationService = spiAutomationService ?? throw new ArgumentNullException(nameof(spiAutomationService));
    }

    protected override async Task<Result> HandleDomainEventAsync(MitigationStatusChangedEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogApplicationInformation(
            "[MITIGATION STATUS] Mitigation {MitigationId} changed to {Status} by {ChangedBy}",
            domainEvent.MitigationId,
            domainEvent.Status,
            domainEvent.ChangedBy);

        if (string.Equals(domainEvent.Status, SMS_Domain.Enums.MitigationStatus.PastExpectedTargetDate.Value, StringComparison.OrdinalIgnoreCase))
        {
            var overdueResult = await _spiAutomationService.UpdateCorrectiveActionClosureAsync(domainEvent.ChangedDate, cancellationToken);
            if (overdueResult.IsFailure)
            {
                _logger.LogApplicationWarning("Failed to update corrective action closure SPI for mitigation {MitigationId}: {Error}", domainEvent.MitigationId, overdueResult.Error?.Message);
                return Result.Failure(overdueResult.Error ?? new Error("MITIGATION_OVERDUE_SPI_FAILED", "Failed to update corrective action closure SPI"));
            }
        }

        return Result.Success();
    }

    protected override async Task<Result> HandleIntegrationEventAsync(MitigationStatusChangedEvent domainEvent, CancellationToken cancellationToken)
    {
        if (!string.Equals(domainEvent.Status, SMS_Domain.Enums.MitigationStatus.Approved.Value, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Success();
        }

        var mitigationResult = await _mediator.SendAsync(new GetMitigationByCodeQuery(new MitigationID(domainEvent.MitigationId)), cancellationToken);
        if (mitigationResult.IsFailure || mitigationResult.Value is null)
        {
            _logger.LogApplicationWarning(
                "Unable to resolve mitigation {MitigationId} for approved notification email.",
                domainEvent.MitigationId);
            return Result.Success();
        }

        var mitigation = mitigationResult.Value;
        var recipient = mitigation.AssignedTo?.Trim();

        if (string.IsNullOrWhiteSpace(recipient))
        {
            _logger.LogApplicationInformation(
                "Skipping approved notification email for mitigation {MitigationId} because AssignedTo is empty.",
                domainEvent.MitigationId);
            return Result.Success();
        }

        if (!recipient.Contains('@'))
        {
            recipient = $"{recipient}@flypdx.com";
        }

        var emailEvent = new EmailNotificationEvent(
            toRecipients: new List<string> { recipient },
            subject: $"Mitigation Approved: {mitigation.Code}",
            body: await BuildMitigationApprovedEmailHtmlAsync(mitigation, domainEvent, cancellationToken),
            isHtmlContent: true,
            priority: EmailPriority.Normal,
            deliveryMode: IntegrationDeliveryMode.BestEffort,
            reportId: domainEvent.ReportId,
            workflowType: "MitigationApproved",
            relatedEntityType: "Mitigation",
            relatedEntityId: mitigation.Code,
            emailMetadata: new Dictionary<string, object>
            {
                { "MitigationId", domainEvent.MitigationId },
                { "Status", domainEvent.Status },
                { "ChangedBy", domainEvent.ChangedBy },
                { "ChangedDate", domainEvent.ChangedDate }
            });

        return await EventBus.PublishIntegrationEventAsync(emailEvent, EventExecutionMode.Queued, cancellationToken);
    }

    private async Task<string> BuildMitigationApprovedEmailHtmlAsync(Mitigation mitigation, MitigationStatusChangedEvent domainEvent, CancellationToken cancellationToken)
    {
        Hazard? hazard = null;
        Report? report = null;
        ReportValidation? reportValidation = null;
        HazardLocation? latestLocation = null;
        RiskAssessment? currentRiskAssessment = null;
        HazardReportTracking? tracking = null;

        if (!string.IsNullOrWhiteSpace(mitigation.HazardCode))
        {
            var hazardResult = await _mediator.SendAsync(new GetHazardByCodeQuery(new HazardID(mitigation.HazardCode)), cancellationToken);
            if (hazardResult.IsSuccess)
            {
                hazard = hazardResult.Value;
            }

            var trackingResult = await _mediator.SendAsync(new GetHazardReportTrackingByHazardCodeQuery(mitigation.HazardCode), cancellationToken);
            if (trackingResult.IsSuccess && trackingResult.Value?.Any() == true)
            {
                tracking = trackingResult.Value
                    .OrderByDescending(t => t.UpdatedDate ?? t.CreatedDate)
                    .FirstOrDefault();
            }

            var locationResult = await _mediator.SendAsync(new GetHazardLocationsByHazardCodeQuery(mitigation.HazardCode), cancellationToken);
            if (locationResult.IsSuccess && locationResult.Value?.Any() == true)
            {
                latestLocation = locationResult.Value
                    .OrderByDescending(l => l.UpdatedDate ?? l.CreatedDate ?? DateTime.MinValue)
                    .ThenByDescending(l => l.DateSelected)
                    .FirstOrDefault();
            }

            var assessmentsResult = await _mediator.SendAsync(new GetRiskAssessmentsByHazardCodeQuery(new HazardID(mitigation.HazardCode)), cancellationToken);
            if (assessmentsResult.IsSuccess && assessmentsResult.Value?.Any() == true)
            {
                currentRiskAssessment = assessmentsResult.Value.FirstOrDefault(ra => ra.RiskAssessmentCategory == RiskAssessmentCategory.Technical)
                                     ?? assessmentsResult.Value.FirstOrDefault();
            }
        }

        var reportCode = hazard?.ReportCode;
        if (!string.IsNullOrWhiteSpace(reportCode))
        {
            var reportResult = await _mediator.SendAsync(new GetReportByCodeQuery(new ReportID(reportCode)), cancellationToken);
            if (reportResult.IsSuccess)
            {
                report = reportResult.Value;
            }

            var validationResult = await _mediator.SendAsync(new GetReportValidationByReportIdQuery(new ReportID(reportCode)), cancellationToken);
            if (validationResult.IsSuccess)
            {
                reportValidation = validationResult.Value;
            }
        }

        var summaryFields = new List<SMSEmailField>
        {
            new() { Label = "Mitigation Code", Value = mitigation.Code },
            new() { Label = "Mitigation ID", Value = domainEvent.MitigationId },
            new() { Label = "Hazard ID", Value = mitigation.HazardCode },
            new() { Label = "Status", Value = domainEvent.Status },
            new() { Label = "Approved/Updated By", Value = domainEvent.ChangedBy },
            new() { Label = "Date", Value = domainEvent.ChangedDate.ToString("MMMM dd, yyyy h:mm tt") },
            new() { Label = "Target Completion", Value = mitigation.TargetDate?.ToString("MMMM dd, yyyy") ?? "Planned target date" }
        };

        if (!string.IsNullOrWhiteSpace(tracking?.TrackingCode))
        {
            summaryFields.Add(new SMSEmailField { Label = "Tracking ID", Value = tracking.TrackingCode });
        }

        var sections = new List<SMSEmailSection>
        {
            new()
            {
                Title = "Hazard Context",
                Fields =
                [
                    new SMSEmailField { Label = "Hazard Category", Value = hazard?.HazardCategory ?? string.Empty },
                    new SMSEmailField { Label = "Hazard Type", Value = hazard?.HazardType ?? string.Empty },
                    new SMSEmailField { Label = "Hazard Risk Level", Value = hazard?.HazardRiskLevel?.Name ?? hazard?.HazardRiskLevel?.Value ?? "Unknown" },
                    new SMSEmailField { Label = "Initial Average Score", Value = hazard?.InitialAverageScore?.ToString("0.##") ?? "N/A" },
                    new SMSEmailField { Label = "Residual Average Score", Value = hazard?.ResidualAverageScore?.ToString("0.##") ?? "N/A" },
                    new SMSEmailField { Label = "Hazard Description", Value = hazard?.Description ?? string.Empty, IsFullWidth = true }
                ]
            },
            new()
            {
                Title = "Report Status Context",
                Fields =
                [
                    new SMSEmailField { Label = "Report ID", Value = reportCode ?? string.Empty },
                    new SMSEmailField { Label = "Validation Decision", Value = reportValidation?.ValidationDecision ?? string.Empty },
                    new SMSEmailField { Label = "Validation Date", Value = reportValidation?.CreatedDate?.ToString("MMMM dd, yyyy h:mm tt") ?? string.Empty },
                    new SMSEmailField { Label = "Location", Value = latestLocation?.Description ?? string.Empty },
                    new SMSEmailField { Label = "Location Validation", Value = latestLocation is null ? "No mapped location" : (latestLocation.IsValidated ? "Validated" : "Validation Required") },
                    new SMSEmailField { Label = "Risk Assessment", Value = currentRiskAssessment?.Code ?? string.Empty },
                    new SMSEmailField { Label = "Risk Assessment Status", Value = currentRiskAssessment?.Status ?? string.Empty },
                    new SMSEmailField { Label = "Risk Assessment Stage", Value = currentRiskAssessment?.Stage ?? string.Empty },
                    new SMSEmailField { Label = "Mitigation Progress", Value = $"{mitigation.Progress}%" },
                    new SMSEmailField { Label = "Contact Email", Value = report?.ReportContactEmail ?? string.Empty }
                ]
            }
        };

        return SMSEmailTemplateBuilder.BuildStandardEmail(
            title: "Mitigation Approved",
            introHtml: "Your mitigation has been approved and is ready for implementation tracking.",
            summaryFields: summaryFields,
            sections: sections,
            footerHtml: "Please proceed with mitigation implementation and status updates in SMS.",
            logoUrl: SMSEmailTemplateBuilder.DefaultLogoUrl);
    }

    protected override async Task<Result> HandleUIEventAsync(MitigationStatusChangedEvent domainEvent, CancellationToken cancellationToken)
    {
        var severity = string.Equals(domainEvent.Status, SMS_Domain.Enums.MitigationStatus.PastExpectedTargetDate.Value, StringComparison.OrdinalIgnoreCase)
            ? UINotificationSeverity.Warning
            : UINotificationSeverity.Info;

        var priority = severity == UINotificationSeverity.Warning ? UIEventPriority.High : UIEventPriority.Normal;

        var reportId = string.Empty;
        var mitigationResult = await _mediator.SendAsync(new GetMitigationByCodeQuery(new MitigationID(domainEvent.MitigationId)), cancellationToken);
        if (mitigationResult.IsSuccess && mitigationResult.Value is not null && !string.IsNullOrWhiteSpace(mitigationResult.Value.HazardCode))
        {
            var hazardResult = await _mediator.SendAsync(new GetHazardByCodeQuery(new HazardID(mitigationResult.Value.HazardCode)), cancellationToken);
            if (hazardResult.IsSuccess)
            {
                reportId = hazardResult.Value?.ReportCode ?? string.Empty;
            }
        }

        var uiEvent = new UINotificationEvent(
            severity: severity,
            title: "Mitigation Status Updated",
            message: $"Mitigation {domainEvent.MitigationId} moved to {domainEvent.Status}.",
            duration: 5000,
            category: "Mitigation",
            sourceLayer: nameof(MitigationStatusChangedEventHandler),
            reportId: reportId,
            targetComponent: "NotificationCenter",
            priority: priority,
            metadata: new Dictionary<string, object>
            {
                { "MitigationId", domainEvent.MitigationId },
                { "Status", domainEvent.Status },
                { "ChangedBy", domainEvent.ChangedBy },
                { "ChangedDate", domainEvent.ChangedDate }
            });

        return await EventBus.PublishUIEventAsync(uiEvent, EventExecutionMode.Manual, cancellationToken);
    }
}

/// <summary>
/// Handles report closure logging and UI notification.
/// </summary>
public sealed class ReportClosedEventHandler : BaseDomainEventHandler<ReportClosedEvent>
{
    private readonly ILogger<ReportClosedEventHandler> _logger;

    public ReportClosedEventHandler(ILogger<ReportClosedEventHandler> logger, IBaseEventBus eventBus) : base(logger, eventBus)
    {
        _logger = logger;
    }

    protected override Task<Result> HandleDomainEventAsync(ReportClosedEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogApplicationInformation(
            "[REPORT EVENT] Report closed. ReportId: {ReportId}, ClosedBy: {ClosedBy}, ClosedDate: {ClosedDate}",
            domainEvent.ReportId,
            domainEvent.ClosedBy,
            domainEvent.ClosedDate);

        return Task.FromResult(Result.Success());
    }

    protected override async Task<Result> HandleUIEventAsync(ReportClosedEvent domainEvent, CancellationToken cancellationToken)
    {
        var uiEvent = new UINotificationEvent(
            severity: UINotificationSeverity.Success,
            title: "Report Closed",
            message: $"Report {domainEvent.ReportId} was closed.",
            duration: 5000,
            category: "Report",
            sourceLayer: nameof(ReportClosedEventHandler),
            reportId: domainEvent.ReportId,
            targetComponent: "NotificationCenter",
            priority: UIEventPriority.Low,
            metadata: new Dictionary<string, object>
            {
                { "ReportId", domainEvent.ReportId },
                { "ClosedBy", domainEvent.ClosedBy },
                { "ClosedDate", domainEvent.ClosedDate }
            });

        return await EventBus.PublishUIEventAsync(uiEvent, EventExecutionMode.Manual, cancellationToken);
    }
}

/// <summary>
/// Reserved handler for report creation events.
/// </summary>
public sealed class ReportCreatedEventHandler : BaseDomainEventHandler<ReportCreatedEvent>
{
    private readonly ILogger<ReportCreatedEventHandler> _logger;

    public ReportCreatedEventHandler(ILogger<ReportCreatedEventHandler> logger, IBaseEventBus eventBus) : base(logger, eventBus)
    {
        _logger = logger;
    }

    protected override Task<Result> HandleDomainEventAsync(ReportCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogApplicationInformation("[REPORT EVENT] Report created. ReportId: {ReportId}, CreatedBy: {CreatedBy}", domainEvent.ReportId, domainEvent.CreatedBy);
        return Task.FromResult(Result.Success());
    }

    protected override async Task<Result> HandleIntegrationEventAsync(ReportCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        var emailEvent = new EmailNotificationEvent(
            toRecipients: new List<string> { "safety.team@pdxairport.com" },
            subject: $"New Report Created: {domainEvent.ReportId}",
            body: $"Report {domainEvent.ReportId} was created by {domainEvent.CreatedBy} on {domainEvent.CreatedDate:yyyy-MM-dd HH:mm} UTC.",
            isHtmlContent: false,
            priority: EmailPriority.Normal,
            deliveryMode: IntegrationDeliveryMode.BestEffort,
            reportId: domainEvent.ReportId,
            workflowType: "ReportCreated",
            relatedEntityType: "Report",
            relatedEntityId: domainEvent.ReportId,
            emailMetadata: new Dictionary<string, object>
            {
                { "Source", nameof(ReportCreatedEventHandler) },
                { "CreatedBy", domainEvent.CreatedBy }
            });

        return await EventBus.PublishIntegrationEventAsync(emailEvent, EventExecutionMode.Queued, cancellationToken);
    }

    protected override async Task<Result> HandleUIEventAsync(ReportCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        var uiEvent = new UINotificationEvent(
            severity: UINotificationSeverity.Info,
            title: "Report Created",
            message: $"Report {domainEvent.ReportId} was created.",
            duration: 5000,
            category: "Report",
            sourceLayer: nameof(ReportCreatedEventHandler),
            reportId: domainEvent.ReportId,
            targetComponent: "NotificationCenter",
            priority: UIEventPriority.Normal,
            metadata: new Dictionary<string, object>
            {
                { "ReportId", domainEvent.ReportId },
                { "CreatedBy", domainEvent.CreatedBy },
                { "CreatedDate", domainEvent.CreatedDate }
            });

        return await EventBus.PublishUIEventAsync(uiEvent, EventExecutionMode.Manual, cancellationToken);
    }
}

/// <summary>
/// Handles report update logging and UI notification.
/// </summary>
public sealed class ReportUpdatedEventHandler : BaseDomainEventHandler<ReportUpdatedEvent>
{
    private readonly ILogger<ReportUpdatedEventHandler> _logger;

    public ReportUpdatedEventHandler(ILogger<ReportUpdatedEventHandler> logger, IBaseEventBus eventBus) : base(logger, eventBus)
    {
        _logger = logger;
    }

    protected override Task<Result> HandleDomainEventAsync(ReportUpdatedEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogApplicationInformation(
            "[REPORT EVENT] Report updated. ReportId: {ReportId}, UpdatedBy: {UpdatedBy}, UpdatedDate: {UpdatedDate}",
            domainEvent.ReportId,
            domainEvent.UpdatedBy,
            domainEvent.UpdatedDate);

        return Task.FromResult(Result.Success());
    }

    protected override async Task<Result> HandleUIEventAsync(ReportUpdatedEvent domainEvent, CancellationToken cancellationToken)
    {
        var uiEvent = new UINotificationEvent(
            severity: UINotificationSeverity.Info,
            title: "Report Updated",
            message: $"Report {domainEvent.ReportId} was updated.",
            duration: 4000,
            category: "Report",
            sourceLayer: nameof(ReportUpdatedEventHandler),
            reportId: domainEvent.ReportId,
            targetComponent: "NotificationCenter",
            priority: UIEventPriority.Normal,
            metadata: new Dictionary<string, object>
            {
                { "ReportId", domainEvent.ReportId },
                { "UpdatedBy", domainEvent.UpdatedBy },
                { "UpdatedDate", domainEvent.UpdatedDate }
            });

        return await EventBus.PublishUIEventAsync(uiEvent, EventExecutionMode.Manual, cancellationToken);
    }
}

/// <summary>
/// Handles risk assessment completion and updates completion SPI metrics.
/// </summary>
public sealed class RiskAssessmentCompletedEventHandler : BaseDomainEventHandler<RiskAssessmentCompletedEvent>
{
    private readonly ISPIAutomationService _spiAutomationService;
    private readonly ILogger<RiskAssessmentCompletedEventHandler> _logger;

    public RiskAssessmentCompletedEventHandler(
        ILogger<RiskAssessmentCompletedEventHandler> logger,
        IBaseEventBus eventBus,
        ISPIAutomationService spiAutomationService) : base(logger, eventBus)
    {
        _logger = logger;
        _spiAutomationService = spiAutomationService ?? throw new ArgumentNullException(nameof(spiAutomationService));
    }

    protected override async Task<Result> HandleDomainEventAsync(RiskAssessmentCompletedEvent domainEvent, CancellationToken cancellationToken)
    {
        var isOnTime = domainEvent.CompletedDate <= domainEvent.TargetCompletionDate;

        var result = await _spiAutomationService.UpdateRiskAssessmentCompletionAsync(
            domainEvent.AssessmentId,
            domainEvent.StartDate,
            domainEvent.CompletedDate,
            isOnTime,
            cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogApplicationWarning("SPI automation failed for risk assessment completion event {AssessmentId}: {Error}", domainEvent.AssessmentId, result.Error?.Message);
            return Result.Failure(result.Error ?? new Error("RISK_ASSESSMENT_SPI_FAILED", "Failed to update risk assessment completion SPI"));
        }

        return Result.Success();
    }
}

/// <summary>
/// Handles validation decisions and updates risk-identification effectiveness SPI metrics.
/// </summary>
public sealed class ValidationDecisionMadeEventHandler : BaseDomainEventHandler<ValidationDecisionMadeEvent>
{
    private readonly ISPIAutomationService _spiAutomationService;
    private readonly ILogger<ValidationDecisionMadeEventHandler> _logger;

    public ValidationDecisionMadeEventHandler(
        ILogger<ValidationDecisionMadeEventHandler> logger,
        IBaseEventBus eventBus,
        ISPIAutomationService spiAutomationService) : base(logger, eventBus)
    {
        _logger = logger;
        _spiAutomationService = spiAutomationService ?? throw new ArgumentNullException(nameof(spiAutomationService));
    }

    protected override async Task<Result> HandleDomainEventAsync(ValidationDecisionMadeEvent domainEvent, CancellationToken cancellationToken)
    {
        var result = await _spiAutomationService.UpdateRiskIdentificationEffectivenessAsync(
            domainEvent.ReportCode,
            domainEvent.ValidatedDate,
            domainEvent.ValidationDecision,
            cancellationToken);

            if (result.IsFailure)
        {
            _logger.LogApplicationWarning("SPI automation failed for validation decision event {ReportCode}: {Error}", domainEvent.ReportCode, result.Error?.Message);
            return Result.Failure(result.Error ?? new Error("VALIDATION_SPI_FAILED", "Failed to update validation SPI metrics"));
        }

        return Result.Success();
    }
}

/// <summary>
/// Reserved handler for risk-assessment creation events.
/// </summary>
public sealed class RiskAssessmentCreatedEventHandler : BaseDomainEventHandler<RiskAssessmentCreatedEvent>
{
    public RiskAssessmentCreatedEventHandler(ILogger<RiskAssessmentCreatedEventHandler> logger, IBaseEventBus eventBus) : base(logger, eventBus) { }
}

/// <summary>
/// Reserved handler for risk-assessment update events.
/// </summary>
public sealed class RiskAssessmentUpdatedEventHandler : BaseDomainEventHandler<RiskAssessmentUpdatedEvent>
{
    private readonly ILogger<RiskAssessmentUpdatedEventHandler> _logger;

    public RiskAssessmentUpdatedEventHandler(ILogger<RiskAssessmentUpdatedEventHandler> logger, IBaseEventBus eventBus) : base(logger, eventBus)
    {
        _logger = logger;
    }

    protected override Task<Result> HandleDomainEventAsync(RiskAssessmentUpdatedEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogApplicationInformation(
            "[RISK ASSESSMENT EVENT] Risk assessment updated. AssessmentId: {AssessmentId}, UpdatedBy: {UpdatedBy}, UpdatedDate: {UpdatedDate}",
            domainEvent.RiskAssessmentId,
            domainEvent.UpdatedBy,
            domainEvent.UpdatedDate);

        return Task.FromResult(Result.Success());
    }

    protected override async Task<Result> HandleUIEventAsync(RiskAssessmentUpdatedEvent domainEvent, CancellationToken cancellationToken)
    {
        var uiEvent = new UINotificationEvent(
            severity: UINotificationSeverity.Info,
            title: "Risk Assessment Updated",
            message: $"Risk assessment {domainEvent.RiskAssessmentId} was updated.",
            duration: 4000,
            category: "RiskAssessment",
            sourceLayer: nameof(RiskAssessmentUpdatedEventHandler),
            reportId: string.Empty,
            targetComponent: "NotificationCenter",
            priority: UIEventPriority.Normal,
            metadata: new Dictionary<string, object>
            {
                { "RiskAssessmentId", domainEvent.RiskAssessmentId },
                { "UpdatedBy", domainEvent.UpdatedBy },
                { "UpdatedDate", domainEvent.UpdatedDate }
            });

        return await EventBus.PublishUIEventAsync(uiEvent, EventExecutionMode.Manual, cancellationToken);
    }
}

/// <summary>
/// Handles SPI compliance transitions for logging and user-facing notifications.
/// </summary>
public sealed class SPIComplianceChangedEventHandler : BaseDomainEventHandler<SPIComplianceChangedEvent>
{
    private readonly ILogger<SPIComplianceChangedEventHandler> _logger;

    public SPIComplianceChangedEventHandler(ILogger<SPIComplianceChangedEventHandler> logger, IBaseEventBus eventBus) : base(logger, eventBus)
    {
        _logger = logger;
    }

    protected override Task<Result> HandleDomainEventAsync(SPIComplianceChangedEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogApplicationInformation(
            "[SPI COMPLIANCE] SPI {SPICode} compliance changed from {PreviousStatus} to {NewStatus}. Current: {CurrentValue}, Threshold: {Threshold}",
            domainEvent.SPICode,
            domainEvent.PreviousStatus,
            domainEvent.NewStatus,
            domainEvent.CurrentValue,
            domainEvent.ComplianceThreshold);

        if (domainEvent.NewStatus == SMS_Domain.Events.SPIComplianceStatus.NonCompliant || domainEvent.NewStatus == SMS_Domain.Events.SPIComplianceStatus.AtRisk)
        {
            _logger.LogApplicationWarning(
                "[SPI COMPLIANCE] SPI {SPICode} is {Status}. Regulatory reporting required: {RequiresRegulatoryReporting}",
                domainEvent.SPICode,
                domainEvent.NewStatus,
                domainEvent.RequiresRegulatoryReporting);
        }

        return Task.FromResult(Result.Success());
    }

    protected override async Task<Result> HandleUIEventAsync(SPIComplianceChangedEvent domainEvent, CancellationToken cancellationToken)
    {
        var (severity, priority, title) = domainEvent.NewStatus switch
        {
            SMS_Domain.Events.SPIComplianceStatus.NonCompliant => (UINotificationSeverity.Error, UIEventPriority.Critical, "SPI Non-Compliant"),
            SMS_Domain.Events.SPIComplianceStatus.AtRisk => (UINotificationSeverity.Warning, UIEventPriority.High, "SPI At Risk"),
            SMS_Domain.Events.SPIComplianceStatus.Compliant => (UINotificationSeverity.Success, UIEventPriority.Low, "SPI Compliant"),
            _ => (UINotificationSeverity.Info, UIEventPriority.Normal, "SPI Compliance Updated")
        };

        var uiEvent = new UINotificationEvent(
            severity: severity,
            title: title,
            message: $"{domainEvent.SPICode} moved to {domainEvent.NewStatus}. Current value {domainEvent.CurrentValue:F2} (threshold {domainEvent.ComplianceThreshold:F2}).",
            duration: 6000,
            category: "SPI",
            sourceLayer: nameof(SPIComplianceChangedEventHandler),
            reportId: HandlerHelpers.ResolveReportId(domainEvent),
            targetComponent: "NotificationCenter",
            priority: priority,
            metadata: new Dictionary<string, object>
            {
                { "SPICode", domainEvent.SPICode },
                { "SPIName", domainEvent.SPIName },
                { "PreviousStatus", domainEvent.PreviousStatus.ToString() },
                { "NewStatus", domainEvent.NewStatus.ToString() },
                { "RegulatoryBody", domainEvent.RegulatoryBody },
                { "RequiresRegulatoryReporting", domainEvent.RequiresRegulatoryReporting }
            });

        return await EventBus.PublishUIEventAsync(uiEvent, EventExecutionMode.Manual, cancellationToken);
    }
}
/// <summary>
/// Handles SPI threshold exceeded events and records threshold processing outcomes.
/// </summary>
public class SPIThresholdEventHandler : BaseDomainEventHandler<SPIThresholdExceededEvent>
{
    private readonly ILogger<SPIThresholdEventHandler> _logger;

    public SPIThresholdEventHandler(
        ILogger<SPIThresholdEventHandler> logger,
        IBaseEventBus eventBus)
        : base(logger, eventBus)
    {
        _logger = logger;
    }

    protected override async Task<Result> ProcessEventAsync(SPIThresholdExceededEvent domainEvent, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogApplicationInformation("[SPI THRESHOLD] Processed SPI threshold exceeded for {SPICode}: {CurrentValue} > {Threshold} (Severity: {Severity})",
                domainEvent.SPICode, domainEvent.CurrentValue, domainEvent.ThresholdValue, domainEvent.Severity);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "[SPI THRESHOLD] Error processing SPI threshold exceeded event for {SPICode}", domainEvent.SPICode);
            return Result.Failure(new Error("SPI_THRESHOLD_HANDLER_ERROR", $"SPI threshold processing failed: {ex.Message}"));
        }
    }
}



}
