using SMS_Domain.Enums;

namespace SMS_Domain.Entities;

public sealed class NotificationSetting
{
    public string NotificationType { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;

    public string TriggerDomainEvent { get; set; } = string.Empty;

    public string AppSettingsSection { get; set; } = string.Empty;

    public string ChannelName { get; set; } = string.Empty;

    public NotificationExecutionMode ExecutionMode { get; set; } = NotificationExecutionMode.Manual;
}
