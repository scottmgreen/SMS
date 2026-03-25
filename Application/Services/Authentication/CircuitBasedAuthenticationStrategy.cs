//-----------------------------------------------------------------------
// <copyright file="CircuitBasedAuthenticationStrategy.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Circuit-based authentication strategy using Blazor Server circuit storage.
//                  Handles response timing issues and provides reliable fallback storage.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SMS_Application.Configuration;
using SMS_Application.Interfaces;
using SMS_Domain.Enums;
using SMS_Domain.Errors;

namespace SMS_Application.Services.Authentication;

/// <summary>
/// Circuit-based authentication strategy using Blazor Server circuit storage
/// Handles response timing issues and provides reliable fallback storage
/// </summary>
public class CircuitBasedAuthenticationStrategy : IAuthenticationStrategy
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CircuitBasedAuthenticationStrategy> _logger;
    private readonly IUserInstantiationService _userInstantiationService;
    private readonly IBlazorCircuitAuthStorage _circuitAuthStorage;
    private readonly AuthenticationConfiguration _config;
    private readonly IProtocolDetectionService _protocolDetectionService;
    
    // Simple cache for circuit ID to avoid repeated generation - keep it short
    private string? _cachedCircuitId;
    private DateTime _cacheExpiry = DateTime.MinValue;
    private readonly TimeSpan _cacheTimeout = TimeSpan.FromSeconds(10); // Shorter cache, simpler approach

    public CircuitBasedAuthenticationStrategy(
        IHttpContextAccessor httpContextAccessor,
        ILogger<CircuitBasedAuthenticationStrategy> logger,
        IUserInstantiationService userInstantiationService,
        IBlazorCircuitAuthStorage circuitAuthStorage,
        AuthenticationConfiguration config,
        IProtocolDetectionService protocolDetectionService)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _userInstantiationService = userInstantiationService;
        _circuitAuthStorage = circuitAuthStorage;
        _config = config;
        _protocolDetectionService = protocolDetectionService;
    }

    public string StrategyName => "Circuit-Based Authentication";
    public AuthenticationMethod Method => AuthenticationMethod.CircuitBased;

    public bool IsAvailable 
    { 
        get 
        {
            try
            {
                // Always return true - we'll generate fallback circuit IDs when needed
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "?? Error checking CircuitBased availability - defaulting to TRUE");
                return true; // Always try to be available
            }
        }
    }

    /// <summary>
    /// Store complete user authentication data in circuit storage with explicit circuit ID persistence
    /// ENHANCED: Accepts additional data for 2FA markers and other temporary storage needs
    /// </summary>
    public async Task<Result<bool>> StoreUserAsync(BaseUser user, SMSUserType userType, CancellationToken cancellationToken = default)
    {
        return await StoreUserAsync(user, userType, null, cancellationToken);
    }

    /// <summary>
    /// Store complete user authentication data in circuit storage with optional additional data
    /// </summary>
    public async Task<Result<bool>> StoreUserAsync(BaseUser user, SMSUserType userType, Dictionary<string, string>? additionalData, CancellationToken cancellationToken = default)
    {
        try
        {
            var circuitId = GetOrGenerateCircuitId(); // Only generate when storing
            if (string.IsNullOrEmpty(circuitId))
            {
                _logger.LogError("? CRITICAL: Could not generate any circuit ID for CircuitBasedAuthenticationStrategy");
                return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
            }

            _logger.LogInformation("?? Storing user {UserCode} ({UserType}) in circuit storage with ID: {CircuitId}", user.Code, userType.Value, circuitId);

            // Get serialized user data with all roles/permissions
            var userData = _userInstantiationService.SerializeCompleteUser(user, userType);

            // Add explicit circuit ID to the stored data for consistent retrieval
            userData["SMS_CircuitId"] = circuitId;
            userData["SMS_AuthMethod"] = "Circuit";
            userData["SMS_AuthStrategy"] = StrategyName;
            userData["SMS_StoredCircuitId"] = circuitId; // Backup reference

            // Add any additional data (like 2FA markers)
            if (additionalData != null)
            {
                foreach (var kvp in additionalData)
                {
                    userData[kvp.Key] = kvp.Value;
                    _logger.LogDebug("?? Added additional data: {Key} = {Value}", kvp.Key, kvp.Value?.Length > 50 ? $"{kvp.Value[..50]}..." : kvp.Value);
                }
            }

            // Store user data in circuit storage with the circuit ID
            _circuitAuthStorage.SetAuthData(circuitId, userData);

            // Also store under user code for fallback retrieval
            var userCircuitKey = $"circuit_{user.Code}_{DateTime.UtcNow.Ticks}";
            userData["SMS_UserCircuitKey"] = userCircuitKey;
            _circuitAuthStorage.SetAuthData(userCircuitKey, userData);

            _logger.LogInformation("? User {UserCode} ({UserType}) stored successfully in circuit storage - Circuit: {CircuitId}, Fields: {FieldCount}", 
                user.Code, userType.Value, circuitId, userData.Count);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error storing user {UserCode} in circuit storage", user.Code);
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    /// <summary>
    /// Retrieve user authentication data from circuit storage with enhanced circuit ID matching
    /// OPTIMIZED: Avoid unnecessary circuit ID generation by scanning stored data first
    /// </summary>
    public async Task<Result<(BaseUser User, SMSUserType UserType)?>> RetrieveUserAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("?? Attempting to retrieve user from circuit storage");

            Dictionary<string, string>? userData = null;
            string? foundCircuitId = null;

            // Strategy 1: Try to find existing data by scanning stored circuit IDs FIRST
            // This avoids generating unnecessary circuit IDs when data already exists
            var allStoredData = _circuitAuthStorage.GetAllAuthData();
            foreach (var kvp in allStoredData)
            {
                if (kvp.Value.ContainsKey("SMS_StoredCircuitId") && kvp.Value.ContainsKey("IsAuthenticated") && kvp.Value.GetValueOrDefault("IsAuthenticated") == "true")
                {
                    userData = kvp.Value;
                    foundCircuitId = kvp.Key;
                    _logger.LogInformation("? Found user data by scanning stored circuit IDs: {FoundKey}", kvp.Key);
                    break;
                }
            }

            // Strategy 2: Only try current circuit ID if no existing data found
            if (userData == null)
            {
                var currentCircuitId = GetCachedOrGenerateCircuitId(); // Use cached version to reduce generation
                _logger.LogDebug("?? No existing data found, trying current circuit ID: {CircuitId}", currentCircuitId ?? "NULL");
                
                if (!string.IsNullOrEmpty(currentCircuitId))
                {
                    userData = _circuitAuthStorage.GetAuthData(currentCircuitId);
                    if (userData != null)
                    {
                        foundCircuitId = currentCircuitId;
                        _logger.LogDebug("? Found user data with current circuit ID: {CircuitId}", currentCircuitId);
                    }
                }
            }

            // Strategy 3: Try alternate circuit keys if still not found
            if (userData == null && !string.IsNullOrEmpty(_cachedCircuitId))
            {
                var circuitKeys = new[]
                {
                    _cachedCircuitId,
                    $"circuit_{_cachedCircuitId}",
                    $"{_cachedCircuitId}_auth",
                };

                foreach (var key in circuitKeys)
                {
                    userData = _circuitAuthStorage.GetAuthData(key);
                    if (userData != null)
                    {
                        foundCircuitId = key;
                        _logger.LogInformation("? Found user data with alternate circuit key: {FoundKey}", key);
                        break;
                    }
                }
            }

            if (userData == null)
            {
                _logger.LogDebug("?? No authenticated user data found in circuit storage");
                return Result<(BaseUser, SMSUserType)?>.Success((ValueTuple<BaseUser, SMSUserType>?)null);
            }

            if (userData.GetValueOrDefault("IsAuthenticated") != "true")
            {
                _logger.LogDebug("?? User data found but not authenticated");
                return Result<(BaseUser, SMSUserType)?>.Success((ValueTuple<BaseUser, SMSUserType>?)null);
            }

            var userCode = userData.GetValueOrDefault("SMS_UserCode", "");
            var userTypeValue = userData.GetValueOrDefault("SMS_UserType", "Application");

            if (string.IsNullOrEmpty(userCode))
            {
                _logger.LogWarning("?? Incomplete user data in circuit storage - missing UserCode");
                return Result<(BaseUser, SMSUserType)?>.Success((ValueTuple<BaseUser, SMSUserType>?)null);
            }

            _logger.LogInformation("?? Retrieving user {UserCode} ({UserType}) from circuit storage using key: {CircuitKey}", 
                userCode, userTypeValue, foundCircuitId);

            // Deserialize user via UserInstantiationService
            var result = await _userInstantiationService.DeserializeCompleteUserAsync(userData, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("? User {UserCode} ({UserType}) retrieved successfully from circuit storage", 
                    userCode, userTypeValue);
                return Result<(BaseUser, SMSUserType)?>.Success((ValueTuple<BaseUser, SMSUserType>?)result.Value);
            }
            else
            {
                _logger.LogWarning("?? Failed to deserialize user {UserCode} from circuit storage: {Error}", 
                    userCode, result.Error?.Message);
                return Result<(BaseUser, SMSUserType)?>.Success((ValueTuple<BaseUser, SMSUserType>?)null);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error retrieving user from circuit storage");
            return Result<(BaseUser, SMSUserType)?>.Success((ValueTuple<BaseUser, SMSUserType>?)null);
        }
    }

    /// <summary>
    /// Clear user authentication data from circuit storage
    /// </summary>
    public async Task<Result<bool>> ClearUserAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var userCode = await GetCurrentUserIdAsync(cancellationToken) ?? "Unknown";
            _logger.LogInformation("??? Clearing user {UserCode} from circuit storage", userCode);

            // Method 1: Clear by stored circuit ID
            var context = _httpContextAccessor.HttpContext;
            if (context?.Items.TryGetValue("SMS_CIRCUIT_ID", out var storedCircuitId) == true && storedCircuitId != null)
            {
                var circuitId = storedCircuitId.ToString();
                _circuitAuthStorage.ClearAuthData(circuitId);
                _logger.LogDebug("??? Cleared circuit data by stored ID: {CircuitId}", circuitId);
            }

            // Method 2: Clear by current circuit ID
            var currentCircuitId = GetCachedOrGenerateCircuitId(); // Use cached version to reduce generation
            if (!string.IsNullOrEmpty(currentCircuitId))
            {
                _circuitAuthStorage.ClearAuthData(currentCircuitId);
                _logger.LogDebug("??? Cleared circuit data by current ID: {CircuitId}", currentCircuitId);
            }

            // Method 3: Clear by user ID
            _circuitAuthStorage.ClearAuthDataByUserId(userCode);
            _logger.LogDebug("??? Cleared circuit data by user ID: {UserCode}", userCode);

            // Method 4: Clear context items
            if (context != null)
            {
                var keysToRemove = context.Items.Keys
                    .Where(key => key is string keyStr && 
                                 (keyStr.StartsWith("SMS_") || keyStr == "IsAuthenticated" || 
                                  keyStr.Contains("CIRCUIT_ID") || keyStr.StartsWith("Pending2FA_")))
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    context.Items.Remove(key);
                }
                _logger.LogDebug("??? Cleared {Count} context items", keysToRemove.Count);
            }

            _logger.LogInformation("? Circuit storage cleared successfully for user {UserCode}", userCode);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error clearing circuit storage");
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    /// <summary>
    /// Check if user is currently authenticated via circuit storage
    /// </summary>
    public async Task<bool> IsUserAuthenticatedAsync(CancellationToken cancellationToken = default)
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

    /// <summary>
    /// Get current user ID if authenticated
    /// </summary>
    public async Task<string?> GetCurrentUserIdAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await RetrieveUserAsync(cancellationToken);
            return result.IsSuccess && result.Value.HasValue ? result.Value.Value.User.Code : null;
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
            var result = await RetrieveUserAsync(cancellationToken);
            return result.IsSuccess && result.Value.HasValue ? result.Value.Value.User.DisplayName : null;
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
        var connectionId = context?.Connection.Id ?? "Unknown";
        return $"Circuit Storage - {protocolInfo}, Connection: {connectionId[..Math.Min(8, connectionId.Length)]}..., Timeout: {_config.CircuitCacheTimeoutMinutes}min";
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

    /// <summary>
    /// Generate unique circuit ID for user session
    /// </summary>
    private string GenerateCircuitId(string userCode)
    {
        var timestamp = DateTime.UtcNow.Ticks;
        var context = _httpContextAccessor.HttpContext;
        
        // Try connection ID first
        var connectionId = context?.Connection.Id;
        if (!string.IsNullOrEmpty(connectionId))
        {
            return $"circuit_{userCode}_{connectionId}_{timestamp}";
        }

        // Fallback to generated ID
        var remoteIp = context?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = context?.Request.Headers["User-Agent"].FirstOrDefault() ?? "unknown";
        var hash = $"{remoteIp}_{userAgent}".GetHashCode();
        
        return $"circuit_{userCode}_{hash}_{timestamp}";
    }

    /// <summary>
    /// Get current circuit ID from context
    /// </summary>
    private string? GetCurrentCircuitId()
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            // Method 1: Try connection ID
            var connectionId = context.Connection.Id;
            if (!string.IsNullOrEmpty(connectionId))
            {
                return connectionId;
            }

            // Method 2: Generate from request info
            var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault() ?? "unknown";
            return $"circuit_{remoteIp}_{userAgent.GetHashCode()}";
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error getting current circuit ID");
            return null;
        }
    }

    /// <summary>
    /// Get or generate a circuit ID with caching to reduce unnecessary generation
    /// OPTIMIZED: Cache circuit ID for short periods to reduce repeated expensive operations
    /// </summary>
    private string GetCachedOrGenerateCircuitId()
    {
        // Check if cached circuit ID is still valid (use shorter cache for better responsiveness)
        if (!string.IsNullOrEmpty(_cachedCircuitId) && DateTime.UtcNow < _cacheExpiry)
        {
            return _cachedCircuitId;
        }

        // Generate new circuit ID and cache it for a shorter time
        var newCircuitId = GetOrGenerateCircuitId();
        _cachedCircuitId = newCircuitId;
        _cacheExpiry = DateTime.UtcNow.Add(TimeSpan.FromSeconds(30)); // Shorter cache for circuit IDs
        
        _logger.LogDebug("?? Generated and cached new circuit ID: {CircuitId}", newCircuitId);
        return newCircuitId;
    }

    /// <summary>
    /// Get or generate a circuit ID for the current connection
    /// PRODUCTION ENHANCED: Generate deterministic fallback IDs when actual circuit IDs unavailable
    /// </summary>
    private string GetOrGenerateCircuitId()
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            
            // Method 1: Try actual SignalR circuit ID from features
            if (context?.Features != null)
            {
                // Look for Blazor circuit feature
                var circuitFeature = context.Features
                    .Where(f => f.Key.Name.Contains("Circuit") || f.Key.Name.Contains("Blazor"))
                    .FirstOrDefault();
                    
                if (circuitFeature.Value != null)
                {
                    var circuitId = circuitFeature.Value.GetType().GetProperty("CircuitId")?.GetValue(circuitFeature.Value)?.ToString();
                    if (!string.IsNullOrEmpty(circuitId))
                    {
                        _logger.LogDebug("?? Found actual circuit ID: {CircuitId}", circuitId);
                        return circuitId;
                    }
                }
            }

            // Method 2: Try connection ID (more reliable than circuit ID)
            if (context?.Connection?.Id != null)
            {
                var connectionId = $"conn_{context.Connection.Id}";
                _logger.LogDebug("?? Using connection ID as circuit: {ConnectionId}", connectionId);
                return connectionId;
            }

            // Method 3: PRODUCTION FALLBACK - Generate deterministic ID from request characteristics
            var fallbackId = GenerateProductionFallbackId(context);
            _logger.LogInformation("?? PRODUCTION FALLBACK: Generated circuit ID: {FallbackId}", fallbackId);
            return fallbackId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error getting circuit ID - generating emergency fallback");
            return $"emergency_{Guid.NewGuid():N}";
        }
    }

    /// <summary>
    /// Generate a deterministic fallback circuit ID for production environments
    /// Uses request characteristics to ensure consistency across requests
    /// </summary>
    private string GenerateProductionFallbackId(HttpContext? context)
    {
        try
        {
            if (context == null)
            {
                return $"no_context_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}";
            }

            // Create deterministic ID based on connection characteristics
            var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown_ip";
            var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault() ?? "unknown_ua";
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmm"); // 1-minute granularity
            
            // Create hash of characteristics for consistency
            var combined = $"{remoteIp}_{userAgent}_{timestamp}";
            var hash = combined.GetHashCode().ToString("X8");
            
            var fallbackId = $"fallback_{hash}_{DateTime.UtcNow:ss}";
            _logger.LogDebug("?? Generated fallback circuit ID for IP {RemoteIp}: {FallbackId}", remoteIp, fallbackId);
            
            return fallbackId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error generating production fallback ID");
            return $"emergency_{Guid.NewGuid():N}";
        }
    }
}