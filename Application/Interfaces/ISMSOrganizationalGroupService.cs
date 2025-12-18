using SMS_Domain.Entities;
using SMS_Shared.Common;

namespace SMS_Application.Interfaces;

/// <summary>
/// Service interface for SMS Organizational Group business operations
/// </summary>
public interface ISMSOrganizationalGroupService
{
    /// <summary>
    /// Creates a new SMS Organizational Group with business validation
    /// </summary>
    Task<Result<SMSOrganizationalGroup>> CreateSMSOrganizationalGroupAsync(SMSOrganizationalGroup group, CancellationToken ct = default);

    /// <summary>
    /// Gets SMS Organizational Group by code
    /// </summary>
    Task<Result<SMSOrganizationalGroup>> GetSMSOrganizationalGroupByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Gets all SMS Organizational Groups
    /// </summary>
    Task<Result<IEnumerable<SMSOrganizationalGroup>>> GetAllSMSOrganizationalGroupsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets SMS Organizational Groups by user code
    /// </summary>
    Task<Result<IEnumerable<SMSOrganizationalGroup>>> GetSMSOrganizationalGroupsByUserCodeAsync(string userCode, CancellationToken ct = default);

    /// <summary>
    /// Gets users by SMS Organizational Group code
    /// </summary>
    Task<Result<IEnumerable<SMSOrganizationalUser>>> GetUsersByGroupCodeAsync(string groupCode, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing SMS Organizational Group with business validation
    /// </summary>
    Task<Result<SMSOrganizationalGroup>> UpdateSMSOrganizationalGroupAsync(SMSOrganizationalGroup group, CancellationToken ct = default);

    /// <summary>
    /// Deletes an SMS Organizational Group with business validation
    /// </summary>
    Task<Result<bool>> DeleteSMSOrganizationalGroupAsync(string groupCode, CancellationToken ct = default);

    /// <summary>
    /// Assigns a user to an organizational group with business validation
    /// </summary>
    Task<Result<bool>> AssignUserToGroupAsync(string userCode, string groupCode, string assignedBy = "SYSTEM", CancellationToken ct = default);

    /// <summary>
    /// Removes a user from an organizational group
    /// </summary>
    Task<Result<bool>> RemoveUserFromGroupAsync(string userCode, string groupCode, CancellationToken ct = default);

    /// <summary>
    /// Clears all group memberships for a user
    /// </summary>
    Task<Result<bool>> ClearUserGroupsAsync(string userCode, CancellationToken ct = default);
}