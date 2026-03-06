using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SMS3.Components.Pages;

public partial class Login : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ILogger<Login> Logger { get; set; } = default!;
    [Inject] private ISMSSessionService SessionService { get; set; } = default!;
    [Inject] private IHttpContextAccessor HttpContextAccessor { get; set; } = default!;

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

            // **SMART LOGIN**: Discover user type and authenticate
            var (user, userType) = await DiscoverAndAuthenticateUserAsync(model.Username, model.Password);

            if (user != null)
            {
                Logger.LogInformation("Smart Login successful for user: {Username}, Type: {UserType}", model.Username, userType.Value);

                // 🔧 BLAZOR SERVER FIX: Use redirect-based authentication instead of direct session creation
                try
                {
                    // Store authentication data in TempData or a cache that survives redirect
                    var authToken = Guid.NewGuid().ToString();
                    
                    // Store auth data temporarily using a service that can survive redirect
                    await StoreTemporaryAuthData(authToken, user, userType);
                    
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

                // 🔐 Record successful authentication audit AFTER session creation (fire and forget)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var authSuccessCommand = new RecordAuthenticationSuccessCommand(
                            model.Username,
                            userType,
                            user.DisplayName,
                            ipAddress,
                            userAgent,
                            sessionId
                        );
                        await Mediator.SendAsync(authSuccessCommand, CancellationToken.None);
                    }
                    catch (Exception auditEx)
                    {
                        Logger.LogError(auditEx, "Failed to record authentication success audit for {Username}", model.Username);
                    }
                });

                // 🔧 BLAZOR SERVER FIX: Navigate to home page
                IsLoading = false;
                ErrorMessage = string.Empty;
                StateHasChanged();
                
                // Use a longer delay to ensure session data is committed
                await Task.Delay(500);
                
                // Navigate to home page with force refresh
                Navigation.NavigateTo("/", forceLoad: true);
            }
            else
            {
                // 🔐 Record failed authentication audit (fire and forget)
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var authFailureCommand = new RecordAuthenticationFailureCommand(
                            model.Username,
                            "Invalid username or password",
                            ipAddress,
                            userAgent,
                            1
                        );
                        await Mediator.SendAsync(authFailureCommand, CancellationToken.None);
                    }
                    catch (Exception auditEx)
                    {
                        Logger.LogError(auditEx, "Failed to record authentication failure audit for {Username}", model.Username);
                    }
                });

                ErrorMessage = "Invalid username or password";
                Logger.LogWarning("Smart Login failed for user: {Username}", model.Username);
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
    /// Smart Login: Try each user type sequentially using the same pattern as other login implementations
    /// This matches the logic used in Login.cshtml.cs, LoginController.cs, and SMS_Blazor Login.razor
    /// </summary>
    private async Task<(BaseUser? user, SMSUserType userType)> DiscoverAndAuthenticateUserAsync(string email, string password)
    {
        // Try SMSApplicationUser first
        try
        {
            Logger.LogDebug("Checking Application User: {Email}", email);
            var appQuery = new GetSMSApplicationUserByUserNameQuery(email);
            var appResult = await Mediator.SendAsync(appQuery, CancellationToken.None);

            if (appResult.IsSuccess && appResult.Value != null && appResult.Value.Authenticate(password))
            {
                appResult.Value.RecordLogin();
                Logger.LogInformation("Application User authenticated successfully: {Email}", email);
                return (appResult.Value, SMSUserType.Application);
            }
        }
        catch (Exception ex)
        {
            Logger.LogDebug("Application user auth failed: {Error}", ex.Message);
        }

        // Try SMSOrganizationalUser
        try
        {
            Logger.LogDebug("Checking Organizational User: {Email}", email);
            var orgQuery = new GetSMSOrganizationalUserByUserNameQuery(email);
            var orgResult = await Mediator.SendAsync(orgQuery, CancellationToken.None);

            if (orgResult.IsSuccess && orgResult.Value != null && orgResult.Value.Authenticate(password))
            {
                orgResult.Value.RecordLogin();
                Logger.LogInformation("Organizational User authenticated successfully: {Email}", email);
                return (orgResult.Value, SMSUserType.Organizational);
            }
        }
        catch (Exception ex)
        {
            Logger.LogDebug("Organizational user auth failed: {Error}", ex.Message);
        }

        // Try SMSStakeholderUser
        try
        {
            Logger.LogDebug("Checking Stakeholder User: {Email}", email);
            var stakeholderQuery = new GetSMSStakeholderUserByUserNameQuery(email);
            var stakeholderResult = await Mediator.SendAsync(stakeholderQuery, CancellationToken.None);

            if (stakeholderResult.IsSuccess && stakeholderResult.Value != null && stakeholderResult.Value.Authenticate(password))
            {
                stakeholderResult.Value.RecordLogin();
                Logger.LogInformation("Stakeholder User authenticated successfully: {Email}", email);
                return (stakeholderResult.Value, SMSUserType.Stakeholder);
            }
        }
        catch (Exception ex)
        {
            Logger.LogDebug("Stakeholder user auth failed: {Error}", ex.Message);
        }

        // No user found or authentication failed
        Logger.LogWarning("Smart Login failed - user not found or invalid credentials: {Email}", email);
        return (null, SMSUserType.Application); // Default fallback
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