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

using SMS_Application.Messaging.Queries;

namespace SMS_Application.Services;

/// <summary>
/// Service for managing SMS stakeholder groups and user group memberships
/// </summary>
public class SMSStakeholderGroupService
{
    private readonly IMediator _mediator;
    private readonly ILogger<SMSStakeholderGroupService> _logger;

    public SMSStakeholderGroupService(
        IMediator mediator,
        ILogger<SMSStakeholderGroupService> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new stakeholder group
    /// </summary>
    public async Task<Result<SMSStakeholderGroup>> CreateStakeholderGroupAsync(
        string groupName,
        string? description = null,
        string createdBy = "SYSTEM")
    {
        try
        {
            _logger.LogInformation("Creating new stakeholder group: {GroupName}", groupName);

            if (string.IsNullOrWhiteSpace(groupName))
            {
                return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.SMSStakeholderGroupError.GroupNameRequired);
            }

            // Generate a code for the group (could be enhanced with proper code generation)
            var code = $"SG-0000";
            SMSStakeholderGroupID id = new(code);
            SMSStakeholderGroup smsgroup = new SMSStakeholderGroup(id);

            smsgroup.Code = code;
            smsgroup.Name = groupName;
            smsgroup.Description = description;
            smsgroup.CreatedBy = createdBy;

            var command = new CreateSMSStakeholderGroupCommand(smsgroup);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created stakeholder group: {GroupName} with code: {Code}",
                    groupName, code);
            }
            else
            {
                _logger.LogError("Failed to create stakeholder group: {GroupName}, Error: {Error}",
                    groupName, result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating stakeholder group: {GroupName}", groupName);
            return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.SMSStakeholderGroupError.CreateFailed);
        }
    }

    /// <summary>
    /// Updates an existing stakeholder group
    /// </summary>
    public async Task<Result<SMSStakeholderGroup>> UpdateStakeholderGroupAsync(
        string groupCode,
        string groupName,
        string? description = null,
        string updatedBy = "SYSTEM")
    {
        try
        {
            _logger.LogInformation("Updating stakeholder group: {GroupCode}", groupCode);

            // First get the existing group
            var getQuery = new GetSMSStakeholderGroupByCodeQuery(groupCode);
            var getResult = await _mediator.SendAsync(getQuery, CancellationToken.None);

            if (!getResult.IsSuccess || getResult.Value == null)
            {
                return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.SMSStakeholderGroupError.NotFound);
            }

            var stakeholderGroup = getResult.Value;
            stakeholderGroup.Name = groupName;
            stakeholderGroup.Description = description;
            stakeholderGroup.UpdatedBy = updatedBy;

            var command = new UpdateSMSStakeholderGroupCommand(stakeholderGroup);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated stakeholder group: {GroupCode}", groupCode);
            }
            else
            {
                _logger.LogError("Failed to update stakeholder group: {GroupCode}, Error: {Error}",
                    groupCode, result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stakeholder group: {GroupCode}", groupCode);
            return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.SMSStakeholderGroupError.UpdateFailed);
        }
    }

    /// <summary>
    /// Deletes a stakeholder group
    /// </summary>
    public async Task<Result<bool>> DeleteStakeholderGroupAsync(SMSStakeholderGroup groupCode)
    {
        try
        {
            _logger.LogInformation("Deleting stakeholder group: {GroupCode}", groupCode);

            var command = new DeleteSMSStakeholderGroupCommand(groupCode);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted stakeholder group: {GroupCode}", groupCode);
            }
            else
            {
                _logger.LogError("Failed to delete stakeholder group: {GroupCode}, Error: {Error}",
                    groupCode, result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting stakeholder group: {GroupCode}", groupCode);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.DeleteFailed);
        }
    }

    /// <summary>
    /// Gets all stakeholder groups
    /// </summary>
    public async Task<Result<IEnumerable<SMSStakeholderGroup>>> GetAllStakeholderGroupsAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving all stakeholder groups");

            var query = new GetAllSMSStakeholderGroupsQuery();
            var result = await _mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} stakeholder groups",
                    result.Value?.Count() ?? 0);
            }
            else
            {
                _logger.LogError("Failed to retrieve stakeholder groups: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all stakeholder groups");
            return Result<IEnumerable<SMSStakeholderGroup>>.Failure<IEnumerable<SMSStakeholderGroup>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    /// <summary>
    /// Gets a stakeholder group by code
    /// </summary>
    public async Task<Result<SMSStakeholderGroup>> GetStakeholderGroupByCodeAsync(string groupCode)
    {
        try
        {
            _logger.LogInformation("Retrieving stakeholder group: {GroupCode}", groupCode);

            var query = new GetSMSStakeholderGroupByCodeQuery(groupCode);
            var result = await _mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved stakeholder group: {GroupCode}", groupCode);
            }
            else
            {
                _logger.LogWarning("Stakeholder group not found: {GroupCode}", groupCode);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stakeholder group: {GroupCode}", groupCode);
            return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    /// <summary>
    /// Gets stakeholder groups for a specific user
    /// </summary>
    public async Task<Result<IEnumerable<SMSStakeholderGroup>>> GetSMSStakeholderGroupsByUserCodeAsync(string userCode)
    {
        try
        {
            _logger.LogInformation("Retrieving stakeholder groups for user: {UserCode}", userCode);

            var query = new GetSMSStakeholderGroupsByUserCodeQuery(userCode);
            var result = await _mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} stakeholder groups for user: {UserCode}",
                    result.Value?.Count() ?? 0, userCode);
            }
            else
            {
                _logger.LogError("Failed to retrieve stakeholder groups for user {UserCode}: {Error}",
                    userCode, result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stakeholder groups for user: {UserCode}", userCode);
            return Result<IEnumerable<SMSStakeholderGroup>>.Failure<IEnumerable<SMSStakeholderGroup>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    /// <summary>
    /// Assigns a user to a stakeholder group
    /// </summary>
    public async Task<Result<bool>> AssignUserToGroupAsync(
        string userCode,
        SMSStakeholderGroupID groupCode,
        string assignedBy = "SYSTEM")
    {
        try
        {
            _logger.LogInformation("Assigning user {UserCode} to group {GroupCode}", userCode, groupCode);

            var command = new AssignUserToStakeholderGroupCommand(userCode, groupCode);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

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
            _logger.LogError(ex, "Error assigning user {UserCode} to group {GroupCode}", userCode, groupCode);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.AssignmentFailed);
        }
    }

    /// <summary>
    /// Removes a user from a stakeholder group
    /// </summary>
    public async Task<Result<bool>> RemoveUserFromGroupAsync(string userCode, SMSStakeholderGroupID groupCode)
    {
        try
        {
            _logger.LogInformation("Removing user {UserCode} from group {GroupCode}", userCode, groupCode);

            var command = new RemoveUserFromStakeholderGroupCommand(userCode, groupCode);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

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
            _logger.LogError(ex, "Error removing user {UserCode} from group {GroupCode}", userCode, groupCode);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.RemovalFailed);
        }
    }

    /// <summary>
    /// Clears all group memberships for a user
    /// </summary>
    public async Task<Result<bool>> ClearUserGroupMembershipsAsync(string userCode)
    {
        try
        {
            _logger.LogInformation("Clearing all group memberships for user: {UserCode}", userCode);

            var command = new ClearUserStakeholderGroupsCommand(userCode);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

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
            _logger.LogError(ex, "Error clearing group memberships for user: {UserCode}", userCode);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.ClearGroupsFailed);
        }
    }

    /// <summary>
    /// Updates a user's group memberships (clears existing and assigns new ones)
    /// </summary>
    public async Task<Result<bool>> UpdateUserGroupMembershipsAsync(
        string userCode,
        IEnumerable<SMSStakeholderGroupID> groupCodes,
        string assignedBy = "SYSTEM")
    {
        try
        {
            _logger.LogInformation("Updating group memberships for user: {UserCode}", userCode);

            // Clear existing memberships first
            var clearResult = await ClearUserGroupMembershipsAsync(userCode);
            if (!clearResult.IsSuccess)
            {
                return clearResult;
            }

            // Assign new memberships
            foreach (var groupCode in groupCodes)
            {
                var assignResult = await AssignUserToGroupAsync(userCode, groupCode, assignedBy);
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
            _logger.LogError(ex, "Error updating group memberships for user: {UserCode}", userCode);
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}
