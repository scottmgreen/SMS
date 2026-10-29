//-----------------------------------------------------------------------
// <copyright file="AuthenticationStrategyManager.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Manager for orchestrating authentication strategies.
//                  Handles method selection, fallback chains, and strategy coordination.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.Configuration;
using SMS_Application.Interfaces;
using SMS_Domain.Entities;
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
    private readonly IAuthenticationStrategy _strategy;

    public AuthenticationStrategyManager(
        ILogger<AuthenticationStrategyManager> logger,
        IAuthenticationStrategy strategy)
    {
        _logger = logger;
        _strategy = strategy;

        _logger.LogApplicationDebug("AuthenticationStrategyManager initialized with strategy: {Strategy}", _strategy.StrategyName);
    }

    public IAuthenticationStrategy PrimaryStrategy => _strategy;

    /// <summary>
    /// Store user using the configured authentication strategy chain
    /// </summary>
    public async Task<Result<bool>> StoreUserAsync(BaseUser user, SMSUserType userType, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogApplicationInformation("Storing user {UserCode} ({UserType}) using unified strategy", user.Code, userType.Value);

            if (!_strategy.IsAvailable)
            {
                _logger.LogApplicationError("No available authentication strategy to store user {UserCode}", user.Code);
                return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
            }

            var result = await _strategy.StoreUserAsync(user, userType, cancellationToken);
            if (!result.IsSuccess)
            {
                _logger.LogApplicationWarning("Authentication strategy failed to store user {UserCode} using {Strategy}: {Error}",
                    user.Code,
                    _strategy.StrategyName,
                    result.Error?.Message);
                return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
            }

            _logger.LogApplicationInformation("User {UserCode} stored successfully using strategy: {Strategy}",
                user.Code,
                _strategy.StrategyName);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error in authentication strategy chain for user {UserCode}", user.Code);
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
            _logger.LogApplicationDebug("Retrieving user using unified strategy");

            if (!_strategy.IsAvailable)
            {
                return Result<(BaseUser, SMSUserType)?>.Success((ValueTuple<BaseUser, SMSUserType>?)null);
            }

            var result = await _strategy.RetrieveUserAsync(cancellationToken);
            if (result.IsSuccess && result.Value.HasValue)
            {
                _logger.LogApplicationDebug("User retrieved using strategy: {Strategy}", _strategy.StrategyName);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error retrieving user from strategy chain");
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
            _logger.LogApplicationInformation("??? Clearing user from authentication strategy");

            if (!_strategy.IsAvailable)
            {
                _logger.LogApplicationWarning("No available authentication strategy while clearing user");
                return Result<bool>.Success(true);
            }

            var result = await _strategy.ClearUserAsync(cancellationToken);
            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("User clearing completed using strategy: {Strategy}", _strategy.StrategyName);
                return Result<bool>.Success(true);
            }

            _logger.LogApplicationWarning("Failed to clear user from strategy {Strategy}: {Error}",
                _strategy.StrategyName,
                result.Error?.Message);
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error clearing user from authentication strategies");
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    /// <summary>
    /// Check if user is authenticated using any available strategy
    /// OPTIMIZED: Cache authentication state temporarily to reduce repeated expensive checks
    /// </summary>
    public async Task<bool> IsUserAuthenticatedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogApplicationDebug("IsUserAuthenticatedAsync - checking unified strategy");

            if (!_strategy.IsAvailable)
            {
                return false;
            }

            return await _strategy.IsUserAuthenticatedAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error checking authentication status");
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
    /// Get status information about all strategies for debugging
    /// </summary>
    public async Task<Dictionary<string, object>> GetStrategyStatusAsync(CancellationToken cancellationToken = default)
    {
        var status = new Dictionary<string, object>();

        try
        {
            status["PrimaryStrategy"] = PrimaryStrategy.StrategyName;
            status["Method"] = PrimaryStrategy.Method.ToString();
            status["Available"] = PrimaryStrategy.IsAvailable;
            status["StorageInfo"] = PrimaryStrategy.GetStorageInfo();
            status["IsAuthenticated"] = await PrimaryStrategy.IsUserAuthenticatedAsync(cancellationToken);
            status["IsValid"] = await PrimaryStrategy.ValidateStoredDataAsync(cancellationToken);
            status["CurrentUserId"] = await PrimaryStrategy.GetCurrentUserIdAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            status["Error"] = ex.Message;
        }

        return status;
    }
}

