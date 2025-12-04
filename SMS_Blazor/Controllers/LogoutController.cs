using Microsoft.AspNetCore.Mvc;
using SMS_Application.Interfaces;

namespace SMS_Blazor.Controllers;

/// <summary>
/// Logout Controller for Blazor Server - handles logout properly
/// Uses SMS Session Service to clear session
/// </summary>
[Route("api/auth")]
public class LogoutController : Controller
{
    private readonly ISMSSessionService _sessionService;
    private readonly ILogger<LogoutController> _logger;

    public LogoutController(ISMSSessionService sessionService, ILogger<LogoutController> logger)
    {
        _sessionService = sessionService;
        _logger = logger;
    }

    [HttpPost("logout")]
    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userId = _sessionService.GetCurrentUserId();
            _logger.LogInformation("Logout initiated for user: {UserId}", userId);

            await _sessionService.ClearSMSSessionAsync();

            _logger.LogInformation("Logout completed for user: {UserId}", userId);
            return Redirect("/login");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return Redirect("/login");
        }
    }
}