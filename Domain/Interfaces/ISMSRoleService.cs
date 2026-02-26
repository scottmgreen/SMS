//-----------------------------------------------------------------------
// <copyright file="ISMSRoleService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain service contract defining complex business operations for SMS smsrole coordination.
//                  Domain service contract defining business operations
//                  and ensuring clean architecture boundaries.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Interfaces;

/// <summary>
/// Domain service interface for SMS role management and authorization
/// </summary>
public interface ISMSRoleService
{
    /// <summary>
    /// Assigns a role to a user with full audit trail
    /// </summary>


    /// <summary>
    /// Removes a role assignment from a user
    /// </summary>
    Task RemoveRoleFromUserAsync(string userID, string roleValue, string removedBy, string reason);

    /// <summary>
    /// Gets all active role assignments for a user
    /// </summary>
    Task<IEnumerable<SMSUserRole>> GetActiveUserRolesAsync(string userID);

    /// <summary>
    /// Gets all users assigned to a specific role
    /// </summary>
    Task<IEnumerable<SMSUserRole>> GetUsersWithRoleAsync(SMSApplicationUserRoleID role);

    /// <summary>
    /// Checks if a user has a specific role
    /// </summary>
    Task<bool> UserHasRoleAsync(string userID, SMSApplicationUserRoleID role);

    /// <summary>
    /// Checks if a user has any role in a specific category
    /// </summary>
    Task<bool> UserHasRoleInCategoryAsync(string userID, string category);

    /// <summary>
    /// Gets the highest authority level for a user across all their roles
    /// </summary>
    Task<int> GetUserMaxAuthorityLevelAsync(string userID);

    /// <summary>
    /// Checks if a user can approve at the specified authority level
    /// </summary>
    Task<bool> UserCanApproveAsync(string userID, int requiredAuthorityLevel);

    /// <summary>
    /// Gets users who can approve at the specified authority level
    /// </summary>
    Task<IEnumerable<SMSUserRole>> GetUsersWhoCanApproveAsync(int requiredAuthorityLevel);

    /// <summary>
    /// Gets role assignments that are expiring within the specified number of days
    /// </summary>
    Task<IEnumerable<SMSUserRole>> GetExpiringRoleAssignmentsAsync(int daysFromNow);

    /// <summary>
    /// Validates that a user has the required role for an operation
    /// </summary>
    Task<bool> ValidateUserAuthorizationAsync(string userID, SMSApplicationUserRoleID requiredRole);

    /// <summary>
    /// Gets all role assignments for audit purposes
    /// </summary>
    Task<IEnumerable<SMSUserRole>> GetRoleAssignmentHistoryAsync(string? userID = null, string? roleValue = null);

    /// <summary>
    /// Bulk assigns roles to multiple users
    /// </summary>
    //Task<IEnumerable<SMSApplicationUserRole>> BulkAssignRoleAsync(
    //    IEnumerable<string> userIDs,
    //    string userType,
    //    SMSRole role,
    //    string department,
    //    string assignedBy);

    /// <summary>
    /// Gets role assignment statistics for reporting
    /// </summary>
    Task<Dictionary<string, int>> GetRoleAssignmentStatsAsync();
}
