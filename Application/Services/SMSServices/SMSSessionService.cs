//-----------------------------------------------------------------------
// <copyright file="SMSSessionService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: REFACTORED SMS Session Service using Authentication Strategy Manager.
//                  Clean, explicit authentication method selection with CQRS-based user loading.
//                  Replaces mixed Session/Circuit/Context approach with strategy pattern.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Application.Configuration;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

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
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly SessionConfiguration _sessionConfig;
    private readonly TwoFactorAuthConfiguration _twoFactorConfig;

    public SMSSessionService(
        ILogger<SMSSessionService> logger,
        IAuthenticationStrategyManager strategyManager,
        IUserInstantiationService userInstantiationService,
        IHttpContextAccessor httpContextAccessor,
        SessionConfiguration sessionConfig,
        TwoFactorAuthConfiguration twoFactorConfig)
    {
        _logger = logger;
        _strategyManager = strategyManager;
        _userInstantiationService = userInstantiationService;
        _httpContextAccessor = httpContextAccessor;
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
            _logger.LogApplicationInformation("Creating SMS session for user {UserCode} ({UserType}) using strategy manager", user.Code, userType.Value);

            _logger.LogApplicationInformation("Ensuring user completeness before session creation...");
            
            var completeUserResult = await _userInstantiationService.EnsureUserCompletenessAsync(user, userType);
            if (completeUserResult.IsFailure)
            {
                _logger.LogApplicationError("Failed to ensure user completeness: {UserCode}: {Error}", user.Code, completeUserResult.Error?.Message);
                throw new InvalidOperationException($"User {user.Code} could not be fully instantiated: {completeUserResult.Error?.Message}");
            }

            var completeUser = completeUserResult.Value;
            _logger.LogApplicationInformation("User completeness ensured - Role: {RoleCode}, Permissions: {PermissionCount}", completeUser.UserRole?.Code ?? "None", completeUser.UserRole?.Permissions?.Count ?? 0);

            // Final validation of user completeness
            if (!_userInstantiationService.IsUserComplete(completeUser))
            {
                var missing = _userInstantiationService.GetMissingComponents(completeUser);
                _logger.LogApplicationError("User {UserCode} is incomplete - Missing: {MissingComponents}", user.Code, string.Join(", ", missing));
                throw new InvalidOperationException($"User {user.Code} is incomplete - missing: {string.Join(", ", missing)}");
            }

            // Store user via authentication strategy manager
            var storeResult = await _strategyManager.StoreUserAsync(completeUser, userType);
            if (storeResult.IsFailure)
            {
                _logger.LogApplicationError("Failed to store user {UserCode} via strategy manager: {Error}", user.Code, storeResult.Error?.Message);
                throw new InvalidOperationException($"Failed to store user {user.Code}: {storeResult.Error?.Message}");
            }

            // Log successful authentication with strategy details
            var primaryStrategy = _strategyManager.PrimaryStrategy;

            if (_sessionConfig.LogSessionActivity)
            {
                _logger.LogApplicationInformation("SMS Session created successfully for user {UserCode} ({UserType}) - Primary: {PrimaryStrategy}, Completeness: 100%", user.Code, userType.Value, primaryStrategy.StrategyName);
            }
            else
            {
                _logger.LogApplicationInformation("SMS Session created successfully for user {UserCode} ({UserType})", user.Code, userType.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error creating SMS session for user {UserCode} ({UserType})", user.Code, userType.Value);
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
            
            _logger.LogApplicationInformation("Clearing SMS session for user: {UserId}", userId);

            // Clear using strategy manager (clears all strategies)
            var clearResult = await _strategyManager.ClearUserAsync();
            
            if (clearResult.IsSuccess)
            {
                if (_sessionConfig.LogSessionActivity)
                {
                    _logger.LogApplicationInformation("SMS Session cleared successfully for user: {UserId} - All strategies cleared", userId);
                }
                else
                {
                    _logger.LogApplicationInformation("SMS Session cleared successfully for user: {UserId}", userId);
                }
            }
            else
            {
                _logger.LogApplicationWarning("Some strategies may not have been cleared for user: {UserId}", userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error clearing SMS session");
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
            _logger.LogApplicationError(ex, "Error getting current user ID");
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
            _logger.LogApplicationError(ex, "Error getting current user display name");
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
            _logger.LogApplicationError(ex, "Error getting current user type");
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
            _logger.LogApplicationError(ex, "Error checking authentication status");
            return false;
        }
    }

    // ?? Two-Factor Authentication Session Methods
    // Session-first implementation for Blazor Server runtime path simplification

    /// <summary>
    /// Store user temporarily for 2FA verification after password authentication.
    /// Stores pending 2FA state in the current HTTP session.
    /// </summary>
    public async Task StorePending2FAUserAsync(BaseUser user, SMSUserType userType)
    {
        try
        {
            _logger.LogApplicationInformation("Storing pending 2FA user: {UserId} ({UserType})", user.Code, userType.Value);

            await ClearPending2FAUserAsync();

            var completeUserResult = await _userInstantiationService.EnsureUserCompletenessAsync(user, userType);
            if (completeUserResult.IsFailure)
            {
                _logger.LogApplicationError("Failed to ensure user completeness for 2FA: {UserCode}: {Error}",
                    user.Code, completeUserResult.Error?.Message);
                throw new InvalidOperationException($"User {user.Code} could not be fully instantiated for 2FA: {completeUserResult.Error?.Message}");
            }

            var completeUser = completeUserResult.Value;
            var userData = _userInstantiationService.SerializeCompleteUser(completeUser, userType);

            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null)
            {
                throw new InvalidOperationException("Session is not available to store pending 2FA data.");
            }

            await session.LoadAsync();
            session.SetString("Pending2FA_UserData", JsonSerializer.Serialize(userData));
            session.SetString("Pending2FA_UserType", userType.Value);
            session.SetString("Pending2FA_StoredAt", DateTime.UtcNow.ToString("O"));
            await session.CommitAsync();

            _logger.LogApplicationInformation("2FA pending state stored in session for user: {UserId} ({UserType})", user.Code, userType.Value);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "EXCEPTION in StorePending2FAUserAsync for user: {UserId}", user.Code);
            throw;
        }
    }

    /// <summary>
    /// Get pending 2FA user data
    /// Uses session-backed pending 2FA data.
    /// </summary>
    public (BaseUser User, SMSUserType UserType)? GetPending2FAUser()
    {
        try
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            var pendingJson = session?.GetString("Pending2FA_UserData");
            var pendingUserType = session?.GetString("Pending2FA_UserType");

            if (string.IsNullOrWhiteSpace(pendingJson))
            {
                return null;
            }

            var userData = JsonSerializer.Deserialize<Dictionary<string, string>>(pendingJson);
            if (userData == null || userData.Count == 0)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(pendingUserType) && !userData.ContainsKey("SMS_UserType"))
            {
                userData["SMS_UserType"] = pendingUserType;
            }

            var result = _userInstantiationService.DeserializeCompleteUserAsync(userData).GetAwaiter().GetResult();
            if (result.IsSuccess)
            {
                return result.Value;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error retrieving pending 2FA user");
            return null;
        }
    }

    /// <summary>
    /// Clear pending 2FA user data
    /// Clears pending 2FA values from the current session.
    /// </summary>
    public async Task ClearPending2FAUserAsync()
    {
        try
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null)
            {
                return;
            }

            await session.LoadAsync();
            session.Remove("Pending2FA_UserData");
            session.Remove("Pending2FA_UserType");
            session.Remove("Pending2FA_StoredAt");
            await session.CommitAsync();

            _logger.LogApplicationInformation("Pending 2FA user data cleared from session");
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error clearing pending 2FA user data");
            throw;
        }
    }

    /// <summary>
    /// Check if there's a user pending 2FA verification
    /// Returns true when pending 2FA markers are present in session.
    /// </summary>
    public bool HasPending2FAUser()
    {
        try
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            var pendingJson = session?.GetString("Pending2FA_UserData");
            return !string.IsNullOrWhiteSpace(pendingJson);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error checking for pending 2FA user");
            return false;
        }
    }
}



