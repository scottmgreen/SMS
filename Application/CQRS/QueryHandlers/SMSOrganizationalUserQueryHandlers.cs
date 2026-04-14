//-----------------------------------------------------------------------
// <copyright file="SMSOrganizationalUserQueryHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query handlers implementing SMS user data retrieval and search logic.
//                  Implements query handlers for processing read operations.
//                  Retrieves and transforms data for presentation layer consumption.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using Microsoft.Extensions.Logging;
using SMS_Application.Messaging.Queries;

namespace SMS_Application.Messaging.QueryHandlers;

/// <summary>
/// Query handler for getting all SMS organizational users
/// </summary>
public class GetAllSMSOrganizationalUsersQueryHandler : BaseQueryBundle, IRequestHandler<GetAllSMSOrganizationalUsersQuery, Result<IEnumerable<SMSOrganizationalUser>>>
{
    private readonly SMSOrganizationalUserService _organizationalUserService;
    private readonly ILogger<GetAllSMSOrganizationalUsersQueryHandler> _logger;

    public GetAllSMSOrganizationalUsersQueryHandler(SMSOrganizationalUserService organizationalUserService, ILogger<GetAllSMSOrganizationalUsersQueryHandler> logger)
    {
        _organizationalUserService = organizationalUserService ?? throw new ArgumentNullException(nameof(organizationalUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSOrganizationalUser>>> HandleAsync(GetAllSMSOrganizationalUsersQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetAllSMSOrganizationalUsersQuery");
            var result = await _organizationalUserService.GetAllSMSOrganizationalUsersAsync(ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} SMS Organizational Users", result.Value?.Count() ?? 0);
            }
            else
            {
                _logger.LogWarning("Failed to retrieve SMS Organizational Users");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetAllSMSOrganizationalUsersQuery", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(DomainErrors.SMSOrganizationalUserError.NotFound);
        }
    }
}

/// <summary>
/// Query handler for getting SMS organizational user by code
/// </summary>
public class GetSMSOrganizationalUserByCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSOrganizationalUserByCodeQuery, Result<SMSOrganizationalUser>>
{
    private readonly SMSOrganizationalUserService _organizationalUserService;
    private readonly ILogger<GetSMSOrganizationalUserByCodeQueryHandler> _logger;

    public GetSMSOrganizationalUserByCodeQueryHandler(SMSOrganizationalUserService organizationalUserService, ILogger<GetSMSOrganizationalUserByCodeQueryHandler> logger)
    {
        _organizationalUserService = organizationalUserService ?? throw new ArgumentNullException(nameof(organizationalUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSOrganizationalUser>> HandleAsync(GetSMSOrganizationalUserByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSOrganizationalUserByCodeQuery for Code: {Code}", request.UserCode);
            var result = await _organizationalUserService.GetSMSOrganizationalUserByCodeAsync(request.UserCode, ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved SMS Organizational User with Code: {Code}", request.UserCode);
            }
            else
            {
                _logger.LogWarning("SMS Organizational User not found with Code: {Code}", request.UserCode);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSMSOrganizationalUserByCodeQuery for Code: {Code}", ApplicationEventIds.Error, ex);
            return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.NotFound);
        }
    }
}

/// <summary>
/// Query handler for getting SMS organizational user by username
/// </summary>
public class GetSMSOrganizationalUserByUserNameQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSOrganizationalUserByUserNameQuery, Result<SMSOrganizationalUser>>
{
    private readonly SMSOrganizationalUserService _organizationalUserService;
    private readonly ILogger<GetSMSOrganizationalUserByUserNameQueryHandler> _logger;

    public GetSMSOrganizationalUserByUserNameQueryHandler(SMSOrganizationalUserService organizationalUserService, ILogger<GetSMSOrganizationalUserByUserNameQueryHandler> logger)
    {
        _organizationalUserService = organizationalUserService ?? throw new ArgumentNullException(nameof(organizationalUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSOrganizationalUser>> HandleAsync(GetSMSOrganizationalUserByUserNameQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSOrganizationalUserByUserNameQuery for UserName: {UserName}", request.UserName);

            // Get all users and filter by username (fallback approach)
            var allUsersResult = await _organizationalUserService.GetAllSMSOrganizationalUsersAsync(ct);
            if (allUsersResult.IsFailure)
            {
                return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(allUsersResult.Error);
            }

            var user = allUsersResult.Value?.FirstOrDefault(u => u.UserName.Value.Equals(request.UserName, StringComparison.OrdinalIgnoreCase));

            if (user != null)
            {
                _logger.LogInformation("Successfully retrieved SMS Organizational User with UserName: {UserName}", request.UserName);
                return Result<SMSOrganizationalUser>.Success(user);
            }
            else
            {
                _logger.LogWarning("SMS Organizational User not found with UserName: {UserName}", request.UserName);
                return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSMSOrganizationalUserByUserNameQuery for UserName: {UserName}", ApplicationEventIds.Error, ex);
            return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.NotFound);
        }
    }
}

/// <summary>
/// Query handler for getting active SMS organizational users
/// </summary>
public class GetActiveSMSOrganizationalUsersQueryHandler : BaseQueryBundle, IRequestHandler<GetActiveSMSOrganizationalUsersQuery, Result<IEnumerable<SMSOrganizationalUser>>>
{
    private readonly SMSOrganizationalUserService _organizationalUserService;
    private readonly ILogger<GetActiveSMSOrganizationalUsersQueryHandler> _logger;

    public GetActiveSMSOrganizationalUsersQueryHandler(SMSOrganizationalUserService organizationalUserService, ILogger<GetActiveSMSOrganizationalUsersQueryHandler> logger)
    {
        _organizationalUserService = organizationalUserService ?? throw new ArgumentNullException(nameof(organizationalUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSOrganizationalUser>>> HandleAsync(GetActiveSMSOrganizationalUsersQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetActiveSMSOrganizationalUsersQuery");

            // Get all users and filter for active ones
            var allUsersResult = await _organizationalUserService.GetAllSMSOrganizationalUsersAsync(ct);
            if (allUsersResult.IsFailure)
            {
                return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(allUsersResult.Error);
            }

            var activeUsers = allUsersResult.Value?.Where(u => u.IsActive) ?? Enumerable.Empty<SMSOrganizationalUser>();

            _logger.LogInformation("Successfully retrieved {Count} active SMS Organizational Users", activeUsers.Count());
            return Result<IEnumerable<SMSOrganizationalUser>>.Success(activeUsers);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetActiveSMSOrganizationalUsersQuery", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(DomainErrors.SMSOrganizationalUserError.NotFound);
        }
    }
}

/// <summary>
/// Query handler for getting SMS organizational users by department
/// </summary>
public class GetSMSOrganizationalUsersByDepartmentQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSOrganizationalUsersByDepartmentQuery, Result<IEnumerable<SMSOrganizationalUser>>>
{
    private readonly SMSOrganizationalUserService _organizationalUserService;
    private readonly ILogger<GetSMSOrganizationalUsersByDepartmentQueryHandler> _logger;

    public GetSMSOrganizationalUsersByDepartmentQueryHandler(SMSOrganizationalUserService organizationalUserService, ILogger<GetSMSOrganizationalUsersByDepartmentQueryHandler> logger)
    {
        _organizationalUserService = organizationalUserService ?? throw new ArgumentNullException(nameof(organizationalUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSOrganizationalUser>>> HandleAsync(GetSMSOrganizationalUsersByDepartmentQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSOrganizationalUsersByDepartmentQuery for Department: {Department}", request.Department);

            // Filter from all users as service method doesn't exist
            var allUsersResult = await _organizationalUserService.GetAllSMSOrganizationalUsersAsync(ct);
            if (allUsersResult.IsFailure)
            {
                return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(allUsersResult.Error);
            }

            var filteredUsers = allUsersResult.Value?.Where(u => string.Equals(u.Department, request.Department, StringComparison.OrdinalIgnoreCase)) ?? Enumerable.Empty<SMSOrganizationalUser>();

            _logger.LogInformation("Successfully retrieved {Count} SMS Organizational Users for Department: {Department}", filteredUsers.Count(), request.Department);
            return Result<IEnumerable<SMSOrganizationalUser>>.Success(filteredUsers);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSMSOrganizationalUsersByDepartmentQuery for Department: {Department}", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(DomainErrors.SMSOrganizationalUserError.NotFound);
        }
    }
}

/// <summary>
/// Query handler for getting SMS organizational users by position
/// </summary>
public class GetSMSOrganizationalUsersByPositionQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSOrganizationalUsersByPositionQuery, Result<IEnumerable<SMSOrganizationalUser>>>
{
    private readonly SMSOrganizationalUserService _organizationalUserService;
    private readonly ILogger<GetSMSOrganizationalUsersByPositionQueryHandler> _logger;

    public GetSMSOrganizationalUsersByPositionQueryHandler(SMSOrganizationalUserService organizationalUserService, ILogger<GetSMSOrganizationalUsersByPositionQueryHandler> logger)
    {
        _organizationalUserService = organizationalUserService ?? throw new ArgumentNullException(nameof(organizationalUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSOrganizationalUser>>> HandleAsync(GetSMSOrganizationalUsersByPositionQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSOrganizationalUsersByPositionQuery for Position: {Position}", request.Position);

            // Filter from all users as service method doesn't exist
            var allUsersResult = await _organizationalUserService.GetAllSMSOrganizationalUsersAsync(ct);
            if (allUsersResult.IsFailure)
            {
                return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(allUsersResult.Error);
            }

            var filteredUsers = allUsersResult.Value?.Where(u => string.Equals(u.Position, request.Position, StringComparison.OrdinalIgnoreCase)) ?? Enumerable.Empty<SMSOrganizationalUser>();

            _logger.LogInformation("Successfully retrieved {Count} SMS Organizational Users for Position: {Position}", filteredUsers.Count(), request.Position);
            return Result<IEnumerable<SMSOrganizationalUser>>.Success(filteredUsers);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSMSOrganizationalUsersByPositionQuery for Position: {Position}", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(DomainErrors.SMSOrganizationalUserError.NotFound);
        }
    }
}

/// <summary>
/// Query handler for getting SMS organizational users by organization level
/// </summary>
public class GetSMSOrganizationalUsersByOrganizationLevelQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSOrganizationalUsersByOrganizationLevelQuery, Result<IEnumerable<SMSOrganizationalUser>>>
{
    private readonly SMSOrganizationalUserService _organizationalUserService;
    private readonly ILogger<GetSMSOrganizationalUsersByOrganizationLevelQueryHandler> _logger;

    public GetSMSOrganizationalUsersByOrganizationLevelQueryHandler(SMSOrganizationalUserService organizationalUserService, ILogger<GetSMSOrganizationalUsersByOrganizationLevelQueryHandler> logger)
    {
        _organizationalUserService = organizationalUserService ?? throw new ArgumentNullException(nameof(organizationalUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSOrganizationalUser>>> HandleAsync(GetSMSOrganizationalUsersByOrganizationLevelQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSOrganizationalUsersByOrganizationLevelQuery for OrganizationLevel: {OrganizationLevel}", request.OrganizationLevel);

            // Filter users by organization level through service
            var allUsersResult = await _organizationalUserService.GetAllSMSOrganizationalUsersAsync(ct);
            if (allUsersResult.IsFailure)
            {
                return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(allUsersResult.Error);
            }

            var filteredUsers = allUsersResult.Value?.Where(u => u.OrganizationLevel.Value.Equals(request.OrganizationLevel, StringComparison.OrdinalIgnoreCase)) ?? Enumerable.Empty<SMSOrganizationalUser>();

            _logger.LogInformation("Successfully retrieved {Count} SMS Organizational Users for OrganizationLevel: {OrganizationLevel}", filteredUsers.Count(), request.OrganizationLevel);
            return Result<IEnumerable<SMSOrganizationalUser>>.Success(filteredUsers);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSMSOrganizationalUsersByOrganizationLevelQuery for OrganizationLevel: {OrganizationLevel}", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(DomainErrors.SMSOrganizationalUserError.NotFound);
        }
    }
}

/// <summary>
/// Query handler for checking if SMS organizational username exists
/// </summary>
public class CheckSMSOrganizationalUserNameExistsQueryHandler : BaseQueryBundle, IRequestHandler<CheckSMSOrganizationalUserNameExistsQuery, Result<bool>>
{
    private readonly SMSOrganizationalUserService _organizationalUserService;
    private readonly ILogger<CheckSMSOrganizationalUserNameExistsQueryHandler> _logger;

    public CheckSMSOrganizationalUserNameExistsQueryHandler(SMSOrganizationalUserService organizationalUserService, ILogger<CheckSMSOrganizationalUserNameExistsQueryHandler> logger)
    {
        _organizationalUserService = organizationalUserService ?? throw new ArgumentNullException(nameof(organizationalUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(CheckSMSOrganizationalUserNameExistsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing CheckSMSOrganizationalUserNameExistsQuery for UserName: {UserName}", request.UserName);

            var result = await _organizationalUserService.GetAllSMSOrganizationalUsersAsync(ct);
            if (result.IsFailure)
            {
                return Result<bool>.Failure<bool>(result.Error);
            }

            var exists = result.Value?.Any(x => x.UserName.Value.Equals(request.UserName, StringComparison.OrdinalIgnoreCase)) ?? false;

            _logger.LogInformation("Username {UserName} exists: {Exists}", request.UserName, exists);

            return Result<bool>.Success(exists);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing CheckSMSOrganizationalUserNameExistsQuery for UserName: {UserName}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

/// <summary>
/// Query handler for getting SMS organizational user statistics
/// </summary>
public class GetSMSOrganizationalUserStatisticsQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSOrganizationalUserStatisticsQuery, Result<Dictionary<string, object>>>
{
    private readonly SMSOrganizationalUserService _organizationalUserService;
    private readonly ILogger<GetSMSOrganizationalUserStatisticsQueryHandler> _logger;

    public GetSMSOrganizationalUserStatisticsQueryHandler(SMSOrganizationalUserService organizationalUserService, ILogger<GetSMSOrganizationalUserStatisticsQueryHandler> logger)
    {
        _organizationalUserService = organizationalUserService ?? throw new ArgumentNullException(nameof(organizationalUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Dictionary<string, object>>> HandleAsync(GetSMSOrganizationalUserStatisticsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSOrganizationalUserStatisticsQuery");

            // Generate basic statistics from all users
            var allUsersResult = await _organizationalUserService.GetAllSMSOrganizationalUsersAsync(ct);
            if (allUsersResult.IsFailure)
            {
                return Result<Dictionary<string, object>>.Failure<Dictionary<string, object>>(allUsersResult.Error);
            }

            var users = allUsersResult.Value ?? Enumerable.Empty<SMSOrganizationalUser>();
            var stats = new Dictionary<string, object>
            {
                ["TotalUsers"] = users.Count(),
                ["ActiveUsers"] = users.Count(u => u.IsActive),
                ["InactiveUsers"] = users.Count(u => !u.IsActive),
                ["UsersRequiringPasswordChange"] = users.Count(u => u.RequiresPasswordChange),
                ["StaleUsers"] = users.Count(u => !u.LastLoginDate.HasValue || u.LastLoginDate < DateTime.UtcNow.AddDays(-90)),
                ["LastLoginDate"] = users.Where(u => u.LastLoginDate.HasValue).Max(u => u.LastLoginDate)
            };

            _logger.LogInformation("Successfully retrieved SMS Organizational User statistics");
            return Result<Dictionary<string, object>>.Success<Dictionary<string, object>>(stats);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSMSOrganizationalUserStatisticsQuery", ApplicationEventIds.Error, ex);
            return Result<Dictionary<string, object>>.Failure<Dictionary<string, object>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

/// <summary>
/// Query handler for validating SMS organizational user credentials
/// </summary>
public class ValidateSMSOrganizationalUserCredentialsQueryHandler : BaseQueryBundle, IRequestHandler<ValidateSMSOrganizationalUserCredentialsQuery, Result<bool>>
{
    private readonly SMSOrganizationalUserService _organizationalUserService;
    private readonly ILogger<ValidateSMSOrganizationalUserCredentialsQueryHandler> _logger;

    public ValidateSMSOrganizationalUserCredentialsQueryHandler(SMSOrganizationalUserService organizationalUserService, ILogger<ValidateSMSOrganizationalUserCredentialsQueryHandler> logger)
    {
        _organizationalUserService = organizationalUserService ?? throw new ArgumentNullException(nameof(organizationalUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(ValidateSMSOrganizationalUserCredentialsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing ValidateSMSOrganizationalUserCredentialsQuery for UserName: {UserName}", request.UserName);

            var result = await _organizationalUserService.AuthenticateSMSOrganizationalUserAsync(request.UserName, request.Password, ct);

            _logger.LogInformation("Credential validation for {UserName}: {IsValid}", request.UserName, result.IsSuccess && result.Value);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing ValidateSMSOrganizationalUserCredentialsQuery for UserName: {UserName}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.LoginFailed);
        }
    }
}