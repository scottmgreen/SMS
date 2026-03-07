using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using SMS_Application.Interfaces;

namespace SMS3.Components.Pages;

public partial class Login : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ILogger<Login> Logger { get; set; } = default!;
    [Inject] private ISMSSessionService SessionService { get; set; } = default!;
    [Inject] private IHttpContextAccessor HttpContextAccessor { get; set; } = default!;
    [Inject] private IAuthenticationService AuthenticationService { get; set; } = default!;

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

            Logger.LogInformation("Attempting Smart Login for user: {Username}", model.Username);

            // Get request context for audit logging
            var httpContext = HttpContextAccessor.HttpContext;
            var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = httpContext?.Request?.Headers?.UserAgent.ToString() ?? "Unknown";
            var sessionId = Guid.NewGuid().ToString();

            // **SMART LOGIN**: Use Application Layer Authentication Service
            var authResult = await AuthenticationService.AuthenticateAsync(model.Username, model.Password, CancellationToken.None);

            if (authResult.IsSuccess && authResult.User != null)
            {
                Logger.LogInformation("Authentication successful for user: {Username}, Type: {UserType}", model.Username, authResult.UserType.Value);

                // 🔐 Record successful authentication audit FIRST (and wait for it)
                try
                {
                    var authSuccessCommand = new RecordAuthenticationSuccessCommand(
                        model.Username,
                        authResult.UserType,
                        authResult.User.DisplayName,
                        ipAddress,
                        userAgent,
                        sessionId
                    );
                    var auditResult = await Mediator.SendAsync(authSuccessCommand, CancellationToken.None);
                    Logger.LogInformation("✅ Authentication success audit recorded for user: {Username}, Result: {IsSuccess}", 
                        model.Username, auditResult.IsSuccess);
                }
                catch (Exception auditEx)
                {
                    Logger.LogError(auditEx, "❌ Failed to record authentication success audit for {Username}", model.Username);
                    // Continue with login even if audit fails - don't block user
                }

                // 🔧 BLAZOR SERVER FIX: Use redirect-based authentication instead of direct session creation
                try
                {
                    // Store authentication data in TempData or a cache that survives redirect
                    var authToken = Guid.NewGuid().ToString();
                    
                    // Store auth data temporarily using a service that can survive redirect
                    await StoreTemporaryAuthData(authToken, authResult.User, authResult.UserType);
                    
                    Logger.LogInformation("Authentication data stored with token: {Token} for user: {Username}", authToken, model.Username);
                    
                    // Navigate with the auth token - this allows session creation on the new request
                    Navigation.NavigateTo($"/auth-complete?token={authToken}", forceLoad: true);
                    return;
                }
                catch (Exception authEx)
                {
                    Logger.LogError(authEx, "Failed to store authentication data for user {Username}", model.Username);
                    ErrorMessage = "Authentication succeeded but login setup failed. Please try again.";
                    return;
                }
            }
            else
            {
                // 🔐 Record failed authentication audit (and wait for it)
                try
                {
                    var authFailureCommand = new RecordAuthenticationFailureCommand(
                        model.Username,
                        "Invalid username or password",
                        ipAddress,
                        userAgent,
                        1
                    );
                    var auditResult = await Mediator.SendAsync(authFailureCommand, CancellationToken.None);
                    Logger.LogInformation("✅ Authentication failure audit recorded for user: {Username}, Result: {IsSuccess}", 
                        model.Username, auditResult.IsSuccess);
                }
                catch (Exception auditEx)
                {
                    Logger.LogError(auditEx, "❌ Failed to record authentication failure audit for {Username}", model.Username);
                    // Continue - don't block user from seeing error message
                }

                ErrorMessage = authResult.ErrorMessage ?? "Invalid username or password";
                Logger.LogWarning("Authentication failed for user: {Username}", model.Username);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during Smart Login for user: {Username}", model.Username);
            ErrorMessage = "An error occurred during login. Please try again.";
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Store authentication data temporarily to survive redirect
    /// </summary>
    private async Task StoreTemporaryAuthData(string token, BaseUser user, SMSUserType userType)
    {
        // Use a simple in-memory cache for the auth token (short-lived)
        var authData = new
        {
            User = user,
            UserType = userType,
            Timestamp = DateTime.UtcNow
        };
        
        // Store in static cache (you could use IMemoryCache here instead)
        TempAuthCache.Store(token, authData, TimeSpan.FromMinutes(5));
    }

    /// <summary>
    /// Simple static cache for temporary auth data during login flow
    /// </summary>
    internal static class TempAuthCache
    {
        private static readonly Dictionary<string, (object Data, DateTime Expiry)> _cache = new();
        private static readonly object _lock = new object();

        public static void Store(string key, object data, TimeSpan expiry)
        {
            lock (_lock)
            {
                _cache[key] = (data, DateTime.UtcNow.Add(expiry));
            }
        }

        public static T? Get<T>(string key) where T : class
        {
            lock (_lock)
            {
                if (_cache.TryGetValue(key, out var entry))
                {
                    if (DateTime.UtcNow <= entry.Expiry)
                    {
                        return entry.Data as T;
                    }
                    else
                    {
                        _cache.Remove(key);
                    }
                }
                return null;
            }
        }

        public static void Remove(string key)
        {
            lock (_lock)
            {
                _cache.Remove(key);
            }
        }
    }

    public class LoginFormModel
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }
}