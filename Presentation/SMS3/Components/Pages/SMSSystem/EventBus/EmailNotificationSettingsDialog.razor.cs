using Microsoft.AspNetCore.Components;
using Radzen;
using SMS_Application.Commands;
using SMS_Application.Interfaces;
using SMS_Application.Queries;
using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS3.Components.Pages.SMSSystem.Services;
using System.Text.Json;

namespace SMS3.Components.Pages.SMSSystem.EventBus;

public class EmailNotificationSettingsDialogBase : ComponentBase
{
    [Inject] protected DialogService DialogService { get; set; } = default!;
    [Inject] protected NotificationService NotificationService { get; set; } = default!;
    [Inject] protected IEmailNotificationSettingsService EmailNotificationSettingsService { get; set; } = default!;
    [Inject] protected IBaseMediator Mediator { get; set; } = default!;

    protected List<NotificationSetting> _settings = [];
    protected List<NotificationSetting> _originalSettings = [];
    protected bool _isSaving;

    protected readonly NotificationExecutionMode[] _allowedModes =
    [
        NotificationExecutionMode.Immediate,
        NotificationExecutionMode.Manual
    ];

    protected override void OnInitialized()
    {
        _settings = EmailNotificationSettingsService
            .GetAllNotificationSettings()
            .Select(x => new NotificationSetting
            {
                NotificationType = x.NotificationType,
                Enabled = x.Enabled,
                TriggerDomainEvent = x.TriggerDomainEvent,
                AppSettingsSection = x.AppSettingsSection,
                ChannelName = x.ChannelName,
                ExecutionMode = x.ExecutionMode
            })
            .ToList();

        _originalSettings = _settings
            .Select(x => new NotificationSetting
            {
                NotificationType = x.NotificationType,
                Enabled = x.Enabled,
                TriggerDomainEvent = x.TriggerDomainEvent,
                AppSettingsSection = x.AppSettingsSection,
                ChannelName = x.ChannelName,
                ExecutionMode = x.ExecutionMode
            })
            .ToList();
    }

    protected async Task SaveAsync()
    {
        if (_isSaving)
        {
            return;
        }

        try
        {
            if (!await ConfirmHighImpactChangesAsync())
            {
                return;
            }

            _isSaving = true;

            var executionPlan = await BuildPendingExecutionPlanAsync();
            var saveResult = await EmailNotificationSettingsService.SaveNotificationSettingsAsync(_settings, CancellationToken.None);
            if (saveResult.IsFailure)
            {
                var isMissingSectionError = string.Equals(
                    saveResult.Error?.Code,
                    "EMAIL_NOTIFICATION_SETTINGS_SECTION_NOT_FOUND",
                    StringComparison.OrdinalIgnoreCase);

                NotificationService.Notify(new NotificationMessage
                {
                    Severity = isMissingSectionError ? NotificationSeverity.Warning : NotificationSeverity.Error,
                    Summary = isMissingSectionError ? "Configuration Section Missing" : "Save Failed",
                    Detail = isMissingSectionError
                        ? "NotificationEvents channel settings were not found in appsettings.json. Verify each notification has Channels:<ChannelName>:ExecutionMode configured, then try again."
                        : saveResult.Error?.Message ?? "Unable to save notification settings.",
                    Duration = isMissingSectionError ? 8000 : 5000
                });
                return;
            }

            if (executionPlan.Count > 0)
            {
                await ExecutePendingEventsAsync(executionPlan);
            }

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Saved",
                Detail = "Notification execution settings were updated.",
                Duration = 3000
            });

            DialogService.Close(true);
        }
        finally
        {
            _isSaving = false;
        }
    }

    protected void Close()
    {
        DialogService.Close(false);
    }

    private async Task<bool> ConfirmHighImpactChangesAsync()
    {
        var domainChange = GetSettingChange("DomainEventPublishing");
        if (domainChange is not null
            && domainChange.Value.Original.Enabled
            && !domainChange.Value.Updated.Enabled)
        {
            var disableConfirm = await DialogService.Confirm(
                "Disabling DomainEventPublishing will suppress new domain event execution and can prevent downstream Integration/UI triggers from running. Existing queued events are not auto-processed by this change. Continue?",
                "Confirm Domain Event Publishing Disable",
                new ConfirmOptions
                {
                    OkButtonText = "Disable",
                    CancelButtonText = "Cancel",
                    AutoFocusFirstElement = true
                });

            if (disableConfirm != true)
            {
                RevertSettingToOriginal("DomainEventPublishing");
                return false;
            }
        }

        if (domainChange is not null
            && domainChange.Value.Original.ExecutionMode == NotificationExecutionMode.Immediate
            && domainChange.Value.Updated.ExecutionMode == NotificationExecutionMode.Manual)
        {
            var domainConfirm = await DialogService.Confirm(
                "Switching DomainEventPublishing from Immediate to Manual will stop immediate domain handler execution and can delay downstream integration/UI triggers for newly published domain events. Continue?",
                "Confirm Domain Event Mode Change",
                new ConfirmOptions
                {
                    OkButtonText = "Continue",
                    CancelButtonText = "Cancel",
                    AutoFocusFirstElement = true
                });

            if (domainConfirm != true)
            {
                RevertSettingToOriginal("DomainEventPublishing");
                return false;
            }
        }

        return true;
    }

    private async Task<Dictionary<string, List<string>>> BuildPendingExecutionPlanAsync()
    {
        var plan = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var updated in _settings)
        {
            if (!IsEmailIntegrationSetting(updated))
            {
                continue;
            }

            var change = GetSettingChange(updated.NotificationType);
            if (change is null)
            {
                continue;
            }

            if (change.Value.Original.ExecutionMode != NotificationExecutionMode.Manual
                || change.Value.Updated.ExecutionMode != NotificationExecutionMode.Immediate)
            {
                continue;
            }

            var queueCodes = await GetPendingManualEmailQueueCodesAsync(updated.NotificationType);
            if (queueCodes.Count == 0)
            {
                continue;
            }

            var confirmation = await DialogService.Confirm(
                $"{queueCodes.Count} pending Manual email queue event(s) exist for '{updated.NotificationType}'. Execute them now after saving?",
                "Execute Existing Manual Queue Events",
                new ConfirmOptions
                {
                    OkButtonText = "Save + Execute",
                    CancelButtonText = "Save Only",
                    AutoFocusFirstElement = true
                });

            if (confirmation == true)
            {
                plan[updated.NotificationType] = queueCodes;
            }
        }

        return plan;
    }

    private async Task<List<string>> GetPendingManualEmailQueueCodesAsync(string notificationType)
    {
        var pendingResult = await Mediator.SendAsync(
            new GetQueuedEventsQuery(QueuedEventStatus.Pending, EventCategory.IntegrationEvent, 5000),
            CancellationToken.None);

        if (pendingResult.IsFailure)
        {
            return [];
        }

        return pendingResult.Value
            .Where(x => string.Equals(x.EventType, SMS_Domain.Enums.EventType.EmailNotification.Value, StringComparison.OrdinalIgnoreCase))
            .Where(x => string.Equals(x.QueuedBy?.Trim(), "ManualExecution", StringComparison.OrdinalIgnoreCase))
            .Where(x => IsMatchingEmailWorkflowType(x.EventData, notificationType))
            .Select(x => x.QueueCode)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private async Task ExecutePendingEventsAsync(Dictionary<string, List<string>> executionPlan)
    {
        var totalExecuted = 0;

        foreach (var queueCode in executionPlan.SelectMany(x => x.Value).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var result = await Mediator.SendAsync(
                new ExecuteQueuedEventCommand(queueCode, "EventSettings-ModeChange"),
                CancellationToken.None);

            if (result.IsSuccess)
            {
                totalExecuted++;
            }
        }

        if (totalExecuted > 0)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Info,
                Summary = "Manual Queue Executed",
                Detail = $"Executed {totalExecuted} existing pending manual event(s) after settings update.",
                Duration = 5000
            });
        }
    }

    private (NotificationSetting Original, NotificationSetting Updated)? GetSettingChange(string notificationType)
    {
        var original = _originalSettings.FirstOrDefault(x =>
            string.Equals(x.NotificationType, notificationType, StringComparison.OrdinalIgnoreCase));

        var updated = _settings.FirstOrDefault(x =>
            string.Equals(x.NotificationType, notificationType, StringComparison.OrdinalIgnoreCase));

        if (original is null || updated is null)
        {
            return null;
        }

        var changed = original.ExecutionMode != updated.ExecutionMode || original.Enabled != updated.Enabled;
        return changed ? (original, updated) : null;
    }

    private static bool IsEmailIntegrationSetting(NotificationSetting setting)
    {
        return setting.AppSettingsSection.Contains(":Channels:Email", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMatchingEmailWorkflowType(string eventData, string notificationType)
    {
        if (string.IsNullOrWhiteSpace(eventData) || string.IsNullOrWhiteSpace(notificationType))
        {
            return false;
        }

        try
        {
            using var doc = JsonDocument.Parse(eventData);
            if (!doc.RootElement.TryGetProperty("WorkflowType", out var workflowTypeElement)
                && !doc.RootElement.TryGetProperty("workflowType", out workflowTypeElement))
            {
                return false;
            }

            var workflowType = workflowTypeElement.GetString();
            return string.Equals(workflowType?.Trim(), notificationType.Trim(), StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private void RevertSettingToOriginal(string notificationType)
    {
        var original = _originalSettings.FirstOrDefault(x =>
            string.Equals(x.NotificationType, notificationType, StringComparison.OrdinalIgnoreCase));

        var updated = _settings.FirstOrDefault(x =>
            string.Equals(x.NotificationType, notificationType, StringComparison.OrdinalIgnoreCase));

        if (original is null || updated is null)
        {
            return;
        }

        updated.Enabled = original.Enabled;
        updated.ExecutionMode = original.ExecutionMode;
    }
}
