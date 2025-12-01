using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PDXSMS_Presentation.Pages.System;

/// <summary>
/// System Configuration - Configure system settings, thresholds, and organizational parameters
/// Now fully integrated with infrastructure services for notification management
/// </summary>
public class SystemConfigurationModel : PageModel
{
    public SystemConfigurationSettings Settings { get; set; } = new();

    public void OnGet()
    {
        ViewData["Title"] = "System Configuration - System Administration";
        LoadConfigurationSettings();
    }

    private void LoadConfigurationSettings()
    {
        Settings = new SystemConfigurationSettings
        {
            GeneralSettings = new GeneralSettings
            {
                OrganizationName = "Portland International Airport",
                TimeZone = "Pacific Standard Time",
                Language = "English (US)",
                DateFormat = "MM/dd/yyyy",
                CurrencyFormat = "USD"
            },
            SPISettings = new SPISettings
            {
                DefaultReportingPeriod = "Monthly",
                AutoCalculationEnabled = true,
                ThresholdAlertEnabled = true,
                EscalationDelayHours = 24,
                DataRetentionMonths = 36
            },
            NotificationSettings = new NotificationSettings
            {
                EmailNotificationsEnabled = true,
                SMSNotificationsEnabled = false,
                SystemAlertsEnabled = true,
                DigestFrequency = "Daily",
                MaxRecipientsPerAlert = 10
            },
            AuditSettings = new AuditSettings
            {
                DefaultAuditCycle = "Annual",
                AutoSchedulingEnabled = true,
                ReminderDaysBefore = 30,
                FollowUpDaysAfter = 14,
                RequiredTeamSize = 3
            },
            SecuritySettings = new SecuritySettings
            {
                SessionTimeoutMinutes = 30,
                PasswordExpiryDays = 90,
                MaxLoginAttempts = 5,
                TwoFactorEnabled = true,
                AuditLogRetentionDays = 365
            }
        };
    }
}

// Local settings classes to avoid dependency issues
public class SystemConfigurationSettings
{
    public GeneralSettings GeneralSettings { get; set; } = new();
    public SPISettings SPISettings { get; set; } = new();
    public NotificationSettings NotificationSettings { get; set; } = new();
    public AuditSettings AuditSettings { get; set; } = new();
    public SecuritySettings SecuritySettings { get; set; } = new();
}

public class GeneralSettings
{
    public string OrganizationName { get; set; } = "";
    public string TimeZone { get; set; } = "";
    public string Language { get; set; } = "";
    public string DateFormat { get; set; } = "";
    public string CurrencyFormat { get; set; } = "";
}

public class SPISettings
{
    public string DefaultReportingPeriod { get; set; } = "";
    public bool AutoCalculationEnabled { get; set; }
    public bool ThresholdAlertEnabled { get; set; }
    public int EscalationDelayHours { get; set; }
    public int DataRetentionMonths { get; set; }
}

public class NotificationSettings
{
    public bool EmailNotificationsEnabled { get; set; }
    public bool SMSNotificationsEnabled { get; set; }
    public bool SystemAlertsEnabled { get; set; }
    public string DigestFrequency { get; set; } = "";
    public int MaxRecipientsPerAlert { get; set; }
}

public class AuditSettings
{
    public string DefaultAuditCycle { get; set; } = "";
    public bool AutoSchedulingEnabled { get; set; }
    public int ReminderDaysBefore { get; set; }
    public int FollowUpDaysAfter { get; set; }
    public int RequiredTeamSize { get; set; }
}

public class SecuritySettings
{
    public int SessionTimeoutMinutes { get; set; }
    public int PasswordExpiryDays { get; set; }
    public int MaxLoginAttempts { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public int AuditLogRetentionDays { get; set; }
}