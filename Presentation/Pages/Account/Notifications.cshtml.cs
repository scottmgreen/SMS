using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PDXSMS_Presentation.Pages.Account;

public class NotificationsModel : PageModel
{
    [BindProperty]
    public NotificationPreferencesDto Preferences { get; set; } = new();

    public List<NotificationDto> Notifications { get; set; } = new();
    public int UnreadCount { get; set; }
    public int TotalNotifications { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public List<NotificationStatDto> NotificationStats { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string FilterType { get; set; } = "All";

    [BindProperty(SupportsGet = true)]
    public string FilterStatus { get; set; } = "All";

    [BindProperty(SupportsGet = true)]
    public string SearchTerm { get; set; } = "";

    public List<SelectListItem> NotificationTypeOptions { get; set; } = new();
    public List<SelectListItem> StatusOptions { get; set; } = new();
    public List<SelectListItem> FrequencyOptions { get; set; } = new();

    public void OnGet(int page = 1)
    {
        CurrentPage = page;
        LoadNotifications();
        LoadNotificationPreferences();
        LoadSelectOptions();
        LoadNotificationStats();
    }

    public async Task<IActionResult> OnPostMarkReadAsync(string notificationId)
    {
        try
        {
            await MarkNotificationAsReadAsync(notificationId);
            TempData["SuccessMessage"] = "Notification marked as read.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMarkAllReadAsync()
    {
        try
        {
            await MarkAllNotificationsAsReadAsync();
            TempData["SuccessMessage"] = "All notifications marked as read.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteNotificationAsync(string notificationId)
    {
        try
        {
            await DeleteNotificationAsync(notificationId);
            TempData["SuccessMessage"] = "Notification deleted.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdatePreferencesAsync()
    {
        try
        {
            await SaveNotificationPreferencesAsync();
            TempData["SuccessMessage"] = "Notification preferences updated successfully!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error updating preferences: {ex.Message}";
        }

        return RedirectToPage();
    }

    private void LoadNotifications()
    {
        var userId = HttpContext.Session.GetString("UserId") ?? "";
        var userRole = HttpContext.Session.GetString("UserRole") ?? "";

        // Generate mock notifications based on user role
        var allNotifications = GenerateMockNotifications(userRole);

        // Apply filters
        if (FilterType != "All")
        {
            allNotifications = allNotifications.Where(n => n.Type.Equals(FilterType, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (FilterStatus == "Unread")
        {
            allNotifications = allNotifications.Where(n => !n.IsRead).ToList();
        }
        else if (FilterStatus == "Read")
        {
            allNotifications = allNotifications.Where(n => n.IsRead).ToList();
        }

        if (!string.IsNullOrEmpty(SearchTerm))
        {
            allNotifications = allNotifications.Where(n => 
                n.Title.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                n.Message.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        TotalNotifications = allNotifications.Count;
        UnreadCount = allNotifications.Count(n => !n.IsRead);

        // Pagination
        const int pageSize = 10;
        TotalPages = (int)Math.Ceiling((double)TotalNotifications / pageSize);
        var skip = (CurrentPage - 1) * pageSize;

        Notifications = allNotifications
            .OrderByDescending(n => n.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToList();
    }

    private List<NotificationDto> GenerateMockNotifications(string userRole)
    {
        var notifications = new List<NotificationDto>();
        var random = new Random();

        // Generate different notifications based on user role
        var baseNotifications = new List<(string Title, string Message, string Type, string Priority, bool IsRead, DateTime CreatedAt, string ActionUrl)>
        {
            ("New Hazard Report Submitted", "Hazard Report H-2024-0156 has been submitted for your review.", "Hazard", "High", false, DateTime.Now.AddMinutes(-15), "/SafetyRiskManagement/HazardProcessing"),
            ("Risk Assessment Due Soon", "Risk Assessment RA-2024-0023 is due for completion within 2 days.", "Deadline", "Medium", false, DateTime.Now.AddHours(-2), "/SafetyRiskManagement/RiskAssessment"),
            ("System Maintenance Scheduled", "PDXSMS will undergo maintenance on Dec 15, 2024 from 2:00-4:00 AM PST.", "System", "Low", true, DateTime.Now.AddDays(-1), ""),
            ("Audit Finding Updated", "Audit Finding AF-2024-0089 has been updated with new corrective actions.", "Audit", "Medium", false, DateTime.Now.AddHours(-4), "/SafetyAssurance/SafetyAudits"),
            ("Training Reminder", "Your annual SMS training is due by December 31, 2024.", "Training", "Medium", true, DateTime.Now.AddDays(-2), "/SafetyPromotion/SafetyTraining"),
            ("Welcome to PDXSMS", "Welcome! Complete your profile setup to get started with the system.", "Welcome", "Low", true, DateTime.Now.AddDays(-7), "/Account/Profile")
        };

        // Add role-specific notifications
        if (userRole == "Administrator")
        {
            baseNotifications.AddRange(new[]
            {
                ("User Account Created", "New user account created for jane.doe@flypdx.com.", "Admin", "Low", false, DateTime.Now.AddHours(-1), "/System/UserManagement"),
                ("System Backup Completed", "Daily system backup completed successfully at 3:00 AM.", "System", "Low", true, DateTime.Now.AddHours(-8), "/System/SystemMonitoring"),
                ("Security Alert", "Multiple failed login attempts detected for user account.", "Security", "High", false, DateTime.Now.AddMinutes(-45), "/System/SystemMonitoring")
            });
        }

        if (userRole == "Safety Manager" || userRole == "Administrator")
        {
            baseNotifications.AddRange(new[]
            {
                ("Monthly Safety Report Ready", "The monthly safety performance report is ready for review.", "Report", "Medium", false, DateTime.Now.AddHours(-3), "/SafetyAssurance/SafetyAssuranceReporting"),
                ("High Priority Hazard", "Critical hazard reported at Gate A12 - immediate attention required.", "Hazard", "High", false, DateTime.Now.AddMinutes(-30), "/SafetyRiskManagement/HazardProcessing")
            });
        }

        // Convert to DTOs with proper styling
        foreach (var (title, message, type, priority, isRead, createdAt, actionUrl) in baseNotifications)
        {
            notifications.Add(new NotificationDto
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                Message = message,
                Type = type,
                Priority = priority,
                IsRead = isRead,
                CreatedAt = createdAt,
                ActionUrl = actionUrl,
                IconClass = GetIconClass(type),
                IconColor = GetIconColor(type),
                TypeBadgeClass = GetTypeBadgeClass(type)
            });
        }

        return notifications;
    }

    private string GetIconClass(string type) => type switch
    {
        "Hazard" => "fa-exclamation-triangle",
        "Deadline" => "fa-clock",
        "System" => "fa-server",
        "Audit" => "fa-clipboard-check",
        "Training" => "fa-graduation-cap",
        "Welcome" => "fa-hand-wave",
        "Admin" => "fa-user-cog",
        "Security" => "fa-shield-alt",
        "Report" => "fa-chart-bar",
        _ => "fa-bell"
    };

    private string GetIconColor(string type) => type switch
    {
        "Hazard" => "text-danger",
        "Deadline" => "text-warning",
        "System" => "text-info",
        "Audit" => "text-primary",
        "Training" => "text-success",
        "Welcome" => "text-info",
        "Admin" => "text-secondary",
        "Security" => "text-danger",
        "Report" => "text-success",
        _ => "text-primary"
    };

    private string GetTypeBadgeClass(string type) => type switch
    {
        "Hazard" => "bg-danger",
        "Deadline" => "bg-warning",
        "System" => "bg-info",
        "Audit" => "bg-primary",
        "Training" => "bg-success",
        "Welcome" => "bg-info",
        "Admin" => "bg-secondary",
        "Security" => "bg-danger",
        "Report" => "bg-success",
        _ => "bg-primary"
    };

    private void LoadNotificationPreferences()
    {
        var userId = HttpContext.Session.GetString("UserId") ?? "";
        
        // Load from session or database (mock data for now)
        Preferences = new NotificationPreferencesDto
        {
            UserId = userId,
            EmailHazards = true,
            EmailAssignments = true,
            EmailDeadlines = true,
            EmailSystem = false,
            BrowserHazards = true,
            BrowserAssignments = true,
            BrowserMentions = true,
            EmailFrequency = "Immediate"
        };
    }

    private void LoadSelectOptions()
    {
        NotificationTypeOptions = new List<SelectListItem>
        {
            new("All Types", "All"),
            new("Hazard Reports", "Hazard"),
            new("Deadlines", "Deadline"),
            new("System Updates", "System"),
            new("Audit Findings", "Audit"),
            new("Training", "Training"),
            new("Security Alerts", "Security")
        };

        StatusOptions = new List<SelectListItem>
        {
            new("All Notifications", "All"),
            new("Unread Only", "Unread"),
            new("Read Only", "Read")
        };

        FrequencyOptions = new List<SelectListItem>
        {
            new("Immediate", "Immediate"),
            new("Daily Digest", "Daily"),
            new("Weekly Summary", "Weekly"),
            new("Never", "Never")
        };
    }

    private void LoadNotificationStats()
    {
        var allNotifications = GenerateMockNotifications(HttpContext.Session.GetString("UserRole") ?? "");
        
        NotificationStats = allNotifications
            .GroupBy(n => n.Type)
            .Select(g => new NotificationStatDto
            {
                Type = g.Key,
                Count = g.Count(),
                BadgeClass = g.Any(n => !n.IsRead) ? "badge bg-danger" : "badge bg-secondary"
            })
            .OrderByDescending(s => s.Count)
            .Take(5)
            .ToList();
    }

    private async Task MarkNotificationAsReadAsync(string notificationId)
    {
        // Mock implementation - in real app, update database
        await Task.Delay(100);
    }

    private async Task MarkAllNotificationsAsReadAsync()
    {
        // Mock implementation - in real app, update database
        await Task.Delay(200);
    }

    private async Task DeleteNotificationAsync(string notificationId)
    {
        // Mock implementation - in real app, delete from database
        await Task.Delay(100);
    }

    private async Task SaveNotificationPreferencesAsync()
    {
        // Mock implementation - in real app, save to database
        await Task.Delay(200);
    }
}

public class NotificationDto
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public string Type { get; set; } = "";
    public string Priority { get; set; } = "";
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ActionUrl { get; set; } = "";
    public string IconClass { get; set; } = "";
    public string IconColor { get; set; } = "";
    public string TypeBadgeClass { get; set; } = "";
}

public class NotificationPreferencesDto
{
    public string UserId { get; set; } = "";
    public bool EmailHazards { get; set; }
    public bool EmailAssignments { get; set; }
    public bool EmailDeadlines { get; set; }
    public bool EmailSystem { get; set; }
    public bool BrowserHazards { get; set; }
    public bool BrowserAssignments { get; set; }
    public bool BrowserMentions { get; set; }
    public string EmailFrequency { get; set; } = "Immediate";
}

public class NotificationStatDto
{
    public string Type { get; set; } = "";
    public int Count { get; set; }
    public string BadgeClass { get; set; } = "";
}