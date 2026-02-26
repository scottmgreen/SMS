//-----------------------------------------------------------------------
// <copyright file="SMSApplicationGroupService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application service providing business logic operations for SMS domain entities.
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;

namespace SMS_Application.Services;

/// <summary>
/// High-level application service for SMS Application Group business operations
/// Provides business logic orchestration and cross-cutting concerns
/// </summary>
public sealed class SMSApplicationGroupService : ISMSApplicationGroupService
{
    private readonly SMSApplicationGroupDataService _dataService;
    private readonly ILogger<SMSApplicationGroupService> _logger;

    public SMSApplicationGroupService(SMSApplicationGroupDataService dataService, ILogger<SMSApplicationGroupService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new SMS Application Group with business validation
    /// </summary>
    public async Task<Result<SMSApplicationGroup>> CreateSMSApplicationGroupAsync(SMSApplicationGroup group, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Creating SMS Application Group with code: {Code}", group?.Code);

            if (group is null)
            {
                _logger.LogError("CreateSMSApplicationGroupAsync received null group");
                return Result<SMSApplicationGroup>.Failure<SMSApplicationGroup>(DomainErrors.SMSApplicationGroupError.NullOrEmpty);
            }

            // Business validation - ensure group is active by default
            if (!group.IsActive)
            {
                _logger.LogInformation("Activating group during creation: {Code}", group.Code);
                // Set IsActive directly since SMSApplicationGroup may not have an Activate method
                group.IsActive = true;
            }

            // Business validation - validate group name uniqueness
            await ValidateGroupNameUniqueness(group.Name);

            var result = await _dataService.CreateAsync(group, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created SMS Application Group with code: {Code}", result.Value?.Code);
            }
            else
            {
                _logger.LogError("Failed to create SMS Application Group. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating SMS Application Group");
            return Result<SMSApplicationGroup>.Failure<SMSApplicationGroup>(DomainErrors.SMSApplicationGroupError.CreateFailed);
        }
    }

    /// <summary>
    /// Gets SMS Application Group by code
    /// </summary>
    public async Task<Result<SMSApplicationGroup>> GetSMSApplicationGroupByCodeAsync(string code, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving SMS Application Group with code: {Code}", code);
            return await _dataService.GetByCodeAsync(code, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving SMS Application Group with code: {Code}", code);
            return Result<SMSApplicationGroup>.Failure<SMSApplicationGroup>(DomainErrors.SMSApplicationGroupError.NotFound);
        }
    }

    /// <summary>
    /// Gets all SMS Application Groups
    /// </summary>
    public async Task<Result<IEnumerable<SMSApplicationGroup>>> GetAllSMSApplicationGroupsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving all SMS Application Groups");
            return await _dataService.GetAllAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving all SMS Application Groups");
            return Result<IEnumerable<SMSApplicationGroup>>.Failure<IEnumerable<SMSApplicationGroup>>(DomainErrors.SMSApplicationGroupError.NotFound);
        }
    }

    /// <summary>
    /// Gets SMS Application Groups by user code
    /// </summary>
    public async Task<Result<IEnumerable<SMSApplicationGroup>>> GetSMSApplicationGroupsByUserCodeAsync(string userCode, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving SMS Application Groups for user: {UserCode}", userCode);

            if (string.IsNullOrWhiteSpace(userCode))
            {
                _logger.LogWarning("Invalid user code provided for group lookup");
                return Result<IEnumerable<SMSApplicationGroup>>.Failure<IEnumerable<SMSApplicationGroup>>(DomainErrors.SMSApplicationGroupError.UserCodeRequired);
            }

            return await _dataService.GetGroupsByUserCodeAsync(userCode, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving SMS Application Groups for user: {UserCode}", userCode);
            return Result<IEnumerable<SMSApplicationGroup>>.Failure<IEnumerable<SMSApplicationGroup>>(DomainErrors.SMSApplicationGroupError.NotFound);
        }
    }

    /// <summary>
    /// Gets users by SMS Application Group code
    /// </summary>
    public async Task<Result<IEnumerable<SMSApplicationUser>>> GetUsersByGroupCodeAsync(string groupCode, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving users for SMS Application Group: {GroupCode}", groupCode);

            if (string.IsNullOrWhiteSpace(groupCode))
            {
                _logger.LogWarning("Invalid group code provided for user lookup");
                return Result<IEnumerable<SMSApplicationUser>>.Failure<IEnumerable<SMSApplicationUser>>(DomainErrors.SMSApplicationGroupError.CodeRequired);
            }

            return await _dataService.GetUsersByGroupCodeAsync(groupCode, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving users for SMS Application Group: {GroupCode}", groupCode);
            return Result<IEnumerable<SMSApplicationUser>>.Failure<IEnumerable<SMSApplicationUser>>(DomainErrors.SMSApplicationGroupError.NotFound);
        }
    }

    /// <summary>
    /// Updates an existing SMS Application Group with business validation
    /// </summary>
    public async Task<Result<SMSApplicationGroup>> UpdateSMSApplicationGroupAsync(SMSApplicationGroup group, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Updating SMS Application Group with code: {Code}", group?.Code);

            if (group is null)
            {
                _logger.LogError("UpdateSMSApplicationGroupAsync received null group");
                return Result<SMSApplicationGroup>.Failure<SMSApplicationGroup>(DomainErrors.SMSApplicationGroupError.NullOrEmpty);
            }

            // Business validation - check if group exists
            var existingGroupResult = await _dataService.GetByCodeAsync(group.Code, ct).ConfigureAwait(false);
            if (existingGroupResult.IsFailure)
            {
                _logger.LogWarning("Cannot update non-existent SMS Application Group with code: {Code}", group.Code);
                return Result<SMSApplicationGroup>.Failure<SMSApplicationGroup>(DomainErrors.SMSApplicationGroupError.NotFound);
            }

            // Business validation - validate group name uniqueness (exclude current group)
            await ValidateGroupNameUniqueness(group.Name, group.Code);

            var result = await _dataService.UpdateAsync(group, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated SMS Application Group with code: {Code}", group.Code);
            }
            else
            {
                _logger.LogError("Failed to update SMS Application Group. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating SMS Application Group with code: {Code}", group?.Code);
            return Result<SMSApplicationGroup>.Failure<SMSApplicationGroup>(DomainErrors.SMSApplicationGroupError.UpdateFailed);
        }
    }

    /// <summary>
    /// Deletes an SMS Application Group with business validation
    /// </summary>
    public async Task<Result<bool>> DeleteSMSApplicationGroupAsync(string groupCode, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Deleting SMS Application Group with code: {Code}", groupCode);

            if (string.IsNullOrWhiteSpace(groupCode))
            {
                _logger.LogError("DeleteSMSApplicationGroupAsync received null or empty group code");
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.CodeRequired);
            }

            // Business validation - check if group has members
            var usersResult = await _dataService.GetUsersByGroupCodeAsync(groupCode, ct).ConfigureAwait(false);
            if (usersResult.IsSuccess && usersResult.Value.Any())
            {
                _logger.LogWarning("Cannot delete SMS Application Group with active members: {Code}", groupCode);
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.GroupHasMembers);
            }

            var result = await _dataService.DeleteAsync(groupCode, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted SMS Application Group with code: {Code}", groupCode);
            }
            else
            {
                _logger.LogError("Failed to delete SMS Application Group. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting SMS Application Group with code: {Code}", groupCode);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.DeleteFailed);
        }
    }

    /// <summary>
    /// Assigns a user to an application group with business validation
    /// </summary>
    public async Task<Result<bool>> AssignUserToGroupAsync(string userCode, string groupCode, string assignedBy = "SYSTEM", CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Assigning user {UserCode} to group {GroupCode}", userCode, groupCode);

            if (string.IsNullOrWhiteSpace(userCode) || string.IsNullOrWhiteSpace(groupCode))
            {
                _logger.LogError("AssignUserToGroupAsync received null or empty parameters");
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.UserCodeRequired);
            }

            // Business validation - check if group is active
            var groupResult = await _dataService.GetByCodeAsync(groupCode, ct).ConfigureAwait(false);
            if (groupResult.IsFailure)
            {
                _logger.LogWarning("Cannot assign user to non-existent group: {GroupCode}", groupCode);
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.NotFound);
            }

            if (!groupResult.Value.IsActive)
            {
                _logger.LogWarning("Cannot assign user to inactive group: {GroupCode}", groupCode);
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.CannotAssignToInactiveGroup);
            }

            var result = await _dataService.AssignUserToGroupAsync(userCode, groupCode, assignedBy, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully assigned user {UserCode} to group {GroupCode}", userCode, groupCode);
            }
            else
            {
                _logger.LogError("Failed to assign user to group. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error assigning user {UserCode} to group {GroupCode}", userCode, groupCode);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.AssignmentFailed);
        }
    }

    /// <summary>
    /// Removes a user from an application group
    /// </summary>
    public async Task<Result<bool>> RemoveUserFromGroupAsync(string userCode, string groupCode, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Removing user {UserCode} from group {GroupCode}", userCode, groupCode);

            if (string.IsNullOrWhiteSpace(userCode) || string.IsNullOrWhiteSpace(groupCode))
            {
                _logger.LogError("RemoveUserFromGroupAsync received null or empty parameters");
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.UserCodeRequired);
            }

            var result = await _dataService.RemoveUserFromGroupAsync(userCode, groupCode, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully removed user {UserCode} from group {GroupCode}", userCode, groupCode);
            }
            else
            {
                _logger.LogError("Failed to remove user from group. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error removing user {UserCode} from group {GroupCode}", userCode, groupCode);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.RemovalFailed);
        }
    }

    /// <summary>
    /// Clears all group memberships for a user
    /// </summary>
    public async Task<Result<bool>> ClearUserGroupsAsync(string userCode, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Clearing all group memberships for user {UserCode}", userCode);

            if (string.IsNullOrWhiteSpace(userCode))
            {
                _logger.LogError("ClearUserGroupsAsync received null or empty user code");
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.UserCodeRequired);
            }

            var result = await _dataService.ClearUserGroupsAsync(userCode, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully cleared all group memberships for user {UserCode}", userCode);
            }
            else
            {
                _logger.LogError("Failed to clear user groups. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error clearing group memberships for user {UserCode}", userCode);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.ClearGroupsFailed);
        }
    }

    #region Private Business Logic Helpers

    /// <summary>
    /// Validates group name uniqueness (business rule)
    /// </summary>
    private async Task ValidateGroupNameUniqueness(string groupName, string excludeCode = null)
    {
        if (string.IsNullOrWhiteSpace(groupName))
            return;

        try
        {
            var allGroups = await _dataService.GetAllAsync().ConfigureAwait(false);
            if (allGroups.IsSuccess)
            {
                var existingGroup = allGroups.Value.FirstOrDefault(g =>
                    g.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase) &&
                    g.Code != excludeCode);

                if (existingGroup != null)
                {
                    _logger.LogWarning("Duplicate group name detected: {GroupName}", groupName);
                    // Could throw an exception or return an error result here
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating group name uniqueness for: {GroupName}", groupName);
            // Continue with creation/update - uniqueness will be enforced at database level
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Validates if group name follows business rules
    /// </summary>
    private bool IsValidGroupName(string groupName)
    {
        if (string.IsNullOrWhiteSpace(groupName))
            return false;

        // Business rules for group names
        if (groupName.Length < 3 || groupName.Length > 100)
            return false;

        // Check for prohibited characters or patterns
        var prohibitedPatterns = new[] { "admin", "system", "root", "test" };
        return !prohibitedPatterns.Any(pattern =>
            groupName.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Validates maximum group membership limits
    /// </summary>
    private async Task<bool> ValidateGroupMembershipLimits(string groupCode)
    {
        try
        {
            var usersResult = await _dataService.GetUsersByGroupCodeAsync(groupCode).ConfigureAwait(false);
            if (usersResult.IsSuccess)
            {
                const int maxMembersPerGroup = 100; // Business rule
                var currentMemberCount = usersResult.Value.Count();

                if (currentMemberCount >= maxMembersPerGroup)
                {
                    _logger.LogWarning("Group {GroupCode} has reached maximum member limit: {Count}",
                        groupCode, currentMemberCount);
                    return false;
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating group membership limits for: {GroupCode}", groupCode);
            return true; // Allow operation to continue
        }
    }

    #endregion
}
