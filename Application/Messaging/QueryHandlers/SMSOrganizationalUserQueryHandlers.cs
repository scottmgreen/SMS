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
// SMS ORGANIZATIONAL USER QUERY HANDLERS
// =============================================

public class GetAllSMSOrganizationalUsersQueryHandler : BaseQueryBundle, IRequestHandler<GetAllSMSOrganizationalUsersQuery, Result<IEnumerable<SMSOrganizationalUser>>>
{
    private readonly SMSOrganizationalUserDataService _dataService;
    private readonly ILogger<GetAllSMSOrganizationalUsersQueryHandler> _logger;

    public GetAllSMSOrganizationalUsersQueryHandler(SMSOrganizationalUserDataService dataService, ILogger<GetAllSMSOrganizationalUsersQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSOrganizationalUser>>> HandleAsync(GetAllSMSOrganizationalUsersQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetAllSMSOrganizationalUsersQuery");
            var result = await _dataService.GetAllSMSOrganizationalUsersAsync(ct);
            
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
            _logger.LogError(ex, "Error processing GetAllSMSOrganizationalUsersQuery");
            return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(DomainErrors.SMSOrganizationalUserError.NotFound);
        }
    }
}

public class GetSMSOrganizationalUserByIdQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSOrganizationalUserByIdQuery, Result<SMSOrganizationalUser>>
{
    private readonly SMSOrganizationalUserDataService _dataService;
    private readonly ILogger<GetSMSOrganizationalUserByIdQueryHandler> _logger;

    public GetSMSOrganizationalUserByIdQueryHandler(SMSOrganizationalUserDataService dataService, ILogger<GetSMSOrganizationalUserByIdQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSOrganizationalUser>> HandleAsync(GetSMSOrganizationalUserByIdQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSOrganizationalUserByIdQuery for ID: {UserId}", request.UserId);
            var result = await _dataService.GetSMSOrganizationalUserByIdAsync(request.UserId, ct);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved SMS Organizational User with ID: {UserId}", request.UserId);
            }
            else
            {
                _logger.LogWarning("SMS Organizational User not found with ID: {UserId}", request.UserId);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetSMSOrganizationalUserByIdQuery for ID: {UserId}", request.UserId);
            return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.NotFound);
        }
    }
}

public class GetSMSOrganizationalUserByCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSOrganizationalUserByCodeQuery, Result<SMSOrganizationalUser>>
{
    private readonly SMSOrganizationalUserDataService _dataService;
    private readonly ILogger<GetSMSOrganizationalUserByCodeQueryHandler> _logger;

    public GetSMSOrganizationalUserByCodeQueryHandler(SMSOrganizationalUserDataService dataService, ILogger<GetSMSOrganizationalUserByCodeQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSOrganizationalUser>> HandleAsync(GetSMSOrganizationalUserByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSOrganizationalUserByCodeQuery for Code: {UserCode}", request.UserCode);
            var result = await _dataService.GetSMSOrganizationalUserByIdAsync(request.UserCode, ct);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved SMS Organizational User with Code: {UserCode}", request.UserCode);
            }
            else
            {
                _logger.LogWarning("SMS Organizational User not found with Code: {UserCode}", request.UserCode);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetSMSOrganizationalUserByCodeQuery for Code: {UserCode}", request.UserCode);
            return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.NotFound);
        }
    }
}

public class GetSMSOrganizationalUserByUserNameQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSOrganizationalUserByUserNameQuery, Result<SMSOrganizationalUser>>
{
    private readonly SMSOrganizationalUserDataService _dataService;
    private readonly ILogger<GetSMSOrganizationalUserByUserNameQueryHandler> _logger;

    public GetSMSOrganizationalUserByUserNameQueryHandler(SMSOrganizationalUserDataService dataService, ILogger<GetSMSOrganizationalUserByUserNameQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSOrganizationalUser>> HandleAsync(GetSMSOrganizationalUserByUserNameQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSOrganizationalUserByUserNameQuery for UserName: {UserName}", request.UserName);
            
            // Get all users and filter by username
            var allUsersResult = await _dataService.GetAllSMSOrganizationalUsersAsync(ct);
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
            _logger.LogError(ex, "Error processing GetSMSOrganizationalUserByUserNameQuery for UserName: {UserName}", request.UserName);
            return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.NotFound);
        }
    }
}

public class GetActiveSMSOrganizationalUsersQueryHandler : BaseQueryBundle, IRequestHandler<GetActiveSMSOrganizationalUsersQuery, Result<IEnumerable<SMSOrganizationalUser>>>
{
    private readonly SMSOrganizationalUserDataService _dataService;
    private readonly ILogger<GetActiveSMSOrganizationalUsersQueryHandler> _logger;

    public GetActiveSMSOrganizationalUsersQueryHandler(SMSOrganizationalUserDataService dataService, ILogger<GetActiveSMSOrganizationalUsersQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSOrganizationalUser>>> HandleAsync(GetActiveSMSOrganizationalUsersQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetActiveSMSOrganizationalUsersQuery");
            var result = await _dataService.GetActiveSMSOrganizationalUsersAsync(ct);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} active SMS Organizational Users", result.Value?.Count() ?? 0);
            }
            else
            {
                _logger.LogWarning("Failed to retrieve active SMS Organizational Users");
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetActiveSMSOrganizationalUsersQuery");
            return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(DomainErrors.SMSOrganizationalUserError.NotFound);
        }
    }
}

public class GetSMSOrganizationalUsersByDepartmentQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSOrganizationalUsersByDepartmentQuery, Result<IEnumerable<SMSOrganizationalUser>>>
{
    private readonly SMSOrganizationalUserDataService _dataService;
    private readonly ILogger<GetSMSOrganizationalUsersByDepartmentQueryHandler> _logger;

    public GetSMSOrganizationalUsersByDepartmentQueryHandler(SMSOrganizationalUserDataService dataService, ILogger<GetSMSOrganizationalUsersByDepartmentQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSOrganizationalUser>>> HandleAsync(GetSMSOrganizationalUsersByDepartmentQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSOrganizationalUsersByDepartmentQuery for Department: {Department}", request.Department);
            var result = await _dataService.GetSMSOrganizationalUsersByDepartmentAsync(request.Department, ct);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} SMS Organizational Users for Department: {Department}", result.Value?.Count() ?? 0, request.Department);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetSMSOrganizationalUsersByDepartmentQuery for Department: {Department}", request.Department);
            return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(DomainErrors.SMSOrganizationalUserError.NotFound);
        }
    }
}

public class GetSMSOrganizationalUsersByPositionQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSOrganizationalUsersByPositionQuery, Result<IEnumerable<SMSOrganizationalUser>>>
{
    private readonly SMSOrganizationalUserDataService _dataService;
    private readonly ILogger<GetSMSOrganizationalUsersByPositionQueryHandler> _logger;

    public GetSMSOrganizationalUsersByPositionQueryHandler(SMSOrganizationalUserDataService dataService, ILogger<GetSMSOrganizationalUsersByPositionQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSOrganizationalUser>>> HandleAsync(GetSMSOrganizationalUsersByPositionQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSOrganizationalUsersByPositionQuery for Position: {Position}", request.Position);
            var result = await _dataService.GetSMSOrganizationalUsersByPositionAsync(request.Position, ct);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} SMS Organizational Users for Position: {Position}", result.Value?.Count() ?? 0, request.Position);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetSMSOrganizationalUsersByPositionQuery for Position: {Position}", request.Position);
            return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(DomainErrors.SMSOrganizationalUserError.NotFound);
        }
    }
}

public class GetSMSOrganizationalUsersByOrganizationLevelQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSOrganizationalUsersByOrganizationLevelQuery, Result<IEnumerable<SMSOrganizationalUser>>>
{
    private readonly SMSOrganizationalUserDataService _dataService;
    private readonly ILogger<GetSMSOrganizationalUsersByOrganizationLevelQueryHandler> _logger;

    public GetSMSOrganizationalUsersByOrganizationLevelQueryHandler(SMSOrganizationalUserDataService dataService, ILogger<GetSMSOrganizationalUsersByOrganizationLevelQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSOrganizationalUser>>> HandleAsync(GetSMSOrganizationalUsersByOrganizationLevelQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSOrganizationalUsersByOrganizationLevelQuery for OrganizationLevel: {OrganizationLevel}", request.OrganizationLevel);
            
            // Filter users by organization level through service
            var allUsersResult = await _dataService.GetAllSMSOrganizationalUsersAsync(ct);
            if (allUsersResult.IsFailure)
            {
                return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(allUsersResult.Error);
            }

            var filteredUsers = allUsersResult.Value?.Where(u => u.OrganizationLevel.Equals(request.OrganizationLevel, StringComparison.OrdinalIgnoreCase)) ?? Enumerable.Empty<SMSOrganizationalUser>();
            
            _logger.LogInformation("Successfully retrieved {Count} SMS Organizational Users for OrganizationLevel: {OrganizationLevel}", filteredUsers.Count(), request.OrganizationLevel);
            return Result<IEnumerable<SMSOrganizationalUser>>.Success(filteredUsers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetSMSOrganizationalUsersByOrganizationLevelQuery for OrganizationLevel: {OrganizationLevel}", request.OrganizationLevel);
            return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(DomainErrors.SMSOrganizationalUserError.NotFound);
        }
    }
}

public class CheckSMSOrganizationalUserNameExistsQueryHandler : BaseQueryBundle, IRequestHandler<CheckSMSOrganizationalUserNameExistsQuery, Result<bool>>
{
    private readonly SMSOrganizationalUserDataService _dataService;
    private readonly ILogger<CheckSMSOrganizationalUserNameExistsQueryHandler> _logger;

    public CheckSMSOrganizationalUserNameExistsQueryHandler(SMSOrganizationalUserDataService dataService, ILogger<CheckSMSOrganizationalUserNameExistsQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(CheckSMSOrganizationalUserNameExistsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing CheckSMSOrganizationalUserNameExistsQuery for UserName: {UserName}", request.UserName);
            var result = await _dataService.GetAllSMSOrganizationalUsersAsync();//(request.UserName);
            var checkresult = result.Value.Any(x=> x.UserName.Value == request.UserName);    
            
            _logger.LogInformation("Username {UserName} exists: {Exists}", request.UserName, result.Value);
            
            return checkresult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing CheckSMSOrganizationalUserNameExistsQuery for UserName: {UserName}", request.UserName);
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

public class GetSMSOrganizationalUserStatisticsQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSOrganizationalUserStatisticsQuery, Result<Dictionary<string, object>>>
{
    private readonly SMSOrganizationalUserDataService _dataService;
    private readonly ILogger<GetSMSOrganizationalUserStatisticsQueryHandler> _logger;

    public GetSMSOrganizationalUserStatisticsQueryHandler(SMSOrganizationalUserDataService dataService, ILogger<GetSMSOrganizationalUserStatisticsQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Dictionary<string, object>>> HandleAsync(GetSMSOrganizationalUserStatisticsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSOrganizationalUserStatisticsQuery");
            
            var statsResult = await _dataService.GetSMSOrganizationalUserStatisticsAsync();
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
                
                _logger.LogInformation("Successfully retrieved SMS Organizational User statistics");
                return Result<Dictionary<string, object>>.Success<Dictionary<string, object>>(stats);
            }
            else
            {
                _logger.LogWarning("Failed to retrieve SMS Organizational User statistics: {Error}", statsResult.Error?.Message);
                return Result<Dictionary<string, object>>.Failure<Dictionary<string, object>>(statsResult.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetSMSOrganizationalUserStatisticsQuery");
            return Result<Dictionary<string, object>>.Failure<Dictionary<string, object>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

public class ValidateSMSOrganizationalUserCredentialsQueryHandler : BaseQueryBundle, IRequestHandler<ValidateSMSOrganizationalUserCredentialsQuery, Result<bool>>
{
    private readonly SMSOrganizationalUserDataService _dataService;
    private readonly ILogger<ValidateSMSOrganizationalUserCredentialsQueryHandler> _logger;

    public ValidateSMSOrganizationalUserCredentialsQueryHandler(SMSOrganizationalUserDataService dataService, ILogger<ValidateSMSOrganizationalUserCredentialsQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(ValidateSMSOrganizationalUserCredentialsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing ValidateSMSOrganizationalUserCredentialsQuery for UserName: {UserName}", request.UserName);

            var userResult = await _dataService.GetSMSOrganizationalUserByUserNameAsync(request.UserName);
            if (userResult.IsFailure)
            {
                _logger.LogWarning("User not found for credential validation: {UserName}", request.UserName);
                return Result<bool>.Success(false);
            }

            var user = userResult.Value;
            var isValid = user.IsActive && user.Authenticate(request.Password);

            user.UpdatedBy = "SYSTEM";
            user.LastLoginDate = DateTime.UtcNow;
            await _dataService.UpdateSMSOrganizationalUserAsync(user, ct);


            _logger.LogInformation("Credential validation for {UserName}: {IsValid}", request.UserName, isValid);

            return Result<bool>.Success(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ValidateSMSOrganizationalUserCredentialsQuery for UserName: {UserName}", request.UserName);
            return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.LoginFailed);
        }
    }
}