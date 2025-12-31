using Microsoft.Extensions.Logging;
using SMS_Application.Common;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Queries;
using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Infrastructure.Services;
using SMS_Shared.Common;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// SMS APPLICATION USER QUERY HANDLERS
// =============================================

public class GetAllSMSApplicationUsersQueryHandler : BaseQueryBundle, IRequestHandler<GetAllSMSApplicationUsersQuery, Result<IEnumerable<SMSApplicationUser>>>
{
    private readonly SMSApplicationUserDataService _dataService;
    private readonly ILogger<GetAllSMSApplicationUsersQueryHandler> _logger;

    public GetAllSMSApplicationUsersQueryHandler(SMSApplicationUserDataService dataService, ILogger<GetAllSMSApplicationUsersQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSApplicationUser>>> HandleAsync(GetAllSMSApplicationUsersQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetAllSMSApplicationUsersQuery");
            var result = await _dataService.GetAllSMSApplicationUsersAsync(ct);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} SMS Application Users", result.Value?.Count() ?? 0);
            }
            else
            {
                _logger.LogWarning("Failed to retrieve SMS Application Users");
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
    private readonly SMSApplicationUserDataService _dataService;
    private readonly ILogger<GetSMSApplicationUserByIdQueryHandler> _logger;

    public GetSMSApplicationUserByIdQueryHandler(SMSApplicationUserDataService dataService, ILogger<GetSMSApplicationUserByIdQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSApplicationUser>> HandleAsync(GetSMSApplicationUserByIdQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSApplicationUserByIdQuery for ID: {UserId}", request.UserId);
            var result = await _dataService.GetSMSApplicationUserByIdAsync(request.UserId, ct);
            
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

public class GetSMSApplicationUserByCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSApplicationUserByCodeQuery, Result<SMSApplicationUser>>
{
    private readonly SMSApplicationUserDataService _dataService;
    private readonly ILogger<GetSMSApplicationUserByCodeQueryHandler> _logger;

    public GetSMSApplicationUserByCodeQueryHandler(SMSApplicationUserDataService dataService, ILogger<GetSMSApplicationUserByCodeQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSApplicationUser>> HandleAsync(GetSMSApplicationUserByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSApplicationUserByCodeQuery for Code: {UserCode}", request.UserCode);
            var result = await _dataService.GetSMSApplicationUserByIdAsync(request.UserCode, ct); // Assuming Code and ID are the same
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved SMS Application User with Code: {UserCode}", request.UserCode);
            }
            else
            {
                _logger.LogWarning("SMS Application User not found with Code: {UserCode}", request.UserCode);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetSMSApplicationUserByCodeQuery for Code: {UserCode}", request.UserCode);
            return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.NotFound);
        }
    }
}

public class GetSMSApplicationUserByUserNameQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSApplicationUserByUserNameQuery, Result<SMSApplicationUser>>
{
    private readonly SMSApplicationUserDataService _dataService;
    private readonly ILogger<GetSMSApplicationUserByUserNameQueryHandler> _logger;

    public GetSMSApplicationUserByUserNameQueryHandler(SMSApplicationUserDataService dataService, ILogger<GetSMSApplicationUserByUserNameQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSApplicationUser>> HandleAsync(GetSMSApplicationUserByUserNameQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSApplicationUserByUserNameQuery for UserName: {UserName}", request.UserName);
            
            // Get all users and filter by username (or implement a specific method in the service)
            var allUsersResult = await _dataService.GetAllSMSApplicationUsersAsync(ct);
            if (allUsersResult.IsFailure)
            {
                return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(allUsersResult.Error);
            }

            var user = allUsersResult.Value?.FirstOrDefault(u => u.UserName.Value.Equals(request.UserName, StringComparison.OrdinalIgnoreCase));
            
            if (user != null)
            {
                _logger.LogInformation("Successfully retrieved SMS Application User with UserName: {UserName}", request.UserName);
                return Result<SMSApplicationUser>.Success(user);
            }
            else
            {
                _logger.LogWarning("SMS Application User not found with UserName: {UserName}", request.UserName);
                return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.NotFound);
            }
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
    private readonly SMSApplicationUserDataService _dataService;
    private readonly ILogger<GetActiveSMSApplicationUsersQueryHandler> _logger;

    public GetActiveSMSApplicationUsersQueryHandler(SMSApplicationUserDataService dataService, ILogger<GetActiveSMSApplicationUsersQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSApplicationUser>>> HandleAsync(GetActiveSMSApplicationUsersQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetActiveSMSApplicationUsersQuery");
            var result = await _dataService.GetActiveSMSApplicationUsersAsync(ct);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} active SMS Application Users", result.Value?.Count() ?? 0);
            }
            else
            {
                _logger.LogWarning("Failed to retrieve active SMS Application Users");
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

//public class GetSMSApplicationUsersByPermissionLevelQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSApplicationUsersByPermissionLevelQuery, Result<IEnumerable<SMSApplicationUser>>>
//{
//    private readonly ISMSApplicationUserRepository _repository;
//    private readonly ILogger<GetSMSApplicationUsersByPermissionLevelQueryHandler> _logger;

//    public GetSMSApplicationUsersByPermissionLevelQueryHandler(ISMSApplicationUserRepository repository, ILogger<GetSMSApplicationUsersByPermissionLevelQueryHandler> logger)
//    {
//        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
//        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//    }

//    public async Task<Result<IEnumerable<SMSApplicationUser>>> HandleAsync(GetSMSApplicationUsersByPermissionLevelQuery request, CancellationToken ct = default)
//    {
//        try
//        {
//            _logger.LogInformation("Processing GetSMSApplicationUsersByPermissionLevelQuery for Permission Level: {PermissionLevel}", request.PermissionLevel);
//            var result = await _repository.GetSMSApplicationUserByPermissionLevelAsync(request.PermissionLevel);
            
//            if (result.IsSuccess)
//            {
//                _logger.LogInformation("Successfully retrieved {Count} SMS Application Users with Permission Level: {PermissionLevel}", 
//                    result.Value?.Count() ?? 0, request.PermissionLevel);
//            }
//            else
//            {
//                _logger.LogWarning("Failed to retrieve SMS Application Users with Permission Level: {PermissionLevel}: {Error}", 
//                    request.PermissionLevel, result.Error?.Message);
//            }
            
//            return result;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error processing GetSMSApplicationUsersByPermissionLevelQuery for Permission Level: {PermissionLevel}", request.PermissionLevel);
//            return Result<IEnumerable<SMSApplicationUser>>.Failure<IEnumerable<SMSApplicationUser>>(DomainErrors.SMSApplicationUserError.NotFound);
//        }
//    }
//}

//public class GetSMSApplicationUsersWithMinimumPermissionQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSApplicationUsersWithMinimumPermissionQuery, Result<IEnumerable<SMSApplicationUser>>>
//{
//    private readonly ISMSApplicationUserRepository _repository;
//    private readonly ILogger<GetSMSApplicationUsersWithMinimumPermissionQueryHandler> _logger;

//    public GetSMSApplicationUsersWithMinimumPermissionQueryHandler(ISMSApplicationUserRepository repository, ILogger<GetSMSApplicationUsersWithMinimumPermissionQueryHandler> logger)
//    {
//        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
//        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//    }

//    public async Task<Result<IEnumerable<SMSApplicationUser>>> HandleAsync(GetSMSApplicationUsersWithMinimumPermissionQuery request, CancellationToken ct = default)
//    {
//        try
//        {
//            _logger.LogInformation("Processing GetSMSApplicationUsersWithMinimumPermissionQuery for Minimum Permission: {MinimumPermissionLevel}", request.MinimumPermissionLevel);
            
//            // Get all users and filter by minimum permission level
//            var allUsersResult = await _repository.GetAllAsync();
//            if (allUsersResult.IsSuccess)
//            {
//                var levels = new[] { "Read", "Write", "Admin", "System" };
//                var requiredLevelIndex = Array.IndexOf(levels, request.MinimumPermissionLevel);
                
//                var filteredUsers = allUsersResult.Value?.Where(u =>
//                {
//                    var userLevelIndex = Array.IndexOf(levels, u.PermissionLevel);
//                    return userLevelIndex >= requiredLevelIndex;
//                }) ?? new List<SMSApplicationUser>();
                
//                _logger.LogInformation("Successfully retrieved {Count} SMS Application Users with minimum permission: {MinimumPermissionLevel}", 
//                    filteredUsers.Count(), request.MinimumPermissionLevel);
//                return Result<IEnumerable<SMSApplicationUser>>.Success<IEnumerable<SMSApplicationUser>>(filteredUsers);
//            }
//            else
//            {
//                _logger.LogWarning("Failed to retrieve SMS Application Users for minimum permission filtering: {Error}", allUsersResult.Error?.Message);
//                return allUsersResult;
//            }
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error processing GetSMSApplicationUsersWithMinimumPermissionQuery for Minimum Permission: {MinimumPermissionLevel}", request.MinimumPermissionLevel);
//            return Result<IEnumerable<SMSApplicationUser>>.Failure<IEnumerable<SMSApplicationUser>>(DomainErrors.SMSApplicationUserError.NotFound);
//        }
//    }
//}

public class CheckSMSApplicationUserNameExistsQueryHandler : BaseQueryBundle, IRequestHandler<CheckSMSApplicationUserNameExistsQuery, Result<bool>>
{
    private readonly SMSApplicationUserDataService _dataService;
    private readonly ILogger<CheckSMSApplicationUserNameExistsQueryHandler> _logger;

    public CheckSMSApplicationUserNameExistsQueryHandler(SMSApplicationUserDataService dataService, ILogger<CheckSMSApplicationUserNameExistsQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(CheckSMSApplicationUserNameExistsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing CheckSMSApplicationUserNameExistsQuery for UserName: {UserName}", request.UserName);
            var result = await _dataService.GetAllSMSApplicationUsersAsync(); //request.UserName);
            var checkresult = result.Value.Any(x => x.UserName.Value == request.UserName);


            _logger.LogInformation("Username {UserName} exists: {Exists}", request.UserName, result.Value);
            
            return checkresult;
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
    private readonly SMSApplicationUserDataService _dataService;
    private readonly ILogger<ValidateSMSApplicationUserCredentialsQueryHandler> _logger;

    public ValidateSMSApplicationUserCredentialsQueryHandler(SMSApplicationUserDataService dataService, ILogger<ValidateSMSApplicationUserCredentialsQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(ValidateSMSApplicationUserCredentialsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing ValidateSMSApplicationUserCredentialsQuery for UserName: {UserName}", request.UserName);
            
            var userResult = await _dataService.GetSMSApplicationUserByUserNameAsync(request.UserName);
            if (userResult.IsFailure)
            {
                _logger.LogWarning("User not found for credential validation: {UserName}", request.UserName);
                return Result<bool>.Success(false);
            }

            var user = userResult.Value;
            var isValid = user.IsActive && user.Authenticate(request.Password);

            user.UpdatedBy = "SYSTEM";
            user.LastLoginDate = DateTime.UtcNow;
            await _dataService.UpdateSMSApplicationUserAsync(user, ct);


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

public class GetSMSApplicationUsersRequiringPasswordChangeQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSApplicationUsersRequiringPasswordChangeQuery, Result<IEnumerable<SMSApplicationUser>>>
{
    private readonly SMSApplicationUserDataService _dataService;
    private readonly ILogger<GetSMSApplicationUsersRequiringPasswordChangeQueryHandler> _logger;

    public GetSMSApplicationUsersRequiringPasswordChangeQueryHandler(SMSApplicationUserDataService dataService, ILogger<GetSMSApplicationUsersRequiringPasswordChangeQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSApplicationUser>>> HandleAsync(GetSMSApplicationUsersRequiringPasswordChangeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSApplicationUsersRequiringPasswordChangeQuery");
            
            var allUsersResult = await _dataService.GetAllSMSApplicationUsersAsync(ct);
            if (allUsersResult.IsSuccess)
            {
                var usersRequiringChange = allUsersResult.Value?.Where(u => u.RequiresPasswordChange) ?? new List<SMSApplicationUser>();
                _logger.LogInformation("Successfully retrieved {Count} users requiring password change", usersRequiringChange.Count());
                return Result<IEnumerable<SMSApplicationUser>>.Success<IEnumerable<SMSApplicationUser>>(usersRequiringChange);
            }
            else
            {
                _logger.LogWarning("Failed to retrieve users requiring password change: {Error}", allUsersResult.Error?.Message);
                return allUsersResult;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetSMSApplicationUsersRequiringPasswordChangeQuery");
            return Result<IEnumerable<SMSApplicationUser>>.Failure<IEnumerable<SMSApplicationUser>>(DomainErrors.SMSApplicationUserError.NotFound);
        }
    }
}

public class GetStaleSMSApplicationUsersQueryHandler : BaseQueryBundle, IRequestHandler<GetStaleSMSApplicationUsersQuery, Result<IEnumerable<SMSApplicationUser>>>
{
    private readonly SMSApplicationUserDataService _dataService;
    private readonly ILogger<GetStaleSMSApplicationUsersQueryHandler> _logger;

    public GetStaleSMSApplicationUsersQueryHandler(SMSApplicationUserDataService dataService, ILogger<GetStaleSMSApplicationUsersQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSApplicationUser>>> HandleAsync(GetStaleSMSApplicationUsersQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetStaleSMSApplicationUsersQuery for {StaleDays} days", request.StaleDays);
            
            var allUsersResult = await _dataService.GetAllSMSApplicationUsersAsync(ct);
            if (allUsersResult.IsSuccess)
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-request.StaleDays);
                var staleUsers = allUsersResult.Value?.Where(u => 
                    !u.LastLoginDate.HasValue || u.LastLoginDate < cutoffDate) ?? new List<SMSApplicationUser>();
                
                _logger.LogInformation("Successfully retrieved {Count} stale users (>{StaleDays} days)", staleUsers.Count(), request.StaleDays);
                return Result<IEnumerable<SMSApplicationUser>>.Success<IEnumerable<SMSApplicationUser>>(staleUsers);
            }
            else
            {
                _logger.LogWarning("Failed to retrieve stale users: {Error}", allUsersResult.Error?.Message);
                return allUsersResult;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetStaleSMSApplicationUsersQuery for {StaleDays} days", request.StaleDays);
            return Result<IEnumerable<SMSApplicationUser>>.Failure<IEnumerable<SMSApplicationUser>>(DomainErrors.SMSApplicationUserError.NotFound);
        }
    }
}

public class GetSMSApplicationUserStatisticsQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSApplicationUserStatisticsQuery, Result<Dictionary<string, object>>>
{
    private readonly SMSApplicationUserDataService _dataService;
    private readonly ILogger<GetSMSApplicationUserStatisticsQueryHandler> _logger;

    public GetSMSApplicationUserStatisticsQueryHandler(SMSApplicationUserDataService dataService, ILogger<GetSMSApplicationUserStatisticsQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Dictionary<string, object>>> HandleAsync(GetSMSApplicationUserStatisticsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSApplicationUserStatisticsQuery");
            
            var statsResult = await _dataService.GetSMSApplicationUserStatisticsAsync();
            if (statsResult.IsSuccess)
            {
                var userStats = statsResult.Value;
                var stats = new Dictionary<string, object>
                {
                    ["TotalUsers"] = userStats.TotalUsers,
                    ["ActiveUsers"] = userStats.ActiveUsers,
                    ["InactiveUsers"] = userStats.InactiveUsers,
                    ["UsersRequiringPasswordChange"] = userStats.UsersRequiringPasswordChange,
                    ["StaleUsers"] = userStats.StaleUsers,
                    ["LastLoginDate"] = userStats.LastLoginDate
                };
                
                _logger.LogInformation("Successfully retrieved SMS Application User statistics");
                return Result<Dictionary<string, object>>.Success<Dictionary<string, object>>(stats);
            }
            else
            {
                _logger.LogWarning("Failed to retrieve SMS Application User statistics: {Error}", statsResult.Error?.Message);
                return Result<Dictionary<string, object>>.Failure<Dictionary<string, object>>(statsResult.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetSMSApplicationUserStatisticsQuery");
            return Result<Dictionary<string, object>>.Failure<Dictionary<string, object>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}