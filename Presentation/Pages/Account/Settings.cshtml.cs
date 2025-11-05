using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PDXSMS_Presentation.Pages.Account;

public class SettingsModel : PageModel
{
    [BindProperty]
    public UserSettingsDto Settings { get; set; } = new();

    public List<SelectListItem> ThemeOptions { get; set; } = new();
    public List<SelectListItem> LanguageOptions { get; set; } = new();
    public List<SelectListItem> DateFormatOptions { get; set; } = new();
    public List<SelectListItem> TimeZoneOptions { get; set; } = new();
    public List<SelectListItem> ItemsPerPageOptions { get; set; } = new();
    public List<SelectListItem> SessionTimeoutOptions { get; set; } = new();

    public void OnGet()
    {
        LoadUserSettings();
        LoadSelectOptions();
    }

    public async Task<IActionResult> OnPostUpdateDisplaySettingsAsync()
    {
        try
        {
            await SaveDisplaySettingsAsync();
            TempData["SuccessMessage"] = "Display settings updated successfully!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error updating display settings: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateDashboardSettingsAsync()
    {
        try
        {
            await SaveDashboardSettingsAsync();
            TempData["SuccessMessage"] = "Dashboard preferences updated successfully!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error updating dashboard settings: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateSecuritySettingsAsync()
    {
        try
        {
            await SaveSecuritySettingsAsync();
            TempData["SuccessMessage"] = "Security settings updated successfully!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error updating security settings: {ex.Message}";
        }

        return RedirectToPage();
    }

    private void LoadUserSettings()
    {
        var userId = HttpContext.Session.GetString("UserId") ?? "";
        
        // Load from session or database (mock data for now)
        Settings = new UserSettingsDto
        {
            UserId = userId,
            Theme = "Light",
            Language = "English",
            DateFormat = "MM/dd/yyyy",
            TimeZone = "Pacific Standard Time",
            ShowHelpTips = true,
            ShowActiveHazards = true,
            ShowPendingReviews = true,
            ShowRecentActivity = true,
            ShowQuickActions = true,
            ShowSystemStatus = true,
            ShowPerformanceMetrics = false,
            ItemsPerPage = "25",
            TwoFactorEnabled = false,
            LogSecurityEvents = true,
            SessionTimeout = "2 hours"
        };
    }

    private void LoadSelectOptions()
    {
        ThemeOptions = new List<SelectListItem>
        {
            new("Light Theme", "Light"),
            new("Dark Theme", "Dark"),
            new("High Contrast", "HighContrast"),
            new("System Default", "System")
        };

        LanguageOptions = new List<SelectListItem>
        {
            new("English", "English"),
            new("Spanish", "Spanish"),
            new("French", "French")
        };

        DateFormatOptions = new List<SelectListItem>
        {
            new("MM/dd/yyyy (12/31/2024)", "MM/dd/yyyy"),
            new("dd/MM/yyyy (31/12/2024)", "dd/MM/yyyy"),
            new("yyyy-MM-dd (2024-12-31)", "yyyy-MM-dd"),
            new("MMM dd, yyyy (Dec 31, 2024)", "MMM dd, yyyy")
        };

        TimeZoneOptions = new List<SelectListItem>
        {
            new("Pacific Standard Time (PST)", "Pacific Standard Time"),
            new("Mountain Standard Time (MST)", "Mountain Standard Time"),
            new("Central Standard Time (CST)", "Central Standard Time"),
            new("Eastern Standard Time (EST)", "Eastern Standard Time"),
            new("Coordinated Universal Time (UTC)", "UTC")
        };

        ItemsPerPageOptions = new List<SelectListItem>
        {
            new("10 items", "10"),
            new("25 items", "25"),
            new("50 items", "50"),
            new("100 items", "100")
        };

        SessionTimeoutOptions = new List<SelectListItem>
        {
            new("30 minutes", "30 minutes"),
            new("1 hour", "1 hour"),
            new("2 hours", "2 hours"),
            new("4 hours", "4 hours"),
            new("8 hours", "8 hours")
        };
    }

    private async Task SaveDisplaySettingsAsync()
    {
        // Mock save - in real implementation, save to database or user preferences
        await Task.Delay(200);
        
        // Store some key settings in session for immediate effect
        HttpContext.Session.SetString("UserTheme", Settings.Theme);
        HttpContext.Session.SetString("UserLanguage", Settings.Language);
    }

    private async Task SaveDashboardSettingsAsync()
    {
        // Mock save - in real implementation, save to database
        await Task.Delay(200);
        
        // Store dashboard preferences in session - simplified without JSON serialization
        HttpContext.Session.SetString("ShowActiveHazards", Settings.ShowActiveHazards.ToString());
        HttpContext.Session.SetString("ShowPendingReviews", Settings.ShowPendingReviews.ToString());
        HttpContext.Session.SetString("ShowRecentActivity", Settings.ShowRecentActivity.ToString());
        HttpContext.Session.SetString("ItemsPerPage", Settings.ItemsPerPage);
    }

    private async Task SaveSecuritySettingsAsync()
    {
        // Mock save - in real implementation, save to database and implement actual 2FA
        await Task.Delay(200);
        
        // Store security settings - simplified
        HttpContext.Session.SetString("TwoFactorEnabled", Settings.TwoFactorEnabled.ToString());
        HttpContext.Session.SetString("SessionTimeout", Settings.SessionTimeout);
    }
}

public class UserSettingsDto
{
    public string UserId { get; set; } = "";
    
    // Display Settings
    public string Theme { get; set; } = "Light";
    public string Language { get; set; } = "English";
    public string DateFormat { get; set; } = "MM/dd/yyyy";
    public string TimeZone { get; set; } = "Pacific Standard Time";
    public bool ShowHelpTips { get; set; } = true;
    
    // Dashboard Settings
    public bool ShowActiveHazards { get; set; } = true;
    public bool ShowPendingReviews { get; set; } = true;
    public bool ShowRecentActivity { get; set; } = true;
    public bool ShowQuickActions { get; set; } = true;
    public bool ShowSystemStatus { get; set; } = true;
    public bool ShowPerformanceMetrics { get; set; } = false;
    public string ItemsPerPage { get; set; } = "25";
    
    // Security Settings
    public bool TwoFactorEnabled { get; set; } = false;
    public bool LogSecurityEvents { get; set; } = true;
    public string SessionTimeout { get; set; } = "2 hours";
}