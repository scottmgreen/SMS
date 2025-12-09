using SMS_Domain.Entities;
using SMS_Domain.Enums;

namespace SMS3.Components.Layout;

/// <summary>
/// Authentication state for Blazor components using Cascading Parameters
/// This replaces session/circuit-based authentication with clean component state
/// </summary>
public class AuthenticationState
{
    public bool IsAuthenticated { get; private set; }
    public string? UserId { get; private set; }
    public string? UserType { get; private set; }
    public string? DisplayName { get; private set; }
    public string? Email { get; private set; }
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public DateTime LoginTime { get; private set; }

    /// <summary>
    /// Set authentication state from successful login
    /// </summary>
    public void SetAuthentication(BaseUser user, SMSUserType userType)
    {
        UserId = user.Code;
        UserType = userType.Value;
        DisplayName = user.DisplayName;
        Email = user.UserName.Value;
        FirstName = user.FirstName.Value;
        LastName = user.LastName.Value;
        IsAuthenticated = true;
        LoginTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Clear authentication state for logout
    /// </summary>
    public void ClearAuthentication()
    {
        IsAuthenticated = false;
        UserId = null;
        UserType = null;
        DisplayName = null;
        Email = null;
        FirstName = null;
        LastName = null;
        LoginTime = default;
    }

    /// <summary>
    /// Check if user has administrative access
    /// </summary>
    public bool HasAdministrativeAccess()
    {
        return IsAuthenticated && 
               (UserType == "APPLICATION" || UserType == "ORGANIZATIONAL");
    }

    /// <summary>
    /// Get user type as Smart Enum
    /// </summary>
    public SMSUserType? GetUserTypeEnum()
    {
        if (!IsAuthenticated || string.IsNullOrEmpty(UserType))
            return null;

        return SMSUserType.FromValue(UserType);
    }
}