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
using Microsoft.AspNetCore.Http;
using SMS_Domain.Entities;

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

            // 🔧 CRITICAL FIX: Always ensure user completeness - the user may not have complete role/permission data
            // This is especially important after 2FA verification where the stored user may be incomplete
            _logger.LogInformation("🔍 Ensuring user completeness before session creation...");
            
            var completeUserResult = await _userInstantiationService.EnsureUserCompletenessAsync(user, userType);
            if (completeUserResult.IsFailure)
            {
                _logger.LogError("❌ Failed to ensure user completeness: {UserCode}: {Error}", 
                    user.Code, completeUserResult.Error?.Message);
                throw new InvalidOperationException($"User {user.Code} could not be fully instantiated: {completeUserResult.Error?.Message}");
            }

            var completeUser = completeUserResult.Value;
            _logger.LogInformation("✅ User completeness ensured - Role: {RoleCode}, Permissions: {PermissionCount}", 
                completeUser.UserRole?.Code ?? "None", completeUser.UserRole?.Permissions?.Count ?? 0);

            // Final validation of user completeness
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
    /// PRODUCTION-OPTIMIZED: ContextBased first for production reliability when circuit IDs fail
    /// CRITICAL FIX: Always clear any existing 2FA data first to prevent session contamination
    /// </summary>
    public async Task StorePending2FAUserAsync(BaseUser user, SMSUserType userType)
    {
        try
        {
            _logger.LogInformation("🔐 Storing pending 2FA user: {UserId} ({UserType}) with HTTP protocol compatibility", user.Code, userType.Value);

            // 🚨 CRITICAL FIX: ALWAYS clear existing 2FA data FIRST to prevent session contamination
            _logger.LogInformation("🧹 CLEARING any existing 2FA data to prevent session contamination...");
            await ClearPending2FAUserAsync();
            
            // 🚨 ADD VERIFICATION: Ensure cleanup completed before proceeding
            await Task.Delay(100); // Brief pause to ensure cleanup completes
            var verifyCleared = GetPending2FAUser();
            if (verifyCleared != null)
            {
                _logger.LogWarning("⚠️ Previous 2FA data not fully cleared on first attempt, trying again...");
                await ClearPending2FAUserAsync();
                await Task.Delay(200);
                
                // Final verification
                verifyCleared = GetPending2FAUser();
                if (verifyCleared != null)
                {
                    _logger.LogError("❌ CRITICAL: Failed to clear previous 2FA data - FORCING aggressive cleanup");
                    await AggressiveClearAllStrategiesAsync(); // New method for deep cleanup
                }
            }
            _logger.LogInformation("✅ Previous 2FA data cleared successfully - ready for new user");

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

            // Add 2FA-specific markers - these MUST be preserved through all strategies
            userData["Pending2FA_UserData"] = System.Text.Json.JsonSerializer.Serialize(userData);
            userData["Pending2FA_UserType"] = userType.Value;
            userData["Pending2FA_StoredAt"] = DateTime.UtcNow.ToString("O");
            userData["Pending2FA_Protocol"] = "HTTP"; // Explicitly mark as HTTP-compatible

            var stored = false;
            var attemptedStrategies = new List<string>();

            // 🎯 PRODUCTION-OPTIMIZED STRATEGY ORDER
            // Try strategies in order of production reliability when circuit IDs fail

            // Strategy 1: Try Context-Based FIRST (most reliable in production when circuit IDs fail)
            var contextStrategy = _strategyManager.GetStrategy(AuthenticationMethod.ContextBased);
            if (contextStrategy?.IsAvailable == true)
            {
                attemptedStrategies.Add("ContextBased");
                _logger.LogInformation("🔄 Attempting ContextBased storage for 2FA (production-optimized)");
                
                try
                {
                    // 🎯 CRITICAL: Pass the 2FA markers to the context strategy
                    var twoFAMarkers = new Dictionary<string, string>
                    {
                        ["Pending2FA_UserData"] = System.Text.Json.JsonSerializer.Serialize(userData),
                        ["Pending2FA_UserType"] = userType.Value,
                        ["Pending2FA_StoredAt"] = DateTime.UtcNow.ToString("O"),
                        ["Pending2FA_Protocol"] = "HTTP"
                    };

                    // Check if the context strategy supports additional data
                    var contextStorageMethod = contextStrategy.GetType().GetMethod("StoreUserAsync", 
                        new Type[] { typeof(BaseUser), typeof(SMSUserType), typeof(Dictionary<string, string>), typeof(CancellationToken) });

                    Result<bool> contextResult;
                    if (contextStorageMethod != null)
                    {
                        // Use enhanced method with 2FA markers
                        contextResult = await (Task<Result<bool>>)contextStorageMethod.Invoke(contextStrategy, 
                            new object[] { completeUser, userType, twoFAMarkers, CancellationToken.None });
                        _logger.LogInformation("🔐 Used enhanced context method with 2FA markers");
                    }
                    else
                    {
                        // Fallback to standard method
                        contextResult = await contextStrategy.StoreUserAsync(completeUser, userType);
                        _logger.LogWarning("⚠️ Context strategy using standard method - 2FA markers may be lost");
                    }

                    if (contextResult.IsSuccess)
                    {
                        stored = true;
                        _logger.LogInformation("✅ SUCCESS: Pending 2FA user stored in ContextBased strategy: {UserId}", user.Code);
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ ContextBased strategy failed: {Error}", contextResult.Error?.Message);
                    }
                }
                catch (Exception contextEx)
                {
                    _logger.LogWarning(contextEx, "⚠️ ContextBased strategy threw exception");
                }
            }
            else
            {
                _logger.LogWarning("⚠️ ContextBased strategy not available - IsAvailable: {IsAvailable}", contextStrategy?.IsAvailable);
            }

            // Strategy 2: Try Circuit-Based SECOND (if context fails and circuit ID available)
            if (!stored)
            {
                var circuitStrategy = _strategyManager.GetStrategy(AuthenticationMethod.CircuitBased);
                if (circuitStrategy?.IsAvailable == true)
                {
                    attemptedStrategies.Add("CircuitBased");
                    _logger.LogInformation("🔄 Attempting CircuitBased storage for 2FA");

                    // CRITICAL: Pass the 2FA markers to the circuit strategy
                    var twoFAMarkers = new Dictionary<string, string>
                    {
                        ["Pending2FA_UserData"] = System.Text.Json.JsonSerializer.Serialize(userData),
                        ["Pending2FA_UserType"] = userType.Value,
                        ["Pending2FA_StoredAt"] = DateTime.UtcNow.ToString("O"),
                        ["Pending2FA_Protocol"] = "HTTP"
                    };

                    try
                    {
                        // Check if the circuit strategy supports additional data
                        var circuitStorageMethod = circuitStrategy.GetType().GetMethod("StoreUserAsync", 
                            new Type[] { typeof(BaseUser), typeof(SMSUserType), typeof(Dictionary<string, string>), typeof(CancellationToken) });

                        Result<bool> circuitResult;
                        if (circuitStorageMethod != null)
                        {
                            // Use enhanced method with 2FA markers
                            circuitResult = await (Task<Result<bool>>)circuitStorageMethod.Invoke(circuitStrategy, 
                                new object[] { completeUser, userType, twoFAMarkers, CancellationToken.None });
                            _logger.LogInformation("🔐 Attempted enhanced circuit method with 2FA markers");
                        }
                        else
                        {
                            // Fallback to standard method
                            circuitResult = await circuitStrategy.StoreUserAsync(completeUser, userType);
                            _logger.LogWarning("⚠️ Circuit strategy using standard method - 2FA markers may be lost");
                        }

                        if (circuitResult.IsSuccess)
                        {
                            stored = true;
                            _logger.LogInformation("✅ SUCCESS: Pending 2FA user stored in CircuitBased strategy: {UserId}", user.Code);
                        }
                        else
                        {
                            _logger.LogWarning("⚠️ CircuitBased strategy failed: {Error}", circuitResult.Error?.Message);
                        }
                    }
                    catch (Exception circuitEx)
                    {
                        _logger.LogWarning(circuitEx, "⚠️ CircuitBased strategy threw exception");
                    }
                }
                else
                {
                    _logger.LogWarning("⚠️ CircuitBased strategy not available - IsAvailable: {IsAvailable}", circuitStrategy?.IsAvailable);
                }
            }

            // Strategy 3: Try Session-Based THIRD (final fallback for HTTPS environments)
            if (!stored)
            {
                var sessionStrategy = _strategyManager.GetStrategy(AuthenticationMethod.SessionBased);
                if (sessionStrategy?.IsAvailable == true)
                {
                    attemptedStrategies.Add("SessionBased");
                    _logger.LogInformation("🔄 Attempting SessionBased storage for 2FA (final fallback)");
                    
                    try
                    {
                        var sessionResult = await sessionStrategy.StoreUserAsync(completeUser, userType);
                        if (sessionResult.IsSuccess)
                        {
                            stored = true;
                            _logger.LogInformation("✅ SUCCESS: Pending 2FA user stored in SessionBased strategy: {UserId}", user.Code);
                        }
                        else
                        {
                            _logger.LogWarning("⚠️ SessionBased strategy failed: {Error}", sessionResult.Error?.Message);
                        }
                    }
                    catch (Exception sessionEx)
                    {
                        _logger.LogWarning(sessionEx, "⚠️ SessionBased strategy threw exception");
                    }
                }
                else
                {
                    _logger.LogWarning("⚠️ SessionBased strategy not available - IsAvailable: {IsAvailable}", sessionStrategy?.IsAvailable);
                }
            }

            // Final verification with detailed logging
            if (!stored)
            {
                _logger.LogError("❌ CRITICAL FAILURE: Failed to store pending 2FA user in ANY strategy");
                _logger.LogError("❌ Attempted strategies: {AttemptedStrategies}", string.Join(", ", attemptedStrategies));
                
                // Get strategy availability for debugging
                var contextAvailable = _strategyManager.GetStrategy(AuthenticationMethod.ContextBased)?.IsAvailable;
                var circuitAvailable = _strategyManager.GetStrategy(AuthenticationMethod.CircuitBased)?.IsAvailable;
                var sessionAvailable = _strategyManager.GetStrategy(AuthenticationMethod.SessionBased)?.IsAvailable;
                
                _logger.LogError("❌ Strategy availability - Context: {Context}, Circuit: {Circuit}, Session: {Session}", 
                    contextAvailable, circuitAvailable, sessionAvailable);
                throw new InvalidOperationException($"2FA storage failed - no compatible strategy available. Attempted: {string.Join(", ", attemptedStrategies)}");
            }

            _logger.LogInformation("🎉 2FA STORAGE SUCCESS using strategy: {SuccessfulStrategy}", attemptedStrategies.Last());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ EXCEPTION in StorePending2FAUserAsync for user: {UserId}", user.Code);
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
                AuthenticationMethod.CircuitBased,    // Try circuit FIRST (persistent, reliable for HTTP)
                AuthenticationMethod.SessionBased,    // Then session (works in HTTPS)
                AuthenticationMethod.ContextBased     // Finally context (request-scoped only)
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
    /// ENHANCED: Only returns true if user is specifically in pending 2FA state, not fully authenticated
    /// FIXED: Properly distinguishes between pending 2FA storage and full authentication
    /// </summary>
    public bool HasPending2FAUser()
    {
        try
        {
            _logger.LogDebug("🔐 HasPending2FAUser: Checking for pending 2FA state across all strategies");

            // Strategy 1: Try Context-Based Authentication first (where temp 2FA data is stored)
            var contextStrategy = _strategyManager.GetStrategy(AuthenticationMethod.ContextBased);
            if (contextStrategy?.IsAvailable == true)
            {
                try
                {
                    var contextResult = contextStrategy.RetrieveUserAsync().GetAwaiter().GetResult();
                    if (contextResult.IsSuccess && contextResult.Value.HasValue)
                    {
                        _logger.LogDebug("🔐 Found user in Context strategy: {UserCode}", contextResult.Value.Value.User.Code);
                        
                        // Context strategy is primarily used for temporary 2FA storage
                        // If we find a user here, they are likely pending 2FA verification
                        _logger.LogDebug("🔐 Context strategy contains user data - likely pending 2FA");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Error checking Context strategy for 2FA user");
                }
            }

            // Strategy 2: Check Circuit-Based Authentication for 2FA markers
            var circuitStrategy = _strategyManager.GetStrategy(AuthenticationMethod.CircuitBased);
            if (circuitStrategy?.IsAvailable == true)
            {
                try
                {
                    var circuitResult = circuitStrategy.RetrieveUserAsync().GetAwaiter().GetResult();
                    if (circuitResult.IsSuccess && circuitResult.Value.HasValue)
                    {
                        _logger.LogDebug("🔐 Found user in Circuit strategy: {UserCode}", circuitResult.Value.Value.User.Code);
                        
                        // For circuit storage, we need to check if this contains 2FA markers
                        // Use reflection to check the actual stored data
                        if (circuitStrategy.GetType().Name == "CircuitBasedAuthenticationStrategy")
                        {
                            var storageField = circuitStrategy.GetType().GetField("_circuitAuthStorage", 
                                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            
                            if (storageField?.GetValue(circuitStrategy) is IBlazorCircuitAuthStorage storage)
                            {
                                var allData = storage.GetAllAuthData();
                                foreach (var kvp in allData)
                                {
                                    // Look for explicit 2FA pending markers
                                    if (kvp.Value.ContainsKey("Pending2FA_UserData") || 
                                        kvp.Value.ContainsKey("Pending2FA_StoredAt") ||
                                        kvp.Value.ContainsKey("Pending2FA_Protocol"))
                                    {
                                        _logger.LogDebug("🔐 Found explicit 2FA markers in Circuit storage - user is pending 2FA");
                                        return true;
                                    }
                                }
                                
                                _logger.LogDebug("🔐 Circuit storage contains user but no 2FA markers - fully authenticated");
                                return false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Error checking Circuit strategy for 2FA markers");
                }
            }

            // Strategy 3: Check Session-Based for any 2FA data
            var sessionStrategy = _strategyManager.GetStrategy(AuthenticationMethod.SessionBased);
            if (sessionStrategy?.IsAvailable == true)
            {
                try
                {
                    var sessionResult = sessionStrategy.RetrieveUserAsync().GetAwaiter().GetResult();
                    if (sessionResult.IsSuccess && sessionResult.Value.HasValue)
                    {
                        _logger.LogDebug("🔐 Found user in Session strategy: {UserCode}", sessionResult.Value.Value.User.Code);
                        
                        // Session-based storage typically indicates full authentication
                        // Only return true if this is specifically marked as 2FA pending
                        // For now, assume session storage means fully authenticated
                        _logger.LogDebug("🔐 Session storage typically means full authentication");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Error checking Session strategy for 2FA user");
                }
            }

            _logger.LogDebug("🔐 HasPending2FAUser: No pending 2FA user found in any strategy");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error checking for pending 2FA user");
            return false;
        }
    }

    /// <summary>
    /// Aggressive cleanup of all cached 2FA data from all strategies and storage locations
    /// Used when standard clearing fails to ensure complete session isolation
    /// </summary>
    private async Task AggressiveClearAllStrategiesAsync()
    {
        try
        {
            _logger.LogWarning("🚨 Performing aggressive cleanup of ALL cached 2FA data");

            var clearResults = new List<string>();

            // Method 1: Clear each strategy individually with force
            var allMethods = new[] 
            { 
                AuthenticationMethod.ContextBased, 
                AuthenticationMethod.CircuitBased, 
                AuthenticationMethod.SessionBased 
            };

            foreach (var method in allMethods)
            {
                try
                {
                    var strategy = _strategyManager.GetStrategy(method);
                    if (strategy != null)
                    {
                        var result = await strategy.ClearUserAsync();
                        clearResults.Add($"{method}: {(result.IsSuccess ? "SUCCESS" : "FAILED")}");
                    }
                }
                catch (Exception ex)
                {
                    clearResults.Add($"{method}: EXCEPTION - {ex.Message}");
                }
            }

            // Method 2: Clear through strategy manager
            try
            {
                var managerResult = await _strategyManager.ClearUserAsync();
                clearResults.Add($"StrategyManager: {(managerResult.IsSuccess ? "SUCCESS" : "FAILED")}");
            }
            catch (Exception ex)
            {
                clearResults.Add($"StrategyManager: EXCEPTION - {ex.Message}");
            }

            _logger.LogWarning("🧹 Aggressive cleanup results: {Results}", string.Join(", ", clearResults));

            // Method 3: If we have access to BlazorCircuitAuthStorage, clear by user ID pattern
            try
            {
                // Clear any circuit auth data that might match 2FA patterns
                var circuitStrategy = _strategyManager.GetStrategy(AuthenticationMethod.CircuitBased);
                if (circuitStrategy != null)
                {
                    // Use reflection to access BlazorCircuitAuthStorage if needed
                    var storageField = circuitStrategy.GetType().GetField("_circuitAuthStorage", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (storageField?.GetValue(circuitStrategy) is IBlazorCircuitAuthStorage storage)
                    {
                        // Get all data and clear any that looks like pending 2FA
                        var allData = storage.GetAllAuthData();
                        var clearedKeys = new List<string>();
                        
                        foreach (var kvp in allData)
                        {
                            if (kvp.Value.ContainsKey("Pending2FA_UserData") || 
                                kvp.Value.ContainsKey("Pending2FA_StoredAt") ||
                                kvp.Value.ContainsKey("SMS_TwoFactorSecretKey"))
                            {
                                storage.ClearAuthData(kvp.Key);
                                clearedKeys.Add(kvp.Key);
                            }
                        }
                        
                        if (clearedKeys.Any())
                        {
                            _logger.LogWarning("🧹 Aggressive cleanup: Cleared {Count} circuit auth entries with 2FA data", clearedKeys.Count);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during aggressive circuit cleanup");
            }

            _logger.LogWarning("✅ Aggressive cleanup completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Critical error during aggressive cleanup");
        }
    }
}
