//-----------------------------------------------------------------------
// <copyright file="SMSStakeholderGroupDataService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Data service coordinating smsstakeholdergroup repository operations with transaction management and business validation.
//                  Infrastructure service providing external system integration
//                  and technical functionality support.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using SMS_Domain.Errors;

namespace SMS_Infrastructure.Services;

/// <summary>
/// Data service for SMS Stakeholder Group operations
/// Provides high-level data access abstraction over repository layer
/// </summary>
public sealed class SMSStakeholderGroupDataService : BaseDataService<SMSStakeholderGroupDataService>
{
    private readonly SMSStakeholderGroupRepository _repository;
    private readonly ILogger<SMSStakeholderGroupDataService> _logger;

    public SMSStakeholderGroupDataService(
        ILogger<SMSStakeholderGroupDataService> logger,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        SMSStakeholderGroupRepository repository)
        : base(logger, serviceScopeFactory, configuration)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new SMS Stakeholder Group
    /// </summary>
    public async Task<Result<SMSStakeholderGroup>> CreateAsync(SMSStakeholderGroup group, CancellationToken ct = default)
    {
        try
        {
            if (group is null)
            {
                _logger.LogError("CreateSMSStakeholderGroupAsync received null group");
                return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.SMSStakeholderGroupError.NullOrEmpty);
            }

            _logger.LogInformation("Creating SMS Stakeholder Group with code: {Code}", group.Code);

            var result = await _repository.CreateAsync(group, ct);

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
    public async Task<Result<SMSStakeholderGroup>> GetByCodeAsync(string groupCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupCode))
            {
                _logger.LogError("GetSMSStakeholderGroupByCodeAsync received null or empty group code");
                return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.SMSStakeholderGroupError.CodeRequired);
            }

            _logger.LogInformation("Retrieving SMS Stakeholder Group with code: {Code}", groupCode);
            return await _repository.GetByCodeAsync(groupCode, ct);
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
    public async Task<Result<IEnumerable<SMSStakeholderGroup>>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving all SMS Stakeholder Groups");
            return await _repository.GetAllAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving all SMS Stakeholder Groups");
            return Result<IEnumerable<SMSStakeholderGroup>>.Failure<IEnumerable<SMSStakeholderGroup>>(DomainErrors.SMSStakeholderGroupError.NotFound);
        }
    }

    /// <summary>
    /// Gets SMS Stakeholder Groups by user code
    /// </summary>
    public async Task<Result<IEnumerable<SMSStakeholderGroup>>> GetGroupsByUserCodeAsync(string userCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode))
            {
                _logger.LogError("GetSMSStakeholderGroupsByUserCodeAsync received null or empty user code");
                return Result<IEnumerable<SMSStakeholderGroup>>.Failure<IEnumerable<SMSStakeholderGroup>>(DomainErrors.SMSStakeholderGroupError.UserCodeRequired);
            }

            _logger.LogInformation("Retrieving SMS Stakeholder Groups by user code: {UserCode}", userCode);
            return await _repository.GetGroupsByUserCodeAsync(userCode, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving SMS Stakeholder Groups by user code: {UserCode}", userCode);
            return Result<IEnumerable<SMSStakeholderGroup>>.Failure<IEnumerable<SMSStakeholderGroup>>(DomainErrors.SMSStakeholderGroupError.NotFound);
        }
    }

    /// <summary>
    /// Updates an existing SMS Stakeholder Group
    /// </summary>
    public async Task<Result<SMSStakeholderGroup>> UpdateAsync(SMSStakeholderGroup group, CancellationToken ct = default)
    {
        try
        {
            if (group is null)
            {
                _logger.LogError("UpdateSMSStakeholderGroupAsync received null group");
                return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.SMSStakeholderGroupError.NullOrEmpty);
            }

            _logger.LogInformation("Updating SMS Stakeholder Group with code: {Code}", group.Code);

            var result = await _repository.UpdateAsync(group, ct);

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
    /// Deletes an SMS Stakeholder Group
    /// </summary>
    public async Task<Result<bool>> DeleteAsync(string groupCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupCode))
            {
                _logger.LogError("DeleteSMSStakeholderGroupAsync received null or empty group code");
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.CodeRequired);
            }

            _logger.LogInformation("Deleting SMS Stakeholder Group with code: {Code}", groupCode);

            var result = await _repository.DeleteAsync(groupCode, ct);

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
    /// Assigns a user to a stakeholder group
    /// </summary>
    public async Task<Result<bool>> AssignUserToGroupAsync(string userCode, string groupCode, string assignedBy = "SYSTEM", CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode) || string.IsNullOrWhiteSpace(groupCode))
            {
                _logger.LogError("AssignUserToGroupAsync received null or empty parameters. UserCode: {UserCode}, GroupCode: {GroupCode}", userCode, groupCode);
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.UserCodeRequired);
            }

            _logger.LogInformation("Assigning user {UserCode} to group {GroupCode}", userCode, groupCode);

            var result = await _repository.AssignUserToGroupAsync(userCode, groupCode, assignedBy, ct);

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
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.AssignmentFailed);
        }
    }

    /// <summary>
    /// Removes a user from a stakeholder group
    /// </summary>
    public async Task<Result<bool>> RemoveUserFromGroupAsync(string userCode, string groupCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode) || string.IsNullOrWhiteSpace(groupCode))
            {
                _logger.LogError("RemoveUserFromGroupAsync received null or empty parameters. UserCode: {UserCode}, GroupCode: {GroupCode}", userCode, groupCode);
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.UserCodeRequired);
            }

            _logger.LogInformation("Removing user {UserCode} from group {GroupCode}", userCode, groupCode);

            var result = await _repository.RemoveUserFromGroupAsync(userCode, groupCode, ct);

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
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.RemovalFailed);
        }
    }

    /// <summary>
    /// Clears all group memberships for a user
    /// </summary>
    public async Task<Result<bool>> ClearUserGroupsAsync(string userCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode))
            {
                _logger.LogError("ClearUserGroupsAsync received null or empty user code");
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.UserCodeRequired);
            }

            _logger.LogInformation("Clearing all group memberships for user {UserCode}", userCode);

            var result = await _repository.ClearUserGroupsAsync(userCode, ct);

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
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.ClearGroupsFailed);
        }
    }

    /// <summary>
    /// Gets users by SMS Stakeholder Group code
    /// </summary>
    public async Task<Result<IEnumerable<SMSStakeholderUser>>> GetUsersByGroupCodeAsync(string groupCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupCode))
            {
                _logger.LogError("GetUsersByGroupCodeAsync received null or empty group code");
                return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderGroupError.CodeRequired);
            }

            _logger.LogInformation("Retrieving users for SMS Stakeholder Group: {GroupCode}", groupCode);
            return await _repository.GetUsersByGroupCodeAsync(groupCode, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving users for SMS Stakeholder Group: {GroupCode}", groupCode);
            return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderGroupError.NotFound);
        }
    }
}
