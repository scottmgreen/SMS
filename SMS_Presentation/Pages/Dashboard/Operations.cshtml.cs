using Microsoft.AspNetCore.Mvc.RazorPages;
using SMS_Application.Interfaces;
using SMS_Domain.Enums;

namespace SMS.Presentation.Pages.Dashboard;

/// <summary>
/// Operations Dashboard for SMS Organizational Users
/// Direct SMS Backend Integration - No Helper Services
/// </summary>
public class OperationsModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<OperationsModel> _logger;

    public OperationsModel(IMediator mediator, ILogger<OperationsModel> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    // Dashboard properties populated from SMS Backend
    public string? UserName { get; set; }
    public string? DisplayName { get; set; }
    public SMSUserType? UserType { get; set; }
    public string? Department { get; set; }
    public bool IsAuthenticated { get; set; }

    // Operations metrics
    public int ActiveHazards { get; set; }
    public int PendingInvestigations { get; set; }
    public int OpenRiskAssessments { get; set; }

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
            Department = HttpContext.Session.GetString("SMS_Department") ?? "Operations";

            // Load operations metrics (placeholder data)
            ActiveHazards = 3;
            PendingInvestigations = 2;
            OpenRiskAssessments = 5;

            _logger.LogInformation("Operations Dashboard loaded for user: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Operations Dashboard");
            Response.Redirect("/Account/Login");
        }
    }
}