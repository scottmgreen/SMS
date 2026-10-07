using SMS_Application.Interfaces;

namespace SMS3.Components.Pages.SMSSystem.Models;

public sealed class EmailNotificationSetting
{
    public string NotificationType { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;

    public string TriggerDomainEvent { get; set; } = string.Empty;

    public string AppSettingsSection { get; set; } = string.Empty;

    public EventExecutionMode ExecutionMode { get; set; } = EventExecutionMode.Manual;
}
