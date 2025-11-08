using Microsoft.Extensions.Logging;
using SMS_Application.Common;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;
using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Domain.Interfaces;
using SMS_Domain.ValueObjects;

using SMS_Infrastructure.Interfaces;

using SMS_Shared.Common;

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// SMS APPLICATION USER COMMAND HANDLERS
// =============================================

public class CreateSMSApplicationUserCommandHandler : BaseCommandBundle, IRequestHandler<CreateSMSApplicationUserCommand, Result<SMSApplicationUser>>
{
    private readonly ISMSApplicationUserRepository _repository;
    private readonly ILogger<CreateSMSApplicationUserCommandHandler> _logger;

    public CreateSMSApplicationUserCommandHandler(ISMSApplicationUserRepository repository, ILogger<CreateSMSApplicationUserCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSApplicationUser>> HandleAsync(CreateSMSApplicationUserCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("CreateSMSApplicationUserCommand received with null request");
                return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
            }

            _logger.LogInformation("Processing CreateSMSApplicationUserCommand for UserName: {UserName}", request.UserName);

            // Check if username already exists
            var existsResult = await _repository.UserNameExistsAsync(request.UserName);
            if (existsResult.IsFailure)
            {
                return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(existsResult.Error);
            }

            if (existsResult.Value)
            {
                _logger.LogWarning("Username {UserName} already exists", request.UserName);
                return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.UserNameError.AlreadyExists);
            }

            // Create value objects
            var firstNameResult = FirstName.Create(request.FirstName);
            if (firstNameResult.IsFailure)
                return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(firstNameResult.Error);

            var lastNameResult = LastName.Create(request.LastName);
            if (lastNameResult.IsFailure)
                return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(lastNameResult.Error);

            var userNameResult = UserName.Create(request.UserName);
            if (userNameResult.IsFailure)
                return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(userNameResult.Error);

            var passwordResult = Password.Create(request.Password);
            if (passwordResult.IsFailure)
                return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(passwordResult.Error);

            // Create the user
            var user = SMSApplicationUser.Create(
                request.Code,
                firstNameResult.Value,
                lastNameResult.Value,
                userNameResult.Value,
                passwordResult.Value,
                request.ApplicationRole,
                request.PermissionLevel,
                request.CreatedBy);

            var result = await _repository.AddAsync(user);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created SMS Application User with ID: {Id}, UserName: {UserName}",
                    result.Value?.UserId, result.Value?.UserName);
            }
            else
            {
                _logger.LogError("Failed to create SMS Application User with UserName: {UserName}. Error: {Error}",
                    request.UserName, result.Error?.Message);
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

public class UpdateSMSApplicationUserCommandHandler : BaseCommandBundle, IRequestHandler<UpdateSMSApplicationUserCommand, Result<bool>>
{
    private readonly ISMSApplicationUserRepository _repository;
    private readonly ILogger<UpdateSMSApplicationUserCommandHandler> _logger;

    public UpdateSMSApplicationUserCommandHandler(ISMSApplicationUserRepository repository, ILogger<UpdateSMSApplicationUserCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(UpdateSMSApplicationUserCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("UpdateSMSApplicationUserCommand received with null request");
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
            }

            _logger.LogInformation("Processing UpdateSMSApplicationUserCommand for UserID: {UserId}", request.UserId);

            // Get the existing user
            var userResult = await _repository.GetByIdAsync(request.UserId);
            if (userResult.IsFailure)
            {
                return Result<bool>.Failure<bool>(userResult.Error);
            }

            var user = userResult.Value;

            // Create value objects
            var firstNameResult = FirstName.Create(request.FirstName);
            if (firstNameResult.IsFailure)
                return Result<bool>.Failure<bool>(firstNameResult.Error);

            var lastNameResult = LastName.Create(request.LastName);
            if (lastNameResult.IsFailure)
                return Result<bool>.Failure<bool>(lastNameResult.Error);

            // Update user information
            user.UpdateBasicInfo(firstNameResult.Value, lastNameResult.Value);

            var result = await _repository.UpdateAsync(user);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated SMS Application User with ID: {UserId}", request.UserId);
            }
            else
            {
                _logger.LogError("Failed to update SMS Application User with ID: {UserId}. Error: {Error}",
                    request.UserId, result.Error?.Message);
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
            _logger.LogError(ex, "Unexpected error occurred while updating SMS Application User with ID: {UserId}", request.UserId);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.UpdateFailed);
        }
    }
}

public class UpdateSMSApplicationUserPasswordCommandHandler : BaseCommandBundle, IRequestHandler<UpdateSMSApplicationUserPasswordCommand, Result<bool>>
{
    private readonly ISMSApplicationUserRepository _repository;
    private readonly ILogger<UpdateSMSApplicationUserPasswordCommandHandler> _logger;

    public UpdateSMSApplicationUserPasswordCommandHandler(ISMSApplicationUserRepository repository, ILogger<UpdateSMSApplicationUserPasswordCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(UpdateSMSApplicationUserPasswordCommand request, CancellationToken ct = default)
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
            var userResult = await _repository.GetByIdAsync(request.UserId);
            if (userResult.IsFailure)
            {
                return Result<bool>.Failure<bool>(userResult.Error);
            }

            var user = userResult.Value;

            // Create new password
            var passwordResult = request.RequiresChange 
                ? Password.CreateTemporary(request.NewPassword)
                : Password.Create(request.NewPassword);

            if (passwordResult.IsFailure)
                return Result<bool>.Failure<bool>(passwordResult.Error);

            // Update user password
            user.UpdatePassword(passwordResult.Value);

            var result = await _repository.UpdateAsync(user);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated password for SMS Application User with ID: {UserId}", request.UserId);
            }
            else
            {
                _logger.LogError("Failed to update password for SMS Application User with ID: {UserId}. Error: {Error}",
                    request.UserId, result.Error?.Message);
            }

            return result;
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

public class AuthenticateSMSApplicationUserCommandHandler : BaseCommandBundle, IRequestHandler<AuthenticateSMSApplicationUserCommand, Result<SMSApplicationUser>>
{
    private readonly ISMSApplicationUserRepository _repository;
    private readonly ILogger<AuthenticateSMSApplicationUserCommandHandler> _logger;

    public AuthenticateSMSApplicationUserCommandHandler(ISMSApplicationUserRepository repository, ILogger<AuthenticateSMSApplicationUserCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSApplicationUser>> HandleAsync(AuthenticateSMSApplicationUserCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("AuthenticateSMSApplicationUserCommand received with null request");
                return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
            }

            _logger.LogInformation("Processing authentication request for UserName: {UserName}", request.UserName);

            // Get user by username
            var userResult = await _repository.GetByUserNameAsync(request.UserName);
            if (userResult.IsFailure)
            {
                _logger.LogWarning("Authentication failed - user not found: {UserName}", request.UserName);
                return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.LoginFailed);
            }

            var user = userResult.Value;

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Authentication failed - user inactive: {UserName}", request.UserName);
                return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.BaseUserError.InactiveUser);
            }

            // Verify password
            if (!user.Authenticate(request.Password))
            {
                _logger.LogWarning("Authentication failed - invalid password for user: {UserName}", request.UserName);
                return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.LoginFailed);
            }

            // Record the login
            user.RecordLogin();
            await _repository.UpdateAsync(user);

            _logger.LogInformation("Successfully authenticated SMS Application User: {UserName}", request.UserName);
            
            return Result<SMSApplicationUser>.Success(user);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("AuthenticateSMSApplicationUserCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred during authentication for user: {UserName}", request.UserName);
            return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.LoginFailed);
        }
    }
}

public class DeleteSMSApplicationUserCommandHandler : BaseCommandBundle, IRequestHandler<DeleteSMSApplicationUserCommand, Result<bool>>
{
    private readonly ISMSApplicationUserRepository _repository;
    private readonly ILogger<DeleteSMSApplicationUserCommandHandler> _logger;

    public DeleteSMSApplicationUserCommandHandler(ISMSApplicationUserRepository repository, ILogger<DeleteSMSApplicationUserCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteSMSApplicationUserCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("DeleteSMSApplicationUserCommand received with null request");
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
            }

            _logger.LogInformation("Processing DeleteSMSApplicationUserCommand for UserID: {UserId}", request.UserId);

            var result = await _repository.DeleteAsync(request.UserId);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted SMS Application User with ID: {UserId}", request.UserId);
            }
            else
            {
                _logger.LogError("Failed to delete SMS Application User with ID: {UserId}. Error: {Error}",
                    request.UserId, result.Error?.Message);
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
            _logger.LogError(ex, "Unexpected error occurred while deleting SMS Application User with ID: {UserId}", request.UserId);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.DeleteFailed);
        }
    }
}