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
    /// Gets a user by their unique SMS Application User ID (type-safe overload)
    /// </summary>
    Task<Result<SMSApplicationUser>> GetByIdAsync(SMSApplicationUserID id);

    /// <summary>
    /// Gets users by application role
    /// </summary>
    Task<Result<IEnumerable<SMSApplicationUser>>> GetBySMSApplicationUserRoleAsync(string applicationRole);

    /// <summary>
    /// Gets users by permission level
    /// </summary>
    Task<Result<IEnumerable<SMSApplicationUser>>> GetSMSApplicationUserByPermissionLevelAsync(string permissionLevel);

    /// <summary>
    /// Gets users with permission level at or above the specified level
    /// </summary>
    Task<Result<IEnumerable<SMSApplicationUser>>> GetSMSApplicationUsersWithMinimumPermissionAsync(string minimumPermissionLevel);

    /// <summary>
    /// Updates application-specific information
    /// </summary>
    Task<Result<bool>> UpdateSMSApplicationUserInfoAsync(SMSApplicationUserID userId, string applicationRole, string permissionLevel);
}