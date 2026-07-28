//-----------------------------------------------------------------------
// <copyright file="SMSApplicationGroupDataService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS Application Group data service providing business operations for SMS domain entities.
//                  Infrastructure layer service implementing data access patterns
//                  through repositories while maintaining clean architecture.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using SMS_Domain.Errors;

namespace SMS_Infrastructure.Services;

/// <summary>
/// SMS Application Group Data Service providing business operations for SMSApplicationGroup entities
/// </summary>
public class SMSApplicationGroupDataService : BaseDataService<SMSApplicationGroupDataService>
{
    private readonly SMSApplicationGroupRepository _repository;
    private readonly ILogger<SMSApplicationGroupDataService> _logger;

    public SMSApplicationGroupDataService(
        ILogger<SMSApplicationGroupDataService> logger,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        SMSApplicationGroupRepository repository)
        : base(logger, serviceScopeFactory, configuration)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new SMS Application Group
    /// </summary>
    public async Task<Result<SMSApplicationGroup>> CreateAsync(SMSApplicationGroup group, CancellationToken ct = default)
    {
        try
        {
            if (group is null)
            {
                _logger.LogInfrastructureError("CreateSMSApplicationGroupAsync received null group");
                return Result<SMSApplicationGroup>.Failure<SMSApplicationGroup>(DomainErrors.SMSApplicationGroupError.NullOrEmpty);
            }

            _logger.LogInfrastructureInformation("Creating SMS Application Group with code: {Code}", group.Code);

            var result = await _repository.CreateAsync(group, ct);

            if (result.IsSuccess)
            {
                _logger.LogInfrastructureInformation("Successfully created SMS Application Group with code: {Code}", result.Value?.Code);
            }
            else
            {
                _logger.LogInfrastructureError("Failed to create SMS Application Group. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Unexpected error creating SMS Application Group");
            return Result<SMSApplicationGroup>.Failure<SMSApplicationGroup>(DomainErrors.SMSApplicationGroupError.CreateFailed);
        }
    }

    /// <summary>
    /// Gets SMS Application Group by code
    /// </summary>
    public async Task<Result<SMSApplicationGroup>> GetByCodeAsync(string groupCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupCode))
            {
                _logger.LogInfrastructureError("GetSMSApplicationGroupByCodeAsync received null or empty group code");
                return Result<SMSApplicationGroup>.Failure<SMSApplicationGroup>(DomainErrors.SMSApplicationGroupError.CodeRequired);
            }

            _logger.LogInfrastructureInformation("Retrieving SMS Application Group with code: {Code}", groupCode);
            return await _repository.GetByCodeAsync(groupCode, ct);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Unexpected error retrieving SMS Application Group with code: {Code}", groupCode);
            return Result<SMSApplicationGroup>.Failure<SMSApplicationGroup>(DomainErrors.SMSApplicationGroupError.NotFound);
        }
    }

    /// <summary>
    /// Gets SMS Application Group by code with its members
    /// </summary>
    public async Task<Result<(SMSApplicationGroup Group, List<SMSApplicationUser> Members)>> GetByCodeWithMembersAsync(string groupCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupCode))
            {
                _logger.LogInfrastructureError("GetSMSApplicationGroupByCodeWithMembersAsync received null or empty group code");
                return Result<(SMSApplicationGroup, List<SMSApplicationUser>)>.Failure<(SMSApplicationGroup, List<SMSApplicationUser>)>(DomainErrors.SMSApplicationGroupError.CodeRequired);
            }

            _logger.LogInfrastructureInformation("Retrieving SMS Application Group with members for code: {Code}", groupCode);
            return await _repository.GetByCodeWithMembersAsync(groupCode, ct);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Unexpected error retrieving SMS Application Group with members for code: {Code}", groupCode);
            return Result<(SMSApplicationGroup, List<SMSApplicationUser>)>.Failure<(SMSApplicationGroup, List<SMSApplicationUser>)>(DomainErrors.SMSApplicationGroupError.NotFound);
        }
    }

    /// <summary>
    /// Gets all SMS Application Groups
    /// </summary>
    public async Task<Result<IEnumerable<SMSApplicationGroup>>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureInformation("Retrieving all SMS Application Groups");
            return await _repository.GetAllAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Unexpected error retrieving all SMS Application Groups");
            return Result<IEnumerable<SMSApplicationGroup>>.Failure<IEnumerable<SMSApplicationGroup>>(DomainErrors.SMSApplicationGroupError.NotFound);
        }
    }

    /// <summary>
    /// Gets SMS Application Groups by user code
    /// </summary>
    //public async Task<Result<IEnumerable<SMSApplicationGroup>>> GetGroupsByUserCodeAsync(string userCode, CancellationToken ct = default)
    //{
    //    try
    //    {
    //        if (string.IsNullOrWhiteSpace(userCode))
    //        {
    //            _logger.LogInfrastructureError("GetSMSApplicationGroupsByUserCodeAsync received null or empty user code");
    //            return Result<IEnumerable<SMSApplicationGroup>>.Failure<IEnumerable<SMSApplicationGroup>>(DomainErrors.SMSApplicationGroupError.UserCodeRequired);
    //        }

    //        _logger.LogInfrastructureInformation("Retrieving SMS Application Groups by user code: {UserCode}", userCode);
    //        return await _repository.GetGroupsByUserCodeAsync(userCode, ct);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogInfrastructureError(ex, "Unexpected error retrieving SMS Application Groups by user code: {UserCode}", userCode);
    //        return Result<IEnumerable<SMSApplicationGroup>>.Failure<IEnumerable<SMSApplicationGroup>>(DomainErrors.SMSApplicationGroupError.NotFound);
    //    }
    //}

    /// <summary>
    /// Updates an existing SMS Application Group
    /// </summary>
    public async Task<Result<SMSApplicationGroup>> UpdateAsync(SMSApplicationGroup group, CancellationToken ct = default)
    {
        try
        {
            if (group is null)
            {
                _logger.LogInfrastructureError("UpdateSMSApplicationGroupAsync received null group");
                return Result<SMSApplicationGroup>.Failure<SMSApplicationGroup>(DomainErrors.SMSApplicationGroupError.NullOrEmpty);
            }

            _logger.LogInfrastructureInformation("Updating SMS Application Group with code: {Code}", group.Code);

            var result = await _repository.UpdateAsync(group, ct);

            if (result.IsSuccess)
            {
                _logger.LogInfrastructureInformation("Successfully updated SMS Application Group with code: {Code}", group.Code);
            }
            else
            {
                _logger.LogInfrastructureError("Failed to update SMS Application Group. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Unexpected error updating SMS Application Group with code: {Code}", group?.Code);
            return Result<SMSApplicationGroup>.Failure<SMSApplicationGroup>(DomainErrors.SMSApplicationGroupError.UpdateFailed);
        }
    }

    /// <summary>
    /// Deletes an SMS Application Group
    /// </summary>
    public async Task<Result<bool>> DeleteAsync(string groupCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupCode))
            {
                _logger.LogInfrastructureError("DeleteSMSApplicationGroupAsync received null or empty group code");
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.CodeRequired);
            }

            _logger.LogInfrastructureInformation("Deleting SMS Application Group with code: {Code}", groupCode);

            var result = await _repository.DeleteAsync(groupCode, ct);

            if (result.IsSuccess)
            {
                _logger.LogInfrastructureInformation("Successfully deleted SMS Application Group with code: {Code}", groupCode);
            }
            else
            {
                _logger.LogInfrastructureError("Failed to delete SMS Application Group. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Unexpected error deleting SMS Application Group with code: {Code}", groupCode);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.DeleteFailed);
        }
    }

    /// <summary>
    /// Assigns a user to an application group
    /// </summary>
    public async Task<Result<bool>> AssignUserToGroupAsync(string userCode, string groupCode, string assignedBy, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode) || string.IsNullOrWhiteSpace(groupCode))
            {
                _logger.LogInfrastructureError("AssignUserToGroupAsync received null or empty parameters. UserCode: {UserCode}, GroupCode: {GroupCode}", userCode, groupCode);
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.UserCodeRequired);
            }

            _logger.LogInfrastructureInformation("Assigning user {UserCode} to group {GroupCode}", userCode, groupCode);

            var result = await _repository.AssignUserToGroupAsync(userCode, groupCode, assignedBy, ct);

            if (result.IsSuccess)
            {
                _logger.LogInfrastructureInformation("Successfully assigned user {UserCode} to group {GroupCode}", userCode, groupCode);
            }
            else
            {
                _logger.LogInfrastructureError("Failed to assign user to group. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Unexpected error assigning user {UserCode} to group {GroupCode}", userCode, groupCode);
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
            if (string.IsNullOrWhiteSpace(userCode) || string.IsNullOrWhiteSpace(groupCode))
            {
                _logger.LogInfrastructureError("RemoveUserFromGroupAsync received null or empty parameters. UserCode: {UserCode}, GroupCode: {GroupCode}", userCode, groupCode);
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.UserCodeRequired);
            }

            _logger.LogInfrastructureInformation("Removing user {UserCode} from group {GroupCode}", userCode, groupCode);

            var result = await _repository.RemoveUserFromGroupAsync(userCode, groupCode, ct);

            if (result.IsSuccess)
            {
                _logger.LogInfrastructureInformation("Successfully removed user {UserCode} from group {GroupCode}", userCode, groupCode);
            }
            else
            {
                _logger.LogInfrastructureError("Failed to remove user from group. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Unexpected error removing user {UserCode} from group {GroupCode}", userCode, groupCode);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.RemovalFailed);
        }
    }

    /// <summary>
    /// Gets users by application group code
    /// </summary>
    public async Task<Result<IEnumerable<SMSApplicationGroup>>> GetGroupsByUserCodeAsync(string userCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode))
            {
                _logger.LogInfrastructureError("GetUsersByGroupCodeAsync received null or empty group code");
                return Result<IEnumerable<SMSApplicationGroup>>.Failure<IEnumerable<SMSApplicationGroup>>(DomainErrors.SMSApplicationGroupError.CodeRequired);
            }

            _logger.LogInfrastructureInformation("Retrieving users for SMS Application Group: {GroupCode}", userCode);

            return await _repository.GetGroupsByUserCodeAsync(userCode, ct);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Unexpected error retrieving users for SMS Application Group: {GroupCode}", userCode);
            return Result<IEnumerable<SMSApplicationGroup>>.Failure<IEnumerable<SMSApplicationGroup>>(DomainErrors.SMSApplicationGroupError.NotFound);
        }
    }


    public async Task<Result<IEnumerable<SMSApplicationUser>>> GetUsersByGroupCodeAsync(string groupCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupCode))
            {
                _logger.LogInfrastructureError("GetUsersByGroupCodeAsync received null or empty group code");
                return Result<IEnumerable<SMSApplicationUser>>.Failure<IEnumerable<SMSApplicationUser>>(DomainErrors.SMSApplicationGroupError.CodeRequired);
            }

            _logger.LogInfrastructureInformation("Retrieving users for SMS Application Group: {GroupCode}", groupCode);

            return await _repository.GetUsersByGroupCodeAsync(groupCode, ct);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Unexpected error retrieving users for SMS Application Group: {GroupCode}", groupCode);
            return Result<IEnumerable<SMSApplicationUser>>.Failure<IEnumerable<SMSApplicationUser>>(DomainErrors.SMSApplicationGroupError.NotFound);
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
                _logger.LogInfrastructureError("ClearUserGroupsAsync received null or empty user code");
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.UserCodeRequired);
            }

            _logger.LogInfrastructureInformation("Clearing all group memberships for user {UserCode}", userCode);

            var result = await _repository.ClearUserGroupsAsync(userCode, ct);

            if (result.IsSuccess)
            {
                _logger.LogInfrastructureInformation("Successfully cleared all group memberships for user {UserCode}", userCode);
            }
            else
            {
                _logger.LogInfrastructureError("Failed to clear user groups. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Unexpected error clearing group memberships for user {UserCode}", userCode);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.ClearGroupsFailed);
        }
    }
}

