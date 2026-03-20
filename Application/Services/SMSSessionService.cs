//-----------------------------------------------------------------------
// <copyright file="SMSSessionService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: REFACTORED SMS Session Service using Authentication Strategy Manager.
//                  Clean, explicit authentication method selection with CQRS-based user loading.
//                  Replaces mixed Session/Circuit/Context approach with strategy pattern.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.Configuration;
using SMS_Application.Interfaces;

namespace SMS_Application.Services;

/// <summary>
/// REFACTORED SMS Session Management Service using Authentication Strategy Manager
/// Clean, explicit authentication method selection with CQRS-based user loading
/// Replaces mixed Session/Circuit/Context approach with strategy pattern
/// </summary>
public class SMSSessionService : ISMSSessionService
{
    private readonly ILogger<SMSSessionService> _logger;
    private readonly IAuthenticationStrategyManager _strategyManager;
    private readonly IUserInstantiationService _userInstantiationService;
    private readonly SessionConfiguration _sessionConfig;
    private readonly TwoFactorAuthConfiguration _twoFactorConfig;

    public SMSSessionService(
        ILogger<SMSSessionService> logger,
        IAuthenticationStrategyManager strategyManager,
        IUserInstantiationService userInstantiationService,
        SessionConfiguration sessionConfig,
        TwoFactorAuthConfiguration twoFactorConfig)
    {
        _logger = logger;
        _strategyManager = strategyManager;
        _userInstantiationService = userInstantiationService;
        _sessionConfig = sessionConfig;
        _twoFactorConfig = twoFactorConfig;
    }

    /// <summary>
    /// Create SMS session using configured authentication strategy
    /// Uses CQRS to ensure complete user instantiation with roles/permissions
    /// </summary>
    public async Task CreateSMSSessionAsync(BaseUser user, SMSUserType userType)
    {
        try
        {
            _logger.LogInformation("🚀 Creating SMS session for user {UserCode} ({UserType}) using strategy manager", 
                user.Code, userType.Value);

            // Ensure user is completely instantiated with all roles and permissions via CQRS
            var completeUserResult = await _userInstantiationService.EnsureUserCompletenessAsync(user, userType);
            if (completeUserResult.IsFailure)
            {
                _logger.LogError("❌ Failed to ensure user completeness for {UserCode}: {Error}", 
                    user.Code, completeUserResult.Error?.Message);
                throw new InvalidOperationException($"User {user.Code} could not be fully instantiated: {completeUserResult.Error?.Message}");
            }

            var completeUser = completeUserResult.Value;

            // Validate user completeness
            if (!_userInstantiationService.IsUserComplete(completeUser))
            {
                var missing = _userInstantiationService.GetMissingComponents(completeUser);
                _logger.LogError("❌ User {UserCode} is incomplete - Missing: {MissingComponents}", 
                    user.Code, string.Join(", ", missing));
                throw new InvalidOperationException($"User {user.Code} is incomplete - missing: {string.Join(", ", missing)}");
            }

            // Store user via authentication strategy manager
            var storeResult = await _strategyManager.StoreUserAsync(completeUser, userType);
            if (storeResult.IsFailure)
            {
                _logger.LogError("❌ Failed to store user {UserCode} via strategy manager: {Error}", 
                    user.Code, storeResult.Error?.Message);
                throw new InvalidOperationException($"Failed to store user {user.Code}: {storeResult.Error?.Message}");
            }

            // Log successful authentication with strategy details
            var primaryStrategy = _strategyManager.PrimaryStrategy;
            var fallbackStrategy = _strategyManager.FallbackStrategy;

            if (_sessionConfig.LogSessionActivity)
            {
                _logger.LogInformation("✅ SMS Session created successfully for user {UserCode} ({UserType}) - Primary: {PrimaryStrategy}, Fallback: {FallbackStrategy}, Completeness: 100%", 
                    user.Code, userType.Value, primaryStrategy.StrategyName, fallbackStrategy?.StrategyName ?? "None");
            }
            else
            {
                _logger.LogInformation("✅ SMS Session created successfully for user {UserCode} ({UserType})", 
                    user.Code, userType.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creating SMS session for user {UserCode} ({UserType})", user.Code, userType.Value);
            throw;
        }
    }

    /// <summary>
    /// Clear SMS session using all available authentication strategies
    /// </summary>
    public async Task ClearSMSSessionAsync()
    {
        try
        {
            // Get user info before clearing for logging
            var userId = await _strategyManager.GetCurrentUserIdAsync() ?? "Unknown";
            
            _logger.LogInformation("🗑️ Clearing SMS session for user: {UserId}", userId);

            // Clear using strategy manager (clears all strategies)
            var clearResult = await _strategyManager.ClearUserAsync();
            
            if (clearResult.IsSuccess)
            {
                if (_sessionConfig.LogSessionActivity)
                {
                    _logger.LogInformation("✅ SMS Session cleared successfully for user: {UserId} - All strategies cleared", userId);
                }
                else
                {
                    _logger.LogInformation("✅ SMS Session cleared successfully for user: {UserId}", userId);
                }
            }
            else
            {
                _logger.LogWarning("⚠️ Some strategies may not have been cleared for user: {UserId}", userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error clearing SMS session");
            throw;
        }
    }

    /// <summary>
    /// Get current user ID using authentication strategy manager
    /// </summary>
    public string? GetCurrentUserId()
    {
        try
        {
            return _strategyManager.GetCurrentUserIdAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting current user ID");
            return null;
        }
    }

    /// <summary>
    /// Get current user display name using authentication strategy manager
    /// </summary>
    public string? GetCurrentUserDisplayName()
    {
        try
        {
            return _strategyManager.GetCurrentUserDisplayNameAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting current user display name");
            return null;
        }
    }

    /// <summary>
    /// Get current user type using authentication strategy manager
    /// </summary}
    public string? GetCurrentUserType()
    {
        try
        {
            var result = _strategyManager.RetrieveUserAsync().GetAwaiter().GetResult();
            return result.IsSuccess && result.Value.HasValue ? result.Value.Value.UserType.Value : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting current user type");
            return null;
        }
    }

    /// <summary>
    /// Check authentication status using authentication strategy manager
    /// </summary>
    public bool IsAuthenticated()
    {
        try
        {
            return _strategyManager.IsUserAuthenticatedAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error checking authentication status");
            return false;
        }
    }

    // 🔐 Two-Factor Authentication Session Methods
    // These remain circuit-based for now as they are temporary storage

    /// <summary>
    /// Store user temporarily for 2FA verification after password authentication
    /// Uses circuit strategy for temporary 2FA data - with HTTP protocol compatibility
    /// </summary>
    public async Task StorePending2FAUserAsync(BaseUser user, SMSUserType userType)
    {
        try
        {
            _logger.LogInformation("🔐 Storing pending 2FA user: {UserId} ({UserType}) with HTTP protocol compatibility", user.Code, userType.Value);

            // Ensure user is complete before storing
            var completeUserResult = await _userInstantiationService.EnsureUserCompletenessAsync(user, userType);
            if (completeUserResult.IsFailure)
            {
                _logger.LogError("❌ Failed to ensure user completeness for 2FA: {UserCode}: {Error}", 
                    user.Code, completeUserResult.Error?.Message);
                throw new InvalidOperationException($"User {user.Code} could not be fully instantiated for 2FA: {completeUserResult.Error?.Message}");
            }

            var completeUser = completeUserResult.Value;

            // Serialize complete user data for 2FA storage
            var userData = _userInstantiationService.SerializeCompleteUser(completeUser, userType);

            // Add 2FA-specific markers
            userData["Pending2FA_UserData"] = System.Text.Json.JsonSerializer.Serialize(userData);
            userData["Pending2FA_UserType"] = userType.Value;
            userData["Pending2FA_StoredAt"] = DateTime.UtcNow.ToString("O");
            userData["Pending2FA_Protocol"] = "HTTP"; // Explicitly mark as HTTP-compatible

            // Try multiple storage strategies for better reliability in HTTP mode
            var stored = false;

            // Strategy 1: Try Context-Based first (most HTTP-compatible)
            var contextStrategy = _strategyManager.GetStrategy(AuthenticationMethod.ContextBased);
            if (contextStrategy?.IsAvailable == true)
            {
                var contextResult = await contextStrategy.StoreUserAsync(completeUser, userType);
                if (contextResult.IsSuccess)
                {
                    stored = true;
                    _logger.LogInformation("✅ Pending 2FA user stored in context strategy: {UserId}", user.Code);
                }
            }

            // Strategy 2: Try Circuit-Based as fallback
            if (!stored)
            {
                var circuitStrategy = _strategyManager.GetStrategy(AuthenticationMethod.CircuitBased);
                if (circuitStrategy?.IsAvailable == true)
                {
                    var circuitResult = await circuitStrategy.StoreUserAsync(completeUser, userType);
                    if (circuitResult.IsSuccess)
                    {
                        stored = true;
                        _logger.LogInformation("✅ Pending 2FA user stored in circuit strategy: {UserId}", user.Code);
                    }
                }
            }

            // Strategy 3: Try Session-Based as last resort (if HTTP protocol allows)
            if (!stored)
            {
                var sessionStrategy = _strategyManager.GetStrategy(AuthenticationMethod.SessionBased);
                if (sessionStrategy?.IsAvailable == true)
                {
                    var sessionResult = await sessionStrategy.StoreUserAsync(completeUser, userType);
                    if (sessionResult.IsSuccess)
                    {
                        stored = true;
                        _logger.LogInformation("✅ Pending 2FA user stored in session strategy: {UserId}", user.Code);
                    }
                }
            }

            if (!stored)
            {
                _logger.LogError("❌ Failed to store pending 2FA user in any strategy");
                throw new InvalidOperationException("2FA storage failed - no compatible strategy available");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to store pending 2FA user: {UserId}", user.Code);
            throw;
        }
    }

    /// <summary>
    /// Get pending 2FA user data
    /// Uses multiple strategies to retrieve temporary 2FA data with HTTP compatibility
    /// </summary>
    public (BaseUser User, SMSUserType UserType)? GetPending2FAUser()
    {
        try
        {
            _logger.LogDebug("🔍 Retrieving pending 2FA user from multiple strategies");

            // Try all strategies to find 2FA data
            var strategies = new[]
            {
                AuthenticationMethod.ContextBased,    // Try context first (most HTTP-compatible)
                AuthenticationMethod.CircuitBased,    // Then circuit
                AuthenticationMethod.SessionBased     // Finally session
            };

            foreach (var strategyMethod in strategies)
            {
                var strategy = _strategyManager.GetStrategy(strategyMethod);
                if (strategy?.IsAvailable == true)
                {
                    try
                    {
                        var result = strategy.RetrieveUserAsync().GetAwaiter().GetResult();
                        if (result.IsSuccess && result.Value.HasValue)
                        {
                            _logger.LogInformation("✅ Retrieved pending 2FA user from {Strategy}: {UserCode} ({UserType})", 
                                strategy.StrategyName, result.Value.Value.User.Code, result.Value.Value.UserType.Value);
                            return result.Value.Value;
                        }
                    }
                    catch (Exception strategyEx)
                    {
                        _logger.LogDebug(strategyEx, "Strategy {Strategy} failed to retrieve 2FA user", strategy.StrategyName);
                    }
                }
            }

            _logger.LogWarning("⚠️ No pending 2FA user data found in any strategy");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error retrieving pending 2FA user");
            return null;
        }
    }

    /// <summary>
    /// Clear pending 2FA user data
    /// Clears from all strategies for complete cleanup
    /// </summary>
    public async Task ClearPending2FAUserAsync()
    {
        try
        {
            _logger.LogInformation("🗑️ Clearing pending 2FA user data from all strategies");

            // Clear from all available strategies for complete cleanup
            var cleared = false;
            var strategies = new[]
            {
                AuthenticationMethod.ContextBased,
                AuthenticationMethod.CircuitBased,
                AuthenticationMethod.SessionBased
            };

            foreach (var strategyMethod in strategies)
            {
                var strategy = _strategyManager.GetStrategy(strategyMethod);
                if (strategy?.IsAvailable == true)
                {
                    try
                    {
                        var result = await strategy.ClearUserAsync();
                        if (result.IsSuccess)
                        {
                            cleared = true;
                            _logger.LogDebug("✅ Cleared 2FA data from {Strategy}", strategy.StrategyName);
                        }
                    }
                    catch (Exception strategyEx)
                    {
                        _logger.LogDebug(strategyEx, "Error clearing 2FA data from {Strategy}", strategy.StrategyName);
                    }
                }
            }

            if (cleared)
            {
                _logger.LogInformation("✅ Pending 2FA user data cleared successfully from available strategies");
            }
            else
            {
                _logger.LogWarning("⚠️ No strategies available for clearing 2FA data");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error clearing pending 2FA user data");
            throw;
        }
    }

    /// <summary>
    /// Check if there's a user pending 2FA verification
    /// </summary>
    public bool HasPending2FAUser()
    {
        try
        {
            return GetPending2FAUser() != null;
        }
        catch
        {
            return false;
        }
    }
}
