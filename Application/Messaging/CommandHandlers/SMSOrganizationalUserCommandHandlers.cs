//-----------------------------------------------------------------------
// <copyright file="SMSOrganizationalUserCommandHandlers.cs" company="SMS Safety Management System">
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
// SMS ORGANIZATIONAL USER COMMAND HANDLERS - Clean Architecture Pattern
// =============================================

public class CreateSMSOrganizationalUserCommandHandler : BaseCommandBundle, IRequestHandler<CreateSMSOrganizationalUserCommand, Result<SMSOrganizationalUser>>
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

            _logger.LogInformation("✅ Clean Architecture: Processing CreateSMSOrganizationalUserCommand for UserName: {UserName}", request.SMSOrganizationalUser.UserName);

            var result = await _organizationalUserService.CreateSMSOrganizationalUserAsync(request.SMSOrganizationalUser, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully created SMS Organizational User with ID: {Id}, UserName: {UserName}",
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

public class UpdateSMSOrganizationalUserCommandHandler : BaseCommandBundle, IRequestHandler<UpdateSMSOrganizationalUserCommand, Result<SMSOrganizationalUser>>
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

            _logger.LogInformation("✅ Clean Architecture: Processing UpdateSMSOrganizationalUserCommand for UserID: {UserId}", request.SMSOrganizationalUser.UserId);

            var result = await _organizationalUserService.UpdateSMSOrganizationalUserAsync(request.SMSOrganizationalUser, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully updated SMS Organizational User with ID: {UserId}", request.SMSOrganizationalUser.UserId);
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

public class UpdateSMSOrganizationalUserPasswordCommandHandler : BaseCommandBundle, IRequestHandler<UpdateSMSOrganizationalUserPasswordCommand, Result<bool>>
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

            _logger.LogInformation("✅ Clean Architecture: Processing UpdateSMSOrganizationalUserPasswordCommand for UserID: {UserId}", request.UserId);

            // Get the existing user
            var userResult = await _organizationalUserService.GetSMSOrganizationalUserByCodeAsync(request.UserId, cancellationToken);
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

            var updateResult = await _organizationalUserService.UpdateSMSOrganizationalUserAsync(user, cancellationToken);

            if (updateResult.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully updated password for SMS Organizational User with ID: {UserId}", request.UserId);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogApplicationError("Failed to update password for SMS Organizational User with ID: {UserId}. Error: {Error}",
                    ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(updateResult.Error);
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

public class AuthenticateSMSOrganizationalUserCommandHandler : BaseCommandBundle, IRequestHandler<AuthenticateSMSOrganizationalUserCommand, Result<bool>>
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

            _logger.LogInformation("✅ Clean Architecture: Processing authentication request for UserName: {UserName}", request.UserName);

            var result = await _organizationalUserService.AuthenticateSMSOrganizationalUserAsync(request.UserName, request.Password, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully authenticated SMS Organizational User: {UserName}", request.UserName);
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

public class RecordSMSOrganizationalUserLoginCommandHandler : BaseCommandBundle, IRequestHandler<RecordSMSOrganizationalUserLoginCommand, Result<bool>>
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

            _logger.LogInformation("✅ Clean Architecture: Processing RecordSMSOrganizationalUserLoginCommand for UserID: {UserId}", request.UserId);

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
                _logger.LogInformation("✅ Clean Architecture: Successfully recorded login for SMS Organizational User with ID: {UserId}", request.UserId);
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

public class DeleteSMSOrganizationalUserCommandHandler : BaseCommandBundle, IRequestHandler<DeleteSMSOrganizationalUserCommand, Result<bool>>
{
    private readonly ISMSOrganizationalUserService _organizationalUserService;
    private readonly ILogger<DeleteSMSOrganizationalUserCommandHandler> _logger;

    public DeleteSMSOrganizationalUserCommandHandler(ISMSOrganizationalUserService organizationalUserService, ILogger<DeleteSMSOrganizationalUserCommandHandler> logger)
    {
        _organizationalUserService = organizationalUserService ?? throw new ArgumentNullException(nameof(organizationalUserService));
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

            _logger.LogInformation("✅ Clean Architecture: Processing DeleteSMSOrganizationalUserCommand for UserID: {UserId}", request.SMSOrganizationalUserId);

            // For now, implement as logical delete by updating the user record
            var userResult = await _organizationalUserService.GetSMSOrganizationalUserByCodeAsync(request.SMSOrganizationalUserId.Value, cancellationToken);
            if (userResult.IsFailure)
            {
                return Result<bool>.Failure<bool>(userResult.Error);
            }

            var user = userResult.Value;
            user.IsActive = false;
            user.UpdatedBy = "SYSTEM";
            user.UpdatedDate = DateTime.UtcNow;

            var updateResult = await _organizationalUserService.UpdateSMSOrganizationalUserAsync(user, cancellationToken);

            if (updateResult.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: Successfully deleted (deactivated) SMS Organizational User with ID: {UserId}", request.SMSOrganizationalUserId);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete SMS Organizational User with ID: {UserId}. Error: {Error}",
                    ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(updateResult.Error);
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
