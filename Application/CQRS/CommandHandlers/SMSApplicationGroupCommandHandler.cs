//-----------------------------------------------------------------------
// <copyright file="SMSApplicationGroupCommandHandler.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application layer component providing functionality for the SMS safety management system.
//                  Implements command handlers for processing write operations.
//                  Handles business logic execution and domain entity coordination.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using Microsoft.Extensions.Logging;

namespace SMS_Application.CommandHandlers;

/// <summary>
/// Command handler for creating SMS application groups
/// </summary>
public class CreateSMSApplicationGroupCommandHandler : BaseCommandBundle, IBaseRequestHandler<CreateSMSApplicationGroupCommand, Result<SMSApplicationGroup>>
{
    private readonly SMSApplicationGroupService _applicationGroupService;
    private readonly ILogger<CreateSMSApplicationGroupCommandHandler> _logger;

    public CreateSMSApplicationGroupCommandHandler(
        SMSApplicationGroupService applicationGroupService,
        ILogger<CreateSMSApplicationGroupCommandHandler> logger)
    {
        _applicationGroupService = applicationGroupService ?? throw new ArgumentNullException(nameof(applicationGroupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSApplicationGroup>> HandleAsync(CreateSMSApplicationGroupCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing CreateSMSApplicationGroupCommand for group: {GroupCode}", request.ApplicationGroup?.Code);

            if (request?.ApplicationGroup == null)
            {
                _logger.LogApplicationWarning("CreateSMSApplicationGroupCommand received with null application group");
                return Result<SMSApplicationGroup>.Failure<SMSApplicationGroup>(DomainErrors.SMSApplicationGroupError.NullOrEmpty);
            }

            var result = await _applicationGroupService.CreateSMSApplicationGroupAsync(request.ApplicationGroup, ct);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully created SMS application group: {GroupCode}", request.ApplicationGroup.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to create SMS application group: {GroupCode}, Error: {Error}",
                    request.ApplicationGroup.Code, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("CreateSMSApplicationGroupCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error processing CreateSMSApplicationGroupCommand for group: {GroupCode}",
                request.ApplicationGroup?.Code);
            return Result<SMSApplicationGroup>.Failure<SMSApplicationGroup>(DomainErrors.SMSApplicationGroupError.CreateFailed);
        }
    }
}

/// <summary>
/// Command handler for updating SMS application groups
/// </summary>
public class UpdateSMSApplicationGroupCommandHandler : BaseCommandBundle, IBaseRequestHandler<UpdateSMSApplicationGroupCommand, Result<SMSApplicationGroup>>
{
    private readonly SMSApplicationGroupService _applicationGroupService;
    private readonly ILogger<UpdateSMSApplicationGroupCommandHandler> _logger;

    public UpdateSMSApplicationGroupCommandHandler(
        SMSApplicationGroupService applicationGroupService,
        ILogger<UpdateSMSApplicationGroupCommandHandler> logger)
    {
        _applicationGroupService = applicationGroupService ?? throw new ArgumentNullException(nameof(applicationGroupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSApplicationGroup>> HandleAsync(UpdateSMSApplicationGroupCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing UpdateSMSApplicationGroupCommand for group: {GroupCode}", request.ApplicationGroup?.Code);

            if (request?.ApplicationGroup == null)
            {
                _logger.LogApplicationWarning("UpdateSMSApplicationGroupCommand received with null application group");
                return Result<SMSApplicationGroup>.Failure<SMSApplicationGroup>(DomainErrors.SMSApplicationGroupError.NullOrEmpty);
            }

            var result = await _applicationGroupService.UpdateSMSApplicationGroupAsync(request.ApplicationGroup, ct);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully updated SMS application group: {GroupCode}", request.ApplicationGroup.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to update SMS application group: {GroupCode}, Error: {Error}",
                    request.ApplicationGroup.Code, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("UpdateSMSApplicationGroupCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error processing UpdateSMSApplicationGroupCommand for group: {GroupCode}",
                request.ApplicationGroup?.Code);
            return Result<SMSApplicationGroup>.Failure<SMSApplicationGroup>(DomainErrors.SMSApplicationGroupError.UpdateFailed);
        }
    }
}

/// <summary>
/// Command handler for deleting SMS application groups
/// </summary>
public class DeleteSMSApplicationGroupCommandHandler : BaseCommandBundle, IBaseRequestHandler<DeleteSMSApplicationGroupCommand, Result<bool>>
{
    private readonly SMSApplicationGroupService _applicationGroupService;
    private readonly ILogger<DeleteSMSApplicationGroupCommandHandler> _logger;

    public DeleteSMSApplicationGroupCommandHandler(
        SMSApplicationGroupService applicationGroupService,
        ILogger<DeleteSMSApplicationGroupCommandHandler> logger)
    {
        _applicationGroupService = applicationGroupService ?? throw new ArgumentNullException(nameof(applicationGroupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteSMSApplicationGroupCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing DeleteSMSApplicationGroupCommand for group: {GroupCode}", request.GroupCode);

            if (string.IsNullOrWhiteSpace(request.GroupCode))
            {
                _logger.LogApplicationWarning("DeleteSMSApplicationGroupCommand received with null or empty group code");
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.CodeRequired);
            }

            var result = await _applicationGroupService.DeleteSMSApplicationGroupAsync(request.GroupCode, ct);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully deleted SMS application group: {GroupCode}", request.GroupCode);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete SMS application group: {GroupCode}, Error: {Error}",
                    request.GroupCode, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("DeleteSMSApplicationGroupCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error processing DeleteSMSApplicationGroupCommand for group: {GroupCode}", request.GroupCode);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.DeleteFailed);
        }
    }
}

/// <summary>
/// Command handler for assigning users to application groups
/// </summary>
public class AssignUserToApplicationGroupCommandHandler : BaseCommandBundle, IBaseRequestHandler<AssignUserToApplicationGroupCommand, Result<bool>>
{
    private readonly SMSApplicationGroupService _applicationGroupService;
    private readonly ILogger<AssignUserToApplicationGroupCommandHandler> _logger;

    public AssignUserToApplicationGroupCommandHandler(
        SMSApplicationGroupService applicationGroupService,
        ILogger<AssignUserToApplicationGroupCommandHandler> logger)
    {
        _applicationGroupService = applicationGroupService ?? throw new ArgumentNullException(nameof(applicationGroupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(AssignUserToApplicationGroupCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing AssignUserToApplicationGroupCommand for user: {UserCode} to group: {GroupCode}",
                request.UserCode, request.GroupCode);

            if (string.IsNullOrWhiteSpace(request.UserCode) || string.IsNullOrWhiteSpace(request.GroupCode))
            {
                _logger.LogApplicationWarning("AssignUserToApplicationGroupCommand received with null or empty user/group code");
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.UserCodeRequired);
            }

            var result = await _applicationGroupService.AssignUserToGroupAsync(request.UserCode, request.GroupCode, request.AssignedBy);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully assigned user {UserCode} to group {GroupCode}", request.UserCode, request.GroupCode);
            }
            else
            {
                _logger.LogApplicationError("Failed to assign user {UserCode} to group {GroupCode}, Error: {Error}",
                    request.UserCode, request.GroupCode, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("AssignUserToApplicationGroupCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error processing AssignUserToApplicationGroupCommand for user: {UserCode} to group: {GroupCode}",
                request.UserCode, request.GroupCode);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.AssignmentFailed);
        }
    }
}

/// <summary>
/// Command handler for removing users from application groups
/// </summary>
public class RemoveUserFromApplicationGroupCommandHandler : BaseCommandBundle, IBaseRequestHandler<RemoveUserFromApplicationGroupCommand, Result<bool>>
{
    private readonly SMSApplicationGroupService _applicationGroupService;
    private readonly ILogger<RemoveUserFromApplicationGroupCommandHandler> _logger;

    public RemoveUserFromApplicationGroupCommandHandler(
        SMSApplicationGroupService applicationGroupService,
        ILogger<RemoveUserFromApplicationGroupCommandHandler> logger)
    {
        _applicationGroupService = applicationGroupService ?? throw new ArgumentNullException(nameof(applicationGroupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(RemoveUserFromApplicationGroupCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing RemoveUserFromApplicationGroupCommand for user: {UserCode} from group: {GroupCode}",
                request.UserCode, request.GroupCode);

            if (string.IsNullOrWhiteSpace(request.UserCode) || string.IsNullOrWhiteSpace(request.GroupCode))
            {
                _logger.LogApplicationWarning("RemoveUserFromApplicationGroupCommand received with null or empty user/group code");
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.UserCodeRequired);
            }

            var result = await _applicationGroupService.RemoveUserFromGroupAsync(request.UserCode, request.GroupCode);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully removed user {UserCode} from group {GroupCode}", request.UserCode, request.GroupCode);
            }
            else
            {
                _logger.LogApplicationError("Failed to remove user {UserCode} from group {GroupCode}, Error: {Error}",
                    request.UserCode, request.GroupCode, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("RemoveUserFromApplicationGroupCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error processing RemoveUserFromApplicationGroupCommand for user: {UserCode} from group: {GroupCode}",
                request.UserCode, request.GroupCode);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.RemovalFailed);
        }
    }
}

/// <summary>
/// Command handler for clearing all user application group memberships
/// </summary>
public class ClearUserApplicationGroupsCommandHandler : BaseCommandBundle, IBaseRequestHandler<ClearUserApplicationGroupsCommand, Result<bool>>
{
    private readonly SMSApplicationGroupService _applicationGroupService;
    private readonly ILogger<ClearUserApplicationGroupsCommandHandler> _logger;

    public ClearUserApplicationGroupsCommandHandler(
        SMSApplicationGroupService applicationGroupService,
        ILogger<ClearUserApplicationGroupsCommandHandler> logger)
    {
        _applicationGroupService = applicationGroupService ?? throw new ArgumentNullException(nameof(applicationGroupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(ClearUserApplicationGroupsCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing ClearUserApplicationGroupsCommand for user: {UserCode}", request.UserCode);

            if (string.IsNullOrWhiteSpace(request.UserCode))
            {
                _logger.LogApplicationWarning("ClearUserApplicationGroupsCommand received with null or empty user code");
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.UserCodeRequired);
            }

            var result = await _applicationGroupService.ClearUserGroupsAsync(request.UserCode, ct);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully cleared all group memberships for user: {UserCode}", request.UserCode);
            }
            else
            {
                _logger.LogApplicationError("Failed to clear group memberships for user {UserCode}, Error: {Error}",
                    request.UserCode, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("ClearUserApplicationGroupsCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error processing ClearUserApplicationGroupsCommand for user: {UserCode}", request.UserCode);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.ClearGroupsFailed);
        }
    }
}

