//-----------------------------------------------------------------------
// <copyright file="ISMSUserRoleRepository.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Repository implementing data access operations for SMS ismsuserrole entities with CRUD operations and business queries.
//                  Infrastructure service contract defining data access operations
//                  and external system integration interfaces.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Infrastructure.Interfaces;

/// <summary>
/// Repository interface for SMS User Role operations
/// </summary>
public interface ISMSUserRoleRepository
{
    /// <summary>
    /// Gets all user role assignments
    /// </summary>
    Task<Result<IEnumerable<SMSUserRole>>> GetAllAsync();

    /// <summary>
    /// Gets all active user role assignments
    /// </summary>
    Task<Result<IEnumerable<SMSUserRole>>> GetAllActiveAsync();

    /// <summary>
    /// Gets a user role by ID
    /// </summary>
    Task<Result<SMSUserRole>> GetByIdAsync(string id);

    /// <summary>
    /// Gets all role assignments for a specific user
    /// </summary>
    Task<Result<IEnumerable<SMSUserRole>>> GetByApplicationUserIdAsync(string userId);
    Task<Result<IEnumerable<SMSUserRole>>> GetByStakeholderUserIdAsync(string userId);

    /// <summary>
    /// Gets active role assignments for a specific user
    /// </summary>
    //Task<Result<IEnumerable<SMSUserRole>>> GetActiveRolesByUserIdAsync(string userId);

    /// <summary>
    /// Gets all users assigned to a specific role
    /// </summary>
    Task<Result<IEnumerable<SMSUserRole>>> GetByRoleValueAsync(string roleValue);

    /// <summary>
    /// Gets role assignments expiring before the specified date
    /// </summary>
    Task<Result<IEnumerable<SMSUserRole>>> GetExpiringRolesAsync(DateTime cutoffDate);

    /// <summary>
    /// Gets role assignments by department
    /// </summary>
    Task<Result<IEnumerable<SMSUserRole>>> GetByDepartmentAsync(string department);

    /// <summary>
    /// Gets role assignments by user type
    /// </summary>
    Task<Result<IEnumerable<SMSUserRole>>> GetByUserTypeAsync(string userType);

    /// <summary>
    /// Adds a new user role assignment
    /// </summary>
    Task<Result<SMSUserRole>> AddAsync(SMSUserRole userRole);

    /// <summary>
    /// Updates an existing user role assignment
    /// </summary>
    Task<Result<bool>> UpdateAsync(SMSUserRole userRole);

    /// <summary>
    /// Deletes a user role assignment
    /// </summary>
    Task<Result<bool>> DeleteAsync(string id);

    /// <summary>
    /// Checks if a user has a specific role assignment
    /// </summary>
    //Task<Result<bool>> UserHasRoleAsync(string userId, string roleValue);
}
