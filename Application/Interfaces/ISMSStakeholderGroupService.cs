//-----------------------------------------------------------------------
// <copyright file="ISMSStakeholderGroupService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application service providing business logic operations for SMS domain entities.
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Application.Interfaces;

/// <summary>
/// Service interface for SMS Stakeholder Group business operations
/// </summary>
public interface ISMSStakeholderGroupService
{
    /// <summary>
    /// Creates a new SMS Stakeholder Group with business validation
    /// </summary>
    Task<Result<SMSStakeholderGroup>> CreateSMSStakeholderGroupAsync(SMSStakeholderGroup group, CancellationToken ct = default);

    /// <summary>
    /// Gets SMS Stakeholder Group by code
    /// </summary>
    Task<Result<SMSStakeholderGroup>> GetSMSStakeholderGroupByCodeAsync(string groupCode, CancellationToken ct = default);

    /// <summary>
    /// Gets all SMS Stakeholder Groups
    /// </summary>
    Task<Result<IEnumerable<SMSStakeholderGroup>>> GetAllSMSStakeholderGroupsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets SMS Stakeholder Groups by user code
    /// </summary>
    Task<Result<IEnumerable<SMSStakeholderGroup>>> GetSMSStakeholderGroupsByUserCodeAsync(string userCode, CancellationToken ct = default);

    /// <summary>
    /// Gets users by SMS Stakeholder Group code
    /// </summary>
    Task<Result<IEnumerable<SMSStakeholderUser>>> GetUsersByGroupCodeAsync(string groupCode, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing SMS Stakeholder Group with business validation
    /// </summary>
    Task<Result<SMSStakeholderGroup>> UpdateSMSStakeholderGroupAsync(SMSStakeholderGroup group, CancellationToken ct = default);

    /// <summary>
    /// Deletes an SMS Stakeholder Group with business validation
    /// </summary>
    Task<Result<bool>> DeleteSMSStakeholderGroupAsync(string groupCode, CancellationToken ct = default);

    /// <summary>
    /// Assigns a user to a stakeholder group with business validation
    /// </summary>
    Task<Result<bool>> AssignUserToGroupAsync(string userCode, SMSStakeholderGroupID groupCode, string assignedBy = "SYSTEM", CancellationToken ct = default);

    /// <summary>
    /// Removes a user from a stakeholder group
    /// </summary>
    Task<Result<bool>> RemoveUserFromGroupAsync(string userCode, SMSStakeholderGroupID groupCode, CancellationToken ct = default);

    /// <summary>
    /// Clears all group memberships for a user
    /// </summary>
    Task<Result<bool>> ClearUserGroupMembershipsAsync(string userCode, CancellationToken ct = default);

    /// <summary>
    /// Updates a user's group memberships (clears existing and assigns new ones)
    /// </summary>
    Task<Result<bool>> UpdateUserGroupMembershipsAsync(string userCode, IEnumerable<SMSStakeholderGroupID> groupCodes, string assignedBy = "SYSTEM", CancellationToken ct = default);
}