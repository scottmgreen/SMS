using Microsoft.Extensions.Logging;
using SMS_Application.Common;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Queries;
using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Domain.Interfaces;

using SMS_Infrastructure.Interfaces;

using SMS_Shared.Common;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// SMS APPLICATION USER QUERY HANDLERS
// =============================================

public class GetAllSMSApplicationUsersQueryHandler : BaseQueryBundle, IRequestHandler<GetAllSMSApplicationUsersQuery, Result<IEnumerable<SMSApplicationUser>>>
{
    private readonly ISMSApplicationUserRepository _repository;
    private readonly ILogger<GetAllSMSApplicationUsersQueryHandler> _logger;

    public GetAllSMSApplicationUsersQueryHandler(ISMSApplicationUserRepository repository, ILogger<GetAllSMSApplicationUsersQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSApplicationUser>>> HandleAsync(GetAllSMSApplicationUsersQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetAllSMSApplicationUsersQuery");
            var result = await _repository.GetAllAsync();
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} SMS Application Users", result.Value?.Count() ?? 0);
            }
            else
            {
                _logger.LogWarning("Failed to retrieve SMS Application Users: {Error}", result.Error?.Message);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetAllSMSApplicationUsersQuery");
            return Result<IEnumerable<SMSApplicationUser>>.Failure<IEnumerable<SMSApplicationUser>>(DomainErrors.SMSApplicationUserError.NotFound);
        }
    }
}

public class GetSMSApplicationUserByIdQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSApplicationUserByIdQuery, Result<SMSApplicationUser>>
{
    private readonly ISMSApplicationUserRepository _repository;
    private readonly ILogger<GetSMSApplicationUserByIdQueryHandler> _logger;

    public GetSMSApplicationUserByIdQueryHandler(ISMSApplicationUserRepository repository, ILogger<GetSMSApplicationUserByIdQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSApplicationUser>> HandleAsync(GetSMSApplicationUserByIdQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSApplicationUserByIdQuery for ID: {UserId}", request.UserId);
            var result = await _repository.GetByIdAsync(request.UserId);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved SMS Application User with ID: {UserId}", request.UserId);
            }
            else
            {
                _logger.LogWarning("SMS Application User not found with ID: {UserId}", request.UserId);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetSMSApplicationUserByIdQuery for ID: {UserId}", request.UserId);
            return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.NotFound);
        }
    }
}

public class GetSMSApplicationUserByUserNameQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSApplicationUserByUserNameQuery, Result<SMSApplicationUser>>
{
    private readonly ISMSApplicationUserRepository _repository;
    private readonly ILogger<GetSMSApplicationUserByUserNameQueryHandler> _logger;

    public GetSMSApplicationUserByUserNameQueryHandler(ISMSApplicationUserRepository repository, ILogger<GetSMSApplicationUserByUserNameQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSApplicationUser>> HandleAsync(GetSMSApplicationUserByUserNameQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSApplicationUserByUserNameQuery for UserName: {UserName}", request.UserName);
            var result = await _repository.GetByUserNameAsync(request.UserName);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved SMS Application User with UserName: {UserName}", request.UserName);
            }
            else
            {
                _logger.LogWarning("SMS Application User not found with UserName: {UserName}", request.UserName);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetSMSApplicationUserByUserNameQuery for UserName: {UserName}", request.UserName);
            return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.NotFound);
        }
    }
}

public class GetActiveSMSApplicationUsersQueryHandler : BaseQueryBundle, IRequestHandler<GetActiveSMSApplicationUsersQuery, Result<IEnumerable<SMSApplicationUser>>>
{
    private readonly ISMSApplicationUserRepository _repository;
    private readonly ILogger<GetActiveSMSApplicationUsersQueryHandler> _logger;

    public GetActiveSMSApplicationUsersQueryHandler(ISMSApplicationUserRepository repository, ILogger<GetActiveSMSApplicationUsersQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSApplicationUser>>> HandleAsync(GetActiveSMSApplicationUsersQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetActiveSMSApplicationUsersQuery");
            var result = await _repository.GetActiveUsersAsync();
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} active SMS Application Users", result.Value?.Count() ?? 0);
            }
            else
            {
                _logger.LogWarning("Failed to retrieve active SMS Application Users: {Error}", result.Error?.Message);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetActiveSMSApplicationUsersQuery");
            return Result<IEnumerable<SMSApplicationUser>>.Failure<IEnumerable<SMSApplicationUser>>(DomainErrors.SMSApplicationUserError.NotFound);
        }
    }
}

public class GetSMSApplicationUsersByRoleQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSApplicationUsersByRoleQuery, Result<IEnumerable<SMSApplicationUser>>>
{
    private readonly ISMSApplicationUserRepository _repository;
    private readonly ILogger<GetSMSApplicationUsersByRoleQueryHandler> _logger;

    public GetSMSApplicationUsersByRoleQueryHandler(ISMSApplicationUserRepository repository, ILogger<GetSMSApplicationUsersByRoleQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSApplicationUser>>> HandleAsync(GetSMSApplicationUsersByRoleQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSApplicationUsersByRoleQuery for Role: {ApplicationRole}", request.ApplicationRole);
            var result = await _repository.GetByApplicationRoleAsync(request.ApplicationRole);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} SMS Application Users with Role: {ApplicationRole}", 
                    result.Value?.Count() ?? 0, request.ApplicationRole);
            }
            else
            {
                _logger.LogWarning("Failed to retrieve SMS Application Users with Role: {ApplicationRole}: {Error}", 
                    request.ApplicationRole, result.Error?.Message);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetSMSApplicationUsersByRoleQuery for Role: {ApplicationRole}", request.ApplicationRole);
            return Result<IEnumerable<SMSApplicationUser>>.Failure<IEnumerable<SMSApplicationUser>>(DomainErrors.SMSApplicationUserError.NotFound);
        }
    }
}

public class GetSMSApplicationUsersByPermissionLevelQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSApplicationUsersByPermissionLevelQuery, Result<IEnumerable<SMSApplicationUser>>>
{
    private readonly ISMSApplicationUserRepository _repository;
    private readonly ILogger<GetSMSApplicationUsersByPermissionLevelQueryHandler> _logger;

    public GetSMSApplicationUsersByPermissionLevelQueryHandler(ISMSApplicationUserRepository repository, ILogger<GetSMSApplicationUsersByPermissionLevelQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSApplicationUser>>> HandleAsync(GetSMSApplicationUsersByPermissionLevelQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSApplicationUsersByPermissionLevelQuery for Permission Level: {PermissionLevel}", request.PermissionLevel);
            var result = await _repository.GetByPermissionLevelAsync(request.PermissionLevel);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} SMS Application Users with Permission Level: {PermissionLevel}", 
                    result.Value?.Count() ?? 0, request.PermissionLevel);
            }
            else
            {
                _logger.LogWarning("Failed to retrieve SMS Application Users with Permission Level: {PermissionLevel}: {Error}", 
                    request.PermissionLevel, result.Error?.Message);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetSMSApplicationUsersByPermissionLevelQuery for Permission Level: {PermissionLevel}", request.PermissionLevel);
            return Result<IEnumerable<SMSApplicationUser>>.Failure<IEnumerable<SMSApplicationUser>>(DomainErrors.SMSApplicationUserError.NotFound);
        }
    }
}

public class CheckSMSApplicationUserNameExistsQueryHandler : BaseQueryBundle, IRequestHandler<CheckSMSApplicationUserNameExistsQuery, Result<bool>>
{
    private readonly ISMSApplicationUserRepository _repository;
    private readonly ILogger<CheckSMSApplicationUserNameExistsQueryHandler> _logger;

    public CheckSMSApplicationUserNameExistsQueryHandler(ISMSApplicationUserRepository repository, ILogger<CheckSMSApplicationUserNameExistsQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(CheckSMSApplicationUserNameExistsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing CheckSMSApplicationUserNameExistsQuery for UserName: {UserName}", request.UserName);
            var result = await _repository.UserNameExistsAsync(request.UserName);
            
            _logger.LogInformation("Username {UserName} exists: {Exists}", request.UserName, result.Value);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing CheckSMSApplicationUserNameExistsQuery for UserName: {UserName}", request.UserName);
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

public class ValidateSMSApplicationUserCredentialsQueryHandler : BaseQueryBundle, IRequestHandler<ValidateSMSApplicationUserCredentialsQuery, Result<bool>>
{
    private readonly ISMSApplicationUserRepository _repository;
    private readonly ILogger<ValidateSMSApplicationUserCredentialsQueryHandler> _logger;

    public ValidateSMSApplicationUserCredentialsQueryHandler(ISMSApplicationUserRepository repository, ILogger<ValidateSMSApplicationUserCredentialsQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(ValidateSMSApplicationUserCredentialsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing ValidateSMSApplicationUserCredentialsQuery for UserName: {UserName}", request.UserName);
            
            var userResult = await _repository.GetByUserNameAsync(request.UserName);
            if (userResult.IsFailure)
            {
                _logger.LogWarning("User not found for credential validation: {UserName}", request.UserName);
                return Result<bool>.Success(false);
            }

            var user = userResult.Value;
            var isValid = user.IsActive && user.Authenticate(request.Password);
            
            _logger.LogInformation("Credential validation for {UserName}: {IsValid}", request.UserName, isValid);
            
            return Result<bool>.Success(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ValidateSMSApplicationUserCredentialsQuery for UserName: {UserName}", request.UserName);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.LoginFailed);
        }
    }
}

public class GetSMSApplicationUserStatisticsQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSApplicationUserStatisticsQuery, Result<UserStatistics>>
{
    private readonly ISMSApplicationUserRepository _repository;
    private readonly ILogger<GetSMSApplicationUserStatisticsQueryHandler> _logger;

    public GetSMSApplicationUserStatisticsQueryHandler(ISMSApplicationUserRepository repository, ILogger<GetSMSApplicationUserStatisticsQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<UserStatistics>> HandleAsync(GetSMSApplicationUserStatisticsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSApplicationUserStatisticsQuery");
            var result = await _repository.GetUserStatisticsAsync();
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved SMS Application User statistics");
            }
            else
            {
                _logger.LogWarning("Failed to retrieve SMS Application User statistics: {Error}", result.Error?.Message);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetSMSApplicationUserStatisticsQuery");
            return Result<UserStatistics>.Failure<UserStatistics>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}