//-----------------------------------------------------------------------
// <copyright file="SMSStakeholderUserDataService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS Stakeholder User data service providing business operations for SMS domain entities.
//                  Infrastructure layer service implementing data access patterns
//                  through repositories while maintaining clean architecture.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Domain.Interfaces;

namespace SMS_Infrastructure.Services;

/// <summary>
/// SMS Stakeholder User Data Service providing business operations for SMSStakeholderUser entities
/// </summary>
public class SMSStakeholderUserDataService : BaseDataService<SMSStakeholderUserDataService>
{
    private readonly SMSStakeholderUserRepository _repository;
    private readonly ILogger<SMSStakeholderUserDataService> _logger;

    public SMSStakeholderUserDataService(
        ILogger<SMSStakeholderUserDataService> logger,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        SMSStakeholderUserRepository repository)
        : base(logger, serviceScopeFactory, configuration)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new SMS Stakeholder User
    /// </summary>
    public async Task<Result<SMSStakeholderUser>> AddAsync(SMSStakeholderUser user, CancellationToken ct = default)
    {
        try
        {
            if (user is null)
            {
                _logger.LogError("CreateSMSStakeholderUserAsync received null user");
                return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.NullOrEmpty);
            }

            _logger.LogInformation("Creating SMS Stakeholder User with code: {Code}", user.Code);

            // Check if username already exists
            var existsResult = await _repository.UserNameExistsAsync(user.UserName.Value);
            if (existsResult.IsSuccess && existsResult.Value)
            {
                _logger.LogWarning("Username {UserName} already exists", user.UserName.Value);
                return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.UserNameError.AlreadyExists);
            }

            var result = await _repository.AddAsync(user);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created SMS Stakeholder User with ID: {Id}", result.Value?.UserId);
            }
            else
            {
                _logger.LogError("Failed to create SMS Stakeholder User. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating SMS Stakeholder User");
            return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.CreateFailed);
        }
    }

    /// <summary>
    /// Gets SMS Stakeholder User by ID
    /// </summary>
    public async Task<Result<SMSStakeholderUser>> GetByCodeAsync(string id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving SMS Stakeholder User with ID: {Id}", id);
            return await _repository.GetByCodeAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving SMS Stakeholder User with ID: {Id}", id);
            return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.NotFound);
        }
    }

    /// <summary>
    /// Gets SMS Stakeholder User by username
    /// </summary>
    public async Task<Result<SMSStakeholderUser>> GetByUserNameAsync(string userName, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving SMS Stakeholder User with UserName: {UserName}", userName);
            return await _repository.GetByUserNameAsync(userName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving SMS Stakeholder User with UserName: {UserName}", userName);
            return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.NotFound);
        }
    }

    /// <summary>
    /// Gets all SMS Stakeholder Users
    /// </summary>
    public async Task<Result<IEnumerable<SMSStakeholderUser>>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving all SMS Stakeholder Users");
            return await _repository.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving all SMS Stakeholder Users");
            return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderUserError.NotFound);
        }
    }

    /// <summary>
    /// Gets all active SMS Stakeholder Users
    /// </summary>
    public async Task<Result<IEnumerable<SMSStakeholderUser>>> GetActiveAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving active SMS Stakeholder Users");
            return await _repository.GetActiveUsersAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving active SMS Stakeholder Users");
            return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderUserError.NotFound);
        }
    }

    /// <summary>
    /// Gets SMS Stakeholder Users by stakeholder type
    /// </summary>
    public async Task<Result<IEnumerable<SMSStakeholderUser>>> GetByStakeholderTypeAsync(string stakeholderType, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving SMS Stakeholder Users by type: {StakeholderType}", stakeholderType);
            return await _repository.GetByStakeholderTypeAsync(stakeholderType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving SMS Stakeholder Users by type: {StakeholderType}", stakeholderType);
            return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderUserError.NotFound);
        }
    }

    /// <summary>
    /// Gets SMS Stakeholder Users by organization
    /// </summary>
    public async Task<Result<IEnumerable<SMSStakeholderUser>>> GetByOrganizationAsync(string organization, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving SMS Stakeholder Users by organization: {Organization}", organization);
            return await _repository.GetByOrganizationAsync(organization);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving SMS Stakeholder Users by organization: {Organization}", organization);
            return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderUserError.NotFound);
        }
    }

    

    

    

    /// <summary>
    /// Gets users requiring AOA access
    /// </summary>


    /// <summary>
    /// Updates an existing SMS Stakeholder User
    /// </summary>
    public async Task<Result<SMSStakeholderUser>> UpdateAsync(SMSStakeholderUser user, CancellationToken ct = default)
    {
        try
        {
            if (user is null)
            {
                _logger.LogError("UpdateSMSStakeholderUserAsync received null user");
                return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.NullOrEmpty);
            }

            _logger.LogInformation("Updating SMS Stakeholder User with ID: {Id}", user.UserId);

            var updateResult = await _repository.UpdateAsync(user);
            if (updateResult.IsFailure)
            {
                _logger.LogError("Failed to update SMS Stakeholder User. Error: {Error}", updateResult.Error?.Message);
                return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(updateResult.Error);
            }

            // Return the updated user
            return await _repository.GetByCodeAsync(user.Code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating SMS Stakeholder User with ID: {Id}", user?.UserId);
            return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.UpdateFailed);
        }
    }

    /// <summary>
    /// Deletes an SMS Stakeholder User
    /// </summary>
    public async Task<Result<bool>> DeleteUserAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Deleting SMS Stakeholder User with ID: {Id}", userId);
            var result = await _repository.DeleteAsync(userId);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted SMS Stakeholder User with ID: {Id}", userId);
            }
            else
            {
                _logger.LogError("Failed to delete SMS Stakeholder User. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting SMS Stakeholder User with ID: {Id}", userId);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.DeleteFailed);
        }
    }

    /// <summary>
    /// Authenticates SMS Stakeholder User
    /// </summary>
    public async Task<Result<bool>> AuthenticateUserAsync(string userName, string plainTextPassword, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Authenticating SMS Stakeholder User: {UserName}", userName);

            var userResult = await _repository.GetByUserNameAsync(userName);
            if (userResult.IsFailure)
            {
                _logger.LogWarning("Authentication failed - user not found: {UserName}", userName);
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.LoginFailed);
            }

            var user = userResult.Value;

            if (!user.Authenticate(plainTextPassword))
            {
                _logger.LogWarning("Authentication failed - invalid password for user: {UserName}", userName);
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.LoginFailed);
            }

            // Record the login
            user.RecordLogin();
            await _repository.UpdateAsync(user);

            _logger.LogInformation("Successfully authenticated SMS Stakeholder User: {UserName}", userName);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during authentication for user: {UserName}", userName);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.LoginFailed);
        }
    }

    /// <summary>
    /// Gets stakeholder type statistics
    /// </summary>
    public async Task<Result<Dictionary<string, int>>> GetStakeholderTypeStatisticsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving stakeholder type statistics");
            return await _repository.GetStakeholderTypeStatisticsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving stakeholder type statistics");
            return Result<Dictionary<string, int>>.Failure<Dictionary<string, int>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    /// <summary>
    /// Gets organization statistics
    /// </summary>
    public async Task<Result<Dictionary<string, int>>> GetOrganizationStatisticsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving organization statistics");
            return await _repository.GetOrganizationStatisticsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving organization statistics");
            return Result<Dictionary<string, int>>.Failure<Dictionary<string, int>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    /// <summary>
    /// Gets SMS Stakeholder User statistics
    /// </summary>
    public async Task<Result<UserStatistics>> GetSMSStakeholderUserStatisticsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving SMS Stakeholder User statistics");
            return await _repository.GetUserStatisticsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving SMS Stakeholder User statistics");
            return Result<UserStatistics>.Failure<UserStatistics>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    /// <summary>
    /// Gets SMS Stakeholder Users by group code
    /// </summary>
    public async Task<Result<IEnumerable<SMSStakeholderUser>>> GetSMSStakeholderUsersByGroupCodeAsync(string groupCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupCode))
            {
                _logger.LogError("GetSMSStakeholderUsersByGroupCodeAsync received null or empty group code");
                return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderUserError.NullOrEmpty);
            }

            _logger.LogInformation("Retrieving SMS Stakeholder Users by group code: {GroupCode}", groupCode);
            return await _repository.GetUsersByGroupCodeAsync(groupCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving SMS Stakeholder Users by group code: {GroupCode}", groupCode);
            return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderUserError.NotFound);
        }
    }
    /// <summary>
    /// Updates SMS Application User password
    /// </summary>
    public async Task<Result<bool>> UpdateSMSStakeholderUserPasswordAsync(SMSStakeholderUserID userId, string hashedPassword, CancellationToken ct = default)
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
}
