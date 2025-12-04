using Microsoft.AspNetCore.Mvc;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Queries;
using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Shared.Common;

namespace SMS_Blazor.Controllers;

/// <summary>
/// Login Controller for Blazor Server - handles form POST to avoid session timing issues
/// Uses EXACT same logic as Login.cshtml.cs from SMS_Presentation
/// </summary>
[Route("api/auth")]
public class LoginController : Controller
{
    private readonly IMediator _mediator;
    private readonly ISMSSessionService _sessionService;
    private readonly ILogger<LoginController> _logger;

    public LoginController(IMediator mediator, ISMSSessionService sessionService, ILogger<LoginController> logger)
    {
        _mediator = mediator;
        _sessionService = sessionService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("Login attempt with missing credentials");
            return Redirect($"/login?ErrorMessage=Email and password are required&Email={Uri.EscapeDataString(email ?? "")}");
        }

        try
        {
            _logger.LogInformation("Direct SMS Backend authentication for: {Email}", email);

            // DIRECT SMS Backend call - Try each user type through CQRS (EXACT same as Login.cshtml.cs)
            var (user, userType) = await AuthenticateWithSMSBackendAsync(email, password);

            if (user == null)
            {
                _logger.LogWarning("Authentication failed for: {Email}", email);
                return Redirect($"/login?ErrorMessage=Invalid email or password&Email={Uri.EscapeDataString(email)}");
            }

            // Create session with SMS Domain Entity data directly
            await _sessionService.CreateSMSSessionAsync(user, userType);

            _logger.LogInformation("SMS Backend authentication successful: {UserId} ({UserType})",
                user.Code, userType.Value);

            // Redirect to home page after successful login
            return Redirect("/");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMS Backend authentication error for: {Email}", email);
            return Redirect($"/login?ErrorMessage=An error occurred during login. Please try again&Email={Uri.EscapeDataString(email)}");
        }
    }

    /// <summary>
    /// Direct SMS Backend Authentication using CQRS/Mediator (EXACT copy from Login.cshtml.cs)
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
}