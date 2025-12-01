using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Queries;
using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Shared.Common;

namespace SMS.Presentation.Pages.Account;

/// <summary>
/// Direct SMS Backend Integration - No Helper Models
/// Uses ONLY SMS Mediator/CQRS from Application layer
/// </summary>
public class Login : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<Login> _logger;

    public Login(IMediator mediator, ILogger<Login> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public string ReturnUrl { get; set; } = string.Empty;

    public string ErrorMessage { get; set; } = string.Empty;

    public void OnGet(string returnUrl = "")
    {
        ReturnUrl = returnUrl ?? "";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Email and password are required.";
            return Page();
        }

        try
        {
            _logger.LogInformation("Direct SMS Backend authentication for: {Email}", Email);

            // DIRECT SMS Backend call - Try each user type through CQRS
            var (user, userType) = await AuthenticateWithSMSBackendAsync(Email, Password);

            if (user == null)
            {
                ErrorMessage = "Invalid email or password.";
                return Page();
            }

            // Create session with SMS Domain Entity data directly
            CreateSMSSession(user, userType);

            // **CRITICAL FIX**: Ensure session is saved before redirect
            await HttpContext.Session.CommitAsync();

            _logger.LogInformation("SMS Backend authentication successful: {UserId} ({UserType})",
                user.Code, userType.Name);

            // Redirect based on SMS User Type
            var redirectUrl = GetDashboardUrl(userType);
            
            // **ENHANCED REDIRECT LOGIC WITH MULTIPLE FALLBACKS**
            try
            {
                if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                {
                    _logger.LogInformation("Redirecting to return URL: {ReturnUrl}", ReturnUrl);
                    return LocalRedirect(ReturnUrl);
                }
                else
                {
                    _logger.LogInformation("Redirecting to dashboard: {DashboardUrl}", redirectUrl);
                    
                    // Try multiple redirect approaches
                    if (redirectUrl == "/Index")
                    {
                        return RedirectToPage("/Index");
                    }
                    else if (redirectUrl.StartsWith("/Dashboard/"))
                    {
                        var pageName = redirectUrl.Replace("/Dashboard/", "");
                        return RedirectToPage("/Dashboard/" + pageName);
                    }
                    else
                    {
                        return LocalRedirect(redirectUrl);
                    }
                }
            }
            catch (Exception redirectEx)
            {
                _logger.LogError(redirectEx, "Redirect failed, trying fallback to Index");
                return RedirectToPage("/Index");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMS Backend authentication error for: {Email}", Email);
            ErrorMessage = "An error occurred during login. Please try again.";
            return Page();
        }
    }

    /// <summary>
    /// Direct SMS Backend Authentication using CQRS/Mediator
    /// </summary>
    private async Task<(BaseUser? user, SMSUserType userType)> AuthenticateWithSMSBackendAsync(string email, string password)
    {
        // Try SMSApplicationUser first
        try
        {
            var appQuery = new GetSMSApplicationUserByUserNameQuery(email);
            var appResult = await _mediator.SendAsync(appQuery, CancellationToken.None);

            if (appResult.IsSuccess && appResult.Value.Authenticate(password))
            {
                appResult.Value.RecordLogin();
                return (appResult.Value, SMSUserType.Application);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Application user auth failed: {Error}", ex.Message);
        }

        // Try SMSOrganizationalUser
        try
        {
            var orgQuery = new GetSMSOrganizationalUserByUserNameQuery(email);
            var orgResult = await _mediator.SendAsync(orgQuery, CancellationToken.None);

            if (orgResult.IsSuccess && orgResult.Value.Authenticate(password))
            {
                orgResult.Value.RecordLogin();
                return (orgResult.Value, SMSUserType.Organizational);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Organizational user auth failed: {Error}", ex.Message);
        }

        // Try SMSStakeholderUser
        try
        {
            var stakeholderQuery = new GetSMSStakeholderUserByUserNameQuery(email);
            var stakeholderResult = await _mediator.SendAsync(stakeholderQuery, CancellationToken.None);

            if (stakeholderResult.IsSuccess && stakeholderResult.Value.Authenticate(password))
            {
                stakeholderResult.Value.RecordLogin();
                return (stakeholderResult.Value, SMSUserType.Stakeholder);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Stakeholder user auth failed: {Error}", ex.Message);
        }

        return (null, SMSUserType.Application); // Default fallback
    }

    /// <summary>
    /// Create session using SMS Domain Entity data directly
    /// </summary>
    private void CreateSMSSession(BaseUser user, SMSUserType userType)
    {
        // **FIX**: Store consistent session keys - use SMS_UserId instead of SMS_UserCode
        HttpContext.Session.SetString("SMS_UserId", user.Code); // This is what middleware expects
        HttpContext.Session.SetString("SMS_UserCode", user.Code);
        HttpContext.Session.SetString("SMS_UserType", userType.Value); // Use .Value, not .Name
        HttpContext.Session.SetString("SMS_Email", user.UserName.Value);
        HttpContext.Session.SetString("SMS_DisplayName", user.DisplayName);
        HttpContext.Session.SetString("SMS_FirstName", user.FirstName.Value);
        HttpContext.Session.SetString("SMS_LastName", user.LastName.Value);

        _logger.LogInformation("Creating SMS Session: UserId={UserId}, UserType={UserType}, DisplayName={DisplayName}",
            user.Code, userType.Value, user.DisplayName);

        // Store user role information (common for all user types)
        if (user.UserRole != null)
        {
            HttpContext.Session.SetString("SMS_UserRoleCode", user.UserRole.Code ?? string.Empty);
            HttpContext.Session.SetString("SMS_UserRoleName", user.UserRole.Name ?? string.Empty);

            // Store serialized permissions with proper null checking and debugging
            try
            {
                if (user.UserRole.Permissions != null && user.UserRole.Permissions.Any())
                {
                    var permissionsData = user.UserRole.Permissions.Select(p => new {
                        Module = p.SMSModule ?? string.Empty,
                        Create = p.Create,
                        Read = p.Read,
                        Update = p.Update,
                        Delete = p.Delete
                    }).ToList();

                    var permissionsJson = global::System.Text.Json.JsonSerializer.Serialize(permissionsData);
                    HttpContext.Session.SetString("SMS_UserPermissions", permissionsJson);

                    _logger.LogInformation("Stored {Count} permissions for user {UserId}",
                        user.UserRole.Permissions.Count, user.Code);
                }
                else
                {
                    // No permissions found - store empty array
                    HttpContext.Session.SetString("SMS_UserPermissions", "[]");
                    _logger.LogWarning("No permissions found for user {UserId} with role {RoleCode}",
                        user.Code, user.UserRole.Code);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to serialize permissions for user {UserId}", user.UserId.Value);
                HttpContext.Session.SetString("SMS_UserPermissions", "[]");
            }
        }
        else
        {
            // No user role assigned
            _logger.LogWarning("No UserRole assigned to user {UserCode}", user.Code);
            HttpContext.Session.SetString("SMS_UserRoleCode", string.Empty);
            HttpContext.Session.SetString("SMS_UserRoleName", string.Empty);
            HttpContext.Session.SetString("SMS_UserPermissions", "[]");
        }

        _logger.LogInformation("Session base data stored successfully");

        // Store user type-specific data from Domain Entities
        switch (userType)
        {
            case var type when type == SMSUserType.Application && user is SMSApplicationUser appUser:
                // Application users now only have basic properties + UserRole
                HttpContext.Session.SetString("SMS_ApplicationUserCode", appUser.Code ?? string.Empty);
                _logger.LogInformation("Application user session data: UserId={UserId}, Role={RoleName}",
                    appUser.ApplicationUserId?.Value, user.UserRole?.Name);
                break;

            case var type when type == SMSUserType.Organizational && user is SMSOrganizationalUser orgUser:
                HttpContext.Session.SetString("SMS_Department", orgUser.Department ?? string.Empty);
                HttpContext.Session.SetString("SMS_Position", orgUser.Position ?? string.Empty);
                HttpContext.Session.SetString("SMS_OrganizationLevel", orgUser.OrganizationLevel ?? string.Empty);
                HttpContext.Session.SetString("SMS_OrganizationalUserId", orgUser.OrganizationalUserId?.Value ?? string.Empty);
                _logger.LogInformation("Organizational user session data: Dept={Dept}, Position={Position}, Level={Level}",
                    orgUser.Department, orgUser.Position, orgUser.OrganizationLevel);
                break;

            case var type when type == SMSUserType.Stakeholder && user is SMSStakeholderUser stakeholderUser:
                HttpContext.Session.SetString("SMS_Organization", stakeholderUser.Organization ?? string.Empty);
                HttpContext.Session.SetString("SMS_StakeholderType", stakeholderUser.StakeholderType ?? string.Empty);
                HttpContext.Session.SetString("SMS_StakeholderUserId", stakeholderUser.StakeholderUserId?.Value ?? string.Empty);
                // Note: AccessLevel removed - now handled through UserRole permissions
                _logger.LogInformation("Stakeholder user session data: Org={Org}, Type={Type}, Role={RoleName}",
                    stakeholderUser.Organization, stakeholderUser.StakeholderType, user.UserRole?.Name);
                break;

            default:
                _logger.LogWarning("Unknown user type or casting failed: UserType={UserType}, UserClass={UserClass}",
                    userType.Value, user.GetType().Name);
                break;
        }

        // **IMPORTANT**: Mark session as authenticated
        HttpContext.Session.SetString("IsAuthenticated", "true");
    }

    /// <summary>
    /// Get dashboard URL based on SMS Domain User Type
    /// </summary>
    private string GetDashboardUrl(SMSUserType userType)
    {
        var url = userType.Value switch
        {
            "APPLICATION" => "/Dashboard/System",
            "ORGANIZATIONAL" => "/Dashboard/Operations", 
            "STAKEHOLDER" => "/Dashboard/Stakeholder",
            _ => "/Index"
        };

        _logger.LogInformation("Dashboard URL for user type {UserType}: {Url}", userType.Value, url);
        return url;
    }
}