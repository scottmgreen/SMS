using Microsoft.Extensions.Logging;
using SMS_Application.Common;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;
using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Domain.Interfaces;
using SMS_Domain.ValueObjects;
using SMS_Infrastructure.Services;
using SMS_Shared.Common;

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// SMS ORGANIZATIONAL USER COMMAND HANDLERS
// =============================================

public class CreateSMSOrganizationalUserCommandHandler : BaseCommandBundle, IRequestHandler<CreateSMSOrganizationalUserCommand, Result<SMSOrganizationalUser>>
{
    private readonly SMSOrganizationalUserDataService _dataService;
    private readonly ILogger<CreateSMSOrganizationalUserCommandHandler> _logger;

    public CreateSMSOrganizationalUserCommandHandler(SMSOrganizationalUserDataService dataService, ILogger<CreateSMSOrganizationalUserCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSOrganizationalUser>> HandleAsync(CreateSMSOrganizationalUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.SMSOrganizationalUser is null)
            {
                _logger.LogError("CreateSMSOrganizationalUserCommand received with null request or user");
                return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.NullOrEmpty);
            }

            _logger.LogInformation("Processing CreateSMSOrganizationalUserCommand for UserName: {UserName}", request.SMSOrganizationalUser.UserName);

            var result = await _dataService.CreateSMSOrganizationalUserAsync(request.SMSOrganizationalUser, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created SMS Organizational User with ID: {Id}, UserName: {UserName}",
                    result.Value?.UserId, result.Value?.UserName);
            }
            else
            {
                _logger.LogError("Failed to create SMS Organizational User with UserName: {UserName}. Error: {Error}",
                    request.SMSOrganizationalUser.UserName, result.Error?.Message);
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
            _logger.LogError(ex, "Unexpected error occurred while creating SMS Organizational User");
            return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.CreateFailed);
        }
    }
}

public class UpdateSMSOrganizationalUserCommandHandler : BaseCommandBundle, IRequestHandler<UpdateSMSOrganizationalUserCommand, Result<SMSOrganizationalUser>>
{
    private readonly SMSOrganizationalUserDataService _dataService;
    private readonly ILogger<UpdateSMSOrganizationalUserCommandHandler> _logger;

    public UpdateSMSOrganizationalUserCommandHandler(SMSOrganizationalUserDataService dataService, ILogger<UpdateSMSOrganizationalUserCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSOrganizationalUser>> HandleAsync(UpdateSMSOrganizationalUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.SMSOrganizationalUser is null)
            {
                _logger.LogError("UpdateSMSOrganizationalUserCommand received with null request or user");
                return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.NullOrEmpty);
            }

            _logger.LogInformation("Processing UpdateSMSOrganizationalUserCommand for UserID: {UserId}", request.SMSOrganizationalUser.UserId);

            var result = await _dataService.UpdateSMSOrganizationalUserAsync(request.SMSOrganizationalUser, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated SMS Organizational User with ID: {UserId}", request.SMSOrganizationalUser.UserId);
            }
            else
            {
                _logger.LogError("Failed to update SMS Organizational User with ID: {UserId}. Error: {Error}",
                    request.SMSOrganizationalUser.UserId, result.Error?.Message);
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
            _logger.LogError(ex, "Unexpected error occurred while updating SMS Organizational User with ID: {UserId}", request?.SMSOrganizationalUser?.UserId);
            return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.UpdateFailed);
        }
    }
}

public class UpdateSMSOrganizationalUserPasswordCommandHandler : BaseCommandBundle, IRequestHandler<UpdateSMSOrganizationalUserPasswordCommand, Result<bool>>
{
    private readonly SMSOrganizationalUserDataService _dataService;
    private readonly ILogger<UpdateSMSOrganizationalUserPasswordCommandHandler> _logger;

    public UpdateSMSOrganizationalUserPasswordCommandHandler(SMSOrganizationalUserDataService dataService, ILogger<UpdateSMSOrganizationalUserPasswordCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(UpdateSMSOrganizationalUserPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("UpdateSMSOrganizationalUserPasswordCommand received with null request");
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.NullOrEmpty);
            }

            _logger.LogInformation("Processing UpdateSMSOrganizationalUserPasswordCommand for UserID: {UserId}", request.UserId);

            // Get the existing user
            var userResult = await _dataService.GetSMSOrganizationalUserByIdAsync(request.UserId, cancellationToken);
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
            user.UpdatedBy = "SYSTEM"; // TODO: Get current user
            user.UpdatedDate = DateTime.UtcNow;

            // Update the entire user record (which includes the new password)
            var updateResult = await _dataService.UpdateSMSOrganizationalUserAsync(user, cancellationToken);

            if (updateResult.IsSuccess)
            {
                _logger.LogInformation("Successfully updated password for SMS Organizational User with ID: {UserId}", request.UserId);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogError("Failed to update password for SMS Organizational User with ID: {UserId}. Error: {Error}",
                    request.UserId, updateResult.Error?.Message);
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
            _logger.LogError(ex, "Unexpected error occurred while updating password for SMS Organizational User with ID: {UserId}", request.UserId);
            return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.PasswordUpdateFailed);
        }
    }
}

public class AuthenticateSMSOrganizationalUserCommandHandler : BaseCommandBundle, IRequestHandler<AuthenticateSMSOrganizationalUserCommand, Result<bool>>
{
    private readonly SMSOrganizationalUserDataService _dataService;
    private readonly ILogger<AuthenticateSMSOrganizationalUserCommandHandler> _logger;

    public AuthenticateSMSOrganizationalUserCommandHandler(SMSOrganizationalUserDataService dataService, ILogger<AuthenticateSMSOrganizationalUserCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(AuthenticateSMSOrganizationalUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("AuthenticateSMSOrganizationalUserCommand received with null request");
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.NullOrEmpty);
            }

            _logger.LogInformation("Processing authentication request for UserName: {UserName}", request.UserName);

            var result = await _dataService.AuthenticateSMSOrganizationalUserAsync(request.UserName, request.Password, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully authenticated SMS Organizational User: {UserName}", request.UserName);
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
            _logger.LogError(ex, "Unexpected error occurred during authentication for user: {UserName}", request.UserName);
            return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.LoginFailed);
        }
    }
}

public class RecordSMSOrganizationalUserLoginCommandHandler : BaseCommandBundle, IRequestHandler<RecordSMSOrganizationalUserLoginCommand, Result<bool>>
{
    private readonly SMSOrganizationalUserDataService _dataService;
    private readonly ILogger<RecordSMSOrganizationalUserLoginCommandHandler> _logger;

    public RecordSMSOrganizationalUserLoginCommandHandler(SMSOrganizationalUserDataService dataService, ILogger<RecordSMSOrganizationalUserLoginCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(RecordSMSOrganizationalUserLoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("RecordSMSOrganizationalUserLoginCommand received with null request");
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.NullOrEmpty);
            }

            _logger.LogInformation("Processing RecordSMSOrganizationalUserLoginCommand for UserID: {UserId}", request.UserId);

            // Get the existing user
            var userResult = await _dataService.GetSMSOrganizationalUserByIdAsync(request.UserId, cancellationToken);
            if (userResult.IsFailure)
            {
                return Result<bool>.Failure<bool>(userResult.Error);
            }

            var user = userResult.Value;

            // Record the login
            user.RecordLogin();

            var updateResult = await _dataService.UpdateSMSOrganizationalUserAsync(user, cancellationToken);

            if (updateResult.IsSuccess)
            {
                _logger.LogInformation("Successfully recorded login for SMS Organizational User with ID: {UserId}", request.UserId);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogError("Failed to record login for SMS Organizational User with ID: {UserId}. Error: {Error}",
                    request.UserId, updateResult.Error?.Message);
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
            _logger.LogError(ex, "Unexpected error occurred while recording login for SMS Organizational User with ID: {UserId}", request.UserId);
            return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.UpdateFailed);
        }
    }
}

public class DeleteSMSOrganizationalUserCommandHandler : BaseCommandBundle, IRequestHandler<DeleteSMSOrganizationalUserCommand, Result<bool>>
{
    private readonly SMSOrganizationalUserDataService _dataService;
    private readonly ILogger<DeleteSMSOrganizationalUserCommandHandler> _logger;

    public DeleteSMSOrganizationalUserCommandHandler(SMSOrganizationalUserDataService dataService, ILogger<DeleteSMSOrganizationalUserCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteSMSOrganizationalUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("DeleteSMSOrganizationalUserCommand received with null request");
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.NullOrEmpty);
            }

            _logger.LogInformation("Processing DeleteSMSOrganizationalUserCommand for UserID: {UserId}", request.SMSOrganizationalUserId);

            var result = await _dataService.DeleteSMSOrganizationalUserAsync(request.SMSOrganizationalUserId.Value, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted SMS Organizational User with ID: {UserId}", request.SMSOrganizationalUserId);
            }
            else
            {
                _logger.LogError("Failed to delete SMS Organizational User with ID: {UserId}. Error: {Error}",
                    request.SMSOrganizationalUserId, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeleteSMSOrganizationalUserCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while deleting SMS Organizational User with ID: {UserId}", request?.SMSOrganizationalUserId);
            return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.DeleteFailed);
        }
    }
}