using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Queries;
using SMS_Domain.Entities;
using SMS_Domain.Enums;

namespace SMS.Presentation.Pages;

/// <summary>
/// Index Model - SMS Backend Integration (Preserving your layout/styling)
/// ONLY fixing the backend code, NOT touching your UI layout
/// </summary>
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    // Properties for your existing layout (keeping same names)
    public bool IsAuthenticated { get; set; }
    public string? DisplayName { get; set; }
    public SMSUserType? UserType { get; set; }
    public string? Department { get; set; }
    public string? Organization { get; set; }
    public string? AccessLevel { get; set; }

    // Dashboard stats to match your layout expectations
    public DashboardStats DashboardStats { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        // CRITICAL SECURITY: Prevent guest users from accessing the dashboard
        var isGuest = HttpContext.Session.GetString("IsGuest") == "true";
        
        if (isGuest)
        {
            _logger.LogWarning("SECURITY VIOLATION: Guest user attempting to access dashboard. Clearing session and redirecting to login.");
            HttpContext.Session.Clear();
            return RedirectToPage("/Account/Login");
        }

        ViewData["Title"] = "PDXSMS - Safety Management System Dashboard";

        try
        {
            var userId = HttpContext.Session.GetString("SMS_UserId");
            var userTypeStr = HttpContext.Session.GetString("SMS_UserType");

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userTypeStr))
            {
                // Anonymous user
                IsAuthenticated = false;
                DisplayName = "Guest";
                _logger.LogInformation("Index page loaded for anonymous user");
                return Page();
            }

            // Authenticated user
            var userType = SMSUserType.FromValue(userTypeStr);
            if (userType == null)
            {
                IsAuthenticated = false;
                DisplayName = "Guest";
                return Page();
            }

            var user = await GetUserFromSMSBackendAsync(userId, userType);
            if (user == null)
            {
                IsAuthenticated = false;
                DisplayName = "Guest";
                HttpContext.Session.Clear();
                return Page();
            }

            // Populate data for your existing layout
            IsAuthenticated = true;
            DisplayName = user.DisplayName;
            UserType = userType;
            
            PopulateUserTypeSpecificData(user, userType);
            PopulateDashboardStats(); // Mock data for your dashboard

            _logger.LogInformation("Index page loaded for user: {UserId} ({UserType})", userId, userType.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading index page");
            IsAuthenticated = false;
            DisplayName = "Guest";
        }

        return Page();
    }

    private async Task<BaseUser?> GetUserFromSMSBackendAsync(string userId, SMSUserType userType)
    {
        try
        {
            if (userType == SMSUserType.Application)
            {
                var query = new GetSMSApplicationUserByCodeQuery(userId);
                var result = await _mediator.SendAsync(query, CancellationToken.None);
                return result.IsSuccess ? result.Value : null;
            }
            else if (userType == SMSUserType.Organizational)
            {
                var query = new GetSMSOrganizationalUserByIdQuery(userId);
                var result = await _mediator.SendAsync(query, CancellationToken.None);
                return result.IsSuccess ? result.Value : null;
            }
            else if (userType == SMSUserType.Stakeholder)
            {
                var query = new GetSMSStakeholderUserByCodeQuery(userId);
                var result = await _mediator.SendAsync(query, CancellationToken.None);
                return result.IsSuccess ? result.Value : null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get user from SMS Backend: {UserId}", userId);
        }

        return null;
    }

    private void PopulateUserTypeSpecificData(BaseUser user, SMSUserType userType)
    {
        switch (userType)
        {
            case var type when type == SMSUserType.Application && user is SMSApplicationUser appUser:
                Department = "IT Administration";
                AccessLevel = GetAccessLevelFromPermissions(user.UserRole);
                break;

            case var type when type == SMSUserType.Organizational && user is SMSOrganizationalUser orgUser:
                Department = orgUser.Department ?? "Unknown Department";
                AccessLevel = GetAccessLevelFromPermissions(user.UserRole) ?? orgUser.OrganizationLevel ?? "No Role Assigned";
                break;

            case var type when type == SMSUserType.Stakeholder && user is SMSStakeholderUser stakeholderUser:
                Organization = stakeholderUser.Organization ?? "Unknown Organization";
                Department = stakeholderUser.StakeholderType ?? "Unknown Stakeholder Type";
                AccessLevel = GetAccessLevelFromPermissions(user.UserRole);
                break;

            default:
                Department = "Unknown";
                AccessLevel = "Unknown";
                Organization = "Unknown";
                break;
        }
    }

    private string GetAccessLevelFromPermissions(SMSUserRole userRole)
    {
        if (userRole?.Permissions == null || !userRole.Permissions.Any())
            return "No Permissions";

        var hasCreate = userRole.Permissions.Any(p => p.Create);
        var hasUpdate = userRole.Permissions.Any(p => p.Update);
        var hasDelete = userRole.Permissions.Any(p => p.Delete);
        var hasRead = userRole.Permissions.Any(p => p.Read);

        if (hasCreate && hasUpdate && hasDelete)
            return "Full Access";
        else if (hasCreate && hasUpdate)
            return "Write Access";
        else if (hasRead)
            return "Read Only";
        else
            return "Limited Access";
    }

    private void PopulateDashboardStats()
    {
        // Mock dashboard stats to work with your existing layout
        DashboardStats = new DashboardStats
        {
            TotalReports = 42,
            ActiveInvestigations = 3,
            PendingReviews = 7,
            RecentAlerts = 2,
            // Properties your layout expects
            ActiveHazards = 5,
            ActiveAudits = 2,
            MitigationsUnderImplementation = 8,
            RecentActivityCount = 12
        };
    }
}

/// <summary>
/// Simple dashboard stats class to support your existing layout
/// </summary>
public class DashboardStats
{
    public int TotalReports { get; set; }
    public int ActiveInvestigations { get; set; }
    public int PendingReviews { get; set; }
    public int RecentAlerts { get; set; }
    
    // Properties your Index.cshtml expects
    public int ActiveHazards { get; set; }
    public int ActiveAudits { get; set; }
    public int MitigationsUnderImplementation { get; set; }
    public int RecentActivityCount { get; set; }
}