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

            _logger.LogInformation("SMS Backend authentication successful: {UserId} ({UserType})",
                user.UserId.Value, userType.Name);

            // Redirect based on SMS User Type
            var redirectUrl = GetDashboardUrl(userType);
            return Redirect(!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl) ? ReturnUrl : redirectUrl);
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
        HttpContext.Session.SetString("SMS_UserId", user.UserId.Value);
        HttpContext.Session.SetString("SMS_UserType", userType.Value);
        HttpContext.Session.SetString("SMS_Email", user.UserName.Value);
        HttpContext.Session.SetString("SMS_DisplayName", user.DisplayName);
        HttpContext.Session.SetString("SMS_FirstName", user.FirstName.Value);
        HttpContext.Session.SetString("SMS_LastName", user.LastName.Value);

        // Store user type-specific data from Domain Entities
        switch (userType)
        {
            case var type when type == SMSUserType.Application && user is SMSApplicationUser appUser:
                HttpContext.Session.SetString("SMS_ApplicationRole", appUser.ApplicationRole);
                HttpContext.Session.SetString("SMS_PermissionLevel", appUser.PermissionLevel);
                break;

            case var type when type == SMSUserType.Organizational && user is SMSOrganizationalUser orgUser:
                HttpContext.Session.SetString("SMS_Department", orgUser.Department);
                HttpContext.Session.SetString("SMS_Position", orgUser.Position);
                HttpContext.Session.SetString("SMS_OrganizationLevel", orgUser.OrganizationLevel);
                break;

            case var type when type == SMSUserType.Stakeholder && user is SMSStakeholderUser stakeholderUser:
                HttpContext.Session.SetString("SMS_Organization", stakeholderUser.Organization);
                HttpContext.Session.SetString("SMS_StakeholderType", stakeholderUser.StakeholderType);
                HttpContext.Session.SetString("SMS_AccessLevel", stakeholderUser.AccessLevel);
                break;
        }
    }

    /// <summary>
    /// Get dashboard URL based on SMS Domain User Type
    /// </summary>
    private static string GetDashboardUrl(SMSUserType userType)
    {
        return userType.Value switch
        {
            "APPLICATION" => "/Dashboard/System",
            "ORGANIZATIONAL" => "/Dashboard/Operations",
            "STAKEHOLDER" => "/Dashboard/Stakeholder",
            _ => "/Index"
        };
    }
}