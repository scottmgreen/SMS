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

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// SMS STAKEHOLDER GROUP COMMAND HANDLERS - Clean Architecture Pattern
// =============================================

/// <summary>
/// Command handler for creating SMS stakeholder groups
/// </summary>
public class CreateSMSStakeholderGroupCommandHandler : BaseCommandBundle, IRequestHandler<CreateSMSStakeholderGroupCommand, Result<SMSStakeholderGroup>>
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
            _logger.LogInformation("✅ Clean Architecture: Processing CreateSMSStakeholderGroupCommand for group: {GroupCode}", request.StakeholderGroup?.Code);

            if (request?.StakeholderGroup == null)
            {
                _logger.LogWarning("CreateSMSStakeholderGroupCommand received with null stakeholder group");
                return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.SMSStakeholderGroupError.NullOrEmpty);
            }

            var result = await _stakeholderGroupService.CreateStakeholderGroupAsync(
                request.StakeholderGroup.Name,
                request.StakeholderGroup.Description,
                request.StakeholderGroup.CreatedBy ?? "SYSTEM");

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully created SMS stakeholder group: {GroupCode}", request.StakeholderGroup.Code);
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
            _logger.LogWarning("CreateSMSStakeholderGroupCommand operation was cancelled");
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
public class UpdateSMSStakeholderGroupCommandHandler : BaseCommandBundle, IRequestHandler<UpdateSMSStakeholderGroupCommand, Result<SMSStakeholderGroup>>
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
            _logger.LogInformation("✅ Clean Architecture: Processing UpdateSMSStakeholderGroupCommand for group: {GroupCode}", request.StakeholderGroup?.Code);

            if (request?.StakeholderGroup == null)
            {
                _logger.LogWarning("UpdateSMSStakeholderGroupCommand received with null stakeholder group");
                return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.SMSStakeholderGroupError.NullOrEmpty);
            }

            var result = await _stakeholderGroupService.UpdateStakeholderGroupAsync(
                request.StakeholderGroup.Code,
                request.StakeholderGroup.Name,
                request.StakeholderGroup.Description,
                request.StakeholderGroup.UpdatedBy ?? "SYSTEM");

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully updated SMS stakeholder group: {GroupCode}", request.StakeholderGroup.Code);
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
            _logger.LogWarning("UpdateSMSStakeholderGroupCommand operation was cancelled");
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
public class DeleteSMSStakeholderGroupCommandHandler : BaseCommandBundle, IRequestHandler<DeleteSMSStakeholderGroupCommand, Result<bool>>
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
            _logger.LogInformation("✅ Clean Architecture: Processing DeleteSMSStakeholderGroupCommand for group: {GroupCode}", request.StakeholderGroup.Code);

            if (string.IsNullOrWhiteSpace(request.StakeholderGroup.Code))
            {
                _logger.LogWarning("DeleteSMSStakeholderGroupCommand received with null or empty group code");
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.CodeRequired);
            }

            var result = await _stakeholderGroupService.DeleteStakeholderGroupAsync(request.StakeholderGroup);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully deleted SMS stakeholder group: {GroupCode}", request.StakeholderGroup.Code);
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
            _logger.LogWarning("DeleteSMSStakeholderGroupCommand operation was cancelled");
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
public class AssignUserToStakeholderGroupCommandHandler : BaseCommandBundle, IRequestHandler<AssignUserToStakeholderGroupCommand, Result<bool>>
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
            _logger.LogInformation("✅ Clean Architecture: Processing AssignUserToStakeholderGroupCommand for user: {UserCode} to group: {GroupCode}",
                request.UserCode, request.StakeholderGroupID);

            if (string.IsNullOrWhiteSpace(request.UserCode) || string.IsNullOrWhiteSpace(request.StakeholderGroupID.Value))
            {
                _logger.LogWarning("AssignUserToStakeholderGroupCommand received with null or empty user/group code");
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.UserCodeRequired);
            }

            var result = await _stakeholderGroupService.AssignUserToGroupAsync(request.UserCode, request.StakeholderGroupID, request.AssignedBy);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully assigned user {UserCode} to group {GroupCode}", request.UserCode, request.StakeholderGroupID.Value);
            }
            else
            {
                _logger.LogApplicationError("Failed to assign user {UserCode} to group {GroupCode}, Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("AssignUserToStakeholderGroupCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing AssignUserToStakeholderGroupCommand for user: {UserCode} to group: {GroupCode}",
                ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.AssignmentFailed);
        }
    }
}

/// <summary>
/// Command handler for removing users from stakeholder groups
/// </summary>
public class RemoveUserFromStakeholderGroupCommandHandler : BaseCommandBundle, IRequestHandler<RemoveUserFromStakeholderGroupCommand, Result<bool>>
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
            _logger.LogInformation("✅ Clean Architecture: Processing RemoveUserFromStakeholderGroupCommand for user: {UserCode} from group: {GroupCode}",
                request.UserCode, request.StakeholderGroupID.Value);

            if (string.IsNullOrWhiteSpace(request.UserCode) || string.IsNullOrWhiteSpace(request.StakeholderGroupID.Value))
            {
                _logger.LogWarning("RemoveUserFromStakeholderGroupCommand received with null or empty user/group code");
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.UserCodeRequired);
            }

            var result = await _stakeholderGroupService.RemoveUserFromGroupAsync(request.UserCode, request.StakeholderGroupID);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully removed user {UserCode} from group {GroupCode}", request.UserCode, request.StakeholderGroupID.Value);
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
            _logger.LogWarning("RemoveUserFromStakeholderGroupCommand operation was cancelled");
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
public class ClearUserStakeholderGroupsCommandHandler : BaseCommandBundle, IRequestHandler<ClearUserStakeholderGroupsCommand, Result<bool>>
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
            _logger.LogInformation("✅ Clean Architecture: Processing ClearUserStakeholderGroupsCommand for user: {UserCode}", request.UserCode);

            if (string.IsNullOrWhiteSpace(request.UserCode))
            {
                _logger.LogWarning("ClearUserStakeholderGroupsCommand received with null or empty user code");
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.UserCodeRequired);
            }

            var result = await _stakeholderGroupService.ClearUserGroupMembershipsAsync(request.UserCode);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully cleared all group memberships for user: {UserCode}", request.UserCode);
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
            _logger.LogWarning("ClearUserStakeholderGroupsCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing ClearUserStakeholderGroupsCommand for user: {UserCode}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.ClearGroupsFailed);
        }
    }
}
