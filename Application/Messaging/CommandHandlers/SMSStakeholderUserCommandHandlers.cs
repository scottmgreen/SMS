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
// SMS STAKEHOLDER USER COMMAND HANDLERS
// =============================================

public class CreateSMSStakeholderUserCommandHandler : BaseCommandBundle, IRequestHandler<CreateSMSStakeholderUserCommand, Result<SMSStakeholderUser>>
{
    private readonly ISMSStakeholderUserRepository _repository;
    private readonly ILogger<CreateSMSStakeholderUserCommandHandler> _logger;

    public CreateSMSStakeholderUserCommandHandler(ISMSStakeholderUserRepository repository, ILogger<CreateSMSStakeholderUserCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSStakeholderUser>> HandleAsync(CreateSMSStakeholderUserCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("CreateSMSStakeholderUserCommand received with null request");
                return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.NullOrEmpty);
            }

            _logger.LogInformation("Processing CreateSMSStakeholderUserCommand for UserName: {UserName}", request.UserName);

            // Check if username already exists
            var existsResult = await _repository.UserNameExistsAsync(request.UserName);
            if (existsResult.IsFailure)
            {
                return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(existsResult.Error);
            }

            if (existsResult.Value)
            {
                _logger.LogWarning("Username {UserName} already exists", request.UserName);
                return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.UserNameError.AlreadyExists);
            }

            // Create value objects
            var firstNameResult = FirstName.Create(request.FirstName);
            if (firstNameResult.IsFailure)
                return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(firstNameResult.Error);

            var lastNameResult = LastName.Create(request.LastName);
            if (lastNameResult.IsFailure)
                return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(lastNameResult.Error);

            var userNameResult = UserName.Create(request.UserName);
            if (userNameResult.IsFailure)
                return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(userNameResult.Error);

            var passwordResult = Password.Create(request.Password);
            if (passwordResult.IsFailure)
                return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(passwordResult.Error);

            // Create the user
            var user = SMSStakeholderUser.Create(
                request.Code,
                firstNameResult.Value,
                lastNameResult.Value,
                userNameResult.Value,
                passwordResult.Value,
                request.StakeholderType,
                request.Organization,
                request.AccessLevel,
                request.CreatedBy);

            var result = await _repository.AddAsync(user);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created SMS Stakeholder User with ID: {Id}, UserName: {UserName}",
                    result.Value?.UserId, result.Value?.UserName);
            }
            else
            {
                _logger.LogError("Failed to create SMS Stakeholder User with UserName: {UserName}. Error: {Error}",
                    request.UserName, result.Error?.Message);
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
            _logger.LogError(ex, "Unexpected error occurred while creating SMS Stakeholder User");
            return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.CreateFailed);
        }
    }
}

public class UpdateSMSStakeholderUserCommandHandler : BaseCommandBundle, IRequestHandler<UpdateSMSStakeholderUserCommand, Result<bool>>
{
    private readonly ISMSStakeholderUserRepository _repository;
    private readonly ILogger<UpdateSMSStakeholderUserCommandHandler> _logger;

    public UpdateSMSStakeholderUserCommandHandler(ISMSStakeholderUserRepository repository, ILogger<UpdateSMSStakeholderUserCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(UpdateSMSStakeholderUserCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("UpdateSMSStakeholderUserCommand received with null request");
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.NullOrEmpty);
            }

            _logger.LogInformation("Processing UpdateSMSStakeholderUserCommand for UserID: {UserId}", request.UserId);

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
                _logger.LogInformation("Successfully updated SMS Stakeholder User with ID: {UserId}", request.UserId);
            }
            else
            {
                _logger.LogError("Failed to update SMS Stakeholder User with ID: {UserId}. Error: {Error}",
                    request.UserId, result.Error?.Message);
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
            _logger.LogError(ex, "Unexpected error occurred while updating SMS Stakeholder User with ID: {UserId}", request.UserId);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.UpdateFailed);
        }
    }
}

public class UpdateSMSStakeholderUserInfoCommandHandler : BaseCommandBundle, IRequestHandler<UpdateSMSStakeholderUserInfoCommand, Result<bool>>
{
    private readonly ISMSStakeholderUserRepository _repository;
    private readonly ILogger<UpdateSMSStakeholderUserInfoCommandHandler> _logger;

    public UpdateSMSStakeholderUserInfoCommandHandler(ISMSStakeholderUserRepository repository, ILogger<UpdateSMSStakeholderUserInfoCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(UpdateSMSStakeholderUserInfoCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("UpdateSMSStakeholderUserInfoCommand received with null request");
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.NullOrEmpty);
            }

            _logger.LogInformation("Processing UpdateSMSStakeholderUserInfoCommand for UserID: {UserId}", request.UserId);

            // Get the existing user
            var userResult = await _repository.GetByIdAsync(request.UserId);
            if (userResult.IsFailure)
            {
                return Result<bool>.Failure<bool>(userResult.Error);
            }

            var user = userResult.Value;

            // Update stakeholder information
            user.UpdateStakeholderInfo(request.StakeholderType, request.Organization, request.AccessLevel);

            var result = await _repository.UpdateAsync(user);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated stakeholder info for SMS Stakeholder User with ID: {UserId}", request.UserId);
            }
            else
            {
                _logger.LogError("Failed to update stakeholder info for SMS Stakeholder User with ID: {UserId}. Error: {Error}",
                    request.UserId, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateSMSStakeholderUserInfoCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while updating stakeholder info for SMS Stakeholder User with ID: {UserId}", request.UserId);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.UpdateFailed);
        }
    }
}

public class UpdateSMSStakeholderUserPasswordCommandHandler : BaseCommandBundle, IRequestHandler<UpdateSMSStakeholderUserPasswordCommand, Result<bool>>
{
    private readonly ISMSStakeholderUserRepository _repository;
    private readonly ILogger<UpdateSMSStakeholderUserPasswordCommandHandler> _logger;

    public UpdateSMSStakeholderUserPasswordCommandHandler(ISMSStakeholderUserRepository repository, ILogger<UpdateSMSStakeholderUserPasswordCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(UpdateSMSStakeholderUserPasswordCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("UpdateSMSStakeholderUserPasswordCommand received with null request");
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.NullOrEmpty);
            }

            _logger.LogInformation("Processing UpdateSMSStakeholderUserPasswordCommand for UserID: {UserId}", request.UserId);

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
                _logger.LogInformation("Successfully updated password for SMS Stakeholder User with ID: {UserId}", request.UserId);
            }
            else
            {
                _logger.LogError("Failed to update password for SMS Stakeholder User with ID: {UserId}. Error: {Error}",
                    request.UserId, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateSMSStakeholderUserPasswordCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while updating password for SMS Stakeholder User with ID: {UserId}", request.UserId);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.PasswordUpdateFailed);
        }
    }
}

public class AuthenticateSMSStakeholderUserCommandHandler : BaseCommandBundle, IRequestHandler<AuthenticateSMSStakeholderUserCommand, Result<SMSStakeholderUser>>
{
    private readonly ISMSStakeholderUserRepository _repository;
    private readonly ILogger<AuthenticateSMSStakeholderUserCommandHandler> _logger;

    public AuthenticateSMSStakeholderUserCommandHandler(ISMSStakeholderUserRepository repository, ILogger<AuthenticateSMSStakeholderUserCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSStakeholderUser>> HandleAsync(AuthenticateSMSStakeholderUserCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("AuthenticateSMSStakeholderUserCommand received with null request");
                return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.NullOrEmpty);
            }

            _logger.LogInformation("Processing authentication request for UserName: {UserName}", request.UserName);

            // Get user by username
            var userResult = await _repository.GetByUserNameAsync(request.UserName);
            if (userResult.IsFailure)
            {
                _logger.LogWarning("Authentication failed - user not found: {UserName}", request.UserName);
                return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.LoginFailed);
            }

            var user = userResult.Value;

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Authentication failed - user inactive: {UserName}", request.UserName);
                return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.BaseUserError.InactiveUser);
            }

            // Verify password
            if (!user.Authenticate(request.Password))
            {
                _logger.LogWarning("Authentication failed - invalid password for user: {UserName}", request.UserName);
                return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.LoginFailed);
            }

            // Record the login
            user.RecordLogin();
            await _repository.UpdateAsync(user);

            _logger.LogInformation("Successfully authenticated SMS Stakeholder User: {UserName}", request.UserName);
            
            return Result<SMSStakeholderUser>.Success(user);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("AuthenticateSMSStakeholderUserCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred during authentication for user: {UserName}", request.UserName);
            return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.LoginFailed);
        }
    }
}

public class DeleteSMSStakeholderUserCommandHandler : BaseCommandBundle, IRequestHandler<DeleteSMSStakeholderUserCommand, Result<bool>>
{
    private readonly ISMSStakeholderUserRepository _repository;
    private readonly ILogger<DeleteSMSStakeholderUserCommandHandler> _logger;

    public DeleteSMSStakeholderUserCommandHandler(ISMSStakeholderUserRepository repository, ILogger<DeleteSMSStakeholderUserCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteSMSStakeholderUserCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request is null)
            {
                _logger.LogError("DeleteSMSStakeholderUserCommand received with null request");
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.NullOrEmpty);
            }

            _logger.LogInformation("Processing DeleteSMSStakeholderUserCommand for UserID: {UserId}", request.UserId);

            var result = await _repository.DeleteAsync(request.UserId);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted SMS Stakeholder User with ID: {UserId}", request.UserId);
            }
            else
            {
                _logger.LogError("Failed to delete SMS Stakeholder User with ID: {UserId}. Error: {Error}",
                    request.UserId, result.Error?.Message);
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
            _logger.LogError(ex, "Unexpected error occurred while deleting SMS Stakeholder User with ID: {UserId}", request.UserId);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.DeleteFailed);
        }
    }
}