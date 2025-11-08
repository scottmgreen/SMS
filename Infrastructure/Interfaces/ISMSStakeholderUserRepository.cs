using SMS_Domain.Entities;
using SMS_Domain.Interfaces;

using SMS_Shared.Common;

namespace SMS_Infrastructure.Interfaces;

/// <summary>
/// Defines the contract for SMS Stakeholder User repository operations
/// </summary>
public interface ISMSStakeholderUserRepository : IBaseUserRepository<SMSStakeholderUser>
{
    /// <summary>
    /// Gets users by stakeholder type
    /// </summary>
    Task<Result<IEnumerable<SMSStakeholderUser>>> GetByStakeholderTypeAsync(string stakeholderType);

    /// <summary>
    /// Gets users by organization
    /// </summary>
    Task<Result<IEnumerable<SMSStakeholderUser>>> GetByOrganizationAsync(string organization);

    /// <summary>
    /// Gets users by access level
    /// </summary>
    Task<Result<IEnumerable<SMSStakeholderUser>>> GetByAccessLevelAsync(string accessLevel);

    /// <summary>
    /// Gets users with access level at or above the specified level
    /// </summary>
    Task<Result<IEnumerable<SMSStakeholderUser>>> GetUsersWithMinimumAccessAsync(string minimumAccessLevel);

    /// <summary>
    /// Updates stakeholder-specific information
    /// </summary>
    Task<Result<bool>> UpdateStakeholderInfoAsync(string userId, string stakeholderType, string organization, string accessLevel);

    /// <summary>
    /// Gets airline stakeholders
    /// </summary>
    Task<Result<IEnumerable<SMSStakeholderUser>>> GetAirlineStakeholdersAsync();

    /// <summary>
    /// Gets ground handler stakeholders
    /// </summary>
    Task<Result<IEnumerable<SMSStakeholderUser>>> GetGroundHandlerStakeholdersAsync();

    /// <summary>
    /// Gets contractor stakeholders
    /// </summary>
    Task<Result<IEnumerable<SMSStakeholderUser>>> GetContractorStakeholdersAsync();

    /// <summary>
    /// Gets users requiring AOA access
    /// </summary>
    Task<Result<IEnumerable<SMSStakeholderUser>>> GetUsersRequiringAOAAccessAsync();

    /// <summary>
    /// Gets stakeholder statistics by type
    /// </summary>
    Task<Result<Dictionary<string, int>>> GetStakeholderTypeStatisticsAsync();

    /// <summary>
    /// Gets stakeholder statistics by organization
    /// </summary>
    Task<Result<Dictionary<string, int>>> GetOrganizationStatisticsAsync();
}