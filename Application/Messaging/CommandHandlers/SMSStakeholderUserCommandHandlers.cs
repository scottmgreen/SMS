using Microsoft.Extensions.Logging;

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// SMS STAKEHOLDER USER COMMAND HANDLERS
// =============================================

public class CreateSMSStakeholderUserCommandHandler : BaseCommandBundle, IRequestHandler<CreateSMSStakeholderUserCommand, Result<SMSStakeholderUser>>
{
    private readonly SMSStakeholderUserDataService _dataService;
    private readonly ILogger<CreateSMSStakeholderUserCommandHandler> _logger;

    public CreateSMSStakeholderUserCommandHandler(SMSStakeholderUserDataService dataService, ILogger<CreateSMSStakeholderUserCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
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

            _logger.LogInformation("Processing CreateSMSStakeholderUserCommand for UserName: {UserName}", request.SMSStakeholderUser.UserName);

            var result = await _dataService.AddAsync(request.SMSStakeholderUser, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created SMS Stakeholder User with ID: {Id}, UserName: {UserName}",
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
    private readonly SMSStakeholderUserDataService _dataService;
    private readonly ILogger<UpdateSMSStakeholderUserCommandHandler> _logger;

    public UpdateSMSStakeholderUserCommandHandler(SMSStakeholderUserDataService dataService, ILogger<UpdateSMSStakeholderUserCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
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

            _logger.LogInformation("Processing UpdateSMSStakeholderUserCommand for UserID: {UserId}", request.SMSStakeholderUser.UserId);

            var result = await _dataService.UpdateAsync(request.SMSStakeholderUser, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated SMS Stakeholder User with ID: {UserId}", request.SMSStakeholderUser.UserId);
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
    private readonly SMSStakeholderUserDataService _dataService;
    private readonly ILogger<UpdateSMSStakeholderUserPasswordCommandHandler> _logger;

    public UpdateSMSStakeholderUserPasswordCommandHandler(SMSStakeholderUserDataService dataService, ILogger<UpdateSMSStakeholderUserPasswordCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
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

            _logger.LogInformation("Processing UpdateSMSStakeholderUserPasswordCommand for UserID: {UserId}", request.UserId);

            // Get the existing user
            var userResult = await _dataService.GetByIdAsync(request.UserId, cancellationToken);
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
            var updateResult = await _dataService.UpdateAsync(user, cancellationToken);

            if (updateResult.IsSuccess)
            {
                _logger.LogInformation("Successfully updated password for SMS Stakeholder User with ID: {UserId}", request.UserId);
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
    private readonly SMSStakeholderUserDataService _dataService;
    private readonly ILogger<AuthenticateSMSStakeholderUserCommandHandler> _logger;

    public AuthenticateSMSStakeholderUserCommandHandler(SMSStakeholderUserDataService dataService, ILogger<AuthenticateSMSStakeholderUserCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
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

            _logger.LogInformation("Processing authentication request for UserName: {UserName}", request.UserName);

            var result = await _dataService.AuthenticateUserAsync(request.UserName, request.Password, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully authenticated SMS Stakeholder User: {UserName}", request.UserName);
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
    private readonly SMSStakeholderUserDataService _dataService;
    private readonly ILogger<RecordSMSStakeholderUserLoginCommandHandler> _logger;

    public RecordSMSStakeholderUserLoginCommandHandler(SMSStakeholderUserDataService dataService, ILogger<RecordSMSStakeholderUserLoginCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
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

            _logger.LogInformation("Processing RecordSMSStakeholderUserLoginCommand for UserID: {UserId}", request.UserId);

            // Get the existing user
            var userResult = await _dataService.GetByIdAsync(request.UserId, cancellationToken);
            if (userResult.IsFailure)
            {
                return Result<bool>.Failure<bool>(userResult.Error);
            }

            var user = userResult.Value;

            // Record the login
            user.RecordLogin();

            var updateResult = await _dataService.UpdateAsync(user, cancellationToken);

            if (updateResult.IsSuccess)
            {
                _logger.LogInformation("Successfully recorded login for SMS Stakeholder User with ID: {UserId}", request.UserId);
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
    private readonly SMSStakeholderUserDataService _dataService;
    private readonly ILogger<DeleteSMSStakeholderUserCommandHandler> _logger;

    public DeleteSMSStakeholderUserCommandHandler(SMSStakeholderUserDataService dataService, ILogger<DeleteSMSStakeholderUserCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
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

            _logger.LogInformation("Processing DeleteSMSStakeholderUserCommand for UserID: {UserId}", request.SMSStakeholderUserId);

            var result = await _dataService.DeleteUserAsync(request.SMSStakeholderUserId.Value, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted SMS Stakeholder User with ID: {UserId}", request.SMSStakeholderUserId);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete SMS Stakeholder User with ID: {UserId}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
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