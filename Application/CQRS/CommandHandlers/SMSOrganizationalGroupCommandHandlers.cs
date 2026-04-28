//-----------------------------------------------------------------------
// <copyright file="SMSOrganizationalGroupCommandHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command handlers implementing business logic for SMS write operations.
//                  Implements command handlers for processing write operations.
//                  Handles business logic execution and domain entity coordination.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// SMS ORGANIZATIONAL GROUP COMMAND HANDLERS - Clean Architecture Pattern
// =============================================

/// <summary>
/// Command handler for creating SMS organizational groups
/// </summary>
public class CreateSMSOrganizationalGroupCommandHandler : BaseCommandBundle, IBaseRequestHandler<CreateSMSOrganizationalGroupCommand, Result<SMSOrganizationalGroup>>
{
    private readonly ISMSOrganizationalGroupService _organizationalGroupService;
    private readonly ILogger<CreateSMSOrganizationalGroupCommandHandler> _logger;

    public CreateSMSOrganizationalGroupCommandHandler(
        ISMSOrganizationalGroupService organizationalGroupService,
        ILogger<CreateSMSOrganizationalGroupCommandHandler> logger)
    {
        _organizationalGroupService = organizationalGroupService ?? throw new ArgumentNullException(nameof(organizationalGroupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSOrganizationalGroup>> HandleAsync(CreateSMSOrganizationalGroupCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("✅ Clean Architecture: Processing CreateSMSOrganizationalGroupCommand for group: {GroupName}", request.OrganizationalGroup?.Name);

            var result = await _organizationalGroupService.CreateSMSOrganizationalGroupAsync(request.OrganizationalGroup, ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully created SMS organizational group: {GroupCode}", result.Value?.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to create SMS organizational group: {Error}", ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CreateSMSOrganizationalGroupCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing CreateSMSOrganizationalGroupCommand", ApplicationEventIds.Error, ex);
            return Result<SMSOrganizationalGroup>.Failure<SMSOrganizationalGroup>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

/// <summary>
/// Command handler for updating SMS organizational groups
/// </summary>
public class UpdateSMSOrganizationalGroupCommandHandler : BaseCommandBundle, IBaseRequestHandler<UpdateSMSOrganizationalGroupCommand, Result<SMSOrganizationalGroup>>
{
    private readonly ISMSOrganizationalGroupService _organizationalGroupService;
    private readonly ILogger<UpdateSMSOrganizationalGroupCommandHandler> _logger;

    public UpdateSMSOrganizationalGroupCommandHandler(
        ISMSOrganizationalGroupService organizationalGroupService,
        ILogger<UpdateSMSOrganizationalGroupCommandHandler> logger)
    {
        _organizationalGroupService = organizationalGroupService ?? throw new ArgumentNullException(nameof(organizationalGroupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSOrganizationalGroup>> HandleAsync(UpdateSMSOrganizationalGroupCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("✅ Clean Architecture: Processing UpdateSMSOrganizationalGroupCommand for group: {GroupCode}", request.OrganizationalGroup?.Code);

            var result = await _organizationalGroupService.UpdateSMSOrganizationalGroupAsync(request.OrganizationalGroup, ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully updated SMS organizational group: {GroupCode}", request.OrganizationalGroup?.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to update SMS organizational group: {Error}", ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateSMSOrganizationalGroupCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing UpdateSMSOrganizationalGroupCommand for group: {GroupCode}", ApplicationEventIds.Error, ex);
            return Result<SMSOrganizationalGroup>.Failure<SMSOrganizationalGroup>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

/// <summary>
/// Command handler for deleting SMS organizational groups
/// </summary>
public class DeleteSMSOrganizationalGroupCommandHandler : BaseCommandBundle, IBaseRequestHandler<DeleteSMSOrganizationalGroupCommand, Result<bool>>
{
    private readonly ISMSOrganizationalGroupService _organizationalGroupService;
    private readonly ILogger<DeleteSMSOrganizationalGroupCommandHandler> _logger;

    public DeleteSMSOrganizationalGroupCommandHandler(
        ISMSOrganizationalGroupService organizationalGroupService,
        ILogger<DeleteSMSOrganizationalGroupCommandHandler> logger)
    {
        _organizationalGroupService = organizationalGroupService ?? throw new ArgumentNullException(nameof(organizationalGroupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteSMSOrganizationalGroupCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("✅ Clean Architecture: Processing DeleteSMSOrganizationalGroupCommand for group: {GroupCode}", request.OrganizationalGroup?.Code);

            var result = await _organizationalGroupService.DeleteSMSOrganizationalGroupAsync(request.OrganizationalGroup.Code, ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully deleted SMS organizational group: {GroupCode}", request.OrganizationalGroup?.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete SMS organizational group: {Error}", ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeleteSMSOrganizationalGroupCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing DeleteSMSOrganizationalGroupCommand for group: {GroupCode}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

/// <summary>
/// Command handler for assigning users to organizational groups
/// </summary>
public class AssignUserToOrganizationalGroupCommandHandler : BaseCommandBundle, IBaseRequestHandler<AssignUserToOrganizationalGroupCommand, Result<bool>>
{
    private readonly ISMSOrganizationalGroupService _organizationalGroupService;
    private readonly ILogger<AssignUserToOrganizationalGroupCommandHandler> _logger;

    public AssignUserToOrganizationalGroupCommandHandler(
        ISMSOrganizationalGroupService organizationalGroupService,
        ILogger<AssignUserToOrganizationalGroupCommandHandler> logger)
    {
        _organizationalGroupService = organizationalGroupService ?? throw new ArgumentNullException(nameof(organizationalGroupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(AssignUserToOrganizationalGroupCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("✅ Clean Architecture: Processing AssignUserToOrganizationalGroupCommand for user: {UserCode} to group: {GroupId}",
                request.UserCode, request.GroupId);

            var result = await _organizationalGroupService.AssignUserToGroupAsync(request.UserCode, request.GroupId, request.AssignedBy, ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully assigned user {UserCode} to organizational group {GroupId}",
                    request.UserCode, request.GroupId);
            }
            else
            {
                _logger.LogApplicationError("Failed to assign user to organizational group: {Error}", ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("AssignUserToOrganizationalGroupCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing AssignUserToOrganizationalGroupCommand for user: {UserCode} to group: {GroupId}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

/// <summary>
/// Command handler for removing users from organizational groups
/// </summary>
public class RemoveUserFromOrganizationalGroupCommandHandler : BaseCommandBundle, IBaseRequestHandler<RemoveUserFromOrganizationalGroupCommand, Result<bool>>
{
    private readonly ISMSOrganizationalGroupService _organizationalGroupService;
    private readonly ILogger<RemoveUserFromOrganizationalGroupCommandHandler> _logger;

    public RemoveUserFromOrganizationalGroupCommandHandler(
        ISMSOrganizationalGroupService organizationalGroupService,
        ILogger<RemoveUserFromOrganizationalGroupCommandHandler> logger)
    {
        _organizationalGroupService = organizationalGroupService ?? throw new ArgumentNullException(nameof(organizationalGroupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(RemoveUserFromOrganizationalGroupCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("✅ Clean Architecture: Processing RemoveUserFromOrganizationalGroupCommand for user: {UserCode} from group: {GroupId}",
                request.UserCode, request.GroupId);

            var result = await _organizationalGroupService.RemoveUserFromGroupAsync(request.UserCode, request.GroupId.Value, ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully removed user {UserCode} from organizational group {GroupId}",
                    request.UserCode, request.GroupId);
            }
            else
            {
                _logger.LogApplicationError("Failed to remove user from organizational group: {Error}", ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("RemoveUserFromOrganizationalGroupCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing RemoveUserFromOrganizationalGroupCommand for user: {UserCode} from group: {GroupId}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

/// <summary>
/// Command handler for clearing user organizational group memberships
/// </summary>
public class ClearUserOrganizationalGroupsCommandHandler : BaseCommandBundle, IBaseRequestHandler<ClearUserOrganizationalGroupsCommand, Result<bool>>
{
    private readonly ISMSOrganizationalGroupService _organizationalGroupService;
    private readonly ILogger<ClearUserOrganizationalGroupsCommandHandler> _logger;

    public ClearUserOrganizationalGroupsCommandHandler(
        ISMSOrganizationalGroupService organizationalGroupService,
        ILogger<ClearUserOrganizationalGroupsCommandHandler> logger)
    {
        _organizationalGroupService = organizationalGroupService ?? throw new ArgumentNullException(nameof(organizationalGroupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(ClearUserOrganizationalGroupsCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("✅ Clean Architecture: Processing ClearUserOrganizationalGroupsCommand for user: {UserCode}", request.UserCode);

            var result = await _organizationalGroupService.ClearUserGroupsAsync(request.UserCode, ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully cleared organizational group memberships for user {UserCode}", request.UserCode);
            }
            else
            {
                _logger.LogApplicationError("Failed to clear user organizational group memberships: {Error}", ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("ClearUserOrganizationalGroupsCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing ClearUserOrganizationalGroupsCommand for user: {UserCode}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}
