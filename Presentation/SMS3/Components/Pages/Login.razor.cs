using System.ComponentModel.DataAnnotations;

using SMS_Domain.Entities;

using Microsoft.AspNetCore.Http;
using SMS_Application.Interfaces;
using SMS_Application.Services;

using SMS3.Configuration.Extensions;

namespace SMS3.Components.Pages;

public partial class Login : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ILogger<Login> Logger { get; set; } = default!;
    [Inject] private ISMSSessionService SessionService { get; set; } = default!;
    [Inject] private IHttpContextAccessor HttpContextAccessor { get; set; } = default!;
    [Inject] private IAuthenticationService AuthenticationService { get; set; } = default!;
    [Inject] private SessionTimerService SessionTimerService { get; set; } = default!;
    [Inject] private TwoFactorAuthService TwoFactorAuthService { get; set; } = default!;

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

            Logger.LogInformation("🔐 Starting Smart Login for user: {Username}", model.Username);

            var authResult = await AuthenticationService.AuthenticateAsync(model.Username, model.Password, CancellationToken.None);

            if (authResult.IsSuccess && authResult.User != null)
            {
                Logger.LogInformation("✅ Authentication successful for user: {Username}, Type: {UserType}", model.Username, authResult.UserType.Value);
                Logger.LogInformation("🔐 2FA Enabled: {TwoFactorEnabled}", authResult.User.TwoFactorEnabled);

                
                // 🔐 CHECK FOR TWO-FACTOR AUTHENTICATION
                if (authResult.User.TwoFactorEnabled)
                {
                    // User has 2FA enabled - redirect to 2FA verification
                    Logger.LogInformation("🔐 User {Username} has 2FA enabled, initiating 2FA flow", model.Username);
                    
                    try
                    {
                        // 🚨 CRITICAL SESSION ISOLATION FIX: Clear any previous user's 2FA data first
                        Logger.LogInformation("🧹 Clearing any previous 2FA session data for session isolation...");
                        await SessionService.ClearPending2FAUserAsync();
                        
                        // Store user temporarily for 2FA verification with explicit wait
                        Logger.LogInformation("🔐 Storing pending 2FA user data for {Username}...", model.Username);
                        await SessionService.StorePending2FAUserAsync(authResult.User, authResult.UserType);
                        
                        // Wait longer to ensure session is committed
                        Logger.LogInformation("🔐 Waiting for session commit...");
                        await Task.Delay(300);
                        
                        // Verify the user was stored before navigation
                        Logger.LogInformation("🔐 Verifying pending 2FA user was stored...");
                        var storedUser = SessionService.GetPending2FAUser();
                        if (storedUser == null)
                        {
                            Logger.LogError("❌ FAILED to store pending 2FA user - session storage verification failed");
                            ErrorMessage = "Failed to initiate 2FA process. Please try again.";
                            return;
                        }
                        
                        // 🚨 VERIFICATION: Ensure stored user matches current user (prevent contamination)
                        if (storedUser.Value.User.UserName.Value != model.Username)
                        {
                            Logger.LogError("❌ CRITICAL SESSION CONTAMINATION: Stored user {StoredUser} does not match login user {LoginUser}",
                                storedUser.Value.User.UserName.Value, model.Username);
                            ErrorMessage = "Session error detected. Please try logging in again.";
                            await SessionService.ClearPending2FAUserAsync(); // Clear contaminated data
                            return;
                        }
                        
                        Logger.LogInformation("✅ Pending 2FA user stored and verified successfully: {UserCode} matches {Username}", 
                            storedUser?.User.Code, model.Username);
                        Logger.LogInformation("🔐 Navigating to /verify-2fa...");
                        
                        // Clear any existing error messages
                        ErrorMessage = string.Empty;
                        StateHasChanged();

                        // Navigate to 2FA verification page
                        Navigation.NavigateToSecure("/verify-2fa", forceLoad: false);

                        // Add additional logging after navigation
                        Logger.LogInformation("✅ Navigation to /verify-2fa initiated successfully");
                        return;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "❌ Exception during 2FA flow setup for {Username}", model.Username);
                        ErrorMessage = "Failed to initiate 2FA process. Please try again.";
                        return;
                    }
                }
                else
                {
                    // No 2FA required - proceed with normal login
                    Logger.LogInformation("🔐 User {Username} does not have 2FA enabled, proceeding with normal login", model.Username);
                    await CompleteLoginAsync(authResult.User, authResult.UserType);
                    return;
                }
            }
            else
            {
                Logger.LogWarning("❌ Authentication failed for user: {Username}", model.Username);
                await HandleLoginFailureAsync(model.Username, authResult.ErrorMessage ?? "Invalid username or password");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "❌ Exception during Smart Login for user: {Username}", model.Username);
            ErrorMessage = "An error occurred during login. Please try again.";
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// ✅ NEW: Get client IP address for audit logging
    /// </summary>
    private string GetClientIPAddress()
    {
        try
        {
            var httpContext = HttpContextAccessor.HttpContext;
            if (httpContext != null)
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

    /// <summary>
    /// ✅ NEW: Get user agent for audit logging
    /// </summary>
    private string GetUserAgent()
    {
        try
        {
            var httpContext = HttpContextAccessor.HttpContext;
            return httpContext?.Request.Headers["User-Agent"].FirstOrDefault() ?? "Blazor";
        }
        catch
        {
            return "Unknown";
        }
    }

    private async Task CompleteLoginAsync(BaseUser user, SMSUserType userType)
    {
        // 🔐 CREATE SESSION-BASED AUTHENTICATION - Replaces static authentication
        await SessionService.CreateSMSSessionAsync(user, userType);
        Logger.LogInformation("✅ Session-based authentication created for user: {Username}", user.UserName.Value);
        
        // 🔐 START SESSION TIMER - Begin countdown for automatic logout
        SessionTimerService.StartTimer();
        Logger.LogInformation("✅ Session timer started for user: {Username}", user.UserName.Value);
        
        // Navigate to home page
        Logger.LogInformation("Navigating to home page after successful authentication");
        Navigation.NavigateTo("/", forceLoad: false);
    }

    private async Task HandleLoginFailureAsync(string username, string errorMessage)
    {
        // 🔐 Record failed authentication audit with simplified data
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
            Logger.LogInformation("✅ Authentication failure audit recorded for user: {Username}, Result: {IsSuccess}", 
                username, auditResult.IsSuccess);
        }
        catch (Exception auditEx)
        {
            Logger.LogError(auditEx, "❌ Failed to record authentication failure audit for {Username}", username);
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
}