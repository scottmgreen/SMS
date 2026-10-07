using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using SMS_Application.Interfaces;
using SMS_Domain.Common;
using SMS_Domain.Entities;
using SMS_Domain.Enums;

namespace SMS3.Components.Pages.SMSSystem.Services;

public sealed class EmailNotificationSettingsService : IEmailNotificationSettingsService, IEmailNotificationExecutionModeResolver
{

    private static readonly IReadOnlyList<NotificationDefinition> NotificationDefinitions =
    [
        new(
            "MitigationApproval",
            "MitigationApprovalRequestedEvent",
            "NotificationEvents:MitigationApproval:Channels:Email",
            NotificationExecutionMode.Manual,
            "Email"),
        new(
            "MitigationApproved",
            "MitigationStatusChangedEvent",
            "NotificationEvents:MitigationApproved:Channels:Email",
            NotificationExecutionMode.Manual,
            "Email"),
        new(
            "MitigationTargetDateNotification",
            "Mitigation target-date scan trigger",
            "NotificationEvents:MitigationTargetDateNotification:Channels:Email",
            NotificationExecutionMode.Manual,
            "Email"),
        new(
            "HazardSubmissionStatusEscalation",
            "Report status escalation scan + API escalation",
            "NotificationEvents:HazardSubmissionStatusEscalation:Channels:Email",
            NotificationExecutionMode.Manual,
            "Email"),
        new(
            "UINotificationError",
            "UINotificationEvent severity Error",
            "NotificationEvents:UINotificationError:Channels:UI",
            NotificationExecutionMode.Immediate,
            "UI"),
        new(
            "UINotificationSuccess",
            "UINotificationEvent severity Success",
            "NotificationEvents:UINotificationSuccess:Channels:UI",
            NotificationExecutionMode.Immediate,
            "UI"),
        new(
            "UINotificationWarning",
            "UINotificationEvent severity Warning",
            "NotificationEvents:UINotificationWarning:Channels:UI",
            NotificationExecutionMode.Immediate,
            "UI"),
        new(
            "UINotificationInfo",
            "UINotificationEvent severity Info",
            "NotificationEvents:UINotificationInfo:Channels:UI",
            NotificationExecutionMode.Immediate,
            "UI"),
        new(
            "DomainEventPublishing",
            "Domain EventBus publish control",
            "NotificationEvents:DomainEventPublishing:Channels:Domain",
            NotificationExecutionMode.Immediate,
            "Domain")
    ];

    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<EmailNotificationSettingsService> _logger;

    public EmailNotificationSettingsService(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        ILogger<EmailNotificationSettingsService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IReadOnlyList<NotificationSetting> GetAllNotificationSettings()
    {
        return NotificationDefinitions
            .Select(definition => new NotificationSetting
            {
                NotificationType = definition.NotificationType,
                Enabled = ReadEnabled(definition),
                TriggerDomainEvent = definition.TriggerDomainEvent,
                AppSettingsSection = definition.SectionPath,
                ChannelName = definition.ChannelName,
                ExecutionMode = ReadExecutionMode(definition)
            })
            .ToList();
    }

    public bool IsEnabled(string? notificationType, bool defaultValue = true)
    {
        var normalizedNotificationType = notificationType?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedNotificationType))
        {
            return defaultValue;
        }

        var definition = NotificationDefinitions.FirstOrDefault(x =>
            string.Equals(x.NotificationType, normalizedNotificationType, StringComparison.OrdinalIgnoreCase));

        if (definition is null)
        {
            return defaultValue;
        }

        return ReadEnabled(definition);
    }

    public NotificationExecutionMode ResolveExecutionMode(string? notificationType, NotificationExecutionMode fallbackMode)
    {
        var normalizedNotificationType = notificationType?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedNotificationType))
        {
            return fallbackMode;
        }

        var definition = NotificationDefinitions.FirstOrDefault(x =>
            string.Equals(x.NotificationType, normalizedNotificationType, StringComparison.OrdinalIgnoreCase));

        if (definition is null)
        {
            return fallbackMode;
        }

        var configured = _configuration.GetValue<string>($"{definition.SectionPath}:ExecutionMode");
        return ParseAllowedMode(configured, definition.DefaultMode);
    }

    public async Task<Result> SaveNotificationSettingsAsync(IEnumerable<NotificationSetting> settings, CancellationToken cancellationToken = default)
    {
        var requestedSettings = settings?.ToList() ?? [];
        if (requestedSettings.Count == 0)
        {
            return Result.Success();
        }

        var appSettingsPath = Path.Combine(_environment.ContentRootPath, "appsettings.json");
        if (!File.Exists(appSettingsPath))
        {
            return Result.Failure(new SMS_Domain.Common.Error("EMAIL_NOTIFICATION_SETTINGS_FILE_NOT_FOUND", $"appsettings.json was not found at '{appSettingsPath}'."));
        }

        string appSettingsText;
        try
        {
            appSettingsText = await File.ReadAllTextAsync(appSettingsPath, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load appsettings.json for email workflow settings update.");
            return Result.Failure(new SMS_Domain.Common.Error("EMAIL_NOTIFICATION_SETTINGS_READ_FAILED", "Failed to read appsettings.json for email notification settings."));
        }

        var updatedAppSettingsText = appSettingsText;
        var updatedEntryCount = 0;

        foreach (var definition in NotificationDefinitions)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var requested = requestedSettings.FirstOrDefault(x =>
                string.Equals(x.NotificationType, definition.NotificationType, StringComparison.OrdinalIgnoreCase));

            if (requested is null)
            {
                continue;
            }

            var parsedMode = ParseAllowedMode(requested.ExecutionMode.ToString(), definition.DefaultMode);

            var replacementResult = ReplaceNotificationSettings(
                updatedAppSettingsText,
                definition.NotificationType,
                requested.Enabled,
                parsedMode.ToString(),
                definition.ChannelName);
            updatedAppSettingsText = replacementResult.UpdatedText;
            updatedEntryCount += replacementResult.ReplacementCount;
        }

        if (updatedEntryCount == 0)
        {
            return Result.Failure(new SMS_Domain.Common.Error(
                "EMAIL_NOTIFICATION_SETTINGS_SECTION_NOT_FOUND",
                "No NotificationEvents email execution mode entries were updated. Verify appsettings contains NotificationEvents entries for configured notification types."));
        }

        try
        {
            await File.WriteAllTextAsync(
                appSettingsPath,
                updatedAppSettingsText,
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist email notification settings to appsettings.json.");
            return Result.Failure(new SMS_Domain.Common.Error("EMAIL_NOTIFICATION_SETTINGS_WRITE_FAILED", "Failed to save email notification settings to appsettings.json."));
        }

        if (_configuration is IConfigurationRoot configurationRoot)
        {
            configurationRoot.Reload();
        }

        return Result.Success();
    }

    private static ReplacementResult ReplaceNotificationSettings(string appSettingsText, string notificationType, bool enabled, string executionMode, string channelName)
    {
        if (string.IsNullOrWhiteSpace(appSettingsText)
            || string.IsNullOrWhiteSpace(notificationType)
            || string.IsNullOrWhiteSpace(executionMode))
        {
            return new ReplacementResult(appSettingsText, 0);
        }

        var enabledReplacement = ReplaceNotificationProperty(
            appSettingsText,
            notificationType,
            "Enabled",
            enabled ? "true" : "false");

        var modeReplacement = ReplaceNotificationProperty(
            enabledReplacement.UpdatedText,
            notificationType,
            "ExecutionMode",
            executionMode,
            channelName);

        return new ReplacementResult(
            modeReplacement.UpdatedText,
            enabledReplacement.ReplacementCount + modeReplacement.ReplacementCount);
    }

    private static ReplacementResult ReplaceNotificationProperty(string appSettingsText, string notificationType, string propertyName, string propertyValue, string channelName = "Email")
    {
        if (string.IsNullOrWhiteSpace(appSettingsText)
            || string.IsNullOrWhiteSpace(notificationType)
            || string.IsNullOrWhiteSpace(propertyName)
            || string.IsNullOrWhiteSpace(propertyValue))
        {
            return new ReplacementResult(appSettingsText, 0);
        }

        string pattern;
        if (string.Equals(propertyName, "Enabled", StringComparison.OrdinalIgnoreCase))
        {
            pattern = $"(\"{Regex.Escape(notificationType)}\"\\s*:\\s*\\{{[\\s\\S]*?\"Enabled\"\\s*:\\s*)(true|false)";
        }
        else
        {
            pattern = $"(\"{Regex.Escape(notificationType)}\"\\s*:\\s*\\{{[\\s\\S]*?\"Channels\"\\s*:\\s*\\{{[\\s\\S]*?\"{Regex.Escape(channelName)}\"\\s*:\\s*\\{{[\\s\\S]*?\"{Regex.Escape(propertyName)}\"\\s*:\\s*\")[^\"]+(\")";
        }

        var regex = new Regex(pattern, RegexOptions.Multiline);
        var replacementCount = 0;
        var updatedText = regex.Replace(
            appSettingsText,
            match =>
            {
                replacementCount++;
                if (string.Equals(propertyName, "Enabled", StringComparison.OrdinalIgnoreCase))
                {
                    return $"{match.Groups[1].Value}{propertyValue}";
                }

                return $"{match.Groups[1].Value}{propertyValue}{match.Groups[2].Value}";
            });

        return new ReplacementResult(updatedText, replacementCount);
    }

    private NotificationExecutionMode ReadExecutionMode(NotificationDefinition definition)
    {
        var configured = _configuration.GetValue<string>($"{definition.SectionPath}:ExecutionMode");
        return ParseAllowedMode(configured, definition.DefaultMode);
    }

    private bool ReadEnabled(NotificationDefinition definition)
    {
        return _configuration.GetValue<bool?>(definition.EnabledSectionPath) ?? definition.DefaultEnabled;
    }

    private static NotificationExecutionMode ParseAllowedMode(string? configured, NotificationExecutionMode defaultMode)
    {
        if (!Enum.TryParse<NotificationExecutionMode>(configured, true, out var parsedMode))
        {
            return defaultMode;
        }

        return parsedMode is NotificationExecutionMode.Immediate or NotificationExecutionMode.Manual
            ? parsedMode
            : defaultMode;
    }

    private sealed record NotificationDefinition(
        string NotificationType,
        string TriggerDomainEvent,
        string SectionPath,
        NotificationExecutionMode DefaultMode,
        string ChannelName,
        bool DefaultEnabled = true)
    {
        public string EnabledSectionPath => $"NotificationEvents:{NotificationType}:Enabled";
    }

    private sealed record ReplacementResult(string UpdatedText, int ReplacementCount);
}
