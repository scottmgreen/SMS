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
// SMS APPLICATION USER COMMAND HANDLERS
// =============================================

public class CreateSMSApplicationUserCommandHandler : BaseCommandBundle, IRequestHandler<CreateSMSApplicationUserCommand, Result<SMSApplicationUser>>
{
    private readonly SMSApplicationUserDataService _dataService;
    private readonly ILogger<CreateSMSApplicationUserCommandHandler> _logger;

    public CreateSMSApplicationUserCommandHandler(SMSApplicationUserDataService dataService, ILogger<CreateSMSApplicationUserCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSApplicationUser>> HandleAsync(CreateSMSApplicationUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.SMSApplicationUser is null)
            {
                _logger.LogError("CreateSMSApplicationUserCommand received with null request or user");
                return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
            }

            _logger.LogInformation("Processing CreateSMSApplicationUserCommand for UserName: {UserName}", request.SMSApplicationUser.UserName);

            var result = await _dataService.CreateSMSApplicationUserAsync(request.SMSApplicationUser, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created SMS Application User with ID: {Id}, UserName: {UserName}",
                    result.Value?.UserId, result.Value?.UserName);
            }
            else
            {
                _logger.LogError("Failed to create SMS Application User with UserName: {UserName}. Error: {Error}",
                    request.SMSApplicationUser.UserName, result.Error?.Message);
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
            _logger.LogError(ex, "Unexpected error occurred while creating SMS Application User");
            return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.CreateFailed);
        }
    }
}

public class UpdateSMSApplicationUserCommandHandler : BaseCommandBundle, IRequestHandler<UpdateSMSApplicationUserCommand, Result<SMSApplicationUser>>
{
    private readonly SMSApplicationUserDataService _dataService;
    private readonly ILogger<UpdateSMSApplicationUserCommandHandler> _logger;

    public UpdateSMSApplicationUserCommandHandler(SMSApplicationUserDataService dataService, ILogger<UpdateSMSApplicationUserCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSApplicationUser>> HandleAsync(UpdateSMSApplicationUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.SMSApplicationUser is null)
            {
                _logger.LogError("UpdateSMSApplicationUserCommand received with null request or user");
                return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
            }

            _logger.LogInformation("Processing UpdateSMSApplicationUserCommand for UserID: {UserId}", request.SMSApplicationUser.UserId);

            var result = await _dataService.UpdateSMSApplicationUserAsync(request.SMSApplicationUser, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated SMS Application User with ID: {UserId}", request.SMSApplicationUser.UserId);
            }
            else
            {
                _logger.LogError("Failed to update SMS Application User with ID: {UserId}. Error: {Error}",
                    request.SMSApplicationUser.UserId, result.Error?.Message);
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
            _logger.LogError(ex, "Unexpected error occurred while updating SMS Application User with ID: {UserId}", request?.SMSApplicationUser?.UserId);
            return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.UpdateFailed);
        }
    }
}

public class UpdateSMSApplicationUserPasswordCommandHandler : BaseCommandBundle, IRequestHandler<UpdateSMSApplicationUserPasswordCommand, Result<bool>>
{
    private readonly SMSApplicationUserDataService _dataService;
    private readonly ILogger<UpdateSMSApplicationUserPasswordCommandHandler> _logger;

    public UpdateSMSApplicationUserPasswordCommandHandler(SMSApplicationUserDataService dataService, ILogger<UpdateSMSApplicationUserPasswordCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(UpdateSMSApplicationUserPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("UpdateSMSApplicationUserPasswordCommand received with null request");
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
            }

            _logger.LogInformation("Processing UpdateSMSApplicationUserPasswordCommand for UserID: {UserId}", request.UserId);

            // Get the existing user
            var userResult = await _dataService.GetSMSApplicationUserByIdAsync(request.UserId, cancellationToken);
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
            user.UpdatedBy = request.UpdatedBy;
            user.UpdatedDate = DateTime.UtcNow;

            var updateResult = await _dataService.UpdateSMSApplicationUserPasswordAsync(user.Code,user.Password.HashedValue, cancellationToken);

            if (updateResult.IsSuccess)
            {
                _logger.LogInformation("Successfully updated password for SMS Application User with ID: {UserId}", request.UserId);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogError("Failed to update password for SMS Application User with ID: {UserId}. Error: {Error}",
                    request.UserId, updateResult.Error?.Message);
                return Result<bool>.Failure<bool>(updateResult.Error);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateSMSApplicationUserPasswordCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while updating password for SMS Application User with ID: {UserId}", request.UserId);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.PasswordUpdateFailed);
        }
    }
}

public class AuthenticateSMSApplicationUserCommandHandler : BaseCommandBundle, IRequestHandler<AuthenticateSMSApplicationUserCommand, Result<bool>>
{
    private readonly SMSApplicationUserDataService _dataService;
    private readonly ILogger<AuthenticateSMSApplicationUserCommandHandler> _logger;

    public AuthenticateSMSApplicationUserCommandHandler(SMSApplicationUserDataService dataService, ILogger<AuthenticateSMSApplicationUserCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(AuthenticateSMSApplicationUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("AuthenticateSMSApplicationUserCommand received with null request");
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
            }

            _logger.LogInformation("Processing authentication request for UserName: {UserName}", request.UserName);

            var result = await _dataService.AuthenticateSMSApplicationUserAsync(request.UserName, request.Password, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully authenticated SMS Application User: {UserName}", request.UserName);
            }
            else
            {
                _logger.LogWarning("Authentication failed for user: {UserName}", request.UserName);
            }
            
            return result.Value;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("AuthenticateSMSApplicationUserCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred during authentication for user: {UserName}", request.UserName);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.LoginFailed);
        }
    }
}

public class RecordSMSApplicationUserLoginCommandHandler : BaseCommandBundle, IRequestHandler<RecordSMSApplicationUserLoginCommand, Result<bool>>
{
    private readonly SMSApplicationUserDataService _dataService;
    private readonly ILogger<RecordSMSApplicationUserLoginCommandHandler> _logger;

    public RecordSMSApplicationUserLoginCommandHandler(SMSApplicationUserDataService dataService, ILogger<RecordSMSApplicationUserLoginCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(RecordSMSApplicationUserLoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("RecordSMSApplicationUserLoginCommand received with null request");
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
            }

            _logger.LogInformation("Processing RecordSMSApplicationUserLoginCommand for UserID: {UserId}", request.UserId);

            var applicationUserId = new SMSApplicationUserID(request.UserId);
            var result = await _dataService.RecordSMSApplicationUserLoginAsync(applicationUserId, request.LoginDate, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully recorded login for SMS Application User with ID: {UserId}", request.UserId);
            }
            else
            {
                _logger.LogError("Failed to record login for SMS Application User with ID: {UserId}. Error: {Error}",
                    request.UserId, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("RecordSMSApplicationUserLoginCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while recording login for SMS Application User with ID: {UserId}", request.UserId);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.UpdateFailed);
        }
    }
}

public class DeleteSMSApplicationUserCommandHandler : BaseCommandBundle, IRequestHandler<DeleteSMSApplicationUserCommand, Result<bool>>
{
    private readonly SMSApplicationUserDataService _dataService;
    private readonly ILogger<DeleteSMSApplicationUserCommandHandler> _logger;

    public DeleteSMSApplicationUserCommandHandler(SMSApplicationUserDataService dataService, ILogger<DeleteSMSApplicationUserCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteSMSApplicationUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("DeleteSMSApplicationUserCommand received with null request");
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
            }

            _logger.LogInformation("Processing DeleteSMSApplicationUserCommand for UserID: {UserId}", request.SMSApplicationUserId);

            var result = await _dataService.DeleteSMSApplicationUserAsync(request.SMSApplicationUserId, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted SMS Application User with ID: {UserId}", request.SMSApplicationUserId);
            }
            else
            {
                _logger.LogError("Failed to delete SMS Application User with ID: {UserId}. Error: {Error}",
                    request.SMSApplicationUserId, result.Error?.Message);
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
            _logger.LogError(ex, "Unexpected error occurred while deleting SMS Application User with ID: {UserId}", request?.SMSApplicationUserId);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.DeleteFailed);
        }
    }
}