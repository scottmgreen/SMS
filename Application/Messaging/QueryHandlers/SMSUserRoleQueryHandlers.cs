using Microsoft.Extensions.Logging;
using SMS_Application.Common;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Queries;
using SMS_Application.Services;
using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Domain.Models;
using SMS_Infrastructure.Services;
using SMS_Shared.Common;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// SMS USER ROLE QUERY HANDLERS
// =============================================

public class GetAllSMSUserRolesQueryHandler : BaseQueryBundle, IRequestHandler<GetAllSMSUserRolesQuery, Result<IEnumerable<SMSUserRole>>>
{
    private readonly SMSUserRoleDataService _userRoleDataService;
    private readonly ILogger<GetAllSMSUserRolesQueryHandler> _logger;

    public GetAllSMSUserRolesQueryHandler(SMSUserRoleDataService userRoleDataService, ILogger<GetAllSMSUserRolesQueryHandler> logger)
    {
        _userRoleDataService = userRoleDataService ?? throw new ArgumentNullException(nameof(userRoleDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSUserRole>>> HandleAsync(GetAllSMSUserRolesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing GetAllSMSUserRolesQuery");

            var result = await _userRoleDataService.GetAllSMSUserRolesAsync();

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} SMS User Roles", result.Value?.Count() ?? 0);
            }
            else
            {
                _logger.LogApplicationError("Failed to retrieve SMS User Roles: {Error}", ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetAllSMSUserRolesQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while retrieving all SMS User Roles", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }
}

public class GetAllActiveSMSUserRolesQueryHandler : BaseQueryBundle, IRequestHandler<GetAllActiveSMSUserRolesQuery, Result<IEnumerable<SMSUserRole>>>
{
    private readonly SMSUserRoleDataService _userRoleDataService;
    private readonly ILogger<GetAllActiveSMSUserRolesQueryHandler> _logger;

    public GetAllActiveSMSUserRolesQueryHandler(SMSUserRoleDataService userRoleDataService, ILogger<GetAllActiveSMSUserRolesQueryHandler> logger)
    {
        _userRoleDataService = userRoleDataService ?? throw new ArgumentNullException(nameof(userRoleDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSUserRole>>> HandleAsync(GetAllActiveSMSUserRolesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing GetAllActiveSMSUserRolesQuery");

            var result = await _userRoleDataService.GetAllActiveSMSUserRolesAsync();

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} active SMS User Roles", result.Value?.Count() ?? 0);
            }
            else
            {
                _logger.LogApplicationError("Failed to retrieve active SMS User Roles: {Error}", ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetAllActiveSMSUserRolesQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while retrieving active SMS User Roles", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }
}

public class GetSMSUserRoleByIdQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSUserRoleByIdQuery, Result<SMSUserRole>>
{
    private readonly SMSUserRoleDataService _userRoleDataService;
    private readonly ILogger<GetSMSUserRoleByIdQueryHandler> _logger;

    public GetSMSUserRoleByIdQueryHandler(SMSUserRoleDataService userRoleDataService, ILogger<GetSMSUserRoleByIdQueryHandler> logger)
    {
        _userRoleDataService = userRoleDataService ?? throw new ArgumentNullException(nameof(userRoleDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSUserRole>> HandleAsync(GetSMSUserRoleByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.UserRoleId is null)
            {
                _logger.LogApplicationError("GetSMSUserRoleByIdQuery received with null request or ID", ApplicationEventIds.Error, null);
                return Result<SMSUserRole>.Failure<SMSUserRole>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogInformation("Processing GetSMSUserRoleByIdQuery for ID: {Id}", request.UserRoleId.Value);

            var result = await _userRoleDataService.GetSMSUserRoleByIdAsync(request.UserRoleId.Value);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved SMS User Role with ID: {Id}", request.UserRoleId.Value);
            }
            else
            {
                _logger.LogWarning("SMS User Role not found with ID: {Id}", request.UserRoleId.Value);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetSMSUserRoleByIdQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while retrieving SMS User Role with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<SMSUserRole>.Failure<SMSUserRole>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }
}

public class GetSMSUserRolesByRoleValueQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSUserRolesByRoleValueQuery, Result<IEnumerable<SMSUserRole>>>
{
    private readonly SMSUserRoleDataService _userRoleDataService;
    private readonly ILogger<GetSMSUserRolesByRoleValueQueryHandler> _logger;

    public GetSMSUserRolesByRoleValueQueryHandler(SMSUserRoleDataService userRoleDataService, ILogger<GetSMSUserRolesByRoleValueQueryHandler> logger)
    {
        _userRoleDataService = userRoleDataService ?? throw new ArgumentNullException(nameof(userRoleDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSUserRole>>> HandleAsync(GetSMSUserRolesByRoleValueQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null || string.IsNullOrWhiteSpace(request.RoleValue))
            {
                _logger.LogApplicationError("GetSMSUserRolesByRoleValueQuery received with null request or RoleValue", ApplicationEventIds.Error, null);
                return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogInformation("Processing GetSMSUserRolesByRoleValueQuery for Role: {RoleValue}", request.RoleValue);

            var result = await _userRoleDataService.GetSMSUserRolesByRoleValueAsync(request.RoleValue);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} SMS User Roles for Role: {RoleValue}", 
                    result.Value?.Count() ?? 0, request.RoleValue);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetSMSUserRolesByRoleValueQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while retrieving SMS User Roles for Role: {RoleValue}", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }
}

public class GetSMSUserRolesByDepartmentQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSUserRolesByDepartmentQuery, Result<IEnumerable<SMSUserRole>>>
{
    private readonly SMSUserRoleDataService _userRoleDataService;
    private readonly ILogger<GetSMSUserRolesByDepartmentQueryHandler> _logger;

    public GetSMSUserRolesByDepartmentQueryHandler(SMSUserRoleDataService userRoleDataService, ILogger<GetSMSUserRolesByDepartmentQueryHandler> logger)
    {
        _userRoleDataService = userRoleDataService ?? throw new ArgumentNullException(nameof(userRoleDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSUserRole>>> HandleAsync(GetSMSUserRolesByDepartmentQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Department))
            {
                _logger.LogApplicationError("GetSMSUserRolesByDepartmentQuery received with null request or Department", ApplicationEventIds.Error, null);
                return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogInformation("Processing GetSMSUserRolesByDepartmentQuery for Department: {Department}", request.Department);

            var result = await _userRoleDataService.GetSMSUserRolesByDepartmentAsync(request.Department);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} SMS User Roles for Department: {Department}", 
                    result.Value?.Count() ?? 0, request.Department);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetSMSUserRolesByDepartmentQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while retrieving SMS User Roles for Department: {Department}", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }
}

public class GetSMSUserRolesByUserTypeQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSUserRolesByUserTypeQuery, Result<IEnumerable<SMSUserRole>>>
{
    private readonly SMSUserRoleDataService _userRoleDataService;
    private readonly ILogger<GetSMSUserRolesByUserTypeQueryHandler> _logger;

    public GetSMSUserRolesByUserTypeQueryHandler(SMSUserRoleDataService userRoleDataService, ILogger<GetSMSUserRolesByUserTypeQueryHandler> logger)
    {
        _userRoleDataService = userRoleDataService ?? throw new ArgumentNullException(nameof(userRoleDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSUserRole>>> HandleAsync(GetSMSUserRolesByUserTypeQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null || string.IsNullOrWhiteSpace(request.UserType))
            {
                _logger.LogApplicationError("GetSMSUserRolesByUserTypeQuery received with null request or UserType", ApplicationEventIds.Error, null);
                return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogInformation("Processing GetSMSUserRolesByUserTypeQuery for UserType: {UserType}", request.UserType);

            var result = await _userRoleDataService.GetSMSUserRolesByUserTypeAsync(request.UserType);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} SMS User Roles for UserType: {UserType}", 
                    result.Value?.Count() ?? 0, request.UserType);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetSMSUserRolesByUserTypeQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while retrieving SMS User Roles for UserType: {UserType}", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }
}
