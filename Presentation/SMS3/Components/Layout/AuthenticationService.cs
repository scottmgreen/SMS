using SMS_Domain.Entities;
using SMS_Domain.Enums;
using Microsoft.AspNetCore.Components;

namespace SMS3.Components.Layout;

/// <summary>
/// Authentication service that can be injected across different layouts
/// This bridges the gap between LoginLayout and MainLayout
/// </summary>
public class AuthenticationService
{
    /// <summary>
    /// Current user display name - ALWAYS AVAILABLE
    /// </summary>
    public string CurrentUserDisplayName => _authState.DisplayName ?? "System User";

    /// <summary>
    /// Current user ID - ALWAYS AVAILABLE
    /// </summary>
    public string CurrentUserId => _authState.UserId ?? "SYSTEM";

    /// <summary>
    /// Is user authenticated - ALWAYS AVAILABLE
    /// </summary>
    public bool IsAuthenticated => _authState.IsAuthenticated;


    private AuthenticationState _authState = new();
    private readonly List<Func<Task>> _authStateChangedCallbacks = new();

    /// <summary>
    /// Get current authentication state
    /// </summary>
    public AuthenticationState AuthState => _authState;

    /// <summary>
    /// Set authentication from successful login
    /// </summary>
    public void SetAuthentication(BaseUser user, SMSUserType userType)
    {
        _authState.SetAuthentication(user, userType);
        _ = NotifyStateChangedAsync(); // Fire and forget async notification
    }

    /// <summary>
    /// Clear authentication for logout
    /// </summary>
    public void ClearAuthentication()
    {
        _authState.ClearAuthentication();
        _ = NotifyStateChangedAsync(); // Fire and forget async notification
    }

    /// <summary>
    /// Subscribe to authentication state changes (async version)
    /// </summary>
    public void OnAuthStateChanged(Func<Task> callback)
    {
        _authStateChangedCallbacks.Add(callback);
    }

    /// <summary>
    /// Subscribe to authentication state changes (sync version - auto-wrapped)
    /// </summary>
    public void OnAuthStateChanged(Action callback)
    {
        _authStateChangedCallbacks.Add(() => {
            callback.Invoke();
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Unsubscribe from authentication state changes
    /// </summary>
    public void RemoveAuthStateChangedCallback(Func<Task> callback)
    {
        _authStateChangedCallbacks.Remove(callback);
    }

    /// <summary>
    /// Notify all subscribers that auth state changed (async safe)
    /// </summary>
    private async Task NotifyStateChangedAsync()
    {
        foreach (var callback in _authStateChangedCallbacks.ToList()) // ToList to avoid modification during iteration
        {
            try
            {
                await callback.Invoke();
            }
            catch (Exception ex)
            {
                // Log error but don't break other callbacks
                Console.WriteLine($"Error in auth state callback: {ex.Message}");
            }
        }
    }
}