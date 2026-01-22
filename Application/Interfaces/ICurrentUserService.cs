namespace SMS_Application.Interfaces;

/// <summary>
/// Service to get current user information for audit trails
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's ID/Name for audit purposes
    /// </summary>
    string UserId { get; }

    /// <summary>
    /// Gets the current user's display name
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Indicates if a user is currently authenticated
    /// </summary>
    bool IsAuthenticated { get; }
}