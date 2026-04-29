//-----------------------------------------------------------------------
// <copyright file="SMSStakeholderGroupService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application service providing business logic operations for SMS domain entities.
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Infrastructure.Services;
using SMS_Application.Interfaces;

namespace SMS_Application.Services;

/// <summary>
/// High-level application service for SMS Stakeholder Group business operations
/// Provides business logic orchestration and cross-cutting concerns
/// </summary>
public sealed class SMSStakeholderGroupService : ISMSStakeholderGroupService
{
    private readonly SMSStakeholderGroupDataService _dataService;
    private readonly ILogger<SMSStakeholderGroupService> _logger;

    public SMSStakeholderGroupService(SMSStakeholderGroupDataService dataService, ILogger<SMSStakeholderGroupService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new SMS Stakeholder Group with business validation
    /// </summary>
    public async Task<Result<SMSStakeholderGroup>> CreateSMSStakeholderGroupAsync(SMSStakeholderGroup group, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Creating SMS Stakeholder Group with code: {Code}", group?.Code);

            if (group is null)
            {
                _logger.LogError("CreateSMSStakeholderGroupAsync received null group");
                return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.SMSStakeholderGroupError.NullOrEmpty);
            }

            // Business validation - ensure group is active by default
            if (!group.IsActive)
            {
                _logger.LogInformation("Activating group during creation: {Code}", group.Code);
                group.IsActive = true;
            }

            var result = await _dataService.CreateAsync(group, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created SMS Stakeholder Group with code: {Code}", result.Value?.Code);
            }
            else
            {
                _logger.LogError("Failed to create SMS Stakeholder Group. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating SMS Stakeholder Group");
            return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.SMSStakeholderGroupError.CreateFailed);
        }
    }

    /// <summary>
    /// Gets SMS Stakeholder Group by code
    /// </summary>
    public async Task<Result<SMSStakeholderGroup>> GetSMSStakeholderGroupByCodeAsync(string groupCode, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving SMS Stakeholder Group with code: {Code}", groupCode);
            return await _dataService.GetByCodeAsync(groupCode, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving SMS Stakeholder Group with code: {Code}", groupCode);
            return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.SMSStakeholderGroupError.NotFound);
        }
    }

    /// <summary>
    /// Gets all SMS Stakeholder Groups
    /// </summary>
    public async Task<Result<IEnumerable<SMSStakeholderGroup>>> GetAllSMSStakeholderGroupsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving all SMS Stakeholder Groups");
            return await _dataService.GetAllAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving all SMS Stakeholder Groups");
            return Result<IEnumerable<SMSStakeholderGroup>>.Failure<IEnumerable<SMSStakeholderGroup>>(DomainErrors.SMSStakeholderGroupError.NotFound);
        }
    }

    /// <summary>
    /// Updates an existing SMS Stakeholder Group with business validation
    /// </summary>
    public async Task<Result<SMSStakeholderGroup>> UpdateSMSStakeholderGroupAsync(SMSStakeholderGroup group, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Updating SMS Stakeholder Group with code: {Code}", group?.Code);

            if (group is null)
            {
                _logger.LogError("UpdateSMSStakeholderGroupAsync received null group");
                return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.SMSStakeholderGroupError.NullOrEmpty);
            }

            // Business validation - check if group exists
            var existingGroupResult = await _dataService.GetByCodeAsync(group.Code, ct).ConfigureAwait(false);
            if (existingGroupResult.IsFailure)
            {
                _logger.LogWarning("Cannot update non-existent SMS Stakeholder Group with code: {Code}", group.Code);
                return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.SMSStakeholderGroupError.NotFound);
            }

            var result = await _dataService.UpdateAsync(group, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated SMS Stakeholder Group with code: {Code}", group.Code);
            }
            else
            {
                _logger.LogError("Failed to update SMS Stakeholder Group. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating SMS Stakeholder Group with code: {Code}", group?.Code);
            return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.SMSStakeholderGroupError.UpdateFailed);
        }
    }

    /// <summary>
    /// Deletes an SMS Stakeholder Group with business validation
    /// </summary>
    public async Task<Result<bool>> DeleteSMSStakeholderGroupAsync(string groupCode, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Deleting SMS Stakeholder Group with code: {Code}", groupCode);

            if (string.IsNullOrWhiteSpace(groupCode))
            {
                _logger.LogError("DeleteSMSStakeholderGroupAsync received null or empty group code");
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.CodeRequired);
            }

            var result = await _dataService.DeleteAsync(groupCode, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted SMS Stakeholder Group with code: {Code}", groupCode);
            }
            else
            {
                _logger.LogError("Failed to delete SMS Stakeholder Group. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting SMS Stakeholder Group with code: {Code}", groupCode);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.DeleteFailed);
        }
    }

    /// <summary>
    /// Gets SMS Stakeholder Groups by user code
    /// </summary>
    public async Task<Result<IEnumerable<SMSStakeholderGroup>>> GetSMSStakeholderGroupsByUserCodeAsync(string userCode, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving SMS Stakeholder Groups for user: {UserCode}", userCode);

            if (string.IsNullOrWhiteSpace(userCode))
            {
                _logger.LogWarning("Invalid user code provided for group lookup");
                return Result<IEnumerable<SMSStakeholderGroup>>.Failure<IEnumerable<SMSStakeholderGroup>>(DomainErrors.SMSStakeholderGroupError.UserCodeRequired);
            }

            return await _dataService.GetGroupsByUserCodeAsync(userCode, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving SMS Stakeholder Groups for user: {UserCode}", userCode);
            return Result<IEnumerable<SMSStakeholderGroup>>.Failure<IEnumerable<SMSStakeholderGroup>>(DomainErrors.SMSStakeholderGroupError.NotFound);
        }
    }

    /// <summary>
    /// Gets users by SMS Stakeholder Group code
    /// </summary>
    public async Task<Result<IEnumerable<SMSStakeholderUser>>> GetUsersByGroupCodeAsync(string groupCode, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving users for SMS Stakeholder Group: {GroupCode}", groupCode);

            if (string.IsNullOrWhiteSpace(groupCode))
            {
                _logger.LogWarning("Invalid group code provided for user lookup");
                return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderGroupError.CodeRequired);
            }

            return await _dataService.GetUsersByGroupCodeAsync(groupCode, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving users for SMS Stakeholder Group: {GroupCode}", groupCode);
            return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderGroupError.NotFound);
        }
    }

    /// <summary>
    /// Assigns a user to an SMS Stakeholder Group
    /// </summary>
    public async Task<Result<bool>> AssignUserToGroupAsync(
        string userCode,
        SMSStakeholderGroupID groupCode,
        string assignedBy = "SYSTEM",
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Assigning user {UserCode} to group {GroupCode}", userCode, groupCode);

            if (string.IsNullOrWhiteSpace(userCode))
            {
                _logger.LogWarning("Invalid user code provided for group assignment");
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.UserCodeRequired);
            }

            var result = await _dataService.AssignUserToGroupAsync(userCode, groupCode.Value, assignedBy, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully assigned user {UserCode} to group {GroupCode}", userCode, groupCode);
            }
            else
            {
                _logger.LogError("Failed to assign user {UserCode} to group {GroupCode}: {Error}",
                    userCode, groupCode, result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error assigning user {UserCode} to group {GroupCode}", userCode, groupCode);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.AssignmentFailed);
        }
    }

    /// <summary>
    /// Removes a user from an SMS Stakeholder Group
    /// </summary>
    public async Task<Result<bool>> RemoveUserFromGroupAsync(string userCode, SMSStakeholderGroupID groupCode, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Removing user {UserCode} from group {GroupCode}", userCode, groupCode);

            if (string.IsNullOrWhiteSpace(userCode))
            {
                _logger.LogWarning("Invalid user code provided for group removal");
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.UserCodeRequired);
            }

            var result = await _dataService.RemoveUserFromGroupAsync(userCode, groupCode.Value, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully removed user {UserCode} from group {GroupCode}", userCode, groupCode);
            }
            else
            {
                _logger.LogError("Failed to remove user {UserCode} from group {GroupCode}: {Error}",
                    userCode, groupCode, result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error removing user {UserCode} from group {GroupCode}", userCode, groupCode);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.RemovalFailed);
        }
    }

    /// <summary>
    /// Clears all group memberships for a user
    /// </summary>
    public async Task<Result<bool>> ClearUserGroupMembershipsAsync(string userCode, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Clearing all group memberships for user: {UserCode}", userCode);

            if (string.IsNullOrWhiteSpace(userCode))
            {
                _logger.LogWarning("Invalid user code provided for clearing group memberships");
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.UserCodeRequired);
            }

            var result = await _dataService.ClearUserGroupsAsync(userCode, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully cleared all group memberships for user: {UserCode}", userCode);
            }
            else
            {
                _logger.LogError("Failed to clear group memberships for user {UserCode}: {Error}",
                    userCode, result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error clearing group memberships for user: {UserCode}", userCode);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.ClearGroupsFailed);
        }
    }

    /// <summary>
    /// Updates a user's group memberships (clears existing and assigns new ones)
    /// </summary>
    public async Task<Result<bool>> UpdateUserGroupMembershipsAsync(
        string userCode,
        IEnumerable<SMSStakeholderGroupID> groupCodes,
        string assignedBy = "SYSTEM",
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Updating group memberships for user: {UserCode}", userCode);

            if (string.IsNullOrWhiteSpace(userCode))
            {
                _logger.LogWarning("Invalid user code provided for updating group memberships");
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.UserCodeRequired);
            }

            // Clear existing memberships first
            var clearResult = await ClearUserGroupMembershipsAsync(userCode, ct).ConfigureAwait(false);
            if (!clearResult.IsSuccess)
            {
                return clearResult;
            }

            // Assign new memberships
            foreach (var groupCode in groupCodes)
            {
                var assignResult = await AssignUserToGroupAsync(userCode, groupCode, assignedBy, ct).ConfigureAwait(false);
                if (!assignResult.IsSuccess)
                {
                    _logger.LogError("Failed to assign user {UserCode} to group {GroupCode} during batch update: {Error}",
                        userCode, groupCode, assignResult.Error?.Message);
                    // Continue with other assignments rather than failing completely
                }
            }

            _logger.LogInformation("Successfully updated group memberships for user: {UserCode}", userCode);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating group memberships for user: {UserCode}", userCode);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.UpdateFailed);
        }
    }
}
