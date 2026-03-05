//-----------------------------------------------------------------------
// <copyright file="SMSStakeholderUserCommandHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command handlers implementing SMS user management business logic and operations.
//                  Implements command handlers for processing write operations.
//                  Handles business logic execution and domain entity coordination.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Application.Interfaces;

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// SMS STAKEHOLDER USER COMMAND HANDLERS - Clean Architecture Pattern
// =============================================

public class CreateSMSStakeholderUserCommandHandler : BaseCommandBundle, IRequestHandler<CreateSMSStakeholderUserCommand, Result<SMSStakeholderUser>>
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

            _logger.LogInformation("✅ Clean Architecture: Processing CreateSMSStakeholderUserCommand for UserName: {UserName}", request.SMSStakeholderUser.UserName);

            var result = await _stakeholderUserService.CreateSMSStakeholderUserAsync(request.SMSStakeholderUser, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully created SMS Stakeholder User with ID: {Id}, UserName: {UserName}",
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
            _logger.LogWarning("CreateSMSStakeholderUserCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while creating SMS Stakeholder User", ApplicationEventIds.Error, ex);
            return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.CreateFailed);
        }
    }
}

public class UpdateSMSStakeholderUserCommandHandler : BaseCommandBundle, IRequestHandler<UpdateSMSStakeholderUserCommand, Result<SMSStakeholderUser>>
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

            _logger.LogInformation("✅ Clean Architecture: Processing UpdateSMSStakeholderUserCommand for UserID: {UserId}", request.SMSStakeholderUser.UserId);

            var result = await _stakeholderUserService.UpdateSMSStakeholderUserAsync(request.SMSStakeholderUser, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully updated SMS Stakeholder User with ID: {UserId}", request.SMSStakeholderUser.UserId);
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
            _logger.LogWarning("UpdateSMSStakeholderUserCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating SMS Stakeholder User with ID: {UserId}", ApplicationEventIds.Error, ex);
            return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.UpdateFailed);
        }
    }
}

public class UpdateSMSStakeholderUserPasswordCommandHandler : BaseCommandBundle, IRequestHandler<UpdateSMSStakeholderUserPasswordCommand, Result<bool>>
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

            _logger.LogInformation("✅ Clean Architecture: Processing UpdateSMSStakeholderUserPasswordCommand for UserID: {UserId}", request.UserId);

            // Get the existing user
            var userResult = await _stakeholderUserService.GetSMSStakeholderUserByIdAsync(request.UserId, cancellationToken);
            if (userResult.IsFailure)
            {
                return Result<bool>.Failure<bool>(userResult.Error);
            }

            var user = userResult.Value;

            // Create new password
            var passwordResult = Password.Create(request.NewPassword);
            if (passwordResult.IsFailure)
                return Result<bool>.Failure<bool>(passwordResult.Error);

            // Update user password
            user.UpdatePassword(passwordResult.Value);
            user.UpdatedBy = "SYSTEM";
            user.UpdatedDate = DateTime.UtcNow;

            var updateResult = await _stakeholderUserService.UpdateSMSStakeholderUserAsync(user, cancellationToken);

            if (updateResult.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully updated password for SMS Stakeholder User with ID: {UserId}", request.UserId);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogApplicationError("Failed to update password for SMS Stakeholder User with ID: {UserId}. Error: {Error}",
                    ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(updateResult.Error);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateSMSStakeholderUserPasswordCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating password for SMS Stakeholder User with ID: {UserId}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.PasswordUpdateFailed);
        }
    }
}

public class AuthenticateSMSStakeholderUserCommandHandler : BaseCommandBundle, IRequestHandler<AuthenticateSMSStakeholderUserCommand, Result<bool>>
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

            _logger.LogInformation("✅ Clean Architecture: Processing authentication request for UserName: {UserName}", request.UserName);

            var result = await _stakeholderUserService.AuthenticateSMSStakeholderUserAsync(request.UserName, request.Password, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully authenticated SMS Stakeholder User: {UserName}", request.UserName);
            }
            else
            {
                _logger.LogWarning("Authentication failed for user: {UserName}", request.UserName);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("AuthenticateSMSStakeholderUserCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred during authentication for user: {UserName}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.LoginFailed);
        }
    }
}

public class RecordSMSStakeholderUserLoginCommandHandler : BaseCommandBundle, IRequestHandler<RecordSMSStakeholderUserLoginCommand, Result<bool>>
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

            _logger.LogInformation("✅ Clean Architecture: Processing RecordSMSStakeholderUserLoginCommand for UserID: {UserId}", request.UserId);

            // Get the existing user and record login
            var userResult = await _stakeholderUserService.GetSMSStakeholderUserByIdAsync(request.UserId, cancellationToken);
            if (userResult.IsFailure)
            {
                return Result<bool>.Failure<bool>(userResult.Error);
            }

            var user = userResult.Value;
            user.RecordLogin();

            var updateResult = await _stakeholderUserService.UpdateSMSStakeholderUserAsync(user, cancellationToken);

            if (updateResult.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully recorded login for SMS Stakeholder User with ID: {UserId}", request.UserId);
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
            _logger.LogWarning("RecordSMSStakeholderUserLoginCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while recording login for SMS Stakeholder User with ID: {UserId}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.UpdateFailed);
        }
    }
}

public class DeleteSMSStakeholderUserCommandHandler : BaseCommandBundle, IRequestHandler<DeleteSMSStakeholderUserCommand, Result<bool>>
{
    private readonly ISMSStakeholderUserService _stakeholderUserService;
    private readonly ILogger<DeleteSMSStakeholderUserCommandHandler> _logger;

    public DeleteSMSStakeholderUserCommandHandler(ISMSStakeholderUserService stakeholderUserService, ILogger<DeleteSMSStakeholderUserCommandHandler> logger)
    {
        _stakeholderUserService = stakeholderUserService ?? throw new ArgumentNullException(nameof(stakeholderUserService));
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

            _logger.LogInformation("✅ Clean Architecture: Processing DeleteSMSStakeholderUserCommand for UserID: {UserId}", request.SMSStakeholderUserId);

            // For now, implement as logical delete by updating the user record
            var userResult = await _stakeholderUserService.GetSMSStakeholderUserByIdAsync(request.SMSStakeholderUserId, cancellationToken);
            if (userResult.IsFailure)
            {
                return Result<bool>.Failure<bool>(userResult.Error);
            }

            var user = userResult.Value;
            user.IsActive = false;
            user.UpdatedBy = "SYSTEM";
            user.UpdatedDate = DateTime.UtcNow;

            var updateResult = await _stakeholderUserService.UpdateSMSStakeholderUserAsync(user, cancellationToken);

            if (updateResult.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully deleted (deactivated) SMS Stakeholder User with ID: {UserId}", request.SMSStakeholderUserId);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete SMS Stakeholder User with ID: {UserId}. Error: {Error}",
                    ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(updateResult.Error);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeleteSMSStakeholderUserCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while deleting SMS Stakeholder User with ID: {UserId}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.DeleteFailed);
        }
    }
}
