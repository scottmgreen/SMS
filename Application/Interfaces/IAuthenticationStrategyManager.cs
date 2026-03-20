//-----------------------------------------------------------------------
// <copyright file="IAuthenticationStrategyManager.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Manager interface for orchestrating authentication strategies.
//                  Handles method selection, fallback chains, and strategy coordination.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Application.Configuration;
using SMS_Domain.Enums;

namespace SMS_Application.Interfaces;

/// <summary>
/// Manager for orchestrating authentication strategies
/// Handles method selection, fallback chains, and strategy coordination
/// </summary>
public interface IAuthenticationStrategyManager
{
    /// <summary>
    /// Get the currently configured primary authentication strategy
    /// </summary>
    IAuthenticationStrategy PrimaryStrategy { get; }

    /// <summary>
    /// Get the currently configured fallback authentication strategy
    /// </summary>
    IAuthenticationStrategy? FallbackStrategy { get; }

    /// <summary>
    /// Store user using the configured authentication strategy chain
    /// </summary>
    Task<Result<bool>> StoreUserAsync(BaseUser user, SMSUserType userType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve user using the configured authentication strategy chain
    /// </summary>
    Task<Result<(BaseUser User, SMSUserType UserType)?>> RetrieveUserAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Clear user using all available authentication strategies
    /// </summary>
    Task<Result<bool>> ClearUserAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if user is authenticated using any available strategy
    /// </summary>
    Task<bool> IsUserAuthenticatedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current user ID using any available strategy
    /// </summary>
    Task<string?> GetCurrentUserIdAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current user display name using any available strategy
    /// </summary>
    Task<string?> GetCurrentUserDisplayNameAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get authentication strategy by method type
    /// </summary>
    IAuthenticationStrategy? GetStrategy(AuthenticationMethod method);

    /// <summary>
    /// Get all available authentication strategies
    /// </summary>
    IEnumerable<IAuthenticationStrategy> GetAllStrategies();

    /// <summary>
    /// Get status information about all strategies for debugging
    /// </summary>
    Task<Dictionary<string, object>> GetStrategyStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate authentication data across all strategies
    /// </summary>
    Task<Dictionary<AuthenticationMethod, bool>> ValidateAllStrategiesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Force refresh of strategy configuration
    /// </summary>
    void RefreshConfiguration();
}