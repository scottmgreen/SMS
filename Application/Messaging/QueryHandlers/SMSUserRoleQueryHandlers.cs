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
                _logger.LogError("Failed to retrieve SMS User Roles: {Error}", result.Error?.Message);
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
            _logger.LogError(ex, "Unexpected error occurred while retrieving all SMS User Roles");
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
                _logger.LogError("Failed to retrieve active SMS User Roles: {Error}", result.Error?.Message);
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
            _logger.LogError(ex, "Unexpected error occurred while retrieving active SMS User Roles");
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
                _logger.LogError("GetSMSUserRoleByIdQuery received with null request or ID");
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
            _logger.LogError(ex, "Unexpected error occurred while retrieving SMS User Role with ID: {Id}", request?.UserRoleId?.Value);
            return Result<SMSUserRole>.Failure<SMSUserRole>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }
}

//public class GetSMSUserRolesByUserIdQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSUserRolesByUserIdQuery, Result<IEnumerable<SMSUserRole>>>
//{
//    private readonly SMSUserRoleDataService _userRoleDataService;
//    private readonly ILogger<GetSMSUserRolesByUserIdQueryHandler> _logger;

//    public GetSMSUserRolesByUserIdQueryHandler(SMSUserRoleDataService userRoleDataService, ILogger<GetSMSUserRolesByUserIdQueryHandler> logger)
//    {
//        _userRoleDataService = userRoleDataService ?? throw new ArgumentNullException(nameof(userRoleDataService));
//        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//    }

//    public async Task<Result<IEnumerable<SMSUserRole>>> HandleAsync(GetSMSUserRolesByUserIdQuery request, CancellationToken cancellationToken)
//    {
//        try
//        {
//            if (request is null || string.IsNullOrWhiteSpace(request.UserId))
//            {
//                _logger.LogError("GetSMSUserRolesByUserIdQuery received with null request or UserId");
//                return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NullOrEmpty);
//            }

//            _logger.LogInformation("Processing GetSMSUserRolesByUserIdQuery for User ID: {UserId}", request.UserId);

//            var result = await _userRoleDataService.GetSMSUserRolesByUserIdAsync(request.UserId);

//            if (result.IsSuccess)
//            {
//                _logger.LogInformation("Successfully retrieved {Count} SMS User Roles for User ID: {UserId}", 
//                    result.Value?.Count() ?? 0, request.UserId);
//            }
//            else
//            {
//                _logger.LogWarning("No SMS User Roles found for User ID: {UserId}", request.UserId);
//            }

//            return result;
//        }
//        catch (OperationCanceledException)
//        {
//            _logger.LogWarning("GetSMSUserRolesByUserIdQuery operation was cancelled");
//            throw;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Unexpected error occurred while retrieving SMS User Roles for User ID: {UserId}", request?.UserId);
//            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
//        }
//    }
//}

//public class GetActiveSMSUserRolesByUserIdQueryHandler : BaseQueryBundle, IRequestHandler<GetActiveSMSUserRolesByUserIdQuery, Result<IEnumerable<SMSUserRole>>>
//{
//    private readonly SMSUserRoleDataService _userRoleDataService;
//    private readonly ILogger<GetActiveSMSUserRolesByUserIdQueryHandler> _logger;

//    public GetActiveSMSUserRolesByUserIdQueryHandler(SMSUserRoleDataService userRoleDataService, ILogger<GetActiveSMSUserRolesByUserIdQueryHandler> logger)
//    {
//        _userRoleDataService = userRoleDataService ?? throw new ArgumentNullException(nameof(userRoleDataService));
//        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//    }

//    public async Task<Result<IEnumerable<SMSUserRole>>> HandleAsync(GetActiveSMSUserRolesByUserIdQuery request, CancellationToken cancellationToken)
//    {
//        try
//        {
//            if (request is null || string.IsNullOrWhiteSpace(request.UserId))
//            {
//                _logger.LogError("GetActiveSMSUserRolesByUserIdQuery received with null request or UserId");
//                return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NullOrEmpty);
//            }

//            _logger.LogInformation("Processing GetActiveSMSUserRolesByUserIdQuery for User ID: {UserId}", request.UserId);

//            var result = await _userRoleDataService.GetActiveSMSUserRolesByUserIdAsync(request.UserId);

//            if (result.IsSuccess)
//            {
//                _logger.LogInformation("Successfully retrieved {Count} active SMS User Roles for User ID: {UserId}", 
//                    result.Value?.Count() ?? 0, request.UserId);
//            }
//            else
//            {
//                _logger.LogWarning("No active SMS User Roles found for User ID: {UserId}", request.UserId);
//            }

//            return result;
//        }
//        catch (OperationCanceledException)
//        {
//            _logger.LogWarning("GetActiveSMSUserRolesByUserIdQuery operation was cancelled");
//            throw;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Unexpected error occurred while retrieving active SMS User Roles for User ID: {UserId}", request?.UserId);
//            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
//        }
//    }
//}

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
                _logger.LogError("GetSMSUserRolesByRoleValueQuery received with null request or RoleValue");
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
            _logger.LogError(ex, "Unexpected error occurred while retrieving SMS User Roles for Role: {RoleValue}", request?.RoleValue);
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
                _logger.LogError("GetSMSUserRolesByDepartmentQuery received with null request or Department");
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
            _logger.LogError(ex, "Unexpected error occurred while retrieving SMS User Roles for Department: {Department}", request?.Department);
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
                _logger.LogError("GetSMSUserRolesByUserTypeQuery received with null request or UserType");
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
            _logger.LogError(ex, "Unexpected error occurred while retrieving SMS User Roles for UserType: {UserType}", request?.UserType);
            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }
}





//public class ValidateUserHasRoleQueryHandler : BaseQueryBundle, IRequestHandler<ValidateUserHasRoleQuery, Result<bool>>
//{
//    private readonly SMSUserRoleDataService _userRoleDataService;
//    private readonly ILogger<ValidateUserHasRoleQueryHandler> _logger;

//    public ValidateUserHasRoleQueryHandler(SMSUserRoleDataService userRoleDataService, ILogger<ValidateUserHasRoleQueryHandler> logger)
//    {
//        _userRoleDataService = userRoleDataService ?? throw new ArgumentNullException(nameof(userRoleDataService));
//        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//    }

//    public async Task<Result<bool>> HandleAsync(ValidateUserHasRoleQuery request, CancellationToken cancellationToken)
//    {
//        try
//        {
//            if (request is null || string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.RoleValue))
//            {
//                _logger.LogError("ValidateUserHasRoleQuery received with null or invalid parameters");
//                return Result<bool>.Failure<bool>(DomainErrors.SMSUserRoleError.NullOrEmpty);
//            }

//            _logger.LogInformation("Processing ValidateUserHasRoleQuery for User: {UserId}, Role: {RoleValue}", 
//                request.UserId, request.RoleValue);

//            var result = await _userRoleDataService.ValidateUserRoleAsync(request.UserId, request.RoleValue);

//            if (result.IsSuccess)
//            {
//                _logger.LogInformation("User {UserId} role validation result: {HasRole}", request.UserId, result.Value);
//            }

//            return result;
//        }
//        catch (OperationCanceledException)
//        {
//            _logger.LogWarning("ValidateUserHasRoleQuery operation was cancelled");
//            throw;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Unexpected error occurred while validating user role");
//            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
//        }
//    }
//}