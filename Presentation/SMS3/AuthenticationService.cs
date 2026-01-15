using SMS_Domain.Entities;
using SMS_Domain.Enums;

namespace SMS3;

/// <summary>
/// CLEAN Authentication service - Simple and reliable
/// </summary>
public class AuthenticationService
{
    private readonly AuthenticationState _authState = new();

    /// <summary>
    /// Current authentication state - EVERYTHING YOU NEED
    /// </summary>
    public AuthenticationState User => _authState;

    // **QUICK ACCESS PROPERTIES**
    public string CurrentUserDisplayName => _authState.DisplayName ?? "System User";
    public string CurrentUserId => _authState.UserId ?? "SYSTEM";
    public bool IsAuthenticated => _authState.IsAuthenticated;
    public BaseUser? CurrentUser => _authState.CurrentUser;

    /// <summary>
    /// Set authentication from successful login
    /// </summary>
    public void SetAuthentication(BaseUser user, SMSUserType userType)
    {
        _authState.SetAuthentication(user, userType);
        StateChanged?.Invoke();
    }

    /// <summary>
    /// Clear authentication for logout
    /// </summary>
    public void ClearAuthentication()
    {
        _authState.ClearAuthentication();
        StateChanged?.Invoke();
    }

    /// <summary>
    /// Event for UI updates
    /// </summary>
    public event Action? StateChanged;
}