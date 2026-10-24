using System.ComponentModel.DataAnnotations;

using SMS_Domain.Entities;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.JSInterop;
using SMS_Application.Interfaces;
using SMS_Application.Services;

using SMS3.Configuration.Extensions;

namespace SMS3.Components.Pages;

public partial class Login : ComponentBase
{
    [Inject] private IBaseMediator Mediator { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ILogger<Login> Logger { get; set; } = default!;
    [Inject] private ISMSSessionService SessionService { get; set; } = default!;
    [Inject] private IHttpContextAccessor HttpContextAccessor { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private SMS_Application.Interfaces.IAuthenticationService AuthenticationService { get; set; } = default!;
    [Inject] private SessionTimerService SessionTimerService { get; set; } = default!;
    [Inject] private TwoFactorAuthService TwoFactorAuthService { get; set; } = default!;
    [Inject] private ICurrentUserService _currentUserService { get; set; } = default!;
    [Inject] private INotificationsScanService _notificationsScanService { get; set; } = default!;
    [Inject] private IActiveUserSessionRegistry _activeUserSessionRegistry { get; set; } = default!;

    private static readonly SemaphoreSlim _dailyMitigationScanLock = new(1, 1);
    private static DateOnly? _lastMitigationScanUtcDate;
    private static readonly SemaphoreSlim _dailyReportStatusScanLock = new(1, 1);
    private static DateOnly? _lastReportStatusScanUtcDate;

    private LoginFormModel LoginModel { get; set; } = new();
    private string ErrorMessage { get; set; } = string.Empty;
    private bool IsLoading { get; set; } = false;

    private async Task HandleLoginAsync(LoginFormModel model)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            StateHasChanged();

            Logger.LogInformation("Starting Smart Login for user: {Username}", model.Username);

            var establishPayload = await JSRuntime.InvokeAsync<AuthSessionResponse>(
                "smsAuth.establishSession",
                model.Username,
                model.Password);

            if (establishPayload is not null && establishPayload.IsSuccess)
            {
                if (establishPayload.RequiresTwoFactor)
                {
                    Navigation.NavigateToSecure("/verify-2fa", forceLoad: true);
                    return;
                }

                SessionTimerService.StartTimer();
                Navigation.NavigateToSecure("/SMSRiskManagement/ReportProcessing", forceLoad: true);
                return;
            }

            Logger.LogWarning("Authentication failed for user: {Username}", model.Username);
            await HandleLoginFailureAsync(model.Username, establishPayload?.ErrorMessage ?? "Invalid username or password");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception during Smart Login for user: {Username}", model.Username);
            ErrorMessage = "An error occurred during login. Please try again.";
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// NEW: Get client IP address for audit logging
    /// </summary>
    private string GetClientIPAddress()
    {
        try
        {
            var httpContext = HttpContextAccessor.HttpContext;
            if (httpContext is not null)
            {
                // Check for forwarded IP first (load balancer/proxy scenarios)
                var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    return forwardedFor.Split(',')[0].Trim();
                }

                // Check for real IP header
                var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
                if (!string.IsNullOrEmpty(realIp))
                {
                    return realIp;
                }

                // Fall back to connection remote IP
                return httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            }
            return "Server";
        }
        catch
        {
            return "Unknown";
        }
    }

    
    private async Task CompleteLoginAsync(BaseUser user, SMSUserType userType)
    {
        // ?? CREATE SESSION-BASED AUTHENTICATION - Replaces static authentication
        await SessionService.CreateSMSSessionAsync(user, userType);

        // Ensure user hint cookie is present even when auth storage happened without HttpContext.
        //TrySetUserHintCookie(user.Code);
        //await TrySetUserHintCookieClientSideAsync(user.Code);
        
        // ?? START SESSION TIMER - Begin countdown for automatic logout
        SessionTimerService.StartTimer();

        // Navigate to default page using secure route obfuscation.
        Navigation.NavigateToSecure("/SMSRiskManagement/ReportProcessing", forceLoad: false);
    }

    private void TrySetUserHintCookie(string userCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode))
            {
                return;
            }

            var context = HttpContextAccessor.HttpContext;
            if (context?.Response is null || context.Response.HasStarted)
            {
                return;
            }

            context.Response.Cookies.Append("SMS_UserCodeHint", userCode, new CookieOptions
            {
                Path = "/",
                HttpOnly = true,
                Secure = context.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                IsEssential = true
            });
        }
        catch (Exception ex)
        {
            Logger.LogDebug(ex, "Could not set SMS_UserCodeHint cookie during login completion.");
        }
    }

    private async Task TrySetUserHintCookieClientSideAsync(string userCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode))
            {
                return;
            }

            var encoded = Uri.EscapeDataString(userCode);
            await JSRuntime.InvokeVoidAsync("eval", $"document.cookie='SMS_UserCodeHint={encoded}; path=/; max-age=7200; SameSite=Lax; Secure'");
        }
        catch (Exception ex)
        {
            Logger.LogDebug(ex, "Could not set client-side SMS_UserCodeHint cookie.");
        }
    }




    private async Task HandleLoginFailureAsync(string username, string errorMessage)
    {
        // ?? Record failed authentication audit with simplified data
        try
        {
            var authFailureCommand = new RecordAuthenticationFailureCommand(
                username,
                errorMessage,
                "Server", // Simplified IP address
                "Blazor", // Simplified user agent
                1
            );
            var auditResult = await Mediator.SendAsync(authFailureCommand, CancellationToken.None);
            Logger.LogInformation("Authentication failure audit recorded for user: {Username}, Result: {IsSuccess}", 
                username, auditResult.IsSuccess);
        }
        catch (Exception auditEx)
        {
            Logger.LogError(auditEx, "Failed to record authentication failure audit for {Username}", username);
            // Continue - don't block user from seeing error message
        }

        ErrorMessage = errorMessage;
        Logger.LogWarning("Authentication failed for user: {Username}", username);
    }

    public class LoginFormModel
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }

    private sealed class AuthSessionResponse
    {
        public bool IsSuccess { get; set; }
        public bool RequiresTwoFactor { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

}


