//-----------------------------------------------------------------------
// <copyright file="IAuthenticationStrategy.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Strategy pattern interface for authentication methods.
//                  Enables clean separation of Session/Circuit/Context authentication.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using SMS_Application.Configuration;
using SMS_Domain.Enums;

namespace SMS_Application.Interfaces;

/// <summary>
/// Strategy pattern interface for authentication methods
/// Provides clean separation between Session, Circuit, and Context-based authentication
/// </summary>
public interface IAuthenticationStrategy
{
    /// <summary>
    /// Name of the authentication strategy
    /// </summary>
    string StrategyName { get; }

    /// <summary>
    /// Authentication method this strategy implements
    /// </summary>
    AuthenticationMethod Method { get; }

    /// <summary>
    /// Store complete user authentication data
    /// </summary>
    Task<Result<bool>> StoreUserAsync(BaseUser user, SMSUserType userType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve user authentication data
    /// Returns null if no user is authenticated or data is invalid
    /// </summary>
    Task<Result<(BaseUser User, SMSUserType UserType)?>> RetrieveUserAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Clear user authentication data
    /// </summary>
    Task<Result<bool>> ClearUserAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if user is currently authenticated via this strategy
    /// </summary>
    Task<bool> IsUserAuthenticatedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current user ID if authenticated
    /// </summary>
    Task<string?> GetCurrentUserIdAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current user display name if authenticated
    /// </summary>
    Task<string?> GetCurrentUserDisplayNameAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if this strategy is available in the current context
    /// (e.g., HttpContext available for Session strategy)
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Get storage method information for logging/debugging
    /// </summary>
    string GetStorageInfo();

    /// <summary>
    /// Validate stored authentication data integrity
    /// </summary>
    Task<bool> ValidateStoredDataAsync(CancellationToken cancellationToken = default);
}