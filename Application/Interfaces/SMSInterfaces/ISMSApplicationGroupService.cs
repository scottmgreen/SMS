//-----------------------------------------------------------------------
// <copyright file="ISMSApplicationGroupService.cs" company="SMS Safety Management System">
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
/// Service interface for SMS Application Group business operations
/// </summary>
public interface ISMSApplicationGroupService
{
    /// <summary>
    /// Creates a new SMS Application Group with business validation
    /// </summary>
    Task<Result<SMSApplicationGroup>> CreateSMSApplicationGroupAsync(SMSApplicationGroup group, CancellationToken ct = default);

    /// <summary>
    /// Gets SMS Application Group by code
    /// </summary>
    Task<Result<SMSApplicationGroup>> GetSMSApplicationGroupByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Gets all SMS Application Groups
    /// </summary>
    Task<Result<IEnumerable<SMSApplicationGroup>>> GetAllSMSApplicationGroupsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets SMS Application Groups by user code
    /// </summary>
    Task<Result<IEnumerable<SMSApplicationGroup>>> GetSMSApplicationGroupsByUserCodeAsync(string userCode, CancellationToken ct = default);

    /// <summary>
    /// Gets users by SMS Application Group code
    /// </summary>
    Task<Result<IEnumerable<SMSApplicationUser>>> GetUsersByGroupCodeAsync(string groupCode, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing SMS Application Group with business validation
    /// </summary>
    Task<Result<SMSApplicationGroup>> UpdateSMSApplicationGroupAsync(SMSApplicationGroup group, CancellationToken ct = default);

    /// <summary>
    /// Deletes an SMS Application Group with business validation
    /// </summary>
    Task<Result<bool>> DeleteSMSApplicationGroupAsync(string groupCode, CancellationToken ct = default);

    /// <summary>
    /// Assigns a user to an application group with business validation
    /// </summary>
    Task<Result<bool>> AssignUserToGroupAsync(string userCode, string groupCode, string assignedBy = "SYSTEM", CancellationToken ct = default);

    /// <summary>
    /// Removes a user from an application group
    /// </summary>
    Task<Result<bool>> RemoveUserFromGroupAsync(string userCode, string groupCode, CancellationToken ct = default);

    /// <summary>
    /// Clears all group memberships for a user
    /// </summary>
    Task<Result<bool>> ClearUserGroupsAsync(string userCode, CancellationToken ct = default);
}
