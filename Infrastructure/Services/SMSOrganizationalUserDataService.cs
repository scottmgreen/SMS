//-----------------------------------------------------------------------
// <copyright file="SMSOrganizationalUserDataService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Data service coordinating smsorganizationaluser repository operations with business logic and validation.
//                  Infrastructure service providing external system integration
//                  and technical functionality support.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Domain.Interfaces;

namespace SMS_Infrastructure.Services;

/// <summary>
/// Data service for SMS Organizational User operations
/// Provides high-level data access abstraction over repository layer
/// </summary>
public sealed class SMSOrganizationalUserDataService : BaseDataService<SMSOrganizationalUserDataService>
{
    private readonly SMSOrganizationalUserRepository _repository;
    private readonly ILogger<SMSOrganizationalUserDataService> _logger;

    public SMSOrganizationalUserDataService(
        ILogger<SMSOrganizationalUserDataService> logger,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        SMSOrganizationalUserRepository repository)
        : base(logger, serviceScopeFactory, configuration)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new SMS Organizational User
    /// </summary>
    public async Task<Result<SMSOrganizationalUser>> CreateSMSOrganizationalUserAsync(SMSOrganizationalUser user, CancellationToken ct = default)
    {
        try
        {
            if (user is null)
            {
                _logger.LogInfrastructureError("CreateSMSOrganizationalUserAsync received null user");
                return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.NullOrEmpty);
            }

            _logger.LogInfrastructureInformation("Creating SMS Organizational User with code: {Code}", user.Code);

            // Check if username already exists
            var existsResult = await _repository.UserNameExistsAsync(user.UserName.Value);
            if (existsResult.IsSuccess && existsResult.Value)
            {
                _logger.LogInfrastructureWarning("Username {UserName} already exists", user.UserName.Value);
                return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.UserNameError.AlreadyExists);
            }

            var result = await _repository.AddAsync(user);

            if (result.IsSuccess)
            {
                _logger.LogInfrastructureInformation("Successfully created SMS Organizational User with ID: {Id}", result.Value?.UserId);
            }
            else
            {
                _logger.LogInfrastructureError("Failed to create SMS Organizational User. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Unexpected error creating SMS Organizational User");
            return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.CreateFailed);
        }
    }

    /// <summary>
    /// Gets SMS Organizational User by ID
    /// </summary>
    public async Task<Result<SMSOrganizationalUser>> GetSMSOrganizationalUserByCodeAsync(string code, CancellationToken ct = default)
    {
        try
        {
            SMSOrganizationalUserID orgid = new SMSOrganizationalUserID(code);
            _logger.LogInfrastructureInformation("Retrieving SMS Organizational User with Code: {Code}", code);
            return await _repository.GetByCodeAsync(orgid);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Unexpected error retrieving SMS Organizational User with Code: {Code}", code);
            return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.NotFound);
        }
    }

    /// <summary>
    /// Gets SMS Organizational User by username
    /// </summary>
    public async Task<Result<SMSOrganizationalUser>> GetSMSOrganizationalUserByUserNameAsync(string userName, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureInformation("Retrieving SMS Organizational User with UserName: {UserName}", userName);
            return await _repository.GetByUserNameAsync(userName);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Unexpected error retrieving SMS Organizational User with UserName: {UserName}", userName);
            return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.NotFound);
        }
    }

    /// <summary>
    /// Gets all SMS Organizational Users
    /// </summary>
    public async Task<Result<IEnumerable<SMSOrganizationalUser>>> GetAllSMSOrganizationalUsersAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureInformation("Retrieving all SMS Organizational Users");
            return await _repository.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Unexpected error retrieving all SMS Organizational Users");
            return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(DomainErrors.SMSOrganizationalUserError.NotFound);
        }
    }

    /// <summary>
    /// Gets all active SMS Organizational Users
    /// </summary>
    public async Task<Result<IEnumerable<SMSOrganizationalUser>>> GetActiveSMSOrganizationalUsersAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureInformation("Retrieving active SMS Organizational Users");
            return await _repository.GetActiveUsersAsync();
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Unexpected error retrieving active SMS Organizational Users");
            return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(DomainErrors.SMSOrganizationalUserError.NotFound);
        }
    }

    /// <summary>
    /// Gets SMS Organizational Users by department
    /// </summary>
    public async Task<Result<IEnumerable<SMSOrganizationalUser>>> GetSMSOrganizationalUsersByDepartmentAsync(string department, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureInformation("Retrieving SMS Organizational Users by department: {Department}", department);
            return await _repository.GetByDepartmentAsync(department);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Unexpected error retrieving SMS Organizational Users by department: {Department}", department);
            return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(DomainErrors.SMSOrganizationalUserError.NotFound);
        }
    }

    /// <summary>
    /// Gets SMS Organizational Users by position
    /// </summary>
    public async Task<Result<IEnumerable<SMSOrganizationalUser>>> GetSMSOrganizationalUsersByPositionAsync(string position, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureInformation("Retrieving SMS Organizational Users by position: {Position}", position);
            return await _repository.GetByPositionAsync(position);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Unexpected error retrieving SMS Organizational Users by position: {Position}", position);
            return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(DomainErrors.SMSOrganizationalUserError.NotFound);
        }
    }

    /// <summary>
    /// Gets department supervisors
    /// </summary>
    //public async Task<Result<IEnumerable<SMSOrganizationalUser>>> GetDepartmentSupervisorsAsync(string department, CancellationToken ct = default)
    //{
    //    try
    //    {
    //        _logger.LogInfrastructureInformation("Retrieving supervisors for department: {Department}", department);
    //        return await _repository.GetDepartmentSupervisorsAsync(department);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogInfrastructureError(ex, "Unexpected error retrieving supervisors for department: {Department}", department);
    //        return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(DomainErrors.SMSOrganizationalUserError.NotFound);
    //    }
    //}

    /// <summary>
    /// Updates an existing SMS Organizational User
    /// </summary>
    public async Task<Result<SMSOrganizationalUser>> UpdateSMSOrganizationalUserAsync(SMSOrganizationalUser user, CancellationToken ct = default)
    {
        try
        {
            if (user is null)
            {
                _logger.LogInfrastructureError("UpdateSMSOrganizationalUserAsync received null user");
                return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.NullOrEmpty);
            }

            _logger.LogInfrastructureInformation("Updating SMS Organizational User with ID: {Id}", user.UserId);

            var updateResult = await _repository.UpdateAsync(user);
            if (updateResult.IsFailure)
            {
                _logger.LogInfrastructureError("Failed to update SMS Organizational User. Error: {Error}", updateResult.Error?.Message);
                return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(updateResult.Error);
            }

            // Return the updated user
            return await _repository.GetByCodeAsync(new(user.Code));
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Unexpected error updating SMS Organizational User with Code: {Code}", user?.UserId);
            return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.UpdateFailed);
        }
    }

    /// <summary>
    /// Deletes an SMS Organizational User
    /// </summary>
    public async Task<Result<bool>> DeleteSMSOrganizationalUserAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureInformation("Deleting SMS Organizational User with ID: {Id}", userId);
            var result = await _repository.DeleteAsync(userId);

            if (result.IsSuccess)
            {
                _logger.LogInfrastructureInformation("Successfully deleted SMS Organizational User with ID: {Id}", userId);
            }
            else
            {
                _logger.LogInfrastructureError("Failed to delete SMS Organizational User. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Unexpected error deleting SMS Organizational User with ID: {Id}", userId);
            return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.DeleteFailed);
        }
    }

    /// <summary>
    /// Authenticates SMS Organizational User
    /// </summary>
    public async Task<Result<bool>> AuthenticateSMSOrganizationalUserAsync(string userName, string plainTextPassword, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureInformation("Authenticating SMS Organizational User: {UserName}", userName);

            var userResult = await _repository.GetByUserNameAsync(userName);
            if (userResult.IsFailure)
            {
                _logger.LogInfrastructureWarning("Authentication failed - user not found: {UserName}", userName);
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.LoginFailed);
            }

            var user = userResult.Value;

            if (!user.Authenticate(plainTextPassword))
            {
                _logger.LogInfrastructureWarning("Authentication failed - invalid password for user: {UserName}", userName);
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.LoginFailed);
            }

            // Record the login
            user.RecordLogin();
            await _repository.UpdateAsync(user);

            _logger.LogInfrastructureInformation("Successfully authenticated SMS Organizational User: {UserName}", userName);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Unexpected error during authentication for user: {UserName}", userName);
            return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.LoginFailed);
        }
    }

    /// <summary>
    /// Gets department statistics
    /// </summary>
    //public async Task<Result<Dictionary<string, int>>> GetDepartmentStatisticsAsync(CancellationToken ct = default)
    //{
    //    try
    //    {
    //        _logger.LogInfrastructureInformation("Retrieving department statistics");
    //        return await _repository.GetDepartmentStatisticsAsync();
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogInfrastructureError(ex, "Unexpected error retrieving department statistics");
    //        return Result<Dictionary<string, int>>.Failure<Dictionary<string, int>>(DomainErrors.GeneralError.UnProcessableRequest);
    //    }
    //}

    /// <summary>
    /// Gets SMS Organizational User statistics
    /// </summary>
    public async Task<Result<UserStatistics>> GetSMSOrganizationalUserStatisticsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureInformation("Retrieving SMS Organizational User statistics");
            return await _repository.GetUserStatisticsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Unexpected error retrieving SMS Organizational User statistics");
            return Result<UserStatistics>.Failure<UserStatistics>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
    /// <summary>
    /// Updates SMS Application User password
    /// </summary>
    public async Task<Result<bool>> UpdateSMSOrganizationalUserPasswordAsync(SMSOrganizationalUserID userId, string hashedPassword, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureInformation("Updating password for SMS Application User with ID: {Id}", userId.Value);
            var result = await _repository.UpdatePasswordAsync(userId, hashedPassword);

            if (result.IsSuccess)
            {
                _logger.LogInfrastructureInformation("Successfully updated password for SMS Application User with ID: {Id}", userId.Value);
            }
            else
            {
                _logger.LogInfrastructureError("Failed to update password for SMS Application User. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Unexpected error updating password for SMS Application User with ID: {Id}", userId.Value);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.PasswordUpdateFailed);
        }
    }
}

