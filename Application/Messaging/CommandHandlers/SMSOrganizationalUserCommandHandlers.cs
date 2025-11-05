using Microsoft.Extensions.Logging;
using SMS_Application.Common;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;
using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Domain.Interfaces;
using SMS_Domain.ValueObjects;
using SMS_Shared.Common;

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// SMS ORGANIZATIONAL USER COMMAND HANDLERS
// =============================================

public class CreateSMSOrganizationalUserCommandHandler : BaseCommandBundle, IRequestHandler<CreateSMSOrganizationalUserCommand, Result<SMSOrganizationalUser>>
{
    private readonly ISMSOrganizationalUserRepository _repository;
    private readonly ILogger<CreateSMSOrganizationalUserCommandHandler> _logger;

    public CreateSMSOrganizationalUserCommandHandler(ISMSOrganizationalUserRepository repository, ILogger<CreateSMSOrganizationalUserCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSOrganizationalUser>> HandleAsync(CreateSMSOrganizationalUserCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("CreateSMSOrganizationalUserCommand received with null request");
                return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.NullOrEmpty);
            }

            _logger.LogInformation("Processing CreateSMSOrganizationalUserCommand for UserName: {UserName}", request.UserName);

            // Check if username already exists
            var existsResult = await _repository.UserNameExistsAsync(request.UserName);
            if (existsResult.IsFailure)
            {
                return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(existsResult.Error);
            }

            if (existsResult.Value)
            {
                _logger.LogWarning("Username {UserName} already exists", request.UserName);
                return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.UserNameError.AlreadyExists);
            }

            // Create value objects
            var firstNameResult = FirstName.Create(request.FirstName);
            if (firstNameResult.IsFailure)
                return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(firstNameResult.Error);

            var lastNameResult = LastName.Create(request.LastName);
            if (lastNameResult.IsFailure)
                return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(lastNameResult.Error);

            var userNameResult = UserName.Create(request.UserName);
            if (userNameResult.IsFailure)
                return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(userNameResult.Error);

            var passwordResult = Password.Create(request.Password);
            if (passwordResult.IsFailure)
                return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(passwordResult.Error);

            // Create the user
            var user = SMSOrganizationalUser.Create(
                request.Code,
                firstNameResult.Value,
                lastNameResult.Value,
                userNameResult.Value,
                passwordResult.Value,
                request.Department,
                request.Position,
                request.OrganizationLevel,
                request.CreatedBy);

            var result = await _repository.AddAsync(user);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created SMS Organizational User with ID: {Id}, UserName: {UserName}",
                    result.Value?.UserId, result.Value?.UserName);
            }
            else
            {
                _logger.LogError("Failed to create SMS Organizational User with UserName: {UserName}. Error: {Error}",
                    request.UserName, result.Error?.Message);
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

public class UpdateSMSOrganizationalUserCommandHandler : BaseCommandBundle, IRequestHandler<UpdateSMSOrganizationalUserCommand, Result<bool>>
{
    private readonly ISMSOrganizationalUserRepository _repository;
    private readonly ILogger<UpdateSMSOrganizationalUserCommandHandler> _logger;

    public UpdateSMSOrganizationalUserCommandHandler(ISMSOrganizationalUserRepository repository, ILogger<UpdateSMSOrganizationalUserCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(UpdateSMSOrganizationalUserCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("UpdateSMSOrganizationalUserCommand received with null request");
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.NullOrEmpty);
            }

            _logger.LogInformation("Processing UpdateSMSOrganizationalUserCommand for UserID: {UserId}", request.UserId);

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
                _logger.LogInformation("Successfully updated SMS Organizational User with ID: {UserId}", request.UserId);
            }
            else
            {
                _logger.LogError("Failed to update SMS Organizational User with ID: {UserId}. Error: {Error}",
                    request.UserId, result.Error?.Message);
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
            _logger.LogError(ex, "Unexpected error occurred while updating SMS Organizational User with ID: {UserId}", request.UserId);
            return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.UpdateFailed);
        }
    }
}

public class UpdateSMSOrganizationalUserInfoCommandHandler : BaseCommandBundle, IRequestHandler<UpdateSMSOrganizationalUserInfoCommand, Result<bool>>
{
    private readonly ISMSOrganizationalUserRepository _repository;
    private readonly ILogger<UpdateSMSOrganizationalUserInfoCommandHandler> _logger;

    public UpdateSMSOrganizationalUserInfoCommandHandler(ISMSOrganizationalUserRepository repository, ILogger<UpdateSMSOrganizationalUserInfoCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(UpdateSMSOrganizationalUserInfoCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("UpdateSMSOrganizationalUserInfoCommand received with null request");
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.NullOrEmpty);
            }

            _logger.LogInformation("Processing UpdateSMSOrganizationalUserInfoCommand for UserID: {UserId}", request.UserId);

            // Get the existing user
            var userResult = await _repository.GetByIdAsync(request.UserId);
            if (userResult.IsFailure)
            {
                return Result<bool>.Failure<bool>(userResult.Error);
            }

            var user = userResult.Value;

            // Update organizational information
            user.UpdateOrganizationalInfo(request.Department, request.Position, request.OrganizationLevel);

            var result = await _repository.UpdateAsync(user);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated organizational info for SMS Organizational User with ID: {UserId}", request.UserId);
            }
            else
            {
                _logger.LogError("Failed to update organizational info for SMS Organizational User with ID: {UserId}. Error: {Error}",
                    request.UserId, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateSMSOrganizationalUserInfoCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while updating organizational info for SMS Organizational User with ID: {UserId}", request.UserId);
            return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.UpdateFailed);
        }
    }
}

public class UpdateSMSOrganizationalUserPasswordCommandHandler : BaseCommandBundle, IRequestHandler<UpdateSMSOrganizationalUserPasswordCommand, Result<bool>>
{
    private readonly ISMSOrganizationalUserRepository _repository;
    private readonly ILogger<UpdateSMSOrganizationalUserPasswordCommandHandler> _logger;

    public UpdateSMSOrganizationalUserPasswordCommandHandler(ISMSOrganizationalUserRepository repository, ILogger<UpdateSMSOrganizationalUserPasswordCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(UpdateSMSOrganizationalUserPasswordCommand request, CancellationToken ct = default)
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
                _logger.LogInformation("Successfully updated password for SMS Organizational User with ID: {UserId}", request.UserId);
            }
            else
            {
                _logger.LogError("Failed to update password for SMS Organizational User with ID: {UserId}. Error: {Error}",
                    request.UserId, result.Error?.Message);
            }

            return result;
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

public class AuthenticateSMSOrganizationalUserCommandHandler : BaseCommandBundle, IRequestHandler<AuthenticateSMSOrganizationalUserCommand, Result<SMSOrganizationalUser>>
{
    private readonly ISMSOrganizationalUserRepository _repository;
    private readonly ILogger<AuthenticateSMSOrganizationalUserCommandHandler> _logger;

    public AuthenticateSMSOrganizationalUserCommandHandler(ISMSOrganizationalUserRepository repository, ILogger<AuthenticateSMSOrganizationalUserCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSOrganizationalUser>> HandleAsync(AuthenticateSMSOrganizationalUserCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("AuthenticateSMSOrganizationalUserCommand received with null request");
                return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.NullOrEmpty);
            }

            _logger.LogInformation("Processing authentication request for UserName: {UserName}", request.UserName);

            // Get user by username
            var userResult = await _repository.GetByUserNameAsync(request.UserName);
            if (userResult.IsFailure)
            {
                _logger.LogWarning("Authentication failed - user not found: {UserName}", request.UserName);
                return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.LoginFailed);
            }

            var user = userResult.Value;

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Authentication failed - user inactive: {UserName}", request.UserName);
                return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.BaseUserError.InactiveUser);
            }

            // Verify password
            if (!user.Authenticate(request.Password))
            {
                _logger.LogWarning("Authentication failed - invalid password for user: {UserName}", request.UserName);
                return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.LoginFailed);
            }

            // Record the login
            user.RecordLogin();
            await _repository.UpdateAsync(user);

            _logger.LogInformation("Successfully authenticated SMS Organizational User: {UserName}", request.UserName);
            
            return Result<SMSOrganizationalUser>.Success(user);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("AuthenticateSMSOrganizationalUserCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred during authentication for user: {UserName}", request.UserName);
            return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.LoginFailed);
        }
    }
}

public class DeleteSMSOrganizationalUserCommandHandler : BaseCommandBundle, IRequestHandler<DeleteSMSOrganizationalUserCommand, Result<bool>>
{
    private readonly ISMSOrganizationalUserRepository _repository;
    private readonly ILogger<DeleteSMSOrganizationalUserCommandHandler> _logger;

    public DeleteSMSOrganizationalUserCommandHandler(ISMSOrganizationalUserRepository repository, ILogger<DeleteSMSOrganizationalUserCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteSMSOrganizationalUserCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("DeleteSMSOrganizationalUserCommand received with null request");
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.NullOrEmpty);
            }

            _logger.LogInformation("Processing DeleteSMSOrganizationalUserCommand for UserID: {UserId}", request.UserId);

            var result = await _repository.DeleteAsync(request.UserId);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted SMS Organizational User with ID: {UserId}", request.UserId);
            }
            else
            {
                _logger.LogError("Failed to delete SMS Organizational User with ID: {UserId}. Error: {Error}",
                    request.UserId, result.Error?.Message);
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
            _logger.LogError(ex, "Unexpected error occurred while deleting SMS Organizational User with ID: {UserId}", request.UserId);
            return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.DeleteFailed);
        }
    }
}