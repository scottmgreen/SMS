//-----------------------------------------------------------------------
// <copyright file="SMSStakeholderGroupCommandHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command handlers implementing business logic for SMS write operations.
//                  Implements command handlers for processing write operations.
//                  Handles business logic execution and domain entity coordination.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using Microsoft.Extensions.Logging;

namespace SMS_Application.CommandHandlers;

// =============================================
// SMS STAKEHOLDER GROUP COMMAND HANDLERS - Clean Architecture Pattern
// =============================================

/// <summary>
/// Command handler for creating SMS stakeholder groups
/// </summary>
public class CreateSMSStakeholderGroupCommandHandler : BaseCommandBundle, IBaseRequestHandler<CreateSMSStakeholderGroupCommand, Result<SMSStakeholderGroup>>
{
    private readonly SMSStakeholderGroupService _stakeholderGroupService;
    private readonly ILogger<CreateSMSStakeholderGroupCommandHandler> _logger;

    public CreateSMSStakeholderGroupCommandHandler(
        SMSStakeholderGroupService stakeholderGroupService,
        ILogger<CreateSMSStakeholderGroupCommandHandler> logger)
    {
        _stakeholderGroupService = stakeholderGroupService ?? throw new ArgumentNullException(nameof(stakeholderGroupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSStakeholderGroup>> HandleAsync(CreateSMSStakeholderGroupCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation(" Processing CreateSMSStakeholderGroupCommand for group: {GroupCode}", request.StakeholderGroup?.Code);

            if (request?.StakeholderGroup == null)
            {
                _logger.LogApplicationWarning("CreateSMSStakeholderGroupCommand received with null stakeholder group");
                return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.SMSStakeholderGroupError.NullOrEmpty);
            }

            var result = await _stakeholderGroupService.CreateSMSStakeholderGroupAsync(request.StakeholderGroup, ct);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully created SMS stakeholder group: {GroupCode}", request.StakeholderGroup.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to create SMS stakeholder group: {GroupCode}, Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("CreateSMSStakeholderGroupCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing CreateSMSStakeholderGroupCommand for group: {GroupCode}",
                ApplicationEventIds.Error, ex);
            return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.SMSStakeholderGroupError.CreateFailed);
        }
    }
}

/// <summary>
/// Command handler for updating SMS stakeholder groups
/// </summary>
public class UpdateSMSStakeholderGroupCommandHandler : BaseCommandBundle, IBaseRequestHandler<UpdateSMSStakeholderGroupCommand, Result<SMSStakeholderGroup>>
{
    private readonly SMSStakeholderGroupService _stakeholderGroupService;
    private readonly ILogger<UpdateSMSStakeholderGroupCommandHandler> _logger;

    public UpdateSMSStakeholderGroupCommandHandler(
        SMSStakeholderGroupService stakeholderGroupService,
        ILogger<UpdateSMSStakeholderGroupCommandHandler> logger)
    {
        _stakeholderGroupService = stakeholderGroupService ?? throw new ArgumentNullException(nameof(stakeholderGroupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSStakeholderGroup>> HandleAsync(UpdateSMSStakeholderGroupCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation(" Processing UpdateSMSStakeholderGroupCommand for group: {GroupCode}", request.StakeholderGroup?.Code);

            if (request?.StakeholderGroup == null)
            {
                _logger.LogApplicationWarning("UpdateSMSStakeholderGroupCommand received with null stakeholder group");
                return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.SMSStakeholderGroupError.NullOrEmpty);
            }

            var result = await _stakeholderGroupService.UpdateSMSStakeholderGroupAsync(request.StakeholderGroup, ct);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully updated SMS stakeholder group: {GroupCode}", request.StakeholderGroup.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to update SMS stakeholder group: {GroupCode}, Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("UpdateSMSStakeholderGroupCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing UpdateSMSStakeholderGroupCommand for group: {GroupCode}",
                ApplicationEventIds.Error, ex);
            return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.SMSStakeholderGroupError.UpdateFailed);
        }
    }
}

/// <summary>
/// Command handler for deleting SMS stakeholder groups
/// </summary>
public class DeleteSMSStakeholderGroupCommandHandler : BaseCommandBundle, IBaseRequestHandler<DeleteSMSStakeholderGroupCommand, Result<bool>>
{
    private readonly SMSStakeholderGroupService _stakeholderGroupService;
    private readonly ILogger<DeleteSMSStakeholderGroupCommandHandler> _logger;

    public DeleteSMSStakeholderGroupCommandHandler(
        SMSStakeholderGroupService stakeholderGroupService,
        ILogger<DeleteSMSStakeholderGroupCommandHandler> logger)
    {
        _stakeholderGroupService = stakeholderGroupService ?? throw new ArgumentNullException(nameof(stakeholderGroupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteSMSStakeholderGroupCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation(" Processing DeleteSMSStakeholderGroupCommand for group: {GroupCode}", request.StakeholderGroup.Code);

            if (string.IsNullOrWhiteSpace(request.StakeholderGroup.Code))
            {
                _logger.LogApplicationWarning("DeleteSMSStakeholderGroupCommand received with null or empty group code");
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.CodeRequired);
            }

            var result = await _stakeholderGroupService.DeleteSMSStakeholderGroupAsync(request.StakeholderGroup.Code, ct);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully deleted SMS stakeholder group: {GroupCode}", request.StakeholderGroup.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete SMS stakeholder group: {GroupCode}, Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("DeleteSMSStakeholderGroupCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing DeleteSMSStakeholderGroupCommand for group: {GroupCode}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.DeleteFailed);
        }
    }
}

/// <summary>
/// Command handler for assigning users to stakeholder groups
/// </summary>
public class AssignUserToStakeholderGroupCommandHandler : BaseCommandBundle, IBaseRequestHandler<AssignUserToStakeholderGroupCommand, Result<bool>>
{
    private readonly SMSStakeholderGroupService _stakeholderGroupService;
    private readonly ILogger<AssignUserToStakeholderGroupCommandHandler> _logger;

    public AssignUserToStakeholderGroupCommandHandler(
        SMSStakeholderGroupService stakeholderGroupService,
        ILogger<AssignUserToStakeholderGroupCommandHandler> logger)
    {
        _stakeholderGroupService = stakeholderGroupService ?? throw new ArgumentNullException(nameof(stakeholderGroupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(AssignUserToStakeholderGroupCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation(" Processing AssignUserToStakeholderGroupCommand for user: {UserCode} to group: {GroupCode}",
                request.UserCode, request.StakeholderGroupID);

            if (string.IsNullOrWhiteSpace(request.UserCode) || string.IsNullOrWhiteSpace(request.StakeholderGroupID.Value))
            {
                _logger.LogApplicationWarning("AssignUserToStakeholderGroupCommand received with null or empty user/group code");
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.UserCodeRequired);
            }

            var result = await _stakeholderGroupService.AssignUserToGroupAsync(request.UserCode, request.StakeholderGroupID, request.AssignedBy);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully assigned user {UserCode} to group {GroupCode}", request.UserCode, request.StakeholderGroupID.Value);
            }
            else
            {
                _logger.LogApplicationError("Failed to assign user {UserCode} to group {GroupCode}, Error: {Error}", ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("AssignUserToStakeholderGroupCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing AssignUserToStakeholderGroupCommand for user: {UserCode} to group: {GroupCode}",ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.AssignmentFailed);
        }
    }
}

/// <summary>
/// Command handler for removing users from stakeholder groups
/// </summary>
public class RemoveUserFromStakeholderGroupCommandHandler : BaseCommandBundle, IBaseRequestHandler<RemoveUserFromStakeholderGroupCommand, Result<bool>>
{
    private readonly SMSStakeholderGroupService _stakeholderGroupService;
    private readonly ILogger<RemoveUserFromStakeholderGroupCommandHandler> _logger;

    public RemoveUserFromStakeholderGroupCommandHandler(
        SMSStakeholderGroupService stakeholderGroupService,
        ILogger<RemoveUserFromStakeholderGroupCommandHandler> logger)
    {
        _stakeholderGroupService = stakeholderGroupService ?? throw new ArgumentNullException(nameof(stakeholderGroupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(RemoveUserFromStakeholderGroupCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation(" Processing RemoveUserFromStakeholderGroupCommand for user: {UserCode} from group: {GroupCode}",
                request.UserCode, request.StakeholderGroupID.Value);

            if (string.IsNullOrWhiteSpace(request.UserCode) || string.IsNullOrWhiteSpace(request.StakeholderGroupID.Value))
            {
                _logger.LogApplicationWarning("RemoveUserFromStakeholderGroupCommand received with null or empty user/group code");
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.UserCodeRequired);
            }

            var result = await _stakeholderGroupService.RemoveUserFromGroupAsync(request.UserCode, request.StakeholderGroupID);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully removed user {UserCode} from group {GroupCode}", request.UserCode, request.StakeholderGroupID.Value);
            }
            else
            {
                _logger.LogApplicationError("Failed to remove user {UserCode} from group {GroupCode}, Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("RemoveUserFromStakeholderGroupCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing RemoveUserFromStakeholderGroupCommand for user: {UserCode} from group: {GroupCode}",
                ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.RemovalFailed);
        }
    }
}

/// <summary>
/// Command handler for clearing all user group memberships
/// </summary>
public class ClearUserStakeholderGroupsCommandHandler : BaseCommandBundle, IBaseRequestHandler<ClearUserStakeholderGroupsCommand, Result<bool>>
{
    private readonly SMSStakeholderGroupService _stakeholderGroupService;
    private readonly ILogger<ClearUserStakeholderGroupsCommandHandler> _logger;

    public ClearUserStakeholderGroupsCommandHandler(
        SMSStakeholderGroupService stakeholderGroupService,
        ILogger<ClearUserStakeholderGroupsCommandHandler> logger)
    {
        _stakeholderGroupService = stakeholderGroupService ?? throw new ArgumentNullException(nameof(stakeholderGroupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(ClearUserStakeholderGroupsCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation(" Processing ClearUserStakeholderGroupsCommand for user: {UserCode}", request.UserCode);

            if (string.IsNullOrWhiteSpace(request.UserCode))
            {
                _logger.LogApplicationWarning("ClearUserStakeholderGroupsCommand received with null or empty user code");
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.UserCodeRequired);
            }

            var result = await _stakeholderGroupService.ClearUserGroupMembershipsAsync(request.UserCode);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully cleared all group memberships for user: {UserCode}", request.UserCode);
            }
            else
            {
                _logger.LogApplicationError("Failed to clear group memberships for user {UserCode}, Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("ClearUserStakeholderGroupsCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing ClearUserStakeholderGroupsCommand for user: {UserCode}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.ClearGroupsFailed);
        }
    }
}

