//-----------------------------------------------------------------------
// <copyright file="DomainEventHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Consolidated domain event handlers for hazard and SPI automation.
// </copyright>
//-----------------------------------------------------------------------



using SMS_Domain.Events;
using SMS_Domain.Events.UIEvents;

namespace SMS_Application.EventHandlers
{
       



public sealed class HazardCreatedEventHandler : BaseDomainEventHandler<SMS_Domain.Events.HazardCreatedEvent>
{
    private readonly IBaseEventBus _eventBus;
    private readonly ILogger<HazardCreatedEventHandler> _logger;

    public HazardCreatedEventHandler(
        ILogger<HazardCreatedEventHandler> logger,
        IBaseEventBus eventBus)
        : base(logger, eventBus)
    {
        _logger = logger;
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }

    protected override async Task<Result> ProcessEventAsync(SMS_Domain.Events.HazardCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("[HAZARD HANDLER] Processing hazard creation for {HazardCode} (Type: {HazardType}, Priority: {Priority})", domainEvent.HazardCode, domainEvent.HazardType, domainEvent.Priority);

            var domainResult = await HandleDomainEventAsync(domainEvent, cancellationToken);
            if (domainResult.IsFailure)
            {
                return domainResult;
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

            _logger.LogInformation("[HAZARD HANDLER] Successfully processed hazard creation for {HazardCode}", domainEvent.HazardCode);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[HAZARD HANDLER] Error processing hazard creation event for {HazardCode}", domainEvent.HazardCode);
            return Result.Failure(new Error("HAZARD_CREATION_HANDLER_ERROR", $"Hazard creation processing failed: {ex.Message}"));
        }
    }

    private Task<Result> HandleDomainEventAsync(SMS_Domain.Events.HazardCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[DOMAIN EVENT] HazardCreatedEvent received for {HazardCode}", domainEvent.HazardCode);
        return Task.FromResult(Result.Success());
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
                _logger.LogWarning("[UI EVENT] Failed to queue popup notification: {Error}", uiResult.Error.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[UI EVENT] Failed to publish UI notification for hazard {HazardCode}", domainEvent.HazardCode);
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
                _logger.LogError("[INTEGRATION EVENT] Failed to queue hazard notification email: {Error}", emailResult.Error.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[INTEGRATION EVENT] Failed to send email notifications for {HazardCode}", domainEvent.HazardCode);
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
public sealed class HazardUpdatedEventHandler : BaseDomainEventHandler<HazardUpdatedEvent>
{
    public HazardUpdatedEventHandler(ILogger<HazardUpdatedEventHandler> logger, IBaseEventBus eventBus) : base(logger, eventBus) { }

    protected override Task<Result> HandleDomainEventAsync(HazardUpdatedEvent domainEvent, CancellationToken cancellationToken)
    {
        Logger.LogInformation("[DOMAIN EVENT] Hazard updated. HazardId: {HazardId}, ReportId: {ReportId}",domainEvent.HazardId,HandlerHelpers.ResolveReportId(domainEvent));

        return Task.FromResult(Result.Success());
    }

    //protected override async Task<Result> HandleIntegrationEventAsync(HazardUpdatedEvent domainEvent, CancellationToken cancellationToken)
    //{
    //    var reportId = HandlerHelpers.ResolveReportId(domainEvent);

    //    var emailEvent = new EmailNotificationEvent(
    //        toRecipients: new List<string> { "safety.team@pdxairport.com" },
    //        subject: $"Hazard Updated: {domainEvent.HazardId}",
    //        body: $"Hazard '{domainEvent.HazardId}' was updated. Report: {reportId}",
    //        isHtmlContent: false,
    //        priority: EmailPriority.Normal,
    //        deliveryMode: IntegrationDeliveryMode.BestEffort,
    //        reportId: reportId,
    //        workflowType: "HazardUpdateNotification",
    //        relatedEntityType: "Hazard",
    //        relatedEntityId: domainEvent.HazardId,
    //        emailMetadata: new Dictionary<string, object>
    //        {
    //            { "Source", nameof(HazardUpdatedEventHandler) },
    //            { "HazardId", domainEvent.HazardId ?? string.Empty }
    //        });

    //    //return await EventBus.PublishIntegrationEventAsync(emailEvent, EventExecutionMode.Manual, cancellationToken);
    //}

    protected override async Task<Result> HandleUIEventAsync(HazardUpdatedEvent domainEvent, CancellationToken cancellationToken)
    {
        var reportId = HandlerHelpers.ResolveReportId(domainEvent);

        var uiEvent = new UINotificationEvent(
            severity: UINotificationSeverity.Info,
            title: "Hazard Updated",
            message: $"Hazard {domainEvent.HazardId} was updated" + (string.IsNullOrWhiteSpace(reportId) ? string.Empty : $" (Report: {reportId})"),
            duration: 5000,
            category: "Hazard",
            sourceLayer: nameof(HazardUpdatedEventHandler),
            reportId: reportId,
            targetComponent: "NotificationCenter",
            priority: UIEventPriority.Normal,
            metadata: new Dictionary<string, object>
            {
                { "HazardId", domainEvent.HazardId ?? string.Empty },
                { "ReportId", reportId }
            });

        return await EventBus.PublishUIEventAsync(uiEvent, EventExecutionMode.Manual, cancellationToken);
    }

    
}

public sealed class HazardDeletedEventHandler : BaseDomainEventHandler<HazardDeletedEvent>
{
    public HazardDeletedEventHandler(ILogger<HazardDeletedEventHandler> logger, IBaseEventBus eventBus) : base(logger, eventBus) { }
}

public sealed class HazardStatusChangedEventHandler : BaseDomainEventHandler<HazardStatusChangedEvent>
{
    public HazardStatusChangedEventHandler(ILogger<HazardStatusChangedEventHandler> logger, IBaseEventBus eventBus) : base(logger, eventBus) { }
}

public sealed class HighRiskIdentifiedEventHandler : BaseDomainEventHandler<HighRiskIdentifiedEvent>
{
    public HighRiskIdentifiedEventHandler(ILogger<HighRiskIdentifiedEventHandler> logger, IBaseEventBus eventBus) : base(logger, eventBus) { }
}

public sealed class MitigationApprovalApprovedEventHandler : BaseDomainEventHandler<MitigationApprovalApprovedEvent>
{
    public MitigationApprovalApprovedEventHandler(ILogger<MitigationApprovalApprovedEventHandler> logger, IBaseEventBus eventBus) : base(logger, eventBus) { }
}

public sealed class MitigationApprovalRequestedEventHandler : BaseDomainEventHandler<MitigationApprovalRequestedEvent>
{
    public MitigationApprovalRequestedEventHandler(ILogger<MitigationApprovalRequestedEventHandler> logger, IBaseEventBus eventBus) : base(logger, eventBus) { }
}

public sealed class MitigationCompletedEventHandler : BaseDomainEventHandler<MitigationCompletedEvent>
{
    public MitigationCompletedEventHandler(ILogger<MitigationCompletedEventHandler> logger, IBaseEventBus eventBus) : base(logger, eventBus) { }
}

public sealed class MitigationCreatedEventHandler : BaseDomainEventHandler<MitigationCreatedEvent>
{
    public MitigationCreatedEventHandler(ILogger<MitigationCreatedEventHandler> logger, IBaseEventBus eventBus) : base(logger, eventBus) { }
}

public sealed class MitigationOverdueEventHandler : BaseDomainEventHandler<MitigationOverdueEvent>
{
    public MitigationOverdueEventHandler(ILogger<MitigationOverdueEventHandler> logger, IBaseEventBus eventBus) : base(logger, eventBus) { }
}

public sealed class MitigationStatusChangedEventHandler : BaseDomainEventHandler<MitigationStatusChangedEvent>
{
    public MitigationStatusChangedEventHandler(ILogger<MitigationStatusChangedEventHandler> logger, IBaseEventBus eventBus) : base(logger, eventBus) { }
}

public sealed class ReportClosedEventHandler : BaseDomainEventHandler<ReportClosedEvent>
{
    public ReportClosedEventHandler(ILogger<ReportClosedEventHandler> logger, IBaseEventBus eventBus) : base(logger, eventBus) { }
}

public sealed class ReportCreatedEventHandler : BaseDomainEventHandler<ReportCreatedEvent>
{
    public ReportCreatedEventHandler(ILogger<ReportCreatedEventHandler> logger, IBaseEventBus eventBus) : base(logger, eventBus) { }
}

public sealed class ReportUpdatedEventHandler : BaseDomainEventHandler<ReportUpdatedEvent>
{
    public ReportUpdatedEventHandler(ILogger<ReportUpdatedEventHandler> logger, IBaseEventBus eventBus) : base(logger, eventBus) { }
}

public sealed class RiskAssessmentCompletedEventHandler : BaseDomainEventHandler<RiskAssessmentCompletedEvent>
{
    public RiskAssessmentCompletedEventHandler(ILogger<RiskAssessmentCompletedEventHandler> logger, IBaseEventBus eventBus) : base(logger, eventBus) { }
}

public sealed class RiskAssessmentCreatedEventHandler : BaseDomainEventHandler<RiskAssessmentCreatedEvent>
{
    public RiskAssessmentCreatedEventHandler(ILogger<RiskAssessmentCreatedEventHandler> logger, IBaseEventBus eventBus) : base(logger, eventBus) { }
}

public sealed class RiskAssessmentUpdatedEventHandler : BaseDomainEventHandler<RiskAssessmentUpdatedEvent>
{
    public RiskAssessmentUpdatedEventHandler(ILogger<RiskAssessmentUpdatedEventHandler> logger, IBaseEventBus eventBus) : base(logger, eventBus) { }
}

public sealed class SPIComplianceChangedEventHandler : BaseDomainEventHandler<SPIComplianceChangedEvent>
{
    public SPIComplianceChangedEventHandler(ILogger<SPIComplianceChangedEventHandler> logger, IBaseEventBus eventBus) : base(logger, eventBus) { }
}
public class HazardEventSPIHandler
{
    private readonly ISPIAutomationService _spiAutomationService;
    private readonly ILogger<HazardEventSPIHandler> _logger;

    public HazardEventSPIHandler(
        ISPIAutomationService spiAutomationService,
        ILogger<HazardEventSPIHandler> logger)
    {
        _spiAutomationService = spiAutomationService ?? throw new ArgumentNullException(nameof(spiAutomationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleHazardCreated(HazardCreatedEvent evt)
    {
        try
        {
            var result = await _spiAutomationService.UpdateHazardReportRateAsync(evt.CreatedDate, CancellationToken.None);

            if (result.IsFailure)
            {
                _logger.LogWarning("SPI Event: Failed to process HazardCreated event for hazard {HazardCode}: {Error}", evt.HazardCode, result.Error.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SPI Event: Error processing HazardCreated event for hazard {HazardCode}", evt.HazardCode);
        }
    }
}

public class MitigationEventSPIHandler
{
    private readonly ISPIAutomationService _spiAutomationService;
    private readonly ILogger<MitigationEventSPIHandler> _logger;

    public MitigationEventSPIHandler(
        ISPIAutomationService spiAutomationService,
        ILogger<MitigationEventSPIHandler> logger)
    {
        _spiAutomationService = spiAutomationService ?? throw new ArgumentNullException(nameof(spiAutomationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleMitigationCompleted(MitigationCompletedEvent evt)
    {
        try
        {
            var result = await _spiAutomationService.UpdateMitigationImplementationRateAsync(
                evt.MitigationId,
                evt.TargetCompletionDate,
                evt.CompletedDate,
                CancellationToken.None);

            if (result.IsFailure)
            {
                _logger.LogWarning("SPI Event: Failed to process MitigationCompleted event for mitigation {MitigationId}: {Error}", evt.MitigationId, result.Error.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SPI Event: Error processing MitigationCompleted event for mitigation {MitigationId}", evt.MitigationId);
        }
    }

    public async Task HandleMitigationOverdue(MitigationOverdueEvent evt)
    {
        try
        {
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SPI Event: Error processing MitigationOverdue event for mitigation {MitigationId}", evt.MitigationId);
        }
    }
}

public class RiskAssessmentEventSPIHandler
{
    private readonly ISPIAutomationService _spiAutomationService;
    private readonly ILogger<RiskAssessmentEventSPIHandler> _logger;

    public RiskAssessmentEventSPIHandler(
        ISPIAutomationService spiAutomationService,
        ILogger<RiskAssessmentEventSPIHandler> logger)
    {
        _spiAutomationService = spiAutomationService ?? throw new ArgumentNullException(nameof(spiAutomationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleRiskAssessmentCompleted(RiskAssessmentCompletedEvent evt)
    {
        try
        {
            var isOnTime = evt.CompletedDate <= evt.TargetCompletionDate;

            var result = await _spiAutomationService.UpdateRiskAssessmentCompletionAsync(
                evt.AssessmentId,
                evt.TargetCompletionDate,
                evt.CompletedDate,
                isOnTime,
                CancellationToken.None);

            if (result.IsFailure)
            {
                _logger.LogWarning("SPI Event: Failed to process RiskAssessmentCompleted event for assessment {AssessmentId}: {Error}", evt.AssessmentId, result.Error.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SPI Event: Error processing RiskAssessmentCompleted event for assessment {AssessmentId}", evt.AssessmentId);
        }
    }
}

public class SPIAutomationEventHandler : BaseDomainEventHandler<SMS_Domain.Events.HazardCreatedEvent>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SPIAutomationEventHandler> _logger;

    public SPIAutomationEventHandler(
        IServiceProvider serviceProvider,
        ILogger<SPIAutomationEventHandler> logger,
        IBaseEventBus eventBus)
        : base(logger, eventBus)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger;
    }

    protected override async Task<Result> ProcessEventAsync(SMS_Domain.Events.HazardCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var spiAutomationService = scope.ServiceProvider.GetRequiredService<ISPIAutomationService>();

            var hazardRateResult = await spiAutomationService.UpdateHazardReportRateAsync(domainEvent.CreatedDate, cancellationToken);

            if (hazardRateResult.IsFailure)
            {
                _logger.LogWarning("[SPI AUTOMATION] Failed to update Hazard Report Rate SPI for {HazardCode}: {Error}", domainEvent.HazardCode, hazardRateResult.Error.Message);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SPI AUTOMATION] SPI automation failed for hazard {HazardCode}", domainEvent.HazardCode);
            return Result.Failure(new Error("SPI_AUTOMATION_FAILED", $"SPI automation failed: {ex.Message}"));
        }
    }
}

public class SPIThresholdEventHandler : BaseDomainEventHandler<SPIThresholdExceededEvent>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SPIThresholdEventHandler> _logger;

    public SPIThresholdEventHandler(
        ILogger<SPIThresholdEventHandler> logger,
        IServiceProvider serviceProvider,
        IBaseEventBus eventBus)
        : base(logger, eventBus)
    {
        _logger = logger;
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    protected override async Task<Result> ProcessEventAsync(SPIThresholdExceededEvent domainEvent, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            _ = scope.ServiceProvider.GetRequiredService<SMSStakeholderGroupService>();
            _ = scope.ServiceProvider.GetRequiredService<SPIEventCoordinator>();

            _logger.LogInformation("[SPI THRESHOLD] Processed SPI threshold exceeded for {SPICode}: {CurrentValue} > {Threshold} (Severity: {Severity})",
                domainEvent.SPICode, domainEvent.CurrentValue, domainEvent.ThresholdValue, domainEvent.Severity);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SPI THRESHOLD] Error processing SPI threshold exceeded event for {SPICode}", domainEvent.SPICode);
            return Result.Failure(new Error("SPI_THRESHOLD_HANDLER_ERROR", $"SPI threshold processing failed: {ex.Message}"));
        }
    }
}



}