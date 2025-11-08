using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SMS.Presentation.Pages.Account;

/// <summary>
/// Direct SMS Backend Logout - No Helper Services
/// Simple session clearing for SMS users
/// </summary>
public class LogoutPageModel : PageModel
{
    private readonly ILogger<LogoutPageModel> _logger;

    public LogoutPageModel(ILogger<LogoutPageModel> logger)
    {
        _logger = logger;
    }

    public IActionResult OnGet()
    {
        return PerformLogout();
    }

    public IActionResult OnPost()
    {
        return PerformLogout();
    }

    private IActionResult PerformLogout()
    {
        // Get user info before clearing session
        var userId = HttpContext.Session.GetString("SMS_UserId");
        var userType = HttpContext.Session.GetString("SMS_UserType");

        _logger.LogInformation("SMS logout initiated for user: {UserId} ({UserType})", userId, userType);

        // Clear SMS session data directly
        HttpContext.Session.Clear();

        _logger.LogInformation("SMS logout completed for user: {UserId}", userId);

        TempData["InfoMessage"] = "You have been successfully logged out of the SMS Portal.";

        return RedirectToPage("/Account/Login");
    }
}

/*
DIRECT SMS BACKEND LOGOUT - NO HELPER SERVICES

FEATURES:
1. Direct SMS session clearing without helper services
2. Comprehensive logout logging
3. User-friendly confirmation message
4. Automatic redirect to login
5. Handles both GET and POST requests

SECURITY:
? Complete SMS session data removal
? Proper audit logging
? Clean session management

INTEGRATION:
? Directly integrates with SMS backend
? Consistent with SMS session management patterns
*/