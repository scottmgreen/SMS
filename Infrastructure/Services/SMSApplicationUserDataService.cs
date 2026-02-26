//-----------------------------------------------------------------------
// <copyright file="SMSApplicationUserDataService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Data service coordinating smsapplicationuser repository operations with business logic and validation.
//                  Infrastructure service providing external system integration
//                  and technical functionality support.
// </copyright>
//-----------------------------------------------------------------------

using Infrastructure.Interfaces;

using SMS_Domain.Errors;
using SMS_Domain.Interfaces;

namespace SMS_Infrastructure.Services;

/// <summary>
/// Data service for SMS Application User operations
/// Provides high-level data access abstraction over repository layer
/// </summary>
public sealed class SMSApplicationUserDataService : BaseDataService<SMSApplicationUserDataService>
{
    private readonly ISMSApplicationUserRepository _repository;
    private readonly ILogger<SMSApplicationUserDataService> _logger;

    public SMSApplicationUserDataService(
        ILogger<SMSApplicationUserDataService> logger,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        ISMSApplicationUserRepository repository)
        : base(logger, serviceScopeFactory, configuration)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new SMS Application User
    /// </summary>
    public async Task<Result<SMSApplicationUser>> CreateSMSApplicationUserAsync(SMSApplicationUser user, CancellationToken ct = default)
    {
        try
        {
            if (user is null)
            {
                _logger.LogError("CreateSMSApplicationUserAsync received null user");
                return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
            }

            _logger.LogInformation("Creating SMS Application User with code: {Code}", user.Code);

            // Check if username already exists
            var existsResult = await _repository.UserNameExistsAsync(user.UserName.Value);
            if (existsResult.IsSuccess && existsResult.Value)
            {
                _logger.LogWarning("Username {UserName} already exists", user.UserName.Value);
                return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.UserNameError.AlreadyExists);
            }

            var result = await _repository.AddAsync(user);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created SMS Application User with ID: {Id}", result.Value?.UserId);
            }
            else
            {
                _logger.LogError("Failed to create SMS Application User. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating SMS Application User");
            return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.CreateFailed);
        }
    }

    /// <summary>
    /// Gets SMS Application User by ID
    /// </summary>
    public async Task<Result<SMSApplicationUser>> GetSMSApplicationUserByIdAsync(SMSApplicationUserID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving SMS Application User with ID: {Id}", id.Value);
            return await _repository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving SMS Application User with ID: {Id}", id.Value);
            return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.NotFound);
        }
    }

    /// <summary>
    /// Gets SMS Application User by ID (string overload for convenience)
    /// </summary>
    public async Task<Result<SMSApplicationUser>> GetSMSApplicationUserByIdAsync(string id, CancellationToken ct = default)
    {
        var typedId = new SMSApplicationUserID(id);
        return await GetSMSApplicationUserByIdAsync(typedId, ct);
    }

    /// <summary>
    /// Gets SMS Application User by username
    /// </summary>
    public async Task<Result<SMSApplicationUser>> GetSMSApplicationUserByUserNameAsync(string userName, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving SMS Application User with UserName: {UserName}", userName);
            return await _repository.GetByUserNameAsync(userName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving SMS Application User with UserName: {UserName}", userName);
            return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.NotFound);
        }
    }

    /// <summary>
    /// Gets all SMS Application Users
    /// </summary>
    public async Task<Result<IEnumerable<SMSApplicationUser>>> GetAllSMSApplicationUsersAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving all SMS Application Users");
            return await _repository.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving all SMS Application Users");
            return Result<IEnumerable<SMSApplicationUser>>.Failure<IEnumerable<SMSApplicationUser>>(DomainErrors.SMSApplicationUserError.NotFound);
        }
    }

    /// <summary>
    /// Gets all active SMS Application Users
    /// </summary>
    public async Task<Result<IEnumerable<SMSApplicationUser>>> GetActiveSMSApplicationUsersAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving active SMS Application Users");
            return await _repository.GetActiveUsersAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving active SMS Application Users");
            return Result<IEnumerable<SMSApplicationUser>>.Failure<IEnumerable<SMSApplicationUser>>(DomainErrors.SMSApplicationUserError.NotFound);
        }
    }

    /// <summary>
    /// Gets SMS Application Users by application role
    /// </summary>
    public async Task<Result<IEnumerable<SMSApplicationUser>>> GetSMSApplicationUsersByRoleAsync(string applicationRole, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving SMS Application Users by role: {ApplicationRole}", applicationRole);
            return await _repository.GetBySMSApplicationUserRoleAsync(applicationRole);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving SMS Application Users by role: {ApplicationRole}", applicationRole);
            return Result<IEnumerable<SMSApplicationUser>>.Failure<IEnumerable<SMSApplicationUser>>(DomainErrors.SMSApplicationUserError.NotFound);
        }
    }

    /// <summary>
    /// Updates an existing SMS Application User
    /// </summary>
    public async Task<Result<SMSApplicationUser>> UpdateSMSApplicationUserAsync(SMSApplicationUser user, CancellationToken ct = default)
    {
        try
        {
            if (user is null)
            {
                _logger.LogError("UpdateSMSApplicationUserAsync received null user");
                return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
            }

            _logger.LogInformation("Updating SMS Application User with ID: {Id}", user.UserId);

            var updateResult = await _repository.UpdateAsync(user);
            if (updateResult.IsFailure)
            {
                _logger.LogError("Failed to update SMS Application User. Error: {Error}", updateResult.Error?.Message);
                return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(updateResult.Error);
            }

            // Return the updated user
            return await _repository.GetByIdAsync(user.Code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating SMS Application User with ID: {Id}", user?.UserId);
            return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.UpdateFailed);
        }
    }

    /// <summary>
    /// Deletes an SMS Application User
    /// </summary>
    public async Task<Result<bool>> DeleteSMSApplicationUserAsync(SMSApplicationUserID userId, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Deleting SMS Application User with ID: {Id}", userId.Value);
            var result = await _repository.DeleteAsync(userId);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted SMS Application User with ID: {Id}", userId.Value);
            }
            else
            {
                _logger.LogError("Failed to delete SMS Application User. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting SMS Application User with ID: {Id}", userId.Value);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.DeleteFailed);
        }
    }

    /// <summary>
    /// Updates SMS Application User password
    /// </summary>
    public async Task<Result<bool>> UpdateSMSApplicationUserPasswordAsync(SMSApplicationUserID userId, string hashedPassword, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Updating password for SMS Application User with ID: {Id}", userId.Value);
            var result = await _repository.UpdatePasswordAsync(userId, hashedPassword);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated password for SMS Application User with ID: {Id}", userId.Value);
            }
            else
            {
                _logger.LogError("Failed to update password for SMS Application User. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating password for SMS Application User with ID: {Id}", userId.Value);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.PasswordUpdateFailed);
        }
    }

    /// <summary>
    /// Records login for SMS Application User
    /// </summary>
    public async Task<Result<bool>> RecordSMSApplicationUserLoginAsync(SMSApplicationUserID userId, DateTime loginDate, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Recording login for SMS Application User with ID: {Id}", userId.Value);
            return await _repository.RecordLoginAsync(userId, loginDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error recording login for SMS Application User with ID: {Id}", userId.Value);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.UpdateFailed);
        }
    }

    /// <summary>
    /// Authenticates SMS Application User
    /// </summary>
    public async Task<Result<bool>> AuthenticateSMSApplicationUserAsync(string userName, string plainTextPassword, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Authenticating SMS Application User: {UserName}", userName);

            var userResult = await _repository.GetByUserNameAsync(userName);
            if (userResult.IsFailure)
            {
                _logger.LogWarning("Authentication failed - user not found: {UserName}", userName);
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.LoginFailed);
            }

            var user = userResult.Value;

            if (!user.Authenticate(plainTextPassword))
            {
                _logger.LogWarning("Authentication failed - invalid password for user: {UserName}", userName);
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.LoginFailed);
            }

            // Record the login
            user.RecordLogin();
            await _repository.UpdateAsync(user);

            _logger.LogInformation("Successfully authenticated SMS Application User: {UserName}", userName);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during authentication for user: {UserName}", userName);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.LoginFailed);
        }
    }

    /// <summary>
    /// Gets SMS Application User statistics
    /// </summary>
    public async Task<Result<UserStatistics>> GetSMSApplicationUserStatisticsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving SMS Application User statistics");
            return await _repository.GetUserStatisticsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving SMS Application User statistics");
            return Result<UserStatistics>.Failure<UserStatistics>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}
