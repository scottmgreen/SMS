//-----------------------------------------------------------------------
// <copyright file="SMSOrganizationalGroupDataService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS Organizational Group data service providing business operations for SMS domain entities.
//                  Infrastructure layer service implementing data access patterns
//                  through repositories while maintaining clean architecture.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using SMS_Domain.Errors;

namespace SMS_Infrastructure.Services;

/// <summary>
/// SMS Organizational Group Data Service providing business operations for SMSOrganizationalGroup entities
/// </summary>
public class SMSOrganizationalGroupDataService : BaseDataService<SMSOrganizationalGroupDataService>
{
    private readonly SMSOrganizationalGroupRepository _repository;
    private readonly ILogger<SMSOrganizationalGroupDataService> _logger;

    public SMSOrganizationalGroupDataService(
        ILogger<SMSOrganizationalGroupDataService> logger,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        SMSOrganizationalGroupRepository repository)
        : base(logger, serviceScopeFactory, configuration)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new SMS Organizational Group
    /// </summary>
    public async Task<Result<SMSOrganizationalGroup>> CreateAsync(SMSOrganizationalGroup group, CancellationToken ct = default)
    {
        try
        {
            if (group is null)
            {
                _logger.LogInfrastructureError("CreateSMSOrganizationalGroupAsync received null group");
                return Result<SMSOrganizationalGroup>.Failure<SMSOrganizationalGroup>(DomainErrors.SMSOrganizationalGroupError.NullOrEmpty);
            }

            _logger.LogInfrastructureInformation("Creating SMS Organizational Group with code: {Code}", group.Code);

            var result = await _repository.AddAsync(group);

            if (result.IsSuccess)
            {
                _logger.LogInfrastructureInformation("Successfully created SMS Organizational Group with code: {Code}", result.Value?.Code);
            }
            else
            {
                _logger.LogInfrastructureError("Failed to create SMS Organizational Group. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Unexpected error creating SMS Organizational Group");
            return Result<SMSOrganizationalGroup>.Failure<SMSOrganizationalGroup>(DomainErrors.SMSOrganizationalGroupError.CreateFailed);
        }
    }

    /// <summary>
    /// Gets SMS Organizational Group by code
    /// </summary>
    public async Task<Result<SMSOrganizationalGroup>> GetByCodeAsync(string groupCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupCode))
            {
                _logger.LogInfrastructureError("GetSMSOrganizationalGroupByCodeAsync received null or empty group code");
                return Result<SMSOrganizationalGroup>.Failure<SMSOrganizationalGroup>(DomainErrors.SMSOrganizationalGroupError.CodeRequired);
            }

            _logger.LogInfrastructureInformation("Retrieving SMS Organizational Group with code: {Code}", groupCode);
            return await _repository.GetByCodeAsync(groupCode);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Unexpected error retrieving SMS Organizational Group with code: {Code}", groupCode);
            return Result<SMSOrganizationalGroup>.Failure<SMSOrganizationalGroup>(DomainErrors.SMSOrganizationalGroupError.NotFound);
        }
    }

    /// <summary>
    /// Gets all SMS Organizational Groups
    /// </summary>
    public async Task<Result<IEnumerable<SMSOrganizationalGroup>>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureInformation("Retrieving all SMS Organizational Groups");
            return await _repository.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Unexpected error retrieving all SMS Organizational Groups");
            return Result<IEnumerable<SMSOrganizationalGroup>>.Failure<IEnumerable<SMSOrganizationalGroup>>(DomainErrors.SMSOrganizationalGroupError.NotFound);
        }
    }

    /// <summary>
    /// Gets SMS Organizational Groups by user code
    /// </summary>
    public async Task<Result<IEnumerable<SMSOrganizationalGroup>>> GetGroupsByUserCodeAsync(string userCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode))
            {
                _logger.LogInfrastructureError("GetSMSOrganizationalGroupsByUserCodeAsync received null or empty user code");
                return Result<IEnumerable<SMSOrganizationalGroup>>.Failure<IEnumerable<SMSOrganizationalGroup>>(DomainErrors.SMSOrganizationalGroupError.UserCodeRequired);
            }

            _logger.LogInfrastructureInformation("Retrieving SMS Organizational Groups by user code: {UserCode}", userCode);

            // Note: This method would need to be implemented in the repository if needed
            // For now, return an empty list as this functionality may not be implemented yet
            var result = await _repository.GetGroupsByUserCodeAsync(userCode);


            return Result<IEnumerable<SMSOrganizationalGroup>>.Success(result.Value.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Unexpected error retrieving SMS Organizational Groups by user code: {UserCode}", userCode);
            return Result<IEnumerable<SMSOrganizationalGroup>>.Failure<IEnumerable<SMSOrganizationalGroup>>(DomainErrors.SMSOrganizationalGroupError.NotFound);
        }
    }

    /// <summary>
    /// Updates an existing SMS Organizational Group
    /// </summary>
    public async Task<Result<SMSOrganizationalGroup>> UpdateAsync(SMSOrganizationalGroup group, CancellationToken ct = default)
    {
        try
        {
            if (group is null)
            {
                _logger.LogInfrastructureError("UpdateSMSOrganizationalGroupAsync received null group");
                return Result<SMSOrganizationalGroup>.Failure<SMSOrganizationalGroup>(DomainErrors.SMSOrganizationalGroupError.NullOrEmpty);
            }

            _logger.LogInfrastructureInformation("Updating SMS Organizational Group with code: {Code}", group.Code);

            var result = await _repository.UpdateAsync(group);

            if (result.IsSuccess)
            {
                _logger.LogInfrastructureInformation("Successfully updated SMS Organizational Group with code: {Code}", group.Code);
                // Return the updated group by getting it from the repository
                return await _repository.GetByCodeAsync(group.Code);
            }
            else
            {
                _logger.LogInfrastructureError("Failed to update SMS Organizational Group. Error: {Error}", result.Error?.Message);
                return Result<SMSOrganizationalGroup>.Failure<SMSOrganizationalGroup>(DomainErrors.SMSOrganizationalGroupError.UpdateFailed);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Unexpected error updating SMS Organizational Group with code: {Code}", group?.Code);
            return Result<SMSOrganizationalGroup>.Failure<SMSOrganizationalGroup>(DomainErrors.SMSOrganizationalGroupError.UpdateFailed);
        }
    }

    /// <summary>
    /// Deletes an SMS Organizational Group
    /// </summary>
    public async Task<Result<bool>> DeleteAsync(string groupCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupCode))
            {
                _logger.LogInfrastructureError("DeleteSMSOrganizationalGroupAsync received null or empty group code");
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalGroupError.CodeRequired);
            }

            _logger.LogInfrastructureInformation("Deleting SMS Organizational Group with code: {Code}", groupCode);

            var result = await _repository.DeleteAsync(groupCode);

            if (result.IsSuccess)
            {
                _logger.LogInfrastructureInformation("Successfully deleted SMS Organizational Group with code: {Code}", groupCode);
            }
            else
            {
                _logger.LogInfrastructureError("Failed to delete SMS Organizational Group. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Unexpected error deleting SMS Organizational Group with code: {Code}", groupCode);
            return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalGroupError.DeleteFailed);
        }
    }

    /// <summary>
    /// Assigns a user to an organizational group
    /// </summary>
    public async Task<Result<bool>> AssignUserToGroupAsync(string userCode, string groupCode, string assignedBy, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode) || string.IsNullOrWhiteSpace(groupCode))
            {
                _logger.LogInfrastructureError("AssignUserToGroupAsync received null or empty parameters. UserCode: {UserCode}, GroupCode: {GroupCode}", userCode, groupCode);
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalGroupError.UserCodeRequired);
            }

            _logger.LogInfrastructureInformation("Assigning user {UserCode} to group {GroupCode}", userCode, groupCode);

            var result = await _repository.AssignUserToGroupAsync(userCode, groupCode, assignedBy);

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
            if (string.IsNullOrWhiteSpace(userCode) || string.IsNullOrWhiteSpace(groupCode))
            {
                _logger.LogInfrastructureError("RemoveUserFromGroupAsync received null or empty parameters. UserCode: {UserCode}, GroupCode: {GroupCode}", userCode, groupCode);
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalGroupError.UserCodeRequired);
            }

            _logger.LogInfrastructureInformation("Removing user {UserCode} from group {GroupCode}", userCode, groupCode);

            var result = await _repository.RemoveUserFromGroupAsync(userCode, groupCode);

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
            if (string.IsNullOrWhiteSpace(userCode))
            {
                _logger.LogInfrastructureError("ClearUserGroupsAsync received null or empty user code");
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalGroupError.UserCodeRequired);
            }

            _logger.LogInfrastructureInformation("Clearing all group memberships for user {UserCode}", userCode);

            var result = await _repository.ClearUserGroupsAsync(userCode, clearedBy: userCode);

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
            return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalGroupError.ClearGroupsFailed);
        }
    }

    /// <summary>
    /// Gets users by organizational group code
    /// </summary>
    public async Task<Result<IEnumerable<SMSOrganizationalUser>>> GetUsersByGroupCodeAsync(string groupCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupCode))
            {
                _logger.LogInfrastructureError("GetUsersByGroupCodeAsync received null or empty group code");
                return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(DomainErrors.SMSOrganizationalGroupError.CodeRequired);
            }

            _logger.LogInfrastructureInformation("Retrieving users for group {GroupCode}", groupCode);

            return await _repository.GetUsersByGroupCodeAsync(groupCode);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Unexpected error retrieving users for group {GroupCode}", groupCode);
            return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(DomainErrors.SMSOrganizationalGroupError.NotFound);
        }
    }
}


