using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Domain.Models;
using SMS_Infrastructure.Common;
using SMS_Infrastructure.Interfaces;
using SMS_Infrastructure.Persistence;
using SMS_Shared.Common;

namespace SMS_Infrastructure.Services;

/// <summary>
/// Data service for SMS User Role operations
/// Provides high-level data access abstraction over repository layer
/// </summary>
public sealed class SMSUserRoleDataService : BaseDataService<SMSUserRoleDataService>
{
    private readonly ISMSUserRoleRepository _repository;
    private readonly ILogger<SMSUserRoleDataService> _logger;

    public SMSUserRoleDataService(
        ILogger<SMSUserRoleDataService> logger,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        ISMSUserRoleRepository repository)
        : base(logger, serviceScopeFactory, configuration)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new SMS User Role assignment
    /// </summary>
    public async Task<Result<SMSUserRole>> CreateSMSUserRoleAsync(SMSUserRole userRole, CancellationToken ct = default)
    {
        try
        {
            if (userRole is null)
            {
                _logger.LogError("CreateSMSUserRoleAsync received null userRole");
                return Result<SMSUserRole>.Failure<SMSUserRole>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogInformation("Creating SMS User Role with code: {Code}", userRole.Code);

            var result = await _repository.AddAsync(userRole);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created SMS User Role with ID: {Id}", result.Value?.Id);
            }
            else
            {
                _logger.LogError("Failed to create SMS User Role. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating SMS User Role");
            return Result<SMSUserRole>.Failure<SMSUserRole>(DomainErrors.SMSUserRoleError.CreateFailed);
        }
    }

    /// <summary>
    /// Gets all SMS User Roles
    /// </summary>
    public async Task<Result<IEnumerable<SMSUserRole>>> GetAllSMSUserRolesAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving all SMS User Roles");
            return await _repository.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving all SMS User Roles");
            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }

    /// <summary>
    /// Gets all active SMS User Roles
    /// </summary>
    public async Task<Result<IEnumerable<SMSUserRole>>> GetAllActiveSMSUserRolesAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving all active SMS User Roles");
            return await _repository.GetAllActiveAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving active SMS User Roles");
            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }

    /// <summary>
    /// Gets SMS User Role by ID
    /// </summary>
    public async Task<Result<SMSUserRole>> GetSMSUserRoleByIdAsync(string id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving SMS User Role with ID: {Id}", id);
            return await _repository.GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving SMS User Role with ID: {Id}", id);
            return Result<SMSUserRole>.Failure<SMSUserRole>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }

    /// <summary>
    /// Gets SMS User Roles by User ID
    /// </summary>
    public async Task<Result<IEnumerable<SMSUserRole>>> GetSMSUserRolesByApplicationUserIdAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogError("GetSMSUserRolesByUserIdAsync received null or empty userId");
                return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogInformation("Retrieving SMS User Roles for user: {UserId}", userId);
            return await _repository.GetByApplicationUserIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving SMS User Roles for user: {UserId}", userId);
            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }
    public async Task<Result<IEnumerable<SMSUserRole>>> GetSMSUserRolesByStakeholderUserIdAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogError("GetSMSUserRolesByUserIdAsync received null or empty userId");
                return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogInformation("Retrieving SMS User Roles for user: {UserId}", userId);
            return await _repository.GetByStakeholderUserIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving SMS User Roles for user: {UserId}", userId);
            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }

    /// <summary>
    /// Gets active SMS User Roles by User ID
    /// </summary>
    //public async Task<Result<IEnumerable<SMSUserRole>>> GetActiveSMSUserRolesByUserIdAsync(string userId, CancellationToken ct = default)
    //{
    //    try
    //    {
    //        if (string.IsNullOrWhiteSpace(userId))
    //        {
    //            _logger.LogError("GetActiveSMSUserRolesByUserIdAsync received null or empty userId");
    //            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NullOrEmpty);
    //        }

    //        _logger.LogInformation("Retrieving active SMS User Roles for user: {UserId}", userId);
    //        return await _repository.GetActiveRolesByUserIdAsync(userId);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Unexpected error retrieving active SMS User Roles for user: {UserId}", userId);
    //        return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
    //    }
    //}

    /// <summary>
    /// Gets SMS User Roles by role value
    /// </summary>
    public async Task<Result<IEnumerable<SMSUserRole>>> GetSMSUserRolesByRoleValueAsync(string roleValue, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(roleValue))
            {
                _logger.LogError("GetSMSUserRolesByRoleValueAsync received null or empty roleValue");
                return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogInformation("Retrieving SMS User Roles for role value: {RoleValue}", roleValue);
            return await _repository.GetByRoleValueAsync(roleValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving SMS User Roles for role value: {RoleValue}", roleValue);
            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }

    /// <summary>
    /// Gets SMS User Roles by department
    /// </summary>
    public async Task<Result<IEnumerable<SMSUserRole>>> GetSMSUserRolesByDepartmentAsync(string department, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(department))
            {
                _logger.LogError("GetSMSUserRolesByDepartmentAsync received null or empty department");
                return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogInformation("Retrieving SMS User Roles for department: {Department}", department);
            return await _repository.GetByDepartmentAsync(department);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving SMS User Roles for department: {Department}", department);
            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }

    /// <summary>
    /// Gets SMS User Roles by user type
    /// </summary>
    public async Task<Result<IEnumerable<SMSUserRole>>> GetSMSUserRolesByUserTypeAsync(string userType, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userType))
            {
                _logger.LogError("GetSMSUserRolesByUserTypeAsync received null or empty userType");
                return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogInformation("Retrieving SMS User Roles for user type: {UserType}", userType);
            return await _repository.GetByUserTypeAsync(userType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving SMS User Roles for user type: {UserType}", userType);
            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }

    /// <summary>
    /// Gets expiring SMS User Roles
    /// </summary>
    public async Task<Result<IEnumerable<SMSUserRole>>> GetExpiringSMSUserRolesAsync(DateTime cutoffDate, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving expiring SMS User Roles before: {CutoffDate}", cutoffDate);
            return await _repository.GetExpiringRolesAsync(cutoffDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving expiring SMS User Roles");
            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }

    /// <summary>
    /// Updates an existing SMS User Role
    /// </summary>
    public async Task<Result<SMSUserRole>> UpdateSMSUserRoleAsync(SMSUserRole userRole, CancellationToken ct = default)
    {
        try
        {
            if (userRole is null)
            {
                _logger.LogError("UpdateSMSUserRoleAsync received null userRole");
                return Result<SMSUserRole>.Failure<SMSUserRole>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogInformation("Updating SMS User Role with ID: {Id}", userRole.Id);

            var updateResult = await _repository.UpdateAsync(userRole);
            if (updateResult.IsFailure)
            {
                _logger.LogError("Failed to update SMS User Role. Error: {Error}", updateResult.Error?.Message);
                return Result<SMSUserRole>.Failure<SMSUserRole>(updateResult.Error);
            }

            // Return the updated user role
            return await _repository.GetByIdAsync(userRole.Code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating SMS User Role with ID: {Id}", userRole?.Id);
            return Result<SMSUserRole>.Failure<SMSUserRole>(DomainErrors.SMSUserRoleError.UpdateFailed);
        }
    }

    /// <summary>
    /// Deletes an SMS User Role
    /// </summary>
    public async Task<Result<bool>> DeleteSMSUserRoleAsync(string id, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                _logger.LogError("DeleteSMSUserRoleAsync received null or empty id");
                return Result<bool>.Failure<bool>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogInformation("Deleting SMS User Role with ID: {Id}", id);
            var result = await _repository.DeleteAsync(id);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted SMS User Role with ID: {Id}", id);
            }
            else
            {
                _logger.LogError("Failed to delete SMS User Role. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting SMS User Role with ID: {Id}", id);
            return Result<bool>.Failure<bool>(DomainErrors.SMSUserRoleError.DeleteFailed);
        }
    }

    /// <summary>
    /// Gets SMS User Role statistics
    /// </summary>
    //public async Task<Result<UserRoleStatistics>> GetSMSUserRoleStatisticsAsync(CancellationToken ct = default)
    //{
    //    try
    //    {
    //        _logger.LogInformation("Retrieving SMS User Role statistics");
    //        return await _repository.GetUserRoleStatisticsAsync();
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Unexpected error retrieving SMS User Role statistics");
    //        return Result<UserRoleStatistics>.Failure<UserRoleStatistics>(DomainErrors.GeneralError.UnProcessableRequest);
    //    }
    //}

    /// <summary>
    /// Validates if a user has a specific role
    /// </summary>
    //public async Task<Result<bool>> ValidateUserRoleAsync(string userId, string roleValue, CancellationToken ct = default)
    //{
    //    try
    //    {
    //        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(roleValue))
    //        {
    //            _logger.LogError("ValidateUserRoleAsync received null or empty parameters");
    //            return Result<bool>.Failure<bool>(DomainErrors.SMSUserRoleError.NullOrEmpty);
    //        }

    //        _logger.LogInformation("Validating role {RoleValue} for user {UserId}", roleValue, userId);
            
    //        var activeRolesResult = await _repository.GetActiveRolesByUserIdAsync(userId);
    //        if (activeRolesResult.IsFailure)
    //        {
    //            return Result<bool>.Failure<bool>(activeRolesResult.Error);
    //        }

    //        var hasRole = activeRolesResult.Value.Any(ur => 
    //            string.Equals(ur.Code, roleValue, StringComparison.OrdinalIgnoreCase));
                
    //        return Result<bool>.Success(hasRole);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Unexpected error validating user role");
    //        return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
    //    }
    //}
}