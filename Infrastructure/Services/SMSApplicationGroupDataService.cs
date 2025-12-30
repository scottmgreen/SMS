using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SMS_Domain.Entities;
using SMS_Domain.Interfaces;
using SMS_Domain.Errors;
using SMS_Infrastructure.Common;
using SMS_Infrastructure.Interfaces;
using SMS_Infrastructure.Persistence;
using SMS_Shared.Common;

namespace SMS_Infrastructure.Services;

/// <summary>
/// Data service for SMS Application Group operations
/// Provides high-level data access abstraction over repository layer
/// </summary>
public sealed class SMSApplicationGroupDataService : BaseDataService<SMSApplicationGroupDataService>
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
                _logger.LogError("CreateSMSApplicationGroupAsync received null group");
                return Result<SMSApplicationGroup>.Failure<SMSApplicationGroup>(DomainErrors.SMSApplicationGroupError.NullOrEmpty);
            }

            _logger.LogInformation("Creating SMS Application Group with code: {Code}", group.Code);
            
            var result = await _repository.CreateAsync(group, ct);

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
    public async Task<Result<SMSApplicationGroup>> GetByCodeAsync(string groupCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupCode))
            {
                _logger.LogError("GetSMSApplicationGroupByCodeAsync received null or empty group code");
                return Result<SMSApplicationGroup>.Failure<SMSApplicationGroup>(DomainErrors.SMSApplicationGroupError.CodeRequired);
            }

            _logger.LogInformation("Retrieving SMS Application Group with code: {Code}", groupCode);
            return await _repository.GetByCodeAsync(groupCode, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving SMS Application Group with code: {Code}", groupCode);
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
                _logger.LogError("GetSMSApplicationGroupByCodeWithMembersAsync received null or empty group code");
                return Result<(SMSApplicationGroup, List<SMSApplicationUser>)>.Failure<(SMSApplicationGroup, List<SMSApplicationUser>)>(DomainErrors.SMSApplicationGroupError.CodeRequired);
            }

            _logger.LogInformation("Retrieving SMS Application Group with members for code: {Code}", groupCode);
            return await _repository.GetByCodeWithMembersAsync(groupCode, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving SMS Application Group with members for code: {Code}", groupCode);
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
            _logger.LogInformation("Retrieving all SMS Application Groups");
            return await _repository.GetAllAsync(ct);
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
    public async Task<Result<IEnumerable<SMSApplicationGroup>>> GetByUserCodeAsync(string userCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode))
            {
                _logger.LogError("GetSMSApplicationGroupsByUserCodeAsync received null or empty user code");
                return Result<IEnumerable<SMSApplicationGroup>>.Failure<IEnumerable<SMSApplicationGroup>>(DomainErrors.SMSApplicationGroupError.UserCodeRequired);
            }

            _logger.LogInformation("Retrieving SMS Application Groups by user code: {UserCode}", userCode);
            return await _repository.GetGroupsByUserCodeAsync(userCode, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving SMS Application Groups by user code: {UserCode}", userCode);
            return Result<IEnumerable<SMSApplicationGroup>>.Failure<IEnumerable<SMSApplicationGroup>>(DomainErrors.SMSApplicationGroupError.NotFound);
        }
    }

    /// <summary>
    /// Updates an existing SMS Application Group
    /// </summary>
    public async Task<Result<SMSApplicationGroup>> UpdateAsync(SMSApplicationGroup group, CancellationToken ct = default)
    {
        try
        {
            if (group is null)
            {
                _logger.LogError("UpdateSMSApplicationGroupAsync received null group");
                return Result<SMSApplicationGroup>.Failure<SMSApplicationGroup>(DomainErrors.SMSApplicationGroupError.NullOrEmpty);
            }

            _logger.LogInformation("Updating SMS Application Group with code: {Code}", group.Code);
            
            var result = await _repository.UpdateAsync(group, ct);

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
    /// Deletes an SMS Application Group
    /// </summary>
    public async Task<Result<bool>> DeleteAsync(string groupCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupCode))
            {
                _logger.LogError("DeleteSMSApplicationGroupAsync received null or empty group code");
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.CodeRequired);
            }

            _logger.LogInformation("Deleting SMS Application Group with code: {Code}", groupCode);
            
            var result = await _repository.DeleteAsync(groupCode, ct);

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
    /// Assigns a user to an application group
    /// </summary>
    public async Task<Result<bool>> AssignUserToGroupAsync(string userCode, string groupCode, string assignedBy = "SYSTEM", CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode) || string.IsNullOrWhiteSpace(groupCode))
            {
                _logger.LogError("AssignUserToGroupAsync received null or empty parameters. UserCode: {UserCode}, GroupCode: {GroupCode}", userCode, groupCode);
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.UserCodeRequired);
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
                _logger.LogError("RemoveUserFromGroupAsync received null or empty parameters. UserCode: {UserCode}, GroupCode: {GroupCode}", userCode, groupCode);
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.UserCodeRequired);
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
                _logger.LogError("GetUsersByGroupCodeAsync received null or empty group code");
                return Result<IEnumerable<SMSApplicationGroup>>.Failure<IEnumerable<SMSApplicationGroup>>(DomainErrors.SMSApplicationGroupError.CodeRequired);
            }

            _logger.LogInformation("Retrieving users for SMS Application Group: {GroupCode}", userCode);
            
            return await _repository.GetGroupsByUserCodeAsync(userCode, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving users for SMS Application Group: {GroupCode}", userCode);
            return Result<IEnumerable<SMSApplicationGroup>>.Failure<IEnumerable<SMSApplicationGroup>>(DomainErrors.SMSApplicationGroupError.NotFound);
        }
    }


    public async Task<Result<IEnumerable<SMSApplicationUser>>> GetUsersByGroupCodeAsync(string groupCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupCode))
            {
                _logger.LogError("GetUsersByGroupCodeAsync received null or empty group code");
                return Result<IEnumerable<SMSApplicationUser>>.Failure<IEnumerable<SMSApplicationUser>>(DomainErrors.SMSApplicationGroupError.CodeRequired);
            }

            _logger.LogInformation("Retrieving users for SMS Application Group: {GroupCode}", groupCode);

            return await _repository.GetUsersByGroupCodeAsync(groupCode, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving users for SMS Application Group: {GroupCode}", groupCode);
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
                _logger.LogError("ClearUserGroupsAsync received null or empty user code");
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.UserCodeRequired);
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
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.ClearGroupsFailed);
        }
    }
}