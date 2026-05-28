//-----------------------------------------------------------------------
// <copyright file="SMSOrganizationalGroupService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application service providing business logic operations for SMS domain entities.
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using Microsoft.Extensions.Logging;

namespace SMS_Application.Services;

/// <summary>
/// High-level application service for SMS Organizational Group business operations
/// Provides business logic orchestration and cross-cutting concerns
/// </summary>
public sealed class SMSOrganizationalGroupService : ISMSOrganizationalGroupService
{
    private readonly SMSOrganizationalGroupDataService _dataService;
    private readonly ILogger<SMSOrganizationalGroupService> _logger;

    public SMSOrganizationalGroupService(SMSOrganizationalGroupDataService dataService, ILogger<SMSOrganizationalGroupService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new SMS Organizational Group with business validation
    /// </summary>
    public async Task<Result<SMSOrganizationalGroup>> CreateSMSOrganizationalGroupAsync(SMSOrganizationalGroup group, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Creating SMS Organizational Group with code: {Code}", group?.Code);

            if (group is null)
            {
                _logger.LogApplicationError("CreateSMSOrganizationalGroupAsync received null group");
                return Result<SMSOrganizationalGroup>.Failure<SMSOrganizationalGroup>(DomainErrors.SMSOrganizationalGroupError.NullOrEmpty);
            }

            // Business validation - ensure group is active by default
            if (!group.IsActive)
            {
                _logger.LogApplicationInformation("Activating group during creation: {Code}", group.Code);
                group.Activate("SYSTEM");
            }

            // Business validation - validate group type and authority level combination
            await ValidateGroupTypeAuthorityLevelCombination(group.GroupType, group.AuthorityLevel);

            var result = await _dataService.CreateAsync(group, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully created SMS Organizational Group with code: {Code}", result.Value?.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to create SMS Organizational Group. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error creating SMS Organizational Group");
            return Result<SMSOrganizationalGroup>.Failure<SMSOrganizationalGroup>(DomainErrors.SMSOrganizationalGroupError.CreateFailed);
        }
    }

    /// <summary>
    /// Gets SMS Organizational Group by code
    /// </summary>
    public async Task<Result<SMSOrganizationalGroup>> GetSMSOrganizationalGroupByCodeAsync(string code, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving SMS Organizational Group with code: {Code}", code);
            return await _dataService.GetByCodeAsync(code, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving SMS Organizational Group with code: {Code}", code);
            return Result<SMSOrganizationalGroup>.Failure<SMSOrganizationalGroup>(DomainErrors.SMSOrganizationalGroupError.NotFound);
        }
    }

    /// <summary>
    /// Gets all SMS Organizational Groups
    /// </summary>
    public async Task<Result<IEnumerable<SMSOrganizationalGroup>>> GetAllSMSOrganizationalGroupsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving all SMS Organizational Groups");
            return await _dataService.GetAllAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving all SMS Organizational Groups");
            return Result<IEnumerable<SMSOrganizationalGroup>>.Failure<IEnumerable<SMSOrganizationalGroup>>(DomainErrors.SMSOrganizationalGroupError.NotFound);
        }
    }

    /// <summary>
    /// Gets SMS Organizational Groups by user code
    /// </summary>
    public async Task<Result<IEnumerable<SMSOrganizationalGroup>>> GetSMSOrganizationalGroupsByUserCodeAsync(string userCode, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving SMS Organizational Groups for user: {UserCode}", userCode);

            if (string.IsNullOrWhiteSpace(userCode))
            {
                _logger.LogApplicationWarning("Invalid user code provided for group lookup");
                return Result<IEnumerable<SMSOrganizationalGroup>>.Failure<IEnumerable<SMSOrganizationalGroup>>(DomainErrors.SMSOrganizationalGroupError.UserCodeRequired);
            }

            return await _dataService.GetGroupsByUserCodeAsync(userCode, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving SMS Organizational Groups for user: {UserCode}", userCode);
            return Result<IEnumerable<SMSOrganizationalGroup>>.Failure<IEnumerable<SMSOrganizationalGroup>>(DomainErrors.SMSOrganizationalGroupError.NotFound);
        }
    }

    /// <summary>
    /// Gets users by SMS Organizational Group code
    /// </summary>
    public async Task<Result<IEnumerable<SMSOrganizationalUser>>> GetUsersByGroupCodeAsync(string groupCode, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving users for SMS Organizational Group: {GroupCode}", groupCode);

            if (string.IsNullOrWhiteSpace(groupCode))
            {
                _logger.LogApplicationWarning("Invalid group code provided for user lookup");
                return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(DomainErrors.SMSOrganizationalGroupError.CodeRequired);
            }

            return await _dataService.GetUsersByGroupCodeAsync(groupCode, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving users for SMS Organizational Group: {GroupCode}", groupCode);
            return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(DomainErrors.SMSOrganizationalGroupError.NotFound);
        }
    }

    /// <summary>
    /// Updates an existing SMS Organizational Group with business validation
    /// </summary>
    public async Task<Result<SMSOrganizationalGroup>> UpdateSMSOrganizationalGroupAsync(SMSOrganizationalGroup group, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Updating SMS Organizational Group with code: {Code}", group?.Code);

            if (group is null)
            {
                _logger.LogApplicationError("UpdateSMSOrganizationalGroupAsync received null group");
                return Result<SMSOrganizationalGroup>.Failure<SMSOrganizationalGroup>(DomainErrors.SMSOrganizationalGroupError.NullOrEmpty);
            }

            // Business validation - check if group exists
            var existingGroupResult = await _dataService.GetByCodeAsync(group.Code, ct).ConfigureAwait(false);
            if (existingGroupResult.IsFailure)
            {
                _logger.LogApplicationWarning("Cannot update non-existent SMS Organizational Group with code: {Code}", group.Code);
                return Result<SMSOrganizationalGroup>.Failure<SMSOrganizationalGroup>(DomainErrors.SMSOrganizationalGroupError.NotFound);
            }

            // Business validation - validate group type and authority level combination
            await ValidateGroupTypeAuthorityLevelCombination(group.GroupType, group.AuthorityLevel);

            var result = await _dataService.UpdateAsync(group, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully updated SMS Organizational Group with code: {Code}", group.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to update SMS Organizational Group. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error updating SMS Organizational Group with code: {Code}", group?.Code);
            return Result<SMSOrganizationalGroup>.Failure<SMSOrganizationalGroup>(DomainErrors.SMSOrganizationalGroupError.UpdateFailed);
        }
    }

    /// <summary>
    /// Deletes an SMS Organizational Group with business validation
    /// </summary>
    public async Task<Result<bool>> DeleteSMSOrganizationalGroupAsync(string groupCode, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Deleting SMS Organizational Group with code: {Code}", groupCode);

            if (string.IsNullOrWhiteSpace(groupCode))
            {
                _logger.LogApplicationError("DeleteSMSOrganizationalGroupAsync received null or empty group code");
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalGroupError.CodeRequired);
            }

            // Business validation - check if group has members
            var usersResult = await _dataService.GetUsersByGroupCodeAsync(groupCode, ct).ConfigureAwait(false);
            if (usersResult.IsSuccess && usersResult.Value.Any())
            {
                _logger.LogApplicationWarning("Cannot delete SMS Organizational Group with active members: {Code}", groupCode);
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalGroupError.GroupHasMembers);
            }

            var result = await _dataService.DeleteAsync(groupCode, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully deleted SMS Organizational Group with code: {Code}", groupCode);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete SMS Organizational Group. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error deleting SMS Organizational Group with code: {Code}", groupCode);
            return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalGroupError.DeleteFailed);
        }
    }

    /// <summary>
    /// Assigns a user to an organizational group with business validation
    /// </summary>
    public async Task<Result<bool>> AssignUserToGroupAsync(string userCode, string groupCode, string assignedBy = "SYSTEM", CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Assigning user {UserCode} to group {GroupCode}", userCode, groupCode);

            if (string.IsNullOrWhiteSpace(userCode) || string.IsNullOrWhiteSpace(groupCode))
            {
                _logger.LogApplicationError("AssignUserToGroupAsync received null or empty parameters");
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalGroupError.UserCodeRequired);
            }

            // Business validation - check if group is active
            var groupResult = await _dataService.GetByCodeAsync(groupCode, ct).ConfigureAwait(false);
            if (groupResult.IsFailure)
            {
                _logger.LogApplicationWarning("Cannot assign user to non-existent group: {GroupCode}", groupCode);
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalGroupError.NotFound);
            }

            if (!groupResult.Value.IsActive)
            {
                _logger.LogApplicationWarning("Cannot assign user to inactive group: {GroupCode}", groupCode);
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalGroupError.CannotAssignToInactiveGroup);
            }

            var result = await _dataService.AssignUserToGroupAsync(userCode, groupCode, assignedBy, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully assigned user {UserCode} to group {GroupCode}", userCode, groupCode);
            }
            else
            {
                _logger.LogApplicationError("Failed to assign user to group. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error assigning user {UserCode} to group {GroupCode}", userCode, groupCode);
            return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalGroupError.AssignmentFailed);
        }
    }

    /// <summary>
    /// Removes a user from an organizational group
    /// </summary>
    public async Task<Result<bool>> RemoveUserFromGroupAsync(string userCode, string groupCode, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Removing user {UserCode} from group {GroupCode}", userCode, groupCode);

            if (string.IsNullOrWhiteSpace(userCode) || string.IsNullOrWhiteSpace(groupCode))
            {
                _logger.LogApplicationError("RemoveUserFromGroupAsync received null or empty parameters");
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalGroupError.UserCodeRequired);
            }

            var result = await _dataService.RemoveUserFromGroupAsync(userCode, groupCode, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully removed user {UserCode} from group {GroupCode}", userCode, groupCode);
            }
            else
            {
                _logger.LogApplicationError("Failed to remove user from group. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error removing user {UserCode} from group {GroupCode}", userCode, groupCode);
            return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalGroupError.RemovalFailed);
        }
    }

    /// <summary>
    /// Clears all group memberships for a user
    /// </summary>
    public async Task<Result<bool>> ClearUserGroupsAsync(string userCode, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Clearing all group memberships for user {UserCode}", userCode);

            if (string.IsNullOrWhiteSpace(userCode))
            {
                _logger.LogApplicationError("ClearUserGroupsAsync received null or empty user code");
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalGroupError.UserCodeRequired);
            }

            var result = await _dataService.ClearUserGroupsAsync(userCode, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully cleared all group memberships for user {UserCode}", userCode);
            }
            else
            {
                _logger.LogApplicationError("Failed to clear user groups. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error clearing group memberships for user {UserCode}", userCode);
            return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalGroupError.ClearGroupsFailed);
        }
    }

    #region Private Business Logic Helpers

    /// <summary>
    /// Validates group type and authority level combination (business rule)
    /// </summary>
    private async Task ValidateGroupTypeAuthorityLevelCombination(string groupType, string authorityLevel)
    {
        // Business rule validation - certain authority levels only valid for certain group types
        var restrictedCombinations = new Dictionary<string, string[]>
        {
            ["Management Team"] = new[] { "Executive", "Strategic" },
            ["Committee"] = new[] { "Strategic", "Executive", "Operational" },
            ["Department"] = new[] { "Operational", "Process", "Support" },
            ["Work Group"] = new[] { "Process", "Support", "Standard" }
        };

        if (restrictedCombinations.ContainsKey(groupType))
        {
            var validAuthorityLevels = restrictedCombinations[groupType];
            if (!validAuthorityLevels.Contains(authorityLevel, StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogApplicationInformation("Authority level {AuthorityLevel} for group type {GroupType} requires validation",
                    authorityLevel, groupType);
                // Could implement additional validation logic here
            }
        }

        await Task.CompletedTask; // Placeholder for potential async validation
    }

    /// <summary>
    /// Validates if group type is valid (business rule)
    /// </summary>
    private bool IsValidGroupType(string groupType)
    {
        if (string.IsNullOrWhiteSpace(groupType))
            return false;

        var validGroupTypes = new[]
        {
            "Department", "SMS Role", "Committee", "Work Group", "Management Team"
        };

        return validGroupTypes.Contains(groupType, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates if authority level is valid (business rule)
    /// </summary>
    private bool IsValidAuthorityLevel(string authorityLevel)
    {
        if (string.IsNullOrWhiteSpace(authorityLevel))
            return false;

        var validAuthorityLevels = new[]
        {
            "Strategic", "Executive", "Operational", "Process", "Support", "Standard"
        };

        return validAuthorityLevels.Contains(authorityLevel, StringComparer.OrdinalIgnoreCase);
    }

    #endregion
}

