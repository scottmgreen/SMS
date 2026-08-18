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
    private readonly ISMSStakeholderUserService _stakeholderUserService;
    private readonly ILogger<SMSStakeholderGroupService> _logger;

    public SMSStakeholderGroupService(
        SMSStakeholderGroupDataService dataService,
        ISMSStakeholderUserService stakeholderUserService,
        ILogger<SMSStakeholderGroupService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _stakeholderUserService = stakeholderUserService ?? throw new ArgumentNullException(nameof(stakeholderUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new SMS Stakeholder Group with business validation
    /// </summary>
    public async Task<Result<SMSStakeholderGroup>> CreateSMSStakeholderGroupAsync(SMSStakeholderGroup group, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Creating SMS Stakeholder Group with code: {Code}", group?.Code);

            if (group is null)
            {
                _logger.LogApplicationError("CreateSMSStakeholderGroupAsync received null group");
                return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.SMSStakeholderGroupError.NullOrEmpty);
            }

            // Business validation - ensure group is active by default
            if (!group.IsActive)
            {
                _logger.LogApplicationInformation("Activating group during creation: {Code}", group.Code);
                group.IsActive = true;
            }

            var result = await _dataService.CreateAsync(group, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                await _dataService.ReplaceAllowedCompaniesByGroupCodeAsync(
                    result.Value.Code,
                    group.AllowedCompanyCodes ?? new List<string>(),
                    group.CreatedBy ?? string.Empty,
                    ct).ConfigureAwait(false);

                await PopulateAllowedCompaniesAsync(result.Value, ct).ConfigureAwait(false);
                _logger.LogApplicationInformation("Successfully created SMS Stakeholder Group with code: {Code}", result.Value?.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to create SMS Stakeholder Group. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error creating SMS Stakeholder Group");
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
            _logger.LogApplicationInformation("Retrieving SMS Stakeholder Group with code: {Code}", groupCode);
            var result = await _dataService.GetByCodeAsync(groupCode, ct).ConfigureAwait(false);
            if (result.IsSuccess && result.Value is not null)
            {
                await PopulateAllowedCompaniesAsync(result.Value, ct).ConfigureAwait(false);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving SMS Stakeholder Group with code: {Code}", groupCode);
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
            _logger.LogApplicationInformation("Retrieving all SMS Stakeholder Groups");
            var result = await _dataService.GetAllAsync(ct).ConfigureAwait(false);
            if (result.IsSuccess && result.Value is not null)
            {
                foreach (var group in result.Value)
                {
                    await PopulateAllowedCompaniesAsync(group, ct).ConfigureAwait(false);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving all SMS Stakeholder Groups");
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
            _logger.LogApplicationInformation("Updating SMS Stakeholder Group with code: {Code}", group?.Code);

            if (group is null)
            {
                _logger.LogApplicationError("UpdateSMSStakeholderGroupAsync received null group");
                return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.SMSStakeholderGroupError.NullOrEmpty);
            }

            // Business validation - check if group exists
            var existingGroupResult = await _dataService.GetByCodeAsync(group.Code, ct).ConfigureAwait(false);
            if (existingGroupResult.IsFailure)
            {
                _logger.LogApplicationWarning("Cannot update non-existent SMS Stakeholder Group with code: {Code}", group.Code);
                return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.SMSStakeholderGroupError.NotFound);
            }

            var result = await _dataService.UpdateAsync(group, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                await _dataService.ReplaceAllowedCompaniesByGroupCodeAsync(
                    group.Code,
                    group.AllowedCompanyCodes ?? new List<string>(),
                    group.UpdatedBy ?? group.CreatedBy ?? string.Empty,
                    ct).ConfigureAwait(false);

                await PopulateAllowedCompaniesAsync(result.Value, ct).ConfigureAwait(false);
                _logger.LogApplicationInformation("Successfully updated SMS Stakeholder Group with code: {Code}", group.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to update SMS Stakeholder Group. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error updating SMS Stakeholder Group with code: {Code}", group?.Code);
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
            _logger.LogApplicationInformation("Deleting SMS Stakeholder Group with code: {Code}", groupCode);

            if (string.IsNullOrWhiteSpace(groupCode))
            {
                _logger.LogApplicationError("DeleteSMSStakeholderGroupAsync received null or empty group code");
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.CodeRequired);
            }

            var result = await _dataService.DeleteAsync(groupCode, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully deleted SMS Stakeholder Group with code: {Code}", groupCode);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete SMS Stakeholder Group. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error deleting SMS Stakeholder Group with code: {Code}", groupCode);
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
            _logger.LogApplicationInformation("Retrieving SMS Stakeholder Groups for user: {UserCode}", userCode);

            if (string.IsNullOrWhiteSpace(userCode))
            {
                _logger.LogApplicationWarning("Invalid user code provided for group lookup");
                return Result<IEnumerable<SMSStakeholderGroup>>.Failure<IEnumerable<SMSStakeholderGroup>>(DomainErrors.SMSStakeholderGroupError.UserCodeRequired);
            }

            return await _dataService.GetGroupsByUserCodeAsync(userCode, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving SMS Stakeholder Groups for user: {UserCode}", userCode);
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
            _logger.LogApplicationInformation("Retrieving users for SMS Stakeholder Group: {GroupCode}", groupCode);

            if (string.IsNullOrWhiteSpace(groupCode))
            {
                _logger.LogApplicationWarning("Invalid group code provided for user lookup");
                return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderGroupError.CodeRequired);
            }

            return await _dataService.GetUsersByGroupCodeAsync(groupCode, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving users for SMS Stakeholder Group: {GroupCode}", groupCode);
            return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderGroupError.NotFound);
        }
    }

    /// <summary>
    /// Assigns a user to an SMS Stakeholder Group
    /// </summary>
    public async Task<Result<bool>> AssignUserToGroupAsync(
        string userCode,
        SMSStakeholderGroupID groupCode,
        string assignedBy,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Assigning user {UserCode} to group {GroupCode}", userCode, groupCode);

            if (string.IsNullOrWhiteSpace(userCode))
            {
                _logger.LogApplicationWarning("Invalid user code provided for group assignment");
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.UserCodeRequired);
            }

            var groupResult = await _dataService.GetByCodeAsync(groupCode.Value, ct).ConfigureAwait(false);
            if (groupResult.IsFailure || groupResult.Value is null)
            {
                _logger.LogApplicationWarning("Cannot assign user to non-existent stakeholder group: {GroupCode}", groupCode);
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.NotFound);
            }

            if (!groupResult.Value.IsActive)
            {
                _logger.LogApplicationWarning("Cannot assign user to inactive stakeholder group: {GroupCode}", groupCode);
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.CannotAssignToInactiveGroup);
            }

            var userResult = await _stakeholderUserService.GetSMSStakeholderUserByCodeAsync(userCode, ct).ConfigureAwait(false);
            if (userResult.IsFailure || userResult.Value is null)
            {
                _logger.LogApplicationWarning("Cannot assign non-existent stakeholder user {UserCode} to group {GroupCode}", userCode, groupCode);
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.UserNotFound);
            }

            var allowedCompaniesResult = await _dataService.GetAllowedCompaniesByGroupCodeAsync(groupCode.Value, ct).ConfigureAwait(false);
            if (allowedCompaniesResult.IsFailure)
            {
                _logger.LogApplicationWarning("Failed loading allowed companies for group {GroupCode}; rejecting assignment", groupCode);
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.AssignmentFailed);
            }

            var allowedCompanies = allowedCompaniesResult.Value?.ToList() ?? new List<string>();
            if (allowedCompanies.Count > 0 && !string.IsNullOrWhiteSpace(userResult.Value.Company))
            {
                var isAllowed = allowedCompanies.Any(c => c.Equals(userResult.Value.Company, StringComparison.OrdinalIgnoreCase));
                if (!isAllowed)
                {
                    _logger.LogApplicationWarning("User {UserCode} company {Company} is not allowed for group {GroupCode}", userCode, userResult.Value.Company, groupCode);
                    return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.CompanyNotAllowed);
                }
            }
            else if (allowedCompanies.Count > 0 && string.IsNullOrWhiteSpace(userResult.Value.Company))
            {
                _logger.LogApplicationWarning("User {UserCode} has no company and group {GroupCode} has explicit allowed companies", userCode, groupCode);
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.CompanyNotAllowed);
            }

            var result = await _dataService.AssignUserToGroupAsync(userCode, groupCode.Value, assignedBy, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully assigned user {UserCode} to group {GroupCode}", userCode, groupCode);
            }
            else
            {
                _logger.LogApplicationError("Failed to assign user {UserCode} to group {GroupCode}: {Error}",
                    userCode, groupCode, result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error assigning user {UserCode} to group {GroupCode}", userCode, groupCode);
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
            _logger.LogApplicationInformation("Removing user {UserCode} from group {GroupCode}", userCode, groupCode);

            if (string.IsNullOrWhiteSpace(userCode))
            {
                _logger.LogApplicationWarning("Invalid user code provided for group removal");
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.UserCodeRequired);
            }

            var result = await _dataService.RemoveUserFromGroupAsync(userCode, groupCode.Value, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully removed user {UserCode} from group {GroupCode}", userCode, groupCode);
            }
            else
            {
                _logger.LogApplicationError("Failed to remove user {UserCode} from group {GroupCode}: {Error}",
                    userCode, groupCode, result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error removing user {UserCode} from group {GroupCode}", userCode, groupCode);
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
            _logger.LogApplicationInformation("Clearing all group memberships for user: {UserCode}", userCode);

            if (string.IsNullOrWhiteSpace(userCode))
            {
                _logger.LogApplicationWarning("Invalid user code provided for clearing group memberships");
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.UserCodeRequired);
            }

            var result = await _dataService.ClearUserGroupsAsync(userCode, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully cleared all group memberships for user: {UserCode}", userCode);
            }
            else
            {
                _logger.LogApplicationError("Failed to clear group memberships for user {UserCode}: {Error}",
                    userCode, result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error clearing group memberships for user: {UserCode}", userCode);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.ClearGroupsFailed);
        }
    }

    /// <summary>
    /// Updates a user's group memberships (clears existing and assigns new ones)
    /// </summary>
    public async Task<Result<bool>> UpdateUserGroupMembershipsAsync(
        string userCode,
        IEnumerable<SMSStakeholderGroupID> groupCodes,
        string assignedBy,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Updating group memberships for user: {UserCode}", userCode);

            if (string.IsNullOrWhiteSpace(userCode))
            {
                _logger.LogApplicationWarning("Invalid user code provided for updating group memberships");
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
                    _logger.LogApplicationError("Failed to assign user {UserCode} to group {GroupCode} during batch update: {Error}",
                        userCode, groupCode, assignResult.Error?.Message);
                    // Continue with other assignments rather than failing completely
                }
            }

            _logger.LogApplicationInformation("Successfully updated group memberships for user: {UserCode}", userCode);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error updating group memberships for user: {UserCode}", userCode);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.UpdateFailed);
        }
    }

    private async Task PopulateAllowedCompaniesAsync(SMSStakeholderGroup group, CancellationToken ct)
    {
        var companiesResult = await _dataService.GetAllowedCompaniesByGroupCodeAsync(group.Code, ct).ConfigureAwait(false);
        if (companiesResult.IsSuccess)
        {
            group.AllowedCompanyCodes = companiesResult.Value?
                .Where(code => !string.IsNullOrWhiteSpace(code))
                .Select(code => code.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new List<string>();
        }
    }
}

