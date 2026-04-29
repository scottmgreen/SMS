//-----------------------------------------------------------------------
// <copyright file="SessionBasedAuthenticationStrategy.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Session-based authentication strategy using HTTP Session cookies.
//                  Preferred method for HTTPS environments with proper security.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SMS_Application.Configuration;
using SMS_Application.Interfaces;
using SMS_Domain.Enums;
using SMS_Domain.Errors;

namespace Application.Services.Strategies;

/// <summary>
/// Session-based authentication strategy using HTTP Session cookies
/// Preferred method for HTTPS environments with proper security
/// </summary>
public class SessionBasedAuthenticationStrategy : IAuthenticationStrategy
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<SessionBasedAuthenticationStrategy> _logger;
    private readonly IUserInstantiationService _userInstantiationService;
    private readonly IProtocolDetectionService _protocolDetectionService;

    public SessionBasedAuthenticationStrategy(
        IHttpContextAccessor httpContextAccessor,
        ILogger<SessionBasedAuthenticationStrategy> logger,
        IUserInstantiationService userInstantiationService,
        IProtocolDetectionService protocolDetectionService)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _userInstantiationService = userInstantiationService;
        _protocolDetectionService = protocolDetectionService;
    }

    public string StrategyName => "Session-Based Authentication";
    public AuthenticationMethod Method => AuthenticationMethod.SessionBased;

    public bool IsAvailable => _httpContextAccessor.HttpContext?.Session != null && 
                              !(_httpContextAccessor.HttpContext?.Response.HasStarted ?? true) &&
                              _protocolDetectionService.CanUseSessionAuth();

    /// <summary>
    /// Store complete user authentication data in HTTP Session
    /// </summary>
    public async Task<Result<bool>> StoreUserAsync(BaseUser user, SMSUserType userType, CancellationToken cancellationToken = default)
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.Session == null)
            {
                _logger.LogWarning("? Session not available for SessionBasedAuthenticationStrategy");
                return Result.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
            }

            if (context.Response.HasStarted)
            {
                _logger.LogWarning("? Response has already started - cannot write to session");
                return Result.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
            }

            _logger.LogInformation("?? Storing user {UserCode} ({UserType}) in session", user.Code, userType.Value);

            await context.Session.LoadAsync(cancellationToken);

            // Get serialized user data with all roles/permissions
            var userData = _userInstantiationService.SerializeCompleteUser(user, userType);

            // Store all data in session
            foreach (var kvp in userData)
            {
                context.Session.SetString(kvp.Key, kvp.Value);
            }

            // Add strategy identifier
            context.Session.SetString("SMS_AuthMethod", "Session");
            context.Session.SetString("SMS_AuthStrategy", StrategyName);

            await context.Session.CommitAsync(cancellationToken);

            _logger.LogInformation("? User {UserCode} ({UserType}) stored successfully in session with {FieldCount} fields", 
                user.Code, userType.Value, userData.Count);

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error storing user {UserCode} in session", user.Code);
            return Result.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    /// <summary>
    /// Retrieve user authentication data from HTTP Session
    /// </summary>
    public async Task<Result<(BaseUser User, SMSUserType UserType)?>> RetrieveUserAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.Session == null)
            {
                _logger.LogDebug("?? Session not available for user retrieval");
                return Result.Success((ValueTuple<BaseUser, SMSUserType>?)null);
            }

            var isAuthenticated = context.Session.GetString("IsAuthenticated");
            if (isAuthenticated != "true")
            {
                _logger.LogDebug("?? No authenticated user in session");
                return Result.Success((ValueTuple<BaseUser, SMSUserType>?)null);
            }

            var userCode = context.Session.GetString("SMS_UserCode");
            var userTypeValue = context.Session.GetString("SMS_UserType");

            if (string.IsNullOrEmpty(userCode) || string.IsNullOrEmpty(userTypeValue))
            {
                _logger.LogWarning("?? Incomplete user data in session - UserCode: {UserCode}, UserType: {UserType}", 
                    userCode ?? "NULL", userTypeValue ?? "NULL");
                return Result.Success((ValueTuple<BaseUser, SMSUserType>?)null);
            }

            _logger.LogInformation("?? Retrieving user {UserCode} ({UserType}) from session", userCode, userTypeValue);

            // Build user data dictionary from session
            var userData = new Dictionary<string, string>();
            var sessionKeys = new[] 
            {
                "SMS_UserId", "SMS_UserCode", "SMS_UserType", "SMS_DisplayName", "SMS_Email",
                "SMS_FirstName", "SMS_LastName", "IsAuthenticated", "SMS_LoginTime", 
                "SMS_UserRoleCode", "SMS_UserRoleName", "SMS_UserPermissions", "SMS_UserPermissionsJson",
                "SMS_TwoFactorEnabled", "SMS_TwoFactorSecretKey", "SMS_FailedTwoFactorAttempts",
                "SMS_LastLoginDate", "SMS_IsActive", "SMS_SerializedAt", "SMS_CompletenessScore",
                "SMS_ApplicationUserCode", "SMS_Department", "SMS_Position", "SMS_OrganizationLevel",
                "SMS_Organization", "SMS_StakeholderType"
            };

            foreach (var key in sessionKeys)
            {
                var value = context.Session.GetString(key);
                if (!string.IsNullOrEmpty(value))
                {
                    userData[key] = value;
                }
            }

            // Deserialize user via UserInstantiationService
            var result = await _userInstantiationService.DeserializeCompleteUserAsync(userData, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("? User {UserCode} ({UserType}) retrieved successfully from session", 
                    userCode, userTypeValue);
                return Result.Success((ValueTuple<BaseUser, SMSUserType>?)result.Value);
            }
            else
            {
                _logger.LogWarning("?? Failed to deserialize user {UserCode} from session: {Error}", 
                    userCode, result.Error?.Message);
                return Result.Success((ValueTuple<BaseUser, SMSUserType>?)null);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error retrieving user from session");
            return Result.Success((ValueTuple<BaseUser, SMSUserType>?)null);
        }
    }

    /// <summary>
    /// Clear user authentication data from HTTP Session
    /// </summary>
    public async Task<Result<bool>> ClearUserAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.Session == null)
            {
                _logger.LogDebug("?? Session not available for clearing");
                return Result.Success(true);
            }

            var userCode = context.Session.GetString("SMS_UserCode");
            _logger.LogInformation("??? Clearing user {UserCode} from session", userCode ?? "Unknown");

            context.Session.Clear();
            
            _logger.LogInformation("? Session cleared successfully for user {UserCode}", userCode ?? "Unknown");
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error clearing session");
            return Result.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    /// <summary>
    /// Check if user is currently authenticated via session
    /// </summary>
    public async Task<bool> IsUserAuthenticatedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.Session == null) return false;

            var isAuthenticated = context.Session.GetString("IsAuthenticated");
            return isAuthenticated == "true";
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get current user ID if authenticated
    /// </summary>
    public async Task<string?> GetCurrentUserIdAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            return context?.Session?.GetString("SMS_UserCode");
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Get current user display name if authenticated
    /// </summary>
    public async Task<string?> GetCurrentUserDisplayNameAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            return context?.Session?.GetString("SMS_DisplayName");
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Get storage method information for logging/debugging
    /// </summary>
    public string GetStorageInfo()
    {
        var context = _httpContextAccessor.HttpContext;
        var protocolInfo = _protocolDetectionService.GetProtocolInfo();
        var sessionId = context?.Session?.Id ?? "Unknown";
        return $"Session Storage - {protocolInfo}, SessionId: {sessionId[..Math.Min(8, sessionId.Length)]}...";
    }

    /// <summary>
    /// Validate stored authentication data integrity
    /// </summary>
    public async Task<bool> ValidateStoredDataAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await RetrieveUserAsync(cancellationToken);
            return result.IsSuccess && result.Value.HasValue;
        }
        catch
        {
            return false;
        }
    }
}