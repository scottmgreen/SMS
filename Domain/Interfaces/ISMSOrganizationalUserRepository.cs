using SMS_Domain.Entities;
using SMS_Shared.Common;

namespace SMS_Domain.Interfaces;

/// <summary>
/// Defines the contract for SMS Organizational User repository operations
/// </summary>
public interface ISMSOrganizationalUserRepository : IBaseUserRepository<SMSOrganizationalUser>
{
    /// <summary>
    /// Gets users by department
    /// </summary>
    Task<Result<IEnumerable<SMSOrganizationalUser>>> GetByDepartmentAsync(string department);

    /// <summary>
    /// Gets users by position
    /// </summary>
    Task<Result<IEnumerable<SMSOrganizationalUser>>> GetByPositionAsync(string position);

    /// <summary>
    /// Gets users by organization level
    /// </summary>
    Task<Result<IEnumerable<SMSOrganizationalUser>>> GetByOrganizationLevelAsync(string organizationLevel);

    /// <summary>
    /// Gets users at or above a specific organization level
    /// </summary>
    Task<Result<IEnumerable<SMSOrganizationalUser>>> GetUsersAtOrAboveLevelAsync(string minimumLevel);

    /// <summary>
    /// Updates organizational-specific information
    /// </summary>
    Task<Result<bool>> UpdateOrganizationalInfoAsync(string userId, string department, string position, string organizationLevel);

    /// <summary>
    /// Gets supervisors for a department
    /// </summary>
    Task<Result<IEnumerable<SMSOrganizationalUser>>> GetDepartmentSupervisorsAsync(string department);

    /// <summary>
    /// Gets department statistics
    /// </summary>
    Task<Result<Dictionary<string, int>>> GetDepartmentStatisticsAsync();
}