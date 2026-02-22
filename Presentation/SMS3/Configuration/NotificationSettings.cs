namespace SMS3.Configuration;

/// <summary>
/// Configuration settings for controlling notification display
/// </summary>
public class NotificationSettings
{
    public const string SectionName = "NotificationSettings";
    
    /// <summary>
    /// Allow error notifications to be displayed
    /// </summary>
    public bool AllowErrorNotifications { get; set; } = true;
    
    /// <summary>
    /// Allow success notifications to be displayed
    /// </summary>
    public bool AllowSuccessNotifications { get; set; } = true;
    
    /// <summary>
    /// Allow warning notifications to be displayed
    /// </summary>
    public bool AllowWarningNotifications { get; set; } = true;
    
    /// <summary>
    /// Allow info notifications to be displayed
    /// </summary>
    public bool AllowInfoNotifications { get; set; } = true;
}