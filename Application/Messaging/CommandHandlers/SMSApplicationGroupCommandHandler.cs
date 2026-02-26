//-----------------------------------------------------------------------
// <copyright file="SMSApplicationGroupCommandHandler.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application layer component providing functionality for the SMS safety management system.
//                  Implements command handlers for processing write operations.
//                  Handles business logic execution and domain entity coordination.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;

namespace SMS_Application.Messaging.CommandHandlers;

/// <summary>
/// Command handler for creating SMS stakeholder groups
/// </summary>
public class CreateSMSApplicationGroupCommandHandler : BaseCommandBundle, IRequestHandler<CreateSMSApplicationGroupCommand, Result<SMSApplicationGroup>>
{
    private readonly SMSApplicationGroupDataService _applicationGroupDataService;
    private readonly ILogger<CreateSMSApplicationGroupCommandHandler> _logger;

    public CreateSMSApplicationGroupCommandHandler(
        SMSApplicationGroupDataService applicationGroupDataService,
        ILogger<CreateSMSApplicationGroupCommandHandler> logger)
    {
        _applicationGroupDataService = applicationGroupDataService ?? throw new ArgumentNullException(nameof(applicationGroupDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSApplicationGroup>> HandleAsync(CreateSMSApplicationGroupCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing CreateSMSApplicationGroupCommand for group: {GroupCode}", request.ApplicationGroup?.Code);

            if (request?.ApplicationGroup == null)
            {
                _logger.LogWarning("CreateSMSApplicationGroupCommand received with null stakeholder group");
                return Result<SMSApplicationGroup>.Failure<SMSApplicationGroup>(DomainErrors.SMSApplicationGroupError.NullOrEmpty);
            }

            var result = await _applicationGroupDataService.CreateAsync(request.ApplicationGroup);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created SMS stakeholder group: {GroupCode}", request.ApplicationGroup.Code);
            }
            else
            {
                _logger.LogError("Failed to create SMS stakeholder group: {GroupCode}, Error: {Error}",
                    request.ApplicationGroup.Code, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CreateSMSApplicationGroupCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing CreateSMSApplicationGroupCommand for group: {GroupCode}",
                request.ApplicationGroup?.Code);
            return Result<SMSApplicationGroup>.Failure<SMSApplicationGroup>(DomainErrors.SMSApplicationGroupError.CreateFailed);
        }
    }
}

/// <summary>
/// Command handler for updating SMS stakeholder groups
/// </summary>
public class UpdateSMSApplicationGroupCommandHandler : BaseCommandBundle, IRequestHandler<UpdateSMSApplicationGroupCommand, Result<SMSApplicationGroup>>
{
    private readonly SMSApplicationGroupDataService _applicationGroupDataService;
    private readonly ILogger<UpdateSMSApplicationGroupCommandHandler> _logger;

    public UpdateSMSApplicationGroupCommandHandler(
        SMSApplicationGroupDataService applicationGroupDataService,
        ILogger<UpdateSMSApplicationGroupCommandHandler> logger)
    {
        _applicationGroupDataService = applicationGroupDataService ?? throw new ArgumentNullException(nameof(applicationGroupDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSApplicationGroup>> HandleAsync(UpdateSMSApplicationGroupCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing UpdateSMSApplicationGroupCommand for group: {GroupCode}", request.ApplicationGroup?.Code);

            if (request?.ApplicationGroup == null)
            {
                _logger.LogWarning("UpdateSMSApplicationGroupCommand received with null stakeholder group");
                return Result<SMSApplicationGroup>.Failure<SMSApplicationGroup>(DomainErrors.SMSApplicationGroupError.NullOrEmpty);
            }

            var result = await _applicationGroupDataService.UpdateAsync(request.ApplicationGroup);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated SMS stakeholder group: {GroupCode}", request.ApplicationGroup.Code);
            }
            else
            {
                _logger.LogError("Failed to update SMS stakeholder group: {GroupCode}, Error: {Error}",
                    request.ApplicationGroup.Code, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateSMSApplicationGroupCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing UpdateSMSApplicationGroupCommand for group: {GroupCode}",
                request.ApplicationGroup?.Code);
            return Result<SMSApplicationGroup>.Failure<SMSApplicationGroup>(DomainErrors.SMSApplicationGroupError.UpdateFailed);
        }
    }
}

/// <summary>
/// Command handler for deleting SMS stakeholder groups
/// </summary>
public class DeleteSMSApplicationGroupCommandHandler : BaseCommandBundle, IRequestHandler<DeleteSMSApplicationGroupCommand, Result<bool>>
{
    private readonly SMSApplicationGroupDataService _applicationGroupDataService;
    private readonly ILogger<DeleteSMSApplicationGroupCommandHandler> _logger;

    public DeleteSMSApplicationGroupCommandHandler(
        SMSApplicationGroupDataService applicationGroupDataService,
        ILogger<DeleteSMSApplicationGroupCommandHandler> logger)
    {
        _applicationGroupDataService = applicationGroupDataService ?? throw new ArgumentNullException(nameof(applicationGroupDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteSMSApplicationGroupCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing DeleteSMSApplicationGroupCommand for group: {GroupCode}", request.GroupCode);

            if (string.IsNullOrWhiteSpace(request.GroupCode))
            {
                _logger.LogWarning("DeleteSMSApplicationGroupCommand received with null or empty group code");
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.CodeRequired);
            }

            var result = await _applicationGroupDataService.DeleteAsync(request.GroupCode);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted SMS stakeholder group: {GroupCode}", request.GroupCode);
            }
            else
            {
                _logger.LogError("Failed to delete SMS stakeholder group: {GroupCode}, Error: {Error}",
                    request.GroupCode, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeleteSMSApplicationGroupCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing DeleteSMSApplicationGroupCommand for group: {GroupCode}", request.GroupCode);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.DeleteFailed);
        }
    }
}

/// <summary>
/// Command handler for assigning users to stakeholder groups
/// </summary>
public class AssignUserToApplicationGroupCommandHandler : BaseCommandBundle, IRequestHandler<AssignUserToApplicationGroupCommand, Result<bool>>
{
    private readonly SMSApplicationGroupDataService _applicationGroupDataService;
    private readonly ILogger<AssignUserToApplicationGroupCommandHandler> _logger;

    public AssignUserToApplicationGroupCommandHandler(
        SMSApplicationGroupDataService applicationGroupDataService,
        ILogger<AssignUserToApplicationGroupCommandHandler> logger)
    {
        _applicationGroupDataService = applicationGroupDataService ?? throw new ArgumentNullException(nameof(applicationGroupDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(AssignUserToApplicationGroupCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing AssignUserToApplicationGroupCommand for user: {UserCode} to group: {GroupCode}",
                request.UserCode, request.GroupCode);

            if (string.IsNullOrWhiteSpace(request.UserCode) || string.IsNullOrWhiteSpace(request.GroupCode))
            {
                _logger.LogWarning("AssignUserToApplicationGroupCommand received with null or empty user/group code");
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.UserCodeRequired);
            }

            var result = await _applicationGroupDataService.AssignUserToGroupAsync(request.UserCode, request.GroupCode, request.AssignedBy);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully assigned user {UserCode} to group {GroupCode}", request.UserCode, request.GroupCode);
            }
            else
            {
                _logger.LogError("Failed to assign user {UserCode} to group {GroupCode}, Error: {Error}",
                    request.UserCode, request.GroupCode, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("AssignUserToApplicationGroupCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AssignUserToApplicationGroupCommand for user: {UserCode} to group: {GroupCode}",
                request.UserCode, request.GroupCode);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.AssignmentFailed);
        }
    }
}

/// <summary>
/// Command handler for removing users from stakeholder groups
/// </summary>
public class RemoveUserFromApplicationGroupCommandHandler : BaseCommandBundle, IRequestHandler<RemoveUserFromApplicationGroupCommand, Result<bool>>
{
    private readonly SMSApplicationGroupDataService _applicationGroupDataService;
    private readonly ILogger<RemoveUserFromApplicationGroupCommandHandler> _logger;

    public RemoveUserFromApplicationGroupCommandHandler(
        SMSApplicationGroupDataService applicationGroupDataService,
        ILogger<RemoveUserFromApplicationGroupCommandHandler> logger)
    {
        _applicationGroupDataService = applicationGroupDataService ?? throw new ArgumentNullException(nameof(applicationGroupDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(RemoveUserFromApplicationGroupCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing RemoveUserFromApplicationGroupCommand for user: {UserCode} from group: {GroupCode}",
                request.UserCode, request.GroupCode);

            if (string.IsNullOrWhiteSpace(request.UserCode) || string.IsNullOrWhiteSpace(request.GroupCode))
            {
                _logger.LogWarning("RemoveUserFromApplicationGroupCommand received with null or empty user/group code");
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.UserCodeRequired);
            }

            var result = await _applicationGroupDataService.RemoveUserFromGroupAsync(request.UserCode, request.GroupCode);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully removed user {UserCode} from group {GroupCode}", request.UserCode, request.GroupCode);
            }
            else
            {
                _logger.LogError("Failed to remove user {UserCode} from group {GroupCode}, Error: {Error}",
                    request.UserCode, request.GroupCode, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("RemoveUserFromApplicationGroupCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing RemoveUserFromApplicationGroupCommand for user: {UserCode} from group: {GroupCode}",
                request.UserCode, request.GroupCode);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.RemovalFailed);
        }
    }
}

/// <summary>
/// Command handler for clearing all user group memberships
/// </summary>
public class ClearUserApplicationGroupsCommandHandler : BaseCommandBundle, IRequestHandler<ClearUserApplicationGroupsCommand, Result<bool>>
{
    private readonly SMSApplicationGroupDataService _applicationGroupDataService;
    private readonly ILogger<ClearUserApplicationGroupsCommandHandler> _logger;

    public ClearUserApplicationGroupsCommandHandler(
        SMSApplicationGroupDataService applicationGroupDataService,
        ILogger<ClearUserApplicationGroupsCommandHandler> logger)
    {
        _applicationGroupDataService = applicationGroupDataService ?? throw new ArgumentNullException(nameof(applicationGroupDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(ClearUserApplicationGroupsCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing ClearUserApplicationGroupsCommand for user: {UserCode}", request.UserCode);

            if (string.IsNullOrWhiteSpace(request.UserCode))
            {
                _logger.LogWarning("ClearUserApplicationGroupsCommand received with null or empty user code");
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.UserCodeRequired);
            }

            var result = await _applicationGroupDataService.ClearUserGroupsAsync(request.UserCode);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully cleared all group memberships for user: {UserCode}", request.UserCode);
            }
            else
            {
                _logger.LogError("Failed to clear group memberships for user {UserCode}, Error: {Error}",
                    request.UserCode, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("ClearUserApplicationGroupsCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ClearUserApplicationGroupsCommand for user: {UserCode}", request.UserCode);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.ClearGroupsFailed);
        }
    }
}
