//-----------------------------------------------------------------------
// <copyright file="ContextBasedAuthenticationStrategy.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Context-based authentication strategy using HttpContext.Items.
//                  Emergency fallback method for request-scoped authentication storage.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SMS_Application.Configuration;
using SMS_Application.Interfaces;
using SMS_Domain.Enums;
using SMS_Domain.Errors;

namespace SMS_Application.Services.Authentication;

/// <summary>
/// Context-based authentication strategy using HttpContext.Items
/// Emergency fallback method for request-scoped authentication storage
/// Note: Data only persists for the current request lifecycle
/// </summary>
public class ContextBasedAuthenticationStrategy : IAuthenticationStrategy
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ContextBasedAuthenticationStrategy> _logger;
    private readonly IUserInstantiationService _userInstantiationService;
    private readonly IProtocolDetectionService _protocolDetectionService;

    public ContextBasedAuthenticationStrategy(
        IHttpContextAccessor httpContextAccessor,
        ILogger<ContextBasedAuthenticationStrategy> logger,
        IUserInstantiationService userInstantiationService,
        IProtocolDetectionService protocolDetectionService)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _userInstantiationService = userInstantiationService;
        _protocolDetectionService = protocolDetectionService;
    }

    public string StrategyName => "Context-Based Authentication";
    public AuthenticationMethod Method => AuthenticationMethod.ContextBased;

    public bool IsAvailable => _httpContextAccessor.HttpContext != null;

    /// <summary>
    /// Store complete user authentication data in HttpContext.Items
    /// WARNING: Only persists for current request - not suitable for long-term auth
    /// </summary>
    public async Task<Result<bool>> StoreUserAsync(BaseUser user, SMSUserType userType, CancellationToken cancellationToken = default)
    {
        return await StoreUserAsync(user, userType, null, cancellationToken);
    }

    /// <summary>
    /// Store complete user authentication data in HttpContext.Items with optional additional data
    /// WARNING: Only persists for current request - not suitable for long-term auth
    /// ENHANCED: Accepts additional data for 2FA markers and other temporary storage needs
    /// </summary>
    public async Task<Result<bool>> StoreUserAsync(BaseUser user, SMSUserType userType, Dictionary<string, string>? additionalData, CancellationToken cancellationToken = default)
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
            {
                _logger.LogWarning("? HttpContext not available for ContextBasedAuthenticationStrategy");
                return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
            }

            _logger.LogInformation("?? Storing user {UserCode} ({UserType}) in HttpContext.Items for 2FA", user.Code, userType.Value);

            // Get serialized user data with all roles/permissions
            var userData = _userInstantiationService.SerializeCompleteUser(user, userType);

            // Add strategy-specific data
            userData["SMS_AuthMethod"] = "Context";
            userData["SMS_AuthStrategy"] = StrategyName;
            userData["SMS_StoredAt"] = DateTime.UtcNow.ToString("O");
            userData["SMS_RequestId"] = context.TraceIdentifier ?? Guid.NewGuid().ToString();

            // ?? CRITICAL: Add any additional data (like 2FA markers)
            if (additionalData != null)
            {
                foreach (var kvp in additionalData)
                {
                    userData[kvp.Key] = kvp.Value;
                    _logger.LogDebug("?? Added additional data to context: {Key} = {Value}", kvp.Key, kvp.Value?.Length > 50 ? $"{kvp.Value[..50]}..." : kvp.Value);
                }
            }

            // Store all data in context items
            foreach (var kvp in userData)
            {
                context.Items[kvp.Key] = kvp.Value;
            }

            // Store keys for cleanup tracking
            context.Items["SMS_AUTH_KEYS"] = string.Join("|", userData.Keys);

            _logger.LogInformation("? SUCCESS: User {UserCode} ({UserType}) stored in HttpContext.Items with {FieldCount} fields (REQUEST-SCOPED)", 
                user.Code, userType.Value, userData.Count);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error storing user {UserCode} in HttpContext.Items", user.Code);
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    /// <summary>
    /// Retrieve user authentication data from HttpContext.Items
    /// </summary>
    public async Task<Result<(BaseUser User, SMSUserType UserType)?>> RetrieveUserAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
            {
                _logger.LogDebug("?? HttpContext not available for user retrieval");
                return Result<(BaseUser, SMSUserType)?>.Success((ValueTuple<BaseUser, SMSUserType>?)null);
            }

            var isAuthenticated = context.Items["IsAuthenticated"]?.ToString();
            if (isAuthenticated != "true")
            {
                _logger.LogDebug("?? No authenticated user in HttpContext.Items");
                return Result<(BaseUser, SMSUserType)?>.Success((ValueTuple<BaseUser, SMSUserType>?)null);
            }

            var userCode = context.Items["SMS_UserCode"]?.ToString();
            var userTypeValue = context.Items["SMS_UserType"]?.ToString();

            if (string.IsNullOrEmpty(userCode) || string.IsNullOrEmpty(userTypeValue))
            {
                _logger.LogWarning("?? Incomplete user data in HttpContext.Items - UserCode: {UserCode}, UserType: {UserType}", 
                    userCode ?? "NULL", userTypeValue ?? "NULL");
                return Result<(BaseUser, SMSUserType)?>.Success((ValueTuple<BaseUser, SMSUserType>?)null);
            }

            _logger.LogInformation("?? Retrieving user {UserCode} ({UserType}) from HttpContext.Items", userCode, userTypeValue);

            // Build user data dictionary from context items
            var userData = new Dictionary<string, string>();
            var authKeysStr = context.Items["SMS_AUTH_KEYS"]?.ToString();
            
            if (!string.IsNullOrEmpty(authKeysStr))
            {
                var authKeys = authKeysStr.Split('|');
                foreach (var key in authKeys)
                {
                    var value = context.Items[key]?.ToString();
                    if (!string.IsNullOrEmpty(value))
                    {
                        userData[key] = value;
                    }
                }
            }
            else
            {
                // Fallback: scan for SMS_ prefixed items
                foreach (var item in context.Items)
                {
                    if (item.Key is string keyStr && 
                        (keyStr.StartsWith("SMS_") || keyStr == "IsAuthenticated") &&
                        item.Value?.ToString() is string value)
                    {
                        userData[keyStr] = value;
                    }
                }
            }

            if (!userData.Any())
            {
                _logger.LogWarning("?? No user data found in HttpContext.Items");
                return Result<(BaseUser, SMSUserType)?>.Success((ValueTuple<BaseUser, SMSUserType>?)null);
            }

            // Deserialize user via UserInstantiationService
            var result = await _userInstantiationService.DeserializeCompleteUserAsync(userData, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("? User {UserCode} ({UserType}) retrieved successfully from HttpContext.Items", 
                    userCode, userTypeValue);
                return Result<(BaseUser, SMSUserType)?>.Success((ValueTuple<BaseUser, SMSUserType>?)result.Value);
            }
            else
            {
                _logger.LogWarning("?? Failed to deserialize user {UserCode} from HttpContext.Items: {Error}", 
                    userCode, result.Error?.Message);
                return Result<(BaseUser, SMSUserType)?>.Success((ValueTuple<BaseUser, SMSUserType>?)null);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error retrieving user from HttpContext.Items");
            return Result<(BaseUser, SMSUserType)?>.Success((ValueTuple<BaseUser, SMSUserType>?)null);
        }
    }

    /// <summary>
    /// Clear user authentication data from HttpContext.Items
    /// </summary>
    public async Task<Result<bool>> ClearUserAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
            {
                _logger.LogDebug("?? HttpContext not available for clearing");
                return Result<bool>.Success(true);
            }

            var userCode = context.Items["SMS_UserCode"]?.ToString() ?? "Unknown";
            _logger.LogInformation("??? Clearing user {UserCode} from HttpContext.Items", userCode);

            // Method 1: Use tracked keys for precise cleanup
            var authKeysStr = context.Items["SMS_AUTH_KEYS"]?.ToString();
            if (!string.IsNullOrEmpty(authKeysStr))
            {
                var keysToRemove = authKeysStr.Split('|').ToList();
                keysToRemove.Add("SMS_AUTH_KEYS"); // Remove the tracker itself

                foreach (var key in keysToRemove)
                {
                    context.Items.Remove(key);
                }
                
                _logger.LogDebug("??? Cleared {Count} tracked context items", keysToRemove.Count);
            }
            else
            {
                // Method 2: Pattern-based cleanup as fallback
                var keysToRemove = context.Items.Keys
                    .Where(key => key is string keyStr && 
                                 (keyStr.StartsWith("SMS_") || keyStr == "IsAuthenticated" || 
                                  keyStr.Contains("CIRCUIT_ID") || keyStr.StartsWith("Pending2FA_")))
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    context.Items.Remove(key);
                }
                
                _logger.LogDebug("??? Cleared {Count} pattern-matched context items", keysToRemove.Count);
            }

            _logger.LogInformation("? HttpContext.Items cleared successfully for user {UserCode}", userCode);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error clearing HttpContext.Items");
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    /// <summary>
    /// Check if user is currently authenticated via context items
    /// </summary>
    public async Task<bool> IsUserAuthenticatedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return false;

            var isAuthenticated = context.Items["IsAuthenticated"]?.ToString();
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
            return context?.Items["SMS_UserCode"]?.ToString();
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
            return context?.Items["SMS_DisplayName"]?.ToString();
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
        var requestId = context?.TraceIdentifier ?? "Unknown";
        return $"Context Storage - {protocolInfo}, RequestId: {requestId[..Math.Min(8, requestId.Length)]}..., Scope: Request-Only";
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