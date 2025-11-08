using SMS_Domain.Entities;
using SMS_Domain.Interfaces;
using SMS_Shared.Common;

namespace SMS_Infrastructure.Interfaces;

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
    /// Gets users in a department with specific roles
    /// </summary>
    //Task<Result<IEnumerable<SMSOrganizationalUser>>> GetDepartmentUsersWithRoleAsync(string department, string role);

    /// <summary>
    /// Updates organizational information
    /// </summary>
    Task<Result<bool>> UpdateOrganizationalInfoAsync(string userId, string department, string position, string organizationLevel);
}