using Microsoft.Extensions.Logging;
using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Infrastructure.Services;
using SMS_Shared.Common;
using static SMS_Domain.Errors.DomainErrors;

namespace SMS_Application.Services;

/// <summary>
/// High-level application service for SMS Application User business operations
/// Provides business logic orchestration and cross-cutting concerns
/// </summary>
public sealed class SMSApplicationUserService
{
    private readonly SMSApplicationUserDataService _dataService;
    private readonly ILogger<SMSApplicationUserService> _logger;

    public SMSApplicationUserService(SMSApplicationUserDataService dataService, ILogger<SMSApplicationUserService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new SMS Application User with business validation
    /// </summary>
    public async Task<Result<SMSApplicationUser>> CreateSMSApplicationUserAsync(SMSApplicationUser user, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Creating SMS Application User with code: {Code}", user?.Code);

            // Business validation - ensure user is not null
            if (user is null)
            {
                _logger.LogError("CreateSMSApplicationUserAsync received null user");
                return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
            }

            // Business validation - ensure user is active by default
            if (!user.IsActive)
            {
                _logger.LogInformation("Activating user during creation: {Code}", user.Code);
                user.Activate();
            }

            var result = await _dataService.CreateSMSApplicationUserAsync(user, ct).ConfigureAwait(false);

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
    public async Task<Result<SMSApplicationUser>> GetSMSApplicationUserByIdAsync(string id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving SMS Application User with ID: {Id}", id);
            return await _dataService.GetSMSApplicationUserByIdAsync(id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving SMS Application User with ID: {Id}", id);
            return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.NotFound);
        }
    }

    /// <summary>
    /// Gets SMS Application User by username
    /// </summary>
    public async Task<Result<SMSApplicationUser>> GetSMSApplicationUserByUserNameAsync(string userName, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving SMS Application User with UserName: {UserName}", userName);
            return await _dataService.GetSMSApplicationUserByUserNameAsync(userName, ct).ConfigureAwait(false);
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
            return await _dataService.GetAllSMSApplicationUsersAsync(ct).ConfigureAwait(false);
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
            return await _dataService.GetActiveSMSApplicationUsersAsync(ct).ConfigureAwait(false);
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
            return await _dataService.GetSMSApplicationUsersByRoleAsync(applicationRole, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving SMS Application Users by role: {ApplicationRole}", applicationRole);
            return Result<IEnumerable<SMSApplicationUser>>.Failure<IEnumerable<SMSApplicationUser>>(DomainErrors.SMSApplicationUserError.NotFound);
        }
    }

    /// <summary>
    /// Updates an existing SMS Application User with business validation
    /// </summary>
    public async Task<Result<SMSApplicationUser>> UpdateSMSApplicationUserAsync(SMSApplicationUser user, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Updating SMS Application User with ID: {Id}", user?.UserId);

            if (user is null)
            {
                _logger.LogError("UpdateSMSApplicationUserAsync received null user");
                return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
            }

            // Business validation - check if user exists
            var existingUserResult = await _dataService.GetSMSApplicationUserByIdAsync(user.UserId.Value, ct).ConfigureAwait(false);
            if (existingUserResult.IsFailure)
            {
                _logger.LogWarning("Cannot update non-existent SMS Application User with ID: {Id}", user.UserId);
                return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.NotFound);
            }

            var result = await _dataService.UpdateSMSApplicationUserAsync(user, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated SMS Application User with ID: {Id}", user.UserId);
            }
            else
            {
                _logger.LogError("Failed to update SMS Application User. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating SMS Application User with ID: {Id}", user?.UserId);
            return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.UpdateFailed);
        }
    }

    /// <summary>
    /// Deletes an SMS Application User with business validation
    /// </summary>
    public async Task<Result<bool>> DeleteSMSApplicationUserAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Deleting SMS Application User with ID: {Id}", userId);

            // Business validation - check if user exists
            var existingUserResult = await _dataService.GetSMSApplicationUserByIdAsync(userId, ct).ConfigureAwait(false);
            if (existingUserResult.IsFailure)
            {
                _logger.LogWarning("Cannot delete non-existent SMS Application User with ID: {Id}", userId);
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.NotFound);
            }

            // Business rule - deactivate instead of hard delete for audit purposes
            var user = existingUserResult.Value;
            user.Deactivate();
            
            var updateResult = await _dataService.UpdateSMSApplicationUserAsync(user, ct).ConfigureAwait(false);
            if (updateResult.IsFailure)
            {
                _logger.LogError("Failed to deactivate SMS Application User. Error: {Error}", updateResult.Error?.Message);
                return Result<bool>.Failure<bool>(updateResult.Error);
            }

            _logger.LogInformation("Successfully deactivated SMS Application User with ID: {Id}", userId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting SMS Application User with ID: {Id}", userId);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.DeleteFailed);
        }
    }

    /// <summary>
    /// Authenticates an SMS Application User with comprehensive business logic
    /// </summary>
    public async Task<Result<SMSApplicationUser>> AuthenticateSMSApplicationUserAsync(string userName, string plainTextPassword, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Authenticating SMS Application User: {UserName}", userName);

            // Business validation
            if (string.IsNullOrWhiteSpace(userName))
            {
                _logger.LogWarning("Authentication failed - empty username");
                return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.UserNameError.NullOrEmpty);
            }

            if (string.IsNullOrWhiteSpace(plainTextPassword))
            {
                _logger.LogWarning("Authentication failed - empty password for user: {UserName}", userName);
                return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.PasswordError.NullOrEmpty);
            }

            var result = await _dataService.AuthenticateSMSApplicationUserAsync(userName, plainTextPassword, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                var user = result.Value;
                
                // Business rule - check if user is active
                if (!user.IsActive)
                {
                    _logger.LogWarning("Authentication failed - user is inactive: {UserName}", userName);
                    return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.BaseUserError.InactiveUser);
                }

                // Business rule - check if password needs to be changed
                if (user.RequiresPasswordChange)
                {
                    _logger.LogInformation("User {UserName} requires password change", userName);
                    // Could return specific result indicating password change required
                }

                _logger.LogInformation("Successfully authenticated SMS Application User: {UserName}", userName);
            }
            else
            {
                _logger.LogWarning("Authentication failed for user: {UserName}", userName);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during authentication for user: {UserName}", userName);
            return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.LoginFailed);
        }
    }

    /// <summary>
    /// Changes user password with business validation
    /// </summary>
    public async Task<Result<bool>> ChangePasswordAsync(string userId, string currentPassword, string newPassword, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Changing password for SMS Application User with ID: {Id}", userId);

            // Get the user first
            var userResult = await _dataService.GetSMSApplicationUserByIdAsync(userId, ct).ConfigureAwait(false);
            if (userResult.IsFailure)
            {
                return Result<bool>.Failure<bool>(userResult.Error);
            }

            var user = userResult.Value;

            // Business validation - verify current password
            if (!user.Authenticate(currentPassword))
            {
                _logger.LogWarning("Password change failed - invalid current password for user: {Id}", userId);
                return Result<bool>.Failure<bool>(DomainErrors.PasswordError.VerificationFailed);
            }

            // Business validation - ensure new password is different
            if (user.Authenticate(newPassword))
            {
                _logger.LogWarning("Password change failed - new password same as current for user: {Id}", userId);
                return Result<bool>.Failure<bool>(DomainErrors.PasswordError.RecentlyUsed);
            }

            // Update password using domain logic
            var passwordResult = SMS_Domain.ValueObjects.Password.Create(newPassword);
            if (passwordResult.IsFailure)
            {
                return Result<bool>.Failure<bool>(passwordResult.Error);
            }

            user.UpdatePassword(passwordResult.Value);

            // Save the updated user
            var updateResult = await _dataService.UpdateSMSApplicationUserAsync(user, ct).ConfigureAwait(false);
            if (updateResult.IsFailure)
            {
                return Result<bool>.Failure<bool>(updateResult.Error);
            }

            _logger.LogInformation("Successfully changed password for SMS Application User with ID: {Id}", userId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error changing password for user: {UserId}", userId);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.PasswordUpdateFailed);
        }
    }

    /// <summary>
    /// Gets comprehensive user statistics with business analysis
    /// </summary>
    public async Task<Result<UserStatistics>> GetSMSApplicationUserStatisticsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving SMS Application User statistics");
            
            var result = await _dataService.GetSMSApplicationUserStatisticsAsync(ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                var stats = result.Value;
                _logger.LogInformation("Retrieved statistics: {TotalUsers} total, {ActiveUsers} active, {InactiveUsers} inactive", 
                    stats.TotalUsers, stats.ActiveUsers, stats.InactiveUsers);

                // Business analysis - log warnings for concerning statistics
                if (stats.InactiveUsers > stats.ActiveUsers)
                {
                    _logger.LogWarning("More inactive users ({InactiveUsers}) than active users ({ActiveUsers})", 
                        stats.InactiveUsers, stats.ActiveUsers);
                }

                if (stats.UsersRequiringPasswordChange > stats.TotalUsers * 0.5)
                {
                    _logger.LogWarning("High number of users requiring password change: {Count}", stats.UsersRequiringPasswordChange);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving SMS Application User statistics");
            return Result<UserStatistics>.Failure<UserStatistics>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}