//-----------------------------------------------------------------------
// <copyright file="SMSApplicationUserQueryHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query handlers for SMS Application User operations using CQRS pattern with MediatR.
//                  Application layer query handlers providing clean separation
//                  between presentation and business logic with validation.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using Microsoft.Extensions.Logging;
using SMS_Application.Queries;

namespace SMS_Application.QueryHandlers;

// =============================================
// SMS APPLICATION USER QUERY HANDLERS
// =============================================

public class GetAllSMSApplicationUsersQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetAllSMSApplicationUsersQuery, Result<IEnumerable<SMSApplicationUser>>>
{
    private readonly SMSApplicationUserService _applicationUserService;
    private readonly ILogger<GetAllSMSApplicationUsersQueryHandler> _logger;

    public GetAllSMSApplicationUsersQueryHandler(SMSApplicationUserService applicationUserService, ILogger<GetAllSMSApplicationUsersQueryHandler> logger)
    {
        _applicationUserService = applicationUserService ?? throw new ArgumentNullException(nameof(applicationUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSApplicationUser>>> HandleAsync(GetAllSMSApplicationUsersQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing GetAllSMSApplicationUsersQuery");
            var result = await _applicationUserService.GetAllSMSApplicationUsersAsync(ct);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully retrieved {Count} SMS Application Users", result.Value?.Count() ?? 0);
            }
            else
            {
                _logger.LogApplicationWarning("Failed to retrieve SMS Application Users");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetAllSMSApplicationUsersQuery", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<SMSApplicationUser>>.Failure<IEnumerable<SMSApplicationUser>>(DomainErrors.SMSApplicationUserError.NotFound);
        }
    }
}


public class GetSMSApplicationUserByCodeQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetSMSApplicationUserByCodeQuery, Result<SMSApplicationUser>>
{
    private readonly SMSApplicationUserService _applicationUserService;
    private readonly ILogger<GetSMSApplicationUserByCodeQueryHandler> _logger;

    public GetSMSApplicationUserByCodeQueryHandler(SMSApplicationUserService applicationUserService, ILogger<GetSMSApplicationUserByCodeQueryHandler> logger)
    {
        _applicationUserService = applicationUserService ?? throw new ArgumentNullException(nameof(applicationUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSApplicationUser>> HandleAsync(GetSMSApplicationUserByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing GetSMSApplicationUserByCodeQuery for Code: {UserCode}", request.UserCode);
            var result = await _applicationUserService.GetSMSApplicationUserByCodeAsync(request.UserCode, ct);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully retrieved SMS Application User with Code: {UserCode}", request.UserCode);
            }
            else
            {
                _logger.LogApplicationWarning("SMS Application User not found with Code: {UserCode}", request.UserCode);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSMSApplicationUserByCodeQuery for Code: {UserCode}", ApplicationEventIds.Error, ex);
            return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.NotFound);
        }
    }
}

public class GetSMSApplicationUserByUserNameQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetSMSApplicationUserByUserNameQuery, Result<SMSApplicationUser>>
{
    private readonly SMSApplicationUserService _applicationUserService;
    private readonly ILogger<GetSMSApplicationUserByUserNameQueryHandler> _logger;

    public GetSMSApplicationUserByUserNameQueryHandler(SMSApplicationUserService applicationUserService, ILogger<GetSMSApplicationUserByUserNameQueryHandler> logger)
    {
        _applicationUserService = applicationUserService ?? throw new ArgumentNullException(nameof(applicationUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSApplicationUser>> HandleAsync(GetSMSApplicationUserByUserNameQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing GetSMSApplicationUserByUserNameQuery for UserName: {UserName}", request.UserName);

            var result = await _applicationUserService.GetSMSApplicationUserByUserNameAsync(request.UserName, ct);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully retrieved SMS Application User with UserName: {UserName}", request.UserName);
            }
            else
            {
                _logger.LogApplicationWarning("SMS Application User not found with UserName: {UserName}", request.UserName);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSMSApplicationUserByUserNameQuery for UserName: {UserName}", ApplicationEventIds.Error, ex);
            return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.NotFound);
        }
    }
}

public class GetActiveSMSApplicationUsersQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetActiveSMSApplicationUsersQuery, Result<IEnumerable<SMSApplicationUser>>>
{
    private readonly SMSApplicationUserService _applicationUserService;
    private readonly ILogger<GetActiveSMSApplicationUsersQueryHandler> _logger;

    public GetActiveSMSApplicationUsersQueryHandler(SMSApplicationUserService applicationUserService, ILogger<GetActiveSMSApplicationUsersQueryHandler> logger)
    {
        _applicationUserService = applicationUserService ?? throw new ArgumentNullException(nameof(applicationUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSApplicationUser>>> HandleAsync(GetActiveSMSApplicationUsersQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing GetActiveSMSApplicationUsersQuery");
            var result = await _applicationUserService.GetActiveSMSApplicationUsersAsync(ct);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully retrieved {Count} active SMS Application Users", result.Value?.Count() ?? 0);
            }
            else
            {
                _logger.LogApplicationWarning("Failed to retrieve active SMS Application Users");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetActiveSMSApplicationUsersQuery", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<SMSApplicationUser>>.Failure<IEnumerable<SMSApplicationUser>>(DomainErrors.SMSApplicationUserError.NotFound);
        }
    }
}

//public class GetSMSApplicationUsersByPermissionLevelQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetSMSApplicationUsersByPermissionLevelQuery, Result<IEnumerable<SMSApplicationUser>>>
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
//            _logger.LogApplicationInformation("Processing GetSMSApplicationUsersByPermissionLevelQuery for Permission Level: {PermissionLevel}", request.PermissionLevel);
//            var result = await _repository.GetSMSApplicationUserByPermissionLevelAsync(request.PermissionLevel);

//            if (result.IsSuccess)
//            {
//                _logger.LogApplicationInformation("Successfully retrieved {Count} SMS Application Users with Permission Level: {PermissionLevel}", 
//                    result.Value?.Count() ?? 0, request.PermissionLevel);
//            }
//            else
//            {
//                _logger.LogApplicationWarning("Failed to retrieve SMS Application Users with Permission Level: {PermissionLevel}: {Error}", 
//                    request.PermissionLevel, result.Error?.Message);
//            }

//            return result;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogApplicationError(ex, "Error processing GetSMSApplicationUsersByPermissionLevelQuery for Permission Level: {PermissionLevel}", request.PermissionLevel);
//            return Result<IEnumerable<SMSApplicationUser>>.Failure<IEnumerable<SMSApplicationUser>>(DomainErrors.SMSApplicationUserError.NotFound);
//        }
//    }
//}

//public class GetSMSApplicationUsersWithMinimumPermissionQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetSMSApplicationUsersWithMinimumPermissionQuery, Result<IEnumerable<SMSApplicationUser>>>
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
//            _logger.LogApplicationInformation("Processing GetSMSApplicationUsersWithMinimumPermissionQuery for Minimum Permission: {MinimumPermissionLevel}", request.MinimumPermissionLevel);

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

//                _logger.LogApplicationInformation("Successfully retrieved {Count} SMS Application Users with minimum permission: {MinimumPermissionLevel}", 
//                    filteredUsers.Count(), request.MinimumPermissionLevel);
//                return Result<IEnumerable<SMSApplicationUser>>.Success<IEnumerable<SMSApplicationUser>>(filteredUsers);
//            }
//            else
//            {
//                _logger.LogApplicationWarning("Failed to retrieve SMS Application Users for minimum permission filtering: {Error}", allUsersResult.Error?.Message);
//                return allUsersResult;
//            }
//        }
//        catch (Exception ex)
//        {
//            _logger.LogApplicationError(ex, "Error processing GetSMSApplicationUsersWithMinimumPermissionQuery for Minimum Permission: {MinimumPermissionLevel}", request.MinimumPermissionLevel);
//            return Result<IEnumerable<SMSApplicationUser>>.Failure<IEnumerable<SMSApplicationUser>>(DomainErrors.SMSApplicationUserError.NotFound);
//        }
//    }
//}

public class CheckSMSApplicationUserNameExistsQueryHandler : BaseQueryBundle, IBaseRequestHandler<CheckSMSApplicationUserNameExistsQuery, Result<bool>>
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
            _logger.LogApplicationInformation("Processing CheckSMSApplicationUserNameExistsQuery for UserName: {UserName}", request.UserName);
            var result = await _dataService.GetAllSMSApplicationUsersAsync(); //request.UserName);
            var checkresult = result.Value.Any(x => x.UserName.Value == request.UserName);


            _logger.LogApplicationInformation("Username {UserName} exists: {Exists}", request.UserName, result.Value);

            return checkresult;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing CheckSMSApplicationUserNameExistsQuery for UserName: {UserName}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

public class ValidateSMSApplicationUserCredentialsQueryHandler : BaseQueryBundle, IBaseRequestHandler<ValidateSMSApplicationUserCredentialsQuery, Result<bool>>
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
            _logger.LogApplicationInformation("Processing ValidateSMSApplicationUserCredentialsQuery for UserName: {UserName}", request.UserName);

            var userResult = await _dataService.GetSMSApplicationUserByUserNameAsync(request.UserName);
            if (userResult.IsFailure)
            {
                _logger.LogApplicationWarning("User not found for credential validation: {UserName}", request.UserName);
                return Result<bool>.Success(false);
            }

            var user = userResult.Value;
            var isValid = user.IsActive && user.Authenticate(request.Password);

            user.UpdatedBy = request.AccessedBy;
            user.LastLoginDate = DateTime.UtcNow;
            await _dataService.UpdateSMSApplicationUserAsync(user, ct);


            _logger.LogApplicationInformation("Credential validation for {UserName}: {IsValid}", request.UserName, isValid);

            return Result<bool>.Success(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing ValidateSMSApplicationUserCredentialsQuery for UserName: {UserName}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.LoginFailed);
        }
    }
}

public class GetSMSApplicationUsersRequiringPasswordChangeQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetSMSApplicationUsersRequiringPasswordChangeQuery, Result<IEnumerable<SMSApplicationUser>>>
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
            _logger.LogApplicationInformation("Processing GetSMSApplicationUsersRequiringPasswordChangeQuery");

            var allUsersResult = await _dataService.GetAllSMSApplicationUsersAsync(ct);
            if (allUsersResult.IsSuccess)
            {
                var usersRequiringChange = allUsersResult.Value?.Where(u => u.RequiresPasswordChange) ?? new List<SMSApplicationUser>();
                _logger.LogApplicationInformation("Successfully retrieved {Count} users requiring password change", usersRequiringChange.Count());
                return Result<IEnumerable<SMSApplicationUser>>.Success<IEnumerable<SMSApplicationUser>>(usersRequiringChange);
            }
            else
            {
                _logger.LogApplicationWarning("Failed to retrieve users requiring password change: {Error}", allUsersResult.Error?.Message);
                return allUsersResult;
            }
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSMSApplicationUsersRequiringPasswordChangeQuery", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<SMSApplicationUser>>.Failure<IEnumerable<SMSApplicationUser>>(DomainErrors.SMSApplicationUserError.NotFound);
        }
    }
}

public class GetStaleSMSApplicationUsersQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetStaleSMSApplicationUsersQuery, Result<IEnumerable<SMSApplicationUser>>>
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
            _logger.LogApplicationInformation("Processing GetStaleSMSApplicationUsersQuery for {StaleDays} days", request.StaleDays);

            var allUsersResult = await _dataService.GetAllSMSApplicationUsersAsync(ct);
            if (allUsersResult.IsSuccess)
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-request.StaleDays);
                var staleUsers = allUsersResult.Value?.Where(u =>
                    !u.LastLoginDate.HasValue || u.LastLoginDate < cutoffDate) ?? new List<SMSApplicationUser>();

                _logger.LogApplicationInformation("Successfully retrieved {Count} stale users (>{StaleDays} days)", staleUsers.Count(), request.StaleDays);
                return Result<IEnumerable<SMSApplicationUser>>.Success<IEnumerable<SMSApplicationUser>>(staleUsers);
            }
            else
            {
                _logger.LogApplicationWarning("Failed to retrieve stale users: {Error}", allUsersResult.Error?.Message);
                return allUsersResult;
            }
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetStaleSMSApplicationUsersQuery for {StaleDays} days", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<SMSApplicationUser>>.Failure<IEnumerable<SMSApplicationUser>>(DomainErrors.SMSApplicationUserError.NotFound);
        }
    }
}

public class GetSMSApplicationUserStatisticsQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetSMSApplicationUserStatisticsQuery, Result<Dictionary<string, object>>>
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
            _logger.LogApplicationInformation("Processing GetSMSApplicationUserStatisticsQuery");

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

                _logger.LogApplicationInformation("Successfully retrieved SMS Application User statistics");
                return Result<Dictionary<string, object>>.Success<Dictionary<string, object>>(stats);
            }
            else
            {
                _logger.LogApplicationWarning("Failed to retrieve SMS Application User statistics: {Error}", statsResult.Error?.Message);
                return Result<Dictionary<string, object>>.Failure<Dictionary<string, object>>(statsResult.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSMSApplicationUserStatisticsQuery", ApplicationEventIds.Error, ex);
            return Result<Dictionary<string, object>>.Failure<Dictionary<string, object>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

