using Microsoft.AspNetCore.Mvc.RazorPages;
using SMS_Application.Interfaces;
using SMS_Domain.Enums;

namespace SMS.Presentation.Pages.Dashboard;

/// <summary>
/// System Dashboard for SMS Application Users (SuperAdmin/Admin)
/// Direct SMS Backend Integration - No Helper Services
/// </summary>
public class SystemModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<SystemModel> _logger;

    public SystemModel(IMediator mediator, ILogger<SystemModel> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    // Dashboard properties populated from SMS Backend
    public string? UserName { get; set; }
    public string? DisplayName { get; set; }
    public SMSUserType? UserType { get; set; }
    public string? PermissionLevel { get; set; }
    public bool IsAuthenticated { get; set; }

    // System metrics
    public int TotalUsers { get; set; }
    public int ActiveSessions { get; set; }
    public int SystemAlerts { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            // Get current user session data
            var userId = HttpContext.Session.GetString("SMS_UserId");
            var userTypeStr = HttpContext.Session.GetString("SMS_UserType");
            var displayName = HttpContext.Session.GetString("SMS_DisplayName");

            if (string.IsNullOrEmpty(userId))
            {
                // Redirect to login if no session
                Response.Redirect("/Account/Login");
                return;
            }

            // Verify user has system admin access
            var userType = SMSUserType.FromValue(userTypeStr ?? "");
            if (userType != SMSUserType.Application)
            {
                // Redirect non-admin users
                Response.Redirect("/Dashboard/Operations");
                return;
            }

            // Populate dashboard data
            IsAuthenticated = true;
            DisplayName = displayName ?? "Admin";
            UserType = userType;
            PermissionLevel = HttpContext.Session.GetString("SMS_PermissionLevel") ?? "Admin";

            // Load system metrics (placeholder data for now)
            TotalUsers = 5; // From your database data
            ActiveSessions = 1;
            SystemAlerts = 0;

            _logger.LogInformation("System Dashboard loaded for admin user: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading System Dashboard");
            Response.Redirect("/Account/Login");
        }
    }
}