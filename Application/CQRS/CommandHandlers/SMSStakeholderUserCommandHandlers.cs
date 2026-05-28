//-----------------------------------------------------------------------
// <copyright file="SMSStakeholderUserCommandHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command handlers implementing SMS user management business logic and operations.
//                  Implements command handlers for processing write operations.
//                  Handles business logic execution and domain entity coordination.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;
using SMS_Infrastructure.Services;

namespace SMS_Application.CommandHandlers;

// =============================================
// SMS STAKEHOLDER USER COMMAND HANDLERS - Clean Architecture Pattern
// =============================================

public class CreateSMSStakeholderUserCommandHandler : BaseCommandBundle, IBaseRequestHandler<CreateSMSStakeholderUserCommand, Result<SMSStakeholderUser>>
{
    private readonly ISMSStakeholderUserService _stakeholderUserService;
    private readonly ILogger<CreateSMSStakeholderUserCommandHandler> _logger;

    public CreateSMSStakeholderUserCommandHandler(ISMSStakeholderUserService stakeholderUserService, ILogger<CreateSMSStakeholderUserCommandHandler> logger)
    {
        _stakeholderUserService = stakeholderUserService ?? throw new ArgumentNullException(nameof(stakeholderUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSStakeholderUser>> HandleAsync(CreateSMSStakeholderUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.SMSStakeholderUser is null)
            {
                _logger.LogApplicationError("CreateSMSStakeholderUserCommand received with null request or user", ApplicationEventIds.Error, null);
                return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.NullOrEmpty);
            }

            _logger.LogApplicationInformation(" Processing CreateSMSStakeholderUserCommand for UserName: {UserName}", request.SMSStakeholderUser.UserName);

            var result = await _stakeholderUserService.CreateSMSStakeholderUserAsync(request.SMSStakeholderUser, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully created SMS Stakeholder User with ID: {Id}, UserName: {UserName}",
                    result.Value?.UserId, result.Value?.UserName);
            }
            else
            {
                _logger.LogApplicationError("Failed to create SMS Stakeholder User with UserName: {UserName}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("CreateSMSStakeholderUserCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while creating SMS Stakeholder User", ApplicationEventIds.Error, ex);
            return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.CreateFailed);
        }
    }
}

public class UpdateSMSStakeholderUserCommandHandler : BaseCommandBundle, IBaseRequestHandler<UpdateSMSStakeholderUserCommand, Result<SMSStakeholderUser>>
{
    private readonly ISMSStakeholderUserService _stakeholderUserService;
    private readonly ILogger<UpdateSMSStakeholderUserCommandHandler> _logger;

    public UpdateSMSStakeholderUserCommandHandler(ISMSStakeholderUserService stakeholderUserService, ILogger<UpdateSMSStakeholderUserCommandHandler> logger)
    {
        _stakeholderUserService = stakeholderUserService ?? throw new ArgumentNullException(nameof(stakeholderUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSStakeholderUser>> HandleAsync(UpdateSMSStakeholderUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.SMSStakeholderUser is null)
            {
                _logger.LogApplicationError("UpdateSMSStakeholderUserCommand received with null request or user", ApplicationEventIds.Error, null);
                return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.NullOrEmpty);
            }

            _logger.LogApplicationInformation(" Processing UpdateSMSStakeholderUserCommand for UserID: {UserId}", request.SMSStakeholderUser.UserId);

            var result = await _stakeholderUserService.UpdateSMSStakeholderUserAsync(request.SMSStakeholderUser, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully updated SMS Stakeholder User with ID: {UserId}", request.SMSStakeholderUser.UserId);
            }
            else
            {
                _logger.LogApplicationError("Failed to update SMS Stakeholder User with ID: {UserId}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("UpdateSMSStakeholderUserCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating SMS Stakeholder User with ID: {UserId}", ApplicationEventIds.Error, ex);
            return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.UpdateFailed);
        }
    }
}

public class UpdateSMSStakeholderUserPasswordCommandHandler : BaseCommandBundle, IBaseRequestHandler<UpdateSMSStakeholderUserPasswordCommand, Result<bool>>
{
    private readonly ISMSStakeholderUserService _stakeholderUserService;
    private readonly ILogger<UpdateSMSStakeholderUserPasswordCommandHandler> _logger;

    public UpdateSMSStakeholderUserPasswordCommandHandler(ISMSStakeholderUserService stakeholderUserService, ILogger<UpdateSMSStakeholderUserPasswordCommandHandler> logger)
    {
        _stakeholderUserService = stakeholderUserService ?? throw new ArgumentNullException(nameof(stakeholderUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(UpdateSMSStakeholderUserPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("UpdateSMSStakeholderUserPasswordCommand received with null request", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.NullOrEmpty);
            }

            _logger.LogApplicationInformation(" Processing UpdateSMSStakeholderUserPasswordCommand for UserID: {UserId}", request.UserId);

            // Create new password with proper hashing
            var passwordResult = Password.Create(request.NewPassword);
            if (passwordResult.IsFailure)
                return Result<bool>.Failure<bool>(passwordResult.Error);

            // FIXED: Access the data service directly to use UpdateSMSStakeholderUserPasswordAsync
            // This calls the dedicated pr_SMSStakeholderUser_UpdatePassword stored procedure
            var dataServiceField = _stakeholderUserService.GetType().GetField("_dataService", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (dataServiceField?.GetValue(_stakeholderUserService) is not SMSStakeholderUserDataService dataService)
            {
                _logger.LogApplicationError("Could not access SMSStakeholderUserDataService for password update", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.PasswordUpdateFailed);
            }

            var userId = new SMSStakeholderUserID(request.UserId);
            var result = await dataService.UpdateSMSStakeholderUserPasswordAsync(userId, passwordResult.Value.HashedValue, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully updated password for SMS Stakeholder User with ID: {UserId}", request.UserId);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogApplicationError("Failed to update password for SMS Stakeholder User with ID: {UserId}. Error: {Error}",
                    ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(result.Error);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("UpdateSMSStakeholderUserPasswordCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating password for SMS Stakeholder User with ID: {UserId}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.PasswordUpdateFailed);
        }
    }
}

public class AuthenticateSMSStakeholderUserCommandHandler : BaseCommandBundle, IBaseRequestHandler<AuthenticateSMSStakeholderUserCommand, Result<bool>>
{
    private readonly ISMSStakeholderUserService _stakeholderUserService;
    private readonly ILogger<AuthenticateSMSStakeholderUserCommandHandler> _logger;

    public AuthenticateSMSStakeholderUserCommandHandler(ISMSStakeholderUserService stakeholderUserService, ILogger<AuthenticateSMSStakeholderUserCommandHandler> logger)
    {
        _stakeholderUserService = stakeholderUserService ?? throw new ArgumentNullException(nameof(stakeholderUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(AuthenticateSMSStakeholderUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("AuthenticateSMSStakeholderUserCommand received with null request", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.NullOrEmpty);
            }

            _logger.LogApplicationInformation(" Processing authentication request for UserName: {UserName}", request.UserName);

            var result = await _stakeholderUserService.AuthenticateSMSStakeholderUserAsync(request.UserName, request.Password, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully authenticated SMS Stakeholder User: {UserName}", request.UserName);
            }
            else
            {
                _logger.LogApplicationWarning("Authentication failed for user: {UserName}", request.UserName);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("AuthenticateSMSStakeholderUserCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred during authentication for user: {UserName}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.LoginFailed);
        }
    }
}

public class RecordSMSStakeholderUserLoginCommandHandler : BaseCommandBundle, IBaseRequestHandler<RecordSMSStakeholderUserLoginCommand, Result<bool>>
{
    private readonly ISMSStakeholderUserService _stakeholderUserService;
    private readonly ILogger<RecordSMSStakeholderUserLoginCommandHandler> _logger;

    public RecordSMSStakeholderUserLoginCommandHandler(ISMSStakeholderUserService stakeholderUserService, ILogger<RecordSMSStakeholderUserLoginCommandHandler> logger)
    {
        _stakeholderUserService = stakeholderUserService ?? throw new ArgumentNullException(nameof(stakeholderUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(RecordSMSStakeholderUserLoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("RecordSMSStakeholderUserLoginCommand received with null request", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.NullOrEmpty);
            }

            _logger.LogApplicationInformation(" Processing RecordSMSStakeholderUserLoginCommand for UserID: {UserId}", request.UserId);

            // Get the existing user and record login
            var userResult = await _stakeholderUserService.GetSMSStakeholderUserByCodeAsync(request.UserId, cancellationToken);
            if (userResult.IsFailure)
            {
                return Result<bool>.Failure<bool>(userResult.Error);
            }

            var user = userResult.Value;
            user.RecordLogin();

            var updateResult = await _stakeholderUserService.UpdateSMSStakeholderUserAsync(user, cancellationToken);

            if (updateResult.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully recorded login for SMS Stakeholder User with ID: {UserId}", request.UserId);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogApplicationError("Failed to record login for SMS Stakeholder User with ID: {UserId}. Error: {Error}",
                    ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(updateResult.Error);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("RecordSMSStakeholderUserLoginCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while recording login for SMS Stakeholder User with ID: {UserId}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.UpdateFailed);
        }
    }
}

/// <summary>
/// ? NEW: Command handler for deactivating SMS Stakeholder Users using CQRS/Mediator pattern
/// Implements proper CQRS pattern with audit pipeline support for soft delete operations
/// </summary>
public class DeactivateSMSStakeholderUserCommandHandler : BaseCommandBundle, IBaseRequestHandler<DeactivateSMSStakeholderUserCommand, Result<SMSStakeholderUser>>
{
    private readonly ISMSStakeholderUserService _stakeholderUserService;
    private readonly ILogger<DeactivateSMSStakeholderUserCommandHandler> _logger;

    public DeactivateSMSStakeholderUserCommandHandler(ISMSStakeholderUserService stakeholderUserService, ILogger<DeactivateSMSStakeholderUserCommandHandler> logger)
    {
        _stakeholderUserService = stakeholderUserService ?? throw new ArgumentNullException(nameof(stakeholderUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSStakeholderUser>> HandleAsync(DeactivateSMSStakeholderUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.SMSStakeholderUser is null)
            {
                _logger.LogApplicationError("DeactivateSMSStakeholderUserCommand received with null request or user", ApplicationEventIds.Error, null);
                return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.NullOrEmpty);
            }

            _logger.LogApplicationInformation("CQRS: Handling DeactivateSMSStakeholderUserCommand for user: {UserCode} - Reason: {Reason}", 
                request.SMSStakeholderUser.Code, request.DeactivationReason);

            // Business logic: Deactivate the user
            request.SMSStakeholderUser.Deactivate();

            // Use the update service method to persist the deactivation
            // The audit pipeline has already set UpdatedBy and UpdatedDate via SetUpdatedBy()
            var result = await _stakeholderUserService.UpdateSMSStakeholderUserAsync(request.SMSStakeholderUser, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("CQRS: Successfully deactivated SMS Stakeholder User: {UserCode}", result.Value.Code);
            }
            else
            {
                _logger.LogApplicationError("CQRS: Failed to deactivate SMS Stakeholder User. Error: {Error}", result.Error?.Message);
            }
            
            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("DeactivateSMSStakeholderUserCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "CQRS: Exception in DeactivateSMSStakeholderUserCommandHandler for user: {UserCode}", 
                request.SMSStakeholderUser?.Code);
            return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.DeleteFailed);
        }
    }
}

public class DeleteSMSStakeholderUserCommandHandler : BaseCommandBundle, IBaseRequestHandler<DeleteSMSStakeholderUserCommand, Result<bool>>
{
    private readonly ISMSStakeholderUserService _stakeholderUserService;
    private readonly IBaseMediator _mediator;
    private readonly ILogger<DeleteSMSStakeholderUserCommandHandler> _logger;

    public DeleteSMSStakeholderUserCommandHandler(
        ISMSStakeholderUserService stakeholderUserService, 
        IBaseMediator mediator,
        ILogger<DeleteSMSStakeholderUserCommandHandler> logger)
    {
        _stakeholderUserService = stakeholderUserService ?? throw new ArgumentNullException(nameof(stakeholderUserService));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteSMSStakeholderUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("DeleteSMSStakeholderUserCommand received with null request", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.NullOrEmpty);
            }

            _logger.LogApplicationInformation("CQRS: Handling DeleteSMSStakeholderUserCommand (legacy) for user ID: {UserId} - Redirecting to deactivation", 
                request.SMSStakeholderUserId.Value);

            // Get the user first - ? FIXED: Use correct method name
            var userResult = await _stakeholderUserService.GetSMSStakeholderUserByCodeAsync(request.SMSStakeholderUserId.Value, cancellationToken);
            if (userResult.IsFailure)
            {
                _logger.LogApplicationWarning("CQRS: Cannot delete non-existent SMS Stakeholder User with ID: {UserId}", request.SMSStakeholderUserId.Value);
                return Result<bool>.Failure<bool>(userResult.Error);
            }

            // Use the new deactivation command for consistency
            var deactivateCommand = new DeactivateSMSStakeholderUserCommand(userResult.Value, "Deleted via legacy delete command");
            var deactivateResult = await _mediator.SendAsync(deactivateCommand, cancellationToken);

            if (deactivateResult.IsSuccess)
            {
                _logger.LogApplicationInformation("CQRS: Successfully processed delete as deactivation for user: {UserCode}", deactivateResult.Value.Code);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogApplicationError("CQRS: Failed to process delete as deactivation. Error: {Error}", deactivateResult.Error?.Message);
                return Result<bool>.Failure<bool>(deactivateResult.Error);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("DeleteSMSStakeholderUserCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while deleting SMS Stakeholder User with ID: {UserId}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.DeleteFailed);
        }
    }
}


