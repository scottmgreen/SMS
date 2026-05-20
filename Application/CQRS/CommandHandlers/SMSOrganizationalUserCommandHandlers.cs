//-----------------------------------------------------------------------
// <copyright file="SMSOrganizationalUserCommandHandlers.cs" company="SMS Safety Management System">
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

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// SMS ORGANIZATIONAL USER COMMAND HANDLERS - Clean Architecture Pattern
// =============================================

public class CreateSMSOrganizationalUserCommandHandler : BaseCommandBundle, IBaseRequestHandler<CreateSMSOrganizationalUserCommand, Result<SMSOrganizationalUser>>
{
    private readonly ISMSOrganizationalUserService _organizationalUserService;
    private readonly ILogger<CreateSMSOrganizationalUserCommandHandler> _logger;

    public CreateSMSOrganizationalUserCommandHandler(ISMSOrganizationalUserService organizationalUserService, ILogger<CreateSMSOrganizationalUserCommandHandler> logger)
    {
        _organizationalUserService = organizationalUserService ?? throw new ArgumentNullException(nameof(organizationalUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSOrganizationalUser>> HandleAsync(CreateSMSOrganizationalUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.SMSOrganizationalUser is null)
            {
                _logger.LogApplicationError("CreateSMSOrganizationalUserCommand received with null request or user", ApplicationEventIds.Error, null);
                return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.NullOrEmpty);
            }

            _logger.LogInformation(" Processing CreateSMSOrganizationalUserCommand for UserName: {UserName}", request.SMSOrganizationalUser.UserName);

            var result = await _organizationalUserService.CreateSMSOrganizationalUserAsync(request.SMSOrganizationalUser, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation(" Successfully created SMS Organizational User with ID: {Id}, UserName: {UserName}",
                    result.Value?.UserId, result.Value?.UserName);
            }
            else
            {
                _logger.LogApplicationError("Failed to create SMS Organizational User with UserName: {UserName}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CreateSMSOrganizationalUserCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while creating SMS Organizational User", ApplicationEventIds.Error, ex);
            return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.CreateFailed);
        }
    }
}

public class UpdateSMSOrganizationalUserCommandHandler : BaseCommandBundle, IBaseRequestHandler<UpdateSMSOrganizationalUserCommand, Result<SMSOrganizationalUser>>
{
    private readonly ISMSOrganizationalUserService _organizationalUserService;
    private readonly ILogger<UpdateSMSOrganizationalUserCommandHandler> _logger;

    public UpdateSMSOrganizationalUserCommandHandler(ISMSOrganizationalUserService organizationalUserService, ILogger<UpdateSMSOrganizationalUserCommandHandler> logger)
    {
        _organizationalUserService = organizationalUserService ?? throw new ArgumentNullException(nameof(organizationalUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSOrganizationalUser>> HandleAsync(UpdateSMSOrganizationalUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.SMSOrganizationalUser is null)
            {
                _logger.LogApplicationError("UpdateSMSOrganizationalUserCommand received with null request or user", ApplicationEventIds.Error, null);
                return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.NullOrEmpty);
            }

            _logger.LogInformation(" Processing UpdateSMSOrganizationalUserCommand for UserID: {UserId}", request.SMSOrganizationalUser.UserId);

            var result = await _organizationalUserService.UpdateSMSOrganizationalUserAsync(request.SMSOrganizationalUser, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation(" Successfully updated SMS Organizational User with ID: {UserId}", request.SMSOrganizationalUser.UserId);
            }
            else
            {
                _logger.LogApplicationError("Failed to update SMS Organizational User with ID: {UserId}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateSMSOrganizationalUserCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating SMS Organizational User with ID: {UserId}", ApplicationEventIds.Error, ex);
            return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.UpdateFailed);
        }
    }
}

/// <summary>
/// ✅ NEW: Command handler for deactivating SMS Organizational Users using CQRS/Mediator pattern
/// Implements proper CQRS pattern with audit pipeline support for soft delete operations
/// </summary>
public class DeactivateSMSOrganizationalUserCommandHandler : BaseCommandBundle, IBaseRequestHandler<DeactivateSMSOrganizationalUserCommand, Result<SMSOrganizationalUser>>
{
    private readonly ISMSOrganizationalUserService _organizationalUserService;
    private readonly ILogger<DeactivateSMSOrganizationalUserCommandHandler> _logger;

    public DeactivateSMSOrganizationalUserCommandHandler(ISMSOrganizationalUserService organizationalUserService, ILogger<DeactivateSMSOrganizationalUserCommandHandler> logger)
    {
        _organizationalUserService = organizationalUserService ?? throw new ArgumentNullException(nameof(organizationalUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSOrganizationalUser>> HandleAsync(DeactivateSMSOrganizationalUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.SMSOrganizationalUser is null)
            {
                _logger.LogApplicationError("DeactivateSMSOrganizationalUserCommand received with null request or user", ApplicationEventIds.Error, null);
                return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.NullOrEmpty);
            }

            _logger.LogInformation("🚀 CQRS: Handling DeactivateSMSOrganizationalUserCommand for user: {UserCode} - Reason: {Reason}", 
                request.SMSOrganizationalUser.Code, request.DeactivationReason);

            // Business logic: Deactivate the user
            request.SMSOrganizationalUser.Deactivate();

            // Use the update service method to persist the deactivation
            // The audit pipeline has already set UpdatedBy and UpdatedDate via SetUpdatedBy()
            var result = await _organizationalUserService.UpdateSMSOrganizationalUserAsync(request.SMSOrganizationalUser, cancellationToken);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ CQRS: Successfully deactivated SMS Organizational User: {UserCode}", result.Value.Code);
            }
            else
            {
                _logger.LogError("❌ CQRS: Failed to deactivate SMS Organizational User. Error: {Error}", result.Error?.Message);
            }
            
            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeactivateSMSOrganizationalUserCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 CQRS: Exception in DeactivateSMSOrganizationalUserCommandHandler for user: {UserCode}", 
                request.SMSOrganizationalUser?.Code);
            return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.DeleteFailed);
        }
    }
}

public class UpdateSMSOrganizationalUserPasswordCommandHandler : BaseCommandBundle, IBaseRequestHandler<UpdateSMSOrganizationalUserPasswordCommand, Result<bool>>
{
    private readonly ISMSOrganizationalUserService _organizationalUserService;
    private readonly ILogger<UpdateSMSOrganizationalUserPasswordCommandHandler> _logger;

    public UpdateSMSOrganizationalUserPasswordCommandHandler(ISMSOrganizationalUserService organizationalUserService, ILogger<UpdateSMSOrganizationalUserPasswordCommandHandler> logger)
    {
        _organizationalUserService = organizationalUserService ?? throw new ArgumentNullException(nameof(organizationalUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(UpdateSMSOrganizationalUserPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("UpdateSMSOrganizationalUserPasswordCommand received with null request", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.NullOrEmpty);
            }

            _logger.LogInformation(" Processing UpdateSMSOrganizationalUserPasswordCommand for UserID: {UserId}", request.UserId);

            // Create new password with proper hashing
            var passwordResult = Password.Create(request.NewPassword);
            if (passwordResult.IsFailure)
                return Result<bool>.Failure<bool>(passwordResult.Error);

            // FIXED: Access the data service directly to use UpdateSMSOrganizationalUserPasswordAsync
            // This calls the dedicated pr_SMSOrganizationalUser_UpdatePassword stored procedure
            var dataServiceField = _organizationalUserService.GetType().GetField("_dataService", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (dataServiceField?.GetValue(_organizationalUserService) is not SMSOrganizationalUserDataService dataService)
            {
                _logger.LogApplicationError("Could not access SMSOrganizationalUserDataService for password update", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.PasswordUpdateFailed);
            }

            var userId = new SMSOrganizationalUserID(request.UserId);
            var result = await dataService.UpdateSMSOrganizationalUserPasswordAsync(userId, passwordResult.Value.HashedValue, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation(" Successfully updated password for SMS Organizational User with ID: {UserId}", request.UserId);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogApplicationError("Failed to update password for SMS Organizational User with ID: {UserId}. Error: {Error}",
                    ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(result.Error);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateSMSOrganizationalUserPasswordCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating password for SMS Organizational User with ID: {UserId}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.PasswordUpdateFailed);
        }
    }
}

public class AuthenticateSMSOrganizationalUserCommandHandler : BaseCommandBundle, IBaseRequestHandler<AuthenticateSMSOrganizationalUserCommand, Result<bool>>
{
    private readonly ISMSOrganizationalUserService _organizationalUserService;
    private readonly ILogger<AuthenticateSMSOrganizationalUserCommandHandler> _logger;

    public AuthenticateSMSOrganizationalUserCommandHandler(ISMSOrganizationalUserService organizationalUserService, ILogger<AuthenticateSMSOrganizationalUserCommandHandler> logger)
    {
        _organizationalUserService = organizationalUserService ?? throw new ArgumentNullException(nameof(organizationalUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(AuthenticateSMSOrganizationalUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("AuthenticateSMSOrganizationalUserCommand received with null request", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.NullOrEmpty);
            }

            _logger.LogInformation(" Processing authentication request for UserName: {UserName}", request.UserName);

            var result = await _organizationalUserService.AuthenticateSMSOrganizationalUserAsync(request.UserName, request.Password, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation(" Successfully authenticated SMS Organizational User: {UserName}", request.UserName);
            }
            else
            {
                _logger.LogWarning("Authentication failed for user: {UserName}", request.UserName);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("AuthenticateSMSOrganizationalUserCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred during authentication for user: {UserName}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.LoginFailed);
        }
    }
}

public class RecordSMSOrganizationalUserLoginCommandHandler : BaseCommandBundle, IBaseRequestHandler<RecordSMSOrganizationalUserLoginCommand, Result<bool>>
{
    private readonly ISMSOrganizationalUserService _organizationalUserService;
    private readonly ILogger<RecordSMSOrganizationalUserLoginCommandHandler> _logger;

    public RecordSMSOrganizationalUserLoginCommandHandler(ISMSOrganizationalUserService organizationalUserService, ILogger<RecordSMSOrganizationalUserLoginCommandHandler> logger)
    {
        _organizationalUserService = organizationalUserService ?? throw new ArgumentNullException(nameof(organizationalUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(RecordSMSOrganizationalUserLoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("RecordSMSOrganizationalUserLoginCommand received with null request", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.NullOrEmpty);
            }

            _logger.LogInformation(" Processing RecordSMSOrganizationalUserLoginCommand for UserID: {UserId}", request.UserId);

            // Get the existing user and record login
            var userResult = await _organizationalUserService.GetSMSOrganizationalUserByCodeAsync(request.UserId, cancellationToken);
            if (userResult.IsFailure)
            {
                return Result<bool>.Failure<bool>(userResult.Error);
            }

            var user = userResult.Value;
            user.RecordLogin();

            var updateResult = await _organizationalUserService.UpdateSMSOrganizationalUserAsync(user, cancellationToken);

            if (updateResult.IsSuccess)
            {
                _logger.LogInformation(" Successfully recorded login for SMS Organizational User with ID: {UserId}", request.UserId);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogApplicationError("Failed to record login for SMS Organizational User with ID: {UserId}. Error: {Error}",
                    ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(updateResult.Error);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("RecordSMSOrganizationalUserLoginCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while recording login for SMS Organizational User with ID: {UserId}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.UpdateFailed);
        }
    }
}

public class DeleteSMSOrganizationalUserCommandHandler : BaseCommandBundle, IBaseRequestHandler<DeleteSMSOrganizationalUserCommand, Result<bool>>
{
    private readonly ISMSOrganizationalUserService _organizationalUserService;
    private readonly IBaseMediator _mediator;
    private readonly ILogger<DeleteSMSOrganizationalUserCommandHandler> _logger;

    public DeleteSMSOrganizationalUserCommandHandler(
        ISMSOrganizationalUserService organizationalUserService, 
        IBaseMediator mediator,
        ILogger<DeleteSMSOrganizationalUserCommandHandler> logger)
    {
        _organizationalUserService = organizationalUserService ?? throw new ArgumentNullException(nameof(organizationalUserService));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteSMSOrganizationalUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("DeleteSMSOrganizationalUserCommand received with null request", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.NullOrEmpty);
            }

            _logger.LogInformation("🚀 CQRS: Handling DeleteSMSOrganizationalUserCommand (legacy) for user ID: {UserId} - Redirecting to deactivation", 
                request.SMSOrganizationalUserId.Value);

            // Get the user first
            var userResult = await _organizationalUserService.GetSMSOrganizationalUserByCodeAsync(request.SMSOrganizationalUserId.Value, cancellationToken);
            if (userResult.IsFailure)
            {
                _logger.LogWarning("❌ CQRS: Cannot delete non-existent SMS Organizational User with ID: {UserId}", request.SMSOrganizationalUserId.Value);
                return Result<bool>.Failure<bool>(userResult.Error);
            }

            // Use the new deactivation command for consistency
            var deactivateCommand = new DeactivateSMSOrganizationalUserCommand(userResult.Value, "Deleted via legacy delete command");
            var deactivateResult = await _mediator.SendAsync(deactivateCommand, cancellationToken);

            if (deactivateResult.IsSuccess)
            {
                _logger.LogInformation("✅ CQRS: Successfully processed delete as deactivation for user: {UserCode}", deactivateResult.Value.Code);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogError("❌ CQRS: Failed to process delete as deactivation. Error: {Error}", deactivateResult.Error?.Message);
                return Result<bool>.Failure<bool>(deactivateResult.Error);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeleteSMSOrganizationalUserCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while deleting SMS Organizational User with ID: {UserId}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.DeleteFailed);
        }
    }
}
