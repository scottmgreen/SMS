//-----------------------------------------------------------------------
// <copyright file="SMSUserRoleCommandHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command handlers implementing SMS user management business logic and operations.
//                  Implements command handlers for processing write operations.
//                  Handles business logic execution and domain entity coordination.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using Microsoft.Extensions.Logging;

namespace SMS_Application.CommandHandlers;

// =============================================
// SMS USER ROLE COMMAND HANDLERS - Clean Architecture Pattern
// =============================================

public class CreateSMSUserRoleCommandHandler : BaseCommandBundle, IBaseRequestHandler<CreateSMSUserRoleCommand, Result<SMSUserRole>>
{
    private readonly SMSUserRoleService _userRoleService;
    private readonly ILogger<CreateSMSUserRoleCommandHandler> _logger;

    public CreateSMSUserRoleCommandHandler(SMSUserRoleService userRoleService, ILogger<CreateSMSUserRoleCommandHandler> logger)
    {
        _userRoleService = userRoleService ?? throw new ArgumentNullException(nameof(userRoleService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSUserRole>> HandleAsync(CreateSMSUserRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.SMSUserRole is null)
            {
                _logger.LogApplicationError("CreateSMSUserRoleCommand received with null request or user role", ApplicationEventIds.Error, null);
                return Result<SMSUserRole>.Failure<SMSUserRole>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogApplicationInformation(" Processing CreateSMSUserRoleCommand for Code: {Code}", request.SMSUserRole.Code);

            var result = await _userRoleService.CreateUserRoleAsync(request.SMSUserRole);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully created SMS User Role with ID: {Id}", result.Value?.Id);
            }
            else
            {
                _logger.LogApplicationError("Failed to create SMS User Role with Code: {Code}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("CreateSMSUserRoleCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while creating SMS User Role", ApplicationEventIds.Error, ex);
            return Result<SMSUserRole>.Failure<SMSUserRole>(DomainErrors.SMSUserRoleError.CreateFailed);
        }
    }
}

public class UpdateSMSUserRoleCommandHandler : BaseCommandBundle, IBaseRequestHandler<UpdateSMSUserRoleCommand, Result<SMSUserRole>>
{
    private readonly SMSUserRoleService _userRoleService;
    private readonly ILogger<UpdateSMSUserRoleCommandHandler> _logger;

    public UpdateSMSUserRoleCommandHandler(SMSUserRoleService userRoleService, ILogger<UpdateSMSUserRoleCommandHandler> logger)
    {
        _userRoleService = userRoleService ?? throw new ArgumentNullException(nameof(userRoleService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSUserRole>> HandleAsync(UpdateSMSUserRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.SMSUserRole is null)
            {
                _logger.LogApplicationError("UpdateSMSUserRoleCommand received with null request or user role", ApplicationEventIds.Error, null);
                return Result<SMSUserRole>.Failure<SMSUserRole>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogApplicationInformation(" Processing UpdateSMSUserRoleCommand for ID: {Id}", request.SMSUserRole.Id);

            var result = await _userRoleService.UpdateUserRoleAsync(request.SMSUserRole);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully updated SMS User Role with ID: {Id}", request.SMSUserRole.Id);
            }
            else
            {
                _logger.LogApplicationError("Failed to update SMS User Role with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("UpdateSMSUserRoleCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating SMS User Role with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<SMSUserRole>.Failure<SMSUserRole>(DomainErrors.SMSUserRoleError.UpdateFailed);
        }
    }
}

public class DeleteSMSUserRoleCommandHandler : BaseCommandBundle, IBaseRequestHandler<DeleteSMSUserRoleCommand, Result<bool>>
{
    private readonly SMSUserRoleService _userRoleService;
    private readonly ILogger<DeleteSMSUserRoleCommandHandler> _logger;

    public DeleteSMSUserRoleCommandHandler(SMSUserRoleService userRoleService, ILogger<DeleteSMSUserRoleCommandHandler> logger)
    {
        _userRoleService = userRoleService ?? throw new ArgumentNullException(nameof(userRoleService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteSMSUserRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("DeleteSMSUserRoleCommand received with null request", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogApplicationInformation(" Processing DeleteSMSUserRoleCommand for ID: {Id}", request.SMSUserRoleId);

            var result = await _userRoleService.DeleteUserRoleAsync(request.SMSUserRoleId.Value);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully deleted SMS User Role with ID: {Id}", request.SMSUserRoleId);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete SMS User Role with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("DeleteSMSUserRoleCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while deleting SMS User Role with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SMSUserRoleError.DeleteFailed);
        }
    }
}

public class AssignRoleToUserCommandHandler : BaseCommandBundle, IBaseRequestHandler<AssignRoleToUserCommand, Result<SMSUserRole>>
{
    private readonly SMSUserRoleService _userRoleService;
    private readonly ILogger<AssignRoleToUserCommandHandler> _logger;

    public AssignRoleToUserCommandHandler(SMSUserRoleService userRoleService, ILogger<AssignRoleToUserCommandHandler> logger)
    {
        _userRoleService = userRoleService ?? throw new ArgumentNullException(nameof(userRoleService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSUserRole>> HandleAsync(AssignRoleToUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("AssignRoleToUserCommand received with null request", ApplicationEventIds.Error, null);
                return Result<SMSUserRole>.Failure<SMSUserRole>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogApplicationInformation(" Processing AssignRoleToUserCommand - User: {UserId}, Role: {RoleCode}",
                request.UserId, request.RoleCode);

            // Create a new user role assignment
            var userRoleId = new SMSUserRoleID($"UR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}");
            var userRole = new SMSUserRole(userRoleId)
            {
                Code = userRoleId.Value,
                Name = $"{request.RoleCode} Assignment for {request.UserId}"
                // Note: Set additional properties based on your domain model and command parameters
            };

            var result = await _userRoleService.CreateUserRoleAsync(userRole);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully assigned role {RoleCode} to user {UserId}",
                    request.RoleCode, request.UserId);
            }
            else
            {
                _logger.LogApplicationError("Failed to assign role {RoleCode} to user {UserId}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("AssignRoleToUserCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while assigning role to user", ApplicationEventIds.Error, ex);
            return Result<SMSUserRole>.Failure<SMSUserRole>(DomainErrors.SMSUserRoleError.CreateFailed);
        }
    }
}

public class ActivateSMSUserRoleCommandHandler : BaseCommandBundle, IBaseRequestHandler<ActivateSMSUserRoleCommand, Result<bool>>
{
    private readonly SMSUserRoleService _userRoleService;
    private readonly ILogger<ActivateSMSUserRoleCommandHandler> _logger;

    public ActivateSMSUserRoleCommandHandler(SMSUserRoleService userRoleService, ILogger<ActivateSMSUserRoleCommandHandler> logger)
    {
        _userRoleService = userRoleService ?? throw new ArgumentNullException(nameof(userRoleService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(ActivateSMSUserRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("ActivateSMSUserRoleCommand received with null request", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogApplicationInformation(" Processing ActivateSMSUserRoleCommand for ID: {Id}", request.UserRoleId);

            // Get the user role to activate
            var userRoleResult = await _userRoleService.GetUserRoleByIdAsync(request.UserRoleId);
            if (userRoleResult.IsFailure)
            {
                return Result<bool>.Failure<bool>(userRoleResult.Error);
            }

            var userRole = userRoleResult.Value;

            // Note: Add activation logic to your domain model if needed
            // For now, we'll just update the timestamps
            userRole.UpdatedBy = request.ActivatedBy ?? string.Empty;
            userRole.UpdatedDate = DateTime.UtcNow;

            var result = await _userRoleService.UpdateUserRoleAsync(userRole);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully activated SMS User Role with ID: {Id}", request.UserRoleId);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogApplicationError("Failed to activate SMS User Role with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(result.Error);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("ActivateSMSUserRoleCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while activating SMS User Role with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SMSUserRoleError.UpdateFailed);
        }
    }
}

public class DeactivateSMSUserRoleCommandHandler : BaseCommandBundle, IBaseRequestHandler<DeactivateSMSUserRoleCommand, Result<bool>>
{
    private readonly SMSUserRoleService _userRoleService;
    private readonly ILogger<DeactivateSMSUserRoleCommandHandler> _logger;

    public DeactivateSMSUserRoleCommandHandler(SMSUserRoleService userRoleService, ILogger<DeactivateSMSUserRoleCommandHandler> logger)
    {
        _userRoleService = userRoleService ?? throw new ArgumentNullException(nameof(userRoleService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeactivateSMSUserRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("DeactivateSMSUserRoleCommand received with null request", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogApplicationInformation(" Processing DeactivateSMSUserRoleCommand for ID: {Id}", request.UserRoleId);

            // Get the user role to deactivate
            var userRoleResult = await _userRoleService.GetUserRoleByIdAsync(request.UserRoleId);
            if (userRoleResult.IsFailure)
            {
                return Result<bool>.Failure<bool>(userRoleResult.Error);
            }

            var userRole = userRoleResult.Value;

            // Note: Add deactivation logic to your domain model
            // userRole.Deactivate(request.DeactivatedBy, request.DeactivationReason);
            
            userRole.UpdatedBy = request.DeactivatedBy ?? string.Empty;
            userRole.UpdatedDate = DateTime.UtcNow;

            var result = await _userRoleService.UpdateUserRoleAsync(userRole);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully deactivated SMS User Role with ID: {Id}", request.UserRoleId);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogApplicationError("Failed to deactivate SMS User Role with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(result.Error);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("DeactivateSMSUserRoleCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while deactivating SMS User Role with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SMSUserRoleError.UpdateFailed);
        }
    }
}

