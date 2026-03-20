//-----------------------------------------------------------------------
// <copyright file="AuthenticationStrategyManager.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Manager for orchestrating authentication strategies.
//                  Handles method selection, fallback chains, and strategy coordination.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SMS_Application.Configuration;
using SMS_Application.Interfaces;
using SMS_Domain.Enums;
using SMS_Domain.Errors;

namespace SMS_Application.Services;

/// <summary>
/// Manager for orchestrating authentication strategies
/// Handles method selection, fallback chains, and strategy coordination
/// </summary>
public class AuthenticationStrategyManager : IAuthenticationStrategyManager
{
    private readonly ILogger<AuthenticationStrategyManager> _logger;
    private readonly IOptionsMonitor<AuthenticationConfiguration> _configOptions;
    private readonly Dictionary<AuthenticationMethod, IAuthenticationStrategy> _strategies;
    private AuthenticationConfiguration _config;

    public AuthenticationStrategyManager(
        ILogger<AuthenticationStrategyManager> logger,
        IOptionsMonitor<AuthenticationConfiguration> configOptions,
        IEnumerable<IAuthenticationStrategy> strategies)
    {
        _logger = logger;
        _configOptions = configOptions;
        _config = configOptions.CurrentValue;
        
        // Build strategy dictionary
        _strategies = strategies.ToDictionary(s => s.Method, s => s);
        
        _logger.LogInformation("?? AuthenticationStrategyManager initialized with {StrategyCount} strategies: {Strategies}", 
            _strategies.Count, string.Join(", ", _strategies.Keys));

        // Log configuration
        _logger.LogInformation("?? Authentication Configuration: {ConfigSummary}", _config.GetConfigurationSummary());

        // Subscribe to configuration changes
        _configOptions.OnChange(config =>
        {
            _config = config;
            _logger.LogInformation("?? Authentication configuration updated: {ConfigSummary}", config.GetConfigurationSummary());
        });
    }

    public IAuthenticationStrategy PrimaryStrategy => GetStrategy(_config.PreferredMethod) 
        ?? throw new InvalidOperationException($"Primary authentication strategy '{_config.PreferredMethod}' not available");

    public IAuthenticationStrategy? FallbackStrategy => _config.EnableFallbackChain 
        ? GetStrategy(_config.FallbackMethod) 
        : null;

    /// <summary>
    /// Store user using the configured authentication strategy chain
    /// </summary>
    public async Task<Result<bool>> StoreUserAsync(BaseUser user, SMSUserType userType, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("?? Storing user {UserCode} ({UserType}) using strategy chain", user.Code, userType.Value);

            // Try primary strategy first
            var primaryStrategy = GetStrategy(_config.PreferredMethod);
            if (primaryStrategy?.IsAvailable == true)
            {
                var primaryResult = await primaryStrategy.StoreUserAsync(user, userType, cancellationToken);
                if (primaryResult.IsSuccess)
                {
                    if (_config.LogAuthenticationDecisions)
                    {
                        _logger.LogInformation("? User {UserCode} stored successfully using primary strategy: {Strategy}", 
                            user.Code, primaryStrategy.StrategyName);
                    }
                    return primaryResult;
                }
                else
                {
                    _logger.LogWarning("?? Primary strategy failed for user {UserCode}: {Error}", user.Code, primaryResult.Error?.Message);
                }
            }
            else
            {
                _logger.LogWarning("?? Primary strategy {Method} not available for user {UserCode}", _config.PreferredMethod, user.Code);
            }

            // Try fallback strategy if enabled
            if (_config.EnableFallbackChain)
            {
                var fallbackStrategy = GetStrategy(_config.FallbackMethod);
                if (fallbackStrategy?.IsAvailable == true)
                {
                    var fallbackResult = await fallbackStrategy.StoreUserAsync(user, userType, cancellationToken);
                    if (fallbackResult.IsSuccess)
                    {
                        _logger.LogInformation("? User {UserCode} stored successfully using fallback strategy: {Strategy}", 
                            user.Code, fallbackStrategy.StrategyName);
                        return fallbackResult;
                    }
                    else
                    {
                        _logger.LogError("? Fallback strategy also failed for user {UserCode}: {Error}", user.Code, fallbackResult.Error?.Message);
                    }
                }
                else
                {
                    _logger.LogWarning("?? Fallback strategy {Method} not available for user {UserCode}", _config.FallbackMethod, user.Code);
                }
            }

            // If strict mode, fail here
            if (_config.StrictMode)
            {
                var error = "Authentication storage failed in strict mode - no fallback attempted";
                _logger.LogError("? {Error} for user {UserCode}", error, user.Code);
                return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
            }

            // Last resort: try any available strategy
            foreach (var strategy in _strategies.Values.Where(s => s.IsAvailable && 
                s.Method != _config.PreferredMethod && s.Method != _config.FallbackMethod))
            {
                try
                {
                    var lastResortResult = await strategy.StoreUserAsync(user, userType, cancellationToken);
                    if (lastResortResult.IsSuccess)
                    {
                        _logger.LogWarning("?? User {UserCode} stored using last resort strategy: {Strategy}", 
                            user.Code, strategy.StrategyName);
                        return lastResortResult;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Last resort strategy {Strategy} failed for user {UserCode}", strategy.StrategyName, user.Code);
                }
            }

            _logger.LogError("? All authentication strategies failed for user {UserCode}", user.Code);
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error in authentication strategy chain for user {UserCode}", user.Code);
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    /// <summary>
    /// Retrieve user using the configured authentication strategy chain
    /// </summary>
    public async Task<Result<(BaseUser User, SMSUserType UserType)?>> RetrieveUserAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("?? Retrieving user using strategy chain");

            // Try primary strategy first
            var primaryStrategy = GetStrategy(_config.PreferredMethod);
            if (primaryStrategy?.IsAvailable == true)
            {
                var primaryResult = await primaryStrategy.RetrieveUserAsync(cancellationToken);
                if (primaryResult.IsSuccess && primaryResult.Value.HasValue)
                {
                    if (_config.LogAuthenticationDecisions)
                    {
                        _logger.LogDebug("? User retrieved using primary strategy: {Strategy}", primaryStrategy.StrategyName);
                    }
                    return primaryResult;
                }
            }

            // Try fallback strategy if enabled
            if (_config.EnableFallbackChain)
            {
                var fallbackStrategy = GetStrategy(_config.FallbackMethod);
                if (fallbackStrategy?.IsAvailable == true)
                {
                    var fallbackResult = await fallbackStrategy.RetrieveUserAsync(cancellationToken);
                    if (fallbackResult.IsSuccess && fallbackResult.Value.HasValue)
                    {
                        _logger.LogDebug("? User retrieved using fallback strategy: {Strategy}", fallbackStrategy.StrategyName);
                        return fallbackResult;
                    }
                }
            }

            // If strict mode, return null here
            if (_config.StrictMode)
            {
                _logger.LogDebug("?? No user found in strict mode - no additional fallback attempted");
                return Result<(BaseUser, SMSUserType)?>.Success((ValueTuple<BaseUser, SMSUserType>?)null);
            }

            // Last resort: try any available strategy
            foreach (var strategy in _strategies.Values.Where(s => s.IsAvailable && 
                s.Method != _config.PreferredMethod && s.Method != _config.FallbackMethod))
            {
                try
                {
                    var lastResortResult = await strategy.RetrieveUserAsync(cancellationToken);
                    if (lastResortResult.IsSuccess && lastResortResult.Value.HasValue)
                    {
                        _logger.LogDebug("?? User retrieved using last resort strategy: {Strategy}", strategy.StrategyName);
                        return lastResortResult;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Last resort strategy {Strategy} failed during retrieval", strategy.StrategyName);
                }
            }

            _logger.LogDebug("?? No authenticated user found in any strategy");
            return Result<(BaseUser, SMSUserType)?>.Success((ValueTuple<BaseUser, SMSUserType>?)null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error retrieving user from strategy chain");
            return Result<(BaseUser, SMSUserType)?>.Success((ValueTuple<BaseUser, SMSUserType>?)null);
        }
    }

    /// <summary>
    /// Clear user using all available authentication strategies
    /// </summary>
    public async Task<Result<bool>> ClearUserAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("??? Clearing user from all authentication strategies");

            var clearResults = new List<(string Strategy, bool Success, string? Error)>();

            // Clear from all available strategies
            foreach (var strategy in _strategies.Values.Where(s => s.IsAvailable))
            {
                try
                {
                    var result = await strategy.ClearUserAsync(cancellationToken);
                    clearResults.Add((strategy.StrategyName, result.IsSuccess, result.Error?.Message));
                    
                    if (result.IsSuccess)
                    {
                        _logger.LogDebug("? User cleared from strategy: {Strategy}", strategy.StrategyName);
                    }
                    else
                    {
                        _logger.LogWarning("?? Failed to clear user from strategy {Strategy}: {Error}", 
                            strategy.StrategyName, result.Error?.Message);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "? Error clearing user from strategy: {Strategy}", strategy.StrategyName);
                    clearResults.Add((strategy.StrategyName, false, ex.Message));
                }
            }

            var successCount = clearResults.Count(r => r.Success);
            var totalCount = clearResults.Count;

            _logger.LogInformation("? User clearing completed - Success: {Success}/{Total} strategies", successCount, totalCount);

            // Return success if at least one strategy succeeded
            return Result<bool>.Success(successCount > 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error clearing user from authentication strategies");
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    /// <summary>
    /// Check if user is authenticated using any available strategy
    /// </summary>
    public async Task<bool> IsUserAuthenticatedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Check primary strategy first
            var primaryStrategy = GetStrategy(_config.PreferredMethod);
            if (primaryStrategy?.IsAvailable == true && await primaryStrategy.IsUserAuthenticatedAsync(cancellationToken))
            {
                return true;
            }

            // Check fallback strategy if enabled
            if (_config.EnableFallbackChain)
            {
                var fallbackStrategy = GetStrategy(_config.FallbackMethod);
                if (fallbackStrategy?.IsAvailable == true && await fallbackStrategy.IsUserAuthenticatedAsync(cancellationToken))
                {
                    return true;
                }
            }

            // If not in strict mode, check other strategies
            if (!_config.StrictMode)
            {
                foreach (var strategy in _strategies.Values.Where(s => s.IsAvailable && 
                    s.Method != _config.PreferredMethod && s.Method != _config.FallbackMethod))
                {
                    try
                    {
                        if (await strategy.IsUserAuthenticatedAsync(cancellationToken))
                        {
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Error checking authentication in strategy {Strategy}", strategy.StrategyName);
                    }
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error checking authentication status");
            return false;
        }
    }

    /// <summary>
    /// Get current user ID using any available strategy
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
    /// Get current user display name using any available strategy
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
    /// Get authentication strategy by method type
    /// </summary>
    public IAuthenticationStrategy? GetStrategy(AuthenticationMethod method)
    {
        return _strategies.TryGetValue(method, out var strategy) ? strategy : null;
    }

    /// <summary>
    /// Get all available authentication strategies
    /// </summary>
    public IEnumerable<IAuthenticationStrategy> GetAllStrategies()
    {
        return _strategies.Values;
    }

    /// <summary>
    /// Get status information about all strategies for debugging
    /// </summary>
    public async Task<Dictionary<string, object>> GetStrategyStatusAsync(CancellationToken cancellationToken = default)
    {
        var status = new Dictionary<string, object>();

        try
        {
            status["Configuration"] = _config.GetConfigurationSummary();
            status["PrimaryStrategy"] = PrimaryStrategy.StrategyName;
            status["FallbackStrategy"] = FallbackStrategy?.StrategyName ?? "None";
            status["FallbackEnabled"] = _config.EnableFallbackChain;
            status["StrictMode"] = _config.StrictMode;

            var strategyDetails = new Dictionary<string, object>();
            foreach (var strategy in _strategies.Values)
            {
                var details = new Dictionary<string, object>
                {
                    ["Name"] = strategy.StrategyName,
                    ["Method"] = strategy.Method.ToString(),
                    ["Available"] = strategy.IsAvailable,
                    ["StorageInfo"] = strategy.GetStorageInfo()
                };

                try
                {
                    details["IsAuthenticated"] = await strategy.IsUserAuthenticatedAsync(cancellationToken);
                    details["IsValid"] = await strategy.ValidateStoredDataAsync(cancellationToken);
                    details["CurrentUserId"] = await strategy.GetCurrentUserIdAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    details["Error"] = ex.Message;
                }

                strategyDetails[strategy.Method.ToString()] = details;
            }

            status["Strategies"] = strategyDetails;
        }
        catch (Exception ex)
        {
            status["Error"] = ex.Message;
        }

        return status;
    }

    /// <summary>
    /// Validate authentication data across all strategies
    /// </summary>
    public async Task<Dictionary<AuthenticationMethod, bool>> ValidateAllStrategiesAsync(CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<AuthenticationMethod, bool>();

        foreach (var strategy in _strategies.Values)
        {
            try
            {
                results[strategy.Method] = strategy.IsAvailable && await strategy.ValidateStoredDataAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error validating strategy {Strategy}", strategy.StrategyName);
                results[strategy.Method] = false;
            }
        }

        return results;
    }

    /// <summary>
    /// Force refresh of strategy configuration
    /// </summary>
    public void RefreshConfiguration()
    {
        _config = _configOptions.CurrentValue;
        _logger.LogInformation("?? Authentication configuration refreshed: {ConfigSummary}", _config.GetConfigurationSummary());
    }
}