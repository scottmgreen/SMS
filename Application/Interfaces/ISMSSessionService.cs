//-----------------------------------------------------------------------
// <copyright file="ISMSSessionService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application service providing business logic operations for SMS domain entities.
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Interfaces;

/// <summary>
/// SMS Session Management Service - Direct Session Handling
/// Uses SMS Domain Entities directly, no wrapper models
/// </summary>
public interface ISMSSessionService
{
    /// <summary>
    /// Creates SMS session using Domain Entity data directly with Blazor Server timing fixes
    /// Includes retry mechanism and fallback to HttpContext.Items for timing issues
    /// </summary>
    /// <param name="user">SMS Domain Entity (BaseUser)</param>
    /// <param name="userType">SMS User Type (Smart Enum)</param>
    Task CreateSMSSessionAsync(BaseUser user, SMSUserType userType);

    /// <summary>
    /// Clears SMS session data
    /// </summary>
    Task ClearSMSSessionAsync();

    /// <summary>
    /// Checks if current session is authenticated
    /// </summary>
    bool IsAuthenticated();

    /// <summary>
    /// Gets current user ID from session
    /// </summary>
    string? GetCurrentUserId();

    /// <summary>
    /// Gets current user display name from session
    /// </summary>
    string? GetCurrentUserDisplayName();

    /// <summary>
    /// Gets current user type from session
    /// </summary>
    string? GetCurrentUserType();

    // 🔐 Two-Factor Authentication Session Methods

    /// <summary>
    /// Store user temporarily for 2FA verification after password authentication
    /// </summary>
    /// <param name="user">Authenticated user awaiting 2FA verification</param>
    /// <param name="userType">User type for proper handling</param>
    Task StorePending2FAUserAsync(BaseUser user, SMSUserType userType);

    /// <summary>
    /// Retrieve pending 2FA user data
    /// </summary>
    /// <returns>Tuple of user and userType if pending 2FA exists, null otherwise</returns>
    (BaseUser User, SMSUserType UserType)? GetPending2FAUser();

    /// <summary>
    /// Clear pending 2FA user data (called after successful 2FA or timeout)
    /// </summary>
    Task ClearPending2FAUserAsync();

    /// <summary>
    /// Check if there's a user pending 2FA verification
    /// </summary>
    bool HasPending2FAUser();
}
