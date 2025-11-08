using Microsoft.AspNetCore.Mvc.RazorPages;
using SMS_Application.Interfaces;
using SMS_Domain.Enums;

namespace SMS.Presentation.Pages.Dashboard;

/// <summary>
/// Stakeholder Dashboard for SMS Stakeholder Users  
/// Direct SMS Backend Integration - No Helper Services
/// </summary>
public class StakeholderModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<StakeholderModel> _logger;

    public StakeholderModel(IMediator mediator, ILogger<StakeholderModel> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    // Dashboard properties populated from SMS Backend
    public string? UserName { get; set; }
    public string? DisplayName { get; set; }
    public SMSUserType? UserType { get; set; }
    public string? Organization { get; set; }
    public string? StakeholderType { get; set; }
    public bool IsAuthenticated { get; set; }

    // Stakeholder metrics
    public int SubmittedReports { get; set; }
    public int AccessibleReports { get; set; }
    public int TrainingModules { get; set; }

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
                Response.Redirect("/Account/Login");
                return;
            }

            // Populate dashboard data
            IsAuthenticated = true;
            DisplayName = displayName ?? "User";
            UserType = SMSUserType.FromValue(userTypeStr ?? "");
            Organization = HttpContext.Session.GetString("SMS_Organization") ?? "Partner Organization";
            StakeholderType = HttpContext.Session.GetString("SMS_Department") ?? "Stakeholder";

            // Load stakeholder metrics (placeholder data)
            SubmittedReports = 0;
            AccessibleReports = 12;
            TrainingModules = 8;

            _logger.LogInformation("Stakeholder Dashboard loaded for user: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Stakeholder Dashboard");
            Response.Redirect("/Account/Login");
        }
    }
}