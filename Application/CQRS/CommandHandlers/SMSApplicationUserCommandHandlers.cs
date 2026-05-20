//-----------------------------------------------------------------------
// <copyright file="SMSApplicationUserCommandHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command handlers implementing SMS user management business logic and operations.
//                  Implements command handlers for processing write operations.
//                  Handles business logic execution and domain entity coordination.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using Microsoft.Extensions.Logging;
using SMS_Infrastructure.Services;

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// SMS APPLICATION USER COMMAND HANDLERS - Clean Architecture Pattern
// =============================================

public class CreateSMSApplicationUserCommandHandler : BaseCommandBundle, IBaseRequestHandler<CreateSMSApplicationUserCommand, Result<SMSApplicationUser>>
{
    private readonly ISMSApplicationUserService _applicationUserService;
    private readonly ILogger<CreateSMSApplicationUserCommandHandler> _logger;

    public CreateSMSApplicationUserCommandHandler(ISMSApplicationUserService applicationUserService, ILogger<CreateSMSApplicationUserCommandHandler> logger)
    {
        _applicationUserService = applicationUserService ?? throw new ArgumentNullException(nameof(applicationUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSApplicationUser>> HandleAsync(CreateSMSApplicationUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.SMSApplicationUser is null)
            {
                _logger.LogApplicationError("CreateSMSApplicationUserCommand received with null request or user", ApplicationEventIds.Error, null);
                return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
            }

            _logger.LogInformation(" Processing CreateSMSApplicationUserCommand for UserName: {UserName}", request.SMSApplicationUser.UserName);

            var result = await _applicationUserService.CreateSMSApplicationUserAsync(request.SMSApplicationUser, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation(" Successfully created SMS Application User with ID: {Id}, UserName: {UserName}",
                    result.Value?.UserId, result.Value?.UserName);
            }
            else
            {
                _logger.LogApplicationError("Failed to create SMS Application User with UserName: {UserName}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CreateSMSApplicationUserCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while creating SMS Application User", ApplicationEventIds.Error, ex);
            return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.CreateFailed);
        }
    }
}

public class UpdateSMSApplicationUserCommandHandler : BaseCommandBundle, IBaseRequestHandler<UpdateSMSApplicationUserCommand, Result<SMSApplicationUser>>
{
    private readonly ISMSApplicationUserService _applicationUserService;
    private readonly ILogger<UpdateSMSApplicationUserCommandHandler> _logger;

    public UpdateSMSApplicationUserCommandHandler(ISMSApplicationUserService applicationUserService, ILogger<UpdateSMSApplicationUserCommandHandler> logger)
    {
        _applicationUserService = applicationUserService ?? throw new ArgumentNullException(nameof(applicationUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSApplicationUser>> HandleAsync(UpdateSMSApplicationUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.SMSApplicationUser is null)
            {
                _logger.LogApplicationError("UpdateSMSApplicationUserCommand received with null request or user", ApplicationEventIds.Error, null);
                return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
            }

            _logger.LogInformation(" Processing UpdateSMSApplicationUserCommand for UserID: {UserId}", request.SMSApplicationUser.UserId);

            var result = await _applicationUserService.UpdateSMSApplicationUserAsync(request.SMSApplicationUser, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation(" Successfully updated SMS Application User with ID: {UserId}", request.SMSApplicationUser.UserId);
            }
            else
            {
                _logger.LogApplicationError("Failed to update SMS Application User with ID: {UserId}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateSMSApplicationUserCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating SMS Application User with ID: {UserId}", ApplicationEventIds.Error, ex);
            return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.UpdateFailed);
        }
    }
}

public class DeactivateSMSApplicationUserCommandHandler : BaseCommandBundle, IBaseRequestHandler<DeactivateSMSApplicationUserCommand, Result<SMSApplicationUser>>
{
    private readonly ISMSApplicationUserService _applicationUserService;
    private readonly ILogger<DeactivateSMSApplicationUserCommandHandler> _logger;

    public DeactivateSMSApplicationUserCommandHandler(ISMSApplicationUserService applicationUserService, ILogger<DeactivateSMSApplicationUserCommandHandler> logger)
    {
        _applicationUserService = applicationUserService ?? throw new ArgumentNullException(nameof(applicationUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSApplicationUser>> HandleAsync(DeactivateSMSApplicationUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.SMSApplicationUser is null)
            {
                _logger.LogApplicationError("DeactivateSMSApplicationUserCommand received with null request or user", ApplicationEventIds.Error, null);
                return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
            }

            _logger.LogInformation(" Processing DeactivateSMSApplicationUserCommand for UserID: {UserId}", request.SMSApplicationUser.UserId);

            // Business logic: Deactivate the user
            request.SMSApplicationUser.Deactivate();

            // Use the update service method to persist the deactivation
            // The audit pipeline has already set UpdatedBy and UpdatedDate via SetUpdatedBy()
            var result = await _applicationUserService.UpdateSMSApplicationUserAsync(request.SMSApplicationUser, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation(" Successfully deactivated SMS Application User with ID: {UserId}", request.SMSApplicationUser.UserId);
            }
            else
            {
                _logger.LogApplicationError("Failed to deactivate SMS Application User with ID: {UserId}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeactivateSMSApplicationUserCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while deactivating SMS Application User with ID: {UserId}", ApplicationEventIds.Error, ex);
            return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.DeleteFailed);
        }
    }
}

public class DeleteSMSApplicationUserCommandHandler : BaseCommandBundle, IBaseRequestHandler<DeleteSMSApplicationUserCommand, Result<bool>>
{
    private readonly ISMSApplicationUserService _applicationUserService;
    private readonly ILogger<DeleteSMSApplicationUserCommandHandler> _logger;

    public DeleteSMSApplicationUserCommandHandler(ISMSApplicationUserService applicationUserService, ILogger<DeleteSMSApplicationUserCommandHandler> logger)
    {
        _applicationUserService = applicationUserService ?? throw new ArgumentNullException(nameof(applicationUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteSMSApplicationUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("DeleteSMSApplicationUserCommand received with null request", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
            }

            _logger.LogInformation(" Processing DeleteSMSApplicationUserCommand for UserID: {UserId}", request.SMSApplicationUserId);

            var result = await _applicationUserService.DeleteSMSApplicationUserAsync(request.SMSApplicationUserId, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation(" Successfully deleted SMS Application User with ID: {UserId}", request.SMSApplicationUserId);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete SMS Application User with ID: {UserId}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeleteSMSApplicationUserCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while deleting SMS Application User with ID: {UserId}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.DeleteFailed);
        }
    }
}

public class UpdateSMSApplicationUserPasswordCommandHandler : BaseCommandBundle, IBaseRequestHandler<UpdateSMSApplicationUserPasswordCommand, Result<bool>>
{
    private readonly ISMSApplicationUserService _applicationUserService;
    private readonly ILogger<UpdateSMSApplicationUserPasswordCommandHandler> _logger;

    public UpdateSMSApplicationUserPasswordCommandHandler(ISMSApplicationUserService applicationUserService, ILogger<UpdateSMSApplicationUserPasswordCommandHandler> logger)
    {
        _applicationUserService = applicationUserService ?? throw new ArgumentNullException(nameof(applicationUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(UpdateSMSApplicationUserPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("UpdateSMSApplicationUserPasswordCommand received with null request", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
            }

            _logger.LogInformation(" Processing UpdateSMSApplicationUserPasswordCommand for UserID: {UserId}", request.UserId);

            // Create new password with proper hashing
            var passwordResult = Password.Create(request.NewPassword);
            if (passwordResult.IsFailure)
                return Result<bool>.Failure<bool>(passwordResult.Error);

            // FIXED: Access the data service directly to use UpdateSMSApplicationUserPasswordAsync
            // This calls the dedicated pr_SMSApplicationUser_UpdatePassword stored procedure
            var dataServiceField = _applicationUserService.GetType().GetField("_dataService", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (dataServiceField?.GetValue(_applicationUserService) is not SMSApplicationUserDataService dataService)
            {
                _logger.LogApplicationError("Could not access SMSApplicationUserDataService for password update", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.PasswordUpdateFailed);
            }

            var userId = new SMSApplicationUserID(request.UserId);
            var result = await dataService.UpdateSMSApplicationUserPasswordAsync(userId, passwordResult.Value.HashedValue, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation(" Successfully updated password for SMS Application User with ID: {UserId}", request.UserId);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogApplicationError("Failed to update password for SMS Application User with ID: {UserId}. Error: {Error}",
                    ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(result.Error);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateSMSApplicationUserPasswordCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating password for SMS Application User with ID: {UserId}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.PasswordUpdateFailed);
        }
    }
}

public class AuthenticateSMSApplicationUserCommandHandler : BaseCommandBundle, IBaseRequestHandler<AuthenticateSMSApplicationUserCommand, Result<bool>>
{
    private readonly ISMSApplicationUserService _applicationUserService;
    private readonly ILogger<AuthenticateSMSApplicationUserCommandHandler> _logger;

    public AuthenticateSMSApplicationUserCommandHandler(ISMSApplicationUserService applicationUserService, ILogger<AuthenticateSMSApplicationUserCommandHandler> logger)
    {
        _applicationUserService = applicationUserService ?? throw new ArgumentNullException(nameof(applicationUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(AuthenticateSMSApplicationUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("AuthenticateSMSApplicationUserCommand received with null request", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
            }

            _logger.LogInformation(" Processing authentication request for UserName: {UserName}", request.UserName);

            var result = await _applicationUserService.AuthenticateSMSApplicationUserAsync(request.UserName, request.Password, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation(" Successfully authenticated SMS Application User: {UserName}", request.UserName);
            }
            else
            {
                _logger.LogWarning("Authentication failed for user: {UserName}", request.UserName);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("AuthenticateSMSApplicationUserCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred during authentication for user: {UserName}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.LoginFailed);
        }
    }
}

public class RecordSMSApplicationUserLoginCommandHandler : BaseCommandBundle, IBaseRequestHandler<RecordSMSApplicationUserLoginCommand, Result<bool>>
{
    private readonly ISMSApplicationUserService _applicationUserService;
    private readonly ILogger<RecordSMSApplicationUserLoginCommandHandler> _logger;

    public RecordSMSApplicationUserLoginCommandHandler(ISMSApplicationUserService applicationUserService, ILogger<RecordSMSApplicationUserLoginCommandHandler> logger)
    {
        _applicationUserService = applicationUserService ?? throw new ArgumentNullException(nameof(applicationUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(RecordSMSApplicationUserLoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("RecordSMSApplicationUserLoginCommand received with null request", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
            }

            _logger.LogInformation(" Processing RecordSMSApplicationUserLoginCommand for UserID: {UserId}", request.UserId);

            // Get the existing user
            var userResult = await _applicationUserService.GetSMSApplicationUserByCodeAsync(request.UserId, cancellationToken);
            if (userResult.IsFailure)
            {
                return Result<bool>.Failure<bool>(userResult.Error);
            }

            var user = userResult.Value;

            // Record the login
            user.RecordLogin();
            user.UpdatedBy = "SYSTEM";
            user.UpdatedDate = DateTime.UtcNow;

            // Update the user via the service
            var updateResult = await _applicationUserService.UpdateSMSApplicationUserAsync(user, cancellationToken);

            if (updateResult.IsSuccess)
            {
                _logger.LogInformation(" Successfully recorded login for SMS Application User with ID: {UserId}", request.UserId);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogApplicationError("Failed to record login for SMS Application User with ID: {UserId}. Error: {Error}",
                    ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(updateResult.Error);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("RecordSMSApplicationUserLoginCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while recording login for SMS Application User with ID: {UserId}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.UpdateFailed);
        }
    }
}
