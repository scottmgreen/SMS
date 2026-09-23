using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using SMS_Application.Common;
using SMS_Application.Queries;
using SMS_Domain.Common;
using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Domain.Events;
using SMS_Domain.ValueObjects;

namespace SMS_Application.Services;

public sealed class NotificationsScanService : INotificationsScanService
{
    private readonly IMitigationService _mitigationService;
    private readonly IBaseMediator _mediator;
    private readonly IBaseEventBus _eventBus;
    private readonly IEventQueueService _eventQueueService;
    private readonly ILogger<NotificationsScanService> _logger;
    private readonly IConfiguration _configuration;

    public NotificationsScanService(
        IMitigationService mitigationService,
        IBaseMediator mediator,
        IBaseEventBus eventBus,
        IEventQueueService eventQueueService,
        ILogger<NotificationsScanService> logger,
        IConfiguration configuration)
    {
        _mitigationService = mitigationService ?? throw new ArgumentNullException(nameof(mitigationService));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _eventQueueService = eventQueueService ?? throw new ArgumentNullException(nameof(eventQueueService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<Result<int>> ScanAllMitigationsAsync(string triggeredBy, CancellationToken cancellationToken = default)
    {
        var recipientGroups = GetMitigationRecipientGroups();
        if (!recipientGroups.Any())
        {
            _logger.LogApplicationInformation("Mitigation target-date scan skipped because no recipient groups are configured.");
            return Result.Success(0);
        }

        var mitigationResult = await _mitigationService.GetAllMitigationsAsync(cancellationToken).ConfigureAwait(false);
        if (mitigationResult.IsFailure || mitigationResult.Value is null)
        {
            _logger.LogApplicationWarning("Mitigation target-date scan failed to load mitigations.");
            return Result.Failure<int>(mitigationResult.Error ?? new Error("MITIGATION_SCAN_LOAD_FAILED", "Failed to load mitigations for target-date scan."));
        }

        var pendingResult = await _eventQueueService.GetQueuedEventsAsync(
            status: QueuedEventStatus.Pending,
            eventType: EventCategory.IntegrationEvent,
            maxResults: 5000).ConfigureAwait(false);

        if (pendingResult.IsFailure)
        {
            _logger.LogApplicationWarning("Mitigation target-date scan failed to load pending queue events.");
            return Result.Failure<int>(pendingResult.Error ?? new Error("MITIGATION_SCAN_QUEUE_LOAD_FAILED", "Failed to load pending queue events."));
        }

        var pendingEvents = pendingResult.Value?.ToList() ?? new List<QueuedEvent>();
        var queuedCount = 0;

        var daysInAdvance = _configuration.GetValue<int?>("MitigationTargetDateNotifications:DaysInAdvance") ?? 14;
        var hoursBefore = _configuration.GetValue<int?>("MitigationTargetDateNotifications:HoursBefore") ?? 24;

        foreach (var mitigation in mitigationResult.Value)
        {
            if (mitigation.TargetDate is null || IsMutedMitigationStatus(mitigation.Status?.Value))
            {
                continue;
            }

            var timingState = GetMitigationTimingState(mitigation.TargetDate.Value, mitigation.Status?.Value, daysInAdvance, hoursBefore);
            if (timingState == MitigationTimingState.None)
            {
                continue;
            }

            var mitigationCode = GetMitigationCode(mitigation);
            if (string.IsNullOrWhiteSpace(mitigationCode))
            {
                continue;
            }

            var alertKey = timingState.ToString().ToUpperInvariant();
            var alreadyPending = pendingEvents.Any(qe =>
                IsMitigationTargetDateNotification(qe.EventData, mitigationCode, alertKey));

            if (alreadyPending)
            {
                continue;
            }

            var emailEvent = BuildMitigationEmailEvent(mitigation, mitigation.HazardCode ?? string.Empty, recipientGroups, timingState, daysInAdvance, hoursBefore);
            var publishResult = await _eventBus.PublishIntegrationEventAsync(emailEvent, EventExecutionMode.Manual, cancellationToken).ConfigureAwait(false);
            if (publishResult.IsSuccess)
            {
                queuedCount++;
                pendingEvents.Add(QueuedEvent.FromIntegrationEvent(emailEvent, triggeredBy));
            }
        }

        return Result.Success(queuedCount);
    }

    public async Task<Result> ProcessMitigationUpdateAsync(Mitigation mitigation, string reportId, CancellationToken cancellationToken = default)
    {
        var mitigationCode = GetMitigationCode(mitigation);
        if (string.IsNullOrWhiteSpace(mitigationCode))
        {
            return Result.Success();
        }

        var cancelResult = await CancelPendingMitigationTargetDateNotificationsAsync(mitigationCode, cancellationToken).ConfigureAwait(false);
        if (cancelResult.IsFailure)
        {
            return cancelResult;
        }

        var isCompleted = mitigation.Progress >= 100 || IsMutedMitigationStatus(mitigation.Status?.Value);
        if (isCompleted || mitigation.TargetDate is null)
        {
            return Result.Success();
        }

        var recipientGroups = GetMitigationRecipientGroups();
        if (!recipientGroups.Any())
        {
            return Result.Success();
        }

        var daysInAdvance = _configuration.GetValue<int?>("MitigationTargetDateNotifications:DaysInAdvance") ?? 14;
        var hoursBefore = _configuration.GetValue<int?>("MitigationTargetDateNotifications:HoursBefore") ?? 24;

        var timingState = GetMitigationTimingState(mitigation.TargetDate.Value, mitigation.Status?.Value, daysInAdvance, hoursBefore);
        if (timingState == MitigationTimingState.None)
        {
            return Result.Success();
        }

        var alertKey = timingState.ToString().ToUpperInvariant();
        var pendingEventsResult = await _eventQueueService.GetQueuedEventsAsync(
            status: QueuedEventStatus.Pending,
            eventType: EventCategory.IntegrationEvent,
            maxResults: 5000).ConfigureAwait(false);

        if (pendingEventsResult.IsFailure)
        {
            return Result.Failure(pendingEventsResult.Error ?? new Error("MITIGATION_PENDING_LOAD_FAILED", "Failed to load pending mitigation notifications."));
        }

        var alreadyPending = (pendingEventsResult.Value ?? [])
            .Any(qe => IsMitigationTargetDateNotification(qe.EventData, mitigationCode, alertKey));

        if (alreadyPending)
        {
            return Result.Success();
        }

        var emailEvent = BuildMitigationEmailEvent(mitigation, reportId, recipientGroups, timingState, daysInAdvance, hoursBefore);
        return await _eventBus.PublishIntegrationEventAsync(emailEvent, EventExecutionMode.Manual, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Result<int>> ScanReportsNeedingStatusEscalationAsync(string triggeredBy, CancellationToken cancellationToken = default)
    {
        var enabled = _configuration.GetValue<bool?>("HazardReportNotifications:EnableStatusEscalationNotification") ?? true;
        if (!enabled)
        {
            _logger.LogApplicationInformation("Report status escalation scan skipped because feature is disabled.");
            return Result.Success(0);
        }

        var recipientGroups = GetReportRecipientGroups();
        if (!recipientGroups.Any())
        {
            _logger.LogApplicationInformation("Report status escalation scan skipped because no recipient groups are configured.");
            return Result.Success(0);
        }

        var reportsResult = await _mediator.SendAsync(new GetAllReportsQuery(), cancellationToken).ConfigureAwait(false);
        if (reportsResult.IsFailure || reportsResult.Value is null)
        {
            return Result.Failure<int>(reportsResult.Error ?? new Error("REPORT_STATUS_ESCALATION_LOAD_FAILED", "Failed to load reports for escalation scan."));
        }

        var pendingEventsResult = await _eventQueueService.GetQueuedEventsAsync(
            status: QueuedEventStatus.Pending,
            eventType: EventCategory.IntegrationEvent,
            maxResults: 5000).ConfigureAwait(false);

        if (pendingEventsResult.IsFailure)
        {
            return Result.Failure<int>(pendingEventsResult.Error ?? new Error("REPORT_STATUS_ESCALATION_QUEUE_LOAD_FAILED", "Failed to load pending queue events."));
        }

        var pendingEvents = pendingEventsResult.Value?.ToList() ?? new List<QueuedEvent>();
        var thresholdHours = _configuration.GetValue<int?>("HazardReportNotifications:StatusEscalationThresholdHours") ?? 48;
        if (thresholdHours <= 0)
        {
            thresholdHours = 48;
        }

        var executionMode = ResolveReportExecutionMode();
        var nowUtc = DateTime.UtcNow;
        var queuedCount = 0;

        foreach (var report in reportsResult.Value)
        {
            if (!IsNeedsValidation(report?.Status) || string.IsNullOrWhiteSpace(report?.Code))
            {
                continue;
            }

            var createdUtc = (report.CreatedDate ?? report.SubmittedDate).ToUniversalTime();
            var dueUtc = createdUtc.AddHours(thresholdHours);
            if (dueUtc > nowUtc)
            {
                continue;
            }

            var reportCode = report.Code.Trim();
            var alreadyPending = pendingEvents.Any(qe => IsPendingReportStatusEscalation(qe.EventData, reportCode));
            if (alreadyPending)
            {
                continue;
            }

            var hazardResult = await _mediator.SendAsync(
                new GetHazardsByReportCodeQuery(new ReportID(reportCode)),
                cancellationToken).ConfigureAwait(false);

            var primaryHazard = hazardResult.IsSuccess && hazardResult.Value is not null
                ? hazardResult.Value.FirstOrDefault(h => h.IsInitialHazard) ?? hazardResult.Value.FirstOrDefault()
                : null;

            var escalationEvent = BuildReportEscalationEmailEvent(
                report,
                primaryHazard,
                recipientGroups,
                thresholdHours,
                dueUtc);

            var publishResult = await _eventBus.PublishIntegrationEventAsync(escalationEvent, executionMode, cancellationToken).ConfigureAwait(false);
            if (!publishResult.IsSuccess)
            {
                _logger.LogApplicationWarning("Failed publishing report status escalation notification for report {ReportCode}: {Error}", reportCode, publishResult.Error?.Message);
                continue;
            }

            pendingEvents.Add(QueuedEvent.FromIntegrationEvent(escalationEvent, triggeredBy));
            queuedCount++;
        }

        return Result.Success(queuedCount);
    }

    private async Task<Result> CancelPendingMitigationTargetDateNotificationsAsync(string mitigationCode, CancellationToken cancellationToken)
    {
        var pendingEventsResult = await _eventQueueService.GetQueuedEventsAsync(
            status: QueuedEventStatus.Pending,
            eventType: EventCategory.IntegrationEvent,
            maxResults: 5000).ConfigureAwait(false);

        if (pendingEventsResult.IsFailure || pendingEventsResult.Value is null)
        {
            return pendingEventsResult.IsFailure
                ? Result.Failure(pendingEventsResult.Error)
                : Result.Success();
        }

        var pendingMitigationAlerts = pendingEventsResult.Value
            .Where(qe => IsMitigationTargetDateNotificationAnyTiming(qe.EventData, mitigationCode))
            .ToList();

        foreach (var pendingAlert in pendingMitigationAlerts)
        {
            var cancelResult = await _eventQueueService.CancelQueuedEventAsync(
                pendingAlert.QueueCode,
                "MitigationUpdate").ConfigureAwait(false);

            if (cancelResult.IsFailure)
            {
                _logger.LogApplicationWarning("Failed to cancel pending mitigation target-date notification {QueueCode}.", pendingAlert.QueueCode);
            }
        }

        return Result.Success();
    }

    private List<string> GetMitigationRecipientGroups()
    {
        return _configuration
            .GetSection("MitigationTargetDateNotifications:RecipientGroups")
            .Get<string[]>()?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList()
            ?? _configuration.GetSection("HazardReportNotifications:RecipientGroups").Get<string[]>()?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
            ?? new List<string>();
    }

    private List<string> GetReportRecipientGroups()
    {
        return _configuration
            .GetSection("HazardReportNotifications:RecipientGroups")
            .Get<string[]>()?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList()
            ?? new List<string>();
    }

    private EventExecutionMode ResolveReportExecutionMode()
    {
        var configured = _configuration.GetValue<string>("HazardReportNotifications:StatusEscalationExecutionMode");
        if (Enum.TryParse<EventExecutionMode>(configured, true, out var parsed))
        {
            return parsed;
        }

        return EventExecutionMode.Manual;
    }

    private static string GetMitigationCode(Mitigation mitigation)
        => (mitigation.Code ?? mitigation.Id.Value ?? string.Empty).Trim();

    private static bool IsMutedMitigationStatus(string? statusValue)
        => string.Equals(statusValue, MitigationStatus.Complete.Value, StringComparison.OrdinalIgnoreCase)
            || string.Equals(statusValue, MitigationStatus.HazardEliminated.Value, StringComparison.OrdinalIgnoreCase);

    private static MitigationTimingState GetMitigationTimingState(DateTime targetDate, string? statusValue, int daysInAdvance, int hoursBefore)
    {
        if (IsMutedMitigationStatus(statusValue))
        {
            return MitigationTimingState.None;
        }

        var now = DateTime.UtcNow;
        var timeUntilTarget = targetDate - now;

        if (timeUntilTarget.TotalHours < 0)
        {
            return MitigationTimingState.Overdue;
        }

        if (timeUntilTarget.TotalHours <= hoursBefore)
        {
            return MitigationTimingState.Due24Hours;
        }

        if (timeUntilTarget.TotalDays <= daysInAdvance)
        {
            return MitigationTimingState.Due14Days;
        }

        return MitigationTimingState.None;
    }

    private static bool IsMitigationTargetDateNotification(string? eventData, string mitigationCode, string alertKey)
        => !string.IsNullOrWhiteSpace(eventData)
            && eventData.Contains("MitigationTargetDateNotification", StringComparison.OrdinalIgnoreCase)
            && eventData.Contains(mitigationCode, StringComparison.OrdinalIgnoreCase)
            && eventData.Contains(alertKey, StringComparison.OrdinalIgnoreCase);

    private static bool IsMitigationTargetDateNotificationAnyTiming(string? eventData, string mitigationCode)
        => !string.IsNullOrWhiteSpace(eventData)
            && eventData.Contains("MitigationTargetDateNotification", StringComparison.OrdinalIgnoreCase)
            && eventData.Contains(mitigationCode, StringComparison.OrdinalIgnoreCase);

    private static bool IsNeedsValidation(string? status)
        => string.Equals(status, ReportStatus.NeedsValidation.Value, StringComparison.OrdinalIgnoreCase)
           || string.Equals(status, ReportStatus.NeedsValidation.Name, StringComparison.OrdinalIgnoreCase);

    private static bool IsPendingReportStatusEscalation(string? eventData, string reportCode)
        => !string.IsNullOrWhiteSpace(eventData)
           && eventData.Contains("HazardSubmissionStatusEscalation", StringComparison.OrdinalIgnoreCase)
           && eventData.Contains(reportCode, StringComparison.OrdinalIgnoreCase);

    private static string BuildMitigationAlertSubject(string mitigationCode, MitigationTimingState timingState)
    {
        var title = timingState switch
        {
            MitigationTimingState.Due14Days => "Mitigation 14 Day Target Date Alert",
            MitigationTimingState.Due24Hours => "Mitigation 24 Hour Target Date Alert",
            MitigationTimingState.Overdue => "Mitigation Past Target Date Alert",
            _ => "Mitigation Target Date Alert"
        };

        return $"{title} - {mitigationCode}";
    }

    private static string BuildMitigationTargetDateNotificationEmailHtml(Mitigation mitigation, MitigationTimingState timingState, int daysInAdvance, int hoursBefore)
    {
        var targetDate = mitigation.TargetDate?.ToString("MMMM dd, yyyy h:mm tt") ?? "Not set";
        var alertText = timingState switch
        {
            MitigationTimingState.Due14Days => $"This mitigation reaches its target date in approximately {daysInAdvance} days.",
            MitigationTimingState.Due24Hours => $"This mitigation reaches its target date in approximately {hoursBefore} hours.",
            MitigationTimingState.Overdue => "This mitigation is now overdue and requires immediate attention.",
            _ => "Mitigation target-date alert."
        };

        return SMSEmailTemplateBuilder.BuildStandardEmail(
            title: "Mitigation Target Date Notification",
            introHtml: alertText,
            summaryFields:
            [
                new SMSEmailField { Label = "Mitigation ID", Value = mitigation.Code },
                new SMSEmailField { Label = "Hazard ID", Value = mitigation.HazardCode ?? string.Empty },
                new SMSEmailField { Label = "Target Date", Value = targetDate },
                new SMSEmailField { Label = "Current Status", Value = mitigation.Status?.Name ?? mitigation.Status?.Value ?? string.Empty }
            ],
            sections:
            [
                new SMSEmailSection
                {
                    Title = "Mitigation Details",
                    Fields =
                    [
                        new SMSEmailField { Label = "Name", Value = mitigation.Name ?? string.Empty },
                        new SMSEmailField { Label = "Assigned To", Value = mitigation.AssignedTo ?? string.Empty },
                        new SMSEmailField { Label = "Progress", Value = $"{mitigation.Progress}%" },
                        new SMSEmailField { Label = "Description", Value = mitigation.Description ?? string.Empty, IsFullWidth = true }
                    ]
                }
            ],
            footerHtml: "This notification was generated by SMS3 mitigation target-date monitoring.");
    }

    private static EmailNotificationEvent BuildMitigationEmailEvent(
        Mitigation mitigation,
        string reportId,
        List<string> recipientGroups,
        MitigationTimingState timingState,
        int daysInAdvance,
        int hoursBefore)
    {
        var mitigationCode = GetMitigationCode(mitigation);
        var alertKey = timingState.ToString().ToUpperInvariant();

        return new EmailNotificationEvent(
            toRecipients: recipientGroups,
            subject: BuildMitigationAlertSubject(mitigationCode, timingState),
            body: BuildMitigationTargetDateNotificationEmailHtml(mitigation, timingState, daysInAdvance, hoursBefore),
            isHtmlContent: true,
            priority: timingState == MitigationTimingState.Overdue ? EmailPriority.High : EmailPriority.Normal,
            reportId: reportId,
            workflowType: "MitigationTargetDateNotification",
            relatedEntityType: "Mitigation",
            relatedEntityId: mitigationCode,
            emailMetadata: new Dictionary<string, object>
            {
                { "MitigationCode", mitigationCode },
                { "HazardCode", mitigation.HazardCode ?? string.Empty },
                { "TargetDate", mitigation.TargetDate?.ToString("O") ?? string.Empty },
                { "TimingAlert", alertKey }
            });
    }

    private static EmailNotificationEvent BuildReportEscalationEmailEvent(
        Report report,
        Hazard? hazard,
        List<string> recipientGroups,
        int thresholdHours,
        DateTime dueUtc)
    {
        var reportCode = report.Code?.Trim() ?? string.Empty;
        var hazardCode = hazard?.Code ?? string.Empty;
        var hazardDescription = hazard?.Description ?? report.Description ?? string.Empty;

        var body = SMSEmailTemplateBuilder.BuildStandardEmail(
            title: "Hazard Report Inaction Notification",
            introHtml: $"Report remains in <strong>{ReportStatus.NeedsValidation.Value}</strong> after {thresholdHours} hour(s).",
            summaryFields:
            [
                new SMSEmailField { Label = "Report ID", Value = reportCode },
                new SMSEmailField { Label = "Hazard ID", Value = hazardCode },
                new SMSEmailField { Label = "Current Status", Value = report.Status ?? string.Empty },
                new SMSEmailField { Label = "Expected Status", Value = ReportStatus.ReadyForProcessing.Value }
            ],
            sections:
            [
                new SMSEmailSection
                {
                    Title = "Status Escalation Details",
                    Fields =
                    [
                        new SMSEmailField { Label = "Report Inaction Threshold", Value = $"{thresholdHours} hour(s)" },
                        new SMSEmailField { Label = "Report Inaction Checkpoint (UTC)", Value = dueUtc.ToString("yyyy-MM-dd HH:mm:ss") },
                        new SMSEmailField { Label = "Hazard Description", Value = hazardDescription, IsFullWidth = true }
                    ]
                }
            ],
            footerHtml: "This escalation was generated by report status monitoring.");

        return new EmailNotificationEvent(
            toRecipients: recipientGroups,
            subject: $"SMS Hazard Report Inaction Notification - {reportCode} / {hazardCode}",
            body: body,
            isHtmlContent: true,
            priority: EmailPriority.High,
            reportId: reportCode,
            workflowType: "HazardSubmissionStatusEscalation",
            relatedEntityType: "Report",
            relatedEntityId: reportCode,
            emailMetadata: new Dictionary<string, object>
            {
                { "ReportCode", reportCode },
                { "HazardCode", hazardCode },
                { "ExpectedStatus", ReportStatus.ReadyForProcessing.Value },
                { "CurrentStatus", report.Status ?? string.Empty },
                { "StatusEscalationThresholdHours", thresholdHours },
                { "StatusCheckDueUtc", dueUtc.ToString("O") }
            });
    }

    private enum MitigationTimingState
    {
        None,
        Due14Days,
        Due24Hours,
        Overdue
    }
}
