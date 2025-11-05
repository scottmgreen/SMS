using Microsoft.Extensions.Logging;
using SMS_Application.Common;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Queries;
using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Domain.Interfaces;
using SMS_Shared.Common;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// SMS ORGANIZATIONAL USER QUERY HANDLERS
// =============================================

public class GetAllSMSOrganizationalUsersQueryHandler : BaseQueryBundle, IRequestHandler<GetAllSMSOrganizationalUsersQuery, Result<IEnumerable<SMSOrganizationalUser>>>
{
    private readonly ISMSOrganizationalUserRepository _repository;
    private readonly ILogger<GetAllSMSOrganizationalUsersQueryHandler> _logger;

    public GetAllSMSOrganizationalUsersQueryHandler(ISMSOrganizationalUserRepository repository, ILogger<GetAllSMSOrganizationalUsersQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSOrganizationalUser>>> HandleAsync(GetAllSMSOrganizationalUsersQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetAllSMSOrganizationalUsersQuery");
            var result = await _repository.GetAllAsync();
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} SMS Organizational Users", result.Value?.Count() ?? 0);
            }
            else
            {
                _logger.LogWarning("Failed to retrieve SMS Organizational Users: {Error}", result.Error?.Message);
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
    private readonly ISMSOrganizationalUserRepository _repository;
    private readonly ILogger<GetSMSOrganizationalUserByIdQueryHandler> _logger;

    public GetSMSOrganizationalUserByIdQueryHandler(ISMSOrganizationalUserRepository repository, ILogger<GetSMSOrganizationalUserByIdQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSOrganizationalUser>> HandleAsync(GetSMSOrganizationalUserByIdQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSOrganizationalUserByIdQuery for ID: {UserId}", request.UserId);
            var result = await _repository.GetByIdAsync(request.UserId);
            
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

public class GetSMSOrganizationalUserByUserNameQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSOrganizationalUserByUserNameQuery, Result<SMSOrganizationalUser>>
{
    private readonly ISMSOrganizationalUserRepository _repository;
    private readonly ILogger<GetSMSOrganizationalUserByUserNameQueryHandler> _logger;

    public GetSMSOrganizationalUserByUserNameQueryHandler(ISMSOrganizationalUserRepository repository, ILogger<GetSMSOrganizationalUserByUserNameQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSOrganizationalUser>> HandleAsync(GetSMSOrganizationalUserByUserNameQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSOrganizationalUserByUserNameQuery for UserName: {UserName}", request.UserName);
            var result = await _repository.GetByUserNameAsync(request.UserName);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved SMS Organizational User with UserName: {UserName}", request.UserName);
            }
            else
            {
                _logger.LogWarning("SMS Organizational User not found with UserName: {UserName}", request.UserName);
            }
            
            return result;
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
    private readonly ISMSOrganizationalUserRepository _repository;
    private readonly ILogger<GetActiveSMSOrganizationalUsersQueryHandler> _logger;

    public GetActiveSMSOrganizationalUsersQueryHandler(ISMSOrganizationalUserRepository repository, ILogger<GetActiveSMSOrganizationalUsersQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSOrganizationalUser>>> HandleAsync(GetActiveSMSOrganizationalUsersQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetActiveSMSOrganizationalUsersQuery");
            var result = await _repository.GetActiveUsersAsync();
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} active SMS Organizational Users", result.Value?.Count() ?? 0);
            }
            else
            {
                _logger.LogWarning("Failed to retrieve active SMS Organizational Users: {Error}", result.Error?.Message);
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
    private readonly ISMSOrganizationalUserRepository _repository;
    private readonly ILogger<GetSMSOrganizationalUsersByDepartmentQueryHandler> _logger;

    public GetSMSOrganizationalUsersByDepartmentQueryHandler(ISMSOrganizationalUserRepository repository, ILogger<GetSMSOrganizationalUsersByDepartmentQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSOrganizationalUser>>> HandleAsync(GetSMSOrganizationalUsersByDepartmentQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSOrganizationalUsersByDepartmentQuery for Department: {Department}", request.Department);
            var result = await _repository.GetByDepartmentAsync(request.Department);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} SMS Organizational Users for Department: {Department}", 
                    result.Value?.Count() ?? 0, request.Department);
            }
            else
            {
                _logger.LogWarning("Failed to retrieve SMS Organizational Users for Department: {Department}: {Error}", 
                    request.Department, result.Error?.Message);
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
    private readonly ISMSOrganizationalUserRepository _repository;
    private readonly ILogger<GetSMSOrganizationalUsersByPositionQueryHandler> _logger;

    public GetSMSOrganizationalUsersByPositionQueryHandler(ISMSOrganizationalUserRepository repository, ILogger<GetSMSOrganizationalUsersByPositionQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSOrganizationalUser>>> HandleAsync(GetSMSOrganizationalUsersByPositionQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSOrganizationalUsersByPositionQuery for Position: {Position}", request.Position);
            var result = await _repository.GetByPositionAsync(request.Position);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} SMS Organizational Users for Position: {Position}", 
                    result.Value?.Count() ?? 0, request.Position);
            }
            else
            {
                _logger.LogWarning("Failed to retrieve SMS Organizational Users for Position: {Position}: {Error}", 
                    request.Position, result.Error?.Message);
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

public class GetSMSOrganizationalUsersByLevelQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSOrganizationalUsersByLevelQuery, Result<IEnumerable<SMSOrganizationalUser>>>
{
    private readonly ISMSOrganizationalUserRepository _repository;
    private readonly ILogger<GetSMSOrganizationalUsersByLevelQueryHandler> _logger;

    public GetSMSOrganizationalUsersByLevelQueryHandler(ISMSOrganizationalUserRepository repository, ILogger<GetSMSOrganizationalUsersByLevelQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSOrganizationalUser>>> HandleAsync(GetSMSOrganizationalUsersByLevelQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSOrganizationalUsersByLevelQuery for Level: {OrganizationLevel}", request.OrganizationLevel);
            var result = await _repository.GetByOrganizationLevelAsync(request.OrganizationLevel);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} SMS Organizational Users for Level: {OrganizationLevel}", 
                    result.Value?.Count() ?? 0, request.OrganizationLevel);
            }
            else
            {
                _logger.LogWarning("Failed to retrieve SMS Organizational Users for Level: {OrganizationLevel}: {Error}", 
                    request.OrganizationLevel, result.Error?.Message);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetSMSOrganizationalUsersByLevelQuery for Level: {OrganizationLevel}", request.OrganizationLevel);
            return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(DomainErrors.SMSOrganizationalUserError.NotFound);
        }
    }
}

public class GetDepartmentSupervisorsQueryHandler : BaseQueryBundle, IRequestHandler<GetDepartmentSupervisorsQuery, Result<IEnumerable<SMSOrganizationalUser>>>
{
    private readonly ISMSOrganizationalUserRepository _repository;
    private readonly ILogger<GetDepartmentSupervisorsQueryHandler> _logger;

    public GetDepartmentSupervisorsQueryHandler(ISMSOrganizationalUserRepository repository, ILogger<GetDepartmentSupervisorsQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSOrganizationalUser>>> HandleAsync(GetDepartmentSupervisorsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetDepartmentSupervisorsQuery for Department: {Department}", request.Department);
            var result = await _repository.GetDepartmentSupervisorsAsync(request.Department);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} supervisors for Department: {Department}", 
                    result.Value?.Count() ?? 0, request.Department);
            }
            else
            {
                _logger.LogWarning("Failed to retrieve supervisors for Department: {Department}: {Error}", 
                    request.Department, result.Error?.Message);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetDepartmentSupervisorsQuery for Department: {Department}", request.Department);
            return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(DomainErrors.SMSOrganizationalUserError.NotFound);
        }
    }
}

public class CheckSMSOrganizationalUserNameExistsQueryHandler : BaseQueryBundle, IRequestHandler<CheckSMSOrganizationalUserNameExistsQuery, Result<bool>>
{
    private readonly ISMSOrganizationalUserRepository _repository;
    private readonly ILogger<CheckSMSOrganizationalUserNameExistsQueryHandler> _logger;

    public CheckSMSOrganizationalUserNameExistsQueryHandler(ISMSOrganizationalUserRepository repository, ILogger<CheckSMSOrganizationalUserNameExistsQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(CheckSMSOrganizationalUserNameExistsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing CheckSMSOrganizationalUserNameExistsQuery for UserName: {UserName}", request.UserName);
            var result = await _repository.UserNameExistsAsync(request.UserName);
            
            _logger.LogInformation("Username {UserName} exists: {Exists}", request.UserName, result.Value);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing CheckSMSOrganizationalUserNameExistsQuery for UserName: {UserName}", request.UserName);
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

public class GetDepartmentStatisticsQueryHandler : BaseQueryBundle, IRequestHandler<GetDepartmentStatisticsQuery, Result<Dictionary<string, int>>>
{
    private readonly ISMSOrganizationalUserRepository _repository;
    private readonly ILogger<GetDepartmentStatisticsQueryHandler> _logger;

    public GetDepartmentStatisticsQueryHandler(ISMSOrganizationalUserRepository repository, ILogger<GetDepartmentStatisticsQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Dictionary<string, int>>> HandleAsync(GetDepartmentStatisticsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetDepartmentStatisticsQuery");
            var result = await _repository.GetDepartmentStatisticsAsync();
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved department statistics for {Count} departments", result.Value?.Count ?? 0);
            }
            else
            {
                _logger.LogWarning("Failed to retrieve department statistics: {Error}", result.Error?.Message);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetDepartmentStatisticsQuery");
            return Result<Dictionary<string, int>>.Failure<Dictionary<string, int>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

public class GetSMSOrganizationalUserStatisticsQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSOrganizationalUserStatisticsQuery, Result<UserStatistics>>
{
    private readonly ISMSOrganizationalUserRepository _repository;
    private readonly ILogger<GetSMSOrganizationalUserStatisticsQueryHandler> _logger;

    public GetSMSOrganizationalUserStatisticsQueryHandler(ISMSOrganizationalUserRepository repository, ILogger<GetSMSOrganizationalUserStatisticsQueryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<UserStatistics>> HandleAsync(GetSMSOrganizationalUserStatisticsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSOrganizationalUserStatisticsQuery");
            var result = await _repository.GetUserStatisticsAsync();
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved SMS Organizational User statistics");
            }
            else
            {
                _logger.LogWarning("Failed to retrieve SMS Organizational User statistics: {Error}", result.Error?.Message);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetSMSOrganizationalUserStatisticsQuery");
            return Result<UserStatistics>.Failure<UserStatistics>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}