//-----------------------------------------------------------------------
// <copyright file="ISMSOrganizationalGroupRepository.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Repository contract defining data access operations for SMS smsorganizationalgroup entities.
//                  Domain service contract defining business operations
//                  and ensuring clean architecture boundaries.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Interfaces;

/// <summary>
/// Repository interface for SMS Organizational Group operations
/// Provides CRUD operations and group membership management for organizational groups
/// </summary>
public interface ISMSOrganizationalGroupRepository
{
    /// <summary>
    /// Retrieves all SMS organizational groups
    /// </summary>
    /// <returns>A result containing all organizational groups or an error</returns>
    Task<Result<IEnumerable<SMSOrganizationalGroup>>> GetAllAsync();

    /// <summary>
    /// Retrieves an SMS organizational group by its code
    /// </summary>
    /// <param name="code">The unique code of the organizational group</param>
    /// <returns>A result containing the organizational group or an error if not found</returns>
    Task<Result<SMSOrganizationalGroup>> GetByCodeAsync(string code);

    /// <summary>
    /// Adds a new SMS organizational group
    /// </summary>
    /// <param name="group">The organizational group to add</param>
    /// <returns>A result containing the created organizational group or an error</returns>
    Task<Result<SMSOrganizationalGroup>> AddAsync(SMSOrganizationalGroup group);

    /// <summary>
    /// Updates an existing SMS organizational group
    /// </summary>
    /// <param name="group">The organizational group with updated information</param>
    /// <returns>A result indicating success or failure</returns>
    Task<Result<bool>> UpdateAsync(SMSOrganizationalGroup group);

    /// <summary>
    /// Deletes an SMS organizational group by its code
    /// </summary>
    /// <param name="code">The unique code of the organizational group to delete</param>
    /// <returns>A result indicating success or failure</returns>
    Task<Result<bool>> DeleteAsync(string code);

    #region Group Membership Operations

    /// <summary>
    /// Assigns a user to an organizational group
    /// </summary>
    /// <param name="userCode">The code of the user to assign</param>
    /// <param name="groupCode">The code of the group to assign the user to</param>
    /// <returns>A result indicating success or failure</returns>
    Task<Result<bool>> AssignUserToGroupAsync(string userCode, string groupCode);

    /// <summary>
    /// Removes a user from an organizational group
    /// </summary>
    /// <param name="userCode">The code of the user to remove</param>
    /// <param name="groupCode">The code of the group to remove the user from</param>
    /// <returns>A result indicating success or failure</returns>
    Task<Result<bool>> RemoveUserFromGroupAsync(string userCode, string groupCode);

    /// <summary>
    /// Retrieves all users belonging to a specific organizational group
    /// </summary>
    /// <param name="groupCode">The code of the organizational group</param>
    /// <returns>A result containing the users in the group or an error</returns>
    Task<Result<IEnumerable<SMSOrganizationalUser>>> GetUsersByGroupCodeAsync(string groupCode);

    /// <summary>
    /// Clears all group memberships for a specific user
    /// </summary>
    /// <param name="userCode">The code of the user to clear group memberships for</param>
    /// <param name="clearedBy">The user who performed the clear operation</param>
    /// <returns>A result indicating success or failure</returns>
    Task<Result<bool>> ClearUserGroupsAsync(string userCode, string clearedBy);

    #endregion
}
