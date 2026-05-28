//-----------------------------------------------------------------------
// <copyright file="SMSUserRoleService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS User management service handling user lifecycle and authentication operations.
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using Microsoft.Extensions.Logging;

using SMS_Application.Interfaces;

namespace SMS_Application.Services;

/// <summary>
/// Application service for SMS User Role management operations
/// Provides business logic abstraction over data service layer
/// </summary>
public interface ISMSUserRoleService
{
    Task<Result<SMSUserRole>> CreateUserRoleAsync(SMSUserRole userRole);
    Task<Result<IEnumerable<SMSUserRole>>> GetAllUserRolesAsync();
    Task<Result<IEnumerable<SMSUserRole>>> GetAllActiveUserRolesAsync();
    Task<Result<SMSUserRole>> GetUserRoleByIdAsync(string id);
    Task<Result<IEnumerable<SMSUserRole>>> GetUserRolesByApplicationUserIdAsync(string userId);
    Task<Result<IEnumerable<SMSUserRole>>> GetUserRolesByStakeholderUserIdAsync(string userId);
    Task<Result<IEnumerable<SMSUserRole>>> GetUserRolesByRoleValueAsync(string roleValue);
    Task<Result<IEnumerable<SMSUserRole>>> GetUserRolesByDepartmentAsync(string department);
    Task<Result<IEnumerable<SMSUserRole>>> GetUserRolesByUserTypeAsync(string userType);

    Task<Result<SMSUserRole>> UpdateUserRoleAsync(SMSUserRole userRole);
    Task<Result<bool>> DeleteUserRoleAsync(string id);
    //Task<Result<UserRoleStatistics>> GetUserRoleStatisticsAsync();
    //Task<Result<bool>> ValidateUserHasRoleAsync(string userId, string roleValue);
}

/// <summary>
/// Application service implementation for SMS User Role operations
/// </summary>
public sealed class SMSUserRoleService : ISMSUserRoleService
{
    private readonly SMSUserRoleDataService _dataService;
    private readonly ILogger<SMSUserRoleService> _logger;

    public SMSUserRoleService(
        SMSUserRoleDataService dataService,
        ILogger<SMSUserRoleService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new SMS User Role assignment
    /// </summary>
    public async Task<Result<SMSUserRole>> CreateUserRoleAsync(SMSUserRole userRole)
    {
        try
        {
            if (userRole is null)
            {
                _logger.LogApplicationError("CreateUserRoleAsync received null userRole");
                return Result<SMSUserRole>.Failure<SMSUserRole>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogApplicationInformation("Creating user role assignment: Code={Code}", userRole.Code);

            var result = await _dataService.CreateSMSUserRoleAsync(userRole);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully created user role assignment with ID: {Id}", result.Value?.Id);
            }
            else
            {
                _logger.LogApplicationError("Failed to create user role assignment: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error creating user role assignment");
            return Result<SMSUserRole>.Failure<SMSUserRole>(DomainErrors.SMSUserRoleError.CreateFailed);
        }
    }

    /// <summary>
    /// Gets all SMS User Role assignments
    /// </summary>
    public async Task<Result<IEnumerable<SMSUserRole>>> GetAllUserRolesAsync()
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving all user role assignments");
            var result = await _dataService.GetAllSMSUserRolesAsync();

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully retrieved {Count} user role assignments", result.Value?.Count() ?? 0);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving all user role assignments");
            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }

    /// <summary>
    /// Gets all active SMS User Role assignments
    /// </summary>
    public async Task<Result<IEnumerable<SMSUserRole>>> GetAllActiveUserRolesAsync()
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving all active user role assignments");
            var result = await _dataService.GetAllActiveSMSUserRolesAsync();

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully retrieved {Count} active user role assignments", result.Value?.Count() ?? 0);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving active user role assignments");
            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }

    /// <summary>
    /// Gets SMS User Role assignment by ID
    /// </summary>
    public async Task<Result<SMSUserRole>> GetUserRoleByIdAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.LogApplicationError("GetUserRoleByIdAsync received null or empty id");
                return Result<SMSUserRole>.Failure<SMSUserRole>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogApplicationInformation("Retrieving user role assignment with ID: {Id}", id);
            return await _dataService.GetSMSUserRoleByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving user role assignment with ID: {Id}", id);
            return Result<SMSUserRole>.Failure<SMSUserRole>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }

    /// <summary>
    /// Gets SMS User Role assignments by user ID
    /// </summary>
    public async Task<Result<IEnumerable<SMSUserRole>>> GetUserRolesByApplicationUserIdAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogApplicationError("GetUserRolesByUserIdAsync received null or empty userId");
                return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogApplicationInformation("Retrieving user role assignments for user: {UserId}", userId);
            var result = await _dataService.GetSMSUserRolesByApplicationUserIdAsync(userId);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully retrieved {Count} user role assignments for user {UserId}",
                    result.Value?.Count() ?? 0, userId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving user role assignments for user: {UserId}", userId);
            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }
    public async Task<Result<IEnumerable<SMSUserRole>>> GetUserRolesByStakeholderUserIdAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogApplicationError("GetUserRolesByUserIdAsync received null or empty userId");
                return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogApplicationInformation("Retrieving user role assignments for user: {UserId}", userId);
            var result = await _dataService.GetSMSUserRolesByStakeholderUserIdAsync(userId);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully retrieved {Count} user role assignments for user {UserId}",
                    result.Value?.Count() ?? 0, userId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving user role assignments for user: {UserId}", userId);
            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }

    /// <summary>
    /// Gets active SMS User Role assignments by user ID
    /// </summary>
    //public async Task<Result<IEnumerable<SMSUserRole>>> GetActiveUserRolesByUserIdAsync(string userId)
    //{
    //    try
    //    {
    //        if (string.IsNullOrWhiteSpace(userId))
    //        {
    //            _logger.LogApplicationError("GetActiveUserRolesByUserIdAsync received null or empty userId");
    //            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NullOrEmpty);
    //        }

    //        _logger.LogApplicationInformation("Retrieving active user role assignments for user: {UserCode}", userId);
    //        var result = await _dataService.GetActiveSMSUserRolesByUserIdAsync(userId);

    //        if (result.IsSuccess)
    //        {
    //            _logger.LogApplicationInformation("Successfully retrieved {Count} active user role assignments for user {UserCode}", 
    //                result.Value?.Count() ?? 0, userId);
    //        }

    //        return result;
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogApplicationError(ex, "Unexpected error retrieving active user role assignments for user: {UserCode}", userId);
    //        return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
    //    }
    //}

    /// <summary>
    /// Gets SMS User Role assignments by role value
    /// </summary>
    public async Task<Result<IEnumerable<SMSUserRole>>> GetUserRolesByRoleValueAsync(string roleValue)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(roleValue))
            {
                _logger.LogApplicationError("GetUserRolesByRoleValueAsync received null or empty roleValue");
                return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogApplicationInformation("Retrieving user role assignments for role: {RoleValue}", roleValue);
            return await _dataService.GetSMSUserRolesByRoleValueAsync(roleValue);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving user role assignments for role: {RoleValue}", roleValue);
            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }

    /// <summary>
    /// Gets SMS User Role assignments by department
    /// </summary>
    public async Task<Result<IEnumerable<SMSUserRole>>> GetUserRolesByDepartmentAsync(string department)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(department))
            {
                _logger.LogApplicationError("GetUserRolesByDepartmentAsync received null or empty department");
                return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogApplicationInformation("Retrieving user role assignments for department: {Department}", department);
            return await _dataService.GetSMSUserRolesByDepartmentAsync(department);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving user role assignments for department: {Department}", department);
            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }

    /// <summary>
    /// Gets SMS User Role assignments by user type
    /// </summary>
    public async Task<Result<IEnumerable<SMSUserRole>>> GetUserRolesByUserTypeAsync(string userType)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userType))
            {
                _logger.LogApplicationError("GetUserRolesByUserTypeAsync received null or empty userType");
                return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogApplicationInformation("Retrieving user role assignments for user type: {UserType}", userType);
            return await _dataService.GetSMSUserRolesByUserTypeAsync(userType);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving user role assignments for user type: {UserType}", userType);
            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }

    /// <summary>
    /// Gets expiring SMS User Role assignments
    /// </summary>
    public async Task<Result<IEnumerable<SMSUserRole>>> GetExpiringUserRolesAsync(DateTime cutoffDate)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving expiring user role assignments before: {CutoffDate}", cutoffDate);
            var result = await _dataService.GetExpiringSMSUserRolesAsync(cutoffDate);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully retrieved {Count} expiring user role assignments",
                    result.Value?.Count() ?? 0);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving expiring user role assignments");
            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }

    /// <summary>
    /// Updates an existing SMS User Role assignment
    /// </summary>
    public async Task<Result<SMSUserRole>> UpdateUserRoleAsync(SMSUserRole userRole)
    {
        try
        {
            if (userRole is null)
            {
                _logger.LogApplicationError("UpdateUserRoleAsync received null userRole");
                return Result<SMSUserRole>.Failure<SMSUserRole>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogApplicationInformation("Updating user role assignment with ID: {Id}", userRole.Id);

            var result = await _dataService.UpdateSMSUserRoleAsync(userRole);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully updated user role assignment with ID: {Id}", userRole.Id);
            }
            else
            {
                _logger.LogApplicationError("Failed to update user role assignment: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error updating user role assignment with ID: {Id}", userRole?.Id);
            return Result<SMSUserRole>.Failure<SMSUserRole>(DomainErrors.SMSUserRoleError.UpdateFailed);
        }
    }

    /// <summary>
    /// Deletes an SMS User Role assignment
    /// </summary>
    public async Task<Result<bool>> DeleteUserRoleAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.LogApplicationError("DeleteUserRoleAsync received null or empty id");
                return Result<bool>.Failure<bool>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogApplicationInformation("Deleting user role assignment with ID: {Id}", id);

            var result = await _dataService.DeleteSMSUserRoleAsync(id);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully deleted user role assignment with ID: {Id}", id);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete user role assignment: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error deleting user role assignment with ID: {Id}", id);
            return Result<bool>.Failure<bool>(DomainErrors.SMSUserRoleError.DeleteFailed);
        }
    }

    /// <summary>
    /// Gets SMS User Role statistics
    /// </summary>
    //public async Task<Result<UserRoleStatistics>> GetUserRoleStatisticsAsync()
    //{
    //    try
    //    {
    //        _logger.LogApplicationInformation("Retrieving user role statistics");
    //        var result = await _dataService.GetSMSUserRoleStatisticsAsync();

    //        if (result.IsSuccess)
    //        {
    //            _logger.LogApplicationInformation("Successfully retrieved user role statistics");
    //        }

    //        return result;
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogApplicationError(ex, "Unexpected error retrieving user role statistics");
    //        return Result<UserRoleStatistics>.Failure<UserRoleStatistics>(DomainErrors.GeneralError.UnProcessableRequest);
    //    }
    //}

    /// <summary>
    /// Validates if a user has a specific role
    /// </summary>
    //public async Task<Result<bool>> ValidateUserHasRoleAsync(string userId, string roleValue)
    //{
    //    try
    //    {
    //        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(roleValue))
    //        {
    //            _logger.LogApplicationError("ValidateUserHasRoleAsync received null or empty parameters");
    //            return Result<bool>.Failure<bool>(DomainErrors.SMSUserRoleError.NullOrEmpty);
    //        }

    //        _logger.LogApplicationInformation("Validating if user {UserCode} has role {RoleValue}", userId, roleValue);

    //        var result = await _dataService.ValidateUserRoleAsync(userId, roleValue);

    //        if (result.IsSuccess)
    //        {
    //            _logger.LogApplicationInformation("User {UserCode} role validation result: {HasRole}", userId, result.Value);
    //        }

    //        return result;
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogApplicationError(ex, "Unexpected error validating user role");
    //        return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
    //    }
    //}
}

