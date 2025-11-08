using SMS_Domain.Entities;
using SMS_Domain.Interfaces;
using SMS_Shared.Common;

namespace SMS_Infrastructure.Interfaces;

/// <summary>
/// Defines the contract for SMS Application User repository operations
/// </summary>
public interface ISMSApplicationUserRepository : IBaseUserRepository<SMSApplicationUser>
{
    /// <summary>
    /// Gets users by application role
    /// </summary>
    Task<Result<IEnumerable<SMSApplicationUser>>> GetByApplicationRoleAsync(string applicationRole);

    /// <summary>
    /// Gets users by permission level
    /// </summary>
    Task<Result<IEnumerable<SMSApplicationUser>>> GetByPermissionLevelAsync(string permissionLevel);

    /// <summary>
    /// Gets users with permission level at or above the specified level
    /// </summary>
    Task<Result<IEnumerable<SMSApplicationUser>>> GetUsersWithMinimumPermissionAsync(string minimumPermissionLevel);

    /// <summary>
    /// Updates application-specific information
    /// </summary>
    Task<Result<bool>> UpdateApplicationInfoAsync(string userId, string applicationRole, string permissionLevel);
}